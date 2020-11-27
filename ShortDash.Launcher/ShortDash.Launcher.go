package main

import (
	"flag"
	"log"

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
	// Get command line parameters
	binaryPathParameter := flag.String("b", "", "Binary Path")
	configPathParameter := flag.String("c", "", "Config Path")
	showConsoleParameter := flag.Bool("s", false, "Show Console")
	flag.Parse()

	environmentPaths := paths.New(*binaryPathParameter, *configPathParameter)

	log.Printf("Binary Path: %s", environmentPaths.BinaryPath)
	log.Printf("Config Path: %s", environmentPaths.ConfigPath)

	consoleVisible := *showConsoleParameter
	console.SetConsoleVisibility(consoleVisible)

	systray.SetTemplateIcon(icon.Data, icon.Data)

	// Launch the ShortDash process
	proc := launcher.New(environmentPaths.BinaryFileName, environmentPaths.BinaryPath, environmentPaths.ConfigPath)
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
			open.Start(proc.ProcessURL)
		case <-settingsMenuItem.ClickedCh:
			open.Start(environmentPaths.ConfigPath)
		case <-exitMenuItem.ClickedCh:
			proc.Kill()
		case <-cmdExited:
			systray.Quit()
			return
		}
	}
}
