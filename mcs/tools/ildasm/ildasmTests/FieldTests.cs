// 
// FieldTests.cs
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
	public class FieldTests : DisassemblerTester
	{
		Mono.ILAsm.Tests.FieldTests t = new Mono.ILAsm.Tests.FieldTests ();
		
		[Test]
		public void TestStaticModuleField ()
		{
			t.TestStaticModuleField ();
			t.TestStaticModuleField (ILDism()
				.Input(t.LastAssembledFile)
				.Run().OutputFileName);
		}
		
		[Test]
		public void TestModuleField ()
		{
			t.TestModuleField ();
			t.TestModuleField (ILDism()
				.Input(t.LastAssembledFile)
				.Run().OutputFileName);
		}
		
		[Test]
		public void TestModuleFieldWithOffset ()
		{
			t.TestModuleFieldWithOffset ();
			t.TestModuleFieldWithOffset (ILDism()
				.Input(t.LastAssembledFile)
				.Run().OutputFileName);
		}
		
		[Test]
		public void TestClassField ()
		{
			t.TestClassField ();
			t.TestClassField (ILDism()
				.Input(t.LastAssembledFile)
				.Run().OutputFileName);
		}
		
		[Test]
		public void TestClassFieldWithOffset ()
		{
			t.TestClassFieldWithOffset ();
			t.TestClassFieldWithOffset (ILDism()
				.Input(t.LastAssembledFile)
				.Run().OutputFileName);
		}
		
		[Test]
		public void TestMultipleClassFieldsWithOffsets ()
		{
			t.TestMultipleClassFieldsWithOffsets ();
			t.TestMultipleClassFieldsWithOffsets (ILDism()
				.Input(t.LastAssembledFile)
				.Run().OutputFileName);
		}
		
		[Test]
		public void TestVectorField ()
		{
			t.TestVectorField ();
			t.TestVectorField (ILDism()
				.Input(t.LastAssembledFile)
				.Run().OutputFileName);
		}
		
		[Test]
		public void TestUnboundedArray ()
		{
			t.TestUnboundedArray ();
			t.TestUnboundedArray (ILDism()
				.Input(t.LastAssembledFile)
				.Run().OutputFileName);
		}
		
		[Test]
		public void TestLowerBoundedArray ()
		{
			t.TestLowerBoundedArray ();
			t.TestLowerBoundedArray (ILDism()
				.Input(t.LastAssembledFile)
				.Run().OutputFileName);
		}
		
		[Test]
		public void TestBoundedArray ()
		{
			t.TestBoundedArray ();
			t.TestBoundedArray (ILDism()
				.Input(t.LastAssembledFile)
				.Run().OutputFileName);
		}
		
		[Test]
		public void TestSizedArray ()
		{
			t.TestSizedArray ();
			t.TestSizedArray (ILDism()
				.Input(t.LastAssembledFile)
				.Run().OutputFileName);
		}
		
		[Test]
		public void TestNegativeSizedArray ()
		{
			t.TestNegativeSizedArray ();
			t.TestNegativeSizedArray (ILDism()
				.Input(t.LastAssembledFile)
				.Run().OutputFileName);
		}
		
		[Test]
		public void TestGenericField ()
		{
			t.TestGenericField ();
			t.TestGenericField (ILDism()
				.Input(t.LastAssembledFile)
				.Run().OutputFileName);
		}
		
		[Test]
		public void TestGenericOrdinalField ()
		{
			t.TestGenericOrdinalField ();
			t.TestGenericOrdinalField (ILDism()
				.Input(t.LastAssembledFile)
				.Run().OutputFileName);
		}
	}
}

