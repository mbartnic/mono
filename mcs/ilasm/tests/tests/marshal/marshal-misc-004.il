.assembly extern mscorlib {}

.class test
{
	.method private hidebysig instance void 
	marshal_param_LPWStr(string  marshal( lpwstr) somevar) cil managed
	{}
	
	.method private hidebysig instance void 
	marshal_param_LPTStr(string  marshal( lptstr) arg) cil managed
	{}
	
}

