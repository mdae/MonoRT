//
// modules.cs:
//
//  Tests for netmodules
//

using System;

public class Tests
{
	public static int Main (string[] args) {
		//
		//  Beginn Aenderung Test
		//
		//return TestDriver.RunTests (typeof (Tests), args);
		int ret = test_0_gettype_nonpublic ();

		if (0 == ret)
			return 0;
		else
			return 1;	
		//
		//  Ende Aenderung Test
		//
	}

	public static int test_0_gettype_nonpublic () {

		if (typeof (Tests).Assembly.GetType ("Foo+Bar") != null)
			return 0;
		else
			return 1;
	}
}
