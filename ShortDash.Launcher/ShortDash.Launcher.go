package main

import (
	"log"
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
	systray.SetTemplateIcon(icon.Data, icon.Data)
	systray.SetTooltip("ShortDash Target")
	launchMenuItem := systray.AddMenuItem("Open", "Open")
	systray.AddSeparator()
	showMenuitem := systray.AddMenuItem("Show Process", "Show Process")
	systray.AddSeparator()
	exitMenuItem := systray.AddMenuItem("Exit", "Exit")

	proc := launcher.New("./")
	err := proc.Start()
	if err != nil {
		log.Fatal(err)
	}

	cmdExited := make(chan error, 1)
	go func() {
		cmdExited <- proc.Wait()
	}()

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
