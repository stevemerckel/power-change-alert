# power-change-alert

## Summary

This is a Windows Service project that will send an email alert as the host powers on/off and drops in/out of battery power.

The intent is to have a notification when power is lost on a known electricl circuit.  It assumed that the host is a laptop or a system that Windows is aware of battery management, and that the on-prem networking will still have power (via UPS) when the host loses wall-power.

## Technologies

* Visual Studio 2019
* C# Windows Service
* Gmail Integration