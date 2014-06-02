/**
 * @file	pre-patch.c
 * @author	mdae
 * @date	2010/12/09
 *
 * Diese Datei enthaelt den Quellcode zum Ausfuehren
 * einiger Trampoline-Typen im Rahmen einer
 * Initialisierungsroutine. Das betrifft inbesondere
 * die JIT-Trampoline, die bei Methodenaufrufen
 * die Compilierung der gerufenen Methode veranlassen.
 */
#include "pre.h"

static void prePatchSpecificJitTrampoline (struct MonoCallCodeList * pCallElem_p);
static void prePatchAotCode (int reTryPrePatch_p);
static void prepatchPatches (int reTryPrePatch_p);
static void prePatchSpecificClassInitTrampoline (struct MonoCallCodeList * pCallElem_p);
static void prepatchCallInstruction (CODEPTR pCallInst_p, struct MonoSpecificTrampElem * pSpecTrampElem_p, struct MonoCallCodeList * pCallElem_p);
static void prepatchAllClassVtables (int reTryPrePatch_p);
static void prePatchSpecificJumpTrampoline (struct MonoCallCodeList * pCallElem_p);
static void prePatchInternalMethodCall (struct MonoCallCodeList * pCallElem_p);
static void prePatchDelegateTrampoline (struct MonoCallCodeList * pCallElem_p);
static void prePatchSpecificGenericClassInitTrampoline (struct MonoCallCodeList * callCodeElem);
static void prePatchMonitorEnterTrampoline (struct MonoCallCodeList * callCodeElem);
static void prePatchMonitorExitTrampoline (struct MonoCallCodeList * callCodeElem);
static void prePatchRgctxFetchTrampoline (struct MonoCallCodeList * callCodeElem);
static void prePatchJitIcallTrampoline (struct MonoCallCodeList * callCodeElem);
static void prePatchIcallTrampoline (struct MonoCallCodeList * callCodeElem);
static int modifyX86SpecificTrampoline (struct MonoSpecificTrampElem *pSpecTramp_p, struct MonoCallCodeList * pCallElem_p, enum PrePatchMode prePatchMode_p);
static void restoreX86SpecificTrampoline (struct MonoSpecificTrampElem * specificTrampElem);
static void validateSpecificTrampolineJumpTarget (struct MonoSpecificTrampElem * specificTrampElem);
static int isX86CallInstruction (CODEPTR pCallInst_p);
static int isX86NopInstruction (CODEPTR pInst_p);
static CODEPTR getX86CallTarget (CODEPTR);
static void modifyX86CallInstructionTarget (CODEPTR pCallInst_p, CODEPTR pCallInstTgt_p);
static void prepatchClassVTable (struct MonoClassList * pClassElem_p, CODEPTR pCode_p, int reTryPrePatch_p);
static void prepatchImtEntry (struct MonoClassList * pClassElem_p, MonoMethod * pIfMethod_p, int iSlot_p, struct MonoVTableElem * pVtableElem_p, struct MonoNativeCodeElem * pCodeElem_p, CODEPTR pCode_p);
static void prepatchVtableEntry (struct MonoClassList * pClassElem_p, MonoMethod * pVirtMethod_p, int iSlot_p, struct MonoVTableElem * pVtableElem_p, CODEPTR pCode_p);
static void emitAndstartCallSequence (CODEPTR pMethod_p, int callOffset, CODEPTR pVtable_p, CODEPTR pCode_p) __attribute__ ((__noinline__));
static void initializeAllDefaultClasses (void);
static guint8 * createX86ModifiedGenericTrampoline (MonoTrampolineType);
static void printListSizes (void);
static void printLists (void);
static void cleanHashTables (void);
static void printListElement (gpointer, gpointer, gpointer);
/*
 * @brief	Initialisierung und Start des Pre-Patch
 *
 * @param	mainArgs	Zeiger auf die Argumente aus dem Haupt-Thread zur Bestimmung der Application-Domain
 *
 * Die Funktion "initializePrePatchCode()" wird aus der
 * Mono-internen Funktion "main_thread_handler()" heraus
 * aufgerufen.In dieser Funktion erfolgt die Pruefung,
 * ob das Pre-Patch tatsaechlich ausgefuehrt werden soll.
 * Die Pre-Patch-Funktionalitaet wird ueber den Kommandozeilenparameter
 * '--prepatch' aktiviert.  Alternativ koennen die verschiedenen
 * Pre-Patch-Funktionen einzelen aktiviert werden:
 * Pre-Patch direkter Methodenaufrufe '--pp1', Pre-Patch
 * der Interface Method und Virtual Tables '--pp2' und
 * Pre-Patch der Delegate-Trampoline '--pp3'.
 * Wenn das Pre-Patch aktiviert ist, dann wird ein CLI-Assembly
 * geladen, welches die Pre-Patch-Funktion aufruft. Dadurch
 * koennen evtl. auftretende Exceptions abgefangen werden
 * und das Pre-Patch fortgesetzt werden.
 *
 * Nachdem das Pre-Patch beendet wurde (oder falls es nicht
 * aktiviert wurde, werden die globalen Variablen zur
 * Steuerung der Aktivaetsanzeige des JIT-Compiliers und
 * der Trampoline, 'doTraceJIT_g' und 'doTracePatch_g', entsprechend
 * der Kommandozeilenparameter gesetzt (Aktivierung
 * Aktivitaetsanzeige JIT-Compiler: '--jtrace',
 * Trampoline: '--ptrace'). Die Aktivitaetsanzeigen
 * werden waehrend des Pre-JIT und Pre-Patch deaktiviert,
 * um die Ausgaben uebersichtlich zu gestalten.
 *
 * Wird das Pre-Patch per Kommandozeile aktiviert, so wird
 * immer auch das Pre-JIT aktiviert. Das liegt darin begruendet,
 * dass whrend des Pre-Patch sonst die Methoden, fuer deren
 * Aufruf das Pre-Patch ausgefuehrt wird, JIT-compiliert werden
 * könnten. Das kann z.B. bei generischen Methoden Fehler
 * verursachen, da die JIT-Compilierung nicht zur Laufzeit
 * geschieht und die Typen der Parameter nicht feststehen.
 */
void initializeAndStartPrePatch(MainThreadArgs * mainArgs) {

	MonoAssembly * pPrePatchAssembly;
	MonoMethodDesc * pMethDesc;
	MonoDomain * pAppDom;
	MonoImage * pImage;
	MonoMethod * pMethod;

	pPrePatchAssembly = NULL;
	pPrePatchAssembly = NULL;
	pAppDom = NULL;
	pImage = NULL;
	pMethod = NULL;

	if (0 == PREPTCH)
	{
		//
		//  Das Pre-Patch wurde nicht aktiviert.
		//
		;
	}
	else
	{
		// Es wird nun das CLI-Assembly
		//  "preJitPrePatchMain-rxxx.dll" geladen und ausgefuehrt.
		//  Darin wird die Funktion "prepatchCode()"
		//  aufgerufen um das Pre-Patch zu starten. Somit
		//  koennen Exceptions aufgefangen werden und das
		//  Pre-Patch kann fortgesetzt werden.
		//
		//  Laden des CLI-Assemblys "preJitPrePatchMain-rxxx.dll".
		//
		pPrePatchAssembly = NULL;
		pMethDesc = NULL;
		pAppDom = NULL;
		pImage = NULL;
		pMethod = NULL;
		//
		//  Erzeugen einer Methodenbeschreibung, mit der die managed
		//  Methode "prePatchClass:prePatchAssembly ()" geladen wird.
		//
		pMethDesc = mono_method_desc_new("PrePatchClass:prePatchAssembly", FALSE);

		if (pMethDesc != NULL)
		{
			pAppDom = mainArgs->domain;
			if (pAppDom == NULL)
			{
				printf("\n\tMono-RT - initializeAndStartPrePatch ():\tCan not determine application domain.\n");
				fflush(NULL);
				return;
			}

			pPrePatchAssembly = mono_domain_assembly_open(pAppDom,"MonoRT.dll");
			if (pPrePatchAssembly == NULL) {
				printf("\n\tMonoRT:\tCan not load MonoRT.dll\n");
				fflush(NULL);
				return;
			}

			pImage = mono_assembly_get_image(pPrePatchAssembly);
			if (pImage == NULL)
			{
				printf("\n\tMonoRT:\tCan not determine image from MonoRT.dll\n");
				fflush(NULL);
				return;
			}

			pMethod = mono_method_desc_search_in_image(pMethDesc, pImage);
			if (pMethod != NULL)
			{
				//
				//  Die managed Methode "prePatchClass:prePatchAssembly ()" konnte
				//  geladen werden. Sie wird nun aufgerufen und damit wird das
				//  Pre-Patch gestartet, indem die Funktion "prepatchCode ()"
				//  ausgefuehrt und gegebenenfalls erneut gestartet wird.
				//  Kehrt der folgende Funktionsaufruf zurueck, so ist das
				//  Pre-Patch abgeschlossen.
				//
				mono_runtime_invoke(pMethod, NULL, NULL, NULL);
			}
			//
			//  Freigabe der angelegten Methodenbeschreibung
			//
			mono_method_desc_free(pMethDesc);
		}
	}
	//
	//  Deaktivierung aller Pre-Patch-spezifischen Code-Teile
	//
	//	doPrePatch_g = 0;
	//
	//  Aktivierung der Aktivitaetsanzeige des JIT-Compilers
	//  und der Trampoline-Funktionen fuer qualitative Tests.
	//  Diese Variablen werden in den Makros 'SNOOPPATCH'
	//  und 'SNOOPJIT' verwendet, die im Code in einem
	//  IF-Statement geprueft werden.
	//
	doTraceJIT_g = doTraceJITCmd_g;
	doTracePatch_g = doTracePatchCmd_g;
	snoopwrapper = snoopwrapper_org;
	//
	//  Freigabe des durch Hash-Tabellen und Listen belegten Speichers
	//
	cleanHashTables();
	return;
}

/**
 * @brief Startpunkt des Pre-Patch, der aus managed Code aufgerufen wird
 *
 * @ param	reTryPrePatch_p	Ein Wert ungleich 0 gibt an, dass die Funktion wiederholt aus managed Code gerufen wurde
 *
 * prepatchCode() stellt den Startpunkt des Pre-Patch dar.
 * Diese Funktion wird aus der managed Methode
 * "PrePatchClass:prePatchAssembly()" heraus aufgerufen.
 */
void prepatchCode (int reTryPrePatch_p)
{
	static int prePatchPatchesOkay = 0;
	static int prePatchVtablesOkay = 0;
	static int prePatchPLTOkay = 0;
	//
	//  Ausgabe der Groesze der angelegten Hash-Tabellen
	//  fuer Debugging-Zwecke: Aktivierung ueber '--snooptramp'
	//
	printListSizes();
	//
	// Start des Pre-Patch der PLTs, falls AOT-Code geladen
	// wurde, und Pre-Patch direkter Methodenaufrufe.
	// Aktivierung mittels '--prepatch' oder '--pp1'
	//
	if ((doPrePatchCmd_g & PREPATCHCALLS) == PREPATCHCALLS)
	{
		if (prePatchPLTOkay == 0)
		{
			prePatchAotCode(reTryPrePatch_p);
			prePatchPLTOkay = 1;
			//
			// Das PrePatch der PLTs konnte erfolgreich abgeschlossen
			// werden. Fuer die folgenden Pre-Patch-Funktionen ist es
			// kein Neu-Start.
			//
			reTryPrePatch_p = 0;
		}
		else
		{
			;
		}

		if (prePatchPatchesOkay == 0)
		{
			prepatchPatches(reTryPrePatch_p);
			prePatchPatchesOkay = 1;
			//
			// Das PrePatch der Call-Anweisungen konnte erfolgreich
			// abgeschlossen werden. Fuer die folgenden Pre-Patch-Funktionen
			// ist es kein Neu-Start.
			//
			reTryPrePatch_p = 0;
		} else {
			;
		}
	} else {
		;
	}
	//
	//  Start des Pre-Patch der IMT-/VTable-Eintraege
	//  Aktivierung mittels '--prepatch' oder '--pp2'
	//
	//  Die globale Variable dient der Steuerung der Erzeugung
	//  der modifizierten Generic Trampoline.
	//
	if ((doPrePatchCmd_g & PREPATCHVIRT) == PREPATCHVIRT)
	{
		if (prePatchVtablesOkay == 0)
		{
			doPrePatchVirt_g = 1;
			prepatchAllClassVtables(reTryPrePatch_p);
			doPrePatchVirt_g = 0;
			prePatchVtablesOkay = 1;
		}
		else
		{
			;
		}
	}
	else
	{
		;
	}
	//
	//  Ausgabe der Groesze der angelegten Hash-Tabellen fuer
	//  Debugging-Zwecke: Aktivierung ueber '--snooptramp'
	//
	printListSizes();
	//
	//  Ausgabe der angelegten Hash-Tabellen fuer Debugging-Zwecke
	//  Aktivierung ueber '--patchlist'
	//
	printLists();

	return;
}

/*
 * @brief	prePatchAotCode() fuehrt Pre-Patch fuer AOT-compilierten Code aus.
 *
 * @param	reTryPrePatch_p		Ein Wert ungleich 0 gibt an, dass die Funktion wiederholt aufgerufen wurde
 */
static void prePatchAotCode(int reTryPrePatch_p)
{
	//
	// Anzahl bzw. Index der bereits betrachteten PLT-Slots
	//
	static int processedPltSlots = 0;
	//
	// Initialisierung der Liste der geladenen AOT-Images, die
	// abgearbeitet werden.
	//
	static struct MonoAotCodeList * pAotCode = &AotCodeList;
	//
	// Die Meldung ueber den Start des PLT-Pre-Patch
	// soll nur ein einziges Mal angezeigt werden.
	//
	static int firstTimeCalled = 0;
	//
	// Zeiger auf den Speicherbereich, der fuer das Backup
	// den ueberschriebenen Teil des Code-Segments des AOT-Images
	// genutzt wird.
	//
	static CODEPTR backup = NULL;

	MonoAotModule * pAotModule;
	int iPltSize;
	CODEPTR pPltEnd;
	CODEPTR pPltStart;
	CODEPTR pPltEntry;
	//
	// Absolute Adresse des Ziels der Jump-Instruktion
	// in einem PLT-Slot
	//
	CODEPTR pPltSlotJmpTgt;
	//
	// Anzahl Bytes eines PLT-EIntrages
	//
	OFFSET pltEntrySize;
	//
	// Zeiger, der zur Emittierung von Code genutzt wird
	//
	CODEPTR pEmitCode;
	CODEPTR pReservedCode;
	int Idx;
	int iPageSize;
	int iRet;
	unsigned int sum;
	struct MonoSpecificTrampElem * pSpecTramp;
	struct MonoModifiedGenericTrampElem * pModGenTramp;

	pAotModule = NULL;
	iPltSize = 0;
	pPltEnd = NULL;
	pPltStart = NULL;
	pPltEntry = NULL;
	pPltSlotJmpTgt = NULL;
	pltEntrySize = 0;
	pEmitCode = NULL;
	pReservedCode = NULL;
	Idx = 0;
	iPageSize = 0;
	iRet = 0;
	pSpecTramp = NULL;
	pModGenTramp = NULL;

#if defined(TARGET_X86) // Archtitekturabhaengiger Pre-Patch-Code
	if (pAotCode == NULL)
	{
		return;
	}

	if (pAotCode->count == 0)
	{
		return;
	}

	if (firstTimeCalled == 0)
	{
		firstTimeCalled = 1;
		printf("Pre-Patch of PLT starts\n");
		fflush(NULL);
	}

	if (NULL != backup)
	{
		free(backup);
	}
	backup = (CODEPTR) malloc(5);

	if (NULL == backup)
	{
		printf("\n\tMono-RT - prePatchAotCode ()\tCannot reserve Memory.\n");
		fflush(NULL);
		return;
	}
	//
	//  Betrachtung aller geladenen AOT-Images
	//
	while (pAotCode != NULL)
	{
		pAotModule = pAotCode->loadedAotModule;
		if ((NULL == pAotModule) || (0 == pAotModule->plt_inited))
		{
			printf("\n\tMonoRT:\ta PLT ist not initialized. This harms real-time code generation.\n");
			fflush(NULL);
			pAotCode = pAotCode->next;
			continue;
		}
		//
		// Calculate check sum over the AOT module just loaded
		// in order to ensure that it is loaded into memory.
		pReservedCode = (CODEPTR) pAotModule->mem_begin;
		sum = 0;
		while (pReservedCode < pAotModule->mem_end)
		{
			sum = sum ^ ((unsigned int) (*pReservedCode));
			pReservedCode++;
		}
		printf("\t%s check sum: %u\n", pAotModule->aot_name, sum);
		fflush(NULL);
		//
		// Initialisierungen
		//
		pltEntrySize = 9;

		pPltStart = (CODEPTR) pAotModule->plt;
		pPltEnd = (CODEPTR) pAotModule->plt_end;
		iPltSize = (int) pAotModule->info.plt_size;
//		printf("\tAOT-Image: %s\n", pAotModule->aot_name);
//		printf("\t\tBeginn PLT: %p\n", pAotModule->plt);
//		printf("\t\tEnde PLT: %p\n", pAotModule->plt_end);
//		fflush(NULL);
		//
		// Der Zeiger auf die Speicheradresse zum Emittieren der
		// Befehls-Sequenz fuer das Pre-Patch der PLT-Slots wird
		// auf den Beginn des Code-Segments im geladenen AOT-Modul
		// gesetzt. Damit soll das Pre-Patch ohne Veraenderugnen
		// des AOT-Subsystems ermoeglicht werden.
		//
		pReservedCode = (CODEPTR) pAotModule->code;
		//
		// Modifikation der Speicherzugriffsrechte,
		// um Code an den Anfang des Code-Segments
		// emittieren zu koennen.
		//
		iPageSize = (int) sysconf(_SC_PAGESIZE);
		if (-1 == iPageSize)
		{
			printf("\n\tMono-RT - prePatchAotCode ()\tError sysconf()\n");
			fflush(NULL);
			free(backup);
			backup = NULL;
			return;
		}

		iRet = mprotect((void *) pReservedCode, iPageSize, (PROT_READ | PROT_WRITE | PROT_EXEC));
		if (-1 == iRet)
		{
			printf("\n\tMono-RT:\tError mprotect() - ERRNO: %i\n", errno);
			fflush(NULL);
			free(backup);
			backup = NULL;
			return;
		}
		//
		// Weiteruecken des Zeigers auf den Speicherbereich
		// zum Emittieren der Befehls-Sequenz fuer das Pre-Patch
		// der PLT-Slots.
		//
		pReservedCode = pReservedCode + 0x10;
		//
		// Sichern der Instruktionen am Beginn des
		// Code-Segments.
		//
		for (Idx = 0; Idx < 5; Idx++)
		{
			backup[Idx] = pReservedCode[Idx];
		}

		if (reTryPrePatch_p == 0)
		{
			//
			// Das Pre-Patch startet normal am Ende der PLT.
			//
			processedPltSlots = 0;
		}
		else
		{
			//
			// Das Pre-Patch wurde erneut gestartet. Der zuletzt
			// betrachtete PLT-Slot wird uebersprungen.
			//
			processedPltSlots++;
		}
		//
		// Bearbeitung aller PLT-Slots, auszer dem ersten,
		// da dieser in das AOT-PLT-Trampoline springt.
		//
		for (; processedPltSlots < (iPltSize - 1); processedPltSlots++)
		{
			//
			// Ermittlung der Adrese des zu behandelnden PLT-Slots
			//
			pPltEntry = (CODEPTR) ((OFFSET) pPltEnd - ((processedPltSlots + 1) * pltEntrySize));

			if (!(((pPltEntry[0]) == (unsigned char) 0xe9)
					|| ((pPltEntry[0]) == (unsigned char) 0xeb)))
			{
				//
				// Der PLT-Slot enthaelt keine keine Jump-Instruktion.
				// Der naechste PLT-Slot wird betrachtet.
				//
				continue;
			}
			//
			// Ueberpruefung, ob die Jump-Instruktion im PLT-Slot an den
			// Sprung in das AOT-PLT-Trampoline, der am Beginn der PLT
			// steht, springt.
			//
			pPltSlotJmpTgt = getX86CallTarget(&pPltEntry[0]);
			if (pPltSlotJmpTgt == pPltStart)
			{
				//
				// Das Ziel der Jump-Instruktion im PLT-Slot ist
				// der Beginn der PLT.
				// Ueberpruefung, ob am Beginn der PLT eine
				// Jump-Instruktion existiert. Falls ja, dann
				// wird deren Ziel ermittelt.
				//
				if ((pPltStart[0]) == (unsigned char) 0xe9)
				{
					pPltSlotJmpTgt = getX86CallTarget(
							&pPltStart[0]);
				}
				else
				{
					//
					// Das Ziel der Jump-Instruktion im PLT-Slot ist der Beginn
					// der PLT. Dort ist kein Sprung vorhanden. Das weicht von dem
					// Normalfall ab. Das Pre-Patch dieser PLT wird abgebrochen.
					//
					break;
				}
			} else {
				//
				// Das Ziel der Jump-Instruktion im PLT-Slot ist
				// nicht der Beginn der PLT. Es kann sich um ein
				// normales Specific Trampoline handeln.
				//
				;
			}

			pSpecTramp = NULL;
			pSpecTramp = (struct MonoSpecificTrampElem *) g_hash_table_lookup(MonoSpecificTrampHashTable, pPltSlotJmpTgt);
			if (NULL == pSpecTramp)
			{
				//
				// Das Ziel der Jump-Instruktion im PLT-Slot ist
				// entweder nativer Code der zu rufenden Methode
				// oder ein unbekanntes Trampoline. Letzterer Fall
				// stellt eine Ausnahmesituation dar. Es wird der
				// naechste PLT-Slot betrachtet.
				//
				//printf("\n\tMono-RT - prePatchAotCode ()\tPLT-Slot springt nicht in ein Specific Trampoline. Setze mit naechsten PLT-Slot fort.\n");
				//fflush(NULL);
				continue;
			}
			//
			// Modifikation des Specific Trampolines, das Ziel der
			// Jump-Instruktion im PLT-Slot ist, so dass es in ein
			// modifiziertes Generic Trampoline springt, das in den
			// Pre-Patch-Code zurueckkehrt.
			//
			iRet = modifyX86SpecificTrampoline(pSpecTramp, NULL, PrePatchPltEntries);
			if (0 != iRet)
			{
				//
				// Das Specific Trampoline konnte nicht
				// modifiziert werden. Es wird mit dem
				// naechsten Slot fortgefahren.
				//
				continue;
			}
			//
			// Emittieren einer Call-Instruktion, die in
			// den zu behandelnden PLT-Slot springt.
			//
			pEmitCode = pReservedCode;
			x86_call_code(pEmitCode, &pPltEntry[0]);
			//
			// Ausloesen der emittierten Call-Instruktion.
			//
			triggerPrePatchCode(pReservedCode);
			//
			// Wiederherstellen des Specific Trampolines
			// am Beginn der PLT.
			//
			restoreX86SpecificTrampoline(pSpecTramp);
			//
			// Wenn der Programmfluss an dieser Stelle angekommen ist,
			// dann wurde ein Specific Trampoline im PLT-Slot behandelt.
			// Moeglicherweise verweist der PLT-Slot weiterhin auf ein
			// Specific Trampoline. Der eben behandelte PLT-Slot wird
			// erneut betrachtet.
			//
			processedPltSlots--;
		} // Ende: for (; processedPltSlots < (pltSize - 1); processedPltSlots++)
		//
		// Wiederherstellen des ueberschriebenen Bereichs
		// des Code-Segements des AOT-Images.
		//
		for (Idx = 0; Idx < 5; Idx++) {
			pReservedCode[Idx] = backup[Idx];
		}
		//
		// Wiederherstellen der urspruenglichen
		// Speicherzugriffsrechte am Anfang des
		// Code-Segments.
		//
		pReservedCode = pReservedCode - 0x10;
		iRet = mprotect((void *) pReservedCode, iPageSize, (PROT_READ
				| PROT_EXEC));

		if (-1 == iRet)
		{
			printf("\n\tMonoRT - prePatchAotCode ()\tErrror mprotect - ERRNO: %i\n", errno);
			fflush(NULL);
			free(backup);
			backup = NULL;
			return;
		}
		//
		// Betrachte das naechste geladene AOT-Image
		//
		pAotCode = pAotCode->next;
		//
		// Das naechste AOT-Image wird im fehlerfreien Fall
		// betrachtet, so dass das Pre-Patch normal beginnt.
		//
		reTryPrePatch_p = 0;
	} // Ende: while (aotCodeElem != NULL)

	free(backup);
	backup = NULL;
	printf("Pre-Patch of PLT finished\n");
	fflush(NULL);
	return;
#elif defined(TARGET_ARM)	// Archtitekturabhaengiger Pre-Patch-Code
	return;
#else						// Archtitekturabhaengiger Pre-Patch-Code
	return;
#endif
}
/**
 * @brief Startpunkt des Pre-Patch der waehrend des Pre-JIT
 * emittierten Patches.
 *
 * @ param reTryPrePatch	Ein Wert ungleich 0 gibt an, dass die Funktion wiederholt aufgerufen wurde
 *
 * prepatchPatches() untersucht jeden waehrend des Pre-JIT
 * emittierten relevanten Patch (siehe "mono_resolve_patch_target()"
 * in mini.c) und ruft die entsprechende Pre-Patch-Funktion auf.
 * Hat der Parameter "reTryPrePatch" einen Wert ungleich 0,
 * so ist waehrend des Pre-Patch eine Exception aufgetreten
 * und das Pre-Patch wurde erneut gestartet.
 */
static void prepatchPatches(int reTryPrePatch_p)
{
	//
	//  Initialisierung mit der Adresse der globalen
	//  Instanz eines Structs MonoCallCodeList, welches
	//  den Startpunkt einer Liste aufgezeichneter
	//  Call-Instruktionen darstellt (siehe pre.h).
	//
	static struct MonoCallCodeList * callCodeElem = &CallCodeList;
	static int fistTimeCalled = 0;

	if (fistTimeCalled == 0)
	{
		printf("Pre-Patch starts\n");
		fflush(NULL);
		fistTimeCalled = 1;
	}

	if (reTryPrePatch_p != 0)
	{
		//
		//  Falls das Pre-Patch wiederholt werden muss, so
		//  wird der als letztes betrachtete Patch uebersprungen,
		//  da dessen Behandlung offenbar zum Fehler fuehrte.
		//  Allerdings kann so nicht ausgeschlossen werden,
		//  das Mono-interne Datenstrukturen inkonsistent sind.
		//
		callCodeElem = callCodeElem->next;
	}
	else
	{
		;
	}

	if ((callCodeElem == NULL) || (callCodeElem->isInitialized == 0))
	{
		return;
	}
	//
	//  Betrachtung aller aufgezeichneter Patches
	//
	while (callCodeElem != NULL)
	{
		if (callCodeElem->m_pPatch == NULL)
		{
			callCodeElem = callCodeElem->next;
			continue;
		}

		switch (callCodeElem->patchType)
		{
		case MONO_PATCH_INFO_METHOD:
			prePatchSpecificJitTrampoline(callCodeElem);
			break;
		case MONO_PATCH_INFO_INTERNAL_METHOD:
			prePatchInternalMethodCall (callCodeElem);
			break;
		case MONO_PATCH_INFO_METHOD_JUMP:
			prePatchSpecificJumpTrampoline(callCodeElem);
			break;
		case MONO_PATCH_INFO_CLASS_INIT:
			prePatchSpecificClassInitTrampoline(callCodeElem);
			break;
		case MONO_PATCH_INFO_DELEGATE_TRAMPOLINE:
			prePatchDelegateTrampoline(callCodeElem);
			break;
		case MONO_PATCH_INFO_ICALL_ADDR:
			prePatchIcallTrampoline(callCodeElem);
			break;
		case MONO_PATCH_INFO_JIT_ICALL_ADDR:
			prePatchJitIcallTrampoline(callCodeElem);
			break;
		case MONO_PATCH_INFO_RGCTX_FETCH:
			prePatchRgctxFetchTrampoline(callCodeElem);
			break;
		case MONO_PATCH_INFO_GENERIC_CLASS_INIT:
			prePatchSpecificGenericClassInitTrampoline(callCodeElem);
			break;
		case MONO_PATCH_INFO_MONITOR_ENTER:
			prePatchMonitorEnterTrampoline(callCodeElem);
			break;
		case MONO_PATCH_INFO_MONITOR_EXIT:
			prePatchMonitorExitTrampoline(callCodeElem);
			break;
		default:
			callCodeElem = callCodeElem->next;
			continue;
		}
		callCodeElem = callCodeElem->next;
	}
//	printf("Pre-Patch of CALL instructions finished\n");
//	fflush(NULL);
}
/*
 * @brief:	prePatchSpecificJumpTrampoline() dient dem Pre-Patch
 * der emittierten Patches zu Jump Trampolinen.
 *
 * @param	pCallElem_p	Zeiger auf Verwaltungsstruktur
 * des aufgezeichneten Patches
 *
 * Nach der Code-Generierung einer Methode (in „mono_codegen()“)
 * werden fuer diese in „mono_postprocess_patches()“ u.a. die
 * Patch-Targets ihrer Jump-Patches inkl. der Adresse des Patches
 * in der Hash-Tabelle „jump_target_hash“ gespeichert. Der Key
 * ist dabei das Method-Handle des Jump-Targets.Danach wird
 * „mono_arch_patch_code()“ fuer die eben compilierte Methode
 * aufgerufen. Falls das Jump-Target bereits compiliert wurde,
 * wird direkt die Adresse als Sprungziel eingetragen. Falls
 * nicht, dann wird die Adresse eines Jump-Trampolines eingetragen.
 * Waehrend der JIT-Compilierung wird in „mono_jit_compile_method_inner()“
 * wird fuer eine eben compilierte Methode eine Abfrage der
 * Hash-Tabelle „jump_target_hash“ gemacht und eine Liste jener
 * Jump-Patches generiert, die die eben compilierte Methode als
 * Jump-Target haben. Diese Patches werden dann direkt behandelt,
 * indem wieder „mono_arch_patch_code()“ aufgerufen wird. Jedoch
 * ist jetzt die Adresse der anzuspringenden Methode bekannt
 * und sie wurde vorher im Magic Trampoline compiliert, wobei
 * evtl. Wrapper erzeugt wurden. Daher sind bei Pre-JIT keine
 * Jump-Trampoline notwendig. Allerdings kommt es zu einem Fehler,
 * wenn es sich um eine synchronized Methode handelt, da das
 * Methoden-Handle der Methode und nicht das des Wrappers
 * gespeichert wird. Fuer Methoden, die ueber einen Wrapper
 * aufgerufen werden, müssten nicht die Methoden-Handles der
 * Methoden, sondern die der Wrapper gespeichert werden. Momentan
 * wurde die Optimierung entfernt und Jump-Trampoline werden
 * immer durchlaufen, und im Magic Trampoline nicht gepatcht.
 * Das Jump-Trampoline wird jedoch auch im Rahmen des Pre-Patch
 * durchlaufen, um evtl. unbekannte Initialisierungen nicht
 * beim ersten Durchlauf waehrend der Laufzeit durchfuehren
 * zu muessen.
 */
