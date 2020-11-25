# Environment setup
set -e
version=$(< version.txt)
version=${version//[$'\t\r\n ']}
mode="CI"
version_suffix=""
build_number=""
while getopts b:m:v: flag
do
    case "${flag}" in
        b) build_number=${OPTARG};;
        m) mode=${OPTARG};;
        v) version_suffix=${OPTARG};;
    esac
done
mode=$(tr '[:lower:]' '[:upper:]' <<< "$mode")
if [ "$mode" = "RELEASE" ]; then
    release_prefix="bin/ShortDash"
elif [ "$mode" = "CI" ]; then
    release_prefix="bin/ShortDash-CI"
    version_suffix="CI"
else
    echo "Invalid Mode: $mode"
    exit 1
fi
if [ "$build_number" != "" ]; then
    version="$version.$build_number"
fi
if [ "$version_suffix" != "" ]; then
    version="$version-$version_suffix"
fi
if [[ "$OSTYPE" == "darwin"* ]]; then
    platform="osx"
elif [[ "$OSTYPE" == "linux"* ]]; then
    platform="linux"
else
    echo "Unhandled Platform: $OSTYPE"
    exit 1
fi

# Functions
function copyPlugins() {
    mkdir -p "$1/ShortDash.Server/plugins"
    cp -r "$plugin_path/." "$1/ShortDash.Server/plugins"
    mkdir -p "$1/ShortDash.Target/plugins"
    cp -r "$plugin_path/." "$1/ShortDash.Target/plugins"
}

function buildPlatform() {
    rid=$1
    release_name="$release_prefix-$rid"
    echo "Building $rid binaries..."
    dotnet publish ShortDash.Server $common_args -r $rid $platform_args -o "$release_name/ShortDash.Server"
    dotnet publish ShortDash.Target $common_args -r $rid $platform_args -o "$release_name/ShortDash.Target"
    copyPlugins $release_name
}

function buildLauncher() {
    rid=$1
    release_name="$release_prefix-$rid"
    echo "Building $rid Launcher binary..."
    cd ShortDash.Launcher
    go build -o "../$release_name/ShortDash.Launcher"
    cd -
}

function createAppBundle() {
    application_name=$2
    bundle_path="$1/ShortDash $application_name.app"
    mkdir -p "$bundle_path"
    cp -a "assets/MacAppBundle/." "$bundle_path"
    mkdir -p "$bundle_path/Contents/MacOS"
    cp -a "$release_name/ShortDash.Launcher" "$bundle_path/Contents/MacOS"
    cp -a "$release_name/ShortDash.$application_name/." "$bundle_path/Contents/Resources"
    sed -i "" "s/{ApplicationName}/$application_name/" "$bundle_path/Contents/Info.plist"
}

# Workflows
echo "Build Mode: $mode"
echo "Version: $version"
echo "Platform: $platform"

common_args="-v m -c Release /p:Version=$version --framework net5.0"
platform_args="-p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true --self-contained true"

# Clean bin folder
rm -rf bin

# Restore and build solution
dotnet restore ShortDash.sln
dotnet build ShortDash.sln -c Release --no-restore

# Run unit tests
dotnet test ShortDash.sln  -c Release --no-restore -v n

# Build Launcher
if [ $platform == "linux" ]; then
    buildLauncher linux-x64
elif [ $platform == "osx" ]; then
    buildLauncher osx-x64
fi

if [ "$mode" != "RELEASE" ]; then
    exit 0
fi

# Build Plugins
echo "Building plugins..."
plugin_path="bin/plugins"
dotnet publish ShortDash.Plugins.Core.Common $common_args -o "$plugin_path/ShortDash.Plugins.Core.Common"
dotnet publish ShortDash.Plugins.Core.Windows $common_args -o "$plugin_path/ShortDash.Plugins.Core.Windows"

if [ $platform == "linux" ]; then
    buildPlatform linux-x64
    buildPlatform linux-arm64

    echo "Packaging..."
    tar czf "$release_prefix-linux-arm64.tar.gz" -C "$release_prefix-linux-arm64" .
    tar czf "$release_prefix-linux-x64.tar.gz" -C "$release_prefix-linux-x64" .
fi

if [ $platform == "osx" ]; then
    release_name="$release_prefix-osx-x64"
    buildPlatform osx-x64
    echo "Bundling..."
    dmg_template_path="$release_name-dmg"
    createAppBundle $dmg_template_path Server
    createAppBundle $dmg_template_path Target
    ln -s /Applications "$dmg_template_path/Applications"
    echo "Packaging..."
    hdiutil create -fs HFS+ -srcfolder $dmg_template_path -volname "ShortDash" "$release_prefix-osx-x64.dmg"
    tar czf "$release_prefix-osx-x64.tar.gz" -C "$release_prefix-osx-x64" .
fi
