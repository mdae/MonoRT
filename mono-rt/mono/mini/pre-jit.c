/**
 * @file	pre-jit.c
 * @author	mdae
 * @date	2011/02/14
 *
 * Diese Datei enthält den Quellcode zum Ausfuehren
 * der Pre-JIT-Compilierung im Rahmen einer Initialisierungsroutine.
 */
#include "pre.h"
#include "jit.h"
/**
 * @brief	insertInPreJitExclusionList() fuellt die Liste der
 * von der Pre-JIT-Compilierung ausgeschlossenen Methoden und Wrapper.
 *
 * Waehrend des Pre-JIT wird vor jedem Aufruf von 'mono_compile_method()'
 * das zu compilierende Element in der globalen Variable
 * "pLastPreCompiledElement_g" gespeichert. Tritt waehrend der
 * Compilierung eine Exception auf, so springt der Pre-JIT-Code
 * in managed Code, der die Exception abfaengt, deren Beschreibung
 * in die Standardausgabe schreibt und das Pre-JIT neu startet.
 * Damit bei diesem Neu-Start der gleiche Fehler nicht wiederholt
 * auftritt, wird das Element, bei dessen Compilierung der
 * Fehler auftrat, vom Pre-JIT ausgeschlossen. Dazu ruft der
 * managed Code 'insertInPreJitExclusionList()' auf, um das
 * letzte zu compilierende Element in die Ausschlussliste
 * einzufuegen. Diese Ausschlussliste wird durch die global
 * sichtbare Hash-Tabelle 'ExcludedFromPreJit' repraesentiert.
 * Dieses Vorgehen arbeitet nur dann korrekt, wenn die Exception
 * waehrend der Ausfuehrung von 'mono_compile_method()' auftrat.
 * Exceptions, die in anderen Funktionen, etwa zur Erzeugung von
 * Wrappern, auftreten, werden damit nicht abgedeckt.
 */
void insertInPreJitExclusionList() {

	if (pLastPreCompiledElement_g != NULL) {
		g_hash_table_insert(ExcludedFromPreJit, pLastPreCompiledElement_g,
				pLastPreCompiledElement_g);
	} else {
		printf(
				"insertLastErrorInExclusionList() ohne korrektes Fehlerelement gerufen.\n");
		fflush(NULL);
	}
	pLastPreCompiledElement_g = NULL;
}
/*
 * @brief	preJitCompile() ist der Startpunkt der Pre-JIT-Compilierung
 * @param	assembly		Zeiger auf das zu pre-compilierende Assembly
 * @param	mainArgs		Zeiger auf die Argumente, mit denen die
 * 							aufgerufene Methode ausgefuehrt werden soll
 *
 * Diese Funktion ist der Startpunkt der Pre-JIT-Compilierung.
 * Deie Funktion "preJitCompile()" ausschlieszlich in der
 * Funktion "main_thread_handler()" (driver.c) aufgerufen.
 * Dabei wird und ihr ein Zeiger auf das Assembly uebergeben,
 * mit dem die Ausfuehrung des Programms beginnen soll.
 * Falls Pre-JIT durch den Nutzer aktiviert wurde, wird
 * eine Liste aller zu pre-compilierenden Assemblies erstellt.
 * Fuer jedes der erfassten Elemente wird eine Funktion
 * aufgerufen, die das Pre-JIT fuer das Assembly startet.
 */
void preJitCompile(MonoAssembly * assembly, MainThreadArgs * mainArgs) {

	struct MonoAssemblyList * assemblyListHeadPtr;

	if (MSRTAA) {
		//
		//  Das Pre-JIT wurde aktiviert.
		//  Waehrend der Pre-JIT-Compilierung
		//  werden die Trace-Ausgaben deaktiviert.
		//
		doTraceCmd_g = doTrace_g;
		doTraceLevel2Cmd_g = doTraceLevel2_g;
		doTrace_g = 0;
		doTraceLevel2_g = 0;
		//
		//  Es handelt sich um einen Aufruf dieser Funktion
		//  aus "main_thread_handler()" heraus. Es werden einige
		//  Initialisierungen fuer den Aufruf der Pre-JIT-Methode
		//  "preJitCompile()" vorgenommen.
		//
		//  Erstellung der Liste der zu pre-compilierenden Assemblies.
		//
		buildPreCompileList(assembly);
		//
		//  Ausgabe der zu pre-compilierenden Assemblies zu Debugging-Zwecken
		//
//		printPreCompileList();

		printf("Pre-compilation starts\n");
		fflush(NULL);
		//
		//  Pre-JIT-Compilierung der zu pre-compilierenden Assemblies
		//
		preCompileAssemblyList((void *) mainArgs);
		//
		//  Freigeben des durch die Hash-Tabelle benoetigten Speichers,
		//  da die Hast-Tabelle nicht mehr benoetigt wird.
		//
		assemblyListHeadPtr = (AssemblyList.head);
		while (assemblyListHeadPtr != (&AssemblyList))
		{
			if ((assemblyListHeadPtr->isPreCompiled == 0)
					&& (assemblyListHeadPtr->assemblyName != NULL)) {
				printf(
						"MonoRT - preJitCompile(): Assembly %s wurde nicht pre-compiliert.\n",
						assemblyListHeadPtr->assemblyName);
				fflush(NULL);
			}

			AssemblyList.head = assemblyListHeadPtr->prev;
			assemblyListHeadPtr = assemblyListHeadPtr->prev;
			free(assemblyListHeadPtr->next->assemblyName);
			assemblyListHeadPtr->next->assemblyName = NULL;
			assemblyListHeadPtr->next->head = NULL;
			assemblyListHeadPtr->next->prev = NULL;
			assemblyListHeadPtr->next->next = NULL;
			assemblyListHeadPtr->next->assembly = NULL;
			free(assemblyListHeadPtr->next);
			assemblyListHeadPtr->next = NULL;
		}
		if ((assemblyListHeadPtr->isPreCompiled == 0)
				&& (assemblyListHeadPtr->assemblyName != NULL)) {
			printf(
					"MonoRT - preJitCompile(): Assembly %s wurde nicht pre-compiliert.\n",
					assemblyListHeadPtr->assemblyName);
			fflush(NULL);
		}
		AssemblyList.head = NULL;
		AssemblyList.prev = NULL;
		AssemblyList.next = NULL;
		AssemblyList.assembly = NULL;
		free (AssemblyList.assemblyName);
		AssemblyList.assemblyName = NULL;

		printf("Pre-compilation finished\n\n");
		fflush(NULL);
	} else {
		//
		//  Das Pre-JIT wurde nicht aktiviert.
		//
		;
	}

	preCompileStackTraceMethod();
	//
	//  Nach der Pre-JIT-Compilierung
	//  werden die Trace-Ausgaben ggf. aktiviert.
	//
	doTrace_g = doTraceCmd_g;
	doTraceLevel2_g = doTraceLevel2Cmd_g;
	return;
}
/**
 * @brief preCompileStackTraceMethod() pre-compiliert die Methode
 * zur Ausgabe eines Stacktraces, da deren Ausfuehrung selbst
 * zum Anzeigen eines Stack-Traces fuehren kann und damit die
 * Lesbarkeit verschlechtert.
 */
void preCompileStackTraceMethod ()
{
	MonoMethodDesc * pMethDesc;
	MonoDomain * pAppDom;
	MonoImage * pImage;
	MonoMethod * pMethod;
	MonoAssembly * pAssembly;

	pMethDesc = NULL;
	pAppDom = NULL;
	pImage = NULL;
	pMethod = NULL;
	pAssembly = NULL;

	if (doPrePatchCmd_g != 0)
	{
		pMethDesc = mono_method_desc_new("MonitoringClass:printStackTrace", FALSE);

		if (pMethDesc != NULL)
		{
			pAppDom = mono_domain_get();
			if (pAppDom == NULL)
			{
				return;
			}

			pAssembly = mono_domain_assembly_open(pAppDom,"MonoRT.dll");
			if (pAssembly == NULL)
			{
				printf("\n\tMonoRT:\tCan not load MonoRT.dll\n");
				fflush(NULL);
				return;
			}

			pImage = mono_assembly_get_image(pAssembly);
			if (pImage == NULL)
			{
				printf(
						"\n\tMonoRT:\tCan not determine image from MonoRT.dll\n");
				fflush(NULL);
				return;
			}
			pMethod = mono_method_desc_search_in_image(pMethDesc, pImage);
			if (pMethod != NULL)
			{
				mono_compile_method(pMethod);
			}
			//
			//  Freigabe der angelegten Methodenbeschreibung
			//
			mono_method_desc_free(pMethDesc);
		}
	}
	return;
}
/**
 * @brief buildPreCompileList() fuellt eine globale Hash-Tabelle
 * mit den Assemblies, die waehrend des Pre-JIT compiliert werden.
 *
 * @param	assem			Zeiger auf das zu pre-compilierende Assembly
 *
 * Das auszufuehrende Assembly und alle potentiell referenzierten Assemblies
 * werden in eine globale Liste aufgenommen.
 */
void buildPreCompileList(MonoAssembly * assem) {

	MonoImage * image;
	int i;
	int equal;
	char * assemblyName;
	struct MonoAssemblyList * assemblyListElem;
	struct MonoAssemblyList * newAssemblyListElem;
	struct MonoAssemblyList * assemblyListHeadPtr;
	static int firstTimeCalled = 0;
	//
	// mono_assembly_get_image() in assembly.h
	//
	image = mono_assembly_get_image(assem);
	if (NULL == image) {
		printf(
				"Mono-RT - buildPreCompileList(): Konnte Image nicht bestimmen. Ende.\n");
		fflush(NULL);
		exit(1);
	}

	assemblyName = (char *) calloc(1, (size_t) (strlen(image->name) + 1));
	if (NULL == assemblyName) {
		printf(
				"Mono-RT - buildPreCompileList(): Konnte Speicher fuer Assembly-Name nicht reservieren. Ende.\n");
		fflush(NULL);
		exit(1);
	}
	//
	//  Ueberpruefung, ob das Assembly bereits in der Liste
	//  der zu pre-compilierenden Assemblies enthalten ist,
	//  oder ob es gar bereits pre-compiliert wurde.
	//
	strncpy(assemblyName, image->name, strlen(image->name));

	assemblyListElem = NULL;
	newAssemblyListElem = NULL;
	assemblyListHeadPtr = NULL;

	if (firstTimeCalled == 0) {
		firstTimeCalled = 1;
		AssemblyList.head = (&AssemblyList);
		AssemblyList.next = NULL;
		AssemblyList.prev = NULL;
		AssemblyList.assembly = assem;
		AssemblyList.assemblyName = assemblyName;
		AssemblyList.isPreCompiled = 0;
	} else {

		assemblyListElem = (&AssemblyList);

		while (assemblyListElem != NULL) {

			equal = 1;

			if ((assemblyListElem != NULL) && (assemblyName != NULL)) {
				equal = strcmp(assemblyListElem->assemblyName, assemblyName);
			}

			if (equal == 0) {
				//
				// Das Assembly befindet sich bereits in der Liste.
				//
				free(assemblyName);
				return;
			} else {
				assemblyListElem = assemblyListElem->next;
			}
		}
		//
		// Das Assembly ist noch nicht in der Liste enthalten.
		// Es wird ein neues Element der Liste zugefuegt.
		//
		newAssemblyListElem = (struct MonoAssemblyList *) malloc(
				sizeof(struct MonoAssemblyList));

		if (NULL == newAssemblyListElem) {
			printf(
					"Mono-RT - buildPreCompileList(): Konnte Speicher fuer Listenelement nicht allokieren.\n");
			fflush(NULL);
			exit(1);
		} else {
			assemblyListHeadPtr = AssemblyList.head;
			assemblyListHeadPtr->next = newAssemblyListElem;
			newAssemblyListElem->prev = assemblyListHeadPtr;
			AssemblyList.head = newAssemblyListElem;
			newAssemblyListElem->head = NULL;
			newAssemblyListElem->next = NULL;
			newAssemblyListElem->assembly = assem;
			newAssemblyListElem->assemblyName = assemblyName;
			newAssemblyListElem->isPreCompiled = 0;
		}
	}
	//
	//  Die Funktion wird fuer alle referenzierten Assemblies des
	//  betrachteten Assemblies rekursiv aufgerufen. Wurde eines
	//  der referenzierten Assemblies bereits erfasst, so wird die
	//  Rekursion aufgebrochen.
	//
	//  Der rekursive Aufruf erfolgt nur genau dann, wenn Mono nicht
	//  im Checkpoint-Modus ist. Im Checkpoint-Modus wird davon
	//  ausgegangen, dass bereits alle Assemblies im GAC compiliert
	//  wurden. Das Verfahren funktioniert nicht, wenn das Nutzer-Programm
	//  aus mehreren Assemblies besteht.
	//
	for (i = 0; i < mono_image_get_table_rows(image, MONO_TABLE_ASSEMBLYREF); i++) {

		mono_assembly_load_reference(image, i);

		if ((image->references[i]) != NULL) {
			buildPreCompileList(image->references[i]);
		}
	}
	return;
}
/**
 * @brief printPreCompileList() gibt die Liste der zu
 * pre-compilierenden Assemblies zu Debugging-Zwecken aus.

 * Es werden die Eintraege der Liste 'AssemblyList'
 * ausgegeben.
 */
