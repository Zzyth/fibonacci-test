#!/bin/bash
set -ef

CONF_FILE="./appsettings.json"

PORT=${PORT:-'7122'}
export ASPNETCORE_URLS=http://+:${PORT}

exec "$@"
