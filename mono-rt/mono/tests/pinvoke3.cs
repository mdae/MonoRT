//
// pinvoke3.cs:
//
//  Tests for native->managed marshalling
//

using System;
using System.Text;
using System.Runtime.InteropServices;

public class Tests {

	[StructLayout (LayoutKind.Sequential)]
	public struct SimpleStruct {
		public bool a;
		public bool b;
		public bool c;
		public string d;
		[MarshalAs(UnmanagedType.LPWStr)]
		public string d2;
	}

	[StructLayout (LayoutKind.Sequential)]
	public class SimpleClass {
		public bool a;
		public bool b;
		public bool c;
		public string d;
	}

	public static SimpleStruct delegate_test_struct (SimpleStruct ss)
	{
		SimpleStruct res;

		res.a = !ss.a;
		res.b = !ss.b;
		res.c = !ss.c;
		res.d = ss.d + "-RES";
		res.d2 = ss.d2 + "-RES";

		return res;
	}

	public static int delegate_test_struct_byref (int a, ref SimpleStruct ss, int b)
	{
		if (a == 1 && b == 2 && ss.a && !ss.b && ss.c && ss.d == "TEST2") {
			ss.a = true;
			ss.b = true;
			ss.c = true;
			ss.d = "TEST3";
			return 0;
		}

		return 1;
	}

	public static int delegate_test_struct_out (int a, out SimpleStruct ss, int b)
	{
		ss.a = true;
		ss.b = true;
		ss.c = true;
		ss.d = "TEST3";
		ss.d2 = "TEST4";

		return 0;
	}

	public static SimpleClass delegate_test_class (SimpleClass ss)
	{
		if (ss == null)
			return null;

		if (! (!ss.a && ss.b && !ss.c && ss.d == "TEST"))
			return null;

		SimpleClass res = ss;

		return res;
	}

	public static int delegate_test_class_byref (ref SimpleClass ss)
	{
		//
		//  Beginn Aenderung Test
		//
		Console.WriteLine("\n\t****C#: int delegate_test_class_byref (ref SimpleClass ss) ****\n");
		//
		//  Ende Aenderung Test
		//
		if (ss == null)
			return -1;

		if (!ss.a && ss.b && !ss.c && ss.d == "TEST") {
			ss.a = true;
			ss.b = false;
			ss.c = true;
			ss.d = "RES";

			return 0;
		}

		return 1;
	}

	public static int delegate_test_class_out (out SimpleClass ss)
	{
		ss = new SimpleClass ();
		ss.a = true;
		ss.b = false;
		ss.c = true;
		ss.d = "RES";

		return 0;
	}

	public static int delegate_test_primitive_byref (ref int i)
	{
		if (i != 1)
			return 1;
		
		i = 2;
		return 0;
	}

	public static int delegate_test_string_marshalling (string s)
	{
		return s == "ABC" ? 0 : 1;
	}

	public static int delegate_test_string_builder_marshalling (StringBuilder s)
	{
		if (s == null)
			return 2;
		else
			return s.ToString () == "ABC" ? 0 : 1;
	}

	[DllImport ("libtest", EntryPoint="mono_test_ref_vtype")]
	public static extern int mono_test_ref_vtype (int a, ref SimpleStruct ss, int b, TestDelegate d);

	public delegate int OutStructDelegate (int a, out SimpleStruct ss, int b);

	[DllImport ("libtest", EntryPoint="mono_test_marshal_out_struct")]
	public static extern int mono_test_marshal_out_struct (int a, out SimpleStruct ss, int b, OutStructDelegate d);
	
	[DllImport ("libtest", EntryPoint="mono_test_marshal_delegate2")]
	public static extern int mono_test_marshal_delegate2 (SimpleDelegate2 d);

	[DllImport ("libtest", EntryPoint="mono_test_marshal_delegate4")]
	public static extern int mono_test_marshal_delegate4 (SimpleDelegate4 d);

	[DllImport ("libtest", EntryPoint="mono_test_marshal_delegate5")]
	public static extern int mono_test_marshal_delegate5 (SimpleDelegate5 d);

	[DllImport ("libtest", EntryPoint="mono_test_marshal_delegate6")]
	public static extern int mono_test_marshal_delegate6 (SimpleDelegate5 d);

	[DllImport ("libtest", EntryPoint="mono_test_marshal_delegate7")]
	public static extern int mono_test_marshal_delegate7 (SimpleDelegate7 d);

	[DllImport ("libtest", EntryPoint="mono_test_marshal_delegate8", CharSet=CharSet.Unicode)]
	public static extern int mono_test_marshal_delegate8 (SimpleDelegate8 d, string s);

	[DllImport ("libtest", EntryPoint="mono_test_marshal_delegate9")]
	public static extern int mono_test_marshal_delegate9 (SimpleDelegate9 d, return_int_delegate d2);

	[DllImport ("libtest", EntryPoint="mono_test_marshal_delegate10")]
	public static extern int mono_test_marshal_delegate10 (SimpleDelegate9 d);

	[DllImport ("libtest", EntryPoint="mono_test_marshal_delegate8")]
	public static extern int mono_test_marshal_delegate11 (SimpleDelegate11 d, string s);

	[DllImport ("libtest", EntryPoint="mono_test_marshal_primitive_byref_delegate")]
	public static extern int mono_test_marshal_primitive_byref_delegate (PrimitiveByrefDelegate d);

	[DllImport ("libtest", EntryPoint="mono_test_marshal_return_delegate_delegate")]
	public static extern int mono_test_marshal_return_delegate_delegate (ReturnDelegateDelegate d);

	public delegate int TestDelegate (int a, ref SimpleStruct ss, int b);

	public delegate SimpleStruct SimpleDelegate2 (SimpleStruct ss);

	public delegate SimpleClass SimpleDelegate4 (SimpleClass ss);

	public delegate int SimpleDelegate5 (ref SimpleClass ss);

	public delegate int SimpleDelegate7 (out SimpleClass ss);

	public delegate int SimpleDelegate8 ([MarshalAs (UnmanagedType.LPWStr)] string s1);

	public delegate int return_int_delegate (int i);

	public delegate int SimpleDelegate9 (return_int_delegate del);

	public delegate int SimpleDelegate11 (StringBuilder s1);

	public delegate int PrimitiveByrefDelegate (ref int i);

	public delegate return_int_delegate ReturnDelegateDelegate ();

	public static int Main () {

		//return TestDriver.RunTests (typeof (Tests));
		//
		//  Beginn Aenderung Test
		//
		 int ret = test_0_marshal_byref_string_delegate();

		if (0 == ret) {
			Console.WriteLine("Pass");
			ret = 0;
		} else {
			Console.WriteLine("Fail");
			ret = 1;
		}
		//
		//  Ende Aenderung Test
		//
		return ret;
	}

