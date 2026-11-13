#!/usr/bin/env bash

set -e

kiota generate \
    --openapi src/Fargo.Api/Fargo.Api.json \
    --language CSharp \
    --class-name FargoApiClient \
    --namespace-name Fargo.Client \
    --output ./src/Fargo.Client/Generated \
