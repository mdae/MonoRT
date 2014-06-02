using System;

class Class1 {
	static int Main(string[] args)
	{
		string s1 = "original";

		try {
			bool huh = s1.StartsWith(null);
		} catch (ArgumentNullException e) {
			//
			// Beginn Aenderung Test
			//
			Console.WriteLine(e.ToString());
			//
			// Ende Aenderung Test
			//
		}

		if (s1.StartsWith("o")){
			return 0;
		} else {
			return 1;
		}
	}
}


