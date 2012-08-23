// 
// ClassTests.cs
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
using Mono.Cecil;
using NUnit.Framework;

namespace Mono.ILAsm.Tests {
	[TestFixture]
	public sealed class ClassTests : AssemblerTester {
		[Test]
		public void TestSimpleClassDirective (string defaultInput = "class/class-001.il")
		{
			ILAsm ()
				.Input (defaultInput)
				.Run ()
				.Expect (ExitCode.Success)
				.GetModule ()
				.Expect (x => x.GetType ("test001").Expect (
					y => y.Name == "test001",
					y => y.BaseType.FullName == "System.Object"));
		}
		
		[Test]
		public void TestValueTypeClassDirective (string defaultInput = "class/class-002.il")
		{
			ILAsm ()
				.Input (defaultInput)
				.Run ()
				.Expect (ExitCode.Success)
				.GetModule ()
				.Expect (x => x.GetType ("test002").IsValueType);
		}
		
		[Test]
		public void TestEnumClassDirective (string defaultInput = "class/class-003.il")
		{
			ILAsm ()
				.Input (defaultInput)
				.Run ()
				.Expect (ExitCode.Success)
				.GetModule ()
				.Expect (x => x.GetType ("test003").IsEnum);
		}
		
		[Test]
		public void TestInterfaceImplementation (string defaultInput = "class/class-004.il")
		{
			ILAsm ()
				.Input (defaultInput)
				.Run ()
				.Expect (ExitCode.Success)
				.GetModule ()
				.Expect (x => x.GetType ("test004").Interfaces.ContainsOne (
					y => y.FullName == "System.ICloneable"));
		}
		
		[Test]
		public void TestMultipleInterfaceImplementations (string defaultInput = "class/class-005.il")
		{
			ILAsm ()
				.Input (defaultInput)
				.Run ()
				.Expect (ExitCode.Success)
				.GetModule ()
				.Expect (x => x.GetType ("test005").Interfaces.ContainsMany (
					y => y.FullName == "System.ICloneable",
					y => y.FullName == "System.IDisposable"));
		}
		
		[Test]
		public void TestSimpleClassInheritance (string defaultInput = "class/class-006.il")
		{
			ILAsm ()
				.Input (defaultInput)
				.Run ()
				.Expect (ExitCode.Success)
				.GetModule ()
				.Expect (x => x.GetType ("test006").BaseType.Name == "test006_base");
		}
		
		[Test]
		public void TestInterfaceClassDirective (string defaultInput = "class/class-007.il")
		{
			ILAsm ()
				.Input (defaultInput)
				.Run ()
				.Expect (ExitCode.Success)
				.GetModule ()
				.Expect (x => x.GetType ("test007").IsInterface);
		}
		
		[Test]
		public void TestSimpleInterfaceImplementation (string defaultInput = "class/class-008.il")
		{
			ILAsm ()
				.Input (defaultInput)
				.Run ()
				.Expect (ExitCode.Success)
				.GetModule ()
				.Expect (x => x.GetType ("test008").Interfaces.ContainsOne (
					y => y.Name == "test008_if"));
		}
		
		[Test]
		public void TestGenericValueTypeConstraint (string defaultInput = "class-generic/class-generic-001.il")
		{
			ILAsm ()
				.Input (defaultInput)
				.Run ()
				.Expect (ExitCode.Success)
				.GetModule ()
				.Expect (x => x.GetType ("test001").GenericParameters.ContainsOne (
					z => z.Attributes.HasBitFlag (GenericParameterAttributes.NotNullableValueTypeConstraint)));
		}
		
		[Test]
		public void TestGenericReferenceTypeConstraint (string defaultInput = "class-generic/class-generic-002.il")
		{
			ILAsm ()
				.Input (defaultInput)
				.Run ()
				.Expect (ExitCode.Success)
				.GetModule ()
				.Expect (x => x.GetType ("test002").GenericParameters.ContainsOne (
					z => z.Attributes.HasBitFlag (GenericParameterAttributes.ReferenceTypeConstraint)));
		}
		
		[Test]
		public void TestGenericConstructorConstraint (string defaultInput = "class-generic/class-generic-003.il")
		{
			ILAsm ()
				.Input (defaultInput)
				.Run ()
				.Expect (ExitCode.Success)
				.GetModule ()
				.Expect (x => x.GetType ("test003").GenericParameters.ContainsOne (
					z => z.Attributes.HasBitFlag (GenericParameterAttributes.DefaultConstructorConstraint)));
		}
		
