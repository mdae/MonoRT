//
// cominterop.cs:
//
//  Tests for COM Interop related features
//

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

public class Tests
{

	[DllImport("libtest")]
	public static extern int mono_test_marshal_bstr_in([MarshalAs(UnmanagedType.BStr)]string str);

	[DllImport("libtest")]
    public static extern int mono_test_marshal_bstr_out([MarshalAs(UnmanagedType.BStr)] out string str);

    [DllImport("libtest")]
    public static extern int mono_test_marshal_bstr_in_null([MarshalAs(UnmanagedType.BStr)]string str);

    [DllImport("libtest")]
    public static extern int mono_test_marshal_bstr_out_null([MarshalAs(UnmanagedType.BStr)] out string str);

	[DllImport("libtest")]
	public static extern int mono_test_marshal_variant_in_sbyte([MarshalAs(UnmanagedType.Struct)]object obj);

	[DllImport("libtest")]
	public static extern int mono_test_marshal_variant_in_byte([MarshalAs(UnmanagedType.Struct)]object obj);

	[DllImport("libtest")]
	public static extern int mono_test_marshal_variant_in_short([MarshalAs(UnmanagedType.Struct)]object obj);

	[DllImport("libtest")]
	public static extern int mono_test_marshal_variant_in_ushort([MarshalAs(UnmanagedType.Struct)]object obj);

	[DllImport("libtest")]
	public static extern int mono_test_marshal_variant_in_int([MarshalAs(UnmanagedType.Struct)]object obj);

	[DllImport("libtest")]
	public static extern int mono_test_marshal_variant_in_uint([MarshalAs(UnmanagedType.Struct)]object obj);

	[DllImport("libtest")]
	public static extern int mono_test_marshal_variant_in_long([MarshalAs(UnmanagedType.Struct)]object obj);

	[DllImport("libtest")]
	public static extern int mono_test_marshal_variant_in_ulong([MarshalAs(UnmanagedType.Struct)]object obj);

	[DllImport("libtest")]
	public static extern int mono_test_marshal_variant_in_float([MarshalAs(UnmanagedType.Struct)]object obj);

	[DllImport("libtest")]
	public static extern int mono_test_marshal_variant_in_double([MarshalAs(UnmanagedType.Struct)]object obj);

	[DllImport("libtest")]
	public static extern int mono_test_marshal_variant_in_bstr ([MarshalAs (UnmanagedType.Struct)]object obj);

	[DllImport ("libtest")]
	public static extern int mono_test_marshal_variant_in_bool_true ([MarshalAs (UnmanagedType.Struct)]object obj);

	[DllImport ("libtest")]
	public static extern int mono_test_marshal_variant_in_bool_false ([MarshalAs (UnmanagedType.Struct)]object obj);

	[DllImport("libtest")]
	public static extern int mono_test_marshal_variant_out_sbyte([MarshalAs(UnmanagedType.Struct)]out object obj);

	[DllImport("libtest")]
	public static extern int mono_test_marshal_variant_out_byte([MarshalAs(UnmanagedType.Struct)]out object obj);

	[DllImport("libtest")]
	public static extern int mono_test_marshal_variant_out_short([MarshalAs(UnmanagedType.Struct)]out object obj);

	[DllImport("libtest")]
	public static extern int mono_test_marshal_variant_out_ushort([MarshalAs(UnmanagedType.Struct)]out object obj);

	[DllImport("libtest")]
	public static extern int mono_test_marshal_variant_out_int([MarshalAs(UnmanagedType.Struct)]out object obj);

	[DllImport("libtest")]
	public static extern int mono_test_marshal_variant_out_uint([MarshalAs(UnmanagedType.Struct)]out object obj);

	[DllImport("libtest")]
	public static extern int mono_test_marshal_variant_out_long([MarshalAs(UnmanagedType.Struct)]out object obj);

	[DllImport("libtest")]
	public static extern int mono_test_marshal_variant_out_ulong([MarshalAs(UnmanagedType.Struct)]out object obj);

	[DllImport("libtest")]
	public static extern int mono_test_marshal_variant_out_float([MarshalAs(UnmanagedType.Struct)]out object obj);

	[DllImport("libtest")]
	public static extern int mono_test_marshal_variant_out_double([MarshalAs(UnmanagedType.Struct)]out object obj);

	[DllImport("libtest")]
	public static extern int mono_test_marshal_variant_out_bstr ([MarshalAs (UnmanagedType.Struct)]out object obj);

	[DllImport("libtest")]
	public static extern int mono_test_marshal_variant_out_bool_true ([MarshalAs (UnmanagedType.Struct)]out object obj);

	[DllImport ("libtest")]
	public static extern int mono_test_marshal_variant_out_bool_false ([MarshalAs (UnmanagedType.Struct)]out object obj);


	public delegate int VarFunc (VarEnum vt, [MarshalAs (UnmanagedType.Struct)] object obj);

	public delegate int VarRefFunc (VarEnum vt, [MarshalAs (UnmanagedType.Struct)] ref object obj);

	[DllImport ("libtest")]
	public static extern int mono_test_marshal_variant_in_sbyte_unmanaged (VarFunc func);

	[DllImport ("libtest")]
	public static extern int mono_test_marshal_variant_in_byte_unmanaged (VarFunc func);

	[DllImport ("libtest")]
	public static extern int mono_test_marshal_variant_in_short_unmanaged (VarFunc func);

	[DllImport ("libtest")]
	public static extern int mono_test_marshal_variant_in_ushort_unmanaged (VarFunc func);

	[DllImport ("libtest")]
	public static extern int mono_test_marshal_variant_in_int_unmanaged (VarFunc func);

	[DllImport ("libtest")]
	public static extern int mono_test_marshal_variant_in_uint_unmanaged (VarFunc func);

	[DllImport ("libtest")]
	public static extern int mono_test_marshal_variant_in_long_unmanaged (VarFunc func);

	[DllImport ("libtest")]
	public static extern int mono_test_marshal_variant_in_ulong_unmanaged (VarFunc func);

	[DllImport ("libtest")]
	public static extern int mono_test_marshal_variant_in_float_unmanaged (VarFunc func);

	[DllImport ("libtest")]
	public static extern int mono_test_marshal_variant_in_double_unmanaged (VarFunc func);

	[DllImport ("libtest")]
	public static extern int mono_test_marshal_variant_in_bstr_unmanaged (VarFunc func);

	[DllImport ("libtest")]
	public static extern int mono_test_marshal_variant_in_bool_true_unmanaged (VarFunc func);

	[DllImport ("libtest")]
	public static extern int mono_test_marshal_variant_in_bool_false_unmanaged (VarFunc func);

