using System;

class Subclient {

	static Subclient () {
		//
		//  Beginn Aenderung Test
		//
		Console.WriteLine("\n\t**** static Subclient (): throw new Exception () ****\n");
		//
		//  Ende Aenderung Test
		//
		throw new Exception ();
	}
	~Subclient () {
		Console.WriteLine ("in finalizer");
	}
}

class Test {
	static Subclient s;
	static void Main () {
		Console.WriteLine ("testing");
		try {
			s = new Subclient ();
		} catch {
			Console.WriteLine ("got it");
		}
		Console.WriteLine ("done");
	}
}

