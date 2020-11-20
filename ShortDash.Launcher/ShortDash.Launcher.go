package main

import (
	"log"
	"os"
	"syscall"

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
	consoleVisible := false
	setConsoleVisibility(false)

	systray.SetTemplateIcon(icon.Data, icon.Data)

	proc := launcher.New("./")
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
	showLogMenuitem := systray.AddMenuItem("Show Log", "Show Log")
	systray.AddSeparator()
	exitMenuItem := systray.AddMenuItem("Exit", "Exit")

	cmdExited := make(chan error, 1)
	go func() {
		cmdExited <- proc.Wait()
	}()

	for {
		select {
		case <-showLogMenuitem.ClickedCh:
			consoleVisible = !consoleVisible
			setConsoleVisibility(consoleVisible)
			if consoleVisible {
				showLogMenuitem.SetTitle("Hide Log")
			} else {
				showLogMenuitem.SetTitle("Show Log")
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

func setConsoleVisibility(showConsole bool) {
	var getConsoleWindow = syscall.NewLazyDLL("kernel32.dll").NewProc("GetConsoleWindow")
	var showWindowAsync = syscall.NewLazyDLL("user32.dll").NewProc("ShowWindowAsync")
	if getConsoleWindow.Find() != nil || showWindowAsync.Find() != nil {
		return
	}
	hwnd, _, _ := getConsoleWindow.Call()
	if showConsole {
		var ShowWindow uintptr = 5
		showWindowAsync.Call(hwnd, ShowWindow)
	} else {
		var HideWindow uintptr = 0
		showWindowAsync.Call(hwnd, HideWindow)
	}
}
