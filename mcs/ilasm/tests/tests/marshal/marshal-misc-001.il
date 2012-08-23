.assembly extern mscorlib {}

.class test
{
	.method private hidebysig instance void 
	marshal_method_01_sizeConst0([out] int32[]&  marshal([0]) ar) cil managed
	{
		newobj     instance void [mscorlib]System.NotImplementedException::.ctor()
		throw
	}
	
	.method private hidebysig instance void 
	marshal_method_01_sizeConst0_sizeParamIndex0([out] int32[]&  marshal([0]) ar) cil managed
	{
		newobj     instance void [mscorlib]System.NotImplementedException::.ctor()
		throw
	}
	
}

