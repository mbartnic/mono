// 
// CustomAttributeTests.cs
//  
// Author:
//       Alex Rønne Petersen <xtzgzorex@gmail.com>
// 
// Copyright (c) 2011 Alex Rønne Petersen
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
using System;
using System.Text;
using NUnit.Framework;

namespace Mono.ILAsm.Tests {
	[TestFixture]
	public class CustomAttributeTests : AssemblerTester {
		[Test]
		public void TestSimpleAttribute (string defaultInput = "custom/custom-001.il")
		{
			ILAsm ()
				.Input (defaultInput)
				.Run ()
				.Expect (ExitCode.Success)
				.GetModule ()
				.Expect (x => x.CustomAttributes.ContainsOne (
					y => y.AttributeType.FullName == "System.Attribute"));
		}
		
		[Test]
		public void TestAttributeByteBlob (string defaultInput = "custom/custom-002.il")
		{
			ILAsm ()
				.Input (defaultInput)
				.Run ()
				.Expect (ExitCode.Success)
				.GetModule ()
				.Expect (x => x.CustomAttributes.ContainsOne (
					y => y.GetBlob ().ListEquals (new byte[] {
						0x00, 0x01, 0x02, 0x03
					})));
		}
		
		[Test]
		public void TestAttributeStringBlob (string defaultInput = "custom/custom-003.il")
		{
			ILAsm ()
				.Input (defaultInput)
				.Run ()
				.Expect (ExitCode.Success)
				.GetModule ()
				.Expect (x => x.CustomAttributes.ContainsOne (
					y => Encoding.Unicode.GetString (y.GetBlob ()) == "Hello World"));
		}
	}
}