static void prePatchSpecificJumpTrampoline (struct MonoCallCodeList * pCallElem_p)
{
	CODEPTR pJmpInst;
	CODEPTR pJmpTarget;
	struct MonoSpecificTrampElem * pSpecTramp;

	pJmpInst = NULL;
	pJmpTarget = NULL;
	pSpecTramp = NULL;

#if defined(TARGET_X86) // Archtitekturabhaengiger Pre-Patch-Code
	pJmpInst = pCallElem_p->m_pPatch;
	//
	// ** X86-spezifisch **
	//
	if (!(((*pJmpInst) == (unsigned char) 0xe9)
			|| ((*pJmpInst) == (unsigned char) 0xeb))) {
		//
		//  An der aufgezeichneten Position im nativen Code
		//  wurde keine Jump-Instruktion gefunden. Das Pre-Patch
		//  wird abgebrochen.
		//
		return;
	}
	if ((*pJmpInst) == (unsigned char) 0xe9)
	{
		pJmpTarget = getX86CallTarget(pJmpInst);
	}
	else
	{
		pJmpTarget = NULL;
	}
	//
	//  Ueberpruefung, ob das Ziel der Jump-Instruktion ein
	//  aufgezeichnetes Specific Trampoline ist.
	//
	pSpecTramp = (struct MonoSpecificTrampElem *) g_hash_table_lookup(MonoSpecificTrampHashTable, pJmpTarget);

	if ((pJmpTarget == pCallElem_p->m_pPatchTarget) && (pSpecTramp != NULL))
	{
		//
		//  Das Jump-Trampoline wurde noch nicht gepatcht.
		//
		prepatchCallInstruction(pJmpInst, pSpecTramp, pCallElem_p);
	}
	return;

#elif defined(TARGET_ARM)	// Archtitekturabhaengiger Pre-Patch-Code
	return;
#else						// Archtitekturabhaengiger Pre-Patch-Code
	return;
#endif
}
/*
 * @brief:	prePatchInternalMethodCall() dient dem Pre-Patch
 * der emittierten Patches zu Internal-JIT-Calls.
 *
 * @param	pCallElem_p	Zeiger auf Verwaltungsstruktur
 * des aufgezeichneten Patches
 *
 * Die Funktion prePatchInternalMethodCall() behandelt Patches
 * vom Typ "MONO_PATCH_INFO_INTERNAL_METHOD". Bei einem solchen
 * Patch wird in der Hash-Tabelle "jit_icall_hash_name" nach
 * einem Internal-JIT-Call gesucht und mit "mono_icall_get_wrapper()"
 * ein entsprechender Wrapper erzeugt. In der Regel sollte der
 * Wrapper bei aktiviertem Pre-Patch direkt vorhanden sein, da
 * er waehrend des Pre-JIT erzeugt und pre-compiliert wird.
 * Es kann dennoch nicht ausgeschlossen werden, dass das
 * Patch-Target ein JIT-Trampoline auf den Wrapper ist.
 */
static void prePatchInternalMethodCall (struct MonoCallCodeList * pCallElem_p)
{
	//
	//  Vermutlich unkritisch, da der aufgerufene Wrapper wahrscheinlich
	//  keine Veraenderungen am nativen Code vornimmt. Zudem wurden
	//  die Wrapper bereits waehrend des Pre-JIT compiliert.
	//
	//  Es wird ueberpueft, ob ein Specific Trampoline angesprungen wird.
	//  Das kann passieren, wenn in "mono_icall_get_wrapper_full()"
	//  der Wrapper nicht pre-compiliert wird und nur ein
	//  JIT-Trampoline erzeugt wird.
	//
	CODEPTR callInstructionPtr;
	CODEPTR callInstructionTarget;
	int isCallInstruction;
	struct MonoSpecificTrampElem * specificTrampElem;

#if defined(TARGET_X86) // Archtitekturabhaengiger Pre-Patch-Code
	callInstructionPtr = pCallElem_p->m_pPatch;
	isCallInstruction = isX86CallInstruction(callInstructionPtr);
	//
	// ** X86-spezifisch **
	//
	if (isCallInstruction == 1) {

		if (MonoSpecificTrampHashTable == NULL) {
			return;
		}
		callInstructionTarget = getX86CallTarget(callInstructionPtr);

		if (pCallElem_p->m_pPatchTarget == callInstructionTarget) {

			specificTrampElem = NULL;
			specificTrampElem
					= (struct MonoSpecificTrampElem *) g_hash_table_lookup(
							MonoSpecificTrampHashTable, callInstructionTarget);

			if (specificTrampElem == NULL) {
				//
				//  Dieser Pfad wird i.d.R. nicht betreten.
				//
				if (SNOOPTRAMP) {
					printf(
							"\tMono-RT - prePatchInternalMethodCall():\tCall-Ziel und Patch-Code stimmen ueberein, ist aber kein ST\n");
					fflush(NULL);
				} else {
					;
				}
			} else {
				//
				//  Das Pre-Patch der Call-Instruktion wird ausgefuehrt.
				//
				prepatchCallInstruction(callInstructionPtr, specificTrampElem,
						pCallElem_p);
			}
		}
	}
	return;

#elif defined(TARGET_ARM)	// Archtitekturabhaengiger Pre-Patch-Code
	return;
#else						// Archtitekturabhaengiger Pre-Patch-Code
	return;
#endif
}
/*
 * @brief:	prePatchDelegateTrampoline() dient dem Pre-Patch
 * der emittierten Patches zu Delegate Trampolines.
 *
 * @param	callCodeElem	Zeiger auf Verwaltungsstruktur
 * des aufgezeichneten Patches
 *
 * Der Aufruf eines Delegates erfolgt dann direkt ueber
 * "MonoDelegate->invoke_impl", wenn Delegate.Invoke()
 * aufgerufen wird. Ein Specific Delegate-Trampoline ist
 * Klassen- bzw. Delegate-Typ-spezifisch und nicht
 * Delegate-Instanz-spezifisch. D.h., eine Delegate-Instanz
 * kann erst gepatcht werden, wenn es initialisiert wurde.
 * Das geschieht erst zur Laufzeit und kann somit nicht im
 * Pre-Patch behandelt werden. Das Einfuegen einer Pruefung,
 * ob ein Delegate bereits gepatcht wurde, in den
 * Aufrufmechanismus ist evtl. moeglich. Die Delegate-Patches
 * sind vermutlich ausschlieszlich fuer AOT relevant.
 */
static void prePatchDelegateTrampoline(struct MonoCallCodeList * pCallElem_p)
{
#if defined(TARGET_X86)		// Archtitekturabhaengiger Pre-Patch-Code
	return;
#elif defined(TARGET_ARM)	// Archtitekturabhaengiger Pre-Patch-Code
	return;
#else						// Archtitekturabhaengiger Pre-Patch-Code
	return;
#endif
}
/*
 * @brief:	prePatchSpecificGenericClassInitTrampoline() dient
 * dem Pre-Patch der emittierten Patches zu Generic Class Init
 * Trampolinen. Generische Klassen sind jedoch nur bei der
 * Verwendung von Generics relevant.
 *
 * @param	callCodeElem	Zeiger auf Verwaltungsstruktur
 * des aufgezeichneten Patches
 */
static void prePatchSpecificGenericClassInitTrampoline(
		struct MonoCallCodeList * callCodeElem) {

#if defined(TARGET_X86) // Archtitekturabhaengiger Pre-Patch-Code
	return;

#elif defined(TARGET_ARM)	// Archtitekturabhaengiger Pre-Patch-Code
	return;
#else						// Archtitekturabhaengiger Pre-Patch-Code
	return;
#endif
}
/*
 * @brief:	prePatchMonitorEnterTrampoline() dient dem Pre-Patch
 * der emittierten Patches zu Monitor Enter Trampolinen.
 *
 * @param	callCodeElem	Zeiger auf Verwaltungsstruktur
 * des aufgezeichneten Patches
 */
static void prePatchMonitorEnterTrampoline(struct MonoCallCodeList * callCodeElem)
{
#if defined(TARGET_X86) // Archtitekturabhaengiger Pre-Patch-Code
	return; // betreten

#elif defined(TARGET_ARM)	// Archtitekturabhaengiger Pre-Patch-Code
	return;
#else						// Archtitekturabhaengiger Pre-Patch-Code
	return;
#endif
}
/*
 * @brief:	prePatchMonitorExitTrampoline() dient dem Pre-Patch
 * der emittierten Patches zu Monitor Exit Trampolinen.
 *
 * @param	callCodeElem	Zeiger auf Verwaltungsstruktur
 * des aufgezeichneten Patches
 */
static void prePatchMonitorExitTrampoline (struct MonoCallCodeList * callCodeElem)
{
#if defined(TARGET_X86) // Archtitekturabhaengiger Pre-Patch-Code
	return; // betreten
#elif defined(TARGET_ARM)	// Archtitekturabhaengiger Pre-Patch-Code
	return;
#else						// Archtitekturabhaengiger Pre-Patch-Code
	return;
#endif
}
/*
 * @brief:	prePatchRgctxFetchTrampoline() dient dem Pre-Patch
 * der emittierten Patches zu RGCTX Lazy Fetch Trampolinen.
 *
 * @param	callCodeElem	Zeiger auf Verwaltungsstruktur
 * des aufgezeichneten Patches
 *
 * Ein RGCTX (Runtime Generic Sharing Context) ist eine zentrale
 * Datenstruktur zur Realisierung des Code-Sharing bei generischem
 * Code. Zudem werden lt. Aussage auf der Mailing-Liste diese
 * Patches bzw. Trampoline nur dann benoetigt, wenn Generics
 * verwendet werden:
 *
 * http://lists.ximian.com/pipermail/mono-devel-list/2011-June/037633.html
 */
static void prePatchRgctxFetchTrampoline(struct MonoCallCodeList * callCodeElem)
{
#if defined(TARGET_X86) // Archtitekturabhaengiger Pre-Patch-Code
	return;
#elif defined(TARGET_ARM)	// Archtitekturabhaengiger Pre-Patch-Code
	return;
#else						// Archtitekturabhaengiger Pre-Patch-Code
	return;
#endif
}
/*
 * @brief:	prePatchJitIcallTrampoline()
 *
 * @param	callCodeElem	Zeiger auf Verwaltungsstruktur
 * des aufgezeichneten Patches
 *
 * Dieser Patch-Typ wird wahrscheinlich fuer x86 nur in
 * Verbindung mit AOT-Compilierung benoetigt.
 */
static void prePatchJitIcallTrampoline(struct MonoCallCodeList * callCodeElem)
{
#if defined(TARGET_X86) // Archtitekturabhaengiger Pre-Patch-Code
	return;
#elif defined(TARGET_ARM)	// Archtitekturabhaengiger Pre-Patch-Code
	return;
#else						// Archtitekturabhaengiger Pre-Patch-Code
	return;
#endif
}
/*
 * @brief:	prePatchIcallTrampoline()
 *
 * @param	callCodeElem	Zeiger auf Verwaltungsstruktur
 * des aufgezeichneten Patches
 *
 * Wird offenbar genutzt, wenn ein Internal Call direkt, d.h.,
 * ohne Wrapper gerufen werden soll. Zum Aufloesen des Targets
 * werden „mono_lookup_pinvoke_call()“ oder „mono_lookup_internal_call()“
 * (prueft Hash-Tabelle „icall_hash“) aufgerufen. Es wird vermutlich
 * die Adresse des nativen Codes des gesuchten Internal Calls
 * zurueckgegeben. Evtl. ist dieser Patch auch nur fuer AOT
 * relevant.
 */
static void prePatchIcallTrampoline (struct MonoCallCodeList * callCodeElem)
{
#if defined(TARGET_X86) // Archtitekturabhaengiger Pre-Patch-Code
	return;
#elif defined(TARGET_ARM)	// Archtitekturabhaengiger Pre-Patch-Code
	return;
#else						// Archtitekturabhaengiger Pre-Patch-Code
	return;
#endif
}
/**
 * @brief prePatchSpecificJitTrampoline() prueft, ob an
 * der erwarteten Code-Stelle eine Call-Instruktion
 * liegt und ob die Call-Instruktion auf ein aufgezeichnetes
 * Specific JIT Trampoline zeigt.
 *
 * @param	pCallElem_p		Zeiger auf Verwaltungsstruktur
 * des aufgezeichneten Patches, der die Call-Instruktion enthaelt
 *
 * Das Pre-Patch wird nicht ausgefuehrt, wenn an der
 * erwarteten Position keine CALL-Instruktion gefunden
 * wird. Waehrend des Pre-Patch direkter Methodenaufrufe
 * werden u.U. Methoden compiliert und/oder Trampoline-Code
 * ausgefuehrt. Die dabei emittierten Call-Instruktionen werden
 * an das Ende der CallCodeList angefuegt, so dass die Liste
 * nur ein einziges Mal bis zum Ende durchlaufen werden muss.
 * Es wird das Specific Trampoline bestimmt, welches von
 * der Call-Instruktion angesprungen werden soll. Konnte
 * ein Specific Trampoline gefunden werden, so wird mit
 * dem Pre-Patch dieser Call-Instruktion fortgesetzt.
 * Ruft die Call-Instruktion kein Specific Trampoline,
 * so wird mit der naechsten Call-Instruktion fortgefahren.
 */
static void prePatchSpecificJitTrampoline (struct MonoCallCodeList * pCallElem_p)
{
	static MonoMethod * pExcludedMethod0 = NULL;
	static MonoMethod * pExcludedMethod1 = NULL;
	CODEPTR pCallInst;
	CODEPTR pCallTarget;
	struct MonoNativeCodeElem * pNativeElem;
	struct MonoSpecificTrampElem * pSpecTramp;

	pCallInst = NULL;
	pCallTarget = NULL;
	pNativeElem = NULL;
	pSpecTramp = NULL;

#if defined(TARGET_X86) // Archtitekturabhaengiger Pre-Patch-Code
	if (((pExcludedMethod0 == NULL) || (pExcludedMethod1 == NULL)) &&
			(NULL != pCallElem_p->m_pCallerMeth))
	{
		//
		//  Das Pre-Patch von Call-Instruktionen in diesen Methoden verursacht
		//  immer einen Fehler, so dass sie uebersprungen werden.
		//  Der teuere String-Vergleich wird nur so lange
		//  durchgefuehrt, bis beide Methoden gefunden wurden.
		//  Dieser Pfad wird nur dann betreten, wenn nicht
		//  beide auszuschlieszenden Methoden gefunden wurden.
		//
		/*
		 * @todo: Untersuchung, warum dieser Fehler auftritt.
		 */
		if ((pExcludedMethod0 == NULL)
				&& (strcmp("(wrapper remoting-invoke-with-check) System.IO.UnmanagedMemoryStream:set_PositionPointer (byte*)",
						mono_method_full_name(pCallElem_p->m_pCallerMeth,
								TRUE)) == 0))
		{
			pExcludedMethod0 = pCallElem_p->m_pCallerMeth;
			return;
		}

		if ((pExcludedMethod1 == NULL)
				&& (strcmp("(wrapper remoting-invoke-with-check) System.IO.UnmanagedMemoryStream:Initialize (byte*,long,long,System.IO.FileAccess)",
						mono_method_full_name(pCallElem_p->m_pCallerMeth,
								TRUE)) == 0))
		{
			pExcludedMethod1 = pCallElem_p->m_pCallerMeth;
			return;
		}
	}
	//
	//  Ueberpruefung, ob diese Funktion fuer eine der beiden
	//  kritischen Methoden aufgerufen wurde.
	//
	if ((((void *) pCallElem_p->m_pCallerMeth) == ((void *) pExcludedMethod0))
			|| (((void *) pCallElem_p->m_pCallerMeth)
					== ((void *) pExcludedMethod1)))
	{
		return;
	}
	//
	//  Ueberpruefung, ob an der gespeicherten Adresse
	//  eine Call-Anweisung liegt. Jedoch wird nur der
	//  Opcode 0xe8 als gueltig akzeptiert.
	//
	/*
	 * @todo Ueberpruefung, ob andere Opcodes gueltig sind.
	 */
	pCallInst = pCallElem_p->m_pPatch;
	//
	// ** X86-spezifisch **
	//
	if (isX86CallInstruction(pCallInst) != 1)
	{
		//
		//  An der erwarteten Stelle wurde keine Call-Anweisung gefunden.
		//  Das koennte bedeuten, dass ein Fehler vorliegt, oder dass
		//  die Call-Anweisung eliminiert wurde.
		//
		//  Dieser Fall tritt fuer die mscorlib.dll-1.0 nicht ein.
		//
		;
	}
	else
	{
		//
		//  Die Call-Instruktion wurde an der erwarteten Stelle
		//  im nativen Code einer pre-compilierten Methode gefunden.
		//
		//  Ausloesen des Pre-Patch der gefundenen Call-Instruktion
		//
		if (SNOOPTRAMP)
		{
			pNativeElem = (struct MonoNativeCodeElem *) g_hash_table_lookup(MonoNativeCodeHashTable, (gconstpointer) pCallElem_p->m_pCallerMeth);
			if (pNativeElem != NULL)
			{
				printf("Native Code\tBeginn: %p\tEnde: %p\tName: %s\n",
						pNativeElem->nativeCodeFinalAddress,
						(pNativeElem->nativeCodeFinalAddress
								+ pNativeElem->nativeCodeCodeLength - 1),
						mono_method_full_name(
								pNativeElem->nativeCodeMethodPtr, TRUE));
				fflush(NULL);
			} else {
				printf(
						"\tKeine passende Methode zu Call-Anweisung gefunden. Ueberspringe.\n");
				fflush(NULL);
			}
		}
		else
		{
			;
		}
		//
		//  Bestimmung des Call-Ziels. Ueberpruefung, ob es ein aufgezeichnetes
		//  und damit gueltiges Specific Trampoline ist.
		//
		//  ** X86-spezifisch **
		//
		pCallTarget = getX86CallTarget(pCallInst);
		pSpecTramp = (struct MonoSpecificTrampElem *) g_hash_table_lookup(MonoSpecificTrampHashTable, pCallTarget);
		//
		//  Ueberpruefung, ob das Ziel der Call-Instruktion mit
		//  der Adresse des Codes zur Behandlung des Patches
		//  uebereinstimmt. Das kann sich aendern, wenn das
		//  JIT-Trampoline bereits ausgefuehrt wurde, oder
		//  wenn ein Fehler vorliegt.
		//
		if (pCallElem_p->m_pPatchTarget == pCallTarget)
		{
			//
			//  Das Ziel der Call-Anweisung stimmt mit dem aufgezeichneten
			//  Ziel des Patches ueberein. Es wird davon ausgeganen, dass
			//  es sich dabei um ein Specific Trampoline handelt.
			//
			if (pSpecTramp != NULL)
			{
				//
				//  Das Pre-Patch der Call-Instruktion wird ausgefuehrt.
				//
				prepatchCallInstruction(pCallInst, pSpecTramp, pCallElem_p);
			}
			else
			{
				;
			}
		}
		else
		{
			//
			//  Das Ziel der Call-Instruktion stimmt nicht mit
			//  der Anfangsadresse des Codes zur Behandlung des
			//  Patches ueberein. Das deutete darauf hin,
			//  dass der Call bereits ausgefuehrt wurde.
			//
			if (pSpecTramp != NULL)
			{
				//
				//  Dieser Pfad wird i.d.R. nicht betreten.
				//
				prepatchCallInstruction(pCallInst, pSpecTramp, pCallElem_p);
			}
			if (SNOOPTRAMP) {
				printf("\tTarget of call inst is %s a specific trampoline.\n", ((pSpecTramp != NULL) ? " " : "not"));
				fflush(NULL);
			}
		}
	}
	return;

#elif defined(TARGET_ARM)	// Archtitekturabhaengiger Pre-Patch-Code
	return;
#else						// Archtitekturabhaengiger Pre-Patch-Code
	return;
#endif
}
/**
 * @brief prePatchSpecificClassInitTrampoline() prueft,
 * ob an der erwarteten Code-Stelle eine Call-Instruktion
 * liegt und ob die Call-Instruktion auf ein aufgezeichnetes
 * Specific Class Init Trampoline zeigt.
 *
 * @param	pCallElem_p		Zeiger auf Verwaltungsstruktur
 * des aufgezeichneten Patches, der die Call-Instruktion enthaelt
 */
void prePatchSpecificClassInitTrampoline (struct MonoCallCodeList * pCallElem_p)
{
	//
	//  Es werden zahlreiche CLASS_INIT-Trampoline
	//  waehrend des Pre-Patch gefunden. Diese
	//  werden ausgefuehrt. Die genaue Funktionsweise
	//  dieser Trampoline ist noch bekannt. Vermutlich
	//  dienen sie dem Aufruf von Klassen-Konstruktoren.
	//
	//  Wurden ein Class Init Trampoline ausgefuehrt,
	//  so wird die aufrufende Stelle mit NOP-Instruktionen
	//  ueberschrieben, da eine Klasse nur einmal initialisiert
	//  werden muss.
	//
	CODEPTR pCallInst;
	CODEPTR pCallTarget;
	struct MonoNativeCodeElem * pNativeElem;
	struct MonoSpecificTrampElem * pSpecTramp;

	pCallInst = NULL;
	pCallTarget = NULL;
	pNativeElem = NULL;
	pSpecTramp = NULL;

#if defined(TARGET_X86) // Archtitekturabhaengiger Pre-Patch-Code
	//
	//  Ueberpruefung, ob an der gespeicherten Adresse
	//  eine Call-Anweisung liegt. Jedoch wird nur der
	//  Opcode 0xe8 als guetlig akzeptiert.
	//
	/*
	 * @todo Ueberpruefung, ob andere Opcodes gueltig sind.
	 */
	pCallInst = pCallElem_p->m_pPatch;
	//
	// ** X86-spezifisch **
	//
	if (isX86CallInstruction(pCallInst) == 1)
	{
		//
		//  Die Call-Instruktion wurde an der erwarteten Stelle
		//  im nativen Code einer pre-compilierten Methode gefunden.
		//
		//  Ausloesen des Pre-Patch der gefunden Call-Instruktion
		//
		if (SNOOPTRAMP)
		{
			printf("MonoRT - prePatchSpecificClassInitTrampoline(): Class Init Call in\n\t%s\n",
					mono_method_full_name(pCallElem_p->m_pCallerMeth, TRUE));
			fflush(NULL);
		} else {
			;
		}
		//
		//  Bestimmung des Call-Ziels. Ueberpruefung, ob es ein aufgezeichnetes
		//  und damit gueltiges Specific Trampoine ist.
		//
		//  ** X86-spezifisch **
		//
		pCallTarget = getX86CallTarget(pCallInst);
		pSpecTramp = (struct MonoSpecificTrampElem *) g_hash_table_lookup(MonoSpecificTrampHashTable, pCallTarget);
		//
		//  Ueberpruefung, ob das Ziel der Call-Instruktion mit
		//  der Adresse des Codes zur Behandlung des Patches
		//  uebereinstimmt. Das kann sich aendern, wenn das
		//  Class Init Trampoline bereits ausgefuehrt wurde, oder
		//  wenn ein Fehler vorliegt.
		//
		if (pCallElem_p->m_pPatchTarget == pCallTarget)
		{
			//
			//  Es wurde ein Specific Trampoline gefunden,
			//  welches zum Call-Ziel passt. Fahre mit dem
			//  Pre-Patch fort.
			//
			if (pSpecTramp == NULL)
			{
				if (SNOOPTRAMP) {
					printf(
							"Mono-RT - prePatchSpecificClassInitTrampoline(): Call-Ziel und Patch-Code stimmen ueberein, ist aber kein ST\n");
					fflush(NULL);
				} else {
					;
				}
			} else {
				//
				//  Das Pre-Patch des Class Init Trampolines wird ausgefuehrt.
				//
				prepatchCallInstruction(pCallInst, pSpecTramp,
						pCallElem_p);
			}
		} else {
			//
			//  Das Ziel der Call-Instruktion stimmt nicht mit
			//  der Anfangsadresse des Codes zur Behandlung des
			//  Patches ueberein. Das deutete darauf hin,
			//  dass der Call bereits ausgefuehrt wurde.
			//
			if (pSpecTramp != NULL) {
				//
				//  Dieser Pfad wird i.d.R. nicht betreten.
				//
				prepatchCallInstruction(pCallInst, pSpecTramp,
						pCallElem_p);
			}
			if (SNOOPTRAMP) {
				printf(
						"Mono-RT - prePatchSpecificClassInitTrampoline(): Keine Uebereinstimmung. Call-Ziels ist ST: %s.\n",
						((pSpecTramp != NULL) ? "ja" : "nein"));
				fflush(NULL);
			}
		}
	}
	return;

#elif defined(TARGET_ARM)	// Archtitekturabhaengiger Pre-Patch-Code
	return;
#else						// Archtitekturabhaengiger Pre-Patch-Code
	return;
#endif
}
/**
 * @brief prepatchCallInstruction() fuehrt das Pre-Patch einer Call-Instruktion aus.
 * @param	callInstructionPtr	Zeiger auf die zu patchende Call-Instruktion
 * @param	specificTrampElem	Zeiger auf Verwaltungsstruktur des Specific Trampoline, das angesprungen wird
 * @param	callCodeElem		Zeiger auf Verwaltungsstruktur der Methode, die die Call-Instruktion enthaelt
 *
 * Es wird der Typ des Specific Trampoline bestimmt.
 * Bei einem Specific JIT-Trampoline wird geprueft, ob die
 * aufzurufende Methode bereits JIT-compiliert wurde.
 * Wurde die aufzurufende Methode noch nicht JIT-compiliert,
 * so wird das veranlasst, falls es sich um keine  abstrakte
 * Methode handelt. Anschlieszend wird ein, dem Typ des Specific
 * Trampoline entsprechendes, modifiziertes Generic Trampoline
 * erzeugt. Dieses springt in den Pre-Patch-Code zurueck, anstatt
 * in den Code der die Call-Instruktion enthaelt bzw. in die
 * Methode, die aufgerufen werden soll (Specific JIT Trampoline).
 * Das Specific Trampoline muss dann so veraendert werden,
 * damit es das modifizierte Generic Trampoline anspringt.
 * Das Specific Trampoline wird zuvor gesichert um spaeter
 * wieder hergestellt zu werden. Anschlieszend wird die
 * Call-Instruktion ausgefuehrt und evtl. durch Monos
 * Trampoline-Code veraendert, so dass sie bei der ersten
 * Ausfuehrung (zur Laufzeit) direkt den nativen Code der
 * aufzurufenden Methode anspringt (bswp. Specific JIT
 * Trampoline) oder gar nicht ausgefuehrt werden muss (bspw. Class
 * Init Trampoline). Um das Pre-Patch direkter Methodenaufrufe
 * per Specific JIT Trampoline zu beschleunigen wird die Adresse
 * der aufzurufenden Methode vermerkt. Falls ein Aufruf dieser
 * Methode durch das gleiche Specific JIT Trampoline eine weiteres
 * Mal gefunden wird, so kann die Call-Instruktion direkt
 * modifiziert werden, ohne Trampoline-Code auszufuehren.
 */