	/* Test structures as arguments and return values of delegates */
/*	public static int test_0_marshal_struct_delegate () {
		//
		//  Beginn Aenderung Test
		//
		Console.WriteLine("\n\t****C#: test_0_marshal_struct_delegate ()****\n");
		//
		//  Ende Aenderung Test
		//
		SimpleDelegate2 d = new SimpleDelegate2 (delegate_test_struct);
		//
		//  Beginn Aenderung Test
		//
		Console.WriteLine("\n\t****C#: impleDelegate2 d = new SimpleDelegate2 (delegate_test_struct) done ****\n");
		//
		//  Ende Aenderung Test
		//
		return mono_test_marshal_delegate2 (d);
	}
*/
	/* Test structures as byref arguments of delegates */
/*	public static int test_0_marshal_byref_struct_delegate () {
		//
		//  Beginn Aenderung Test
		//
		Console.WriteLine("\n\t****C#: test_0_marshal_byref_struct_delegate ()****\n");
		//
		//  Ende Aenderung Test
		//
		SimpleStruct ss = new SimpleStruct ();
		TestDelegate d = new TestDelegate (delegate_test_struct_byref);
		//
		//  Beginn Aenderung Test
		//
		Console.WriteLine("\n\t****C#: TestDelegate d = new TestDelegate (delegate_test_struct_byref) done ****\n");
		//
		//  Ende Aenderung Test
		//		
		ss.b = true;
		ss.d = "TEST1";

		if (mono_test_ref_vtype (1, ref ss, 2, d) != 0)
			return 1;

		if (! (ss.a && ss.b && ss.c && ss.d == "TEST3"))
			return 2;
		
		return 0;
	}
*/
	/* Test structures as out arguments of delegates */
/*	public static int test_0_marshal_out_struct_delegate () {
		//
		//  Beginn Aenderung Test
		//
		Console.WriteLine("\n\t****C#: test_0_marshal_out_struct_delegate ()****\n");
		//
		//  Ende Aenderung Test
		//
		SimpleStruct ss = new SimpleStruct ();
		OutStructDelegate d = new OutStructDelegate (delegate_test_struct_out);
		//
		//  Beginn Aenderung Test
		//
		Console.WriteLine("\n\t****C#: OutStructDelegate d = new OutStructDelegate (delegate_test_struct_out) done ****\n");
		//
		//  Ende Aenderung Test
		//
		return mono_test_marshal_out_struct (1, out ss, 2, d);
	}
*/
	/* Test classes as arguments and return values of delegates */
/*	public static int test_0_marshal_class_delegate () {
		//
		//  Beginn Aenderung Test
		//
		Console.WriteLine("\n\t****C#: test_0_marshal_class_delegate ()****\n");
		//
		//  Ende Aenderung Test
		//
		SimpleDelegate4 d = new SimpleDelegate4 (delegate_test_class);
		//
		//  Beginn Aenderung Test
		//
		Console.WriteLine("\n\t****C#: SimpleDelegate4 d = new SimpleDelegate4 (delegate_test_class); done ****\n");
		//
		//  Ende Aenderung Test
		//
		return mono_test_marshal_delegate4 (d);
	}
*/
	/* Test classes as byref arguments of delegates */
/*	public static int test_0_marshal_byref_class_delegate () {
		//
		//  Beginn Aenderung Test
		//
		Console.WriteLine("\n\t****C#: test_0_marshal_byref_class_delegate ()****\n");
		//
		//  Ende Aenderung Test
		//
		SimpleDelegate5 d = new SimpleDelegate5 (delegate_test_class_byref);
		//
		//  Beginn Aenderung Test
		//
		Console.WriteLine("\n\t****C#: SimpleDelegate5 d = new SimpleDelegate5 (delegate_test_class_byref) done ****\n");
		//
		//  Ende Aenderung Test
		//
		return mono_test_marshal_delegate5 (d);
	}
*/
	/* Test classes as out arguments of delegates */
/*	public static int test_0_marshal_out_class_delegate () {
		//
		//  Beginn Aenderung Test
		//
		Console.WriteLine("\n\t****C#: test_0_marshal_out_class_delegate ()****\n");
		//
		//  Ende Aenderung Test
		//
		SimpleDelegate7 d = new SimpleDelegate7 (delegate_test_class_out);
		//
		//  Beginn Aenderung Test
		//
		Console.WriteLine("\n\t****C#: SimpleDelegate7 d = new SimpleDelegate7 (delegate_test_class_out) done ****\n");
		//
		//  Ende Aenderung Test
		//
		return mono_test_marshal_delegate7 (d);
	}
*/
	/* Test string marshalling with delegates */
/*	public static int test_0_marshal_string_delegate () {
		//
		//  Beginn Aenderung Test
		//
		Console.WriteLine("\n\t****C#: test_0_marshal_string_delegate ()****\n");
		//
		//  Ende Aenderung Test
		//
		SimpleDelegate8 d = new SimpleDelegate8 (delegate_test_string_marshalling);
		//
		//  Beginn Aenderung Test
		//
		Console.WriteLine("\n\t****C#: SimpleDelegate8 d = new SimpleDelegate8 (delegate_test_string_marshalling) done ****\n");
		//
		//  Ende Aenderung Test
		//
		return mono_test_marshal_delegate8 (d, "ABC");
	}
*/
	/* Test string builder marshalling with delegates */
/*	public static int test_0_marshal_string_builder_delegate () {
		//
		//  Beginn Aenderung Test
		//
		Console.WriteLine("\n\t****C#: test_0_marshal_string_builder_delegate ()****\n");
		//
		//  Ende Aenderung Test
		//
		SimpleDelegate11 d = new SimpleDelegate11 (delegate_test_string_builder_marshalling);
		//
		//  Beginn Aenderung Test
		//
		Console.WriteLine("\n\t****C#: SimpleDelegate11 d = new SimpleDelegate11 (delegate_test_string_builder_marshalling) done ****\n");
		//
		//  Ende Aenderung Test
		//
		if (mono_test_marshal_delegate11 (d, null) != 2)
			return 2;

		return mono_test_marshal_delegate11 (d, "ABC");
	}
*/
	/* Test that the delegate wrapper correctly catches null byref arguments */
/*	public static int test_0_marshal_byref_class_delegate_null () {
		//
		//  Beginn Aenderung Test
		//
		Console.WriteLine("\n\t****C#: test_0_marshal_byref_class_delegate_null ()****\n");
		//
		//  Ende Aenderung Test
		//
		SimpleDelegate5 d = new SimpleDelegate5 (delegate_test_class_byref);
		//
		//  Beginn Aenderung Test
		//
		Console.WriteLine("\n\t****C#: SimpleDelegate5 d = new SimpleDelegate5 (delegate_test_class_byref) done ****\n");
		//
		//  Ende Aenderung Test
		//
		try {
			mono_test_marshal_delegate6 (d);
			return 1;
		}
		catch (ArgumentNullException ex) {
			return 0;
		}
	}
*/
	static int return_self (int i) {
		return i;
	}

