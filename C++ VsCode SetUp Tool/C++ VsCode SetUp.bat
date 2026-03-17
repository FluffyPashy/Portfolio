:: Custom made batch script to setup C/C++ in Microsoft Visual Studio Code
:: Made for educational use, by Rico Rothe


:: Check if ProjectFolder contains spaces
@echo off
for %%I in (.) do set CurrDirName=%%~nxI
if not "%CurrDirName%"=="%CurrDirName: =%" goto WORKSPACE_NAME_ERR

:: Introduction, important info.
echo Make sure to place the script inside of your workspacefolder before continuing!
@ping -n 3 localhost> nul
pause

:: Variable for multiple choice questions
set "SELECTION=0"
setlocal

:: Starting program Sequence
goto MSVC_PREREQUISITES

	:: Make sure the user has MSVC installed (manual)
	:MSVC_PREREQUISITES
		cls
		set /p SELECTION= Are Microsoft Visual C++ (MSVC) and C/C++ extension for VS Code already installed? ([y]/n): 
		if /I "%SELECTION%"=="" goto MSVC_PATHMETHOD
		if /I "%SELECTION%"=="Y" goto MSVC_PATHMETHOD
		if /I "%SELECTION%"=="N" goto MSVC_INSTALL
		echo please only enter (Y/y) or (N/n)!
		ping -n 2 localhost> nul
		goto MSVC_PREREQUISITES
	
	:: Gives further introductions for the user how to install MSVC
	:MSVC_INSTALL
		echo:
		echo Follow the instruction of the prerequisites from VsCode:
		echo:
		echo https://code.visualstudio.com/docs/cpp/config-msvc#_prerequisites
		echo:
		echo Press any button to continue with the configuration
		pause
		goto MSVC_PATHMETHOD
	
	:: Ask the user how they want to provide the VsDevCmd.bat path
	:MSVC_PATHMETHOD
		cls
		echo How would you like to locate VsDevCmd.bat?
		echo   [1] Search a drive automatically (slower)
		echo   [2] Paste the full path yourself
		echo:
		set /p SELECTION= Enter 1 or 2: 
		if "%SELECTION%" EQU "1" goto MSVC_DRIVESELECT
		if "%SELECTION%" EQU "2" goto MSVC_MANUALPATH
		echo Please only enter 1 or 2!
		ping -n 2 localhost> nul
		goto MSVC_PATHMETHOD
	
	:: Ask the user which drive to search
	:MSVC_DRIVESELECT
		cls
		set /p SELECTION= Enter the drive letter to search for VsDevCmd.bat (Default: C): 
		if /I "%SELECTION%" EQU "" set SELECTION=C
		set DRIVE=%SELECTION%
		goto MSVC_GETPATH
	
	:: Let the user paste the full path to VsDevCmd.bat
	:MSVC_MANUALPATH
		cls
		echo Paste the full path to VsDevCmd.bat and press Enter.
		echo Example: C:\Program Files\Microsoft Visual Studio\2022\Community\Common7\Tools\VsDevCmd.bat
		echo:
		set /p msvc_path= Path: 
		if "%msvc_path%" EQU "" goto MSVC_MANUALPATH
		if not exist "%msvc_path%" goto MSVC_PATHFAILED
		cd /D "%~dp0"
		goto CONFIGURATION
	
	:: Search the selected drive and recursive directories for VsDevCmd.bat
	:MSVC_GETPATH
		echo getting VsDevCmd path...
		set "msvc_path="

		:: Fast path: check common Visual Studio install locations first.
		for %%B in ("%DRIVE%:\Program Files\Microsoft Visual Studio" "%DRIVE%:\Program Files (x86)\Microsoft Visual Studio") do (
			if not defined msvc_path if exist "%%~B" (
				for %%Y in (2026 2022 2019 2017) do (
					for %%E in (Enterprise Professional Community BuildTools Preview) do (
						if exist "%%~B\%%Y\%%E\Common7\Tools\VsDevCmd.bat" set "msvc_path=%%~B\%%Y\%%E\Common7\Tools\VsDevCmd.bat"
					)
				)
			)
		)

		:: Fallback: recursive search only inside Program Files folders.
		for %%R in ("%DRIVE%:\Program Files" "%DRIVE%:\Program Files (x86)") do (
			if not defined msvc_path if exist "%%~R" (
				for /f "delims=" %%i in ('where /R "%%~R" VsDevCmd.bat 2^>nul') do if not defined msvc_path set "msvc_path=%%i"
			)
		)

		if not defined msvc_path goto MSVC_PATHFAILED
		echo %msvc_path%
		cd /D "%~dp0"
		goto CONFIGURATION
	
	:: If VsDevCmd.bat was not found return to the DriveSelect
	:MSVC_PATHFAILED
		echo Could not find VsDevCmd.bat using the provided location.
		echo make sure MSVC is properly installed!
		ping -n 5 localhost> nul
		goto MSVC_PATHMETHOD
	
	:: Generate all folders, launch.json and tasks.json for VsCode
	:CONFIGURATION
		echo:
		echo Generating configuration files...
		ping -n 3 localhost> nul
		set "msvc_path_json=%msvc_path:\=\\%"
	:: //////////////////////////////////////////////////////////////////////////////
	:: Create folders // Unable to create Source folder with spaces using src instead
	:: //////////////////////////////////////////////////////////////////////////////
		md ".vscode", "Build", "Header Files", "src", "Resource Files", "dependencies\include", "dependencies\lib" 2>nul
		cd ".vscode"
		type nul > "launch.json"
		(
			echo {
			echo 	"configurations": [
			echo 		{
			echo 			"name": "C/C++: cl.exe active file debug",
			echo 			"type": "cppvsdbg",
			echo 			"request": "launch",
			echo 			"program": "${workspaceFolder}\\Build\\${fileBasenameNoExtension}.exe",
			echo 			"args": [],
			echo 			"stopAtEntry": false,
			echo 			"cwd": "${workspaceFolder}\\Build",
			echo 			"environment": [],
			echo 			"console": "externalTerminal",
			echo 			"preLaunchTask": "C/C++: cl.exe active file compile",
			echo 		}
			echo 	],
			echo 	"version": "2.0.0"
			echo }
		) > "launch.json"
		type nul > "tasks.json"
		(
			echo {
			echo 	"version": "2.0.0",
			echo 	"windows": {
			echo 		"options": {
			echo 			"shell": {
			echo 				"executable": "cmd.exe",
			echo 				"args": [
			echo 					"/C",
			echo 					// The path to VsDevCmd.bat depends on the version of Visual Studio you have installed.
			echo 					// Open cmd.exe and type "WHERE /R C: VsDevCmd.bat" to find the path and paste it like "\"PATH\"".
			echo 					"\"%msvc_path_json%\"",
			echo 					"&&"
			echo 				]
			echo 			}
			echo 		}
			echo 	},
			echo 	"tasks": [
			echo 		{
			echo 			"label": "C/C++: cl.exe active file compile",
			echo 			"type": "shell",
			echo 			"command": "cl.exe",
			echo 			"args": [
			echo 				// Comment in if you want to use C++17, otherwise it will default to C++14	
			echo 				//"/std:c++17",
			echo 				"/Zi",
			echo 				"/EHsc",
			echo 				"/nologo",
			echo 				"",
			echo 				// Comment in if you want to use a separate header folder
			echo 				//"/I${workspaceFolder}\\Header Files",
			echo					"",
			echo 				// Comment in if you want to use a separate dependencies/include folder
			echo 				//"/I${workspaceFolder}\\dependencies\\include",
			echo 				"",
			echo 				"${workspaceFolder}/*.cpp",
			echo 				"",
			echo					// Comment in to compile all cpp files in the src folder
			echo 				//"${workspaceFolder}/src/*.cpp"
			echo 				"",
			echo 				"/Fe:${workspaceFolder}\\Build\\${fileBasenameNoExtension}.exe",
			echo 				"",
			echo					// Comment in to link against all libraries in the dependencies/lib folder
			echo 				//"/link",
			echo 				//"${workspaceFolder}\\dependencies\\lib\\*.lib"
			echo 			],
			echo 			"options": {
			echo 				"cwd": "${workspaceFolder}\\Build"
			echo 			},
			echo 			"problemMatcher": [
			echo 				"$msCompile"
			echo 			],
			echo 			"group": {
			echo 				"kind": "build",
			echo 				"isDefault": true
			echo 			},
			echo 			"detail": "Task generated by the debugger."
			echo 		}
			echo 	],
			echo }
		) > "tasks.json"
		cd ".."
		type nul > "main.cpp"
		echo:
		echo Done. Happy Coding :)
		@ping -n 5 localhost> nul
		exit /b 0
		
:WORKSPACE_NAME_ERR
echo Your workspaceFolder contains spaces, preventing the compiler to work as intended!
echo Invalid "My Folder"
echo Valid "MyFolder" / "My_Folder"
pause
exit /b 1