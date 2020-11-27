package launcher

import (
	"fmt"
	"log"
	"os"
	"os/exec"
	"path"
	"strings"
)

// Launcher manages the ShortDash process
type Launcher struct {
	basePath       string
	binaryFileName string
	configPath     string
	cmd            *exec.Cmd
	IsServer       bool
	showConsole    bool
	ProcessURL     string
}

// New ShortDash Process Launcher
func New(binaryFileName string, binaryPath string, configPath string) Launcher {
	initializeAppSettings(binaryPath, configPath)
	launcher := Launcher{basePath: binaryPath}
	launcher.binaryFileName = binaryFileName
	launcher.configPath = configPath
	launcher.ProcessURL = getProcessURL(launcher.binaryFileName, launcher.configPath)
	launcher.IsServer = strings.Contains(launcher.binaryFileName, "Server")
	return launcher
}

// Start the ShortDash process
func (l *Launcher) Start() error {
	log.Printf("Starting %s...", l.binaryFileName)
	args := ""
	cmd := exec.Command(path.Join(l.basePath, l.binaryFileName), args)
	cmd.Dir = l.basePath
	cmd.Stdout = os.Stdout
	cmd.Stderr = os.Stderr
	if l.basePath != l.configPath {
		cmd.Env = append(os.Environ(), fmt.Sprintf("SHORTDASH_CONFIG_PATH=%s", l.configPath))
	}
	l.cmd = cmd
	return cmd.Start()
}

// Kill the ShortDash Process
func (l *Launcher) Kill() {
	l.cmd.Process.Kill()
}

// Wait for the ShortDash proces to finish
func (l *Launcher) Wait() error {
	log.Printf("%s is now running...", l.binaryFileName)
	return l.cmd.Wait()
}

// Retrieves the process URL based on configuration or defaults
func getProcessURL(binaryFileName string, configPath string) string {
	isServer := strings.Contains(binaryFileName, "Server")
	port := 5100
	if !isServer {
		port = 5101
	}
	settings := readAppSettings(configPath)
	url := strings.Split(settings.Urls, ";")[0]
	if url != "" {
		return strings.ReplaceAll(url, "*", "localhost")
	}
	return fmt.Sprintf("http://localhost:%d", port)
}
