#pragma once

#ifdef EXPORT
    #define dll extern "C" __declspec(dllexport)
#else
    #define dll extern "C" __declspec(dllimport)
#endif

#include <windows.h>
#include <psapi.h>

dll void __stdcall clearMemory();
dll int __stdcall getCpuClockSpeed();
dll double __stdcall getCpuUtilization();