	[DllImport ("libtest")]
	public static extern int mono_test_marshal_variant_out_sbyte_unmanaged (VarRefFunc func);

	[DllImport ("libtest")]
	public static extern int mono_test_marshal_variant_out_byte_unmanaged (VarRefFunc func);

	[DllImport ("libtest")]
	public static extern int mono_test_marshal_variant_out_short_unmanaged (VarRefFunc func);

	[DllImport ("libtest")]
	public static extern int mono_test_marshal_variant_out_ushort_unmanaged (VarRefFunc func);

	[DllImport ("libtest")]
	public static extern int mono_test_marshal_variant_out_int_unmanaged (VarRefFunc func);

	[DllImport ("libtest")]
	public static extern int mono_test_marshal_variant_out_uint_unmanaged (VarRefFunc func);

	[DllImport ("libtest")]
	public static extern int mono_test_marshal_variant_out_long_unmanaged (VarRefFunc func);

	[DllImport ("libtest")]
	public static extern int mono_test_marshal_variant_out_ulong_unmanaged (VarRefFunc func);

	[DllImport ("libtest")]
	public static extern int mono_test_marshal_variant_out_float_unmanaged (VarRefFunc func);

	[DllImport ("libtest")]
	public static extern int mono_test_marshal_variant_out_double_unmanaged (VarRefFunc func);

	[DllImport ("libtest")]
	public static extern int mono_test_marshal_variant_out_bstr_unmanaged (VarRefFunc func);

	[DllImport ("libtest")]
	public static extern int mono_test_marshal_variant_out_bool_true_unmanaged (VarRefFunc func);

	[DllImport ("libtest")]
	public static extern int mono_test_marshal_variant_out_bool_false_unmanaged (VarRefFunc func);

    [DllImport ("libtest")]
	public static extern int mono_test_marshal_com_object_create (out IntPtr pUnk);

	[DllImport ("libtest")]
	public static extern int mono_test_marshal_com_object_same (out IntPtr pUnk);

    [DllImport ("libtest")]
    public static extern int mono_test_marshal_com_object_destroy (IntPtr pUnk);

	[DllImport ("libtest")]
	public static extern int mono_test_marshal_com_object_ref_count (IntPtr pUnk);

	[DllImport ("libtest")]
	public static extern int mono_test_marshal_ccw_identity ([MarshalAs (UnmanagedType.Interface)]ITest itest);

	[DllImport ("libtest")]
	public static extern int mono_test_marshal_ccw_reflexive ([MarshalAs (UnmanagedType.Interface)]ITest itest);

	[DllImport ("libtest")]
	public static extern int mono_test_marshal_ccw_transitive ([MarshalAs (UnmanagedType.Interface)]ITest itest);

	[DllImport ("libtest")]
	public static extern int mono_test_marshal_ccw_itest ([MarshalAs (UnmanagedType.Interface)]ITest itest);

	[DllImport ("libtest")]
	public static extern int mono_test_marshal_ccw_itest ([MarshalAs (UnmanagedType.Interface)]ITestPresSig itest);

	[DllImport("libtest")]
	public static extern int mono_test_marshal_variant_out_safearray_1dim_vt_bstr_empty([MarshalAs(UnmanagedType.SafeArray, SafeArraySubType = VarEnum.VT_VARIANT)]out Array array);

	[DllImport("libtest")]
	public static extern int mono_test_marshal_variant_out_safearray_1dim_vt_bstr ([MarshalAs (UnmanagedType.SafeArray, SafeArraySubType = VarEnum.VT_VARIANT)]out Array array);

	[DllImport("libtest")]
	public static extern int mono_test_marshal_variant_out_safearray_2dim_vt_int ([MarshalAs (UnmanagedType.SafeArray, SafeArraySubType = VarEnum.VT_VARIANT)]out Array array);

	[DllImport("libtest")]
	public static extern int mono_test_marshal_variant_out_safearray_4dim_vt_int ([MarshalAs (UnmanagedType.SafeArray, SafeArraySubType = VarEnum.VT_VARIANT)]out Array array);

	[DllImport ("libtest")]
	public static extern bool mono_cominterop_is_supported ();

