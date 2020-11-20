package main

import (
	"os"

	"ShortDash.Launcher/icon"
	"ShortDash.Launcher/launcher"
	"github.com/getlantern/systray"
	"github.com/skratchdot/open-golang/open"
)

func main() {
	onExit := func() {
		// Clean up here
	}

	systray.Run(onReady, onExit)
}

func fileExists(filename string) bool {
	info, err := os.Stat(filename)
	if os.IsNotExist(err) {
		return false
	}
	return !info.IsDir()
}

func onReady() {
	proc := launcher.New("./")

	cmdExited := make(chan error, 1)
	go func() {
		cmdExited <- proc.Wait()
	}()

	systray.SetTemplateIcon(icon.Data, icon.Data)
	systray.SetTooltip("ShortDash Target")
	launchMenuItem := systray.AddMenuItem("Open", "Open")
	systray.AddSeparator()
	showMenuitem := systray.AddMenuItem("Show Process", "Show Process")
	systray.AddSeparator()
	exitMenuItem := systray.AddMenuItem("Exit", "Exit")

	for {
		select {
		case <-showMenuitem.ClickedCh:
			open.Run(proc.ProcessURL)
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
