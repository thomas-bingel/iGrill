# iGrill
C# library to connect to the iGrill using UWP (Universal Windows Platform)

Work in Progress...

This library can be used to connect to the iGrill Mini, v2 and v3. 
Using a standard Windows or Windows IoT Core on a Raspberry Pi.

![iGrill Image](https://product-images.weber.com/accessory-images/7204B1_REV.png)

Usage of the library
``` C# 
var iGrill = new IGrillLibrary.IGrill(IGrillVersion.IGrill2);
    igrill.OnTemperatureChanged += (object sender, TemperatureChangedEventArg args) =>
    {
        Debug.WriteLine(String.Format("Probe {0} = {1}°C", args.ProbeIndex, args.Temperature));
    };
    await igrill.ConnectAsync("[DEVICE_ID]");
```

## The App

Work in Progress...


## Tasks
- [x] Using Configuration to persist selected device
- [ ] Using MQTT
- [ ] Configuration (Settings)
- [ ] Multilanguage
- [ ] Add Azure Pipeline

Starting App in CommandLine (Checkout, Build, Run)

