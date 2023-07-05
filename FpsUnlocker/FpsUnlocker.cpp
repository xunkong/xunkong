#pragma comment(linker,"/subsystem:\"Windows\" /entry:\"mainCRTStartup\"")

#define KEY_TOGGLE VK_END
#define KEY_INCREASE VK_UP
#define KEY_INCREASE_SMALL VK_RIGHT
#define KEY_DECREASE VK_DOWN
#define KEY_DECREASE_SMALL VK_LEFT
#define FPS_TARGET 60

// https://gitee.com/Euphony_Facetious/genshin-fps-unlock

// 别碰这下面的东西，除非你懂

#ifdef WIN32
#error You must build in x64
#endif

#include <Windows.h>
#include <TlHelp32.h>
#include <vector>
#include <string>
#include <thread>
#include <Psapi.h>
#include <direct.h>
#include <iostream>
#include <sstream>
using namespace std;

std::string GamePath{};
int FpsValue = FPS_TARGET;

// 特征搜索 - 不是我写的 - 忘了在哪拷的
uintptr_t PatternScan(void* module, const char* signature)
{
	static auto pattern_to_byte = [](const char* pattern) {
		auto bytes = std::vector<int>{};
		auto start = const_cast<char*>(pattern);
		auto end = const_cast<char*>(pattern) + strlen(pattern);

		for (auto current = start; current < end; ++current) {
			if (*current == '?') {
				++current;
				if (*current == '?')
					++current;
				bytes.push_back(-1);
			}
			else {
				bytes.push_back(strtoul(current, &current, 16));
			}
		}
		return bytes;
	};

	auto dosHeader = (PIMAGE_DOS_HEADER)module;
	auto ntHeaders = (PIMAGE_NT_HEADERS)((std::uint8_t*)module + dosHeader->e_lfanew);

	auto sizeOfImage = ntHeaders->OptionalHeader.SizeOfImage;
	auto patternBytes = pattern_to_byte(signature);
	auto scanBytes = reinterpret_cast<std::uint8_t*>(module);

	auto s = patternBytes.size();
	auto d = patternBytes.data();

	for (auto i = 0ul; i < sizeOfImage - s; ++i) {
		bool found = true;
		for (auto j = 0ul; j < s; ++j) {
			if (scanBytes[i + j] != d[j] && d[j] != -1) {
				found = false;
				break;
			}
		}
		if (found) {
			return (uintptr_t)&scanBytes[i];
		}
	}
	return 0;
}

std::string GetLastErrorAsString(DWORD code)
{
	LPSTR buf = nullptr;
	FormatMessageA(FORMAT_MESSAGE_ALLOCATE_BUFFER | FORMAT_MESSAGE_FROM_SYSTEM | FORMAT_MESSAGE_IGNORE_INSERTS,
		NULL, code, MAKELANGID(LANG_NEUTRAL, SUBLANG_DEFAULT), (LPSTR)&buf, 0, NULL);
	std::string ret = buf;
	LocalFree(buf);
	return ret;
}

// 获取目标进程DLL信息
bool GetModule(HANDLE hProcess, std::string ModuleName, PMODULEENTRY32 pEntry)
{
	if (!pEntry)
		return false;

	std::vector<HMODULE> modules(1024);
	ZeroMemory(modules.data(), modules.size() * sizeof(HMODULE));
	DWORD cbNeeded = 0;

	if (!K32EnumProcessModules(hProcess, modules.data(), modules.size() * sizeof(HMODULE), &cbNeeded))
		return false;

	modules.resize(cbNeeded / sizeof(HMODULE));
	for (auto& it : modules)
	{
		char szModuleName[MAX_PATH]{};
		if (!K32GetModuleBaseNameA(hProcess, it, szModuleName, MAX_PATH))
			continue;

		if (ModuleName != szModuleName)
			continue;

		MODULEINFO modInfo{};
		if (!K32GetModuleInformation(hProcess, it, &modInfo, sizeof(MODULEINFO)))
			continue;

		pEntry->modBaseAddr = (BYTE*)modInfo.lpBaseOfDll;
		pEntry->modBaseSize = modInfo.SizeOfImage;
		return true;
	}


	return false;
}

// 通过进程名搜索进程ID
DWORD GetPID(std::string ProcessName)
{
	DWORD pid = 0;
	PROCESSENTRY32 pe32{};
	pe32.dwSize = sizeof(pe32);
	HANDLE snap = CreateToolhelp32Snapshot(TH32CS_SNAPPROCESS, 0);
	for (Process32First(snap, &pe32); Process32Next(snap, &pe32);)
	{
		if (pe32.szExeFile == ProcessName)
		{
			pid = pe32.th32ProcessID;
			break;
		}
	}
	CloseHandle(snap);
	return pid;
}



