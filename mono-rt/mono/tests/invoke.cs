using System;
using System.Reflection;

class Test {
	
	public struct SimpleStruct {
		public bool a;
		public bool b;

		public SimpleStruct (bool arg) {
			a = arg;
			b = false;
		}
	}
	
	static void Test2 () {
		Console.WriteLine ("Test2 called");
	}
	
	public static SimpleStruct Test1 (SimpleStruct ss) {
		Console.WriteLine ("Test1 called " + ss.a + " " + ss.b);
		SimpleStruct res = new SimpleStruct ();
		res.a = !ss.a;
		res.b = !ss.b;
		return res;
	}

	public static void Foo(ref int x, ref int y)
	{
		x = 20;
		y = 30;
	}
	
	static int Main () {
		Type t = typeof (Test);

		MethodInfo m2 = t.GetMethod ("Test2");
		if (m2 != null)
			return 1;

		MethodInfo m1 = t.GetMethod ("Test1");
		if (m1 == null)
			return 1;

		object [] args = new object [1];
		SimpleStruct ss = new SimpleStruct ();
		ss.a = true;
		ss.b = false;
		args [0] = ss;
		//
		//  Beginn Aenderung Test
		//
		Console.WriteLine("\n\t****C#: VOR m1.Invoke(object, object[]) ****\n");
		//
		//  Ende Aenderung Test
		//
		SimpleStruct res = (SimpleStruct)m1.Invoke (null, args);
		//
		//  Beginn Aenderung Test
		//
		Console.WriteLine("\n\t****C#: NACH m1.Invoke(object, object[]) ****\n");
		//
		//  Ende Aenderung Test
		//
		if (res.a == true)
			return 1;
		if (res.b == false)
			return 1;

		// Test that the objects for byref valuetype arguments are 
		// automatically created
		MethodInfo m3 = typeof(Test).GetMethod("Foo");
		
		args = new object[2];
		//
		//  Beginn Aenderung Test
		//
		Console.WriteLine("\n\t****C#: VOR m3.Invoke(object, object[]) ****\n");
		//
		//  Ende Aenderung Test
		//		
		m3.Invoke(null, args);
		//
		//  Beginn Aenderung Test
		//
		Console.WriteLine("\n\t****C#: NACH m3.Invoke(object, object[]) ****\n");
		//
		//  Ende Aenderung Test
		//
		if ((((int)(args [0])) != 20) || (((int)(args [1])) != 30))
			return 2;

		// Test the return value from  ConstructorInfo.Invoke when a precreated
		// valuetype is used.
		ConstructorInfo ci = typeof (SimpleStruct).GetConstructor (new Type [] { typeof (bool) });
		//
		//  Beginn Aenderung Test
		//
		Console.WriteLine("\n\t****C#: VOR SimpleStruct ConstructorInfo.Invoke() ****\n");
		//
		//  Ende Aenderung Test
		//
		ci.Invoke (ss, new object [] { false });
		//
		//  Beginn Aenderung Test
		//
		Console.WriteLine("\n\t****C#: NACH SimpleStruct ConstructorInfo.Invoke() ****\n");
		//
		//  Ende Aenderung Test
		//
		// Test invoking of the array Get/Set methods
		string[,] arr = new string [10, 10];
		//
		//  Beginn Aenderung Test
		//
		Console.WriteLine("\n\t****C#: VOR String-Array-Setter.Invoke() ****\n");
		//
		//  Ende Aenderung Test
		//
		arr.GetType ().GetMethod ("Set").Invoke (arr, new object [] { 1, 1, "FOO" });
		//
		//  Beginn Aenderung Test
		//
		Console.WriteLine("\n\t****C#: NACH String-Array-Setter.Invoke() ****\n");
		//
		//  Ende Aenderung Test
		//
		//
		//  Beginn Aenderung Test
		//
		Console.WriteLine("\n\t****C#: VOR String-Getter.Invoke() ****\n");
		//
		//  Ende Aenderung Test
		//
		string s = (string)arr.GetType ().GetMethod ("Get").Invoke (arr, new object [] { 1, 1 });
		if (s != "FOO")
			return 3;
		//
		//  Beginn Aenderung Test
		//
		Console.WriteLine("\n\t****C#: NACH String-Getter.Invoke() ****\n");
		//
		//  Ende Aenderung Test
		//
		//
		//  Beginn Aenderung Test
		//
		Console.WriteLine("\n\t****C#: VOR String-Constructor-Invoke ****\n");
		//
		//  Ende Aenderung Test
		//
		// Test the sharing of runtime invoke wrappers for string ctors
		typeof (string).GetConstructor (new Type [] { typeof (char[]) }).Invoke (null, new object [] { new char [] { 'a', 'b', 'c' } });
		//
		//  Beginn Aenderung Test
		//
		Console.WriteLine("\n\t****C#: NACH String-Constructor-Invoke ****\n");
		//
		//  Ende Aenderung Test
		//
		//
		//  Beginn Aenderung Test
		//
		Console.WriteLine("\n\t****C#: VOR Assembly-GetType-Invoke ****\n");
		//
		//  Ende Aenderung Test
		//
		typeof (Assembly).GetMethod ("GetType", new Type [] { typeof (string), }).Invoke (typeof (int).Assembly, new object [] { "A" });
		//
		//  Beginn Aenderung Test
		//
		Console.WriteLine("\n\t****C#: NACH Assembly-GetType-Invoke ****\n");
		//
		//  Ende Aenderung Test
		//
		return 0;
	}
}
