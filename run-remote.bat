@echo off

ECHO starting ASP.Net Server...
ssh pi@raspberrypi "pkill dotnet; dotnet /home/pi/webapp/nZain.Dashboard.Host.dll"
ECHO.

PAUSE