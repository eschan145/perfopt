#pragma once

#ifdef EXPORT
    #define export extern "C" __declspec(dllexport)
#else
    #define export extern "C" __declspec(dllimport) __stdcall
#endif

#include <windows.h>
#include <psapi.h>

export void __stdcall clearMemory();
