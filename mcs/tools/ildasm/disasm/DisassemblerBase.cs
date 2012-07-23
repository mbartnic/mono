// 
// DisassemblerBase.cs
//  
// Author:
//       Alex Rønne Petersen <xtzgzorex@gmail.com>
//       Jb Evain (jbevain@gmail.com)
// 
// Copyright (c) 2011 Alex Rønne Petersen, Jb Evain
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
using System.IO;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Collections.Generic;

namespace Mono.ILDasm {
	internal abstract class DisassemblerBase {
		public CodeWriter Writer { get; private set; }

		protected DisassemblerBase (TextWriter output)
		{
			Writer = new CodeWriter (output);
		}
		
		protected DisassemblerBase (CodeWriter writer)
		{
			Writer = writer;
		}
		
		public static bool EscapeAlways { get; set; }
		
		public static ModuleDefinition Module { get; set; }
		
		public static string EscapeOrEmpty (string identifier)
		{
			return identifier == string.Empty ?
				string.Empty : " " + Escape (identifier);
		}
		
		public static string Escape (string identifier)
		{
			if (EscapeAlways)
				return "'" + EscapeString (identifier) + "'";
			
			// Since keywords in ILAsm don't have any odd symbols, we
			// can just escape them with apostrophes.
			if (KeywordTable.Keywords.ContainsKey (identifier))
				return "'" + identifier + "'";
			
			if (!IsIdentifierStartChar (identifier [0]))
				return "'" + EscapeString (identifier) + "'";
			
			foreach (var chr in identifier)
				if (!IsIdentifierChar (chr))
					return "'" + EscapeString (identifier) + "'";
			
			return identifier;
		}
		
		public static string EscapeType (string identifier)
		{
			var strings = identifier.Split ('/');
			
			var sb = new StringBuilder (Escape (strings [0]));
			
			for (var i = 1; i < strings.Length; i++)
				sb.Append ("/").Append (Escape (strings [i]));
			
			return sb.ToString ();
		}
		
		public static string EscapeString (string str)
		{
			return str.Replace ("'", "\\'").Replace ("\\", "\\\\");
		}
		
		public static string EscapeQString (string str)
		{
			var sb = new StringBuilder ();
			str = str.Replace ("\"", "\\\"").Replace ("\\", "\\\\");
			
			foreach (var chr in str) {
				if (chr == '\t')
					sb.Append ("\\t");
				else if (chr == '\n')
					sb.Append ("\\n");
				else if (chr == '\r')
					sb.Append ("\\r");
				else
					sb.Append (chr);
			}
			
			return sb.ToString ();
		}
		
		public static string ToByteList (byte[] bytes)
		{
			var sb = new StringBuilder ("( ");
			
			for (var i = 0; i < bytes.Length; i++)
			{
				sb.Append (bytes [i].ToString ("X2"));
				
				if (i != bytes.Length - 1) {
					if ((i + 1) % 20 == 0)
						sb.AppendLine ().Append ("  ");
					else
						sb.Append (" ");
				} else
					sb.Append (" ");
			}
			
			return sb.Append (")").ToString ();
		}
		
		public static bool IsIdentifierStartChar (char chr)
		{
			return char.IsLetter (chr) || "_$@?`".IndexOf (chr) != -1;
		}
		
		public static bool IsIdentifierChar (char chr)
		{
			return char.IsLetterOrDigit (chr) || "_$@?`.".IndexOf (chr) != -1;
		}

		public static string Stringize (MarshalInfo marshalInfo)
		{
			if (marshalInfo is ArrayMarshalInfo)
				return Stringize ((ArrayMarshalInfo) marshalInfo);
			if (marshalInfo is CustomMarshalInfo)
				return Stringize ((CustomMarshalInfo) marshalInfo);
			if (marshalInfo is FixedArrayMarshalInfo)
				return Stringize ((FixedArrayMarshalInfo) marshalInfo);
			if (marshalInfo is FixedSysStringMarshalInfo)
				return Stringize ((FixedSysStringMarshalInfo) marshalInfo);
			if (marshalInfo is SafeArrayMarshalInfo)
				return Stringize ((SafeArrayMarshalInfo) marshalInfo);

			return Stringize(marshalInfo.NativeType);
		}

		public static string Stringize (CustomMarshalInfo mInfo)
		{
			throw new NotImplementedException ();
		}

		public static string Stringize (SafeArrayMarshalInfo mInfo)
		{
			throw new NotImplementedException ();
		}

