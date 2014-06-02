using System;

public class Test {
	public static int Main() {
		try {
			Console.WriteLine("In try");
			throw new Exception();
			//return(1);
		}
		catch (Exception e) {
			//
			// Beginn Aenderung Test
			//
			Console.WriteLine(e.ToString());
			//
			// Ende Aenderung Test
			//
			Console.WriteLine("In catch");
			return(0);
		}
		finally {
			Console.WriteLine("In finally");
		}
		return 2;
	}
}

