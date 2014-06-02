//
//  bug-47295.cs:
//
//    Regression test for bug #47295.
//
//  Test from Marcus Urban (mathpup@mylinuxisp.com)
//   

using System; 
using System.Reflection; 
using System.Reflection.Emit; 
using System.Runtime.InteropServices; 
 
 
public class Testing 
{ 
    public static void Method(int value) 
    { 
        Console.WriteLine( "Method( {0} )", value ); 
    } 
 
 
    [StructLayout(LayoutKind.Sequential)] 
    internal struct DelegateList 
    { 
        internal Delegate del; 
    } 
 
 
    public static void Main() 
    { 
        // Create a dynamic assembly and module to contain the 
        // subclass of MulticastDelegate that we will create 
 
        AssemblyName asmName = new AssemblyName(); 
        asmName.Name = "DynamicAssembly"; 
 
        AssemblyBuilder asmBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(asmName, AssemblyBuilderAccess.Run); 
 
        ModuleBuilder modBuilder = asmBuilder.DefineDynamicModule ( "DynamicModule" );
 
        TypeBuilder typeBuilder = modBuilder.DefineType( "MyType", TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.Sealed, typeof( System.MulticastDelegate ) );

        ConstructorBuilder cb = typeBuilder.DefineConstructor(MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.RTSpecialName | MethodAttributes.SpecialName, CallingConventions.Standard, new Type[] { typeof(Object), typeof (IntPtr) } ); 
 
        cb.SetImplementationFlags( MethodImplAttributes.Runtime | MethodImplAttributes.Managed ); 
 
        MethodBuilder mb = typeBuilder.DefineMethod( "Invoke", MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.HideBySig, typeof(void), new Type[] { typeof(int) } ); 
 
        mb.SetImplementationFlags( MethodImplAttributes.Runtime | MethodImplAttributes.Managed );

	ParameterBuilder pb = mb.DefineParameter (1, ParameterAttributes.HasFieldMarshal, "foo");
	pb.SetMarshal (UnmanagedMarshal.DefineUnmanagedMarshal (UnmanagedType.I2));
 
        // Create an instance of the delegate type and invoke it -- just to test 

        Type myDelegateType = typeBuilder.CreateType();
 	//
	//  Beginn Aenderung Test
	//
	Console.WriteLine ("\n\t*** VOR Delegate.CreateDelegate() ***\n");
	//
	//  Ende Aenderung Test
	//
        Delegate d = Delegate.CreateDelegate( myDelegateType, typeof( Testing ), "Method" );
 	//
	//  Beginn Aenderung Test
	//
	Console.WriteLine ("\n\t*** NACH Delegate.CreateDelegate() ***\n");
	Console.WriteLine ("\n\t*** VOR d.DynamicInvoke( new object[] { 8 } ) ***\n");
	//
	//  Ende Aenderung Test
	//
        d.DynamicInvoke( new object[] { 8 } );
 	//
	//  Beginn Aenderung Test
	//
 	Console.WriteLine ("\n\t*** NACH d.DynamicInvoke( new object[] { 8 } ) ***\n");
	//
	//  Ende Aenderung Test
	//
        DelegateList delegateList = new DelegateList();
        delegateList.del = d;
        IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf(delegateList)); 
 
        // The execption seems to occur at this statement:
 	//
	//  Beginn Aenderung Test
	//
 	Console.WriteLine ("\n\t*** VOR Marshal.StructureToPtr( delegateList, ptr, false ) ***\n");
	//
	//  Ende Aenderung Test
	//
        Marshal.StructureToPtr(delegateList, ptr, false);
 	//
	//  Beginn Aenderung Test
	//
 	Console.WriteLine ("\n\t*** NACH Marshal.StructureToPtr( delegateList, ptr, false ) ***\n");
	//
	//  Ende Aenderung Test
	//
    } 
 
} 
