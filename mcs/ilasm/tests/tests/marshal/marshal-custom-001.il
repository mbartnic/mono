.assembly extern mscorlib
{
}

.class test001
{
	.method private hidebysig instance void CustomMarshalGuid_Class(object  marshal( custom ("ReferencedLibraryNamespace.SomeTypeWithGuid, ReferencedLibrary, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null", "")) param1) cil managed
	{
		ret
	}
}
