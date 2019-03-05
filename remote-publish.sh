#!/usr/bin/env bash

set -e

API_KEY=$1
SOURCE=$2

PROJECT_DIR="src/Container.Abstractions"
CSPROJ="${PROJECT_DIR}/Container.Abstractions.csproj"
ASSEMBLY_NAME="TestContainers.Container.Abstractions"
CONFIGURATION="Release"

dotnet pack ${CSPROJ} -c ${CONFIGURATION}
dotnet nuget push ${PROJECT_DIR}/bin/${CONFIGURATION}/${ASSEMBLY_NAME}.*.nupkg -s ${SOURCE}