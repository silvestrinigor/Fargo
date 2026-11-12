#!/usr/bin/env bash

set -e

kiota generate \
    --openapi src/Fargo.ServiceHttp/Fargo.ServiceHttp.json \
    --language CSharp \
    --class-name FargoApiClient \
    --namespace-name Fargo.ClientHttp \
    --output ./src/Fargo.ClientHttp/Generated \