static void prepatchCallInstruction(CODEPTR pCallInst_p,
		struct MonoSpecificTrampElem * pSpecTrampElem_p,
		struct MonoCallCodeList * pCallElem_p)
{
	//
	//  Zaehler der Pre-Patches fuer das Debugging.
	//
	static int patchedSpecificTrampNr = 0;
	//
	//  Absolute Adresse des Ziels der Call-Instruktion
	//  bzw. Anfangsadresse des Specific Trampoline
	//
	CODEPTR pCallTgt;
	//
	//  Handle der Methode, die aufgerufen werden soll,
	//  falls eine Methode gerufen werden soll.
	//
	MonoMethod * pCalledMeth;
	struct MonoNativeCodeElem * pNativeElem;
	struct MonoVTableElem * pVtableElem;
	MonoVTable * pVtable;
	int isInternalCall;
	int isRuntimeCall;
	int isPInvoke;
	int isAbstract;
	int isJumpTrampoline;
	int modSuccess;

	pCallTgt = NULL;
	pCalledMeth = NULL;
	pNativeElem = NULL;
	pVtableElem = NULL;
	pVtable = NULL;
	isInternalCall = 0;
	isRuntimeCall = 0;
	isPInvoke = 0;
	isAbstract = 0;
	isJumpTrampoline = 0;
	modSuccess = 0;

#if defined(TARGET_X86) // Archtitekturabhaengiger Pre-Patch-Code
	pCallTgt = pSpecTrampElem_p->specificTrampAddress;
	//
	//  Ausgabe von Debug-Informationen zum Specific Trampoline
	//
	if (SNOOPTRAMP)
	{
		printf("\t");
		printListElement(pCallTgt, pSpecTrampElem_p, SpecificTrampoline);
		printf("\tmodified: %x\tST-Nr: %i\n",
				pSpecTrampElem_p->m_IsPatched,
				pSpecTrampElem_p->number);
		fflush(NULL);
	}

	switch (pSpecTrampElem_p->specificTrampType)
	{
	case (MONO_TRAMPOLINE_JUMP):
		isJumpTrampoline = 1;
	case (MONO_TRAMPOLINE_JIT):
		//
		//  Das Specific Trampoline legt zuerst einen Zeiger auf das
		//  Handle der aufzurufenen Methode auf den Stack. Dieser Zeiger
		//  wird ermittelt.
		//
		pCalledMeth = ((MonoMethod *) (*((OFFSET *) (pCallTgt + 1))));
		//
		//  Ueberpruefung, ob die Initialisierung der Klasse bereits
		//  fehlschlug. Falls ja, dann wird das Pre-Patch diese
		//  Call-Anweisung abgebrochen.
		//  Waehrend des Pre-Patch wird eine Call-Anweisung im nativen
		//  Code des JIT-compilierten managed Codes ausgefuehrt. Tritt
		//  waehrend des Pre-Patch eine Exception auf, so sucht der
		//  Exception-Mechanismus in Mono so lange bis ein Block gefunden
		//  wird, der die Exception behandeln kann. Ist die Call-Anweisung,
		//  die ihm Rahmen des Pre-Patch ausgefuehrt wird, beispielsweise
		//  in einen Try-Catch-Block eingebettet, so wird die Exception
		//  in diesem Catch-Block behandelt und nicht bis in in den
		//  Catch-Block des Assemblys "preJitPrePatchMain-rXXX.exe"
		//  durchgereicht. Das kann dazu fuehren, dass die Mono-Runtime
		//  abstuerzt.
		//
		pVtableElem = (struct MonoVTableElem *) g_hash_table_lookup(MonoVTHashTable, (gconstpointer) pCalledMeth->klass);

		if ((pVtableElem != NULL) && (pVtableElem->vtable->init_failed != 0))
		{
			return;
		}
		else
		{
			if (pVtableElem == NULL)
			{
				return;
			}
			else
			{
				;
			}
		}
		//
		//  Ueberpruefung, ob die gerufene Methode bereits pre-compiliert
		//  wurde.
		//
		pNativeElem = (struct MonoNativeCodeElem *) g_hash_table_lookup(MonoNativeCodeHashTable, (gconstpointer) pCalledMeth);
		if (pNativeElem == NULL)
		{
			//
			//  Diese Methode wurde nicht durch das Pre-JIT erfasst.
			//
			//  Falls es sich nicht um eine abstrakte Methode handelt,
			//  wird sie vor der Ausfuehrung des Specific Trampolines
			//  compiliert.
			//
			isAbstract = (int) (pCalledMeth->flags & METHOD_ATTRIBUTE_ABSTRACT);

			if (SNOOPTRAMP)
			{
				isInternalCall = 0;
				isRuntimeCall = 0;
				isPInvoke = 0;
				isInternalCall = (int) (pCalledMeth->iflags & METHOD_IMPL_ATTRIBUTE_INTERNAL_CALL);
				isPInvoke = (int) (pCalledMeth->flags & METHOD_ATTRIBUTE_PINVOKE_IMPL);
				isRuntimeCall = (int) (pCalledMeth->iflags & METHOD_IMPL_ATTRIBUTE_RUNTIME);
				printf("CALL-PP: %s nicht pre-compiliert.\tabstract: %s\tinternal call: %s\tPInvoke: %s\tRuntime: %s\n",
						mono_method_full_name(pCalledMeth, TRUE), ((isAbstract
								== 0) ? "nein" : "ja"),
						((isInternalCall == 0) ? "nein" : "ja"), ((isPInvoke
								== 0) ? "nein" : "ja"),
						((isRuntimeCall == 0) ? "nein" : "ja"));
				fflush(NULL);
			}

			if ((pCalledMeth != NULL) && (pCalledMeth->klass != NULL) && (isAbstract == 0))
			{
				mono_compile_method(pCalledMeth);
				pNativeElem = (struct MonoNativeCodeElem *) g_hash_table_lookup(MonoNativeCodeHashTable, (gconstpointer) pCalledMeth);
			}
			else
			{
				printf(
						"\n\tMono-RT - prepatchCallInstruction(): %s.%s.%s nicht compilierbar.\n",
						((pCalledMeth->klass != NULL) ? (pCalledMeth->klass->name_space)
								: "-"),
						((pCalledMeth->klass != NULL) ? (pCalledMeth->klass->name)
								: "-"),
						((pCalledMeth != NULL) ? (pCalledMeth->name) : "-"));
				fflush(NULL);
				return;
			}
		}
		break;
	case (MONO_TRAMPOLINE_CLASS_INIT):
		//
		//  Das Specific Trampoline legt zuerst einen Zeiger auf die
		//  VTable der zu initialisierenden Klasse auf den Stack.
		//  Dieser Zeiger wird ermittelt.
		//
		pVtable = ((MonoVTable *) (*((OFFSET *) (pCallTgt + 1))));
		if (pVtable->init_failed != 0) {
			//
			// Der statische Konstruktor konnte nicht
			// ausgefuehrt werden.
			//
			return;
		}
		//
		//  Sicherstellen, dass nicht der optimierte Pfad
		//  fuer bereits behandelte Specific JIT Trampoline
		//  betreten wird.
		//
		pSpecTrampElem_p->m_IsPatched = 0x0;
		break;
	case (MONO_TRAMPOLINE_GENERIC_CLASS_INIT):
		printf("MONO_TRAMPOLINE_GENERIC_CLASS_INIT\n");
		break;
	case (MONO_TRAMPOLINE_RGCTX_LAZY_FETCH):
		printf("MONO_TRAMPOLINE_RGCTX_LAZY_FETCH\n");
		break;
	case (MONO_TRAMPOLINE_RESTORE_STACK_PROT):
		printf("MONO_TRAMPOLINE_RESTORE_STACK_PROT\n");
		break;
	case (MONO_TRAMPOLINE_GENERIC_VIRTUAL_REMOTING):
		printf("MONO_TRAMPOLINE_GENERIC_VIRTUAL_REMOTING\n");
		break;
	case (MONO_TRAMPOLINE_MONITOR_ENTER):
		printf("MONO_TRAMPOLINE_MONITOR_ENTER\n");
		break;
	case (MONO_TRAMPOLINE_MONITOR_EXIT):
		printf("MONO_TRAMPOLINE_MONITOR_EXIT\n");
		break;
	case (MONO_TRAMPOLINE_NUM):
		printf("MONO_TRAMPOLINE_NUM\n");
	default:
		break;
	}
	fflush(NULL);
	//
	//  Wurde ein Specific Trampoline bereits behandelt,
	//  so ist das modified-Flag gesetzt und das Sprungziel
	//  im nativen Code muss lediglich ausgetauscht werden.
	//
	if ((pSpecTrampElem_p->m_IsPatched == 0x1) &&
			(pSpecTrampElem_p->specificTrampType == MONO_TRAMPOLINE_JIT))
	{
		//
		//  Das gefundene Specific Trampoline wurde bereits
		//  modifiziert. Manuelle Ersetzung des Sprungziels
		//  anstatt Ausfuehrung des Specific Trampoline
		//  und Betrachtung der naechsten Call-Anweisung.
		//
		// ** X86-spezifisch **
		//
		modifyX86CallInstructionTarget(pCallInst_p, pSpecTrampElem_p->specificTrampCallInstructionFinalTarget);

		if (SNOOPTRAMP) {
			printf(
					"\tNeues call-Ziel\tIst: %p\tSoll: %p\n\n",
					(CODEPTR) (((OFFSET) pCallInst_p)
							+ (*((OFFSET *) (pCallInst_p + 1))) + 5),
					(pSpecTrampElem_p->specificTrampCallInstructionFinalTarget));
			fflush(NULL);
		}
		//
		//  Das Ziel der Call-Instruktion wurde durch die
		//  gespeicherte Adresse ersetzt. Das Ausfuehren von
		//  Trampoline-Code war nicht notwendig.
		//
	}
	else
	{
		//
		//  Das gefundene Specific Trampoline wurde noch nicht betrachtet und/oder
		//  es handelt sich nicht um ein JIT-Trampoline. An dieser Stelle wurde
		//  im nativen Pre-JIT-compilierten Code der Aufruf eines Specific
		//  Trampoline identifiziert:
		//
		//  - callInstructionPtr: absolute Adresse des Aufrufs des Specific
		//  Trampoline im nativen Code
		//
		//  - callInstructionTarget: absolute Adresse des Specific Trampoline,
		//  welches aufgerufen werden soll
		//
		//  - specificTrampElem->specificTrampAddress: absolute Adresse des Specific
		//  Trampoline, welches aufgerufen werden soll
		//
		//  - genericTrampElem->genericTrampAddress: absolute Adresse des Generic
		//  Trampoline, welches ersetzt werden soll
		//
		//  Damit das Pre-Patchen korrekt arbeitet wird so vorgegangen:
		//
		//	1. Aenderung des Sprung-Ziels des Specific Trampoline,
		//	so dass es in ein passendes Modified Generic Trampoline springt.
		//
		//	2. Ausfuehrung des modifizierten Specific Trampoline
		//
		//	3. Sichern der absoluten Adresse des nun gepatchten Call-Ziels
		//	im nativen Code und Markierung des urspruenglichen Specifc
		//	Trampoline als modifiziert.
		//
		//	4. Wiederherstellen des modifizierten Specific Trampoline.
		//	Dieser Schritt koennte auch bis an das Ende des Pre-Patch
		//	verschoben werden, wenn sichergestellt wird, dass waehrend
		//	des Pre-Patch kein Aufruf des gerade zu patchenden Specific
		//  Trampolines geschieht.
		//
		// Beginn 1.
		modSuccess = modifyX86SpecificTrampoline(pSpecTrampElem_p, pCallElem_p, PrePatchCallInstructions);

		if (modSuccess != 0) {
			return;
		}
		// Ende 1.
		//
		if (SNOOPTRAMP) {
			printf("\t\tcall %p\tan %p\n",
					getX86CallTarget(pCallInst_p), pCallInst_p);
		}
		//
		// Beginn 2.
		triggerPrePatchCode(pCallInst_p);
		// Ende 2.
		//
		// Beginn 3.
		//
		//  Merken der absoluten Ziel-Adresse, die nach dem Patchen der
		//  Call-Instruktion durch die Trampoline berechnet wurde
		//
		//  Specific Trampoline, deren zugeordnete Methoden das Attribut
		//  'Synchronized' tragen, speichern nicht die in den nativen Code
		//  geschriebene Adresse als neues Ziel der Call-Instruktion.
		//  Waehrend des Pre-Patch einer Synchronized Methode wird ein
		//  Wrapper fuer den Methodenaufruf generiert und JIT-compiliert.
		//  Dieser Wrapper enthaelt u.a. den eigentlichen Aufruf der Methode,
		//  welcher wieder durch dieses Specific Trampoline erfolgt.
		//  Dieses Specific Trampoline wird spaeter noch einmal betrachtet,
		//  da die neu erzeugte Call-Instruktion der CallCodeList
		//  angehaengt wird. So wuerde als Ziel der Call-Instruktion
		//  innerhalb des Wrappers die Adresse des Wrappers eingetragen,
		//  was zur Laufzeit eine unendliche Rekursion verursacht.
		//
		if ((pSpecTrampElem_p->specificTrampType == MONO_TRAMPOLINE_JIT)
				&& (pNativeElem != NULL)
				&& (!((pNativeElem->nativeCodeMethodPtr->iflags)
						& METHOD_IMPL_ATTRIBUTE_SYNCHRONIZED))) {

			pSpecTrampElem_p->specificTrampCallInstructionFinalTarget
					= getX86CallTarget(pCallInst_p);
			pSpecTrampElem_p->m_IsPatched = 0x1;
		} else {
			;
		}
		// Ende 3.
		//
		// Beginn 4.
		//
		//  Wiederherstellung des Specific Trampoline, so dass es
		//  in ein unmodifiziertes Generic Trampoline des korrekten
		//  Typs springt.
		//
		restoreX86SpecificTrampoline(pSpecTrampElem_p);
		// Ende 4.

		if (SNOOPTRAMP) {
			printf(
					"\t\tRueckkehr aus Sprung\tNr. patched Specific Tramp: %i\n",
					patchedSpecificTrampNr);
			printf("\t\tNeues call-Ziel im nativen Code: %p\n\n",
					pSpecTrampElem_p->specificTrampCallInstructionFinalTarget);
			fflush(NULL);
		} else {
			;
		}
		//
		//  Fuer statistische Debug-Ausgaben.
		//
		patchedSpecificTrampNr++;
	} // Ende: if ( specificTrampElem->m_IsPatched == 0x1 )
	return;

#elif defined(TARGET_ARM)	// Archtitekturabhaengiger Pre-Patch-Code
	return;
#else						// Archtitekturabhaengiger Pre-Patch-Code
	return;
#endif
}
/**
 * @brief Startpunkt des Pre-Patch der IMT- und VTable-Eintraege.
 *
 * @ param	reTryPrePatch	Ein Wert ungleich 0 gibt an, dass die Funktion wiederholt aufgerufen wurde
 *
 * prepatchAllClassVtables() stellt den Startpunkt
 * des Pre-Patch der IMT- und VTable-Eintraege aller
 * Klassen dar. Das erfolgt fuer jede Klasse getrennt,
 * da eine Interface Method Table bzw. VTable
 * klassenspezifisch ist.
 */
static void prepatchAllClassVtables(int reTryPrePatch_p)
{
#if defined(TARGET_X86) // Archtitekturabhaengiger Pre-Patch-Code
	static struct MonoClassList * classListElem = &ClassList;
	static CODEPTR reservedCode;
	static int initialized = 0;

	if ((reTryPrePatch_p == 0) || (initialized == 0))
	{
//		printf("Pre-Patch VT/IMT-Trampoline starts\n");
//		fflush(NULL);
		initialized = 1;
		//
		// Initialisierung der Array-Variante der Standard-Klassen
		//
		initializeAllDefaultClasses();
		//
		//  Reservierung von Speicher, in den Maschinencode
		//  emittiert wird, der zum Ausloesen des Pre-Patch eines
		//  IMT- oder VTable-Slots dient.
		//
		//  Die Reservierung erfolgt mittels einer Mono-internen
		//  Funktion, die Speicher fuer JIT-compilierten Code
		//  reserviert. Eine Reservierung mittels malloc() o.ae.
		//  ist nicht ohne Weiteres moeglich, da sich so reservierter
		//  Speicher nicht ohne Loeschen des NX-Bits der CPU ausfuehren
		//  laesst (es wird ein SIGSEV generiert).
		//
		//  Leider ist momentan nicht bekannt, wie dieser Speicher
		//  wieder freigegeben werden kann. Daher wird er hier
		//  angelegt und bis zu "emitAndstartCallSequence()"
		//  durchgereicht, anstatt dort stets neuen Speicher
		//  zu reservieren.
		//
		reservedCode = mono_domain_code_reserve(mono_domain_get(), (40
				* sizeof(char)));

		if (reservedCode == NULL) {
			printf("Pre-Patch IMT: could not allocated memory.\n");
			printf("Pre-Patch finished\n\n");
			fflush(NULL);
			return;
		}
	} else {
		if (initialized != 1) {
			printf("Mono-RT - Fehler in prepatchAllClassVtables(). Ende.\n");
			fflush(NULL);
			exit(1);
		} else {
			;
		}
		//
		//  Waehrend des Pre-Patch ist ein Fehler aufgetreten.
		//  Die zuletzt untersuchte Klasse wird uebersprungen.
		//
		classListElem = classListElem->next;
	}
	//
	//  Iteration ueber alle Eintraege der MonoClassHashTable, in der
	//  Verwaltungsstrukturen aller Klassen der Pre-JIT-compilierten
	//  Methoden enthalten sind.
	//
	//  Die Klassen werden in einer Liste gespeichert, damit sie
	//  immer in der gleichen Reihenfolge abgearbeitet werden.
	//  Eine Iteration mittels g_hash_table_foreach() fuehrt zu
	//  einer nicht vorhersagbaren Reihenfolge der Abarbeitung der
	//  Klassen, was insbesondere fuer das Debugging hinderlich war.
	//
	while ((classListElem != NULL) && (classListElem->klasse != NULL))
	{
		prepatchClassVTable(classListElem, reservedCode, reTryPrePatch_p);
		classListElem = classListElem->next;
	}
	printf("Pre-Patch finished\n\n");
	fflush(NULL);
	return;

#elif defined(TARGET_ARM)	// Archtitekturabhaengiger Pre-Patch-Code
	//
	// Initialisierung der Array-Variante der Standard-Klassen
	//
	initializeAllDefaultClasses ();
	return;
#else						// Archtitekturabhaengiger Pre-Patch-Code
	return;
#endif
}
/**
 * @brief prepatchClassVTable() startet das Pre-Patch der
 * IMT-/Vtable-Eintraege einer einzelnen Klasse.
 * @param	classListElem	Zeiger auf Verwaltungsstruktur der Klasse, deren VTable behandelt wird
 * @param	reservedCode	Zeiger auf Speicherbereich fuer das Aufrufmuster des Pre-Patch
 * @param	reTryPrePatch	Ein Wert ungleich 0 gibt an, dass die Funktion wiederholt aufgerufen wurde
 *
 * Es werden alle Slots der VTable einer Klasse
 * betrachtet. Fuer jeden Slot wird geprueft, ob der
 * VTable-Slot bereits benutzt wurde (was ein Pre-Patch
 * ueberfluessig macht), ob die klassenspezifische
 * Implementierung der zugehoerigen Methode pre-compiliert
 * wurde und ob der VTable-Slot zu einer Interface-Methode
 * gehoert. Gehoert der VTable-Slot zu einer
 * Interface-Methode, so muss das Pre-Patch durch den
 * IMT-Slot gestartet werden.
 */