void printPreCompileList() {

	MonoImage * image;
	int i;
	struct MonoAssemblyList * assemblyListElem;

	image = NULL;
	i = 0;
	assemblyListElem = (&AssemblyList);

	while (assemblyListElem != NULL) {

		image = mono_assembly_get_image(assemblyListElem->assembly);
		if (image == NULL) {
			printf(
					"Mono-RT - printPreCompileList ()\tKonnte Image nicht bestimmen. Ende.\n");
			fflush(NULL);
			exit(1);
		}

		printf("assembliesToPreCompile[%i]: %s\n", i, image->name);
		fflush(NULL);
		assemblyListElem = assemblyListElem->next;
		i++;
	}
	return;
}
/**
 * @brief preCompileAssemblyList() pre-compiliert die Assemblies
 * in der Liste der zu pre-compilierenden Assemblies.
 *
 * @param	mainArgs		Argumente aus dem Main-Thread Monos zur Bestimmung der AppDomain
 *
 * Es werden die Eintraege der Liste 'AssemblyList'pre-compiliert.
 * Dazu wird eine managed Methode aufgerufen um eventuell auftretende
 * Exceptions abfangen zu koennen und um das Pre-JIT fortsetzen zu koennen.
 */
void preCompileAssemblyList (void * pArgs_p)
{
	//
	//  Variablen zum Aufruf der managed Pre-JIT-Methode
	//
	MonoMethodDesc * pMethDesc;
	MonoDomain * pAppDom;
	MonoAssembly * pPreCompAssembly;
	MonoImage * pImage;
	MonoMethod * pMethod;
	void * argumentsForManagedMethod[1];
	MainThreadArgs * pArgs;
	//
	//  Variable zur Handhabung des zu pre-compilierenden Assemblys
	//
	MonoAssembly * pAssembly;
	struct MonoAssemblyList * pAssemblyElem;
	char * pcName;

	pMethDesc = NULL;
	pAppDom = NULL;
	pPreCompAssembly = NULL;
	pImage = NULL;
	pMethod = NULL;
	pArgs = (MainThreadArgs *) pArgs_p;
	pAssemblyElem = (&AssemblyList);
	pcName = NULL;

	while (pAssemblyElem != NULL)
	{
		pMethDesc = NULL;
		pAppDom = NULL;
		pPreCompAssembly = NULL;
		pImage = NULL;
		pMethod = NULL;
		pAssembly = (MonoAssembly *) pAssemblyElem->assembly;
		pcName = pAssemblyElem->assemblyName;

		if (pAssemblyElem->isPreCompiled != 0)
		{
			printf("MonoRT:\tAssembly %s already compiled.\n", pcName);
			fflush(NULL);
			pAssemblyElem = pAssemblyElem->next;
			continue;
		}
		//
		//  Erzeugen einer Methodenbeschreibung, mit der die managed
		//  Methode zum Pre-JIT geladen wird.
		//
		pMethDesc = mono_method_desc_new("PreJitClass:preJitCompileAssembly", FALSE);

		if (pMethDesc != NULL)
		{
			pAppDom = pArgs->domain;
			if (pAppDom == NULL)
			{
				printf("\n\tMonoRT:\tCan not determine application domain.\n");
				fflush(NULL);
				return;
			}
			//
			//  Das Assembly, welches Pre-JIT startet, wird in die gleiche
			//  Application-Domain geladen, mit der das eigentliche Programm-Assembly
			//  ausgefuehrt werden soll. Das Pre-JIT-Assembly muss in dem gleichen
			//  Verzeichnis wie das Programm-Assembly liegen und mit dem Datei-Namen
			//  "preJitPrePatchMain-rxxx.dll" benannt sein.
			//
			pPreCompAssembly = mono_domain_assembly_open(pAppDom, "MonoRT.dll");
			if (pPreCompAssembly == NULL)
			{
				printf("\n\tMonoRT:\tCan not load MonoRT.dll\n");
				fflush(NULL);
				return;
			}

			pImage = mono_assembly_get_image(pPreCompAssembly);
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
				//  Die managed Methode "PreJitClass:preJitCompileAssembly ()" konnte
				//  geladen werden. Sie wird nun aufgerufen und damit wird das
				//  Pre-JIT gestartet.
				//
				argumentsForManagedMethod[0] = &(pAssembly);
				mono_runtime_invoke(pMethod, NULL, argumentsForManagedMethod, NULL);
				//
				//  Das Assembly wurde pre-compiliert.
				//
				//  Speicherung der bereits pre-compilierten Assemblies,
				//  damit beim Pre-JIT des Nutzer-Assemblies referenzierte
				//  Assemblies nicht noch einmal pre-compiliert werden.
				//
				pAssemblyElem->isPreCompiled = 1;

			}
			//
			//  Freigabe der angelegten Methodenbeschreibung
			//
			mono_method_desc_free(pMethDesc);
		}
		pAssemblyElem = pAssemblyElem->next;
	}
	return;
}
/**
 * @brief preCompile() pre-compiliert ein Assembly.
 * @param	args				Datenstruktur, die das zu compilierende Assembly enthaelt
 * @param	reTryCompilation	Zeigt an, ob es sich um einen wiederholten Compilationsversuch handelt
 * @return	Gibt bei erfolgreicher Compilierung 1 zurueck.
 *
 * Diese Funktion pre-compiliert ein einziges Assembly. Es werden
 * die darin enthalten Methoden compiliert, sowie zahlreiche Wrapper
 * erzeugt und compiliert. Die Funktion ist so geschrieben, dass
 * sie nach einem Abbruch fortgesetzt werden kann.
 */
