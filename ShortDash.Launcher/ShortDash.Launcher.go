package main

import (
	"flag"
	"log"
	"os"
	"path/filepath"

	"ShortDash.Launcher/console"
	"ShortDash.Launcher/icon"
	"ShortDash.Launcher/launcher"
	"github.com/getlantern/systray"
	"github.com/skratchdot/open-golang/open"
)

func main() {
	systray.Run(onReady, onExit)
}

func onExit() {
	// Clean up here
}

func onReady() {
	// Always execute with the working directory as the executable path
	executable, err := os.Executable()
	workingDir, _ := filepath.Abs(filepath.Dir(executable))
	os.Chdir(workingDir)

	// Get command line parameters
	binaryPathParameter := flag.String("b", workingDir, "Binary Path")
	configPathParameter := flag.String("c", "", "Config Path")
	showConsoleParameter := flag.Bool("s", false, "Show Console")
	flag.Parse()

	if *configPathParameter == "" {
		configPathParameter = binaryPathParameter
	}

	binaryPath, _ := filepath.Abs(*binaryPathParameter)
	configPath, _ := filepath.Abs(*configPathParameter)

	log.Printf("Binary Path: %s", binaryPath)
	log.Printf("Config Path: %s", configPath)

	consoleVisible := *showConsoleParameter
	console.SetConsoleVisibility(consoleVisible)

	systray.SetTemplateIcon(icon.Data, icon.Data)

	// Launch the ShortDash process
	proc := launcher.New(binaryPath, configPath)
	err = proc.Start()
	if err != nil {
		log.Fatal(err)
	}

	processTitle := "ShortDash Server"
	if !proc.IsServer {
		processTitle = "ShortDash Target"
	}

	// Setup System Tray menus
	systray.SetTooltip(processTitle)
	launchMenuItem := systray.AddMenuItem(processTitle, processTitle)
	systray.AddSeparator()
	showLogMenuItem := systray.AddMenuItem("Show Log", "Show Log")
	if console.CanShowConsole() {
		systray.AddSeparator()
	} else {
		showLogMenuItem.Hide()
	}
	exitMenuItem := systray.AddMenuItem("Exit", "Exit")

	cmdExited := make(chan error, 1)
	go func() {
		cmdExited <- proc.Wait()
	}()

	for {
		select {
		case <-showLogMenuItem.ClickedCh:
			consoleVisible = !consoleVisible
			console.SetConsoleVisibility(consoleVisible)
			if consoleVisible {
				showLogMenuItem.SetTitle("Hide Log")
			} else {
				showLogMenuItem.SetTitle("Show Log")
			}
		case <-launchMenuItem.ClickedCh:
			open.Run(proc.ProcessURL)
		case <-exitMenuItem.ClickedCh:
			proc.Kill()
		case <-cmdExited:
			systray.Quit()
			return
		}
	}
}