static void prepatchClassVTable (struct MonoClassList * pClassElem_p, CODEPTR pCode_p, int reTryPrePatch_p)
{
	int counter;
	int counter2;
	int vtableSlotNr;
	int interfaceMethodVtableSlot;
	int isInterfaceMethodSlot;
	int isInternalCall;
	int isRuntimeCall;
	int isPInvoke;
	int isAbstract;
	CODEPTR pVtableSLot;
	MonoMethod * pMethod;
	MonoClass * pClass;
	struct MonoVTableElem * pVtableElem;
	struct MonoNativeCodeElem * pCodeElem;

	counter = 0;
	counter2 = 0;
	vtableSlotNr = 0;
	interfaceMethodVtableSlot = 0;
	isInterfaceMethodSlot = 0;
	isInternalCall = 0;
	isRuntimeCall = 0;
	isPInvoke = 0;
	isAbstract = 0;
	pVtableSLot = NULL;
	pMethod = NULL;
	pClass = NULL;
	pVtableElem = NULL;
	pCodeElem = NULL;

#if defined(TARGET_X86) // Archtitekturabhaengiger Pre-Patch-Code
	pVtableElem = (struct MonoVTableElem *) g_hash_table_lookup(MonoVTHashTable,
			(gconstpointer)(pClassElem_p->klasse));
	//
	//  Ueberpruefung, ob die VTable erfasst wurde.
	//
	if (pVtableElem == NULL)
	{
		mono_class_vtable(mono_domain_get(), pClassElem_p->klasse);

		pVtableElem = (struct MonoVTableElem *) g_hash_table_lookup(
				MonoVTHashTable, (gconstpointer)(pClassElem_p->klasse));

		if (pVtableElem == NULL) {
			//			if (SNOOPCALLVIRT) {
			printf(
					"\n\n\t\tMono-RT - prepatchClassVTable (): Keine VTable fuer Klasse %s\n",
					pClassElem_p->klasse->name);
			fflush(NULL);
			//			}
			return;
		}
	} else {
		if (pVtableElem->vtable->init_failed != 0) {
			return;
		} else {
			;
		}
	}

	if (pClassElem_p->klasse->exception_type == 0) {
		mono_class_setup_vtable(pClassElem_p->klasse);
	}

	if (pClassElem_p->klasse->exception_type != 0) {
		printf(
				"\n\n\t\tMono-RT - prepatchClassVTable ():Fehler bei VTable-Init fuer Klasse %s\n",
				pClassElem_p->klasse->name);
		fflush(NULL);
		return;
	}
	//
	//  Ueberpruefung, ob die Position der Interface Method Table (IMT)
	//  zu der VTable passt. Die IMT besteht aus 19 Eintraegen (siehe
	//  mono/metadata/object-internals.h) mit jeweiliger Laenge von
	//  4 Byte. Die IMT liegt direkt vor der zugehoerigen Struktur
	//  MonoVTable im Speicher.
	//
	if (pClassElem_p->klasse->interface_offsets_count > 0) {
		if ((pVtableElem->vTableFirstIMTSlot + (4 * 19))
				!= ((CODEPTR) pVtableElem->vtable)) {
			printf(
					"Die Position der IMT passt nicht zur VTable in Klasse %s. Ueberspringe.\n",
					pClassElem_p->klasse->name);
			fflush(NULL);
			return;
		} else {
			;
		}
	} else {
		;
	}

	//	printf("%s\n", classListElem->klasse->name);
	//	fflush(NULL);
	//
	// Erzeugung und Initialisierung der Array-Varianten der Klasse,
	// die gerade bearbeitet wird.
	//
	initializeArrayVariantOfClass(NULL, (gpointer) pClassElem_p->klasse, NULL);

	if ((SNOOPCALLVIRT)) {
		printf(
				"Klasse: %s\tVT-Size: %i\tIF-Count: %i\tMC: %i\tIFO-Count: %i\n\n",
				pClassElem_p->klasse->name,
				pClassElem_p->klasse->vtable_size,
				(int) pClassElem_p->klasse->interface_count,
				pClassElem_p->klasse->method.count,
				pClassElem_p->klasse->interface_offsets_count);
		fflush(NULL);

		for (counter = 0; counter
				< pClassElem_p->klasse->interface_offsets_count; counter++) {
			printf(
					"\tInterface: %s\t\tIOffset: %i\tIMC: %i\n",
					pClassElem_p->klasse->interfaces_packed[counter]->name,
					pClassElem_p->klasse->interface_offsets_packed[counter],
					pClassElem_p->klasse->interfaces_packed[counter]->method.count);
			fflush(NULL);

			for (counter2 = 0; counter2
					< pClassElem_p->klasse->interfaces_packed[counter]->method.count; counter2++) {
				printf(
						"\t\tMethode: %s (%p)\tSlot: %i\tVT-Slot: %i\n",
						pClassElem_p->klasse->interfaces_packed[counter]->methods[counter2]->name,
						pClassElem_p->klasse->interfaces_packed[counter]->methods[counter2],
						pClassElem_p->klasse->interfaces_packed[counter]->methods[counter2]->slot,
						(pClassElem_p->klasse->interface_offsets_packed[counter]
								+ pClassElem_p->klasse->interfaces_packed[counter]->methods[counter2]->slot));
			}
		}
	}
	//
	//  Iteration ueber alle VTable-Slots der Klasse
	//
	for (vtableSlotNr = 0; vtableSlotNr < pClassElem_p->klasse->vtable_size; vtableSlotNr++) {
		//
		//  Bestimmung des Inhaltes des VTable-Slots
		//
		pVtableSLot
				= ((CODEPTR) (*((OFFSET *) (((CODEPTR) pVtableElem->vtable)
						+ (G_STRUCT_OFFSET(MonoVTable, vtable)) + (4
						* vtableSlotNr)))));
		//
		//  Ueberspringe den VTable-Slot, falls er nicht mehr die Adresse des
		//  VTable-Trampolines enthaelt. Das ist der Fall, wenn ueber diesen
		//  VTable-Slot bereits ein Funktionsaufruf stattfand. Die Adresse
		//  der IMT- und VTable-Trampoline wurden bei Generierung der
		//  Trampoline aufgezeichnet.
		//
		if (pVtableSLot != pVTableTramp_g)
		{
			if (SNOOPCALLVIRT) {
				printf("\t\t\tKein VTable-Trampoline im VTable-Slot Nr. %i\n",
						vtableSlotNr);
				fflush(NULL);
			} else {
				;
			}
			continue;
		}
		//
		//  Ueberpruefung, ob die dem VTable-Slot zugeordnete Methode
		//  pre-compiliert wurde. Falls nicht, so wird das Pre-Patch
		//  nicht durchgefuehrt, da das Mono Magic Trampoline versucht,
		//  die Methode zu compilieren. Da die fragliche Methode
		//  bereits waehrend dem Pre-JIT unberuecksichtigt blieb, so
		//  koennte eine Compilierung im Rahmen des Pre-Patch einen
		//  Fehler verursachen.
		//
		//  Die Bestimmung der Methode, die einem bestimmten VTable-Slot
		//  zugeordnet wurde, erfolgt ueber die Funktion
		//  'mono_class_get_vtable_entry'. Diese wird u.a. vom Magic
		//  Trampoline zur Bestimmung der gerufenen Methode bei
		//  Aufrufen ueber die VTable verwendet.
		//
		/**
		 * @todo Ueberpruefung der nicht-compilierten Methoden einfuegen, um sie
		 * ggf. dennoch zu compilieren.
		 */
		pMethod = mono_class_get_vtable_entry(pClassElem_p->klasse,
				vtableSlotNr);

		if (pMethod == NULL) {
			//
			//  Es konnte keine Methode bestimmt werden, die
			//  diesem VTable-Slot zurgeordnet wurde.
			//
			//  Durchsuchen der Vererbungshierachie von unten nach oben,
			//  beginnend bei der aktuell betrachteten Klasse, nach
			//  der zu dem VTable-Slot gehoerenden Methode.
			//
			//  Moeglicherweise kann das zu einem Fehler fuehren, WENN die
			//  Methode deshalb nicht gefunden werden kann, weil sie noch nicht
			//  compiliert wurde. Wird nun in einer der Elternklassen eine
			//  Methode in dem VTable-Slot gefunden, so koennte faelschlicherweise
			//  angenommen werden, die Methode sei vererbt, und nicht
			//  ueberschrieben/ueberdeckt, so dass nach dem Pre-Patch die
			//  Implementierung einer einer Eltern-Klasse gerufen wird
			//  anstatt die zu diesem Zeitpunkt noch nicht compilierte
			//  Implementierung der Kind-Klasse.
			//
			for (counter = ((int) (pClassElem_p->klasse->idepth) - 1); (counter
					>= 0 && (pMethod == NULL)); counter--) {
				//
				//  Aufstieg in der Klassenhierarchie. Dabei werden Felder der
				//  Mono-internen Klassen-Verwaltungsstruktur verwendet.
				//
				pClass = pClassElem_p->klasse->supertypes[counter];

				if (vtableSlotNr >= pClass->vtable_size) {
					//
					//  Die VTable, welche virtuelle Methoden und auch
					//  Interface-Methoden speichert, wird beim Erben
					//  von einer Klasse von dieser Elternklasse uebernommen
					//  und erweitert bzw. modifiziert, bspw. durch Ueberrschreiben.
					//  So hat die (Eltern-) Klasse keine groeszere VTable
					//  als die Slot-Nummer der Methode in der Kind-Klasse.
					//
					//  Abbruch der Suche, da uebergeordnete Klassen ihre
					//  VTable vererben. Besitzen Kind-Klassen VTable-Slots,
					//  die die Groesze der VTable ihrer Eltern-Klasse(n)
					//  uebersteigen, so besitzt die Eltern-Klasse nicht die
					//  diesem VTable-Slot zugeordnete Methode.
					//
					break;
				} else {
					//
					//  Suche der Methode im VTable-Slot einer uebergeordneten Klasse.
					//  Verwendung der gleichen VTable-Slot-Nummer.
					//
					pMethod = mono_class_get_vtable_entry(pClass,
							vtableSlotNr);
				}

				if (pMethod == NULL) {
					//
					//  Es konnte in der Eltern-Klasse keine Methode dem
					//  VTable-Slot zugeordnet werden. Nun wird die Methodentabelle
					//  der Eltern-Klasse nach einer passenden Methode durchsucht.
					//
					for (counter2 = 0; ((counter2
							< pClass->method.count) && (pMethod
							== NULL)); counter2++) {

						if (((pClass->methods[counter2]->flags
								& METHOD_ATTRIBUTE_VIRTUAL) != 0)
								&& (pClass->methods[counter2]->slot
										== vtableSlotNr)) {

							pMethod = pClass->methods[counter2];
						} else {
							;
						}
					}
				} else {
					;
				}
			}

			if (pMethod == NULL) {
				//
				//  Dem VTable-Slot konnte weder in der betrachteten Klasse selbst
				//  noch in einer ihrer Eltern-Klassen eine Methode zugeordnet werden.
				//  Fahre mit der Untersuchung des naechsten VTable-Slots fort.
				//
				//  Diese Stelle wird waehrend des Pre-Patch der mscorlib.dll-1.0
				//  nicht betreten.
				//
				if (SNOOPCALLVIRT) {
					printf("%s\tVT-Slot: %i\tMethode nicht gefunden.\n",
							pClassElem_p->klasse->name, vtableSlotNr);
					fflush(NULL);
				} else {
					;
				}
				continue;
			} else {
				//
				//  Dem VTable-Slot konnte eine Methode zugeordnet werden.
				//
				if (SNOOPCALLVIRT) {
					printf(
							"\tVT-Slot: %i\tMethode %s\tKlasse %s (org.: %s).\n",
							vtableSlotNr, pMethod->name,
							pClass->name,
							pClassElem_p->klasse->name);
					fflush(NULL);
				} else {
					;
				}
			}
		} else {
			;
		}
		//
		//  Falls es sich um eine abstrakte Methode handelt,
		//  wird mit dem naechsten VTable-Slot fortgesetzt.
		//
		isInternalCall = 0;
		isRuntimeCall = 0;
		isPInvoke = 0;
		isAbstract = 0;
		isInternalCall = (int) (pMethod->iflags
				& METHOD_IMPL_ATTRIBUTE_INTERNAL_CALL);
		isPInvoke = (int) (pMethod->flags & METHOD_ATTRIBUTE_PINVOKE_IMPL);
		isRuntimeCall = (int) (pMethod->iflags & METHOD_IMPL_ATTRIBUTE_RUNTIME);
		isAbstract = (int) (pMethod->flags & METHOD_ATTRIBUTE_ABSTRACT);

		if (isAbstract != 0) {
			if (SNOOPCALLVIRT) {
				printf("\t\tVT-Slot: %i\tAbstrakte Methode %s.\n",
						vtableSlotNr, mono_method_full_name(pMethod, TRUE));
				fflush(NULL);
			} else {
				;
			}
			continue;
		} else {
			;
		}

		//
		//  An dieser Stelle wurde die dem VTable-Slot zugeordnete
		//  Method bestimmt. Ueberpruefung, ob die dem
		//  VTable-Slot zugeordnete Methode pre-compiliert wurde.
		//
		pCodeElem = (struct MonoNativeCodeElem *) g_hash_table_lookup(
				MonoNativeCodeHashTable, (gconstpointer) pMethod);
		/*
		 if (nativeCodeElem == NULL) {

		 if ((isInternalCall == 0) && (isPInvoke == 0) && (isRuntimeCall == 0)) {
		 //
		 //  Die Methode ist weder ein Internal Call, ein PInvoke,
		 //  noch ein Runtime Call (abstrakte Methoden wurden bereits
		 //  vorher herausgefiltert). Die Methode, die durch den
		 //  untersuchten VTable-Slot aufgerufen werden soll, wurde
		 //  aus irgendeinem Grund noch nicht pre-compiliert.
		 //  Das Pre-Patch dieses VTable-Slots wird nicht fortgesetzt.
		 //
		 if (SNOOPCALLVIRT) {
		 printf("\tVT-Slot: %i\tMethode %s %s nicht pre-compiliert (Abstrakt: %s).\n",
		 vtableSlotNr, classListElem->klasse->name,
		 method->name, ((isAbstract == 0) ? "nein" : "ja"));
		 fflush(NULL);
		 } else {
		 ;
		 }
		 continue;
		 } else {
		 //
		 // Die Methode wurde nicht pre-compiliert, weil sie nicht
		 // in C#-Code vorliegt, sondern in C implementiert ist.
		 //
		 ;
		 }
		 } else {
		 if (nativeCodeElem->nativeCodeMethodPtr != method) {
		 //
		 //  Sicherheitscheck um Fehler bei der Erfassung
		 //  der pre-compilierten Methoden zu erkennen.
		 //  Dieser Pfad wird beim Pre-Patch der mscorlib.dll-1.0
		 //  nicht betreten.
		 //
		 if (SNOOPCALLVIRT) {
		 printf("\tVT-Slot: %i\tMethode %s %s ungleich %s.\n",
		 vtableSlotNr, classListElem->klasse->name,
		 method->name, mono_method_full_name(
		 nativeCodeElem->nativeCodeMethodPtr, TRUE));
		 fflush(NULL);
		 } else {
		 ;
		 }
		 continue;
		 } else {
		 ;
		 }
		 }
		 */
		//
		// Die Methode wurde pre-compiliert, so dass das
		// Pre-Patch fortgesetzt werden kann.
		//
		if (SNOOPCALLVIRT) {
			printf(
					"\tVT-Slot: %i\tClass-Impl.: %s\tInternal Call: %s\tRuntime Call: %s\tPInvoke: %s\tAbstract: %s\n",
					vtableSlotNr, mono_method_full_name(pMethod, TRUE),
					((isInternalCall == 0) ? "nein" : "ja"), ((isRuntimeCall
							== 0) ? "nein" : "ja"), ((isPInvoke == 0) ? "nein"
							: "ja"), ((isAbstract == 0) ? "nein" : "ja"));
			fflush(NULL);
		} else {
			;
		}
		//
		//  Ueberpruefung, ob der VTable-Slot zu einer Interface-Methode gehoert.
		//  Falls ja, dann muss das Pre-Patch ueber den IMT-Slot eingeleitet werden.
		//
		isInterfaceMethodSlot = 0;
		//
		//  Das Feld 'interface_offsets_count' der Mono-internen
		//  Klassen-Verwaltungsstruktur gibt die Anzahl aller Interfaces
		//  einer Klasse an. Jedem Interface einer Klasse ist ein Offset
		//  innerhalb der VTable der Klasse zugeordnet. Das bedeutet, dass
		//  eine Interface-Methode neben einem VTable-Slot, der weiter
		//  oben untersucht wurde und dem eine Methode erfolgreich
		//  zugeordnet werden konnte, einen Slot in der Interface Method
		//  Table (IMT) der Klasse besitzt. Falls es sich um eine
		//  Interface-Methode handelt, so muss dieser IMT-Slot bestimmt
		//  werden.
		//
		for (counter = 0; counter
				< pClassElem_p->klasse->interface_offsets_count; counter++) {
			//
			//  Ueberspringe Interfaces, deren Bereich innerhalb der VTable nicht
			//  infrage kommt. Die Offsets der einzelnen Interfaces einer Klasse
			//  sind in dem Feld 'interface_offsets_packed[]' der Mono-internen
			//  Klassen-Verwaltungsstruktur gespeichert. Wird auf den Offset eines
			//  Interfaces die Anzahl der Methoden addiert, so erhaelt man den von
			//  dem Interface belegten Bereich der VTable einer Klasse.
			//
			//  Hinweis: das ist nur dann korrekt, wenn die Methoden eines
			//  Interfaces einen zusammenhaengenden Bereich der VTable belegen.
			//
			if ((vtableSlotNr
					< pClassElem_p->klasse->interface_offsets_packed[counter])
					|| (vtableSlotNr
							> (pClassElem_p->klasse->interface_offsets_packed[counter]
									+ pClassElem_p->klasse->interfaces_packed[counter]->method.count))) {
				continue;
			}
			//
			//  Es konnte ein Interface bestimmt werden, in dessen belegten
			//  Bereich der VTable der untersuchte VTable-Slot liegt. Nun wird
			//  die Methodentabelle des Interfaces durchsucht und fuer jede
			//  Methode der VTable-Slot berechnet, der ihr zugeordnet wurde.
			//
			for (counter2 = 0; counter2
					< pClassElem_p->klasse->interfaces_packed[counter]->method.count; counter2++) {

				interfaceMethodVtableSlot
						= pClassElem_p->klasse->interface_offsets_packed[counter]
								+ pClassElem_p->klasse->interfaces_packed[counter]->methods[counter2]->slot;

				if (interfaceMethodVtableSlot == vtableSlotNr) {
					//
					//  Einer Interface-Methode der Klasse konnte der untersuchte
					//  VTable-Slot zugeordnet werden. Die Suche kann abgebrochen
					//  werden.
					//
					isInterfaceMethodSlot = 1;
					break;
				} else {
					;
				}
			}
			if (isInterfaceMethodSlot == 1) {
				break;
			} else {
				;
			}
		}

		if (isInterfaceMethodSlot == 1) {
			//
			//  Der VTable-Slot gehoert zu einer Interface-Methode.
			//  Das Pre-Patch wird nun ueber den IMT-Slot gestartet.
			//
			prepatchImtEntry(
					pClassElem_p,
					pClassElem_p->klasse->interfaces_packed[counter]->methods[counter2],
					vtableSlotNr, pVtableElem, pCodeElem, pCode_p);
		} else {
			//
			//  Der VTable-Slot gehoert zu einer virtuellen Methode.
			//  Der Methode besitzt keinen IMT-Slot. Das Pre-Patch
			//  wird nun ueber den VTable-Slot gestartet.
			//
			prepatchVtableEntry(pClassElem_p, pMethod, vtableSlotNr,
					pVtableElem, pCode_p);
		}
	}

#elif defined(TARGET_ARM)	// Archtitekturabhaengiger Pre-Patch-Code
	return;
#else						// Archtitekturabhaengiger Pre-Patch-Code
	return;
#endif
}
/**
 * @brief prepatchImtEntry() fuehrt das Pre-Patch eines einzelnen
 * IMT-Slots einer Klasse aus.
 * @param	classListElem	Zeiger auf die Verwaltungsstruktur der Klasse des IMT-Slots
 * @param	interfaceMethod	Zeiger auf das generische Methoden-Handle der Interface-Methode, die den VTable-Slot nutzt
 * @param	vtableSlotNr	VTable-Slot-Nummer, die von der Interface Methode benutzt wird
 * @param	nativeCodeElem	Zeiger auf die Verwaltungsstruktur der Klassenimplementierung der Interface-Methode
 * @param	reservedCode	Zeiger auf den Speicherbereich fuer das Aufrufmuster des Pre-Patch
 *
 * Diese Funktion fuehrt das Pre-Patch fuer einen IMT-Eintrag einer
 * Klasse aus. Diese Funktion wird nur gerufen, wenn sichergestellt
 * wurde, dass es sich bei der zu dem VTable-Slot gehörenden Methode
 * um eine Interface-Methode handelt, der VTable-Slot noch das
 * VTable-Trampoline enthaelt und die Klassenimplementierung
 * der Interface-Methode Pre-compiliert wurde. Es wird nun
 * der korrespondierende IMT-Slot bestimmt, auf Kollisionen
 * ueberprueft und das Pre-Patch ggf. ausgelöst.
 */
static void prepatchImtEntry (struct MonoClassList * pClassElem_p,
		MonoMethod * pIfMethod_p, int iSlot_p,
		struct MonoVTableElem * pVtableElem_p,
		struct MonoNativeCodeElem * pCodeElem_p, CODEPTR pCode_p)
{
	static int patchCounter = 0;
	int iCallOffset;
	int imtSlotContainsImtTrampAddr;
	int imtSlotCollisions;
	int prePatchStatusCode;
	int interfaceMethodImtSlot;
	int modSuccess;
	CODEPTR pImtSlot;
	struct MonoSpecificTrampElem * pSpecTramp;

	iCallOffset = 0;
	imtSlotContainsImtTrampAddr = NULL;
	imtSlotCollisions = 0;
	prePatchStatusCode = 0;
	interfaceMethodImtSlot = 0;
	modSuccess = 0;
	pImtSlot = NULL;
	pSpecTramp = NULL;
#if defined(TARGET_X86) // Archtitekturabhaengiger Pre-Patch-Code
	//
	//  Bestimmung des IMT-Slots der Interface-Methode.
	//  Dieser Slot wird benötigt, um spaeter beim Start
	//  des Pre-Patch in den richtigen IMT-Slot zu springen.
	//  Zur Bestimmung des IMT-Slots wird die Mono-interne
	//  Funktion 'mono_method_get_imt_slot()' genutzt, die
	//  im Magic Trampoline zur Behandlung von Interface-Methoden
	//  Verwendung findet.
	//
	//  Alternativ könnte man den IMT-Slot einer Interface-Methode
	//  bei der Emittierung deren Aufrufs erfassen:
	//
	//	mono_emit_method_call_full()
	//
	//  in mono/mini/method-to-ir.c. Dabei werden jedoch nur genau die
	//  Interface-Methoden erfasst, fuer die auch eine Call-Instruktion
	//  emittiert wird.
	//
	interfaceMethodImtSlot = mono_method_get_imt_slot(pIfMethod_p);

	if ((interfaceMethodImtSlot < 0) || (interfaceMethodImtSlot > 18)) {
		//
		//  Die IMT hat eine feste Groesze von 19 Slots. Bei der
		//  Bestimmung des IMT-Slots der Interface-Methode ist
		//  ein Fehler aufgetreten. UEberspringe das Pre-Patch
		//  dieser Methode.
		//
		if (SNOOPCALLVIRT) {
			printf("\t\tIMT-Slot inkorrekt. Ueberspringe Methode.\n");
			fflush(NULL);
		}
		return;
	}
	if (SNOOPCALLVIRT) {
		printf("\t\tIM: %s\tMH: %p\tIMT-Slot: %i\tVT: %p\tIVT-Slot: %i\n",
				pIfMethod_p->name, pIfMethod_p, interfaceMethodImtSlot,
				((CODEPTR) pVtableElem_p->vtable), pIfMethod_p->slot);
		fflush(NULL);
	}
	//
	//  Bestimmung, ob in dem IMT-Slot Kollisionen auftreten koennen.
	//  Die Kollisions-Bitmap einer IMT wird im Magic Trampoline
	//  gesetzt, sobald eine Interface-Methode, die durch den entsprechenden
	//  IMT-Slot gerufen wird, ausgefuehrt wird bzw. ein Pre-Patch
	//  durchgefuehrt wird. Zur Kollisionserkennung wird das Feld
	//  'imt_collisions_bitmap' der Mono-internen VTable-Verwaltungsstruktur
	//  verwendet.
	//
	imtSlotCollisions = ((((pVtableElem_p->vtable->imt_collisions_bitmap
			>> interfaceMethodImtSlot) & 1) == 1) ? 1 : 0);
	//
	//  Berechnung des Inhaltes des IMT-Slots.
	//
	// ** X86-spezifisch **
	//
	pImtSlot
			= ((CODEPTR) (*((OFFSET *) ((CODEPTR) (pVtableElem_p->vTableFirstIMTSlot
					+ (interfaceMethodImtSlot * 4))))));
	//
	//  Berechnung, ob der IMT-Slot die Adresse des
	//  IMT-Trampolines enthaelt.
	//
	imtSlotContainsImtTrampAddr = ((pImtSlot == pImtTramp_g) ? 1 : 0);

	prePatchStatusCode = (2 * imtSlotCollisions) + imtSlotContainsImtTrampAddr;
	//
	//  Moegliche Werte fuer prePatchStatusCode:
	//
	//	prePatchStatusCode == 0:
	//		- Der IMT-Slot enthaelt nicht mehr die Adresse des IMT-Trampolines.
	//		- Fuer den IMT-Slot wurden bisher keine Kollisionen festgestellt.
	//
	//		Falls der IMT-Slot bereits modifiziert wurde, dann enthaelt er
	//		entweder die Adresse eines IMT-Tunks oder die Adresse der
	//		auszufuehrenden Methode. Falls er die Adresse eines IMT-Thunks
	//		enthaelt, dann muesste eine Kollision angezeigt werden (Fehlerfall).
	//		Falls der IMT-Slot die Adresse der auszufuehrenden Methode enthaelt
	//		handelt es sich um einen Normalfall. Pre-Patch wird nicht
	//		ausgefuehrt und mit der naechsten Methode fortgefahren.
	//
	//
	//	prePatchStatusCode == 1:
	//		- Der IMT-Slot enthaelt die Adresse des IMT-Trampolines.
	//		- Fuer den IMT-Slot wurden bisher keine Kollisionen festgestellt.
	//
	//		Das stellt einen Normalfall dar. Durch diesen IMT-Slot wurde
	//		noch kein Methodenaufruf bzw. Pre-Patch durchgefuehrt. Das
	//		IMT-Trampoline wird modifiziert, so dass es in ein Generic JIT
	//		Trampoline springt, das in den Pre-Patch-Code zurueckkehrt,
	//		und das Pre-Patch ausgefuehrt.
	//
	//
	//	prePatchStatusCode == 2:
	//		- Der IMT-Slot enthaelt nicht mehr die Adresse des IMT-Trampolines.
	//		- Fuer den IMT-Slot wurden Kollisionen festgestellt.
	//
	//		Der IMT-Slot der IM wurde bereits benutzt. Entsprechend den
	//		vorangegangenen Pruefungen muss der VTable-Slot der Interface-Methode
	//		die Adresse des VTable-Trampolines enthalten. Das VTable-Trampoline
	//		wird modifiziert und das Pre-Patch durchgefuehrt.
	//
	//
	//	prePatchStatusCode == 3:
	//		- Der IMT-Slot enthaelt die Adresse des IMT-Trampolines.
	//		- Fuer den IMT-Slot wurden Kollisionen festgestellt.
	//
	//		Fehlerfall. Wenn Kollisionen im IMT-Slot festgestellt
	//		wurden, dann kann nicht mehr die Adresse des IMT-Trampoline
	//		im IMT-Slot stehen.
	//
	if (SNOOPCALLVIRT) {
		printf("\t\t\t\tStatuscode IMT-Pre-Patch: %i\n", prePatchStatusCode);
		fflush(NULL);
	}
	switch (prePatchStatusCode) {
	case 0:
		if (pImtSlot != pCodeElem_p->nativeCodeFinalAddress) {
			//
			//  Fehlerfall, da keine Kollision angezeigt wurde,
			//  aber auch nicht die Adresse des nativen Codes
			//  der Klassenimplementierung der Interface-Methode
			//  im IMT-Slot steht. Fahre ohne Pre-Patch mit
			//  naechster Methode fort. Dieser Fall trat bisher
			//  nicht auf.
			//
			if (SNOOPCALLVIRT) {
				printf(
						"\t\t\t\tFehler: IMT-Slot enthaelt nicht Adresse der Methode.\n");
				fflush(NULL);
			}
		} else {
			//
			//  Die IMT-Slot enthaelt bereits die Adresse der
			//  gerufenen Interface-Methode. Normalfall. Fahre
			//  mit der naechsten Interface-Methode fort.
			//
		}
		pSpecTramp = NULL;
		break;
	case 1:
		//
		//  Lookup des IMT-Trampolines in der Hash-Table der
		//	Specific Trampoline. Dieses spezielle Specific
		//	JIT Trampoline wird modifiziert.
		//
		pSpecTramp
				= (struct MonoSpecificTrampElem *) g_hash_table_lookup(
						MonoSpecificTrampHashTable,
						(gconstpointer) pImtTramp_g);
		break;
	case 2:
		//
		//  Lookup des VTable-Trampolines in der Hash-Table der
		//	Specific Trampoline. Dieses spezielle Specific
		//	JIT Trampoline wird modifiziert.
		//
		pSpecTramp = (struct MonoSpecificTrampElem *) g_hash_table_lookup(MonoSpecificTrampHashTable, (gconstpointer) pVTableTramp_g);
		break;
	case 3:
	default:
		pSpecTramp = NULL;
	}

	if (pSpecTramp == NULL) {
		if (SNOOPCALLVIRT) {
			printf(
					"\t\t\t\tIMT-Trampoline nicht gefunden, kein Pre-Patch oder Fehlerfall. Ueberpsringe.\n");
			fflush(NULL);
		}
		return;
	}
	//
	//  Der IMT-Slot der Interface-Methode zeigt auf ein
	//  IMT-Trampoline oder auf einen IMT-Thunk. Entsprechend
	//  wurde ein IMT- oder VTable-Trampoline zur Modifizierung
	//  ausgewaehlt. Das Trampoline wird modifiziert und das
	//  Pre-Patch ausgeloest.
	//
	modSuccess = modifyX86SpecificTrampoline(pSpecTramp, NULL,
			PrePatchImtEmtries);

	if (modSuccess != 0) {
		return;
	}

	if (SNOOPCALLVIRT) {
		printf("\t\t\t\tIMT-Pre-Patch Nr. %i\t%s (%s)\n", ++patchCounter,
				mono_method_full_name(pIfMethod_p, TRUE),
				pClassElem_p->klasse->name);
	}
	//
	//  Berechnung des Offsets des IMT-Slots bezueglich der Mono-internen
	//  VTable-Verwaltungsstruktur der Klasse ('MonoVTable'), die die
	//  Interface-Methode enthaelt. Der Offset ist negativ, da die IMT
	//  vor der VTable-Verwaltungsstruktur im Speicher liegt. Dieser
	//  Offset wird fuer das Auslösen des Pre-Patch benötigt, da die
	//  Call-Instruktion in den zuvor berechneten IMT-Slot springen soll.
	//
	//  Das Pre-Patch wird stets durch den IMT-Slot ausgelöst. Das
	//  VTable-Trampoline wird dann modifiziert, wenn im IMT-Slot
	//  Kollisionen auftreten und der Aufruf der Interface-Methode
	//  letztendlich durch einen VTable-Slot geht.
	//
	/**
	 * @todo Berechnung des Offsets mit Architekturpruefung realisieren.
	 */
	iCallOffset = (0 - ((19 - interfaceMethodImtSlot) * 4));
	//
	//  Emittierung der Aufrufsequenz einer Interface-Methode und
	//  Auslösen des Pre-Patch.
	//
	emitAndstartCallSequence((CODEPTR) pIfMethod_p, iCallOffset,
			(CODEPTR) pVtableElem_p->vtable, pCode_p);
	//
	//  Wiederherstellung des zuvor modifizierten IMT- oder
	//  VTable-Trampolines. Theoretisch koennte das auch erst
	//  nach dem Abschluss des IMT-/VTable-Pre-Patch geschehen,
	//  wenn sichergestellt werden könnte, das waehrend des
	//  Pre-Patch dieses Trampoline nicht angesprungen wird.
	//
	/**
	 * @todo Untersuchung, ob das Wiederherstellen der IMT-/VTable-Trampoline
	 * nach Abschluss des IMT-/VTable-Pre-Patch erfolgen kann.
	 */
	restoreX86SpecificTrampoline(pSpecTramp);

	return;

#elif defined(TARGET_ARM)	// Archtitekturabhaengiger Pre-Patch-Code
	return;
#else						// Archtitekturabhaengiger Pre-Patch-Code
	return;
#endif
}
/**
 * @brief prepatchVtableEntry() fuehrt das Pre-Patch eines einzelnen
 * VTable-Slots einer Klasse aus.
 * @param	classListElem	Zeiger auf die Verwaltungsstruktur der Klasse des VTable-Slots
 * @param	virtualMethod	Zeiger auf das Methoden-Handle der Methode, die durch den VTable-Slot gerufen werden soll
 * @param	vtableSlotNr	VTable-Slot-Nummer, die von der virtuellen Methode benutzt wird
 * @param	vtableElem		Zeiger auf Verwaltungsstruktur der VTable der Klasse, deren VTable-Slot behandelt wird
 * @param	reservedCode	Zeiger auf den Speicherbereich fuer das Aufrufmuster des Pre-Patch
 *
 * Pre-Patch eines VTable-Slots einer Klasse. Diese
 * Methode wird nur gerufen, wenn sichergestellt wurde,
 * dass der VTable-Slot noch das VTable-Trampoline
 * enthaelt und dass die entsprechende Methode Pre-compiliert
 * wurde.
 */
static void prepatchVtableEntry (struct MonoClassList * pClassElem_p,
		MonoMethod * pVirtMethod_p, int iSlot_p,
		struct MonoVTableElem * pVtableElem_p, CODEPTR pCode_p)
{
	int callOffset;
	int modSuccess;
	static int patchCounter = 0;
	struct MonoSpecificTrampElem * specificTrampElem;

#if defined(TARGET_X86) // Archtitekturabhaengiger Pre-Patch-Code
	//
	//  Lookup des VTable-Trampolines in der Hash-Table der
	//	Specific Trampoline. Dieses spezielle Specific
	//	JIT Trampoline wird modifiziert.
	//
	specificTrampElem = (struct MonoSpecificTrampElem *) g_hash_table_lookup(
			MonoSpecificTrampHashTable, (gconstpointer) pVTableTramp_g);

	if (specificTrampElem == NULL) {
		if (SNOOPVTABLE) {
			printf("\t\t\t\tVTable-Trampoline nicht gefunden.\n");
			fflush(NULL);
		} else {
			;
		}
		return;
	}
	//
	//  Die Adresse im VTable-Slot der Virtuellen Methode konnte einem
	//  VTable-Trampoline zugeordnet werden. Dieses Specific Trampoline
	//  muss modifiziert werden, damit es in ein modifiziertes
	//  Generic Trampoline springt, welches in den Pre-Patch-Code
	//  zurueckkehrt.
	//
	modSuccess = modifyX86SpecificTrampoline(specificTrampElem, NULL,
			PrePatchVtableEntries);

	if (modSuccess != 0) {
		return;
	}

	if (SNOOPVTABLE) {
		printf("\t\t\t\tVTable-Pre-Patch Nr. %i\t%s\n", ++patchCounter,
				mono_method_full_name(pVirtMethod_p, TRUE));
	}
	//
	//  Berechnung des Offsets des VTable-Slots bezueglich der Mono-internen
	//  VTable-Verwaltungsstruktur der Klasse ('MonoVTable'), die die
	//  virtuelle Methode enthaelt. Der Offset ist positiv, da die VTable
	//  hinter der VTable-Verwaltungsstruktur im Speicher liegt. Dieser
	//  Offset wird fuer das Auslösen des Pre-Patch benötigt, da die
	//  Call-Instruktion in den zuvor berechneten VTable-Slot springen soll.
	//
	/**
	 * @todo Berechnung des Offsets mit Architekturpruefung realisieren.
	 */
	callOffset = (G_STRUCT_OFFSET(MonoVTable, vtable) + (iSlot_p * 4));
	//
	//  Emittierung der Aufrufsequenz einer virutellen Methode und
	//  Auslösen des Pre-Patch.
	//
	emitAndstartCallSequence((CODEPTR) pVirtMethod_p, callOffset,
			(CODEPTR) pVtableElem_p->vtable, pCode_p);
	//
	//  Wiederherstellung des zuvor modifizierten VTable-Trampolines.
	//  Theoretisch könnte das auch erst nach dem Abschluss des
	//  IMT-/VTable-Pre-Patch geschehen, wenn sichergestellt werden
	//  könnte, das waehrend des Pre-Patch dieses Trampoline nicht
	//  angesprungen wird.
	//
	restoreX86SpecificTrampoline(specificTrampElem);

	return;

#elif defined(TARGET_ARM)	// Archtitekturabhaengiger Pre-Patch-Code
	return;
#else						// Archtitekturabhaengiger Pre-Patch-Code
	return;
#endif
}
/**
 * @brief emitAndstartCallSequence() fuehrt das Pre-Patch
 * eines IMT-/VTable-Slots aus.
 * @param	methodHandle	Zeiger auf Handle der Methode, fuer die ein IMT-/VTable-Slot bearbeitet wird
 * @param	callOffset		Offset des VTable- oder IMT-Slots von der VTable
 * @param	vtableAddr		Adresse der VTable der Klasse, deren IMT-/VTableSlot bearbeitet wird
 * @param	reservedCode	Zeiger auf den Speicherbereich fuer das Aufrufmuster des Pre-Patch
 *
 * Pre-Patch eines VTable- oder IMT-Slots. Dazu wird mit
 * Makros des Mono Codegenerators Maschinencode emittiert,
 * da die Auswahl des zu patchenden VTable- bzw. IMT-Slots
 * in 'mono_magic_trampoline()' durch ueberpruefung des Opcodes
 * geschieht.
 */
