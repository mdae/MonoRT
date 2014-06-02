using System;
using System.Reflection;
using System.Runtime.InteropServices;

public class Test
{
	[DllImport ("libtest")]
	public static extern int test_method_thunk (int test_id, IntPtr testMethodHandle, IntPtr createObjectHandle);

	static void RunTests(int series, Type type)
	{
		const string Prefix = "Test";
		MethodInfo createObjectMethod = type.GetMethod ("CreateObject");
		
		foreach (MethodInfo mi in type.GetMethods ()) {
			string name = mi.Name;
			if (!name.StartsWith (Prefix))
				continue;

			int id = Convert.ToInt32 (name.Substring (Prefix.Length));
			//
			//  Beginn Aenderung Test
			//
			Console.WriteLine("\n\t****C#: VOR test_method_thunk (series + id, mi.MethodHandle.Value, createObjectMethod.MethodHandle.Value) für " + mi.Name.ToString() +" ****\n");
			//
			//  Ende Aenderung Test
			//
			int res = test_method_thunk (series + id, mi.MethodHandle.Value, createObjectMethod.MethodHandle.Value);
			//
			//  Beginn Aenderung Test
			//
			Console.WriteLine("\n\t****C#: NACH test_method_thunk (series + id, mi.MethodHandle.Value, createObjectMethod.MethodHandle.Value) für " + mi.Name.ToString() +" ****\n");
			//
			//  Ende Aenderung Test
			//
			if (res != 0) {
				Console.WriteLine ("{0} returned {1}", mi, res);
				Environment.Exit ((id << 3) + res);
			}
		}
	}

	public static int Main ()
	{
		//
		//  Beginn Aenderung Test
		//
		Console.WriteLine("\n\t****C#: VOR RunTests (0, typeof (Test)); ****\n");
		//
		//  Ende Aenderung Test
		//
		RunTests (0, typeof (Test));
		//
		//  Beginn Aenderung Test
		//
		Console.WriteLine("\n\t****C#: NACH RunTests (0, typeof (Test)); ****\n");
		//
		//  Ende Aenderung Test
		//
		RunTests (100, typeof (TestStruct));
		return 0;
	}

	public static object CreateObject ()
	{
		//
		//  Beginn Aenderung Test
		//
		Console.WriteLine("\n\t\t\t****C#: static object CreateObject ****\n");
		//
		//  Ende Aenderung Test
		//
		return new Test ();
	}

	public static void Test0 ()
	{
		//
		//  Beginn Aenderung Test
		//
		Console.WriteLine("\n\t\t\t****C#: static void Test0 ****\n");
		//
		//  Ende Aenderung Test
		//
	}

	public static int Test1 ()
	{
		//
		//  Beginn Aenderung Test
		//
		Console.WriteLine("\n\t\t\t****C#: static int Test1 () ****\n");
		//
		//  Ende Aenderung Test
		//
		return 42;
	}

	public static string Test2 (string s)
	{
		//
		//  Beginn Aenderung Test
		//
		Console.WriteLine("\n\t\t\t****C#: static string Test2 (string s) ****\n");
		//
		//  Ende Aenderung Test
		//
		return s;
	}

	public string Test3 (string a)
	{
		//
		//  Beginn Aenderung Test
		//
		Console.WriteLine("\n\t\t\t****C#: string Test3 (string a) ****\n");
		//
		//  Ende Aenderung Test
		//
		return a;
	}

	public int Test4 (string a, int i)
	{
		//
		//  Beginn Aenderung Test
		//
		Console.WriteLine("\n\t\t\t****C#: int Test4 (string a, int i) ****\n");
		//
		//  Ende Aenderung Test
		//
		return i;
	}

	public int Test5 (string a, int i)
	{
		//
		//  Beginn Aenderung Test
		//
		Console.WriteLine("\n\t\t\t****C#: int Test5 (string a, int i) ****\n");
		//
		//  Ende Aenderung Test
		//
		throw new NotImplementedException ();
	}

	public bool Test6 (byte a1, short a2, int a3, long a4, float a5, double a6, string a7)
	{
		//
		//  Beginn Aenderung Test
		//
		Console.WriteLine("\n\t\t\t****C#:  bool Test6 (byte a1, short a2, int a3, long a4, float a5, double a6, string a7) ****\n");
		//
		//  Ende Aenderung Test
		//
		return  a1 == 254 &&
			a2 == 32700 &&
			a3 == -245378 &&
			a4 == 6789600 &&
			(Math.Abs (a5 - 3.1415) < 0.001) &&
			(Math.Abs (a6 - 3.1415) < 0.001) &&
			a7 == "Test6";
	}

	public static long Test7 ()
	{
		//
		//  Beginn Aenderung Test
		//
		Console.WriteLine("\n\t\t\t****C#: static long Test7 () ****\n");
		//
		//  Ende Aenderung Test
		//
		return Int64.MaxValue;
	}

	public static void Test8 (ref byte a1, ref short a2, ref int a3, ref long a4, ref float a5, ref double a6, ref string a7)
	{
		//
		//  Beginn Aenderung Test
		//
		Console.WriteLine("\n\t\t\t****C#: static void Test8 (ref byte a1, ref short a2, ref int a3, ref long a4, ref float a5, ref double a6, ref string a7) ****\n");
		//
		//  Ende Aenderung Test
		//
		a1 = 254;
		a2 = 32700;
		a3 = -245378;
		a4 = 6789600;
		a5 = 3.1415f;
		a6 = 3.1415;
		a7 = "Test8";
	}

	public static void Test9 (ref byte a1, ref short a2, ref int a3, ref long a4, ref float a5, ref double a6, ref string a7)
	{
		//
		//  Beginn Aenderung Test
		//
		Console.WriteLine("\n\t\t\t****C#: static void Test9 (ref byte a1, ref short a2, ref int a3, ref long a4, ref float a5, ref double a6, ref string a7) ****\n");
		//
		//  Ende Aenderung Test
		//
		throw new NotImplementedException ();
	}

	public static void Test10 (ref Test obj)
	{
		//
		//  Beginn Aenderung Test
		//
		Console.WriteLine("\n\t\t\t****C#: static void Test10 (ref Test obj) ****\n");
		//
		//  Ende Aenderung Test
		//
		obj = new Test ();
	}
}


public struct TestStruct
{
	public int A;
	public double B;

	public static object CreateObject ()
	{
		return new TestStruct ();
	}

	public static bool Test0 (TestStruct s)
	{
		bool res =  s.A == 42 && Math.Abs (s.B - 3.1415) < 0.001;

		/* these changes must not be visible in unmanaged code */
		s.A = 12;
		s.B = 13;

		return res;
	}

	public static void Test1 (ref TestStruct s)
	{
		s.A = 42;
		s.B = 3.1415;
	}

	public static TestStruct Test2 ()
	{
		TestStruct s = new TestStruct ();
		s.A = 42;
		s.B = 3.1415;
		return s;
	}

	public void Test3 ()
	{
		A = 1;
		B = 17;
	}
}
