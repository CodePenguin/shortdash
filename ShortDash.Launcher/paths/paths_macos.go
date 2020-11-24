//+build darwin

package paths

import (
	"io/ioutil"
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
	// Create config path if it does not exist
	if _, err := os.Stat(paths.ConfigPath); err != nil {
		err := os.Mkdir(paths.ConfigPath, 0700)
		if err != nil {
			panic(err)
		}
	}
	// Copy appsettings.json to config path so it can be manually edited if it doesn't exist
	binaryPathAppSettingsFileName := path.Join(paths.BinaryPath, "appsettings.json")
	configPathAppSettingsFileName := path.Join(paths.ConfigPath, "appsettings.json")
	if _, err := os.Stat(configPathAppSettingsFileName); err != nil {
		copyFile(binaryPathAppSettingsFileName, configPathAppSettingsFileName)
	}
}

func copyFile(sourceFileName string, destFileName string) {
	input, err := ioutil.ReadFile(sourceFileName)
	if err != nil {
		panic(err)
	}
	err = ioutil.WriteFile(destFileName, input, 0644)
	if err != nil {
		panic(err)
	}
}
