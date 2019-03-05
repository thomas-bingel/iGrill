# iGrill
C# library to connect to the iGrill using UWP (Universal Windows Platform)

This library can be used to connect to the iGrill Mini, v2 and v3. 
Using a standard Windows or Windows IoT Core on a Raspberry Pi.

[![Build Status](https://dev.azure.com/tbingel/iGrill/_apis/build/status/thomas-bingel.iGrill?branchName=master)](https://dev.azure.com/tbingel/iGrill/_build/latest?definitionId=1&branchName=master)
[![nuget](https://img.shields.io/nuget/v/IGrill.Core.svg)](https://www.nuget.org/packages/IGrill.Core/)
[![GitHub](https://img.shields.io/github/release-pre/thomas-bingel/iGrill.svg)](https://github.com/thomas-bingel/iGrill)

Usage of the library
``` C# 
igrill = IGrill.Core.IGrillFactory.FromDeviceInformation(device);
igrill.OnTemperatureChanged += (object sender, TemperatureChangedEventArg args) =>
{
    Debug.WriteLine(String.Format("Probe {0} = {1}°C", args.ProbeIndex, args.Temperature));
};
await igrill.ConnectAsync();
```

## Tasks
- [x] Tested with iGrill Mini
- [x] Tested with iGrill 2
- [ ] Tested with iGrill 3
- [x] Detect which iGrill it is
- [ ] Docu (Add Bluetooth right)

## The App

Work in Progress...

### Tasks
- [x] Using Configuration to persist selected device
- [ ] Using MQTT
- [ ] Configuration (Settings)
- [ ] Multilanguage
- [x] Add Azure Pipeline
- [ ] Starting App in CommandLine (Checkout, Build, Run)

