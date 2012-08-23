.assembly extern mscorlib
{
}

.class test001
{
	.method private hidebysig instance void 
	CustomMarshal_NonNeutralCulture_MarshalTypeRef(object  marshal( custom ("NonNeutralCultureAssemblyReferenceNamespace.SomeClass, NonNeutralCultureAssemblyReference, Version=0.0.0.0, Culture=en-us, PublicKeyToken=null", "")) param1) cil managed
	{
		ret
	}
}

