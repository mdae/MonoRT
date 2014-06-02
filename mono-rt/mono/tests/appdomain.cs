using System;
using System.Security.Policy;
using System.Threading;

class Container {

	[LoaderOptimization (LoaderOptimization.SingleDomain)]
	static int arg_sum (string[] args) {
		int res = 0;
		foreach (string s in args) {
			res += Convert.ToInt32 (s);
		}
		return res;
	}
	
	static int Main ()
	{
		int res;
		
		AppDomainSetup setup = new AppDomainSetup ();
		setup.ApplicationBase = ".";

		Console.WriteLine (AppDomain.CurrentDomain.FriendlyName);
		//
		//  Beginn Aenderung Test
		//
		Console.WriteLine("\n\tVOR AppDomain.CreateDomain()\n");
//		MonitoringClass.setMonitoringOptions(false, false, false);
		//
		//  Ende Aenderung Test
		//
		AppDomain newDomain = AppDomain.CreateDomain ("NewDomain", null, setup);
		//
		//  Beginn Aenderung Test
		//
		
		Console.WriteLine("\n\tNACH AppDomain.CreateDomain()\n");
		//
		//  Ende Aenderung Test
		//
		string[] args = { "1", "2", "3"};		
		res = newDomain.ExecuteAssembly ("appdomain-client.exe", null, args);
		if (res != arg_sum (args))
			return 1;

//		MonitoringClass.setMonitoringOptions(true, true, true);
		Console.WriteLine ("test-ok");

		return 0;
	}
}
