#ifndef __CONTROLPARAMETER_H__
#define __TEST_H__

#define PREPATCHALL 	0xf
#define PREPATCHCALLS	0x1
#define PREPATCHVIRT	0x2
#define PREPATCHDEL		0x4
#define PREPATCHDELEXT	0xc
//
//  Steuerung der Debug-Ausgaben
//
int doTrace_g;
int doTraceCmd_g;
int doTraceLevel2_g;
int doTraceLevel2Cmd_g;
int snoopcap;
int snoopcan;
int snooptc;
int doTraceJIT_g;
int doTraceJITCmd_g;
int doTracePatch_g;
int doTracePatchCmd_g;
int snooppatch;
int snooppatch_org;
int snoopcallvirt;
int snoopvtpp;
int snoopasm;
int snooptramp;
int snoopwrapper;
int snoopwrapper_org;
int patchlist;
int disablegc;
int doWaitForChkPoint_g;
//
//  Steuerung der Pre-JIT-Compilierung
//
int doPreJIT_g;
void * pLastPreCompiledElement_g;
//
// Variablen zur Steuerung des Pre-Patch
//
int doPrePatch_g;
int doPrePatchCmd_g;
int doPrePatchVirt_g;
//
//  Globale Variable zur Aktivierung des Slow Path-Modus
//  der Monitor-Trampoline.
//
int useSlowPath_g;
//
// disableMonoGetGenericContextFromCode_g wird u.a.
// in der Funktion "fireDelegateCtor()" in
// "mono/mini"pre-patch.c" geschrieben. Sie wird immer
// dann auf den Wert 1 gesetzt, wenn ein (Delegate-)
// Pre-Patch ausgefuehrt werden soll. Die Variable wird
// in "mono_get_generic_context_from_code()" in
// "mono/mini/mini-generic-sharing.c" auf den Wert 1
// geprueft. Wenn das der Falls ist, dann gibt die
// Funkion "mono_get_generic_context_from_code()" stets
// den Wert NULL zurueck. Andernfalls koennte eine
// Assertion ausgeloest werden, da zum Zeitpunkt des
// Pre-Patch kein Generic Sharing Context vorhanden
// sein koennte.
//
int disableMonoGetGenericContextFromCode_g;
int createModifiedDelegateTrampoline_g;
int validateSpecificTrampoline_g;
//
//  Makro-Definitionen fuer Monitoring-Funktionen
//  und zur Steuerung des Pre-Patch etc.
//
#define MSRTAA ((doPreJIT_g == 1) ? 1 : 0)
#define SNOOP ((doTrace_g == 1) ? 1 : 0)
#define SNOOP2 ((doTraceLevel2_g == 1) ? 1 : 0)
#define SNOOPCAN ((snoopcan == 1) ? 1 : 0)
#define SNOOPCAP ((snoopcap == 1) ? 1 : 0)
#define SNOOPTC ((snooptc == 1) ? 1 : 0)
#define SNOOPJIT ((doTraceJIT_g == 1) ? 1 : 0)
#define SNOOPPATCH ((doTracePatch_g == 1) ? 1 : 0)
#define SNOOPCALLVIRT ((snoopcallvirt == 1) ? 1 : 0)
#define SNOOPVTABLE ((snoopvtpp == 1) ? 1 : 0)
#define PREPTCH ((doPrePatch_g >= 1) ? 1 : 0)
#define PREPTCHCALLVIRT ((doPrePatchVirt_g == 1) ? 1 : 0)
#define SNOOPASM ((snoopasm == 1) ? 1 : 0)
#define SNOOPTRAMP ((snooptramp == 1) ? 1 : 0)
#define PATCHLIST ((patchlist == 1) ? 1 : 0)
#define MONITORSLOWPATH ((useSlowPath_g == 1) ? 1 : 0)
#define SNOOPWRAP ((snoopwrapper == 1) ? 1 : 0)

#endif /* __CONTROLPARAMETER_H__ */
