#!/bin/sh
set -e

# Render injects PORT at runtime; bind Kestrel to all interfaces on that port.
if [ -n "${PORT}" ]; then
  export ASPNETCORE_URLS="http://+:${PORT}"
elif [ -z "${ASPNETCORE_URLS}" ]; then
  export ASPNETCORE_URLS="http://+:8080"
fi

exec dotnet brewbase.server.dll
