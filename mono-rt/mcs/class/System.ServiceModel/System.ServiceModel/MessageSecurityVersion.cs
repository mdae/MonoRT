//
// MessageSecurityVersion.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2006 Novell, Inc.  http://www.novell.com
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;
using System.ServiceModel.Description;
using System.ServiceModel.Channels;
using System.ServiceModel.Security;
using System.ServiceModel.Security.Tokens;

namespace System.ServiceModel
{
	public abstract class MessageSecurityVersion
	{
		// Types
		class MessageSecurityTokenVersion : SecurityTokenVersion
		{
			static string [] specs10_profile_source, specs11_source, specs11_profile_source;
			static readonly MessageSecurityTokenVersion wss10basic, wss11, wss11basic;


			static MessageSecurityTokenVersion ()
			{
				specs10_profile_source = new string [] {
					Constants.WssNamespace,
					Constants.WstNamespace,
					Constants.WsscNamespace,
					Constants.WSBasicSecurityProfileCore1,
					};
				specs11_source = new string [] {
					Constants.Wss11Namespace,
					Constants.WstNamespace,
					Constants.WsscNamespace,
					};
				specs11_profile_source = new string [] {
					Constants.Wss11Namespace,
					Constants.WstNamespace,
					Constants.WsscNamespace,
					Constants.WSBasicSecurityProfileCore1,
					};

				wss10basic = new MessageSecurityTokenVersion (false, true);
				wss11basic = new MessageSecurityTokenVersion (true, true);
				wss11 = new MessageSecurityTokenVersion (true, false);
			}

			public static MessageSecurityTokenVersion GetVersion (bool isWss11, bool basicProfile)
			{
				if (isWss11)
					return basicProfile ? wss11basic : wss11;
				else
					return wss10basic;
			}

			ReadOnlyCollection<string> specs;

			MessageSecurityTokenVersion (bool wss11, bool basicProfile)
			{
				string [] src;
				if (wss11)
					src = basicProfile ? specs11_profile_source : specs11_source;
				else
					src = basicProfile ? specs10_profile_source : null;
				specs = new ReadOnlyCollection<string> (src);
			}

			public override ReadOnlyCollection<string> GetSecuritySpecifications ()
			{
				return specs;
			}
		}

		class MessageSecurityVersionImpl : MessageSecurityVersion
		{
			bool wss11, basic_profile;

			public MessageSecurityVersionImpl (bool wss11, bool basicProfile)
			{
				this.wss11 = wss11;
				this.basic_profile = basicProfile;
			}

			public override BasicSecurityProfileVersion BasicSecurityProfileVersion {
				get { return basic_profile ? BasicSecurityProfileVersion.BasicSecurityProfile10 : null; }
			}

			public override SecurityTokenVersion SecurityTokenVersion {
				get { return MessageSecurityTokenVersion.GetVersion (wss11, basic_profile); }
			}

			public override SecurityVersion SecurityVersion {
				get { return wss11 ? SecurityVersion.WSSecurity11 : SecurityVersion.WSSecurity10; }
			}
		}

		// Static members

		static MessageSecurityVersion wss10_basic, wss11, wss11_basic;

		static MessageSecurityVersion ()
		{
			wss10_basic = new MessageSecurityVersionImpl (false, true);
			wss11 = new MessageSecurityVersionImpl (true, false);
			wss11_basic = new MessageSecurityVersionImpl (true, true);
		}

		public static MessageSecurityVersion Default {
			get { return wss11; }
		}

		// guys, have you ever seen such silly member names??

		public static MessageSecurityVersion WSSecurity10WSTrustFebruary2005WSSecureConversationFebruary2005WSSecurityPolicy11BasicSecurityProfile10 {
			get { return wss10_basic; }
		}

		public static MessageSecurityVersion WSSecurity11WSTrustFebruary2005WSSecureConversationFebruary2005WSSecurityPolicy11 {
			get { return wss11; }
		}

		public static MessageSecurityVersion WSSecurity11WSTrustFebruary2005WSSecureConversationFebruary2005WSSecurityPolicy11BasicSecurityProfile10 {
			get { return wss11_basic; }
		}

		// Instance members

		MessageSecurityVersion ()
		{
		}

		public abstract BasicSecurityProfileVersion BasicSecurityProfileVersion { get; }

		public abstract SecurityTokenVersion SecurityTokenVersion { get; }

		public abstract SecurityVersion SecurityVersion { get; }
	}
}
