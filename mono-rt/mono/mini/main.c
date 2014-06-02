#include <config.h>
#include "mini.h"
#ifndef PLATFORM_WIN32
#include "buildver.h"
#endif

//
//  Beginn Pre-JIT/-Patch Code
//
#include "pre.h"
//
//  Ende Pre-JIT/-Patch Code
//

#ifdef PLATFORM_WIN32

int
main ()
{
	int argc;
	gunichar2** argvw;
	gchar** argv;
	int i;

	argvw = CommandLineToArgvW (GetCommandLine (), &argc);
	argv = g_new0 (gchar*, argc + 1);
	for (i = 0; i < argc; i++)
		argv [i] = g_utf16_to_utf8 (argvw [i], -1, NULL, NULL, NULL);
	argv [argc] = NULL;

	LocalFree (argvw);

	return mono_main (argc, argv);
}

#else

int
main (int argc, char* argv[])
{
	int ret;
	mono_build_date = build_date;
	//
	//  Beginn Pre-JIT/-Patch Code
	//
/*	ret = mlockall(MCL_CURRENT | MCL_FUTURE);

	if (ret != 0) {
		printf("Fehler mlockall(). Ende.\n");
		fflush(NULL);
		exit(1);
	}
*/	//
	//  Ende Pre-JIT/-Patch Code
	//
	ret = mono_main (argc, argv);
	//
	//  Beginn Pre-JIT/-Patch Code
	//
//	munlockall();

/*	if (fclose (fileDescriptorForDebug) != 0) {
		printf("Fehler Debug-Datei schliessen.");
		fflush(NULL);
	}
	printf("Debug-Datei geschlossen.");
	fflush(NULL);
*/	//
	//  Ende Pre-JIT/-Patch Code
	//
	return ret;
}

#endif
