# C++ VS Code Setup Batch

Windows batch script for initializing a basic C/C++ workspace in Visual Studio Code with MSVC.

The script creates a starter project structure and generates `.vscode/launch.json` and `.vscode/tasks.json` so the workspace can build and debug with `cl.exe`.

## Overview

The script walks through a short setup flow and then prepares a Visual Studio Code workspace for MSVC-based C/C++ development.

It can:

- validate that the current workspace folder name does not contain spaces
- confirm that MSVC and the VS Code C/C++ extension are installed
- direct users to the official VS Code MSVC setup guide when prerequisites are missing
- locate `VsDevCmd.bat` automatically by searching common Visual Studio installation paths
- accept a manually provided `VsDevCmd.bat` path
- generate `.vscode/launch.json` for debugging the compiled executable
- generate `.vscode/tasks.json` for building with `cl.exe`
- create a starter folder structure for headers, sources, resources, and dependencies
- create an empty `main.cpp`

## Prerequisites

Before running the script, ensure the following are installed:

- Windows
- Visual Studio Build Tools or Visual Studio with MSVC installed
- Visual Studio Code
- the Microsoft C/C++ extension for Visual Studio Code

Official prerequisite guide:

- https://code.visualstudio.com/docs/cpp/config-msvc#_prerequisites

## Usage

### 1. Place the script in the project root

Copy `C++ VsCode SetUp.bat` into the root folder of the C/C++ project you want to configure.

Run the script from the workspace folder that should receive the generated files.

### 2. Ensure the workspace folder name contains no spaces

The script exits immediately if the current folder name contains spaces.

Examples:

- valid: `MyProject`
- valid: `My_Project`
- invalid: `My Project`

### 3. Run the batch file

Double-click the batch file or run it from Command Prompt.

The script prompts for:

1. whether MSVC and the VS Code C/C++ extension are already installed
2. how `VsDevCmd.bat` should be located

### 4. Choose how to locate `VsDevCmd.bat`

Available options:

- `1` Search automatically
- `2` Enter the full path manually

#### Automatic search

If automatic search is selected, the script asks for a drive letter and then:

- checks common Visual Studio installation folders first
- falls back to a recursive search inside `Program Files` and `Program Files (x86)`

This is the preferred option when Visual Studio was installed in a standard location.

#### Manual path entry

If manual entry is selected, paste the full path to `VsDevCmd.bat`.

Example:

```bat
C:\Program Files\Microsoft Visual Studio\2022\Community\Common7\Tools\VsDevCmd.bat
```

## Generated Files and Folders

After a successful run, the script creates the following structure if it does not already exist:

```text
.vscode/
Build/
Header Files/
src/
Resource Files/
dependencies/
  include/
  lib/
main.cpp
```

It also generates:

- `.vscode/launch.json`
- `.vscode/tasks.json`

## Generated Configuration

### launch.json

The generated launch configuration:

- uses `cppvsdbg`
- launches the compiled executable from the `Build` folder
- runs the build task before launch
- opens the program in an external terminal

### tasks.json

The generated build task:

- runs through `cmd.exe`
- initializes the MSVC environment using `VsDevCmd.bat`
- compiles with `cl.exe`
- enables debug information with `/Zi`
- enables standard exception handling with `/EHsc`
- suppresses the compiler banner with `/nologo`
- writes the executable to the `Build` folder
- uses the `$msCompile` problem matcher in Visual Studio Code

The file also includes commented examples for:

- enabling C++17
- adding a custom include path for `Header Files`
- adding a custom include path for `dependencies/include`
- compiling additional `.cpp` files from `src`
- linking `.lib` files from `dependencies/lib`

## Visual Studio Search Targets

The script checks these Visual Studio version folders:

- `2026`
- `2022`
- `2019`
- `2017`

It searches for these editions:

- `Enterprise`
- `Professional`
- `Community`
- `BuildTools`
- `Preview`

## Typical Workflow

1. Open the workspace in Visual Studio Code.
2. Place header files in `Header Files`.
3. Place source files in the project root or in `src`.
4. Update `tasks.json` if the project needs additional source paths, include paths, or libraries.
5. Press `Ctrl+Shift+B` to build.
6. Start debugging with the generated launch configuration.

## Limitations

This script is intentionally lightweight and has a few constraints:

- it only supports Windows
- it is designed for MSVC, not MinGW or Clang
- it requires a workspace folder name with no spaces
- it creates a generic starter configuration, so larger projects will usually require manual adjustments
- the generated task includes commented options that must be enabled manually for multi-file or library-based projects

## When You May Need to Edit tasks.json

You will likely need to customize the generated task if the project:

- stores all source files in `src`
- uses custom include directories
- links external `.lib` files
- requires a newer C++ language standard
- contains more than a single entry-point file

## Troubleshooting

### The script reports that the workspace folder contains spaces

Rename the project folder so it does not contain spaces, then run the script again.

### `VsDevCmd.bat` cannot be found automatically

Try the manual path option and paste the full path to `VsDevCmd.bat`.

### Build fails in Visual Studio Code

Open `.vscode/tasks.json` and confirm that:

- the `VsDevCmd.bat` path is correct
- the source file paths match the project layout
- any required include and library paths are enabled

## Intended Use

This script is best suited for:

- students learning C/C++ with MSVC
- small educational projects
- quickly bootstrapping a new Visual Studio Code C++ workspace

It is not a full build system, but it provides a practical starting point with minimal manual setup.