int preCompile(MonoAssembly * pAssemblyToPreCompile_p, int reTryCompilation)
{
	static int count = 0;
	static int fail_count = 0;
	static int ignore_count = 0;
	static MonoImage * pImage = NULL;
	static int normalMethodsOkay = 0;

	static int abstractCount = 0;
	static int icallCount = 0;
	static int runtimeCount = 0;
	static int pinvokeCount = 0;

	MonoAssembly * pAssembly;
	MonoMethod * pMethod;
	gboolean skip;
	int i;

	pAssembly = pAssemblyToPreCompile_p;

	if (reTryCompilation == 0)
	{
		//
		//  Es handelt sich um den ersten Versuch der
		//  Pre-Compilierung der Methoden eines Assemblys.
		//  Initialisiere die Checkpoint-Variable
		//  "normalMethodsOkay" mit 0.
		//
		pImage = mono_assembly_get_image(pAssembly);

		if (pImage == NULL) {
			printf("Konnte Image nicht bestimmen. Ende.\n");
			fflush(NULL);
			exit(1);
		}

		count = fail_count = 0;
		normalMethodsOkay = 0;

		printf("\tPre-compile\t%s\n", pImage->name);
		fflush(NULL);
	}

	if (normalMethodsOkay == 0)
	{
		//
		//  Beginn: Iteration ueber alle Methoden im betrachteten Assembly
		//
		for (i = 0, fail_count = 0, ignore_count = 0, icallCount = 0, abstractCount
				= 0, runtimeCount = 0, pinvokeCount = 0; i
				< pImage->tables[MONO_TABLE_METHOD].rows; ++i) {

			guint32 token;
			MonoMethodSignature *sig;

			skip = FALSE;
			token = MONO_TOKEN_METHOD_DEF | (i + 1);

			if (mono_metadata_has_generic_params(pImage, token))
			{
				if (SNOOPCAN) {
					pMethod = mono_get_method(pImage, token, NULL);
					printf("preCompile() skips %i token: %i\t%s\n",	fail_count, token, (pMethod ? mono_method_full_name(pMethod, TRUE) : "NULL"));
					fflush(NULL);
				}
				fail_count++;
				continue;
			}
			//
			// mono_get_method() in loader.h über mini.h
			//
			pMethod = mono_get_method(pImage, token, NULL);

			if (pMethod == NULL)
			{
				if (SNOOPCAN)
				{
					printf("preCompile() ueberspringt, da keine Methode fuer Token gefunden wurde: %i\n",(int) token);
					fflush(NULL);
				}
				fail_count++;
				continue;
			}
			//
			// mono_class_setup_methods() in class-internals.h ueber mini.h
			//
			// Initialisierung des Methoden-Feldes einer Klasse.
			// Erzeugung der MonoMethod-Strukturen der Methoden.
			//
			if (pMethod->klass != NULL)
			{
				mono_class_setup_methods(pMethod->klass);
			}
			else
			{
				printf("\tmethod->klass == NULL");
				fflush(NULL);
			}
			//
			//  Methoden mit dem Implementationsattribut "METHOD_IMPL_ATTRIBUTE_INTERNAL_CALL"
			//  sind eine Art PInvoke, die es ermoeglichen, eine C#-Methode als C-Funktion
			//  zu implementieren. Die Registrierung der ICalls erfolgt entweder in der
			//  Datei "icall-def.h" oder mittels der Funktion "mono_add_internal_call()".
			//  Im ersten Fall wird eine statische Tabelle aufgebaut, im zweiten Fall
			//  eine Hash-Tabelle. Die Icalls werden durch einen Wrapper aufgerufen,
			//  der in "mono_marshal_get_native_wrapper()" erzeugt wird. Diese Funktion
			//  wird u.a. in "mono_jit_compile_method_inner()" und "preCompileWrapper()"
			//  aufgerufen.
			//
			if ((pMethod->iflags & METHOD_IMPL_ATTRIBUTE_INTERNAL_CALL) != 0)
			{
				if (SNOOPCAN)
				{
					//
					// mono_method_full_name() in debug-helpers.h ueber mini.h
					//
					printf("\t(METHOD_IMPL_ATTRIBUTE_INTERNAL_CALL) %s\n",
							mono_method_full_name(pMethod, TRUE));
					fflush(NULL);
				}
				ignore_count++;
				icallCount++;
				continue;
			}
			//
			//  Methoden mit dem Implementationsattribut "METHOD_IMPL_ATTRIBUTE_RUNTIME" sind
			//  Methoden, die von der Runtime, d.h. Mono, selbst implementiert werden. Das
			//  betrifft nach derzeitigem Kenntnisstand Methoden, die mit Delegaten im
			//  Zusammenhang stehen:
			//
			//      Delegate-Konstruktor, -Invoke, -BeginInvoke, -EndInvoke
			//
			//  Die Methoden werden i.d.R. ueber einen Wrapper aufgerufen, der je
			//  nach Methode in "mono_jit_compile_method_inner()" erzeugt wird.
			//  Daher wird das Pre-JIT fuer diese Methoden fortgesetzt. In der
			//  Funktion "preCompileWrapper()" wird ein ein weiterer Wrapper
			//  mittels "mono_marshal_get_native_wrapper()" erzeugt. Der
			//  Delegate-Konstruktor "mono_delegate_ctor()" ist beispielsweise
			//  als ein Internal-JIT-Call realisiert, der ueber einen Icall-Wrapper
			//  ausgefuehrt wird. Dieser Icall-Wrapper wird in "mono_jit_compile_method_inner()"
			//  mit "mono_icall_get_wrapper_full()" erzeugt und waehrend des
			//  Pre-JIT anschlieszend  pre-compiliert. Weitere Runtime-Calls
			//  muessen nicht als Internal-JIT-Call realisiert sein. Die
			//  Internal-JIT-Calls werden mit "register_icall()" registriert.
			//  Ihre Wrapper werden in "preCompileJitIcallWrapper()" behandelt.
			//  Weitere Informationen zur Funktionsweise des Wrappers sind
			//  hier zu finden:
			//
			//    http://monoruntime.wordpress.com/tag/stack-unwind/
			//
			//  Im Wesentlichen sieht der IL-Code des Wrapper so aus:
			//
			//    CEE_LDARG 		// wiederholt, je nach Anzahl Argumente
			//    CEE_MONO_LDPTR	// Mono-spezifisch
			//    CEE_CALLI			// hier wird ein Aufruf zu "mono_get_native_calli_wrapper()" emittiert
			//    ...
			//
			if (pMethod->iflags & METHOD_IMPL_ATTRIBUTE_RUNTIME) {
				if (SNOOPCAN) {
					printf("\t(METHOD_IMPL_ATTRIBUTE_RUNTIME) %s\n",
							mono_method_full_name(pMethod, TRUE));
					fflush(NULL);
				}
				//				ignore_count++;
				runtimeCount++;
				//				continue;
			}
			//
			//  Methoden mit dem Implementationsattribut "METHOD_ATTRIBUTE_PINVOKE_IMPL"
			//  sind PInvokes, die durch einen Wrapper aufgerufen werden, der in
			//  "preCompileWrapper()" erzeugt und pre-compiliert wird. Siehe Methoden
			//  mit dem Attribut "METHOD_IMPL_ATTRIBUTE_INTERNAL_CALL".
			//
			if (pMethod->flags & METHOD_ATTRIBUTE_PINVOKE_IMPL) {
				if (SNOOPCAN) {
					printf("\t(METHOD_ATTRIBUTE_PINVOKE_IMPL) %s\n",
							mono_method_full_name(pMethod, TRUE));
					fflush(NULL);
				}
				ignore_count++;
				pinvokeCount++;
				continue;
			}
			//
			//  Methoden mit dem Implementationsattribut "METHOD_ATTRIBUTE_ABSTRACT"
			//  haben keinen Code, so dass sie nicht pre-compiliert werden koennen.
			//
			if (pMethod->flags & METHOD_ATTRIBUTE_ABSTRACT) {
				//
				//  Abstrakte Methoden werden nicht (pre-)compiliert.
				//
				if (SNOOPCAN) {
					//
					// mono_method_full_name() in debug-helpers.h ueber mini.h
					//
					//					printf("preCompile() ueberspringt abstrakte Methode %i: %s\n",fail_count, mono_method_full_name(method, TRUE));
					//					printf("\t(Abstrakte Methode) %s\n",mono_method_full_name(method, TRUE));
					printf("\t(METHOD_ATTRIBUTE_ABSTRACT) %s\n",
							mono_method_full_name(pMethod, TRUE));
					fflush(NULL);
				}
				ignore_count++;
				abstractCount++;
				continue;
			}
			if (pMethod->klass->generic_container) {
				if (SNOOPCAN) {
					//					printf("preCompile() ueberspringt %i (generic container): %s\n", fail_count, mono_method_full_name(method, TRUE));
					printf("\t(generic container) %s\n", mono_method_full_name(
							pMethod, TRUE));
					fflush(NULL);
				}
				fail_count++;
				continue;
			}
			//
			//  Bestimmung der Signatur der Methode, die compiliert werden soll.
			//  Es folgt die Untersuchung der Signatur.
			//
			sig = mono_method_signature(pMethod);
			if (!sig) {
				if (SNOOPCAN) {
					printf(
							"preCompile() could not retrieve method signature for %s (%i)\n",
							mono_method_full_name(pMethod, TRUE), fail_count);
					fflush(NULL);
				}
				fail_count++;
				continue;
			}
			//
			//  Falls die Methode generische Parameter bzw. einen
			//  generichen Rueckgabetyp hat, so wird sie nicht
			//  pre-compiliert.
			//
			if (sig->has_type_parameters) {
				if (SNOOPCAN) {
					printf("\t(type parameter): %s\n", mono_method_full_name(
							pMethod, TRUE));
					fflush(NULL);
				}
				fail_count++;
				continue;
			}

			count++;
			if (SNOOPCAP) {
				//				printf("\tpreCompile() compiliert (Type %s): %d %s\n", compType, count, mono_method_full_name(method, TRUE));
				printf("\t%s\n", mono_method_full_name(pMethod, TRUE));
				fflush(NULL);
			}
			checkedMonoCompileMethod(pMethod);
			//			if (SNOOPCAP) {
			//				printf("preCompile() compilierte fertig: %i\n", count);
			//				fflush(NULL);
			//			}
		}

		if (SNOOPCAN || SNOOPCAP) {
			printf("preCompile()\tAnzahl Iterationen Hauptschleife: %i\n", i);
			printf("preCompile()\tAnzahl ignorierter Methoden: %i\n",
					ignore_count);
			printf(
					"preCompile()\tAnzahl nicht pre-compilierbarer Methoden: %i\n",
					fail_count);
			printf("preCompile()\tAnzahl Abstrakter Methoden: %i\n",
					abstractCount);
			printf("preCompile()\tAnzahl ICall-Methoden: %i\n", icallCount);
			printf("preCompile()\tAnzahl Runtime Methoden: %i\n", runtimeCount);
			printf("preCompile()\tAnzahl PInvoke Methoden: %i\n", pinvokeCount);
		}

		normalMethodsOkay = 1;
		reTryCompilation = 0;
	} else {
		;
	}
	//
	//  Ende: Iteration ueber alle Methoden im auszufuehrenden Assembly
	//
	// Beginn: rekursive Pre-JIT-Compilierung alle referenzierten Assemblys sowie
	// zusaetzlicher Methoden (die im .NET-Profil 1.0 offenbar nicht benoetigt werden)
	//
	// Der Code zur Auswahl der zu compilierenden Methoden
	// entspricht dem Full-AOT-Code
	//
	if (MSRTAA) {

		if (SNOOPCAN || SNOOPCAP) {
			printf("preCompile() -> precompile_wrapper()\n");
			fflush(NULL);
		}

		preCompileWrapper(pAssembly, reTryCompilation);

		if (SNOOPCAN || SNOOPCAP) {
			printf("preCompile() <- preCompileWrapper()\n");
			fflush(NULL);
		}
	}

	if (SNOOPCAN || SNOOPCAP) {
		printf("leave preCompile() fuer %s\n", pImage->name);
		fflush(NULL);
	}
	//
	//  Durch die Rueckgabe des Wertes 1 wird der Funktion
	//  "preJitCompile()" signalisiert, dass die Pre-Compilierung
	//  des aktuellen Assemblys erfolgreich war.
	//
	return 1;
}
/*
 * @brief	preCompileWrapper() erzeugt und pre-compiliert diverse
 * Wrapper fuer die Methoden eines Assemblys.
 * @param	assembly			Zeiger auf das zu pre-compilierende Assembly
 * @param	reTryCompilation	Zeigt an, ob es sich um einen wiederholten Compilationsversuch handelt
 */
