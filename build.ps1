param ($m='CI', $v='', $b='')
$version = (Get-Content '.\version.txt' -Raw).Trim()
$mode = $m.ToUpper()
$version_suffix = $v
$build_number = $b

If ($mode -eq 'RELEASE')
{
    $release_prefix = 'bin/ShortDash'
}
ElseIf ($mode -eq 'CI')
{
    $release_prefix = 'bin/ShortDash-CI'
    $version_suffix = 'CI'
}
Else
{
    Write-Host "Invalid Mode: $mode"
    exit 1
}

If ($build_number -ne "")
{
    $version = "$version.$build_number"
}
If ($version_suffix -ne "")
{
    $version = "$version-$version_suffix"
}

Write-Host "Build Mode: $mode"
Write-Host "Version: $version"

$common_args = "-v m -c Release /p:Version=$version --framework net5.0"
$platform_args = "-p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true --self-contained true"

# Clean bin folder
Remove-Item -Path bin -Recurse -ErrorAction SilentlyContinue

# Restore and build solution
Invoke-Expression "dotnet restore ShortDash.Windows.sln"
Invoke-Expression "dotnet build ShortDash.Windows.sln -c Release --no-restore"

# Run Unit Tests
Invoke-Expression "dotnet test ShortDash.Windows.sln -c Release --no-restore -v n"

# Build Windows Launcher
Write-Host 'Building Windows binaries'
$rid = 'win-x64'
$release_name = "$release_prefix-$rid"
$plugin_path = "$release_name/ShortDash.Server/plugins"
Invoke-Expression "dotnet publish ShortDash.Launcher.Windows $common_args-windows -r $rid $platform_args /p:PublishTrimmed=true -o ""$release_name/ShortDash.Launcher"""

if ($mode -eq 'RELEASE')
{
    Invoke-Expression "dotnet publish ShortDash.Server $common_args -r $rid $platform_args -o ""$release_name/ShortDash.Server"""
    Invoke-Expression "dotnet publish ShortDash.Target $common_args -r $rid $platform_args -o ""$release_name/ShortDash.Target"""

    # Copy Launcher
    Copy-Item -Path "$release_name/ShortDash.Launcher/*" -Destination "$release_name/ShortDash.Server" -Recurse
    Copy-Item -Path "$release_name/ShortDash.Launcher/*" -Destination "$release_name/ShortDash.Target" -Recurse

    # Build Plugins
    Invoke-Expression "dotnet publish ShortDash.Plugins.Core.Common $common_args -r $rid -o ""$plugin_path/ShortDash.Plugins.Core.Common"""
    Invoke-Expression "dotnet publish ShortDash.Plugins.Core.Windows $common_args -r $rid -o ""$plugin_path/ShortDash.Plugins.Core.Windows"""
    Copy-Item -Path "$plugin_path" -Destination "$release_name/ShortDash.Target/plugins" -Recurse

    # Package binaries
    Compress-Archive -Path "$release_name\ShortDash.Server","$release_name\ShortDash.Target" -Destination "$release_name.zip"

    # Generate installer
    iscc ShortDash.iss /DBuildVersion=$version
}
