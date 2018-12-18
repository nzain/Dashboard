@echo off

ECHO cleaning the rpi's ~/Secrets dir...
ssh pi@raspberrypi "rm -rf /home/pi/Secrets/; pkill dotnet"
ECHO.

ECHO deploying into rpi's ~/Secrets dir...
scp -r .\nZain.Dashboard.Host\Secrets pi@raspberrypi:/home/pi/
ECHO.

PAUSE