	static int call_int_delegate (return_int_delegate d) {
		return d (55);
	}

/*	public static int test_55_marshal_delegate_delegate () {
		//
		//  Beginn Aenderung Test
		//
		Console.WriteLine("\n\t****C#: test_55_marshal_delegate_delegate ()****\n");
		//
		//  Ende Aenderung Test
		//
		SimpleDelegate9 d = new SimpleDelegate9 (call_int_delegate);
		//
		//  Beginn Aenderung Test
		//
		Console.WriteLine("\n\t****C#: SimpleDelegate9 d = new SimpleDelegate9 (call_int_delegate) done ****\n");
		//
		//  Ende Aenderung Test
		//
		return mono_test_marshal_delegate9 (d, new return_int_delegate (return_self));
	}
*/
/*	public static int test_0_marshal_primitive_byref_delegate () {
		//
		//  Beginn Aenderung Test
		//
		Console.WriteLine("\n\t****C#: test_0_marshal_primitive_byref_delegate ()****\n");
		//
		//  Ende Aenderung Test
		//
		PrimitiveByrefDelegate d = new PrimitiveByrefDelegate (delegate_test_primitive_byref);
		//
		//  Beginn Aenderung Test
		//
		Console.WriteLine("\n\t****C#: PrimitiveByrefDelegate d = new PrimitiveByrefDelegate (delegate_test_primitive_byref) done ****\n");
		//
		//  Ende Aenderung Test
		//
		return mono_test_marshal_primitive_byref_delegate (d);
	}
*/
/*	public static return_int_delegate return_delegate () {
		//
		//  Beginn Aenderung Test
		//
		Console.WriteLine("\n\t****C#: return_delegate ()****\n");
		//
		//  Ende Aenderung Test
		//
		return new return_int_delegate (return_self);
	}

	public static int test_55_marshal_return_delegate_delegate () {
		//
		//  Beginn Aenderung Test
		//
		Console.WriteLine("\n\t****C#: test_55_marshal_return_delegate_delegate ()****\n");
		//
		//  Ende Aenderung Test
		//
		return mono_test_marshal_return_delegate_delegate (new ReturnDelegateDelegate (return_delegate));
	}
*/
	/* Passing and returning strings */

/*	public delegate String ReturnStringDelegate (String s);

	[DllImport ("libtest", EntryPoint="mono_test_return_string")]
	public static extern String mono_test_marshal_return_string_delegate (ReturnStringDelegate d);

	public static String managed_return_string (String s) {
		if (s != "TEST")
			return "";
		else
			return "12345";
	}

	public static int test_0_marshal_return_string_delegate () {
		//
		//  Beginn Aenderung Test
		//
		Console.WriteLine("\n\t****C#: test_0_marshal_return_string_delegate ()****\n");
		//
		//  Ende Aenderung Test
		//
		ReturnStringDelegate d = new ReturnStringDelegate (managed_return_string);
		//
		//  Beginn Aenderung Test
		//
		Console.WriteLine("\n\t****C#: ReturnStringDelegate d = new ReturnStringDelegate (managed_return_string) done ****\n");
		//
		//  Ende Aenderung Test
		//
		String s = mono_test_marshal_return_string_delegate (d);

		return (s == "12345") ? 0 : 1;
	}
*/
	/* Passing and returning enums */
/*
	public enum FooEnum {
		Foo1,
		Foo2,
		Foo3
	};

	public delegate FooEnum ReturnEnumDelegate (FooEnum e);

	[DllImport ("libtest", EntryPoint="mono_test_marshal_return_enum_delegate")]
	public static extern int mono_test_marshal_return_enum_delegate (ReturnEnumDelegate d);

	public static FooEnum managed_return_enum (FooEnum e) {
		return (FooEnum)((int)e + 1);
	}

	public static int test_0_marshal_return_enum_delegate () {
		ReturnEnumDelegate d = new ReturnEnumDelegate (managed_return_enum);
		//
		//  Beginn Aenderung Test
		//
		Console.WriteLine("\n\t****C#: ReturnEnumDelegate d = new ReturnEnumDelegate (managed_return_enum) done ****\n");
		//
		//  Ende Aenderung Test
		//
		FooEnum e = (FooEnum)mono_test_marshal_return_enum_delegate (d);
		//
		//  Beginn Aenderung Test
		//
		Console.WriteLine("\n\t****C#: NACH Aufruf PInvoke ****\n");
		//
		//  Ende Aenderung Test
		//
		return e == FooEnum.Foo3 ? 0 : 1;
	}
*/
	/* Passing and returning blittable structs */
/*
	[StructLayout (LayoutKind.Sequential)]
	public struct BlittableStruct {
		public int a, b, c;
		public long d;
	}

	public static BlittableStruct delegate_test_blittable_struct (BlittableStruct ss)
	{
		BlittableStruct res;

		res.a = -ss.a;
		res.b = -ss.b;
		res.c = -ss.c;
		res.d = -ss.d;

		return res;
	}

	public delegate BlittableStruct SimpleDelegate10 (BlittableStruct ss);

	[DllImport ("libtest", EntryPoint="mono_test_marshal_blittable_struct_delegate")]
	public static extern int mono_test_marshal_blittable_struct_delegate (SimpleDelegate10 d);

	public static int test_0_marshal_blittable_struct_delegate () {
		//
		//  Beginn Aenderung Test
		//
		Console.WriteLine("\n\t****C#: test_0_marshal_blittable_struct_delegate ()****\n");
	
		//return mono_test_marshal_blittable_struct_delegate (new SimpleDelegate10 (delegate_test_blittable_struct));
		SimpleDelegate10 d = new SimpleDelegate10 (delegate_test_blittable_struct);
		Console.WriteLine("\n\t****C#: SimpleDelegate10 d = new SimpleDelegate10 (delegate_test_blittable_struct) done ****\n");
		return mono_test_marshal_blittable_struct_delegate (d);
		//
		//  Ende Aenderung Test
		//
	}
*/	/*
	 * Passing and returning small structs
	 */

