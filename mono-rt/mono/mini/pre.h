#ifndef __PRE_H__
#define __PRE_H__

#include "controlParameter.h"
#include "mini.h"
#include <mono/metadata/assembly.h>
#include <mono/metadata/marshal.h>
#include <mono/metadata/gc-internal.h>
#include <mono/metadata/monitor.h>
#include <mono/metadata/object-internals.h>
#include <pthread.h>
#include <fcntl.h>
#include <sys/mman.h>
#include <unistd.h>
#include <errno.h>

FILE * fileDescriptorForDebug;
//
//  Die Groeße der Trampoline ist in tramp-x86.c, Z. 457 festgelegt
//
#define SPECIFIC_TRAMP_SIZE 10
//
//  Maximale Anzahl der Methoden, deren nativer Code waehrend
//  der JIT-Compilierung angezeigt wird.
//
#define MAXMETHODSASMTRACE 10
//
//  Maximale Anzahl Woerter auf dem Stack, die untersucht
//  werden um die Ruecksprungadresse in das Generic
//  Delegate Trampoline zu finden ('mono_delegate_trampoline()').
//
#define MAXSTACKSEARCH 100

enum PrePatchMode {
	PrePatchCallInstructions,	// pCallModTrampHash_g
	PrePatchImtEmtries,			// pImtModTrampHash_g
	PrePatchVtableEntries,		// pVtableModTrampHash_g
	PrePatchPltEntries			//
};

//
//  Hilfsdatenstruktur fuer den Pre-JIT-Code
//
struct PreArgs {
	MonoAssembly *ass;
	int verbose;
	unsigned int opts;
};
//
//  Die folgende Datenstruktur wurde in driver.c
//  definiert. Da sie nun jedoch Quellcodedatei-uebergreifend
//  verwendet wird, wird sie hier definiert.
//
typedef struct {
	MonoDomain *domain;
	const char *file;
	int argc;
	char **argv;
	guint32 opts;
	char *aot_options;
} MainThreadArgs;
//
//  Typdefinition der Hilfsdatenstruktur, damit
//  sie fuer eine Funktionsdeklaration verwendet werden
//  kann.
//
//typedef struct PreArgs PreCompileArgs;
//
//  Typdefinitionen, die das Berechnen der Sprungziele
//  im Pre-Patch Code ruebersichtlicher machen sollen.
//
typedef unsigned char* CODEPTR;
typedef int OFFSET;
typedef unsigned int OUTPUTTYPE;
//
//  Verwaltung der originalen Generic Trampoline
//
//  Hashtable: MonoGenericTrampHashTable
//  - key: address
//  - value: struct MonoGenericTrampElem
//
//  mono_arch_create_trampoline_code(), tramp-x86.c, Z. 449
//
struct MonoGenericTrampElem
{
	// Typ des Trampolines (JIT, JUMP, ...)
	MonoTrampolineType genericTrampType;
	// Anfangsadresse des nativen Codes des Trampolines
	CODEPTR genericTrampAddress;
};
//
//  Verwaltung der Specific Trampoline
//
//  Hashtable: MonoSpecificTrampHashTable
//  - key: address
//  - value: struct MonoSpecificTrampElem
//
//  mono_arch_create_specific_trampoline(), tramp-x86.c, Z. 479
//
struct MonoSpecificTrampElem
{
	// Typ des Trampolines (JIT, JUMP, ...)
	MonoTrampolineType specificTrampType;
	// Anfangsadresse des nativen Codes des Trampolines
	CODEPTR specificTrampAddress;
	// Anfangsadresse des anzuspringenden Generic Trampoline
	CODEPTR specificTrampJumpInstructionTarget;
	// Zwischenablage der Anfangsadresse des anzuspringenden Generic Trampoline
	CODEPTR specificTrampSavedJumpInstructionTarget;
	// Anfangsadresse der Methode, die aufgerufen wird
	CODEPTR specificTrampCallInstructionFinalTarget;
	// Methode Handle, das auf den Stack gelegt wird
	CODEPTR specificTrampPushValue;
	// Anfangsadresse des Modified Generic Trampoline, das
	// waehrend des Pre-Patch angesprungen werden soll
	CODEPTR specificTrampModifiedGenericTrampAddress;
	//  Zeiger auf einen Speicherbereich, der als Zwischenablage fuer ein
	//  Spezifischens Trampoline dient
	CODEPTR specificTrampCopy;
	// Laenge des nativen Codes des Specific Trampoline
	unsigned int specificTrampCodeLength;
	// Offset der Jump-Instruktion innerhalbt des Specific Trampoline
	OFFSET specificTrampOffsetJumpInstruction;
	// Kennzeichnung, ob das Trampoline waehrend des
	// Pre-Patch bereits betrachtet/ausgefuehrt wurde
	unsigned char m_IsPatched;
	// Nummer des Trampoline fuer interne Zwecke
	int number;
	// Kennzeichnung, ob die Werte im Speicher noch mit den
	// in diesem struct gespeicherten uebereinstimmen (obsolet)
	unsigned char specificTrampPassedCheck;
	// Kennzeichnung, ob das Trampoline einen short oder
	// near jump ausfuehrt
	unsigned char specificTrampHasShortJump;
};
//
//  Globale Variablen zur Steuerung des Pre-Patch
//  zur Laufzeit, beispielsweise fuer Delegates.
//
CODEPTR pImtTramp_g;
CODEPTR pVTableTramp_g;
//  Speichert die Adresse des Generic Trampoline vom
//  Typ "MONO_TRAMPOLINE_DELEGATE", das modifiziert
//  wurde um in den Pre-Patch-Code zurueckzuspringen.
CODEPTR pModDelTramp_g;
//  Speichert die Adresse des gueltigen
//  Generic Trampoline vom Typ "MONO_TRAMPOLINE_DELEGATE"
CODEPTR pDelTramp_g;

