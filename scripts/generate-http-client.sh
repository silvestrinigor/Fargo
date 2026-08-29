#!/usr/bin/env bash

set -e

kiota generate \
    --openapi src/Fargo.Http/Fargo.Http.json \
    --language CSharp \
    --class-name FargoApiClient \
    --namespace-name Fargo.Http.Client \
    --output ./src/Fargo.Http.Client/Generated \
