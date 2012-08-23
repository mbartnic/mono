// 
// AssemblyTests.cs
//  
// Author:
//       mbartnic <${AuthorEmail}>
// 
// Copyright (c) 2012 mbartnic
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
using NUnit.Framework;

namespace Mono.ILDasm.Tests
{
	[TestFixture]
	public sealed class AssemblyTests : DisassemblerTester	{
		
		Mono.ILAsm.Tests.AssemblyTests t = new Mono.ILAsm.Tests.AssemblyTests();
		
		[Test]
		public void TestEmptyAssemblyDirective ()
		{
			t.TestEmptyAssemblyDirective ();
			t.TestEmptyAssemblyDirective (ILDism()
				.Input(t.LastAssembledFile)
				.Run().OutputFileName);
		}
		
		[Test]
		public void TestFullAssemblyDirective ()
		{
			t.TestFullAssemblyDirective ();
			t.TestFullAssemblyDirective (ILDism()
				.Input(t.LastAssembledFile)
				.Run().OutputFileName);
		}
		
		[Test]
		public void TestRawLocale ()
		{
			t.TestRawLocale ();
			t.TestRawLocale (ILDism()
				.Input(t.LastAssembledFile)
				.Run().OutputFileName);
		}
		
		[Test]
		public void TestMultipleAssemblyDirectives ()
		{
			t.TestMultipleAssemblyDirectives ();
			t.TestMultipleAssemblyDirectives (ILDism()
				.Input(t.LastAssembledFile)
				.Run().OutputFileName);
		}
		
		[Test]
		public void TestRetargetableAssembly ()
		{
			t.TestRetargetableAssembly ();
			t.TestRetargetableAssembly (ILDism()
				.Input(t.LastAssembledFile)
				.Run().OutputFileName);
		}
		
		[Test]
		public void TestMiscellaneousAssemblyAttributes ()
		{
			t.TestMiscellaneousAssemblyAttributes ();
			t.TestMiscellaneousAssemblyAttributes (ILDism()
				.Input(t.LastAssembledFile)
				.Run().OutputFileName);
		}
		
		[Test]
		public void TestInvalidAssemblyHashAlgorithm ()
		{
			t.TestInvalidAssemblyHashAlgorithm ();
			t.TestInvalidAssemblyHashAlgorithm (ILDism()
				.Input(t.LastAssembledFile)
				.Run().OutputFileName);
		}
		
		[Test]
		public void TestEmptyAssemblyExternDirective ()
		{
			t.TestEmptyAssemblyExternDirective ();
			t.TestEmptyAssemblyExternDirective (ILDism()
				.Input(t.LastAssembledFile)
				.Run().OutputFileName);
		}
		
		[Test]
		public void TestFullAssemblyExternDirective ()
		{
			t.TestFullAssemblyExternDirective ();
			t.TestFullAssemblyExternDirective (ILDism()
				.Input(t.LastAssembledFile)
				.Run().OutputFileName);
		}
		
		[Test]
		public void TestAssemblyExternDirectiveWithPublicKey ()
		{
			t.TestAssemblyExternDirectiveWithPublicKey ();
			t.TestAssemblyExternDirectiveWithPublicKey (ILDism()
				.Input(t.LastAssembledFile)
				.Run().OutputFileName);
		}
		
		[Test]
		public void TestAssemblyExternDirectiveWithPublicKeyAndToken ()
		{
			t.TestAssemblyExternDirectiveWithPublicKeyAndToken ();
			t.TestAssemblyExternDirectiveWithPublicKeyAndToken (ILDism()
				.Input(t.LastAssembledFile)
				.Run().OutputFileName);
		}
		
		[Test]
		public void TestShadowedAssemblyExternDirective ()
		{
			t.TestShadowedAssemblyExternDirective ();
			t.TestShadowedAssemblyExternDirective (ILDism()
				.Input(t.LastAssembledFile)
				.Run().OutputFileName);
		}
		
		[Test]
		public void TestShadowedAssemblyExternAsDirective ()
		{
			t.TestShadowedAssemblyExternAsDirective ();
			t.TestShadowedAssemblyExternAsDirective (ILDism()
				.Input(t.LastAssembledFile)
				.Run().OutputFileName);
		}
		
		[Test]
		public void TestLocaleBytes ()
		{
			t.TestLocaleBytes ();
			t.TestLocaleBytes (ILDism()
				.Input(t.LastAssembledFile)
				.Run().OutputFileName);
		}
	}
}

