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
using Mono.Cecil;
using System.IO;
using NUnit.Framework;
using Mono.Cecil.Mdb;

namespace Mono.ILDasm.Tests {
	public abstract class DisassemblerTester {
		
		public static string BasePath {get;set;}
		
		static DisassemblerTester ()
		{
			BasePath = @".";
		}
		
		/*protected sealed class Assembler {
			readonly Mono.ILAsm.Driver driver;
			readonly List<string> arguments = new List<string> ();
			Error? expected_error;
			Warning? expected_warning;
			Error? resulting_error;
			Warning? resulting_warning;
			
			public Assembler ()
			{
				driver = new Mono.ILAsm.Driver ();
				driver.Target = Target.Dll;
				driver.DebuggingInfo = true;
				driver.Output = TextWriter.Null;
				
				driver.Report.Quiet = true;
				driver.Report.Warning += OnWarning;
				driver.Report.Error += OnError;
			}
			
			private void OnWarning (object sender, WarningEventArgs e)
			{
				resulting_warning = e.Warning;
			}
			
			private void OnError (object sender, Mono.ILAsm.ErrorEventArgs e)
			{
				resulting_error = e.Error;
			}
			
			public Assembler Input (params string[] fileNames)
			{
				var path = BasePath + "tests/";
				
				foreach (var file in fileNames)
					arguments.Add (path + file);
				
				return this;
			}
			
			public Assembler Dll ()
			{
				driver.Target = Target.Dll;
				return this;
			}
			
			public Assembler Exe ()
			{
				driver.Target = Target.Exe;
				return this;
			}
			
			public Assembler Debug ()
			{
				driver.DebuggingInfo = true;
				return this;
			}
			
			public Assembler Release ()
			{
				driver.DebuggingInfo = false;
				return this;
			}
			
			public Assembler Output (string fileName)
			{
				driver.OutputFileName = fileName;
				return this;
			}
			
			public Assembler Argument (string argument, ArgumentType type)
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
			
			public Assembler Argument (string argument)
			{
				return Argument (argument, ArgumentType.Slash);
			}
			
			public Assembler Argument (string argument, ArgumentType type, string value)
			{
				Argument (argument + ":" + value, type);
				return this;
			}
			
			public Assembler Argument (string argument, string value)
			{
				return Argument (argument, ArgumentType.Slash, value);
			}
			
			public Assembler Mute ()
			{
				driver.Output = TextWriter.Null;
				driver.Report.Quiet = true;
				
				return this;
			}
			
			public Assembler Unmute ()
			{
				driver.Output = Console.Out;
				driver.Report.Quiet = false;
				
				return this;
			}
			
			public Assembler ExpectError (Error error)
			{
				expected_error = error;
				return this;
			}
			
			public Assembler ExpectWarning (Warning warning)
			{
				expected_warning = warning;
				return this;
			}
			
			public AssemblerOutput Run ()
			{
				if (expected_warning != null)
					driver.Report.WarningOutput = TextWriter.Null;
				
				if (expected_error != null)
					driver.Report.ErrorOutput = TextWriter.Null;
				
				var result = driver.Run (arguments.ToArray ());
				
				// Reset stuff to defaults.
				driver.Target = Target.Dll;
				driver.DebuggingInfo = true;
				driver.Output = Console.Out;
				
				driver.Report.Quiet = false;
				driver.Report.Warning -= OnWarning;
				driver.Report.Error -= OnError;
				driver.Report.ErrorOutput = Console.Error;
				driver.Report.WarningOutput = Console.Out;
				
				Assert.AreEqual (expected_warning, resulting_warning);
				Assert.AreEqual (expected_error, resulting_error);
				
				return new AssemblerOutput (driver.OutputFileName, driver.DebuggingInfo, result);
			}
		}*/
		
		protected sealed class AssemblerOutput {
			public AssemblerOutput (string fileName, bool debug, ExitCode? result)
			{
				Result = result;
				file_name = fileName;
				this.debug = debug;
			}
			
