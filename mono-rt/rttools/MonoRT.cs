using System;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Diagnostics;


public class PrePatchClass {

	[MethodImplAttribute(MethodImplOptions.InternalCall)]
	static extern void prepatchCode (int reTryPrePatch);

	public static bool contPrePatch = true;

	public static void prePatchAssembly () {

		int reTryPrePatch = 0;

		while (contPrePatch == true) {

			try {
				if (reTryPrePatch == 1) {
					Console.WriteLine("\n\tMono-RT - Pre-Patch Re-Start ... ");
				}
				prepatchCode (reTryPrePatch);
				contPrePatch = false;

			} catch (Exception e) {
				Console.WriteLine (e.ToString ());
				contPrePatch = true;
				reTryPrePatch = 1;
			}
		}
		return;
	}
}


public class PreJitClass {

	[MethodImplAttribute(MethodImplOptions.InternalCall)]
	static extern void preCompile (IntPtr assemblyToPreCompile, int reTryCompilation);

	[MethodImplAttribute(MethodImplOptions.InternalCall)]
	static extern void insertInPreJitExclusionList ();

	public static void preJitCompileAssembly (IntPtr assemblyToPreCompile) {

		int reTryPreJIT = 0;
		bool contCompile = true;

		while (contCompile == true) {

			try {
				if (reTryPreJIT == 1) {
					Console.WriteLine("\n\tMono-RT - Pre-JIT Re-Start ... ");
				}

				preCompile (assemblyToPreCompile, reTryPreJIT);
				contCompile = false;

			} catch (Exception e) {
				Console.WriteLine (e.ToString ());
				insertInPreJitExclusionList ();
				contCompile = true;
				reTryPreJIT = 1;
			}
		}
		return;
	}
}


public class MonitoringClass {

	[MethodImplAttribute(MethodImplOptions.InternalCall)]
	static extern void setMonitoringOptionsInternal (int jitTrace, int patchTrace, int wrapperTrace);

	public static string printStackTrace () {

		string ret;

		try {
			StackTrace st = new StackTrace (1, true);
			ret = st.ToString().Replace(":line ",":");

			if (ret == String.Empty) {
				ret = "\tKein Stack-Trace ermittelbar.\n";
			}
		} catch {
			ret = "\tKein Stack-Trace ermittelbar.\n";
		}
		return ret;
	}

	public static void setMonitoringOptions (bool jtrace, bool ptrace, bool wtrace) {

		int monitorJit, monitorTrampoline, monitorWrapper;

		if (jtrace == true) {
			monitorJit = 1;
		} else {
			monitorJit = 0;
		}

		if (ptrace == true) {
			monitorTrampoline = 1;
		} else {
			monitorTrampoline = 0;
		}

		if (wtrace == true) {
			monitorWrapper = 1;
		} else {
			monitorWrapper = 0;
		}

		setMonitoringOptionsInternal(monitorJit, monitorTrampoline, monitorWrapper);

		return;		
	}
}
