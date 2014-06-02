using System;
using System.Reflection;

namespace MonoBug
{
        public class Program
        {
                static private int Main(string[] args)
                {
                        Assembly assembly = Assembly.GetExecutingAssembly ();
                        Type type = assembly.GetType("MonoBug.Program", true);
                        MethodInfo info = type.GetMethod("Foo");
			//
			//  Beginn Aenderung Test
			//
			Console.WriteLine("\n\t****C#: VOR info.GetCustomAttributes (false) ****\n");
			//
			//  Ende Aenderung Test
			//
                        object[] attributes = info.GetCustomAttributes (false);
			//
			//  Beginn Aenderung Test
			//
			Console.WriteLine("\n\t****C#: NACH info.GetCustomAttributes (false) ****\n");
			//
			//  Ende Aenderung Test
			//
			int found = 0;
                        foreach (object obj in attributes)
                        {
                                Console.WriteLine("Attribute of type {0} found", obj.GetType().ToString());
				found ++;
                        }
			return found == 1? 0: 1;
                }

                [My("blah", new string[] { "crash" }, "additional parameter")]
                public void Foo()
                {
                }
        }

        [AttributeUsage(AttributeTargets.Method)]
        class MyAttribute : Attribute
        {
                public MyAttribute(params object[] arguments)
                {
                }
        }
}