			public ExitCode? Result { get; private set; }
			
			private readonly string file_name;
			
			private readonly bool debug;
			
			public AssemblerOutput Expect (ExitCode? result)
			{
				Assert.AreEqual (result, Result);
				return this;
			}
			
			public AssembledModule GetModule ()
			{
				return new AssembledModule (file_name, debug);
			}
		}
		
		public delegate bool ModulePredicate (ModuleDefinition module);
		
		protected sealed class AssembledModule {
			public AssembledModule (string fileName, bool debug)
			{
				Module = ModuleDefinition.ReadModule (fileName, new ReaderParameters
				{
					SymbolReaderProvider = debug ? new MdbReaderProvider () : null,
				});
			}
			
			public ModuleDefinition Module { get; private set; }
			
			public AssembledModule Expect (ModulePredicate predicate)
			{
				Assert.IsTrue (predicate (Module));
				return this;
			}
		}
		
		protected sealed class Disassembler {
			readonly Driver driver;
			readonly List<string> arguments = new List<string> ();
			
			string outputFileName = @"../../disassembledFile.il";
			string inputFileName = string.Empty;
			
			public Disassembler ()
			{
				driver = new Driver ();
			}
			
			public DisassemblerOutput Run ()
			{
				Assert.AreNotSame (inputFileName, string.Empty);
				arguments.Add ("/output:" + outputFileName);
				arguments.Add (inputFileName);
				
				foreach (var arg in arguments) {
					Console.WriteLine (arg);
				}
				
				arguments.Add (inputFileName);
				var result = driver.Run (arguments.ToArray ());
				Assert.AreEqual (result, ExitCode.Success);
				return new DisassemblerOutput (inputFileName, outputFileName, result);
			}
			
			public Disassembler ILFileAsInput (string fileToAssemble, Target t = Target.Dll)
			{
				Console.WriteLine (System.Environment.CurrentDirectory);
				
				
				List<string> args = new List<string> ();
				var assemblyFileName = "tmpAssembly." + (t == Target.Dll ? "dll" : "exe");
				var ilasm = new Mono.ILAsm.Driver ();
				
				if (t == Target.Dll)
					args.Add ("/dll");
				args.Add (@"/output:" + assemblyFileName);
				args.Add (fileToAssemble);
				
				var result = ilasm.Run (args.ToArray ());
				
				if (result != Mono.ILAsm.ExitCode.Success)
					Assert.Fail ();
				
				return Input (assemblyFileName);
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
			
			public DisassemblerOutput (string inputFileName, string outputFileName, ExitCode? result)
			{
				Result = result;
				InputFileName = inputFileName;
				OutputFileName = outputFileName;
			}
			
			public void Verify ()
			{
				var input = new ILComparer ();
				input.Parse (InputFileName);
				var output = new ILComparer ();
				output.Parse (OutputFileName);
				
				if (!input.Compare (output))
					Assert.Fail ();
			}
		}
		
		protected enum ArgumentType : byte {
			Slash = 0,
			Dash = 1,
			DoubleDash = 2,
		}
		
		protected Disassembler ILDism ()
		{
			return new Disassembler ();
		}
		
		/*protected Assembler ILAsm ()
		{
			return new Assembler ();
		}*/
		
		protected sealed class ILComparer {
			
			CodeGenerator codegen = null;
			ILParser parser = null;
			
			public ILComparer ()
			{
				codegen = new CodeGenerator (string.Empty);
			}
			
			public void Parse (string inputFile)
			{
				var reader = File.OpenText (inputFile);
				var scanner = new ILTokenizer (reader);
				
				parser = new ILParser (codegen, scanner);
				parser.yyparse (new ScannerAdapter (scanner));
				
				reader.Close ();
			}
			
			public bool Compare (ILComparer comp)
			{
				throw new NotImplementedException ();
			}
		}
	}
}