int main(int argc, char* argv[])
{

	if (argc < 2)
	{
		return 0;
	}
	char* exePath = nullptr;
	int TargetFPS = FpsValue, width = 0, height = 0;
	std::string popup = "";
	for (size_t i = 1; i < argc; i++)
	{
		if (!strcmp(argv[i], "-exe"))
		{
			if (i + 1 < argc)
			{
				exePath = argv[i + 1];
			}
		}
		if (!strcmp(argv[i], "-fps"))
		{
			if (i + 1 < argc)
			{
				TargetFPS = atoi(argv[i + 1]);
			}
		}
		if (!strcmp(argv[i], "-popupwindow"))
		{
			popup = "-popupwindow";
		}
		if (!strcmp(argv[i], "-screen-width"))
		{
			if (i + 1 < argc)
			{
				width = atoi(argv[i + 1]);
			}
		}
		if (!strcmp(argv[i], "-screen-height"))
		{
			if (i + 1 < argc)
			{
				height = atoi(argv[i + 1]);
			}
		}
	}

	if (TargetFPS <= 60)
	{
		TargetFPS = 60;
	}

	ostringstream comm;
	comm << popup;
	if (width > 0 && height > 0)
	{
		comm << " -screen-width " << width << " -screen-height " << height;
	}
	std::string CommandLine{ comm.str() };
	std::string ProcessPath = exePath;
	std::string ProcessDir{};

	if (ProcessPath.length() < 8)
		return 0;

	printf("FPS 解锁器\n");

	ProcessDir = ProcessPath.substr(0, ProcessPath.find_last_of("\\"));

	DWORD pid = GetPID(ProcessPath.substr(ProcessPath.find_last_of("\\") + 1));
	if (pid)
	{
		printf("检测到游戏已在运行！请手动关闭游戏从启动器启动\n");
		return 0;
	}

	STARTUPINFOA si{};
	PROCESS_INFORMATION pi{};
	if (!CreateProcessA(ProcessPath.c_str(), (LPSTR)CommandLine.c_str(), nullptr, nullptr, FALSE, 0, nullptr, ProcessDir.c_str(), &si, &pi))
	{
		DWORD code = GetLastError();
		printf("CreateProcess failed (%d): %s", code, GetLastErrorAsString(code).c_str());
		return 0;
	}

	CloseHandle(pi.hThread);
	printf("PID: %d\n", pi.dwProcessId);

	if (TargetFPS == 60)
	{
		return 0;
	}

	//HWND testwindow;//声明窗口句柄
	//testwindow = GetForegroundWindow();//当前窗口句柄
	//ShowWindow(testwindow, SW_HIDE);//隐藏窗口
	//SetConsoleTitleA("");

	// 等待UnityPlayer.dll加载和获取DLL信息
	MODULEENTRY32 hUnityPlayer{};
	MODULEENTRY32 hUserAssembly{};


	while (!GetModule(pi.hProcess, "UnityPlayer.dll", &hUnityPlayer))
		std::this_thread::sleep_for(std::chrono::milliseconds(100));

	while (!GetModule(pi.hProcess, "UserAssembly.dll", &hUserAssembly))
		std::this_thread::sleep_for(std::chrono::milliseconds(100));


	// 在本进程内申请UnityPlayer.dll大小的内存 - 用于特征搜索
	LPVOID mem = VirtualAlloc(nullptr, hUnityPlayer.modBaseSize + hUserAssembly.modBaseSize, MEM_COMMIT | MEM_RESERVE, PAGE_READWRITE);
	if (!mem)
	{
		DWORD code = GetLastError();
		printf("VirtualAlloc failed (%d): %s", code, GetLastErrorAsString(code).c_str());
		return 0;
	}

	// 把整个模块读出来
	ReadProcessMemory(pi.hProcess, hUnityPlayer.modBaseAddr, mem, hUnityPlayer.modBaseSize, nullptr);

	LPVOID ua = (LPVOID)((uintptr_t)mem + hUnityPlayer.modBaseSize);
	ReadProcessMemory(pi.hProcess, hUserAssembly.modBaseAddr, ua, hUserAssembly.modBaseSize, nullptr);

	PIMAGE_DOS_HEADER dos = (PIMAGE_DOS_HEADER)mem;
	PIMAGE_NT_HEADERS nt = (PIMAGE_NT_HEADERS)((uintptr_t)mem + dos->e_lfanew);

	printf("Searching for pattern...\n");

	uintptr_t pfps = 0;
	if (nt->FileHeader.TimeDateStamp < 0x645B245A) // <3.7
	{
		/*
			 7F 0F              jg   0x11
			 8B 05 ? ? ? ?      mov eax, dword ptr[rip+?]
		*/
		uintptr_t address = PatternScan(mem, "7F 0F 8B 05 ? ? ? ?");
		if (!address)
			return MessageBoxA(nullptr, "outdated fps pattern", "Error", MB_OK | MB_ICONERROR) == -1 && VirtualFree(mem, 0, MEM_RELEASE) == -1; // lazy returns, should always evaluate to false


		uintptr_t rip = address + 2;
		int32_t rel = *(int32_t*)(rip + 2);
		pfps = rip + rel + 6;
		pfps -= (uintptr_t)mem;
		pfps = (uintptr_t)hUnityPlayer.modBaseAddr + pfps;
	}
	else
	{


		/*
			 7F 0F              jg   0x11
			 8B 05 ? ? ? ?      mov eax, dword ptr[rip+?]
		*/
		uintptr_t address = PatternScan(ua, "E8 ? ? ? ? 85 C0 7E 07 E8 ? ? ? ? EB 05");
		if (!address)
			return MessageBoxA(nullptr, "outdated fps pattern", "Error", MB_OK | MB_ICONERROR) == -1 && VirtualFree(mem, 0, MEM_RELEASE) == -1;


		// 计算相对地址 (FPS)
		{
			uintptr_t rip = address;
			rip += *(int32_t*)(rip + 1) + 5;
			rip += *(int32_t*)(rip + 3) + 7;

			uintptr_t ptr = 0;
			uintptr_t data = rip - (uintptr_t)ua + (uintptr_t)hUserAssembly.modBaseAddr;
			while (!ptr)
			{
				ReadProcessMemory(pi.hProcess, (LPCVOID)data, &ptr, sizeof(uintptr_t), nullptr);
				std::this_thread::sleep_for(std::chrono::milliseconds(100));
			}

			rip = ptr - (uintptr_t)hUnityPlayer.modBaseAddr + (uintptr_t)mem;
			while (*(uint8_t*)rip == 0xE8 || *(uint8_t*)rip == 0xE9)
				rip += *(int32_t*)(rip + 1) + 5;

			pfps = rip + *(int32_t*)(rip + 2) + 6;
			pfps -= (uintptr_t)mem;
			pfps = (uintptr_t)hUnityPlayer.modBaseAddr + pfps;
		}
	}

	// 计算相对地址 (垂直同步)
	uintptr_t address = PatternScan(mem, "E8 ? ? ? ? 8B E8 49 8B 1E");
	uintptr_t pvsync = 0;
	if (address)
	{
		uintptr_t ppvsync = 0;
		uintptr_t rip = address;
		int32_t rel = *(int32_t*)(rip + 1);
		rip = rip + rel + 5;
		uint64_t rax = *(uint32_t*)(rip + 3);
		ppvsync = rip + rax + 7;
		ppvsync -= (uintptr_t)mem;
		printf("VSync Offset: %X\n", ppvsync);
		ppvsync = (uintptr_t)hUnityPlayer.modBaseAddr + ppvsync;

		uintptr_t buffer = 0;
		while (!buffer)
		{
			ReadProcessMemory(pi.hProcess, (LPCVOID)ppvsync, &buffer, sizeof(buffer), nullptr);
			std::this_thread::sleep_for(std::chrono::milliseconds(100));
		}

		rip += 7;
		pvsync = *(uint32_t*)(rip + 2);
		pvsync = buffer + pvsync;
	}

	VirtualFree(mem, 0, MEM_RELEASE);
	printf("Done\n\n");

	DWORD dwExitCode = STILL_ACTIVE;
	while (dwExitCode == STILL_ACTIVE)
	{
		GetExitCodeProcess(pi.hProcess, &dwExitCode);

		// 每两秒检查一次
		std::this_thread::sleep_for(std::chrono::seconds(2));
		int fps = 0;
		ReadProcessMemory(pi.hProcess, (LPVOID)pfps, &fps, sizeof(fps), nullptr);
		if (fps == -1)
			continue;
		if (fps != TargetFPS)
			WriteProcessMemory(pi.hProcess, (LPVOID)pfps, &TargetFPS, sizeof(TargetFPS), nullptr);

		int vsync = 0;
		ReadProcessMemory(pi.hProcess, (LPVOID)pvsync, &vsync, sizeof(vsync), nullptr);
		if (vsync)
		{
			vsync = 0;
			// 关闭垂直同步
			WriteProcessMemory(pi.hProcess, (LPVOID)pvsync, &vsync, sizeof(vsync), nullptr);
		}
	}

	CloseHandle(pi.hProcess);
	TerminateProcess((HANDLE)-1, 0);

	return 0;
}