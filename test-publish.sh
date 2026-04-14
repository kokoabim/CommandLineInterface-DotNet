#!/usr/bin/env bash

set -eo pipefail

dotnet publish -c Release -o ./dist/test-build/Kokoabim.CommandLineInterface src/Kokoabim.CommandLineInterface/Kokoabim.CommandLineInterface.csproj
dotnet publish -c Release -o ./dist/test-build/Kokoabim.CommandLineInterface.AppHost src/Kokoabim.CommandLineInterface.AppHost/Kokoabim.CommandLineInterface.AppHost.csproj