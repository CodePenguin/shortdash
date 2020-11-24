package main

import (
	"flag"
	"log"
	"os"
	"path"
	"path/filepath"

	"ShortDash.Launcher/console"
	"ShortDash.Launcher/icon"
	"ShortDash.Launcher/launcher"
	"ShortDash.Launcher/paths"
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
	defaultPaths := paths.New()
	os.Chdir(defaultPaths.ExecutablePath)

	// Get command line parameters
	binaryPathParameter := flag.String("b", defaultPaths.BinaryPath, "Binary Path")
	configPathParameter := flag.String("c", defaultPaths.ConfigPath, "Config Path")
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
	err := proc.Start()
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
	if !console.CanShowConsole() {
		showLogMenuItem.Hide()
	}
	settingsMenuItem := systray.AddMenuItem("Settings", "Settings")
	systray.AddSeparator()
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
		case <-settingsMenuItem.ClickedCh:
			open.Run(path.Join(configPath, "appsettings.json"))
		case <-exitMenuItem.ClickedCh:
			proc.Kill()
		case <-cmdExited:
			systray.Quit()
			return
		}
	}
}