void preCompileWrapper(MonoAssembly *assembly, int reTryCompilation) {

	MonoMethod *method, *invoke, *m, *wrapper;
	int i, j;
	MonoMethodSignature *sig, *csig;
	guint32 token;
	gboolean skip;
	gpointer methodCompileRet;
	//
	//  Statische Variablen fuer das Checkpointing
	//  zur Beschleunigung eines Neu-Starts des
	//  Pre-JIT.
	//
	static int count = 0;
	static int normalWrappersOkay = 0;
	static int mscorlibWrappersOkay = 0;
	static int remotingInvokeWrappersOkay = 0;
	static int delegateInvokeWrappersOkay = 0;
	static int StructureToPtrWrappersOkay = 0;
	static int pinvokeWrappersOkay = 0;
	static int synchronizedWrappersOkay = 0;
	static int unboxWrappersOkay = 0;
	static int thunkInvokeWrappersOkay = 0;
	static int ignore_count = 0;
	static int fail_count = 0;
	static MonoImage *image = NULL;

	m = NULL;

	if (reTryCompilation == 0) {
		//
		//  Es handelt sich um den ersten Versuch der
		//  Pre-Compilierung der Wrapper eines Assemblys.
		//  Initialisiere die Checkpoint-Variablen mit 0.
		//
		image = mono_assembly_get_image(assembly);

		if (image == NULL) {
			printf(
					"Mono-RT - preCompileWrapper(): Konnte Image nicht bestimmen. Ende.\n");
			fflush(NULL);
			exit(1);
		}

		normalWrappersOkay = 0;
		mscorlibWrappersOkay = 0;
		remotingInvokeWrappersOkay = 0;
		delegateInvokeWrappersOkay = 0;
		StructureToPtrWrappersOkay = 0;
		pinvokeWrappersOkay = 0;
		synchronizedWrappersOkay = 0;
		unboxWrappersOkay = 0;
		thunkInvokeWrappersOkay = 0;

		//		printf("Pre-JIT:\t%s (Wrapper)\n", image->name);
		//		fflush(NULL);
	}

	if (normalWrappersOkay == 0) {
		//
		//  Generierung und Pre-Compilierung der Wrapper
		//  "Normaler" Methoden
		//
		for (i = 0, fail_count = 0, ignore_count = 0; i
				< image->tables[MONO_TABLE_METHOD].rows; ++i) {

			token = MONO_TOKEN_METHOD_DEF | (i + 1);
			skip = FALSE;
			sig = NULL;
			method = NULL;
			invoke = NULL;
			if (mono_metadata_has_generic_params(image, token)) {
				if (SNOOPCAN) {
					printf(
							"preCompileWrapper() ueberspringt token (generische Parameter): %i\n",
							token);
					fflush(NULL);
				}
				fail_count++;
				continue;
			}

			method = mono_get_method(image, token, NULL);

			if (method == NULL) {
				if (SNOOPCAN) {
					printf(
							"preCompileWrapper() konnte Methode nicht laden bei Token %i\n",
							token);
					fflush(NULL);
				}
				fail_count++;
				continue;
			}

			if (method->flags & METHOD_ATTRIBUTE_PINVOKE_IMPL) {
				ignore_count++;
				skip = TRUE;
				continue;
				/*				if (SNOOPCAN) {
				 printf("preCompileWrapper() ueberspringt (PInvoke) %s\n",mono_method_full_name(method, TRUE));
				 printf("\t(PInvoke Methode) %s\n",mono_method_full_name(method, TRUE));
				 fflush(NULL);
				 }
				 */}
			if (method->iflags & METHOD_IMPL_ATTRIBUTE_RUNTIME) {
				ignore_count++;
				skip = TRUE;
				continue;
				/*				if (SNOOPCAN) {
				 printf("preCompileWrapper() ueberspringt (Runtime) %s\n", mono_method_full_name(method, TRUE));
				 printf("\t(Runtime Methode) %s\n", mono_method_full_name(method, TRUE));
				 fflush(NULL);
				 }
				 */}
			if (method->flags & METHOD_ATTRIBUTE_ABSTRACT) {
				ignore_count++;
				skip = TRUE;
				continue;
				/*				if (SNOOPCAN) {
				 printf("preCompileWrapper() ueberspringt (Abstract) %s\n", mono_method_full_name(method, TRUE));
				 printf("\t(Abstrakte Methode) %s\n", mono_method_full_name(method, TRUE));
				 fflush(NULL);
				 }
				 */}
			if (method->is_generic) {
				fail_count++;
				skip = TRUE;
				continue;
				if (SNOOPCAN) {
					printf("\t(Generic Method) %s\n", mono_method_full_name(
							method, TRUE));
					fflush(NULL);
				}
			}
			if (method->klass->generic_container) {
				fail_count++;
				skip = TRUE;
				continue;
				if (SNOOPCAN) {
					printf("\t(Class Generic Container) %s\n",
							mono_method_full_name(method, TRUE));
					fflush(NULL);
				}
			}
			/* Skip methods which can not be handled by get_runtime_invoke () */
			sig = mono_method_signature(method);

			if (sig == NULL) {
				if (SNOOPCAN) {
					printf("\t(keine Signatur) %s\n", mono_method_full_name(
							method, TRUE));
					fflush(NULL);
				}
				fail_count++;
				skip = TRUE;
				continue;
			}
			if ((sig->ret->type == MONO_TYPE_PTR) || (sig->ret->type
					== MONO_TYPE_TYPEDBYREF)) {
				skip = TRUE;
				if (SNOOPCAN) {
					printf("\t(Rueckgabetyp) %s\n", mono_method_full_name(
							method, TRUE));
					fflush(NULL);
				}
				fail_count++;
				continue;
			}
			for (j = 0; j < sig->param_count; j++) {
				if (sig->params[j]->type == MONO_TYPE_TYPEDBYREF) {
					skip = TRUE;
					if (SNOOPCAN) {
						//						printf("preCompileWrapper() ueberspringt (Parametertyp Typebyref) %s\n", mono_method_full_name(method, TRUE));
						printf("\t(Parametertyp Typebyref) %s\n",
								mono_method_full_name(method, TRUE));
						fflush(NULL);
					}
					fail_count++;
					continue;
				}
			}
			/*
			 #ifdef MONO_ARCH_DYN_CALL_SUPPORTED
			 if (!method->klass->contextbound) {
			 MonoDynCallInfo *info = mono_arch_dyn_call_prepare (sig);

			 if (info) {
			 // Supported by the dynamic runtime-invoke wrapper
			 skip = TRUE;
			 if (SNOOPCAN) {
			 printf("\t(Contextbound) %s\n", mono_method_full_name(method, TRUE));
			 fflush(NULL);
			 }
			 fail_count++;
			 continue;
			 g_free (info);
			 }
			 }
			 #endif
			 */
			if (skip == FALSE) {

				invoke = NULL;
				invoke = mono_marshal_get_runtime_invoke(method, FALSE);

				if (invoke == NULL) {
					printf("\tFehler mono_marshal_get_runtime_invoke() %s\n",
							mono_method_full_name(method, TRUE));
					fflush(NULL);
					exit(1);
				}

				if (SNOOPCAP) {
					printf("\t%s fuer %s\n",
							mono_method_full_name(invoke, TRUE),
							mono_method_full_name(method, TRUE));
					fflush(NULL);
				}
				methodCompileRet = checkedMonoCompileMethod(invoke);
				count++;
			}
		}

		if (SNOOPCAN || SNOOPCAP) {
			printf(
					"preCompileWrapper() Runtime-Invoke Wrapper\tAnzahl Iterationen Hauptschleife: %i\n",
					i);
			printf(
					"preCompileWrapper() Runtime-Invoke Wrapper\tAnzahl ignorierter Methoden: %i\n",
					ignore_count);
			printf(
					"preCompileWrapper() Runtime-Invoke Wrapper\tAnzahl nicht pre-compilierbarer Methoden: %i\n",
					fail_count);
			printf("preCompileWrapper(): Normale Methoden fertig\n");
			printf(
					"preCompileWrapper(): Beginne mit speziellen mscorlib-Methoden\n");
			fflush(NULL);
		}
		normalWrappersOkay = 1;
	} else {
		;
	}

	if (mscorlibWrappersOkay == 0) {
		//
		//  Erzeugung spezieller Wrapper der mscorlib.dll
		//
		fail_count = 0;
		ignore_count = 0;
		count = 0;

		if (strcmp(assembly->aname.name, "mscorlib") == 0) {
#ifdef MONO_ARCH_HAVE_TLS_GET
			MonoMethodDesc *desc;
			MonoMethod *orig_method;
			int nallocators;
#endif
			//
			//  Runtime-Invoke-Wrapper fuer Klassenkonstruktoren
			//
			//  void runtime-invoke () [.cctor]
			//
			/*
			 * @todo	Freigabe von csig
			 */
			csig = mono_metadata_signature_alloc(mono_defaults.corlib, 0);
			csig->ret = &mono_defaults.void_class->byval_arg;
			invoke = precompile_get_runtime_invoke_sig(csig);
			if (invoke != NULL) {
				if (SNOOPCAP) {
					printf("\tvoid runtime-invoke () [.cctor]: %s\n",
							mono_method_full_name(invoke, TRUE));
					fflush(NULL);
				}
				checkedMonoCompileMethod(invoke);
				count++;
			} else {
				if (SNOOPCAN) {
					printf("\tFehler void runtime-invoke () [.cctor]\n");
					fflush(NULL);
				}
				fail_count++;
			}
			//
			//  Runtime-Invoke-Wrapper fuer Finalizer?
			//  Der Wrapper heiszt "System.Object:runtime_invoke_void_this()"
			//
			//  void runtime-invoke () [Finalize]
			//
			csig = mono_metadata_signature_alloc(mono_defaults.corlib, 0);
			csig->hasthis = 1;
			csig->ret = &mono_defaults.void_class->byval_arg;
			invoke = precompile_get_runtime_invoke_sig(csig);
			if (invoke != NULL) {
				if (SNOOPCAP) {
					printf("\tvoid runtime-invoke () [Finalize]: %s\n",
							mono_method_full_name(invoke, TRUE));
					fflush(NULL);
				}
				checkedMonoCompileMethod(invoke);
				count++;
			} else {
				if (SNOOPCAN) {
					printf("\tFehler void runtime-invoke () [Finalize]\n");
					fflush(NULL);
				}
				fail_count++;
			}
			//
			//  Runtime-Invoke-Wrapper fuer Exception-Konstruktoren
			//
			//  void runtime-invoke (string) [exception ctor]
			//
			csig = mono_metadata_signature_alloc(mono_defaults.corlib, 1);
			csig->hasthis = 1;
			csig->ret = &mono_defaults.void_class->byval_arg;
			csig->params[0] = &mono_defaults.string_class->byval_arg;
			invoke = precompile_get_runtime_invoke_sig(csig);
			if (invoke != NULL) {
				if (SNOOPCAP) {
					printf(
							"\tvoid runtime-invoke (string) [exception ctor]: %s\n",
							mono_method_full_name(invoke, TRUE));
					fflush(NULL);
				}
				checkedMonoCompileMethod(invoke);
				count++;
			} else {
				if (SNOOPCAN) {
					printf(
							"\tFehler void runtime-invoke (string) [exception ctor]\n");
					fflush(NULL);
				}
				fail_count++;
			}
			//
			//  Runtime-Invoke-Wrapper fuer Exception-Konstruktoren
			//
			//  void runtime-invoke (string, string) [exception ctor]
			//
			csig = mono_metadata_signature_alloc(mono_defaults.corlib, 2);
			csig->hasthis = 1;
			csig->ret = &mono_defaults.void_class->byval_arg;
			csig->params[0] = &mono_defaults.string_class->byval_arg;
			csig->params[1] = &mono_defaults.string_class->byval_arg;
			invoke = precompile_get_runtime_invoke_sig(csig);
			if (invoke != NULL) {
				if (SNOOPCAP) {
					printf(
							"\tvoid runtime-invoke (string, string) [exception ctor]: %s\n",
							mono_method_full_name(invoke, TRUE));
					fflush(NULL);
				}
				checkedMonoCompileMethod(invoke);
				count++;
			} else {
				if (SNOOPCAN) {
					printf(
							"\tFehler void runtime-invoke (string, string) [exception ctor]\n");
					fflush(NULL);
				}
				fail_count++;
			}
			//
			//  Runtime-Invoke-Wrapper fuer Exception.ToString-Methoden
			//
			//  string runtime-invoke () [Exception.ToString ()]
			//
			csig = mono_metadata_signature_alloc(mono_defaults.corlib, 0);
			csig->hasthis = 1;
			csig->ret = &mono_defaults.string_class->byval_arg;
			invoke = NULL;
			invoke = precompile_get_runtime_invoke_sig(csig);
			if (invoke != NULL) {
				if (SNOOPCAP) {
					printf(
							"\tstring runtime-invoke () [Exception.ToString ()]: %s\n",
							mono_method_full_name(invoke, TRUE));
					fflush(NULL);
				}
				checkedMonoCompileMethod(invoke);
				count++;
			} else {
				if (SNOOPCAN) {
					printf(
							"\tFehler string runtime-invoke () [Exception.ToString ()]\n");
					fflush(NULL);
				}
				fail_count++;
			}
			//
			//  Runtime-Invoke-Wrapper fuer Exception-Konstruktoren
			//
			//  void runtime-invoke (string, Exception) [exception ctor]
			//
			csig = mono_metadata_signature_alloc(mono_defaults.corlib, 2);
			csig->hasthis = 1;
			csig->ret = &mono_defaults.void_class->byval_arg;
			csig->params[0] = &mono_defaults.string_class->byval_arg;
			csig->params[1] = &mono_defaults.exception_class->byval_arg;
			invoke = precompile_get_runtime_invoke_sig(csig);
			if (invoke != NULL) {
				if (SNOOPCAP) {
					printf(
							"\tvoid runtime-invoke (string, Exception) [exception ctor]: %s\n",
							mono_method_full_name(invoke, TRUE));
					fflush(NULL);
				}
				checkedMonoCompileMethod(invoke);
				count++;
			} else {
				if (SNOOPCAN) {
					printf(
							"\tFehler void runtime-invoke (string, Exception) [exception ctor]\n");
					fflush(NULL);
				}
				fail_count++;
			}
			//
			//  Runtime-Invoke-Wrapper fuer die Assembly-Resolve-Methode?
			//
			//  Assembly runtime-invoke (string, bool) [DoAssemblyResolve]
			//
			csig = mono_metadata_signature_alloc(mono_defaults.corlib, 2);
			csig->hasthis = 1;
			csig->ret = &(mono_class_from_name(mono_defaults.corlib,
					"System.Reflection", "Assembly"))->byval_arg;
			csig->params[0] = &mono_defaults.string_class->byval_arg;
			csig->params[1] = &mono_defaults.boolean_class->byval_arg;
			invoke = NULL;
			invoke = precompile_get_runtime_invoke_sig(csig);
			if (invoke != NULL) {
				if (SNOOPCAP) {
					printf(
							"\tAssembly runtime-invoke (string, bool) [DoAssemblyResolve]: %s\n",
							mono_method_full_name(invoke, TRUE));
					fflush(NULL);
				}
				checkedMonoCompileMethod(invoke);
				count++;
			} else {
				if (SNOOPCAN) {
					printf(
							"\tFehler Assembly runtime-invoke (string, bool) [DoAssemblyResolve]\n");
					fflush(NULL);
				}
				fail_count++;
			}
			//
			//  Runtime-Invoke-Wrapper fuer Finalizer
			//  Anderns als bei den oben erzeugten Wrappers wird hier
			//  eine virtuelle Methode aufgerufen. Der Wrapper heiszt
			//  "System.Object:runtime_invoke_virtual_void_this()"
			//
			/* runtime-invoke used by finalizers */
			invoke
					= mono_marshal_get_runtime_invoke(
							mono_class_get_method_from_name_flags(
									mono_defaults.object_class, "Finalize", 0,
									0), TRUE);
			if (invoke != NULL) {
				if (SNOOPCAP) {
					printf("\truntime-invoke used by finalizers: %s\n",
							mono_method_full_name(invoke, TRUE));
					fflush(NULL);
				}
				checkedMonoCompileMethod(invoke);
			} else {
				if (SNOOPCAN) {
					printf("\tFehler runtime-invoke used by finalizers\n");
					fflush(NULL);
				}
				fail_count++;
			}
			//
			//  Die Methode "mono_get_context_capture_method()" liefert
			//  fuer mscorlib.dll-Versionen <2 und _nicht_ aktiviertem
			//  Security Manager (Kommandozeilenparameter "--security")
			//  NULL zurueck.
			//
			/* This is used by mono_runtime_capture_context () */
			method = NULL;
			method = mono_get_context_capture_method();
			if (method != NULL) {
				invoke = mono_marshal_get_runtime_invoke(method, FALSE);
				if (invoke != NULL) {
					/*
					 if (strcmp(invoke->name, "runtime_invoke_void__this___intptr") == 0) {
					 printf("Wrapper runtime_invoke_void__this___intptr gefunden\n");
					 fflush(NULL);
					 }
					 */
					if (SNOOPCAP) {
						printf(
								"\tWrapper fuer mono_get_context_capture_method(): %s fuer %s\n",
								mono_method_full_name(invoke, TRUE),
								mono_method_full_name(method, TRUE));
						fflush(NULL);
					}
					checkedMonoCompileMethod(invoke);
					count++;
				} else {
					if (SNOOPCAN) {
						printf(
								"\tFehler bei mono_runtime_capture_context () - invoke\n");
						fflush(NULL);
					}
				}
				fail_count++;
			} else {
				if (SNOOPCAN) {
					printf(
							"\tFehler bei mono_runtime_capture_context () - method\n");
					fflush(NULL);
				}
				fail_count++;
			}

#ifdef MONO_ARCH_DYN_CALL_SUPPORTED
			invoke = mono_marshal_get_runtime_invoke_dynamic ();
			if (invoke != NULL) {
				if (SNOOPCAP) {
					printf("\tmono_marshal_get_runtime_invoke_dynamic(): %s\n", mono_method_full_name(invoke, TRUE));
					fflush(NULL);
				}
				checkedMonoCompileMethod (invoke);
				count++;
			} else {
				if (SNOOPCAN) {
					printf("\tFehler bei mono_marshal_get_runtime_invoke_dynamic ()\n");
					fflush(NULL);
				}
				fail_count++;
			}
#endif
			//
			//  Erzeugung der Wrapper fuer Internal-JIT-Calls. Diese werden
			//  u.a. in der Hash-Tabelle "jit_icall_hash_name" gespeichert.
			//  Die Speicherung erfolgt bei der Registrierung des Internal-JIT-Calls
			//  via "register_icall()". Fuer alle in dieser Hash-Tabelle
			//  gespeicherten Internal-JIT-Calls werden die entsprechenden
			//  Wrapper erzeugt.
			//
			g_hash_table_foreach(mono_get_jit_icall_info(),
					preCompileJitIcallWrapper, assembly);

			/* stelemref */
			invoke = mono_marshal_get_stelemref();
			if (invoke != NULL) {
				if (SNOOPCAP) {
					printf("\tstelemref: %s\n", mono_method_full_name(invoke,
							TRUE));
					fflush(NULL);
				}
				checkedMonoCompileMethod(invoke);
				count++;
			} else {
				if (SNOOPCAN) {
					printf("\tFehler stelemref\n");
					fflush(NULL);
				}
				fail_count++;
			}

#ifdef MONO_ARCH_HAVE_TLS_GET
			/* Managed Allocators */
			nallocators = mono_gc_get_managed_allocator_types();
			for (i = 0; i < nallocators; ++i) {
				m = NULL;
				m = mono_gc_get_managed_allocator_by_type(i);
				if (m != NULL) {
					if (SNOOPCAP) {
						printf("\tManaged Allocators: %s\n",
								mono_method_full_name(m, TRUE));
						fflush(NULL);
					}
					checkedMonoCompileMethod(m);
					count++;
				} else {
					if (SNOOPCAN) {
						printf("\tFehler managed Allocators\n");
						fflush(NULL);
					}
					fail_count++;
				}
			}
			/* Monitor Enter/Exit */

			desc = mono_method_desc_new("Monitor:Enter", FALSE);
			orig_method = mono_method_desc_search_in_class(desc,
					mono_defaults.monitor_class);
			g_assert(orig_method);
			mono_method_desc_free(desc);
			method = mono_monitor_get_fast_path(orig_method);
			if (method != 0) {
				if (SNOOPCAP) {
					printf("\tMonitor Enter: %s\n", mono_method_full_name(
							method, TRUE));
					fflush(NULL);
				}
				checkedMonoCompileMethod(method);
				count++;
			} else {
				if (SNOOPCAN) {
					printf("\tFehler Monitor Enter\n");
					fflush(NULL);
				}
				fail_count++;
			}
			desc = mono_method_desc_new("Monitor:Exit", FALSE);
			orig_method = mono_method_desc_search_in_class(desc,
					mono_defaults.monitor_class);
			g_assert(orig_method);
			mono_method_desc_free(desc);
			method = mono_monitor_get_fast_path(orig_method);
			if (method) {
				if (SNOOPCAP) {
					printf("\tMonitor Exit: %s\n", mono_method_full_name(
							method, TRUE));
					fflush(NULL);
				}
				checkedMonoCompileMethod(method);
				count++;
			} else {
				if (SNOOPCAN) {
					printf("\tFehler Monitor Exit\n");
					fflush(NULL);
				}
				fail_count++;
			}

#endif
		}

		if (SNOOPCAN || SNOOPCAP) {
			printf(
					"preCompileWrapper() mscorlib.dll Wrapper\tAnzahl erzeugter Wrapper: %i\n",
					count);
			printf(
					"preCompileWrapper() mscorlib.dll Wrapper\tAnzahl nicht erzeugter Wrapper %i\n",
					fail_count);
			printf("preCompileWrapper(): mscorlib fertig\n");
			printf("preCompileWrapper(): Beginne  Remoting INvoke Wrapper\n");
			fflush(NULL);
		}
		mscorlibWrappersOkay = 1;
	} else {
		;
	}

	if (remotingInvokeWrappersOkay == 0) {
		//
		// remoting-invoke wrappers
		//
		MonoMethodSignature * sig;
		MonoMethodSignature * wrapperSig;
		MonoType * retType;
		MonoMethod * cominteropWrapper;
		MonoMethod * wrapperWrapper;

		for (i = 0, fail_count = 0, ignore_count = 0; i
				< image->tables[MONO_TABLE_METHOD].rows; ++i) {

			token = MONO_TOKEN_METHOD_DEF | (i + 1);

			method = NULL;
			method = mono_get_method(image, token, NULL);

			if (method == NULL) {
				fail_count++;
				continue;
			}

			sig = NULL;
			sig = mono_method_signature(method);

			if (sig == NULL) {
				if (SNOOPCAN) {
					printf("\tKeine Signatur fuer %s\n", mono_method_full_name(
							method, TRUE));
					fflush(NULL);
				}
				fail_count++;
				continue;
			}

			if ((method != NULL) && (sig->hasthis)) {
				//
				//  Erzeugung eines Wrappers vom Typ "MONO_WRAPPER_COMINTEROP"
				//  Die Funktion "mono_cominterop_get_invoke(MonoMethod *)" wird
				//  bspw. auch im Magic Trampoline oder bei der Behandlung des
				//  IL-Opcode "LDVIRTFTN" aufgerufen.
				//
				if (method->is_generic == 1) {
					printf("\n\tGeneric Method %s\n", mono_method_full_name(method, TRUE));
					fflush(NULL);
				}

				cominteropWrapper = mono_cominterop_get_invoke(method);

				if (cominteropWrapper != NULL) {
					if (SNOOPCAP) {
						printf("\tCominerop Invoke(): %s fuer %s\n",
								mono_method_full_name(cominteropWrapper, TRUE),
								mono_method_full_name(method, TRUE));
						fflush(NULL);
					}
					checkedMonoCompileMethod(cominteropWrapper);
					count++;
					wrapperSig = mono_method_signature(cominteropWrapper);
					retType = mono_type_get_underlying_type(wrapperSig->ret);
					//
					//  Ueberpruefung des Rueckgabetypes des erzeugten Wrappers.
					//  Bei einem positivem Ergebnis wird ein weiterer Wrapper
					//  um den eben erzeugten Wrapper generiert. Dieser ist bspw.
					//  fuer den Aufruf von COM-Funktionen per Delegate notwendig.
					//
					//  Die hier angewendete Filterung ist keine gute Loesung und
					//  basiert auf einer Assertion in der Funktion "mono_mb_emit_restore_result()"
					//  in marshal.c, Z. 2650.
					//
					switch (retType->type) {
					case MONO_TYPE_VOID:
					case MONO_TYPE_PTR:
					case MONO_TYPE_STRING:
					case MONO_TYPE_CLASS:
					case MONO_TYPE_OBJECT:
					case MONO_TYPE_ARRAY:
					case MONO_TYPE_SZARRAY:
					case MONO_TYPE_U1:
					case MONO_TYPE_BOOLEAN:
					case MONO_TYPE_I1:
					case MONO_TYPE_U2:
					case MONO_TYPE_CHAR:
					case MONO_TYPE_I2:
					case MONO_TYPE_I:
					case MONO_TYPE_U:
					case MONO_TYPE_I4:
					case MONO_TYPE_U4:
					case MONO_TYPE_U8:
					case MONO_TYPE_I8:
					case MONO_TYPE_R4:
					case MONO_TYPE_R8:
					case MONO_TYPE_GENERICINST:
					case MONO_TYPE_VALUETYPE: {
						wrapperWrapper = mono_marshal_get_remoting_invoke(
								cominteropWrapper);
						if (wrapper) {
							if (SNOOPCAP) {
								printf("\tRemoting Invoke(): %s fuer %s\n",
										mono_method_full_name(wrapperWrapper,
												TRUE), mono_method_full_name(
												cominteropWrapper, TRUE));
								fflush(NULL);
							}
							checkedMonoCompileMethod(wrapperWrapper);
							count++;
						} else {
							if (SNOOPCAN) {
								printf(
										"\tFehler mono_marshal_get_remoting_invoke() fuer %s",
										mono_method_full_name(
												cominteropWrapper, TRUE));
								fflush(NULL);
							}
							fail_count++;
						}
						break;
					}
					default:
						if (SNOOPCAN) {
							printf(
									"\tFehler mono_marshal_get_remoting_invoke() fuer %s",
									mono_method_full_name(cominteropWrapper,
											TRUE));
							fflush(NULL);
						}
						fail_count++;
					}
					/*
					 if ((wrapperSig->hasthis) && (m->klass->marshalbyref || m->klass == mono_defaults.object_class)) {
					 wrapper = mono_marshal_get_remoting_invoke(m);

					 if (wrapper) {
					 if (SNOOPCAP) {
					 printf("\tRemoting Invoke(): %s fuer %s\n",	mono_method_full_name(wrapper, TRUE),	mono_method_full_name(m, TRUE));
					 fflush(NULL);
					 }
					 checkedMonoCompileMethod(wrapper);
					 count++;
					 } else {
					 if (SNOOPCAN) {
					 printf("\tFehler mono_marshal_get_remoting_invoke() fuer %s", mono_method_full_name(m, TRUE));
					 fflush(NULL);
					 }
					 fail_count++;
					 }
					 }
					 */
				} else {
					if (SNOOPCAN) {
						printf(
								"\tFehler mono_cominterop_get_invoke () fuer %s",
								mono_method_full_name(method, TRUE));
						fflush(NULL);
					}
					fail_count++;
				}
			}

			if ((method != NULL) && (sig->hasthis)
					&& (method->klass->marshalbyref || method->klass
							== mono_defaults.object_class)) {

				m = mono_marshal_get_remoting_invoke_with_check(method);
				if (m) {
					if (SNOOPCAP) {
						printf("\tRemoting Invoke with check: %s fuer %s\n",
								mono_method_full_name(m, TRUE),
								mono_method_full_name(method, TRUE));
						fflush(NULL);
					}
					checkedMonoCompileMethod(m);
					count++;
				} else {
					if (SNOOPCAN) {
						printf(
								"\tFehler mono_marshal_get_remoting_invoke_with_check() fuer %s",
								mono_method_full_name(method, TRUE));
						fflush(NULL);
					}
					fail_count++;
				}

				m = mono_marshal_get_remoting_invoke(method);
				if (m != NULL) {
					if (SNOOPCAP) {
						printf("\tRemoting Invoke(): %s fuer %s\n",
								mono_method_full_name(m, TRUE),
								mono_method_full_name(method, TRUE));
						fflush(NULL);
					}
					checkedMonoCompileMethod(m);
					count++;
				} else {
					if (SNOOPCAN) {
						printf(
								"\tFehler mono_marshal_get_remoting_invoke() fuer %s",
								mono_method_full_name(method, TRUE));
						fflush(NULL);
					}
					fail_count++;
				}
			}
			//
			//  Um zu verhindern, dass bei einem expliziten Cast eines
			//  Proxy-Objektes in ein Interface ein Remoting-Invoke-Wrapper
			//  erzeugt werden muss, wird dies hier getan.
			//
			if (method->klass->flags & TYPE_ATTRIBUTE_INTERFACE) {
				m = mono_marshal_get_remoting_invoke(method);
				if (m != NULL) {
					if (SNOOPCAP) {
						printf("\tRemoting Invoke(): %s fuer %s\n",
								mono_method_full_name(m, TRUE),
								mono_method_full_name(method, TRUE));
						fflush(NULL);
					}
					checkedMonoCompileMethod(m);
					count++;
				} else {
					if (SNOOPCAN) {
						printf(
								"\tFehler mono_marshal_get_remoting_invoke() fuer %s",
								mono_method_full_name(method, TRUE));
						fflush(NULL);
					}
					fail_count++;
				}
			}
		}

		if (SNOOPCAN || SNOOPCAP) {
			printf(
					"preCompileWrapper() Remoting Wrapper\tAnzahl Iterationen Hauptschleife: %i\n",
					i);
			printf(
					"preCompileWrapper() Remoting Wrapper\tAnzahl ignorierter Methoden: %i\n",
					ignore_count);
			printf(
					"preCompileWrapper() Remoting Wrapper\tAnzahl nicht pre-compilierbarer Methoden: %i\n",
					fail_count);
			printf("preCompileWrapper(): remoting-invoke wrappers fertig\n");
			printf("preCompileWrapper(): Beginne Delegate Invoke Wrapper\n");
			fflush(NULL);
		}
		remotingInvokeWrappersOkay = 1;
	} else {
		;
	}

	if (delegateInvokeWrappersOkay == 0) {
		/* delegate-invoke wrappers */
		for (i = 0, fail_count = 0, ignore_count = 0; i
				< image->tables[MONO_TABLE_TYPEDEF].rows; ++i) {
			MonoClass *klass;

			token = MONO_TOKEN_TYPE_DEF | (i + 1);
			klass = mono_class_get(image, token);

			if (klass->delegate && klass != mono_defaults.delegate_class
					&& klass != mono_defaults.multicastdelegate_class
					&& !klass->generic_container) {

				method = mono_get_delegate_invoke(klass);
				if (method != NULL) {
					//
					// Erzeuge Runtime-Invoke-Wrapper fuer Delegate.Invoke-Methode.
					// Damit sollen asynchrone Aufrufe der Delegaten abgedeckt werden.
					//
					checkedMonoCompileMethod(mono_marshal_get_runtime_invoke(
							method, FALSE));
					//
					// Erzeuge einen Wrapper, der alle Methoden in einem Multicast-Delegate
					// aufruft.
					//
					m = mono_marshal_get_delegate_invoke(method, NULL);
				} else {
					if (SNOOPCAN) {
						printf(
								"\tFehler mono_get_delegate_invoke() fuer Klasse %s\n",
								klass->name);
						fflush(NULL);
					}
					fail_count++;
				}
				if (m != NULL) {
					if (SNOOPCAP) {
						printf("\t%s fuer %s\n",
								mono_method_full_name(m, TRUE),
								mono_method_full_name(method, TRUE));
						fflush(NULL);
					}
					checkedMonoCompileMethod(m);
					count++;
				} else {
					if (SNOOPCAN) {
						printf(
								"\tFehler mono_marshal_get_delegate_invoke() fuer %s\n",
								mono_method_full_name(method, TRUE));
						fflush(NULL);
					}
					fail_count++;
				}
				method = NULL;
				method = mono_class_get_method_from_name_flags(klass,
						"BeginInvoke", -1, 0);
				invoke = NULL;
				if (method != NULL) {
					invoke = mono_marshal_get_delegate_begin_invoke(method);
				} else {
					if (SNOOPCAN) {
						printf(
								"\tFehler BeginInvoke() fuer Klasse %s - method\n",
								klass->name);
						fflush(NULL);
					}
					fail_count++;
				}
				if (invoke != NULL) {
					if (SNOOPCAP) {
						printf("\t%s \n", mono_method_full_name(invoke, TRUE));
						fflush(NULL);
					}
					checkedMonoCompileMethod(invoke);
					count++;
				} else {
					if (SNOOPCAN) {
						printf(
								"\tFehler BeginInvoke() fuer Klasse %s - invoke\n",
								klass->name);
						fflush(NULL);
					}
					fail_count++;
				}

				method = NULL;
				method = mono_class_get_method_from_name_flags(klass,
						"EndInvoke", -1, 0);
				invoke = NULL;
				if (method != NULL) {
					invoke = mono_marshal_get_delegate_end_invoke(method);
				} else {
					if (SNOOPCAN) {
						printf(
								"\tFehler EndInvoke() fuer Klasse %s - method\n",
								klass->name);
						fflush(NULL);
					}
					fail_count++;
				}
				if (invoke != NULL) {
					if (SNOOPCAP) {
						printf("\t%s \n", mono_method_full_name(invoke, TRUE));
						fflush(NULL);
					}
					checkedMonoCompileMethod(invoke);
					count++;
				} else {
					if (SNOOPCAN) {
						printf(
								"\tFehler EndInvoke() fuer Klasse %s - invoke\n",
								klass->name);
						fflush(NULL);
					}
					fail_count++;
				}

			} else {
				//				if (SNOOPCA) {
				//					printf("preCompileWrapper() - kein Delegate-Invoke fuer Klasse: %s \n", klass->name);
				//					fflush(NULL);
				//				}
				ignore_count++;
			}
		}

		if (SNOOPCAN || SNOOPCAP) {
			printf(
					"preCompileWrapper() delegate-invoke Wrapper\tAnzahl Iterationen Hauptschleife: %i\n",
					i);
			printf(
					"preCompileWrapper() delegate-invoke Wrapper\tAnzahl ignorierter Methoden: %i\n",
					ignore_count);
			printf(
					"preCompileWrapper() delegate-invoke Wrapper\tAnzahl nicht pre-compilierbarer Methoden: %i\n",
					fail_count);
			printf("preCompileWrapper(): delegate-invoke wrapper fertig\n");
			printf("preCompileWrapper(): Beginne synchronized Wrappers\n");
			fflush(NULL);
		}
		delegateInvokeWrappersOkay = 1;
	} else {
		;
	}

	if (synchronizedWrappersOkay == 0) {
		/* Synchronized wrappers */
		for (i = 0, fail_count = 0, ignore_count = 0; i
				< image->tables[MONO_TABLE_METHOD].rows; ++i) {

			token = MONO_TOKEN_METHOD_DEF | (i + 1);

			method = NULL;
			method = mono_get_method(image, token, NULL);

			if (method == NULL) {
				fail_count++;
				continue;
			}

			if ((method->iflags & METHOD_IMPL_ATTRIBUTE_SYNCHRONIZED)
					&& (!method->is_generic)) {
				invoke = NULL;
				invoke = mono_marshal_get_synchronized_wrapper(method);

				if (invoke != NULL) {
					if (SNOOPCAP) {
						printf("\tSynchronized Wrapper: %s fuer %s\n",
								mono_method_full_name(invoke, TRUE),
								mono_method_full_name(method, TRUE));
						fflush(NULL);
					}
					checkedMonoCompileMethod(invoke);
					count++;
				} else {
					if (SNOOPCAN) {
						printf("\tFehler Synchronized Wrapper fuer %s\n",
								mono_method_full_name(method, TRUE));
						fflush(NULL);
					}
					fail_count++;
				}
			} else {
				if (SNOOPCAN && (method->iflags
						& METHOD_IMPL_ATTRIBUTE_SYNCHRONIZED)) {
					printf("\tFehler Synchronized Wrapper fuer %s \n",
							mono_method_full_name(method, TRUE));
					fflush(NULL);
				}
				ignore_count++;
			}
		}

		if (SNOOPCAN || SNOOPCAP) {
			printf(
					"preCompileWrapper() synchronized Wrapper\tAnzahl Iterationen Hauptschleife: %i\n",
					i);
			printf(
					"preCompileWrapper() synchronized Wrapper\tAnzahl ignorierter Methoden: %i\n",
					ignore_count);
			printf(
					"preCompileWrapper() synchronized Wrapper\tAnzahl nicht pre-compilierbarer Methoden: %i\n",
					fail_count);
			printf("preCompileWrapper(): synchronized wrapper fertig\n");
			printf("preCompileWrapper(): Beginne PInvoke wrapper\n");
			fflush(NULL);
		}
		synchronizedWrappersOkay = 1;
	} else {
		;
	}
	//
	//  Erzeugung der Wrapper fuer den Aufruf von Internal Calls,
	//  PInvokes und Runtime Calls.
	//
	if (pinvokeWrappersOkay == 0) {
		/* pinvoke wrappers */
		for (i = 0, fail_count = 0, ignore_count = 0; i
				< image->tables[MONO_TABLE_METHOD].rows; ++i) {
			token = MONO_TOKEN_METHOD_DEF | (i + 1);
			method = NULL;
			method = mono_get_method(image, token, NULL);

			if (method == NULL) {
				fail_count++;
				continue;
			}
			if ((method->flags & METHOD_ATTRIBUTE_PINVOKE_IMPL)
					|| (method->iflags & METHOD_IMPL_ATTRIBUTE_INTERNAL_CALL)
					|| ((method->iflags & METHOD_IMPL_ATTRIBUTE_RUNTIME)
							&& ((mono_method_signature(method)->pinvoke) != 0))) {

				invoke = mono_marshal_get_native_wrapper(method, TRUE, FALSE);

				if (invoke != NULL) {
					if (SNOOPCAP) {
						printf("\t%s fuer %s\n", mono_method_full_name(invoke,
								TRUE), mono_method_full_name(method, TRUE));
						fflush(NULL);
					}
					checkedMonoCompileMethod(invoke);
					count++;
				} else {
					if (SNOOPCAN) {
						printf(
								"\tFehler mono_marshal_get_native_wrapper() fuer %s\n",
								mono_method_full_name(method, TRUE));
						fflush(NULL);
					}
					fail_count++;
				}
			} else {
				ignore_count++;
			}
		}

		if (SNOOPCAN || SNOOPCAP) {
			printf(
					"preCompileWrapper() PInvoke Wrapper\tAnzahl Iterationen Hauptschleife: %i\n",
					i);
			printf(
					"preCompileWrapper() PInvoke Wrapper\tAnzahl ignorierter Methoden: %i\n",
					ignore_count);
			printf(
					"preCompileWrapper() PInvoke Wrapper\tAnzahl nicht pre-compilierbarer Wrapper: %i\n",
					fail_count);
			printf("preCompileWrapper(): pinvoke wrapper fertig\n");
			printf(
					"preCompileWrapper(): Beginne StructureToPtr/PtrToStructure wrapper\n");
			fflush(NULL);
		}
		pinvokeWrappersOkay = 1;
	} else {
		;
	}

	if (StructureToPtrWrappersOkay == 0) {
		/* StructureToPtr/PtrToStructure wrappers */
		for (i = 0; i < image->tables[MONO_TABLE_TYPEDEF].rows; ++i) {
			MonoClass *klass;
			MonoMarshalType *info;
			MonoType *ftype;
			MonoMarshalNative ntype;
			MonoMarshalConv conv;
			int k, doNotMarshal;
			gboolean last_field;
			int msize;
			int usize;
			//
			// Jedes Feld einer Klasse/Struktur, für die ein
			// StructureToPtr/PtrToStructure-Wrapper erzeugt werden
			// soll, wird darauf ueberprueft, ob es bei der Erzeugung des
			// Wrappers einen Fehler verursachen wuerde, der Mono
			// terminieren laesst.
			//
			// Der Ausgangspunkt fuer die Ueberpruefung ist die Fehlersituation
			// ('g_error()') in der Funktion "emit_ptr_to_object_conv()"
			// in mono/metadata/marshal.c. Der Fehler tritt dann auf, wenn
			// ein Feld ein Array im managed Code darstellt und dieses mit
			// der Marshalling-Konvention "MONO_MARSHAL_CONV_ARRAY_LPARRAY"
			// in die native Repraesentation konvertiert werden soll, die
			// Klasse nicht blittable (http://msdn.microsoft.com/en-us/library/75dwhxf7.aspx)
			// ist und keine Layout-Informationen für das Marshalling
			// angegeben wurden.
			//
			// Der Code zur Filterung der Klassen/Strukturen, fuer die ein
			// Wrapper erzeugt werden soll, wurde den Funktionen "emit_struct_conv()"
			// und "emit_ptr_to_object_conv()" aus mono/metadata/marshal.c
			// entnommen, da der dort auftretende Fehler beseitigt werden soll.
			// Fehlersituationen, die in anderen Funktionen auftreten koennen
			// wurden nicht beruecksichtigt. Die Korrektheit der auf diesem
			// Weg erzeugten Wrapper kann nicht verifiziert werden, sondern
			// lediglich experimentell untersucht werden.
			//
			token = MONO_TOKEN_TYPE_DEF | (i + 1);
			klass = mono_class_get(image, token);

			if (!klass->generic_container && !klass->generic_class) {
				//
				// Initialisierung von Marshalling-Informationen
				// einer Klasse aus deren Metadaten. Dabei wird der
				// Member "MonoMarshalType * marshal_info" der Mono-internen
				// Struktur zur Repraesentation einer Klasse (_MonoClass)
				// initialisiert.
				//
				info = mono_marshal_load_type_info(klass);
				doNotMarshal = 0;

				for (k = 0; k < info->num_fields; k++) {
					ftype = info->fields[k].field->type;

					if (ftype->attrs & FIELD_ATTRIBUTE_STATIC) {
						if (SNOOPCAN) {
							printf("(Static Attribute) %s.%s\n",
									klass->name_space, klass->name);
							fflush(NULL);
						}
						continue;
					}
					//
					// Der folgende Funktion bestimmt die native Repraesentation eines Feldes
					// der Klasse/Struktur, fuer die ein StructureToPtr/PtrToStructure-Wrapper
					// erzeugt werden soll. Der Aufruf wurde aus der Funktion "emit_struct_conv()"
					// in mono/metadata/marshal.c uebernommen. Als Seiteneffekt bestimmt diese
					// Funktion die Marshalling-Konvention (gespeichert in 'conv') des
					// untersuchten Feldes.
					//
					// Der Parameter 'as_field', der bei diesem Aufruf auf TRUE gesetzt wird,
					// wird in der Funktion offenbar nicht ausgewertet. Damit spielt es hier
					// keine Rolle, welcher Wert uebergeben wird.
					//
					ntype = mono_type_to_unmanaged(ftype,
							info->fields[k].mspec, TRUE, klass->unicode, &conv);
					/*
					 if (conv == MONO_MARSHAL_CONV_ARRAY_LPARRAY) {
					 printf("\tMONO_MARSHAL_CONV_ARRAY_LPARRAY fuer %s.%s\n",klass->name_space, klass->name);
					 printf("\t\tBlittable: %i\tNative Size: %i\n", klass->blittable, info->native_size);
					 printf("\t\tAuto Layout: %i\n", ((klass->flags & TYPE_ATTRIBUTE_LAYOUT_MASK) == TYPE_ATTRIBUTE_AUTO_LAYOUT));
					 fflush(NULL);
					 }
					 */
					msize = 0;
					usize = 0;
					if (k < (info->num_fields - 1)) {
						last_field = FALSE;
					} else {
						last_field = TRUE;
					}
					if (last_field) {
						msize = klass->instance_size
								- info->fields[k].field->offset;
						usize = info->native_size - info->fields[k].offset;
					} else {
						msize = info->fields[k + 1].field->offset
								- info->fields[k].field->offset;
						usize = info->fields[k + 1].offset
								- info->fields[k].offset;
					}

					if ( //
					// Sind die folgenden Bedingungen erfuellt, dann tritt ein
					// Fehler in "emit_ptr_to_object_conv()" auf.
					//
					(
					// Gilt folgende Bedingung nicht, so tritt der zu behebende Fehler
					// in "emit_ptr_to_object_conv()" nicht auf.
					(conv == MONO_MARSHAL_CONV_ARRAY_LPARRAY) &&
					// Gelten folgende Bedingungen (einzeln) nicht, so ruft
							// "emit_struct_conv()" nicht "emit_ptr_to_object_conv()" auf.
							(klass->blittable == 0) && (info->native_size != 0)
							&& ((((klass->flags & TYPE_ATTRIBUTE_LAYOUT_MASK)
									== TYPE_ATTRIBUTE_AUTO_LAYOUT) == 0)
									&& (klass != mono_defaults.safehandle_class)))
							||
							//
							// Sind die folgenden Bedingungen erfuellt, dann tritt ein Fehler
							// in "emit_struct_conv()" auf.
							//
							((klass != mono_defaults.safehandle_class)
									&& (((klass->flags
											& TYPE_ATTRIBUTE_LAYOUT_MASK)
											== TYPE_ATTRIBUTE_EXPLICIT_LAYOUT)
											&& (usize == 0))
									&& (MONO_TYPE_IS_REFERENCE (info->fields [k].field->type)
											|| ((!last_field
													&& MONO_TYPE_IS_REFERENCE (info->fields [k + 1].field->type)))))) {
						//
						// Der Fehler in "emit_ptr_to_object_conv()" tritt genau dann auf,
						// wenn ein Feld der Klasse/Struktur die Marshalling-Konvention
						// "MONO_MARSHAL_CONV_ARRAY_LPARRAY" zugeordnet wurde (siehe
						// "mono_type_to_unmanaged()"), die Klasse nicht blittable ist,
						// d.h., nicht ohne Konvertierung marshalled werden kann, eine
						// Groesze groeszer Null hat und kein Layout-Attribut zugewiesen
						// wurde, beispielsweise durch Annotationen im C#-Code.
						//
						doNotMarshal = 1;
						//
						// Ausgabe der Klassen/Strukturen, fuer die kein
						// StructureToPtr/PtrToStructure-Wrapper erzeugt wurde.
						// Ein Anwender sollte sich diese Liste als Warnung anzeigen
						// lassen koennen um zu ueberpruefen, ob eine seiner eigenen
						// Datenstrukturen davon betroffen ist.
						//
						if (SNOOPCAN) {
							printf(
									"\tSkip StructureToPtr/PtrToStructure-Wrapper fuer %s.%s\n",
									klass->name_space, klass->name);
							fflush(NULL);
						}
						break;
					}
				}
				//
				// Die Wrapper werden nur dann generiert, wenn kein Fehler zu erwarten ist.
				//
				if (doNotMarshal == 0) {
					if (SNOOPCAP) {
						printf("\t%s.%s\n", klass->name_space, klass->name);
						fflush(NULL);
					}
					invoke = mono_marshal_get_struct_to_ptr(klass);
					checkedMonoCompileMethod(invoke);
					count++;

					invoke = mono_marshal_get_ptr_to_struct(klass);
					checkedMonoCompileMethod(invoke);
					count++;
				}
				//
				// Die Wrapper werden nur dann generiert, wenn kein Fehler zu erwarten ist.
				//
			} else {
				if (SNOOPCAN) {
					printf("(Generic) %s.%s\n", klass->name_space, klass->name);
					fflush(NULL);
				}
				//				printf("\t\tKein StrToPtr/PtrToStructure-Wrapper fuer %s.%s (ist Generic Container)\n",	klass->name_space, klass->name);
				//				fflush(NULL);
				count++;
			}
		}

		if (SNOOPCAN || SNOOPCAP) {
			printf("preCompileWrapper(): StructureToPtr wrapper fertig\n");
			fflush(NULL);
		}
		StructureToPtrWrappersOkay = 1;
	} else {
		;
	}

	if (unboxWrappersOkay == 0) {
		//
		//  Generierung der unbox-Wrapper.
		//
		for (i = 0, fail_count = 0, ignore_count = 0; i
				< image->tables[MONO_TABLE_METHOD].rows; ++i) {
			invoke = NULL;
			token = MONO_TOKEN_METHOD_DEF | (i + 1);
			method = NULL;
			method = mono_get_method(image, token, NULL);

			if (method == NULL) {
				fail_count++;
				continue;
			}

			if ((method != NULL) && (mono_method_signature(method)->hasthis)
					&& (method->klass->valuetype)) {

				invoke = mono_marshal_get_unbox_wrapper(method);
				if (invoke != NULL) {
					if (SNOOPCAP) {
						printf("\t%s fuer %s\n", mono_method_full_name(invoke,
								TRUE), mono_method_full_name(method, TRUE));
						fflush(NULL);
					}
					checkedMonoCompileMethod(invoke);
				} else {
					if (SNOOPCAN) {
						printf(
								"\tFehler mono_marshal_get_unbox_wrapper() fuer %s\n",
								mono_method_full_name(method, TRUE));
						fflush(NULL);
					}
					fail_count++;
				}
			} else {
				ignore_count++;
			}
		}

		if (SNOOPCAN || SNOOPCAP) {
			printf(
					"preCompileWrapper() Unbox Wrapper\tAnzahl Iterationen Hauptschleife: %i\n",
					i);
			printf(
					"preCompileWrapper() Unbox Wrapper\tAnzahl ignorierter Methoden: %i\n",
					ignore_count);
			printf(
					"preCompileWrapper() Unbox Wrapper\tAnzahl nicht pre-compilierbarer Wrapper: %i\n",
					fail_count);
			printf("preCompileWrapper(): Unbox wrapper fertig\n");
			fflush(NULL);
		}
		unboxWrappersOkay = 1;
	} else {
		;
	}
	/*
	 if (thunkInvokeWrappersOkay == 0) {
	 for (i = 0, fail_count = 0, ignore_count = 0; i < image->tables[MONO_TABLE_METHOD].rows; ++i) {
	 invoke = NULL;
	 token = MONO_TOKEN_METHOD_DEF | (i + 1);
	 method = NULL;
	 method = mono_get_method(image, token, NULL);

	 if (method == NULL) {
	 fail_count++;
	 continue;
	 }

	 if ((method->klass->flags & TYPE_ATTRIBUTE_INTERFACE) == 0) {
	 invoke = mono_marshal_get_thunk_invoke_wrapper (method);

	 if (invoke != NULL) {
	 checkedMonoCompileMethod(invoke);
	 }
	 }
	 }
	 thunkInvokeWrappersOkay = 1;
	 }
	 */

}
/*
 * @brief precompile_get_runtime_invoke_sig() erzeugt
 * einen Runtime-Invoke-Wrapper fuer Klassenkonstruktoren,
 * d.h., einen Runtime-Invoke Wrapper fuer eine Methode
 * ohne Rueckgabewert und ohne formale Parameter.
 * Der Wrapper wird durch Monos Wrapper-Sharing nach
 * seiner Erzeugung fuer alle Klassenkonstruktoren verwendet
 * werden koennen. Die erzeugte Methode "FOO" dient dabei
 * als Dummy. Die als Parameter uebergebene Signatur fuer
 * derartige Methoden wird in "mono_metadata_signature_alloc()"
 * generiert und vor dem Aufruf mit dem void-Rueckgabewert
 * versehen.
 */
