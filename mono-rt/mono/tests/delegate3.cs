using System;
using System.Threading;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;

class Test {
	delegate int SimpleDelegate (int a);

	static int cb_state = 0;
	
	static int F (int a) {
		Console.WriteLine ("Test.F from delegate: " + a);
		throw new NotImplementedException ();
	}

	static void async_callback (IAsyncResult ar)
	{
		AsyncResult ares = (AsyncResult)ar;
		//
		//  Beginn Aenderung Test
		//
		Console.WriteLine("\n\tVOR AsyncCallback ac = new AsyncCallback (async_callback)\n");
		//
		//  Ende Aenderung Test
		//
		AsyncCallback ac = new AsyncCallback (async_callback);
		//
		//  Beginn Aenderung Test
		//
		Console.WriteLine("\n\tNACH AsyncCallback ac = new AsyncCallback (async_callback)\n");
		//
		//  Ende Aenderung Test
		//
		Console.WriteLine ("Async Callback " + ar.AsyncState);
		cb_state++;
		//
		//  Beginn Aenderung Test
		//
		Console.WriteLine("\n\tVOR SimpleDelegate d = (SimpleDelegate)ares.AsyncDelegate\n");
		//
		//  Ende Aenderung Test
		//
		SimpleDelegate d = (SimpleDelegate)ares.AsyncDelegate;
		//
		//  Beginn Aenderung Test
		//
		Console.WriteLine("\n\tNACH SimpleDelegate d = (SimpleDelegate)ares.AsyncDelegate\n");
		//
		//  Ende Aenderung Test
		//
		if (cb_state < 5)
			d.BeginInvoke (cb_state, ac, cb_state);
		
		//throw new NotImplementedException ();
	}
	
	static int Main () {
		//
		//  Beginn Aenderung Test
		//
		Console.WriteLine("\n\tVOR SimpleDelegate d = new SimpleDelegate (F);\n");
		//
		//  Ende Aenderung Test
		//
		SimpleDelegate d = new SimpleDelegate (F);
		//
		//  Beginn Aenderung Test
		//
		Console.WriteLine("\n\tNACH SimpleDelegate d = new SimpleDelegate (F);\n");
		Console.WriteLine("\n\tVOR AsyncCallback ac = new AsyncCallback (async_callback);\n");
		//
		//  Ende Aenderung Test
		//
		AsyncCallback ac = new AsyncCallback (async_callback);
		//
		//  Beginn Aenderung Test
		//
		Console.WriteLine("\n\tNACH AsyncCallback ac = new AsyncCallback (async_callback)\n");
		Console.WriteLine("\n\tVOR IAsyncResult ar1 = d.BeginInvoke (cb_state, ac, cb_state);\n");
		//
		//  Ende Aenderung Test
		//
		IAsyncResult ar1 = d.BeginInvoke (cb_state, ac, cb_state);
		//
		//  Beginn Aenderung Test
		//
		Console.WriteLine("\n\tNACH IAsyncResult ar1 = d.BeginInvoke (cb_state, ac, cb_state)\n");
		Console.WriteLine("\n\tVOR ar1.AsyncWaitHandle.WaitOne ();\n");
		//
		//  Ende Aenderung Test
		//
		ar1.AsyncWaitHandle.WaitOne ();
		//
		//  Beginn Aenderung Test
		//
		Console.WriteLine("\n\tNACH ar1.AsyncWaitHandle.WaitOne ();)\n");
		//
		//  Ende Aenderung Test
		//

		while (cb_state < 5)
			Thread.Sleep (200);

		return 0;
	}
}