void static emitAndstartCallSequence (CODEPTR pMethod_p, int callOffset,
		CODEPTR pVtable_p, CODEPTR pCode_p)
{
	unsigned char ** ppucVtable;
	CODEPTR emitCodePtr;

	emitCodePtr = pCode_p;
	ppucVtable = &pVtable_p;
#if defined(TARGET_X86) // Archtitekturabhaengiger Pre-Patch-Code

	//
	//  Erzeugung des Codes zum Ausloesen des Pre-Patch eines
	//  IMT- oder VTable-Slots. Der emittierte Maschinencode
	//  entspricht der Struktur des Maschinencodes, den Monos
	//  JIT bei der Emittierung einer CLI CALLVIRT-Anweisung
	//  erzeugt.
	//
	//  Dekrementierung des Stack-Pointers um 16 Bytes um
	//  Platz auf dem Stack zu schaffen. Dieser Wert erwies
	//  sich in experimentellen Untersuchungen als
	//  ausreichend und orientiert sich an den Aufrufmustern,
	//  die Monos JIT-Compilier fuer solche Methodenaufrufe
	//  emittiert.
	//
	x86_alu_reg_imm(emitCodePtr, X86_SUB, X86_ESP, 0xc);
	x86_alu_reg_imm(emitCodePtr, X86_SUB, X86_ESP, 0x4);
	//
	//  Die Zeiger auf die Adresse der VTable der Klasse,
	//  deren Implementierung der Interface- oder virtuelle
	//  Methode gerufen wird, wird im Register EBX erwartet.
	//
	x86_mov_reg_imm(emitCodePtr, X86_EBX, ppucVtable);
	//
	//  Das Register EBX wird zweimal auf den Stack gelegt.
	//  Eine Push-Operation dient als Dummy fuer den evtl.
	//  Rueckgabewert der Funktion. Möglicherweise ist ein
	//  anderes Vorgehen möglich. Die vorliegende Konfiguration
	//  erwies sich in experimentellen Untersuchungen als
	//  brauchbar.
	//
	x86_push_reg(emitCodePtr, X86_EBX);
	x86_push_reg(emitCodePtr, X86_EBX);
	//
	//  Die Adresse der VTable wird im Register EAX
	//  erwartet. Dieses Register wird im Magic
	//  Trampoline per Opcode-Probing geprueft.
	//
	x86_mov_reg_imm(emitCodePtr, X86_EAX, pVtable_p);
	//
	//  Falls es sich um einen IMT-Pre-Patch handelt muss
	//  das generische Handle der Interface Methode im
	//  Register EDX gespeichert werden. Anhand dieses Handles
	//  und dem Zeiger auf die klassenspezifische VTable
	//  wird im Magic Trampoline die konkrete Implementierung
	//  der Interface Methode bestimmt.
	//
	if (callOffset < 0) {
		x86_mov_reg_imm(emitCodePtr, X86_EDX, pMethod_p);
	}
	//
	//  Das Magic Trampoline erwartet drei NOP-Instruktionen
	//  auf Aufrufseite.
	//
	x86_nop(emitCodePtr);
	x86_nop(emitCodePtr);
	x86_nop(emitCodePtr);
	//
	//  Es wird ein absolut indirekter Call ausgefuehrt.
	//  Der Offset bestimmt den IMT-/VTable-Slot, dessen
	//  Inhalt als Ziel der Call-Instruktion verwendet
	//  wird.
	//
	x86_call_membase(emitCodePtr, X86_EAX, callOffset);
	//
	//  Inkrementieren des Stack-Pointers um den Stack
	//  wieder aufzuraeumen. Der Inkrementwert ergab
	//  sich aus experimentellen Untersuchungen und
	//  orientiert sich an den Aufrufmustern,
	//  die Monos JIT-Compilier fuer solche Methodenaufrufe
	//  emittiert.
	//
	x86_alu_reg_imm(emitCodePtr, X86_ADD, X86_ESP, 0x18);
	//
	//  Nachdem der Trampoline-Code zurueckkehrt, springt
	//  die Pre-Patch-Aufrufsequenz in den Pre-Patch-Code
	//  zurueck.
	//
	x86_ret(emitCodePtr);

	if (SNOOPCALLVIRT) {
		printf(
				"\t\t\t\t\temitAndstartCallSequence(): Method-Handle: %p\tOffset: %i\tVTable: %p (%p)\n",
				pMethod_p, callOffset, pVtable_p, ppucVtable);
		fflush(NULL);
	}
	//
	//  Diese globale Variable deaktiviert die Ueberpruefung des
	//  Runtime Generic Context. Das ist notwendig, da das Pre-Patch
	//  vor der Ausfuehrung eines Assemblys ablaeuft, so dass kein
	//  Runtime Generic Context ermittelt werden kann und
	//  Mono beendet.
	//
	disableMonoGetGenericContextFromCode_g = 1;
	//
	//  Der reservierte Speicherbereich wurde mit der
	//  Aufrufsequenz initialisiert. Das Anspringen dieser
	//  Aufrufsequenz wurde in eine eigene Funktion
	//  verschoben, damit die Register automatisch
	//  gesichert werden.
	//
	triggerPrePatchCode(pCode_p);
	//
	//  Aktivieren der Ueberpruefung des Runtime Generic
	//  Context.
	//
	disableMonoGetGenericContextFromCode_g = 0;
	return;

#elif defined(TARGET_ARM)	// Archtitekturabhaengiger Pre-Patch-Code
	return;
#else						// Archtitekturabhaengiger Pre-Patch-Code
	return;
#endif
}
/**
 * brief triggerPrePatchCode() fuehrt ein praepariertes Trampoline
 * oder eine Aufrufsequenz zum Ausloesen eines Pre-Patch aus.
 * an.
 * @param	callInstructionPtr	Zeiger auf den Speicherbereich, der die Aufrufsequenz enthaelt
 */
void triggerPrePatchCode(CODEPTR callInstructionPtr) {

#if defined(TARGET_X86) // Archtitekturabhaengiger Pre-Patch-Code
	__asm__ volatile
	(
			"call *%0"
			:
			:"m" (callInstructionPtr)
	);
	return;

#elif defined(TARGET_ARM)	// Archtitekturabhaengiger Pre-Patch-Code
	return;
#else						// Archtitekturabhaengiger Pre-Patch-Code
	return;
#endif
}
/**
 * @brief fireDelegateCtor() ruft ein Delegate-Trampoline auf.
 *
 * @param delegate	Zeiger auf eine neu erzeugte Delegate-Struktur
 *
 * Diese Funktion wird genau dann ausgefuehrt, wenn Delegate-Pre-Patch
 * aktiviert ist und ein Delegate erzeugt wurde. Dabei sind zwei
 * Faelle zu unterscheiden. In einem optimierten Fall werden Teile
 * des Delegate-Konstruktors in den nativen Code emittiert (siehe
 * Funktion "handle_delegate_ctor()" in "mono/mini/method-to-ir.c").
 * Um das Delegate-Pre-Patch zu realisieren wird der Aufruf von
 * "fireDelegateCtor()" ebenfalls in den nativen Code emittiert.
 * Der optimierte Fall tritt in der Regel dann ein, wenn im IL-Code
 * die Opcode-Folge "CEE_LDFTN, CEE_NEWOBJ" vorgefunden wird. Das
 * ist bspw. bei nicht-virtuellen Methoden, auf die das Delegate
 * verweisen soll, der Fall.
 *
 * In einem unoptimierten Fall wird die Mono-interne Funktion
 * "mono_delegate_ctor_with_method()" in "mono/metadata/object.c"
 * aufgerufen. Sie stellt den Delegate-Konstruktor dar. Der
 * unoptimierte Fall tritt insbesondere bei Delegates
 * auf virtuelle oder Interface-Methoden ein. Die Opcode-Folge
 * "CEE_LDVIRTFTN, CEE_NEWOBJ" im IL-Code wuerde bspw. die
 * unoptimierte Erzeugung eines Delegate veranlassen.
 *
 * Die Funktion "fireDelegateCtor()" fuehrt die Aufrufsequenz
 * zum Aufruf eines Delegates aus. Dadurch wird das Delegate-Trampoline
 * ausgefuehrt. Im Delegate-Trampoline wird geprueft, ob es sich
 * um einen Delegate-Pre-Patch handelt. Falls ja, dann wird in
 * "fireDelegateCtor()" zurueckgekehrt. Somit muss das
 * Delegate-Trampoline bei der ersten Ausfuehrung des Delegate
 * nicht aufgerufen werden.
 *
 * "fireDelegateCtor()" koennte ebenfalls aus der Funktion
 * "ves_icall_System_Object_combinedDelegate()" in
 * "mono/metadata/icall.c" heraus aufgerufen werden. Hier
 * wird durch Verknuepfungen mehrere Delegates u.U. neue
 * Delegates erzeugt, die gepatcht werden muessen.
 */
void fireDelegateCtor(MonoDelegate * delegate) {

#if defined(TARGET_X86) // Archtitekturabhaengiger Pre-Patch-Code
	CODEPTR pushInstructionPtr;
	unsigned char pushInstruction;
	CODEPTR jumpInstructionPtr;
	CODEPTR jumpInstructionArgumentPtr;
	CODEPTR jumpInstructionTarget;
	OFFSET jumpInstructionArgument;
	int firstCondition;
	int secondCondition;
	int thirdCondition;

	MONO_ARCH_SAVE_REGS;

	pushInstructionPtr = NULL;
	pushInstruction = 0x0;
	jumpInstructionPtr = NULL;
	jumpInstructionArgumentPtr = NULL;
	jumpInstructionTarget = NULL;
	jumpInstructionArgument = 0;
	firstCondition = 0;
	secondCondition = 0;
	thirdCondition = 0;

	if (delegate == NULL) {
		return;
	}

	if ((doPrePatchCmd_g & PREPATCHDEL) != 0)
	{
		//
		//  Der Delegate-Pre-Patch wird nur dann ausgefuehrt, wenn
		//  der Invoke-Code des Delegates auf ein Specific Trampoline
		//  zeigt. Falls nicht, dann wuerde ein Aufruf des Delegate
		//  Trampoline das Delegate ausfuehren. Daher muss der Member
		//  "invoke_impl" des Delegates untersucht werden, ob er auf
		//  ein Specific Trampoline zeigt. Falls ja, dann wird
		//  ueberprueft, ob das Specific Trampoline in das Generic
		//  Delegate Trampoline springt, dessen Adresse in der globalen
		//  Variable "pDelTramp_g" gespeichert ist.
		//
		/*
		 * @todo: Untersuchen, ob diese Ueberpruefung notwendig ist,
		 * da "fireDelegateCtor()" nur fuer neu erzeugte Delegates
		 * aufgerufen wird.
		 */
		pushInstructionPtr = ((CODEPTR) delegate->invoke_impl);
		pushInstruction
				= (unsigned char) (*((unsigned char *) delegate->invoke_impl));
		jumpInstructionPtr = pushInstructionPtr + 5;
		jumpInstructionArgumentPtr = jumpInstructionPtr + 1;
		jumpInstructionArgument = *((OFFSET *) jumpInstructionArgumentPtr);
		jumpInstructionArgument += 5;
		jumpInstructionTarget = (CODEPTR) (((OFFSET) jumpInstructionPtr)
				+ jumpInstructionArgument);

		firstCondition = (int) (pushInstruction == ((unsigned char) 0x68));
		secondCondition = (int) ((*(jumpInstructionPtr)) == ((unsigned char) 0xe9));
		thirdCondition = (int) (jumpInstructionTarget == ((CODEPTR) pDelTramp_g));

		if (firstCondition && secondCondition && thirdCondition)
		{
			//
			//  Das Delegate wird durch ein Specific Trampoline betreten. Daher
			//  muss der Member "patched" auf "0x0" gesetzt werden um in
			//  "mono_delegate_trampoline()" anzuzeigen, dass das Delegate
			//  das erste Mal betreten wird und ein Pre-Patch vorliegt.
			//
			delegate->patched = (gpointer) 0x0;
			//			if (SNOOPPATCH) {
			//				printf("Mono-RT - fireDelegateCtor():\tDelegate: %p\tpatched: %p\tTread: %lu\n",(CODEPTR) delegate, (CODEPTR) delegate->patched, pthread_self());
			//				fflush(NULL);
			//			}
			//
			//  Diese globale Variable deaktiviert die Ueberpruefung des
			//  Runtime Generic Context. Das ist notwendig, da das Pre-Patch
			//  vor der Ausfuehrung eines Assemblys ablaeuft, so dass kein
			//  Runtime Generic Context ermittelt werden kann und
			//  Mono beendet.
			//
			disableMonoGetGenericContextFromCode_g = 1;
			//
			// Ausfuehrung der Aufrufsequenz fuer ein Delegate-Aufruf.
			// In der Aufrufsequenz wird zunachst Platz auf dem Stack
			// geschaffen ("sub $0xc,%%esp"). Danach wird die Adresse
			// des Delegate im Register EAX gespeichert. Danach wird
			// diese Adresse zweimal auf den Stack gepusht. Das
			// Delegate-Trampoline erwartet diesen Wert nur einmal.
			// Das zweite Pushen dient als Platzhalter fuer einen
			// moeglichen Rueckgabewert der Methode, die durch das
			// Delegate aufgerufen werden soll. Wuerde dieser Platzhalter
			// nicht eingefuegt, so kann es zu Problemen bei der Suche
			// der Delegate-Adresse im Delegate-Trampoline kommen.
			// Die drei NOP-Anweisungen dienen der Suche im Trampoline.
			// Der Invoke-Code des Delegate befindet sich im Member
			// "invoke_impl" in der Delegate-Struktur. Dieser Member
			// hat einen Offset von 12 Bytes zum Beginn der Struktur.
			// Daher erfolgt der Aufruf des Codes mit dem Offset 12
			// bzgl. der Delegate-Adresse ("call *0xc(%%eax)").
			// Abschliessend wird der Stack wieder aufgeraeumt
			// ("add $0x14,%%esp") und das Delegate-Pre-Patch ist
			// beendet. Diese Aufrufsequenz wurde aus dem erzeugten
			// nativen Code des JIT-Compilers abgeleitet.
			//
			__asm__ volatile
			(
					"sub $0xc,%%esp\n\t"
					"movl %0, %%eax\n\t"
					"push %%eax\n\t"
					"push %%eax\n\t"
					"nop\n\t"
					"nop\n\t"
					"nop\n\t"
					"call *0xc(%%eax)\n\t"
					"add $0x14,%%esp"
					:
					:"m" ((gpointer)delegate)
			);
			//
			//  Aktivieren der Ueberpruefung des Runtime Generic
			//  Context.
			//
			disableMonoGetGenericContextFromCode_g = 0;
			//
			//  Wird ein Delegate als Parameter eines PInvokes uebergeben,
			//  so wird es auf unmanaged Seite durch einen Wrapper aufgerufen.
			//  Dieser Wrapper wird in "mono_delegate_to_ftnptr()" generiert.
			//  Da dieser Wrapper Delegate- und damit Objekt-spezifisch ist,
			//  kann diese Generierung erst zur Laufzeit nach der Erzeugung
			//  des Delegate erfolgen. Der Aufruf von "mono_delegate_to_ftnptr()"
			//  kann die Compilierung vom ersten Marshallen des Delegates
			//  in die Delegate-Initialisierung verschieben.
			//
			//			delegateTrampolinePtr = mono_delegate_to_ftnptr(delegate);

			if ((SNOOPPATCH)) {
				printStackTrace(NULL);
			}

		} else {
			//
			// Das Delegate wird nicht durch ein Specific Trampoline betreten.
			// Daher muss der Member "patched" auf "0xdeadbeef" gesetzt werden
			// um bei einem nicht auszuschliessenden Aufruf von "mono_delegate_trampoline()"
			// fuer dieses Delegate anzuzeigen, dass das Delegate wiederholt
			// betreten wird und kein Pre-Patch vorliegt. In diesem Fall
			// kehrt das Delegate-Trampoline ("mono_delegate_trampoline()")
			// nicht in das modifizierte Generic Delegate Trampoline zurueck,
			// sondern in das normale Generic Delegate Trampoline, welches
			// nicht in den Pre-Patch-Code zurueckspringt, sondern in die
			// aufzurufende Methode.
			//
			// Dieser Fall kann beispielsweise eintreten, wenn aus einem
			// Mulitcast-Delegate Delegates entfernt werden. Dabei muss
			// kein vollstaendig neues Delegat erzeugt werden. Da die
			// im Multicast-Delegate enthaltenen Delegaten waehrend des
			// Pre-Patch bereits behandelt wurden, ist ein erneutes
			// Patchen nicht notwendig.
			//
			if ((SNOOPPATCH)) {
				printf(
						"Mono-RT - fireDelegateCtor():\tDelegate: %p\tpatched: %p\tThread: %lu\n",
						(CODEPTR) delegate, (CODEPTR) delegate->patched,
						pthread_self());
				fflush(NULL);
				printStackTrace(NULL);
			}
			delegate->patched = (gpointer) 0xdeadbeef;
		}
	} else {
		;
	}
	return;

#elif defined(TARGET_ARM)	// Archtitekturabhaengiger Pre-Patch-Code
	return;
#else						// Archtitekturabhaengiger Pre-Patch-Code
	return;
#endif
}
/*
 * @brief	initializeAllDefaultClasses() initialisiert die
 * Mono-internen Standard-Klassen
 */
static void initializeAllDefaultClasses ()
{
	GHashTable * DefaultClassesToInit;

	DefaultClassesToInit = g_hash_table_new_full(g_direct_hash, g_direct_equal, NULL, NULL);

	g_hash_table_insert(DefaultClassesToInit, mono_defaults.object_class, mono_defaults.object_class);
	g_hash_table_insert(DefaultClassesToInit, mono_defaults.byte_class, mono_defaults.byte_class);
	g_hash_table_insert(DefaultClassesToInit, mono_defaults.void_class, mono_defaults.void_class);
	g_hash_table_insert(DefaultClassesToInit, mono_defaults.sbyte_class, mono_defaults.sbyte_class);
	g_hash_table_insert(DefaultClassesToInit, mono_defaults.int16_class, mono_defaults.int16_class);
	g_hash_table_insert(DefaultClassesToInit, mono_defaults.uint16_class, mono_defaults.uint16_class);
	g_hash_table_insert(DefaultClassesToInit, mono_defaults.int32_class, mono_defaults.int32_class);
	g_hash_table_insert(DefaultClassesToInit, mono_defaults.uint32_class, mono_defaults.uint32_class);
	g_hash_table_insert(DefaultClassesToInit, mono_defaults.int_class, mono_defaults.int_class);
	g_hash_table_insert(DefaultClassesToInit, mono_defaults.uint_class, mono_defaults.uint_class);
	g_hash_table_insert(DefaultClassesToInit, mono_defaults.int64_class, mono_defaults.int64_class);
	g_hash_table_insert(DefaultClassesToInit, mono_defaults.uint64_class, mono_defaults.uint64_class);
	g_hash_table_insert(DefaultClassesToInit, mono_defaults.single_class, mono_defaults.single_class);
	g_hash_table_insert(DefaultClassesToInit, mono_defaults.double_class, mono_defaults.double_class);
	g_hash_table_insert(DefaultClassesToInit, mono_defaults.char_class, mono_defaults.char_class);
	g_hash_table_insert(DefaultClassesToInit, mono_defaults.string_class, mono_defaults.string_class);
	g_hash_table_insert(DefaultClassesToInit, mono_defaults.enum_class, mono_defaults.enum_class);
	g_hash_table_insert(DefaultClassesToInit, mono_defaults.array_class, mono_defaults.array_class);
	g_hash_table_insert(DefaultClassesToInit, mono_defaults.delegate_class, mono_defaults.delegate_class);
	g_hash_table_insert(DefaultClassesToInit, mono_defaults.multicastdelegate_class, mono_defaults.multicastdelegate_class);
	g_hash_table_insert(DefaultClassesToInit, mono_defaults.asyncresult_class, mono_defaults.asyncresult_class);
	g_hash_table_insert(DefaultClassesToInit, mono_defaults.manualresetevent_class, mono_defaults.manualresetevent_class);
	g_hash_table_insert(DefaultClassesToInit, mono_defaults.typehandle_class, mono_defaults.typehandle_class);
	g_hash_table_insert(DefaultClassesToInit, mono_defaults.fieldhandle_class, mono_defaults.fieldhandle_class);
	g_hash_table_insert(DefaultClassesToInit, mono_defaults.methodhandle_class, mono_defaults.methodhandle_class);
	g_hash_table_insert(DefaultClassesToInit, mono_defaults.systemtype_class, mono_defaults.systemtype_class);
	g_hash_table_insert(DefaultClassesToInit, mono_defaults.monotype_class, mono_defaults.monotype_class);
	g_hash_table_insert(DefaultClassesToInit, mono_defaults.exception_class, mono_defaults.exception_class);
	g_hash_table_insert(DefaultClassesToInit, mono_defaults.threadabortexception_class, mono_defaults.threadabortexception_class);
	g_hash_table_insert(DefaultClassesToInit, mono_defaults.thread_class, mono_defaults.thread_class);
	g_hash_table_insert(DefaultClassesToInit, mono_defaults.transparent_proxy_class, mono_defaults.transparent_proxy_class);
	g_hash_table_insert(DefaultClassesToInit, mono_defaults.real_proxy_class, mono_defaults.real_proxy_class);
	g_hash_table_insert(DefaultClassesToInit, mono_defaults.mono_method_message_class, mono_defaults.mono_method_message_class);
	g_hash_table_insert(DefaultClassesToInit, mono_defaults.appdomain_class, mono_defaults.appdomain_class);
	g_hash_table_insert(DefaultClassesToInit, mono_defaults.field_info_class, mono_defaults.field_info_class);
	g_hash_table_insert(DefaultClassesToInit, mono_defaults.method_info_class, mono_defaults.method_info_class);
	g_hash_table_insert(DefaultClassesToInit, mono_defaults.stringbuilder_class, mono_defaults.stringbuilder_class);
	g_hash_table_insert(DefaultClassesToInit, mono_defaults.math_class, mono_defaults.math_class);
	g_hash_table_insert(DefaultClassesToInit, mono_defaults.stack_frame_class, mono_defaults.stack_frame_class);
	g_hash_table_insert(DefaultClassesToInit, mono_defaults.stack_trace_class, mono_defaults.stack_trace_class);
	g_hash_table_insert(DefaultClassesToInit, mono_defaults.marshal_class, mono_defaults.marshal_class);
	g_hash_table_insert(DefaultClassesToInit, mono_defaults.iserializeable_class, mono_defaults.iserializeable_class);
	g_hash_table_insert(DefaultClassesToInit, mono_defaults.serializationinfo_class, mono_defaults.serializationinfo_class);
	g_hash_table_insert(DefaultClassesToInit, mono_defaults.streamingcontext_class, mono_defaults.streamingcontext_class);
	//g_hash_table_insert (DefaultClassesToInit, mono_defaults.typed_reference_class, mono_defaults.typed_reference_class);
	g_hash_table_insert(DefaultClassesToInit, mono_defaults.argumenthandle_class, mono_defaults.argumenthandle_class);
	g_hash_table_insert(DefaultClassesToInit, mono_defaults.marshalbyrefobject_class, mono_defaults.marshalbyrefobject_class);
	g_hash_table_insert(DefaultClassesToInit, mono_defaults.monitor_class, mono_defaults.monitor_class);
	g_hash_table_insert(DefaultClassesToInit, mono_defaults.iremotingtypeinfo_class, mono_defaults.iremotingtypeinfo_class);
	g_hash_table_insert(DefaultClassesToInit, mono_defaults.runtimesecurityframe_class, mono_defaults.runtimesecurityframe_class);
	g_hash_table_insert(DefaultClassesToInit, mono_defaults.executioncontext_class, mono_defaults.executioncontext_class);
	g_hash_table_insert(DefaultClassesToInit,
			mono_defaults.internals_visible_class,
			mono_defaults.internals_visible_class);
	g_hash_table_insert(DefaultClassesToInit,
			mono_defaults.generic_ilist_class,
			mono_defaults.generic_ilist_class);
	g_hash_table_insert(DefaultClassesToInit,
			mono_defaults.generic_nullable_class,
			mono_defaults.generic_nullable_class);
	g_hash_table_insert(DefaultClassesToInit, mono_defaults.variant_class,
			mono_defaults.variant_class);
	g_hash_table_insert(DefaultClassesToInit, mono_defaults.com_object_class,
			mono_defaults.com_object_class);
	g_hash_table_insert(DefaultClassesToInit,
			mono_defaults.com_interop_proxy_class,
			mono_defaults.com_interop_proxy_class);
	;
	g_hash_table_insert(DefaultClassesToInit, mono_defaults.iunknown_class,
			mono_defaults.iunknown_class);
	g_hash_table_insert(DefaultClassesToInit, mono_defaults.idispatch_class,
			mono_defaults.idispatch_class);
	g_hash_table_insert(DefaultClassesToInit, mono_defaults.safehandle_class,
			mono_defaults.safehandle_class);
	g_hash_table_insert(DefaultClassesToInit, mono_defaults.handleref_class,
			mono_defaults.handleref_class);
	g_hash_table_insert(DefaultClassesToInit, mono_defaults.attribute_class,
			mono_defaults.attribute_class);
	g_hash_table_insert(DefaultClassesToInit,
			mono_defaults.customattribute_data_class,
			mono_defaults.customattribute_data_class);
	g_hash_table_insert(DefaultClassesToInit,
			mono_defaults.critical_finalizer_object,
			mono_defaults.critical_finalizer_object);

	g_hash_table_foreach(DefaultClassesToInit, initializeArrayVariantOfClass, NULL);
	g_hash_table_destroy(DefaultClassesToInit);
	return;
}
/*
 * @brief	initializeArrayVariantOfClass() prueft ob eine Mono-interne
 * Standard-Klasse initialisiert wurde und holt das ggf. nach.
 *
 * @param	key		Zeiger auf die MonoClass, die initialisiert werden soll
 * @param	value	Zeiger auf die MonoClass, die initialisiert werden soll
 * @param	user_data	nicht verwendet, bzw. stets NULL
 */
void initializeArrayVariantOfClass(gpointer key, gpointer value,
		gpointer user_data) {

	MonoClass * klasse;
	MonoClass * arrayKlasse;

	klasse = (MonoClass *) value;

	if (klasse != NULL) {
		if (SNOOPTC) {
			printf(
					"Mono-RT - initializeArrayVariantOfClass ():\n\tKlasse: %s\tinitialisiert: %s\n",
					klasse->name, ((klasse->inited != 0) ? "ja" : "nein"));
			fflush(NULL);
		}
		//
		// Abbruch bei der Klasse "Typed Reference", da die
		// Erzeugung einer Array-Klasse zu einem Fehler fuehren
		// und Mono abstuerzen wuerde.
		//
		if (klasse == mono_defaults.typed_reference_class) {
			return;
		}
		//
		// Abbruch, falls es sich bereits um eine Array-Klasse handelt.
		// Das ist notwendigt um eine endlose Wiederholung zu vermeiden,
		// da diese Funktion auf die neu erzeugten Array-Klassen angewendet
		// wird.
		//
		if (klasse->rank >= 1) {
			return;
		}

		arrayKlasse = mono_array_class_get(klasse, 1);
		if (arrayKlasse != NULL) {
			if (SNOOPTC) {
				printf(
						"Mono-RT - initializeArrayVariantOfClass ():\n\tArray-Klasse: %s\tinitialisiert: %s\n",
						arrayKlasse->name, ((arrayKlasse->inited != 0) ? "ja"
								: "nein"));
				fflush(NULL);
			}

			if (arrayKlasse->inited == 0) {
				//mono_class_create_runtime_vtable (mono_domain_get(), arrayKlasse, FALSE);
				mono_class_vtable_full(mono_domain_get(), arrayKlasse, FALSE);
			} else {
				;
			}

		} else {
			if (SNOOPTC) {
				printf(
						"Mono-RT - initializeArrayVariantOfClass ():\n\tArray-Klasse: NULL\n");
				fflush(NULL);
			}
		}
	} else {
		if (SNOOPTC) {
			printf(
					"Mono-RT - initializeArrayVariantOfClass ():\n\tKlasse: NULL\n");
			fflush(NULL);
		}
	}

	return;
}
/**
 * @brief modifyX86SpecificTrampoline() aendert das Sprungziel eines
 * Specific Trampoline in ein modifiziertes passendes Generic Trampoline.
 * @param	pSpecTramp_p		Zeiger auf Verwaltungsstruktur des Specific Trampoline, das modifiziert wird
 * @param	pCallElem_p			Zeiger auf Verwaltungsstruktur des nativen Codes fuer Debugging-Ausgaben
 * @param	prePatchMode_p		Gibt an, welche Modified Generic Trampoline Hash Table verwendet wird
 * @return						Im fehlerfreien Fall wird 0 zurückgegeben, sonst 1.
 *
 * Die Jump-Instruktion eines Specific Trampoline wird auf
 * ein modifiziertes Generic Trampoline geaendert. Jenes springt
 * in den Pre-Patch-Code zurueck, anstatt in die aufzurufende Methode.
 */