MonoMethod* precompile_get_runtime_invoke_sig(MonoMethodSignature *sig) {
	MonoMethodBuilder *mb;
	MonoMethod *m;
	MonoMethod * invoke;

	mb = mono_mb_new(mono_defaults.object_class, "FOO", MONO_WRAPPER_NONE);
	m = mono_mb_create_method(mb, sig, 16);

	invoke = mono_marshal_get_runtime_invoke(m, FALSE);

	//	if (strcmp(invoke->name, "runtime_invoke_void__this___intptr") == 0) {
	//		printf("Wrapper runtime_invoke_void__this___intptr gefunden\n");
	//	}

	return invoke;
}
/*
 * @brief	preCompileJitIcallWrapper() erzeugt Wrapper fuer Internal-JIT-Calls
 * @param	key			Key der Hash-Tabelle "jit_icall_hash_name", fuer den die Funktion aufgerufen wird
 * @param	value		Value der Hash-Tabelle "jit_icall_hash_name", fuer den die Funktion aufgerufen wird
 * @param	user_data	Zeiger auf das zu pre-compilierende Assembly
 *
 * Die Funktion preCompileJitIcallWrapper() generiert und
 * pre-compiliert Wrapper fuer Internal-JIT-Calls. Internal-JIT-Calls
 * sind Funktionen, zu denen der JIT-Compiler Aufrufe in den
 * nativen Code emittierten kann. Das ist fuer den Nutzer/Programmierer
 * transparent. Ein Internal-JIT-Call wird mit "mono_register_jit_icall()"
 * bzw. "register_icall()" registriert und dabei in die Hash-Tabellen
 * "jit_icall_hash_name" (Key: Name) und "jit_icall_hash_addr"
 * (Key: vmtl. Funktionszeiger) aufgenommen. Diese Registrierung
 * der Internal-JIT-Calls erfolgt waehrend der Initialisierungsphase
 * Monos. Die Emitterung eines Internal-JIT-Calls erfolgt in der
 * Phase bei der Umwandlung des IL-Codes in eine Intermediate
 * Represenation ("method_to_ir()"). Bei der Emittierung werden
 * die Funktionen "mono_emit_jit_icall()" und "mono_emit_native_call()"
 * genutzt, wobei der Wrapper fuer den Internal-JIT-Call
 * erzeugt wird.
 *
 * Zur Generierung eines Wrappers wird die Funktion
 * "mono_icall_get_wrapper_full()" aufgerufen. Diese
 * pre-compiliert den erzeugten Wrapper, wenn Pre-JIT
 * aktiviert ist. Der Wrapper wird dabei mit
 * "mono_register_jit_icall_wrapper()" registriert,
 * indem er in der Hash-Tabelle "jit_icall_hash_addr"
 * gespeichert wird.
 */
