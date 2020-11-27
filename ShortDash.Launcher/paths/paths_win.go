//+build windows

package paths

import (
	"os"
	"path"
)

func setBinaryPath(paths *Paths) {
	// Use the default values
}

func setConfigPath(paths *Paths) {
	paths.ConfigPath = os.ExpandEnv(path.Join("${LocalAppData}", paths.baseFileName))
}
