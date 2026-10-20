#!/usr/bin/env bash

# Regenerates the Swagger 2.0 document that DocFX uses for the REST API
# reference. DocFX only consumes Swagger 2.0, while the server emits OpenAPI
# 3.x, so this script:
#   1. Builds the HTTP API project, which emits the OpenAPI 3.x document
#      (via the OpenApiGenerateDocuments MSBuild setting).
#   2. Converts that document to Swagger 2.0 for DocFX.
#
# Run it before `docfx docfx.json`. The docs CI workflow runs it automatically.

set -e

OPENAPI_JSON="src/server/Fargo.ServiceHttp/Fargo.ServiceHttp.json"
SWAGGER_JSON="docs/web-api/reference/fargo.swagger.json"

echo "Generating OpenAPI document from the HTTP API project..."
dotnet build src/server/Fargo.ServiceHttp/Fargo.ServiceHttp.csproj \
    -c Release \
    --nologo

echo "Converting OpenAPI 3.x to Swagger 2.0 for DocFX..."
dotnet run tools/openapi-to-swagger/convert.cs -- \
    "$OPENAPI_JSON" \
    "$SWAGGER_JSON"

echo "Done. REST API reference spec written to $SWAGGER_JSON."
