using System;

class A : T {}
class T {
	static int Main ()
	{
		object o = (T [][]) (object) (new A [][] {});
 		//
		//  Beginn Aenderung Test
		//
		// return o.GetHashCode () - o.GetHashCode ();
//		Console.WriteLine ("\n\t*** VOR hash1 = o.GetHashCode () ***\n");
		int hash1 = o.GetHashCode ();
		
//		Console.WriteLine ("\n\t*** NACH hash1 = o.GetHashCode () ***\n");
//		Console.WriteLine ("\n\t*** VOR hash2 = o.GetHashCode () ***\n");
		int hash2 = o.GetHashCode ();
//		Console.WriteLine ("\n\t*** NACH hash2 = o.GetHashCode () ***\n");
		int ret = hash1 - hash2;
		return ret;
		//
		//  Ende Aenderung Test
		//
	}
}

