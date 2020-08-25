# Power Change Alerter

## Summary

This is a Windows Service project that will send an email alert as the host powers on/off and drops in/out of battery power.  Additional alerts can be configured for other cases, such as large changes in system date/time.

The intent is to have a notification when power is lost on a known electricl circuit.  It assumed that the host is a laptop or a system that Windows is aware of battery management, and that the on-prem networking will still have power (e.g. UPS systems) when the premises loses wall-power.

One possible use-case for this is to provide a "heads-up" notice to home owners if there are items depending on constant electricity, such as freezers, refrigerators, garage door openers, and various IoT devices.

## Initial Need

Having UPS systems in the house to keep things alive during brown- or blackouts is great.  However, excessive outtages that happen while away from the house can lead to unexpected consequences.  For example, several hundred dollars worth of food in a refrigerator can become spoiled while you are away for a weekend.

The immediate need was to write an application to send notices to my cell phone.  I happened to have a spare laptop laying around, so it made sense to rely on its internal message pump as it changed from wall power to battery (and vice-versa).  Combine that with a free email address service and how a cell provider can translate SMTP messages to cellular text messages, and I had a working concept.  The first implementation was done as a Windows Service solely from my own familiarity, and it was a quick means to an end.

## Technologies

* C# Windows Service
* .NET Framework and .NET Core
* SMTP Communication
* PowerShell
* MSI Installer

## NuGet Packages

* Serilog, plus several sinks
* Newtonsoft JSON
* Microsoft Extension packages
* Trinet.Core.IO.Ntfs

## Development Environment Setup

Ensure the following is installed:

* .NET Framework version 4.8
* .NET Core version 3.1
* PowerShell (at least version 5)
* Visual Studio (Community Edition is fine)
* Windows Installer XML (aka _WiX_)
* WiX Toolset extension for Visual Studio

> Aside from _WiX_, the installations listed above are available from the Visual Studio Installer app.

Once the above packages have been installed, load the `PowerChangeAlerter.sln` file in Visual Studio and do a complete rebuild of the solution.  This will also cause any NuGet packages to be pulled down.

## Before Running or Debugging

If this is your first time downloading the repo, then you will need to do an extra step.  Browse to the `$/PowerChangeAlerter.Common` directory, and you will find a file called `settings.runtime.TEMPLATE`.  Make a copy of this JSON file, and name it `settings.runtime`.  Doing this will also resolve a Visual Studio error for missing a file in the CSPROJ file's manifest.  You would then need to file in the appropriate fields in the JSON file.

> If you do not do the above step, you will get an exception when you debug or run the application.  This also applies for generating a MSI file.

## Application Overview

The solution file has references to a few projects:

* PowerChangeAlerter - Primary starting point (i.e. startup project)
* PowerChangeAlerter.Common - Shared classes and functionality, the guts of the application
* PowerChangeAlerter.Install - Creates MSI installer files
* PowerChangeAlerter.Sandbox - Just a dummy console app used for ad-hoc testing of functionality

The primary worker (`PowerChangeAlerter.csproj`) is designed to run as a Windows Service.  Since those cannot be natively debugged, there is an extra commandline option for running the project from Visual Studio or a command prompt -- just add the switch `-cli` to the project's properties window in the _Debug --> Command line arguments_ section.  Or just open a command prompt and run this:

```
PowerChangeAlerter.exe -cli
```

This will dump out some generalized information and ensure that the necessary event listeners are working.

(Re-)Building the solution will cause _almost_ all projects to compile.  There is one project that is excluded from the normal (re-)build process -- `PowerChangeAlerter.Install`.  This was done to keep the normal build times as short as possible, by omitting the MSI installer file creation.  See the next section for details.

## Logging

Serilog is used for capturing and routing log messages.  By default, it is setup to use a rolling log file system.  Additional features can be bolted into the `settings.serilog` file.

Where the log files are written depends on how the application is started:

* **Visual Studio Debugging or Command-line -** Look for a `logs` directory under the runtime's EXE location, usually in the `bin` directory
* **Registered as a Windows Service -** `C:\ProgramData\Power Change Alerter\logs`

## Create Installer

Right-clicking on the `PowerChangeAlerter.Install` project in Visual Studio's _Solution Explorer_ will open options to rebuild that project and generate a MSI file.  For convenience, the generated MSI file is copied to the root of the repo, and will remove any other MSI files found there to eliminate confusion.  This happens as a post-build step that runs the following PowerShell script: `copy-msi-with-rename.ps1`.

## Installing the Application

Run the generated MSI file, and click through the few prompts.  If an existing copy of the service exists already, the installer will stop and remove the existing service, then replace it with the installer's version.

## Running the Windows Service

The host machine will need the following:

* Windows 7 or higher (64-bit only)
* .NET Framework version 4.8

The service itself is setup to run with the following options:

* Start automatically
* Run as Local System
* Desktop interactive is turned off

Starting the service will dump several lines of initialization information.  If you get any failures during startup, check the log files written to the location specified in the _Logging_ section above.  After the service has been running for a while, you will periodically see some "uptime minutes" messages being written to the log file.

## Development Suggestions

Use _DebugView_ to listen for additional `Debug` level messages that are scattered through the application.  If you have Visual Studio running at the same time, then look at its _Output_ window for the debug messages -- they are not redirected back to the debug stream when caught there.

## Future Ideas

In no particular order...

* Improved parameterization of Serilog options with `Serilog.Settings.Configuration` and `Serilog.Sinks.Map`
* Dependency injection with configuration file for IoC bindings
* Expose REST API from (random or explicit) TCP port for querying state, returning metrics, and more
* Create base class for declaring/discovering new background worker functionality
* Lightweight telemetry: CPU + RAM utilization
* Track internet uptime, send outtage notice after service is restored
* Unit tests, mocks
* Look at Arduino, with a battery, and modify as needed to get a POC working on that platform