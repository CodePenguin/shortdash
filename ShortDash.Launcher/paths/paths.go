package paths

import (
	"os"
	"path/filepath"
)

type Paths struct {
	BinaryPath string
	ConfigPath string
	ExecutablePath string
}

// New default paths structure
func New() Paths {
	paths := Paths{BinaryPath: "", ConfigPath: "", ExecutablePath: getExecutablePath()}
	setDefaultPaths(&paths)
	return paths
}

func getExecutablePath() string {
	executable, _ := os.Executable()
	execuablePath, _ := filepath.Abs(filepath.Dir(executable))
	return execuablePath
}