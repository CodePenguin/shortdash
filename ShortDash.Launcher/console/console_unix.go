//+build linux darwin

package console

// CanShowConsole returns true if the OS supports console management
func CanShowConsole() bool {
	return false
}

// SetConsoleVisibility is not supported on OSX or Linux
func SetConsoleVisibility(showConsole bool) {
	// Not supported on this platform
}