char * acAsmMethods_g[MAXMETHODSASMTRACE];
//
//  Verwaltung der modifizierten Generic Trampoline
//
//  Hashtable: MonoModifiedGenericTrampHashTable
//  - key: type
//  - value: struct MonoModifiedGenericTrampElem
//
//  prepatchSingleCallInstruction(), pre-patch.c, Z. ???
//
struct MonoModifiedGenericTrampElem
{
	// Typ des Trampolines (JIT, JUMP, ...)
	MonoTrampolineType modifiedGenericTrampType;
	// Anfangsadresse des nativen Codes des Trampolines
	CODEPTR m_pModGenTramp;
};
//
//  Verwaltung des (pre-)compilierten Codes
//
//  Hashtable: MonoNativeCodeHashTable
//  - key: nativeCodeMethodPtr
//  - value: struct MonoNativeCodeElem
//
//  mono_codegen(), mini.c, Z. 3181
//
struct MonoNativeCodeElem {
	// Zeiger auf Methoden-Handle der compilierten Methode
	MonoMethod * nativeCodeMethodPtr;
	// absolute Adresse des nativen Codes zum Zeitpunkt als
	// eine Call-Instruktion emittiert wurde
	CODEPTR nativeCodeTempAddress;
	// finale absolute Adresse des nativen Codes
	CODEPTR nativeCodeFinalAddress;
	// Laenge des nativen Codes der compilierten Methode
	unsigned int nativeCodeCodeLength;
	// VTable-Slot falls die Methode eine Interface-Methode ist.
	// Der Slot berechnet sich aus dem Offset des Interfaces
	// innerhalb der Klasse und der Slot-Nummer der Methode selbst.
	int interfaceMethodVtableSlot;
};
//
//  Verwaltung der Methoden, fuer die das IMT-Pre-Patch
//  nicht durchgefuehrt werden konnte.
//
//  Hashtable: MonoNotImtPrePatchedMethodsHashTable
//  - key: notImtPrePatchedMethodPtr
//  - value: struct MonoNotImtPrePatchedMethodElem
//
//  mono_magic_trampoline(), mini-trampolines.c, Z. 575
//
struct MonoNotImtPrePatchedMethodElem {
	// Zeiger auf Methoden-Handle der Klassenimplementierung,
	// fuer deren Interface-Methode kein IMT-Pre-Patch durchgefuehrt
	// werden konnte.
	MonoMethod * notImtPrePatchedMethodPtr;
	// VTable-Slot, den die Interface-Methode benutzt
	// haette, wenn das IMT-Pre-Patch funktioniert haette.
	int usedVtableSlot;
};
//
//  Verwaltung der (pre-)compilierten Klassen
//
//  Hashtable: MonoClassHashTable
//  - key: klasse
//  - value: struct MonoClassElem
//
//  insertListElement(), pre-patch.c, Z. ???
//
struct MonoClassElem
{
	// Zeiger auf die originale Struktur MonoClass zur Beschreibung der Klasse
	MonoClass *klasse;
};
//
//  Die (pre-) compilierten Klassen werden vorerst
//  zusaetzlich in einer Liste verwaltet, um sie
//  in einer deterministischen Reihenfolge wiedergeben
//  zu koennen (vorrangig zum Debugging).
//
struct MonoClassList
{
	MonoClass *klasse;
	// Zeiger auf das letzte Listenelement
	struct MonoClassList *head;
	// Zeiger auf das nachfolgende Listenelement
	struct MonoClassList *next;
	// Zeiger auf das vorangehende Listenelement
	struct MonoClassList *prev;
	//  Zeigt an, ob die Liste geloescht wurde
	int isDestroyed;
} ClassList;