	/* TEST 1: 4 byte long INTEGER struct */
/*
	[StructLayout (LayoutKind.Sequential)]
	public struct SmallStruct1 {
		public int i;
	}

	public static SmallStruct1 delegate_test_struct (SmallStruct1 ss) {
		SmallStruct1 res;

		res.i = -ss.i;
		//
		//  Beginn Aenderung Test
		//
		Console.WriteLine("\n\t****C#: SmallStruct1 delegate_test_struct (SmallStruct1)****\n");
		//
		//  Ende Aenderung Test
		//
		return res;
	}

	public delegate SmallStruct1 SmallStructDelegate1 (SmallStruct1 ss);

	[DllImport ("libtest", EntryPoint="mono_test_marshal_small_struct_delegate1")]
	public static extern int mono_test_marshal_small_struct_delegate (SmallStructDelegate1 d);

	public static int test_0_marshal_small_struct_delegate1 () {
		//
		//  Beginn Aenderung Test
		//
		Console.WriteLine("\n\t****C#: test_0_marshal_small_struct_delegate1 ()****\n");

		//return mono_test_marshal_small_struct_delegate (new SmallStructDelegate1 (delegate_test_struct));
		SmallStructDelegate1 d = new SmallStructDelegate1 (delegate_test_struct);
		Console.WriteLine("\n\t****C#: SmallStructDelegate d = new SmallStructDelegate1 (delegate_test_struct) done ****\n");
		return mono_test_marshal_small_struct_delegate (d);
		//
		//  Ende Aenderung Test
		//
	}
*/
	/* TEST 2: 2+2 byte long INTEGER struct */
/*
	[StructLayout (LayoutKind.Sequential)]
	public struct SmallStruct2 {
		public short i, j;
	}

	public static SmallStruct2 delegate_test_struct (SmallStruct2 ss) {
		SmallStruct2 res;

		res.i = (short)-ss.i;
		res.j = (short)-ss.j;
		//
		//  Beginn Aenderung Test
		//
		Console.WriteLine("\n\t****C#: SmallStruct2 delegate_test_struct (SmallStruct2)****\n");
		//
		//  Ende Aenderung Test
		//
		return res;
	}

	public delegate SmallStruct2 SmallStructDelegate2 (SmallStruct2 ss);

	[DllImport ("libtest", EntryPoint="mono_test_marshal_small_struct_delegate2")]
	public static extern int mono_test_marshal_small_struct_delegate (SmallStructDelegate2 d);

	public static int test_0_marshal_small_struct_delegate2 () {
		//
		//  Beginn Aenderung Test
		//
		Console.WriteLine("\n\t****C#: test_0_marshal_small_struct_delegate2 ()****\n");

		//return mono_test_marshal_small_struct_delegate (new SmallStructDelegate2 (delegate_test_struct));
		SmallStructDelegate2 d = new SmallStructDelegate2 (delegate_test_struct);
		Console.WriteLine("\n\t****C#: SSmallStructDelegate2 d = new SmallStructDelegate2 (delegate_test_struct) done ****\n");
		return mono_test_marshal_small_struct_delegate (d);
		//
		//  Ende Aenderung Test
		//		
	}
*/
	/* TEST 3: 2+1 byte long INTEGER struct */
/*
	[StructLayout (LayoutKind.Sequential)]
	public struct SmallStruct3 {
		public short i;
		public byte j;
	}

	public static SmallStruct3 delegate_test_struct (SmallStruct3 ss) {
		SmallStruct3 res;

		res.i = (short)-ss.i;
		res.j = (byte)-ss.j;
		//
		//  Beginn Aenderung Test
		//
		Console.WriteLine("\n\t****C#: SmallStruct3 delegate_test_struct (SmallStruct3)****\n");
		//
		//  Ende Aenderung Test
		//
		return res;
	}

	public delegate SmallStruct3 SmallStructDelegate3 (SmallStruct3 ss);

	[DllImport ("libtest", EntryPoint="mono_test_marshal_small_struct_delegate3")]
	public static extern int mono_test_marshal_small_struct_delegate (SmallStructDelegate3 d);

	public static int test_0_marshal_small_struct_delegate3 () {
		//
		//  Beginn Aenderung Test
		//
		Console.WriteLine("\n\t****C#: test_0_marshal_small_struct_delegate3 ()****\n");

		//return mono_test_marshal_small_struct_delegate (new SmallStructDelegate3 (delegate_test_struct));
		SmallStructDelegate3 d = new SmallStructDelegate3 (delegate_test_struct);
		Console.WriteLine("\n\t****C#: SmallStructDelegate3 d = new SmallStructDelegate3 (delegate_test_struct) done ****\n");
		return mono_test_marshal_small_struct_delegate (d);
		//
		//  Ende Aenderung Test
		//		
	}
*/
	/* TEST 4: 2 byte long INTEGER struct */
/*
	[StructLayout (LayoutKind.Sequential)]
	public struct SmallStruct4 {
		public short i;
	}

	public static SmallStruct4 delegate_test_struct (SmallStruct4 ss) {
		SmallStruct4 res;

		res.i = (short)-ss.i;
		//
		//  Beginn Aenderung Test
		//
		Console.WriteLine("\n\t****C#: SmallStruct4 delegate_test_struct (SmallStruct4)****\n");
		//
		//  Ende Aenderung Test
		//		
		return res;
	}

	public delegate SmallStruct4 SmallStructDelegate4 (SmallStruct4 ss);

	[DllImport ("libtest", EntryPoint="mono_test_marshal_small_struct_delegate4")]
	public static extern int mono_test_marshal_small_struct_delegate (SmallStructDelegate4 d);

	public static int test_0_marshal_small_struct_delegate4 () {
		//
		//  Beginn Aenderung Test
		//
		Console.WriteLine("\n\t****C#: test_0_marshal_small_struct_delegate4 ()****\n");

		//return mono_test_marshal_small_struct_delegate (new SmallStructDelegate4 (delegate_test_struct));
		SmallStructDelegate4 d = new SmallStructDelegate4 (delegate_test_struct);
		Console.WriteLine("\n\t****C#: SmallStructDelegate4 d = new SmallStructDelegate4 (delegate_test_struct) done ****\n");
		return mono_test_marshal_small_struct_delegate (d);
		//
		//  Ende Aenderung Test
		//		
	}
*/
	/* TEST 5: 8 byte long INTEGER struct */
/*
	[StructLayout (LayoutKind.Sequential)]
	public struct SmallStruct5 {
		public long l;
	}

	public static SmallStruct5 delegate_test_struct (SmallStruct5 ss) {
		SmallStruct5 res;

		res.l = -ss.l;
		//
		//  Beginn Aenderung Test
		//
		Console.WriteLine("\n\t****C#: SmallStruct5 delegate_test_struct (SmallStruct5)****\n");
		//
		//  Ende Aenderung Test
		//		
		return res;
	}

	public delegate SmallStruct5 SmallStructDelegate5 (SmallStruct5 ss);

	[DllImport ("libtest", EntryPoint="mono_test_marshal_small_struct_delegate5")]
	public static extern int mono_test_marshal_small_struct_delegate (SmallStructDelegate5 d);

	public static int test_0_marshal_small_struct_delegate5 () {
		//
		//  Beginn Aenderung Test
		//
		Console.WriteLine("\n\t****C#: test_0_marshal_small_struct_delegate5 ()****\n");

		//return mono_test_marshal_small_struct_delegate (new SmallStructDelegate5 (delegate_test_struct));
		SmallStructDelegate5 d = new SmallStructDelegate5 (delegate_test_struct);
		Console.WriteLine("\n\t****C#: SmallStructDelegate5 d = new SmallStructDelegate5 (delegate_test_struct) done ****\n");
		return mono_test_marshal_small_struct_delegate (d);
		//
		//  Ende Aenderung Test
		//		
	}
*/
	/* TEST 6: 4+4 byte long INTEGER struct */
/*
	[StructLayout (LayoutKind.Sequential)]
	public struct SmallStruct6 {
		public int i, j;
	}

	public static SmallStruct6 delegate_test_struct (SmallStruct6 ss) {
		SmallStruct6 res;

		res.i = -ss.i;
		res.j = -ss.j;
		//
		//  Beginn Aenderung Test
		//
		Console.WriteLine("\n\t****C#: SmallStruct6 delegate_test_struct (SmallStruct6) ****\n");
		//
		//  Ende Aenderung Test
		//		
		return res;
	}

	public delegate SmallStruct6 SmallStructDelegate6 (SmallStruct6 ss);

	[DllImport ("libtest", EntryPoint="mono_test_marshal_small_struct_delegate6")]
	public static extern int mono_test_marshal_small_struct_delegate (SmallStructDelegate6 d);

	public static int test_0_marshal_small_struct_delegate6 () {
		//
		//  Beginn Aenderung Test
		//
		Console.WriteLine("\n\t****C#: test_0_marshal_small_struct_delegate6 ()****\n");

		//return mono_test_marshal_small_struct_delegate (new SmallStructDelegate6 (delegate_test_struct));
		SmallStructDelegate6 d = new SmallStructDelegate6 (delegate_test_struct);
		Console.WriteLine("\n\t****C#: SmallStructDelegate6 d = new SmallStructDelegate6 (delegate_test_struct) done ****\n");
		return mono_test_marshal_small_struct_delegate (d);
		//
		//  Ende Aenderung Test
		//
	}
*/
	/* TEST 7: 4+2 byte long INTEGER struct */
/*
	[StructLayout (LayoutKind.Sequential)]
	public struct SmallStruct7 {
		public int i;
		public short j;
	}

	public static SmallStruct7 delegate_test_struct (SmallStruct7 ss) {
		SmallStruct7 res;

		res.i = -ss.i;
		res.j = (short)-ss.j;
		//
		//  Beginn Aenderung Test
		//
		Console.WriteLine("\n\t****C#: SmallStruct7 delegate_test_struct (SmallStruct7) ****\n");
		//
		//  Ende Aenderung Test
		//		
		return res;
	}

	public delegate SmallStruct7 SmallStructDelegate7 (SmallStruct7 ss);

	[DllImport ("libtest", EntryPoint="mono_test_marshal_small_struct_delegate7")]
	public static extern int mono_test_marshal_small_struct_delegate (SmallStructDelegate7 d);

	public static int test_0_marshal_small_struct_delegate7 () {
		//
		//  Beginn Aenderung Test
		//
		Console.WriteLine("\n\t****C#: test_0_marshal_small_struct_delegate7 ()****\n");
		//
		//  Ende Aenderung Test
		//
		return mono_test_marshal_small_struct_delegate (new SmallStructDelegate7 (delegate_test_struct));
	}
*/
	/* TEST 8: 4 byte long FLOAT struct */
/*
	[StructLayout (LayoutKind.Sequential)]
	public struct SmallStruct8 {
		public float i;
	}

	public static SmallStruct8 delegate_test_struct (SmallStruct8 ss) {
		SmallStruct8 res;

		res.i = -ss.i;
		//
		//  Beginn Aenderung Test
		//
		Console.WriteLine("\n\t****C#: SmallStruct8 delegate_test_struct (SmallStruct8) ****\n");
		//
		//  Ende Aenderung Test
		//		
		return res;
	}

	public delegate SmallStruct8 SmallStructDelegate8 (SmallStruct8 ss);

	[DllImport ("libtest", EntryPoint="mono_test_marshal_small_struct_delegate8")]
	public static extern int mono_test_marshal_small_struct_delegate (SmallStructDelegate8 d);

	public static int test_0_marshal_small_struct_delegate8 () {
		//
		//  Beginn Aenderung Test
		//
		Console.WriteLine("\n\t****C#: test_0_marshal_small_struct_delegate8 ()****\n");
		//
		//  Ende Aenderung Test
		//
		return mono_test_marshal_small_struct_delegate (new SmallStructDelegate8 (delegate_test_struct));
	}
*/
	/* TEST 9: 8 byte long FLOAT struct */
/*
	[StructLayout (LayoutKind.Sequential)]
	public struct SmallStruct9 {
		public double i;
	}

	public static SmallStruct9 delegate_test_struct (SmallStruct9 ss) {
		SmallStruct9 res;

		res.i = -ss.i;
		//
		//  Beginn Aenderung Test
		//
		Console.WriteLine("\n\t****C#: SmallStruct9 delegate_test_struct (SmallStruct9) ****\n");
		//
		//  Ende Aenderung Test
		//		
		return res;
	}

	public delegate SmallStruct9 SmallStructDelegate9 (SmallStruct9 ss);

	[DllImport ("libtest", EntryPoint="mono_test_marshal_small_struct_delegate9")]
	public static extern int mono_test_marshal_small_struct_delegate (SmallStructDelegate9 d);

	public static int test_0_marshal_small_struct_delegate9 () {
		//
		//  Beginn Aenderung Test
		//
		Console.WriteLine("\n\t****C#: test_0_marshal_small_struct_delegate9 ()****\n");
		//
		//  Ende Aenderung Test
		//
		return mono_test_marshal_small_struct_delegate (new SmallStructDelegate9 (delegate_test_struct));
	}
*/
	/* TEST 10: 4+4 byte long FLOAT struct */
/*
	[StructLayout (LayoutKind.Sequential)]
	public struct SmallStruct10 {
		public float i;
		public float j;
	}

	public static SmallStruct10 delegate_test_struct (SmallStruct10 ss) {
		SmallStruct10 res;

		res.i = -ss.i;
		res.j = -ss.j;
		//
		//  Beginn Aenderung Test
		//
		Console.WriteLine("\n\t****C#: SmallStruct10 delegate_test_struct (SmallStruct10) ****\n");
		//
		//  Ende Aenderung Test
		//		
		return res;
	}

	public delegate SmallStruct10 SmallStructDelegate10 (SmallStruct10 ss);

	[DllImport ("libtest", EntryPoint="mono_test_marshal_small_struct_delegate10")]
	public static extern int mono_test_marshal_small_struct_delegate (SmallStructDelegate10 d);

	public static int test_0_marshal_small_struct_delegate10 () {
		//
		//  Beginn Aenderung Test
		//
		Console.WriteLine("\n\t****C#: test_0_marshal_small_struct_delegate10 ()****\n");
		//
		//  Ende Aenderung Test
		//
		return mono_test_marshal_small_struct_delegate (new SmallStructDelegate10 (delegate_test_struct));
	}
*/
	/* TEST 11: 4+4 byte long MIXED struct */
/*
	[StructLayout (LayoutKind.Sequential)]
	public struct SmallStruct11 {
		public float i;
		public int j;
	}

	public static SmallStruct11 delegate_test_struct (SmallStruct11 ss) {
		SmallStruct11 res;

		res.i = -ss.i;
		res.j = -ss.j;
		//
		//  Beginn Aenderung Test
		//
		Console.WriteLine("\n\t****C#: SmallStruct11 delegate_test_struct (SmallStruct11) ****\n");
		//
		//  Ende Aenderung Test
		//		
		return res;
	}

	public delegate SmallStruct11 SmallStructDelegate11 (SmallStruct11 ss);

	[DllImport ("libtest", EntryPoint="mono_test_marshal_small_struct_delegate11")]
	public static extern int mono_test_marshal_small_struct_delegate (SmallStructDelegate11 d);

	public static int test_0_marshal_small_struct_delegate11 () {
		//
		//  Beginn Aenderung Test
		//
		Console.WriteLine("\n\t****C#: test_0_marshal_small_struct_delegate11 ()****\n");
		//
		//  Ende Aenderung Test
		//
		return mono_test_marshal_small_struct_delegate (new SmallStructDelegate11 (delegate_test_struct));
	}
*/
	/*
	 * Passing arrays
	 */
	public delegate int ArrayDelegate1 (int i, string j, [In, MarshalAs(UnmanagedType.LPArray, ArraySubType=UnmanagedType.LPStr, SizeParamIndex=0)] string[] arr);

