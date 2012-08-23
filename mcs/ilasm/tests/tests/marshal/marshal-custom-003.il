.assembly extern mscorlib
{
}

.class test001
{
	.method private hidebysig instance void 
	CustomMarshalingMethod_MarshalType(object  marshal( custom ("SomeMarhaler.SomeType", "SomeAditionalInformation")) param1) cil managed
	{
		ret
	}
}
