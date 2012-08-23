.assembly extern mscorlib {}

.class test
{
	.method private hidebysig instance void 
	marshal_param_Bstr(string  marshal( bstr) someVar) cil managed
	{}
	
	.method private hidebysig instance void 
	marshal_param_LPStr(string  marshal( lpstr) someVar) cil managed
	{}
	
}

