using System;
using System.IO;
using System.Web;
using System.Web.Hosting;

class TinyHost : MarshalByRefObject
{
	public static TinyHost CreateHost ()
	{
		string path = Directory.GetCurrentDirectory ();
		string bin = Path.Combine (path, "bin");
		string asm = Path.GetFileName (typeof (TinyHost).Assembly.Location);

		Directory.CreateDirectory (bin);
		File.Copy (asm, Path.Combine (bin, asm), true);
		//
		//  Beginn Aenderung Test
		//
		//return (TinyHost) ApplicationHost.CreateApplicationHost (
		//	typeof (TinyHost), "/", path);
		Console.WriteLine("\n\t****C#: VOR ApplicationHost.CreateApplicationHost () ****\n");
		TinyHost th = (TinyHost) ApplicationHost.CreateApplicationHost (typeof (TinyHost), "/", path);
		Console.WriteLine("\n\t****C#: NACH ApplicationHost.CreateApplicationHost () ****\n");
		return th;
		//
		//  Ende Aenderung Test
		//

	}

	public void Execute (string page)
	{
		SimpleWorkerRequest req = new SimpleWorkerRequest (page, "", Console.Out);
		HttpRuntime.ProcessRequest (req);
	}

	static void Main ()
	{
		TinyHost h = CreateHost ();
		//
		//  Beginn Aenderung Test
		//
		Console.WriteLine("\n\t****C#: TinyHost h = CreateHost () done ****\n");
		//
		//  Ende Aenderung Test
		//
		StreamWriter w = new StreamWriter ("page.aspx");
		w.WriteLine (@"<%@ Page Language=""C#"" %>");
		w.WriteLine (@"<% Console.WriteLine(""Hello""); %>");
		w.Close ();
		h.Execute ("page.aspx");
	}
}
