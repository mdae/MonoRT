using System;
using System.Threading;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Activation;
using System.Runtime.Remoting.Contexts;
using System.Runtime.Remoting.Messaging;

public class MyContextAttribute: Attribute, IContextAttribute  {
	public void GetPropertiesForNewContext (IConstructionCallMessage msg)
	{
	}

	public bool IsContextOK (Context ctx, IConstructionCallMessage msg)
	{
		//
		//  Beginn Aenderung Test
		//
		Console.WriteLine("\n\t****C#: bool IsContextOK (Context ctx, IConstructionCallMessage msg) ****\n");
		//
		//  Ende Aenderung Test
		//
		return false;
	}
}

// CBO class whose objects are always in the correct context
class UnlockedCbo : ContextBoundObject {
	public int Counter;


	void Inc (ref int a)
	{
		a++;
	}

	public void Inc ()
	{
		Inc (ref Counter);
	}
}

// CBO class whose objects are always out of context
[MyContext]
class LockedCbo : UnlockedCbo {
}

class Mbr : MarshalByRefObject {
	public int Counter;

	void Inc (ref int a)
	{
		a++;
	}

	public void Inc ()
	{
		Inc (ref Counter);
	}
}

class Test {
	static int Main ()
	{
		// warning CS0197 is expected several times
		
		UnlockedCbo uc = new UnlockedCbo ();
		Interlocked.Increment (ref uc.Counter);
		uc.Inc ();
		//
		//  Beginn Aenderung Test
		//
		Console.WriteLine("\n\t****C#: VOR LockedCbo lc = new LockedCbo () ****\n");
		//
		//  Ende Aenderung Test
		//
		LockedCbo lc = new LockedCbo ();
		//
		//  Beginn Aenderung Test
		//
		Console.WriteLine("\n\t****C#: NACH LockedCbo lc = new LockedCbo () ****\n");
		//
		//  Ende Aenderung Test
		//
		try {	
			//
			//  Beginn Aenderung Test
			//
			Console.WriteLine("\n\t****C#: VOR Interlocked.Increment (ref lc.Counter) ****\n");
			//
			//  Ende Aenderung Test
			//
			Interlocked.Increment (ref lc.Counter);
			return 1;
		} catch (InvalidOperationException e) {
			//
			//  Beginn Aenderung Test
			//
			Console.WriteLine(e.ToString());
			//
			//  Ende Aenderung Test
			//
		}

		lc.Inc ();

		if (lc.Counter != 1)
			return 2;

		Mbr m = new Mbr ();
		Interlocked.Increment (ref m.Counter);
		m.Inc ();

		if (m.Counter != 2)
			return 3;

		Mbr rm = (Mbr) CreateRemote (typeof (Mbr));
		try {
			Interlocked.Increment (ref rm.Counter);
			return 4;
		} catch (InvalidOperationException) {
		}

		rm.Inc ();

		if (rm.Counter != 1)
			return 5;

		return 0;
	}

	static object CreateRemote (Type t)
	{
		AppDomain d = AppDomain.CreateDomain ("foo");
		return d.CreateInstanceAndUnwrap (t.Assembly.FullName, t.FullName);
	}
}
