using System;
using System.Threading;
using System.Runtime.InteropServices;

class foo {
	delegate void foo_delegate ();
	
	static void function () {
		Console.WriteLine ("Delegate method");
	}

	static void async_callback (IAsyncResult ar)
	{
		Console.WriteLine ("Async callback " + ar.AsyncState);
		Environment.Exit(0);
	}
	
	public static int Main () {
		Environment.ExitCode = 2;
		foo_delegate d = new foo_delegate (function);
		//
		//  Beginn Aenderung Test
		//
		Console.WriteLine("\n\t**** foo_delegate d = new foo_delegate (function) done ****\n");
		//
		//  Ende Aenderung Test
		//
		AsyncCallback ac = new AsyncCallback (async_callback);
		//
		//  Beginn Aenderung Test
		//
		Console.WriteLine("\n\t**** AsyncCallback ac = new AsyncCallback (async_callback) done ****\n");
		//
		//  Ende Aenderung Test
		//
		IAsyncResult ar1 = d.BeginInvoke (ac, "foo");
		//
		//  Beginn Aenderung Test
		//
		Console.WriteLine("\n\t**** NACH d.BeginInvoke () ****\n");
		Console.WriteLine("\n\t**** VOR ar1.AsyncWaitHandle.WaitOne() ****\n");
		//
		//  Ende Aenderung Test
		//
		ar1.AsyncWaitHandle.WaitOne();
		//
		//  Beginn Aenderung Test
		//
		Console.WriteLine("\n\t**** NACH ar1.AsyncWaitHandle.WaitOne() ****\n");
		Console.WriteLine("\n\t**** VOR d.EndInvoke(ar1) ****\n");
		//
		//  Ende Aenderung Test
		//
		d.EndInvoke(ar1);
		//
		//  Beginn Aenderung Test
		//
		Console.WriteLine("\n\t**** NACH d.EndInvoke(ar1) ****\n");
		//
		//  Ende Aenderung Test
		//
		Thread.Sleep(1000);
		Console.WriteLine("Main returns");
		return 1;
	}
}
