#include <windows.h>

constexpr BYTE HLT = 0xF4;

EXTERN_C __declspec(dllexport)
LONG __stdcall VEHHandler(
	_EXCEPTION_POINTERS* ExceptionInfo
)
{
	PEXCEPTION_RECORD record = ExceptionInfo->ExceptionRecord;
	if (record->ExceptionCode != EXCEPTION_PRIV_INSTRUCTION)
		return EXCEPTION_CONTINUE_SEARCH;
	volatile PBYTE address = (volatile PBYTE)record->ExceptionAddress;
	BYTE instruction = *address;
	if (instruction != HLT)
		return EXCEPTION_CONTINUE_EXECUTION;
	size_t step = 1;
	do
	{
		for (size_t i = 0; i < step; i++)
			_mm_pause();
		step = step < 64 ? step <<= 1 : 64;
	} while ((instruction = *address) == HLT);
	return EXCEPTION_CONTINUE_EXECUTION;
}