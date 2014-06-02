CC      = g++

default: all

all: libtimer.so

libtimer.so: timer.c
	$(CC) -fPIC -DPIC -c -O0 timer.c
	$(CC) -shared -I/home/user/Desktop/ibm/ibm-wrt-i386-60/include -o libtimer.so -W-soname timer.o

clean:
	rm -f *timer.*o
