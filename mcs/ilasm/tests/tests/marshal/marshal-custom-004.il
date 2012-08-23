.assembly extern mscorlib
{
}

.class test001
{
	.method private hidebysig instance void 
	CustomMarshalingMethod_MarshalTypeRef(object  marshal( custom ("ReferencedLibraryNamespace.SomeType, ReferencedLibrary, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null", "SomeAditionalInformation")) param1) cil managed
	{
		ret
	}

	
}
