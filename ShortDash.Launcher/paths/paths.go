package paths

import (
	"os"
	"path"
	"path/filepath"
	"runtime"
	"strings"
)

// Paths stores the calculated paths for the environment
type Paths struct {
	baseFileName   string
	BinaryFileName string
	BinaryPath     string
	ConfigPath     string
	ExecutablePath string
}

// New environment paths structure
func New(binaryPath string, configPath string) Paths {
	paths := Paths{BinaryPath: binaryPath, ConfigPath: configPath, ExecutablePath: getExecutablePath()}

	// Always execute with the working directory as the executable path
	os.Chdir(paths.ExecutablePath)

	// Determine binary path
	if paths.BinaryPath == "" {
		setBinaryPath(&paths)
	}
	paths.BinaryPath, _ = filepath.Abs(paths.BinaryPath)
	paths.BinaryFileName = findBinaryFileName(paths.BinaryPath)
	paths.baseFileName = strings.TrimSuffix(paths.BinaryFileName, ".exe")

	// Determine config path
	if paths.ConfigPath == "" {
		setConfigPath(&paths)
	}
	paths.ConfigPath, _ = filepath.Abs(paths.ConfigPath)

	return paths
}

// Checks the file system to determine what binary should be executed
func findBinaryFileName(basePath string) string {
	if runtime.GOOS == "windows" {
		const ServerExecutable = "ShortDash.Server.exe"
		const TargetExecutable = "ShortDash.Target.exe"
		if _, err := os.Stat(path.Join(basePath, ServerExecutable)); err == nil {
			return ServerExecutable
		} else if _, err := os.Stat(path.Join(basePath, TargetExecutable)); err == nil {
			return TargetExecutable
		}
	} else {
		const ServerApplication = "ShortDash.Server"
		const TargetApplication = "ShortDash.Target"
		if _, err := os.Stat(path.Join(basePath, ServerApplication)); err == nil {
			return ServerApplication
		} else if _, err := os.Stat(path.Join(basePath, TargetApplication)); err == nil {
			return TargetApplication
		}
	}
	panic("ShortDash binary not found.")
}

// Returns the path where the launcher is executing from
func getExecutablePath() string {
	executable, _ := os.Executable()
	execuablePath, _ := filepath.Abs(filepath.Dir(executable))
	return execuablePath
}
