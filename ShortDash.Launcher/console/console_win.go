//+build windows

package console

import (
	"syscall"
)

// CanShowConsole returns true if the OS supports console management
func CanShowConsole() bool {
	return true
}

// SetConsoleVisibility toggles the display of the console for the application
func SetConsoleVisibility(showConsole bool) {
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
