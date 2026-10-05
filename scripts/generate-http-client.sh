#!/usr/bin/env bash

set -e

kiota generate \
    --openapi src/server/Fargo.ServiceHttp/Fargo.ServiceHttp.json \
    --language CSharp \
    --class-name FargoApiClient \
    --namespace-name Fargo.ClientHttp \
    --output ./src/client/Fargo.ClientHttp/Generated \
