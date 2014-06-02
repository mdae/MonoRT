#include <errno.h>
#include <stdio.h>
#include <stdint.h>
#include <stdlib.h>
#include <unistd.h>
#include <sys/types.h>
#include <time.h>
#include <sys/wait.h>

unsigned long long getticks(void)
{
  #if (defined(__i386__))
      unsigned long long ret;
 
      __asm__ __volatile__("rdtsc": "=A" (ret));
      return ret;
  #endif     
       
  #if ( defined(__x86_64__) )
 
      unsigned a, d; 
      asm volatile("rdtsc" : "=a" (a), "=d" (d)); 
      return ((unsigned long long)a) | (((unsigned long long)d) << 32);
  #endif
}



unsigned long long get_null_measurement_delay()
{
	unsigned long long tsc_before, tsc_after;    
	tsc_before = getticks();
	tsc_after = getticks();
	return (tsc_after - tsc_before);
}


unsigned long long calibrate_tsc( void )
{
	unsigned long long diff, min, max;
	int i;

	diff = get_null_measurement_delay();
	min = diff;
	max = diff;
		
	for (i = 1; i <= 10; i++) {
		diff = get_null_measurement_delay();
		
		if (diff < min)
			min = diff;
	}
	return min;
}


unsigned long long getcycles( void )
{
	int delay = 5;
	unsigned long long tsc_before = 0;
	unsigned long long tsc_after = 0;
	unsigned long long overhead = 0;
	unsigned long long cycles_per_second = 0;

	tsc_before = getticks();
	sleep(delay);
	tsc_after = getticks();

	overhead = calibrate_tsc();
	
	cycles_per_second = (tsc_after - tsc_before - overhead)/ ((unsigned long long) delay);
	
	return cycles_per_second;
}



/*
  Flush the CPU and filesystem caches:
 
 - create an new process
 
 - open a non-interactive shell, that:
 
 - flushes the CPU caches by triggering the write-method of a special driver
   (included in subdirectory 'kernel-module')
 
 - writes the filesystem cache to disk by calling 'sync'
 
 - drops the filesystem cache
 
 - flushes the CPU cache again
 
*/

int cleancache ( void )
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