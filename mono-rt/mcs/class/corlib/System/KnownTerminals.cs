//
// System.KnownTerminals
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2005 Novell, Inc (http://www.novell.com)
//

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
// Arrays in this file are the contents of terminfo files dumped using the following program:
//
// using System;
// using System.IO;
// using System.Text;
// 
// public class DumpIntoArray {
// 	static int Main (string [] args)
// 	{
// 		if (args.Length != 2) {
// 			Console.Error.WriteLine ("Need array name and file name.");
// 			return 1;
// 		}
// 
// 		Console.WriteLine ("\t\tpublic static byte [] {0} = {{", args [0]);
// 		using (Stream st = File.OpenRead (args [1]))
// 			DumpStream (st);
// 
// 		Console.WriteLine ("\t\t};");
// 		return 0;
// 	}
// 
// 	static void DumpStream (Stream st)
// 	{
// 		byte [] buffer = new byte [4096];
// 		StringBuilder line = new StringBuilder ();
// 		int nread;
// 
// 		while ((nread = st.Read (buffer, 0, buffer.Length)) > 0) {
// 			for (int i = 0; i < nread; i++) {
// 				if (line.Length == 0)
// 					line.Append ("\t\t\t");
// 
// 				line.AppendFormat ("{0},", buffer [i]);
// 				if (line.Length > 100) {
// 					Console.WriteLine ("{0}", line);
// 					line.Length = 0;
// 				}
// 			}
// 		}
// 	}
// }
//

#if NET_2_0  || BOOTSTRAP_NET_2_0
namespace System
{
	static class KnownTerminals
	{
		public static byte [] linux {
			get { return new byte[] {
			26,1,20,0,29,0,16,0,125,1,41,3,108,105,110,117,120,124,108,105,110,117,120,32,99,111,110,115,111,
			108,101,0,0,1,0,0,1,1,0,0,0,0,0,0,0,1,1,0,0,0,0,0,1,0,0,0,0,0,0,1,1,0,255,255,8,0,255,255,255,255,
			255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,8,0,64,0,18,0,255,255,0,0,
			2,0,4,0,21,0,26,0,33,0,37,0,41,0,255,255,52,0,69,0,71,0,75,0,87,0,255,255,89,0,101,0,255,255,105,
			0,109,0,121,0,125,0,255,255,255,255,129,0,135,0,140,0,255,255,255,255,145,0,150,0,155,0,255,255,160,
			0,165,0,170,0,175,0,184,0,190,0,255,255,255,255,198,0,203,0,209,0,215,0,255,255,255,255,255,255,255,
			255,255,255,255,255,233,0,237,0,255,255,241,0,255,255,255,255,255,255,243,0,255,255,248,0,255,255,
			255,255,255,255,255,255,252,0,1,1,7,1,12,1,17,1,22,1,27,1,33,1,39,1,45,1,51,1,56,1,255,255,61,1,255,
			255,65,1,70,1,75,1,255,255,255,255,255,255,79,1,255,255,255,255,255,255,255,255,255,255,255,255,255,
			255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,83,1,255,255,86,1,95,1,255,255,
			104,1,255,255,113,1,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,
			255,255,122,1,255,255,255,255,255,255,128,1,131,1,142,1,145,1,147,1,150,1,247,1,255,255,250,1,255,
			255,255,255,255,255,255,255,255,255,255,255,252,1,255,255,255,255,255,255,255,255,0,2,255,255,65,
			2,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,
			255,255,255,255,255,255,69,2,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,
			255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,74,2,255,255,
			255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,
			255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,
			255,255,255,255,255,255,255,255,255,255,76,2,82,2,88,2,94,2,100,2,106,2,112,2,118,2,124,2,130,2,255,
			255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,
			255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,
			255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,
			255,255,255,255,255,255,255,255,255,255,136,2,255,255,255,255,255,255,255,255,255,255,255,255,255,
			255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,
			255,255,255,255,255,255,255,255,141,2,152,2,157,2,163,2,167,2,176,2,180,2,255,255,255,255,255,255,
			255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,
			255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,
			255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,
			255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,
			255,255,255,255,5,3,255,255,255,255,255,255,9,3,19,3,255,255,255,255,255,255,255,255,255,255,255,
			255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,
			29,3,35,3,7,0,13,0,27,91,37,105,37,112,49,37,100,59,37,112,50,37,100,114,0,27,91,51,103,0,27,91,72,
			27,91,74,0,27,91,75,0,27,91,74,0,27,91,37,105,37,112,49,37,100,71,0,27,91,37,105,37,112,49,37,100,
			59,37,112,50,37,100,72,0,10,0,27,91,72,0,27,91,63,50,53,108,27,91,63,49,99,0,8,0,27,91,63,50,53,104,
			27,91,63,48,99,0,27,91,67,0,27,91,65,0,27,91,63,50,53,104,27,91,63,56,99,0,27,91,80,0,27,91,77,0,
			27,91,49,49,109,0,27,91,53,109,0,27,91,49,109,0,27,91,50,109,0,27,91,52,104,0,27,91,56,109,0,27,91,
			55,109,0,27,91,55,109,0,27,91,52,109,0,27,91,37,112,49,37,100,88,0,27,91,49,48,109,0,27,91,48,59,
			49,48,109,0,27,91,52,108,0,27,91,50,55,109,0,27,91,50,52,109,0,27,91,63,53,104,27,91,63,53,108,36,
			60,50,48,48,47,62,0,27,91,64,0,27,91,76,0,127,0,27,91,51,126,0,27,91,66,0,27,91,91,65,0,27,91,50,
			49,126,0,27,91,91,66,0,27,91,91,67,0,27,91,91,68,0,27,91,91,69,0,27,91,49,55,126,0,27,91,49,56,126,
			0,27,91,49,57,126,0,27,91,50,48,126,0,27,91,49,126,0,27,91,50,126,0,27,91,68,0,27,91,54,126,0,27,
			91,53,126,0,27,91,67,0,27,91,65,0,13,10,0,27,91,37,112,49,37,100,80,0,27,91,37,112,49,37,100,77,0,
			27,91,37,112,49,37,100,64,0,27,91,37,112,49,37,100,76,0,27,99,27,93,82,0,27,56,0,27,91,37,105,37,
			112,49,37,100,100,0,27,55,0,10,0,27,77,0,27,91,48,59,49,48,37,63,37,112,49,37,116,59,55,37,59,37,
			63,37,112,50,37,116,59,52,37,59,37,63,37,112,51,37,116,59,55,37,59,37,63,37,112,52,37,116,59,53,37,
			59,37,63,37,112,53,37,116,59,50,37,59,37,63,37,112,54,37,116,59,49,37,59,37,63,37,112,55,37,116,59,
			56,37,59,37,63,37,112,57,37,116,59,49,49,37,59,109,0,27,72,0,9,0,27,91,71,0,43,16,44,17,45,24,46,
			25,48,219,96,4,97,177,102,248,103,241,104,176,105,206,106,217,107,191,108,218,109,192,110,197,111,
			126,112,196,113,196,114,196,115,95,116,195,117,180,118,193,119,194,120,179,121,243,122,242,123,227,
			124,216,125,156,126,254,0,27,91,90,0,27,91,52,126,0,26,0,27,91,50,51,126,0,27,91,50,52,126,0,27,91,
			50,53,126,0,27,91,50,54,126,0,27,91,50,56,126,0,27,91,50,57,126,0,27,91,51,49,126,0,27,91,51,50,126,
			0,27,91,51,51,126,0,27,91,51,52,126,0,27,91,49,75,0,27,91,37,105,37,100,59,37,100,82,0,27,91,54,110,
			0,27,91,63,54,99,0,27,91,99,0,27,91,51,57,59,52,57,109,0,27,93,82,0,27,93,80,37,112,49,37,120,37,
			112,50,37,123,50,53,54,125,37,42,37,123,49,48,48,48,125,37,47,37,48,50,120,37,112,51,37,123,50,53,
			54,125,37,42,37,123,49,48,48,48,125,37,47,37,48,50,120,37,112,52,37,123,50,53,54,125,37,42,37,123,
			49,48,48,48,125,37,47,37,48,50,120,0,27,91,77,0,27,91,51,37,112,49,37,100,109,0,27,91,52,37,112,49,
			37,100,109,0,27,91,49,49,109,0,27,91,49,48,109,0
			}; // linux
			}
		}

		public static byte [] xterm {
			get { return new byte[] {
			26,1,28,0,29,0,15,0,157,1,150,4,120,116,101,114,109,124,88,49,49,32,116,101,114,109,105,110,97,108,
			32,101,109,117,108,97,116,111,114,0,0,1,0,0,1,0,0,0,1,0,0,0,0,1,1,0,0,0,0,0,0,0,1,0,0,1,0,0,1,0,80,
			0,8,0,24,0,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,8,0,64,
			0,0,0,4,0,6,0,8,0,25,0,30,0,38,0,42,0,46,0,255,255,57,0,74,0,76,0,80,0,87,0,255,255,89,0,96,0,255,
			255,100,0,255,255,104,0,108,0,255,255,255,255,112,0,114,0,119,0,124,0,255,255,255,255,133,0,138,0,
			255,255,143,0,148,0,153,0,158,0,167,0,169,0,174,0,255,255,183,0,188,0,194,0,200,0,255,255,255,255,
			255,255,218,0,255,255,255,255,255,255,236,0,255,255,240,0,255,255,255,255,255,255,242,0,255,255,247,
			0,255,255,255,255,255,255,255,255,251,0,255,0,5,1,9,1,13,1,17,1,23,1,29,1,35,1,41,1,47,1,51,1,255,
			255,56,1,255,255,60,1,65,1,70,1,255,255,255,255,255,255,74,1,78,1,86,1,255,255,255,255,255,255,255,
			255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,94,1,
			103,1,112,1,121,1,255,255,130,1,139,1,148,1,255,255,157,1,255,255,255,255,255,255,166,1,170,1,175,
			1,255,255,180,1,183,1,255,255,255,255,201,1,204,1,215,1,218,1,220,1,223,1,45,2,255,255,48,2,255,255,
			255,255,255,255,255,255,255,255,255,255,50,2,255,255,255,255,255,255,255,255,54,2,255,255,107,2,255,
			255,255,255,111,2,117,2,255,255,255,255,123,2,255,255,255,255,255,255,255,255,255,255,255,255,255,
			255,255,255,130,2,134,2,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,
			255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,
			255,255,255,255,255,255,138,2,255,255,255,255,145,2,255,255,255,255,255,255,255,255,150,2,155,2,162,
			2,255,255,255,255,167,2,255,255,174,2,255,255,255,255,255,255,181,2,255,255,255,255,255,255,255,255,
			255,255,186,2,192,2,198,2,203,2,208,2,213,2,218,2,226,2,234,2,242,2,250,2,2,3,10,3,18,3,26,3,31,3,
			36,3,41,3,46,3,54,3,62,3,70,3,78,3,86,3,94,3,102,3,110,3,115,3,120,3,125,3,130,3,138,3,146,3,154,
			3,162,3,170,3,178,3,186,3,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,
			255,255,255,255,255,255,255,255,255,255,255,255,194,3,255,255,255,255,255,255,255,255,255,255,255,
			255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,
			255,255,255,255,255,255,255,255,255,255,199,3,210,3,215,3,223,3,227,3,255,255,255,255,255,255,255,
			255,236,3,50,4,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,
			255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,
			255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,
			255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,
			255,255,255,255,255,255,120,4,255,255,255,255,255,255,124,4,134,4,255,255,255,255,255,255,255,255,
			255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,
			255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,
			255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,
			255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,144,4,147,4,27,91,90,0,7,0,13,
			0,27,91,37,105,37,112,49,37,100,59,37,112,50,37,100,114,0,27,91,51,103,0,27,91,72,27,91,50,74,0,27,
			91,75,0,27,91,74,0,27,91,37,105,37,112,49,37,100,71,0,27,91,37,105,37,112,49,37,100,59,37,112,50,
			37,100,72,0,10,0,27,91,72,0,27,91,63,50,53,108,0,8,0,27,91,63,50,53,104,0,27,91,67,0,27,91,65,0,27,
			91,80,0,27,91,77,0,14,0,27,91,53,109,0,27,91,49,109,0,27,91,63,49,48,52,57,104,0,27,91,52,104,0,27,
			91,56,109,0,27,91,55,109,0,27,91,55,109,0,27,91,52,109,0,27,91,37,112,49,37,100,88,0,15,0,27,91,109,
			15,0,27,91,63,49,48,52,57,108,0,27,91,52,108,0,27,91,50,55,109,0,27,91,50,52,109,0,27,91,63,53,104,
			36,60,49,48,48,47,62,27,91,63,53,108,0,27,91,33,112,27,91,63,51,59,52,108,27,91,52,108,27,62,0,27,
			91,76,0,127,0,27,91,51,126,0,27,79,66,0,27,79,80,0,27,91,50,49,126,0,27,79,81,0,27,79,82,0,27,79,
			83,0,27,91,49,53,126,0,27,91,49,55,126,0,27,91,49,56,126,0,27,91,49,57,126,0,27,91,50,48,126,0,27,
			79,72,0,27,91,50,126,0,27,79,68,0,27,91,54,126,0,27,91,53,126,0,27,79,67,0,27,79,65,0,27,91,63,49,
			108,27,62,0,27,91,63,49,104,27,61,0,27,91,37,112,49,37,100,80,0,27,91,37,112,49,37,100,77,0,27,91,
			37,112,49,37,100,66,0,27,91,37,112,49,37,100,64,0,27,91,37,112,49,37,100,76,0,27,91,37,112,49,37,
			100,68,0,27,91,37,112,49,37,100,67,0,27,91,37,112,49,37,100,65,0,27,91,105,0,27,91,52,105,0,27,91,
			53,105,0,27,99,0,27,91,33,112,27,91,63,51,59,52,108,27,91,52,108,27,62,0,27,56,0,27,91,37,105,37,
			112,49,37,100,100,0,27,55,0,10,0,27,77,0,27,91,48,37,63,37,112,54,37,116,59,49,37,59,37,63,37,112,
			50,37,116,59,52,37,59,37,63,37,112,49,37,112,51,37,124,37,116,59,55,37,59,37,63,37,112,52,37,116,
			59,53,37,59,37,63,37,112,55,37,116,59,56,37,59,109,37,63,37,112,57,37,116,14,37,101,15,37,59,0,27,
			72,0,9,0,27,79,69,0,96,96,97,97,102,102,103,103,105,105,106,106,107,107,108,108,109,109,110,110,111,
			111,112,112,113,113,114,114,115,115,116,116,117,117,118,118,119,119,120,120,121,121,122,122,123,123,
			124,124,125,125,126,126,0,27,91,90,0,27,91,63,55,104,0,27,91,63,55,108,0,27,40,66,27,41,48,0,27,79,
			70,0,27,79,77,0,27,91,51,59,53,126,0,27,79,53,70,0,27,79,53,72,0,27,91,50,59,53,126,0,27,79,53,68,
			0,27,91,54,59,53,126,0,27,91,53,59,53,126,0,27,79,53,67,0,27,91,50,51,126,0,27,91,50,52,126,0,27,
			79,50,80,0,27,79,50,81,0,27,79,50,82,0,27,79,50,83,0,27,91,49,53,59,50,126,0,27,91,49,55,59,50,126,
			0,27,91,49,56,59,50,126,0,27,91,49,57,59,50,126,0,27,91,50,48,59,50,126,0,27,91,50,49,59,50,126,0,
			27,91,50,51,59,50,126,0,27,91,50,52,59,50,126,0,27,79,53,80,0,27,79,53,81,0,27,79,53,82,0,27,79,53,
			83,0,27,91,49,53,59,53,126,0,27,91,49,55,59,53,126,0,27,91,49,56,59,53,126,0,27,91,49,57,59,53,126,
			0,27,91,50,48,59,53,126,0,27,91,50,49,59,53,126,0,27,91,50,51,59,53,126,0,27,91,50,52,59,53,126,0,
			27,79,54,80,0,27,79,54,81,0,27,79,54,82,0,27,79,54,83,0,27,91,49,53,59,54,126,0,27,91,49,55,59,54,
			126,0,27,91,49,56,59,54,126,0,27,91,49,57,59,54,126,0,27,91,50,48,59,54,126,0,27,91,50,49,59,54,126,
			0,27,91,50,51,59,54,126,0,27,91,50,52,59,54,126,0,27,91,49,75,0,27,91,37,105,37,100,59,37,100,82,
			0,27,91,54,110,0,27,91,63,49,59,50,99,0,27,91,99,0,27,91,51,57,59,52,57,109,0,27,91,51,37,63,37,112,
			49,37,123,49,125,37,61,37,116,52,37,101,37,112,49,37,123,51,125,37,61,37,116,54,37,101,37,112,49,
			37,123,52,125,37,61,37,116,49,37,101,37,112,49,37,123,54,125,37,61,37,116,51,37,101,37,112,49,37,
			100,37,59,109,0,27,91,52,37,63,37,112,49,37,123,49,125,37,61,37,116,52,37,101,37,112,49,37,123,51,
			125,37,61,37,116,54,37,101,37,112,49,37,123,52,125,37,61,37,116,49,37,101,37,112,49,37,123,54,125,
			37,61,37,116,51,37,101,37,112,49,37,100,37,59,109,0,27,91,77,0,27,91,51,37,112,49,37,100,109,0,27,
			91,52,37,112,49,37,100,109,0,27,108,0,27,109,0
			}; // xterm
			}
		}

		public static byte [] ansi {
			get { return new byte [] {
			26,1,40,0,23,0,16,0,125,1,68,2,97,110,115,105,124,97,110,115,105,47,112,99,45,116,101,114,109,32,
			99,111,109,112,97,116,105,98,108,101,32,119,105,116,104,32,99,111,108,111,114,0,0,1,0,0,0,0,0,0,0,
			0,0,0,0,1,1,0,0,0,0,0,0,0,1,0,80,0,8,0,24,0,255,255,255,255,255,255,255,255,255,255,255,255,255,255,
			255,255,255,255,255,255,8,0,64,0,3,0,0,0,4,0,6,0,255,255,8,0,13,0,20,0,24,0,28,0,255,255,39,0,56,
			0,60,0,255,255,64,0,255,255,255,255,68,0,255,255,72,0,255,255,76,0,80,0,255,255,255,255,84,0,90,0,
			95,0,255,255,255,255,255,255,255,255,100,0,255,255,105,0,110,0,115,0,120,0,129,0,135,0,255,255,255,
			255,255,255,143,0,147,0,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,151,0,255,
			255,155,0,255,255,255,255,255,255,255,255,255,255,157,0,255,255,255,255,255,255,255,255,255,255,255,
			255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,161,0,165,0,255,255,169,0,255,
			255,255,255,255,255,173,0,255,255,255,255,255,255,177,0,255,255,255,255,255,255,255,255,255,255,255,
			255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,181,0,255,255,186,0,195,
			0,204,0,213,0,222,0,231,0,240,0,249,0,2,1,11,1,255,255,255,255,255,255,255,255,20,1,25,1,30,1,255,
			255,255,255,255,255,255,255,255,255,50,1,255,255,61,1,255,255,63,1,149,1,255,255,152,1,255,255,255,
			255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,156,1,255,255,219,1,255,
			255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,
			255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,
			255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,
			255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,
			255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,
			255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,
			255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,
			255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,
			255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,
			255,255,255,255,255,255,255,255,255,255,255,255,255,255,223,1,255,255,255,255,255,255,255,255,255,
			255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,
			255,255,255,255,255,255,255,255,255,255,255,255,228,1,239,1,244,1,7,2,11,2,255,255,255,255,255,255,
			255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,
			255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,
			255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,
			255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,
			255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,20,2,30,2,255,255,255,255,255,255,
			40,2,44,2,48,2,52,2,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,
			255,255,56,2,62,2,27,91,90,0,7,0,13,0,27,91,50,103,0,27,91,72,27,91,74,0,27,91,75,0,27,91,74,0,27,
			91,37,105,37,112,49,37,100,71,0,27,91,37,105,37,112,49,37,100,59,37,112,50,37,100,72,0,27,91,66,0,
			27,91,72,0,27,91,68,0,27,91,67,0,27,91,65,0,27,91,80,0,27,91,77,0,27,91,49,49,109,0,27,91,53,109,
			0,27,91,49,109,0,27,91,56,109,0,27,91,55,109,0,27,91,55,109,0,27,91,52,109,0,27,91,37,112,49,37,100,
			88,0,27,91,49,48,109,0,27,91,48,59,49,48,109,0,27,91,109,0,27,91,109,0,27,91,76,0,8,0,27,91,66,0,
			27,91,72,0,27,91,76,0,27,91,68,0,27,91,67,0,27,91,65,0,13,27,91,83,0,27,91,37,112,49,37,100,80,0,
			27,91,37,112,49,37,100,77,0,27,91,37,112,49,37,100,66,0,27,91,37,112,49,37,100,64,0,27,91,37,112,
			49,37,100,83,0,27,91,37,112,49,37,100,76,0,27,91,37,112,49,37,100,68,0,27,91,37,112,49,37,100,67,
			0,27,91,37,112,49,37,100,84,0,27,91,37,112,49,37,100,65,0,27,91,52,105,0,27,91,53,105,0,37,112,49,
			37,99,27,91,37,112,50,37,123,49,125,37,45,37,100,98,0,27,91,37,105,37,112,49,37,100,100,0,10,0,27,
			91,48,59,49,48,37,63,37,112,49,37,116,59,55,37,59,37,63,37,112,50,37,116,59,52,37,59,37,63,37,112,
			51,37,116,59,55,37,59,37,63,37,112,52,37,116,59,53,37,59,37,63,37,112,54,37,116,59,49,37,59,37,63,
			37,112,55,37,116,59,56,37,59,37,63,37,112,57,37,116,59,49,49,37,59,109,0,27,72,0,27,91,73,0,43,16,
			44,17,45,24,46,25,48,219,96,4,97,177,102,248,103,241,104,176,106,217,107,191,108,218,109,192,110,
			197,111,126,112,196,113,196,114,196,115,95,116,195,117,180,118,193,119,194,120,179,121,243,122,242,
			123,227,124,216,125,156,126,254,0,27,91,90,0,27,91,49,75,0,27,91,37,105,37,100,59,37,100,82,0,27,
			91,54,110,0,27,91,63,37,91,59,48,49,50,51,52,53,54,55,56,57,93,99,0,27,91,99,0,27,91,51,57,59,52,
			57,109,0,27,91,51,37,112,49,37,100,109,0,27,91,52,37,112,49,37,100,109,0,27,40,66,0,27,41,66,0,27,
			42,66,0,27,43,66,0,27,91,49,49,109,0,27,91,49,48,109,0
			}; // ansi
			}
		}
	}
}
#endif