		public static string Stringize (NativeType nType)
		{
			switch (nType) {
			case NativeType.FixedSysString:
				return "fixed sysstring ";
			case NativeType.FixedArray:
				return "fixed array ";
			case NativeType.Struct:
				return "struct ";
			case NativeType.Boolean:
				return "bool ";
			case NativeType.I1:
				return "int8 ";
			case NativeType.I2:
				return "int16 ";
			case NativeType.I4:
				return "int32 ";
			case NativeType.I8:
				return "int64 ";
			case NativeType.U1:
				return "unsigned int8 ";
			case NativeType.U2:
				return "unsigned int16 ";
			case NativeType.U4:
				return "unsigned int32 ";
			case NativeType.U8:
				return "unsigned int64 ";
			case NativeType.Currency:
				return "currency ";
			case NativeType.BStr:
				return "bstr ";
			case NativeType.LPStr:
				return "lpstr ";
			case NativeType.LPWStr:
				return "lpwstr ";
			case NativeType.LPTStr:
				return "lptstr ";
			case NativeType.Max:
			case NativeType.None:
				return string.Empty;
			case NativeType.R4:
				return "float32 ";
			case NativeType.R8:
				return "float64 ";
			case NativeType.IUnknown:
				return "iunknown ";
			case NativeType.LPStruct:
				return "lpstruct ";
			case NativeType.CustomMarshaler:
				return "custom ";
			case NativeType.Error:
				return "error ";
			case NativeType.IDispatch:
				return "idispatch ";
			case NativeType.VariantBool:
				return "variant bool ";
			case NativeType.ASAny:
				return "as any ";
			case NativeType.SafeArray:
				return "safearray ";
			case NativeType.Func:
				return "method ";
			case NativeType.TBStr:
				return "tbstr ";
			case NativeType.ANSIBStr:
				return "ansi bstr ";
			case NativeType.ByValStr:
				return "byvalstr ";
			case NativeType.Array:
				return "array ";
			default:
				throw new NotImplementedException (nType.ToString());
			};
		}

		public static string Stringize (FixedArrayMarshalInfo mInfo)
		{
			return string.Format ("{0}[{1}] {2}", Stringize(mInfo.NativeType), 
				mInfo.Size != -1 ? mInfo.Size.ToString() : string.Empty,
				Stringize(mInfo.ElementType));
		}

		public static string Stringize (FixedSysStringMarshalInfo mInfo)
		{
			return string.Format ("{0}[{1}]", Stringize(mInfo.NativeType), mInfo.Size);
		}

		private static string Stringize (ArrayMarshalInfo mInfo)
		{
			//mbartnic: some magic done by microsoft
			/*
			 * wrote following based on ms ildasm to be Microsoft - compatible
			 * 
			 * ecma specifies (Partition II Chapter 7.4) that SizeParameterIndex should be 
			 * greater than 0 if in use. 
			 * [Quote]
			 * .method int32 M2( int32 marshal(int32), bool[] marshal(bool[+1]) ) 
			 * Method M2 takes two arguments: an int32, and an array of bools: the number of elements in that array is 
			 * given by the value of the first parameter.	[/Quote]
			 * 
			 * if we disassemble following method:
			 * void marshal_param_LPArray_SizeParamIndex ([MarshalAs (UnmanagedType.LPArray, ArraySubType = UnmanagedType.R8, SizeParamIndex=0)] object crazyArray) { return; }
			 * 
			 * we obtain following il code (made by ms ildasm):
			 * .method private hidebysig instance void 
			 *		marshal_param_LPArray_SizeParamIndex(object  marshal( float64[ + 0]) crazyArray) cil managed { ret }
			 *		
			 * so we obtain theoretically meaningless ' + 0' in marshalled array which according to spec
			 * should not be there
			*/

			var sb = new StringBuilder ();
			sb.Append (Stringize (mInfo.ElementType));
			sb.Append ("[");
			if (mInfo.Size == -1 && mInfo.SizeParameterIndex != -1)
				sb.Append ("+" + mInfo.SizeParameterIndex);
			else if (mInfo.Size != -1) {
				sb.Append (mInfo.Size);
				if (mInfo.SizeParameterIndex > 0
					|| (mInfo.SizeParameterIndex == 0 && mInfo.SizeParameterMultiplier == -1))
					sb.Append ("+" + mInfo.SizeParameterIndex);
			}
			sb.Append ("]");
			return sb.ToString ();
		}

