// 
// DisassemblerTester.cs
//  
// Author:
//       Marcin Bartnicki <mbartnic@gmail.com>
// 
// Copyright (c) 2012 Marcin Bartnicki
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
using System.Collections.Generic;
using System.IO;
using Mono.ILAsm.Tests;
using Mono.Cecil;
using Mono.Cecil.Mdb;
using NUnit.Framework;

namespace Mono.ILDasm.Tests {
	public abstract class DisassemblerTester : AssemblerTester {
				
		protected sealed class Disassembler {
			readonly Driver driver;
			readonly List<string> arguments = new List<string> ();
			
			string outputFileName = @"disassembledFile.il";
			string inputFileName = string.Empty;
			
			public Disassembler ()
			{
				driver = new Driver ();
			}
			
			public DisassemblerOutput Run ()
			{
				Assert.AreNotSame (inputFileName, string.Empty);
				arguments.Add ("/output:" + BasePath + "tests/" + outputFileName);
				arguments.Add (inputFileName);
				
				foreach (var arg in arguments) {
					Console.WriteLine (arg);
				}
				
				arguments.Add (inputFileName);
				var result = driver.Run (arguments.ToArray ());
				Assert.AreEqual (result, ExitCode.Success);
				return new DisassemblerOutput (inputFileName, outputFileName, result);
			}
			
			
			
			public Disassembler Input (string fileToDisassemble)
			{
				inputFileName = fileToDisassemble;
				return this;
			}
			
			public Disassembler Output (string fileName)
			{
				outputFileName = fileName;
				return this;
			}
			
			public Disassembler Argument (string argument, ArgumentType type)
			{
				string arg = null;
				switch (type) {
				case ArgumentType.Slash:
					arg = "/";
					break;
				case ArgumentType.Dash:
					arg = "-";
					break;
				case ArgumentType.DoubleDash:
					arg = "--";
					break;
				}
				
				arguments.Add (arg + argument);
				return this;
			}
			
			public Disassembler Argument (string argument)
			{
				return Argument (argument, ArgumentType.Slash);
			}
			
			public Disassembler Argument (string argument, ArgumentType type, string value)
			{
				Argument (argument + ":" + value, type);
				return this;
			}
			
			public Disassembler Argument (string argument, string value)
			{
				return Argument (argument, ArgumentType.Slash, value);
			}
		}
		
		protected sealed class DisassemblerOutput {
			
			public ExitCode? Result {get;private set;}
			
			public string OutputFileName {get;private set;}
			
			public string InputFileName {get; private set;}
			
			public bool WasDllFile { get { return InputFileName.ToLower ().EndsWith ("dll");}}
			
			public DisassemblerOutput (string inputFileName, string outputFileName, ExitCode? result)
			{
				Result = result;
				InputFileName = inputFileName;
				OutputFileName = outputFileName;
			}
		}
		
		protected Disassembler ILDism ()
		{
			return new Disassembler ();
		}
		
		protected AssembledModule Verify(DisassemblerOutput disOut) {
			throw new NotImplementedException();
		}
	}
}

