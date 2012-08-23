.assembly extern mscorlib {}

.class test
{
	.method private hidebysig instance void 
	marshal_param_currency_array(valuetype [mscorlib]System.Decimal[]  marshal( currency) money) cil managed
	{}
	
	.method private hidebysig instance void 
	marshal_param_LPArray_SizeParamIndex(object  marshal( float64[ + 0]) crazyArray) cil managed
	{}
	
	.method private hidebysig instance void 
	marshal_param_LPArray(object  marshal( float64[]) crazyArray) cil managed
	{}
	
}

