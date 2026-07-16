#!/usr/bin/env bash
set -e

cd "$(dirname "${BASH_SOURCE[0]}")"
export ASPNETCORE_ENVIRONMENT=Development

dotnet run --project "src/UserRegistration.Api/UserRegistration.Api.csproj"