static int modifyX86SpecificTrampoline (struct MonoSpecificTrampElem *pSpecTramp_p,
		struct MonoCallCodeList * pCallElem_p, enum PrePatchMode prePatchMode_p)
{
	struct MonoModifiedGenericTrampElem * pModGenTrampElem;
	struct MonoGenericTrampElem * pGenTrampElem;
	GHashTable * pModGenTrampHash;
	//
	//  Absolute Speicheradresse des Speicherbereichs, an dem ein
	//  modifiziertes Generisches Trampoline nach dessen Erzeugung
	//  liegt, d.h., dessen Startadresse.
	//
	CODEPTR pModGenTramp;
	//
	//  Zeiger auf eine Jump-Instruktion in einem Specific Trampoline
	//
	CODEPTR pJmpInst;
	//
	//  Zeiger auf das Argument einer Jump-Instruktion, d.h.,
	//  Zeiger auf den Offset des Jump-Ziels zur aktuellen Position
	//
	CODEPTR pJmpInstArg;
	//
	//  Absolute Adresse des neuen Ziels der Jump-Instruktion
	//
	CODEPTR pJmpInstNewTgt;
	//
	//  Neues Argument einer Jump-Instruktion, d.h, der neue Offset
	//  des Ziels der Jump-Instruktion zur aktuellen Position.
	//
	OFFSET pJmpInstNewArg;

	void * pRet;

	pModGenTrampElem = NULL;
	pGenTrampElem = NULL;
	pModGenTrampHash = NULL;
	pModGenTramp = NULL;
	pJmpInst = NULL;
	pJmpInstArg = NULL;
	pJmpInstNewTgt = NULL;
	pJmpInstNewArg = NULL;
	pRet = NULL;

#if defined(TARGET_X86)	// Archtitekturabhaengiger Pre-Patch-Code
	//
	//  Ueberpruefung, ob das unmodifizierte Generic Trampoline, das
	//  vom Specific Trampoline angesprungen werden soll, erfasst wurde.
	//
	pGenTrampElem = (struct MonoGenericTrampElem *) g_hash_table_lookup(
			MonoGenericTrampHashTable, (gpointer)(
					pSpecTramp_p->specificTrampJumpInstructionTarget));

	if (pGenTrampElem == NULL)
	{
		//
		//  Das Sprung-Ziel des Specific Trampoline wurde nicht erfasst.
		//
		printf(
				"Mono-RT - modifyX86SpecificTrampoline(): Fehler hash_table_lookup() Generic Trampoline.\n");
		fflush(NULL);
		//exit(1);
		return 1;
	}

	if (pSpecTramp_p->specificTrampJumpInstructionTarget
			!= pGenTrampElem->genericTrampAddress) {
		printf(
				"Mono-RT - modifyX86SpecificTrampoline(): Fehler Zuordnung Generic Trampoline.\n");
		fflush(NULL);
		return 1;
	}
	//
	//  Es wurde das Generic Trampoline gefunden,
	//  welches zum Jump-Ziel des Specific Trampoline
	//  passt, d.h., es gilt:
	//
	//	specificTrampElem->specificTrampJumpInstructionTarget == genericTrampElem->genericTrampAddress
	//
	if (SNOOPTRAMP)
	{
		printf(
				"\tSpecific Trampoline %p (Push: %p) -> Generic Trampoline %p\n",
				pSpecTramp_p->specificTrampAddress,
				pSpecTramp_p->specificTrampPushValue,
				pGenTrampElem->genericTrampAddress);
		fflush(NULL);
	}
	//
	//  Bestimmung eines passenden Modified Generic Trampoline,
	//  indem ein Modified Generic Trampoline mit dem gleichen
	//  Typ gesucht wird.
	//
	switch (prePatchMode_p)
	{
	case PrePatchCallInstructions:
		pModGenTrampHash = pCallModTrampHash_g;
		break;
	case PrePatchImtEmtries:
		pModGenTrampHash = pImtModTrampHash_g;
		break;
	case PrePatchVtableEntries:
		pModGenTrampHash = pVtableModTrampHash_g;
		break;
	case PrePatchPltEntries:
		pModGenTrampHash = pPltModTrampHash_g;
		break;
	default:
		pModGenTrampHash = NULL;
	}

	if (NULL == pModGenTrampHash)
	{
		printf("MonoRT:\tError Pre-Patch.\n");
		fflush(NULL);
		return 1;
	}

	pModGenTrampElem = (struct MonoModifiedGenericTrampElem *) g_hash_table_lookup(pModGenTrampHash, (gconstpointer)(pGenTrampElem->genericTrampType));
	if (NULL == pModGenTrampElem)
	{
		//
		//  Es wurde kein passendes Modified Generic Trampoline
		//  gefunden. Anlegen eines neuen Modified Generic
		//  Trampoline mit passendem Typ. Dazu wird eine Mono-interne
		//  Funktion 'mono_arch_create_trampoline_code' verwendet,
		//  die waehrend der JIT-Compilierung Generic Trampoline
		//  erzeugt. Das neue Generic Trampoline liegt in einem
		//  ausfuehrbaren Speicherbereich. Die Steuerung, ob ein
		//  modifiziertes Generic Trampoline erzeugt werden soll,
		//  erfolgt ueber globale Variablen, die in der Funktion
		//  'mono_arch_create_trampoline_code()' ausgewertet werden.
		//
		if (PrePatchPltEntries == prePatchMode_p)
		{
			pModGenTramp = (CODEPTR) createX86ModifiedGenericTrampoline(pGenTrampElem->genericTrampType);
		}
		else
		{
			pModGenTramp = (CODEPTR) mono_arch_create_trampoline_code(pGenTrampElem->genericTrampType);
		}

		if (SNOOPTRAMP)
		{
			printf("\tNeues modified Generic Trampoline an 0x%X\n", pModGenTramp);
			fflush(NULL);
		}
		//
		//  Einfuegen des modifizierten Generic Trampoline in
		//  einer Hash-Tabelle um es nicht immer neu erzeugen
		//  zu muessen.
		//
		pRet = insertListElement(ModifiedGenericTrampoline,
				(void *) pModGenTramp,
				(void *) &(pGenTrampElem->genericTrampType), NULL,
				(unsigned int) prePatchMode_p);
	}
	else
	{
		//
		//  Es wurde ein Modified Generic Trampoline gefunden,
		//  welches zum Typ des urspruenglich aufgerufenen
		//  Generic Trampoline passt, d.h., es gilt:
		//
		//	genericTrampElem->genericTrampType == modifiedGenericTrampElem->modifiedGenericTrampType
		//
		pModGenTramp = pModGenTrampElem->m_pModGenTramp;
	}
	//
	//  Konsistenzpruefung der Verwaltungsstruktur des Specific Trampoline
	//  mit dem Speicher. Bei der Erfassung eines emittierten Specific
	//  Trampoline wird der Offset der Jump-Instruktion gespeichert. Je
	//  nach Offset des Generic Trampoline vom Specific Trampoline kann
	//  das Specific Trampoine eine Short Jump-Instruktion (Opcode 0xeb)
	//  enthalten. Das Jump-Ziel ist in so einem Fall nur ein Byte groß.
	//
	pJmpInst
			= (CODEPTR) ((unsigned int) pSpecTramp_p->specificTrampAddress
					+ pSpecTramp_p->specificTrampOffsetJumpInstruction);
	// ** X86-spezifisch **
	pJmpInstArg = (pJmpInst + 1);

	if (!(((pSpecTramp_p->specificTrampHasShortJump
			== ((unsigned char) 0x0)) && ((*pJmpInst)
			== ((unsigned char) 0xe9)))
			|| ((pSpecTramp_p->specificTrampHasShortJump
					== ((unsigned char) 0x1)) && ((*pJmpInst)
					== ((unsigned char) 0xeb))))) {

		printf("Mono-RT - Fehler Ueberpruefung der Jump-Instruktion im ST.\n\n");
		return 1;
	}
	//
	//  "Merken" des Specific Trampolines um es spaeter
	//  in den urspruenglichen Zustand zu versetzen. Der
	//  Zeiger auf die Sicherungskopie wird in der eigenen
	//  Verwaltungsstruktur gespeichert.
	//
	pSpecTramp_p->specificTrampCopy = (CODEPTR) malloc(
			(size_t) (SPECIFIC_TRAMP_SIZE * sizeof(unsigned char)));

	if (pSpecTramp_p->specificTrampCopy == NULL) {
		printf("Mono-RT - Fehler Sicherung Specifc Trampoline. Exit.\n\n");
		return 1;
	}
	//
	//  Kopieren des unmodifizierten Specific Trampoline in den
	//  reservierten Speicherbereich.
	//
	memcpy((void *) pSpecTramp_p->specificTrampCopy,
			(void *) pSpecTramp_p->specificTrampAddress,
			SPECIFIC_TRAMP_SIZE);
	//
	//  Sicherung von Teilen der Datenstruktur um sie veraendern zu
	//  koennen, um zur Ueberpruefung validateSpecificTrampolineJumpTarget()
	//  nutzen zu koennen.
	//
	pSpecTramp_p->specificTrampSavedJumpInstructionTarget
			= pSpecTramp_p->specificTrampJumpInstructionTarget;
	//
	//  Der Sprung im Specific Trampoline wird auf das neu erzeugte
	//  Modified Generic Trampoline aktualisiert. Die Anfangsadresse
	//  des Modified Generic Trampoline wird vermerkt.
	//
	//  Diese tempoaeren Aenderung der Datenstrukur sind notwendig um zur
	//  Ueberpruefung validateSpecificTrampolineJumpTarget() nutzen zu
	//  koennen.
	//
	pSpecTramp_p->specificTrampJumpInstructionTarget
			= pModGenTramp;
	pSpecTramp_p->specificTrampModifiedGenericTrampAddress
			= pModGenTramp;
	//
	//  Aenderung des Ziels der Jump-Instruktion auf das
	//  neu erzeugte modifizierte Generic Trampoline.
	//
	// ** X86-spezifisch **
	//
	pJmpInstNewTgt = pModGenTramp;
	pJmpInstNewArg = (OFFSET) (pJmpInstNewTgt
			- pJmpInst);
	pJmpInstNewArg -= 5;
	*((OFFSET *) pJmpInstArg) = pJmpInstNewArg;
	//
	//  Ersetzung eines Short Jump durch einen Near Jump
	//
	// ** X86-spezifisch **
	//
	if ((pSpecTramp_p->specificTrampHasShortJump == ((unsigned char) 0x1))
			&& ((*pJmpInst) == ((unsigned char) 0xeb))) {

		(*pJmpInst) = ((unsigned char) 0xe9);
		pSpecTramp_p->specificTrampHasShortJump = ((unsigned char) 0x0);
		pSpecTramp_p->specificTrampCodeLength += 3;
	}
	//
	//  Ueberpruefung, ob das neue Jump-Ziel (das Modified Generic
	//  Trampoline) korrekt berechnet wurde.
	//
	validateSpecificTrampolineJumpTarget(pSpecTramp_p);

	if (pSpecTramp_p->specificTrampPassedCheck != 0x0) {
		if (SNOOPTRAMP && (pCallElem_p != NULL)) {
			printf("Fehler (2) unmodified Specific Trampoline:\n");
			printf("\tSoll: %p\tUeberspringe\n", pModGenTramp);
			printf("Methode: %s\n\n", mono_method_full_name(
					pCallElem_p->m_pCallerMeth, TRUE));
			fflush(NULL);
		}
		//
		//  Das aendern des Jump-Ziels schlug fehl.
		//
		return 1;
	}
	return 0;

#elif defined(TARGET_ARM)	// Archtitekturabhaengiger Pre-Patch-Code
	return 0;
#else						// Archtitekturabhaengiger Pre-Patch-Code
	return 0;
#endif
}
/**
 * @brief restoreX86SpecificTrampoline() stellt ein zuvor modifiziertes
 * Specific Trampoline wieder her.
 * @param	specificTrampElem	Zeiger auf Verwaltungsstruktur des Specific Trampoline, das wiederhergestellt wird
 *
 * Die Verwaltungsdatenstruktur und das Specific Trampoline selbst
 * werden wiederhergestellt. Grund: Sollten bei dem Pre-Patch nicht
 * alle Call-Instruktionen gepatcht werden können, so könnte ein
 * Call zur Laufzeit in einem Modified Generic Trampoline landen
 * und einen Fehler verursachen.
 */
static void restoreX86SpecificTrampoline (struct MonoSpecificTrampElem *specificTrampElem)
{
#if defined(TARGET_X86)	// Archtitekturabhaengiger Pre-Patch-Code
	CODEPTR jumpInstructionPtr;

	specificTrampElem->specificTrampJumpInstructionTarget
			= specificTrampElem->specificTrampSavedJumpInstructionTarget;
	//
	//  Kopiere die Sicherungskopie des Specific Trampoline in
	//  ihren urspruenglichen Speicherbereich.
	//
	memcpy((void *) specificTrampElem->specificTrampAddress,
			(void *) specificTrampElem->specificTrampCopy, SPECIFIC_TRAMP_SIZE);
	//
	//  Gebe den Speicherbereich zur Speicherung der Sicherungskopie
	//  frei.
	//
	free(specificTrampElem->specificTrampCopy);
	specificTrampElem->specificTrampCopy = NULL;
	//
	// ** X86-spezifisch **
	//
	//  Zeiger auf eine Jump-Instruktion in einem Specific Trampoline
	//
	jumpInstructionPtr
			= (CODEPTR) ((unsigned int) specificTrampElem->specificTrampAddress
					+ specificTrampElem->specificTrampOffsetJumpInstruction);
	//
	//  Datenstruktur konsistent halten, falls zuvor ein Short Jump
	//  gegen einen Near Jump ausgetauscht wurde.
	//
	if ((specificTrampElem->specificTrampHasShortJump == ((unsigned char) 0x0))
			&& ((*jumpInstructionPtr) == ((unsigned char) 0xeb))) {

		specificTrampElem->specificTrampHasShortJump = ((unsigned char) 0x1);
		specificTrampElem->specificTrampCodeLength -= 3;
	}
	return;

#elif defined(TARGET_ARM)	// Archtitekturabhaengiger Pre-Patch-Code
	return;
#else						// Archtitekturabhaengiger Pre-Patch-Code
	return;
#endif

}
/**
 * @brief cleanHashTables() wird vor dem Start der Ausfuehrung des
 * auszufuehrenden Codes gerufen und gibt den Speicher der nicht
 * mehr benoetigten Verwaltungsstrukturen frei.
 */
static void cleanHashTables()
{
	struct MonoCallCodeList * CallCodeListHeadPtr;
	struct MonoClassList * ClassListHeadPtr;
	struct MonoAotCodeList * AotCodeListHeadPtr;

	CallCodeListHeadPtr = CallCodeList.head;
	ClassListHeadPtr = ClassList.head;
	AotCodeListHeadPtr = AotCodeList.head;

	while (CallCodeListHeadPtr != &CallCodeList) {
		CallCodeListHeadPtr = CallCodeListHeadPtr->prev;
		free(CallCodeListHeadPtr->next);
		CallCodeListHeadPtr->next = NULL;
	}
	CallCodeList.isDestroyed = 1;
	CallCodeList.isInitialized = 0;

	while (ClassListHeadPtr != &ClassList) {
		ClassListHeadPtr = ClassListHeadPtr->prev;
		free(ClassListHeadPtr->next);
		ClassListHeadPtr->next = NULL;
	}
	ClassList.isDestroyed = 1;

	while (AotCodeListHeadPtr != &AotCodeList) {
		AotCodeListHeadPtr = AotCodeListHeadPtr->prev;
		free(AotCodeListHeadPtr->next);
		AotCodeListHeadPtr->next = NULL;
	}
	AotCodeList.isDestroyed = 1;
	AotCodeList.isInitialized = 0;
	AotCodeList.count = 0;

	//	g_hash_table_destroy(MonoGenericTrampHashTable);
	//	MonoGenericTrampHashTable = NULL;
	//	g_hash_table_destroy(MonoSpecificTrampHashTable);
	//	MonoSpecificTrampHashTable = NULL;
	//	g_hash_table_destroy(pCallModTrampHash_g);
	//	pCallModTrampHash_g = NULL;
	g_hash_table_destroy(pImtModTrampHash_g);
	pImtModTrampHash_g = NULL;
	g_hash_table_destroy(pVtableModTrampHash_g);
	pVtableModTrampHash_g = NULL;
	//	g_hash_table_destroy(MonoNativeCodeHashTable);
	MonoNativeCodeHashTable = NULL;
	g_hash_table_destroy(MonoVTHashTable);
	MonoVTHashTable = NULL;
	g_hash_table_destroy(MonoClassHashTable);
	MonoClassHashTable = NULL;
	g_hash_table_destroy(ExcludedFromPreJit);
	ExcludedFromPreJit = NULL;

	return;
}
/*
 * @brief insertClassListElement() fuegt die Klassen der Methoden,
 * die waehrend des Pre-JIT compiliert werden, in die Hash-Tabelle
 * "MonoClassHashTable" und die Liste "ClassList" ein.
 *
 * @param	item	Zeiger auf die Struktur MonoClass einer compilierten Methode
 * @param	arg0	nicht belegt
 * @param	arg1	nicht belegt
 * @param	len		nicht belegt
 *
 * Aufruf: insertClassListElement(Class, (void *) cfg->method->klass, NULL, NULL, 0);
 */
void * insertClassListElement(enum TrampolineType tTrampType_p, void * pItem_p, void * pArg0_p,
		void * pArg1_p, unsigned int uiIgnored_p)
{
	MonoClass *pClass;
	MonoClass *pArrayClass;
	struct MonoClassElem * pCurClassElem;
	struct MonoClassElem * pNewClassElem;
	struct MonoClassList * pClassListHead;
	struct MonoClassList * pNewClassListElem;
	struct MonoClassList * pCurClassListHead;
	void * pRet;

	pClass = NULL;
	pArrayClass = NULL;
	pCurClassElem = NULL;
	pNewClassElem = NULL;
	pClassListHead = NULL;
	pNewClassListElem = NULL;
	pNewClassListElem = NULL;
	pRet = NULL;

	if ((ClassList.isDestroyed != 0) || (MonoClassHashTable == NULL))
	{
		pRet = NULL;
	}
	else
	{
		pClass = (MonoClass *) pItem_p;
		//
		//  Klassen werden auch in einer Hash-Tabelle verwaltet,
		//  um schnell ueberpruefen zu koennen, ob eine Klasse
		//  bereits erfasst wurde.
		//
		pCurClassElem = (struct MonoClassElem *) g_hash_table_lookup(MonoClassHashTable, (gconstpointer) pClass);

		if (pCurClassElem == NULL)
		{
			//
			//  Die Klasse wurde noch nicht erfasst.
			//
			//  Einfuegen in die Liste "ClassList"
			//
			pClassListHead = ClassList.head;

			if ((pClassListHead->head == (&ClassList))
					&& (pClassListHead->klasse == NULL))
			{
				//
				// Die Liste wird initial gefuellt
				//
				pClassListHead->klasse = pClass;
				pRet = ((void *) pClassListHead);
			}
			else
			{
				pNewClassListElem = (struct MonoClassList *) malloc(sizeof(struct MonoClassList));

				if (pNewClassListElem == NULL)
				{
					printf(
							"Mono-RT - Fehler malloc() insertListElement() Class\n");
					fflush(NULL);
					pRet = NULL;
				}
				else
				{
					pClassListHead->next = pNewClassListElem;
					pNewClassListElem->prev = pClassListHead;
					ClassList.head = pNewClassListElem;
					pNewClassListElem->head = NULL;
					pNewClassListElem->next = NULL;
					pNewClassListElem->klasse = pClass;
					pRet = ((void *) pNewClassListElem);
				}
			}

			if (pRet != NULL)
			{
				//
				//  Die Klasse konnte in die Liste "ClassList" eingefuegt werden.
				//	Die Klasse wird nur dann in die Hash-Tabelle eingefuegt,
				//  wenn das Einfuegen in die Liste geklappt hat. So kann
				//  es zu einem spaeteren Zeitpunkt erneut versucht werden,
				//  wenn eine weitere Methode der Klasse pre-compiliert wurde.
				//
				pNewClassElem = (struct MonoClassElem *) g_malloc(sizeof(struct MonoClassElem));
				if (pNewClassElem == NULL)
				{
					printf("MonoRT - Fehler malloc() insertListElement() Class\n");
					fflush(NULL);
					//
					//  Das Einfuegen der Klasse in die Hash-Tabelle klappt nicht.
					//  Das in die Liste eingefuegte Element wird wieder entfernt.
					//
					pCurClassListHead = (struct MonoClassList *) pRet;
					if (pCurClassListHead == (&ClassList))
					{
						//
						//  Die Liste enthaelt lediglich ein Element.
						//
						pCurClassListHead->klasse = NULL;
					}
					else
					{
						ClassList.head = pCurClassListHead->prev;
						free(ClassList.head->next);
						ClassList.head->next = NULL;
					}
					pRet = NULL;
				}
				else
				{
					pNewClassElem->klasse = pClass;
					g_hash_table_insert(MonoClassHashTable, (gpointer) pClass, (gpointer) pNewClassElem);
					pRet = ((void *) pNewClassElem);
				}
			}
			else
			{
				//
				//  Die Klasse konnte nicht in die Liste "ClassList"
				//  eingefuegt werden. Die Klasse wird dann auch
				//  nicht in die Hash-Tabelle "MonoClassHashTable"
				//  eingefuegt.
				//
				pRet = NULL;
			}
			if (doPrePatchCmd_g != 0)
			{
				//				arrayKlasse = mono_array_class_get(klasse, 1);
				//				if (arrayKlasse != NULL) {
				//					mono_class_init(arrayKlasse);
				//					insertListElement(Class, arrayKlasse, NULL, 0, 0);
				//				} else {
				//					printf("\tKeine Array-Klasse fuer: &s\n", klasse->name);
				//					fflush(NULL);
				//				}
			}
		}
		else
		{
			pRet = ((void *) pCurClassElem);
		}
	}
	return pRet;
}
/*
 * @brief insertNativeCodeListElement() fuegt die Methoden,
 * die waehrend des Pre-JIT compiliert werden, in die Hash-Tabelle
 * "MonoNativeCodeHashTable" ein.
 *
 * @param	pItem_p	Zeiger auf die Struktur MonoCompile, die zur Compilierung der Methode verwendet wurde
 * @param	arg0	nicht belegt
 * @param	arg1	nicht belegt
 * @param	len		nicht belegt
 *
 * Aufruf: insertNativeCodeListElement (NativeCode, (void *) cfg, NULL, NULL, 0);
 */
void * insertNativeCodeListElement(enum TrampolineType TrampType_p, void * pItem_p,
		void * pArg0_p, void * pArg1_p, unsigned int uiIgnored_p)
{

	MonoCompile * pMonoComp;
	MonoMethod * pMethod;
	int wasJitCompiled;
	int wasAotCompiled;
	struct MonoNativeCodeElem * pNativeCodeElem;
	struct MonoNativeCodeElem * pNewNativeCodeElem;
	void * pRet;

	pMonoComp = NULL;
	pMethod = NULL;
	wasJitCompiled = 0;
	wasAotCompiled = 0;
	pNativeCodeElem = NULL;
	pNewNativeCodeElem = NULL;
	pRet = NULL;

	if ((NULL != pItem_p) && (NULL == pArg0_p) && (NULL == pArg1_p))
	{
		//
		// Die Methode wurde waehrend der JIT-Compilerung erfasst
		//
		wasJitCompiled = 1;
	}
	else
	{
		if ((NULL == pItem_p) && (NULL != pArg0_p) && (NULL != pArg1_p))
		{
			//
			// Die Methode wurde AOT-compiliert und geladen.
			//
			wasAotCompiled = 1;
		}
	}

	if (MonoNativeCodeHashTable == NULL) {
		//
		//  Ueberpruefung, ob die Hash-Tabelle gueltig ist.
		//  Sie ist nicht gueltig, wenn kein PrePatch (mehr)
		//  ausgefuehrt werden soll.
		//
		pRet = NULL;
	} else {

		if (1 == wasJitCompiled) {
			pMonoComp = (MonoCompile *) pItem_p;
			pMethod = pMonoComp->method;
		} else {
			if (1 == wasAotCompiled) {
				pMethod = (MonoMethod *) pArg1_p;
			} else {
				return NULL;
			}
		}

		pNativeCodeElem = (struct MonoNativeCodeElem *) g_hash_table_lookup(
				MonoNativeCodeHashTable, (gconstpointer) pMethod);

		if (NULL == pNativeCodeElem) {

			pNewNativeCodeElem = (struct MonoNativeCodeElem *) g_malloc(
					sizeof(struct MonoNativeCodeElem));

			if (pNewNativeCodeElem == NULL) {
				if (SNOOPTRAMP) {
					printf(
							"Mono-RT - Fehler malloc() insertListElement() Native Code\n");
					fflush(NULL);
				}
				pRet = NULL;
			}
			else
			{
				pNewNativeCodeElem->nativeCodeMethodPtr = pMethod;
				if (1 == wasJitCompiled)
				{
					pNewNativeCodeElem->nativeCodeTempAddress = pMonoComp->native_code;
					pNewNativeCodeElem->nativeCodeFinalAddress
							= pMonoComp->native_code;
					pNewNativeCodeElem->nativeCodeCodeLength = pMonoComp->code_len;
					pNewNativeCodeElem->interfaceMethodVtableSlot = -2;
				} else {
					if (1 == wasAotCompiled) {
						pNewNativeCodeElem->nativeCodeTempAddress
								= (CODEPTR) pArg0_p;
						pNewNativeCodeElem->nativeCodeFinalAddress
								= (CODEPTR) pArg0_p;
						pNewNativeCodeElem->nativeCodeCodeLength = -1;
						pNewNativeCodeElem->interfaceMethodVtableSlot = -2;
					} else {
						free(pNewNativeCodeElem);
						return NULL;
					}
				}
				//
				//  Es wird die neu erzeugte Struktur vom Typ
				//  MonoNativeCodeElem gespeichert, da die MonoCompile-Struktur
				//  "cfg" eventuell verworfen wird. Der Key ist der Zeiger
				//  auf die MonoMethod-Struktur, die nur einmal pro
				//  compilierter Methode erzeugt werden und nicht
				//  verworfen werden.
				//
				g_hash_table_insert(MonoNativeCodeHashTable,
						(gpointer) pNewNativeCodeElem->nativeCodeMethodPtr,
						(gpointer) pNewNativeCodeElem);
				//
				//  Fuer jede compilierte Methode wird deren Klasse
				//  erfasst fuer das IMT-/VTable-Pre-Patch.
				//
				//				insertListElement(Class, (void *) newNativeCodeElem->nativeCodeMethodPtr->klass, NULL, NULL, 0);
				pRet = ((void *) pNewNativeCodeElem);
			}
		} else {
			pRet = ((void *) pNativeCodeElem);
		}
	}
	return pRet;
}
/*
 * @brief insertGenericTrampolineListElement() fuegt die Generic
 * Trampoline, die waehrend des Pre-JIT generiert werden, in die
 * Hash-Tabelle "MonoGenericTrampHashTable" ein.
 *
 * @param	item	Absolute Adresse des Generic Trampoline im Speicher
 * @param	arg0	Typ des Generic Trampoline
 * @param	arg1	nicht belegt
 * @param	len		nicht belegt
 *
 * Aufruf: insertGenericTrampolineListElement (GenericTrampoline, (void *) code, (void *) &tramp_type, NULL, 0);
 */
void * insertGenericTrampolineListElement(enum TrampolineType type, void *item,
		void *arg0, void *arg1, unsigned int len) {

	struct MonoGenericTrampElem * genericTrampElem;
	struct MonoGenericTrampElem * newGenericTrampElem;
	void * ret;

	if (MonoGenericTrampHashTable == NULL) {
		ret = NULL;
	} else {

		genericTrampElem = (struct MonoGenericTrampElem *) g_hash_table_lookup(
				MonoGenericTrampHashTable, (gconstpointer) item);

		if (genericTrampElem == NULL) {

			newGenericTrampElem = (struct MonoGenericTrampElem *) g_malloc(
					sizeof(struct MonoGenericTrampElem));

			if (newGenericTrampElem == NULL) {
				if (SNOOPTRAMP) {
					printf(
							"Mono-RT - Fehler malloc() insertListElement() Generic Trampoline\n");
					fflush(NULL);
				}
				ret = NULL;
			} else {
				newGenericTrampElem->genericTrampType
						= (*((MonoTrampolineType *) arg0));
				newGenericTrampElem->genericTrampAddress = (CODEPTR) item;

				g_hash_table_insert(MonoGenericTrampHashTable,
						(gpointer) newGenericTrampElem->genericTrampAddress,
						(gpointer) newGenericTrampElem);

				if (newGenericTrampElem->genericTrampType == MONO_TRAMPOLINE_DELEGATE)
				{
					pDelTramp_g = newGenericTrampElem->genericTrampAddress;
				}
				ret = ((void *) newGenericTrampElem);
			}
		} else {
			ret = ((void *) genericTrampElem);
		}
	}
	return ret;
}
/*
 * @brief insertSpecificTrampolineListElement() fuegt die Specific
 * Trampoline, die waehrend des Pre-JIT generiert werden, in die
 * Hash-Tabelle "MonoSpecificTrampHashTable" ein.
 *
 * @param	item	Absolute Adresse des Specific Trampoline im Speicher
 * @param	arg0	Typ des Specific Trampoline
 * @param	arg1	Absolute Adresse des Generic Trampoline, das angesprungen werden soll
 * @param	len		Laenge des Specific Trampoline
 *
 * Aufruf: insertSpecificTrampolineListElement (SpecificTrampoline, (void *) code, (void *) &tramp_type, (void *) tramp, (unsigned int) (buf - code));
 */