	[DllImport ("libtest", EntryPoint="mono_test_marshal_array_delegate")]
	public static extern int mono_test_marshal_array_delegate1 (string[] arr, int len, ArrayDelegate1 d);

	public static int array_delegate1 (int i, string j, string[] arr) {
		if (arr.Length != 2)
			return 1;
		if ((arr [0] != "ABC") || (arr [1] != "DEF"))
			return 2;
		//
		//  Beginn Aenderung Test
		//
		Console.WriteLine("\n\t****C#: int array_delegate1 (int i, string j, string[] arr) ****\n");
		//
		//  Ende Aenderung Test
		//
		return 0;
	}
/*
	public static int test_0_marshal_array_delegate_string () {	
		string[] arr = new string [] { "ABC", "DEF" };
		//
		//  Beginn Aenderung Test
		//
		Console.WriteLine("\n\t****C#: test_0_marshal_array_delegate_string ()****\n");

		ArrayDelegate1 d = new ArrayDelegate1 (array_delegate1);
		Console.WriteLine("\n\t****C#: ArrayDelegate1 d = new ArrayDelegate1 (array_delegate1) done ****\n");
		return mono_test_marshal_array_delegate1 (arr, arr.Length, d);
		//
		//  Ende Aenderung Test
		//
	}
*/
	public static int array_delegate2 (int i, string j, string[] arr) {
		//
		//  Beginn Aenderung Test
		//
		Console.WriteLine("\n\t****C#: int array_delegate2 (int i, string j, string[] arr) ****\n");
		//
		//  Ende Aenderung Test
		//
		return (arr == null) ? 0 : 1;
	}
/*
	public static int test_0_marshal_array_delegate_null () {
		//
		//  Beginn Aenderung Test
		//
		Console.WriteLine("\n\t****C#: test_0_marshal_array_delegate_null ()****\n");

		//return mono_test_marshal_array_delegate1 (null, 0, new ArrayDelegate1 (array_delegate2));
		ArrayDelegate1 d = new ArrayDelegate1 (array_delegate2);
		Console.WriteLine("\n\t****C#: ArrayDelegate1 d = new ArrayDelegate1 (array_delegate2) done ****\n");
		return mono_test_marshal_array_delegate1 (null, 0, d);
		//
		//  Ende Aenderung Test
		//	
	}
*/
	public delegate int ArrayDelegate3 (int i, string j, [In, MarshalAs(UnmanagedType.LPArray, ArraySubType=UnmanagedType.LPStr, SizeParamIndex=3)] string[] arr);