	public static int Main ()
	{

		bool isWindows = !(((int)Environment.OSVersion.Platform == 4) ||
			((int)Environment.OSVersion.Platform == 128));

		if (mono_cominterop_is_supported ())
		{

			#region BSTR Tests
/*
			//
			//  Beginn Aenderung Test
			//
			Console.WriteLine("\n\t**** C#: VOR mono_test_marshal_bstr_in () ****\n");
			//
			//  Ende Aenderung Test
			//
			string str;
			if (mono_test_marshal_bstr_in ("mono_test_marshal_bstr_in") != 0)
				return 1;
			//
			//  Beginn Aenderung Test
			//
			Console.WriteLine("\n\t**** C#: NACH mono_test_marshal_bstr_in () ****\n");
			//
			//  Ende Aenderung Test
			//
			if (mono_test_marshal_bstr_out (out str) != 0 || str != "mono_test_marshal_bstr_out")
				return 2;
			if (mono_test_marshal_bstr_in_null (null) != 0)
				return 1;
			if (mono_test_marshal_bstr_out_null (out str) != 0 || str != null)
				return 2;
*/
			#endregion // BSTR Tests

			#region VARIANT Tests
/*
			object obj;
			if (mono_test_marshal_variant_in_sbyte ((sbyte)100) != 0)
				return 13;
			if (mono_test_marshal_variant_in_byte ((byte)100) != 0)
				return 14;
			if (mono_test_marshal_variant_in_short ((short)314) != 0)
				return 15;
			if (mono_test_marshal_variant_in_ushort ((ushort)314) != 0)
				return 16;
			if (mono_test_marshal_variant_in_int ((int)314) != 0)
				return 17;
			if (mono_test_marshal_variant_in_uint ((uint)314) != 0)
				return 18;
			if (mono_test_marshal_variant_in_long ((long)314) != 0)
				return 19;
			if (mono_test_marshal_variant_in_ulong ((ulong)314) != 0)
				return 20;
			if (mono_test_marshal_variant_in_float ((float)3.14) != 0)
				return 21;
			if (mono_test_marshal_variant_in_double ((double)3.14) != 0)
				return 22;
			if (mono_test_marshal_variant_in_bstr ("PI") != 0)
				return 23;
			if (mono_test_marshal_variant_out_sbyte (out obj) != 0 || (sbyte)obj != 100)
				return 24;
			if (mono_test_marshal_variant_out_byte (out obj) != 0 || (byte)obj != 100)
				return 25;
			if (mono_test_marshal_variant_out_short (out obj) != 0 || (short)obj != 314)
				return 26;
			if (mono_test_marshal_variant_out_ushort (out obj) != 0 || (ushort)obj != 314)
				return 27;
			if (mono_test_marshal_variant_out_int (out obj) != 0 || (int)obj != 314)
				return 28;
			if (mono_test_marshal_variant_out_uint (out obj) != 0 || (uint)obj != 314)
				return 29;
			if (mono_test_marshal_variant_out_long (out obj) != 0 || (long)obj != 314)
				return 30;
			if (mono_test_marshal_variant_out_ulong (out obj) != 0 || (ulong)obj != 314)
				return 31;
			if (mono_test_marshal_variant_out_float (out obj) != 0 || ((float)obj - 3.14) / 3.14 > .001)
				return 32;
			if (mono_test_marshal_variant_out_double (out obj) != 0 || ((double)obj - 3.14) / 3.14 > .001)
				return 33;
			if (mono_test_marshal_variant_out_bstr (out obj) != 0 || (string)obj != "PI")
				return 34;


			VarFunc func = new VarFunc (mono_test_marshal_variant_in_callback);

			//
			//  Beginn Aenderung Test
			//
			Console.WriteLine("\n\t**** C#: VarFunc func = new VarFunc (mono_test_marshal_variant_in_callback) done ****\n");
			Console.WriteLine("\n\t**** C#: VOR mono_test_marshal_variant_in_sbyte_unmanaged (func) ****\n");
			//
			//  Ende Aenderung Test
			//
			if (mono_test_marshal_variant_in_sbyte_unmanaged (func) != 0)
				return 35;

			//
			//  Beginn Aenderung Test
			//
			Console.WriteLine("\n\t**** C#: NACH mono_test_marshal_variant_in_sbyte_unmanaged (func) ****\n");
			Console.WriteLine("\n\t**** C#: VOR mono_test_marshal_variant_in_byte_unmanaged (func) ****\n");
			//
			//  Ende Aenderung Test
			//
			if (mono_test_marshal_variant_in_byte_unmanaged (func) != 0)
				return 36;

			//
			//  Beginn Aenderung Test
			//
			Console.WriteLine("\n\t**** C#: NACH mono_test_marshal_variant_in_byte_unmanaged (func) ****\n");
			Console.WriteLine("\n\t**** C#: VOR mono_test_marshal_variant_in_short_unmanaged (func) ****\n");
			//
			//  Ende Aenderung Test
			//
			if (mono_test_marshal_variant_in_short_unmanaged (func) != 0)
				return 37;

			//
			//  Beginn Aenderung Test
			//
			Console.WriteLine("\n\t**** C#: mono_test_marshal_variant_in_short_unmanaged (func) ****\n");
			Console.WriteLine("\n\t**** C#: VOR mono_test_marshal_variant_in_ushort_unmanaged (func) ****\n");
			//
			//  Ende Aenderung Test
			//
			if (mono_test_marshal_variant_in_ushort_unmanaged (func) != 0)
				return 38;

			//
			//  Beginn Aenderung Test
			//
			Console.WriteLine("\n\t**** C#: NACH mono_test_marshal_variant_in_ushort_unmanaged (func) ****\n");
			Console.WriteLine("\n\t**** C#: VOR mono_test_marshal_variant_in_int_unmanaged (func) ****\n");
			//
			//  Ende Aenderung Test
			//
			if (mono_test_marshal_variant_in_int_unmanaged (func) != 0)
				return 39;

			//
			//  Beginn Aenderung Test
			//
			Console.WriteLine("\n\t**** C#: NACH mono_test_marshal_variant_in_int_unmanaged (func) ****\n");
			Console.WriteLine("\n\t**** C#: VOR mono_test_marshal_variant_in_uint_unmanaged (func) ****\n");
			//
			//  Ende Aenderung Test
			//
			if (mono_test_marshal_variant_in_uint_unmanaged (func) != 0)
				return 40;

			//
			//  Beginn Aenderung Test
			//
			Console.WriteLine("\n\t**** C#: NACH mono_test_marshal_variant_in_uint_unmanaged (func) ****\n");
			Console.WriteLine("\n\t**** C#: VOR mono_test_marshal_variant_in_long_unmanaged (func) ****\n");
			//
			//  Ende Aenderung Test
			//
			if (mono_test_marshal_variant_in_long_unmanaged (func) != 0)
				return 41;

			//
			//  Beginn Aenderung Test
			//
			Console.WriteLine("\n\t**** C#: NACH mono_test_marshal_variant_in_long_unmanaged (func) ****\n");
			Console.WriteLine("\n\t**** C#: VOR mono_test_marshal_variant_in_ulong_unmanaged (func) ****\n");
			//
			//  Ende Aenderung Test
			//
			if (mono_test_marshal_variant_in_ulong_unmanaged (func) != 0)
				return 42;

			//
			//  Beginn Aenderung Test
			//
			Console.WriteLine("\n\t**** C#: NACH mono_test_marshal_variant_in_ulong_unmanaged (func) ****\n");
			Console.WriteLine("\n\t**** C#: VOR mono_test_marshal_variant_in_float_unmanaged (func) ****\n");
			//
			//  Ende Aenderung Test
			//
			if (mono_test_marshal_variant_in_float_unmanaged (func) != 0)
				return 43;

			//
			//  Beginn Aenderung Test
			//
			Console.WriteLine("\n\t**** C#: NACH mono_test_marshal_variant_in_float_unmanaged (func) ****\n");
			Console.WriteLine("\n\t**** C#: VOR mono_test_marshal_variant_in_double_unmanaged (func) ****\n");
			//
			//  Ende Aenderung Test
			//
			if (mono_test_marshal_variant_in_double_unmanaged (func) != 0)
				return 44;

			//
			//  Beginn Aenderung Test
			//
			Console.WriteLine("\n\t**** C#: NACH mono_test_marshal_variant_in_double_unmanaged (func) ****\n");
			Console.WriteLine("\n\t**** C#: VOR mono_test_marshal_variant_in_bstr_unmanaged (func) ****\n");
			//
			//  Ende Aenderung Test
			//
			if (mono_test_marshal_variant_in_bstr_unmanaged (func) != 0)
				return 45;

			//
			//  Beginn Aenderung Test
			//
			Console.WriteLine("\n\t**** C#: NACH mono_test_marshal_variant_in_bstr_unmanaged (func) ****\n");
			Console.WriteLine("\n\t**** C#: VOR mono_test_marshal_variant_in_bool_true_unmanaged (func) ****\n");
			//
			//  Ende Aenderung Test
			//
			if (mono_test_marshal_variant_in_bool_true_unmanaged (func) != 0)
				return 46;
			//
			//  Beginn Aenderung Test
			//
			Console.WriteLine("\n\t**** C#: NACH mono_test_marshal_variant_in_bool_true_unmanaged (func) ****\n");
			//
			//  Ende Aenderung Test
			//

			VarRefFunc reffunc = new VarRefFunc (mono_test_marshal_variant_out_callback);

			//
			//  Beginn Aenderung Test
			//
			Console.WriteLine("\n\t**** C#: VarRefFunc reffunc = new VarRefFunc (mono_test_marshal_variant_out_callback) done ****\n");
			Console.WriteLine("\n\t**** C#: VOR mono_test_marshal_variant_out_sbyte_unmanaged (reffunc) ****\n");
			//
			//  Ende Aenderung Test
			//
			if (mono_test_marshal_variant_out_sbyte_unmanaged (reffunc) != 0)
				return 50;

			//
			//  Beginn Aenderung Test
			//
			Console.WriteLine("\n\t**** C#: NACH mono_test_marshal_variant_out_sbyte_unmanaged (reffunc) ****\n");
			Console.WriteLine("\n\t**** C#: VOR mono_test_marshal_variant_out_byte_unmanaged (func) ****\n");
			//
			//  Ende Aenderung Test
			//
			if (mono_test_marshal_variant_out_byte_unmanaged (reffunc) != 0)
				return 51;

			//
			//  Beginn Aenderung Test
			//
			Console.WriteLine("\n\t**** C#: NACH mono_test_marshal_variant_out_byte_unmanaged (reffunc) ****\n");
			Console.WriteLine("\n\t**** C#: VOR mono_test_marshal_variant_out_short_unmanagedd (func) ****\n");
			//
			//  Ende Aenderung Test
			//
			if (mono_test_marshal_variant_out_short_unmanaged (reffunc) != 0)
				return 52;
			//
			//  Beginn Aenderung Test
			//
			Console.WriteLine("\n\t**** C#: NACH mono_test_marshal_variant_out_short_unmanaged (reffunc) ****\n");
			Console.WriteLine("\n\t**** C#: VOR mono_test_marshal_variant_out_ushort_unmanaged (func) ****\n");
			//
			//  Ende Aenderung Test
			//
			if (mono_test_marshal_variant_out_ushort_unmanaged (reffunc) != 0)
				return 53;
			//
			//  Beginn Aenderung Test
			//
			Console.WriteLine("\n\t**** C#: NACH mono_test_marshal_variant_out_ushort_unmanaged (reffunc) ****\n");
			Console.WriteLine("\n\t**** C#: VOR mono_test_marshal_variant_out_int_unmanaged (func) ****\n");
			//
			//  Ende Aenderung Test
			//
			if (mono_test_marshal_variant_out_int_unmanaged (reffunc) != 0)
				return 54;
			//
			//  Beginn Aenderung Test
			//
			Console.WriteLine("\n\t**** C#: NACH mono_test_marshal_variant_out_int_unmanaged (reffunc) ****\n");
			Console.WriteLine("\n\t**** C#: VOR mono_test_marshal_variant_out_uint_unmanaged (func) ****\n");
			//
			//  Ende Aenderung Test
			//
			if (mono_test_marshal_variant_out_uint_unmanaged (reffunc) != 0)
				return 55;
			//
			//  Beginn Aenderung Test
			//
			Console.WriteLine("\n\t**** C#: NACH mono_test_marshal_variant_out_uint_unmanaged (reffunc) ****\n");
			Console.WriteLine("\n\t**** C#: VOR mono_test_marshal_variant_out_long_unmanaged (func) ****\n");
			//
			//  Ende Aenderung Test
			//
			if (mono_test_marshal_variant_out_long_unmanaged (reffunc) != 0)
				return 56;
			//
			//  Beginn Aenderung Test
			//
			Console.WriteLine("\n\t**** C#: NACH mono_test_marshal_variant_out_long_unmanaged (reffunc) ****\n");
			Console.WriteLine("\n\t**** C#: VOR mono_test_marshal_variant_out_ulong_unmanaged (func) ****\n");
			//
			//  Ende Aenderung Test
			//
			if (mono_test_marshal_variant_out_ulong_unmanaged (reffunc) != 0)
				return 57;
			//
			//  Beginn Aenderung Test
			//
			Console.WriteLine("\n\t**** C#: NACH mono_test_marshal_variant_out_ulong_unmanaged (reffunc) ****\n");
			Console.WriteLine("\n\t**** C#: VOR mono_test_marshal_variant_out_float_unmanaged (func) ****\n");
			//
			//  Ende Aenderung Test
			//
			if (mono_test_marshal_variant_out_float_unmanaged (reffunc) != 0)
				return 58;
			//
			//  Beginn Aenderung Test
			//
			Console.WriteLine("\n\t**** C#: NACH mono_test_marshal_variant_out_float_unmanaged (reffunc) ****\n");
			Console.WriteLine("\n\t**** C#: VOR mono_test_marshal_variant_out_double_unmanaged (func) ****\n");
			//
			//  Ende Aenderung Test
			//
			if (mono_test_marshal_variant_out_double_unmanaged (reffunc) != 0)
				return 59;
			//
			//  Beginn Aenderung Test
			//
			Console.WriteLine("\n\t**** C#: NACH mono_test_marshal_variant_out_double_unmanaged (reffunc) ****\n");
			Console.WriteLine("\n\t**** C#: VOR mono_test_marshal_variant_out_bstr_unmanaged (func) ****\n");
			//
			//  Ende Aenderung Test
			//
			if (mono_test_marshal_variant_out_bstr_unmanaged (reffunc) != 0)
				return 60;
			//
			//  Beginn Aenderung Test
			//
			Console.WriteLine("\n\t**** C#: NACH mono_test_marshal_variant_out_bstr_unmanaged (reffunc) ****\n");
			Console.WriteLine("\n\t**** C#: VOR mono_test_marshal_variant_out_bool_true_unmanaged (func) ****\n");
			//
			//  Ende Aenderung Test
			//
			if (mono_test_marshal_variant_out_bool_true_unmanaged (reffunc) != 0)
				return 61;
			//
			//  Beginn Aenderung Test
			//
			Console.WriteLine("\n\t**** C#: NACH mono_test_marshal_variant_out_bool_true_unmanaged (reffunc) ****\n");
			//
			//  Ende Aenderung Test
			//
*/
			#endregion // VARIANT Tests

			#region Runtime Callable Wrapper Tests
/*
			IntPtr pUnk;
			if (mono_test_marshal_com_object_create (out pUnk) != 0)
				return 145;

			if (mono_test_marshal_com_object_ref_count (pUnk) != 1)
				return 146;

			if (Marshal.AddRef (pUnk) != 2)
				return 147;

			if (mono_test_marshal_com_object_ref_count (pUnk) != 2)
				return 148;

			if (Marshal.Release (pUnk) != 1)
				return 149;

			if (mono_test_marshal_com_object_ref_count (pUnk) != 1)
				return 150;

			object com_obj = Marshal.GetObjectForIUnknown (pUnk);

			if (com_obj == null)
				return 151;

			ITest itest = com_obj as ITest;

			if (itest == null)
				return 152;

			IntPtr pUnk2;
			if (mono_test_marshal_com_object_same (out pUnk2) != 0)
				return 153;

			object com_obj2 = Marshal.GetObjectForIUnknown (pUnk2);
			
			if (com_obj != com_obj2)
				return 154;

			if (!com_obj.Equals (com_obj2))
				return 155;

			IntPtr pUnk3;
			if (mono_test_marshal_com_object_create (out pUnk3) != 0)
				return 156;

			object com_obj3 = Marshal.GetObjectForIUnknown (pUnk3);
			if (com_obj == com_obj3)
				return 157;

			if (com_obj.Equals (com_obj3))
				return 158;

			// com_obj & com_obj2 share a RCW
			if (Marshal.ReleaseComObject (com_obj2) != 1)
				return 159;

			// com_obj3 should only have one RCW
			if (Marshal.ReleaseComObject (com_obj3) != 0)
				return 160;

			IntPtr iunknown = Marshal.GetIUnknownForObject (com_obj);
			if (iunknown == IntPtr.Zero)
				return 170;

			if (pUnk != iunknown)
				return 171;

			if (TestITest (itest) != 0)
				return 172;

			if (TestITestPresSig (itest as ITestPresSig) != 0)
				return 173;

			if (TestITestDelegate (itest) != 0)
				return 174;
*/
			#endregion // Runtime Callable Wrapper Tests

			#region COM Callable Wrapper Tests

			ManagedTest test = new ManagedTest ();
			//
			//  Beginn Aenderung Test
			//
			Console.WriteLine("\n\t**** C#: ManagedTest test = new ManagedTest () done ****\n");
			Console.WriteLine("\n\t**** C#: VOR mono_test_marshal_ccw_itest (test) ****\n");
			//
			//  Ende Aenderung Test
			//
			mono_test_marshal_ccw_itest (test);
			//
			//  Beginn Aenderung Test
			//
			Console.WriteLine("\n\t**** C#: NACH mono_test_marshal_ccw_itest (test) ****\n");
			//
			//  Ende Aenderung Test
			//
			if (test.Status != 0)
				return 200;

			ManagedTestPresSig test_pres_sig = new ManagedTestPresSig ();
			//
			//  Beginn Aenderung Test
			//
			Console.WriteLine("\n\t**** C#: ManagedTestPresSig test_pres_sig = new ManagedTestPresSig () done ****\n");
			Console.WriteLine("\n\t**** C#: VOR mono_test_marshal_ccw_itest (test_pres_sig)****\n");
			//
			//  Ende Aenderung Test
			//
			mono_test_marshal_ccw_itest (test_pres_sig);
			//
			//  Beginn Aenderung Test
			//
			Console.WriteLine("\n\t**** C#: NACH mono_test_marshal_ccw_itest (test_pres_sig) ****\n");
			//
			//  Ende Aenderung Test
			//

			#endregion // COM Callable Wrapper Tests

			#region SAFEARRAY tests
/*			
			if (isWindows) {
			//
			//  Beginn Aenderung Test
			//
			Console.WriteLine("\n\t**** C#: Test ****\n");
			//
			//  Ende Aenderung Test
			//
				Array array;
				if ((mono_test_marshal_variant_out_safearray_1dim_vt_bstr_empty(out array) != 0) || (array.Rank != 1) || (array.Length != 0))
					return 62;

				if ((mono_test_marshal_variant_out_safearray_1dim_vt_bstr (out array) != 0) || (array.Rank != 1) || (array.Length != 10))
					return 63;
				for (int i = 0; i < 10; ++i) {
					if (i != Convert.ToInt32(array.GetValue (i)))
						return 64;
				}

				if ((mono_test_marshal_variant_out_safearray_2dim_vt_int (out array) != 0) || (array.Rank != 2))
					return 65;
				if (   (array.GetLowerBound (0) != 0) || (array.GetUpperBound (0) != 3)
					|| (array.GetLowerBound (1) != 0) || (array.GetUpperBound (1) != 2))
					return 66;
				for (int i = array.GetLowerBound (0); i <= array.GetUpperBound (0); ++i)
				{
					for (int j = array.GetLowerBound (1); j <= array.GetUpperBound (1); ++j) {
						if ((i + 1) * 10 + (j + 1) != (int)array.GetValue (new long[] { i, j }))
							return 67;
					}
				}

				if ((mono_test_marshal_variant_out_safearray_4dim_vt_int (out array) != 0) || (array.Rank != 4))
					return 68;
				if (   (array.GetLowerBound (0) != 15) || (array.GetUpperBound (0) != 24)
					|| (array.GetLowerBound (1) != 20) || (array.GetUpperBound (1) != 22)
					|| (array.GetLowerBound (2) !=  5) || (array.GetUpperBound (2) != 10)
					|| (array.GetLowerBound (3) != 12) || (array.GetUpperBound (3) != 18) )
					return 69;

				int index = 0;
				for (int i = array.GetLowerBound (3); i <= array.GetUpperBound (3); ++i) {
					for (int j = array.GetLowerBound (2); j <= array.GetUpperBound (2); ++j) {
						for (int k = array.GetLowerBound (1); k <= array.GetUpperBound (1); ++k) {
							for (int l = array.GetLowerBound (0); l <= array.GetUpperBound (0); ++l) {
								if (index != (int)array.GetValue (new long[] { l, k, j, i }))
									return 70;
								++index;
							}
						}
					}
				}
			}*/
			#endregion // SafeArray Tests

		}

        return 0;
	}


