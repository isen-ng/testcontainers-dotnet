#!/usr/bin/env bash

set -e

eval SOURCE="~/.nuget/packages"

CONFIGURATION="Release"
VERSION="1.4.7-SNAPSHOT"

function local_publish() {
    dotnet pack $1 -c ${CONFIGURATION} -property:Version=${VERSION}
    dotnet nuget delete $2 ${VERSION} -s ${SOURCE} --non-interactive || true
    dotnet nuget push $3/bin/${CONFIGURATION}/$2.${VERSION}.nupkg -s ${SOURCE}
}

PROJECT_DIR="src/Container.Abstractions"
CSPROJ="${PROJECT_DIR}/Container.Abstractions.csproj"
ASSEMBLY_NAME="TestContainers.Container.Abstractions"

local_publish ${CSPROJ} ${ASSEMBLY_NAME} ${PROJECT_DIR}

PROJECT_DIR="src/Container.Database"
CSPROJ="${PROJECT_DIR}/Container.Database.csproj"
ASSEMBLY_NAME="TestContainers.Container.Database"

local_publish ${CSPROJ} ${ASSEMBLY_NAME} ${PROJECT_DIR}

PROJECT_DIR="src/Container.Database.AdoNet"
CSPROJ="${PROJECT_DIR}/Container.Database.AdoNet.csproj"
ASSEMBLY_NAME="TestContainers.Container.Database.AdoNet"

local_publish ${CSPROJ} ${ASSEMBLY_NAME} ${PROJECT_DIR}

PROJECT_DIR="src/Container.Database.PostgreSql"
CSPROJ="${PROJECT_DIR}/Container.Database.PostgreSql.csproj"
ASSEMBLY_NAME="TestContainers.Container.Database.PostgreSql"

local_publish ${CSPROJ} ${ASSEMBLY_NAME} ${PROJECT_DIR}

PROJECT_DIR="src/Container.Database.MsSql"
CSPROJ="${PROJECT_DIR}/Container.Database.MsSql.csproj"
ASSEMBLY_NAME="TestContainers.Container.Database.MsSql"

local_publish ${CSPROJ} ${ASSEMBLY_NAME} ${PROJECT_DIR}

PROJECT_DIR="src/Container.Database.ArangoDb"
CSPROJ="${PROJECT_DIR}/Container.Database.ArangoDb.csproj"
ASSEMBLY_NAME="TestContainers.Container.Database.ArangoDb"

local_publish ${CSPROJ} ${ASSEMBLY_NAME} ${PROJECT_DIR}
