# MaxMind GeoIP database

Download the dataset from [MAXMIND's website](https://dev.maxmind.com/geoip/geolite2-free-geolocation-data). The data is already in a format
acceptable by this tool. Just copy the files to your system and set the options

```csharp

builder.Services.Configure<MaxMindLocationOptions>(options =>
{
    options.IPv4BlocksPath = @"c:/geoip/GeoLite2-City-Blocks-IPv4.csv";
    options.IPv6BlocksPath = @"c:/geoip/GeoLite2-City-Blocks-IPv6.csv";
    options.LocationsPath = @"c:/geoip/GeoLite2-City-Locations-{0}.csv";
});

services.AddMaxMindLocation(options);


```

By adding `{0}` to the locations path you can utilize the language parameter in the `GetLocation` method. The default language if none is provided is `en`.

Please review the license before utilizing this library that can be found [here](https://www.maxmind.com/en/geolite2/eula).