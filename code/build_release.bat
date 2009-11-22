@echo off
%windir%\Microsoft.NET\Framework\v4.0.21006\msbuild.exe /p:Configuration=Release /verbosity:m /fl /flp:Verbosity=normal core\core.sln
if %errorlevel% NEQ 0 goto end
%windir%\Microsoft.NET\Framework\v4.0.21006\msbuild.exe /p:Configuration=Release /verbosity:m /fl /flp:Verbosity=normal;Append plugins\plugins.sln
if %errorlevel% NEQ 0 goto end
%windir%\Microsoft.NET\Framework\v4.0.21006\msbuild.exe /p:Configuration=Release /verbosity:m /fl /flp:Verbosity=normal;Append frontend\frontend.sln
:end
pause