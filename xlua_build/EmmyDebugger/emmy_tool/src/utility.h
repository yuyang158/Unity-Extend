#pragma once
#include <Windows.h>
#include <string>
#include <vector>

struct ExeInfo {
	size_t			entryPoint;
	bool            managed;
	bool            i386;
};

bool GetExeInfo(LPCSTR fileName, ExeInfo&info);

/**
* Returns the top level window for the specified process. The first such window
* that's found is returned.
*/
HWND GetProcessWindow(DWORD processId);

struct Process {
	unsigned int    id;     // Windows process identifier
	std::string     name;   // Executable name
	std::string     title;  // Name from the main window of the process.
	std::string     path;   // Full path
	std::string     iconPath;// Icon path
};

/**
* Returns all of the processes on the machine that can be debugged.
*/
void GetProcesses(std::vector<Process>& processes);

int GetProcessByName(const char* name);