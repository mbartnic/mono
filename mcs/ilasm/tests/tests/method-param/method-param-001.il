.assembly extern mscorlib {}

.method public hidebysig static void  Test_default_param_long([opt] bool logicFalse,
				[opt] bool logicTrue,
				[opt] char characterShort,
				[opt] char characterLong,
				[opt] object nothing,
				[opt] string str,
				[opt] string strLanguageSpecific,
				[opt] string strEmpty,
				[opt] float32 floating32,
				[opt] float64 floating64,
				[opt] int8 signedByte,
				[opt] int16 integer16,
				[opt] int32 integer32,
				[opt] int64 integer64,
				[opt] uint8 unsignedByte,
				[opt] uint16 unsignedInt16,
				[opt] uint32 unsignedInt32,
				[opt] uint64 unsignedInt64) cil managed
{
	.param [1] = bool(false)
	.param [2] = bool(true)
	.param [3] = char(0x0061)
	.param [4] = char(0x017A)
	.param [5] = nullref
	.param [6] = "Some text"
	.param [7] = bytearray (05 01 07 01 19 01 5B 01 7A 01 7C 01 42 01 F3 00 ) // ......[.z.|.B...
	.param [8] = ""
	.param [9] = float32(0xFFC00000)
	.param [10] = float64(4.9406564584124654e-324)
	.param [11] = int8(0x7F)
	.param [12] = int16(0xFFEF)
	.param [13] = int32(0xFFFFFFFF)
	.param [14] = int64(0xFFFFFFFFFFFFFFFF)
	.param [15] = uint8(0xFF)
	.param [16] = uint16(0xABAB)
	.param [17] = uint32(0xABABABAB)
	.param [18] = uint64(0xABABABABABABABAB)

	ret
}
