#!/bin/bash
# End-to-end test for the DryIoc.dll NuGet package CompileTime DI install/update experience.
set -eu

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT_DIR="$(cd "$SCRIPT_DIR/../.." && pwd)"
TEST_DIR="$(mktemp -d)"
trap 'rm -rf "$TEST_DIR"' EXIT
export NUGET_PACKAGES="$TEST_DIR/packages"
export DOTNET_CLI_HOME="$TEST_DIR/dotnet-home"
export DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1
VERSION="6.0.0-e2e"

echo "--- Pack DryIoc.dll ---"
dotnet build "$ROOT_DIR/src/DryIoc/DryIoc.csproj" -c Release \
    -p:TargetFrameworks=net10.0 -p:Version="$VERSION" -v:minimal

create_consumer() {
    local name="$1"
    dotnet new console -o "$TEST_DIR/$name" -f net10.0 --no-restore
    dotnet add "$TEST_DIR/$name" package DryIoc.dll --version "$VERSION" --no-restore
    cp "$SCRIPT_DIR/Program.cs" "$TEST_DIR/$name/Program.cs"
}

build_consumer() {
    dotnet build "$TEST_DIR/$1" --source "$ROOT_DIR/.dist/packages" -p:NuGetAudit=false -v:minimal "${@:2}"
}

echo "--- Default consumer: no scaffolding, including on subsequent builds ---"
create_consumer default
build_consumer default
build_consumer default
[ ! -e "$TEST_DIR/default/CompileTimeDI" ]
[ ! -e "$TEST_DIR/default/.config/dotnet-tools.json" ]
dotnet run --project "$TEST_DIR/default" --no-build

echo "--- ExcludeAssets=build: package targets are not imported, even with opt-in ---"
create_consumer exclude
sed 's/Version="6.0.0-e2e"/Version="6.0.0-e2e" ExcludeAssets="build"/' \
    "$TEST_DIR/exclude/exclude.csproj" > "$TEST_DIR/exclude.csproj"
mv "$TEST_DIR/exclude.csproj" "$TEST_DIR/exclude/exclude.csproj"
build_consumer exclude -p:EnableDryIocCompileTimeDI=true
[ ! -e "$TEST_DIR/exclude/CompileTimeDI" ]
[ ! -e "$TEST_DIR/exclude/.config/dotnet-tools.json" ]
dotnet run --project "$TEST_DIR/exclude" --no-build

echo "--- Opt in to scaffolding ---"
create_consumer optin
build_consumer optin -p:EnableDryIocCompileTimeDI=true -p:SkipCompTimeGen=true
for f in "CompileTimeDI/Container.Generated.tt" "CompileTimeDI/CompileTimeRegistrations.ttinclude" \
          "CompileTimeDI/CompileTimeExampleServices.cs" "CompileTimeDI/Container.Generated.cs" \
          ".config/dotnet-tools.json"; do
    [ -f "$TEST_DIR/optin/$f" ] || { echo "MISSING: $f"; exit 1; }
done
dotnet run --project "$TEST_DIR/optin" --no-build

echo "--- User-owned files are preserved on subsequent opt-in builds ---"
echo "<# CUSTOM #>" >> "$TEST_DIR/optin/CompileTimeDI/CompileTimeRegistrations.ttinclude"
echo "// CUSTOM" >> "$TEST_DIR/optin/CompileTimeDI/Container.Generated.cs"
cp "$TEST_DIR/optin/CompileTimeDI/CompileTimeRegistrations.ttinclude" "$TEST_DIR/registrations"
cp "$TEST_DIR/optin/CompileTimeDI/Container.Generated.cs" "$TEST_DIR/generated"
build_consumer optin -p:EnableDryIocCompileTimeDI=true -p:SkipCompTimeGen=true
cmp "$TEST_DIR/registrations" "$TEST_DIR/optin/CompileTimeDI/CompileTimeRegistrations.ttinclude"
cmp "$TEST_DIR/generated" "$TEST_DIR/optin/CompileTimeDI/Container.Generated.cs"

echo "--- Disabling opt-in neither regenerates nor recreates files ---"
rm "$TEST_DIR/optin/CompileTimeDI/Container.Generated.tt"
build_consumer optin
[ ! -e "$TEST_DIR/optin/CompileTimeDI/Container.Generated.tt" ]
cmp "$TEST_DIR/registrations" "$TEST_DIR/optin/CompileTimeDI/CompileTimeRegistrations.ttinclude"
cmp "$TEST_DIR/generated" "$TEST_DIR/optin/CompileTimeDI/Container.Generated.cs"

echo "=== All E2E tests PASSED ==="
