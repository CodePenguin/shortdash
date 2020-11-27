//+build darwin

package paths

import (
	"os"
	"os/user"
	"path"
	"path/filepath"
	"strings"
)

func setDefaultPaths(paths *Paths) {
	// Detect if running in application bundle
	if !strings.HasSuffix(paths.ExecutablePath, "Contents/MacOS") {
		return
	}
	// Get user home directory
	usr, err := user.Current()
	if err != nil {
		panic(err)
	}
	// Get bundle paths and names
	bundleFilePath := strings.TrimSuffix(paths.ExecutablePath, "Contents/MacOS")
	bundleFileName := filepath.Base(bundleFilePath)
	bundleAppName := strings.TrimSuffix(bundleFileName, ".app")
	// Set new default paths
	paths.BinaryPath = filepath.Join(paths.ExecutablePath, "../Resources")
	paths.ConfigPath = path.Join(usr.HomeDir, "Library/Application Support", bundleAppName)
}