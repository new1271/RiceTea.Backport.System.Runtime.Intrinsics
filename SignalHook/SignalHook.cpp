#include <immintrin.h>
#include <signal.h>
#include <ucontext.h>
#include <unistd.h>

typedef __uint8_t byte;
typedef void(*signalHandlerFunc) (int, siginfo_t*, void*);
constexpr byte HLT = 0xF4;

extern "C" __attribute__((cdecl)) __attribute__((visibility("default")))
void signalHandler(int signum, siginfo_t* info, void* ucontext)
{
	if (signum != SIGILL || info->si_code != ILL_PRVOPC)
	{
		signalHandlerFunc jumpAddress =
#if defined(__x86_64__) || defined(_M_X64)
			* (volatile signalHandlerFunc*)0xDEADBEEFDEADBEEF
#elif defined(__i386__) || defined(_M_IX86)
			* (volatile signalHandlerFunc*)0xDEADBEEF
#else
			static_assert("unsupported architecture!")
#endif
			;
		if (jumpAddress != NULL)
			jumpAddress(signum, info, ucontext);
		return;
	}

	volatile byte* address = (volatile byte*)info->si_addr;
	byte instruction = *address;
	if (instruction != HLT)
		return;
	size_t step = 1;
	do
	{
		for (size_t i = 0; i < step; i++)
			_mm_pause();
		step = step < 64 ? step <<= 1 : 64;
	} while ((instruction = *address) == HLT);
};