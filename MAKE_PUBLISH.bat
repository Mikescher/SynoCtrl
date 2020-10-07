@ECHO OFF

SET vers=ERR
SET hash=ERR

FOR /F %%i IN ('git describe --abbrev^=0 --tags') DO SET vers=%%i
FOR /F %%i IN ('git rev-parse --short HEAD') DO SET hash=%%i

echo "VERSION := %vers%"
echo "COMMIT  := %hash%"

echo.

rmdir /s /q Publish\Self-Contained
rmdir /s /q Publish\Linux-x64
rmdir /s /q Publish\Win-x64

echo.

dotnet publish Source\SynoCtrl --nologo -p:PublishProfile=Production_Linux-x64
echo.
dotnet publish Source\SynoCtrl --nologo -p:PublishProfile=Production_SelfContained
echo.
dotnet publish Source\SynoCtrl --nologo -p:PublishProfile=Production_Win-x64

echo.
echo.

7z a -tzip Publish/SynoCtrl_%vers%_%hash%_windows-selfcontained.zip  %CD%\Publish\Self-Contained\SynoCtrl.exe
echo.
7z a -tzip Publish/SynoCtrl_%vers%_%hash%_windows-x64.zip            %CD%\Publish\Win-x64\SynoCtrl.exe
echo.
7z a -tzip Publish/SynoCtrl_%vers%_%hash%_linux-x64.zip              %CD%\Publish\Linux-x64\SynoCtrl

echo.
echo.

PAUSE