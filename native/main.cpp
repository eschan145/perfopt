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
system("taskkill /im perfopt.exe /f");
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