		public static string Stringize (MethodCallingConvention conv)
		{
			switch (conv)
			{
			case MethodCallingConvention.Default:
			case (MethodCallingConvention) 0x10: // Has generic arguments.
				return "default";
			case MethodCallingConvention.VarArg:
				return "vararg";
			case MethodCallingConvention.C:
				return "unmanaged cdecl";
			case MethodCallingConvention.StdCall:
				return "unmanaged stdcall";
			case MethodCallingConvention.ThisCall:
				return "unmanaged thiscall";
			case MethodCallingConvention.FastCall:
				return "unmanaged fastcall";
			case (MethodCallingConvention) 0x20:
				return "instance";
			case (MethodCallingConvention) 0x40:
				return "explicit";
			default:
				return "callconv (" + conv.ToInt32Hex () + ")";
			}
		}
		
		static string TypeToName (TypeReference type)
		{
			switch (type.FullName)
			{
			case "System.Object":
				return "object";
			case "System.Void":
				return "void";
			case "System.TypedReference":
				return "typedref";
			case "System.IntPtr":
				return "native int";
			case "System.UIntPtr":
				return "native uint";
			case "System.Byte":
				return "uint8";
			case "System.SByte":
				return "int8";
			case "System.Int16":
				return "int16";
			case "System.UInt16":
				return "uint16";
			case "System.Int32":
				return "int32";
			case "System.UInt32":
				return "uint32";
			case "System.Int64":
				return "int64";
			case "System.UInt64":
				return "uint64";
			case "System.Single":
				return "float32";
			case "System.Double":
				return "float64";
			case "System.Char":
				return "char";
			case "System.Boolean":
				return "bool";
			case "System.String":
				return "string";
			default:
				return type.FullName;
			}
		}

		public static string NumberToHexString (sbyte val)
		{
			return string.Format ("{0}", val.ToString ("X2"));
		}

		public static string NumberToHexString (byte val)
		{
			return string.Format ("{0}", val.ToString ("X2"));
		}

		public static string NumberToHexString (float val, bool convertToBigEndian = true)
		{
			return bytesToHex (BitConverter.GetBytes (val), convertToBigEndian);
		}

		public static string NumberToHexString (double val, bool convertToBigEndian = true)
		{
			return bytesToHex (BitConverter.GetBytes (val), convertToBigEndian);
		}

		public static string NumberToHexString (short val, bool convertToBigEndian = true)
		{
			return bytesToHex (BitConverter.GetBytes (val), convertToBigEndian);
		}

		public static string NumberToHexString (ushort val, bool convertToBigEndian = true)
		{
			return bytesToHex (BitConverter.GetBytes (val), convertToBigEndian);
		}

		public static string NumberToHexString (int val, bool convertToBigEndian = true)
		{
			return bytesToHex (BitConverter.GetBytes (val), convertToBigEndian);
		}

		public static string NumberToHexString (uint val, bool convertToBigEndian = true)
		{
			return bytesToHex (BitConverter.GetBytes (val), convertToBigEndian);
		}

		public static string NumberToHexString (long val, bool convertToBigEndian = true)
		{
			return bytesToHex (BitConverter.GetBytes (val), convertToBigEndian);
		}

		public static string NumberToHexString (ulong val, bool convertToBigEndian = true)
		{
			return bytesToHex (BitConverter.GetBytes (val), convertToBigEndian);
		}

		public static string NumberToHexString (char val, bool convertToBigEndian = true)
		{
			return bytesToHex (BitConverter.GetBytes (val), convertToBigEndian);
		}

		private static string bytesToHex(byte[] bytes, bool convertToBigEndian = true)
		{
			if (BitConverter.IsLittleEndian && convertToBigEndian)
				Array.Reverse (bytes);

			var sb = new StringBuilder ();

			sb.Append ("0x");
			for (int i = 0; i < bytes.Length; i++)
				sb.Append (bytes[i].ToString ("X2"));

			return sb.ToString ();
		}

