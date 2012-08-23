// 
// ClassTests.cs
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
using Mono.Cecil;
using NUnit.Framework;

namespace Mono.ILDasm.Tests {
	[TestFixture]
	public sealed class ClassTests : DisassemblerTester {
		Mono.ILAsm.Tests.ClassTests baseTests = new Mono.ILAsm.Tests.ClassTests();
		
		[Test]
		public void TestSimpleClassDirective ()
		{
			baseTests.TestSimpleClassDirective ();
			
			baseTests.TestSimpleClassDirective (ILDism()
				.Input(baseTests.LastAssembledFile)
				.Run().OutputFileName);
		}
		
		[Test]
		public void TestValueTypeClassDirective ()
		{
			baseTests.TestValueTypeClassDirective ();
			baseTests.TestValueTypeClassDirective (ILDism()
				.Input(baseTests.LastAssembledFile)
				.Run().OutputFileName);
		}
		
		[Test]
		public void TestEnumClassDirective ()
		{
			baseTests.TestEnumClassDirective ();
			baseTests.TestEnumClassDirective (ILDism()
				.Input(baseTests.LastAssembledFile)
				.Run().OutputFileName);
		}
		
		[Test]
		public void TestInterfaceImplementation ()
		{
			baseTests.TestInterfaceImplementation ();
			baseTests.TestInterfaceImplementation (ILDism()
				.Input(baseTests.LastAssembledFile)
				.Run().OutputFileName);
		}
		
		[Test]
		public void TestMultipleInterfaceImplementations ()
		{
			baseTests.TestMultipleInterfaceImplementations ();
			baseTests.TestMultipleInterfaceImplementations (ILDism()
				.Input(baseTests.LastAssembledFile)
				.Run().OutputFileName);
		}
		
		[Test]
		public void TestSimpleClassInheritance ()
		{
			baseTests.TestSimpleClassInheritance ();
			baseTests.TestSimpleClassInheritance (ILDism()
				.Input(baseTests.LastAssembledFile)
				.Run().OutputFileName);
		}
		
		[Test]
		public void TestInterfaceClassDirective ()
		{
			baseTests.TestInterfaceClassDirective ();
			baseTests.TestInterfaceClassDirective (ILDism()
				.Input(baseTests.LastAssembledFile)
				.Run().OutputFileName);
		}
		
		[Test]
		public void TestSimpleInterfaceImplementation ()
		{
			baseTests.TestSimpleInterfaceImplementation ();
			baseTests.TestSimpleInterfaceImplementation (ILDism()
				.Input(baseTests.LastAssembledFile)
				.Run().OutputFileName);
		}
		
		[Test]
		public void TestGenericValueTypeConstraint ()
		{
			baseTests.TestGenericValueTypeConstraint ();
			baseTests.TestGenericValueTypeConstraint (ILDism()
				.Input(baseTests.LastAssembledFile)
				.Run().OutputFileName);
		}
		
		[Test]
		public void TestGenericReferenceTypeConstraint ()
		{
			baseTests.TestGenericReferenceTypeConstraint ();
			baseTests.TestGenericReferenceTypeConstraint (ILDism()
				.Input(baseTests.LastAssembledFile)
				.Run().OutputFileName);
		}
		
		[Test]
		public void TestGenericConstructorConstraint ()
		{
			baseTests.TestGenericConstructorConstraint ();
			baseTests.TestGenericConstructorConstraint (ILDism()
				.Input(baseTests.LastAssembledFile)
				.Run().OutputFileName);
		}
		
		[Test]
		public void TestGenericInterfaceImplementation ()
		{
			baseTests.TestGenericInterfaceImplementation ();
			baseTests.TestGenericInterfaceImplementation (ILDism()
				.Input(baseTests.LastAssembledFile)
				.Run().OutputFileName);
		}
		
		[Test]
		public void TestGenericClassDirective ()
		{
			baseTests.TestGenericClassDirective ();
			baseTests.TestGenericClassDirective (ILDism()
				.Input(baseTests.LastAssembledFile)
				.Run().OutputFileName);
		}
		
		[Test]
		public void TestGenericInterfaceConstraint ()
		{
			baseTests.TestGenericInterfaceConstraint ();
			baseTests.TestGenericInterfaceConstraint (ILDism()
				.Input(baseTests.LastAssembledFile)
				.Run().OutputFileName);
		}
		
		[Test]
		public void TestCovariantTypeParameter ()
		{
			baseTests.TestCovariantTypeParameter ();
			baseTests.TestCovariantTypeParameter (ILDism()
				.Input(baseTests.LastAssembledFile)
				.Run().OutputFileName);
		}
		
		[Test]
		public void TestContravariantTypeParameter ()
		{
			baseTests.TestContravariantTypeParameter ();
			baseTests.TestContravariantTypeParameter (ILDism()
				.Input(baseTests.LastAssembledFile)
				.Run().OutputFileName);
		}
		
		[Test]
		public void TestClassPackSize ()
		{
			baseTests.TestClassPackSize ();
			baseTests.TestClassPackSize (ILDism()
				.Input(baseTests.LastAssembledFile)
				.Run().OutputFileName);
		}
		
		[Test]
		public void TestPackSizeInAutoLayoutClass ()
		{
			baseTests.TestPackSizeInAutoLayoutClass ();
			baseTests.TestPackSizeInAutoLayoutClass (ILDism()
				.Input(baseTests.LastAssembledFile)
				.Run().OutputFileName);
		}
		
		[Test]
		public void TestClassSize ()
		{
			baseTests.TestClassSize ();
			baseTests.TestClassSize (ILDism()
				.Input(baseTests.LastAssembledFile)
				.Run().OutputFileName);
		}
		
		[Test]
		public void TestMinusOneClassSize ()
		{
			baseTests.TestMinusOneClassSize ();
			baseTests.TestMinusOneClassSize (ILDism()
				.Input(baseTests.LastAssembledFile)
				.Run().OutputFileName);
		}
	}
}

