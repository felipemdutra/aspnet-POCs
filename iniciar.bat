@echo off
setlocal

cd /d "%~dp0"
set "ASPNETCORE_ENVIRONMENT=Development"

dotnet run --project "%~dp0src\UserRegistration.Api\UserRegistration.Api.csproj"

endlocal
