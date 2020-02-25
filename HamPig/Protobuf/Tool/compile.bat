@echo off

set COMPILER=%cd%\protoc
set SRC=.\Source
set DST=.\cs

del /f /s /q %DST%
for %%i in (%SRC%\*.proto) do (
	echo compile file : %%~fi
	%COMPILER% %%i --csharp_out=%DST%
)

pause