		[Test]
		public void TestGenericInterfaceImplementation (string defaultInput = "class-generic/class-generic-004.il")
		{
			ILAsm ()
				.Input (defaultInput)
				.Run ()
				.Expect (ExitCode.Success)
				.GetModule ()
				.Expect (x => x.GetType ("test004").Interfaces.ContainsOne (
					y => y.FullName == "System.IEquatable`1<test004_dummy>"));
		}
		
		[Test]
		public void TestGenericClassDirective (string defaultInput = "class-generic/class-generic-005.il")
		{
			ILAsm ()
				.Input (defaultInput)
				.Run ()
				.Expect (ExitCode.Success)
				.GetModule ()
				.Expect (x => x.GetType ("test005").GenericParameters.ContainsMany (
					z => z.Name == "T1",
					z => z.Name == "T2"));
		}
		
		[Test]
		public void TestGenericInterfaceConstraint (string defaultInput = "class-generic/class-generic-006.il")
		{
			ILAsm ()
				.Input (defaultInput)
				.Run ()
				.Expect (ExitCode.Success)
				.GetModule ()
				.Expect (x => x.GetType ("test006").GenericParameters.ContainsOne (
					y => y.Constraints.ContainsOne (
						z => z.FullName == "System.ICloneable")));
		}
		
		[Test]
		public void TestCovariantTypeParameter (string defaultInput = "class-generic/class-generic-007.il")
		{
			ILAsm ()
				.Input (defaultInput)
				.Run ()
				.Expect (ExitCode.Success)
				.GetModule ()
				.Expect (x => x.GetType ("test007").GenericParameters.ContainsOne (
					y => y.IsCovariant));
		}
		
		[Test]
		public void TestContravariantTypeParameter (string defaultInput = "class-generic/class-generic-008.il")
		{
			ILAsm ()
				.Input (defaultInput)
				.Run ()
				.Expect (ExitCode.Success)
				.GetModule ()
				.Expect (x => x.GetType ("test008").GenericParameters.ContainsOne (
					y => y.IsContravariant));
		}
		
		[Test]
		public void TestNonPowerOfTwoClassPackSize (string defaultInput = "class-layout/class-layout-001.il")
		{
			ILAsm ()
				.Input (defaultInput)
				.ExpectError (Error.InvalidPackSize)
				.Run ()
				.Expect (ExitCode.Error);
		}
		
		[Test]
		public void TestOutOfRangePackSize ()
		{
			ILAsm ()
				.Input ("class-layout/class-layout-002.il")
				.ExpectError (Error.InvalidPackSize)
				.Run ()
				.Expect (ExitCode.Error);
		}
		
		[Test]
		public void TestClassPackSize (string defaultInput = "class-layout/class-layout-003.il")
		{
			ILAsm ()
				.Input (defaultInput)
				.Run ()
				.Expect (ExitCode.Success)
				.GetModule ()
				.Expect (x => x.GetType ("test003").PackingSize == 32);
		}
		
		[Test]
		public void TestPackSizeInAutoLayoutClass (string defaultInput = "class-layout/class-layout-004.il")
		{
			ILAsm ()
				.Input (defaultInput)
				.ExpectWarning (Warning.LayoutInfoInAutoLayoutType)
				.Run ()
				.Expect (ExitCode.Success)
				.GetModule ()
				.Expect (x => x.GetType ("test004").PackingSize == 16);
		}
		
		[Test]
		public void TestClassSize (string defaultInput = "class-layout/class-layout-005.il")
		{
			ILAsm ()
				.Input (defaultInput)
				.Run ()
				.Expect (ExitCode.Success)
				.GetModule ()
				.Expect (x => x.GetType ("test005").ClassSize == 1024);
		}
		
		[Test]
		public void TestMinusOneClassSize (string defaultInput = "class-layout/class-layout-006.il")
		{
			ILAsm ()
				.Input (defaultInput)
				.Run ()
				.Expect (ExitCode.Success)
				.GetModule ()
				.Expect (x => x.GetType ("test006").ClassSize == -1);
		}
	}
}