	[DllImport ("libtest", EntryPoint="mono_test_marshal_array_delegate")]
	public static extern int mono_test_marshal_array_delegate3 (string[] arr, int len, ArrayDelegate3 d);

	public static int array_delegate3 (int i, string j, string[] arr) {
		//
		//  Beginn Aenderung Test
		//
		Console.WriteLine("\n\t****C#: int array_delegate3 (int i, string j, string[] arr) ****\n");
		//
		//  Ende Aenderung Test
		//
		return (arr == null) ? 0 : 1;
	}

/*	public static int test_0_marshal_array_delegate_bad_paramindex () {
		//
		//  Beginn Aenderung Test
		//
		Console.WriteLine("\n\t****C#: test_0_marshal_array_delegate_bad_paramindex ()****\n");
		//
		//  Ende Aenderung Test
		//
		try {
			//
			//  Beginn Aenderung Test
			//
			ArrayDelegate3 d = new ArrayDelegate3 (array_delegate3);
			Console.WriteLine("\n\t****C#: ArrayDelegate3 d = new ArrayDelegate3 (array_delegate3) done ****\n");
			mono_test_marshal_array_delegate3 (null, 0, d);
			//
			//  Ende Aenderung Test
			//
			return 1;
		}
		catch (MarshalDirectiveException) {
			return 0;
		}
	}
*/
	public delegate int ArrayDelegate4 (int i, string j, [In, MarshalAs(UnmanagedType.LPArray, ArraySubType=UnmanagedType.LPStr, SizeParamIndex=1)] string[] arr);

	[DllImport ("libtest", EntryPoint="mono_test_marshal_array_delegate")]
	public static extern int mono_test_marshal_array_delegate4 (string[] arr, int len, ArrayDelegate4 d);

	public static int array_delegate4 (int i, string j, string[] arr) {
		//
		//  Beginn Aenderung Test
		//
		Console.WriteLine("\n\t****C#: int array_delegate4 (int i, string j, string[] arr) ****\n");
		//
		//  Ende Aenderung Test
		//
		return (arr == null) ? 0 : 1;
	}

/*	public static int test_0_marshal_array_delegate_bad_paramtype () {
		//
		//  Beginn Aenderung Test
		//
		Console.WriteLine("\n\t****C#: test_0_marshal_array_delegate_bad_paramtype ()****\n");
		//
		//  Ende Aenderung Test
		//
		try {
			//
			//  Beginn Aenderung Test
			//
			//mono_test_marshal_array_delegate4 (null, 0, new ArrayDelegate4 (array_delegate4));
			ArrayDelegate4 d = new ArrayDelegate4 (array_delegate4);
			Console.WriteLine("\n\t****C#: ArrayDelegate4 d = new ArrayDelegate4 (array_delegate4) done ****\n");
			mono_test_marshal_array_delegate4 (null, 0, d);
			//
			//  Ende Aenderung Test
			//
			return 1;
		}
		catch (MarshalDirectiveException) {
			return 0;
		}
	}
*/
	public delegate int ArrayDelegate4_2 (int i, string j, string[] arr);

	[DllImport ("libtest", EntryPoint="mono_test_marshal_array_delegate")]
	public static extern int mono_test_marshal_array_delegate4_2 (string[] arr, int len, ArrayDelegate4_2 d);

	public static int array_delegate4_2 (int i, string j, string[] arr) {
		//
		//  Beginn Aenderung Test
		//
		Console.WriteLine("\n\t****C#: int array_delegate4_2 (int i, string j, string[] arr) ****\n");
		//
		//  Ende Aenderung Test
		//
		return (arr == null) ? 0 : 1;
	}

/*	public static int test_0_marshal_array_delegate_no_marshal_directive () {
		//
		//  Beginn Aenderung Test
		//
		Console.WriteLine("\n\t****C#: test_0_marshal_array_delegate_no_marshal_directive ()****\n");
		//
		//  Ende Aenderung Test
		//
		try {
			//
			//  Beginn Aenderung Test
			//
			ArrayDelegate4_2 d = new ArrayDelegate4_2 (array_delegate4_2);
			Console.WriteLine("\n\t****C#: ArrayDelegate4_2 d = new ArrayDelegate4_2 (array_delegate4_2) done ****\n");
			mono_test_marshal_array_delegate4_2 (null, 0, d);
			//
			//  Ende Aenderung Test
			//
			return 1;
		}
		catch (MarshalDirectiveException) {
			return 0;
		}
	}
*/
	public delegate int ArrayDelegate4_3 (int i, string j, string[] arr);

	[DllImport ("libtest", EntryPoint="mono_test_marshal_array_delegate")]
	public static extern int mono_test_marshal_array_delegate4_3 (string[] arr, int len, ArrayDelegate4_3 d);

	public int array_delegate4_3 (int i, string j, string[] arr) {
		//
		//  Beginn Aenderung Test
		//
		Console.WriteLine("\n\t****C#: int array_delegate4_3 (int i, string j, string[] arr) ****\n");
		//
		//  Ende Aenderung Test
		//
		return (arr == null) ? 0 : 1;
	}

/*	public static int test_0_marshal_array_delegate_no_marshal_directive_instance () {
		//
		//  Beginn Aenderung Test
		//
		Console.WriteLine("\n\t****C#: test_0_marshal_array_delegate_no_marshal_directive_instance ()****\n");
		//
		//  Ende Aenderung Test
		//
		try {
			Tests t = new Tests ();
			//
			//  Beginn Aenderung Test
			//
			ArrayDelegate4_3 d = new ArrayDelegate4_3 (t.array_delegate4_3);
			Console.WriteLine("\n\t****C#: ArrayDelegate4_3 d = new ArrayDelegate4_3 (t.array_delegate4_3) done ****\n");
			mono_test_marshal_array_delegate4_3 (null, 0, d);
			//
			//  Ende Aenderung Test
			//
			//mono_test_marshal_array_delegate4_3 (null, 0, new ArrayDelegate4_3 (t.array_delegate4_3));
			return 1;
		}
		catch (MarshalDirectiveException) {
			return 0;
		}
	}
*/
	public delegate int ArrayDelegate5 (int i, string j, [In, MarshalAs(UnmanagedType.LPArray, ArraySubType=UnmanagedType.LPWStr, SizeParamIndex=0)] string[] arr);

	[DllImport ("libtest", EntryPoint="mono_test_marshal_array_delegate", CharSet=CharSet.Unicode)]
	public static extern int mono_test_marshal_array_delegate5 (string[] arr, int len, ArrayDelegate5 d);

