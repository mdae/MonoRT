using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Collections;
using System.Threading;

class MainClass
{
	public static void Main (string[] args)
	{
		if (args.Length < 1) {
			Console.WriteLine ("Too few arguments.");
			return;
		}

		string[] strarray = new string[args.Length - 1];

		for (int iIndex = 0; iIndex < (args.Length-1); iIndex++) {
			strarray [iIndex] = args [iIndex + 1];
		}
		object[] objarray = new object[]{strarray};

		Assembly AppAssembly = Assembly.LoadFile (args [0]);
		AssemblyName[] RefAssemblyNames = AppAssembly.GetReferencedAssemblies ();
		Assembly[] AssemblyArray = new Assembly[RefAssemblyNames.Length + 1];
		AssemblyArray [0] = AppAssembly;

		for (int i = 0; i < RefAssemblyNames.Length; i++) {
			AssemblyArray [i + 1] = Assembly.Load (RefAssemblyNames [i]);
		}

		Console.WriteLine ();
		//Console.WriteLine ("Assemblys to pre-comile:");
		foreach (Assembly assem in AssemblyArray) {
			Console.WriteLine (assem.GetName ());
		}

		foreach (Assembly assem in AssemblyArray) {

			//Console.WriteLine ("Pre-Compile: " + assem.GetName ());
			Type[] mytypes = assem.GetTypes ();

			//BindingFlags flags = (BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly);
			BindingFlags flags = (BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
		
			foreach (Type t in mytypes) {
				MethodInfo[] mi = t.GetMethods (flags);
				// Object obj = Activator.CreateInstance(t);

				foreach (MethodInfo m in mi) {
					if ((m.IsAbstract == true) || (m.IsGenericMethod == true)|| (m.IsGenericMethodDefinition == true) || (m.ContainsGenericParameters == true))
						continue;

					try{
						RuntimeHelpers.PrepareMethod (m.MethodHandle);
					}
					catch (SystemException ex)
					{
						if (ex is System.EntryPointNotFoundException ||
						    ex is System.DllNotFoundException ||
						    ex is System.ArgumentException)
						{
						}
						else
						{
							Console.WriteLine("Method that causes exception: " + m.Name);
							throw;
						}
					}
				}
			}
		}
		MethodInfo Entr = AppAssembly.EntryPoint;
		//Console.WriteLine ("EntryPoint: " + Entr.Name);

		Entr.Invoke (null, objarray);
		return;
	}
}
