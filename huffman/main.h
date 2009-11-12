extern "C"
{
	__declspec(dllexport) void HuffmanInit();
	__declspec(dllexport) unsigned int HuffmanReadUInt(unsigned char *buffer, int *bitOffset, int bits);
	__declspec(dllexport) void HuffmanWriteUInt(unsigned char *buffer, int *bitOffset, unsigned int value, int bits);
}