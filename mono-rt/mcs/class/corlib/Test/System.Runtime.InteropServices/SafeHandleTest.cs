//
// System.Runtime.InteropServices.SafeHandle Test Cases
//
// Authors:
// 	Miguel de Icaza (miguel@novell.com)
//
// Copyright (C) 2004-2006 Novell, Inc (http://www.novell.com)
//
#if NET_2_0
using NUnit.Framework;
using System;
using System.Runtime.InteropServices;
using System.Security;
using Microsoft.Win32.SafeHandles;

namespace MonoTests.System.Runtime.InteropServices
{
	[TestFixture]
	public class SafeHandleTest 
	{
		//
		// This mimics SafeFileHandle, but does not actually own a handle
		// We use this to test ownership and dispose exceptions.
		//
		public class FakeSafeHandle : SafeHandleZeroOrMinusOneIsInvalid
		{
			public bool released = false;
			
			public FakeSafeHandle (): base (true)
			{
			}
			
			public FakeSafeHandle (bool ownership) : base (ownership)
			{
			}

			protected override bool ReleaseHandle ()
			{
				released = true;
				return true;
			}
		}
		
		[Test]
		[ExpectedException (typeof (ObjectDisposedException))]
		public void Dispose1 ()
		{
			FakeSafeHandle sf = new FakeSafeHandle ();

			sf.DangerousRelease ();
			sf.DangerousRelease ();
		}

		[Test]
		[ExpectedException (typeof (ObjectDisposedException))]
		public void Dispose2 ()
		{
			FakeSafeHandle sf = new FakeSafeHandle ();

			sf.DangerousRelease ();
			sf.Close ();
		}

		[Test]
		[ExpectedException (typeof (ObjectDisposedException))]
		public void Dispose3 ()
		{
			FakeSafeHandle sf = new FakeSafeHandle ();

			sf.Close ();
			sf.DangerousRelease ();
		}

		[Test]
		public void NoReleaseUnowned ()
		{
			FakeSafeHandle sf = new FakeSafeHandle (false);

			sf.Close ();
			Assert.AreEqual (sf.released, false, "r1");

			sf = new FakeSafeHandle (false);
			sf.DangerousRelease ();
			Assert.AreEqual (sf.released, false, "r2");

			sf = new FakeSafeHandle (false);
			((IDisposable) sf).Dispose ();
			Assert.AreEqual (sf.released, false, "r3");
		}
	}
}

#endif
