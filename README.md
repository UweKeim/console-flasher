# Console Flasher

A small command line tool to flash the console window via the [`FlashConsoleEx` Windows API call](https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-flashwindowex).

I'm using this tool heavily inside my build scripts (which are CMD batch files) to signal myself if a build script fails.

By flashing the CMD window I can do other things during long running build scripts and do not have to constantly monitor the running build processes.

### Example usage

This is an example of a "build.cmd" batch script I'm using the tool with:

```cmd
PUSHD 
CD /d %~dp0 

SET CSSCRIPT_DIR=\\build-server\tools\cs-script

"\\build-server\tools\cs-script\cscs.exe" /dbg "%~dp0\do-build.cs" %*
if %ERRORLEVEL% GEQ 1 "\\build-server\tools\console-flasher.exe" & EXIT /B %ERRORLEVEL%

POPD 
EXIT /B 0
```

The above script calls the actual build script (which is a [CS-Script](https://www.cs-script.net/) script in my case) and if the build exists with an error, the console window is flashed.