	public static int array_delegate5 (int i, string j, string[] arr) {
		if (arr.Length != 2)
			return 1;
		if ((arr [0] != "ABC") || (arr [1] != "DEF"))
			return 2;
		//
		//  Beginn Aenderung Test
		//
		Console.WriteLine("\n\t****C#: int array_delegate5 (int i, string j, string[] arr) ****\n");
		//
		//  Ende Aenderung Test
		//
		return 0;
	}

/*	public static int test_0_marshal_array_delegate_unicode_string () {
		//
		//  Beginn Aenderung Test
		//
		Console.WriteLine("\n\t****C#: test_0_marshal_array_delegate_unicode_string ()****\n");
		//
		//  Ende Aenderung Test
		//
		string[] arr = new string [] { "ABC", "DEF" };
		//
		//  Beginn Aenderung Test
		//
		//  return mono_test_marshal_array_delegate5 (arr, arr.Length, new ArrayDelegate5 (array_delegate5));
		ArrayDelegate5 d = new ArrayDelegate5 (array_delegate5);
		Console.WriteLine("\n\t****C#: ArrayDelegate5 d = new ArrayDelegate5 (array_delegate5) done ****\n");
		return mono_test_marshal_array_delegate5 (arr, arr.Length, d);
		//
		//  Ende Aenderung Test
		//
	}
*/
	public delegate int ArrayDelegate6 (int i, string j, [In, MarshalAs(UnmanagedType.LPArray, ArraySubType=UnmanagedType.LPStr, SizeConst=2)] string[] arr);

	[DllImport ("libtest", EntryPoint="mono_test_marshal_array_delegate")]
	public static extern int mono_test_marshal_array_delegate6 (string[] arr, int len, ArrayDelegate6 d);

