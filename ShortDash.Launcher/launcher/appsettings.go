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
