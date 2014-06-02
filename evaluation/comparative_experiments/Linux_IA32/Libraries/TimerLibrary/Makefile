CC      = gcc

default: all

all: libtimer.so

libtimer.so: timer.c
	$(CC) -fPIC -DPIC -c -O0 timer.c
	$(CC) -shared -o libtimer.so -W-soname timer.o

clean:
	rm -f *timer.*o