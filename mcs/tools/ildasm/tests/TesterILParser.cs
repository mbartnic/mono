// created by jay 0.7 (c) 1998 Axel.Schreiner@informatik.uni-osnabrueck.de

#line 2 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
//
// Mono.ILAsm.ILParser
// 
// (C) Sergey Chaban (serge@wildwestsoftware.com)
// (C) 2003 Jackson Harper, All rights reserved
// (C) 2011 Alex Rønne Petersen <xtzgzorex@gmail.com>
//

using System;
using System.IO;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Collections.Generic;

namespace Mono.ILDasm.Tests {
	internal class ILParser {
		static readonly int yacc_verbose_flag = 0;
		readonly CodeGenerator codegen;
		readonly ILTokenizer tokenizer;
		readonly Stack<string> namespace_stack = new Stack<string> ();
		readonly Stack<TypeDefinition> typedef_stack = new Stack<TypeDefinition> ();
		readonly Dictionary<string, Label> labels = new Dictionary<string, Label> ();
		readonly Stack<Scope> scope_stack = new Stack<Scope> ();
		readonly Dictionary<Instruction, object> label_jumps = new Dictionary<Instruction, object> ();
		readonly List<Label> current_labels = new List<Label> ();
		bool is_enum_class;
		bool is_value_class;
		bool has_public_key;
		bool has_public_key_token;
		bool is_explicit_call;
		bool is_instance_call;
		bool is_parameter_marshal_notation;
		bool type_attr_visibility_set;
		bool is_inside_scope;
		bool custom_attr_unsigned;
		bool generic_param_in_method;
		int custom_attr_blob_seq_length;
		string pinvoke_mod_name;
		ILProcessor il;
		Instruction last_instr;

		public ILParser (CodeGenerator codegen, ILTokenizer tokenizer)
		{
			this.codegen = codegen;
			this.tokenizer = tokenizer;

			scope_stack.Push (null);
		}

		delegate TypeReference TypeCreator ();

		// Gives type inference, e.g.:
		// $$ = DeferType (() => new TypeReference(null, null, null, null));
		static TypeCreator DeferType (TypeCreator fun)
		{
			return fun;
		}

		static TypeReference EvaluateType (object obj)
		{
			return ((TypeCreator) obj) ();
		}

		TypeReference MakeTypeReference (QualifiedName name, IMetadataScope scope)
		{
			if (name.Nestings.Count == 0)
				return new TypeReference (name.FullNamespace, name.Name,
					codegen.CurrentModule, scope);

			var outer = new TypeReference (name.FullNamespace, name.Nestings [0],
				codegen.CurrentModule, scope);

			var nested = outer;

			for (var i = 1; i < name.Nestings.Count; i++) {
				nested = new TypeReference (null, name.Nestings [i],
					codegen.CurrentModule, scope) {
					DeclaringType = nested,
				};
			}

			return new TypeReference (null, name.Name,
				codegen.CurrentModule, scope) {
				DeclaringType = nested,
			};
		}

		Instruction FindInstruction (int idx)
		{
			return il.Body.Instructions [idx];
		}

		Instruction FindInstruction (string name)
		{
			Label label;

			if (!labels.TryGetValue (name, out label));

			return label.Instruction;
		}

		ICustomAttributeProvider ResolveCustomAttributeOwner (object owner)
		{
			ICustomAttributeProvider provider;

			if (owner is TypeReference)
				provider = ((TypeReference) owner).Resolve ();
			else if (owner is FieldReference)
				provider = ((FieldReference) owner).Resolve ();
			else
				provider = ((MethodReference) owner).Resolve ();

			return provider;
		}

#line default

  /** error output stream.
      It should be changeable.
    */
  public System.IO.TextWriter ErrorOutput = System.Console.Out;

  /** simplified error message.
      @see <a href="#yyerror(java.lang.String, java.lang.String[])">yyerror</a>
    */
  public void yyerror (string message) {
    yyerror(message, null);
  }

  /* An EOF token */
  public int eof_token;

  /** (syntax) error message.
      Can be overwritten to control message format.
      @param message text to be displayed.
      @param expected vector of acceptable tokens, if available.
    */
  public void yyerror (string message, string[] expected) {
    if ((yacc_verbose_flag > 0) && (expected != null) && (expected.Length  > 0)) {
      ErrorOutput.Write (message+", expecting");
      for (int n = 0; n < expected.Length; ++ n)
        ErrorOutput.Write (" "+expected[n]);
        ErrorOutput.WriteLine ();
    } else
      ErrorOutput.WriteLine (message);
  }

  /** debugging support, requires the package jay.yydebug.
      Set to null to suppress debugging messages.
    */
  internal yydebug.yyDebug debug;

  protected const int yyFinal = 1;
 // Put this array into a separate class so it is only initialized if debugging is actually used
 // Use MarshalByRefObject to disable inlining
 class YYRules : MarshalByRefObject {
  public static readonly string [] yyRule = {
    "$accept : il_file",
    "il_file : decls",
    "decls :",
    "decls : decls decl",
    "decl : class_all",
    "decl : namespace_all",
    "decl : method_all",
    "decl : field_decl",
    "decl : data_decl",
    "decl : vtable_decl",
    "decl : vtfixup_decl",
    "decl : file_decl",
    "decl : assembly_all",
    "decl : assemblyref_all",
    "decl : exptype_all",
    "decl : manifestres_all",
    "decl : module_head",
    "decl : sec_decl",
    "decl : customattr_decl",
    "decl : D_SUBSYSTEM int32",
    "decl : D_CORFLAGS int32",
    "decl : D_FILE K_ALIGNMENT int32",
    "decl : D_IMAGEBASE int64",
    "decl : D_STACKRESERVE int64",
    "decl : D_TYPELIST OPEN_BRACE class_refs CLOSE_BRACE",
    "decl : D_MSCORLIB",
    "decl : extsource_spec",
    "decl : language_decl",
    "decl : typedef_decl",
    "decl : comp_control",
    "line_directive : D_LINE",
    "line_directive : D_XLINE",
    "extsource_spec : line_directive int32 SQSTRING",
    "extsource_spec : line_directive int32",
    "extsource_spec : line_directive int32 COLON int32 SQSTRING",
    "extsource_spec : line_directive int32 COLON int32",
    "extsource_spec : line_directive int32 COLON int32 COMMA int32 SQSTRING",
    "extsource_spec : line_directive int32 COLON int32 COMMA int32",
    "extsource_spec : line_directive int32 COMMA int32 COLON int32 SQSTRING",
    "extsource_spec : line_directive int32 COMMA int32 COLON int32",
    "extsource_spec : line_directive int32 COMMA int32 COLON int32 COMMA int32 SQSTRING",
    "extsource_spec : line_directive int32 COMMA int32 COLON int32 COMMA int32",
    "extsource_spec : line_directive int32 QSTRING",
    "language_decl : D_LANGUAGE SQSTRING",
    "language_decl : D_LANGUAGE SQSTRING COMMA SQSTRING",
    "language_decl : D_LANGUAGE SQSTRING COMMA SQSTRING COMMA SQSTRING",
    "vtable_decl : D_VTABLE ASSIGN OPEN_PARENS bytes CLOSE_PARENS",
    "vtfixup_decl : D_VTFIXUP OPEN_BRACKET int32 CLOSE_BRACKET vtfixup_attr K_AT id",
    "vtfixup_attr :",
    "vtfixup_attr : vtfixup_attr K_INT32",
    "vtfixup_attr : vtfixup_attr K_INT64",
    "vtfixup_attr : vtfixup_attr K_FROMUNMANAGED",
    "vtfixup_attr : vtfixup_attr K_CALLMOSTDERIVED",
    "vtfixup_attr : vtfixup_attr K_RETAINAPPDOMAIN",
    "typedef_decl : typedef_directive",
    "typedef_directive : D_TYPEDEF type K_AS comp_name",
    "typedef_directive : D_TYPEDEF class_ref K_AS comp_name",
    "typedef_directive : D_TYPEDEF member_ref K_AS comp_name",
    "typedef_directive : D_TYPEDEF customattr K_AS comp_name",
    "typedef_directive : D_TYPEDEF customattr_owner K_AS comp_name",
    "comp_control : comp_control_decl",
    "comp_control_decl : D_XDEFINE comp_name",
    "comp_control_decl : D_XDEFINE comp_name QSTRING",
    "comp_control_decl : D_XUNDEF comp_name",
    "comp_control_decl : D_XIFDEF comp_name",
    "comp_control_decl : D_XIFNDEF comp_name",
    "comp_control_decl : D_XELSE",
    "comp_control_decl : D_XENDIF",
    "comp_control_decl : D_XINCLUDE QSTRING",
    "comp_control_decl : SEMICOLON",
    "namespace_all : namespace_head OPEN_BRACE decls CLOSE_BRACE",
    "namespace_head : D_NAMESPACE comp_name_str",
    "class_all : class_head OPEN_BRACE class_decls CLOSE_BRACE",
    "$$1 :",
    "$$2 :",
    "class_head : D_CLASS class_attr comp_name $$1 formal_typars_clause $$2 extends_clause impl_clause",
    "class_attr :",
    "class_attr : class_attr K_PUBLIC",
    "class_attr : class_attr K_PRIVATE",
    "class_attr : class_attr K_NESTED K_PRIVATE",
    "class_attr : class_attr K_NESTED K_PUBLIC",
    "class_attr : class_attr K_NESTED K_FAMILY",
    "class_attr : class_attr K_NESTED K_ASSEMBLY",
    "class_attr : class_attr K_NESTED K_FAMANDASSEM",
    "class_attr : class_attr K_NESTED K_FAMORASSEM",
    "class_attr : class_attr K_VALUE",
    "class_attr : class_attr K_ENUM",
    "class_attr : class_attr K_INTERFACE",
    "class_attr : class_attr K_SEALED",
    "class_attr : class_attr K_ABSTRACT",
    "class_attr : class_attr K_AUTO",
    "class_attr : class_attr K_SEQUENTIAL",
    "class_attr : class_attr K_EXPLICIT",
    "class_attr : class_attr K_ANSI",
    "class_attr : class_attr K_UNICODE",
    "class_attr : class_attr K_AUTOCHAR",
    "class_attr : class_attr K_IMPORT",
    "class_attr : class_attr K_SERIALIZABLE",
    "class_attr : class_attr K_BEFOREFIELDINIT",
    "class_attr : class_attr K_SPECIALNAME",
    "class_attr : class_attr K_RTSPECIALNAME",
    "class_attr : class_attr K_FLAGS OPEN_PARENS int32 CLOSE_PARENS",
    "extends_clause :",
    "extends_clause : K_EXTENDS type_spec",
    "impl_clause :",
    "impl_clause : impl_class_refs",
    "impl_class_refs :",
    "impl_class_refs : K_IMPLEMENTS type_spec",
    "impl_class_refs : impl_class_refs COMMA type_spec",
    "formal_typars_clause :",
    "formal_typars_clause : OPEN_ANGLE_BRACKET formal_typars CLOSE_ANGLE_BRACKET",
    "formal_typars : formal_typar_gpar",
    "formal_typars : formal_typars COMMA formal_typar_gpar",
    "formal_typar_gpar : formal_typar_attr constraints_clause formal_typar",
    "formal_typar_attr :",
    "formal_typar_attr : formal_typar_attr PLUS",
    "formal_typar_attr : formal_typar_attr DASH",
    "formal_typar_attr : formal_typar_attr D_CTOR",
    "formal_typar_attr : formal_typar_attr K_VALUETYPE",
    "formal_typar_attr : formal_typar_attr K_CLASS",
    "formal_typar : comp_name_str",
    "constraints_clause :",
    "constraints_clause : OPEN_PARENS constraints CLOSE_PARENS",
    "constraints : type_spec",
    "constraints : constraints COMMA type_spec",
    "class_decls :",
    "class_decls : class_decls class_decl",
    "class_decl : method_all",
    "class_decl : class_all",
    "class_decl : event_all",
    "class_decl : prop_all",
    "class_decl : field_decl",
    "class_decl : data_decl",
    "class_decl : sec_decl",
    "class_decl : extsource_spec",
    "class_decl : customattr_decl",
    "class_decl : param_type_decl",
    "class_decl : D_SIZE int32",
    "class_decl : D_PACK int32",
    "class_decl : D_OVERRIDE type_spec DOUBLE_COLON method_name K_WITH call_conv type type_spec DOUBLE_COLON method_name OPEN_PARENS sig_args CLOSE_PARENS",
    "class_decl : D_OVERRIDE generic_method_ref K_WITH generic_method_ref",
    "class_decl : language_decl",
    "class_decl : comp_control",
    "param_type_decl : D_PARAM K_TYPE id",
    "param_type_decl : D_PARAM K_TYPE OPEN_BRACKET int32 CLOSE_BRACKET",
    "class_refs : class_ref",
    "class_refs : class_refs COMMA class_ref",
    "class_ref : OPEN_BRACKET comp_name CLOSE_BRACKET slashed_name",
    "class_ref : OPEN_BRACKET mdtoken CLOSE_BRACKET slashed_name",
    "class_ref : OPEN_BRACKET STAR CLOSE_BRACKET slashed_name",
    "class_ref : OPEN_BRACKET D_MODULE comp_name_str CLOSE_BRACKET slashed_name",
    "class_ref : slashed_name",
    "class_ref : mdtoken_type",
    "class_ref : D_THIS",
    "class_ref : D_BASE",
    "class_ref : D_NESTER",
    "mdtoken_type : mdtoken",
    "value_type_prefix : K_VALUE K_CLASS",
    "value_type_prefix : K_VALUETYPE",
    "type : K_CLASS class_ref",
    "type : value_type_prefix class_ref",
    "type : type OPEN_BRACKET CLOSE_BRACKET",
    "type : type OPEN_BRACKET bounds CLOSE_BRACKET",
    "type : type AMPERSAND",
    "type : type STAR",
    "type : type K_PINNED",
    "type : type K_MODREQ OPEN_PARENS type_spec CLOSE_PARENS",
    "type : type K_MODOPT OPEN_PARENS type_spec CLOSE_PARENS",
    "type : K_METHOD call_conv type STAR OPEN_PARENS sig_args CLOSE_PARENS",
    "type : type typars_clause",
    "type : BANG int32",
    "type : BANG id",
    "type : BANG BANG int32",
    "type : BANG BANG id",
    "type : K_OBJECT",
    "type : K_VOID",
    "type : K_TYPEDREF",
    "type : K_NATIVE K_INT",
    "type : K_NATIVE K_UNSIGNED K_INT",
    "type : K_NATIVE K_UINT",
    "type : K_NATIVE K_FLOAT",
    "type : simple_type",
    "type : ELLIPSIS type",
    "unsigned_int8 : K_UINT8",
    "unsigned_int8 : K_UNSIGNED K_INT8",
    "unsigned_int16 : K_UINT16",
    "unsigned_int16 : K_UNSIGNED K_INT16",
    "unsigned_int32 : K_UINT32",
    "unsigned_int32 : K_UNSIGNED K_INT32",
    "unsigned_int64 : K_UINT64",
    "unsigned_int64 : K_UNSIGNED K_INT64",
    "int_type : K_INT8",
    "int_type : K_INT16",
    "int_type : K_INT32",
    "int_type : K_INT64",
    "int_type : unsigned_int8",
    "int_type : unsigned_int16",
    "int_type : unsigned_int32",
    "int_type : unsigned_int64",
    "simple_type : K_FLOAT32",
    "simple_type : K_FLOAT64",
    "simple_type : K_CHAR",
    "simple_type : K_BOOL",
    "simple_type : K_STRING",
    "simple_type : int_type",
    "typars_clause_opt :",
    "typars_clause_opt : typars_clause",
    "typars_clause : OPEN_ANGLE_BRACKET typars CLOSE_ANGLE_BRACKET",
    "typars : type",
    "typars : typars COMMA type",
    "type_spec : class_ref",
    "type_spec : OPEN_BRACKET comp_name CLOSE_BRACKET",
    "type_spec : OPEN_BRACKET D_MODULE comp_name_str CLOSE_BRACKET",
    "type_spec : type",
    "bounds : bound",
    "bounds : bounds COMMA bound",
    "bound :",
    "bound : ELLIPSIS",
    "bound : int32",
    "bound : int32 ELLIPSIS int32",
    "bound : int32 ELLIPSIS",
    "call_conv : K_INSTANCE call_conv",
    "call_conv : K_EXPLICIT call_conv",
    "call_conv : call_kind",
    "call_conv : K_CALLCONV OPEN_PARENS int32 CLOSE_PARENS",
    "call_kind :",
    "call_kind : K_DEFAULT",
    "call_kind : K_VARARG",
    "call_kind : K_UNMANAGED K_CDECL",
    "call_kind : K_UNMANAGED K_STDCALL",
    "call_kind : K_UNMANAGED K_THISCALL",
    "call_kind : K_UNMANAGED K_FASTCALL",
    "marshal_info :",
    "marshal_info : K_CUSTOM OPEN_PARENS comp_qstring COMMA comp_qstring CLOSE_PARENS",
    "marshal_info : K_CUSTOM OPEN_PARENS comp_qstring COMMA comp_qstring COMMA comp_qstring COMMA comp_qstring CLOSE_PARENS",
    "marshal_info : K_FIXED K_SYSSTRING OPEN_BRACKET int32 CLOSE_BRACKET",
    "marshal_info : K_FIXED K_ARRAY OPEN_BRACKET int32 CLOSE_BRACKET",
    "marshal_info : K_FIXED K_ARRAY OPEN_BRACKET int32 CLOSE_BRACKET marshal_type",
    "marshal_info : marshal_type STAR",
    "marshal_info : marshal_type OPEN_BRACKET CLOSE_BRACKET",
    "marshal_info : marshal_type OPEN_BRACKET int32 CLOSE_BRACKET",
    "marshal_info : marshal_type OPEN_BRACKET int32 PLUS int32 CLOSE_BRACKET",
    "marshal_info : marshal_type OPEN_BRACKET PLUS int32 CLOSE_BRACKET",
    "marshal_info : K_SAFEARRAY safearray_type",
    "marshal_info : K_SAFEARRAY safearray_type COMMA comp_qstring",
    "marshal_info : K_SAFEARRAY",
    "marshal_info : OPEN_BRACKET int32 CLOSE_BRACKET",
    "marshal_info : marshal_type",
    "unsigned_int : K_UINT",
    "unsigned_int : K_UNSIGNED K_INT",
    "marshal_type : K_CURRENCY",
    "marshal_type : K_BOOL",
    "marshal_type : K_INT8",
    "marshal_type : K_INT16",
    "marshal_type : K_INT32",
    "marshal_type : K_INT64",
    "marshal_type : K_FLOAT32",
    "marshal_type : K_FLOAT64",
    "marshal_type : K_ERROR",
    "marshal_type : unsigned_int8",
    "marshal_type : unsigned_int16",
    "marshal_type : unsigned_int32",
    "marshal_type : unsigned_int64",
    "marshal_type : K_BSTR",
    "marshal_type : K_LPSTR",
    "marshal_type : K_LPWSTR",
    "marshal_type : K_LPTSTR",
    "marshal_type : K_IUNKNOWN",
    "marshal_type : K_IDISPATCH",
    "marshal_type : K_STRUCT",
    "marshal_type : K_INTERFACE",
    "marshal_type : K_INT",
    "marshal_type : unsigned_int",
    "marshal_type : K_BYVALSTR",
    "marshal_type : K_ANSI K_BSTR",
    "marshal_type : K_TBSTR",
    "marshal_type : K_VARIANT K_BOOL",
    "marshal_type : K_METHOD",
    "marshal_type : K_AS K_ANY",
    "marshal_type : K_LPSTRUCT",
    "marshal_type : native_type",
    "native_type : K_VARIANT",
    "native_type : K_VOID",
    "native_type : K_SYSCHAR",
    "native_type : K_DECIMAL",
    "native_type : K_DATE",
    "native_type : K_OBJECTREF",
    "native_type : K_NESTED K_STRUCT",
    "safearray_type : K_VARIANT",
    "safearray_type : K_CURRENCY",
    "safearray_type : K_BOOL",
    "safearray_type : K_INT8",
    "safearray_type : K_INT16",
    "safearray_type : K_INT32",
    "safearray_type : K_FLOAT32",
    "safearray_type : K_FLOAT64",
    "safearray_type : unsigned_int8",
    "safearray_type : unsigned_int16",
    "safearray_type : unsigned_int32",
    "safearray_type : K_DECIMAL",
    "safearray_type : K_DATE",
    "safearray_type : K_BSTR",
    "safearray_type : K_IUNKNOWN",
    "safearray_type : K_IDISPATCH",
    "safearray_type : K_INT",
    "safearray_type : unsigned_int",
    "safearray_type : K_ERROR",
    "safearray_type : variant_type",
    "variant_type : K_NULL",
    "variant_type : K_VOID",
    "variant_type : K_INT64",
    "variant_type : unsigned_int64",
    "variant_type : STAR",
    "variant_type : variant_type OPEN_BRACKET CLOSE_BRACKET",
    "variant_type : OPEN_BRACKET CLOSE_BRACKET",
    "variant_type : variant_type K_VECTOR",
    "variant_type : variant_type AMPERSAND",
    "variant_type : K_LPSTR",
    "variant_type : K_LPWSTR",
    "variant_type : K_SAFEARRAY",
    "variant_type : K_HRESULT",
    "variant_type : K_CARRAY",
    "variant_type : K_USERDEFINED",
    "variant_type : K_RECORD",
    "variant_type : K_FILETIME",
    "variant_type : K_BLOB",
    "variant_type : K_STREAM",
    "variant_type : K_STORAGE",
    "variant_type : K_STREAMED_OBJECT",
    "variant_type : K_STORED_OBJECT",
    "variant_type : K_BLOB_OBJECT",
    "variant_type : K_CF",
    "variant_type : K_CLSID",
    "field_decl : D_FIELD repeat_opt field_attr marshal_spec type id at_opt init_opt",
    "repeat_opt :",
    "repeat_opt : OPEN_BRACKET int32 CLOSE_BRACKET",
    "field_attr :",
    "field_attr : field_attr K_PUBLIC",
    "field_attr : field_attr K_PRIVATE",
    "field_attr : field_attr K_FAMILY",
    "field_attr : field_attr K_ASSEMBLY",
    "field_attr : field_attr K_FAMANDASSEM",
    "field_attr : field_attr K_FAMORASSEM",
    "field_attr : field_attr K_PRIVATESCOPE",
    "field_attr : field_attr K_STATIC",
    "field_attr : field_attr K_INITONLY",
    "field_attr : field_attr K_RTSPECIALNAME",
    "field_attr : field_attr K_SPECIALNAME",
    "field_attr : field_attr K_LITERAL",
    "field_attr : field_attr K_NOTSERIALIZED",
    "marshal_spec :",
    "marshal_spec : K_MARSHAL OPEN_PARENS marshal_info CLOSE_PARENS",
    "at_opt :",
    "at_opt : K_AT id",
    "init_opt :",
    "init_opt : ASSIGN field_init",
    "field_init_primitive : K_FLOAT32 OPEN_PARENS float64 CLOSE_PARENS",
    "field_init_primitive : K_FLOAT64 OPEN_PARENS float64 CLOSE_PARENS",
    "field_init_primitive : K_FLOAT32 OPEN_PARENS int64 CLOSE_PARENS",
    "field_init_primitive : K_FLOAT64 OPEN_PARENS int64 CLOSE_PARENS",
    "field_init_primitive : K_INT64 OPEN_PARENS int64 CLOSE_PARENS",
    "field_init_primitive : unsigned_int64 OPEN_PARENS int64 CLOSE_PARENS",
    "field_init_primitive : K_INT32 OPEN_PARENS int64 CLOSE_PARENS",
    "field_init_primitive : unsigned_int32 OPEN_PARENS int64 CLOSE_PARENS",
    "field_init_primitive : K_INT16 OPEN_PARENS int64 CLOSE_PARENS",
    "field_init_primitive : unsigned_int16 OPEN_PARENS int64 CLOSE_PARENS",
    "field_init_primitive : K_INT8 OPEN_PARENS int64 CLOSE_PARENS",
    "field_init_primitive : unsigned_int8 OPEN_PARENS int64 CLOSE_PARENS",
    "field_init_primitive : K_CHAR OPEN_PARENS int64 CLOSE_PARENS",
    "field_init_primitive : K_BOOL OPEN_PARENS truefalse CLOSE_PARENS",
    "field_init_primitive : K_BYTEARRAY bytes_list",
    "field_init : field_init_primitive",
    "field_init : comp_qstring",
    "field_init : K_NULLREF",
    "data_decl : data_head data_body",
    "data_name :",
    "data_name : id ASSIGN",
    "data_head : D_DATA tls cil data_name",
    "tls :",
    "tls : K_TLS",
    "cil :",
    "cil : K_CIL",
    "data_body : OPEN_BRACE dataitem_list CLOSE_BRACE",
    "data_body : dataitem",
    "dataitem_list : dataitem",
    "dataitem_list : dataitem_list COMMA dataitem",
    "data_repeat_opt : repeat_opt",
    "dataitem : K_CHAR STAR OPEN_PARENS comp_qstring CLOSE_PARENS",
    "dataitem : AMPERSAND OPEN_PARENS id CLOSE_PARENS",
    "dataitem : K_BYTEARRAY bytes_list",
    "dataitem : K_FLOAT32 OPEN_PARENS float64 CLOSE_PARENS data_repeat_opt",
    "dataitem : K_FLOAT64 OPEN_PARENS float64 CLOSE_PARENS data_repeat_opt",
    "dataitem : K_INT64 OPEN_PARENS int64 CLOSE_PARENS data_repeat_opt",
    "dataitem : K_INT32 OPEN_PARENS int32 CLOSE_PARENS data_repeat_opt",
    "dataitem : K_INT16 OPEN_PARENS int32 CLOSE_PARENS data_repeat_opt",
    "dataitem : K_INT8 OPEN_PARENS int32 CLOSE_PARENS data_repeat_opt",
    "dataitem : K_FLOAT32 data_repeat_opt",
    "dataitem : K_FLOAT64 data_repeat_opt",
    "dataitem : K_INT64 data_repeat_opt",
    "dataitem : K_INT32 data_repeat_opt",
    "dataitem : K_INT16 data_repeat_opt",
    "dataitem : K_INT8 data_repeat_opt",
    "method_all : method_head OPEN_BRACE method_decls CLOSE_BRACE",
    "$$3 :",
    "$$4 :",
    "method_head : D_METHOD meth_attr call_conv param_attr type sig_marshal_clause method_name $$3 formal_typars_clause $$4 OPEN_PARENS sig_args CLOSE_PARENS impl_attr",
    "meth_attr :",
    "meth_attr : meth_attr K_STATIC",
    "meth_attr : meth_attr K_PUBLIC",
    "meth_attr : meth_attr K_PRIVATE",
    "meth_attr : meth_attr K_FAMILY",
    "meth_attr : meth_attr K_ASSEMBLY",
    "meth_attr : meth_attr K_FAMANDASSEM",
    "meth_attr : meth_attr K_FAMORASSEM",
    "meth_attr : meth_attr K_PRIVATESCOPE",
    "meth_attr : meth_attr K_FINAL",
    "meth_attr : meth_attr K_VIRTUAL",
    "meth_attr : meth_attr K_ABSTRACT",
    "meth_attr : meth_attr K_HIDEBYSIG",
    "meth_attr : meth_attr K_NEWSLOT",
    "meth_attr : meth_attr K_REQSECOBJ",
    "meth_attr : meth_attr K_SPECIALNAME",
    "meth_attr : meth_attr K_RTSPECIALNAME",
    "meth_attr : meth_attr K_STRICT",
    "meth_attr : meth_attr K_COMPILERCONTROLLED",
    "meth_attr : meth_attr K_UNMANAGEDEXP",
    "meth_attr : meth_attr K_FLAGS OPEN_PARENS int32 CLOSE_PARENS",
    "meth_attr : meth_attr K_PINVOKEIMPL OPEN_PARENS comp_qstring K_AS comp_qstring pinv_attr CLOSE_PARENS",
    "meth_attr : meth_attr K_PINVOKEIMPL OPEN_PARENS comp_qstring pinv_attr CLOSE_PARENS",
    "meth_attr : meth_attr K_PINVOKEIMPL OPEN_PARENS pinv_attr CLOSE_PARENS",
    "pinv_attr :",
    "pinv_attr : pinv_attr K_NOMANGLE",
    "pinv_attr : pinv_attr K_ANSI",
    "pinv_attr : pinv_attr K_UNICODE",
    "pinv_attr : pinv_attr K_AUTOCHAR",
    "pinv_attr : pinv_attr K_LASTERR",
    "pinv_attr : pinv_attr K_WINAPI",
    "pinv_attr : pinv_attr K_PLATFORMAPI",
    "pinv_attr : pinv_attr K_CDECL",
    "pinv_attr : pinv_attr K_STDCALL",
    "pinv_attr : pinv_attr K_THISCALL",
    "pinv_attr : pinv_attr K_FASTCALL",
    "pinv_attr : pinv_attr K_BESTFIT COLON K_ON",
    "pinv_attr : pinv_attr K_BESTFIT COLON K_OFF",
    "pinv_attr : pinv_attr K_CHARMAPERROR COLON K_ON",
    "pinv_attr : pinv_attr K_CHARMAPERROR COLON K_OFF",
    "pinv_attr : pinv_attr K_FLAGS OPEN_PARENS int32 CLOSE_PARENS",
    "impl_attr :",
    "impl_attr : impl_attr K_NATIVE",
    "impl_attr : impl_attr K_CIL",
    "impl_attr : impl_attr K_IL",
    "impl_attr : impl_attr K_OPTIL",
    "impl_attr : impl_attr K_MANAGED",
    "impl_attr : impl_attr K_UNMANAGED",
    "impl_attr : impl_attr K_FORWARDREF",
    "impl_attr : impl_attr K_PRESERVESIG",
    "impl_attr : impl_attr K_RUNTIME",
    "impl_attr : impl_attr K_INTERNALCALL",
    "impl_attr : impl_attr K_SYNCHRONIZED",
    "impl_attr : impl_attr K_NOINLINING",
    "impl_attr : impl_attr K_NOOPTIMIZATION",
    "param_attr :",
    "param_attr : param_attr OPEN_BRACKET K_IN CLOSE_BRACKET",
    "param_attr : param_attr OPEN_BRACKET K_OUT CLOSE_BRACKET",
    "param_attr : param_attr OPEN_BRACKET K_OPT CLOSE_BRACKET",
    "param_attr : param_attr OPEN_BRACKET int32 CLOSE_BRACKET",
    "sig_args :",
    "sig_args : sig_arg_list",
    "sig_arg_list : sig_arg",
    "sig_arg_list : sig_arg_list COMMA sig_arg",
    "sig_marshal_clause :",
    "sig_marshal_clause : K_MARSHAL OPEN_PARENS marshal_info CLOSE_PARENS",
    "sig_marshal_clause : K_MARSHAL OPEN_PARENS marshal_type CLOSE_PARENS",
    "sig_arg : ELLIPSIS",
    "sig_arg : param_attr type sig_marshal_clause",
    "sig_arg : param_attr type sig_marshal_clause id",
    "method_decls :",
    "method_decls : method_decls method_decl",
    "method_decl : D_EMITBYTE int32",
    "method_decl : D_MAXSTACK int32",
    "method_decl : D_LOCALS OPEN_PARENS sig_args CLOSE_PARENS",
    "method_decl : D_LOCALS K_INIT OPEN_PARENS sig_args CLOSE_PARENS",
    "method_decl : D_ENTRYPOINT",
    "method_decl : D_ZEROINIT",
    "method_decl : D_EXPORT OPEN_BRACKET int32 CLOSE_BRACKET",
    "method_decl : D_EXPORT OPEN_BRACKET int32 CLOSE_BRACKET K_AS id",
    "method_decl : D_VTENTRY int32 COLON int32",
    "method_decl : D_OVERRIDE type_spec DOUBLE_COLON method_name",
    "method_decl : D_OVERRIDE generic_method_ref",
    "method_decl : scope_block",
    "method_decl : D_PARAM OPEN_BRACKET int32 CLOSE_BRACKET init_opt",
    "method_decl : param_type_decl",
    "method_decl : id COLON",
    "method_decl : seh_block",
    "method_decl : instr",
    "method_decl : sec_decl",
    "method_decl : extsource_spec",
    "method_decl : language_decl",
    "method_decl : customattr_decl",
    "method_decl : data_decl",
    "method_decl : comp_control",
    "$$5 :",
    "scope_block : OPEN_BRACE $$5 method_decls CLOSE_BRACE",
    "seh_block : try_block seh_clauses",
    "try_block : D_TRY scope_block",
    "try_block : D_TRY id K_TO id",
    "try_block : D_TRY int32 K_TO int32",
    "seh_clauses : seh_clause",
    "seh_clauses : seh_clauses seh_clause",
    "seh_clause : K_CATCH type_spec handler_block",
    "seh_clause : filter_clause handler_block",
    "seh_clause : K_FINALLY handler_block",
    "seh_clause : K_FAULT handler_block",
    "filter_clause : K_FILTER scope_block",
    "filter_clause : K_FILTER id",
    "filter_clause : K_FILTER int32",
    "handler_block : scope_block",
    "handler_block : K_HANDLER id K_TO id",
    "handler_block : K_HANDLER int32 K_TO int32",
    "assign_opt :",
    "assign_opt : ASSIGN",
    "instr : INSTR_NONE",
    "instr : INSTR_LOCAL int32",
    "instr : INSTR_LOCAL id",
    "instr : INSTR_PARAM int32",
    "instr : INSTR_PARAM id",
    "instr : INSTR_I int32",
    "instr : INSTR_I8 int64",
    "instr : INSTR_R float64",
    "instr : INSTR_R int64",
    "instr : INSTR_R bytes_list",
    "instr : INSTR_BRTARGET label",
    "instr : INSTR_METHOD method_ref",
    "instr : INSTR_FIELD field_ref",
    "instr : INSTR_TYPE type_spec",
    "instr : INSTR_STRING comp_qstring",
    "instr : INSTR_STRING K_ANSI comp_qstring",
    "instr : INSTR_STRING K_BYTEARRAY assign_opt bytes_list",
    "instr : INSTR_SIG call_conv type OPEN_PARENS sig_args CLOSE_PARENS",
    "instr : INSTR_TOK owner_type",
    "instr : INSTR_SWITCH OPEN_PARENS labels CLOSE_PARENS",
    "instr : INSTR_PHI int16_seq",
    "label : id",
    "label : int32",
    "labels :",
    "labels : label",
    "labels : label COMMA labels",
    "member_ref : K_METHOD method_ref",
    "member_ref : K_FIELD field_ref",
    "field_ref : type declaring_type comp_name_str",
    "field_ref : mdtoken",
    "declaring_type :",
    "declaring_type : type_spec DOUBLE_COLON",
    "$$6 :",
    "method_ref : call_conv type declaring_type method_name typars_clause_opt $$6 OPEN_PARENS sig_args CLOSE_PARENS",
    "method_ref : mdtoken",
    "method_name : D_CTOR",
    "method_name : D_CCTOR",
    "method_name : comp_name_str",
    "generic_method_ref : K_METHOD call_conv type type_spec DOUBLE_COLON method_name generic_arity OPEN_PARENS sig_args CLOSE_PARENS",
    "generic_arity :",
    "generic_arity : OPEN_BRACKET OPEN_ANGLE_BRACKET int32 CLOSE_ANGLE_BRACKET CLOSE_BRACKET",
    "owner_type : type_spec",
    "owner_type : member_ref",
    "event_all : event_head OPEN_BRACE event_decls CLOSE_BRACE",
    "event_head : D_EVENT event_attr type_spec comp_name_str",
    "event_head : D_EVENT event_attr id",
    "event_attr :",
    "event_attr : event_attr K_RTSPECIALNAME",
    "event_attr : event_attr K_SPECIALNAME",
    "event_decls :",
    "event_decls : event_decls event_decl",
    "event_decl : D_ADDON method_ref",
    "event_decl : D_REMOVEON method_ref",
    "event_decl : D_FIRE method_ref",
    "event_decl : D_OTHER method_ref",
    "event_decl : customattr_decl",
    "event_decl : extsource_spec",
    "event_decl : language_decl",
    "event_decl : comp_control",
    "prop_all : prop_head OPEN_BRACE prop_decls CLOSE_BRACE",
    "prop_head : D_PROPERTY call_conv prop_attr type comp_name_str OPEN_PARENS sig_args CLOSE_PARENS init_opt",
    "prop_attr :",
    "prop_attr : prop_attr K_RTSPECIALNAME",
    "prop_attr : prop_attr K_SPECIALNAME",
    "prop_decls :",
    "prop_decls : prop_decls prop_decl",
    "prop_decl : D_SET method_ref",
    "prop_decl : D_GET method_ref",
    "prop_decl : D_OTHER method_ref",
    "prop_decl : customattr_decl",
    "prop_decl : extsource_spec",
    "prop_decl : language_decl",
    "prop_decl : comp_control",
    "customattr_decl : customattr",
    "customattr_decl : customattr_owner",
    "customattr : D_CUSTOM method_ref",
    "customattr : D_CUSTOM method_ref ASSIGN comp_qstring",
    "customattr : D_CUSTOM method_ref ASSIGN bytes_list",
    "$$7 :",
    "customattr : D_CUSTOM method_ref ASSIGN OPEN_BRACE $$7 customattr_blob CLOSE_BRACE",
    "customattr_owner : D_CUSTOM OPEN_PARENS owner_type CLOSE_PARENS method_ref",
    "customattr_owner : D_CUSTOM OPEN_PARENS owner_type CLOSE_PARENS method_ref ASSIGN comp_qstring",
    "customattr_owner : D_CUSTOM OPEN_PARENS owner_type CLOSE_PARENS method_ref ASSIGN bytes_list",
    "$$8 :",
    "customattr_owner : D_CUSTOM OPEN_PARENS owner_type CLOSE_PARENS method_ref ASSIGN OPEN_BRACE $$8 customattr_blob CLOSE_BRACE",
    "customattr_blob : customattr_blob_args customattr_blob_nvp",
    "customattr_blob_args :",
    "customattr_blob_args : customattr_blob_args customattr_blob_init",
    "customattr_blob_args : customattr_blob_args comp_control",
    "customattr_blob_nvp :",
    "customattr_blob_nvp : customattr_blob_nvp prop_or_field customattr_blob_type comp_name_str ASSIGN customattr_blob_init",
    "customattr_blob_nvp : customattr_blob_nvp comp_control",
    "customattr_blob_type : simple_type",
    "customattr_blob_type : K_TYPE",
    "customattr_blob_type : K_OBJECT",
    "customattr_blob_type : K_ENUM K_CLASS SQSTRING",
    "customattr_blob_type : K_ENUM class_ref",
    "customattr_blob_type : customattr_blob_type OPEN_BRACKET CLOSE_BRACKET",
    "customattr_blob_init : K_FLOAT32 OPEN_PARENS float64 CLOSE_PARENS",
    "customattr_blob_init : K_FLOAT64 OPEN_PARENS float64 CLOSE_PARENS",
    "customattr_blob_init : K_FLOAT32 OPEN_PARENS int64 CLOSE_PARENS",
    "customattr_blob_init : K_FLOAT64 OPEN_PARENS int64 CLOSE_PARENS",
    "customattr_blob_init : K_INT64 OPEN_PARENS int64 CLOSE_PARENS",
    "customattr_blob_init : unsigned_int64 OPEN_PARENS int64 CLOSE_PARENS",
    "customattr_blob_init : K_INT32 OPEN_PARENS int64 CLOSE_PARENS",
    "customattr_blob_init : unsigned_int32 OPEN_PARENS int64 CLOSE_PARENS",
    "customattr_blob_init : K_INT16 OPEN_PARENS int64 CLOSE_PARENS",
    "customattr_blob_init : unsigned_int16 OPEN_PARENS int64 CLOSE_PARENS",
    "customattr_blob_init : K_INT8 OPEN_PARENS int64 CLOSE_PARENS",
    "customattr_blob_init : unsigned_int8 OPEN_PARENS int64 CLOSE_PARENS",
    "customattr_blob_init : K_CHAR OPEN_PARENS int64 CLOSE_PARENS",
    "customattr_blob_init : K_BOOL OPEN_PARENS truefalse CLOSE_PARENS",
    "customattr_blob_init : K_BYTEARRAY bytes_list",
    "customattr_blob_init : K_STRING OPEN_PARENS K_NULLREF CLOSE_PARENS",
    "customattr_blob_init : K_STRING OPEN_PARENS SQSTRING CLOSE_PARENS",
    "customattr_blob_init : K_TYPE OPEN_PARENS K_CLASS SQSTRING CLOSE_PARENS",
    "customattr_blob_init : K_TYPE OPEN_PARENS class_ref CLOSE_PARENS",
    "customattr_blob_init : K_TYPE OPEN_PARENS K_NULLREF CLOSE_PARENS",
    "customattr_blob_init : K_OBJECT OPEN_PARENS customattr_blob_init CLOSE_PARENS",
    "customattr_blob_init : K_FLOAT32 OPEN_BRACKET int32 CLOSE_BRACKET OPEN_PARENS float32_seq CLOSE_PARENS",
    "customattr_blob_init : K_FLOAT64 OPEN_BRACKET int32 CLOSE_BRACKET OPEN_PARENS float64_seq CLOSE_PARENS",
    "$$9 :",
    "customattr_blob_init : K_INT8 OPEN_BRACKET int32 CLOSE_BRACKET OPEN_PARENS $$9 int8_seq CLOSE_PARENS",
    "$$10 :",
    "customattr_blob_init : K_INT16 OPEN_BRACKET int32 CLOSE_BRACKET OPEN_PARENS $$10 int16_seq CLOSE_PARENS",
    "$$11 :",
    "customattr_blob_init : K_INT32 OPEN_BRACKET int32 CLOSE_BRACKET OPEN_PARENS $$11 int32_seq CLOSE_PARENS",
    "$$12 :",
    "customattr_blob_init : K_INT64 OPEN_BRACKET int32 CLOSE_BRACKET OPEN_PARENS $$12 int64_seq CLOSE_PARENS",
    "$$13 :",
    "customattr_blob_init : unsigned_int8 OPEN_BRACKET int32 CLOSE_BRACKET OPEN_PARENS $$13 int8_seq CLOSE_PARENS",
    "$$14 :",
    "customattr_blob_init : unsigned_int16 OPEN_BRACKET int32 CLOSE_BRACKET OPEN_PARENS $$14 int16_seq CLOSE_PARENS",
    "$$15 :",
    "customattr_blob_init : unsigned_int32 OPEN_BRACKET int32 CLOSE_BRACKET OPEN_PARENS $$15 int32_seq CLOSE_PARENS",
    "$$16 :",
    "customattr_blob_init : unsigned_int64 OPEN_BRACKET int32 CLOSE_BRACKET OPEN_PARENS $$16 int64_seq CLOSE_PARENS",
    "customattr_blob_init : K_CHAR OPEN_BRACKET int32 CLOSE_BRACKET OPEN_PARENS int16_seq CLOSE_PARENS",
    "customattr_blob_init : K_BOOL OPEN_BRACKET int32 CLOSE_BRACKET OPEN_PARENS bool_seq CLOSE_PARENS",
    "customattr_blob_init : K_STRING OPEN_BRACKET int32 CLOSE_BRACKET OPEN_PARENS string_seq CLOSE_PARENS",
    "customattr_blob_init : K_TYPE OPEN_BRACKET int32 CLOSE_BRACKET OPEN_PARENS class_seq CLOSE_PARENS",
    "customattr_blob_init : K_OBJECT OPEN_BRACKET int32 CLOSE_BRACKET OPEN_PARENS obj_seq CLOSE_PARENS",
    "float32_seq :",
    "float32_seq : float32_seq float64",
    "float32_seq : float32_seq int32",
    "float64_seq :",
    "float64_seq : float64_seq float64",
    "float64_seq : float64_seq int64",
    "int8_seq :",
    "int8_seq : int8_seq int32",
    "int16_seq :",
    "int16_seq : int16_seq int32",
    "int32_seq :",
    "int32_seq : int32_seq int32",
    "int64_seq :",
    "int64_seq : int64_seq int64",
    "bool_seq :",
    "bool_seq : bool_seq truefalse",
    "string_seq :",
    "string_seq : string_seq K_NULLREF",
    "string_seq : string_seq SQSTRING",
    "class_seq :",
    "class_seq : class_seq K_NULLREF",
    "class_seq : class_seq K_CLASS SQSTRING",
    "class_seq : class_seq class_ref",
    "obj_seq :",
    "obj_seq : obj_seq customattr_blob_init",
    "sec_bytearray : ASSIGN bytes_list",
    "sec_bytearray : K_BYTEARRAY bytes_list",
    "sec_decl : D_PERMISSION sec_action type_spec OPEN_PARENS nameval_pairs CLOSE_PARENS",
    "sec_decl : D_PERMISSION sec_action type_spec ASSIGN OPEN_PARENS customattr_blob CLOSE_PARENS",
    "sec_decl : D_PERMISSION sec_action type_spec",
    "sec_decl : D_PERMISSIONSET sec_action sec_bytearray",
    "sec_decl : D_PERMISSIONSET sec_action comp_qstring",
    "sec_decl : D_PERMISSIONSET sec_action ASSIGN OPEN_BRACE permissions CLOSE_BRACE",
    "sec_action : K_REQUEST",
    "sec_action : K_DEMAND",
    "sec_action : K_ASSERT",
    "sec_action : K_DENY",
    "sec_action : K_PERMITONLY",
    "sec_action : K_LINKCHECK",
    "sec_action : K_INHERITCHECK",
    "sec_action : K_REQMIN",
    "sec_action : K_REQOPT",
    "sec_action : K_REQREFUSE",
    "sec_action : K_PREJITGRANT",
    "sec_action : K_PREJITDENY",
    "sec_action : K_NONCASDEMAND",
    "sec_action : K_NONCASLINKDEMAND",
    "sec_action : K_NONCASINHERITANCE",
    "permissions : permission",
    "permissions : permissions COMMA permission",
    "permission : class_ref ASSIGN OPEN_BRACE customattr_blob_nvp CLOSE_BRACE",
    "permission : K_CLASS SQSTRING ASSIGN OPEN_BRACE customattr_blob_nvp CLOSE_BRACE",
    "nameval_pairs : nameval_pair",
    "nameval_pairs : nameval_pairs COMMA nameval_pair",
    "nameval_pair : comp_qstring ASSIGN cavalue",
    "cavalue : truefalse",
    "cavalue : int32",
    "cavalue : K_INT32 OPEN_PARENS int32 CLOSE_PARENS",
    "cavalue : comp_qstring",
    "cavalue : obscure_cavalue",
    "obscure_cavalue : class_ref OPEN_PARENS K_INT8 COLON int32 CLOSE_PARENS",
    "obscure_cavalue : class_ref OPEN_PARENS K_INT16 COLON int32 CLOSE_PARENS",
    "obscure_cavalue : class_ref OPEN_PARENS K_INT32 COLON int32 CLOSE_PARENS",
    "obscure_cavalue : class_ref OPEN_PARENS int32 CLOSE_PARENS",
    "prop_or_field : K_PROPERTY",
    "prop_or_field : K_FIELD",
    "module_head : D_MODULE",
    "module_head : D_MODULE comp_name_str",
    "module_head : D_MODULE K_EXTERN comp_name_str",
    "file_decl : D_FILE file_attr comp_name file_entry D_HASH ASSIGN bytes_list file_entry",
    "file_decl : D_FILE file_attr comp_name file_entry",
    "file_attr :",
    "file_attr : file_attr K_NOMETADATA",
    "file_entry :",
    "file_entry : D_ENTRYPOINT",
    "assembly_all : assembly_head OPEN_BRACE assembly_decls CLOSE_BRACE",
    "assembly_head : D_ASSEMBLY asm_attr comp_name_str",
    "asm_attr :",
    "asm_attr : asm_attr K_RETARGETABLE",
    "asm_attr : asm_attr K_LEGACY K_LIBRARY",
    "asm_attr : asm_attr K_CIL",
    "asm_attr : asm_attr K_X86",
    "asm_attr : asm_attr K_IA64",
    "asm_attr : asm_attr K_AMD64",
    "asm_attr : asm_attr K_NOAPPDOMAIN",
    "asm_attr : asm_attr K_NOPROCESS",
    "asm_attr : asm_attr K_NOMACHINE",
    "assembly_decls :",
    "assembly_decls : assembly_decls assembly_decl",
    "locale_or_culture : D_LOCALE",
    "locale_or_culture : D_CULTURE",
    "assembly_decl : D_PUBLICKEY ASSIGN bytes_list",
    "assembly_decl : D_VER int32 COLON int32 COLON int32 COLON int32",
    "assembly_decl : locale_or_culture comp_qstring",
    "assembly_decl : locale_or_culture ASSIGN bytes_list",
    "assembly_decl : D_HASH K_ALGORITHM int32",
    "assembly_decl : customattr_decl",
    "assembly_decl : sec_decl",
    "assembly_decl : comp_control",
    "assemblyref_all : assemblyref_head OPEN_BRACE assemblyref_decls CLOSE_BRACE",
    "assemblyref_head : D_ASSEMBLY K_EXTERN asm_attr comp_name_str",
    "assemblyref_head : D_ASSEMBLY K_EXTERN asm_attr comp_name_str K_AS comp_name_str",
    "assemblyref_decls :",
    "assemblyref_decls : assemblyref_decls assemblyref_decl",
    "assemblyref_decl : D_VER int32 COLON int32 COLON int32 COLON int32",
    "assemblyref_decl : D_PUBLICKEY ASSIGN bytes_list",
    "assemblyref_decl : D_PUBLICKEYTOKEN ASSIGN bytes_list",
    "assemblyref_decl : locale_or_culture comp_qstring",
    "assemblyref_decl : locale_or_culture ASSIGN bytes_list",
    "assemblyref_decl : D_HASH ASSIGN bytes_list",
    "assemblyref_decl : K_AUTO",
    "assemblyref_decl : customattr_decl",
    "assemblyref_decl : comp_control",
    "exptype_all : exptype_head OPEN_BRACE exptype_decls CLOSE_BRACE",
    "exptype_directive : D_CLASS K_EXTERN",
    "exptype_directive : D_EXPORT",
    "exptype_head : exptype_directive expt_attr comp_name",
    "expt_attr :",
    "expt_attr : expt_attr K_PRIVATE",
    "expt_attr : expt_attr K_PUBLIC",
    "expt_attr : expt_attr K_FORWARDER",
    "expt_attr : expt_attr K_NESTED K_PUBLIC",
    "expt_attr : expt_attr K_NESTED K_PRIVATE",
    "expt_attr : expt_attr K_NESTED K_FAMILY",
    "expt_attr : expt_attr K_NESTED K_ASSEMBLY",
    "expt_attr : expt_attr K_NESTED K_FAMANDASSEM",
    "expt_attr : expt_attr K_NESTED K_FAMORASSEM",
    "exptype_decls :",
    "exptype_decls : exptype_decls exptype_decl",
    "exptype_decl : D_FILE comp_name",
    "exptype_decl : D_CLASS K_EXTERN comp_name",
    "exptype_decl : D_ASSEMBLY K_EXTERN comp_name_str",
    "exptype_decl : mdtoken_type",
    "exptype_decl : D_CLASS int32",
    "exptype_decl : customattr_decl",
    "exptype_decl : comp_control",
    "manifestres_all : manifestres_head OPEN_BRACE manifestres_decls CLOSE_BRACE",
    "manifestres_head : D_MRESOURCE manres_attr comp_name_str",
    "manifestres_head : D_MRESOURCE manres_attr comp_name_str K_AS comp_name_str",
    "manres_attr :",
    "manres_attr : manres_attr K_PUBLIC",
    "manres_attr : manres_attr K_PRIVATE",
    "manifestres_decls :",
    "manifestres_decls : manifestres_decl",
    "manifestres_decls : manifestres_decls manifestres_decl",
    "manifestres_decl : D_FILE comp_name_str K_AT int32",
    "manifestres_decl : D_ASSEMBLY K_EXTERN comp_name_str",
    "manifestres_decl : customattr_decl",
    "manifestres_decl : comp_control",
    "slashed_name : comp_name",
    "slashed_name : slashed_name SLASH comp_name",
    "mdtoken : K_MDTOKEN OPEN_PARENS int32 CLOSE_PARENS",
    "comp_qstring : QSTRING",
    "comp_qstring : comp_qstring PLUS QSTRING",
    "int32 : INT64",
    "int64 : INT64",
    "float64 : FLOAT64",
    "float64 : K_FLOAT32 OPEN_PARENS INT32 CLOSE_PARENS",
    "float64 : K_FLOAT32 OPEN_PARENS INT64 CLOSE_PARENS",
    "float64 : K_FLOAT64 OPEN_PARENS INT64 CLOSE_PARENS",
    "float64 : K_FLOAT64 OPEN_PARENS INT32 CLOSE_PARENS",
    "hexbyte : HEXBYTE",
    "$$17 :",
    "bytes_list : OPEN_PARENS $$17 bytes CLOSE_PARENS",
    "bytes :",
    "bytes : hexbytes",
    "hexbytes : hexbyte",
    "hexbytes : hexbytes hexbyte",
    "truefalse : K_TRUE",
    "truefalse : K_FALSE",
    "id : ID",
    "id : SQSTRING",
    "comp_name_str : comp_name",
    "comp_name : id",
    "comp_name : COMP_NAME",
    "comp_name : comp_name DOT comp_name",
  };
 public static string getRule (int index) {
    return yyRule [index];
 }
}
  protected static readonly string [] yyNames = {    
    "end-of-file",null,null,null,null,null,null,null,null,null,null,null,
    null,null,null,null,null,null,null,null,null,null,null,null,null,null,
    null,null,null,null,null,null,null,"'!'",null,null,null,null,"'&'",
    null,"'('","')'","'*'","'+'","','","'-'","'.'","'/'",null,null,null,
    null,null,null,null,null,null,null,"':'","';'","'<'","'='","'>'",null,
    null,null,null,null,null,null,null,null,null,null,null,null,null,null,
    null,null,null,null,null,null,null,null,null,null,null,null,null,
    "'['",null,"']'",null,null,null,null,null,null,null,null,null,null,
    null,null,null,null,null,null,null,null,null,null,null,null,null,null,
    null,null,null,null,null,"'{'",null,"'}'",null,null,null,null,null,
    null,null,null,null,null,null,null,null,null,null,null,null,null,null,
    null,null,null,null,null,null,null,null,null,null,null,null,null,null,
    null,null,null,null,null,null,null,null,null,null,null,null,null,null,
    null,null,null,null,null,null,null,null,null,null,null,null,null,null,
    null,null,null,null,null,null,null,null,null,null,null,null,null,null,
    null,null,null,null,null,null,null,null,null,null,null,null,null,null,
    null,null,null,null,null,null,null,null,null,null,null,null,null,null,
    null,null,null,null,null,null,null,null,null,null,null,null,null,null,
    null,null,null,null,null,null,null,null,null,null,null,null,null,null,
    "UNKNOWN","EOF","ID","QSTRING","SQSTRING","COMP_NAME","INT32","INT64",
    "FLOAT64","HEXBYTE","DOT","OPEN_BRACE","CLOSE_BRACE","OPEN_BRACKET",
    "CLOSE_BRACKET","OPEN_PARENS","CLOSE_PARENS","COMMA","COLON",
    "DOUBLE_COLON","\"::\"","SEMICOLON","ASSIGN","STAR","AMPERSAND",
    "PLUS","SLASH","BANG","ELLIPSIS","\"...\"","DASH",
    "OPEN_ANGLE_BRACKET","CLOSE_ANGLE_BRACKET","INSTR_NONE","INSTR_I",
    "INSTR_I8","INSTR_R","INSTR_BRTARGET","INSTR_METHOD","INSTR_FIELD",
    "INSTR_TYPE","INSTR_STRING","INSTR_SIG","INSTR_TOK","INSTR_SWITCH",
    "INSTR_PHI","INSTR_LOCAL","INSTR_PARAM","D_ASSEMBLY","D_CCTOR",
    "D_CLASS","D_IMAGEBASE","D_CORFLAGS","D_CTOR","D_CUSTOM","D_DATA",
    "D_EMITBYTE","D_ENTRYPOINT","D_EVENT","D_EXPORT","D_FIELD","D_FILE",
    "D_FIRE","D_GET","D_HASH","D_LANGUAGE","D_LINE","D_XLINE","D_LOCALE",
    "D_CULTURE","D_LOCALIZED","D_LOCALS","D_MAXSTACK","D_METHOD",
    "D_MODULE","D_MRESOURCE","D_MANIFESTRES","D_NAMESPACE","D_OTHER",
    "D_OVERRIDE","D_PACK","D_PARAM","D_PERMISSION","D_PERMISSIONSET",
    "D_PROPERTY","D_PUBLICKEY","D_PUBLICKEYTOKEN","D_ADDON","D_REMOVEON",
    "D_SET","D_SIZE","D_STACKRESERVE","D_SUBSYSTEM","D_TRY","D_VER",
    "D_VTABLE","D_VTENTRY","D_VTFIXUP","D_ZEROINIT","D_THIS","D_BASE",
    "D_NESTER","D_TYPELIST","D_MSCORLIB","D_PDIRECT","D_TYPEDEF",
    "D_XDEFINE","D_XUNDEF","D_XIFDEF","D_XIFNDEF","D_XELSE","D_XENDIF",
    "D_XINCLUDE","K_AT","K_AS","K_IMPLICITCOM","K_IMPLICITRES","K_EXTERN",
    "K_INSTANCE","K_EXPLICIT","K_DEFAULT","K_VARARG","K_UNMANAGED",
    "K_CDECL","K_STDCALL","K_THISCALL","K_FASTCALL","K_MARSHAL","K_IN",
    "K_OUT","K_OPT","K_STATIC","K_PUBLIC","K_PRIVATE","K_FAMILY",
    "K_INITONLY","K_RTSPECIALNAME","K_STRICT","K_SPECIALNAME",
    "K_ASSEMBLY","K_FAMANDASSEM","K_FAMORASSEM","K_PRIVATESCOPE",
    "K_LITERAL","K_NOTSERIALIZED","K_VALUE","K_NOT_IN_GC_HEAP",
    "K_INTERFACE","K_SEALED","K_ABSTRACT","K_AUTO","K_SEQUENTIAL",
    "K_ANSI","K_UNICODE","K_AUTOCHAR","K_BESTFIT","K_IMPORT",
    "K_SERIALIZABLE","K_NESTED","K_EXTENDS","K_IMPLEMENTS","K_FINAL",
    "K_VIRTUAL","K_HIDEBYSIG","K_NEWSLOT","K_UNMANAGEDEXP",
    "K_PINVOKEIMPL","K_NOMANGLE","K_LASTERR","K_WINAPI","K_PLATFORMAPI",
    "K_NATIVE","K_IL","K_CIL","K_OPTIL","K_MANAGED","K_FORWARDREF",
    "K_RUNTIME","K_INTERNALCALL","K_SYNCHRONIZED","K_NOINLINING",
    "K_NOOPTIMIZATION","K_CUSTOM","K_FIXED","K_SYSSTRING","K_ARRAY",
    "K_VARIANT","K_CURRENCY","K_SYSCHAR","K_VOID","K_BOOL","K_INT8",
    "K_INT16","K_INT32","K_INT64","K_FLOAT","K_FLOAT32","K_FLOAT64",
    "K_ERROR","K_UNSIGNED","K_UINT","K_UINT8","K_UINT16","K_UINT32",
    "K_UINT64","K_DECIMAL","K_DATE","K_BSTR","K_LPSTR","K_LPWSTR",
    "K_LPTSTR","K_OBJECTREF","K_IUNKNOWN","K_IDISPATCH","K_STRUCT",
    "K_SAFEARRAY","K_INT","K_BYVALSTR","K_TBSTR","K_LPVOID","K_ANY",
    "K_LPSTRUCT","K_NULL","K_VECTOR","K_HRESULT","K_CARRAY",
    "K_USERDEFINED","K_RECORD","K_FILETIME","K_BLOB","K_STREAM",
    "K_STORAGE","K_STREAMED_OBJECT","K_STORED_OBJECT","K_BLOB_OBJECT",
    "K_CF","K_CLSID","K_METHOD","K_CLASS","K_PINNED","K_MODREQ",
    "K_MODOPT","K_TYPEDREF","K_TYPE","K_CHAR","K_WCHAR","K_FROMUNMANAGED",
    "K_CALLMOSTDERIVED","K_RETAINAPPDOMAIN","K_BYTEARRAY","K_WITH",
    "K_INIT","K_TO","K_CATCH","K_FILTER","K_FINALLY","K_FAULT",
    "K_HANDLER","K_TLS","K_FIELD","K_PROPERTY","K_REQUEST","K_DEMAND",
    "K_ASSERT","K_DENY","K_PERMITONLY","K_LINKCHECK","K_INHERITCHECK",
    "K_REQMIN","K_REQOPT","K_REQREFUSE","K_PREJITGRANT","K_PREJITDENY",
    "K_NONCASDEMAND","K_NONCASLINKDEMAND","K_NONCASINHERITANCE",
    "K_NOMETADATA","K_ALGORITHM","K_RETARGETABLE","K_LEGACY","K_LIBRARY",
    "K_X86","K_IA64","K_AMD64","K_PRESERVESIG","K_BEFOREFIELDINIT",
    "K_ALIGNMENT","K_NULLREF","K_VALUETYPE","K_COMPILERCONTROLLED",
    "K_REQSECOBJ","K_ENUM","K_OBJECT","K_STRING","K_TRUE","K_FALSE",
    "K_ON","K_OFF","K_CHARMAPERROR","K_MDTOKEN","K_FLAGS","K_CALLCONV",
    "K_NOAPPDOMAIN","K_NOMACHINE","K_NOPROCESS","K_ILLEGAL","K_UNUSED",
    "K_WRAPPER","K_FORWARDER",
  };

  /** index-checked interface to yyNames[].
      @param token single character or %token value.
      @return token name or [illegal] or [unknown].
    */
  public static string yyname (int token) {
    if ((token < 0) || (token > yyNames.Length)) return "[illegal]";
    string name;
    if ((name = yyNames[token]) != null) return name;
    return "[unknown]";
  }

  int yyExpectingState;
  /** computes list of expected tokens on error by tracing the tables.
      @param state for which to compute the list.
      @return list of token names.
    */
  protected int [] yyExpectingTokens (int state){
    int token, n, len = 0;
    bool[] ok = new bool[yyNames.Length];
    if ((n = yySindex[state]) != 0)
      for (token = n < 0 ? -n : 0;
           (token < yyNames.Length) && (n+token < yyTable.Length); ++ token)
        if (yyCheck[n+token] == token && !ok[token] && yyNames[token] != null) {
          ++ len;
          ok[token] = true;
        }
    if ((n = yyRindex[state]) != 0)
      for (token = n < 0 ? -n : 0;
           (token < yyNames.Length) && (n+token < yyTable.Length); ++ token)
        if (yyCheck[n+token] == token && !ok[token] && yyNames[token] != null) {
          ++ len;
          ok[token] = true;
        }
    int [] result = new int [len];
    for (n = token = 0; n < len;  ++ token)
      if (ok[token]) result[n++] = token;
    return result;
  }
  protected string[] yyExpecting (int state) {
    int [] tokens = yyExpectingTokens (state);
    string [] result = new string[tokens.Length];
    for (int n = 0; n < tokens.Length;  n++)
      result[n++] = yyNames[tokens [n]];
    return result;
  }

  /** the generated parser, with debugging messages.
      Maintains a state and a value stack, currently with fixed maximum size.
      @param yyLex scanner.
      @param yydebug debug message writer implementing yyDebug, or null.
      @return result of the last reduction, if any.
      @throws yyException on irrecoverable parse error.
    */
  internal Object yyparse (yyParser.yyInput yyLex, Object yyd)
				 {
    this.debug = (yydebug.yyDebug)yyd;
    return yyparse(yyLex);
  }

  /** initial size and increment of the state/value stack [default 256].
      This is not final so that it can be overwritten outside of invocations
      of yyparse().
    */
  protected int yyMax;

  /** executed at the beginning of a reduce action.
      Used as $$ = yyDefault($1), prior to the user-specified action, if any.
      Can be overwritten to provide deep copy, etc.
      @param first value for $1, or null.
      @return first.
    */
  protected Object yyDefault (Object first) {
    return first;
  }

	static int[] global_yyStates;
	static object[] global_yyVals;
	protected bool use_global_stacks;
	object[] yyVals;					// value stack
	object yyVal;						// value stack ptr
	int yyToken;						// current input
	int yyTop;

  /** the generated parser.
      Maintains a state and a value stack, currently with fixed maximum size.
      @param yyLex scanner.
      @return result of the last reduction, if any.
      @throws yyException on irrecoverable parse error.
    */
  internal Object yyparse (yyParser.yyInput yyLex)
  {
    if (yyMax <= 0) yyMax = 256;		// initial size
    int yyState = 0;                   // state stack ptr
    int [] yyStates;               	// state stack 
    yyVal = null;
    yyToken = -1;
    int yyErrorFlag = 0;				// #tks to shift
	if (use_global_stacks && global_yyStates != null) {
		yyVals = global_yyVals;
		yyStates = global_yyStates;
   } else {
		yyVals = new object [yyMax];
		yyStates = new int [yyMax];
		if (use_global_stacks) {
			global_yyVals = yyVals;
			global_yyStates = yyStates;
		}
	}

    /*yyLoop:*/ for (yyTop = 0;; ++ yyTop) {
      if (yyTop >= yyStates.Length) {			// dynamically increase
        global::System.Array.Resize (ref yyStates, yyStates.Length+yyMax);
        global::System.Array.Resize (ref yyVals, yyVals.Length+yyMax);
      }
      yyStates[yyTop] = yyState;
      yyVals[yyTop] = yyVal;
      if (debug != null) debug.push(yyState, yyVal);

      /*yyDiscarded:*/ while (true) {	// discarding a token does not change stack
        int yyN;
        if ((yyN = yyDefRed[yyState]) == 0) {	// else [default] reduce (yyN)
          if (yyToken < 0) {
            yyToken = yyLex.advance() ? yyLex.token() : 0;
            if (debug != null)
              debug.lex(yyState, yyToken, yyname(yyToken), yyLex.value());
          }
          if ((yyN = yySindex[yyState]) != 0 && ((yyN += yyToken) >= 0)
              && (yyN < yyTable.Length) && (yyCheck[yyN] == yyToken)) {
            if (debug != null)
              debug.shift(yyState, yyTable[yyN], yyErrorFlag-1);
            yyState = yyTable[yyN];		// shift to yyN
            yyVal = yyLex.value();
            yyToken = -1;
            if (yyErrorFlag > 0) -- yyErrorFlag;
            goto continue_yyLoop;
          }
          if ((yyN = yyRindex[yyState]) != 0 && (yyN += yyToken) >= 0
              && yyN < yyTable.Length && yyCheck[yyN] == yyToken)
            yyN = yyTable[yyN];			// reduce (yyN)
          else
            switch (yyErrorFlag) {
  
            case 0:
              yyExpectingState = yyState;
              // yyerror(String.Format ("syntax error, got token `{0}'", yyname (yyToken)), yyExpecting(yyState));
              if (debug != null) debug.error("syntax error");
              if (yyToken == 0 /*eof*/ || yyToken == eof_token) throw new yyParser.yyUnexpectedEof ();
              goto case 1;
            case 1: case 2:
              yyErrorFlag = 3;
              do {
                if ((yyN = yySindex[yyStates[yyTop]]) != 0
                    && (yyN += Token.yyErrorCode) >= 0 && yyN < yyTable.Length
                    && yyCheck[yyN] == Token.yyErrorCode) {
                  if (debug != null)
                    debug.shift(yyStates[yyTop], yyTable[yyN], 3);
                  yyState = yyTable[yyN];
                  yyVal = yyLex.value();
                  goto continue_yyLoop;
                }
                if (debug != null) debug.pop(yyStates[yyTop]);
              } while (-- yyTop >= 0);
              if (debug != null) debug.reject();
              throw new yyParser.yyException("irrecoverable syntax error");
  
            case 3:
              if (yyToken == 0) {
                if (debug != null) debug.reject();
                throw new yyParser.yyException("irrecoverable syntax error at end-of-file");
              }
              if (debug != null)
                debug.discard(yyState, yyToken, yyname(yyToken),
  							yyLex.value());
              yyToken = -1;
              goto continue_yyDiscarded;		// leave stack alone
            }
        }
        int yyV = yyTop + 1-yyLen[yyN];
        if (debug != null)
          debug.reduce(yyState, yyStates[yyV-1], yyN, YYRules.getRule (yyN), yyLen[yyN]);
        yyVal = yyV > yyTop ? null : yyVals[yyV]; // yyVal = yyDefault(yyV > yyTop ? null : yyVals[yyV]);
        switch (yyN) {
case 9:
#line 452 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
			}
  break;
case 10:
#line 455 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
			}
  break;
case 17:
  case_17();
  break;
case 18:
  case_18();
  break;
case 19:
#line 477 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				codegen.CurrentModule.Kind = (ModuleKind) (int) yyVals[0+yyTop];
			}
  break;
case 20:
#line 481 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				codegen.CurrentModule.Attributes = (ModuleAttributes) (int) yyVals[0+yyTop];
			}
  break;
case 21:
#line 486 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				var fa = (int) yyVals[0+yyTop];
			}
  break;
case 22:
#line 490 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				var ib = (long) yyVals[0+yyTop];
			}
  break;
case 25:
  case_25();
  break;
case 54:
#line 544 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
			}
  break;
case 60:
#line 555 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
			}
  break;
case 70:
#line 572 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				codegen.CurrentNamespace = namespace_stack.Pop ();
			}
  break;
case 71:
  case_71();
  break;
case 72:
  case_72();
  break;
case 73:
  case_73();
  break;
case 74:
  case_74();
  break;
case 75:
  case_75();
  break;
case 76:
  case_76();
  break;
case 77:
  case_77();
  break;
case 78:
#line 695 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { type_attr_visibility_set = true; }
  break;
case 79:
#line 697 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = (TypeAttributes) yyVals[-2+yyTop] | TypeAttributes.NestedPrivate; }
  break;
case 80:
#line 698 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = (TypeAttributes) yyVals[-2+yyTop] | TypeAttributes.NestedPublic; }
  break;
case 81:
#line 699 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = (TypeAttributes) yyVals[-2+yyTop] | TypeAttributes.NestedFamily; }
  break;
case 82:
#line 700 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = (TypeAttributes) yyVals[-2+yyTop] | TypeAttributes.NestedAssembly;}
  break;
case 83:
#line 701 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = (TypeAttributes) yyVals[-2+yyTop] | TypeAttributes.NestedFamANDAssem; }
  break;
case 84:
#line 702 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = (TypeAttributes) yyVals[-2+yyTop] | TypeAttributes.NestedFamORAssem; }
  break;
case 85:
#line 703 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { is_value_class = true; }
  break;
case 86:
#line 704 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { is_enum_class = true; }
  break;
case 87:
#line 705 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = (TypeAttributes) yyVals[-1+yyTop] | TypeAttributes.Interface | TypeAttributes.Abstract; }
  break;
case 88:
#line 706 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = (TypeAttributes) yyVals[-1+yyTop] | TypeAttributes.Sealed; }
  break;
case 89:
#line 707 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = (TypeAttributes) yyVals[-1+yyTop] | TypeAttributes.Abstract; }
  break;
case 91:
#line 709 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = (TypeAttributes) yyVals[-1+yyTop] | TypeAttributes.SequentialLayout; }
  break;
case 92:
#line 710 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = (TypeAttributes) yyVals[-1+yyTop] | TypeAttributes.ExplicitLayout; }
  break;
case 94:
#line 712 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = (TypeAttributes) yyVals[-1+yyTop] | TypeAttributes.UnicodeClass; }
  break;
case 95:
#line 713 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = (TypeAttributes) yyVals[-1+yyTop] | TypeAttributes.AutoClass; }
  break;
case 96:
#line 714 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = (TypeAttributes) yyVals[-1+yyTop] | TypeAttributes.Import; }
  break;
case 97:
#line 715 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = (TypeAttributes) yyVals[-1+yyTop] | TypeAttributes.Serializable; }
  break;
case 98:
#line 716 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = (TypeAttributes) yyVals[-1+yyTop] | TypeAttributes.BeforeFieldInit; }
  break;
case 99:
#line 717 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = (TypeAttributes) yyVals[-1+yyTop] | TypeAttributes.SpecialName; }
  break;
case 100:
#line 718 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = (TypeAttributes) yyVals[-1+yyTop] | TypeAttributes.RTSpecialName; }
  break;
case 101:
#line 722 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				yyVal = (TypeAttributes) yyVals[-4+yyTop] | (TypeAttributes) (int) yyVals[-1+yyTop];
			}
  break;
case 103:
#line 729 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				yyVal = yyVals[0+yyTop];
			}
  break;
case 104:
#line 735 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				yyVal = new List<TypeCreator> ();
			}
  break;
case 107:
  case_107();
  break;
case 108:
#line 749 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				((List<TypeCreator>) yyVals[-2+yyTop]).Add ((TypeCreator) yyVals[0+yyTop]);
			}
  break;
case 109:
#line 755 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				yyVal = new List<GenericParameter> ();
			}
  break;
case 110:
#line 759 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				yyVal = yyVals[-1+yyTop];
			}
  break;
case 111:
  case_111();
  break;
case 112:
#line 771 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				((List<GenericParameter>) yyVals[-2+yyTop]).Add ((GenericParameter) yyVals[0+yyTop]);
			}
  break;
case 113:
  case_113();
  break;
case 114:
#line 791 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				yyVal = GenericParameterAttributes.NonVariant;
			}
  break;
case 115:
#line 792 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = (GenericParameterAttributes) yyVals[-1+yyTop] | GenericParameterAttributes.Covariant; }
  break;
case 116:
#line 793 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = (GenericParameterAttributes) yyVals[-1+yyTop] | GenericParameterAttributes.Contravariant; }
  break;
case 117:
#line 794 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = (GenericParameterAttributes) yyVals[-1+yyTop] | GenericParameterAttributes.DefaultConstructorConstraint; }
  break;
case 118:
#line 795 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = (GenericParameterAttributes) yyVals[-1+yyTop] | GenericParameterAttributes.NotNullableValueTypeConstraint; }
  break;
case 119:
#line 796 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = (GenericParameterAttributes) yyVals[-1+yyTop] | GenericParameterAttributes.ReferenceTypeConstraint; }
  break;
case 121:
#line 805 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				yyVal = new List<TypeCreator> ();
			}
  break;
case 122:
#line 809 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				yyVal = yyVals[-1+yyTop];
			}
  break;
case 123:
  case_123();
  break;
case 124:
#line 821 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				((List<TypeCreator>) yyVals[-2+yyTop]).Add ((TypeCreator) yyVals[0+yyTop]);
			}
  break;
case 133:
  case_133();
  break;
case 135:
  case_135();
  break;
case 137:
  case_137();
  break;
case 138:
  case_138();
  break;
case 139:
  case_139();
  break;
case 140:
  case_140();
  break;
case 143:
  case_143();
  break;
case 144:
  case_144();
  break;
case 145:
  case_145();
  break;
case 146:
#line 940 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				((List<TypeCreator>) yyVals[-2+yyTop]).Add ((TypeCreator) yyVals[0+yyTop]);
			}
  break;
case 147:
  case_147();
  break;
case 148:
  case_148();
  break;
case 149:
#line 959 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				/* TODO: How does this work?*/
			}
  break;
case 150:
  case_150();
  break;
case 151:
  case_151();
  break;
case 153:
#line 990 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				yyVal = DeferType (() => codegen.CurrentType);
			}
  break;
case 154:
  case_154();
  break;
case 155:
  case_155();
  break;
case 156:
  case_156();
  break;
case 159:
#line 1020 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				yyVal = yyVals[0+yyTop];
			}
  break;
case 160:
  case_160();
  break;
case 161:
  case_161();
  break;
case 162:
  case_162();
  break;
case 163:
  case_163();
  break;
case 164:
  case_164();
  break;
case 165:
  case_165();
  break;
case 166:
  case_166();
  break;
case 167:
  case_167();
  break;
case 168:
  case_168();
  break;
case 169:
  case_169();
  break;
case 170:
  case_170();
  break;
case 171:
  case_171();
  break;
case 172:
  case_172();
  break;
case 173:
  case_173();
  break;
case 174:
  case_174();
  break;
case 175:
#line 1172 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = DeferType (() => codegen.Corlib.Void); }
  break;
case 176:
#line 1173 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = DeferType (() => codegen.Corlib.TypedReference); }
  break;
case 177:
#line 1174 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = DeferType (() => codegen.Corlib.IntPtr); }
  break;
case 178:
#line 1175 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = DeferType (() => codegen.Corlib.UIntPtr); }
  break;
case 179:
#line 1176 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = DeferType (() => codegen.Corlib.UIntPtr); }
  break;
case 180:
#line 1177 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = DeferType (() => codegen.Corlib.Single); }
  break;
case 182:
  case_182();
  break;
case 191:
#line 1203 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = DeferType (() => codegen.Corlib.SByte); }
  break;
case 192:
#line 1204 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = DeferType (() => codegen.Corlib.Int16); }
  break;
case 193:
#line 1205 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = DeferType (() => codegen.Corlib.Int32); }
  break;
case 194:
#line 1206 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = DeferType (() => codegen.Corlib.Int64); }
  break;
case 195:
#line 1207 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = DeferType (() => codegen.Corlib.Byte); }
  break;
case 196:
#line 1208 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = DeferType (() => codegen.Corlib.UInt16); }
  break;
case 197:
#line 1209 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = DeferType (() => codegen.Corlib.UInt32); }
  break;
case 198:
#line 1210 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = DeferType (() => codegen.Corlib.UInt64); }
  break;
case 199:
#line 1212 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = DeferType (() => codegen.Corlib.Single); }
  break;
case 200:
#line 1213 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = DeferType (() => codegen.Corlib.Double); }
  break;
case 201:
#line 1214 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = DeferType (() => codegen.Corlib.Char); }
  break;
case 202:
#line 1215 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = DeferType (() => codegen.Corlib.Boolean); }
  break;
case 203:
#line 1216 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = DeferType (() => codegen.Corlib.String); }
  break;
case 207:
#line 1227 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				yyVal = yyVals[-1+yyTop];
			}
  break;
case 208:
  case_208();
  break;
case 209:
#line 1239 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				((List<TypeCreator>) yyVals[-2+yyTop]).Add ((TypeCreator) yyVals[0+yyTop]);
			}
  break;
case 211:
  case_211();
  break;
case 212:
  case_212();
  break;
case 214:
  case_214();
  break;
case 215:
#line 1273 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				((List<ArrayDimension>) yyVals[-2+yyTop]).Add ((ArrayDimension) yyVals[0+yyTop]);
			}
  break;
case 216:
#line 1279 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				yyVal = new ArrayDimension (null, null);
			}
  break;
case 217:
#line 1283 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				yyVal = new ArrayDimension (null, null);
			}
  break;
case 218:
  case_218();
  break;
case 219:
  case_219();
  break;
case 220:
#line 1303 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				yyVal = new ArrayDimension ((int) yyVals[-1+yyTop], null);
			}
  break;
case 221:
#line 1306 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { is_instance_call = true; yyVal = yyVals[0+yyTop] ?? MethodCallingConvention.Default; }
  break;
case 222:
#line 1307 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { is_explicit_call = true; yyVal = yyVals[0+yyTop] ?? MethodCallingConvention.Default; }
  break;
case 224:
#line 1312 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				yyVal = (MethodCallingConvention) yyVals[-1+yyTop];
			}
  break;
case 225:
#line 1318 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				yyVal = MethodCallingConvention.Default;
			}
  break;
case 226:
#line 1319 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = MethodCallingConvention.Default; }
  break;
case 227:
#line 1320 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = MethodCallingConvention.VarArg; }
  break;
case 228:
#line 1321 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = MethodCallingConvention.C; }
  break;
case 229:
#line 1322 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = MethodCallingConvention.StdCall; }
  break;
case 230:
#line 1323 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = MethodCallingConvention.ThisCall; }
  break;
case 231:
#line 1324 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = MethodCallingConvention.FastCall; }
  break;
case 233:
  case_233();
  break;
case 234:
  case_234();
  break;
case 235:
  case_235();
  break;
case 236:
  case_236();
  break;
case 237:
  case_237();
  break;
case 238:
  case_238();
  break;
case 239:
  case_239();
  break;
case 240:
  case_240();
  break;
case 241:
  case_241();
  break;
case 242:
  case_242();
  break;
case 243:
  case_243();
  break;
case 244:
  case_244();
  break;
case 245:
  case_245();
  break;
case 246:
  case_246();
  break;
case 247:
#line 1434 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				yyVal = new MarshalInfo ((NativeType) yyVals[0+yyTop]);
			}
  break;
case 250:
#line 1441 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = NativeType.Currency; }
  break;
case 251:
#line 1442 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = NativeType.Boolean; }
  break;
case 252:
#line 1443 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = NativeType.I1; }
  break;
case 253:
#line 1444 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = NativeType.I2; }
  break;
case 254:
#line 1445 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = NativeType.I4; }
  break;
case 255:
#line 1446 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = NativeType.I8; }
  break;
case 256:
#line 1447 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = NativeType.R4; }
  break;
case 257:
#line 1448 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = NativeType.R8; }
  break;
case 258:
#line 1449 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = NativeType.Error; }
  break;
case 259:
#line 1450 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = NativeType.U1; }
  break;
case 260:
#line 1451 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = NativeType.U2; }
  break;
case 261:
#line 1452 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = NativeType.U4; }
  break;
case 262:
#line 1453 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = NativeType.U8; }
  break;
case 263:
#line 1454 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = NativeType.BStr; }
  break;
case 264:
#line 1455 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = NativeType.LPStr; }
  break;
case 265:
#line 1456 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = NativeType.LPWStr; }
  break;
case 266:
#line 1457 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = NativeType.LPTStr; }
  break;
case 267:
#line 1458 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = NativeType.IUnknown; }
  break;
case 268:
#line 1459 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = NativeType.IDispatch; }
  break;
case 269:
#line 1460 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = NativeType.Struct; }
  break;
case 270:
#line 1461 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = NativeType.IntF; }
  break;
case 271:
#line 1462 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = NativeType.Int; }
  break;
case 272:
#line 1463 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = NativeType.UInt; }
  break;
case 273:
#line 1464 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = NativeType.ByValStr; }
  break;
case 274:
#line 1465 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = NativeType.ANSIBStr; }
  break;
case 275:
#line 1466 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = NativeType.TBStr; }
  break;
case 276:
#line 1467 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = NativeType.VariantBool; }
  break;
case 277:
#line 1468 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = NativeType.Func; }
  break;
case 278:
#line 1469 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = NativeType.ASAny; }
  break;
case 279:
#line 1470 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = NativeType.LPStruct; }
  break;
case 280:
  case_280();
  break;
case 288:
#line 1487 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = VariantType.Variant; }
  break;
case 289:
#line 1488 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = VariantType.CY; }
  break;
case 290:
#line 1489 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = VariantType.Bool; }
  break;
case 291:
#line 1490 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = VariantType.I1; }
  break;
case 292:
#line 1491 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = VariantType.I2; }
  break;
case 293:
#line 1492 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = VariantType.I4; }
  break;
case 294:
#line 1493 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = VariantType.R4; }
  break;
case 295:
#line 1494 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = VariantType.R8; }
  break;
case 296:
#line 1495 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = VariantType.UI1; }
  break;
case 297:
#line 1496 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = VariantType.UI2; }
  break;
case 298:
#line 1497 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = VariantType.UI4; }
  break;
case 299:
#line 1498 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = VariantType.Decimal; }
  break;
case 300:
#line 1499 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = VariantType.Date; }
  break;
case 301:
#line 1500 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = VariantType.BStr; }
  break;
case 302:
#line 1501 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = VariantType.Unknown; }
  break;
case 303:
#line 1502 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = VariantType.Dispatch; }
  break;
case 304:
#line 1503 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = VariantType.Int; }
  break;
case 305:
#line 1504 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = VariantType.UInt; }
  break;
case 306:
#line 1505 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = VariantType.Error; }
  break;
case 307:
  case_307();
  break;
case 333:
  case_333();
  break;
case 335:
#line 1588 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				yyVal = yyVals[-1+yyTop];
			}
  break;
case 336:
#line 1594 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				yyVal = FieldAttributes.CompilerControlled;
			}
  break;
case 337:
#line 1595 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = (FieldAttributes) yyVals[-1+yyTop] | FieldAttributes.Public; }
  break;
case 338:
#line 1596 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = (FieldAttributes) yyVals[-1+yyTop] | FieldAttributes.Private; }
  break;
case 339:
#line 1597 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = (FieldAttributes) yyVals[-1+yyTop] | FieldAttributes.Family; }
  break;
case 340:
#line 1598 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = (FieldAttributes) yyVals[-1+yyTop] | FieldAttributes.Assembly; }
  break;
case 341:
#line 1599 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = (FieldAttributes) yyVals[-1+yyTop] | FieldAttributes.FamANDAssem; }
  break;
case 342:
#line 1600 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = (FieldAttributes) yyVals[-1+yyTop] | FieldAttributes.FamORAssem; }
  break;
case 344:
#line 1602 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = (FieldAttributes) yyVals[-1+yyTop] | FieldAttributes.Static; }
  break;
case 345:
#line 1603 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = (FieldAttributes) yyVals[-1+yyTop] | FieldAttributes.InitOnly; }
  break;
case 346:
#line 1604 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = (FieldAttributes) yyVals[-1+yyTop] | FieldAttributes.RTSpecialName; }
  break;
case 347:
#line 1605 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = (FieldAttributes) yyVals[-1+yyTop] | FieldAttributes.SpecialName; }
  break;
case 348:
#line 1606 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = (FieldAttributes) yyVals[-1+yyTop] | FieldAttributes.Literal; }
  break;
case 349:
#line 1607 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = (FieldAttributes) yyVals[-1+yyTop] | FieldAttributes.NotSerialized; }
  break;
case 351:
#line 1614 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				yyVal = yyVals[-1+yyTop];
			}
  break;
case 353:
#line 1621 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				yyVal = yyVals[0+yyTop];
			}
  break;
case 355:
#line 1628 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				yyVal = yyVals[0+yyTop];
			}
  break;
case 356:
#line 1634 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				yyVal = (float) (double) yyVals[-1+yyTop];
			}
  break;
case 357:
#line 1638 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				yyVal = yyVals[-1+yyTop];
			}
  break;
case 358:
#line 1642 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				yyVal = BitConverter.ToSingle (BitConverter.GetBytes ((long) yyVals[-1+yyTop]), BitConverter.IsLittleEndian ? 0 : 4);
			}
  break;
case 359:
#line 1646 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				yyVal = BitConverter.Int64BitsToDouble ((long) yyVals[-1+yyTop]);
			}
  break;
case 360:
#line 1650 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				yyVal = yyVals[-1+yyTop];
			}
  break;
case 361:
#line 1654 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				yyVal = (ulong) (long) yyVals[-1+yyTop];
			}
  break;
case 362:
#line 1658 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				yyVal = (int) (long) yyVals[-1+yyTop];
			}
  break;
case 363:
#line 1662 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				yyVal = (uint) (long) yyVals[-1+yyTop];
			}
  break;
case 364:
#line 1666 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				yyVal = (short) (long) yyVals[-1+yyTop];
			}
  break;
case 365:
#line 1670 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				yyVal = (ushort) (long) yyVals[-1+yyTop];
			}
  break;
case 366:
#line 1674 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				yyVal = (sbyte) (long) yyVals[-1+yyTop];
			}
  break;
case 367:
#line 1678 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				yyVal = (byte) (long) yyVals[-1+yyTop];
			}
  break;
case 368:
#line 1682 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				yyVal = (char) (long) yyVals[-1+yyTop];
			}
  break;
case 369:
#line 1686 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				yyVal = yyVals[-1+yyTop];
			}
  break;
case 370:
#line 1690 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				yyVal = yyVals[0+yyTop];
			}
  break;
case 373:
#line 1698 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				yyVal = new Null ();
			}
  break;
case 374:
  case_374();
  break;
case 377:
#line 1718 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				yyVal = yyVals[0+yyTop];
			}
  break;
case 378:
#line 1724 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				yyVal = false;
			}
  break;
case 379:
#line 1728 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				yyVal = true;
			}
  break;
case 380:
#line 1734 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				yyVal = false;
			}
  break;
case 381:
#line 1738 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				yyVal = true;
			}
  break;
case 382:
#line 1744 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				yyVal = yyVals[-1+yyTop];
			}
  break;
case 383:
  case_383();
  break;
case 384:
  case_384();
  break;
case 385:
#line 1762 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				((List<object>) yyVals[-2+yyTop]).Add (yyVals[0+yyTop]);
			}
  break;
case 386:
#line 1767 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
			}
  break;
case 387:
#line 1773 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				yyVal = yyVals[-1+yyTop];
			}
  break;
case 388:
#line 1777 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				yyVal = IntPtr.Zero;
			}
  break;
case 389:
#line 1781 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				yyVal = yyVals[0+yyTop];
			}
  break;
case 390:
#line 1785 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				yyVal = yyVals[-2+yyTop];
			}
  break;
case 391:
#line 1789 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				yyVal = (float) (double) yyVals[-2+yyTop];
			}
  break;
case 392:
#line 1793 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				yyVal = yyVals[-2+yyTop];
			}
  break;
case 393:
#line 1797 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				yyVal = yyVals[-2+yyTop];
			}
  break;
case 394:
#line 1801 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				yyVal = (short) (int) yyVals[-2+yyTop];
			}
  break;
case 395:
#line 1805 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				yyVal = (byte) (int) yyVals[-2+yyTop];
			}
  break;
case 396:
#line 1809 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				yyVal = 0.0f;
			}
  break;
case 397:
#line 1813 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				yyVal = 0.0d;
			}
  break;
case 398:
#line 1817 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				yyVal = (long) 0;
			}
  break;
case 399:
#line 1821 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				yyVal = (int) 0;
			}
  break;
case 400:
#line 1825 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				yyVal = (short) 0;
			}
  break;
case 401:
#line 1829 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				yyVal = (byte) 0;
			}
  break;
case 402:
  case_402();
  break;
case 403:
  case_403();
  break;
case 404:
  case_404();
  break;
case 405:
  case_405();
  break;
case 406:
#line 2000 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				yyVal = MethodAttributes.CompilerControlled;
			}
  break;
case 407:
#line 2001 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = (MethodAttributes) yyVals[-1+yyTop] | MethodAttributes.Static; }
  break;
case 408:
#line 2002 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = (MethodAttributes) yyVals[-1+yyTop] | MethodAttributes.Public; }
  break;
case 409:
#line 2003 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = (MethodAttributes) yyVals[-1+yyTop] | MethodAttributes.Private; }
  break;
case 410:
#line 2004 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = (MethodAttributes) yyVals[-1+yyTop] | MethodAttributes.Family; }
  break;
case 411:
#line 2005 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = (MethodAttributes) yyVals[-1+yyTop] | MethodAttributes.Assembly; }
  break;
case 412:
#line 2006 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = (MethodAttributes) yyVals[-1+yyTop] | MethodAttributes.FamANDAssem; }
  break;
case 413:
#line 2007 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = (MethodAttributes) yyVals[-1+yyTop] | MethodAttributes.FamORAssem; }
  break;
case 415:
#line 2009 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = (MethodAttributes) yyVals[-1+yyTop] | MethodAttributes.Final; }
  break;
case 416:
#line 2010 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = (MethodAttributes) yyVals[-1+yyTop] | MethodAttributes.Virtual; }
  break;
case 417:
#line 2011 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = (MethodAttributes) yyVals[-1+yyTop] | MethodAttributes.Abstract; }
  break;
case 418:
#line 2012 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = (MethodAttributes) yyVals[-1+yyTop] | MethodAttributes.HideBySig; }
  break;
case 419:
#line 2013 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = (MethodAttributes) yyVals[-1+yyTop] | MethodAttributes.NewSlot; }
  break;
case 420:
#line 2014 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = (MethodAttributes) yyVals[-1+yyTop] | MethodAttributes.RequireSecObject; }
  break;
case 421:
#line 2015 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = (MethodAttributes) yyVals[-1+yyTop] | MethodAttributes.SpecialName; }
  break;
case 422:
#line 2016 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = (MethodAttributes) yyVals[-1+yyTop] | MethodAttributes.RTSpecialName; }
  break;
case 423:
#line 2017 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = (MethodAttributes) yyVals[-1+yyTop] | MethodAttributes.CheckAccessOnOverride; }
  break;
case 425:
#line 2019 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = (MethodAttributes) yyVals[-1+yyTop] | MethodAttributes.UnmanagedExport; }
  break;
case 426:
#line 2023 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				yyVal = (MethodAttributes) yyVals[-4+yyTop] | (MethodAttributes) (int) yyVals[-1+yyTop];
			}
  break;
case 427:
  case_427();
  break;
case 428:
  case_428();
  break;
case 429:
#line 2036 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
			}
  break;
case 430:
#line 2042 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				yyVal = PInvokeAttributes.CharSetNotSpec;
			}
  break;
case 431:
#line 2043 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = (PInvokeAttributes) yyVals[-1+yyTop] | PInvokeAttributes.NoMangle; }
  break;
case 432:
#line 2044 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = (PInvokeAttributes) yyVals[-1+yyTop] | PInvokeAttributes.CharSetAnsi; }
  break;
case 433:
#line 2045 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = (PInvokeAttributes) yyVals[-1+yyTop] | PInvokeAttributes.CharSetUnicode; }
  break;
case 434:
#line 2046 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = (PInvokeAttributes) yyVals[-1+yyTop] | PInvokeAttributes.CharSetAuto; }
  break;
case 435:
#line 2047 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = (PInvokeAttributes) yyVals[-1+yyTop] | PInvokeAttributes.SupportsLastError; }
  break;
case 436:
#line 2048 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = (PInvokeAttributes) yyVals[-1+yyTop] | PInvokeAttributes.CallConvWinapi; }
  break;
case 437:
#line 2049 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = (PInvokeAttributes) yyVals[-1+yyTop] | PInvokeAttributes.CallConvWinapi; }
  break;
case 438:
#line 2050 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = (PInvokeAttributes) yyVals[-1+yyTop] | PInvokeAttributes.CallConvCdecl; }
  break;
case 439:
#line 2051 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = (PInvokeAttributes) yyVals[-1+yyTop] | PInvokeAttributes.CallConvStdCall; }
  break;
case 440:
#line 2052 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = (PInvokeAttributes) yyVals[-1+yyTop] | PInvokeAttributes.CallConvThiscall; }
  break;
case 441:
#line 2053 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = (PInvokeAttributes) yyVals[-1+yyTop] | PInvokeAttributes.CallConvFastcall; }
  break;
case 442:
#line 2054 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = (PInvokeAttributes) yyVals[-3+yyTop] | PInvokeAttributes.BestFitEnabled; }
  break;
case 443:
#line 2055 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = (PInvokeAttributes) yyVals[-3+yyTop] | PInvokeAttributes.BestFitDisabled; }
  break;
case 444:
#line 2056 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = (PInvokeAttributes) yyVals[-3+yyTop] | PInvokeAttributes.ThrowOnUnmappableCharEnabled; }
  break;
case 445:
#line 2057 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = (PInvokeAttributes) yyVals[-3+yyTop] | PInvokeAttributes.ThrowOnUnmappableCharDisabled; }
  break;
case 446:
#line 2061 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				yyVal = (PInvokeAttributes) yyVals[-4+yyTop] | (PInvokeAttributes) (int) yyVals[-1+yyTop];
			}
  break;
case 447:
#line 2067 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				yyVal = MethodImplAttributes.Managed;
			}
  break;
case 448:
#line 2070 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
			}
  break;
case 449:
#line 2071 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = (MethodImplAttributes) yyVals[-1+yyTop] | MethodImplAttributes.IL; }
  break;
case 450:
#line 2072 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = (MethodImplAttributes) yyVals[-1+yyTop] | MethodImplAttributes.IL; }
  break;
case 453:
#line 2077 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
			}
  break;
case 454:
#line 2078 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = (MethodImplAttributes) yyVals[-1+yyTop] | MethodImplAttributes.ForwardRef; }
  break;
case 455:
#line 2079 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = (MethodImplAttributes) yyVals[-1+yyTop] | MethodImplAttributes.PreserveSig; }
  break;
case 456:
#line 2080 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = (MethodImplAttributes) yyVals[-1+yyTop] | MethodImplAttributes.Runtime; }
  break;
case 457:
#line 2081 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = (MethodImplAttributes) yyVals[-1+yyTop] | MethodImplAttributes.InternalCall; }
  break;
case 458:
#line 2082 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = (MethodImplAttributes) yyVals[-1+yyTop] | MethodImplAttributes.Synchronized; }
  break;
case 459:
#line 2083 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = (MethodImplAttributes) yyVals[-1+yyTop] | MethodImplAttributes.NoInlining; }
  break;
case 460:
#line 2084 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = (MethodImplAttributes) yyVals[-1+yyTop] | MethodImplAttributes.NoOptimization; }
  break;
case 461:
#line 2090 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				yyVal = ParameterAttributes.None;
			}
  break;
case 462:
#line 2091 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = (ParameterAttributes) yyVals[-3+yyTop] | ParameterAttributes.In; }
  break;
case 463:
#line 2092 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = (ParameterAttributes) yyVals[-3+yyTop] | ParameterAttributes.Out; }
  break;
case 464:
#line 2093 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = (ParameterAttributes) yyVals[-3+yyTop] | ParameterAttributes.Optional; }
  break;
case 465:
#line 2094 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = (ParameterAttributes) yyVals[-3+yyTop] | (ParameterAttributes) (int) yyVals[-1+yyTop]; }
  break;
case 466:
#line 2100 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				yyVal = new List<ParameterDefinition> ();
			}
  break;
case 468:
  case_468();
  break;
case 469:
#line 2113 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				((List<ParameterDefinition>) yyVals[-2+yyTop]).Add ((ParameterDefinition) yyVals[0+yyTop]);
			}
  break;
case 471:
#line 2120 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				yyVal = (MarshalInfo) yyVals[-1+yyTop] ;
			}
  break;
case 472:
#line 2124 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				yyVal = new MarshalInfo ((NativeType) yyVals[-1+yyTop]);
			}
  break;
case 473:
#line 2130 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				/* TODO: Construct the sentinel type somehow...*/
			}
  break;
case 474:
  case_474();
  break;
case 475:
  case_475();
  break;
case 478:
#line 2154 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				/* TODO: Cecil doesn't support this.*/
			}
  break;
case 479:
#line 2158 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				il.Body.MaxStackSize = (int) yyVals[0+yyTop];
			}
  break;
case 480:
  case_480();
  break;
case 481:
  case_481();
  break;
case 482:
  case_482();
  break;
case 483:
#line 2188 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				il.Body.InitLocals = true;
			}
  break;
case 484:
#line 2192 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				/* TODO: Implement exports (Cecil doesn't support these yet).*/
			}
  break;
case 485:
#line 2195 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
			}
  break;
case 486:
#line 2198 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
			}
  break;
case 487:
  case_487();
  break;
case 488:
#line 2209 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				codegen.CurrentMethod.Overrides.Add ((MethodReference) yyVals[0+yyTop]);
			}
  break;
case 490:
  case_490();
  break;
case 492:
  case_492();
  break;
case 494:
  case_494();
  break;
case 495:
  case_495();
  break;
case 498:
  case_498();
  break;
case 501:
  case_501();
  break;
case 502:
  case_502();
  break;
case 503:
  case_503();
  break;
case 504:
  case_504();
  break;
case 505:
  case_505();
  break;
case 506:
  case_506();
  break;
case 507:
  case_507();
  break;
case 508:
#line 2353 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				((List<ExceptionHandler>) yyVals[-1+yyTop]).Add ((ExceptionHandler) yyVals[0+yyTop]);
			}
  break;
case 509:
  case_509();
  break;
case 510:
  case_510();
  break;
case 511:
  case_511();
  break;
case 512:
  case_512();
  break;
case 513:
#line 2399 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				yyVal = ((Scope) yyVals[0+yyTop]).Start;
			}
  break;
case 514:
#line 2403 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				yyVal = FindInstruction ((string) yyVals[0+yyTop]);
			}
  break;
case 515:
#line 2407 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				yyVal = FindInstruction ((int) yyVals[0+yyTop]);
			}
  break;
case 516:
  case_516();
  break;
case 517:
  case_517();
  break;
case 518:
  case_518();
  break;
case 521:
  case_521();
  break;
case 522:
  case_522();
  break;
case 523:
  case_523();
  break;
case 524:
  case_524();
  break;
case 525:
  case_525();
  break;
case 526:
  case_526();
  break;
case 527:
  case_527();
  break;
case 528:
  case_528();
  break;
case 529:
  case_529();
  break;
case 530:
  case_530();
  break;
case 531:
  case_531();
  break;
case 532:
  case_532();
  break;
case 533:
  case_533();
  break;
case 534:
  case_534();
  break;
case 535:
  case_535();
  break;
case 536:
  case_536();
  break;
case 537:
  case_537();
  break;
case 538:
  case_538();
  break;
case 539:
  case_539();
  break;
case 540:
  case_540();
  break;
case 541:
#line 2701 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				/* Not actually used.*/
			}
  break;
case 544:
#line 2711 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				yyVal = new List<object> ();
			}
  break;
case 545:
  case_545();
  break;
case 546:
  case_546();
  break;
case 547:
#line 2729 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				yyVal = yyVals[0+yyTop];
			}
  break;
case 548:
#line 2733 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				yyVal = yyVals[0+yyTop];
			}
  break;
case 549:
  case_549();
  break;
case 550:
#line 2757 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				yyVal = codegen.GetFieldByMetadataToken ((int) yyVals[0+yyTop]);
			}
  break;
case 551:
  case_551();
  break;
case 553:
  case_553();
  break;
case 554:
  case_554();
  break;
case 555:
#line 2829 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				yyVal = codegen.GetMethodByMetadataToken ((int) yyVals[0+yyTop]);
			}
  break;
case 559:
  case_559();
  break;
case 561:
#line 2867 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				yyVal = yyVals[-2+yyTop];
			}
  break;
case 562:
#line 2873 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				yyVal = EvaluateType (yyVals[0+yyTop]);
			}
  break;
case 564:
  case_564();
  break;
case 565:
  case_565();
  break;
case 567:
#line 2895 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				yyVal = EventAttributes.None;
			}
  break;
case 568:
#line 2896 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = (EventAttributes) yyVals[-1+yyTop] & EventAttributes.RTSpecialName; }
  break;
case 569:
#line 2897 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = (EventAttributes) yyVals[-1+yyTop] & EventAttributes.SpecialName; }
  break;
case 572:
  case_572();
  break;
case 573:
  case_573();
  break;
case 574:
  case_574();
  break;
case 575:
  case_575();
  break;
case 576:
  case_576();
  break;
case 580:
  case_580();
  break;
case 581:
  case_581();
  break;
case 582:
#line 2977 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				yyVal = PropertyAttributes.None;
			}
  break;
case 583:
#line 2978 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = (PropertyAttributes) yyVals[-1+yyTop] | PropertyAttributes.RTSpecialName; }
  break;
case 584:
#line 2979 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = (PropertyAttributes) yyVals[-1+yyTop] | PropertyAttributes.SpecialName; }
  break;
case 587:
  case_587();
  break;
case 588:
  case_588();
  break;
case 589:
  case_589();
  break;
case 590:
  case_590();
  break;
case 595:
  case_595();
  break;
case 596:
#line 3032 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				yyVal = codegen.CurrentCustomAttribute = new CustomAttribute ((MethodReference) yyVals[0+yyTop]);
			}
  break;
case 597:
  case_597();
  break;
case 598:
#line 3042 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				yyVal = codegen.CurrentCustomAttribute = new CustomAttribute ((MethodReference) yyVals[-2+yyTop], (byte[]) yyVals[0+yyTop]);
			}
  break;
case 599:
#line 3046 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				yyVal = codegen.CurrentCustomAttribute = new CustomAttribute ((MethodReference) yyVals[-2+yyTop]);
			}
  break;
case 601:
  case_601();
  break;
case 602:
  case_602();
  break;
case 603:
  case_603();
  break;
case 604:
  case_604();
  break;
case 606:
  case_606();
  break;
case 608:
#line 3095 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				codegen.CurrentCustomAttribute.ConstructorArguments.Add ((CustomAttributeArgument) yyVals[0+yyTop]);
			}
  break;
case 610:
#line 3102 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				yyVal = new List<Tuple<TokenType, CustomAttributeNamedArgument>> ();
			}
  break;
case 611:
  case_611();
  break;
case 613:
#line 3117 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				yyVal = EvaluateType (yyVals[0+yyTop]);
			}
  break;
case 614:
#line 3121 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				yyVal = codegen.Corlib.Type;
			}
  break;
case 615:
#line 3125 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				yyVal = codegen.Corlib.Object;
			}
  break;
case 616:
#line 3129 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				yyVal = codegen.CurrentModule.GetType ((string) yyVals[0+yyTop], true);
			}
  break;
case 617:
#line 3133 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				yyVal = EvaluateType (yyVals[0+yyTop]);
			}
  break;
case 618:
#line 3137 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				yyVal = new ArrayType (EvaluateType (yyVals[-2+yyTop]));
			}
  break;
case 619:
  case_619();
  break;
case 620:
#line 3148 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				yyVal = new CustomAttributeArgument (codegen.Corlib.Double, yyVals[-1+yyTop]);
			}
  break;
case 621:
  case_621();
  break;
case 622:
  case_622();
  break;
case 623:
#line 3163 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				yyVal = new CustomAttributeArgument (codegen.Corlib.Int64, yyVals[-1+yyTop]);
			}
  break;
case 624:
  case_624();
  break;
case 625:
  case_625();
  break;
case 626:
  case_626();
  break;
case 627:
  case_627();
  break;
case 628:
  case_628();
  break;
case 629:
  case_629();
  break;
case 630:
  case_630();
  break;
case 631:
  case_631();
  break;
case 632:
#line 3207 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				yyVal = new CustomAttributeArgument (codegen.Corlib.Boolean, yyVals[-1+yyTop]);
			}
  break;
case 633:
  case_633();
  break;
case 634:
#line 3222 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				yyVal = new CustomAttributeArgument (codegen.Corlib.String, null);
			}
  break;
case 635:
#line 3226 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				yyVal = new CustomAttributeArgument (codegen.Corlib.String, yyVals[-1+yyTop]);
			}
  break;
case 636:
  case_636();
  break;
case 637:
#line 3235 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				yyVal = new CustomAttributeArgument (codegen.Corlib.Type, yyVals[-1+yyTop]);
			}
  break;
case 638:
#line 3239 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				yyVal = new CustomAttributeArgument (codegen.Corlib.Type, null);
			}
  break;
case 639:
#line 3243 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				yyVal = new CustomAttributeArgument (codegen.Corlib.Object, yyVals[-1+yyTop]);
			}
  break;
case 640:
  case_640();
  break;
case 641:
  case_641();
  break;
case 642:
#line 3261 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				custom_attr_unsigned = false;
			}
  break;
case 643:
  case_643();
  break;
case 644:
#line 3272 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				custom_attr_unsigned = false;
			}
  break;
case 645:
  case_645();
  break;
case 646:
#line 3283 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				custom_attr_unsigned = false;
			}
  break;
case 647:
  case_647();
  break;
case 648:
#line 3294 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				custom_attr_unsigned = false;
			}
  break;
case 649:
  case_649();
  break;
case 650:
#line 3305 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				custom_attr_unsigned = true;
			}
  break;
case 651:
  case_651();
  break;
case 652:
#line 3316 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				custom_attr_unsigned = true;
			}
  break;
case 653:
  case_653();
  break;
case 654:
#line 3327 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				custom_attr_unsigned = true;
			}
  break;
case 655:
  case_655();
  break;
case 656:
#line 3338 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				custom_attr_unsigned = true;
			}
  break;
case 657:
  case_657();
  break;
case 658:
  case_658();
  break;
case 659:
  case_659();
  break;
case 660:
  case_660();
  break;
case 661:
  case_661();
  break;
case 662:
  case_662();
  break;
case 663:
#line 3386 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				yyVal = new CustomAttributeArgument [0];
			}
  break;
case 664:
  case_664();
  break;
case 665:
  case_665();
  break;
case 666:
#line 3413 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				yyVal = new CustomAttributeArgument [0];
			}
  break;
case 667:
  case_667();
  break;
case 668:
  case_668();
  break;
case 669:
#line 3439 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				yyVal = new CustomAttributeArgument [0];
			}
  break;
case 670:
  case_670();
  break;
case 671:
#line 3457 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				yyVal = new CustomAttributeArgument [0];
			}
  break;
case 672:
  case_672();
  break;
case 673:
#line 3475 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				yyVal = new CustomAttributeArgument [0];
			}
  break;
case 674:
  case_674();
  break;
case 675:
#line 3492 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				yyVal = new CustomAttributeArgument [0];
			}
  break;
case 676:
  case_676();
  break;
case 677:
#line 3509 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				yyVal = new CustomAttributeArgument [0];
			}
  break;
case 678:
  case_678();
  break;
case 679:
#line 3525 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				yyVal = new CustomAttributeArgument [0];
			}
  break;
case 680:
  case_680();
  break;
case 681:
  case_681();
  break;
case 682:
#line 3551 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				yyVal = new CustomAttributeArgument [0];
			}
  break;
case 683:
  case_683();
  break;
case 684:
  case_684();
  break;
case 685:
  case_685();
  break;
case 686:
#line 3587 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				yyVal = new CustomAttributeArgument [0];
			}
  break;
case 687:
  case_687();
  break;
case 688:
#line 3603 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				yyVal = yyVals[0+yyTop];
			}
  break;
case 689:
#line 3607 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				yyVal = yyVals[0+yyTop];
			}
  break;
case 690:
  case_690();
  break;
case 691:
  case_691();
  break;
case 692:
  case_692();
  break;
case 693:
  case_693();
  break;
case 694:
  case_694();
  break;
case 695:
  case_695();
  break;
case 696:
#line 3669 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = SecurityAction.Request; }
  break;
case 697:
#line 3670 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = SecurityAction.Demand; }
  break;
case 698:
#line 3671 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = SecurityAction.Assert; }
  break;
case 699:
#line 3672 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = SecurityAction.Deny; }
  break;
case 700:
#line 3673 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = SecurityAction.PermitOnly; }
  break;
case 701:
#line 3674 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = SecurityAction.LinkDemand; }
  break;
case 702:
#line 3675 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = SecurityAction.InheritDemand; }
  break;
case 703:
#line 3676 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = SecurityAction.RequestMinimum; }
  break;
case 704:
#line 3677 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = SecurityAction.RequestOptional; }
  break;
case 705:
#line 3678 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = SecurityAction.RequestRefuse; }
  break;
case 706:
#line 3679 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = SecurityAction.PreJitGrant; }
  break;
case 707:
#line 3680 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = SecurityAction.PreJitDeny; }
  break;
case 708:
#line 3681 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = SecurityAction.NonCasDemand; }
  break;
case 709:
#line 3682 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = SecurityAction.NonCasLinkDemand; }
  break;
case 710:
#line 3683 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = SecurityAction.NonCasInheritance; }
  break;
case 711:
#line 3689 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				yyVal = new List<SecurityAttribute> ();
			}
  break;
case 712:
#line 3693 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				((List<SecurityAttribute>) yyVals[-2+yyTop]).Add ((SecurityAttribute) yyVals[0+yyTop]);
			}
  break;
case 713:
  case_713();
  break;
case 714:
  case_714();
  break;
case 715:
#line 3727 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				yyVal = new List<Tuple<string, object>> ();
			}
  break;
case 716:
#line 3731 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				((List<Tuple<string, object>>) yyVals[-2+yyTop]).Add ((Tuple<string, object>) yyVals[0+yyTop]);
			}
  break;
case 717:
#line 3737 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				yyVal = new Tuple<string, object> ((string) yyVals[-2+yyTop], yyVals[0+yyTop]);
			}
  break;
case 720:
#line 3745 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				yyVal = yyVals[-1+yyTop];
			}
  break;
case 722:
#line 3750 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				/* TODO: Figure out how these work.*/
			}
  break;
case 727:
#line 3762 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				yyVal = TokenType.Property;
			}
  break;
case 728:
#line 3766 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				yyVal = TokenType.Field;
			}
  break;
case 730:
  case_730();
  break;
case 731:
  case_731();
  break;
case 734:
#line 3798 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				yyVal = true;
			}
  break;
case 735:
#line 3802 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				yyVal = false;
			}
  break;
case 736:
#line 3808 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				yyVal = false;
			}
  break;
case 737:
#line 3812 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				yyVal = true;
			}
  break;
case 739:
  case_739();
  break;
case 740:
#line 3832 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				yyVal = AssemblyAttributes.SideBySideCompatible;
			}
  break;
case 741:
#line 3836 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				yyVal = ((AssemblyAttributes) yyVals[-1+yyTop]) | AssemblyAttributes.Retargetable;
			}
  break;
case 754:
#line 3860 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				codegen.CurrentModule.Assembly.Name.PublicKey = (byte[]) yyVals[0+yyTop];
			}
  break;
case 755:
  case_755();
  break;
case 756:
#line 3869 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				codegen.CurrentModule.Assembly.Name.Culture = (string) yyVals[0+yyTop];
			}
  break;
case 757:
  case_757();
  break;
case 758:
  case_758();
  break;
case 759:
  case_759();
  break;
case 760:
  case_760();
  break;
case 762:
  case_762();
  break;
case 763:
  case_763();
  break;
case 764:
  case_764();
  break;
case 767:
  case_767();
  break;
case 768:
  case_768();
  break;
case 769:
  case_769();
  break;
case 770:
#line 3968 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				codegen.CurrentAssemblyReference.Culture = (string) yyVals[0+yyTop];
			}
  break;
case 771:
  case_771();
  break;
case 772:
#line 3978 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				codegen.CurrentAssemblyReference.Hash = (byte[]) yyVals[0+yyTop];
			}
  break;
case 773:
#line 3981 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
			}
  break;
case 779:
  case_779();
  break;
case 780:
#line 4005 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				yyVal = TypeAttributes.NotPublic;
			}
  break;
case 781:
#line 4006 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = (TypeAttributes) yyVals[-1+yyTop] | TypeAttributes.NotPublic; }
  break;
case 782:
#line 4007 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = (TypeAttributes) yyVals[-1+yyTop] | TypeAttributes.Public; }
  break;
case 783:
#line 4008 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = (TypeAttributes) yyVals[-1+yyTop] | TypeAttributes.Forwarder; }
  break;
case 784:
#line 4009 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = (TypeAttributes) yyVals[-2+yyTop] | TypeAttributes.NestedPublic; }
  break;
case 785:
#line 4010 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = (TypeAttributes) yyVals[-2+yyTop] | TypeAttributes.NestedPrivate; }
  break;
case 786:
#line 4011 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = (TypeAttributes) yyVals[-2+yyTop] | TypeAttributes.NestedFamily; }
  break;
case 787:
#line 4012 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = (TypeAttributes) yyVals[-2+yyTop] | TypeAttributes.NestedAssembly; }
  break;
case 788:
#line 4013 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = (TypeAttributes) yyVals[-2+yyTop] | TypeAttributes.NestedFamANDAssem; }
  break;
case 789:
#line 4014 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  { yyVal = (TypeAttributes) yyVals[-2+yyTop] | TypeAttributes.NestedFamORAssem; }
  break;
case 791:
  case_791();
  break;
case 793:
#line 4029 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				yyVal = codegen.GetTypeByName ((QualifiedName) yyVals[0+yyTop]);
			}
  break;
case 794:
#line 4033 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				yyVal = codegen.GetAssemblyReference ((string) yyVals[0+yyTop]);
			}
  break;
case 796:
#line 4038 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				yyVal = yyVals[0+yyTop];
			}
  break;
case 799:
  case_799();
  break;
case 800:
  case_800();
  break;
case 801:
  case_801();
  break;
case 802:
#line 4126 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				yyVal = (ManifestResourceAttributes) 0;
			}
  break;
case 803:
  case_803();
  break;
case 804:
  case_804();
  break;
case 805:
#line 4143 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				yyVal = new List<object> ();
			}
  break;
case 806:
  case_806();
  break;
case 807:
#line 4153 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				((List<object>) yyVals[-1+yyTop]).Add (yyVals[0+yyTop]);
			}
  break;
case 808:
  case_808();
  break;
case 809:
#line 4164 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				yyVal = (string) yyVals[0+yyTop];
			}
  break;
case 813:
  case_813();
  break;
case 814:
#line 4193 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				yyVal = yyVals[-1+yyTop];
			}
  break;
case 816:
#line 4200 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				yyVal = string.Format ("{0}{1}", yyVals[-2+yyTop], yyVals[0+yyTop]);
			}
  break;
case 817:
  case_817();
  break;
case 820:
#line 4217 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				yyVal = (double) BitConverter.ToSingle (BitConverter.GetBytes ((int) yyVals[-1+yyTop]), 0);
			}
  break;
case 821:
  case_821();
  break;
case 822:
#line 4226 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				yyVal = BitConverter.ToDouble (BitConverter.GetBytes ((long) yyVals[-1+yyTop]), 0);
			}
  break;
case 823:
#line 4230 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				yyVal = (double) BitConverter.ToSingle (BitConverter.GetBytes ((int) yyVals[-1+yyTop]), 0);
			}
  break;
case 825:
#line 4239 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				tokenizer.InByteArray  = true;
			}
  break;
case 826:
  case_826();
  break;
case 827:
#line 4251 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				yyVal = new byte [0];
			}
  break;
case 828:
#line 4255 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				yyVal = ((List<byte>) yyVals[0+yyTop]).ToArray ();
			}
  break;
case 829:
  case_829();
  break;
case 830:
#line 4267 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				((List<byte>) yyVals[-1+yyTop]).Add (Convert.ToByte (yyVals[0+yyTop]));
			}
  break;
case 831:
#line 4273 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				yyVal = true;
			}
  break;
case 832:
#line 4277 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				yyVal = false;
			}
  break;
case 835:
#line 4287 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
  {
				yyVal = ((QualifiedName) yyVals[0+yyTop]).FullName;
			}
  break;
case 836:
  case_836();
  break;
case 837:
  case_837();
  break;
case 838:
  case_838();
  break;
#line default
        }
        yyTop -= yyLen[yyN];
        yyState = yyStates[yyTop];
        int yyM = yyLhs[yyN];
        if (yyState == 0 && yyM == 0) {
          if (debug != null) debug.shift(0, yyFinal);
          yyState = yyFinal;
          if (yyToken < 0) {
            yyToken = yyLex.advance() ? yyLex.token() : 0;
            if (debug != null)
               debug.lex(yyState, yyToken,yyname(yyToken), yyLex.value());
          }
          if (yyToken == 0) {
            if (debug != null) debug.accept(yyVal);
            return yyVal;
          }
          goto continue_yyLoop;
        }
        if (((yyN = yyGindex[yyM]) != 0) && ((yyN += yyState) >= 0)
            && (yyN < yyTable.Length) && (yyCheck[yyN] == yyState))
          yyState = yyTable[yyN];
        else
          yyState = yyDgoto[yyM];
        if (debug != null) debug.shift(yyStates[yyTop], yyState);
	 goto continue_yyLoop;
      continue_yyDiscarded: ;	// implements the named-loop continue: 'continue yyDiscarded'
      }
    continue_yyLoop: ;		// implements the named-loop continue: 'continue yyLoop'
    }
  }

/*
 All more than 3 lines long rules are wrapped into a method
*/
void case_17()
#line 463 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				codegen.CurrentModule.GetModuleType ().SecurityDeclarations.Add (codegen.CurrentSecurityDeclaration);
				codegen.CurrentSecurityDeclaration = null;
			}

void case_18()
#line 468 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				if (yyVals[0+yyTop] != null) {
					codegen.CurrentModule.CustomAttributes.Add ((CustomAttribute) yyVals[0+yyTop]);
					codegen.CurrentCustomAttribute = null;
				}
			}

void case_25()
#line 494 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				/* TODO: We need to set the native image base to 0x00510000 here.*/
				codegen.IsCorlib = true;
			}

void case_71()
#line 576 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				namespace_stack.Push (codegen.CurrentNamespace);

				if (codegen.CurrentNamespace != string.Empty)
					codegen.CurrentNamespace += ".";

				codegen.CurrentNamespace += (string) yyVals[0+yyTop];
			}

void case_72()
#line 587 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				/* We've assembled the System.Object type; register it.*/
				if (codegen.IsCorlib && codegen.Corlib.Object == null &&
					codegen.CurrentType.FullName == "System.Object")
					codegen.Corlib.Object = codegen.CurrentType;

				codegen.CurrentType = null;
				codegen.GenericContext.CurrentTypeProvider = null;
				codegen.CurrentCustomAttributeProvider = null;

				if (typedef_stack.Count > 0) {
					codegen.CurrentType = typedef_stack.Pop ();
					codegen.GenericContext.CurrentTypeProvider = codegen.CurrentType;
				}
			}

void case_73()
#line 605 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				var qn = (QualifiedName) yyVals[0+yyTop];
				var attr = (TypeAttributes) yyVals[-1+yyTop];
				var nester = codegen.CurrentType;

				if (nester != null) {
					typedef_stack.Push (nester);

					if (!attr.HasAnyBitFlag (TypeAttributes.NestedPublic) &&
						!attr.HasAnyBitFlag (TypeAttributes.NestedPrivate) &&
						!attr.HasAnyBitFlag (TypeAttributes.NestedFamily) &&
						!attr.HasAnyBitFlag (TypeAttributes.NestedAssembly) &&
						!attr.HasAnyBitFlag (TypeAttributes.NestedFamANDAssem) &&
						!attr.HasAnyBitFlag (TypeAttributes.NestedFamORAssem)) {

						if (attr.HasBitFlag (TypeAttributes.Public)) {
							attr &= ~TypeAttributes.Public;
							attr |= TypeAttributes.NestedPublic;
						} else /* In the case of NotPublic.*/
							attr |= TypeAttributes.NestedPrivate;
					}
				}

				codegen.CurrentType = new TypeDefinition (codegen.CurrentNamespace +
					qn.FullNamespace, qn.Name, attr) {
					DeclaringType = nester,
				};

				if (nester != null)
					nester.NestedTypes.Add (codegen.CurrentType);

				codegen.GenericContext.CurrentTypeProvider = codegen.CurrentType;
			}

void case_74()
#line 639 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				foreach (var arg in (List<GenericParameter>) yyVals[0+yyTop])
					codegen.CurrentType.GenericParameters.Add (arg);
			}

void case_75()
#line 644 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				TypeReference baseType;

				/* if we're assembling mscorlib and defining System.Object,*/
				/* no base type should be defined.*/
				var isSysObj = codegen.IsCorlib && codegen.Corlib.Object == null &&
					codegen.CurrentType.FullName == "System.Object";

				if (yyVals[-1+yyTop] == null) {
					if (is_value_class)
						baseType = codegen.Corlib.ValueType;
					else if (is_enum_class)
						baseType = codegen.Corlib.Enum;
					else if (isSysObj || codegen.NoAutoInherit)
						baseType = null;
					else {
						var obj = codegen.Corlib.Object;

						baseType = obj;
					}
				} else {
					baseType = EvaluateType (yyVals[-1+yyTop]);
				}

				if (isSysObj && baseType != null) {
					codegen.CurrentType.BaseType = null;
				} else if (!codegen.CurrentType.IsInterface)
					codegen.CurrentType.BaseType = baseType;

				foreach (var interf in (List<TypeCreator>) yyVals[0+yyTop])
					codegen.CurrentType.Interfaces.Add (interf ());

				if (!codegen.CurrentType.IsNested)
					codegen.CurrentModule.Types.Add (codegen.CurrentType);
			}

void case_76()
#line 682 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				is_value_class = false;
				is_enum_class = false;
				type_attr_visibility_set = false;
				yyVal = TypeAttributes.NotPublic;
			}

void case_77()
#line 689 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				if (!type_attr_visibility_set) {
					type_attr_visibility_set = true;
					yyVal = (TypeAttributes) yyVals[-1+yyTop] | TypeAttributes.Public;
				}
			}

void case_107()
#line 741 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				yyVal = new List<TypeCreator> () {
					(TypeCreator) yyVals[0+yyTop],
				};
			}

void case_111()
#line 763 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				yyVal = new List<GenericParameter> () {
					(GenericParameter) yyVals[0+yyTop],
				};
			}

void case_113()
#line 775 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				var gp = new GenericParameter ((string) yyVals[0+yyTop],
					generic_param_in_method ? codegen.GenericContext.CurrentMethodProvider :
						codegen.GenericContext.CurrentTypeProvider) {
					Attributes = (GenericParameterAttributes) yyVals[-2+yyTop],
				};

				foreach (var constraint in (List<TypeCreator>) yyVals[-1+yyTop])
					gp.Constraints.Add (constraint ());

				yyVal = gp;
			}

void case_123()
#line 813 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				yyVal = new List<TypeCreator> () {
					(TypeCreator) yyVals[0+yyTop],
				};
			}

void case_133()
#line 835 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				codegen.CurrentType.SecurityDeclarations.Add (codegen.CurrentSecurityDeclaration);
				codegen.CurrentSecurityDeclaration = null;
			}

void case_135()
#line 841 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				if (yyVals[0+yyTop] != null) {
					var provider = codegen.CurrentCustomAttributeProvider != null ?
						codegen.CurrentCustomAttributeProvider : codegen.CurrentType;

					provider.CustomAttributes.Add ((CustomAttribute) yyVals[0+yyTop]);
					codegen.CurrentCustomAttribute = null;
				}
			}

void case_137()
#line 852 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				var size = (int) yyVals[0+yyTop];

				codegen.CurrentType.ClassSize = size;
			}

void case_138()
#line 858 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				var p = (int) yyVals[0+yyTop];

				codegen.CurrentType.PackingSize = (short) (int) yyVals[0+yyTop];
			}

void case_139()
#line 864 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				var conv = (MethodCallingConvention) yyVals[-7+yyTop];
				var retType = EvaluateType (yyVals[-6+yyTop]);

				var overridingMethod = new MethodReference ((string) yyVals[-3+yyTop], retType, EvaluateType (yyVals[-5+yyTop])) {
					CallingConvention = conv,
				};

				var baseMethod = new MethodReference ((string) yyVals[-9+yyTop], retType, EvaluateType (yyVals[-11+yyTop])) {
					CallingConvention = conv,
				};

				if (is_instance_call) {
					overridingMethod.HasThis = true;
					baseMethod.HasThis = true;
				}

				if (is_explicit_call) {
					overridingMethod.ExplicitThis = true;
					baseMethod.ExplicitThis = true;
				}

				is_instance_call = false;
				is_explicit_call = false;

				foreach (var arg in (List<ParameterDefinition>) yyVals[-1+yyTop]) {
					overridingMethod.Parameters.Add (arg);
					baseMethod.Parameters.Add (arg);
				}

				var tup = new Tuple<MethodReference, MethodReference> (overridingMethod, baseMethod);
				codegen.ExplicitOverrides.Add (tup);
			}

void case_140()
#line 898 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				var tup = new Tuple<MethodReference, MethodReference> ((MethodReference) yyVals[0+yyTop], (MethodReference) yyVals[-2+yyTop]);
				codegen.ExplicitOverrides.Add (tup);
			}

void case_143()
#line 907 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				var name = (string) yyVals[0+yyTop];
				GenericParameter param = null;

				var provider = generic_param_in_method ? codegen.GenericContext.CurrentMethodProvider :
					codegen.GenericContext.CurrentTypeProvider;

				foreach (var parameter in provider.GenericParameters)
					if (parameter.Name == name)
						param = parameter;

				codegen.CurrentCustomAttributeProvider = param;
			}

void case_144()
#line 921 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				var id = (int) yyVals[-1+yyTop]; /* One-based.*/
				var provider = generic_param_in_method ? codegen.GenericContext.CurrentMethodProvider :
					codegen.GenericContext.CurrentTypeProvider;
				var genParams = provider.GenericParameters;

				codegen.CurrentCustomAttributeProvider = genParams [id - 1];
			}

void case_145()
#line 932 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				yyVal = new List<TypeCreator> {
					(TypeCreator) yyVals[0+yyTop],
				};
			}

void case_147()
#line 944 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				var name = (QualifiedName) yyVals[-2+yyTop];
				var type = (QualifiedName) yyVals[0+yyTop];
				var scope = codegen.GetScope (name.FullName, false);

				yyVal = DeferType (() => MakeTypeReference (type, scope));
			}

void case_148()
#line 952 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				/* TODO: We need to look up a module or assembly reference*/
				/* by the given metadata token and construct a type.*/
			}

void case_150()
#line 961 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				var module = (string) yyVals[-2+yyTop];
				var type = (QualifiedName) yyVals[0+yyTop];

				if (module == codegen.CurrentModule.Name) {
					var typeRef = MakeTypeReference (type, codegen.CurrentModule);

					codegen.ModuleTypeReferences.Add (typeRef);

					yyVal = DeferType (() => typeRef);
				} else {
					var scope = codegen.GetScope (module, true);

					yyVal = DeferType (() => MakeTypeReference (type, scope));
				}
			}

void case_151()
#line 978 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				var qn = (QualifiedName) yyVals[0+yyTop];
				var typeRef = MakeTypeReference (qn, codegen.CurrentModule);

				codegen.ModuleTypeReferences.Add (typeRef);

				yyVal = DeferType (() => typeRef);
			}

void case_154()
#line 992 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				var baseType = codegen.CurrentType.BaseType;

				yyVal = DeferType (() => baseType);
			}

void case_155()
#line 998 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				var nester = codegen.CurrentType.DeclaringType;

				yyVal = DeferType (() => nester);
			}

void case_156()
#line 1006 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				var type = codegen.GetTypeByMetadataToken ((int) yyVals[0+yyTop]);

				yyVal = DeferType (() => type);
			}

void case_160()
#line 1022 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				var type = yyVals[0+yyTop];

				yyVal = DeferType (() => {
					var t = EvaluateType (type);
					t.IsValueType = true;

					return t;
				});
			}

void case_161()
#line 1033 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				var elem = yyVals[-2+yyTop];

				yyVal = DeferType (() => new ArrayType (EvaluateType (elem)));
			}

void case_162()
#line 1039 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				var elem = yyVals[-3+yyTop];
				var bounds = (List<ArrayDimension>) yyVals[-1+yyTop];

				yyVal = DeferType (() =>
				{
					var type = new ArrayType (EvaluateType (elem), bounds.Count);

					for (var i = 0; i < bounds.Count; i++)
						type.Dimensions [i] = bounds [i];

					return type;
				});
			}

void case_163()
#line 1054 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				var elem = yyVals[-1+yyTop];

				yyVal = DeferType (() => new ByReferenceType (EvaluateType (elem)));
			}

void case_164()
#line 1060 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				var elem = yyVals[-1+yyTop];

				yyVal = DeferType (() => new PointerType (EvaluateType (elem)));
			}

void case_165()
#line 1066 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				var elem = yyVals[-1+yyTop];

				yyVal = DeferType (() => new PinnedType (EvaluateType (elem)));
			}

void case_166()
#line 1072 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				var type = yyVals[-4+yyTop];
				var mod = yyVals[-1+yyTop];

				yyVal = DeferType (() => new RequiredModifierType (EvaluateType (mod), EvaluateType (type)));
			}

void case_167()
#line 1079 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				var type = yyVals[-4+yyTop];
				var mod = yyVals[-1+yyTop];

				yyVal = DeferType (() => new OptionalModifierType (EvaluateType (mod), EvaluateType (type)));
			}

void case_168()
#line 1086 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				var callConv = yyVals[-5+yyTop];
				var retType = yyVals[-4+yyTop];
				var args = yyVals[-1+yyTop];

				yyVal = DeferType (() =>
				{
					var func = new FunctionPointerType {
						CallingConvention = (MethodCallingConvention) callConv,
						ReturnType = EvaluateType (retType),
					};

					if (is_instance_call)
						func.HasThis = true;

					if (is_explicit_call)
						func.ExplicitThis = true;

					is_instance_call = false;
					is_explicit_call = false;

					foreach (var arg in (List<ParameterDefinition>) args)
						func.Parameters.Add (arg);

					return func;
				});
			}

void case_169()
#line 1114 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				var type = yyVals[-1+yyTop];
				var typArgs = yyVals[0+yyTop];

				yyVal = DeferType (() =>
				{
					var innerType = EvaluateType (type);

					var gti = new GenericInstanceType (innerType);

					foreach (var ga in (List<TypeCreator>) typArgs)
						gti.GenericArguments.Add (ga ());

					return gti;
				});
			}

void case_170()
#line 1131 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				var i = (int) yyVals[0+yyTop];

				yyVal = DeferType (() => new GenericParameter (i, GenericParameterType.Type,
					codegen.CurrentModule));
			}

void case_171()
#line 1138 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				var id = (string) yyVals[0+yyTop];

				yyVal = DeferType (() =>
				{
					var gp = codegen.GenericContext.Resolve (id, GenericParameterType.Type);

					return gp;
				});
			}

void case_172()
#line 1149 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				var i = (int) yyVals[0+yyTop];

				yyVal = DeferType (() => new GenericParameter (i, GenericParameterType.Method,
					codegen.CurrentModule));
			}

void case_173()
#line 1156 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				var id = (string) yyVals[0+yyTop];

				yyVal = DeferType (() =>
				{
					var gp = codegen.GenericContext.Resolve (id, GenericParameterType.Method);

					return gp;
				});
			}

void case_174()
#line 1167 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				var obj = codegen.Corlib.Object;

				yyVal = DeferType (() => obj);
			}

void case_182()
#line 1180 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				var elem = yyVals[0+yyTop];

				yyVal = DeferType (() => new SentinelType (EvaluateType (elem)));
			}

void case_208()
#line 1231 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				yyVal = new List<TypeCreator> () {
					(TypeCreator) yyVals[0+yyTop],
				};
			}

void case_211()
#line 1244 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				/* TODO: This probably means that we should return a*/
				/* reference to the <Module> type in the given module.*/
			}

void case_212()
#line 1249 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				var module = (string) yyVals[-1+yyTop];

				if (module == codegen.CurrentModule.Name)
					yyVal = DeferType (() => codegen.CurrentModule.GetModuleType ());
				else {
					var mod = codegen.GetModuleReference (module);

					yyVal = DeferType (() => new TypeReference (string.Empty,
						"<Module>", codegen.CurrentModule, mod));
				}
			}

void case_214()
#line 1265 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				yyVal = new List<ArrayDimension> {
					(ArrayDimension) yyVals[0+yyTop],
				};
			}

void case_218()
#line 1285 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				var size = (int) yyVals[0+yyTop];

				if (size < 0) {
					yyVal = new ArrayDimension (null, 0);
				} else
					yyVal = new ArrayDimension (null, size);
			}

void case_219()
#line 1294 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				var lower = (int) yyVals[-2+yyTop];
				var upper = (int) yyVals[0+yyTop];

				yyVal = new ArrayDimension (lower, upper);
			}

void case_233()
#line 1329 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				var type = codegen.CurrentModule.GetType ((string) yyVals[-3+yyTop], true);

				yyVal = new CustomMarshalInfo {
					Cookie = (string) yyVals[-1+yyTop],
					ManagedType = type,
				};
			}

void case_234()
#line 1338 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				/* TODO: What do we do with the two last strings?*/
				var type = codegen.CurrentModule.GetType ((string) yyVals[-7+yyTop], true);

				yyVal = new CustomMarshalInfo {
					Cookie = (string) yyVals[-5+yyTop],
					ManagedType = type,
				};
			}

void case_235()
#line 1348 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				yyVal = new FixedSysStringMarshalInfo {
					Size = (int) yyVals[-1+yyTop],
				};
			}

void case_236()
#line 1354 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				yyVal = new FixedArrayMarshalInfo {
					Size = (int) yyVals[-1+yyTop],
				};
			}

void case_237()
#line 1360 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				yyVal = new FixedArrayMarshalInfo {
					Size = (int) yyVals[-2+yyTop],
					ElementType = (NativeType) yyVals[0+yyTop],
				};
			}

void case_238()
#line 1367 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				/* Emit an invalid native type. Don't ask me how this works;*/
				/* blame Microsoft's ILAsm.*/
				yyVal = new MarshalInfo ((NativeType) 0);
			}

void case_239()
#line 1373 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				/* FIXME: This is not Microsoft-compatible. I have no idea how*/
				/* they calculate the values they emit for jagged arrays.*/
				yyVal = new ArrayMarshalInfo {
					ElementType = (NativeType) yyVals[-2+yyTop],
				};
			}

void case_240()
#line 1381 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				yyVal = new ArrayMarshalInfo {
					ElementType = (NativeType) yyVals[-3+yyTop],
					Size = (int) yyVals[-1+yyTop],
				};
			}

void case_241()
#line 1388 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				is_parameter_marshal_notation = true;

				yyVal = new ArrayMarshalInfo {
					ElementType = (NativeType) yyVals[-5+yyTop],
					Size = (int) yyVals[-3+yyTop],
					SizeParameterIndex = (int) yyVals[-1+yyTop],
				};
			}

void case_242()
#line 1398 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				is_parameter_marshal_notation = true;

				yyVal = new ArrayMarshalInfo {
					ElementType = (NativeType) yyVals[-4+yyTop],
					SizeParameterIndex = (int) yyVals[-1+yyTop],
				};
			}

void case_243()
#line 1407 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				yyVal = new SafeArrayMarshalInfo {
					ElementType = (VariantType) yyVals[0+yyTop],
				};
			}

void case_244()
#line 1413 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				/* TODO: What do we do with the string?*/
				yyVal = new SafeArrayMarshalInfo {
					ElementType = (VariantType) yyVals[-2+yyTop],
				};
			}

void case_245()
#line 1420 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				yyVal = new SafeArrayMarshalInfo {
					ElementType = VariantType.None,
				};
			}

void case_246()
#line 1426 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				yyVal = new ArrayMarshalInfo() {
					Size = (int)yyVals[-1+yyTop],
				};
			}

void case_280()
#line 1472 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				/* The grammar supports these types, but we can't*/
				/* actually use them.*/
			}

void case_307()
#line 1507 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				/* The grammar supports these types, but we can't*/
				/* actually use them.*/
			}

void case_333()
#line 1541 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				var isGlobal = codegen.CurrentType == null;
				var attrs = (FieldAttributes) yyVals[-5+yyTop];
				var isStatic = attrs.HasBitFlag(FieldAttributes.Static);

				if (isGlobal && !isStatic)
				{
					attrs |= FieldAttributes.Static;
				}

				var field = new FieldDefinition ((string) yyVals[-2+yyTop], attrs, EvaluateType (yyVals[-3+yyTop]));

				if (yyVals[-6+yyTop] != null) {
					if (!isGlobal)
						field.Offset = (int) yyVals[-6+yyTop];
				}

				if (yyVals[-4+yyTop] != null) {
					if (is_parameter_marshal_notation) {
						/* TODO: This warning should be emitted more closely to the relevant code.*/

						is_parameter_marshal_notation = false;
					}

					field.MarshalInfo = (MarshalInfo) yyVals[-4+yyTop];
				}

				if (yyVals[-1+yyTop] != null) {
					var dataLoc = (string) yyVals[-1+yyTop];
					var mapping = codegen.GetFieldDataMapping (codegen.CurrentType ?? codegen.CurrentModule.GetModuleType ());
					mapping.Add (field, dataLoc);
				}

				if (yyVals[0+yyTop] != null)
					field.Constant = yyVals[0+yyTop] is Null ? null : yyVals[0+yyTop];

				if (isGlobal)
					codegen.CurrentModule.GetModuleType ().Fields.Add (field);
				else
					codegen.CurrentType.Fields.Add (field);
			}

void case_374()
#line 1702 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				if (yyVals[-1+yyTop] != null)
				{
					foreach (var item in (List<object>) yyVals[0+yyTop])
						codegen.DataConstants.Add ((string) yyVals[-1+yyTop], item);
				}
			}

void case_383()
#line 1746 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				yyVal = new List<object> {
					yyVals[0+yyTop],
				};
			}

void case_384()
#line 1754 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				yyVal = new List<object> {
					yyVals[0+yyTop],
				};
			}

void case_402()
#line 1833 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				/* If the method has no code, we emit a ret.*/
				if (il.Body.Instructions.Count == 0 &&
					!codegen.CurrentMethod.IsAbstract) {

					il.Emit (OpCodes.Ret);
				} else {
					/* We need to adjust the end instruction of exception*/
					/* handlers and their protected regions.*/
					foreach (var handler in il.Body.ExceptionHandlers) {
						if (handler.TryEnd != null)
							handler.TryEnd = handler.TryEnd.Next;

						if (handler.HandlerEnd != null)
							handler.HandlerEnd = handler.HandlerEnd.Next;
					}

					/* Go over all branching instructions and attempt to map*/
					/* them to a label. If we can't find a label, we error.*/
					/* This holds true for offsets too, which is unlike MS.NET's*/
					/* ILAsm where invalid offsets are allowed...*/
					foreach (var jump in label_jumps) {
						if (jump.Value is List<object>) {
							var jumpLabels = new List<Instruction> ();

							foreach (var obj in (List<object>) jump.Value) {
								if (obj is int) {
									var size = jump.Key.GetSize ();
									var addr = jump.Key.Offset + size + (int) obj;
									jumpLabels.Add (FindInstruction (addr));
								} else
									jumpLabels.Add (FindInstruction ((string) obj));
							}

							jump.Key.Operand = jumpLabels.ToArray ();
						} else {
							Instruction target;

							if (jump.Value is int) {
								var size = jump.Key.GetSize ();
								var addr = jump.Key.Offset + size + (int) jump.Value;
								target = FindInstruction (addr);
							} else
								target = FindInstruction ((string) jump.Value);

							jump.Key.Operand = target;
						}
					}
				}

				codegen.CurrentMethod = null;
				codegen.CurrentScope = null;
				codegen.CurrentCustomAttributeProvider = null;
				codegen.GenericContext.CurrentTypeProvider = codegen.CurrentType;
				generic_param_in_method = false;
				il = null;

				current_labels.Clear ();
				labels.Clear ();
				label_jumps.Clear ();
			}

void case_403()
#line 1897 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				var name = (string) yyVals[0+yyTop];
				var attr = (MethodAttributes) yyVals[-5+yyTop];
				var isClassMember = codegen.CurrentType != null;

				if (isClassMember) {

					if (codegen.CurrentType.IsInterface &&
						!attr.HasBitFlag (MethodAttributes.Abstract) &&
						!attr.HasBitFlag (MethodAttributes.Virtual) &&
						!attr.HasBitFlag (MethodAttributes.Static)) {

						attr |= MethodAttributes.Abstract | MethodAttributes.Virtual;
					}
				}

				/* Use a dummy return type. Since the return type may be*/
				/* a generic parameter, we delay resolving it until we have*/
				/* the full list of generic parameters.*/
				var method = new MethodDefinition (name, attr,
					new TypeReference (null, null, null, null)) {
					CallingConvention = (MethodCallingConvention) yyVals[-4+yyTop],
				};

				/* Even though this doesn't make much sense, we have to*/
				/* do it in order to be compatible.*/
				if (!method.HasBody)
					method.Body = new MethodBody (method);

				method.Body.Scope = new Scope ();

				if (is_instance_call)
					method.HasThis = true;

				if (is_explicit_call)
					method.ExplicitThis = true;

				is_instance_call = false;
				is_explicit_call = false;

				if (isClassMember)
					method.DeclaringType = codegen.CurrentType;

				if (codegen.CurrentPInvokeInfo != null) {
					if (codegen.GetModuleReference (pinvoke_mod_name) != null) {
						var module = codegen.GetModuleReference (pinvoke_mod_name);

						if (module == null) {
							module = new ModuleReference (pinvoke_mod_name);
							codegen.CurrentModule.ModuleReferences.Add (module);
						}

						codegen.CurrentPInvokeInfo.Module = module;
						pinvoke_mod_name = null;
					}

					if (codegen.CurrentPInvokeInfo.EntryPoint == null)
						codegen.CurrentPInvokeInfo.EntryPoint = name;

					method.PInvokeInfo = codegen.CurrentPInvokeInfo;
					codegen.CurrentPInvokeInfo = null;
				}

				method.MethodReturnType.Attributes = (ParameterAttributes) yyVals[-3+yyTop];

				method.MethodReturnType.MarshalInfo = yyVals[-1+yyTop] != null ? (MarshalInfo) yyVals[-1+yyTop] : null;

				codegen.CurrentMethod = method;
				codegen.GenericContext.CurrentMethodProvider = method;
				generic_param_in_method = true;
			}

void case_404()
#line 1969 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				foreach (var arg in (List<GenericParameter>) yyVals[0+yyTop])
					codegen.CurrentMethod.GenericParameters.Add (arg);

				codegen.GenericContext.CurrentLocalMethodProvider = codegen.CurrentMethod;

				codegen.CurrentMethod.ReturnType = EvaluateType (yyVals[-4+yyTop]);
			}

void case_405()
#line 1978 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				foreach (var arg in (List<ParameterDefinition>) yyVals[-2+yyTop])
					codegen.CurrentMethod.Parameters.Add (arg);

				codegen.GenericContext.CurrentLocalMethodProvider = null;

				codegen.CurrentMethod.ImplAttributes = (MethodImplAttributes) yyVals[0+yyTop];

				codegen.CurrentScope = codegen.CurrentMethod.Body.Scope;

				il = codegen.CurrentMethod.Body.GetILProcessor ();

				if (codegen.CurrentType != null)
					codegen.CurrentType.Methods.Add (codegen.CurrentMethod);
				else
					codegen.CurrentModule.GetModuleType ().Methods.Add (codegen.CurrentMethod);
			}

void case_427()
#line 2025 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				codegen.CurrentPInvokeInfo = new PInvokeInfo ((PInvokeAttributes) yyVals[-1+yyTop], (string) yyVals[-2+yyTop], null);
				pinvoke_mod_name = (string) yyVals[-4+yyTop];
			}

void case_428()
#line 2030 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				codegen.CurrentPInvokeInfo = new PInvokeInfo ((PInvokeAttributes) yyVals[-1+yyTop], null, null);
				pinvoke_mod_name = (string) yyVals[-2+yyTop];
			}

void case_468()
#line 2105 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				yyVal = new List<ParameterDefinition> {
					(ParameterDefinition) yyVals[0+yyTop],
				};
			}

void case_474()
#line 2132 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				yyVal = new ParameterDefinition (EvaluateType (yyVals[-1+yyTop])) {
					Attributes = (ParameterAttributes) yyVals[-2+yyTop],
					MarshalInfo = yyVals[0+yyTop] != null ? (MarshalInfo) yyVals[0+yyTop] : null,
				};
			}

void case_475()
#line 2139 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				yyVal = new ParameterDefinition ((string) yyVals[0+yyTop], (ParameterAttributes) yyVals[-3+yyTop],
					EvaluateType (yyVals[-2+yyTop])) {
					MarshalInfo = yyVals[-1+yyTop] != null ? (MarshalInfo) yyVals[-1+yyTop] : null,
				};
			}

void case_480()
#line 2160 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				foreach (var param in (List<ParameterDefinition>) yyVals[-1+yyTop]) {
					var variable = new VariableDefinition (param.Name, param.ParameterType);

					il.Body.Variables.Add (variable);
					codegen.CurrentScope.Variables.Add (variable);
				}
			}

void case_481()
#line 2169 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				foreach (var param in (List<ParameterDefinition>) yyVals[-1+yyTop]) {
					var variable = new VariableDefinition (param.Name, param.ParameterType);

					il.Body.Variables.Add (variable);
					codegen.CurrentScope.Variables.Add (variable);
				}

				il.Body.InitLocals = true;
			}

void case_482()
#line 2180 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				/* FIXME: Cecil doesn't preserve the entry point if the*/
				/* module is not compiled as an executable.*/
				codegen.CurrentModule.EntryPoint = codegen.CurrentMethod;
			}

void case_487()
#line 2200 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				var overridenMethod = new MethodReference ((string) yyVals[0+yyTop],
					codegen.CurrentMethod.ReturnType, EvaluateType (yyVals[-2+yyTop]));

				codegen.CurrentMethod.Overrides.Add (overridenMethod);
			}

void case_490()
#line 2212 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				var idx = (int) yyVals[-2+yyTop];

				var paramCount = codegen.CurrentMethod.Parameters.Count;

				if (!codegen.CurrentMethod.IsStatic)
					paramCount++;

				ParameterDefinition param;

				if (!codegen.CurrentMethod.IsStatic && idx == 1)
					param = codegen.CurrentMethod.Body.ThisParameter;
				else
					param = codegen.CurrentMethod.Parameters [idx - 1];

				codegen.CurrentCustomAttributeProvider = param;

				if (yyVals[0+yyTop] != null) {
					param.Constant = yyVals[0+yyTop] is Null ? null : yyVals[0+yyTop];
					param.HasDefault = true;
				}
			}

void case_492()
#line 2236 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				var name = (string) yyVals[-1+yyTop];

				var label = new Label (name);

				current_labels.Add (label);
				labels.Add (name, label);
			}

void case_494()
#line 2246 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				var instr = (Instruction) yyVals[0+yyTop];

				is_inside_scope = false;

				foreach (var label in current_labels)
					label.Instruction = instr;

				current_labels.Clear ();

				last_instr = instr;
			}

void case_495()
#line 2259 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				codegen.CurrentMethod.SecurityDeclarations.Add (codegen.CurrentSecurityDeclaration);
				codegen.CurrentSecurityDeclaration = null;
			}

void case_498()
#line 2266 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				if (yyVals[0+yyTop] != null) {
					var provider = codegen.CurrentCustomAttributeProvider != null ?
						codegen.CurrentCustomAttributeProvider : codegen.CurrentType;

					provider.CustomAttributes.Add ((CustomAttribute) yyVals[0+yyTop]);
					codegen.CurrentCustomAttribute = null;
				}
			}

void case_501()
#line 2280 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				var scope = new Scope ();

				var instr = il.Create (OpCodes.Nop);
				il.Append (instr);
				scope.Start = instr;

				is_inside_scope = true;
				scope_stack.Push (codegen.CurrentScope);

				codegen.CurrentScope.Scopes.Add (scope);
				codegen.CurrentScope = scope;
			}

void case_502()
#line 2294 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				var scope = codegen.CurrentScope;

				if (!is_inside_scope)
					scope.End = last_instr;

				is_inside_scope = false;

				yyVal = scope;

				codegen.CurrentScope = scope_stack.Pop ();
			}

void case_503()
#line 2309 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				var area = (Tuple<Instruction, Instruction>) yyVals[-1+yyTop];

				foreach (var handler in (List<ExceptionHandler>) yyVals[0+yyTop]) {
					handler.TryStart = area.X;
					handler.TryEnd = area.Y;

					il.Body.ExceptionHandlers.Add (handler);
				}
			}

void case_504()
#line 2322 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				var scope = (Scope) yyVals[0+yyTop];

				yyVal = new Tuple<Instruction, Instruction> (scope.Start,
					scope.End);
			}

void case_505()
#line 2329 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				var label1 = FindInstruction ((string) yyVals[-2+yyTop]);
				var label2 = FindInstruction ((string) yyVals[0+yyTop]);

				yyVal = new Tuple<Instruction, Instruction> (label1, label2);
			}

void case_506()
#line 2336 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				var label1 = FindInstruction ((int) yyVals[-2+yyTop]);
				var label2 = FindInstruction ((int) yyVals[0+yyTop]);

				yyVal = new Tuple<Instruction, Instruction> (label1, label2);
			}

void case_507()
#line 2345 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				yyVal = new List<ExceptionHandler> {
					(ExceptionHandler) yyVals[0+yyTop],
				};
			}

void case_509()
#line 2357 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				var clause = (Tuple<Instruction, Instruction>) yyVals[0+yyTop];

				yyVal = new ExceptionHandler (ExceptionHandlerType.Catch) {
					CatchType = EvaluateType (yyVals[-1+yyTop]),
					HandlerStart = clause.X,
					HandlerEnd = clause.Y,
				};
			}

void case_510()
#line 2367 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				var clause = (Tuple<Instruction, Instruction>) yyVals[0+yyTop];

				yyVal = new ExceptionHandler (ExceptionHandlerType.Filter) {
					FilterStart = (Instruction) yyVals[-1+yyTop],
					HandlerStart = clause.X,
					HandlerEnd = clause.Y,
				};
			}

void case_511()
#line 2377 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				var clause = (Tuple<Instruction, Instruction>) yyVals[0+yyTop];

				yyVal = new ExceptionHandler (ExceptionHandlerType.Finally) {
					HandlerStart = clause.X,
					HandlerEnd = clause.Y,
				};
			}

void case_512()
#line 2386 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				var clause = (Tuple<Instruction, Instruction>) yyVals[0+yyTop];

				yyVal = new ExceptionHandler (ExceptionHandlerType.Fault) {
					HandlerStart = clause.X,
					HandlerEnd = clause.Y,
				};
			}

void case_516()
#line 2411 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				var scope = (Scope) yyVals[0+yyTop];

				yyVal = new Tuple<Instruction, Instruction> (scope.Start,
					scope.End);
			}

void case_517()
#line 2418 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				var label1 = FindInstruction ((string) yyVals[-2+yyTop]);
				var label2 = FindInstruction ((string) yyVals[0+yyTop]);

				yyVal = new Tuple<Instruction, Instruction> (label1, label2);
			}

void case_518()
#line 2425 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				var label1 = FindInstruction ((int) yyVals[-2+yyTop]);
				var label2 = FindInstruction ((int) yyVals[0+yyTop]);

				yyVal = new Tuple<Instruction, Instruction> (label1, label2);
			}

void case_521()
#line 2438 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{

				var instr = il.Create ((OpCode) yyVals[0+yyTop]);
				il.Append (instr);

				yyVal = instr;
			}

void case_522()
#line 2446 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				var idx = (int) yyVals[0+yyTop];

				var variable = il.Body.Variables [idx];
				var instr = il.Create ((OpCode) yyVals[-1+yyTop], variable);
				il.Append (instr);

				yyVal = instr;
			}

void case_523()
#line 2456 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				var name = (string) yyVals[0+yyTop];
				VariableDefinition variable = null;

				foreach (var local in codegen.CurrentScope.Variables)
					if (local.Name == name)
						variable = local;

				if (variable == null)
					foreach (var scope in scope_stack.QueueReverse ())
						if (scope != null)
							foreach (var local in scope.Variables)
								if (local.Name == name)
									variable = local;


				var instr = il.Create ((OpCode) yyVals[-1+yyTop], variable);
				il.Append (instr);

				yyVal = instr;
			}

void case_524()
#line 2478 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				var idx = (int) yyVals[0+yyTop];

				var paramCount = codegen.CurrentMethod.Parameters.Count;

				if (!codegen.CurrentMethod.IsStatic)
					paramCount++;

				ParameterDefinition param;

				if (!codegen.CurrentMethod.IsStatic && idx == 0)
					param = codegen.CurrentMethod.Body.ThisParameter;
				else {
					if (codegen.CurrentMethod.IsStatic)
						param = codegen.CurrentMethod.Parameters [idx];
					else
						param = codegen.CurrentMethod.Parameters [idx - 1];
				}

				var instr = il.Create ((OpCode) yyVals[-1+yyTop], param);
				il.Append (instr);

				yyVal = instr;
			}

void case_525()
#line 2503 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				var name = (string) yyVals[0+yyTop];
				ParameterDefinition param = null;

				foreach (var parameter in codegen.CurrentMethod.Parameters)
					if (parameter.Name == name)
						param = parameter;

				var instr = il.Create ((OpCode) yyVals[-1+yyTop], param);
				il.Append (instr);

				yyVal = instr;
			}

void case_526()
#line 2517 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				var op = (OpCode) yyVals[-1+yyTop];
				Instruction instr;

				if (op.OperandType == OperandType.ShortInlineI) {
					if (op == OpCodes.Ldc_I4_S)
						instr = il.Create (op, (sbyte) (int) yyVals[0+yyTop]);
					else
						instr = il.Create (op, (byte) (int) yyVals[0+yyTop]);
				}
				else
					instr = il.Create (op, (int) yyVals[0+yyTop]);

				il.Append (instr);

				yyVal = instr;
			}

void case_527()
#line 2535 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				var instr = il.Create ((OpCode) yyVals[-1+yyTop], (long) yyVals[0+yyTop]);
				il.Append (instr);

				yyVal = instr;
			}

void case_528()
#line 2542 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				var op = (OpCode) yyVals[-1+yyTop];
				Instruction instr;

				if (op == OpCodes.Ldc_R4)
					instr = il.Create (op, (float) (double) yyVals[0+yyTop]);
				else
					instr = il.Create (op, (double) yyVals[0+yyTop]);

				il.Append (instr);

				yyVal = instr;
			}

void case_529()
#line 2556 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				var op = (OpCode) yyVals[-1+yyTop];
				Instruction instr;

				if (op == OpCodes.Ldc_R4) {
					var val = BitConverter.ToSingle (BitConverter.GetBytes ((int) (long) yyVals[0+yyTop]),
						BitConverter.IsLittleEndian ? 0 : 4);

					instr = il.Create (op, val);
				} else {
					var val = BitConverter.Int64BitsToDouble ((long) yyVals[0+yyTop]);

					instr = il.Create (op, val);
				}

				il.Append (instr);

				yyVal = instr;
			}

void case_530()
#line 2576 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				var op = (OpCode) yyVals[-1+yyTop];
				Instruction instr;

				if (op == OpCodes.Ldc_R4) {
					var val = BitConverter.ToSingle ((byte[]) yyVals[0+yyTop],
						BitConverter.IsLittleEndian ? 0 : 4);

					instr = il.Create (op, val);
				} else {
					var val = BitConverter.ToDouble ((byte[]) yyVals[0+yyTop],
						BitConverter.IsLittleEndian ? 0 : 8);

					instr = il.Create (op, val);
				}

				il.Append (instr);

				yyVal = instr;
			}

void case_531()
#line 2597 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				/* Some hackery so we can jump to not-yet-created labels.*/
				var instr = il.Create (OpCodes.Nop);
				instr.OpCode = (OpCode) yyVals[-1+yyTop];

				label_jumps.Add (instr, yyVals[0+yyTop]);
				il.Append (instr);

				yyVal = instr;
			}

void case_532()
#line 2608 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				var instr = il.Create ((OpCode) yyVals[-1+yyTop], (MethodReference) yyVals[0+yyTop]);
				il.Append (instr);

				yyVal = instr;
			}

void case_533()
#line 2615 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				var instr = il.Create ((OpCode) yyVals[-1+yyTop], (FieldReference) yyVals[0+yyTop]);
				il.Append (instr);

				yyVal = instr;
			}

void case_534()
#line 2622 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				var instr = il.Create ((OpCode) yyVals[-1+yyTop], EvaluateType (yyVals[0+yyTop]));
				il.Append (instr);

				yyVal = instr;
			}

void case_535()
#line 2629 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				var instr = il.Create ((OpCode) yyVals[-1+yyTop], (string) yyVals[0+yyTop]);
				il.Append (instr);

				yyVal = instr;
			}

void case_536()
#line 2636 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				/* TODO: Do we reencode the string or something?*/
				var instr = il.Create ((OpCode) yyVals[-2+yyTop], (string) yyVals[-1+yyTop]);
				il.Append (instr);

				yyVal = instr;
			}

void case_537()
#line 2644 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				var text = Encoding.Unicode.GetString ((byte[]) yyVals[0+yyTop]);
				var instr = il.Create ((OpCode) yyVals[-3+yyTop], text);
				il.Append (instr);

				yyVal = instr;
			}

void case_538()
#line 2652 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				var call = new CallSite (EvaluateType (yyVals[-3+yyTop]));

				if (is_instance_call)
					call.HasThis = true;

				if (is_explicit_call)
					call.ExplicitThis = true;

				is_instance_call = false;
				is_explicit_call = false;

				foreach (var param in (List<ParameterDefinition>) yyVals[-1+yyTop])
					call.Parameters.Add (param);

				var instr = il.Create ((OpCode) yyVals[-5+yyTop], call);
				il.Append (instr);

				yyVal = instr;
			}

void case_539()
#line 2673 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				Instruction instr;

				if (yyVals[0+yyTop] is TypeReference)
					instr = il.Create ((OpCode) yyVals[-1+yyTop], (TypeReference) yyVals[0+yyTop]);
				else if (yyVals[0+yyTop] is FieldReference)
					instr = il.Create ((OpCode) yyVals[-1+yyTop], (FieldReference) yyVals[0+yyTop]);
				else
					instr = il.Create ((OpCode) yyVals[-1+yyTop], (MethodReference) yyVals[0+yyTop]);

				il.Append (instr);

				yyVal = instr;
			}

void case_540()
#line 2688 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				/* See the note in INSTR_BRTARGET.*/
				var instr = il.Create (OpCodes.Nop);
				instr.OpCode = (OpCode) yyVals[-3+yyTop];

				label_jumps.Add (instr, yyVals[-1+yyTop]);
				il.Append (instr);

				yyVal = instr;
			}

void case_545()
#line 2713 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				yyVal = new List<object> {
					yyVals[0+yyTop],
				};
			}

void case_546()
#line 2719 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				((List<object>) yyVals[0+yyTop]).Add ((object) yyVals[-2+yyTop]);

				yyVal = yyVals[0+yyTop];
			}

void case_549()
#line 2737 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				var declType = EvaluateType (yyVals[-1+yyTop]);

				codegen.GenericContext.CurrentLocalTypeProvider = declType;

				var type = EvaluateType (yyVals[-2+yyTop]);

				codegen.GenericContext.CurrentLocalTypeProvider = null;

				var field = new FieldReference ((string) yyVals[0+yyTop],
					type, declType);

				if (declType == codegen.CurrentModule.GetModuleType ())
					codegen.ModuleFieldReferences.Add (field);

				yyVal = field;
			}

void case_551()
#line 2761 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				/* The spec says that this syntax can refer to a method*/
				/* in the current class. However, Microsoft's ILAsm does*/
				/* not agree, and insists that this syntax can only refer*/
				/* to module-global methods. We maintain compatibility in*/
				/* this case.*/
				yyVal = DeferType (() => codegen.CurrentModule.GetModuleType ());
			}

void case_553()
#line 2773 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				var declType = EvaluateType (yyVals[-2+yyTop]);

				/* See the note in method_head.*/
				var method = new MethodReference ((string) yyVals[-1+yyTop],
					new TypeReference (null, null, null, null), declType) {
					CallingConvention = (MethodCallingConvention) yyVals[-4+yyTop],
				};

				if (is_instance_call)
					method.HasThis = true;

				if (is_explicit_call)
					method.ExplicitThis = true;

				is_instance_call = false;
				is_explicit_call = false;

				if (yyVals[0+yyTop] != null) {
					var genMethod = new GenericInstanceMethod (method);

					foreach (var typeArg in (List<TypeCreator>) yyVals[0+yyTop]) {
						method.GenericParameters.Add(new GenericParameter(method));
						genMethod.GenericArguments.Add (typeArg ());
					}

					method = genMethod;
				}

				codegen.GenericContext.CurrentLocalTypeProvider = declType;
				codegen.GenericContext.CurrentLocalMethodProvider = method;

				method.ReturnType = EvaluateType (yyVals[-3+yyTop]);

				codegen.CurrentMethodReference = method;
			}

void case_554()
#line 2810 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				var method = codegen.CurrentMethodReference;

				codegen.CurrentMethodReference = null;

				foreach (var arg in (List<ParameterDefinition>) yyVals[-1+yyTop])
					method.Parameters.Add (arg);

				codegen.GenericContext.CurrentLocalMethodProvider = null;
				codegen.GenericContext.CurrentLocalTypeProvider = null;

				if (method.DeclaringType == codegen.CurrentModule.GetModuleType ())
					codegen.ModuleMethodReferences.Add (method);

				yyVal = method;
			}

void case_559()
#line 2838 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				var method = new MethodReference ((string) yyVals[-4+yyTop], EvaluateType (yyVals[-7+yyTop]), EvaluateType (yyVals[-6+yyTop])) {
					CallingConvention = (MethodCallingConvention) yyVals[-8+yyTop],
				};

				if (is_instance_call)
					method.HasThis = true;

				if (is_explicit_call)
					method.ExplicitThis = true;

				is_instance_call = false;
				is_explicit_call = false;

				foreach (var param in (List<ParameterDefinition>) yyVals[-1+yyTop])
					method.Parameters.Add (param);

				if (yyVals[-3+yyTop] != null)
					for (var i = 0; i < (int) yyVals[-3+yyTop]; i++)
						method.GenericParameters.Add (new GenericParameter (method));

				yyVal = method;
			}

void case_564()
#line 2878 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				codegen.CurrentType.Events.Add (codegen.CurrentEvent);
				codegen.CurrentEvent = null;
			}

void case_565()
#line 2885 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				codegen.CurrentEvent = new EventDefinition ((string) yyVals[0+yyTop],
					(EventAttributes) yyVals[-2+yyTop], EvaluateType (yyVals[-1+yyTop]));
			}

void case_572()
#line 2905 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				codegen.EventAccessorMethods.Add (new Tuple<EventDefinition, AccessorType, MethodReference> (
					codegen.CurrentEvent,
					AccessorType.Add,
					(MethodReference) yyVals[0+yyTop]));
			}

void case_573()
#line 2912 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				codegen.EventAccessorMethods.Add (new Tuple<EventDefinition, AccessorType, MethodReference> (
					codegen.CurrentEvent,
					AccessorType.Remove,
					(MethodReference) yyVals[0+yyTop]));
			}

void case_574()
#line 2919 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				codegen.EventAccessorMethods.Add (new Tuple<EventDefinition, AccessorType, MethodReference> (
					codegen.CurrentEvent,
					AccessorType.Fire,
					(MethodReference) yyVals[0+yyTop]));
			}

void case_575()
#line 2926 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				codegen.EventAccessorMethods.Add (new Tuple<EventDefinition, AccessorType, MethodReference> (
					codegen.CurrentEvent,
					AccessorType.Other,
					(MethodReference) yyVals[0+yyTop]));
			}

void case_576()
#line 2933 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				if (yyVals[0+yyTop] != null) {
					codegen.CurrentEvent.CustomAttributes.Add ((CustomAttribute) yyVals[0+yyTop]);
					codegen.CurrentCustomAttribute = null;
				}
			}

void case_580()
#line 2945 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				codegen.CurrentType.Properties.Add (codegen.CurrentProperty);
				codegen.CurrentProperty = null;
			}

void case_581()
#line 2952 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				codegen.CurrentProperty = new PropertyDefinition ((string) yyVals[-4+yyTop],
					(PropertyAttributes) yyVals[-6+yyTop], EvaluateType (yyVals[-5+yyTop]));

				if (is_instance_call)
					codegen.CurrentProperty.HasThis = true;

				is_instance_call = false;
				is_explicit_call = false;

				/* FIXME: When no getter/setter is defined (i.e. right at*/
				/* this point), Cecil just returns a dummy collection that's*/
				/* useful for just about nothing, and our parameters end up*/
				/* going nowhere.*/
				foreach (var param in (List<ParameterDefinition>) yyVals[-2+yyTop])
					codegen.CurrentProperty.Parameters.Add (param);

				if (yyVals[0+yyTop] != null)
					codegen.CurrentProperty.Constant = yyVals[0+yyTop] is Null ? null : yyVals[0+yyTop];
			}

void case_587()
#line 2987 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				codegen.PropertyAccessorMethods.Add (new Tuple<PropertyDefinition, AccessorType, MethodReference> (
					codegen.CurrentProperty,
					AccessorType.Set,
					(MethodReference) yyVals[0+yyTop]));
			}

void case_588()
#line 2994 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				codegen.PropertyAccessorMethods.Add (new Tuple<PropertyDefinition, AccessorType, MethodReference> (
					codegen.CurrentProperty,
					AccessorType.Get,
					(MethodReference) yyVals[0+yyTop]));
			}

void case_589()
#line 3001 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				codegen.PropertyAccessorMethods.Add (new Tuple<PropertyDefinition, AccessorType, MethodReference> (
					codegen.CurrentProperty,
					AccessorType.Other,
					(MethodReference) yyVals[0+yyTop]));
			}

void case_590()
#line 3008 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				if (yyVals[0+yyTop] != null) {
					codegen.CurrentProperty.CustomAttributes.Add ((CustomAttribute) yyVals[0+yyTop]);
					codegen.CurrentCustomAttribute = null;
				}
			}

void case_595()
#line 3021 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				((ICustomAttributeProvider) yyVals[0+yyTop]).CustomAttributes.Add (codegen.CurrentCustomAttribute);
				codegen.CurrentCustomAttribute = null;

				yyVal = null;
			}

void case_597()
#line 3034 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				var bytes = Encoding.Unicode.GetBytes ((string) yyVals[0+yyTop]);

				yyVal = codegen.CurrentCustomAttribute = new CustomAttribute ((MethodReference) yyVals[-2+yyTop], bytes);
			}

void case_601()
#line 3051 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				codegen.CurrentCustomAttribute = new CustomAttribute ((MethodReference) yyVals[0+yyTop]);

				yyVal = ResolveCustomAttributeOwner (yyVals[-2+yyTop]);
			}

void case_602()
#line 3057 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				var bytes = Encoding.Unicode.GetBytes ((string) yyVals[0+yyTop]);

				codegen.CurrentCustomAttribute = new CustomAttribute ((MethodReference) yyVals[-2+yyTop], bytes);

				yyVal = ResolveCustomAttributeOwner (yyVals[-4+yyTop]);
			}

void case_603()
#line 3065 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				codegen.CurrentCustomAttribute = new CustomAttribute ((MethodReference) yyVals[-2+yyTop], (byte[]) yyVals[0+yyTop]);

				yyVal = ResolveCustomAttributeOwner (yyVals[-4+yyTop]);
			}

void case_604()
#line 3071 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				codegen.CurrentCustomAttribute = new CustomAttribute ((MethodReference) yyVals[-2+yyTop]);

				yyVal = ResolveCustomAttributeOwner (yyVals[-4+yyTop]);
			}

void case_606()
#line 3081 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				foreach (var pair in (List<Tuple<TokenType, CustomAttributeNamedArgument>>) yyVals[-1+yyTop]) {
					if (pair.X == TokenType.Field)
						codegen.CurrentCustomAttribute.Fields.Add (pair.Y);
					else
						codegen.CurrentCustomAttribute.Properties.Add (pair.Y);
				}
			}

void case_611()
#line 3104 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				var namedArg = new CustomAttributeNamedArgument ((string) yyVals[-2+yyTop],
					(CustomAttributeArgument) yyVals[0+yyTop]);
				var tuple = new Tuple<TokenType, CustomAttributeNamedArgument> ((TokenType) yyVals[-4+yyTop], namedArg);

				((List<Tuple<TokenType, CustomAttributeNamedArgument>>) yyVals[-5+yyTop]).Add (tuple);
			}

void case_619()
#line 3141 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				yyVal = new CustomAttributeArgument (codegen.Corlib.Single,
					(float) (double) yyVals[-1+yyTop]);
			}

void case_621()
#line 3150 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				yyVal = new CustomAttributeArgument (codegen.Corlib.Single,
					BitConverter.ToSingle (BitConverter.GetBytes ((long) yyVals[-1+yyTop]),
						BitConverter.IsLittleEndian ? 0 : 4));
			}

void case_622()
#line 3156 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				yyVal = new CustomAttributeArgument (codegen.Corlib.Double,
					BitConverter.Int64BitsToDouble ((long) yyVals[-1+yyTop]));
			}

void case_624()
#line 3165 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				yyVal = new CustomAttributeArgument (codegen.Corlib.UInt64,
					(ulong) (long) yyVals[-1+yyTop]);
			}

void case_625()
#line 3170 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				yyVal = new CustomAttributeArgument (codegen.Corlib.Int32,
					(int) (long) yyVals[-1+yyTop]);
			}

void case_626()
#line 3175 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				yyVal = new CustomAttributeArgument (codegen.Corlib.UInt32,
					(uint) (long) yyVals[-1+yyTop]);
			}

void case_627()
#line 3180 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				yyVal = new CustomAttributeArgument (codegen.Corlib.Int16,
					(short) (long) yyVals[-1+yyTop]);
			}

void case_628()
#line 3185 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				yyVal = new CustomAttributeArgument (codegen.Corlib.UInt16,
					(ushort) (long) yyVals[-1+yyTop]);
			}

void case_629()
#line 3190 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				yyVal = new CustomAttributeArgument (codegen.Corlib.SByte,
					(sbyte) (long) yyVals[-1+yyTop]);
			}

void case_630()
#line 3195 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				yyVal = new CustomAttributeArgument (codegen.Corlib.Byte,
					(byte) (long) yyVals[-1+yyTop]);
			}

void case_631()
#line 3200 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				yyVal = new CustomAttributeArgument (codegen.Corlib.Char,
					(char) (long) yyVals[-1+yyTop]);
			}

void case_633()
#line 3209 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				var bytes = (byte[]) yyVals[0+yyTop];
				var cas = new List<CustomAttributeArgument> (bytes.Length);

				foreach (var b in bytes)
					cas.Add (new CustomAttributeArgument (codegen.Corlib.Byte, b));

				yyVal = new CustomAttributeArgument (new ArrayType (codegen.Corlib.Byte),
					cas.ToArray ());
			}

void case_636()
#line 3228 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				yyVal = new CustomAttributeArgument (codegen.Corlib.Type,
					codegen.CurrentModule.GetType ((string) yyVals[-1+yyTop], true));
			}

void case_640()
#line 3245 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				custom_attr_blob_seq_length = System.Math.Max (0, (int) yyVals[-4+yyTop]);

				yyVal = new CustomAttributeArgument (new ArrayType (codegen.Corlib.Single),
					yyVals[-1+yyTop]);
			}

void case_641()
#line 3252 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				custom_attr_blob_seq_length = System.Math.Max (0, (int) yyVals[-4+yyTop]);

				yyVal = new CustomAttributeArgument (new ArrayType (codegen.Corlib.Double),
					yyVals[-1+yyTop]);
			}

void case_643()
#line 3263 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				custom_attr_blob_seq_length = System.Math.Max (0, (int) yyVals[-5+yyTop]);

				yyVal = new CustomAttributeArgument (new ArrayType (codegen.Corlib.SByte),
					yyVals[-1+yyTop]);
			}

void case_645()
#line 3274 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				custom_attr_blob_seq_length = System.Math.Max (0, (int) yyVals[-5+yyTop]);

				yyVal = new CustomAttributeArgument (new ArrayType (codegen.Corlib.Int16),
					yyVals[-1+yyTop]);
			}

void case_647()
#line 3285 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				custom_attr_blob_seq_length = System.Math.Max (0, (int) yyVals[-5+yyTop]);

				yyVal = new CustomAttributeArgument (new ArrayType (codegen.Corlib.Int32),
					yyVals[-1+yyTop]);
			}

void case_649()
#line 3296 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				custom_attr_blob_seq_length = System.Math.Max (0, (int) yyVals[-5+yyTop]);

				yyVal = new CustomAttributeArgument (new ArrayType (codegen.Corlib.Int64),
					yyVals[-1+yyTop]);
			}

void case_651()
#line 3307 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				custom_attr_blob_seq_length = System.Math.Max (0, (int) yyVals[-5+yyTop]);

				yyVal = new CustomAttributeArgument (new ArrayType (codegen.Corlib.Byte),
					yyVals[-1+yyTop]);
			}

void case_653()
#line 3318 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				custom_attr_blob_seq_length = System.Math.Max (0, (int) yyVals[-5+yyTop]);

				yyVal = new CustomAttributeArgument (new ArrayType (codegen.Corlib.UInt16),
					yyVals[-1+yyTop]);
			}

void case_655()
#line 3329 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				custom_attr_blob_seq_length = System.Math.Max (0, (int) yyVals[-5+yyTop]);

				yyVal = new CustomAttributeArgument (new ArrayType (codegen.Corlib.UInt32),
					yyVals[-1+yyTop]);
			}

void case_657()
#line 3340 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				custom_attr_blob_seq_length = System.Math.Max (0, (int) yyVals[-5+yyTop]);

				yyVal = new CustomAttributeArgument (new ArrayType (codegen.Corlib.UInt64),
					yyVals[-1+yyTop]);
			}

void case_658()
#line 3347 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				custom_attr_blob_seq_length = System.Math.Max (0, (int) yyVals[-4+yyTop]);

				yyVal = new CustomAttributeArgument (new ArrayType (codegen.Corlib.Char),
					yyVals[0+yyTop]);
			}

void case_659()
#line 3354 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				custom_attr_blob_seq_length = System.Math.Max (0, (int) yyVals[-4+yyTop]);

				yyVal = new CustomAttributeArgument (new ArrayType (codegen.Corlib.Boolean),
					yyVals[0+yyTop]);
			}

void case_660()
#line 3361 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				custom_attr_blob_seq_length = System.Math.Max (0, (int) yyVals[-4+yyTop]);

				yyVal = new CustomAttributeArgument (new ArrayType (codegen.Corlib.String),
					yyVals[-1+yyTop]);
			}

void case_661()
#line 3368 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				custom_attr_blob_seq_length = System.Math.Max (0, (int) yyVals[-4+yyTop]);

				yyVal = new CustomAttributeArgument (new ArrayType (codegen.Corlib.Type),
					yyVals[-1+yyTop]);
			}

void case_662()
#line 3375 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				custom_attr_blob_seq_length = System.Math.Max (0, (int) yyVals[-4+yyTop]);

				yyVal = new CustomAttributeArgument (new ArrayType (codegen.Corlib.Object),
					yyVals[-1+yyTop]);
			}

void case_664()
#line 3388 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				var arr = (CustomAttributeArgument[]) yyVals[-1+yyTop];

				if (arr.Length != custom_attr_blob_seq_length) {
					arr = arr.Inflate (1);
					arr [arr.Length - 1] = new CustomAttributeArgument (codegen.Corlib.Single,
						(float) (double) yyVals[0+yyTop]);
				}
			}

void case_665()
#line 3398 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				var arr = (CustomAttributeArgument[]) yyVals[-1+yyTop];

				if (arr.Length != custom_attr_blob_seq_length) {
					arr = arr.Inflate (1);
					arr [arr.Length - 1] = new CustomAttributeArgument (codegen.Corlib.Single,
						BitConverter.ToSingle (BitConverter.GetBytes ((int) yyVals[0+yyTop]),
							BitConverter.IsLittleEndian ? 0 : 4));
				}
			}

void case_667()
#line 3415 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				var arr = (CustomAttributeArgument[]) yyVals[-1+yyTop];

				if (arr.Length != custom_attr_blob_seq_length) {
					arr = arr.Inflate (1);
					arr [arr.Length - 1] = new CustomAttributeArgument (codegen.Corlib.Double,
						yyVals[0+yyTop]);
				}
			}

void case_668()
#line 3425 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				var arr = (CustomAttributeArgument[]) yyVals[-1+yyTop];

				if (arr.Length != custom_attr_blob_seq_length) {
					arr = arr.Inflate (1);
					arr [arr.Length - 1] = new CustomAttributeArgument (codegen.Corlib.Double,
						BitConverter.Int64BitsToDouble ((long) yyVals[0+yyTop]));
				}
			}

void case_670()
#line 3441 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				var arr = (CustomAttributeArgument[]) yyVals[-1+yyTop];

				if (arr.Length != custom_attr_blob_seq_length) {
					arr = arr.Inflate (1);
					arr [arr.Length - 1] = new CustomAttributeArgument (custom_attr_unsigned ?
						codegen.Corlib.Byte : codegen.Corlib.SByte,
						custom_attr_unsigned ? (object) (byte) (int) yyVals[0+yyTop] :
							(object) (sbyte) (int) yyVals[0+yyTop]);
				}
			}

void case_672()
#line 3459 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				var arr = (CustomAttributeArgument[]) yyVals[-1+yyTop];

				if (arr.Length != custom_attr_blob_seq_length) {
					arr = arr.Inflate (1);
					arr [arr.Length - 1] = new CustomAttributeArgument (custom_attr_unsigned ?
						codegen.Corlib.UInt16 : codegen.Corlib.Int16,
						custom_attr_unsigned ? (object) (ushort) (int) yyVals[0+yyTop] :
							(object) (short) (int) yyVals[0+yyTop]);
				}
			}

void case_674()
#line 3477 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				var arr = (CustomAttributeArgument[]) yyVals[-1+yyTop];

				if (arr.Length != custom_attr_blob_seq_length) {
					arr = arr.Inflate (1);
					arr [arr.Length - 1] = new CustomAttributeArgument (custom_attr_unsigned ?
						codegen.Corlib.UInt32 : codegen.Corlib.Int32,
						custom_attr_unsigned ? (object) (uint) (int) yyVals[0+yyTop] : yyVals[0+yyTop]);
				}
			}

void case_676()
#line 3494 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				var arr = (CustomAttributeArgument[]) yyVals[-1+yyTop];

				if (arr.Length != custom_attr_blob_seq_length) {
					arr = arr.Inflate (1);
					arr [arr.Length - 1] = new CustomAttributeArgument (custom_attr_unsigned ?
						codegen.Corlib.UInt64 : codegen.Corlib.Int64,
						custom_attr_unsigned ? (object) (ulong) (long) yyVals[0+yyTop] : yyVals[0+yyTop]);
				}
			}

void case_678()
#line 3511 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				var arr = (CustomAttributeArgument[]) yyVals[-1+yyTop];

				if (arr.Length != custom_attr_blob_seq_length) {
					arr = arr.Inflate (1);
					arr [arr.Length - 1] = new CustomAttributeArgument (codegen.Corlib.Boolean,
						yyVals[0+yyTop]);
				}
			}

void case_680()
#line 3527 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				var arr = (CustomAttributeArgument[]) yyVals[-1+yyTop];

				if (arr.Length != custom_attr_blob_seq_length) {
					arr = arr.Inflate (1);
					arr [arr.Length - 1] = new CustomAttributeArgument (codegen.Corlib.String,
						null);
				}
			}

void case_681()
#line 3537 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				var arr = (CustomAttributeArgument[]) yyVals[-1+yyTop];

				if (arr.Length != custom_attr_blob_seq_length) {
					arr = arr.Inflate (1);
					arr [arr.Length - 1] = new CustomAttributeArgument (codegen.Corlib.String,
						yyVals[0+yyTop]);
				}
			}

void case_683()
#line 3553 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				var arr = (CustomAttributeArgument[]) yyVals[-1+yyTop];

				if (arr.Length != custom_attr_blob_seq_length) {
					arr = arr.Inflate (1);
					arr [arr.Length - 1] = new CustomAttributeArgument (codegen.Corlib.Type,
						null);
				}
			}

void case_684()
#line 3563 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				var arr = (CustomAttributeArgument[]) yyVals[-2+yyTop];

				if (arr.Length != custom_attr_blob_seq_length) {
					arr = arr.Inflate (1);
					arr [arr.Length - 1] = new CustomAttributeArgument (codegen.Corlib.Type,
						codegen.CurrentModule.GetType ((string) yyVals[0+yyTop], true));
				}
			}

void case_685()
#line 3573 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				var arr = (CustomAttributeArgument[]) yyVals[-1+yyTop];

				if (arr.Length != custom_attr_blob_seq_length) {
					arr = arr.Inflate (1);
					arr [arr.Length - 1] = new CustomAttributeArgument (codegen.Corlib.Type,
						yyVals[0+yyTop]);
				}
			}

void case_687()
#line 3589 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				var arr = (CustomAttributeArgument[]) yyVals[-1+yyTop];

				if (arr.Length != custom_attr_blob_seq_length) {
					arr = arr.Inflate (1);
					arr [arr.Length - 1] = new CustomAttributeArgument (codegen.Corlib.Object,
						yyVals[0+yyTop]);
				}
			}

void case_690()
#line 3611 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				if (codegen.CurrentSecurityDeclaration == null)
					codegen.CurrentSecurityDeclaration = new SecurityDeclaration ((SecurityAction) yyVals[-4+yyTop]);

				var attr = new SecurityAttribute (EvaluateType (yyVals[-3+yyTop]));

				foreach (var obj in (List<Tuple<string, object>>) yyVals[-1+yyTop]) {
					TypeReference type;

					if (obj.Y is bool)
						type = codegen.Corlib.Boolean;
					else if (obj.Y is int)
						type = codegen.Corlib.Int32;
					else
						type = codegen.Corlib.String;

					attr.Properties.Add (new CustomAttributeNamedArgument (obj.X,
						new CustomAttributeArgument (type, obj.Y)));
				}

				codegen.CurrentSecurityDeclaration.SecurityAttributes.Add (attr);
			}

void case_691()
#line 3634 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				if (codegen.CurrentSecurityDeclaration == null)
					codegen.CurrentSecurityDeclaration = new SecurityDeclaration ((SecurityAction) yyVals[-5+yyTop]);

				/* TODO: We can't pass security attribute constructor args with Cecil.*/
			}

void case_692()
#line 3641 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				if (codegen.CurrentSecurityDeclaration == null)
					codegen.CurrentSecurityDeclaration = new SecurityDeclaration ((SecurityAction) yyVals[-1+yyTop]);

				codegen.CurrentSecurityDeclaration.SecurityAttributes.Add (new SecurityAttribute (EvaluateType (yyVals[0+yyTop])));
			}

void case_693()
#line 3648 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				if (codegen.CurrentSecurityDeclaration == null)
					codegen.CurrentSecurityDeclaration = new SecurityDeclaration ((SecurityAction) yyVals[-1+yyTop], (byte[]) yyVals[0+yyTop]);
			}

void case_694()
#line 3653 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				if (codegen.CurrentSecurityDeclaration == null)
					codegen.CurrentSecurityDeclaration = new SecurityDeclaration ((SecurityAction) yyVals[-1+yyTop]);

				/* TODO: We need a way to set the XML.*/
			}

void case_695()
#line 3660 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				if (codegen.CurrentSecurityDeclaration == null)
					codegen.CurrentSecurityDeclaration = new SecurityDeclaration ((SecurityAction) yyVals[-4+yyTop]);

				foreach (var attr in (List<SecurityAttribute>) yyVals[-1+yyTop])
					codegen.CurrentSecurityDeclaration.SecurityAttributes.Add (attr);
			}

void case_713()
#line 3697 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				var attr = new SecurityAttribute (EvaluateType (yyVals[-4+yyTop]));

				foreach (var arg in (List<Tuple<TokenType, CustomAttributeNamedArgument>>) yyVals[-1+yyTop]) {
					if (arg.X == TokenType.Field)
						attr.Fields.Add (arg.Y);
					else
						attr.Properties.Add (arg.Y);
				}

				yyVal = attr;
			}

void case_714()
#line 3710 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				var attr = new SecurityAttribute (codegen.CurrentModule.GetType ((string) yyVals[-4+yyTop], true));

				foreach (var arg in (List<Tuple<TokenType, CustomAttributeNamedArgument>>) yyVals[-1+yyTop]) {
					if (arg.X == TokenType.Field)
						attr.Fields.Add (arg.Y);
					else
						attr.Properties.Add (arg.Y);
				}

				yyVal = attr;
			}

void case_730()
#line 3771 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				var name = (string) yyVals[0+yyTop];

				if (!codegen.HasModuleDirective) {
					/* Microsoft's ILAsm only takes into account the*/
					/* first .module directive*/
					codegen.CurrentModule.Name = name;
					codegen.HasModuleDirective = true;
				}
			}

void case_731()
#line 3782 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				var name = (string) yyVals[0+yyTop];

				var module = new ModuleReference (name);
				codegen.CurrentModule.ModuleReferences.Add (module);
			}

void case_739()
#line 3819 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				var name = (string) yyVals[0+yyTop];
				var asmName = codegen.CurrentModule.Assembly.Name;

				asmName.Attributes = (AssemblyAttributes) yyVals[-1+yyTop];
				asmName.Name = name;
				codegen.HasAssemblyDirective = true;
			}

void case_755()
#line 3862 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				codegen.CurrentModule.Assembly.Name.Version =
					new Version ((int) yyVals[-6+yyTop], (int) yyVals[-4+yyTop], (int) yyVals[-2+yyTop], (int) yyVals[0+yyTop]);
			}

void case_757()
#line 3871 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				/* I assume this is UTF-16. I really have no clue, though,*/
				/* and it isn't documented...*/
				var text = Encoding.Unicode.GetString ((byte[]) yyVals[0+yyTop]);
				codegen.CurrentModule.Assembly.Name.Culture = text;
			}

void case_758()
#line 3878 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				var algo = (AssemblyHashAlgorithm) (int) yyVals[0+yyTop];

				codegen.CurrentModule.Assembly.Name.HashAlgorithm = algo;
			}

void case_759()
#line 3884 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				if (yyVals[0+yyTop] != null) {
					codegen.CurrentModule.Assembly.CustomAttributes.Add ((CustomAttribute) yyVals[0+yyTop]);
					codegen.CurrentCustomAttribute = null;
				}
			}

void case_760()
#line 3891 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				codegen.CurrentModule.Assembly.SecurityDeclarations.Add (codegen.CurrentSecurityDeclaration);
				codegen.CurrentSecurityDeclaration = null;
			}

void case_762()
#line 3899 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				var asmRef = codegen.CurrentAssemblyReference;
				var asmRefAlias = asmRef as AliasedAssemblyNameReference;
				var name = asmRefAlias != null ? asmRefAlias.Alias : asmRef.Name;

				/* If we already have a reference with this name, we*/
				/* ignore the one we just built.*/
				if (codegen.GetAliasedAssemblyReference (name) == null) {
					/* If we have a public key and a token, we prefer the*/
					/* token. Cecil prefers the public key, so we have to*/
					/* do this ourselves.*/
					if (has_public_key && has_public_key_token) {
						var token = asmRef.PublicKeyToken;
						asmRef.PublicKey = null;
						asmRef.PublicKeyToken = token;
					}

					if (asmRefAlias != null)
						codegen.AliasedAssemblyReferences.Add (asmRefAlias.Alias, asmRefAlias);

					codegen.CurrentModule.AssemblyReferences.Add (asmRef);
				}
				
				codegen.CurrentAssemblyReference = null;
				has_public_key = false;
				has_public_key_token = false;
			}

void case_763()
#line 3929 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				codegen.CurrentAssemblyReference = new AssemblyNameReference ((string) yyVals[0+yyTop],
					new Version (0, 0, 0, 0)) {
					Attributes = (AssemblyAttributes) yyVals[-1+yyTop],
				};
			}

void case_764()
#line 3936 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{

				codegen.CurrentAssemblyReference = new AliasedAssemblyNameReference ((string) yyVals[-2+yyTop],
					new Version (0, 0, 0, 0)) {
					Alias = (string) yyVals[0+yyTop],
					Attributes = (AssemblyAttributes) yyVals[-3+yyTop],
				};
			}

void case_767()
#line 3951 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				codegen.CurrentAssemblyReference.Version =
					new Version ((int) yyVals[-6+yyTop], (int) yyVals[-4+yyTop], (int) yyVals[-2+yyTop], (int) yyVals[0+yyTop]);
			}

void case_768()
#line 3956 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				codegen.CurrentAssemblyReference.PublicKey = (byte[]) yyVals[0+yyTop];
				has_public_key = true;
			}

void case_769()
#line 3961 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				codegen.CurrentAssemblyReference.PublicKeyToken = (byte[]) yyVals[0+yyTop];
				has_public_key_token = true;
			}

void case_771()
#line 3970 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				/* See the note in assembly_decl -> D_LOCALE.*/
				var text = Encoding.Unicode.GetString ((byte[]) yyVals[0+yyTop]);
				codegen.CurrentAssemblyReference.Culture = text;
			}

void case_779()
#line 3995 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				var qn = (QualifiedName) yyVals[0+yyTop];

				yyVal = new Tuple<QualifiedName, TypeAttributes> (qn, (TypeAttributes) yyVals[-1+yyTop]);
			}

void case_791()
#line 4019 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				/* Just use the last declaration.*/
				yyVal = yyVals[0+yyTop];
			}

void case_799()
#line 4044 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				var head = yyVals[-3+yyTop] as Tuple<ManifestResourceAttributes, string>;
				var aliasedHead = yyVals[-3+yyTop] as Tuple<ManifestResourceAttributes, string, string>;

				string resName;
				string fileName;
				ManifestResourceAttributes attr;

				if (aliasedHead != null) {
					attr = aliasedHead.X;
					resName = aliasedHead.Y;
					fileName = aliasedHead.Z;
				} else {
					attr = head.X;
					resName = head.Y;
					fileName = head.Y;
				}

				Resource rsc;

				object resourceArg = null;
				var decls = (List<object>) yyVals[-1+yyTop];

				for (var i = 0; i < decls.Count; i++) {
					var decl = decls [i];

					if (!(decl is CustomAttribute)) {
						decls.Remove (decl);
						resourceArg = decl;
					}
				}

				if (resourceArg != null) {
					var asmDecl = resourceArg as string;

					if (asmDecl != null) {
						var asmRef = codegen.GetAssemblyReference (asmDecl);
						rsc = new AssemblyLinkedResource (resName, attr, asmRef);
					} else {
						/* TODO: Propagate nometadata/.entrypoint/offset.*/
						/* TODO: Error if the file was not declared with .file.*/
						var fileDecl = resourceArg as Tuple<string, int>;
						rsc = new LinkedResource (resName, attr, fileDecl.X);
					}
				} else {
					byte[] bytes = null;

					try
					{
						bytes = File.ReadAllBytes (fileName);
					}
					catch (Exception ex)
					{
						throw new System.IO.FileNotFoundException ();
					}

					rsc = new EmbeddedResource (resName, attr, bytes);
				}

				/* TODO: Cecil doesn't support CAs on resources yet.*/
				/*foreach (var cattr in decls)*/
				/*	rsc.CustomAttributes.Add ((CustomAttribute) cattr);*/

				codegen.CurrentModule.Resources.Add (rsc);
			}

void case_800()
#line 4112 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				yyVal = new Tuple<ManifestResourceAttributes, string> ((ManifestResourceAttributes) yyVals[-1+yyTop],
					(string) yyVals[0+yyTop]);
			}

void case_801()
#line 4117 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				yyVal = new Tuple<ManifestResourceAttributes, string, string> ((ManifestResourceAttributes) yyVals[-3+yyTop],
					(string) yyVals[-2+yyTop], (string) yyVals[0+yyTop]);
			}

void case_803()
#line 4128 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				/* Blame Microsoft for this madness.*/
				yyVal = ((ManifestResourceAttributes) yyVals[-1+yyTop] != 0 && (ManifestResourceAttributes) yyVals[-1+yyTop] !=
					ManifestResourceAttributes.Public) ? 0 : ManifestResourceAttributes.Public;
			}

void case_804()
#line 4134 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				yyVal = ((ManifestResourceAttributes) yyVals[-1+yyTop] != 0 && (ManifestResourceAttributes) yyVals[-1+yyTop] !=
					ManifestResourceAttributes.Private) ? 0 : ManifestResourceAttributes.Private;
			}

void case_806()
#line 4145 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				yyVal = new List<object> {
					yyVals[0+yyTop],
				};
			}

void case_808()
#line 4157 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				yyVal = new Tuple<string, int> ((string) yyVals[-2+yyTop],
					(int) yyVals[0+yyTop]);
			}

void case_813()
#line 4171 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				var sl = (QualifiedName) yyVals[-2+yyTop];

				var qn = new QualifiedName {
					Name = ((QualifiedName) yyVals[0+yyTop]).FullName, /* TODO: Verify this.*/
				};

				foreach (var ns in sl.Namespaces)
					qn.Namespaces.Add (ns);

				foreach (var nesting in sl.Nestings)
					qn.Nestings.Add (nesting);

				qn.Nestings.Add (sl.Name);

				yyVal = qn;
			}

void case_817()
#line 4204 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				yyVal = BitConverter.ToInt32 (BitConverter.GetBytes ((long) yyVals[0+yyTop]),
					BitConverter.IsLittleEndian ? 0 : 4);
			}

void case_821()
#line 4219 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				yyVal = (double) BitConverter.ToSingle (BitConverter.GetBytes ((long) yyVals[-1+yyTop]),
					BitConverter.IsLittleEndian ? 0 : 4);
			}

void case_826()
#line 4241 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				tokenizer.InByteArray  = false;

				yyVal = yyVals[-1+yyTop];
			}

void case_829()
#line 4259 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				yyVal = new List<byte> {
					Convert.ToByte (yyVals[0+yyTop]),
				};
			}

void case_836()
#line 4291 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				/* HACK: A bug exists in the tokenizer, which causes it*/
				/* to feed us an ID token with dots. We work around this*/
				/* for now.*/
				var str = (string) yyVals[0+yyTop];
				var nspaces = str.Split ('.');

				if (str.StartsWith (".")) {
					var dots = string.Empty;

					foreach (var chr in str) {
						if (chr == '.')
							dots += chr;
						else
							break;
					}

					var newNamespaces = new string[nspaces.Length - dots.Length];

					for (int i = dots.Length, j = 0; i < nspaces.Length; i++, j++)
						newNamespaces [j] = nspaces [i];

					newNamespaces [0] = dots + newNamespaces [0];
					nspaces = newNamespaces;
				}

				var qn = new QualifiedName {
					Name = nspaces [nspaces.Length - 1],
				};

				for (var i = 0; i < nspaces.Length; i++)
					if (i != nspaces.Length - 1)
						qn.Namespaces.Add (nspaces [i]);

				yyVal = qn;
			}

void case_837()
#line 4328 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				var strings = ((string) yyVals[0+yyTop]).Split ('.');

				var qn = new QualifiedName {
					Name = strings [1],
				};
				qn.Namespaces.Add (strings [0]);

				yyVal = qn;
			}

void case_838()
#line 4339 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"
{
				var n1 = (QualifiedName) yyVals[-2+yyTop];
				var n2 = (QualifiedName) yyVals[0+yyTop];

				var qn = new QualifiedName {
					Name = n2.Name,
				};

				foreach (var ns in n1.Namespaces)
					qn.Namespaces.Add (ns);

				qn.Namespaces.Add (n1.Name);

				foreach (var ns in n2.Namespaces)
					qn.Namespaces.Add (ns);

				yyVal = qn;
			}

#line default
   static readonly short [] yyLhs  = {              -1,
    0,    1,    1,    2,    2,    2,    2,    2,    2,    2,
    2,    2,    2,    2,    2,    2,    2,    2,    2,    2,
    2,    2,    2,    2,    2,    2,    2,    2,    2,   25,
   25,   21,   21,   21,   21,   21,   21,   21,   21,   21,
   21,   21,   22,   22,   22,    8,    9,   27,   27,   27,
   27,   27,   27,   23,   29,   29,   29,   29,   29,   24,
   36,   36,   36,   36,   36,   36,   36,   36,   36,    4,
   37,    3,   43,   45,   39,   41,   41,   41,   41,   41,
   41,   41,   41,   41,   41,   41,   41,   41,   41,   41,
   41,   41,   41,   41,   41,   41,   41,   41,   41,   41,
   41,   44,   44,   46,   46,   48,   48,   48,   42,   42,
   49,   49,   50,   51,   51,   51,   51,   51,   51,   53,
   52,   52,   54,   54,   40,   40,   55,   55,   55,   55,
   55,   55,   55,   55,   55,   55,   55,   55,   55,   55,
   55,   55,   58,   58,   20,   20,   32,   32,   32,   32,
   32,   32,   32,   32,   32,   65,   66,   66,   30,   30,
   30,   30,   30,   30,   30,   30,   30,   30,   30,   30,
   30,   30,   30,   30,   30,   30,   30,   30,   30,   30,
   30,   30,   70,   70,   71,   71,   72,   72,   73,   73,
   74,   74,   74,   74,   74,   74,   74,   74,   69,   69,
   69,   69,   69,   69,   75,   75,   68,   76,   76,   47,
   47,   47,   47,   67,   67,   77,   77,   77,   77,   77,
   60,   60,   60,   60,   78,   78,   78,   78,   78,   78,
   78,   79,   79,   79,   79,   79,   79,   79,   79,   79,
   79,   79,   79,   79,   79,   79,   79,   83,   83,   81,
   81,   81,   81,   81,   81,   81,   81,   81,   81,   81,
   81,   81,   81,   81,   81,   81,   81,   81,   81,   81,
   81,   81,   81,   81,   81,   81,   81,   81,   81,   81,
   84,   84,   84,   84,   84,   84,   84,   82,   82,   82,
   82,   82,   82,   82,   82,   82,   82,   82,   82,   82,
   82,   82,   82,   82,   82,   82,   82,   85,   85,   85,
   85,   85,   85,   85,   85,   85,   85,   85,   85,   85,
   85,   85,   85,   85,   85,   85,   85,   85,   85,   85,
   85,   85,    6,   86,   86,   87,   87,   87,   87,   87,
   87,   87,   87,   87,   87,   87,   87,   87,   87,   88,
   88,   89,   89,   90,   90,   92,   92,   92,   92,   92,
   92,   92,   92,   92,   92,   92,   92,   92,   92,   92,
   91,   91,   91,    7,   98,   98,   96,   99,   99,  100,
  100,   97,   97,  101,  101,  103,  102,  102,  102,  102,
  102,  102,  102,  102,  102,  102,  102,  102,  102,  102,
  102,    5,  109,  110,  104,  106,  106,  106,  106,  106,
  106,  106,  106,  106,  106,  106,  106,  106,  106,  106,
  106,  106,  106,  106,  106,  106,  106,  106,  106,  112,
  112,  112,  112,  112,  112,  112,  112,  112,  112,  112,
  112,  112,  112,  112,  112,  112,  111,  111,  111,  111,
  111,  111,  111,  111,  111,  111,  111,  111,  111,  111,
  107,  107,  107,  107,  107,   61,   61,  113,  113,  108,
  108,  108,  114,  114,  114,  105,  105,  115,  115,  115,
  115,  115,  115,  115,  115,  115,  115,  115,  115,  115,
  115,  115,  115,  115,  115,  115,  115,  115,  115,  115,
  119,  116,  117,  120,  120,  120,  121,  121,  122,  122,
  122,  122,  124,  124,  124,  123,  123,  123,  125,  125,
  118,  118,  118,  118,  118,  118,  118,  118,  118,  118,
  118,  118,  118,  118,  118,  118,  118,  118,  118,  118,
  118,  126,  126,  130,  130,  130,   33,   33,  128,  128,
  132,  132,  133,  127,  127,   59,   59,   59,   62,  134,
  134,  129,  129,   56,  135,  135,  137,  137,  137,  136,
  136,  138,  138,  138,  138,  138,  138,  138,  138,   57,
  139,  141,  141,  141,  140,  140,  142,  142,  142,  142,
  142,  142,  142,   17,   17,   34,   34,   34,  144,   34,
   35,   35,   35,  145,   35,  143,  146,  146,  146,  147,
  147,  147,  150,  150,  150,  150,  150,  150,  148,  148,
  148,  148,  148,  148,  148,  148,  148,  148,  148,  148,
  148,  148,  148,  148,  148,  148,  148,  148,  148,  148,
  148,  154,  148,  155,  148,  157,  148,  159,  148,  160,
  148,  161,  148,  162,  148,  163,  148,  148,  148,  148,
  148,  148,  151,  151,  151,  152,  152,  152,  153,  153,
  131,  131,  156,  156,  158,  158,  164,  164,  165,  165,
  165,  166,  166,  166,  166,  167,  167,  168,  168,   16,
   16,   16,   16,   16,   16,  169,  169,  169,  169,  169,
  169,  169,  169,  169,  169,  169,  169,  169,  169,  169,
  171,  171,  172,  172,  170,  170,  173,  174,  174,  174,
  174,  174,  175,  175,  175,  175,  149,  149,   15,   15,
   15,   10,   10,  176,  176,  177,  177,   11,  178,  180,
  180,  180,  180,  180,  180,  180,  180,  180,  180,  179,
  179,  182,  182,  181,  181,  181,  181,  181,  181,  181,
  181,   12,  183,  183,  184,  184,  185,  185,  185,  185,
  185,  185,  185,  185,  185,   13,  188,  188,  186,  189,
  189,  189,  189,  189,  189,  189,  189,  189,  189,  187,
  187,  190,  190,  190,  190,  190,  190,  190,   14,  191,
  191,  193,  193,  193,  192,  192,  192,  194,  194,  194,
  194,   63,   63,   64,   80,   80,   18,   19,   93,   93,
   93,   93,   93,  195,  196,   95,   26,   26,  197,  197,
   94,   94,   28,   28,   38,   31,   31,   31,
  };
   static readonly short [] yyLen = {           2,
    1,    0,    2,    1,    1,    1,    1,    1,    1,    1,
    1,    1,    1,    1,    1,    1,    1,    1,    2,    2,
    3,    2,    2,    4,    1,    1,    1,    1,    1,    1,
    1,    3,    2,    5,    4,    7,    6,    7,    6,    9,
    8,    3,    2,    4,    6,    5,    7,    0,    2,    2,
    2,    2,    2,    1,    4,    4,    4,    4,    4,    1,
    2,    3,    2,    2,    2,    1,    1,    2,    1,    4,
    2,    4,    0,    0,    8,    0,    2,    2,    3,    3,
    3,    3,    3,    3,    2,    2,    2,    2,    2,    2,
    2,    2,    2,    2,    2,    2,    2,    2,    2,    2,
    5,    0,    2,    0,    1,    0,    2,    3,    0,    3,
    1,    3,    3,    0,    2,    2,    2,    2,    2,    1,
    0,    3,    1,    3,    0,    2,    1,    1,    1,    1,
    1,    1,    1,    1,    1,    1,    2,    2,   13,    4,
    1,    1,    3,    5,    1,    3,    4,    4,    4,    5,
    1,    1,    1,    1,    1,    1,    2,    1,    2,    2,
    3,    4,    2,    2,    2,    5,    5,    7,    2,    2,
    2,    3,    3,    1,    1,    1,    2,    3,    2,    2,
    1,    2,    1,    2,    1,    2,    1,    2,    1,    2,
    1,    1,    1,    1,    1,    1,    1,    1,    1,    1,
    1,    1,    1,    1,    0,    1,    3,    1,    3,    1,
    3,    4,    1,    1,    3,    0,    1,    1,    3,    2,
    2,    2,    1,    4,    0,    1,    1,    2,    2,    2,
    2,    0,    6,   10,    5,    5,    6,    2,    3,    4,
    6,    5,    2,    4,    1,    3,    1,    1,    2,    1,
    1,    1,    1,    1,    1,    1,    1,    1,    1,    1,
    1,    1,    1,    1,    1,    1,    1,    1,    1,    1,
    1,    1,    1,    2,    1,    2,    1,    2,    1,    1,
    1,    1,    1,    1,    1,    1,    2,    1,    1,    1,
    1,    1,    1,    1,    1,    1,    1,    1,    1,    1,
    1,    1,    1,    1,    1,    1,    1,    1,    1,    1,
    1,    1,    3,    2,    2,    2,    1,    1,    1,    1,
    1,    1,    1,    1,    1,    1,    1,    1,    1,    1,
    1,    1,    8,    0,    3,    0,    2,    2,    2,    2,
    2,    2,    2,    2,    2,    2,    2,    2,    2,    0,
    4,    0,    2,    0,    2,    4,    4,    4,    4,    4,
    4,    4,    4,    4,    4,    4,    4,    4,    4,    2,
    1,    1,    1,    2,    0,    2,    4,    0,    1,    0,
    1,    3,    1,    1,    3,    1,    5,    4,    2,    5,
    5,    5,    5,    5,    5,    2,    2,    2,    2,    2,
    2,    4,    0,    0,   14,    0,    2,    2,    2,    2,
    2,    2,    2,    2,    2,    2,    2,    2,    2,    2,
    2,    2,    2,    2,    2,    5,    8,    6,    5,    0,
    2,    2,    2,    2,    2,    2,    2,    2,    2,    2,
    2,    4,    4,    4,    4,    5,    0,    2,    2,    2,
    2,    2,    2,    2,    2,    2,    2,    2,    2,    2,
    0,    4,    4,    4,    4,    0,    1,    1,    3,    0,
    4,    4,    1,    3,    4,    0,    2,    2,    2,    4,
    5,    1,    1,    4,    6,    4,    4,    2,    1,    5,
    1,    2,    1,    1,    1,    1,    1,    1,    1,    1,
    0,    4,    2,    2,    4,    4,    1,    2,    3,    2,
    2,    2,    2,    2,    2,    1,    4,    4,    0,    1,
    1,    2,    2,    2,    2,    2,    2,    2,    2,    2,
    2,    2,    2,    2,    2,    3,    4,    6,    2,    4,
    2,    1,    1,    0,    1,    3,    2,    2,    3,    1,
    0,    2,    0,    9,    1,    1,    1,    1,   10,    0,
    5,    1,    1,    4,    4,    3,    0,    2,    2,    0,
    2,    2,    2,    2,    2,    1,    1,    1,    1,    4,
    9,    0,    2,    2,    0,    2,    2,    2,    2,    1,
    1,    1,    1,    1,    1,    2,    4,    4,    0,    7,
    5,    7,    7,    0,   10,    2,    0,    2,    2,    0,
    6,    2,    1,    1,    1,    3,    2,    3,    4,    4,
    4,    4,    4,    4,    4,    4,    4,    4,    4,    4,
    4,    4,    2,    4,    4,    5,    4,    4,    4,    7,
    7,    0,    8,    0,    8,    0,    8,    0,    8,    0,
    8,    0,    8,    0,    8,    0,    8,    7,    7,    7,
    7,    7,    0,    2,    2,    0,    2,    2,    0,    2,
    0,    2,    0,    2,    0,    2,    0,    2,    0,    2,
    2,    0,    2,    3,    2,    0,    2,    2,    2,    6,
    7,    3,    3,    3,    6,    1,    1,    1,    1,    1,
    1,    1,    1,    1,    1,    1,    1,    1,    1,    1,
    1,    3,    5,    6,    1,    3,    3,    1,    1,    4,
    1,    1,    6,    6,    6,    4,    1,    1,    1,    2,
    3,    8,    4,    0,    2,    0,    1,    4,    3,    0,
    2,    3,    2,    2,    2,    2,    2,    2,    2,    0,
    2,    1,    1,    3,    8,    2,    3,    3,    1,    1,
    1,    4,    4,    6,    0,    2,    8,    3,    3,    2,
    3,    3,    1,    1,    1,    4,    2,    1,    3,    0,
    2,    2,    2,    3,    3,    3,    3,    3,    3,    0,
    2,    2,    3,    3,    1,    2,    1,    1,    4,    3,
    5,    0,    2,    2,    0,    1,    2,    4,    3,    1,
    1,    1,    3,    4,    1,    3,    1,    1,    1,    4,
    4,    4,    4,    1,    0,    4,    0,    1,    1,    2,
    1,    1,    1,    1,    1,    1,    1,    3,
  };
   static readonly short [] yyDefRed = {            2,
    0,    0,   69,    0,    0,    0,    0,    0,    0,  778,
    0,    0,    0,   30,   31,  406,    0,  802,    0,    0,
    0,    0,    0,    0,    0,    0,   25,    0,    0,    0,
    0,    0,   66,   67,    0,    3,    4,    5,    6,    7,
    8,    9,   10,   11,   12,   13,   14,   15,   16,   17,
   18,   26,   27,   28,   29,    0,   54,  594,  595,   60,
    0,    0,    0,    0,    0,    0,    0,  780,    0,  740,
    0,  777,    0,  818,   22,  817,   20,    0,    0,    0,
  226,  227,    0,    0,    0,    0,  555,  223,    0,  379,
    0,    0,  336,    0,    0,    0,    0,  833,  834,  837,
    0,  836,    0,  730,    0,   71,  696,  697,  698,  699,
  700,  701,  702,  703,  704,  705,  706,  707,  708,  709,
  710,    0,    0,   23,   19,    0,    0,    0,    0,    0,
    0,  153,  154,  155,    0,    0,  175,  202,  191,  192,
  193,  194,  199,  200,    0,  183,  185,  187,  189,    0,
    0,  176,  201,    0,  158,  174,  203,    0,    0,    0,
    0,    0,    0,    0,  156,  152,    0,  181,  195,  196,
  197,  198,  204,    0,    0,    0,    0,   68,    0,    2,
  125,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,  374,  383,  476,  750,  765,  790,    0,    0,    0,
  743,  741,    0,  744,  745,  746,  747,  749,  748,  739,
   92,   77,   78,  100,   99,   85,   87,   88,   89,   90,
   91,   93,   94,   95,   96,   97,    0,   98,   86,    0,
    0,    0,    0,  210,  563,  562,    0,  221,  222,  228,
  229,  230,  231,    0,    0,    0,    0,    0,  381,    0,
    0,    0,   21,  735,    0,    0,  407,  408,  409,  410,
  422,  423,  421,  411,  412,  413,  414,  417,  415,  416,
  418,  419,  425,    0,  424,  420,    0,  461,  731,    0,
  803,  804,    0,    0,  815,    0,    0,    0,  693,    0,
    0,    0,  145,    0,    0,    0,    0,    0,  170,  171,
    0,  157,  180,    0,  179,  177,  184,  186,  188,  190,
    0,  547,  159,    0,  550,  548,    0,  164,  163,    0,
    0,  165,    0,    0,  169,    0,    0,    0,    0,    0,
  160,   62,   42,   32,    0,    0,    0,    0,    0,  384,
    0,    0,  386,  401,    0,  400,    0,  399,    0,  398,
    0,  396,    0,  397,    0,  825,  389,    0,    0,    0,
    0,  782,  781,    0,  783,    0,    0,    0,  810,  811,
    0,  806,    0,  742,   80,   79,   81,   82,   83,   84,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,  599,    0,  598,    0,  377,  335,    0,  344,  337,
  338,  339,  345,  346,  347,  340,  341,  342,  343,  348,
  349,    0,  737,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,  688,  689,    0,  824,    0,  829,    0,
   48,   24,    0,    0,    0,    0,    0,  172,  173,  178,
    0,    0,  161,  217,    0,    0,  214,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,   70,
   72,   76,  567,    0,    0,    0,    0,    0,  128,  127,
  131,  132,  133,  135,  134,  141,  142,  126,  129,  130,
  136,    0,    0,  382,    0,    0,    0,    0,    0,    0,
  819,    0,    0,    0,    0,    0,    0,  501,  402,  521,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,  671,    0,    0,    0,  482,    0,    0,    0,    0,
    0,    0,    0,  483,  499,  495,  498,  496,  497,  500,
    0,  491,  477,  489,  493,  494,    0,  738,    0,  752,
  753,    0,    0,  760,  759,  761,  751,    0,  762,    0,
    0,    0,    0,  773,  774,  775,    0,  766,  776,    0,
    0,    0,  797,  798,  795,  791,  784,  785,  786,  787,
  788,  789,    0,    0,  799,  807,    0,    0,  114,   74,
    0,    0,    0,  814,  224,    0,  552,  557,  556,  558,
    0,  607,  376,    0,    0,    0,    0,    0,    0,    0,
    0,    0,  801,    0,    0,  715,  607,    0,    0,    0,
  711,  816,   46,  830,    0,  146,    0,    0,    0,    0,
    0,  549,    0,  162,    0,    0,  207,    0,    0,    0,
   34,    0,    0,    0,    0,    0,  138,    0,  582,  137,
  570,  585,  385,  388,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,  476,  526,  527,  529,  528,  530,
  543,  542,  531,  532,  533,  534,    0,    0,    0,    0,
  539,    0,    0,  522,  523,  524,  525,  478,    0,    0,
    0,  479,    0,  488,    0,    0,    0,  504,    0,  492,
    0,    0,    0,    0,    0,  507,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
  796,    0,  809,    0,  764,  101,    0,  111,    0,    0,
    0,    0,  206,  553,    0,    0,    0,    0,  270,    0,
    0,    0,    0,    0,  250,  283,  282,  251,  252,  253,
  254,  255,  256,  257,  258,    0,  248,  284,  285,  263,
  264,  265,  266,  286,  267,  268,  269,    0,  271,  273,
  275,  279,  277,  259,  260,  261,  262,    0,    0,  272,
  280,    0,    0,   45,    0,    0,  429,  438,  439,  440,
  441,  432,  433,  434,    0,  431,  435,  436,  437,    0,
    0,  426,    0,    0,    0,    0,    0,    0,    0,  690,
    0,    0,    0,    0,  695,    0,    0,   49,   50,   51,
   52,   53,    0,    0,  219,  215,    0,  166,  167,    0,
    0,  568,  569,    0,    0,    0,    0,    0,    0,  143,
    0,    0,    0,  395,  394,  393,  392,    0,    0,    0,
    0,  390,  391,  387,  826,    0,    0,  520,    0,    0,
    0,    0,  672,    0,  473,    0,    0,    0,  468,    0,
    0,    0,    0,    0,    0,    0,  515,  514,  513,    0,
  516,  511,  512,  508,  510,  758,  754,    0,  757,  772,
  768,  769,    0,  771,  794,    0,  808,  114,  110,    0,
  115,  116,  117,  119,  118,    0,    0,    0,  604,    0,
  603,    0,  600,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,  609,    0,    0,    0,    0,
    0,  608,    0,  278,  274,  287,    0,    0,    0,  276,
  249,    0,  312,  288,  289,  309,  290,  291,  292,  293,
  310,  294,  295,  306,  299,  300,  301,  317,  318,  302,
  303,  319,  304,  308,  320,  321,  322,  323,  324,  325,
  326,  327,  328,  329,  330,  331,  332,  296,  297,  298,
  311,    0,  305,    0,  351,    0,  238,    0,    0,    0,
    0,  428,    0,    0,    0,  462,  463,  464,  465,    0,
  403,    0,  831,  832,  719,    0,    0,  718,  717,  722,
  716,  691,    0,  610,  712,   47,    0,   38,    0,   36,
  565,    0,    0,    0,  140,    0,  583,  584,    0,  564,
    0,    0,    0,    0,  576,  577,  578,  579,  571,  580,
    0,    0,    0,  590,  591,  592,  593,  586,  820,  821,
  823,  822,  502,  537,    0,    0,  540,    0,  480,    0,
    0,    0,  487,    0,  506,  505,  486,  509,    0,    0,
    0,    0,  112,  123,    0,  120,  113,  103,    0,   75,
    0,  607,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,  633,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,  728,  727,  612,    0,  246,    0,
    0,    0,  314,    0,    0,  316,  315,  239,    0,    0,
  353,    0,  333,  732,    0,  442,  443,  444,  445,    0,
    0,    0,    0,    0,    0,  610,    0,  168,    0,    0,
    0,    0,  144,    0,  574,  575,  572,  573,  588,  589,
  587,    0,  546,    0,    0,  469,  481,  490,    0,    0,
    0,    0,  122,    0,  107,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,  614,    0,  615,  613,    0,    0,    0,
    0,    0,  313,    0,  240,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,  373,    0,    0,    0,    0,
    0,  355,  371,  427,  446,  471,  472,  404,    0,    0,
    0,    0,    0,    0,  713,   40,    0,    0,    0,    0,
  538,  485,  475,  518,  517,    0,    0,  124,  108,  605,
  554,    0,  632,    0,  629,    0,  627,    0,  625,    0,
  623,    0,  621,  619,    0,  622,  620,    0,    0,  638,
  637,    0,  631,    0,  639,    0,  635,  634,    0,  630,
    0,  628,    0,  626,    0,  624,    0,  617,    0,    0,
    0,  235,    0,  242,    0,    0,    0,    0,    0,    0,
    0,    0,    0,  370,    0,    0,    0,    0,    0,  720,
    0,    0,    0,  726,  714,    0,    0,    0,    0,    0,
  677,  642,  644,  646,  648,  663,  666,  682,  636,  671,
  686,  679,  650,  652,  654,  656,  616,  618,    0,  233,
    0,  237,  241,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,  755,  767,    0,  669,  671,
  673,  675,    0,    0,    0,    0,    0,    0,  669,  671,
  673,  675,  611,    0,  369,  366,  364,  362,  360,  358,
  356,  359,  357,  368,  367,  365,  363,  361,    0,  723,
  724,  725,    0,    0,    0,  581,  659,  678,    0,    0,
    0,    0,  640,  665,  664,  641,  668,  667,  661,    0,
  683,  685,  658,  662,  687,  681,  660,  680,    0,    0,
    0,    0,    0,  447,    0,    0,    0,  643,  670,  645,
  647,  674,  649,  676,  684,  651,  653,  655,  657,    0,
    0,    0,  559,    0,  234,  453,  448,  450,  449,  451,
  452,  454,  456,  457,  458,  459,  460,  455,  561,    0,
  139,
  };
  protected static readonly short [] yyDgoto  = {             1,
    2,   36,   37,   38,   39,   40,   41,   42,   43,   44,
   45,   46,   47,   48,   49,   50,   51,  853, 1454,  292,
   52,   53,   54, 1107,   56,  428,  615,  102,   57,  233,
  159,  234,  235,   58,   59,   60,   61,  590,   62,  338,
   73,  580,  382,  898,  720, 1070,  390, 1071,  717,  718,
  719,  896, 1067, 1065,  478,  479,  480,  532,  591,   86,
  856,  636,  164,  165,  166,  167,  446,  325,  168,  169,
  170,  171,  172,  173,  724,  449,  447,   88,  768,  604,
  769,  972,  770,  771,  974,  343,  252,  412,  979, 1123,
 1232, 1233,  494,  998,  357,   63,  192,  396,   91,  250,
  339,  193,  344,   64,  358,   97,  857,  798, 1133, 1319,
 1461,  599,  858,  859,  533,  871,  535,  536,  655,  537,
  695,  696,  872,  697,  849,  851,   89,  316,  237,  852,
  673,  391,  902, 1373,  482,  832,  633, 1029,  483,  833,
  831, 1038,  725,  592, 1072,  726,  921,  922, 1108, 1208,
 1383, 1384, 1419, 1379, 1380, 1421, 1381, 1422, 1382, 1389,
 1390, 1391, 1392, 1378, 1388, 1385, 1387,  289,  122,  605,
  610,  611,  606,  999, 1000,   95,  414,   65,  359,   71,
  547,  548,   66,  360,  558,   67,  361,   68,  198,  566,
   69,  371,  105,  372,  429,  497,  430,
  };
  protected static readonly short [] yySindex = {            0,
    0, 9819,    0, -334, -306, -156, -108,  -99, -331,    0,
  -68, -296,   23,    0,    0,    0,  -54,    0,  476, 2411,
 2411, -156, -108,   -9,   37,   56,    0, 7730,  476,  476,
  476,  476,    0,    0,  113,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0, -108,    0,    0,    0,    0,
  127,  149,  780,  155,  181,  188,  200,    0,  215,    0,
  154,    0,  509,    0,    0,    0,    0, 7806,  292,  292,
    0,    0,  319,  226,  285, 8360,    0,    0,  225,    0,
   93, -108,    0, -108, -164,  327, 8724,    0,    0,    0,
  476,    0,  300,    0,  334,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0, 7881, -146,    0,    0,  336, -108,  370, -180,  358,
 8360,    0,    0,    0,  126,  118,    0,    0,    0,    0,
    0,    0,    0,    0,  340,    0,    0,    0,    0,  452,
  370,    0,    0, 7600,    0,    0,    0, 1370,  300,  276,
  279,  289,  346,  428,    0,    0,  370,    0,    0,    0,
    0,    0,    0,  140,  300,  300,  300,    0,  129,    0,
    0,  749,  449, -203, -131,  277,  410,  539,  542,  472,
  460,    0,    0,    0,    0,    0,    0, -215, 2978,  154,
    0,    0,  228,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,  544,    0,    0,  536,
  300, -130,  368,    0,    0,    0,  501,    0,    0,    0,
    0,    0,    0, -108, -108,  292, 7461,  112,    0,  538,
  549, 1509,    0,    0, -133,  581,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,  579,    0,    0,  602,    0,    0,  476,
    0,    0,  510,  133,    0, -147,  460,  618,    0,  646,
  661,   34,    0,  675,  476,  157,  677,  402,    0,    0,
  368,    0,    0,  479,    0,    0,    0,    0,    0,    0,
 8360,    0,    0, 7461,    0,    0,   10,    0,    0, 8360,
  476,    0,  653,  682,    0,  476,  476,  476,  476,  476,
    0,    0,    0,    0, -108, -108, 9641, 9705,  169,    0,
  538, -108,    0,    0, -108,    0, -108,    0, -156,    0,
 -205,    0, -205,    0,  683,    0,    0, 8995, 7485, 8173,
  632,    0,    0,  750,    0,  300,  575,  476,    0,    0,
 3196,    0,  591,    0,    0,    0,    0,    0,    0,    0,
 -108,  668,  476,  195,  452,  691,  692, 8360,  -67,  690,
  174,    0,  618,    0,  688,    0,    0,  696,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0, 8360,    0,  658,  718,  733, -108, 3970,  300,  476,
  733,  722, -102,    0,    0,  745,    0,  736,    0,  646,
    0,    0,  370,  476,  735,  476,  476,    0,    0,    0,
 7524,  476,    0,    0,  727,   63,    0,  368, -136,  300,
 7881, 7881,  300,  300,  300,  300,  300,  738,  -85,    0,
    0,    0,    0, 7958, -108,  514,  292, -108,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,  754,  763,    0,  749,  765,  767,  768,  769,  770,
    0,  775,  777,  779,  781,  733,  646,    0,    0,    0,
 -108, -156,  -32,  402,  452, 7600, 7881, -218,  292, 7806,
  778,    0,  402,  402, -108,    0,  783, -241, -108, 7958,
 -240,  394, -108,    0,    0,    0,    0,    0,    0,    0,
  788,    0,    0,    0,    0,    0,  324,    0,  521,    0,
    0,  785, -108,    0,    0,    0,    0, -183,    0,  789,
  791,  793, -108,    0,    0,    0, -125,    0,    0,  684,
 -145,  476,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,  476,  686,    0,    0,  476,  801,    0,    0,
  807,  476,  803,    0,    0,  371,    0,    0,    0,    0,
  792,    0,    0, 8545, 1137,  805,  826, -210, 1508,  816,
 -221,  252,    0,  238,  413,    0,    0,  830,  818,  295,
    0,    0,    0,    0,  109,    0,  428,  476,  428,  428,
  823,    0, -108,    0, -137, 8360,    0,  827,  829, -108,
    0, -108, 7746,  292,  831,  596,    0,  267,    0,    0,
    0,    0,    0,    0,  -68,  -68,  -68,  -68,  432,  440,
  -68,  -68,  -30,  833,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,  733,  835,  618, 8360,
    0,  402, -108,    0,    0,    0,    0,    0, -108,  825,
  836,    0,  840,    0, -108,  611,  612,    0,  851,    0,
 7881,  394, -229, -229,  324,    0, -229, -108,  460,  854,
  460,  618,  460,  460,  460,  867,  460,  618,  476,  476,
    0,  300,    0, -108,    0,    0,    1,    0,  -21,  729,
  476,  242,    0,    0,  869, 8099, -108,  672,    0,  689,
  679,  884,  305,  710,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,  345,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0, 8605,    0,    0,
    0,    0,    0,    0,    0,    0,    0,  886,  -36,    0,
    0,  795,  460,    0,  733, 1827,    0,    0,    0,    0,
    0,    0,    0,    0,  887,    0,    0,    0,    0,  888,
  889,    0,  895,  897,  898,  899,  900,  174,  231,    0,
  733,  901,  902,  911,    0, -102,  538,    0,    0,    0,
    0,    0,  428,  825,    0,    0,  368,    0,    0,  -63,
  919,    0,    0,    0,  476, 8360,  174,  693, -108,    0,
 8208, 2517, 2836,    0,    0,    0,    0,  910,  912,  913,
  916,    0,    0,    0,    0, 9078,  618,    0,  460,  863,
  917,  920,    0,  921,    0,  928, 3970,  930,    0,  825,
  174,  923, -108,  538, -108, -229,    0,    0,    0,  402,
    0,    0,    0,    0,    0,    0,    0, -108,    0,    0,
    0,    0, -108,    0,    0,  300,    0,    0,    0, 7881,
    0,    0,    0,    0,    0,  476, 7881,  773,    0,  618,
    0,  934,    0,  547,  551,  554,  573,  582,  585,  589,
  590,  600,  460,  603,  624,    0,  625,  633,  636,  637,
  608,    0,  939,    0,    0,    0,  733,  937,  941,    0,
    0,  942,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,  938,    0, -178,    0,  -86,    0,  538,  944,  918,
  618,    0,  251,  293, -108,    0,    0,    0,    0, 8545,
    0,  953,    0,    0,    0,  954,  618,    0,    0,    0,
    0,    0,  959,    0,    0,    0,  963,    0, -108,    0,
    0, 7524,  730,  292,    0,  967,    0,    0, 1111,    0,
  452,  452,  452,  452,    0,    0,    0,    0,    0,    0,
  452,  452,  452,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,  825,  402,    0,  868,    0,  252,
  825,  969,    0,  944,    0,    0,    0,    0,  737,  740,
  981,  982,    0,    0,  485,    0,    0,    0, 7881,    0,
  987,    0,  825, -108,  233, -108, -156, -108, -156, -108,
 -156, -108, -156, -108, -231, -108, -231, -108,  -35, -108,
 -156,    0, -108, 1742, -108, -196, -108, -156, -108, -156,
 -108, -156, -108, -156,    0,    0,    0, 2064,    0,   73,
 -108, -108,    0,  733,  992,    0,    0,    0, -108,  -61,
    0,  877,    0,    0, 2146,    0,    0,    0,    0,  991,
  996,  205,  668, -108, -149,    0,  376,    0, 1010, 1002,
  292, 8360,    0, 1007,    0,    0,    0,    0,    0,    0,
    0, 1009,    0,  538,  538,    0,    0,    0, -108,  538,
 -108, -108,    0, 7881,    0, 7881, 1011, 1014, 1018, 1019,
 1020, 1021, 1022, 1024, 1035, 1034, 1037, 1036, 1039, 1038,
 1040, 1041, 1044, 1045, 1048, 1059, 1056, 1061, 1050, 1073,
 1080, 1079, 1082, 1081, 1083, 1084, 1093, 1086, 1098, 1103,
 1102, 1106, 1107,    0,  -45,    0,    0,  414,  733, 1108,
 1113,  618,    0, 1114,    0, -108, 1110, 1115, 1116, 1117,
 1118, 1121, 1122, 1123,  460,    0, 1125, 1128, 1129, 1130,
  618,    0,    0,    0,    0,    0,    0,    0, 1131, 1133,
 1135, 1136, 1132,  660,    0,    0,  174, 8360, 7461,  825,
    0,    0,    0,    0,    0, 1138, 1144,    0,    0,    0,
    0, 1150,    0, 1151,    0, 1152,    0, 1158,    0, 1159,
    0, 1161,    0,    0, 1170,    0,    0, 1172, 1176,    0,
    0, 1181,    0, 1182,    0, 1228,    0,    0, 1256,    0,
 1267,    0, 1268,    0, 1269,    0, 1281,    0, 1277, 1274,
  325,    0, 8768,    0, 1287,  233, -156, -156, -156, -156,
 -231, -231, -156,    0, -156, -156, -156, -156, 1288,    0,
 -108, -108, -108,    0,    0, 1289, 7461, 1290, -108, -108,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0, 1742,    0,
  733,    0,    0, 1291, 1292, 1293, 1294, 1295, 1297, 1298,
 1299, 1300, 1302, 1303, 1304, 1305, 1306,  825, 1307, 1308,
 1309, 1273, 1313, 1286,  944,    0,    0, -193,    0,    0,
    0,    0, -160,   33,  -31,  -11, 1096, -198,    0,    0,
    0,    0,    0,   88,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0, 1314,    0,
    0,    0, -108,  825,  174,    0,    0,    0,    9,  128,
  175,  208,    0,    0,    0,    0,    0,    0,    0, 1325,
    0,    0,    0,    0,    0,    0,    0,    0,  232,  307,
  360,  364,  733,    0, 1097, 1316, 1320,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,  401,
  552, 1322,    0,  825,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0, 1321,
    0,
  };
  protected static readonly short [] yyRindex = {            0,
    0, 1598,    0,  170, 1329,    0,    0, 8420, 4227,    0,
 8225, -150,    0,    0,    0,    0, 2012,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0, 8027, 8027,
    0,    0,    0,    0,    0,    0,    0,    0, 5065,    0,
  914,    0,    0,    0,    0, 6085, 4237,    0,    0,    0,
    0,    0, 5583,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0, 8420,
    0,    0,    0,    0,    0,    0,    0,    0, 1494,    0,
    0,    0,    0, 2132,    0,    0,    0,    0,    0,    0,
    0,    0,    0, 3997, 4264, 4531, 4798,    0, 6301,    0,
    0,    0,    0, 7064, 7064, 7064, 7064, 7064, 7064,    0,
    0,    0,    0,    0,    0,    0,    0,    0, 1330,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
  146,    0, 5699,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0, 8420,  280,    0,    0, 1477,
    0, 8480,    0,    0, 1693,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0, 1332, 6412,    0,    0,    0, 6524,    0, 1328,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
 3727,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0, 1331,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0, 1335,    0,    0,    0,    0,
    0,    0, 1336,    0,    0,    0,    0,    0,    0,    0,
    0,  -75,    0,    0, 8420,    0,    0,    0, 1331,    0,
    0,    0, 5183,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0, 2331, 6199, 2465,    0,    0, 1167,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0, 1333,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
  280,    0,    0,    0,  451,    0,    0,   30,    0, 2650,
    0,    0, 2969, 3288, 3607, 3884, 1813,    0, 6626,    0,
    0,    0,    0,    0,    0,    0, 8344,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0, 1328,    0,    0,    0,
    0,    0,    0,    0, 8420,    0,    0,    0, 8420,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0, 5829, 5311,    0,    0,    0,    0,    0,    0,    0,
 1338,    0,    0, 1339,    0,    0,    0, 2465,    0,    0,
    0,  457,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0, 2451,    0, 2770, 3089,
 7445,    0,  502,    0,  527,    0,    0,    0,    0,    0,
    0,    0,    0, 8420,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0, 7064, 7064, 7064, 7064,    0,    0,
 7064, 7064,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0, 1347, 9161,    0,
    0, 1349, 9244,    0,    0,    0,    0,    0,    0, 4501,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0, 9327,    0,    0,    0,    0,    0,
    0, 9754,    0,    0,    0,    0,    0, 8838,    0,    0,
    0,  652,    0,    0,    0,    0,    0,    0,  519, -152,
 5957,    0,    0,    0,    0, -163,    0,    0,    0,    0,
    0,    0,    0,  312,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0, 1351,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0, 1356,    0,
    0,  936,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0, 3408, 4501,    0,    0,   62,    0,    0, 6739,
 6851,    0,    0,  494,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0, 9410,    0,    0,    0,
 1357,    0,    0,    0,    0,    0,    0, 1358,    0, 4501,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,  853,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,   89,    0, 5453,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
  366,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0, 1359,    0,  617,    0,    0,    0,    0, 7242, 7334,
 2465,    0,    0,    0,    0,    0,    0,    0,    0, 1339,
    0,    0,    0,    0,    0,    0,  654,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0, 8420,    0,    0,    0,    0,    0,    0,
 8420, 8420, 8420, 8420,    0,    0,    0,    0,    0,    0,
 8420, 8420, 8420,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0, 4501, 1349,    0, 9493,    0,  268,
 4768,    0,    0, 9576,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
 1343,    0, 4501,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0, 1364,    0,    0,    0,    0,    0, 6953,    0,
 8420,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,  671,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0, 1365,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
 7176,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0, 4501,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0, 1366,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0, 1369,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0, 4501,    0,    0,
    0,    0,    0,    0, 1374,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0, 4501,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
 1375,    0,    0, 4501,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,
  };
  protected static readonly short [] yyGindex = {            0,
 1457,    0, 1310,    0, 1311, 1315, -274,    0,    0,    0,
    0,    0,    0,    0,    0, -288, -189,   -7,   32,    0,
 -302, -280,    0,    7,    0, 1147,    0,  -95,    0,  -24,
  -12,  -27, 1619, 1624, 1626,    0,    0,   57,    0,    0,
    0,  522,    0,    0,    0,    0,  -65,    0,    0,  771,
    0,    0,    0,    0,    0,    0,    0, 1318, -795,  -56,
 -787, -495, -346,   13, 1296,    0,    0, 1069,  561, -549,
 -546, -543, -539,    0,    0,    0, 1046,    0,  680, -111,
 -964,    0,  922,    0,    0, 1661,    0,    0,    0,-1032,
    0,    0, -351,-1047, -234,    0,    0,    0,    0,    0,
    0, -153,  -42,    0, 1023,    0, 1397,  626,    0,    0,
    0, -590,    0,  628,    0, -343,    0,    0,    0,    0,
    0,  990, -595,    0,    0, 1173, -144, 1180, 1177,  642,
-1254, 1368,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0, -596,    0,    0,    0, -886,-1057,    0,    0,
    0,    0,  301,    0,    0,  298,    0,  299,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0, 1671,    0,
    0,  890,  893,    0,    0,    0,  715,    0,    0, 1627,
    0, 1340,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0, 1327, 1271,    0,    0,
  };
  protected static readonly short [] yyTable = {            77,
  160,  495,  991,  158,  103,  312,  103,  776,   55,  369,
  802,  288,  236,  394,  534,  125,  174,  175,  176,  177,
   87, 1158,  238,  239,  684, 1132, 1007, 1170,  340,  685,
  680, 1013,   74,  491,  300,  475, 1192,   75,  498,   70,
  278,  285,   76,   98,  764,   99,  100,  765,  179,  473,
  766,  424,  425,  124,  767,  528,  284,  476,  103,  491,
  231,  247, 1436,  472, 1194, 1053,   92,   72,  342,  526,
  544,  426, 1052,  104, 1437,  106,  285,  529,   98, 1417,
   99,  100,  255,  525,  251, 1386,  253,  617,  103,  619,
  620, 1115,  103,  311,   98,  701,   99,  100,  873,  294,
  293,  875, 1116,   76,  491,  610,  301,   74,  734,  610,
  734,  734, 1423,  285,   76,  102,  296, 1137,   76,  291,
  423,  102,  299,  313,  356, 1420,   76,  210,   98,  314,
   99,  100,  286,  280,  285, 1440,  393,  626,   92,  331,
  345,  297,  346,  348,  350,  352,  354,  444,  474,  294,
  295,  659,  627,  707,  395,   76,   98,  279,   99,  100,
  775,  283,   87,  793,  794,  795,  315,  129,  527,  545,
  555,  563,   78,  362,  363,  631,  917,   76,  688,  918,
  413,  369,  919,   90, 1118,  366,  920,  103,  632,  388,
  667,   98,  109,   99,  100, 1119,   76, 1008,  109,  364,
  383,   92,  439,  443,   98,  370,   99,  100,  968, 1215,
 1009,  969,  294,   98,  970,   99,  100,  444,  971,  384,
 1216,  492,  493,   98,  129,   99,  100,   98,  710,   99,
  100,   74,  491,  976,  129,  619,  386,  387,  129,  356,
  583, 1429,  844,  977,  297,  486,   94,  492,  493, 1244,
  890,  426,   76,  132,  133,  134,  373, 1152, 1354,  638,
  891, 1433,  531,  383,  102,  892,  681,  419,  660,  126,
 1058,  813,   76,   76,  888,   79,   80,   81,   82,   83,
  443, 1448,  103,   96,  870, 1168,  441,  668,  893,  889,
  438, 1393,  492,  493,  444,  448,   74,  491, 1240, 1241,
 1242, 1117,  432,  208,  598, 1426,  127,  433,  450,  445,
  132,  133,  134,  453,  454,  455,  456,  457,  208,  101,
  132,  133,  134,  128,  132,  133,  134,  458,  459, 1435,
 1418,  643, 1015,  624,  487,  209,  625,  488, 1352,  489,
  109,  109, 1416,   55,  477, 1438, 1209, 1195,  869,  365,
  209,  435,  610,  610,  426,  103,  104,  993,  994,  287,
  664, 1443,  106,  586,  530,  546,  556,  564,  254,  426,
  103,  285,  178,  578,  813,   84,  384,  370,  103,  392,
  490,  445,  734,  356,  653,  628,  629,  595,  333,  334,
 1125,   76,  608,  602,  180,  609,  669,   87,  635,  332,
 1450,  297,  335,  336,  421,  616,  280,  103,  662,  600,
  639,  422,   98,   73,   99,  100,  181,  675,  677,   73,
  492,  493,  194,  280,  574,   84,  687,  436,  740,  103,
  740,  740,   98,   73,   99,  100,  702,  484,   76,  581,
  764,  666,  485,  765,  236,  708,  766, 1451,  195, 1297,
  767, 1326,  670,   84,  683,  196,   84,  637,   85, 1186,
  640,  280, 1328, 1430,  877,  582,  879,  197,  880,  881,
  882,   74,  884,  894,  976, 1167,  603, 1237,  807,  588,
 1453,  314,  199,  589,  977,  492,  493,  901,   84,   98,
  285,   99,  100,  656,   76,   76,  661,  244,  622,  772,
  129,  285,  534,  248, 1456,  674,  676,  678, 1187,  899,
   84,  682, 1431,  356,  686,  689,  799,   87,  315,  426,
   84,  317,  249,  895,   84,   98,  470,   99,  470, 1026,
 1035,  318,  319,  657,  658,  700,  829,  824,  980,  320,
  470,  470,  830,  528,  917,  706,   92,  918,  347,  712,
  919, 1027, 1036,  711,  920,  847,  245,  526,  808,  809,
  103,   73,   73,  805,  103,  529,  280,  825,  806,  303,
   76,  525, 1227,  304,  305, 1228,  662,  826, 1229, 1457,
 1409,  281, 1230,  201,  281,  551,  132,  133,  134,  551,
  306,  281,   98,  796,   99,  100,  868, 1350, 1351,  740,
  256,  817,  834,  835,  836,  837,  426,  290,  842,  843,
  900,  810,  811,  812, 1044,  815,   98,  445,   99, 1447,
  302,   76,  820,   76,  821,  866, 1446,   74,   98,  713,
   99,  100, 1458,  715,  606,  797, 1459,  317,  606,  129,
  317,  298, 1025, 1034, 1245,  850,  326,  318,  319,  327,
  621,  319,   98,    3,   99,  320,  527,   76,  320,  328,
   98,  498,   99,  981,  661,   76,   79,   80,   81,   82,
   83,  854,   98, 1465,   99,  100, 1480,  862, 1092,   92,
  992,  349,  426, 1299,  867,  800,  801,  997,  202,  203,
  876,  204,  205,  206,  838,  839,  103,  886,  240,  241,
  242,  243,  840,  841,  740,  740,  887,  740,  740,  740,
  330, 1006,  207,  208,  209,  470,  329,  470,  470,  923,
  341,  218,  281,  282,  218,  132,  133,  134,  740,  740,
  740,  356,  916, 1181,   98, 1184,   99,  100,   29,   30,
   31,   32,   33,   34,   35,  928,  929,  322,  323,  324,
  531,  355,  836,  764,  836,  836,  765, 1163, 1164,  766,
  836,  566,  470,  767,  374,  885,  470,   98, 1056,   99,
  100,  996,  220,  385, 1060,  220,  836,  121,  609,  121,
  121,  993,  994,  993,  994,  103,   84,  307,  308,  309,
  310,  995,  307,  308,  309,  310,   98,  216,   99,  917,
  216, 1012,  918, 1126, 1127,  919, 1019,  381,   92,  920,
  351,   92,  103,  353,  103, 1110, 1074,  931, 1075,  397,
 1076, 1016, 1077, 1078, 1064, 1079,   79,   80,   81,   82,
   83, 1068, 1050,  691,  692,  693,  694,  917, 1028, 1037,
  918,  415, 1080,  919, 1081, 1128, 1129,  920,  103,   85,
  416, 1082,  530, 1083, 1084, 1055, 1085, 1057, 1086, 1088,
 1087, 1089, 1059,  322,  323,  324,  322,  323,  324, 1090,
 1061, 1091, 1093,  417, 1094, 1062, 1145, 1146, 1147, 1148,
  420, 1011, 1121,  103,  211,    3, 1149, 1150, 1151,  307,
  307, 1105, 1106, 1095, 1097, 1096, 1098,  212,  213,  426,
  559,  214, 1099,  215, 1100, 1101, 1103, 1102, 1104,    3,
  216,  427,  217,  218,  219,  220,  221,  222,  223,  224,
  792,  225,  226,  227,  451,   84,  721,  721, 1325,  792,
 1466,  431,  375,  376,  377,  352,  560,    3,  561,  378,
  379,  380,    8,  474,  474,  434, 1140,  437,  573,  562,
  662,  440, 1066,  452,  496,  579,  792, 1142,  792, 1360,
 1362,  577,  792,  584,  585,  587,  593,  594, 1120,  792,
   29,   30,   31,   32,   33,   34,   35, 1130,  596, 1467,
 1468, 1469, 1470, 1471, 1472, 1473, 1474, 1475, 1476, 1477,
 1314,  597,  285,  607,   29,   30,   31,   32,   33,   34,
   35, 1139, 1212, 1165,  612,  618,  103,   84,  613,   85,
 1231,  623,  630,  638,  792,  792,  792,  792,  792,  792,
  792,  641,   29,   30,   31,   32,   33,   34,   35,  183,
  642, 1425, 1428,   87,   87,   87,   87,  644,  661,  645,
  646,  647,  648,   87,   87,   87,  649,  182,  650,  672,
  228,  651,  679,  652,  698,  714,  229,  709, 1252, 1253,
  183, 1188,  690,  699, 1255,  230, 1169,  703, 1171,  704,
 1173,  705, 1175,  716, 1177, 1144, 1179,  721, 1182,  320,
 1185,  722, 1189,  773, 1248, 1191,  774, 1193,  792, 1196,
  803, 1198, 1478, 1200,  814, 1202,  804, 1301, 1258,  818,
 1259,  819,  828, 1210, 1211,  845,  827,  860, 1172,  855,
 1174, 1214, 1176,  848, 1178,  861, 1180, 1249, 1183,  863,
  864,  793, 1190, 1105, 1106,  865, 1239, 1243,  878, 1197,
  793, 1199,  317, 1201, 1045, 1203,  285,  903,  567,  568,
  569,  883,  318,  319,  897,  570,  571,  572,  924,  926,
  320, 1254,  925, 1256, 1257,  927,  930,  793,  975,  793,
  985,  983,  984,  793,  978,  986,  838,  987,  988,  989,
  793,  990,  380, 1002,  380, 1105, 1106, 1298, 1004, 1010,
 1003,  380, 1039, 1140, 1040, 1041, 1014,   84, 1042, 1069,
 1046, 1048, 1047, 1054,  380,  103,  184,  185,  186,  187,
 1049,  188,  189, 1051,  352, 1073, 1111,  792, 1305, 1109,
 1112, 1114, 1113,  352,  352,  793,  793,  793,  793,  793,
  793,  793, 1122, 1327, 1134, 1135, 1136,  184,  185,  186,
  187,  413,  188,  189,  103, 1138, 1141, 1143, 1154, 1394,
  352, 1157,  352,  352,  352, 1159,  352,  352, 1160,  190,
  352,  352,  352,  352,  191, 1161, 1162,  352,  352,  352,
 1166, 1374, 1213, 1235, 1300,  352,  352,  352, 1236,  352,
 1246,  352,  352,  352,  352,  352,  352, 1247, 1250, 1260,
  190, 1251,  352,  352,  352,  191, 1261,  352, 1262,  352,
 1264, 1263, 1266, 1265,  352,  352, 1267,  352,  352,  352,
  352,  352,  352,  352,  352, 1268, 1269, 1270, 1271, 1272,
 1273, 1275, 1274, 1369, 1370, 1371, 1276, 1277, 1278, 1279,
 1282, 1376, 1377, 1217, 1218, 1219, 1220, 1221, 1280, 1222,
 1223, 1460,  145, 1281,  146,  147,  148,  149, 1355, 1356,
 1357, 1358, 1359, 1361, 1363, 1283, 1364, 1365, 1366, 1367,
 1284, 1285, 1286, 1287, 1289, 1288, 1291, 1432,  322,  323,
  324,  380,  380,  380,  380, 1290,  380,  380, 1434,   98,
 1292,   99,  100, 1293, 1294, 1424, 1295, 1224, 1302, 1296,
  317, 1306, 1225, 1303, 1304, 1462, 1307, 1308, 1309, 1310,
  318,  319, 1311, 1312, 1313,   98, 1315,   99,  320, 1316,
 1317, 1318,  103, 1320, 1324, 1445,  317, 1321,  793, 1322,
 1323, 1449, 1329, 1452,  380, 1427,  318,  319, 1330,  380,
 1226, 1331, 1332, 1333,  320,  838,  838,  838,  838, 1334,
 1335, 1449, 1336, 1452,  838,  838,  838,  838,  838,  838,
  838, 1337,  838, 1338,  838,  838,  838,  838, 1339,  838,
  838,  838, 1340, 1341,  838,  838,  838,  838,  838,  838,
  838,  838,  838,  838,  838,  838,  838,  838,  838,  838,
  838,  838,  838,  838,  838,  838,  838,  838,  838,  838,
  838,  838,  838,  838,  838,  838,  838,  838,  838,  838,
  838,  838,  838,  812,  838,  838,  838,  838,  838, 1342,
  838,  838,  838,  838,  838,  838,  838,  838,  838,  838,
  838,  838,  838,  838,  838,  838,  838,  838,  838,  838,
  838,  838,  838,  838,  838,  838,  838, 1343,  838,  838,
  838,  838,  838,  838,  838,  838,  838,  838, 1344, 1345,
 1346, 1347,  904,  905,  906,  907,  908, 1348,  909,  910,
  838,  145, 1349,  146,  147,  148,  149, 1353, 1372, 1368,
 1413, 1415, 1375, 1395, 1396, 1397, 1398, 1399,  838, 1400,
 1401, 1402, 1403,  838, 1404, 1405, 1406, 1407, 1408, 1410,
 1411, 1412,  838,  838, 1414, 1455, 1444,   76, 1463,   76,
   76, 1464, 1479, 1481,  838,  911,  912,    1,  805,  800,
  827,  913,  779,  763,  216,  828,  322,  323,  324,  205,
  105,  232,  838,  838,  838,  838,  838,  838,  519,  838,
  838,  544,  838,  245,  838,  838,  838,  838,  247,  545,
  467,  243,  322,  323,  324,  109,  337,  244,  236,  317,
  560,  354,  405,  654,  914,  915,  161,  469,  470,  318,
  319,  162,  471,  163, 1238,  481,  565,  320, 1063,  723,
  838,  838,  838,  838,  838,  838,  838,  838, 1207, 1131,
  816,   93,  838,  838,  418, 1155,  663,  846, 1156,  973,
  838,  442,  838,  838,  874,  665,  671, 1153, 1441, 1439,
 1442,  123,  736, 1001, 1124, 1005,  200,  576,    0,  557,
  614,    0,    0,    0,   76,    0,    0,    0,    0,    0,
  838,  838,    0,    0,    0,  838,  838,   76,   76,    0,
    0,   76,  838,   76,    0,    0,    0,    0,    0,    0,
   76,    0,   76,   76,   76,   76,   76,   76,   76,   76,
  321,   76,   76,   76,  375,    0,    0,    0,    0,    0,
    0,    0,  812,    0,  812,  812,    0,  375,    0,    0,
    0,  812,  812,  812,    0,  812,  812,  812,    0,  812,
    0,  812,  812,  812,  812,    0,  812,  812,  812,    0,
  777,  812,  812,  812,  812,  812,  812,  812,  812,  812,
  812,  812,  812,  812,  812,  812,  812,  812,  812,  812,
  812,  812,  812,  812,  812,  812,  812,  812,  812,  812,
  812,  812,  813,    0,  812,  812,  812,  812,  812,  812,
    0,  812,  812,  812,  812,  812,    0,  812,    0,  812,
  812,  812,  812,  812,  812,  812,    0,    0,    0,    0,
  812,  812,  812,  812,  812,  812,  812,  812,  812,  812,
  812,  812,  812,  812,    0,  812,  812,  812,  812,  812,
  812,  812,  812,    0,  812,  322,  323,  324,    0,    0,
   76,    0,    0,    0,    0,    0,   76,  812,    0,    0,
    0,    0,    0,    0,    0,   76,    0,  778,  779,  780,
  781,    0,  398,    0,    0,  812,  399,  400,  401,  402,
  403,  404,    0,  405,  406,  407,  408,  409,  410,  411,
  812,    0,    0,    0,    0,    0,  782,  783,  784,  785,
    0,  812,    0,    0,  375,  375,  375,  375,    0,  375,
  375,  786,  787,  788,  789,    0,    0,    0,    0,  812,
  812,  812,  812,  812,  812,    0,  812,  812,    0,  812,
    0,  812,  812,  812,  812,    0,    0,    0,    0,    0,
    0,  736,    0,    0,    0,    0,    0,    0,    0,    0,
  736,    0,    0,    0,    0,    0,    0,  375,    0,    0,
    0,    0,  375,    0,    0,    0,    0,  812,  812,  812,
  812,  812,  812,    0,  812,    0,    0,  736,    0,  736,
  736,  736,    0,  736,  736,    0,    0,  812,  736,  736,
  736,  729,    0,  736,  736,  736,  736,    0,    0,    0,
    0,    0,  736,  736,  736,    0,  736,    0,    0,    0,
    0,  736,  736,    0,    0,    0,    0,  812,  812,    0,
  736,  736,  812,  812,  736,    0,  736,    0,    0,  812,
    0,  736,  736,    0,  736,  736,  736,  736,  736,  736,
  736,  736,  790,    0,  791,    0,    0,    0,    0,    0,
    0,  813,    0,  813,  813,    0,    0,    0,    0,    0,
  813,  813,  813,    0,  813,  813,  813,    0,  813,    0,
  813,  813,  813,  813,    0,  813,  813,  813,    0,  982,
  813,  813,  813,  813,  813,  813,  813,  813,  813,  813,
  813,  813,  813,  813,  813,  813,  813,  813,  813,  813,
  813,  813,  813,  813,  813,  813,  813,  813,  813,  813,
  813,  151,    0,  813,  813,  813,  813,  813,  813,    0,
  813,  813,  813,  813,  813,    0,  813,    0,  813,  813,
  813,  813,  813,  813,  813,    0,    0,    0,    0,  813,
  813,  813,  813,  813,  813,  813,  813,  813,  813,  813,
  813,  813,  813,    0,  813,  813,  813,  813,  813,  813,
  813,  813,    0,  813,    0,    0,    0,    0,  904,  905,
  906,  907,  908,    0,  909,  910,  813,  145,    0,  146,
  147,  148,  149,    0,    0,    0,  778,  779,  780,  781,
    0,    0,    0,    0,  813,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,  813,
    0,    0,    0,    0,    0,  782,  783,  784,  785,    0,
  813,  911,  912,    0,    0,    0,    0,  913,    0,    0,
  786,  787,  788,  789,    0,    0,    0,    0,  813,  813,
  813,  813,  813,  813,    0,  813,  813,    0,  813,    0,
  813,  813,  813,  813,    0,    0,    0,    0,    0,    0,
  729,    0,    0,    0,    0,    0,    0,    0,    0,  729,
  914,  915,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,  813,  813,  813,  813,
  813,  813,    0,  813,    0,    0,  729,    0,  729,  729,
  729,    0,  729,  729,    0,    0,  813,  729,  729,  729,
  733,    0,    0,  729,  729,  729,    0,    0,    0,    0,
    0,  729,  729,  729,    0,  729,    0,    0,    0,    0,
  729,  729,    0,    0,    0,    0,  813,  813,    0,  729,
  729,  813,  813,  729,    0,  729,    0,    0,  813,    0,
  729,  729,    0,  729,  729,  729,  729,  729,  729,  729,
  729,  790,    0,  791,    0,    0,    0,    0,    0,    0,
  151,    0,  151,  151,    0,    0,    0,    0,    0,  151,
  151,  151,    0,  151,  151,  151,    0,  151,    0,  151,
  151,  151,  151,    0,    0,  151,  151,    0, 1234,  151,
  151,  151,  151,  151,  151,  151,  151,  151,  151,  151,
  151,  151,  151,  151,  151,  151,  151,  151,  151,  151,
  151,  151,  151,  151,  151,  151,  151,  151,  151,  151,
  149,    0,  151,  151,  151,  151,  151,  151,    0,  151,
  151,  151,  151,  151,    0,  151,    0,  151,  151,  151,
  151,  151,  151,  151,    0,    0,    0,    0,  151,  151,
  151,  151,  151,  151,  151,  151,  151,  151,  151,  151,
  151,  151,    0,  151,  151,  151,  151,  151,  151,  151,
  151,    0,  151,    0,    0,    0,    0,    0,    0,    0,
  138,  139,  140,  141,  142,  151,  143,  144,    0,  145,
    0,  146,  147,  148,  149,  778,  779,  780,  781,    0,
    0,    0,    0,  151,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,  151,    0,
    0,    0,    0,    0,  782,  783,  784,  785,    0,  151,
    0,    0,    0, 1204,  153,    0,    0,    0,    0,  786,
  787,  788,  789,    0,    0,    0,    0,  151,  151,  151,
  151,  151,  151,    0,  151,  151,    0,  151,    0,  151,
  151,  151,  151,    0,    0,    0,    0,    0,    0,  733,
    0,    0,    0,    0,    0,    0,    0,    0,  733,    0,
    0, 1205, 1206,  157,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,  151,  151,  151,  151,  151,
  151,    0,  151,    0,    0,  733,    0,  733,  733,  733,
    0,  733,  733,    0,    0,  151,  733,  733,  733,   55,
    0,    0,  733,  733,  733,    0,    0,    0,    0,    0,
  733,  733,  733,    0,  733,    0,    0,    0,    0,  733,
  733,    0,    0,    0,    0,  151,  151,    0,  733,  733,
  151,  151,  733,    0,  733,    0,    0,  151,    0,  733,
  733,    0,  733,  733,  733,  733,  733,  733,  733,  733,
  790,    0,  791,    0,    0,    0,    0,    0,    0,  149,
    0,  149,  149,    0,    0,    0,    0,    0,  149,  149,
  149,    0,  149,  149,  149,    0,  149,    0,  149,  149,
  149,  149,    0,    0,  149,  149,    0,  430,  149,  149,
  149,  149,  149,  149,  149,  149,  149,  149,  149,  149,
  149,  149,  149,  149,  149,  149,  149,  149,  149,  149,
  149,  149,  149,  149,  149,  149,  149,  149,  149,  147,
    0,  149,  149,  149,  149,  149,  149,    0,  149,  149,
  149,  149,  149,    0,  149, 1020,  149,  149,  149,  149,
  149,  149,  149,    0,    3,    0,    0,  149,  149,  149,
  149,  149,  149,  149,  149,  149,  149,  149,  149,  149,
  149,    0,  149,  149,  149,  149,  149,  149,  149,  149,
    0,  149,    0,    0,    0,    0,    0,    8,    0,    0,
    0,    0,    0,    0,  149, 1021,    0,    0,   13,   14,
   15,    0,    0,    0,  430,  430,  430,  430,    0,    0,
    0, 1022,  149,    0,    0,    0,    0,    0,    0,    0,
 1023, 1024,    0,    0,    0,    0,    0,  149,    0,    0,
    0,    0,    0,  430,  430,  430,  430,    0,  149,   29,
   30,   31,   32,   33,   34,   35,    0,    0,  430,  430,
  430,  430,    0,    0,    0,    0,  149,  149,  149,  149,
  149,  149,    0,  149,  149,    0,  149,    0,  149,  149,
  149,  149,    0,    0,    0,    0,    0,    0,   55,    0,
    0,    0,    0,    0,    0,    0,    0,   55,  107,  108,
  109,  110,  111,  112,  113,  114,  115,  116,  117,  118,
  119,  120,  121,    0,  149,  149,  149,  149,  149,  149,
    0,  149,    0,    0,   55,    0,   55,   55,   55,    0,
   55,   55,    0,    0,  149,   55,   55,   55,   56,    0,
    0,   55,   55,   55,    0,    0,    0,    0,    0,   55,
   55,   55,    0,   55,    0,    0,    0,    0,   55,   55,
    0,    0,    0,    0,  149,  149,    0,   55,   55,  149,
  149,   55,    0,   55,    0,    0,  149,    0,   55,   55,
    0,   55,   55,   55,   55,   55,   55,   55,   55,  430,
    0,  430,    0,    0,    0,    0,    0,    0,  147,    0,
  147,  147,    0,    0,    0,    0,    0,  147,  147,  147,
    0,  147,  147,  147,    0,  147,    0,  147,  147,  147,
  147,    0,    0,  147,  147,    0,    0,  147,  147,  147,
  147,  147,  147,  147,  147,  147,  147,  147,  147,  147,
  147,  147,  147,  147,  147,  147,  147,  147,  147,  147,
  147,  147,  147,  147,  147,  147,  147,  147,  148,    0,
  147,  147,  147,  147,  147,  147,    0,  147,  147,  147,
  147,  147,    0,  147, 1030,  147,  147,  147,  147,  147,
  147,  147,    0,    3,    0,    0,  147,  147,  147,  147,
  147,  147,  147,  147,  147,  147,  147,  147,  147,  147,
    0,  147,  147,  147,  147,  147,  147,  147,  147,    0,
  147,    0,    0,    0,    0,    0,    8,    0,    0,    0,
    0,    0,    0,  147,    0, 1031,    0,   13,   14,   15,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
 1032,  147,    0,    0,    0,    0,    0,    0,    0,    0,
    0, 1033,    0,    0,    0,    0,  147,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,  147,   29,   30,
   31,   32,   33,   34,   35,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,  147,  147,  147,  147,  147,
  147,    0,  147,  147,    0,  147,    0,  147,  147,  147,
  147,    0,    0,    0,    0,    0,    0,   56,    0,    0,
    0,    0,    0,    0,    0,    0,   56,    0,    0,    0,
    0,    0,    0,    0,    0,    3,    0,    0,    0,    0,
    0,    0,    0,  147,  147,  147,  147,  147,  147,    0,
  147,    0,    0,   56,    0,   56,   56,   56,    0,   56,
   56,    0,  367,  147,   56,   56,   56,   57,    8,    0,
   56,   56,   56,    0,    0,  368,    0,    0,   56,   56,
   56,    0,   56,    0,    0,    0,    0,   56,   56,    0,
    0,    0,    0,  147,  147,    0,   56,   56,  147,  147,
   56,    0,   56,    0,    0,  147,    0,   56,   56,    0,
   56,   56,   56,   56,   56,   56,   56,   56,    0,    0,
   29,   30,   31,   32,   33,   34,   35,  148,    0,  148,
  148,    0,    0,    0,    0,    0,  148,  148,  148,    0,
  148,  148,  148,    0,  148,    0,  148,  148,  148,  148,
    0,    0,  148,  148,    0,    0,  148,  148,  148,  148,
  148,  148,  148,  148,  148,  148,  148,  148,  148,  148,
  148,  148,  148,  148,  148,  148,  148,  148,  148,  148,
  148,  148,  148,  148,  148,  148,  148,  150,    0,  148,
  148,  148,  148,  148,  148,    0,  148,  148,  148,  148,
  148,    0,  148,    0,  148,  148,  148,  148,  148,  148,
  148,    0,    0,    0,    0,  148,  148,  148,  148,  148,
  148,  148,  148,  148,  148,  148,  148,  148,  148,    0,
  148,  148,  148,  148,  148,  148,  148,  148,    0,  148,
    0,    0,    0,    0,  575,    0,    0,    0,    0,    0,
    0,    0,  148,    3,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
  148,    0,    0,    0,    0,    0,    0,    0,    0,    0,
  367,    0,    0,    0,    0,  148,    8,    0,    0,    0,
    0,    0,    0,  368,    0,    0,  148,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,  148,  148,  148,  148,  148,  148,
    0,  148,  148,    0,  148,    0,  148,  148,  148,  148,
    0,    0,    0,    0,    0,    0,   57,    0,   29,   30,
   31,   32,   33,   34,   35,   57,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,  148,  148,  148,  148,  148,  148,    0,  148,
    0,    0,   57,    0,   57,   57,   57,    0,   57,   57,
    0,    0,  148,   57,   57,   57,   58,    0,    0,   57,
   57,   57,    0,    0,    0,    0,    0,   57,   57,   57,
    0,   57,    0,    0,    0,    0,   57,   57,    0,    0,
    0,    0,  148,  148,    0,   57,   57,  148,  148,   57,
    0,   57,    0,    0,  148,    0,   57,   57,    0,   57,
   57,   57,   57,   57,   57,   57,   57,    0,    0,    0,
    0,    0,    0,    0,    0,    0,  150,    0,  150,  150,
    0,    0,    0,    0,    0,  150,  150,  150,    0,  150,
  150,  150,    0,  150,    0,  150,  150,  150,  150,    0,
    0,  150,  150,    0,    0,  150,  150,  150,  150,  150,
  150,  150,  150,  150,  150,  150,  150,  150,  150,  150,
  150,  150,  150,  150,  150,  150,  150,  150,  150,  150,
  150,  150,  150,  150,  150,  150,  182,    0,  150,  150,
  150,  150,  150,  150,    0,  150,  150,  150,  150,  150,
    0,  150,    0,  150,  150,  150,  150,  150,  150,  150,
    0,    0,    0,    0,  150,  150,  150,  150,  150,  150,
  150,  150,  150,  150,  150,  150,  150,  150,    0,  150,
  150,  150,  150,  150,  150,  150,  150,    0,  150,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,  150,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,  150,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,  150,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,  150,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,  150,  150,  150,  150,  150,  150,    0,
  150,  150,    0,  150,    0,  150,  150,  150,  150,    0,
    0,    0,    0,    0,    0,   58,    0,    0,    0,    0,
    0,    0,    0,   59,   58,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,  150,  150,  150,  150,  150,  150,    0,  150,    0,
    0,   58,    0,   58,   58,   58,    0,   58,   58,    0,
    0,  150,   58,   58,   58,    0,    0,    0,   58,   58,
   58,    0,    0,    0,    0,    0,   58,   58,   58,    0,
   58,    0,    0,    0,    0,   58,   58,    0,    0,    0,
    0,  150,  150,    0,   58,   58,  150,  150,   58,    0,
   58,    0,    0,  150,    0,   58,   58,    0,   58,   58,
   58,   58,   58,   58,   58,   58,    0,    0,    0,    0,
    0,    0,    0,    0,    0,  182,    0,  182,  182,    0,
    0,    0,    0,    0,  182,  182,   61,    0,  182,  182,
  182,    0,  182,    0,  182,  182,    0,    0,    0,    0,
  182,  182,    0,    0,    0,  182,  182,  182,  182,  182,
  182,  182,  182,  182,  182,  182,  182,  182,  182,  182,
  182,  182,  182,  182,  182,  182,  182,  182,  182,  182,
  182,  182,  182,  182,  182,    0,    0,  182,  182,  182,
  182,  182,  182,    0,  182,  182,  182,  182,  182,    0,
  182,    0,  182,  182,  182,  182,  182,  182,  182,    0,
    0,    0,    0,  182,  182,  182,  182,  182,  182,  182,
  182,  182,  182,  182,  182,  182,  182,    0,  182,  182,
  182,  182,  182,  182,  182,  182,    0,  182,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
  182,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,  182,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,  182,    0,    0,    0,    0,    0,    0,
    0,    0,   59,    0,  182,    0,    0,    0,    0,    0,
    0,   59,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,  182,  182,  182,  182,  182,  182,    0,  182,
  182,    0,  182,    0,  182,  182,  182,  182,   59,    0,
   59,   59,   59,    0,   59,   59,    0,    0,    0,   59,
   59,   59,    0,    0,    0,   59,   59,   59,    0,    0,
    0,    0,    0,   59,   59,   59,    0,   59,    0,    0,
  182,  182,   59,   59,    0,  182,    0,  182,    0,    0,
    0,   59,   59,    0,    0,   59,    0,   59,    0,  601,
  182,    0,   59,   59,    0,   59,   59,   59,   59,   59,
   59,   59,   59,  130,  131,   61,    0,   61,    0,    0,
    0,    0,    0,   63,   61,   61,    0,    0,    0,   61,
    0,  182,    0,    0,   61,  182,  182,    0,    0,    0,
    0,    0,  182,    0,    0,    0,   61,   61,   61,   61,
   61,   61,   61,   61,   61,   61,   61,   61,   61,   61,
   61,   61,    0,   61,   61,   61,    0,   61,   61,   61,
   61,   61,   61,   61,   61,   61,   61,   61,   61,   61,
   61,   61,   61,    0,   61,   61,   61,   61,   61,    0,
   61,   61,   61,   61,   61,   61,   61,   61,   61,   61,
   61,   61,   61,   61,   61,   61,   61,   61,   61,   61,
   61,   61,    0,    0,    0,   61,   61,    0,   61,   61,
   61,   61,   61,   61,   61,   61,    0,    0,    0,    0,
    0,  135,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,  136,    0,    0,
    0,    0,    0,   61,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,  137,  138,  139,  140,  141,
  142,    0,  143,  144,    0,  145,    0,  146,  147,  148,
  149,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,   61,   61,   61,   61,   61,    0,   61,
   61,    0,   61,    0,   61,   61,   61,   61,    0,    0,
    0,    0,    0,  246,  151,    0,    0,    0,  152,    0,
  153,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,  378,    0,  378,    0,    0,
    0,    0,    0,    0,  378,    0,   61,   61,    0,    0,
    0,    0,   61,    0,    0,    0,  225,  378,    0,    0,
    0,    0,   61,   61,  155,    0,    0,    0,  156,  157,
  225,  225,   63,    0,   63,    0,    0,    0,    0,    0,
   64,   63,   63,    0,    0,    0,   63,    0,    0,    0,
    0,   63,    0,    0,    0,   61,   61,    0,    0,    0,
    0,    0,   61,   63,   63,   63,   63,   63,   63,   63,
   63,   63,   63,   63,   63,   63,   63,   63,   63,    0,
   63,   63,   63,    0,   63,   63,   63,   63,   63,   63,
   63,   63,   63,   63,   63,   63,   63,   63,   63,   63,
    0,   63,   63,   63,   63,   63,    0,   63,   63,   63,
   63,   63,   63,   63,   63,   63,   63,   63,   63,   63,
   63,   63,   63,   63,   63,   63,   63,   63,   63,    0,
    0,    0,   63,   63,    0,   63,   63,   63,   63,   63,
   63,   63,   63,    0,    0,    0,    0,    0,  225,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,  378,    0,    0,    0,
    0,    0,    0,    0,  225,    0,    0,    0,    0,    0,
   63,    0,    0,    0,  378,  378,  378,  378,    0,  378,
  378,    0,  225,  225,  225,  225,  225,  225,    0,  225,
  225,    0,  225,    0,  225,  225,  225,  225,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
   63,   63,   63,   63,   63,    0,   63,   63,    0,   63,
    0,   63,   63,   63,   63,    0,    0,  378,    0,    0,
  225,  225,  378,    0,    0,  225,    0,  225,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,   63,   63,    0,    0,    0,    0,   63,
  461,    0,    0,  466,    0,    0,    0,    0,    0,   63,
   63,  225,    0,    0,  461,  225,  225,    0,    0,   64,
    0,   64,    0,    0,    0,    0,    0,   65,   64,   64,
    0,    0,    0,   64,    0,    0,    0,    0,   64,    0,
    0,    0,   63,   63,    0,    0,    0,    0,    0,   63,
   64,   64,   64,   64,   64,   64,   64,   64,   64,   64,
   64,   64,   64,   64,   64,   64,    0,   64,   64,   64,
    0,   64,   64,   64,   64,   64,   64,   64,   64,   64,
   64,   64,   64,   64,   64,   64,   64,    0,   64,   64,
   64,   64,   64,    0,   64,   64,   64,   64,   64,   64,
   64,   64,   64,   64,   64,   64,   64,   64,   64,   64,
   64,   64,   64,   64,   64,   64,    0,    0,    0,   64,
   64,    0,   64,   64,   64,   64,   64,   64,   64,   64,
    0,    0,  461,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,  461,    0,
    0,    0,    0,    0,    0,    0,    0,   64,    0,    0,
    0,    0,    0,    0,    0,    0,  461,  461,  461,  461,
  461,  461,    0,  461,  461,    0,  461,    0,  461,  461,
  461,  461,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,   64,   64,   64,
   64,   64,    0,   64,   64,    0,   64,    0,   64,   64,
   64,   64,    0,    0,  461,  461,    0,    0,    0,  461,
    0,  461,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
   64,   64,    0,    0,    0,    0,   64,  461,    0,    0,
    0,    0,    0,    0,    0,  461,   64,   64,    0,  461,
  461,  461,    0,    0,    0,    0,   65,    0,   65,    0,
    0,    0,    0,    0,  596,   65,   65,    0,    0,    0,
   65,    0,    0,    0,    0,   65,    0,    0,    0,   64,
   64,    0,    0,    0,    0,    0,   64,   65,   65,   65,
   65,   65,   65,   65,   65,   65,   65,   65,   65,   65,
   65,   65,   65,    0,   65,   65,   65,    0,   65,   65,
   65,   65,   65,   65,   65,   65,   65,   65,   65,   65,
   65,   65,   65,   65,    0,   65,   65,   65,   65,   65,
    0,   65,   65,   65,   65,   65,   65,   65,   65,   65,
   65,   65,   65,   65,   65,   65,   65,   65,   65,   65,
   65,   65,   65,    0,    0,    0,   65,   65,    0,   65,
   65,   65,   65,   65,   65,   65,   65,    0,    0,  461,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,  597,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,  461,    0,    0,    0,    0,
    0,    0,    0,    0,   65,    0,    0,    0,    0,    0,
    0,    0,    0,  461,  461,  461,  461,  461,  461,    0,
  461,  461,    0,  461,    0,  461,  461,  461,  461,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,   65,   65,   65,   65,   65,    0,
   65,   65,    0,   65,    0,   65,   65,   65,   65,    0,
    0,  461,  461,    0,    0,    0,  461,    0,  461,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,   65,   65,    0,
    0,    0,    0,   65,    0,    0,    0,    0,    0,    0,
  601,    0,  461,   65,   65,    0,  461,  461,    0,    0,
    0,    0,    0,  596,    0,  596,    0,    0,    0,    0,
    0,    0,  596,  596,    0,    0,    0,    0,    0,    0,
    0,    0,  596,    0,    0,    0,   65,   65,    0,    0,
    0,    0,    0,   65,  596,  596,  596,  596,  596,  596,
  596,  596,  596,  596,  596,  596,  596,  596,  596,  596,
    0,  596,  596,  596,    0,  596,  596,  596,  596,  596,
  596,  596,  596,  596,  596,  596,  596,  596,  596,  596,
  596,    0,  596,  596,  596,  596,  596,    0,  596,  596,
  596,  596,  596,  596,  596,  596,  596,  596,  596,  596,
  596,  596,  596,  596,  596,  596,  596,  596,  596,  596,
    0,    0,    0,  596,  596,    0,  596,  596,  596,  596,
  596,  596,  596,  596,    0,  596,    0,    0,    0,    0,
    0,  597,    0,  597,    0,    0,    0,    0,    0,    0,
  597,  597,  602,    0,    0,    0,    0,    0,    0,    0,
  597,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,  596,  597,  597,  597,  597,  597,  597,  597,  597,
  597,  597,  597,  597,  597,  597,  597,  597,    0,  597,
  597,  597,    0,  597,  597,  597,  597,  597,  597,  597,
  597,  597,  597,  597,  597,  597,  597,  597,  597,    0,
  597,  597,  597,  597,  597,    0,  597,  597,  597,  597,
  597,  597,  597,  597,  597,  597,  597,  597,  597,  597,
  597,  597,  597,  597,  597,  597,  597,  597,    0,    0,
    0,  597,  597,    0,  597,  597,  597,  597,  597,  597,
  597,  597,    0,  597,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,  601,
    0,  601,    0,    0,    0,    0,    0,    0,  601,  601,
    0,    0,  835,    0,    0,    0,    0,    0,  601,  597,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
  601,  601,  601,  601,  601,  601,  601,  601,  601,  601,
  601,  601,  601,  601,  601,  601,    0,  601,  601,  601,
  596,  601,  601,  601,  601,  601,  601,  601,  601,  601,
  601,  601,  601,  601,  601,  601,  601,    0,  601,  601,
  601,  601,  601,    0,  601,  601,  601,  601,  601,  601,
  601,  601,  601,  601,  601,  601,  601,  601,  601,  601,
  601,  601,  601,  601,  601,  601,    0,    0,    0,  601,
  601,    0,  601,  601,  601,  601,  601,  601,  601,  601,
    0,  601,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,  213,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,  602,    0,  602,    0,    0,    0,  601,    0,    0,
  602,  602,    0,    0,    0,    0,    0,    0,    0,    0,
  602,    0,    0,    0,    0,    0,    0,    0,  597,    0,
    0,    0,  602,  602,  602,  602,  602,  602,  602,  602,
  602,  602,  602,  602,  602,  602,  602,  602,    0,  602,
  602,  602,    0,  602,  602,  602,  602,  602,  602,  602,
  602,  602,  602,  602,  602,  602,  602,  602,  602,    0,
  602,  602,  602,  602,  602,    0,  602,  602,  602,  602,
  602,  602,  602,  602,  602,  602,  602,  602,  602,  602,
  602,  602,  602,  602,  602,  602,  602,  602,    0,    0,
    0,  602,  602,    0,  602,  602,  602,  602,  602,  602,
  602,  602,    0,  602,    0,    0,    0,    0,  211,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,  835,    0,  835,    0,    0,    0,    0,    0,    0,
  835,  835,  835,  835,  835,  835,  835,    0,    0,  602,
  835,  835,    0,    0,    0,    0,  601,    0,    0,    0,
  835,  835,  835,  835,  835,  835,  835,  835,  835,  835,
  835,  835,  835,  835,  835,  835,  835,  835,    0,  835,
  835,  835,    0,  835,  835,  835,  835,    0,  835,  835,
  835,    0,    0,    0,  835,  835,  835,    0,    0,    0,
  835,  835,  835,  835,  835,    0,  835,    0,  835,    0,
  835,  835,  835,    0,    0,    0,    0,    0,    0,    0,
  835,  835,  835,    0,  835,  835,  835,  835,    0,    0,
    0,  835,  835,    0,  835,  835,  835,  835,  835,  835,
  835,  835,  835,  835,    0,    0,  212,  213,    0,  213,
  213,    0,    0,    0,    0,    0,  213,  213,    0,    0,
  213,  213,  213,    0,  213,    0,  213,  213,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,  213,  213,
  213,  213,  213,  213,  213,  213,  213,  213,  213,  213,
  213,  213,  213,  213,    0,  213,  213,  213,  602,  213,
  213,  213,  213,  213,  213,  213,  213,    0,    0,  213,
  213,  213,  213,  213,  213,    0,  213,  213,  213,  213,
  213,    0,  213,    0,  213,  213,  213,  213,  213,  213,
  213,    0,    0,    0,    0,  213,  213,  213,  213,  213,
  213,  213,  213,  213,    0,    0,    0,  213,  213,    0,
  213,  213,  213,  213,  213,  213,  213,  213,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,   43,    0,    0,    0,    0,  835,
    0,    0,    0,    0,    0,    0,  211,  211,    0,    0,
  211,  211,  211,    0,  211,    0,  211,  211,    0,    0,
    0,    0,    0,    0,    0,  213,    0,    0,  211,  211,
  211,  211,  211,  211,  211,  211,  211,  211,  211,  211,
  211,  211,  211,  211,    0,  211,  211,  211,  835,  211,
  211,  211,  211,  211,  211,  211,  211,    0,    0,  211,
  211,  211,  211,  211,  211,    0,  211,  211,  211,  211,
  211,    0,  211,    0,  211,  211,  211,  211,  211,  211,
  211,    0,    0,    0,    0,  211,  211,  211,  211,  211,
  211,  211,  211,  211,    0,    0,    0,  211,  211,    0,
  211,  211,  211,  211,  211,  211,  211,  211,   44,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,  213,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,  212,  212,    0,    0,  212,  212,
  212,    0,  212,    0,  212,  212,    0,    0,    0,    0,
    0,    0,    0,    0,    0,  211,  212,  212,  212,  212,
  212,  212,  212,  212,  212,  212,  212,  212,  212,  212,
  212,  212,    0,  212,  212,  212,    0,  212,  212,  212,
  212,  212,  212,  212,  212,    0,    0,  212,  212,  212,
  212,  212,  212,    0,  212,  212,  212,  212,  212,    0,
  212,    0,  212,  212,  212,  212,  212,  212,  212,    0,
   33,    0,    0,  212,  212,  212,  212,  212,  212,  212,
  212,  212,    0,    0,    0,  212,  212,    0,  212,  212,
  212,  212,  212,  212,  212,  212,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,  211,   43,    0,   43,    0,    0,    0,    0,
    0,    0,   43,   43,    0,    0,    0,    0,    0,    0,
    0,    0,   43,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,  212,   43,   43,   43,   43,   43,   43,
   43,   43,   43,   43,   43,   43,   43,   43,   43,   43,
    0,   43,   43,   43,    0,   43,   43,   43,   43,   43,
   43,   43,   43,   43,   43,    0,   43,   43,   43,    0,
    0,  692,   43,   43,   43,   43,   43,    0,   43,   43,
   43,   43,   43,   43,   43,   43,    0,    0,   43,   43,
   43,   43,   43,   43,   43,    0,   43,   43,   43,   43,
    0,    0,    0,   43,   43,    0,   43,   43,   43,   43,
   43,   43,   43,   43,    0,    0,    0,   44,    0,   44,
    0,    0,    0,    0,    0,    0,   44,   44,    0,    0,
  212,    0,    0,    0,    0,    0,   44,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,   44,   44,
   44,   44,   44,   44,   44,   44,   44,   44,   44,   44,
   44,   44,   44,   44,    0,   44,   44,   44,    0,   44,
   44,   44,   44,   44,   44,   44,   44,   44,   44,    0,
   44,   44,   44,  694,    0,    0,   44,   44,   44,   44,
   44,    0,   44,   44,   44,   44,   44,   44,   44,   44,
    0,    0,   44,   44,   44,   44,   44,   44,   44,    0,
   44,   44,   44,   44,    0,    0,    0,   44,   44,   33,
   44,   44,   44,   44,   44,   44,   44,   44,   33,   33,
    0,    0,    0,    0,    0,    0,    0,    0,   33,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
   33,   33,   33,   33,   33,   33,   33,   33,   33,   33,
   33,   33,   33,   33,   33,   33,    0,   33,   33,   33,
    0,   33,   33,   33,   33,   33,   33,   33,   33,   33,
   33,    0,   33,   33,   33,   35,    0,    0,   33,   33,
   33,   33,   33,    0,   33,   33,   33,   33,   33,   33,
   33,   33,    0,    0,   33,   33,   33,   33,   33,   33,
   33,    0,   33,   33,   33,   33,    0,    0,    0,   33,
   33,    0,   33,   33,   33,   33,   33,   33,   33,   33,
  692,    0,  692,    0,    0,    0,    0,    0,    0,  692,
  692,    0,    0,    0,    0,    0,    0,    0,    0,  692,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,  692,  692,  692,  692,  692,  692,  692,  692,  692,
  692,  692,  692,  692,  692,  692,  692,    0,  692,  692,
  692,    0,  692,  692,  692,  692,  692,  692,  692,  692,
    0,    0,  692,  692,  692,  692,  692,  692,   39,  692,
  692,  692,  692,  692,    0,  692,    0,  692,  692,  692,
  692,  692,  692,  692,    0,    0,    0,    0,  692,  692,
  692,  692,  692,  692,  692,  692,  692,    0,    0,    0,
  692,  692,    0,  692,  692,  692,  692,  692,  692,  692,
  692,    0,  694,    0,  694,    0,    0,    0,    0,    0,
    0,  694,  694,    0,    0,    0,    0,    0,    0,    0,
    0,  694,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,  694,  694,  694,  694,  694,  694,  694,
  694,  694,  694,  694,  694,  694,  694,  694,  694,    0,
  694,  694,  694,    0,  694,  694,  694,  694,  694,  694,
  694,  694,    0,    0,  694,  694,  694,  694,  694,  694,
   37,  694,  694,  694,  694,  694,    0,  694,    0,  694,
  694,  694,  694,  694,  694,  694,    0,    0,    0,    0,
  694,  694,  694,  694,  694,  694,  694,  694,  694,    0,
    0,    0,  694,  694,   35,  694,  694,  694,  694,  694,
  694,  694,  694,   35,   35,    0,    0,    0,    0,    0,
    0,    0,    0,   35,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,   35,   35,   35,   35,   35,
   35,   35,   35,   35,   35,   35,   35,   35,   35,   35,
   35,    0,   35,   35,   35,    0,   35,   35,   35,   35,
   35,   35,   35,   35,   35,   35,    0,   35,   35,   35,
    0,    0,   41,   35,   35,   35,   35,   35,    0,   35,
   35,   35,   35,   35,   35,   35,   35,    0,    0,   35,
   35,   35,   35,   35,   35,   35,    0,   35,   35,   35,
   35,    0,    0,    0,   35,   35,    0,   35,   35,   35,
   35,   35,   35,   35,   35,    0,    0,   39,    0,    0,
    0,    0,    0,    0,    0,    0,   39,   39,    0,    0,
    0,    0,    0,    0,    0,    0,   39,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,   39,   39,
   39,   39,   39,   39,   39,   39,   39,   39,   39,   39,
   39,   39,   39,   39,    0,   39,   39,   39,    0,   39,
   39,   39,   39,   39,   39,   39,   39,   39,   39,    0,
   39,   39,   39,  334,    0,    0,   39,   39,   39,   39,
   39,    0,   39,   39,   39,   39,   39,   39,   39,   39,
    0,    0,   39,   39,   39,   39,   39,   39,   39,    0,
   39,   39,   39,   39,    0,    0,    0,   39,   39,    0,
   39,   39,   39,   39,   39,   39,   39,   39,    0,   37,
    0,    0,    0,    0,    0,    0,    0,    0,   37,   37,
    0,    0,    0,    0,    0,    0,    0,    0,   37,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
   37,   37,   37,   37,   37,   37,   37,   37,   37,   37,
   37,   37,   37,   37,   37,   37,    0,   37,   37,   37,
    0,   37,   37,   37,   37,   37,   37,   37,   37,   37,
   37,    0,   37,   37,   37,  372,    0,    0,   37,   37,
   37,   37,   37,    0,   37,   37,   37,   37,   37,   37,
   37,   37,    0,    0,   37,   37,   37,   37,   37,   37,
   37,    0,   37,   37,   37,   37,    0,    0,    0,   37,
   37,   41,   37,   37,   37,   37,   37,   37,   37,   37,
   41,   41,    0,    0,    0,    0,    0,    0,    0,    0,
   41,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,  354,   41,   41,   41,   41,   41,   41,   41,   41,
   41,   41,   41,   41,   41,   41,   41,   41,    0,   41,
   41,   41,    0,   41,   41,   41,   41,   41,   41,   41,
   41,   41,   41,    0,   41,   41,   41,    0,    0,    0,
   41,   41,   41,   41,   41,    0,   41,   41,   41,   41,
   41,   41,   41,   41,    0,    0,   41,   41,   41,   41,
   41,   41,   41,    0,   41,   41,   41,   41,    0,    0,
    0,   41,   41,    0,   41,   41,   41,   41,   41,   41,
   41,   41,  334,    0,  334,    0,    0,    0,    0,    0,
    0,  334,  334,  736,    0,    0,    0,  334,    0,    0,
    0,  334,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,  334,  334,  334,  334,  334,  334,  334,
  334,  334,  334,  334,  334,  334,  334,  334,  334,    0,
  334,  334,  334,    0,  334,  334,  334,  334,  334,  334,
  334,  334,    0,    0,    0,  334,  334,  334,    0,    0,
    0,  334,  334,  334,  334,  334,    0,  334,    0,  334,
  334,  334,  334,  334,  334,    0,    0,    0,    0,    0,
  334,  334,  334,  334,    0,  334,  334,  334,  334,    0,
    0,    0,  334,  334,    0,  334,  334,  334,  334,  334,
  334,  334,  334,    0,  372,    0,  372,    0,    0,    0,
    0,    0,    0,  372,  372,    0,    0,    0,    0,    0,
    0,    0,    0,  372,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,  372,  372,  372,  372,  372,
  372,  372,  372,  372,  372,  372,  372,  372,  372,  372,
  372,    0,  372,  372,  372,    0,  372,  372,  372,  372,
  372,  372,  372,  372,    0,    0,    0,  372,  372,  372,
    0,    0,    0,  372,  372,  372,  372,  372,    0,  372,
  354,  372,  372,  372,  372,  372,  372,    0,    0,  354,
    0,    0,  372,  372,  372,  372,    0,  372,  372,  372,
  372,    0,    0,    0,  372,  372,    0,  372,  372,  372,
  372,  372,  372,  372,  372,    0,  354,    0,  354,  354,
  354,    0,  354,  354,    0,    0,  354,  354,  354,  354,
    0,    0,    0,  354,  354,  354,    0,    0,    0,    0,
    0,  354,  354,  354,    0,  354,    0,  354,  354,  354,
  354,  354,  354,    0,    0,    0,    0,    0,  354,  354,
  354,    0,    0,  354,    0,  354,    0,    0,    0,    0,
  354,  354,  736,  354,  354,  354,  354,  354,  354,  354,
  354,  736,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,  736,    0,
  736,  736,  736,    0,  736,  736,    0,    0,    0,  736,
  736,  736,    0,    0,    0,  736,  736,  736,    0,    0,
    0,    0,    0,  736,  736,  736,    0,  736,    0,    0,
    0,    0,  736,  736,    0,    0,    0,    0,    0,    0,
    0,  736,  736,    0,    0,  736,    0,  736,    0,    0,
    0,    0,  736,  736,    0,  736,  736,  736,  736,  736,
  736,  736,  736,  164,    0,  164,  164,    0,    0,    0,
    0,    0,    0,    0,  164,    0,    0,    0,    0,   98,
    0,   99,  100,    0,  164,  164,    0,    0,  164,  164,
  389,    0,  164,    0,    0,    0,    0,    0,    0,    0,
  318,  319,    0,    0,  130,  131,    0,    0,  320,    0,
  164,    0,    0,  538,  164,    0,    0,    0,    0,    0,
    0,    0,    3,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,   98,    0,   99,  100,    0,    0,    0,    0,
    0,    0,    0,  389,    0,    8,    0,    0,    0,    0,
  164,  164,  164,  621,  319,  539,    0,  130,  131,  540,
  541,  320,    0,    0,    0,    0,  132,  133,  134,    0,
    0,    0,    0,   20,   21,    0,  542,    0,    0,    0,
    0,    0,    0,    0,    0,  543,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,  164,   29,   30,   31,
   32,   33,   34,   35,    0,    0,    0,    0,    0,    0,
    0,    0,  135,    0,    0,    0,    0,    0,    0,    0,
    0,    0,  164,    0,    0,    0,    0,    0,    0,  132,
  133,  134,    0,  130,  131,    0,    0,    0,  136,    0,
  164,  164,  164,  164,  164,  164,    0,  164,  164,    0,
  164,    0,  164,  164,  164,  164,  137,  138,  139,  140,
  141,  142,    0,  143,  144,    0,  145,    0,  146,  147,
  148,  149,    0,    0,    0,  135,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,  164,  164,
  164,  164,  164,  164,    0,  164,    0,    0,    0,    0,
    0,  136,    0,    0,  246,  151,  322,  323,  324,  152,
    0,  153,    0,    0,    0,    0,    0,    0,    0,  137,
  138,  139,  140,  141,  142,    0,  143,  144,    0,  145,
    0,  146,  147,  148,  149,    0,    0,    0,   98,  164,
   99,  100,    0,  164,  164,    0,    0,    0,    0,  129,
  164,  135,    0,    0,   98,  155,   99,  100,    0,  156,
  157,    0,    0,  130,  131,  232,   84,  246,  151,  322,
  323,  324,  152,    0,  153,    0,    0,  136,    0,  130,
  131,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    8,    0,    0,    0,    0,  137,  138,  139,  140,  141,
  142,    0,  143,  144,    0,  145,    0,  146,  147,  148,
  149,    0,    0,    0,   98,    0,   99,  100,  155,    0,
    0,    0,  156,  157,    0,  232,    0,    0,    0,   84,
    0,    0,    0,    0,    0,  132,  133,  134,    0,  130,
  131,    0,    0,  246,  151,    0,    0,    0,  152,    0,
  153,  132,  133,  134,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,  135,    0,    0,    0,    0,    0,    0,  822,   98,
  823,   99,  100,    0,  155,    0,    0,  135,  156,  157,
  232,    0,    0,    0,    0,   84,    0,  136,    0,    0,
    0,  132,  133,  134,  130,  131,    0,    0,    0,    0,
    0,    0,    0,  136,    0,  137,  138,  139,  140,  141,
  142,    0,  143,  144,    0,  145,    0,  146,  147,  148,
  149,  137,  138,  139,  140,  141,  142,    0,  143,  144,
    0,  145,    0,  146,  147,  148,  149,  135,    0,    0,
    0,    0,    0,    0,    0,    0,   98,    0,   99,  100,
    0,    0,    0,  150,  151,    0,    0,  232,  152,    0,
  153,    0,    0,  136,    0,    0,  132,  133,  134,  246,
  151,  130,  131,    0,  152,  154,  153,    0,    0,    0,
    0,  137,  138,  139,  140,  141,  142,    0,  143,  144,
    0,  145,    0,  146,  147,  148,  149,    0,    0,    0,
    0,    0,    0,    0,  155,    0,    0,    0,  156,  157,
    0,    0,  135,    0,    0,   84,    0,    0,    0,    0,
  155,    0,    0,    0,  156,  157,  225,    0,    0,  150,
  151,   84,    0,    0,  152,    0,  153,    0,  136,    0,
  225,  225,    0,  132,  133,  134,    0,    0,    0,    0,
    0,  154,    0,    0,    0,    0,  137,  138,  139,  140,
  141,  142,    0,  143,  144,    0,  145,    0,  146,  147,
  148,  149,    0,    0,    0,    0,    0,    0,    0,    0,
  155,    0,    0,    0,  156,  157,    0,    0,    0,  135,
    0,   84,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,  246,  151,    3,    0,    0,  152,
    0,  153,    0,    0,    0,  136,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,  137,  138,  139,  140,  141,  142,    0,
  143,  144,    0,  145,    0,  146,  147,  148,  149,  225,
    0,  225,    0,    0,    0,  155,    0,    0,  225,  156,
  157,    0,    0,    0,    0,    0,   84,    0,    0,    0,
    0,  549,    0,    0,    0,    0,    0,    0,    0,    0,
    3,  634,  151,    0,  225,    0,  152,    0,  153,    0,
    0,   29,   30,   31,   32,   33,   34,   35,    0,    0,
    0,    0,  225,  225,  225,  225,  225,  225,    0,  225,
  225,    0,  225,    8,  225,  225,  225,  225,    0,    0,
    0,  130,  131,  550,    0,    0,    0,  540,  541,    0,
    0,    0,  155,    0,    0,    0,  156,  157,  334,  334,
    0,    0,    0,   84,  551,  552,    0,    0,    0,    0,
  225,  225,    0,  553,    0,  225,    0,  225,    0,    0,
    0,    0,    0,    0,    0,   29,   30,   31,   32,   33,
   34,   35,    0,    0,    0,  904,  905,  906,  907,  908,
    0,  909,  910,    0,  145,    0,  146,  147,  148,  149,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,  225,    0,    0,    0,  225,  225,    0,    0,  554,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,  911,  912,
 1017,    0, 1018,    0,  913,    0,    0,    0,  334,  135,
    0,    0,  334,  334,  334,  334,  334,  334,    0,  334,
  334,  334,  334,  334,  334,  334,  334,  225,  225,    0,
    0,    0,    0,    0,    0,  136,    0,    0,    0,    0,
    0,    0,    0,  130,  131,    0,    0,  914,  915,    0,
    0,    0,  334,  137,  138,  139,  140,  141,  142,    0,
  143,  144,    0,  145,    0,  146,  147,  148,  149,    0,
  334,  334,  334,  334,  334,  334,    0,  334,  334,    0,
  334,    0,  334,  334,  334,  334,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,  246,  151,  225,  225,    0,  152,    0,  153,    0,
    0,    0,    0,    0,    0,    0,    0,    0,  334,  334,
    0,    0,    0,  334,    0,  334,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,  225,    0,  225,    0,
    0,    0,    0,    0,    0,  225,    0,    0,    0,    0,
    0,    0,  155,    0,    0,    0,  156,  157,    0,    0,
    0,  135,    0,  350,  350,    0,    0,    0,    0,  334,
    0,  225,    0,  334,  334,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,  136,    0,  225,
  225,  225,  225,  225,  225,    0,  225,  225,    0,  225,
    0,  225,  225,  225,  225,  137,  138,  139,  140,  141,
  142,    0,  143,  144,  727,  145,    0,  146,  147,  148,
  149,  225,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,  225,  225,    0,
    0,    0,  225,    0,  225,    0,    0,  225,    0,    0,
    0,    0,    0,  246,  151,    0,    0,    0,  152,    0,
  153,    0,    0,    0,    0,  225,  225,  225,  225,  225,
  225,    0,  225,  225,  932,  225,    0,  225,  225,  225,
  225,  350,    0,    0,  933,    0,    0,    0,  225,    0,
    0,    0,  225,  225,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,  155,    0,    0,  350,  156,  157,
    0,    0,    0,  225,  225,  728,    0,    0,  225,    0,
  225,    0,    0,    0,    0,  350,  350,  350,  350,  350,
  350,    0,  350,  350,    0,  350,    0,  350,  350,  350,
  350,    0,    0,    0,    0,    0,    0,    0,  729,    0,
    0,    0,    0,  730,    0,    0,    0,    0,    0,  731,
    0,    0,    0,    0,  225,    0,    0,    0,  225,  225,
    0,    0,    0,  350,  350,    0,    0,    0,  350,    0,
  350,    0,    0,  732,  733,    0,    0,  734,  735,  736,
  737,  738,  739,  740,  741,  742,    0,  743,  744,  745,
  746,  747,  146,  147,  148,  149,  748,  749,  750,  751,
  752,  753,  754,  755,  756,  757,  758,  759,  760,  761,
    0,    0,  762,    0,  350,    0,    0,    0,  350,  350,
    0,    0,    0,    0,    0,    0,    0,    0,  763,    0,
    0,    0,    0,    0,    0,    0,    0,  934,  935,    0,
  936,  937,  938,  939,  940,  941,    0,  942,  943,  944,
  746,  747,  146,  147,  148,  149,  945,  946,  947,  948,
  949,    0,    0,  950,  951,    0,  952,  953,    0,    0,
    0,    0,    0,  954,    0,  955,  956,  957,  958,  959,
  960,  961,  962,  963,  964,  965,  966,  967,   79,   80,
   81,   82,   83,    0,    0,    0,  770,    0,    0,    0,
    0,  257,  258,  259,  260,  770,  261,  262,  263,  264,
  265,  266,  267,    0,    0,    0,    0,    0,    0,  268,
    0,    0,    0,    0,    0,    0,    0,    0,  728,    0,
    0,  269,  270,  271,  272,  273,  274,    0,  770,    0,
    0,    0,    0,    0,    0,    0,    0,    0,  770,    0,
    0,    0,  770,  770,    0,    0,    0,    0,    0,    0,
    0,  729,    0,    0,    0,    0,  730,    0,    0,  770,
  770,    0,  731,    0,    0,    0,    0,    0,  770,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
  770,  770,  770,  770,  770,  770,  770,    0,    0,    0,
  734,  735,  736,  737,  738,  739,  740,  741,  742,    0,
  743,  744,  745,  746,  747,  146,  147,  148,  149,  748,
  749,  750,  751,  752,  753,  754,  755,  756,  757,    0,
  759,  760,  761,    0,  770,  762,    0,    0,    0,    0,
    0,    0,    0,   98,    0,   99,    0,    0,    0,    0,
    0,  763,  498,  499,    0,    0,    0,    0,    0,  275,
  276,    0,    3,    0,    0,    0,    0,    0,    0,    0,
  277,   85,    0,    0,  500,  501,  502,  503,  504,  505,
  506,  507,  508,  509,  510,  511,  512,  513,  514,    0,
    0,    0,    0,    0,    0,    8,    9,  515,  516,    0,
  517,    0,    0,    0,    0,    0,   13,   14,   15,    0,
    0,    0,  518,  519,    0,    0,    0,    0,    0,    0,
  520,    0,  521,   20,   21,    0,   98,    0,   99,    0,
    0,    0,    0,    0,  522,  498, 1043,  523,    0,  524,
    0,    0,    0,    0,    0,    3,    0,   29,   30,   31,
   32,   33,   34,   35,    0,    0,    0,  500,  501,  502,
  503,  504,  505,  506,  507,  508,  509,  510,  511,  512,
  513,  514,    0,    0,    0,    0,    0,    0,    8,    9,
  515,  516,    0,  517,    0,    0,    0,    0,    0,   13,
   14,   15,    0,    0,    0,  518,  519,    0,    0,    0,
    0,    0,    0,  520,    0,  521,   20,   21,    0,  535,
    0,  535,    0,    0,    0,    0,    0,  522,  535,  535,
  523,    0,  524,    0,    0,    0,    0,    0,  535,    0,
   29,   30,   31,   32,   33,   34,   35,    0,    0,    0,
  535,  535,  535,  535,  535,  535,  535,  535,  535,  535,
  535,  535,  535,  535,  535,    0,    0,    0,    0,    0,
    0,  535,  535,  535,  535,    0,  535,    0,    0,    0,
    0,    0,  535,  535,  535,    0,    0,    0,  535,  535,
    0,    0,    0,    0,    0,    0,  535,    0,  535,  535,
  535,    0,  541,    0,  541,    0,    0,    0,    0,    0,
  535,  541,  541,  535,    0,  535,    0,    0,    0,    0,
    0,  541,    0,  535,  535,  535,  535,  535,  535,  535,
    0,    0,    0,  541,  541,  541,  541,  541,  541,  541,
  541,  541,  541,  541,  541,  541,  541,  541,    0,    0,
    0,    0,    0,    0,  541,  541,  541,  541,    0,  541,
    0,    0,    0,    0,    0,  541,  541,  541,    0,    0,
    0,  541,  541,    0,    0,    0,    0,    0,    0,  541,
    0,  541,  541,  541,    0,  503,    0,  503,    0,    0,
    0,    0,    0,  541,  503,  503,  541,    0,  541,    0,
    0,    0,    0,    0,  503,    0,  541,  541,  541,  541,
  541,  541,  541,    0,    0,    0,  503,  503,  503,  503,
  503,  503,  503,  503,  503,  503,  503,  503,  503,  503,
  503,    0,    0,    0,    0,    0,    0,  503,  503,  503,
  503,    0,  503,    0,    0,    0,    0,    0,  503,  503,
  503,    0,    0,    0,  503,  503,    0,    0,    0,    0,
    0,    0,  503,    0,  503,  503,  503,    0,  536,    0,
  536,    0,    0,    0,    0,    0,  503,  536,  536,  503,
    0,  503,    0,    0,    0,    0,    0,  536,    0,  503,
  503,  503,  503,  503,  503,  503,    0,    0,    0,  536,
  536,  536,  536,  536,  536,  536,  536,  536,  536,  536,
  536,  536,  536,  536,    0,    0,    0,    0,    0,    0,
  536,  536,  536,  536,    0,  536,    0,    0,    0,    0,
    0,  536,  536,  536,    0,    0,    0,  536,  536,    0,
    0,    0,    0,    0,    0,  536,    0,  536,  536,  536,
    0,  484,    0,  484,    0,    0,    0,    0,    0,  536,
  484,  484,  536,    0,  536,    0,    0,    0,    0,    0,
  484,    0,  536,  536,  536,  536,  536,  536,  536,    0,
    0,    0,  484,  484,  484,  484,  484,  484,  484,  484,
  484,  484,  484,  484,  484,  484,  484,    0,    0,    0,
    0,    0,    0,  484,  484,  484,  484,    0,  484,    0,
    0,    0,    0,    0,  484,  484,  484,    0,    0,    0,
  484,  484,    0,    0,    0,    0,    0,    0,  484,    0,
  484,  484,  484,    0,  354,    0,  354,    0,    0,    0,
    0,    0,  484,  354,  354,  484,    0,  484,    0,    0,
    0,    0,    0,  354,    0,  484,  484,  484,  484,  484,
  484,  484,    0,    0,    0,  354,  354,  354,  354,  354,
  354,  354,  354,  354,  354,  354,  354,  354,  354,  354,
    0,    0,    0,    0,    0,    0,  354,  354,  354,  354,
    0,  354,    0,    0,    0,    0,    0,  354,  354,  354,
    0,    0,    0,  354,  354,    0,    0,    0,    0,  460,
    0,  354,    0,  354,  354,  354,    0,    0,    3,    0,
    0,    0,    0,    0,    0,  354,    0,    0,  354,    0,
  354,    0,    0,    0,    0,    0,    0,    0,  354,  354,
  354,  354,  354,  354,  354,    4,    0,    5,    6,    7,
    0,    8,    9,    0,    0,    0,   10,   11,   12,    0,
    0,    0,   13,   14,   15,    0,    0,    0,    0,    0,
   16,   17,   18,  461,   19,    0,    0,    0,    0,   20,
   21,    0,    3,    0,    0,    0,    0,    0,   22,   23,
    0,    0,   24,    0,   25,    0,    0,    0,    0,   26,
   27,    0,   28,   29,   30,   31,   32,   33,   34,   35,
    0,  462,    0,    0,    0,    8,    9,    0,    0,  463,
    0,   11,  756,    0,    0,    0,   13,   14,   15,    0,
    0,  756,    0,    0,   16,    0,    0,    0,    0,    0,
  464,  465,  466,   20,   21,  467,    0,    0,    0,    0,
    0,  468,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,  756,    0,    0,   29,   30,   31,
   32,   33,   34,   35,  756,    0,    0,    0,  756,  756,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,  756,  756,    0,  756,    3,    0,    0,    0,
    0,    0,    0,    0,  756,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,  756,  756,  756,  756,
  756,  756,  756,    4,    0,    5,    6,    7,    0,    8,
    9,    0,    0,    0,   10,   11,   12,    0,    0,    0,
   13,   14,   15,    0,    0,    0,    0,    0,   16,   17,
   18,    0,   19,    0,    0,    0,    0,   20,   21,    0,
    0,    0,    0,    0,    0,    0,   22,   23,    0,    0,
   24,    0,   25,    0,    0,    0,    0,   26,   27,    0,
   28,   29,   30,   31,   32,   33,   34,   35,
  };
  protected static readonly short [] yyCheck = {             7,
   28,  353,  798,   28,   17,  150,   19,  598,    2,  199,
  607,  123,   78,  248,  358,   23,   29,   30,   31,   32,
    8, 1054,   79,   80,  520,  990,  814, 1075,  182,  270,
  272,  827,  264,  265,  130,  338, 1094,    6,  268,  374,
   97,  260,  264,  259,  594,  261,  262,  594,   56,  338,
  594,  286,  287,   22,  594,  358,  122,  338,   71,  265,
   73,   86,  261,  338,  261,  861,  270,  374,  272,  358,
  359,  282,  860,   17,  273,   19,  260,  358,  259,  273,
  261,  262,   95,  358,   92, 1340,   94,  434,  101,  436,
  437,  270,  105,  150,  259,  279,  261,  262,  694,  280,
  128,  697,  281,  264,  265,  269,  131,  264,  259,  273,
  261,  262,  273,  260,  264,  268,  129, 1004,  264,  127,
  268,  274,  130,  151,  272, 1380,  264,   71,  259,  154,
  261,  262,  279,  267,  260, 1390,  248,  274,  270,  167,
  272,  129,  185,  186,  187,  188,  189,  285,  338,  280,
  331,  503,  289,  279,  250,  264,  259,  101,  261,  262,
  371,  105,  150,  385,  386,  387,  154,  270,  358,  359,
  360,  361,  272,  389,  390,  261,  726,  264,  522,  726,
  314,  371,  726,  515,  271,  198,  726,  200,  274,  246,
  409,  259,  268,  261,  262,  282,  264,  261,  274,  415,
  331,  270,  298,  271,  259,  199,  261,  262,  758,  271,
  274,  758,  280,  259,  758,  261,  262,  285,  758,  232,
  282,  453,  454,  259,  270,  261,  262,  259,  374,  261,
  262,  264,  265,  270,  270,  582,  244,  245,  270,  272,
  385,  273,  273,  280,  232,  341,  543,  453,  454, 1136,
  272,  282,  264,  356,  357,  358,  200, 1045, 1306,  500,
  282,  273,  358,  331,  417,  287,  508,  280,  503,  279,
  866,  618,  264,  264,  274,  375,  376,  377,  378,  379,
  271,  273,  295,  261,  514, 1073,  311,  506,  310,  289,
  298, 1349,  453,  454,  285,  320,  264,  265,  448,  449,
  450,  480,  269,  274,  416,  273,  270,  274,  321,  317,
  356,  357,  358,  326,  327,  328,  329,  330,  289,  374,
  356,  357,  358,  268,  356,  357,  358,  335,  336, 1387,
 1378,  485,  828,  271,  342,  274,  274,  345, 1303,  347,
  416,  417, 1375,  337,  338,  544,  274,  544,  692,  565,
  289,  295,  516,  517,  282,  368,  268,  551,  552,  506,
  505,  274,  274,  388,  358,  359,  360,  361,  533,  282,
  383,  260,  260,  381,  721,  556,  389,  371,  391,  268,
  349,  389,  533,  272,  496,  451,  452,  412,  260,  261,
  981,  264,  495,  418,  268,  423,  508,  385,  464,  260,
  273,  389,  274,  275,  272,  433,  267,  420,  504,  417,
  467,  279,  259,  268,  261,  262,  268,  513,  514,  274,
  453,  454,  268,  267,  368,  556,  522,  271,  259,  442,
  261,  262,  259,  288,  261,  262,  548,  269,  264,  383,
  990,  507,  274,  990,  510,  557,  990,  273,  268,  495,
  990, 1247,  509,  556,  520,  268,  556,  465,  558,  495,
  468,  267, 1250,  495,  699,  271,  701,  268,  703,  704,
  705,  264,  707,  495,  270, 1072,  420,  273,  370,  306,
  273,  506,  268,  310,  280,  453,  454,  722,  556,  259,
  260,  261,  262,  501,  264,  264,  504,  272,  442,  595,
  270,  260,  846,  279,  273,  513,  514,  515,  544,  268,
  556,  519,  544,  272,  522,  523,  279,  505,  506,  282,
  556,  270,  430,  545,  556,  259,  259,  261,  261,  832,
  833,  280,  281,  502,  503,  543,  270,  633,  773,  288,
  273,  274,  638,  846, 1094,  553,  270, 1094,  272,  562,
 1094,  832,  833,  561, 1094,  667,  272,  846,  450,  451,
  573,  416,  417,  269,  577,  846,  267,  633,  274,  452,
  264,  846, 1122,  456,  457, 1122,  672,  634, 1122,  273,
 1368,  270, 1122,  430,  273,  306,  356,  357,  358,  310,
  473,  280,  259,  601,  261,  262,  692,  273,  274,  430,
  274,  626,  645,  646,  647,  648,  282,  272,  651,  652,
  722,  503,  504,  505,  849,  623,  259,  625,  261, 1415,
  495,  264,  630,  264,  632,  691, 1414,  264,  259,  573,
  261,  262,  273,  577,  269,  384,  273,  270,  273,  270,
  270,  284,  832,  833,  269,  670,  371,  280,  281,  371,
  280,  281,  259,  278,  261,  288,  846,  264,  288,  371,
  259,  268,  261,  775,  672,  264,  375,  376,  377,  378,
  379,  679,  259,  273,  261,  262, 1464,  685,  913,  270,
  450,  272,  282,  270,  692,  273,  274,  799,  535,  536,
  698,  538,  539,  540,  263,  264,  709,  710,  380,  381,
  382,  383,  263,  264,  535,  536,  714,  538,  539,  540,
  283,  807,  559,  560,  561,  259,  371,  261,  262,  727,
  272,  271,  389,  390,  274,  356,  357,  358,  559,  560,
  561,  272,  726, 1085,  259, 1087,  261,  262,  363,  364,
  365,  366,  367,  368,  369,  441,  442,  496,  497,  498,
  846,  280,  259, 1303,  261,  262, 1303,  273,  274, 1303,
  267,  268,  306, 1303,  537,  709,  310,  259,  864,  261,
  262,  799,  271,  273,  870,  274,  283,  259,  806,  261,
  262,  551,  552,  551,  552,  798,  556,  448,  449,  450,
  451,  799,  448,  449,  450,  451,  259,  271,  261, 1349,
  274,  826, 1349,  553,  554, 1349,  831,  272,  270, 1349,
  272,  270,  825,  272,  827,  927,  270,  473,  272,  271,
  270,  829,  272,  270,  890,  272,  375,  376,  377,  378,
  379,  897,  857,  510,  511,  512,  513, 1387,  832,  833,
 1387,  261,  270, 1387,  272,  553,  554, 1387,  861,  558,
  272,  270,  846,  272,  270,  863,  272,  865,  270,  270,
  272,  272,  870,  496,  497,  498,  496,  497,  498,  270,
  878,  272,  270,  272,  272,  883, 1021, 1022, 1023, 1024,
  371,  825,  978,  896,  376,  278, 1031, 1032, 1033,  273,
  274,  516,  517,  270,  270,  272,  272,  389,  390,  282,
  269,  393,  270,  395,  272,  270,  270,  272,  272,  278,
  402,  266,  404,  405,  406,  407,  408,  409,  410,  411,
  269,  413,  414,  415,  272,  556,  273,  274,  269,  278,
  379,  271,  389,  390,  391,    0,  305,  278,  307,  396,
  397,  398,  311,  273,  274,  271, 1012,  271,  374,  318,
 1046,  473,  896,  272,  272,  288,  305, 1014,  307, 1311,
 1312,  371,  311,  273,  273,  276,  279,  272,  976,  318,
  363,  364,  365,  366,  367,  368,  369,  985,  321,  428,
  429,  430,  431,  432,  433,  434,  435,  436,  437,  438,
 1225,  274,  260,  272,  363,  364,  365,  366,  367,  368,
  369, 1009, 1114, 1069,  260,  271, 1019,  556,  273,  558,
 1122,  285,  275,  500,  363,  364,  365,  366,  367,  368,
  369,  268,  363,  364,  365,  366,  367,  368,  369,  281,
  268, 1383, 1384, 1021, 1022, 1023, 1024,  273, 1046,  273,
  273,  273,  273, 1031, 1032, 1033,  272,  268,  272,  272,
  542,  273,  270,  273,  534,  370,  548,  374, 1154, 1155,
  281, 1089,  275,  279, 1160,  557, 1074,  279, 1076,  279,
 1078,  279, 1080,  273, 1082, 1019, 1084,  271, 1086,  288,
 1088,  279, 1090,  279, 1141, 1093,  261, 1095,  273, 1097,
  261, 1099,  541, 1101,  272, 1103,  279, 1209, 1164,  273,
 1166,  273,  507, 1111, 1112,  273,  276,  272, 1077,  285,
 1079, 1119, 1081,  279, 1083,  276, 1085, 1142, 1087,  509,
  509,  269, 1091,  516,  517,  275, 1134, 1135,  275, 1098,
  278, 1100,  270, 1102,  272, 1104,  260,  269,  389,  390,
  391,  275,  280,  281,  416,  396,  397,  398,  477,  471,
  288, 1159,  464, 1161, 1162,  272,  447,  305,  273,  307,
  272,  275,  275,  311,  370,  271,    0,  271,  271,  271,
  318,  272,  259,  273,  261,  516,  517, 1205,  268,  261,
  279,  268,  273, 1249,  273,  273,  494,  556,  273,  417,
  274,  271,  273,  271,  281, 1208,  448,  449,  450,  451,
  273,  453,  454,  274,  269,  272,  270,  556, 1216,  271,
  270,  274,  271,  278,  279,  363,  364,  365,  366,  367,
  368,  369,  279, 1248,  272,  272,  268,  448,  449,  450,
  451,  314,  453,  454, 1247,  273,  507,  271,  371, 1351,
  305,  273,  307,  308,  309,  509,  311,  312,  509,  501,
  315,  316,  317,  318,  506,  275,  275,  322,  323,  324,
  274, 1327,  271,  273, 1208,  330,  331,  332,  273,  334,
  261,  336,  337,  338,  339,  340,  341,  276,  272,  269,
  501,  273,  347,  348,  349,  506,  273,  352,  271,  354,
  271,  273,  271,  273,  359,  360,  273,  362,  363,  364,
  365,  366,  367,  368,  369,  271,  273,  271,  273,  271,
  273,  271,  273, 1321, 1322, 1323,  273,  273,  271,  261,
  271, 1329, 1330,  447,  448,  449,  450,  451,  273,  453,
  454, 1443,  456,  273,  458,  459,  460,  461, 1307, 1308,
 1309, 1310, 1311, 1312, 1313,  273, 1315, 1316, 1317, 1318,
  271,  273,  271,  273,  271,  273,  271, 1385,  496,  497,
  498,  448,  449,  450,  451,  273,  453,  454,  273,  259,
  273,  261,  262,  271,  273, 1383,  271,  501,  271,  273,
  270,  272,  506,  271,  271,  289,  272,  272,  272,  272,
  280,  281,  272,  272,  272,  259,  272,  261,  288,  272,
  272,  272, 1415,  273,  273, 1413,  270,  275,  556,  275,
  275, 1419,  275, 1421,  501, 1384,  280,  281,  275,  506,
  544,  272,  272,  272,  288,  259,  260,  261,  262,  272,
  272, 1439,  272, 1441,  268,  269,  270,  271,  272,  273,
  274,  272,  276,  272,  278,  279,  280,  281,  273,  283,
  284,  285,  272,  272,  288,  289,  290,  291,  292,  293,
  294,  295,  296,  297,  298,  299,  300,  301,  302,  303,
  304,  305,  306,  307,  308,  309,  310,  311,  312,  313,
  314,  315,  316,  317,  318,  319,  320,  321,  322,  323,
  324,  325,  326,    0,  328,  329,  330,  331,  332,  272,
  334,  335,  336,  337,  338,  339,  340,  341,  342,  343,
  344,  345,  346,  347,  348,  349,  350,  351,  352,  353,
  354,  355,  356,  357,  358,  359,  360,  272,  362,  363,
  364,  365,  366,  367,  368,  369,  370,  371,  272,  272,
  272,  261,  447,  448,  449,  450,  451,  271,  453,  454,
  384,  456,  279,  458,  459,  460,  461,  271,  270,  272,
  288,  276,  273,  273,  273,  273,  273,  273,  402,  273,
  273,  273,  273,  407,  273,  273,  273,  273,  273,  273,
  273,  273,  416,  417,  272,  261,  273,  259,  273,  261,
  262,  272,  271,  273,  428,  500,  501,    0,  269,  268,
  273,  506,  268,  268,  274,  273,  496,  497,  498,  272,
  268,  273,  446,  447,  448,  449,  450,  451,  272,  453,
  454,  273,  456,  273,  458,  459,  460,  461,  273,  273,
  273,  273,  496,  497,  498,  272,  180,  273,  273,  270,
  272,  268,  268,  497,  549,  550,   28,  338,  338,  280,
  281,   28,  338,   28, 1133,  338,  361,  288,  888,  591,
  494,  495,  496,  497,  498,  499,  500,  501, 1108,  990,
  625,   11,  506,  507,  278, 1050,  504,  655, 1051,  758,
  514,  314,  516,  517,  695,  506,  510, 1046, 1391, 1389,
 1392,   21,    0,  801,  980,  806,   70,  371,   -1,  360,
  430,   -1,   -1,   -1,  376,   -1,   -1,   -1,   -1,   -1,
  544,  545,   -1,   -1,   -1,  549,  550,  389,  390,   -1,
   -1,  393,  556,  395,   -1,   -1,   -1,   -1,   -1,   -1,
  402,   -1,  404,  405,  406,  407,  408,  409,  410,  411,
  371,  413,  414,  415,  268,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,  259,   -1,  261,  262,   -1,  281,   -1,   -1,
   -1,  268,  269,  270,   -1,  272,  273,  274,   -1,  276,
   -1,  278,  279,  280,  281,   -1,  283,  284,  285,   -1,
  273,  288,  289,  290,  291,  292,  293,  294,  295,  296,
  297,  298,  299,  300,  301,  302,  303,  304,  305,  306,
  307,  308,  309,  310,  311,  312,  313,  314,  315,  316,
  317,  318,    0,   -1,  321,  322,  323,  324,  325,  326,
   -1,  328,  329,  330,  331,  332,   -1,  334,   -1,  336,
  337,  338,  339,  340,  341,  342,   -1,   -1,   -1,   -1,
  347,  348,  349,  350,  351,  352,  353,  354,  355,  356,
  357,  358,  359,  360,   -1,  362,  363,  364,  365,  366,
  367,  368,  369,   -1,  371,  496,  497,  498,   -1,   -1,
  542,   -1,   -1,   -1,   -1,   -1,  548,  384,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,  557,   -1,  380,  381,  382,
  383,   -1,  384,   -1,   -1,  402,  388,  389,  390,  391,
  392,  393,   -1,  395,  396,  397,  398,  399,  400,  401,
  417,   -1,   -1,   -1,   -1,   -1,  409,  410,  411,  412,
   -1,  428,   -1,   -1,  448,  449,  450,  451,   -1,  453,
  454,  424,  425,  426,  427,   -1,   -1,   -1,   -1,  446,
  447,  448,  449,  450,  451,   -1,  453,  454,   -1,  456,
   -1,  458,  459,  460,  461,   -1,   -1,   -1,   -1,   -1,
   -1,  269,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
  278,   -1,   -1,   -1,   -1,   -1,   -1,  501,   -1,   -1,
   -1,   -1,  506,   -1,   -1,   -1,   -1,  494,  495,  496,
  497,  498,  499,   -1,  501,   -1,   -1,  305,   -1,  307,
  308,  309,   -1,  311,  312,   -1,   -1,  514,  316,  317,
  318,    0,   -1,  321,  322,  323,  324,   -1,   -1,   -1,
   -1,   -1,  330,  331,  332,   -1,  334,   -1,   -1,   -1,
   -1,  339,  340,   -1,   -1,   -1,   -1,  544,  545,   -1,
  348,  349,  549,  550,  352,   -1,  354,   -1,   -1,  556,
   -1,  359,  360,   -1,  362,  363,  364,  365,  366,  367,
  368,  369,  555,   -1,  557,   -1,   -1,   -1,   -1,   -1,
   -1,  259,   -1,  261,  262,   -1,   -1,   -1,   -1,   -1,
  268,  269,  270,   -1,  272,  273,  274,   -1,  276,   -1,
  278,  279,  280,  281,   -1,  283,  284,  285,   -1,  273,
  288,  289,  290,  291,  292,  293,  294,  295,  296,  297,
  298,  299,  300,  301,  302,  303,  304,  305,  306,  307,
  308,  309,  310,  311,  312,  313,  314,  315,  316,  317,
  318,    0,   -1,  321,  322,  323,  324,  325,  326,   -1,
  328,  329,  330,  331,  332,   -1,  334,   -1,  336,  337,
  338,  339,  340,  341,  342,   -1,   -1,   -1,   -1,  347,
  348,  349,  350,  351,  352,  353,  354,  355,  356,  357,
  358,  359,  360,   -1,  362,  363,  364,  365,  366,  367,
  368,  369,   -1,  371,   -1,   -1,   -1,   -1,  447,  448,
  449,  450,  451,   -1,  453,  454,  384,  456,   -1,  458,
  459,  460,  461,   -1,   -1,   -1,  380,  381,  382,  383,
   -1,   -1,   -1,   -1,  402,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  417,
   -1,   -1,   -1,   -1,   -1,  409,  410,  411,  412,   -1,
  428,  500,  501,   -1,   -1,   -1,   -1,  506,   -1,   -1,
  424,  425,  426,  427,   -1,   -1,   -1,   -1,  446,  447,
  448,  449,  450,  451,   -1,  453,  454,   -1,  456,   -1,
  458,  459,  460,  461,   -1,   -1,   -1,   -1,   -1,   -1,
  269,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  278,
  549,  550,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,  494,  495,  496,  497,
  498,  499,   -1,  501,   -1,   -1,  305,   -1,  307,  308,
  309,   -1,  311,  312,   -1,   -1,  514,  316,  317,  318,
    0,   -1,   -1,  322,  323,  324,   -1,   -1,   -1,   -1,
   -1,  330,  331,  332,   -1,  334,   -1,   -1,   -1,   -1,
  339,  340,   -1,   -1,   -1,   -1,  544,  545,   -1,  348,
  349,  549,  550,  352,   -1,  354,   -1,   -1,  556,   -1,
  359,  360,   -1,  362,  363,  364,  365,  366,  367,  368,
  369,  555,   -1,  557,   -1,   -1,   -1,   -1,   -1,   -1,
  259,   -1,  261,  262,   -1,   -1,   -1,   -1,   -1,  268,
  269,  270,   -1,  272,  273,  274,   -1,  276,   -1,  278,
  279,  280,  281,   -1,   -1,  284,  285,   -1,  273,  288,
  289,  290,  291,  292,  293,  294,  295,  296,  297,  298,
  299,  300,  301,  302,  303,  304,  305,  306,  307,  308,
  309,  310,  311,  312,  313,  314,  315,  316,  317,  318,
    0,   -1,  321,  322,  323,  324,  325,  326,   -1,  328,
  329,  330,  331,  332,   -1,  334,   -1,  336,  337,  338,
  339,  340,  341,  342,   -1,   -1,   -1,   -1,  347,  348,
  349,  350,  351,  352,  353,  354,  355,  356,  357,  358,
  359,  360,   -1,  362,  363,  364,  365,  366,  367,  368,
  369,   -1,  371,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
  447,  448,  449,  450,  451,  384,  453,  454,   -1,  456,
   -1,  458,  459,  460,  461,  380,  381,  382,  383,   -1,
   -1,   -1,   -1,  402,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  417,   -1,
   -1,   -1,   -1,   -1,  409,  410,  411,  412,   -1,  428,
   -1,   -1,   -1,  500,  501,   -1,   -1,   -1,   -1,  424,
  425,  426,  427,   -1,   -1,   -1,   -1,  446,  447,  448,
  449,  450,  451,   -1,  453,  454,   -1,  456,   -1,  458,
  459,  460,  461,   -1,   -1,   -1,   -1,   -1,   -1,  269,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  278,   -1,
   -1,  548,  549,  550,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,  494,  495,  496,  497,  498,
  499,   -1,  501,   -1,   -1,  305,   -1,  307,  308,  309,
   -1,  311,  312,   -1,   -1,  514,  316,  317,  318,    0,
   -1,   -1,  322,  323,  324,   -1,   -1,   -1,   -1,   -1,
  330,  331,  332,   -1,  334,   -1,   -1,   -1,   -1,  339,
  340,   -1,   -1,   -1,   -1,  544,  545,   -1,  348,  349,
  549,  550,  352,   -1,  354,   -1,   -1,  556,   -1,  359,
  360,   -1,  362,  363,  364,  365,  366,  367,  368,  369,
  555,   -1,  557,   -1,   -1,   -1,   -1,   -1,   -1,  259,
   -1,  261,  262,   -1,   -1,   -1,   -1,   -1,  268,  269,
  270,   -1,  272,  273,  274,   -1,  276,   -1,  278,  279,
  280,  281,   -1,   -1,  284,  285,   -1,  273,  288,  289,
  290,  291,  292,  293,  294,  295,  296,  297,  298,  299,
  300,  301,  302,  303,  304,  305,  306,  307,  308,  309,
  310,  311,  312,  313,  314,  315,  316,  317,  318,    0,
   -1,  321,  322,  323,  324,  325,  326,   -1,  328,  329,
  330,  331,  332,   -1,  334,  269,  336,  337,  338,  339,
  340,  341,  342,   -1,  278,   -1,   -1,  347,  348,  349,
  350,  351,  352,  353,  354,  355,  356,  357,  358,  359,
  360,   -1,  362,  363,  364,  365,  366,  367,  368,  369,
   -1,  371,   -1,   -1,   -1,   -1,   -1,  311,   -1,   -1,
   -1,   -1,   -1,   -1,  384,  319,   -1,   -1,  322,  323,
  324,   -1,   -1,   -1,  380,  381,  382,  383,   -1,   -1,
   -1,  335,  402,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
  344,  345,   -1,   -1,   -1,   -1,   -1,  417,   -1,   -1,
   -1,   -1,   -1,  409,  410,  411,  412,   -1,  428,  363,
  364,  365,  366,  367,  368,  369,   -1,   -1,  424,  425,
  426,  427,   -1,   -1,   -1,   -1,  446,  447,  448,  449,
  450,  451,   -1,  453,  454,   -1,  456,   -1,  458,  459,
  460,  461,   -1,   -1,   -1,   -1,   -1,   -1,  269,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,  278,  518,  519,
  520,  521,  522,  523,  524,  525,  526,  527,  528,  529,
  530,  531,  532,   -1,  494,  495,  496,  497,  498,  499,
   -1,  501,   -1,   -1,  305,   -1,  307,  308,  309,   -1,
  311,  312,   -1,   -1,  514,  316,  317,  318,    0,   -1,
   -1,  322,  323,  324,   -1,   -1,   -1,   -1,   -1,  330,
  331,  332,   -1,  334,   -1,   -1,   -1,   -1,  339,  340,
   -1,   -1,   -1,   -1,  544,  545,   -1,  348,  349,  549,
  550,  352,   -1,  354,   -1,   -1,  556,   -1,  359,  360,
   -1,  362,  363,  364,  365,  366,  367,  368,  369,  555,
   -1,  557,   -1,   -1,   -1,   -1,   -1,   -1,  259,   -1,
  261,  262,   -1,   -1,   -1,   -1,   -1,  268,  269,  270,
   -1,  272,  273,  274,   -1,  276,   -1,  278,  279,  280,
  281,   -1,   -1,  284,  285,   -1,   -1,  288,  289,  290,
  291,  292,  293,  294,  295,  296,  297,  298,  299,  300,
  301,  302,  303,  304,  305,  306,  307,  308,  309,  310,
  311,  312,  313,  314,  315,  316,  317,  318,    0,   -1,
  321,  322,  323,  324,  325,  326,   -1,  328,  329,  330,
  331,  332,   -1,  334,  269,  336,  337,  338,  339,  340,
  341,  342,   -1,  278,   -1,   -1,  347,  348,  349,  350,
  351,  352,  353,  354,  355,  356,  357,  358,  359,  360,
   -1,  362,  363,  364,  365,  366,  367,  368,  369,   -1,
  371,   -1,   -1,   -1,   -1,   -1,  311,   -1,   -1,   -1,
   -1,   -1,   -1,  384,   -1,  320,   -1,  322,  323,  324,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
  335,  402,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,  346,   -1,   -1,   -1,   -1,  417,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,  428,  363,  364,
  365,  366,  367,  368,  369,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,  446,  447,  448,  449,  450,
  451,   -1,  453,  454,   -1,  456,   -1,  458,  459,  460,
  461,   -1,   -1,   -1,   -1,   -1,   -1,  269,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,  278,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,  278,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,  494,  495,  496,  497,  498,  499,   -1,
  501,   -1,   -1,  305,   -1,  307,  308,  309,   -1,  311,
  312,   -1,  305,  514,  316,  317,  318,    0,  311,   -1,
  322,  323,  324,   -1,   -1,  318,   -1,   -1,  330,  331,
  332,   -1,  334,   -1,   -1,   -1,   -1,  339,  340,   -1,
   -1,   -1,   -1,  544,  545,   -1,  348,  349,  549,  550,
  352,   -1,  354,   -1,   -1,  556,   -1,  359,  360,   -1,
  362,  363,  364,  365,  366,  367,  368,  369,   -1,   -1,
  363,  364,  365,  366,  367,  368,  369,  259,   -1,  261,
  262,   -1,   -1,   -1,   -1,   -1,  268,  269,  270,   -1,
  272,  273,  274,   -1,  276,   -1,  278,  279,  280,  281,
   -1,   -1,  284,  285,   -1,   -1,  288,  289,  290,  291,
  292,  293,  294,  295,  296,  297,  298,  299,  300,  301,
  302,  303,  304,  305,  306,  307,  308,  309,  310,  311,
  312,  313,  314,  315,  316,  317,  318,    0,   -1,  321,
  322,  323,  324,  325,  326,   -1,  328,  329,  330,  331,
  332,   -1,  334,   -1,  336,  337,  338,  339,  340,  341,
  342,   -1,   -1,   -1,   -1,  347,  348,  349,  350,  351,
  352,  353,  354,  355,  356,  357,  358,  359,  360,   -1,
  362,  363,  364,  365,  366,  367,  368,  369,   -1,  371,
   -1,   -1,   -1,   -1,  269,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,  384,  278,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
  402,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
  305,   -1,   -1,   -1,   -1,  417,  311,   -1,   -1,   -1,
   -1,   -1,   -1,  318,   -1,   -1,  428,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,  446,  447,  448,  449,  450,  451,
   -1,  453,  454,   -1,  456,   -1,  458,  459,  460,  461,
   -1,   -1,   -1,   -1,   -1,   -1,  269,   -1,  363,  364,
  365,  366,  367,  368,  369,  278,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,  494,  495,  496,  497,  498,  499,   -1,  501,
   -1,   -1,  305,   -1,  307,  308,  309,   -1,  311,  312,
   -1,   -1,  514,  316,  317,  318,    0,   -1,   -1,  322,
  323,  324,   -1,   -1,   -1,   -1,   -1,  330,  331,  332,
   -1,  334,   -1,   -1,   -1,   -1,  339,  340,   -1,   -1,
   -1,   -1,  544,  545,   -1,  348,  349,  549,  550,  352,
   -1,  354,   -1,   -1,  556,   -1,  359,  360,   -1,  362,
  363,  364,  365,  366,  367,  368,  369,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,  259,   -1,  261,  262,
   -1,   -1,   -1,   -1,   -1,  268,  269,  270,   -1,  272,
  273,  274,   -1,  276,   -1,  278,  279,  280,  281,   -1,
   -1,  284,  285,   -1,   -1,  288,  289,  290,  291,  292,
  293,  294,  295,  296,  297,  298,  299,  300,  301,  302,
  303,  304,  305,  306,  307,  308,  309,  310,  311,  312,
  313,  314,  315,  316,  317,  318,    0,   -1,  321,  322,
  323,  324,  325,  326,   -1,  328,  329,  330,  331,  332,
   -1,  334,   -1,  336,  337,  338,  339,  340,  341,  342,
   -1,   -1,   -1,   -1,  347,  348,  349,  350,  351,  352,
  353,  354,  355,  356,  357,  358,  359,  360,   -1,  362,
  363,  364,  365,  366,  367,  368,  369,   -1,  371,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,  384,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  402,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,  417,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,  428,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,  446,  447,  448,  449,  450,  451,   -1,
  453,  454,   -1,  456,   -1,  458,  459,  460,  461,   -1,
   -1,   -1,   -1,   -1,   -1,  269,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,    0,  278,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,  494,  495,  496,  497,  498,  499,   -1,  501,   -1,
   -1,  305,   -1,  307,  308,  309,   -1,  311,  312,   -1,
   -1,  514,  316,  317,  318,   -1,   -1,   -1,  322,  323,
  324,   -1,   -1,   -1,   -1,   -1,  330,  331,  332,   -1,
  334,   -1,   -1,   -1,   -1,  339,  340,   -1,   -1,   -1,
   -1,  544,  545,   -1,  348,  349,  549,  550,  352,   -1,
  354,   -1,   -1,  556,   -1,  359,  360,   -1,  362,  363,
  364,  365,  366,  367,  368,  369,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,  259,   -1,  261,  262,   -1,
   -1,   -1,   -1,   -1,  268,  269,    0,   -1,  272,  273,
  274,   -1,  276,   -1,  278,  279,   -1,   -1,   -1,   -1,
  284,  285,   -1,   -1,   -1,  289,  290,  291,  292,  293,
  294,  295,  296,  297,  298,  299,  300,  301,  302,  303,
  304,  305,  306,  307,  308,  309,  310,  311,  312,  313,
  314,  315,  316,  317,  318,   -1,   -1,  321,  322,  323,
  324,  325,  326,   -1,  328,  329,  330,  331,  332,   -1,
  334,   -1,  336,  337,  338,  339,  340,  341,  342,   -1,
   -1,   -1,   -1,  347,  348,  349,  350,  351,  352,  353,
  354,  355,  356,  357,  358,  359,  360,   -1,  362,  363,
  364,  365,  366,  367,  368,  369,   -1,  371,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
  384,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  402,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,  417,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,  269,   -1,  428,   -1,   -1,   -1,   -1,   -1,
   -1,  278,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,  446,  447,  448,  449,  450,  451,   -1,  453,
  454,   -1,  456,   -1,  458,  459,  460,  461,  305,   -1,
  307,  308,  309,   -1,  311,  312,   -1,   -1,   -1,  316,
  317,  318,   -1,   -1,   -1,  322,  323,  324,   -1,   -1,
   -1,   -1,   -1,  330,  331,  332,   -1,  334,   -1,   -1,
  494,  495,  339,  340,   -1,  499,   -1,  501,   -1,   -1,
   -1,  348,  349,   -1,   -1,  352,   -1,  354,   -1,  270,
  514,   -1,  359,  360,   -1,  362,  363,  364,  365,  366,
  367,  368,  369,  284,  285,  259,   -1,  261,   -1,   -1,
   -1,   -1,   -1,    0,  268,  269,   -1,   -1,   -1,  273,
   -1,  545,   -1,   -1,  278,  549,  550,   -1,   -1,   -1,
   -1,   -1,  556,   -1,   -1,   -1,  290,  291,  292,  293,
  294,  295,  296,  297,  298,  299,  300,  301,  302,  303,
  304,  305,   -1,  307,  308,  309,   -1,  311,  312,  313,
  314,  315,  316,  317,  318,  319,  320,  321,  322,  323,
  324,  325,  326,   -1,  328,  329,  330,  331,  332,   -1,
  334,  335,  336,  337,  338,  339,  340,  341,  342,  343,
  344,  345,  346,  347,  348,  349,  350,  351,  352,  353,
  354,  355,   -1,   -1,   -1,  359,  360,   -1,  362,  363,
  364,  365,  366,  367,  368,  369,   -1,   -1,   -1,   -1,
   -1,  402,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,  428,   -1,   -1,
   -1,   -1,   -1,  407,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,  446,  447,  448,  449,  450,
  451,   -1,  453,  454,   -1,  456,   -1,  458,  459,  460,
  461,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,  447,  448,  449,  450,  451,   -1,  453,
  454,   -1,  456,   -1,  458,  459,  460,  461,   -1,   -1,
   -1,   -1,   -1,  494,  495,   -1,   -1,   -1,  499,   -1,
  501,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,  259,   -1,  261,   -1,   -1,
   -1,   -1,   -1,   -1,  268,   -1,  500,  501,   -1,   -1,
   -1,   -1,  506,   -1,   -1,   -1,  270,  281,   -1,   -1,
   -1,   -1,  516,  517,  545,   -1,   -1,   -1,  549,  550,
  284,  285,  259,   -1,  261,   -1,   -1,   -1,   -1,   -1,
    0,  268,  269,   -1,   -1,   -1,  273,   -1,   -1,   -1,
   -1,  278,   -1,   -1,   -1,  549,  550,   -1,   -1,   -1,
   -1,   -1,  556,  290,  291,  292,  293,  294,  295,  296,
  297,  298,  299,  300,  301,  302,  303,  304,  305,   -1,
  307,  308,  309,   -1,  311,  312,  313,  314,  315,  316,
  317,  318,  319,  320,  321,  322,  323,  324,  325,  326,
   -1,  328,  329,  330,  331,  332,   -1,  334,  335,  336,
  337,  338,  339,  340,  341,  342,  343,  344,  345,  346,
  347,  348,  349,  350,  351,  352,  353,  354,  355,   -1,
   -1,   -1,  359,  360,   -1,  362,  363,  364,  365,  366,
  367,  368,  369,   -1,   -1,   -1,   -1,   -1,  402,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,  430,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,  428,   -1,   -1,   -1,   -1,   -1,
  407,   -1,   -1,   -1,  448,  449,  450,  451,   -1,  453,
  454,   -1,  446,  447,  448,  449,  450,  451,   -1,  453,
  454,   -1,  456,   -1,  458,  459,  460,  461,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
  447,  448,  449,  450,  451,   -1,  453,  454,   -1,  456,
   -1,  458,  459,  460,  461,   -1,   -1,  501,   -1,   -1,
  494,  495,  506,   -1,   -1,  499,   -1,  501,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,  500,  501,   -1,   -1,   -1,   -1,  506,
  270,   -1,   -1,  273,   -1,   -1,   -1,   -1,   -1,  516,
  517,  545,   -1,   -1,  284,  549,  550,   -1,   -1,  259,
   -1,  261,   -1,   -1,   -1,   -1,   -1,    0,  268,  269,
   -1,   -1,   -1,  273,   -1,   -1,   -1,   -1,  278,   -1,
   -1,   -1,  549,  550,   -1,   -1,   -1,   -1,   -1,  556,
  290,  291,  292,  293,  294,  295,  296,  297,  298,  299,
  300,  301,  302,  303,  304,  305,   -1,  307,  308,  309,
   -1,  311,  312,  313,  314,  315,  316,  317,  318,  319,
  320,  321,  322,  323,  324,  325,  326,   -1,  328,  329,
  330,  331,  332,   -1,  334,  335,  336,  337,  338,  339,
  340,  341,  342,  343,  344,  345,  346,  347,  348,  349,
  350,  351,  352,  353,  354,  355,   -1,   -1,   -1,  359,
  360,   -1,  362,  363,  364,  365,  366,  367,  368,  369,
   -1,   -1,  402,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  428,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,  407,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,  446,  447,  448,  449,
  450,  451,   -1,  453,  454,   -1,  456,   -1,  458,  459,
  460,  461,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,  447,  448,  449,
  450,  451,   -1,  453,  454,   -1,  456,   -1,  458,  459,
  460,  461,   -1,   -1,  494,  495,   -1,   -1,   -1,  499,
   -1,  501,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
  500,  501,   -1,   -1,   -1,   -1,  506,  270,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,  545,  516,  517,   -1,  549,
  550,  284,   -1,   -1,   -1,   -1,  259,   -1,  261,   -1,
   -1,   -1,   -1,   -1,    0,  268,  269,   -1,   -1,   -1,
  273,   -1,   -1,   -1,   -1,  278,   -1,   -1,   -1,  549,
  550,   -1,   -1,   -1,   -1,   -1,  556,  290,  291,  292,
  293,  294,  295,  296,  297,  298,  299,  300,  301,  302,
  303,  304,  305,   -1,  307,  308,  309,   -1,  311,  312,
  313,  314,  315,  316,  317,  318,  319,  320,  321,  322,
  323,  324,  325,  326,   -1,  328,  329,  330,  331,  332,
   -1,  334,  335,  336,  337,  338,  339,  340,  341,  342,
  343,  344,  345,  346,  347,  348,  349,  350,  351,  352,
  353,  354,  355,   -1,   -1,   -1,  359,  360,   -1,  362,
  363,  364,  365,  366,  367,  368,  369,   -1,   -1,  402,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,    0,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,  428,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,  407,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,  446,  447,  448,  449,  450,  451,   -1,
  453,  454,   -1,  456,   -1,  458,  459,  460,  461,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,  447,  448,  449,  450,  451,   -1,
  453,  454,   -1,  456,   -1,  458,  459,  460,  461,   -1,
   -1,  494,  495,   -1,   -1,   -1,  499,   -1,  501,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,  500,  501,   -1,
   -1,   -1,   -1,  506,   -1,   -1,   -1,   -1,   -1,   -1,
    0,   -1,  545,  516,  517,   -1,  549,  550,   -1,   -1,
   -1,   -1,   -1,  259,   -1,  261,   -1,   -1,   -1,   -1,
   -1,   -1,  268,  269,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,  278,   -1,   -1,   -1,  549,  550,   -1,   -1,
   -1,   -1,   -1,  556,  290,  291,  292,  293,  294,  295,
  296,  297,  298,  299,  300,  301,  302,  303,  304,  305,
   -1,  307,  308,  309,   -1,  311,  312,  313,  314,  315,
  316,  317,  318,  319,  320,  321,  322,  323,  324,  325,
  326,   -1,  328,  329,  330,  331,  332,   -1,  334,  335,
  336,  337,  338,  339,  340,  341,  342,  343,  344,  345,
  346,  347,  348,  349,  350,  351,  352,  353,  354,  355,
   -1,   -1,   -1,  359,  360,   -1,  362,  363,  364,  365,
  366,  367,  368,  369,   -1,  371,   -1,   -1,   -1,   -1,
   -1,  259,   -1,  261,   -1,   -1,   -1,   -1,   -1,   -1,
  268,  269,    0,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
  278,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,  407,  290,  291,  292,  293,  294,  295,  296,  297,
  298,  299,  300,  301,  302,  303,  304,  305,   -1,  307,
  308,  309,   -1,  311,  312,  313,  314,  315,  316,  317,
  318,  319,  320,  321,  322,  323,  324,  325,  326,   -1,
  328,  329,  330,  331,  332,   -1,  334,  335,  336,  337,
  338,  339,  340,  341,  342,  343,  344,  345,  346,  347,
  348,  349,  350,  351,  352,  353,  354,  355,   -1,   -1,
   -1,  359,  360,   -1,  362,  363,  364,  365,  366,  367,
  368,  369,   -1,  371,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  259,
   -1,  261,   -1,   -1,   -1,   -1,   -1,   -1,  268,  269,
   -1,   -1,    0,   -1,   -1,   -1,   -1,   -1,  278,  407,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
  290,  291,  292,  293,  294,  295,  296,  297,  298,  299,
  300,  301,  302,  303,  304,  305,   -1,  307,  308,  309,
  556,  311,  312,  313,  314,  315,  316,  317,  318,  319,
  320,  321,  322,  323,  324,  325,  326,   -1,  328,  329,
  330,  331,  332,   -1,  334,  335,  336,  337,  338,  339,
  340,  341,  342,  343,  344,  345,  346,  347,  348,  349,
  350,  351,  352,  353,  354,  355,   -1,   -1,   -1,  359,
  360,   -1,  362,  363,  364,  365,  366,  367,  368,  369,
   -1,  371,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,    0,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,  259,   -1,  261,   -1,   -1,   -1,  407,   -1,   -1,
  268,  269,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
  278,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  556,   -1,
   -1,   -1,  290,  291,  292,  293,  294,  295,  296,  297,
  298,  299,  300,  301,  302,  303,  304,  305,   -1,  307,
  308,  309,   -1,  311,  312,  313,  314,  315,  316,  317,
  318,  319,  320,  321,  322,  323,  324,  325,  326,   -1,
  328,  329,  330,  331,  332,   -1,  334,  335,  336,  337,
  338,  339,  340,  341,  342,  343,  344,  345,  346,  347,
  348,  349,  350,  351,  352,  353,  354,  355,   -1,   -1,
   -1,  359,  360,   -1,  362,  363,  364,  365,  366,  367,
  368,  369,   -1,  371,   -1,   -1,   -1,   -1,    0,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,  259,   -1,  261,   -1,   -1,   -1,   -1,   -1,   -1,
  268,  269,  270,  271,  272,  273,  274,   -1,   -1,  407,
  278,  279,   -1,   -1,   -1,   -1,  556,   -1,   -1,   -1,
  288,  289,  290,  291,  292,  293,  294,  295,  296,  297,
  298,  299,  300,  301,  302,  303,  304,  305,   -1,  307,
  308,  309,   -1,  311,  312,  313,  314,   -1,  316,  317,
  318,   -1,   -1,   -1,  322,  323,  324,   -1,   -1,   -1,
  328,  329,  330,  331,  332,   -1,  334,   -1,  336,   -1,
  338,  339,  340,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
  348,  349,  350,   -1,  352,  353,  354,  355,   -1,   -1,
   -1,  359,  360,   -1,  362,  363,  364,  365,  366,  367,
  368,  369,  370,  371,   -1,   -1,    0,  259,   -1,  261,
  262,   -1,   -1,   -1,   -1,   -1,  268,  269,   -1,   -1,
  272,  273,  274,   -1,  276,   -1,  278,  279,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  290,  291,
  292,  293,  294,  295,  296,  297,  298,  299,  300,  301,
  302,  303,  304,  305,   -1,  307,  308,  309,  556,  311,
  312,  313,  314,  315,  316,  317,  318,   -1,   -1,  321,
  322,  323,  324,  325,  326,   -1,  328,  329,  330,  331,
  332,   -1,  334,   -1,  336,  337,  338,  339,  340,  341,
  342,   -1,   -1,   -1,   -1,  347,  348,  349,  350,  351,
  352,  353,  354,  355,   -1,   -1,   -1,  359,  360,   -1,
  362,  363,  364,  365,  366,  367,  368,  369,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,    0,   -1,   -1,   -1,   -1,  507,
   -1,   -1,   -1,   -1,   -1,   -1,  268,  269,   -1,   -1,
  272,  273,  274,   -1,  276,   -1,  278,  279,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,  417,   -1,   -1,  290,  291,
  292,  293,  294,  295,  296,  297,  298,  299,  300,  301,
  302,  303,  304,  305,   -1,  307,  308,  309,  556,  311,
  312,  313,  314,  315,  316,  317,  318,   -1,   -1,  321,
  322,  323,  324,  325,  326,   -1,  328,  329,  330,  331,
  332,   -1,  334,   -1,  336,  337,  338,  339,  340,  341,
  342,   -1,   -1,   -1,   -1,  347,  348,  349,  350,  351,
  352,  353,  354,  355,   -1,   -1,   -1,  359,  360,   -1,
  362,  363,  364,  365,  366,  367,  368,  369,    0,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,  514,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,  268,  269,   -1,   -1,  272,  273,
  274,   -1,  276,   -1,  278,  279,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,  417,  290,  291,  292,  293,
  294,  295,  296,  297,  298,  299,  300,  301,  302,  303,
  304,  305,   -1,  307,  308,  309,   -1,  311,  312,  313,
  314,  315,  316,  317,  318,   -1,   -1,  321,  322,  323,
  324,  325,  326,   -1,  328,  329,  330,  331,  332,   -1,
  334,   -1,  336,  337,  338,  339,  340,  341,  342,   -1,
    0,   -1,   -1,  347,  348,  349,  350,  351,  352,  353,
  354,  355,   -1,   -1,   -1,  359,  360,   -1,  362,  363,
  364,  365,  366,  367,  368,  369,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,  514,  259,   -1,  261,   -1,   -1,   -1,   -1,
   -1,   -1,  268,  269,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,  278,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,  417,  290,  291,  292,  293,  294,  295,
  296,  297,  298,  299,  300,  301,  302,  303,  304,  305,
   -1,  307,  308,  309,   -1,  311,  312,  313,  314,  315,
  316,  317,  318,  319,  320,   -1,  322,  323,  324,   -1,
   -1,    0,  328,  329,  330,  331,  332,   -1,  334,  335,
  336,  337,  338,  339,  340,  341,   -1,   -1,  344,  345,
  346,  347,  348,  349,  350,   -1,  352,  353,  354,  355,
   -1,   -1,   -1,  359,  360,   -1,  362,  363,  364,  365,
  366,  367,  368,  369,   -1,   -1,   -1,  259,   -1,  261,
   -1,   -1,   -1,   -1,   -1,   -1,  268,  269,   -1,   -1,
  514,   -1,   -1,   -1,   -1,   -1,  278,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  290,  291,
  292,  293,  294,  295,  296,  297,  298,  299,  300,  301,
  302,  303,  304,  305,   -1,  307,  308,  309,   -1,  311,
  312,  313,  314,  315,  316,  317,  318,  319,  320,   -1,
  322,  323,  324,    0,   -1,   -1,  328,  329,  330,  331,
  332,   -1,  334,  335,  336,  337,  338,  339,  340,  341,
   -1,   -1,  344,  345,  346,  347,  348,  349,  350,   -1,
  352,  353,  354,  355,   -1,   -1,   -1,  359,  360,  259,
  362,  363,  364,  365,  366,  367,  368,  369,  268,  269,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  278,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
  290,  291,  292,  293,  294,  295,  296,  297,  298,  299,
  300,  301,  302,  303,  304,  305,   -1,  307,  308,  309,
   -1,  311,  312,  313,  314,  315,  316,  317,  318,  319,
  320,   -1,  322,  323,  324,    0,   -1,   -1,  328,  329,
  330,  331,  332,   -1,  334,  335,  336,  337,  338,  339,
  340,  341,   -1,   -1,  344,  345,  346,  347,  348,  349,
  350,   -1,  352,  353,  354,  355,   -1,   -1,   -1,  359,
  360,   -1,  362,  363,  364,  365,  366,  367,  368,  369,
  259,   -1,  261,   -1,   -1,   -1,   -1,   -1,   -1,  268,
  269,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  278,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,  290,  291,  292,  293,  294,  295,  296,  297,  298,
  299,  300,  301,  302,  303,  304,  305,   -1,  307,  308,
  309,   -1,  311,  312,  313,  314,  315,  316,  317,  318,
   -1,   -1,  321,  322,  323,  324,  325,  326,    0,  328,
  329,  330,  331,  332,   -1,  334,   -1,  336,  337,  338,
  339,  340,  341,  342,   -1,   -1,   -1,   -1,  347,  348,
  349,  350,  351,  352,  353,  354,  355,   -1,   -1,   -1,
  359,  360,   -1,  362,  363,  364,  365,  366,  367,  368,
  369,   -1,  259,   -1,  261,   -1,   -1,   -1,   -1,   -1,
   -1,  268,  269,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,  278,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,  290,  291,  292,  293,  294,  295,  296,
  297,  298,  299,  300,  301,  302,  303,  304,  305,   -1,
  307,  308,  309,   -1,  311,  312,  313,  314,  315,  316,
  317,  318,   -1,   -1,  321,  322,  323,  324,  325,  326,
    0,  328,  329,  330,  331,  332,   -1,  334,   -1,  336,
  337,  338,  339,  340,  341,  342,   -1,   -1,   -1,   -1,
  347,  348,  349,  350,  351,  352,  353,  354,  355,   -1,
   -1,   -1,  359,  360,  259,  362,  363,  364,  365,  366,
  367,  368,  369,  268,  269,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,  278,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,  290,  291,  292,  293,  294,
  295,  296,  297,  298,  299,  300,  301,  302,  303,  304,
  305,   -1,  307,  308,  309,   -1,  311,  312,  313,  314,
  315,  316,  317,  318,  319,  320,   -1,  322,  323,  324,
   -1,   -1,    0,  328,  329,  330,  331,  332,   -1,  334,
  335,  336,  337,  338,  339,  340,  341,   -1,   -1,  344,
  345,  346,  347,  348,  349,  350,   -1,  352,  353,  354,
  355,   -1,   -1,   -1,  359,  360,   -1,  362,  363,  364,
  365,  366,  367,  368,  369,   -1,   -1,  259,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,  268,  269,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,  278,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  290,  291,
  292,  293,  294,  295,  296,  297,  298,  299,  300,  301,
  302,  303,  304,  305,   -1,  307,  308,  309,   -1,  311,
  312,  313,  314,  315,  316,  317,  318,  319,  320,   -1,
  322,  323,  324,    0,   -1,   -1,  328,  329,  330,  331,
  332,   -1,  334,  335,  336,  337,  338,  339,  340,  341,
   -1,   -1,  344,  345,  346,  347,  348,  349,  350,   -1,
  352,  353,  354,  355,   -1,   -1,   -1,  359,  360,   -1,
  362,  363,  364,  365,  366,  367,  368,  369,   -1,  259,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  268,  269,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  278,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
  290,  291,  292,  293,  294,  295,  296,  297,  298,  299,
  300,  301,  302,  303,  304,  305,   -1,  307,  308,  309,
   -1,  311,  312,  313,  314,  315,  316,  317,  318,  319,
  320,   -1,  322,  323,  324,    0,   -1,   -1,  328,  329,
  330,  331,  332,   -1,  334,  335,  336,  337,  338,  339,
  340,  341,   -1,   -1,  344,  345,  346,  347,  348,  349,
  350,   -1,  352,  353,  354,  355,   -1,   -1,   -1,  359,
  360,  259,  362,  363,  364,  365,  366,  367,  368,  369,
  268,  269,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
  278,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,    0,  290,  291,  292,  293,  294,  295,  296,  297,
  298,  299,  300,  301,  302,  303,  304,  305,   -1,  307,
  308,  309,   -1,  311,  312,  313,  314,  315,  316,  317,
  318,  319,  320,   -1,  322,  323,  324,   -1,   -1,   -1,
  328,  329,  330,  331,  332,   -1,  334,  335,  336,  337,
  338,  339,  340,  341,   -1,   -1,  344,  345,  346,  347,
  348,  349,  350,   -1,  352,  353,  354,  355,   -1,   -1,
   -1,  359,  360,   -1,  362,  363,  364,  365,  366,  367,
  368,  369,  259,   -1,  261,   -1,   -1,   -1,   -1,   -1,
   -1,  268,  269,    0,   -1,   -1,   -1,  274,   -1,   -1,
   -1,  278,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,  290,  291,  292,  293,  294,  295,  296,
  297,  298,  299,  300,  301,  302,  303,  304,  305,   -1,
  307,  308,  309,   -1,  311,  312,  313,  314,  315,  316,
  317,  318,   -1,   -1,   -1,  322,  323,  324,   -1,   -1,
   -1,  328,  329,  330,  331,  332,   -1,  334,   -1,  336,
  337,  338,  339,  340,  341,   -1,   -1,   -1,   -1,   -1,
  347,  348,  349,  350,   -1,  352,  353,  354,  355,   -1,
   -1,   -1,  359,  360,   -1,  362,  363,  364,  365,  366,
  367,  368,  369,   -1,  259,   -1,  261,   -1,   -1,   -1,
   -1,   -1,   -1,  268,  269,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,  278,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,  290,  291,  292,  293,  294,
  295,  296,  297,  298,  299,  300,  301,  302,  303,  304,
  305,   -1,  307,  308,  309,   -1,  311,  312,  313,  314,
  315,  316,  317,  318,   -1,   -1,   -1,  322,  323,  324,
   -1,   -1,   -1,  328,  329,  330,  331,  332,   -1,  334,
  269,  336,  337,  338,  339,  340,  341,   -1,   -1,  278,
   -1,   -1,  347,  348,  349,  350,   -1,  352,  353,  354,
  355,   -1,   -1,   -1,  359,  360,   -1,  362,  363,  364,
  365,  366,  367,  368,  369,   -1,  305,   -1,  307,  308,
  309,   -1,  311,  312,   -1,   -1,  315,  316,  317,  318,
   -1,   -1,   -1,  322,  323,  324,   -1,   -1,   -1,   -1,
   -1,  330,  331,  332,   -1,  334,   -1,  336,  337,  338,
  339,  340,  341,   -1,   -1,   -1,   -1,   -1,  347,  348,
  349,   -1,   -1,  352,   -1,  354,   -1,   -1,   -1,   -1,
  359,  360,  269,  362,  363,  364,  365,  366,  367,  368,
  369,  278,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  305,   -1,
  307,  308,  309,   -1,  311,  312,   -1,   -1,   -1,  316,
  317,  318,   -1,   -1,   -1,  322,  323,  324,   -1,   -1,
   -1,   -1,   -1,  330,  331,  332,   -1,  334,   -1,   -1,
   -1,   -1,  339,  340,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,  348,  349,   -1,   -1,  352,   -1,  354,   -1,   -1,
   -1,   -1,  359,  360,   -1,  362,  363,  364,  365,  366,
  367,  368,  369,  259,   -1,  261,  262,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,  270,   -1,   -1,   -1,   -1,  259,
   -1,  261,  262,   -1,  280,  281,   -1,   -1,  284,  285,
  270,   -1,  288,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
  280,  281,   -1,   -1,  284,  285,   -1,   -1,  288,   -1,
  306,   -1,   -1,  269,  310,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,  278,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,  259,   -1,  261,  262,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,  270,   -1,  311,   -1,   -1,   -1,   -1,
  356,  357,  358,  280,  281,  321,   -1,  284,  285,  325,
  326,  288,   -1,   -1,   -1,   -1,  356,  357,  358,   -1,
   -1,   -1,   -1,  339,  340,   -1,  342,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,  351,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,  402,  363,  364,  365,
  366,  367,  368,  369,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,  402,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,  428,   -1,   -1,   -1,   -1,   -1,   -1,  356,
  357,  358,   -1,  284,  285,   -1,   -1,   -1,  428,   -1,
  446,  447,  448,  449,  450,  451,   -1,  453,  454,   -1,
  456,   -1,  458,  459,  460,  461,  446,  447,  448,  449,
  450,  451,   -1,  453,  454,   -1,  456,   -1,  458,  459,
  460,  461,   -1,   -1,   -1,  402,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  494,  495,
  496,  497,  498,  499,   -1,  501,   -1,   -1,   -1,   -1,
   -1,  428,   -1,   -1,  494,  495,  496,  497,  498,  499,
   -1,  501,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  446,
  447,  448,  449,  450,  451,   -1,  453,  454,   -1,  456,
   -1,  458,  459,  460,  461,   -1,   -1,   -1,  259,  545,
  261,  262,   -1,  549,  550,   -1,   -1,   -1,   -1,  270,
  556,  402,   -1,   -1,  259,  545,  261,  262,   -1,  549,
  550,   -1,   -1,  284,  285,  270,  556,  494,  495,  496,
  497,  498,  499,   -1,  501,   -1,   -1,  428,   -1,  284,
  285,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
  311,   -1,   -1,   -1,   -1,  446,  447,  448,  449,  450,
  451,   -1,  453,  454,   -1,  456,   -1,  458,  459,  460,
  461,   -1,   -1,   -1,  259,   -1,  261,  262,  545,   -1,
   -1,   -1,  549,  550,   -1,  270,   -1,   -1,   -1,  556,
   -1,   -1,   -1,   -1,   -1,  356,  357,  358,   -1,  284,
  285,   -1,   -1,  494,  495,   -1,   -1,   -1,  499,   -1,
  501,  356,  357,  358,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,  402,   -1,   -1,   -1,   -1,   -1,   -1,  393,  259,
  395,  261,  262,   -1,  545,   -1,   -1,  402,  549,  550,
  270,   -1,   -1,   -1,   -1,  556,   -1,  428,   -1,   -1,
   -1,  356,  357,  358,  284,  285,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,  428,   -1,  446,  447,  448,  449,  450,
  451,   -1,  453,  454,   -1,  456,   -1,  458,  459,  460,
  461,  446,  447,  448,  449,  450,  451,   -1,  453,  454,
   -1,  456,   -1,  458,  459,  460,  461,  402,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,  259,   -1,  261,  262,
   -1,   -1,   -1,  494,  495,   -1,   -1,  270,  499,   -1,
  501,   -1,   -1,  428,   -1,   -1,  356,  357,  358,  494,
  495,  284,  285,   -1,  499,  516,  501,   -1,   -1,   -1,
   -1,  446,  447,  448,  449,  450,  451,   -1,  453,  454,
   -1,  456,   -1,  458,  459,  460,  461,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,  545,   -1,   -1,   -1,  549,  550,
   -1,   -1,  402,   -1,   -1,  556,   -1,   -1,   -1,   -1,
  545,   -1,   -1,   -1,  549,  550,  270,   -1,   -1,  494,
  495,  556,   -1,   -1,  499,   -1,  501,   -1,  428,   -1,
  284,  285,   -1,  356,  357,  358,   -1,   -1,   -1,   -1,
   -1,  516,   -1,   -1,   -1,   -1,  446,  447,  448,  449,
  450,  451,   -1,  453,  454,   -1,  456,   -1,  458,  459,
  460,  461,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
  545,   -1,   -1,   -1,  549,  550,   -1,   -1,   -1,  402,
   -1,  556,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,  494,  495,  278,   -1,   -1,  499,
   -1,  501,   -1,   -1,   -1,  428,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,  446,  447,  448,  449,  450,  451,   -1,
  453,  454,   -1,  456,   -1,  458,  459,  460,  461,  393,
   -1,  395,   -1,   -1,   -1,  545,   -1,   -1,  402,  549,
  550,   -1,   -1,   -1,   -1,   -1,  556,   -1,   -1,   -1,
   -1,  269,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
  278,  494,  495,   -1,  428,   -1,  499,   -1,  501,   -1,
   -1,  363,  364,  365,  366,  367,  368,  369,   -1,   -1,
   -1,   -1,  446,  447,  448,  449,  450,  451,   -1,  453,
  454,   -1,  456,  311,  458,  459,  460,  461,   -1,   -1,
   -1,  284,  285,  321,   -1,   -1,   -1,  325,  326,   -1,
   -1,   -1,  545,   -1,   -1,   -1,  549,  550,  284,  285,
   -1,   -1,   -1,  556,  342,  343,   -1,   -1,   -1,   -1,
  494,  495,   -1,  351,   -1,  499,   -1,  501,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,  363,  364,  365,  366,  367,
  368,  369,   -1,   -1,   -1,  447,  448,  449,  450,  451,
   -1,  453,  454,   -1,  456,   -1,  458,  459,  460,  461,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,  545,   -1,   -1,   -1,  549,  550,   -1,   -1,  407,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  500,  501,
  393,   -1,  395,   -1,  506,   -1,   -1,   -1,  384,  402,
   -1,   -1,  388,  389,  390,  391,  392,  393,   -1,  395,
  396,  397,  398,  399,  400,  401,  402,  284,  285,   -1,
   -1,   -1,   -1,   -1,   -1,  428,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,  284,  285,   -1,   -1,  549,  550,   -1,
   -1,   -1,  428,  446,  447,  448,  449,  450,  451,   -1,
  453,  454,   -1,  456,   -1,  458,  459,  460,  461,   -1,
  446,  447,  448,  449,  450,  451,   -1,  453,  454,   -1,
  456,   -1,  458,  459,  460,  461,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,  494,  495,  284,  285,   -1,  499,   -1,  501,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  494,  495,
   -1,   -1,   -1,  499,   -1,  501,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,  393,   -1,  395,   -1,
   -1,   -1,   -1,   -1,   -1,  402,   -1,   -1,   -1,   -1,
   -1,   -1,  545,   -1,   -1,   -1,  549,  550,   -1,   -1,
   -1,  402,   -1,  284,  285,   -1,   -1,   -1,   -1,  545,
   -1,  428,   -1,  549,  550,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,  428,   -1,  446,
  447,  448,  449,  450,  451,   -1,  453,  454,   -1,  456,
   -1,  458,  459,  460,  461,  446,  447,  448,  449,  450,
  451,   -1,  453,  454,  270,  456,   -1,  458,  459,  460,
  461,  402,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,  494,  495,   -1,
   -1,   -1,  499,   -1,  501,   -1,   -1,  428,   -1,   -1,
   -1,   -1,   -1,  494,  495,   -1,   -1,   -1,  499,   -1,
  501,   -1,   -1,   -1,   -1,  446,  447,  448,  449,  450,
  451,   -1,  453,  454,  270,  456,   -1,  458,  459,  460,
  461,  402,   -1,   -1,  280,   -1,   -1,   -1,  545,   -1,
   -1,   -1,  549,  550,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,  545,   -1,   -1,  428,  549,  550,
   -1,   -1,   -1,  494,  495,  371,   -1,   -1,  499,   -1,
  501,   -1,   -1,   -1,   -1,  446,  447,  448,  449,  450,
  451,   -1,  453,  454,   -1,  456,   -1,  458,  459,  460,
  461,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  404,   -1,
   -1,   -1,   -1,  409,   -1,   -1,   -1,   -1,   -1,  415,
   -1,   -1,   -1,   -1,  545,   -1,   -1,   -1,  549,  550,
   -1,   -1,   -1,  494,  495,   -1,   -1,   -1,  499,   -1,
  501,   -1,   -1,  439,  440,   -1,   -1,  443,  444,  445,
  446,  447,  448,  449,  450,  451,   -1,  453,  454,  455,
  456,  457,  458,  459,  460,  461,  462,  463,  464,  465,
  466,  467,  468,  469,  470,  471,  472,  473,  474,  475,
   -1,   -1,  478,   -1,  545,   -1,   -1,   -1,  549,  550,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  494,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,  443,  444,   -1,
  446,  447,  448,  449,  450,  451,   -1,  453,  454,  455,
  456,  457,  458,  459,  460,  461,  462,  463,  464,  465,
  466,   -1,   -1,  469,  470,   -1,  472,  473,   -1,   -1,
   -1,   -1,   -1,  479,   -1,  481,  482,  483,  484,  485,
  486,  487,  488,  489,  490,  491,  492,  493,  375,  376,
  377,  378,  379,   -1,   -1,   -1,  269,   -1,   -1,   -1,
   -1,  388,  389,  390,  391,  278,  393,  394,  395,  396,
  397,  398,  399,   -1,   -1,   -1,   -1,   -1,   -1,  406,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  371,   -1,
   -1,  418,  419,  420,  421,  422,  423,   -1,  311,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  321,   -1,
   -1,   -1,  325,  326,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,  404,   -1,   -1,   -1,   -1,  409,   -1,   -1,  342,
  343,   -1,  415,   -1,   -1,   -1,   -1,   -1,  351,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
  363,  364,  365,  366,  367,  368,  369,   -1,   -1,   -1,
  443,  444,  445,  446,  447,  448,  449,  450,  451,   -1,
  453,  454,  455,  456,  457,  458,  459,  460,  461,  462,
  463,  464,  465,  466,  467,  468,  469,  470,  471,   -1,
  473,  474,  475,   -1,  407,  478,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,  259,   -1,  261,   -1,   -1,   -1,   -1,
   -1,  494,  268,  269,   -1,   -1,   -1,   -1,   -1,  546,
  547,   -1,  278,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
  557,  558,   -1,   -1,  290,  291,  292,  293,  294,  295,
  296,  297,  298,  299,  300,  301,  302,  303,  304,   -1,
   -1,   -1,   -1,   -1,   -1,  311,  312,  313,  314,   -1,
  316,   -1,   -1,   -1,   -1,   -1,  322,  323,  324,   -1,
   -1,   -1,  328,  329,   -1,   -1,   -1,   -1,   -1,   -1,
  336,   -1,  338,  339,  340,   -1,  259,   -1,  261,   -1,
   -1,   -1,   -1,   -1,  350,  268,  269,  353,   -1,  355,
   -1,   -1,   -1,   -1,   -1,  278,   -1,  363,  364,  365,
  366,  367,  368,  369,   -1,   -1,   -1,  290,  291,  292,
  293,  294,  295,  296,  297,  298,  299,  300,  301,  302,
  303,  304,   -1,   -1,   -1,   -1,   -1,   -1,  311,  312,
  313,  314,   -1,  316,   -1,   -1,   -1,   -1,   -1,  322,
  323,  324,   -1,   -1,   -1,  328,  329,   -1,   -1,   -1,
   -1,   -1,   -1,  336,   -1,  338,  339,  340,   -1,  259,
   -1,  261,   -1,   -1,   -1,   -1,   -1,  350,  268,  269,
  353,   -1,  355,   -1,   -1,   -1,   -1,   -1,  278,   -1,
  363,  364,  365,  366,  367,  368,  369,   -1,   -1,   -1,
  290,  291,  292,  293,  294,  295,  296,  297,  298,  299,
  300,  301,  302,  303,  304,   -1,   -1,   -1,   -1,   -1,
   -1,  311,  312,  313,  314,   -1,  316,   -1,   -1,   -1,
   -1,   -1,  322,  323,  324,   -1,   -1,   -1,  328,  329,
   -1,   -1,   -1,   -1,   -1,   -1,  336,   -1,  338,  339,
  340,   -1,  259,   -1,  261,   -1,   -1,   -1,   -1,   -1,
  350,  268,  269,  353,   -1,  355,   -1,   -1,   -1,   -1,
   -1,  278,   -1,  363,  364,  365,  366,  367,  368,  369,
   -1,   -1,   -1,  290,  291,  292,  293,  294,  295,  296,
  297,  298,  299,  300,  301,  302,  303,  304,   -1,   -1,
   -1,   -1,   -1,   -1,  311,  312,  313,  314,   -1,  316,
   -1,   -1,   -1,   -1,   -1,  322,  323,  324,   -1,   -1,
   -1,  328,  329,   -1,   -1,   -1,   -1,   -1,   -1,  336,
   -1,  338,  339,  340,   -1,  259,   -1,  261,   -1,   -1,
   -1,   -1,   -1,  350,  268,  269,  353,   -1,  355,   -1,
   -1,   -1,   -1,   -1,  278,   -1,  363,  364,  365,  366,
  367,  368,  369,   -1,   -1,   -1,  290,  291,  292,  293,
  294,  295,  296,  297,  298,  299,  300,  301,  302,  303,
  304,   -1,   -1,   -1,   -1,   -1,   -1,  311,  312,  313,
  314,   -1,  316,   -1,   -1,   -1,   -1,   -1,  322,  323,
  324,   -1,   -1,   -1,  328,  329,   -1,   -1,   -1,   -1,
   -1,   -1,  336,   -1,  338,  339,  340,   -1,  259,   -1,
  261,   -1,   -1,   -1,   -1,   -1,  350,  268,  269,  353,
   -1,  355,   -1,   -1,   -1,   -1,   -1,  278,   -1,  363,
  364,  365,  366,  367,  368,  369,   -1,   -1,   -1,  290,
  291,  292,  293,  294,  295,  296,  297,  298,  299,  300,
  301,  302,  303,  304,   -1,   -1,   -1,   -1,   -1,   -1,
  311,  312,  313,  314,   -1,  316,   -1,   -1,   -1,   -1,
   -1,  322,  323,  324,   -1,   -1,   -1,  328,  329,   -1,
   -1,   -1,   -1,   -1,   -1,  336,   -1,  338,  339,  340,
   -1,  259,   -1,  261,   -1,   -1,   -1,   -1,   -1,  350,
  268,  269,  353,   -1,  355,   -1,   -1,   -1,   -1,   -1,
  278,   -1,  363,  364,  365,  366,  367,  368,  369,   -1,
   -1,   -1,  290,  291,  292,  293,  294,  295,  296,  297,
  298,  299,  300,  301,  302,  303,  304,   -1,   -1,   -1,
   -1,   -1,   -1,  311,  312,  313,  314,   -1,  316,   -1,
   -1,   -1,   -1,   -1,  322,  323,  324,   -1,   -1,   -1,
  328,  329,   -1,   -1,   -1,   -1,   -1,   -1,  336,   -1,
  338,  339,  340,   -1,  259,   -1,  261,   -1,   -1,   -1,
   -1,   -1,  350,  268,  269,  353,   -1,  355,   -1,   -1,
   -1,   -1,   -1,  278,   -1,  363,  364,  365,  366,  367,
  368,  369,   -1,   -1,   -1,  290,  291,  292,  293,  294,
  295,  296,  297,  298,  299,  300,  301,  302,  303,  304,
   -1,   -1,   -1,   -1,   -1,   -1,  311,  312,  313,  314,
   -1,  316,   -1,   -1,   -1,   -1,   -1,  322,  323,  324,
   -1,   -1,   -1,  328,  329,   -1,   -1,   -1,   -1,  269,
   -1,  336,   -1,  338,  339,  340,   -1,   -1,  278,   -1,
   -1,   -1,   -1,   -1,   -1,  350,   -1,   -1,  353,   -1,
  355,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  363,  364,
  365,  366,  367,  368,  369,  305,   -1,  307,  308,  309,
   -1,  311,  312,   -1,   -1,   -1,  316,  317,  318,   -1,
   -1,   -1,  322,  323,  324,   -1,   -1,   -1,   -1,   -1,
  330,  331,  332,  269,  334,   -1,   -1,   -1,   -1,  339,
  340,   -1,  278,   -1,   -1,   -1,   -1,   -1,  348,  349,
   -1,   -1,  352,   -1,  354,   -1,   -1,   -1,   -1,  359,
  360,   -1,  362,  363,  364,  365,  366,  367,  368,  369,
   -1,  307,   -1,   -1,   -1,  311,  312,   -1,   -1,  315,
   -1,  317,  269,   -1,   -1,   -1,  322,  323,  324,   -1,
   -1,  278,   -1,   -1,  330,   -1,   -1,   -1,   -1,   -1,
  336,  337,  338,  339,  340,  341,   -1,   -1,   -1,   -1,
   -1,  347,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,  311,   -1,   -1,  363,  364,  365,
  366,  367,  368,  369,  321,   -1,   -1,   -1,  325,  326,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,  339,  340,   -1,  342,  278,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,  351,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,  363,  364,  365,  366,
  367,  368,  369,  305,   -1,  307,  308,  309,   -1,  311,
  312,   -1,   -1,   -1,  316,  317,  318,   -1,   -1,   -1,
  322,  323,  324,   -1,   -1,   -1,   -1,   -1,  330,  331,
  332,   -1,  334,   -1,   -1,   -1,   -1,  339,  340,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,  348,  349,   -1,   -1,
  352,   -1,  354,   -1,   -1,   -1,   -1,  359,  360,   -1,
  362,  363,  364,  365,  366,  367,  368,  369,
  };

#line 4360 "/home/mbartnic/work/mono-git/mcs/tools/ildasm/tests/parser/TesterILParser.jay"

}
#line default
namespace yydebug {
        using System;
	 internal interface yyDebug {
		 void push (int state, Object value);
		 void lex (int state, int token, string name, Object value);
		 void shift (int from, int to, int errorFlag);
		 void pop (int state);
		 void discard (int state, int token, string name, Object value);
		 void reduce (int from, int to, int rule, string text, int len);
		 void shift (int from, int to);
		 void accept (Object value);
		 void error (string message);
		 void reject ();
	 }
	 
	 class yyDebugSimple : yyDebug {
		 void println (string s){
			 Console.Error.WriteLine (s);
		 }
		 
		 public void push (int state, Object value) {
			 println ("push\tstate "+state+"\tvalue "+value);
		 }
		 
		 public void lex (int state, int token, string name, Object value) {
			 println("lex\tstate "+state+"\treading "+name+"\tvalue "+value);
		 }
		 
		 public void shift (int from, int to, int errorFlag) {
			 switch (errorFlag) {
			 default:				// normally
				 println("shift\tfrom state "+from+" to "+to);
				 break;
			 case 0: case 1: case 2:		// in error recovery
				 println("shift\tfrom state "+from+" to "+to
					     +"\t"+errorFlag+" left to recover");
				 break;
			 case 3:				// normally
				 println("shift\tfrom state "+from+" to "+to+"\ton error");
				 break;
			 }
		 }
		 
		 public void pop (int state) {
			 println("pop\tstate "+state+"\ton error");
		 }
		 
		 public void discard (int state, int token, string name, Object value) {
			 println("discard\tstate "+state+"\ttoken "+name+"\tvalue "+value);
		 }
		 
		 public void reduce (int from, int to, int rule, string text, int len) {
			 println("reduce\tstate "+from+"\tuncover "+to
				     +"\trule ("+rule+") "+text);
		 }
		 
		 public void shift (int from, int to) {
			 println("goto\tfrom state "+from+" to "+to);
		 }
		 
		 public void accept (Object value) {
			 println("accept\tvalue "+value);
		 }
		 
		 public void error (string message) {
			 println("error\t"+message);
		 }
		 
		 public void reject () {
			 println("reject");
		 }
		 
	 }
}
// %token constants
 class Token {
  public const int UNKNOWN = 257;
  public const int EOF = 258;
  public const int ID = 259;
  public const int QSTRING = 260;
  public const int SQSTRING = 261;
  public const int COMP_NAME = 262;
  public const int INT32 = 263;
  public const int INT64 = 264;
  public const int FLOAT64 = 265;
  public const int HEXBYTE = 266;
  public const int DOT = 267;
  public const int OPEN_BRACE = 268;
  public const int CLOSE_BRACE = 269;
  public const int OPEN_BRACKET = 270;
  public const int CLOSE_BRACKET = 271;
  public const int OPEN_PARENS = 272;
  public const int CLOSE_PARENS = 273;
  public const int COMMA = 274;
  public const int COLON = 275;
  public const int DOUBLE_COLON = 276;
  public const int SEMICOLON = 278;
  public const int ASSIGN = 279;
  public const int STAR = 280;
  public const int AMPERSAND = 281;
  public const int PLUS = 282;
  public const int SLASH = 283;
  public const int BANG = 284;
  public const int ELLIPSIS = 285;
  public const int DASH = 287;
  public const int OPEN_ANGLE_BRACKET = 288;
  public const int CLOSE_ANGLE_BRACKET = 289;
  public const int INSTR_NONE = 290;
  public const int INSTR_I = 291;
  public const int INSTR_I8 = 292;
  public const int INSTR_R = 293;
  public const int INSTR_BRTARGET = 294;
  public const int INSTR_METHOD = 295;
  public const int INSTR_FIELD = 296;
  public const int INSTR_TYPE = 297;
  public const int INSTR_STRING = 298;
  public const int INSTR_SIG = 299;
  public const int INSTR_TOK = 300;
  public const int INSTR_SWITCH = 301;
  public const int INSTR_PHI = 302;
  public const int INSTR_LOCAL = 303;
  public const int INSTR_PARAM = 304;
  public const int D_ASSEMBLY = 305;
  public const int D_CCTOR = 306;
  public const int D_CLASS = 307;
  public const int D_IMAGEBASE = 308;
  public const int D_CORFLAGS = 309;
  public const int D_CTOR = 310;
  public const int D_CUSTOM = 311;
  public const int D_DATA = 312;
  public const int D_EMITBYTE = 313;
  public const int D_ENTRYPOINT = 314;
  public const int D_EVENT = 315;
  public const int D_EXPORT = 316;
  public const int D_FIELD = 317;
  public const int D_FILE = 318;
  public const int D_FIRE = 319;
  public const int D_GET = 320;
  public const int D_HASH = 321;
  public const int D_LANGUAGE = 322;
  public const int D_LINE = 323;
  public const int D_XLINE = 324;
  public const int D_LOCALE = 325;
  public const int D_CULTURE = 326;
  public const int D_LOCALIZED = 327;
  public const int D_LOCALS = 328;
  public const int D_MAXSTACK = 329;
  public const int D_METHOD = 330;
  public const int D_MODULE = 331;
  public const int D_MRESOURCE = 332;
  public const int D_MANIFESTRES = 333;
  public const int D_NAMESPACE = 334;
  public const int D_OTHER = 335;
  public const int D_OVERRIDE = 336;
  public const int D_PACK = 337;
  public const int D_PARAM = 338;
  public const int D_PERMISSION = 339;
  public const int D_PERMISSIONSET = 340;
  public const int D_PROPERTY = 341;
  public const int D_PUBLICKEY = 342;
  public const int D_PUBLICKEYTOKEN = 343;
  public const int D_ADDON = 344;
  public const int D_REMOVEON = 345;
  public const int D_SET = 346;
  public const int D_SIZE = 347;
  public const int D_STACKRESERVE = 348;
  public const int D_SUBSYSTEM = 349;
  public const int D_TRY = 350;
  public const int D_VER = 351;
  public const int D_VTABLE = 352;
  public const int D_VTENTRY = 353;
  public const int D_VTFIXUP = 354;
  public const int D_ZEROINIT = 355;
  public const int D_THIS = 356;
  public const int D_BASE = 357;
  public const int D_NESTER = 358;
  public const int D_TYPELIST = 359;
  public const int D_MSCORLIB = 360;
  public const int D_PDIRECT = 361;
  public const int D_TYPEDEF = 362;
  public const int D_XDEFINE = 363;
  public const int D_XUNDEF = 364;
  public const int D_XIFDEF = 365;
  public const int D_XIFNDEF = 366;
  public const int D_XELSE = 367;
  public const int D_XENDIF = 368;
  public const int D_XINCLUDE = 369;
  public const int K_AT = 370;
  public const int K_AS = 371;
  public const int K_IMPLICITCOM = 372;
  public const int K_IMPLICITRES = 373;
  public const int K_EXTERN = 374;
  public const int K_INSTANCE = 375;
  public const int K_EXPLICIT = 376;
  public const int K_DEFAULT = 377;
  public const int K_VARARG = 378;
  public const int K_UNMANAGED = 379;
  public const int K_CDECL = 380;
  public const int K_STDCALL = 381;
  public const int K_THISCALL = 382;
  public const int K_FASTCALL = 383;
  public const int K_MARSHAL = 384;
  public const int K_IN = 385;
  public const int K_OUT = 386;
  public const int K_OPT = 387;
  public const int K_STATIC = 388;
  public const int K_PUBLIC = 389;
  public const int K_PRIVATE = 390;
  public const int K_FAMILY = 391;
  public const int K_INITONLY = 392;
  public const int K_RTSPECIALNAME = 393;
  public const int K_STRICT = 394;
  public const int K_SPECIALNAME = 395;
  public const int K_ASSEMBLY = 396;
  public const int K_FAMANDASSEM = 397;
  public const int K_FAMORASSEM = 398;
  public const int K_PRIVATESCOPE = 399;
  public const int K_LITERAL = 400;
  public const int K_NOTSERIALIZED = 401;
  public const int K_VALUE = 402;
  public const int K_NOT_IN_GC_HEAP = 403;
  public const int K_INTERFACE = 404;
  public const int K_SEALED = 405;
  public const int K_ABSTRACT = 406;
  public const int K_AUTO = 407;
  public const int K_SEQUENTIAL = 408;
  public const int K_ANSI = 409;
  public const int K_UNICODE = 410;
  public const int K_AUTOCHAR = 411;
  public const int K_BESTFIT = 412;
  public const int K_IMPORT = 413;
  public const int K_SERIALIZABLE = 414;
  public const int K_NESTED = 415;
  public const int K_EXTENDS = 416;
  public const int K_IMPLEMENTS = 417;
  public const int K_FINAL = 418;
  public const int K_VIRTUAL = 419;
  public const int K_HIDEBYSIG = 420;
  public const int K_NEWSLOT = 421;
  public const int K_UNMANAGEDEXP = 422;
  public const int K_PINVOKEIMPL = 423;
  public const int K_NOMANGLE = 424;
  public const int K_LASTERR = 425;
  public const int K_WINAPI = 426;
  public const int K_PLATFORMAPI = 427;
  public const int K_NATIVE = 428;
  public const int K_IL = 429;
  public const int K_CIL = 430;
  public const int K_OPTIL = 431;
  public const int K_MANAGED = 432;
  public const int K_FORWARDREF = 433;
  public const int K_RUNTIME = 434;
  public const int K_INTERNALCALL = 435;
  public const int K_SYNCHRONIZED = 436;
  public const int K_NOINLINING = 437;
  public const int K_NOOPTIMIZATION = 438;
  public const int K_CUSTOM = 439;
  public const int K_FIXED = 440;
  public const int K_SYSSTRING = 441;
  public const int K_ARRAY = 442;
  public const int K_VARIANT = 443;
  public const int K_CURRENCY = 444;
  public const int K_SYSCHAR = 445;
  public const int K_VOID = 446;
  public const int K_BOOL = 447;
  public const int K_INT8 = 448;
  public const int K_INT16 = 449;
  public const int K_INT32 = 450;
  public const int K_INT64 = 451;
  public const int K_FLOAT = 452;
  public const int K_FLOAT32 = 453;
  public const int K_FLOAT64 = 454;
  public const int K_ERROR = 455;
  public const int K_UNSIGNED = 456;
  public const int K_UINT = 457;
  public const int K_UINT8 = 458;
  public const int K_UINT16 = 459;
  public const int K_UINT32 = 460;
  public const int K_UINT64 = 461;
  public const int K_DECIMAL = 462;
  public const int K_DATE = 463;
  public const int K_BSTR = 464;
  public const int K_LPSTR = 465;
  public const int K_LPWSTR = 466;
  public const int K_LPTSTR = 467;
  public const int K_OBJECTREF = 468;
  public const int K_IUNKNOWN = 469;
  public const int K_IDISPATCH = 470;
  public const int K_STRUCT = 471;
  public const int K_SAFEARRAY = 472;
  public const int K_INT = 473;
  public const int K_BYVALSTR = 474;
  public const int K_TBSTR = 475;
  public const int K_LPVOID = 476;
  public const int K_ANY = 477;
  public const int K_LPSTRUCT = 478;
  public const int K_NULL = 479;
  public const int K_VECTOR = 480;
  public const int K_HRESULT = 481;
  public const int K_CARRAY = 482;
  public const int K_USERDEFINED = 483;
  public const int K_RECORD = 484;
  public const int K_FILETIME = 485;
  public const int K_BLOB = 486;
  public const int K_STREAM = 487;
  public const int K_STORAGE = 488;
  public const int K_STREAMED_OBJECT = 489;
  public const int K_STORED_OBJECT = 490;
  public const int K_BLOB_OBJECT = 491;
  public const int K_CF = 492;
  public const int K_CLSID = 493;
  public const int K_METHOD = 494;
  public const int K_CLASS = 495;
  public const int K_PINNED = 496;
  public const int K_MODREQ = 497;
  public const int K_MODOPT = 498;
  public const int K_TYPEDREF = 499;
  public const int K_TYPE = 500;
  public const int K_CHAR = 501;
  public const int K_WCHAR = 502;
  public const int K_FROMUNMANAGED = 503;
  public const int K_CALLMOSTDERIVED = 504;
  public const int K_RETAINAPPDOMAIN = 505;
  public const int K_BYTEARRAY = 506;
  public const int K_WITH = 507;
  public const int K_INIT = 508;
  public const int K_TO = 509;
  public const int K_CATCH = 510;
  public const int K_FILTER = 511;
  public const int K_FINALLY = 512;
  public const int K_FAULT = 513;
  public const int K_HANDLER = 514;
  public const int K_TLS = 515;
  public const int K_FIELD = 516;
  public const int K_PROPERTY = 517;
  public const int K_REQUEST = 518;
  public const int K_DEMAND = 519;
  public const int K_ASSERT = 520;
  public const int K_DENY = 521;
  public const int K_PERMITONLY = 522;
  public const int K_LINKCHECK = 523;
  public const int K_INHERITCHECK = 524;
  public const int K_REQMIN = 525;
  public const int K_REQOPT = 526;
  public const int K_REQREFUSE = 527;
  public const int K_PREJITGRANT = 528;
  public const int K_PREJITDENY = 529;
  public const int K_NONCASDEMAND = 530;
  public const int K_NONCASLINKDEMAND = 531;
  public const int K_NONCASINHERITANCE = 532;
  public const int K_NOMETADATA = 533;
  public const int K_ALGORITHM = 534;
  public const int K_RETARGETABLE = 535;
  public const int K_LEGACY = 536;
  public const int K_LIBRARY = 537;
  public const int K_X86 = 538;
  public const int K_IA64 = 539;
  public const int K_AMD64 = 540;
  public const int K_PRESERVESIG = 541;
  public const int K_BEFOREFIELDINIT = 542;
  public const int K_ALIGNMENT = 543;
  public const int K_NULLREF = 544;
  public const int K_VALUETYPE = 545;
  public const int K_COMPILERCONTROLLED = 546;
  public const int K_REQSECOBJ = 547;
  public const int K_ENUM = 548;
  public const int K_OBJECT = 549;
  public const int K_STRING = 550;
  public const int K_TRUE = 551;
  public const int K_FALSE = 552;
  public const int K_ON = 553;
  public const int K_OFF = 554;
  public const int K_CHARMAPERROR = 555;
  public const int K_MDTOKEN = 556;
  public const int K_FLAGS = 557;
  public const int K_CALLCONV = 558;
  public const int K_NOAPPDOMAIN = 559;
  public const int K_NOMACHINE = 560;
  public const int K_NOPROCESS = 561;
  public const int K_ILLEGAL = 562;
  public const int K_UNUSED = 563;
  public const int K_WRAPPER = 564;
  public const int K_FORWARDER = 565;
  public const int yyErrorCode = 256;
 }
 namespace yyParser {
  using System;
  /** thrown for irrecoverable syntax errors and stack overflow.
    */
  internal class yyException : System.Exception {
    public yyException (string message) : base (message) {
    }
  }
  internal class yyUnexpectedEof : yyException {
    public yyUnexpectedEof (string message) : base (message) {
    }
    public yyUnexpectedEof () : base ("") {
    }
  }

  /** must be implemented by a scanner object to supply input to the parser.
    */
  internal interface yyInput {
    /** move on to next token.
        @return false if positioned beyond tokens.
        @throws IOException on input error.
      */
    bool advance (); // throws java.io.IOException;
    /** classifies current token.
        Should not be called if advance() returned false.
        @return current %token or single character.
      */
    int token ();
    /** associated with current token.
        Should not be called if advance() returned false.
        @return value for token().
      */
    Object value ();
  }
 }
} // close outermost namespace, that MUST HAVE BEEN opened in the prolog
