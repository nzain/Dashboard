@echo off

ECHO starting ASP.Net Server...
ssh pi@raspberrypi "dotnet /home/pi/webapp/nZain.Dashboard.Host.dll"
ECHO.