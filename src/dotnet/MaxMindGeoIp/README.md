# MaxMind GeoIP database

Download the dataset from [MAXMIND's website](https://dev.maxmind.com/geoip/geolite2-free-geolocation-data). The data is already in a format
acceptable by this tool. Just copy the files to your system and set the options

```csharp

var options = new MaxMindGeoIpOptions(
	"c:/geoip/GeoLite2-City-Blocks-IPv4.csv",
	"c:/geoip/GeoLite2-City-Locations-{0}.csv"
);

services.AddMaxMindGeoIp(options);


```

By adding `{0}` to the locations path you can utilize the language parameter in the `GetLocation` method.

Please review the license before utilizing this library that can be found [here](https://www.maxmind.com/en/geolite2/eula).