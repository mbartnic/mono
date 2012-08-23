.assembly extern mscorlib
{
}

.class test001
{
	.method private hidebysig instance void 
	CustomMarshalingMethod_MarshalType_NoMarshalCookie(object  marshal( custom ("ReferencedLibraryNamespace.SomeType, ReferencedLibrary, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null", "")) param1) cil managed
	{
		ret
	}

	
}