void preCompileJitIcallWrapper(gpointer key, gpointer value, gpointer user_data) {

	MonoJitICallInfo *callinfo = value;
	gconstpointer code = NULL;

	if (!callinfo->sig)
		return;
	//
	//  Der Wrapper zum Aufruf der Funktion, die in callinfo->func gespeichert ist,
	//  wird erzeugt. Der Parameter "do_compile" wird auf den Wert "TRUE" gesetzt,
	//  damit der Wrapper compiliert wird und dessen Adresse zurueckgegeben wird,
	//  anstatt ein JIT-Trampoline auf den Wrapper zu erzeugen.
	//
	code = mono_icall_get_wrapper_full(callinfo, TRUE);

	return;
}

gboolean precompile_can_marshal_struct(MonoClass *klass) {
	MonoClassField *field;
	gboolean can_marshal = TRUE;
	gpointer iter = NULL;

	if ((klass->flags & TYPE_ATTRIBUTE_LAYOUT_MASK)
			== TYPE_ATTRIBUTE_AUTO_LAYOUT)
		return FALSE;

	/* Only allow a few field types to avoid asserts in the marshalling code */
	while ((field = mono_class_get_fields(klass, &iter))) {
		if ((field->type->attrs & FIELD_ATTRIBUTE_STATIC))
			continue;

		switch (field->type->type) {
		case MONO_TYPE_I4:
		case MONO_TYPE_U4:
		case MONO_TYPE_I1:
		case MONO_TYPE_U1:
		case MONO_TYPE_BOOLEAN:
		case MONO_TYPE_I2:
		case MONO_TYPE_U2:
		case MONO_TYPE_CHAR:
		case MONO_TYPE_I8:
		case MONO_TYPE_U8:
		case MONO_TYPE_I:
		case MONO_TYPE_U:
		case MONO_TYPE_PTR:
		case MONO_TYPE_R4:
		case MONO_TYPE_R8:
		case MONO_TYPE_STRING:
			break;
		case MONO_TYPE_VALUETYPE:
			if (!precompile_can_marshal_struct(mono_class_from_mono_type(
					field->type)))
				can_marshal = FALSE;
			break;
		default:
			can_marshal = FALSE;
			break;
		}
	}

	/* Special cases */
	/* Its hard to compute whenever these can be marshalled or not */
	if (!strcmp(klass->name_space, "System.Net.NetworkInformation.MacOsStructs"))
		return TRUE;

	return can_marshal;
}
/*
 * @brief	checkedMonoCompileMethod() JIT-compiliert eine Methode
 * und uerberprueft vorher, ob diese vom Pre-JIT ausgeschlossen werden soll.
 * @param	method	Zeiger auf die MonoMethod-Struktur der zu compilierenden Methode
 * @return	Zeiger auf den nativen Code der compilierten Methode oder NULL, falls die Compilierung nicht klappte.
 *
 * Diese Funktion wird immer dann aufgerufen, wenn ein Element im
 * Rahmen des Pre-JIT compiliert werden soll. Falls die Compilierung
 * eine Exception ausloest, so wird diese Exception im managed Code
 * abgefangen und das Pre-JIT neu gestartet. Das den Fehler verursachende
 * Element soll zukuenftig ausgeschlossen werden. Daher wird vor der
 * Compilierung geprueft, ob sich das Element in der Ausschlussliste,
 * welche durch die global sichtbare Hash-Tabelle "ExcludedFromPreJit"
 * repraesentiert ist, befindet. Falls ja, dann gibt "checkedMonoCompileMethod()"
 * NULL zurueck. Falls nicht, so wird das zu compilierende Element in der
 * global sichtbaren Variable "pLastPreCompiledElement_g" Element
 * gespeichert, um es spaeter im Falle einer Exception in die
 * Ausschlussliste aufzunehmen. Dies wird in der Funktion
 * "insertInPreJitExclusionList()" gemacht, die aus dem managed Code,
 * der die Exception abgefangen hat, aufgerufen wird.
 *
 */