	public static int array_delegate6 (int i, string j, string[] arr) {
		if (arr.Length != 2)
			return 1;
		if ((arr [0] != "ABC") || (arr [1] != "DEF"))
			return 2;
		//
		//  Beginn Aenderung Test
		//
		Console.WriteLine("\n\t****C#: int array_delegate6 (int i, string j, string[] arr) ****\n");
		//
		//  Ende Aenderung Test
		//
		return 0;
	}
/*
	public static int test_0_marshal_array_delegate_sizeconst () {
		//
		//  Beginn Aenderung Test
		//
		Console.WriteLine("\n\t****C#: test_0_marshal_array_delegate_sizeconst ()****\n");
		//
		//  Ende Aenderung Test
		//
		string[] arr = new string [] { "ABC", "DEF" };
		//
		//  Beginn Aenderung Test
		//
		ArrayDelegate6 d = new ArrayDelegate6 (array_delegate6);
		Console.WriteLine("\n\t****C#: ArrayDelegate6 d = new ArrayDelegate6 (array_delegate6) done ****\n");
		return mono_test_marshal_array_delegate6 (arr, 1024, d);
		//return mono_test_marshal_array_delegate6 (arr, 1024, new ArrayDelegate6 (array_delegate6));
		//
		//  Ende Aenderung Test
		//
	}
*/
/*	public delegate int ArrayDelegate7 (int i, string j, [In, MarshalAs(UnmanagedType.LPArray, ArraySubType=UnmanagedType.LPStr, SizeConst=1, SizeParamIndex=0)] string[] arr);

	[DllImport ("libtest", EntryPoint="mono_test_marshal_array_delegate")]
	public static extern int mono_test_marshal_array_delegate7 (string[] arr, int len, ArrayDelegate7 d);

	public static int array_delegate7 (int i, string j, string[] arr) {
		if (arr.Length != 2)
			return 1;
		if ((arr [0] != "ABC") || (arr [1] != "DEF"))
			return 2;
		//
		//  Beginn Aenderung Test
		//
		Console.WriteLine("\n\t****C#: int array_delegate7 (int i, string j, string[] arr) ****\n");
		//
		//  Ende Aenderung Test
		//
		return 0;
	}

	public static int test_0_marshal_array_delegate_sizeconst_paramindex () {
		//
		//  Beginn Aenderung Test
		//
		Console.WriteLine("\n\t****C#: test_0_marshal_array_delegate_sizeconst_paramindex ()****\n");
		//
		//  Ende Aenderung Test
		//
		string[] arr = new string [] { "ABC", "DEF" };
		//
		//  Beginn Aenderung Test
		//
		ArrayDelegate7 d = new ArrayDelegate7 (array_delegate7);
		Console.WriteLine("\n\t****C#: ArrayDelegate7 d = new ArrayDelegate7 (array_delegate7) done ****\n");
		return mono_test_marshal_array_delegate7 (arr, 1, d);
		//return mono_test_marshal_array_delegate7 (arr, 1, new ArrayDelegate7 (array_delegate7));
		//
		//  Ende Aenderung Test
		//
	}
*/
/*	public delegate int ArrayDelegate8 (int i, string j, [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex=0)] int[] arr);

	[DllImport ("libtest", EntryPoint="mono_test_marshal_array_delegate")]
	public static extern int mono_test_marshal_array_delegate8 (int[] arr, int len, ArrayDelegate8 d);

	public static int array_delegate8 (int i, string j, int[] arr) {
		if (arr.Length != 2)
			return 1;
		if ((arr [0] != 42) || (arr [1] != 43))
			return 2;
		//
		//  Beginn Aenderung Test
		//
		Console.WriteLine("\n\t****C#: int array_delegate8 (int i, string j, int[] arr) ****\n");
		//
		//  Ende Aenderung Test
		//
		return 0;
	}

	public static int test_0_marshal_array_delegate_blittable () {
		//
		//  Beginn Aenderung Test
		//
		Console.WriteLine("\n\t****C#: test_0_marshal_array_delegate_blittable ()****\n");
		//
		//  Ende Aenderung Test
		//
		int[] arr = new int [] { 42, 43 };
		return mono_test_marshal_array_delegate8 (arr, 2, new ArrayDelegate8 (array_delegate8));
	}
*/
	/* Array with size param of type long */
/*
	public delegate int ArrayDelegate8_2 (long i, string j, [In, MarshalAs(UnmanagedType.LPArray, ArraySubType=UnmanagedType.LPStr, SizeParamIndex=0)] string[] arr);

	[DllImport ("libtest", EntryPoint="mono_test_marshal_array_delegate_long")]
	public static extern int mono_test_marshal_array_delegate8_2 (string[] arr, long len, ArrayDelegate8_2 d);

	public static int array_delegate8_2 (long i, string j, string[] arr) {
		if (arr.Length != 2)
			return 1;
		if ((arr [0] != "ABC") || (arr [1] != "DEF"))
			return 2;
		//
		//  Beginn Aenderung Test
		//
		Console.WriteLine("\n\t****C#: int array_delegate8_2 (long i, string j, string[] arr) ****\n");
		//
		//  Ende Aenderung Test
		//
		return 0;
	}

	public static int test_0_marshal_array_delegate_long_param () {
		//
		//  Beginn Aenderung Test
		//
		Console.WriteLine("\n\t****C#: test_0_marshal_array_delegate_long_param ()****\n");
		//
		//  Ende Aenderung Test
		//
		string[] arr = new string [] { "ABC", "DEF" };
		return mono_test_marshal_array_delegate8_2 (arr, arr.Length, new ArrayDelegate8_2 (array_delegate8_2));
	}
*/
	/*
	 * [Out] blittable arrays
	 */
/*
	public delegate int ArrayDelegate9 (int i, string j, [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex=0)] int[] arr);

	[DllImport ("libtest", EntryPoint="mono_test_marshal_out_array_delegate")]
	public static extern int mono_test_marshal_out_array_delegate (int[] arr, int len, ArrayDelegate9 d);

	public static int array_delegate9 (int i, string j, int[] arr) {
		if (arr.Length != 2)
			return 1;

		arr [0] = 1;
		arr [1] = 2;
		//
		//  Beginn Aenderung Test
		//
		Console.WriteLine("\n\t****C#: int array_delegate9 (int i, string j, int[] arr) ****\n");
		//
		//  Ende Aenderung Test
		//
		return 0;
	}

	public static int test_0_marshal_out_array_delegate () {
		//
		//  Beginn Aenderung Test
		//
		Console.WriteLine("\n\t****C#: test_0_marshal_out_array_delegate ()****\n");
		//
		//  Ende Aenderung Test
		//
		int[] arr = new int [] { 42, 43 };
		return mono_test_marshal_out_array_delegate (arr, 2, new ArrayDelegate9 (array_delegate9));
	}
*/
	/*
	 * [Out] string arrays
	 */
/*
	public delegate int ArrayDelegate10 (int i, string j, [Out, MarshalAs(UnmanagedType.LPArray, ArraySubType=UnmanagedType.LPStr, SizeConst=2)] string[] arr);

	[DllImport ("libtest", EntryPoint="mono_test_marshal_out_string_array_delegate")]
	public static extern int mono_test_marshal_out_string_array_delegate (string[] arr, int len, ArrayDelegate10 d);

	public static int array_delegate10 (int i, string j, string[] arr) {
		if (arr.Length != 2)
			return 1;

		arr [0] = "ABC";
		arr [1] = "DEF";
		//
		//  Beginn Aenderung Test
		//
		Console.WriteLine("\n\t****C#: int array_delegate10 (int i, string j, string[] arr) ****\n");
		//
		//  Ende Aenderung Test
		//
		return 0;
	}

	public static int test_0_marshal_out_string_array_delegate () {
		//
		//  Beginn Aenderung Test
		//
		Console.WriteLine("\n\t****C#: test_0_marshal_out_string_array_delegate ()****\n");
		//
		//  Ende Aenderung Test
		//
		string[] arr = new string [] { "", "" };
		return mono_test_marshal_out_string_array_delegate (arr, 2, new ArrayDelegate10 (array_delegate10));
	}
*/
	/*
	 * [In, Out] classes
	 */
/*
	public delegate int InOutByvalClassDelegate ([In, Out] SimpleClass ss);

	[DllImport ("libtest", EntryPoint="mono_test_marshal_inout_byval_class_delegate")]
	public static extern int mono_test_marshal_inout_byval_class_delegate (InOutByvalClassDelegate d);

	public static int delegate_test_byval_class_inout (SimpleClass ss) {
		if ((ss.a != false) || (ss.b != true) || (ss.c != false) || (ss.d != "FOO"))
			return 1;

		ss.a = true;
		ss.b = false;
		ss.c = true;
		ss.d = "RES";
		//
		//  Beginn Aenderung Test
		//
		Console.WriteLine("\n\t****C#: int delegate_test_byval_class_inout (SimpleClass ss) ****\n");
		//
		//  Ende Aenderung Test
		//
		return 0;
	}

	public static int test_0_marshal_inout_byval_class_delegate () {
		//
		//  Beginn Aenderung Test
		//
		Console.WriteLine("\n\t****C#: test_0_marshal_inout_byval_class_delegate ()****\n");
		//
		//  Ende Aenderung Test
		//
		return mono_test_marshal_inout_byval_class_delegate (new InOutByvalClassDelegate (delegate_test_byval_class_inout));
	}
*/
	/*
	 * Returning unicode strings
	 */
/*
	[return: MarshalAs(UnmanagedType.LPWStr)]
	public delegate string ReturnUnicodeStringDelegate([MarshalAs(UnmanagedType.LPWStr)] string message);

	[DllImport ("libtest", EntryPoint="mono_test_marshal_return_unicode_string_delegate")]
	public static extern int mono_test_marshal_return_unicode_string_delegate (ReturnUnicodeStringDelegate d);

	public static String return_unicode_string_delegate (string message) {
		//
		//  Beginn Aenderung Test
		//
		Console.WriteLine("\n\t****C#: String return_unicode_string_delegate (string message) ****\n");
		//
		//  Ende Aenderung Test
		//
		return message;
	}

	public static int test_0_marshal_return_unicode_string_delegate () {
		//
		//  Beginn Aenderung Test
		//
		Console.WriteLine("\n\t****C#: test_0_marshal_return_unicode_string_delegate ()****\n");
		//
		//  Ende Aenderung Test
		//
		return mono_test_marshal_return_unicode_string_delegate (new ReturnUnicodeStringDelegate (return_unicode_string_delegate));
	}
*/
	/*
	 * Returning string arrays
	 */
/*	public delegate string[] ReturnArrayDelegate (int i);

	[DllImport ("libtest", EntryPoint="mono_test_marshal_return_string_array_delegate")]
	public static extern int mono_test_marshal_return_string_array_delegate (ReturnArrayDelegate d);

	public static String[] return_array_delegate (int i) {
		String[] arr = new String [2];

		arr [0] = "ABC";
		arr [1] = "DEF";
		//
		//  Beginn Aenderung Test
		//
		Console.WriteLine("\n\t****C#: String[] return_array_delegate (int i) ****\n");
		//
		//  Ende Aenderung Test
		//
		return arr;
	}

	public static String[] return_array_delegate_null (int i) {
		//
		//  Beginn Aenderung Test
		//
		Console.WriteLine("\n\t****C#: String[] return_array_delegate_null (int i) ****\n");
		//
		//  Ende Aenderung Test
		//
		return null;
	}

	public static int test_0_marshal_return_string_array_delegate () {
		//
		//  Beginn Aenderung Test
		//
		Console.WriteLine("\n\t****C#: test_0_marshal_return_string_array_delegate ()****\n");
		//
		//  Ende Aenderung Test
		//
		return mono_test_marshal_return_string_array_delegate (new ReturnArrayDelegate (return_array_delegate));
	}

	public static int test_3_marshal_return_string_array_delegate_null () {
		//
		//  Beginn Aenderung Test
		//
		Console.WriteLine("\n\t****C#: test_3_marshal_return_string_array_delegate_null ()****\n");
		//
		//  Ende Aenderung Test
		//
		return mono_test_marshal_return_string_array_delegate (new ReturnArrayDelegate (return_array_delegate_null));
	}
*/
	/*
	 * Byref string marshalling
	 */
	public delegate int ByrefStringDelegate (ref string s);

	[DllImport ("libtest", EntryPoint="mono_test_marshal_byref_string_delegate")]
	public static extern int mono_test_marshal_byref_string_delegate (ByrefStringDelegate d);

	public static int byref_string_delegate (ref string s) {
		if (s != "ABC")
			return 1;

		s = "DEF";
		//
		//  Beginn Aenderung Test
		//
		Console.WriteLine("\n\t****C#: int byref_string_delegate (ref string s) ****\n");
		//
		//  Ende Aenderung Test
		//
		return 0;
	}

	public static int test_0_marshal_byref_string_delegate () {
		//
		//  Beginn Aenderung Test
		//
		Console.WriteLine("\n\t****C#: test_0_marshal_byref_string_delegate ()****\n");
		//
		//  Ende Aenderung Test
		//
		return mono_test_marshal_byref_string_delegate (new ByrefStringDelegate (byref_string_delegate));
	}

}
