# Running ShortDash as a service

* [Windows](#windows)
* [Linux](#linux)

## Windows

ShortDash Server and Target can be installed as a Windows Service.  The logs will be stored in the Windows Event Viewer.  You must publish the application before attempting to install it as a service.

In the steps below, replace {outputpath} with the path where you want ShortDash installed.

Important Notes:

* Running as a service may prevent certain actions from being performed if they attempt to interact with the user's desktop environment.
* All data is stored and secured in the local application data folder for the user running the server or target application.
* You may want to change the user that the service executes as.  This can be done on the command line or using the Services Manager.

### Server (Windows)

The following steps will install the ShortDash Server as a service that will be executed on system startup:

```bash
set ShortDashInstallPath="{outputPath}"
dotnet publish ShortDash.Server -c Release -o %ShortDashInstallPath%\ShortDashServer
dotnet publish ShortDash.Plugins.Core.Common  -c Release -o %ShortDashInstallPath%\ShortDashServer\plugins\ShortDash.Plugins.Core.Common
sc create ShortDashServer displayname="ShortDash Server" start=delayed-auto binPath="%ShortDashInstallPath%\ShortDashServer\ShortDash.Server.exe"
sc start ShortDashServer
```

### Target (Windows)

Perform the same steps as the Server instructions above but replace "Server" with "Target".

## Linux

The following instructions use the Linux systemd service infrastructure.

### Server (Linux)

The following steps will install the ShortDash Server as a service that will be executed on system startup:

```bash
sudo mkdir /usr/local/ShortDashServer
dotnet publish ShortDash.Server -c Release -o /usr/local/ShortDashServer
dotnet publish ShortDash.Plugins.Core.Common  -c Release -o /usr/local/ShortDashServer/plugins/ShortDash.Plugins.Core.Common
chmod +x /usr/local/ShortDashServer/ShortDash.Server
sudo nano /etc/systemd/system/ShortDashServer.service
sudo systemctl daemon-reload
sudo systemctl enable ShortDashServer
sudo systemctl start ShortDashServer
```

Use the following contents for the `ShortDashServer.service` file:

```ini
[Unit]
Description=ShortDash Server

[Service]
Type=notify
WorkingDirectory=/usr/local/ShortDashServer
ExecStart=/usr/local/ShortDashServer/ShortDash.Server
Environment=DOTNET_ROOT=/root/.dotnet
SyslogIdentifier=ShortDashServer
Restart=always
RestartSec=5

[Install]
WantedBy=multi-user.target
```

Note: You will need to change the `/root/.dotnet` path above based on where dotnet was installed on the local system.

### Target (Linux)

Perform the same steps as the Server instructions above but replace "Server" with "Target".