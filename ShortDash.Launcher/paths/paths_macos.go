//+build darwin

package paths

import (
	"os/user"
	"path"
	"path/filepath"
	"strings"
)

func setBinaryPath(paths *Paths) {
	// Detect if running in application bundle
	if !strings.HasSuffix(paths.ExecutablePath, "Contents/MacOS") {
		return
	}
	// Set new binary path
	paths.BinaryPath = filepath.Join(paths.ExecutablePath, "../Resources")
}

func setConfigPath(paths *Paths) {
	// Get user home directory
	usr, err := user.Current()
	if err != nil {
		panic(err)
	}
	paths.ConfigPath = path.Join(usr.HomeDir, ".local/share", paths.baseFileName)
}
