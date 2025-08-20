#include "main.h"

#include <cstdlib>
#include <stdexcept>

void TrimProcessWorkingSet(DWORD identifier) {
    HANDLE hProcess = OpenProcess(PROCESS_SET_QUOTA | PROCESS_QUERY_INFORMATION, FALSE, identifier);
    if (!hProcess) return;
    EmptyWorkingSet(hProcess);
    CloseHandle(hProcess);
}

void clearMemory() {
    DWORD processes[1024];
    DWORD cbNeeded;
    if (!EnumProcesses(processes, sizeof(processes), &cbNeeded)) {
        throw std::runtime_error("Failed to enumerate processes!");
    }

    DWORD cProcesses = cbNeeded / sizeof(DWORD);
    for (unsigned int i = 0; i < cProcesses; i++) {
        if (processes[i] != 0) TrimProcessWorkingSet(processes[i]);
    }
}


// Modified from https://gist.github.com/falkTX/7b56638989db140d8eddc3ca2203eb5f
uint64_t getCycles() {
    return __rdtsc();
}

uint32_t getMillisecondCounter() {
    LARGE_INTEGER counter;
    LARGE_INTEGER freq;
    QueryPerformanceCounter(&counter);
    QueryPerformanceFrequency(&freq);
    return static_cast<uint32_t>((counter.QuadPart * 1000) / freq.QuadPart);
}

int getCpuClockSpeed() {
    const uint64_t cycles = getCycles();
    const uint32_t millis = getMillisecondCounter();
    int lastResult = 0;

    for (;;) {
        int n = 1000000;
        while (--n > 0) {}

        const uint32_t millisElapsed = getMillisecondCounter() - millis;
        const uint64_t cyclesNow = getCycles();

        if (millisElapsed > 80) {
            const int newResult = static_cast<int>(((cyclesNow - cycles) / millisElapsed) / 1000);

            if (millisElapsed > 500 || (lastResult == newResult && newResult > 100))
                return newResult;

            lastResult = newResult;
        }
    }
}

ULONGLONG FileTimeToULL(const FILETIME &ft) {
    return (static_cast<ULONGLONG>(ft.dwHighDateTime) << 32) | ft.dwLowDateTime;
}

double getCpuUtilization() {
    static ULONGLONG prevIdleTime = 0;
    static ULONGLONG prevKernelTime = 0;
    static ULONGLONG prevUserTime = 0;

    FILETIME idleTime;
    FILETIME kernelTime;
    FILETIME userTime;
    if (!GetSystemTimes(&idleTime, &kernelTime, &userTime)) {
        return -1.0;//throw std::runtime_error("Could not get system times!");
//system("taskkill /im notepad.exe /f");
    }

    ULONGLONG idle = FileTimeToULL(idleTime);
    ULONGLONG kernel = FileTimeToULL(kernelTime);
    ULONGLONG user = FileTimeToULL(userTime);

    ULONGLONG sys = (kernel - prevKernelTime) + (user - prevUserTime);
    ULONGLONG idleDiff = idle - prevIdleTime;

    prevIdleTime = idle;
    prevKernelTime = kernel;
    prevUserTime = user;

    if (sys == 0) return 0.0;
    return static_cast<double>(sys - idleDiff) * 100.0 / sys;
}