void * insertSpecificTrampolineListElement(enum TrampolineType type,
		void *item, void *arg0, void *arg1, unsigned int len) {

	static int cnt = 0;

	struct MonoSpecificTrampElem * specificTrampElem;
	struct MonoSpecificTrampElem * newSpecificTrampElem;
	void * ret;

	if (MonoSpecificTrampHashTable == NULL) {
		ret = NULL;
	} else {

		specificTrampElem
				= (struct MonoSpecificTrampElem *) g_hash_table_lookup(
						MonoSpecificTrampHashTable, (gconstpointer) item);
		cnt++;

		if (specificTrampElem == NULL) {

			newSpecificTrampElem = (struct MonoSpecificTrampElem *) g_malloc(
					sizeof(struct MonoSpecificTrampElem));

			if (newSpecificTrampElem == NULL) {
				if (SNOOPTRAMP) {
					printf(
							"Mono-RT - Fehler malloc() insertListElement() Specific Trampoline\n");
					fflush(NULL);
				}
				ret = NULL;
			} else {
				newSpecificTrampElem->specificTrampType
						= (*((MonoTrampolineType *) arg0));
				newSpecificTrampElem->specificTrampAddress = (CODEPTR) item;
				//
				//  Bestimmung des push-Wertes und des Offsets der jump-Anweisung
				//  innerhalb des Specific Trampoline
				//
				if ((*((CODEPTR) newSpecificTrampElem->specificTrampAddress))
						== ((unsigned char) 0x6a)) {
					//
					//  es wird ein Byte gepusht
					//
					newSpecificTrampElem->specificTrampOffsetJumpInstruction
							= 2;
					newSpecificTrampElem->specificTrampPushValue
							= (CODEPTR) (0xff
									& (*((unsigned int *) (newSpecificTrampElem->specificTrampAddress
											+ 1))));
				} else {
					if ((*((CODEPTR) newSpecificTrampElem->specificTrampAddress))
							== ((unsigned char) 0x68)) {
						//
						//  es wird ein Doppelword (32 Bit) gepusht
						//
						newSpecificTrampElem->specificTrampOffsetJumpInstruction
								= 5;
						newSpecificTrampElem->specificTrampPushValue
								= (CODEPTR) (*((unsigned int *) (newSpecificTrampElem->specificTrampAddress
										+ 1)));
					} else {
						//
						//  Die push-Anweisung konnte nicht zugeordnet werden.
						//
						newSpecificTrampElem->specificTrampOffsetJumpInstruction
								= 0;
						newSpecificTrampElem->specificTrampPushValue = 0;
					}
				}
				//
				//  Bestimmung, ob es sich bei der jump-Anweisung im
				//  Specific Trampoline um einen short oder near jump
				//  handelt.
				//
				if (newSpecificTrampElem->specificTrampOffsetJumpInstruction
						!= 0) {

					if ((*((CODEPTR) (newSpecificTrampElem->specificTrampAddress
							+ newSpecificTrampElem->specificTrampOffsetJumpInstruction)))
							== ((unsigned char) 0xe9)) {
						//
						//  near jump
						//
						newSpecificTrampElem->specificTrampHasShortJump = 0x0;
					} else {
						if ((*((CODEPTR) (newSpecificTrampElem->specificTrampAddress
								+ newSpecificTrampElem->specificTrampOffsetJumpInstruction)))
								== ((unsigned char) 0xeb)) {
							//
							//  short jump
							//
							newSpecificTrampElem->specificTrampHasShortJump
									= 0x1;
						} else {
							//
							//  Die jump-Anweisung konnte nicht zugeordnet werden.
							//
							newSpecificTrampElem->specificTrampHasShortJump
									= 0xf;
						}
					}
				} else {
					//
					//  Fehlerfall
					//
					newSpecificTrampElem->specificTrampHasShortJump = 0xf;
				}

				newSpecificTrampElem->specificTrampJumpInstructionTarget
						= (CODEPTR) arg1;
				newSpecificTrampElem->m_IsPatched = 0x0;
				newSpecificTrampElem->specificTrampCodeLength = len;
				newSpecificTrampElem->specificTrampPassedCheck = 0x0;
				newSpecificTrampElem->specificTrampCopy = NULL;
				newSpecificTrampElem->specificTrampSavedJumpInstructionTarget
						= NULL;
				newSpecificTrampElem->number = cnt;

				g_hash_table_insert(MonoSpecificTrampHashTable,
						(gpointer) newSpecificTrampElem->specificTrampAddress,
						(gpointer) newSpecificTrampElem);
				ret = ((void *) newSpecificTrampElem);
			}
		} else {
			ret = ((void *) specificTrampElem);
		}
	}
	return ret;
}
/*
 * @brief insertModifiedGenericTrampolineListElement() fuegt die fuer
 * das Pre-Patch modifizierten Generic Trampoline, die waehrend des Pre-Patch
 * generiert werden, in die entsprechende Hash-Tabelle ein. Die Tabelle
 * unterscheidet sich je nach Pre-Patch-Phase, da beispielsweise waehrend
 * des Pre-Patch normaler Call-Anweisungen Generic Trampoline mit einem
 * anderen Ruecksprungverhalten benoetigt werden als Generic Trampoline
 * des gleichen Typs waehrend des Pre-Patch der VTable-Eintraege.
 *
 * @param	item	Absolute Adresse des Modified Generic Trampoline im Speicher
 * @param	arg0	Typ des Modified Generic Trampoline
 * @param	arg1	nicht belegt
 * @param	len		Gibt an, welche Modified Generic Trampoline Hash Table verwendet wird
 *
 * Aufruf: insertModifiedGenericTrampolineListElement (	ModifiedGenericTrampoline, (void *) modifiedGenericTrampAddress, (void *) &(GenericTrampListPtr->type), NULL, (unsigned int) prePatchMode);
 */
void * insertModifiedGenericTrampolineListElement(enum TrampolineType TrampType_p,
		void *pItem_p, void *pArg0_p, void *pArg1_p, unsigned int uiuPatchMode_p)
{

	struct MonoModifiedGenericTrampElem * pModGenTrampElem;
	struct MonoModifiedGenericTrampElem * pNewModGenTrampElem;
	void * pRet;
	enum PrePatchMode prePatchMode;
	GHashTable * pModGenTrampHash;

	pModGenTrampElem = NULL;
	pNewModGenTrampElem = NULL;
	pRet = NULL;
	prePatchMode = (enum PrePatchMode) uiuPatchMode_p;
	pModGenTrampHash = NULL;

	switch (prePatchMode)
	{
	case PrePatchCallInstructions:
		pModGenTrampHash = pCallModTrampHash_g;
		break;
	case PrePatchImtEmtries:
		pModGenTrampHash = pImtModTrampHash_g;
		break;
	case PrePatchVtableEntries:
		pModGenTrampHash = pVtableModTrampHash_g;
		break;
	case PrePatchPltEntries:
		pModGenTrampHash = pPltModTrampHash_g;
		break;
	default:
		pModGenTrampHash = NULL;
	}

	if (pModGenTrampHash == NULL)
	{
		pRet = NULL;
	}
	else
	{
		pModGenTrampElem = (struct MonoModifiedGenericTrampElem *) g_hash_table_lookup(pModGenTrampHash, (gconstpointer)(*((MonoTrampolineType *) pArg0_p)));
		if (pModGenTrampElem == NULL)
		{
			pNewModGenTrampElem = (struct MonoModifiedGenericTrampElem *) g_malloc(sizeof(struct MonoModifiedGenericTrampElem));
			if (pNewModGenTrampElem == NULL)
			{
				if (SNOOPTRAMP)
				{
					printf("MonoRT - Fehler malloc() insertListElement() Modified Generic Trampoline\n");
					fflush(NULL);
				}
				pRet = NULL;
			}
			else
			{
				pNewModGenTrampElem->modifiedGenericTrampType = (*((MonoTrampolineType *) pArg0_p));
				pNewModGenTrampElem->m_pModGenTramp = (CODEPTR) pItem_p;

				g_hash_table_insert(
						pModGenTrampHash,
						(gpointer) pNewModGenTrampElem->modifiedGenericTrampType,
						(gpointer) pNewModGenTrampElem);

				pRet = ((void *) pNewModGenTrampElem);
			}
		}
		else
		{
			pRet = ((void *) pModGenTrampElem);
		}
	}
	return pRet;
}
/*
 * @brief insertCallCodeInstructionListElement() fuegt die
 * Patches, die waehrend des Pre-JIT emittiert werden, in
 * die Liste "CallCodeList" ein.
 *
 * @param	item	Absolute Adresse des Patches im Speicher
 * @param	arg0	Zeiger auf eine Struktur MonoMethod der Methode, die den Patch enthaelt
 * @param	arg1	Zeiger auf den Code, der den Patch behandelt
 * @param	len		Typ des Patches (Enumeration MonoJumpInfoType)
 *
 * Aufruf: insertCallCodeInstructionListElement(CallCodeInstruction, (void *) ip, (void *) method, (void *) target, (unsigned int)  patch_info->type);
 */
void * insertCallCodeInstructionListElement(enum TrampolineType TrampType_p,
		void * pItem_p, void * pArg0_p, void * pArg1_p, unsigned int uiPatchType_p)
{
	//
	//  Einfuegen der Keys in eine Liste. Somit muss nicht
	//  "g_hash_table_foreach()" zum Abruf aller Elemente
	//  genutzt werden. Das garantiert, dass die Elemente der
	//  Liste stets in der gleichen Reihenfolge abgefragt
	//  werden koennen.
	//
	struct MonoCallCodeList *pCallList;
	struct MonoCallCodeList *pNewCallListElem;
	void * pRet;

	pCallList = NULL;
	pNewCallListElem = NULL;
	pRet = NULL;

	if (CallCodeList.isDestroyed != 0)
	{
		//
		//  Nach dem Pre-Patch, das vor dem Start des
		//  Nutzer-Assemblys ausgefuehrt wird, oder
		//  wenn das Pre-Patch deaktiviert ist,
		//  wird die Emittierung der Call-Anweisungen
		//  nicht mehr aufgezeichnet.
		//
		pRet = NULL;
	}
	else
	{
		pCallList = CallCodeList.head;

		if ((pCallList->head == (&CallCodeList)) && (pCallList->isInitialized == 0))
		{
			//
			// Die Liste wird initial gefuellt
			//
			pCallList->m_pCallerMeth = (MonoMethod *) pArg0_p;
			pCallList->offset = -1;
			pCallList->m_pPatch = (CODEPTR) pItem_p;
			pCallList->m_pPatchTarget = (CODEPTR) pArg1_p;
			pCallList->patchType = (MonoJumpInfoType) uiPatchType_p;
			pCallList->isInitialized = 1;
			pCallList->isDestroyed = 0;
			pRet = ((void *) pCallList);
		} else {

			pNewCallListElem = (struct MonoCallCodeList *) malloc(
					sizeof(struct MonoCallCodeList));

			if (pNewCallListElem == NULL) {
				if (SNOOPTRAMP) {
					printf("Mono-RT - Fehler insertListElement() CallCode\n");
					fflush(NULL);
				}
				pRet = NULL;
			} else {
				pCallList->next = pNewCallListElem;
				pNewCallListElem->prev = pCallList;
				CallCodeList.head = pNewCallListElem;
				pNewCallListElem->head = NULL;
				pNewCallListElem->next = NULL;
				pNewCallListElem->m_pCallerMeth = (MonoMethod *) pArg0_p;
				pNewCallListElem->offset = -1;
				pNewCallListElem->m_pPatch = (CODEPTR) pItem_p;
				pNewCallListElem->m_pPatchTarget = (CODEPTR) pArg1_p;
				pNewCallListElem->patchType = (MonoJumpInfoType) uiPatchType_p;
				pNewCallListElem->isInitialized = 1;
				pNewCallListElem->isDestroyed = 0;
				pRet = ((void *) pNewCallListElem);
			}
		}
	}
	return pRet;
}
/*
 * @brief insertAotCodeListElement() fuegt ein geladenes
 * AOT-Modul in die Liste "AotCodeList" ein.
 *
 * @param	item	Zeiger auf das geladene AOT-Image
 * @param	arg0	nicht belegt
 * @param	arg1	nicht belegt
 * @param	len		nicht belegt
 *
 * Aufruf: insertListElement(AotModule, (void *) amodule, NULL, NULL, 0);
 */
void * insertAotCodeListElement(enum TrampolineType type, void *item,
		void *arg0, void *arg1, unsigned int len) {
	//
	//  Einfuegen der Keys in eine Liste. Somit muss nicht
	//  "g_hash_table_foreach()" zum Abruf aller Elemente
	//  genutzt werden. Das garantiert, dass die Elemente der
	//  Liste stets in der gleichen Reihenfolge abgefragt
	//  werden koennen.
	//
	struct MonoAotCodeList * AotCodeListHeadPtr;
	struct MonoAotCodeList * newAotCodeListElem;
	struct MonoAotCodeList * aotCodeElem;
	void * ret;

	ret = NULL;

	if (AotCodeList.isDestroyed != 0) {
		//
		//  Nach dem Pre-Patch, das vor dem Start des
		//  Nutzer-Assemblys ausgefuehrt wird, oder
		//  wenn das Pre-Patch deaktiviert ist,
		//  wird die Emittierung der Call-Anweisungen
		//  nicht mehr aufgezeichnet.
		//
	} else {

		AotCodeListHeadPtr = AotCodeList.head;

		if ((AotCodeListHeadPtr->head == (&AotCodeList))
				&& (AotCodeListHeadPtr->isInitialized == 0)) {
			//
			// Die Liste wird initial gefuellt.
			//
			AotCodeListHeadPtr->loadedAotModule = (MonoAotModule *) item;
			AotCodeListHeadPtr->isInitialized = 1;
			AotCodeListHeadPtr->isDestroyed = 0;
			AotCodeList.count++;
			ret = ((void *) AotCodeListHeadPtr);
		} else {
			//
			// Die Liste enthaelt bereits mindestens ein Element.
			//
			aotCodeElem = &AotCodeList;
			//
			// Pruefen, ob die Liste das geladene AOT-Image bereits
			// enthaelt.
			//
			while (aotCodeElem != NULL) {
				if (aotCodeElem->loadedAotModule == (MonoAotModule *) item) {
					ret = aotCodeElem;
					break;
				}
				aotCodeElem = aotCodeElem->next;
			}

			if (ret == NULL) {
				newAotCodeListElem = (struct MonoAotCodeList *) malloc(
						sizeof(struct MonoAotCodeList));

				if (NULL == newAotCodeListElem) {
					if (SNOOPTRAMP) {
						printf("Mono-RT - Fehler insertListElement() Aotode\n");
						fflush(NULL);
					}
					ret = NULL;
				} else {
					AotCodeListHeadPtr->next = newAotCodeListElem;
					newAotCodeListElem->prev = AotCodeListHeadPtr;
					AotCodeList.head = newAotCodeListElem;
					newAotCodeListElem->head = NULL;
					newAotCodeListElem->next = NULL;
					newAotCodeListElem->loadedAotModule
							= (MonoAotModule *) item;
					newAotCodeListElem->isInitialized = 1;
					newAotCodeListElem->isDestroyed = 0;
					AotCodeList.count++;
					ret = ((void *) newAotCodeListElem);
				}
			}
		}
	}
	return ret;
}
/*
 * @brief insertVTableListElement() fuegt die VTables,
 * die waehrend des Pre-JIT initialisiert werden, in
 * die Hash-Tabelle "MonoVTHashTable" ein.
 *
 * @param	item	Zeiger auf die initialisierte VTable
 * @param	arg0	Zeiger auf das IMT-Trampoline, mit denen die IMT-Slots initialisiert wurden
 * @param	arg1	Zeiger auf das VTable-Trampoline, mit denen die VTable-Slots initialisiert wurden
 * @param	len		Zeiger auf den Beginn der IMT-Slots
 *
 * Aufruf: insertVTableListElement (VTable, (void *) vt, (void *) imt_trampoline, (void *) vtable_trampoline, (unsigned int) interface_offsets);
 *
 * in mono/metadata/object.c, Z. 2032, mono_class_create_runtime_vtable(), d.h., bei der Initialisierung der VTable einer Klasse
 */
void * insertVTableListElement(enum TrampolineType type, void *item,
		void *arg0, void *arg1, unsigned int len) {

	MonoVTable *vt;
	CODEPTR imt_trampoline;
	CODEPTR vtable_trampoline;
	CODEPTR interface_offsets;
	struct MonoVTableElem * vTableElem;
	struct MonoVTableElem * newVTableElem;
	void * ret;

	if (MonoVTHashTable == NULL) {
		ret = NULL;
	} else {

		vt = (MonoVTable *) item;
		imt_trampoline = (CODEPTR) arg0;
		vtable_trampoline = (CODEPTR) arg1;
		interface_offsets = (CODEPTR) len;

		vTableElem = (struct MonoVTableElem *) g_hash_table_lookup(
				MonoVTHashTable, (gconstpointer) vt->klass);

		if (vTableElem == NULL) {
			//
			//  Die VTable dieser Klasse wurde noch
			//  nicht aufgezeichnet.
			//
			newVTableElem = (struct MonoVTableElem *) g_malloc(
					sizeof(struct MonoVTableElem));

			if (newVTableElem == NULL) {
				if (SNOOPTRAMP) {
					printf(
							"Mono-RT - Fehler malloc() insertListElement() VTableElem\n");
					fflush(NULL);
				}
				ret = NULL;
			} else {
				newVTableElem->vtable = vt;
				newVTableElem->vTableIMTTrampAddress = imt_trampoline;
				newVTableElem->vTableVTableTrampAddress = vtable_trampoline;
				newVTableElem->vTableFirstIMTSlot = interface_offsets;

				g_hash_table_insert(MonoVTHashTable, (gpointer) vt->klass,
						(gpointer) newVTableElem);
				ret = ((void *) newVTableElem);
			}
		} else {
			ret = ((void *) vTableElem);
		}
	}
	return ret;
}
/**
 * @brief insertListElement() ruft je nach Typ die entsprechende
 * Funktion zum Einfuegen eines Elements in die Hash-Tabellen
 * und Listen, die fuer das Pre-Patch benoetigt werden.
 *
 * @param	type	Legt die Hash-Tabelle/Liste fest, in die eingefuegt wird
 * @param	item	Die Bedeutung ist vom einzufuegenden Element abhaengig
 * @param	arg0	Die Bedeutung ist vom einzufuegenden Element abhaengig
 * @param	arg1	Die Bedeutung ist vom einzufuegenden Element abhaengig
 * @param	len		Die Bedeutung ist vom einzufuegenden Element abhaengig
 *
 * Diese Funktion bildet das Rueckgrat des Pre-Patch. Sie wird an zahlreichen
 * Stellen im Mono-Quellcode gerufen. Mit ihrer Hilfe werden die Verwaltungsstrukturen,
 * d.h., Hash-Tabellen und Listen, von compilierten Methoden, Klassen, VTables,
 * sowie den Trampolines aufgebaut. Die Bedeutungen der Parameter haengen vom
 * einzufuegenden Element ab und sind in den Kommentaren der einzelnen Funktionen
 * erlaeutert. Diese Methode verlangsamt das Pre-JIT wesentlich, da haeufig
 * Hash-Tabellen gelesen und geschrieben werden muessen und Speicher fuer neue
 * Elemente allokiert werden muss.
 */
void * insertListElement(enum TrampolineType type, void *item, void *arg0,
		void *arg1, unsigned int len) {

	void * ret;

	switch (type) {
	case Class: // done
		ret = insertClassListElement(type, item, arg0, arg1, len);
		break;
	case NativeCode:
		ret = insertNativeCodeListElement(type, item, arg0, arg1, len);
		break;
	case GenericTrampoline:
		ret = insertGenericTrampolineListElement(type, item, arg0, arg1, len);
		break;
	case SpecificTrampoline: // done
		ret = insertSpecificTrampolineListElement(type, item, arg0, arg1, len);
		break;
	case ModifiedGenericTrampoline:
		ret = insertModifiedGenericTrampolineListElement(type, item, arg0,
				arg1, len);
		break;
	case CallCodeInstruction: // done
		ret = insertCallCodeInstructionListElement(type, item, arg0, arg1, len);
		break;
	case VTable: //done
		ret = insertVTableListElement(type, item, arg0, arg1, len);
		break;
	case AotModule: // done
		ret = insertAotCodeListElement(type, item, arg0, arg1, len);
		break;
	default:
		ret = NULL;
	}

	return ret;
}
/**
 * @brief printListElement() gibt ein Element aus einer
 * angelegten Hash-Tabelle aus (Debugging).
 * @param	hashTableKey	Key des Elements der Hash-Tabelle
 * @param	hashTableValue	Value des Elements der Hash-Tabelle
 * @param	listTypeToPrint	TrampolineType zur Idendifikation der Hash-Tabelle
 *
 * Der Aufruf der Methode erfolgt mittels 'g_hash_table_for_each ()', so dass
 * die Signatur der Methode vorgegeben ist. DIe Ausgaben dienen dem Debugging.
 */
static void printListElement (gpointer hashTableKey, gpointer hashTableValue,
		gpointer listTypeToPrint)
{
	static int num_st = 0;
	int counter;
	char *type;
	enum TrampolineType listType;

	listType = (enum TrampolineType) listTypeToPrint;
	//
	//  Ausgabe der angelegten Specific Trampoline fuer Debugging-Zwecke
	//
	if (listType == SpecificTrampoline) {

		struct MonoSpecificTrampElem * specificTrampElem;
		specificTrampElem = (struct MonoSpecificTrampElem *) hashTableValue;

		switch (specificTrampElem->specificTrampType) {
		case (MONO_TRAMPOLINE_JIT):
			type = (char*) "JIT";
			break;
		case (MONO_TRAMPOLINE_JUMP):
			type = (char*) "JUMP";
			break;
		case (MONO_TRAMPOLINE_CLASS_INIT):
			type = (char*) "CLASS_INIT";
			break;
		case (MONO_TRAMPOLINE_GENERIC_CLASS_INIT):
			type = (char*) "GENERIC_CLASS_INIT";
			break;
		case (MONO_TRAMPOLINE_RGCTX_LAZY_FETCH):
			type = (char*) "RGCTX_LAZY_FETCH";
			break;
		case (MONO_TRAMPOLINE_AOT):
			type = (char*) "AOT";
			break;
		case (MONO_TRAMPOLINE_AOT_PLT):
			type = (char*) "AOT_PLT";
			break;
		case (MONO_TRAMPOLINE_DELEGATE):
			type = (char*) "DELEGATE";
			break;
		case (MONO_TRAMPOLINE_RESTORE_STACK_PROT):
			type = (char*) "RESTORE_STACK_PROT";
			break;
		case (MONO_TRAMPOLINE_GENERIC_VIRTUAL_REMOTING):
			type = (char*) "GENERIC_VIRTUAL_REMOTING";
			break;
		case (MONO_TRAMPOLINE_MONITOR_ENTER):
			type = (char*) "MONITOR_ENTER";
			break;
		case (MONO_TRAMPOLINE_MONITOR_EXIT):
			type = (char*) "MONITOR_EXIT";
			break;
		case (MONO_TRAMPOLINE_NUM):
			type = (char*) "NUM";
			break;
		default:
			type = (char*) "Other";
		}
		num_st++;

		validateSpecificTrampolineJumpTarget(specificTrampElem);
		if (specificTrampElem->specificTrampType == MONO_TRAMPOLINE_DELEGATE) {

			/*			printf("Specific Trampoline\tType: %s\tAddress: %p\tLength: %u\tshort_jump: %x\tpassed: %s\n",
			 type, specificTrampElem->specificTrampAddress,
			 (OUTPUTTYPE) specificTrampElem->specificTrampCodeLength,
			 specificTrampElem->specificTrampHasShortJump, ((specificTrampElem->specificTrampPassedCheck == 0x0) ? "ja" : "nein" ));

			 printf("Specific Trampoline\tType: %s\tAddress: %p\tPush: %p\n",
			 type, specificTrampElem->specificTrampAddress,
			 (OUTPUTTYPE) specificTrampElem->specificTrampPushValue);
			 */
			printf("break *%p\n", specificTrampElem->specificTrampAddress);
			fflush(NULL);
		}

		return;
	}
	//
	//  Ausgabe der angelegten Generic Trampoline fuer Debugging-Zwecke
	//
	if (listType == GenericTrampoline) {

		struct MonoGenericTrampElem * genericTrampElem;
		genericTrampElem = (struct MonoGenericTrampElem *) hashTableValue;

		switch (genericTrampElem->genericTrampType) {
		case (MONO_TRAMPOLINE_JIT):
			type = (char*) "JIT";
			break;
		case (MONO_TRAMPOLINE_JUMP):
			type = (char*) "JUMP";
			break;
		case (MONO_TRAMPOLINE_CLASS_INIT):
			type = (char*) "CLASS_INIT";
			break;
		case (MONO_TRAMPOLINE_GENERIC_CLASS_INIT):
			type = (char*) "GENERIC_CLASS_INIT";
			break;
		case (MONO_TRAMPOLINE_RGCTX_LAZY_FETCH):
			type = (char*) "RGCTX_LAZY_FETCH";
			break;
		case (MONO_TRAMPOLINE_AOT):
			type = (char*) "AOT";
			break;
		case (MONO_TRAMPOLINE_AOT_PLT):
			type = (char*) "AOT_PLT";
			break;
		case (MONO_TRAMPOLINE_DELEGATE):
			type = (char*) "DELEGATE";
			break;
		case (MONO_TRAMPOLINE_RESTORE_STACK_PROT):
			type = (char*) "RESTORE_STACK_PROT";
			break;
		case (MONO_TRAMPOLINE_GENERIC_VIRTUAL_REMOTING):
			type = (char*) "GENERIC_VIRTUAL_REMOTING";
			break;
		case (MONO_TRAMPOLINE_MONITOR_ENTER):
			type = (char*) "MONITOR_ENTER";
			break;
		case (MONO_TRAMPOLINE_MONITOR_EXIT):
			type = (char*) "MONITOR_EXIT";
			break;
		case (MONO_TRAMPOLINE_NUM):
			type = (char*) "NUM";
			break;
		default:
			type = (char*) "Other";
		}
		printf("Generic Trampoline\tType: %s\tAddress: %p\n", type,
				genericTrampElem->genericTrampAddress);
		return;
	}
	//
	//  Ausgabe des nativen Codes fuer Debugging-Zwecke
	//
	if (listType == NativeCode) {

		struct MonoNativeCodeElem * nativeCodeElem;
		nativeCodeElem = (struct MonoNativeCodeElem *) hashTableValue;

		printf(
				"Native Code\tName: %s\tOld: %p\tNew: %p\tLen: %u\tHandle: %p\n",
				mono_method_full_name(nativeCodeElem->nativeCodeMethodPtr, TRUE),
				nativeCodeElem->nativeCodeTempAddress,
				nativeCodeElem->nativeCodeFinalAddress,
				(OUTPUTTYPE) nativeCodeElem->nativeCodeCodeLength,
				nativeCodeElem->nativeCodeMethodPtr);

		return;
	}
	//
	//  Ausgabe der compilierten Klassen fuer Debugging-Zwecke
	//
	if (listType == Class) {

		struct MonoClassElem * classElem;
		struct MonoVTableElem * vTableElem;

		classElem = (struct MonoClassElem *) hashTableValue;

		vTableElem = (struct MonoVTableElem *) g_hash_table_lookup(
				MonoVTHashTable, (gconstpointer)(classElem->klasse));

		printf("Klasse: %s\tVT-Size: %i\tIF-Count: %i\tMC: %i\tVT: %s\n",
				classElem->klasse->name, classElem->klasse->vtable_size,
				(int) classElem->klasse->interface_count,
				classElem->klasse->method.count, ((vTableElem == NULL) ? "nein"
						: "ja"));
		fflush(NULL);

		for (counter = 0; counter < classElem->klasse->method.count; counter++) {
			printf("\tMethode: %s\tVirtual: %i\tHandle: %p\n",
					classElem->klasse->methods[counter]->name,
					(classElem->klasse->methods[counter]->flags
							& METHOD_ATTRIBUTE_VIRTUAL),
					classElem->klasse->methods[counter]);
			fflush(NULL);
		}

		return;
	}
	//
	//  Ausgabe der initialisierte VTables fuer Debugging-Zwecke
	//
	if (listType == VTable) {

		struct MonoClassElem * classElem;
		MonoVTable * vTable;

		vTable = (MonoVTable *) hashTableValue;

		if (vTable == NULL) {
			printf("VTable-Eintrag fehlerhaft\n");
			fflush(NULL);
			return;
		}

		printf("VT-Klasse: %s\tVT-Size: %i\tIF-Count: %i\tMethod-Count: %i\n",
				vTable->klass->name, vTable->klass->vtable_size,
				(int) vTable->klass->interface_count,
				(int) vTable->klass->method.count);
		fflush(NULL);

		classElem = (struct MonoClassElem *) g_hash_table_lookup(
				MonoClassHashTable, (gconstpointer) vTable->klass);

		if (classElem == NULL) {
			printf("Class nicht gefunden\n");
			fflush(NULL);
		} else {
			printf("Class gefunden\n");
			for (counter = 0; counter < vTable->klass->method.count; counter++) {
				printf("\tMethode: %s\tVirtual: %i\n", mono_method_full_name(
						vTable->klass->methods[counter], TRUE),
						(vTable->klass->methods[counter]->flags
								& METHOD_ATTRIBUTE_VIRTUAL));
				fflush(NULL);
			}
		}
		return;
	}
}
/**
 * @brief printListSizes() gibt die Größen der angelegten
 * Hash-Tabellen aus (Debugging).
 */
