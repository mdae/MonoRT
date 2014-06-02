CC      = g++

default: all

all: libtimer.so

libtimer.so: timer.c
	$(CC) -fPIC -DPIC -I//usr/local/jamaica/target/linux-x86/include/ -c -m32 -O0 timer.c
	$(CC) -shared -I/usr/local/jamaica/target/linux-x86/include/ -o libtimer.so -W-soname timer.o

clean:
	rm -f *timer.*o
