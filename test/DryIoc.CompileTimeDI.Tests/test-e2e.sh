#!/bin/bash
# End-to-end test for DryIoc.dll NuGet package CompileTime DI install/update experience.
set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT_DIR="$SCRIPT_DIR/../.."
TEST_DIR="$SCRIPT_DIR"

echo "=== DryIoc CompileTimeDI NuGet E2E Test ==="

echo "--- Pack DryIoc.dll ---"
dotnet build "$ROOT_DIR/src/DryIoc/DryIoc.csproj" -c Release -p:SkipCompTimeGen=true -p:LatestSupportedNet=net9.0 -v:minimal
ls "$ROOT_DIR/.dist/packages"/DryIoc.dll.*.nupkg

echo "--- Fresh install (clear caches) ---"
rm -rf "$HOME/.nuget/packages/dryioc.dll" "$TEST_DIR/CompileTimeDI" "$TEST_DIR/.config" "$TEST_DIR/bin" "$TEST_DIR/obj"

echo "--- First build (triggers CopyCompileTimeDIToProject) ---"
dotnet build "$TEST_DIR/DryIoc.CompileTimeDI.Tests.csproj" -c Debug -v:minimal

echo "--- Verify copied files ---"
for f in "CompileTimeDI/Container.Generated.tt" "CompileTimeDI/CompileTimeRegistrations.ttinclude" \
          "CompileTimeDI/CompileTimeRegistrations.Example.cs" "CompileTimeDI/Container.Generated.cs" \
          ".config/dotnet-tools.json"; do
    [ -f "$TEST_DIR/$f" ] && echo "  ✓ $f" || { echo "  ✗ MISSING: $f"; exit 1; }
done

echo "--- Run ---"
dotnet run --project "$TEST_DIR/DryIoc.CompileTimeDI.Tests.csproj" -c Debug --no-build

echo "--- Update simulation: user files must NOT be overwritten ---"
echo "<# CUSTOM #>" >> "$TEST_DIR/CompileTimeDI/CompileTimeRegistrations.ttinclude"
echo "// CUSTOM" >> "$TEST_DIR/CompileTimeDI/Container.Generated.cs"
HASH_TT=$(md5sum "$TEST_DIR/CompileTimeDI/CompileTimeRegistrations.ttinclude")
HASH_CS=$(md5sum "$TEST_DIR/CompileTimeDI/Container.Generated.cs")

dotnet build "$TEST_DIR/DryIoc.CompileTimeDI.Tests.csproj" -c Debug -v:minimal --no-restore

[ "$HASH_TT" = "$(md5sum "$TEST_DIR/CompileTimeDI/CompileTimeRegistrations.ttinclude")" ] \
    && echo "  ✓ CompileTimeRegistrations.ttinclude preserved" || { echo "  ✗ OVERWRITTEN!"; exit 1; }
[ "$HASH_CS" = "$(md5sum "$TEST_DIR/CompileTimeDI/Container.Generated.cs")" ] \
    && echo "  ✓ Container.Generated.cs preserved" || { echo "  ✗ OVERWRITTEN!"; exit 1; }

echo "=== All E2E tests PASSED ==="
