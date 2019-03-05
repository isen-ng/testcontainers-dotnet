#!/usr/bin/env bash

set -e

eval SOURCE="~/.nuget/packages"

PROJECT_DIR="src/Container.Abstractions"
VERSION="1.0.0-PRERELEASE"
CSPROJ="${PROJECT_DIR}/Container.Abstractions.csproj"
ASSEMBLY_NAME="TestContainers.Container.Abstractions"
CONFIGURATION="Release"

dotnet pack ${CSPROJ} -c ${CONFIGURATION} -property:Version=${VERSION}
dotnet nuget delete ${ASSEMBLY_NAME} -s ${SOURCE} --non-interactive || true
dotnet nuget push ${PROJECT_DIR}/bin/${CONFIGURATION}/${ASSEMBLY_NAME}.${VERSION}.nupkg -s ${SOURCE}