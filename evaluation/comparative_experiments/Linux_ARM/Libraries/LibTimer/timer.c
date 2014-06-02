#include <errno.h>
#include <stdio.h>
#include <stdint.h>
#include <stdlib.h>
#include <unistd.h>
#include <sys/types.h>
#include <time.h>
#include <sys/wait.h>
#include <sys/stat.h>
#include <fcntl.h>

unsigned long long getticks(void)
{
  #if (defined(__i386__))
  unsigned long long ret;
  __asm__ __volatile__("rdtsc": "=A" (ret));
  return ret;

  #elif (defined(__x86_64__)) 
  unsigned a, d; 
  asm volatile("rdtsc" : "=a" (a), "=d" (d)); 
  return ((unsigned long long)a) | (((unsigned long long)d) << 32);
  
  #elif (defined (__arm__))
  unsigned uiCycles;
  __asm__ __volatile__ ("mrc p15, 0, %0, c15, c12, 1" : "=r" (uiCycles));
  return ((unsigned long long) uiCycles);

  #else
  #error Unknown Architecture!
  #endif
}

void initTimer ()
{
  #if (defined (__arm__))
  __asm__ __volatile__ ("mcr p15, 0, %0, c15, c12, 0" : : "r" (1));
  #endif
  return;
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
  #if (defined(__i386__)) || (defined(__x86_64__))
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
	
  #elif (defined (__arm__))
	return 0;
  #else
  #error Unknown Architecture!
  #endif
}


int cleancache ( void )
{
  int pid,w,status;
  #if (defined(__i386__)) || (defined(__x86_64__))
  
	pid = fork();
	
	if (pid == 0)
	{
		execl("/bin/sh","sh","-c","echo 1 > /dev/testdev2; sync; echo 3 | tee /proc/sys/vm/drop_caches; echo 1 > /dev/testdev2",(char *)0);
		exit(127);
	}
	w = wait(&status);
  #elif (defined (__arm__))
	pid = fork();	
	if (pid == 0)
	{
		execl("/bin/sh","sh","-c", "sync; echo 3 | tee /proc/sys/vm/drop_caches; echo 1 > /dev/testdev2",(char *)0);
		exit(127);
	}
	w = wait(&status);

  #endif
	return 1;
}