//
//  Verwaltung der emittierten Patches
//
//  Die Patch-Informationen werden in einer Liste verwaltet
//
//  mono_arch_patch_code(), mini-x86.c
//
struct MonoCallCodeList
{
	//  Zeiger auf das Handle der Methode, in der die Call-Anweisung enthalten ist
	MonoMethod * m_pCallerMeth;
	//  obsolet: Offset der Call-Anweisung im nativen Code
	OFFSET offset;
	//  Anfangsadresse des Patches im nativen Code
	CODEPTR m_pPatch;
	//  Start-Adresse des Codes zur Behandlung des Patches,
	//  z.B. ein Specific JIT Trampoline
	CODEPTR m_pPatchTarget;
	//  Gibt den Typ des aufgezeichneten Patches an
	MonoJumpInfoType patchType;
	//  Zeigt an, ob die Liste geloescht wurde
	int isDestroyed;
	//  Zeigt an, ob die Liste bereits initial benutzt wurde
	int isInitialized;
	//  Index im globalen "CallArray" zur Bestimmung der neuen
	//  Angangsadresse des nativen Codes des struct MonoCompile
	struct MonoCallCodeList *head;
	//  Zeiger auf das nachfolgende Listenelement
	struct MonoCallCodeList *next;
	//  Zeiger auf das vorangehende Listenelement
	struct MonoCallCodeList *prev;
} CallCodeList;


struct MonoAssemblyList
{
	MonoAssembly * assembly;
	char * assemblyName;
	int isPreCompiled;
	struct MonoAssemblyList * head;
	//  Zeiger auf das nachfolgende Listenelement
	struct MonoAssemblyList * next;
	//  Zeiger auf das vorangehende Listenelement
	struct MonoAssemblyList * prev;
} AssemblyList;


struct MonoVTableElem {
	// Zeiger auf die Struktur MonoVTable der erfassten VTable
	MonoVTable *vtable;
	// Anfangsadresse des native Codes des IMT-Trampolines 
	CODEPTR vTableIMTTrampAddress;
	// Anfangsadresse des native Codes des VTable-Trampolines 
	CODEPTR vTableVTableTrampAddress;
	// Absolute Adresse des ersten IMT-Slots
	CODEPTR vTableFirstIMTSlot;
};


