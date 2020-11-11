version="0.0.1"

set -e
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
mode=${mode^^}
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

echo "Build Mode: $mode"
echo "Version: $version"

common_args="-v m -c Release  /p:Version=$version --framework netcoreapp3.1"
platform_args="-p:PublishSingleFile=true --self-contained true"

# Clean bin folder
rm -rf bin

# Build Cross-Platform
echo "Building Cross-Platform binaries"
release_name="$release_prefix-cross-platform"
plugin_path="$release_name/ShortDash.Server/plugins"
dotnet publish ShortDash.Server $common_args -o "$release_name/ShortDash.Server"
dotnet publish ShortDash.Target $common_args -o "$release_name/ShortDash.Target"
# Build Plugins
dotnet build ShortDash.Plugins.Core.Common $common_args -o "$plugin_path/ShortDash.Plugins.Core.Common"
dotnet build ShortDash.Plugins.Core.Windows $common_args -o "$plugin_path/ShortDash.Plugins.Core.Windows"
cp -r "$plugin_path" "$release_name/ShortDash.Target/plugins"

if [ "$mode" = "RELEASE" ]; then

    function buildPlatform() {
        rid=$1
        echo "Building $rid binaries"
        release_name="$release_prefix-$rid"
        dotnet publish ShortDash.Server $common_args -r $rid $platform_args -o "$release_name/ShortDash.Server"
        dotnet publish ShortDash.Target $common_args -r $rid $platform_args -o "$release_name/ShortDash.Target"
        if [ "$rid" = "win-x64" ]; then
            cp -r "$plugin_path" "$release_name/ShortDash.Server/plugins"
            cp -r "$plugin_path" "$release_name/ShortDash.Target/plugins"
        else 
            mkdir -p "$release_name/ShortDash.Server/plugins/ShortDash.Plugins.Core.Common"
            cp -r "$plugin_path/ShortDash.Plugins.Core.Common" "$release_name/ShortDash.Server/plugins/ShortDash.Plugins.Core.Common"
            mkdir -p "$release_name/ShortDash.Target/plugins/ShortDash.Plugins.Core.Common"
            cp -r "$plugin_path/ShortDash.Plugins.Core.Common" "$release_name/ShortDash.Target/plugins/ShortDash.Plugins.Core.Common"
        fi
    }

    buildPlatform linux-x64
    buildPlatform linux-arm64
    buildPlatform osx-x64
    buildPlatform win-x64
fi