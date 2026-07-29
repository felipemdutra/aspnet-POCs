# syntax=docker/dockerfile:1.20

# ============================================================================
# Stage 1 - restore: download NuGet dependencies into a shared cache.
# ============================================================================
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS restore
WORKDIR /src

# Copy ONLY the .csproj files first. As long as the dependency list does
# not change, this layer is reused and the slow `dotnet restore` step
# is skipped on subsequent builds.
COPY ["src/UserRegistration.Api/UserRegistration.Api.csproj",        "src/UserRegistration.Api/"]
COPY ["src/UserRegistration.Application/UserRegistration.Application.csproj", "src/UserRegistration.Application/"]
COPY ["src/UserRegistration.Domain/UserRegistration.Domain.csproj",  "src/UserRegistration.Domain/"]
COPY ["src/UserRegistration.Infrastructure/UserRegistration.Infrastructure.csproj", "src/UserRegistration.Infrastructure/"]

# `--mount=type=cache` is a BuildKit feature that persists the NuGet
# package folder across builds, so packages downloaded once do not have
# to be re-downloaded on the next build. The folder is not part of the
# final image and disappears with the build sandbox.
RUN --mount=type=cache,target=/root/.nuget/packages \
    dotnet restore "src/UserRegistration.Api/UserRegistration.Api.csproj"

# ============================================================================
# Stage 2 - build: compile and publish a release build of the API.
# ============================================================================
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Bring in the rest of the source code on top of the restore layer.
COPY . .

# Reuse the same NuGet cache mount so publish does not redownload.
RUN --mount=type=cache,target=/root/.nuget/packages \
    dotnet publish "src/UserRegistration.Api/UserRegistration.Api.csproj" \
        -c Release \
        -o /app/publish \
        /p:UseAppHost=false \
        /p:PublishTrimmed=false

# ============================================================================
# Stage 3 - final: ship only the ASP.NET runtime + the published output.
# ============================================================================
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Copy the published binaries from the build stage, owned by app:app.
COPY --from=build --chown=app:app /app/publish .

RUN mkdir -p /app/data && chown app:app /app/data

# SQLite data lives under /app/data so a named volume can be mounted
# here to survive container restarts.
VOLUME ["/app/data"]

# Run as the pre-created non-root "app" user that ships with the runtime
# image. Containers should never run as root unless they really have to.
USER app
ENV ConnectionStrings__Default="Data Source=/app/data/users.db"
# Match the port the app hard-codes in Program.cs.
ENV ASPNETCORE_URLS=http://+:5291
EXPOSE 5291

ENTRYPOINT ["dotnet", "UserRegistration.Api.dll"]