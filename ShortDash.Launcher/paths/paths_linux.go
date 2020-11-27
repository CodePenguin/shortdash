//+build linux

package paths

import (
	"os/user"
	"path"
)

func setBinaryPath(paths *Paths) {
	// Use the default values
}

func setConfigPath(paths *Paths) {
	// Get user home directory
	usr, err := user.Current()
	if err != nil {
		panic(err)
	}
	paths.ConfigPath = path.Join(usr.HomeDir, ".local/share", paths.baseFileName)
}
