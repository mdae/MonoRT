CC      = g++

default: all

all: complex_variant.out simple_variant.out

complex_variant.out: complex_variant.cpp timer.c
	$(CC) complex_variant.cpp timer.c -o complex_variant.out
	
simple_variant.out: simple_variant.cpp timer.c
	$(CC) simple_variant.cpp timer.c -o simple_variant.out
clean:
	rm -f *.out