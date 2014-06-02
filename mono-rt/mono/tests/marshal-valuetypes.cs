using System;

public class Test: MarshalByRefObject
{
        public DateTime Stamp = new DateTime (1968, 1, 2);
	public double perc = 5.4;

        static int Main ()
        {
		//
		//  Beginn Aenderung Test
		//
                AppDomain d = AppDomain.CreateDomain ("foo");
                Test t = (Test) d.CreateInstanceAndUnwrap (typeof (Test).Assembly.FullName, typeof (Test).FullName);
		//Test t = new Test();
		//
		//  Ende Aenderung Test
		//
		if (t.Stamp != new DateTime (1968, 1, 2))
			return 1;
		t.perc = 7.2;
		if (t.perc != 7.2)
			return 2;
		return 0;
        }
}
