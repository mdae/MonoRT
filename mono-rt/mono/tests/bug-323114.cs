using System;

public enum Enum64 : long
{
	A = Int64.MaxValue,
}

delegate Enum64 EnumDelegate (Enum64 value);

class Test
{
	static Enum64 Method (Enum64 value)
	{
		return value;
	}

	static int Main ()
	{
		EnumDelegate d = new EnumDelegate (Method);
 		//
		//  Beginn Aenderung Test
		//
		Console.WriteLine("\n\t**** EnumDelegate d = new EnumDelegate (Method) done ****\n");
		//
		//Enum64 r = d.EndInvoke (d.BeginInvoke (Enum64.A, null, null));
		IAsyncResult ret = d.BeginInvoke (Enum64.A, null, null);
		Console.WriteLine("\n\t**** Delegate.BeginInvoke () done ****\n");
		Console.WriteLine("\n\t**** VOR Enum64 r = d.EndInvoke (ret) ****\n");
		Enum64 r = d.EndInvoke (ret);
		Console.WriteLine("\n\t**** NACH Enum64 r = d.EndInvoke (ret) ****\n");
		//  
		//  Bei synchronem Aufruf muss kein Wrapper generiert und compiliert werden.
		//
		//  Enum64 r = d(Enum64.A);
 		//
		//  Ende Aenderung Test
		//
		return r == Enum64.A ? 0 : 1;
	}
}
