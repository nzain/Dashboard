@echo off

IF EXIST %~dp0\webapp (
	ECHO cleaning up...
	rmdir /s /q %~dp0\webapp
) ELSE (
	ECHO Nothing to clean up!
)
ECHO.

REM  -r linux-arm
dotnet publish -c Release -o %~dp0\webapp .\nZain.Dashboard.Host\nZain.Dashboard.Host.csproj
ECHO.

ECHO cleaning the rpi's ~/webapp dir...
ssh pi@raspberrypi "rm -rf /home/pi/webapp/; pkill dotnet"
ECHO.

ECHO deploying into rpi's ~/webapp dir...
scp -r .\webapp pi@raspberrypi:/home/pi/
ECHO.

ECHO starting ASP.Net Server...
ssh pi@raspberrypi "dotnet /home/pi/webapp/nZain.Dashboard.Host.dll"
ECHO.
REM ECHO reboot the rpi...
REM ssh pi@raspberrypi "sudo reboot"

PAUSE