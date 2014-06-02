#include "interp.h"
#include "embed.h"

int
main (int argc, char* argv[])
{
	int ret;

	ret = mlockall(MCL_CURRENT | MCL_FUTURE);

	if (ret != 0) {
		printf("mlockall() Fehler\n");
		fflush(NULL);
	}

	return mono_main (argc, argv);
}