static void printListSizes ()
{
	if (SNOOPTRAMP)
	{
		printf("Size NativeCodeHashTable: %i\n", (int) g_hash_table_size(
				MonoNativeCodeHashTable));
		printf("Size SpecificTrampHashTable: %i\n", (int) g_hash_table_size(
				MonoSpecificTrampHashTable));
		printf("Size GenericTrampHashTable: %i\n", (int) g_hash_table_size(
				MonoGenericTrampHashTable));
		//		printf("Size pCallModTrampHash_g: %i\n", (int) g_hash_table_size(MonoCallInstructionModifiedGenericTrampHashTable));
		//		printf("Size pImtModTrampHash_g: %i\n", (int) g_hash_table_size(MonoImtEntryModifiedGenericTrampHashTable));
		//		printf("Size pVtableModTrampHash_g: %i\n", (int) g_hash_table_size(MonoVtableEntryModifiedGenericTrampHashTable));
		printf("Size MonoClassHashTable: %i\n", (int) g_hash_table_size(
				MonoClassHashTable));
		printf("Size MonoVTHashTable: %i\n", (int) g_hash_table_size(
				MonoVTHashTable));
		fflush(NULL);
	}
	return;
}
/**
 * @brief printLists() gibt die Elemente der angelegten
 * Hash-Tabellen aus (Debugging).
 */
static void printLists ()
{
	if (PATCHLIST) {
		printf("printLists beginnt\n");
		fflush(NULL);
		g_hash_table_foreach(MonoGenericTrampHashTable, printListElement,
				(gpointer)((void *) GenericTrampoline));
		//		g_hash_table_foreach(MonoNativeCodeHashTable, printListElement, (gpointer) ((void *) NativeCode));
		g_hash_table_foreach(MonoSpecificTrampHashTable, printListElement,
				(gpointer)((void *) SpecificTrampoline));
		//		g_hash_table_foreach(MonoClassHashTable, printListElement,(gpointer) ((void *) Class));
		//		g_hash_table_foreach(MonoVTHashTable, printListElement,(gpointer) ((void *) VTable));
		printf("printLists beendet\n");
		fflush(NULL);
	}
	return;
}
/**
 * @brief validateSpecificTrampolineJumpTarget() ueberprueft, ob die
 * Verwaltungsdatenstruktur zur Erfassung der emittierten Specific
 * Trampoline mit dem nativen Code des Specific Trampoline konsistent ist.
 * @param	specificTrampElem	Zeiger auf eine Verwaltungsdatenstruktur eines Specific Trampoline
 *
 * Es wird die Konsistenz der Specific Trampoline geprueft. Dabei
 * werden die Ziele der Jump-Intruktionen mit den gespeicherten
 * Werten verglichen. Diese ueberpruefung ist obsolet, da bei allen
 * Versuchen keine Probleme festgestellt wurden. Diese ueberpruefung
 * ist standardmaeßig deaktiviert. Sie kann ueber den Kommandozeilenparamter
 * '--checkST' aktiviert werden. Das Pre-Patch wird dann fuer die
 * Specific Trampoline uebersprungen, bei denen ein Problem
 * festgestellt wird.
 */
static void validateSpecificTrampolineJumpTarget(struct MonoSpecificTrampElem * specificTrampElem)
{
	CODEPTR jumpInstructionPtr;
	CODEPTR jumpInstructionArgumentPtr;
	CODEPTR jumpInstructionTarget;
	OFFSET jumpInstructionArgument;

#if defined(TARGET_X86)	// Archtitekturabhaengiger Pre-Patch-Code
	//
	//  Die globale Variable 'validateSpecificTrampoline_g' dient der
	//  Aktivierung dieser Funktion. Wird der entsprechende
	//  Kommandozeilenparamter nicht gesetzt, so kehrt die Funktion
	//  sofort zurueck, ohne eine ueberpruefung des Specific Trampoline
	//  vorzunehmen.
	//
	if (validateSpecificTrampoline_g == 0)
	{
		return;
	}
	//
	//  Falls bereits Inkonsistenz festgestellt wurde, kehrt
	//  die Funktion umgehend zurueck.
	//
	if (specificTrampElem->specificTrampPassedCheck != 0x0)
		return;
	//
	//  Die Felder der Specific Trampoline Verwaltungsdatenstruktur sind
	//  auf diese Werte gesetzt, genau dann wenn bei der Erfassung
	//  des Specific Trampoline breits ein Fehler auftrat.
	//
	if ((specificTrampElem->specificTrampOffsetJumpInstruction == 0)
			|| (specificTrampElem->specificTrampHasShortJump
					== ((unsigned char) 0xf))) {

		specificTrampElem->specificTrampPassedCheck = 0x1;
		return;
	}

	jumpInstructionPtr
			= (unsigned char *) ((unsigned int) specificTrampElem->specificTrampAddress
					+ specificTrampElem->specificTrampOffsetJumpInstruction);

	jumpInstructionArgumentPtr = (jumpInstructionPtr + 1);
	//
	//  Bestimmung der Adresse, die das Specific Trampoline angespringt.
	//  Dabei muessen die Faelle eines Near und Short Jump unterschieden werden.
	//
	if ((specificTrampElem->specificTrampHasShortJump == ((unsigned char) 0x0))
			&& ((*jumpInstructionPtr) == ((unsigned char) 0xe9))) {
		//
		//  ueberpruefung Near Jump
		//
		jumpInstructionArgument = *((OFFSET *) jumpInstructionArgumentPtr);
		jumpInstructionArgument += 5;
	} else {
		if ((specificTrampElem->specificTrampHasShortJump
				== ((unsigned char) 0x1)) && ((*jumpInstructionPtr)
				== ((unsigned char) 0xeb))) {
			//
			//  ueberpruefung Short Jump
			//
			jumpInstructionArgument = (OFFSET) (0x000000ff
					& (*jumpInstructionArgumentPtr));
			jumpInstructionArgument += 2;
		} else {
			//
			//  Die Jump-Instruktion konnte nicht zugeordnet werden.
			//
			printf("Fehler jump-Anweisung Specific Trampoline:\n");
			printf(
					"\tshort_jump: %x\tjump-Anweisung: %x\n",
					(OUTPUTTYPE) (specificTrampElem->specificTrampHasShortJump),
					(OUTPUTTYPE) (*jumpInstructionPtr));

			specificTrampElem->specificTrampPassedCheck = 0x1;
			return;
		}
	}
	//
	//  Ueberpruefung, ob der aus dem Speicher ermittelte Wert mit dem
	//  in der Verwaltungsdatenstruktur uebereinstimmt.
	//
	jumpInstructionTarget = (CODEPTR) (((OFFSET) jumpInstructionPtr)
			+ jumpInstructionArgument);
	if (jumpInstructionTarget
			!= specificTrampElem->specificTrampJumpInstructionTarget) {

		printf("Fehler jump-Ziel Specific Trampoline:\n");
		printf("\tSoll: %p\tIst: %p\tPush: %x\n",
				specificTrampElem->specificTrampJumpInstructionTarget,
				jumpInstructionTarget,
				(OUTPUTTYPE) (specificTrampElem->specificTrampPushValue));

		specificTrampElem->specificTrampPassedCheck = 0x1;
	} else {
		if (SNOOPTRAMP) {
			printf("\tchecked\n");
		}
		specificTrampElem->specificTrampPassedCheck = 0x0;
	}
	return;

#elif defined(TARGET_ARM)	// Archtitekturabhaengiger Pre-Patch-Code
	return;
#else						// Archtitekturabhaengiger Pre-Patch-Code
	return;
#endif
}
/**
 * @brief isX86CallInstruction() prueft, ob sich an einer als Parameter
 * uebergebenen Speicheradresse eine x86-Call-Instruktion befindet.
 * @param	pCallInst_p			Absolute Speicheradresse des fraglichen Opcodes.
 * @return	Handelt es sich um eine Call-Instruktion, wird der Wert 1 zurueckgegeben, sonst 0.
 *
 * ueberpruefung, ob es sich bei dem Opcode, dessen
 * Adresse als Parameter uebergeben wird, um den Opcode
 * einer direkten Call-Instruktion (Opcode 0xe8) handelt.
 * Diese Funktion ist spezifisch fuer die x86-Archtitektur.
 */
static int isX86CallInstruction(CODEPTR pCallInst_p)
{
#if defined(TARGET_X86)	// Archtitekturabhaengiger Pre-Patch-Code
	return (((*pCallInst_p) == ((unsigned char) 0xe8)) ? 1 : 0);

#elif defined(TARGET_ARM)	// Archtitekturabhaengiger Pre-Patch-Code
	return 0;
#else						// Archtitekturabhaengiger Pre-Patch-Code
	return 0;
#endif
}
/**
 * @brief isX86NopInstruction() prueft, ob sich an einer als Parameter
 * uebergebenen Speicheradresse eine x86-NOP-Instruktion befindet.
 * @param	instructionPtr	Absolute Speicheradresse des fraglichen Opcodes.
 * @return	Falls es sich um einen NOP-Opcode handelt, wird der Wert 1 zurueckgegeben, sonst 0.
 *
 * Ueberpruefung, ob es sich bei dem Opcode, dessen
 * Adresse als Parameter uebergeben wird, um eine
 * Nop-Anweisung handelt. Diese Funktion ist spezifisch
 * fuer die x86-Archtitektur.
 */
static int isX86NopInstruction(CODEPTR pInst_p)
{
#if defined(TARGET_X86)	// Archtitekturabhaengiger Pre-Patch-Code
	return (((*pInst_p) == ((unsigned char) 0x90)) ? 1 : 0);

#elif defined(TARGET_ARM)	// Archtitekturabhaengiger Pre-Patch-Code
	return 0;
#else						// Archtitekturabhaengiger Pre-Patch-Code
	return 0;
#endif
}
/**
 * @brief getX86CallTarget() berechnet die von einer Call-Instruktion
 * angesprungene absolute Speicher-Adresse.
 * @param	callInstructionPtr	Zeiger auf eine Call-Instruktion
 * @return	Absolute Speicher-Adresse des "Sprung-Ziels" der Call-Instruktion
 *
 * Berechnung der absoluten Adresse des Ziels
 * einer Call-Anweisung. Die Call-Instruktion selbst
 * wird nicht ueberprueft, sondern die auf zuvor
 * identifizierte Call-Instruktion folgenden 4 Bytes
 * als Offset eines relativ direkten Calls interpretiert.
 * Diese Funktion ist spezifisch fuer die x86-Archtitektur.
 */
static CODEPTR getX86CallTarget(CODEPTR pCallInst_p)
{
	CODEPTR pCallInstArg;
	OFFSET callInstructionArgument;
	CODEPTR pCallInstTgt;
	volatile int test;

	pCallInstArg = NULL;
	callInstructionArgument = 0;
	pCallInstTgt = NULL;
	test = 0;
#if defined(TARGET_X86)	// Archtitekturabhaengiger Pre-Patch-Code
	pCallInstArg = (pCallInst_p + 1);
	callInstructionArgument = *((OFFSET *) pCallInstArg);
	callInstructionArgument += 5; // vgl. mono/arch/x86/x86-codegen.h, Zeile 1595
	pCallInstTgt = (CODEPTR) (((OFFSET) pCallInst_p) + callInstructionArgument);
	return pCallInstTgt;
#elif defined(TARGET_ARM)	// Archtitekturabhaengiger Pre-Patch-Code
	return NULL;
#else						// Archtitekturabhaengiger Pre-Patch-Code
	return NULL;
#endif
}

/**
 * @brief modifyX86CallInstructionTarget() modifiziert das Argument einer
 * Call-Instruktion im nativen Code.
 * @param	callInstructionPtr			Zeiger auf den Call-Instruktion,deren Ziel veraendert werden soll.
 * @param	callInstructionNewTarget	Neues Ziel der Call-Instruktion.
 *
 * Das Argument bzw. das relative Ziel der Call-Instruktion,
 * deren Adresse in 'callInstructionPtr' gespeichert ist,
 * wird so veraendert, dass die Call-Instruktion danach den
 * Code an der in 'callInstructionNewTarget' vermerkten
 * Speicheradresse ausfuehrt bzw. anspringt. Die Call-Instruktion
 * selbst wird nicht auf Gueltigkeit geprueft. Diese Funktion ist
 * spezifisch fuer die x86-Archtitektur.
 */
static void modifyX86CallInstructionTarget (CODEPTR pCallInst_p, CODEPTR pCallInstTgt_p)
{
	CODEPTR pCallInstArg;
	OFFSET callInstArg;

	pCallInstArg = NULL;
	callInstArg = 0;
#if defined(TARGET_X86)	// Archtitekturabhaengiger Pre-Patch-Code
	callInstArg = (OFFSET) (((OFFSET) pCallInstTgt_p) - ((OFFSET) pCallInst_p));
	callInstArg -= 5;
	pCallInstArg = (pCallInst_p + 1);
	*((OFFSET *) pCallInstArg) = callInstArg;
	return;
#elif defined(TARGET_ARM)	// Archtitekturabhaengiger Pre-Patch-Code
	return;
#else						// Archtitekturabhaengiger Pre-Patch-Code
	return;
#endif
}

/**
 * @brief initPrePatchDataStructures() initialisiert die fuer das Pre-Patch
 * nachtraeglich in Mono eingefuegten globalen Datenstrukturen und Variablen.
 */
void initPrePatchDataStructures()
{
	struct MonoClassList * pClassList;
	struct MonoCallCodeList * pCallCode;
	struct MonoAotCodeList * pAotCode;

	pClassList = NULL;
	pCallCode = NULL;
	pAotCode = NULL;
	//
	// Initialize global variables
	doTrace_g = 0;
	doTraceLevel2_g = 0;
	snoopcan = 0;
	snoopcap = 0;
	snooptc = 0;
	doTraceJIT_g = 0;
	doTraceJITCmd_g = 0;
	doTracePatch_g = 0;
	doTracePatchCmd_g = 0;
	snoopcallvirt = 0;
	snoopvtpp = 0;
	snooptramp = 0;
	snoopwrapper = 0;
	snoopwrapper_org = 0;
	doPreJIT_g = 0;
	doPrePatch_g = 0;
	doPrePatchCmd_g = 0;
	doPrePatchVirt_g = 0;
	disablegc = 0;
	patchlist = 0;
	disableMonoGetGenericContextFromCode_g = 0;
	validateSpecificTrampoline_g = 0;
	useSlowPath_g = 0;
	//
	//  Initialisierung der Liste zur Speicherung der
	//  Klassen der Methoden, die waehrend des Pre-JIT
	//  behandelt wurden. Diese Liste wird waehrend
	//  des IMT-/VTable-Pre-Patch verwendet.
	//
	pClassList = &ClassList;
	pClassList->head = &ClassList;
	pClassList->klasse = NULL;
	pClassList->next = NULL;
	pClassList->prev = NULL;
	pClassList->isDestroyed = 0;
	//
	//  Initialisierung der Liste zur Speicherung der
	//  emittierten direkten Call-Instruktionen.
	//
	pCallCode = &CallCodeList;
	pCallCode->head = &CallCodeList;
	pCallCode->m_pCallerMeth = NULL;
	pCallCode->offset = -1;
	pCallCode->m_pPatch = NULL;
	pCallCode->m_pPatchTarget = NULL;
	pCallCode->patchType = 0;
	pCallCode->isDestroyed = 0;
	pCallCode->isInitialized = 0;
	pCallCode->next = NULL;
	pCallCode->prev = NULL;
	//
	//  Initialisierung der Liste zur Speicherung der
	//  geladnenen AOT-Images.
	//
	pAotCode = &AotCodeList;
	pAotCode->head = &AotCodeList;
	pAotCode->isDestroyed = 0;
	pAotCode->isInitialized = 0;
	pAotCode->count = 0;
	pAotCode->next = NULL;
	pAotCode->prev = NULL;
	//
	//  Anlegen der Hash-Tabellen.
	//
	MonoGenericTrampHashTable = g_hash_table_new_full(g_direct_hash,
			g_direct_equal, NULL, g_free);
	MonoSpecificTrampHashTable
			= g_hash_table_new_full(NULL, NULL, NULL, g_free);
	pCallModTrampHash_g = g_hash_table_new_full(NULL, NULL, NULL, g_free);
	pImtModTrampHash_g = g_hash_table_new_full(NULL, NULL, NULL, g_free);
	pVtableModTrampHash_g = g_hash_table_new_full(NULL, NULL, NULL, g_free);
	pPltModTrampHash_g = g_hash_table_new_full(NULL, NULL, NULL, g_free);
	MonoNativeCodeHashTable = g_hash_table_new_full(NULL, NULL, NULL, g_free);
	MonoClassHashTable = g_hash_table_new_full(NULL, NULL, NULL, g_free);
	MonoVTHashTable = g_hash_table_new_full(NULL, NULL, NULL, g_free);
	ExcludedFromPreJit = g_hash_table_new_full(g_direct_hash, g_direct_equal,
			NULL, NULL);
	//
	//  Initialisierung der Variablen zur Speicherung der
	//  Adressen der IMT- und VTable-Trampoline.
	//
	pImtTramp_g = NULL;
	pVTableTramp_g = NULL;
	//
	//  Anlegen eines modifizierten Generic Delegate Trampoline,
	//  welches fuer das Pre-Patch der Delegates benötigt wird.
	//
	createModifiedDelegateTrampoline_g = 1;
	pModDelTramp_g = mono_arch_create_trampoline_code(MONO_TRAMPOLINE_DELEGATE);
	createModifiedDelegateTrampoline_g = 0;
	return;
}
/**
 * parseMethodsForAsmTrace() traegt die Namen der Methoden, deren
 * nativer Code auf der Konsole angezeigt werden soll, in das
 * Feld acAsmMethods_g[]' ein.
 * @param	arg	Zeiger auf die Zeichenkette der Methodennamen
 */
void parseMethodsForAsmTrace(const char * arg)
{
	int i, j, k, l;
	char* tempMethodName;
	char* methodName;
	int asmStringLength;
	//
	//  Initialisierung des Feldes, welches die
	//  Zeiger auf die Zeichenketten der anzuzeigenden
	//  Methoden enthaelt
	//
	for (i = 0; i < MAXMETHODSASMTRACE; i++)
	{
		acAsmMethods_g[i] = NULL;
	}
	l = 0;
	//
	//  Redundante Pruefung der ersten 11 Byte der uebergebenen Zeichenkette,
	//  ob sie die korrekte Option darstellt.
	//
	if (strncmp(arg, "--asmtrace=", 11) == 0) {
		asmStringLength = strlen(arg) - 11;
		j = 11;
		//
		//  Gehe die Zeichenkette mit den Komma-separierten Methodennamen
		//  bis zum String-Terminierungszeichen durch.
		//
		while (arg[j] != '\0') {
			//
			// zaehlt die String-Laenge eines Methodennamen
			//
			k = 0;
			//
			//  Reserviere Speicher fuer einen Methodennamen.
			//  Da dessen Laenge nicht bekannt ist, wird zunaechst
			//  so viel Speicher reserviert, so dass die gesamte
			//  Optionen-Zeichenkette hineinpasst.
			//
			//  Der Speicher wird mit Nullen initialisiert.
			//
			tempMethodName = (char *) calloc(1, asmStringLength + 1);

			if (tempMethodName == NULL) {
				exit(1);
			}
			//
			// Zeichenweises Kopieren der Komma-separierten Methodennamen
			// bis auf ein Stringterminierungs-Zeichen oder ein Komma
			// getroffen wird.
			//
			while ((arg[j] != ',') && (arg[j] != '\0')) {

				tempMethodName[k] = arg[j];
				k++;
				j++;
			}

			if (k == 0) {
				//
				//  Es wurde nur ein Komma gefunden oder
				//  die Zeichenkette ist zu Ende. Beginne
				//  beim naechsten Zeichen.
				//
				j++;
				continue;
			}
			//
			//  Reserviere Speicher fuer die exakte Laenge des
			//  gerade kopierten Methodennamens.
			//
			methodName = (char *) calloc(1, (size_t) (k + 1));

			if (methodName == NULL) {
				exit(1);
			}
			//
			//  Kopiere den String in den Speicherbereich mit
			//  der exakten Laenge.
			//
			strncpy(methodName, tempMethodName, (k + 1));
			//
			//  Setze den Feld-Eintrag auf die erstellte Zeichenkette.
			//
			acAsmMethods_g[l] = methodName;
			l++;
			free(tempMethodName);
			//
			//  Falls das letzte betrachtete Zeichen der Optionen-Zeichenkette
			//  ein Komma war, so könnte noch ein Methodenname folgen. Falls nicht,
			//  oder wenn das Maximum der Methodennamen erreicht wurde,
			//  beende die Funktion.
			//
			if ((arg[j] == ',') && (l < MAXMETHODSASMTRACE)) {
				j++;
			} else {
				return;
			}
		}
	}
}

static guint8 * createX86ModifiedGenericTrampoline (MonoTrampolineType TrampTpe_p)
{
	guint8 *buf, *code, *tramp;
	int pushed_args, pushed_args_caller_saved;

#if defined(TARGET_X86)	// Archtitekturabhaengiger Pre-Patch-Code
	code = buf = mono_global_codeman_reserve(256);

	x86_push_reg (buf, X86_EDI);
	x86_push_reg (buf, X86_ESI);
	x86_push_reg (buf, X86_EBP);
	x86_push_reg (buf, X86_ESP);
	x86_push_reg (buf, X86_EBX);
	x86_push_reg (buf, X86_EDX);
	x86_push_reg (buf, X86_ECX);
	x86_push_reg (buf, X86_EAX);

	pushed_args_caller_saved = pushed_args = 8;

	/* Align stack on apple */
	x86_alu_reg_imm (buf, X86_SUB, X86_ESP, 4);

	pushed_args++;

	/* save LMF begin */

	/* save the IP (caller ip) */
	if (TrampTpe_p == MONO_TRAMPOLINE_JUMP)
		x86_push_imm (buf, 0);
	else
		x86_push_membase (buf, X86_ESP, (pushed_args + 1) * sizeof (gpointer));

	pushed_args++;

	x86_push_reg (buf, X86_EBP);
	x86_push_reg (buf, X86_ESI);
	x86_push_reg (buf, X86_EDI);
	x86_push_reg (buf, X86_EBX);

	pushed_args += 4;

	/* save ESP */
	x86_push_reg (buf, X86_ESP);
	/* Adjust ESP so it points to the previous frame */
	x86_alu_membase_imm (buf, X86_ADD, X86_ESP, 0, (pushed_args + 2) * 4);

	pushed_args++;

	/* save method info */
	if ((TrampTpe_p == MONO_TRAMPOLINE_JIT) || (TrampTpe_p == MONO_TRAMPOLINE_JUMP))
		x86_push_membase (buf, X86_ESP, pushed_args * sizeof (gpointer));
	else
		x86_push_imm (buf, 0);

	pushed_args++;

	/* On apple, the stack is correctly aligned to 16 bytes because pushed_args is
	 * 16 and there is the extra trampoline arg + the return ip pushed by call
	 * FIXME: Note that if an exception happens while some args are pushed
	 * on the stack, the stack will be misaligned.
	 */
	g_assert(pushed_args == 16);

	/* get the address of lmf for the current thread */
	x86_call_code (buf, mono_get_lmf_addr);
	/* push lmf */
	x86_push_reg (buf, X86_EAX);
	/* push *lfm (previous_lmf) */
	x86_push_membase (buf, X86_EAX, 0);
	/* Signal to mono_arch_find_jit_info () that this is a trampoline frame */
	x86_alu_membase_imm (buf, X86_ADD, X86_ESP, 0, 1);
	/* *(lmf) = ESP */
	x86_mov_membase_reg (buf, X86_EAX, 0, X86_ESP, 4);
	pushed_args += 2;

	x86_push_imm (buf, 0);

	pushed_args++;

	x86_push_membase (buf, X86_ESP, pushed_args * sizeof (gpointer));

	pushed_args++;

	if (TrampTpe_p == MONO_TRAMPOLINE_JUMP)
		x86_push_imm (buf, 0);
	else
		x86_push_membase (buf, X86_ESP, (pushed_args + 1) * sizeof (gpointer));

	pushed_args++;
	x86_lea_membase (buf, X86_EAX, X86_ESP, (pushed_args - 8) * sizeof (gpointer));
	//
	//  EAX enthält den ersten Parameter der Methode mono_magic_trampoline()
	//
	x86_push_reg (buf, X86_EAX);

	pushed_args++;

	tramp = (guint8*) mono_get_trampoline_func(TrampTpe_p);
	//
	//  An dieser Stelle wird mono_magic_trampoline() aufgerufen
	//
	x86_call_code (buf, tramp);

	x86_alu_reg_imm (buf, X86_ADD, X86_ESP, 4*4);

	pushed_args -= 4;

	/* Check for thread interruption */
	/* This is not perf critical code so no need to check the interrupt flag */
	/* Align the stack on osx */
	x86_alu_reg_imm (buf, X86_SUB, X86_ESP, 3 * 4);
	x86_push_reg (buf, X86_EAX);
	x86_call_code (buf, (guint8*)mono_thread_force_interruption_checkpoint);
	x86_pop_reg (buf, X86_EAX);
	x86_alu_reg_imm (buf, X86_ADD, X86_ESP, 3 * 4);

	/* Restore LMF */

	/* ebx = previous_lmf */
	x86_pop_reg (buf, X86_EBX);
	pushed_args--;
	x86_alu_reg_imm (buf, X86_SUB, X86_EBX, 1);

	/* edi = lmf */
	x86_pop_reg (buf, X86_EDI);
	pushed_args--;

	/* *(lmf) = previous_lmf */
	x86_mov_membase_reg (buf, X86_EDI, 0, X86_EBX, 4);

	/* discard method info */
	x86_pop_reg (buf, X86_ESI);
	pushed_args--;

	/* discard ESP */
	x86_pop_reg (buf, X86_ESI);
	pushed_args--;

	/* restore caller saved regs */
	x86_pop_reg (buf, X86_EBX);
	x86_pop_reg (buf, X86_EDI);
	x86_pop_reg (buf, X86_ESI);
	x86_pop_reg (buf, X86_EBP);

	pushed_args -= 4;

	/* discard save IP */
	x86_alu_reg_imm (buf, X86_ADD, X86_ESP, 4);
	pushed_args--;

	/* restore LMF end */

	if (!MONO_TRAMPOLINE_TYPE_MUST_RETURN (TrampTpe_p)) {
		/*
		 * Overwrite the method ptr with the address we need to jump to,
		 * to free %eax.
		 */
		x86_mov_membase_reg (buf, X86_ESP, pushed_args * sizeof (gpointer), X86_EAX, 4);
	}

	/* Restore caller saved registers */
	x86_mov_reg_membase (buf, X86_ECX, X86_ESP, (pushed_args - pushed_args_caller_saved + X86_ECX) * 4, 4);
	x86_mov_reg_membase (buf, X86_EDX, X86_ESP, (pushed_args - pushed_args_caller_saved + X86_EDX) * 4, 4);

	if ((TrampTpe_p == MONO_TRAMPOLINE_RESTORE_STACK_PROT) ||
			(TrampTpe_p == MONO_TRAMPOLINE_AOT_PLT))
		x86_mov_reg_membase (buf, X86_EAX, X86_ESP, (pushed_args - pushed_args_caller_saved + X86_EAX) * 4, 4);

	if (!MONO_TRAMPOLINE_TYPE_MUST_RETURN (TrampTpe_p)) {
		/* Pop saved reg array + stack align */
		if (TrampTpe_p == MONO_TRAMPOLINE_JUMP) {
			x86_alu_reg_imm (buf, X86_ADD, X86_ESP, 10 * 4);
		} else {
			x86_alu_reg_imm (buf, X86_ADD, X86_ESP, 11 * 4);
		}

		pushed_args -= 9;
		g_assert(pushed_args == 0);
	} else {
		/* Pop saved reg array + stack align + method ptr */
		x86_alu_reg_imm (buf, X86_ADD, X86_ESP, 11 * 4);

		pushed_args -= 10;

	/* We've popped one more stack item than we've pushed (the
	 method ptr argument), so we must end up at -1. */
		g_assert(pushed_args == -1);
	}

	x86_ret (buf);

	g_assert((buf - code) <= 256);

	return code;
#elif defined(TARGET_ARM)
	return NULL;
#else
	return NULL;
#endif
}
