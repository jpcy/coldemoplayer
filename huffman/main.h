extern "C"
{
	__declspec(dllexport) void HuffmanInit();
	__declspec(dllexport) unsigned int HuffmanReadUInt(unsigned char *buffer, int *bitOffset, int bits);
}