		public static string Stringize (ValueType val, bool shortForm = false)
		{
			var result = new StringBuilder ();
			var typeCode = Convert.GetTypeCode (val);

			switch (typeCode) {
			case TypeCode.Boolean:
				return string.Format ("bool ({0})", (bool) val ? "true" : "false");
			case TypeCode.Empty:
				return string.Format ("nullref");
			case TypeCode.Byte:
				if (!shortForm)
					return string.Format ("uint8 ({0})", val);
				return val.ToString();
			case TypeCode.Char:
				if (!shortForm)
					return string.Format ("char ({0})", NumberToHexString ((char) val));
				else
					return NumberToHexString ((char) val);
			case TypeCode.Single:
				return string.Format ("float32 ({0})", NumberToHexString ((float) val));
			case TypeCode.Double:
				return string.Format ("float64 ({0})", NumberToHexString ((double) val));
			case TypeCode.Int16:
				if (!shortForm)
					return string.Format ("int16 ({0})", (Int16) val);
				else
					return NumberToHexString ((Int16) val);
			case TypeCode.Int32:
				if (!shortForm)
					return string.Format ("int32 ({0})", (Int32) val);
				else
					return NumberToHexString ((Int32) val);
			case TypeCode.Int64:
				if (!shortForm)
					return string.Format ("int64 ({0})", (Int64) val);
				else
					return NumberToHexString ((Int64) val);
			case TypeCode.SByte:
				if (!shortForm)
					return string.Format ("int8({0})", val);
				return val.ToString();
			case TypeCode.UInt16:
				if (!shortForm)
					return string.Format ("uint16 ({0})", (UInt16) val);
				else
					return NumberToHexString ((UInt16) val);
			case TypeCode.UInt32:
				if (!shortForm)
					return string.Format ("uint32 ({0})", (UInt32) val);
				else
					return NumberToHexString ((UInt32) val);
			case TypeCode.UInt64:
				if (!shortForm)
					return string.Format ("uint64 ({0})", (UInt64) val);
				else
					return NumberToHexString ((UInt64) val);
			default:
				throw new ArgumentException ("val");
			};
        }

		public static string Stringize (String str)
		{
			var chars = str.ToCharArray ();

			if (Array.Exists<char> (chars, x => (int) x > 0x7f)) { //handle non-ASCII
				var sb = new StringBuilder ();
				var bytearray = new byte[chars.Length * 2];

				sb.Append ("bytearray ");

				for (int i = 0; i < chars.Length; i++) {
					var bytes = BitConverter.GetBytes (chars[i]);
					if (BitConverter.IsLittleEndian)
						Array.Reverse (bytes);

					bytearray[i * 2] = bytes[0];
					bytearray[i * 2 + 1] = bytes[1];
				}

				sb.Append (ToByteList (bytearray));

				return sb.ToString();
			} else //ASCII characters
				return str;
		}

		public static string Stringize (ParameterDefinition param)
		{
			var sb = new StringBuilder ();

			if (param.IsIn)
				sb.Append ("[in]");
			if (param.IsOptional)
				sb.Append ("[opt]");
			if (param.IsOut)
				sb.Append ("[out]");

			sb.Append (Stringize (param.ParameterType));

			if (param.HasMarshalInfo)
				sb.AppendFormat (" marshal ({0}) ", Stringize(param.MarshalInfo));

			sb.Append (EscapeOrEmpty (param.Name));

			return sb.ToString ();
		}

		public static string Stringize (TypeReference type)
		{
			if (type is ArrayType)
				return Stringize ((ArrayType) type);
			else if (type is ByReferenceType)
				return Stringize ((ByReferenceType) type);
			else if (type is FunctionPointerType)
				return Stringize ((FunctionPointerType) type);
			else if (type is OptionalModifierType)
				return Stringize ((OptionalModifierType) type);
			else if (type is RequiredModifierType)
				return Stringize ((RequiredModifierType) type);
			else if (type is PinnedType)
				return Stringize ((PinnedType) type);
			else if (type is PointerType)
				return Stringize ((PointerType) type);
			else if (type is SentinelType)
				return Stringize ((SentinelType) type);
			else if (type is GenericParameter)
				return Stringize ((GenericParameter) type);
			else if (type is GenericInstanceType)
				return Stringize ((GenericInstanceType) type);
			else {
				var sb = new StringBuilder ();
				var corName = TypeToName (type);
				var isCorlib = type.Scope.Name == "mscorlib" && corName != type.FullName;

				if (type.MetadataType == MetadataType.ValueType)
					sb.Append ("valuetype ");
				else if (type.MetadataType == MetadataType.Class)
					sb.Append ("class ");

				if (type.Scope is ModuleReference) {
					if (type.Scope.Name != Module.Name)
						sb.AppendFormat ("[.module {0}]", type.Scope.Name);
				} else if (!isCorlib)
					sb.AppendFormat ("[{0}]", type.Scope.Name);
				
				sb.Append (isCorlib ? corName : EscapeType (type.FullName));
				
				return sb.ToString ();
			}
		}
		
		public static string Stringize (ArrayType type)
		{
			var sb = new StringBuilder (Stringize (type.ElementType));
			
			if (type.IsVector)
				return sb.Append ("[]").ToString ();
			
			sb.Append ("[");
			
			for (var i = 0; i < type.Rank; i++) {
				var dim = type.Dimensions [i];
				
				if (dim.LowerBound == null && dim.UpperBound == null)
					sb.Append ("...");
				else if (dim.LowerBound == null)
					sb.Append (dim.UpperBound);
				else if (dim.UpperBound == null)
					sb.AppendFormat ("{0} ...", dim.LowerBound);
				else
					sb.AppendFormat ("{0} ... {1}", dim.LowerBound, dim.UpperBound);
				
				if (i != type.Rank - 1)
					sb.Append (", ");
			}
			
			return sb.Append ("]").ToString ();
		}
		
