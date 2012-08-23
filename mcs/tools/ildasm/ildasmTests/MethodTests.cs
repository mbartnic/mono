// 
// MethodTests.cs
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
	public class MethodTests : DisassemblerTester {
		Mono.ILAsm.Tests.MethodTests t = new Mono.ILAsm.Tests.MethodTests ();
		
		[Test]
		public void TestSimpleMethod ()
		{
			t.TestSimpleMethod ();
			t.TestSimpleMethod (ILDism()
				.Input(t.LastAssembledFile)
				.Run().OutputFileName);
		}
		
		[Test]
		public void TestEmptyMethod ()
		{
			t.TestEmptyMethod ();
			t.TestEmptyMethod (ILDism()
				.Input(t.LastAssembledFile)
				.Run().OutputFileName);
		}
		
		[Test]
		public void TestParametrizedMethod ()
		{
			t.TestParametrizedMethod ();
			t.TestParametrizedMethod (ILDism()
				.Input(t.LastAssembledFile)
				.Run().OutputFileName);
		}
		
		[Test]
		public void TestGenericMethod ()
		{
			t.TestGenericMethod ();
			t.TestGenericMethod (ILDism()
				.Input(t.LastAssembledFile)
				.Run().OutputFileName);
		}
		
		[Test]
		public void TestGenericMethodWithGenericReturnType ()
		{
			t.TestGenericMethodWithGenericReturnType ();
			t.TestGenericMethodWithGenericReturnType (ILDism()
				.Input(t.LastAssembledFile)
				.Run().OutputFileName);
		}
		
		[Test]
		public void TestGenericMethodWithGenericParameterType ()
		{
			t.TestGenericMethodWithGenericParameterType ();
			t.TestGenericMethodWithGenericParameterType (ILDism()
				.Input(t.LastAssembledFile)
				.Run().OutputFileName);
		}
		
		[Test]
		public void TestMethodLocalsWithInit ()
		{
			t.TestMethodLocalsWithInit ();
			t.TestMethodLocalsWithInit (ILDism()
				.Input(t.LastAssembledFile)
				.Run().OutputFileName);
		}
		
		[Test]
		public void TestMethodWithZeroInit ()
		{
			t.TestMethodWithZeroInit ();
			t.TestMethodWithZeroInit (ILDism()
				.Input(t.LastAssembledFile)
				.Run().OutputFileName);
		}
		
		[Test]
		public void TestEntryPointMethod ()
		{
			t.TestEntryPointMethod ();
			t.TestEntryPointMethod (ILDism()
				.Input(t.LastAssembledFile)
				.Run().OutputFileName);
		}
		
		[Test]
		public void TestNoOperandInstructions ()
		{
			t.TestNoOperandInstructions ();
			t.TestNoOperandInstructions (ILDism()
				.Input(t.LastAssembledFile)
				.Run().OutputFileName);
		}
		
		[Test]
		public void TestParameterOperand ()
		{
			t.TestParameterOperand ();
			t.TestParameterOperand (ILDism()
				.Input(t.LastAssembledFile)
				.Run().OutputFileName);
		}

	}
}

