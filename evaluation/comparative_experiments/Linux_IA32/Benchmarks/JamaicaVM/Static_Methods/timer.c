#include <jni.h>
#include <stdio.h>
#include "NHRT1000StaticMethods.h"
#include <errno.h>
#include <stdint.h>
#include <stdlib.h>
#include <unistd.h>
#include <sys/types.h>
#include <time.h>
#include <sys/wait.h>

JNIEXPORT jint JNICALL Java_NHRT1000StaticMethods_cleancache (JNIEnv * env, jclass c)
{  
	int pid,w,status;
	
	pid = fork();
	
	if (pid == 0)
	{
		execl("/bin/sh","sh","-c","echo 1 > /dev/testdev2; sync; echo 3 | tee /proc/sys/vm/drop_caches; echo 1 > /dev/testdev2",(char *)0);
		exit(127);
	}
	w = wait(&status);
	return 1;  
}