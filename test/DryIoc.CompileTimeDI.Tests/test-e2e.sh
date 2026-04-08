#!/bin/bash
# End-to-end test for DryIoc.dll NuGet package CompileTime DI install experience
# 
# This script:
# 1. Builds and packs the DryIoc.dll package to .dist/packages/
# 2. Clears any old test artifacts
# 3. Builds the test project (first build - triggers CopyCompileTimeDIToProject via BeforeTargets="Build")
# 4. Verifies that the T4 files were copied to the test project
# 5. Runs the test project
# 6. Simulates a package update and verifies user files are NOT overwritten

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT_DIR="$SCRIPT_DIR/../.."
TEST_DIR="$SCRIPT_DIR"
PACKAGE_DIR="$ROOT_DIR/.dist/packages"

echo ""
echo "=== DryIoc CompileTimeDI NuGet Install End-to-End Test ==="
echo ""

# Step 1: Pack the DryIoc.dll package
echo "--- Step 1: Building and packing DryIoc.dll ---"
dotnet build "$ROOT_DIR/src/DryIoc/DryIoc.csproj" -c Release \
    -p:SkipCompTimeGen=true \
    -p:LatestSupportedNet=net9.0 \
    -v:minimal
echo "Package created at: $PACKAGE_DIR"
ls "$PACKAGE_DIR"/DryIoc.dll.*.nupkg

echo ""
echo "--- Step 2: Clear global NuGet cache for DryIoc.dll (to force fresh install) ---"
rm -rf "$HOME/.nuget/packages/dryioc.dll"
rm -rf "$TEST_DIR/CompileTimeDI"
rm -rf "$TEST_DIR/.config"
rm -rf "$TEST_DIR/bin"
rm -rf "$TEST_DIR/obj"

echo ""
echo "--- Step 3: Build test project (first build - triggers CopyCompileTimeDIToProject) ---"
dotnet build "$TEST_DIR/DryIoc.CompileTimeDI.Tests.csproj" -c Debug -v:minimal

echo ""
echo "--- Step 4: Verify T4 files were copied ---"
EXPECTED_FILES=(
    "CompileTimeDI/Container.Generated.tt"
    "CompileTimeDI/CompileTimeRegistrations.ttinclude"
    "CompileTimeDI/CompileTimeRegistrations.Example.cs"
    "CompileTimeDI/Container.Generated.cs"
    ".config/dotnet-tools.json"
)

ALL_OK=true
for f in "${EXPECTED_FILES[@]}"; do
    if [ -f "$TEST_DIR/$f" ]; then
        echo "  ✓ $f"
    else
        echo "  ✗ MISSING: $f"
        ALL_OK=false
    fi
done

if [ "$ALL_OK" != "true" ]; then
    echo ""
    echo "ERROR: Some expected files were not copied!"
    exit 1
fi

echo ""
echo "--- Step 5: Run the test project ---"
dotnet run --project "$TEST_DIR/DryIoc.CompileTimeDI.Tests.csproj" -c Debug --no-build

echo ""
echo "--- Step 6: Simulate package update (should NOT overwrite CompileTimeRegistrations.ttinclude or Container.Generated.cs) ---"
# Add a marker comment to the user-customizable file
MARKER="<# USER_CUSTOM_MARKER_DO_NOT_OVERWRITE #>"
echo "$MARKER" >> "$TEST_DIR/CompileTimeDI/CompileTimeRegistrations.ttinclude"
TTINCLUDE_HASH_BEFORE=$(md5sum "$TEST_DIR/CompileTimeDI/CompileTimeRegistrations.ttinclude")

# Add a marker to Container.Generated.cs (simulates user-generated content)
echo "// USER_GENERATED_MARKER" >> "$TEST_DIR/CompileTimeDI/Container.Generated.cs"
GENERATED_HASH_BEFORE=$(md5sum "$TEST_DIR/CompileTimeDI/Container.Generated.cs")

# Re-build (simulates restoring/building after a package update)
dotnet build "$TEST_DIR/DryIoc.CompileTimeDI.Tests.csproj" -c Debug -v:minimal --no-restore

TTINCLUDE_HASH_AFTER=$(md5sum "$TEST_DIR/CompileTimeDI/CompileTimeRegistrations.ttinclude")
GENERATED_HASH_AFTER=$(md5sum "$TEST_DIR/CompileTimeDI/Container.Generated.cs")

if [ "$TTINCLUDE_HASH_BEFORE" == "$TTINCLUDE_HASH_AFTER" ]; then
    echo "  ✓ CompileTimeRegistrations.ttinclude was NOT overwritten (user customizations preserved)"
else
    echo "  ✗ FAIL: CompileTimeRegistrations.ttinclude was overwritten!"
    exit 1
fi

if [ "$GENERATED_HASH_BEFORE" == "$GENERATED_HASH_AFTER" ]; then
    echo "  ✓ Container.Generated.cs was NOT overwritten (SkipCompTimeGen=true skips T4 generation)"
else
    echo "  ✗ FAIL: Container.Generated.cs was overwritten!"
    exit 1
fi

echo ""
echo "=== All end-to-end tests PASSED ==="
echo ""
