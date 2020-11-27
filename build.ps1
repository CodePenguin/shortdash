param ($m='CI', $v='', $b='', $r='')
$build_number = $b
$mode = $m.ToUpper()
$version = ''
$version_suffix = $v
$ref = $r

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
If ($ref -ne '')
{
    $split = ($ref -replace 'refs/tags/v').split('-')
    $version = $split[0]
    $version_suffix = If ($split.Length -gt 1) {$split[1]} Else {$version_suffix}
}
If ($version -eq '')
{
    $version = '0.0.0'
}
If ($build_number -ne '')
{
    $version = "$version.$build_number"
}
$full_version = $version
If ($version_suffix -ne '')
{
    $full_version = "$version-$version_suffix"
}

Write-Host "Build Mode: $mode"
Write-Host "Version: $full_version"

$common_args = "-v m -c Release /p:Version=$full_version --framework net5.0"
$platform_args = "-p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true --self-contained true"

# Clean bin folder
Remove-Item -Path bin -Recurse -ErrorAction SilentlyContinue

# Restore and build solution
Invoke-Expression "dotnet restore ShortDash.sln"
Invoke-Expression "dotnet build ShortDash.sln -c Release --no-restore"

# Run unit tests
Invoke-Expression "dotnet test ShortDash.sln -c Release --no-restore -v n"

# Setup output paths
$rid = 'win-x64'
$release_name = "$release_prefix-$rid"
$plugin_path = "$release_name/ShortDash.Server/plugins"

# Build Launcher
Write-Host 'Building Launcher...'
Push-Location ShortDash.Launcher
Invoke-Expression "go build -o ""../$release_name/ShortDash.Launcher.exe"""
Pop-Location

If ($mode -ne "RELEASE") {
    exit 0
}

Write-Host 'Publishing Applications...'
Invoke-Expression "dotnet publish ShortDash.Server $common_args -r $rid $platform_args -o ""$release_name/ShortDash.Server"""
Invoke-Expression "dotnet publish ShortDash.Target $common_args -r $rid $platform_args -o ""$release_name/ShortDash.Target"""

# Build Plugins
Write-Host 'Publishing Plugins...'
Invoke-Expression "dotnet publish ShortDash.Plugins.Core.Common $common_args -r $rid -o ""$plugin_path/ShortDash.Plugins.Core.Common"""
Invoke-Expression "dotnet publish ShortDash.Plugins.Core.Windows $common_args -r $rid -o ""$plugin_path/ShortDash.Plugins.Core.Windows"""
Copy-Item -Path "$plugin_path" -Destination "$release_name/ShortDash.Target/plugins" -Recurse

# Package binaries
Write-Host 'Generating Archives...'
Compress-Archive -Path "$release_name\ShortDash.Launcher.exe","$release_name\ShortDash.Server","$release_name\ShortDash.Target" -Destination "$release_name.zip"

# Generate installer
Write-Host 'Building Installer...'
iscc ShortDash.iss /DBuildVersion=$version

