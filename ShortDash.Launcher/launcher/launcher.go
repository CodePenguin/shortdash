package launcher

import (
	"fmt"
	"log"
	"os"
	"os/exec"
	"runtime"
	"strings"
)

// Launcher manages the ShortDash process
type Launcher struct {
	basePath       string
	binaryFileName string
	cmd            *exec.Cmd
	IsServer       bool
	showConsole    bool
	ProcessURL     string
}

// New ShortDash Process Launcher
func New(basePath string) Launcher {
	launcher := Launcher{basePath: basePath}
	launcher.binaryFileName = findBinaryFileName(basePath)
	launcher.ProcessURL = getProcessURL(launcher.binaryFileName, basePath)
	launcher.IsServer = strings.Contains(launcher.binaryFileName, "Server")
	return launcher
}

// Start the ShortDash process
func (l *Launcher) Start() error {
	log.Printf("Starting %s...", l.binaryFileName)
	fileName := l.binaryFileName
	args := ""
	cmd := exec.Command(fileName, args)
	cmd.Dir = l.basePath
	cmd.Stdout = os.Stdout
	cmd.Stderr = os.Stderr
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

// Checks the file system to determine what binary should be executed
func findBinaryFileName(basePath string) string {
	if runtime.GOOS == "windows" {
		const ServerExecutable = "ShortDash.Server.exe"
		const TargetExecutable = "ShortDash.Target.exe"
		if _, err := os.Stat(ServerExecutable); err == nil {
			return ServerExecutable
		} else if _, err := os.Stat(TargetExecutable); err == nil {
			return TargetExecutable
		}
	} else {
		const ServerApplication = "ShortDash.Server"
		const TargetApplication = "ShortDash.Target"
		if _, err := os.Stat(ServerApplication); err == nil {
			return ServerApplication
		} else if _, err := os.Stat(TargetApplication); err == nil {
			return TargetApplication
		}
	}
	panic("ShortDash binary not found.")
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