	[ComImport ()]
	[Guid ("00000000-0000-0000-0000-000000000001")]
	[InterfaceType (ComInterfaceType.InterfaceIsIUnknown)]
	public interface ITest
	{
		// properties need to go first since mcs puts them there
		ITest Test
		{
			[return: MarshalAs (UnmanagedType.Interface)]
			[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId (5242884)]
			get;
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		void SByteIn (sbyte val);
		[MethodImplAttribute (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		void ByteIn (byte val);
		[MethodImplAttribute (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		void ShortIn (short val);
		[MethodImplAttribute (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		void UShortIn (ushort val);
		[MethodImplAttribute (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		void IntIn (int val);
		[MethodImplAttribute (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		void UIntIn (uint val);
		[MethodImplAttribute (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		void LongIn (long val);
		[MethodImplAttribute (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		void ULongIn (ulong val);
		[MethodImplAttribute (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		void FloatIn (float val);
		[MethodImplAttribute (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		void DoubleIn (double val);
		[MethodImplAttribute (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		void ITestIn ([MarshalAs (UnmanagedType.Interface)]ITest val);
		[MethodImplAttribute (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		void ITestOut ([MarshalAs (UnmanagedType.Interface)]out ITest val);
	}

	[ComImport ()]
	[Guid ("00000000-0000-0000-0000-000000000001")]
	[InterfaceType (ComInterfaceType.InterfaceIsIUnknown)]
	public interface ITestPresSig
	{
		// properties need to go first since mcs puts them there
		ITestPresSig Test
		{
			[return: MarshalAs (UnmanagedType.Interface)]
			[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId (5242884)]
			get;
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[PreserveSig ()]
		int SByteIn (sbyte val);
		[MethodImplAttribute (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[PreserveSig ()]
		int ByteIn (byte val);
		[MethodImplAttribute (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[PreserveSig ()]
		int ShortIn (short val);
		[MethodImplAttribute (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[PreserveSig ()]
		int UShortIn (ushort val);
		[MethodImplAttribute (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[PreserveSig ()]
		int IntIn (int val);
		[MethodImplAttribute (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[PreserveSig ()]
		int UIntIn (uint val);
		[MethodImplAttribute (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[PreserveSig ()]
		int LongIn (long val);
		[MethodImplAttribute (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[PreserveSig ()]
		int ULongIn (ulong val);
		[MethodImplAttribute (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[PreserveSig ()]
		int FloatIn (float val);
		[MethodImplAttribute (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[PreserveSig ()]
		int DoubleIn (double val);
		[MethodImplAttribute (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[PreserveSig ()]
		int ITestIn ([MarshalAs (UnmanagedType.Interface)]ITestPresSig val);
		[MethodImplAttribute (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[PreserveSig ()]
		int ITestOut ([MarshalAs (UnmanagedType.Interface)]out ITestPresSig val);
	}

	delegate void SByteInDelegate (sbyte val);
	delegate void ByteInDelegate (byte val);
	delegate void ShortInDelegate (short val);
	delegate void UShortInDelegate (ushort val);
	delegate void IntInDelegate (int val);
	delegate void UIntInDelegate (uint val);
	delegate void LongInDelegate (long val);
	delegate void ULongInDelegate (ulong val);
	delegate void FloatInDelegate (float val);
	delegate void DoubleInDelegate (double val);
	delegate void ITestInDelegate (ITest val);
	delegate void ITestOutDelegate (out ITest val);

	public class ManagedTestPresSig : ITestPresSig
	{		// properties need to go first since mcs puts them there
		public ITestPresSig Test
		{
			get
			{
				return new ManagedTestPresSig ();
			}
		}

		public int SByteIn (sbyte val)
		{
			//
			//  Beginn Aenderung Test
			//
			Console.WriteLine("\n\t**** C#: int SByteIn (sbyte val) (ManagedTestPresSig) ****\n");
			//
			//  Ende Aenderung Test
			//
			if (val != -100)
				return 1;
			return 0;
		}

		public int ByteIn (byte val)
		{
			//
			//  Beginn Aenderung Test
			//
			Console.WriteLine("\n\t**** C#: int ByteIn (byte val) (ManagedTestPresSig) ****\n");
			//
			//  Ende Aenderung Test
			//
			if (val != 100)
				return 2;
			return 0;
		}

		public int ShortIn (short val)
		{
			//
			//  Beginn Aenderung Test
			//
			Console.WriteLine("\n\t**** C#: int ShortIn (short val) (ManagedTestPresSig) ****\n");
			//
			//  Ende Aenderung Test
			//
			if (val != -100)
				return 3;
			return 0;
		}

		public int UShortIn (ushort val)
		{
			//
			//  Beginn Aenderung Test
			//
			Console.WriteLine("\n\t**** C#: int UShortIn (ushort val) (ManagedTestPresSig) ****\n");
			//
			//  Ende Aenderung Test
			//
			if (val != 100)
				return 4;
			return 0;
		}

		public int IntIn (int val)
		{
			//
			//  Beginn Aenderung Test
			//
			Console.WriteLine("\n\t**** C#: int IntIn (int val) (ManagedTestPresSig) ****\n");
			//
			//  Ende Aenderung Test
			//
			if (val != -100)
				return 5;
			return 0;
		}

		public int UIntIn (uint val)
		{
			//
			//  Beginn Aenderung Test
			//
			Console.WriteLine("\n\t**** C#: int UIntIn (uint val) (ManagedTestPresSig) ****\n");
			//
			//  Ende Aenderung Test
			//
			if (val != 100)
				return 6;
			return 0;
		}

		public int LongIn (long val)
		{
			//
			//  Beginn Aenderung Test
			//
			Console.WriteLine("\n\t**** C#: int LongIn (long val) (ManagedTestPresSig) ****\n");
			//
			//  Ende Aenderung Test
			//
			if (val != -100)
				return 7;
			return 0;
		}

		public int ULongIn (ulong val)
		{
			//
			//  Beginn Aenderung Test
			//
			Console.WriteLine("\n\t**** C#: int ULongIn (ulong val) (ManagedTestPresSig) ****\n");
			//
			//  Ende Aenderung Test
			//
			if (val != 100)
				return 8;
			return 0;
		}

		public int FloatIn (float val)
		{
			//
			//  Beginn Aenderung Test
			//
			Console.WriteLine("\n\t**** C#: int FloatIn (float val) (ManagedTestPresSig) ****\n");
			//
			//  Ende Aenderung Test
			//
			if (Math.Abs (val - 3.14f) > .000001)
				return 9;
			return 0;
		}

		public int DoubleIn (double val)
		{
			//
			//  Beginn Aenderung Test
			//
			Console.WriteLine("\n\t**** C#: int DoubleIn (double val) (ManagedTestPresSig) ****\n");
			//
			//  Ende Aenderung Test
			//
			if (Math.Abs (val - 3.14f) > .000001)
				return 10;
			return 0;
		}

		public int ITestIn ([MarshalAs (UnmanagedType.Interface)]ITestPresSig val)
		{
			//
			//  Beginn Aenderung Test
			//
			Console.WriteLine("\n\t**** C#: int ITestIn ([MarshalAs (UnmanagedType.Interface)]ITestPresSig val) (ManagedTestPresSig) ****\n");
			//
			//  Ende Aenderung Test
			//
			if (val == null)
				return 11;
			if (null == val as ManagedTestPresSig)
				return 12;
			return 0;
		}

		public int ITestOut ([MarshalAs (UnmanagedType.Interface)]out ITestPresSig val)
		{
			//
			//  Beginn Aenderung Test
			//
			Console.WriteLine("\n\t**** C#: int ITestOut ([MarshalAs (UnmanagedType.Interface)]out ITestPresSig val) (ManagedTestPresSig) ****\n");
			//
			//  Ende Aenderung Test
			//
			val = new ManagedTestPresSig ();
			return 0;
		}
	}

	public class ManagedTest : ITest
	{
		private int status = 0;
		public int Status
		{
			get { return status; }
		}
		public void SByteIn (sbyte val)
		{
			//
			//  Beginn Aenderung Test
			//
			Console.WriteLine("\n\t****C#: int ManagedTest:SByteIn (sbyte val) (ManagedTest) ****\n");
			//
			//  Ende Aenderung Test
			//
			if (val != -100)
				status = 1;
		}

		public void ByteIn (byte val)
		{
			//
			//  Beginn Aenderung Test
			//
			Console.WriteLine("\n\t**** C#: void ByteIn (byte val) (ManagedTest) ****\n");
			//
			//  Ende Aenderung Test
			//
			if (val != 100)
				status = 2;
		}

		public void ShortIn (short val)
		{
			//
			//  Beginn Aenderung Test
			//
			Console.WriteLine("\n\t**** C#: void ShortIn (short val) (ManagedTest) ****\n");
			//
			//  Ende Aenderung Test
			//
			if (val != -100)
				status = 3;
		}

		public void UShortIn (ushort val)
		{
			//
			//  Beginn Aenderung Test
			//
			Console.WriteLine("\n\t**** C#: void UShortIn (ushort val) (ManagedTest) ****\n");
			//
			//  Ende Aenderung Test
			//
			if (val != 100)
				status = 4;
		}

		public void IntIn (int val)
		{
			//
			//  Beginn Aenderung Test
			//
			Console.WriteLine("\n\t**** C#: void IntIn (int val) (ManagedTest) ****\n");
			//
			//  Ende Aenderung Test
			//
			if (val != -100)
				status = 5;
		}

		public void UIntIn (uint val)
		{
			//
			//  Beginn Aenderung Test
			//
			Console.WriteLine("\n\t**** C#: void UIntIn (uint val) (ManagedTest) ****\n");
			//
			//  Ende Aenderung Test
			//
			if (val != 100)
				status = 6;
		}

		public void LongIn (long val)
		{
			//
			//  Beginn Aenderung Test
			//
			Console.WriteLine("\n\t**** C#: void LongIn (long val) (ManagedTest) ****\n");
			//
			//  Ende Aenderung Test
			//
			if (val != -100)
				status = 7;
		}

		public void ULongIn (ulong val)
		{
			//
			//  Beginn Aenderung Test
			//
			Console.WriteLine("\n\t**** C#: void ULongIn (ulong val) (ManagedTest) ****\n");
			//
			//  Ende Aenderung Test
			//
			if (val != 100)
				status = 8;
		}

		public void FloatIn (float val)
		{
			//
			//  Beginn Aenderung Test
			//
			Console.WriteLine("\n\t**** C#: void FloatIn (float val) (ManagedTest) ****\n");
			//
			//  Ende Aenderung Test
			//
			if (Math.Abs (val - 3.14f) > .000001)
				status = 9;
		}

		public void DoubleIn (double val)
		{
			//
			//  Beginn Aenderung Test
			//
			Console.WriteLine("\n\t**** C#: void DoubleIn (double val) (ManagedTest) ****\n");
			//
			//  Ende Aenderung Test
			//
			if (Math.Abs (val - 3.14) > .000001)
				status = 10;
		}

		public void ITestIn (ITest val)
		{
			//
			//  Beginn Aenderung Test
			//
			Console.WriteLine("\n\t**** C#: void ITestIn (ITest val) (ManagedTest) ****\n");
			//
			//  Ende Aenderung Test
			//
			if (val == null)
				status = 11;
			if (null == val as ManagedTest)
				status = 12;
		}

		public void ITestOut (out ITest val)
		{
			//
			//  Beginn Aenderung Test
			//
			Console.WriteLine("\n\t**** C#: void ITestOut (out ITest val)) (ManagedTest) ****\n");
			//
			//  Ende Aenderung Test
			//
			val = new ManagedTest ();
			//
			//  Beginn Aenderung Test
			//
			Console.WriteLine("\n\t**** C#: val = new ManagedTest () fertig (ManagedTest) ****\n");
			//
			//  Ende Aenderung Test
			//
		}

		public ITest Test
		{
			get
			{
				return new ManagedTest ();
			}
		}
	}

	public static int mono_test_marshal_variant_in_callback (VarEnum vt, object obj)
	{
		//
		//  Beginn Aenderung Test
		//
		Console.WriteLine("\n\t**** C#: mono_test_marshal_variant_in_callback ()****\n");
		//
		//  Ende Aenderung Test
		//
		switch (vt)
		{
		case VarEnum.VT_I1:
			if (obj.GetType () != typeof (sbyte))
				return 1;
			if ((sbyte)obj != -100)
				return 2;
			break;
		case VarEnum.VT_UI1:
			if (obj.GetType () != typeof (byte))
				return 1;
			if ((byte)obj != 100)
				return 2;
			break;
		case VarEnum.VT_I2:
			if (obj.GetType () != typeof (short))
				return 1;
			if ((short)obj != -100)
				return 2;
			break;
		case VarEnum.VT_UI2:
			if (obj.GetType () != typeof (ushort))
				return 1;
			if ((ushort)obj != 100)
				return 2;
			break;
		case VarEnum.VT_I4:
			if (obj.GetType () != typeof (int))
				return 1;
			if ((int)obj != -100)
				return 2;
			break;
		case VarEnum.VT_UI4:
			if (obj.GetType () != typeof (uint))
				return 1;
			if ((uint)obj != 100)
				return 2;
			break;
		case VarEnum.VT_I8:
			if (obj.GetType () != typeof (long))
				return 1;
			if ((long)obj != -100)
				return 2;
			break;
		case VarEnum.VT_UI8:
			if (obj.GetType () != typeof (ulong))
				return 1;
			if ((ulong)obj != 100)
				return 2;
			break;
		case VarEnum.VT_R4:
			if (obj.GetType () != typeof (float))
				return 1;
			if (Math.Abs ((float)obj - 3.14f) > 1e-10)
				return 2;
			break;
		case VarEnum.VT_R8:
			if (obj.GetType () != typeof (double))
				return 1;
			if (Math.Abs ((double)obj - 3.14) > 1e-10)
				return 2;
			break;
		case VarEnum.VT_BSTR:
			if (obj.GetType () != typeof (string))
				return 1;
			if ((string)obj != "PI")
				return 2;
			break;
		case VarEnum.VT_BOOL:
			if (obj.GetType () != typeof (bool))
				return 1;
			if ((bool)obj != true)
				return 2;
			break;
		}
		return 0;
	}

	public static int mono_test_marshal_variant_out_callback (VarEnum vt, ref object obj)
	{
		//
		//  Beginn Aenderung Test
		//
		Console.WriteLine("\n\t**** C#: mono_test_marshal_variant_out_callback ()****\n");
		//
		//  Ende Aenderung Test
		//
		switch (vt) {
		case VarEnum.VT_I1:
			obj = (sbyte)-100;
			break;
		case VarEnum.VT_UI1:
			obj = (byte)100;
			break;
		case VarEnum.VT_I2:
			obj = (short)-100;
			break;
		case VarEnum.VT_UI2:
			obj = (ushort)100;
			break;
		case VarEnum.VT_I4:
			obj = (int)-100;
			break;
		case VarEnum.VT_UI4:
			obj = (uint)100;
			break;
		case VarEnum.VT_I8:
			obj = (long)-100;
			break;
		case VarEnum.VT_UI8:
			obj = (ulong)100;
			break;
		case VarEnum.VT_R4:
			obj = (float)3.14f;
			break;
		case VarEnum.VT_R8:
			obj = (double)3.14;
			break;
		case VarEnum.VT_BSTR:
			obj = "PI";
			break;
		case VarEnum.VT_BOOL:
			obj = true;
			break;
		}
		return 0;
	}

	public static int TestITest (ITest itest)
	{
		try {
			ITest itest2;
			itest.SByteIn (-100);
			itest.ByteIn (100);
			itest.ShortIn (-100);
			itest.UShortIn (100);
			itest.IntIn (-100);
			itest.UIntIn (100);
			itest.LongIn (-100);
			itest.ULongIn (100);
			itest.FloatIn (3.14f);
			itest.DoubleIn (3.14);
			itest.ITestIn (itest);
			itest.ITestOut (out itest2);
		}
		catch (Exception ex) {
			return 1;
		}
		return 0;
	}

	public static int TestITestPresSig (ITestPresSig itest)
	{
		ITestPresSig itest2;
		if (itest.SByteIn (-100) != 0)
			return 1000;
		if (itest.ByteIn (100) != 0)
			return 1001;
		if (itest.ShortIn (-100) != 0)
			return 1002;
		if (itest.UShortIn (100) != 0)
			return 1003;
		if (itest.IntIn (-100) != 0)
			return 1004;
		if (itest.UIntIn (100) != 0)
			return 1005;
		if (itest.LongIn (-100) != 0)
			return 1006;
		if (itest.ULongIn (100) != 0)
			return 1007;
		if (itest.FloatIn (3.14f) != 0)
			return 1008;
		if (itest.DoubleIn (3.14) != 0)
			return 1009;
		if (itest.ITestIn (itest) != 0)
			return 1010;
		if (itest.ITestOut (out itest2) != 0)
			return 1011;
		return 0;
	}

	public static int TestITestDelegate (ITest itest)
	{
		try {
			ITest itest2;
			//
			//  Beginn Aenderung Test
			//
			Console.WriteLine("\n\t**** ITest itest2 done ****\n");
			//
			//  Ende Aenderung Test
			//
			SByteInDelegate SByteInFcn= itest.SByteIn;
			//
			//  Beginn Aenderung Test
			//
			Console.WriteLine("\n\t**** SByteInDelegate SByteInFcn= itest.SByteIn done ****\n");
			//
			//  Ende Aenderung Test
			//
			ByteInDelegate ByteInFcn = itest.ByteIn;
			//
			//  Beginn Aenderung Test
			//
			Console.WriteLine("\n\t**** ByteInDelegate ByteInFcn = itest.ByteIn done ****\n");
			//
			//  Ende Aenderung Test
			//
			UShortInDelegate UShortInFcn = itest.UShortIn;
			//
			//  Beginn Aenderung Test
			//
			Console.WriteLine("\n\t**** UShortInDelegate UShortInFcn = itest.UShortIn done ****\n");
			//
			//  Ende Aenderung Test
			//
			IntInDelegate IntInFcn = itest.IntIn;
			//
			//  Beginn Aenderung Test
			//
			Console.WriteLine("\n\t**** IntInDelegate IntInFcn = itest.IntIn done ****\n");
			//
			//  Ende Aenderung Test
			//
			UIntInDelegate UIntInFcn = itest.UIntIn;
			//
			//  Beginn Aenderung Test
			//
			Console.WriteLine("\n\t**** UIntInDelegate UIntInFcn = itest.UIntIn done ****\n");
			//
			//  Ende Aenderung Test
			//
			LongInDelegate LongInFcn = itest.LongIn;
			//
			//  Beginn Aenderung Test
			//
			Console.WriteLine("\n\t**** LongInDelegate LongInFcn = itest.LongIn done ****\n");
			//
			//  Ende Aenderung Test
			//
			ULongInDelegate ULongInFcn = itest.ULongIn;
			//
			//  Beginn Aenderung Test
			//
			Console.WriteLine("\n\t**** ULongInDelegate ULongInFcn = itest.ULongIn done ****\n");
			//
			//  Ende Aenderung Test
			//
			FloatInDelegate FloatInFcn = itest.FloatIn;
			//
			//  Beginn Aenderung Test
			//
			Console.WriteLine("\n\t**** FloatInDelegate FloatInFcn = itest.FloatIn done ****\n");
			//
			//  Ende Aenderung Test
			//
			DoubleInDelegate DoubleInFcn = itest.DoubleIn;
			//
			//  Beginn Aenderung Test
			//
			Console.WriteLine("\n\t**** DoubleInDelegate DoubleInFcn = itest.DoubleIn done ****\n");
			//
			//  Ende Aenderung Test
			//
			ITestInDelegate ITestInFcn = itest.ITestIn;
			//
			//  Beginn Aenderung Test
			//
			Console.WriteLine("\n\t**** ITestInDelegate ITestInFcn = itest.ITestIn done ****\n");
			//
			//  Ende Aenderung Test
			//
			ITestOutDelegate ITestOutFcn = itest.ITestOut;
			//
			//  Beginn Aenderung Test
			//
			Console.WriteLine("\n\t**** ITestOutDelegate ITestOutFcn = itest.ITestOut done ****\n");
			//
			//  Ende Aenderung Test
			//
			SByteInFcn (-100);
			ByteInFcn (100);
			UShortInFcn (100);
			IntInFcn (-100);
			UIntInFcn (100);
			LongInFcn (-100);
			ULongInFcn (100);
			FloatInFcn (3.14f);
			DoubleInFcn (3.14);
			ITestInFcn (itest);
			ITestOutFcn (out itest2);
		}
		catch (Exception) {
			return 1;
		}
		return 0;
	}
}