gpointer checkedMonoCompileMethod(MonoMethod * method) {

	gpointer nativeCode;

	if (g_hash_table_lookup(ExcludedFromPreJit, (gpointer) method) != NULL) {
		nativeCode = NULL;
	} else {
		pLastPreCompiledElement_g = (gpointer) method;
		nativeCode = mono_compile_method(method);
	}
	return nativeCode;
}
/*
 * @brief	printStackTrace() gibt den aktuellen managed Stacktrace aus
 *
 * Diese Funktion wird waehrend der Laufzeit des Anwender-Programms
 * aufgerufen, wenn die Ueberwachung des JIT-Compilers (Kommandozeilenparameter
 * "--jtrace"), der Trampoline ("--ptrace") oder der Wrapper-Erzeugung
 * ("--wtrace") aktiviert wurde. Diese Funktion ruft die managed Methode
 * "string printStackTrace ()" im Assembly "preJitPrePatchMain-xxx.dll"
 * auf.
 */
void printStackTrace(char ** ppcMessageToUser_p)
{
	MonoMethodDesc * pMethDesc;
	MonoMethod * pMethod;
	MonoDomain * pAppDom;
	MonoAssembly * pAssembly;
	MonoImage * pImage;
	MonoObject * managedResult;
	char * pcMessage;

	static __thread int inUse = 0;


	pMethDesc = NULL;
	pMethod = NULL;
	pAppDom = NULL;
	pAssembly = NULL;
	pImage = NULL;
	managedResult = NULL;
	pcMessage = NULL;

	if (0 == inUse)
	{
		inUse = 1;
	}
	else
	{
		//
		//  Die Ausgabe eines Stack-Traces wird abgebrochen, wenn
		//  sie im Rahmen der Ausgabe eines Stack-Traces erfolgt.
		//
		if ((ppcMessageToUser_p != NULL) && ((*ppcMessageToUser_p) != NULL))
		{
			free((*ppcMessageToUser_p));
			*ppcMessageToUser_p = NULL;
		}
		return;
	}

	if ((ppcMessageToUser_p != NULL) && ((*ppcMessageToUser_p) != NULL))
	{
		printf("\n%s", (*ppcMessageToUser_p));
		fflush(NULL);
		free((*ppcMessageToUser_p));
		*ppcMessageToUser_p = NULL;
	}
	else
	{
		if (ppcMessageToUser_p != NULL)
		{
			//
			//  In der aufrufenden Funktion konnte keine
			//  Nachricht erzeugt werden, bspw. weil kein
			//  Speicher reserviert werden konnte.
			//
			printf("\nKeine Informationen verfuegbar.\n");
			fflush(NULL);
		}
		else
		{
			//
			//  Sonderfall fuer das Delegate Trampoline.
			//  Der Aufruf von printStackTrace() aus dem
			//  Trampoline erzeugt keinen Stack-Trace. Daher
			//  wird die Funktion aus fireDelegateCtor()
			//  aufgerufen. Die Nachricht an den Nutzer
			//  wurde bereits im Delegate Trampoline im
			//  ausgegeben, wenn das Delegate Trampoine im
			//  Pre-Patch-Fall ist.
			//  Wird kein Pre-Patch im Delegate-Trampoline
			//  ausgegeben, so wird printStackTrace()
			//  normal aufgerufen, wobei kein Stack-Trace
			//  ermittelt werden wird.
			//
			;
		}
	}
	//
	//  Erzeugen einer Methodenbeschreibung, mit der die managed
	//  Methode zur Ausgabe des Stack-Traces geladen wird.
	//
	pMethDesc = mono_method_desc_new("MonitoringClass:printStackTrace", FALSE);
	if (pMethDesc != NULL)
	{
		pAppDom = mono_domain_get();
		if (pAppDom == NULL)
		{
			return;
		}

		pAssembly = mono_domain_assembly_open(pAppDom, "MonoRT.dll");
		if (pAssembly == NULL)
		{
			printf("\n\tMonoRT:\tCan not load MonoRT.dll\n");
			fflush(NULL);
			return;
		}

		pImage = mono_assembly_get_image(pAssembly);
		if (pImage == NULL)
		{
			printf("\n\tMonoRT:\tCan not determine image from MonoRT.dll\n");
			fflush(NULL);
			return;
		}

		pMethod = mono_method_desc_search_in_image(pMethDesc, pImage);
		if (pMethod != NULL)
		{
			managedResult = mono_runtime_invoke(pMethod, NULL, NULL, NULL);
		}
		//
		//  Freigabe der angelegten Methodenbeschreibung
		//
		mono_method_desc_free(pMethDesc);
	}

	if (managedResult != NULL)
	{
		pcMessage = mono_string_to_utf8((MonoString *) managedResult);
		if (pcMessage != NULL)
		{
			printf("%s\n\n", pcMessage);
			fflush(NULL);
			g_free(pcMessage);
			pcMessage = NULL;
		}
	}
	inUse = 0;
	return;
}
/*
 * @brief	setMonitoringOptionsInternal() aktiviert oder deaktiviert
 * das Monitoring.
 */
void setMonitoringOptionsInternal(int jitTrace, int patchTrace,
		int wrapperTrace)
{
	doTraceJIT_g = jitTrace;
	doTracePatch_g = patchTrace;
	snoopwrapper = wrapperTrace;

	return;
}