		public static string Stringize (ByReferenceType type)
		{
			return Stringize (type.ElementType) + "&";
		}
		
		public static string Stringize (FunctionPointerType type)
		{
			var sb = new StringBuilder ("method ");
			
			if (type.HasThis)
				sb.Append ("instance ");
			
			if (type.ExplicitThis)
				sb.Append ("explicit ");
			
			sb.AppendFormat ("{0} ", Stringize (type.CallingConvention));
			
			sb.Append (Stringize (type.ReturnType));
			sb.Append (" * ");
			sb.Append ("(");
			
			for (var i = 0; i < type.Parameters.Count; i++) {
				sb.Append (Stringize (type.Parameters [i].ParameterType));
				
				if (i != type.Parameters.Count - 1)
					sb.Append (", ");
			}
			
			return sb.Append (")").ToString ();
		}
		
		public static string Stringize (OptionalModifierType type)
		{
			return Stringize (type.ElementType) +
				" modopt (" + Stringize (type.ModifierType) + ")";
		}
		
		public static string Stringize (RequiredModifierType type)
		{
			return Stringize (type.ElementType) +
				"modreq (" + Stringize (type.ModifierType) + ")";
		}
		
		public static string Stringize (PinnedType type)
		{
			return Stringize (type.ElementType) + " pinned";
		}
		
		public static string Stringize (PointerType type)
		{
			return Stringize (type.ElementType) + "*";
		}
		
		public static string Stringize (SentinelType type)
		{
			return "..." + Stringize (type.ElementType);
		}
		
		public static string Stringize (GenericParameter type)
		{
			// HACK: We sometimes get incorrect names from Cecil.
			if (type.Owner is MethodReference)
				return "!!" + type.Position;
			else
				return "!" + type.Position;
		}
		
		public static string Stringize (GenericInstanceType type)
		{
			var sb = new StringBuilder ();
			
			sb.Append (Stringize (type.ElementType));
			sb.Append ("<");
			
			for (var i = 0; i < type.GenericArguments.Count; i++) {
				sb.Append (Stringize (type.GenericArguments [i]));
				
				if (i != type.GenericArguments.Count - 1)
					sb.Append (", ");
			}
			
			sb.Append (">");
			
			return sb.ToString ();
		}
		
		public static string Stringize (MethodReference method)
		{
			StringBuilder sb = new StringBuilder ();
			
			if (method.HasThis)
				sb.Append ("instance ");
			
			if (method.ExplicitThis)
				sb.Append ("explicit ");
			
			sb.AppendFormat ("{0} ", Stringize (method.CallingConvention));
			sb.AppendFormat ("{0} ", Stringize (method.ReturnType));
			
			if (method.DeclaringType != null)
				sb.AppendFormat ("{0}::", Stringize (method.DeclaringType));
			
			sb.AppendFormat (Escape (method.Name));
			
			if (method is GenericInstanceMethod) {
				sb.Append ("<");
				
				var genMeth = (GenericInstanceMethod) method;
				for (var i = 0; i < genMeth.GenericArguments.Count; i++) {
					sb.Append (Stringize (genMeth.GenericArguments [i]));
					
					if (i != genMeth.GenericArguments.Count - 1)
						sb.Append (", ");
				}
				
				sb.Append (">");
			}
			
			sb.Append (" (");
			
			for (var i = 0; i < method.Parameters.Count; i++) {
				var param = method.Parameters [i];
				sb.AppendFormat (Stringize (param.ParameterType));
				
				if (i != method.Parameters.Count - 1)
					sb.Append (", ");
			}
			
			sb.Append (")");
			
			return sb.ToString ();
		}
		
		public static string Stringize (FieldReference field)
		{
			var sb = new StringBuilder ();
			
			sb.AppendFormat ("{0} ", Stringize (field.FieldType));
			
			if (field.DeclaringType != Module.GetModuleType ())
				sb.AppendFormat ("{0}::", Stringize (field.DeclaringType));
			
			sb.Append (Escape (field.Name));
			
			return sb.ToString ();
		}
		
		public static string Stringize (Instruction instr)
		{
			return instr.MakeLabel () + ": " + instr.OpCode;
		}
	}
}