typedef struct MonoAotModule {
	char *aot_name;
	guint32 opts;
	gpointer *got;
	GHashTable *name_cache;
	GHashTable *extra_methods;
	GHashTable *method_to_code;
	GHashTable *method_ref_to_method;
	MonoAssemblyName *image_names;
	char **image_guids;
	MonoAssembly *assembly;
	MonoImage **image_table;
	guint32 image_table_len;
	gboolean out_of_date;
	gboolean plt_inited;
	guint8 *mem_begin;
	guint8 *mem_end;
	guint8 *code;
	guint8 *code_end;
	guint8 *plt;
	guint8 *plt_end;
	guint32 *code_offsets;
	guint8 *method_info;
	guint32 *method_info_offsets;
	guint8 *got_info;
	guint32 *got_info_offsets;
	guint8 *ex_info;
	guint32 *ex_info_offsets;
	guint32 *method_order;
	guint32 *method_order_end;
	guint8 *class_info;
	guint32 *class_info_offsets;
	guint32 *methods_loaded;
	guint16 *class_name_table;
	guint32 *extra_method_table;
	guint32 *extra_method_info_offsets;
	guint8 *extra_method_info;
	guint8 *unwind_info;

	guint8 *trampolines [MONO_AOT_TRAMP_NUM];
	guint32 trampoline_index [MONO_AOT_TRAMP_NUM];

	MonoAotFileInfo info;

	gpointer *globals;
	MonoDl *sofile;
} MonoAotModule;
//
//  Verwaltung der geladenen AOT-Images
//
struct MonoAotCodeList {
	//  Zeiger auf das Handle der Methode, in der die Call-Anweisung enthalten ist
	MonoAotModule * loadedAotModule;
	//  Zeigt an, ob die Liste geloescht wurde
	int isDestroyed;
	//  Zeigt an, ob die Liste bereits initial benutzt wurde
	int isInitialized;
	//  Zaehlt die Anzahl der geladenen AOT-Module
	int count;
	//  Zeiger auf den Kopf der Liste
	struct MonoAotCodeList * head;
	//  Zeiger auf das nachfolgende Listenelement
	struct MonoAotCodeList * next;
	//  Zeiger auf das vorangehende Listenelement
	struct MonoAotCodeList * prev;
} AotCodeList;

//
//  Hash-Tabellen zum Verwalten der Pre-Patch-Informationen
//  Key: Adresse des Specific/Generic/Modified Generic Trampoline
//  Value: Pointer auf struct des entsprechenden Trampolines
//
GHashTable *MonoGenericTrampHashTable;
GHashTable *MonoSpecificTrampHashTable;
GHashTable * pCallModTrampHash_g;
GHashTable * pImtModTrampHash_g;
GHashTable * pVtableModTrampHash_g;
GHashTable * pPltModTrampHash_g;
GHashTable *MonoNativeCodeHashTable;
GHashTable *MonoClassHashTable;
GHashTable *MonoVTHashTable;
GHashTable *ExcludedFromPreJit;
//
//  Deklaration der Methoden des Pre-JIT
//
void preJitCompile (MonoAssembly *, MainThreadArgs *);
void preCompileStackTraceMethod (void);
int preCompile (MonoAssembly *, int);
gpointer checkedMonoCompileMethod (MonoMethod *);
void preCompileWrapper (MonoAssembly *, int);
MonoMethod* precompile_get_runtime_invoke_sig (MonoMethodSignature *);
void preCompileJitIcallWrapper (gpointer , gpointer, gpointer);
gboolean precompile_can_marshal_struct (MonoClass *);
void buildPreCompileList (MonoAssembly *);
void preCompileAssemblyList (void *);
void printPreCompileList (void);
void insertInPreJitExclusionList (void);
//
//  Deklaration der Methoden des Pre-Patch
//
void initializeAndStartPrePatch (MainThreadArgs * mainArgs);
void prepatchCode (int);
void triggerPrePatchCode (CODEPTR) __attribute__ ((__noinline__));
void initializeArrayVariantOfClass (gpointer, gpointer, gpointer);
void fireDelegateCtor (MonoDelegate *) __attribute__ ((__noinline__)) MONO_INTERNAL;
//
//  Deklaration von Methoden zur Verwaltung der von
//  Pre-JIT und Pre-Patch benoetigten Datenstrukturen.
//
void * insertListElement (enum TrampolineType, void *, void *, void *, unsigned int);
void * insertClassListElement (enum TrampolineType, void *, void *, void *, unsigned int);
void * insertNativeCodeListElement (enum TrampolineType, void *, void *, void *, unsigned int);
void * insertGenericTrampolineListElement (enum TrampolineType, void *, void *, void *, unsigned int);
void * insertSpecificTrampolineListElement (enum TrampolineType, void *, void *, void *, unsigned int);
void * insertModifiedGenericTrampolineListElement (enum TrampolineType, void *, void *, void *, unsigned int);
void * insertCallCodeInstructionListElement (enum TrampolineType, void *, void *, void *, unsigned int);
void * insertVTableListElement (enum TrampolineType, void *, void *, void *, unsigned int);
void initPrePatchDataStructures(void);
void parseMethodsForAsmTrace (const char *);
void printStackTrace (char **);
void setMonitoringOptionsInternal (int, int, int);


#endif /* __PRE_H__ */
