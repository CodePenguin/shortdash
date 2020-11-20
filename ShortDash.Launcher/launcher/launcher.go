package launcher

import (
	"log"
	"os"
	"os/exec"
)

// Launcher manages the ShortDash process
type Launcher struct {
	basePath       string
	binaryFileName string
	cmd            *exec.Cmd
	showConsole    bool
	ProcessURL     string
}

// New ShortDash Process Launcher
func New(basePath string) Launcher {
	launcher := Launcher{basePath: basePath}
	launcher.binaryFileName = findBinaryFileName(basePath)
	launcher.ProcessURL = processURLFromBinary(launcher.binaryFileName)
	return launcher
}

// Start the ShortDash process
func (l *Launcher) Start() error {
	log.Printf("Starting %s...", l.binaryFileName)
	cmd := exec.Command(l.binaryFileName)
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
	log.Printf("Waiting for %s to complete...", l.binaryFileName)
	return l.cmd.Wait()
}

func findBinaryFileName(basePath string) string {
	const ServerApplication = "ShortDash.Server"
	const ServerExecutable = "ShortDash.Server.exe"
	const ServerLibrary = "ShortDash.Server.dll"
	const TargetApplication = "ShortDash.Target"
	const TargetLibrary = "ShortDash.Target.dll"
	const TargetExecutable = "ShortDash.Target.exe"
	if _, err := os.Stat(ServerApplication); err == nil {
		return ServerApplication
	} else if _, err := os.Stat(ServerExecutable); err == nil {
		return ServerExecutable
	} else if _, err := os.Stat(ServerLibrary); err == nil {
		return ServerLibrary
	} else if _, err := os.Stat(TargetApplication); err == nil {
		return TargetApplication
	} else if _, err := os.Stat(TargetLibrary); err == nil {
		return TargetLibrary
	} else if _, err := os.Stat(TargetExecutable); err == nil {
		return TargetExecutable
	}
	panic("ShortDash binary not found.")
}

func processURLFromBinary(binaryFileName string) string {
	return "http://localhost:5101"
}
