#include <windows.h>

constexpr BYTE HLT = 0xF4;
constexpr BYTE JMP = 0xE9;

EXTERN_C __declspec(dllexport)
LONG __stdcall PvectoredExceptionHandler(
	_EXCEPTION_POINTERS* ExceptionInfo
)
{
	PEXCEPTION_RECORD record = ExceptionInfo->ExceptionRecord;
	if (record->ExceptionCode != EXCEPTION_PRIV_INSTRUCTION)
		return EXCEPTION_CONTINUE_SEARCH;
	BYTE instructionHeader = *(volatile PBYTE)(record->ExceptionAddress);
	if (instructionHeader == HLT || instructionHeader == JMP)
		return EXCEPTION_CONTINUE_EXECUTION;
	return EXCEPTION_CONTINUE_SEARCH;
}