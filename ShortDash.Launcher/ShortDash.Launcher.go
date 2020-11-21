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
	workingDir, _ := os.Getwd()
	binaryPath := flag.String("b", workingDir, "Binary Path")
	configPath := flag.String("c", "", "Config Path")
	flag.Parse()

	if *configPath == "" {
		configPath = binaryPath
	}

	absoluteBinaryPath, _ := filepath.Abs(*binaryPath)
	absoluteConfigPath, _ := filepath.Abs(*configPath)

	consoleVisible := false
	console.SetConsoleVisibility(false)

	systray.SetTemplateIcon(icon.Data, icon.Data)

	proc := launcher.New(absoluteBinaryPath, absoluteConfigPath)
	err := proc.Start()
	if err != nil {
		log.Fatal(err)
	}

	processTitle := "ShortDash Server"
	if !proc.IsServer {
		processTitle = "ShortDash Target"
	}

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
