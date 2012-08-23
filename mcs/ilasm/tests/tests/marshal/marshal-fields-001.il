.assembly extern mscorlib
{
}

.class test001
{
	.field public  marshal( fixed sysstring [10]) string field1
	.field public  marshal( fixed array [100]) uint8[] field2
	.field public  marshal( fixed array [100] unsigned int16) uint8[] field3
}

