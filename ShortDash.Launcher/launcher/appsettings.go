package launcher

import (
	"encoding/json"
	"io/ioutil"
	"os"
	"path"
)

type appSettings struct {
	Urls string `json:"Urls"`
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

func initializeAppSettings(binaryPath string, configPath string) {
	if (configPath == "" || configPath == binaryPath) {
		return
	}
	// Create config path if it does not exist
	if _, err := os.Stat(configPath); err != nil {
		err := os.Mkdir(configPath, 0700)
		if err != nil {
			panic(err)
		}
	}
	// Copy appsettings.json to config path so it can be manually edited if it doesn't exist
	binaryPathAppSettingsFileName := path.Join(binaryPath, "appsettings.json")
	configPathAppSettingsFileName := path.Join(configPath, "appsettings.json")
	if _, err := os.Stat(configPathAppSettingsFileName); err != nil {
		copyFile(binaryPathAppSettingsFileName, configPathAppSettingsFileName)
	}
}

func readAppSettings(configPath string) appSettings {
	jsonFile, err := os.Open(path.Join(configPath, "appsettings.json"))
	if err != nil {
		return appSettings{}
	}
	defer jsonFile.Close()
	fileBytes, _ := ioutil.ReadAll(jsonFile)
	var settings appSettings
	json.Unmarshal(fileBytes, &settings)
	return settings
}
