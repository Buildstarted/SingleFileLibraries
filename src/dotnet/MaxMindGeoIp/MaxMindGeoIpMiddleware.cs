using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace MaxMindGeoIp;

public class LocationMiddleware
{
    private const string XForwardedForHeader = "X-Forwarded-For";
    private readonly RequestDelegate next;

    public LocationMiddleware(RequestDelegate next)
    {
        this.next = next;
    }

    //check if twofactor completed
    public async Task Invoke(HttpContext context, GeoIPLocation location, MaxMindGeoIpDb maxminddb)
    {
        if (location == null)
        {
            return;
        }

        context.Request.HttpContext.Request.Headers.TryGetValue(XForwardedForHeader, out var ipvalue);

        if (!ipvalue.Any())
        {
            ipvalue = context.Request.HttpContext.Connection.RemoteIpAddress.ToString();
        }

        maxminddb.GetLocation(ipvalue).CopyTo(location);

        context.Items["X-GeoIP-Location"] = location;

        await next(context);
    }
}

public static class LocationExtensions
{
    public static IServiceCollection AddMaxMindGeoIp(this IServiceCollection services, MaxMindGeoIpOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        services.AddSingleton(context => new MaxMindGeoIpDb(options));

        return services;
    }

    public static IServiceCollection AddLocationMiddleware(this IServiceCollection services, MaxMindGeoIpOptions options)
    {
        services.AddMaxMindGeoIp(options);
        services.AddScoped<GeoIPLocation>();

        return services;
    }

    public static IApplicationBuilder UseLocationMiddleware(this IApplicationBuilder builder)
    {
        builder.UseMiddleware<LocationMiddleware>();

        return builder;
    }
}

public class MaxMindGeoIpOptions
{
    public string BlocksPath { get; }
    public string LocationsPath { get; }

    public MaxMindGeoIpOptions(string blockspath, string locationspath)
    {
        ArgumentNullException.ThrowIfNull(blockspath);
        ArgumentNullException.ThrowIfNull(locationspath);

        BlocksPath = blockspath;
        LocationsPath = locationspath;
    }
}

public class MaxMindGeoIpDb
{
    private MaxMindGeoIpOptions options;

    public MaxMindGeoIpDb(MaxMindGeoIpOptions options)
    {
        this.options = options;
    }

    public GeoIPLocation GetLocation(string sourceip, string language = "en")
    {
        try
        {
            var block = GetBlockInfo(sourceip);

            var location = GetLocationInfo(block.id, language);

            return new GeoIPLocation(block.isvpn, double.Parse(block.latitude), double.Parse(block.longitude), location.continentname, location.countryname.Replace("\"", ""), location.cityname.Replace("\"", ""), location.sub1code, block.postal.Replace("\"", ""), location.timezone);
        }
        catch
        {
            return new GeoIPLocation(null, 0, 0, null, null, null, null, null, null);
        }
    }

    private string Find(string parameter, string path, Func<string, string, int> comparer)
    {
        if (!File.Exists(path))
        {
            return null;
        }

        using (var fs = File.OpenRead(path))
        {
            var low = 0L;
            // We don't need to start at the very end
            var high = fs.Length - (40 - 1); // EOF - 1

            using (var sr = new StreamReader(fs))
            {

                while (low <= high)
                {
                    var middle = (low + high + 1) / 2;
                    fs.Seek(middle, SeekOrigin.Begin);

                    // Resync with base stream after seek
                    sr.DiscardBufferedData();

                    var readLine = sr.ReadLine();

                    // 1) If we are NOT at the beginning of the file, we may have only read a partial line so
                    //    Read again to make sure we get a full line.
                    // 2) No sense reading again if we are at the EOF
                    if ((middle > 0) && (!sr.EndOfStream)) readLine = sr.ReadLine() ?? "";

                    var parts = readLine.Split(',');
                    var locationcol = parts[0];

                    var compare = comparer(parameter, locationcol);

                    if (compare < 0)
                    {
                        high = middle - 1;
                    }
                    else if (compare > 0)
                    {
                        low = middle + 1;
                    }
                    else
                    {
                        return readLine;
                    }
                }
            }
        }

        return null;
    }

    private (string id, string language, string continent, string continentname, string countryiso, string countryname, string sub1code, string sub1iso, string sub2code, string sub2iso, string cityname, string metrocode, string timezone, string eumember) GetLocationInfo(string locationid, string language)
    {
        var path = options.LocationsPath;
        if(path.Contains("{0}"))
        {
            path = String.Format(path, language);
        }

        var result = Find(locationid, path, (s, s1) =>
        {
            var compare = int.Parse(s) < int.Parse(s1) ? -1 : (int.Parse(s) > int.Parse(s1) ? 1 : 0);
            return compare;
        });

        if (result != null)
        {
            var parts = result.Split(",");
            return (parts[0], parts[1], parts[2], parts[3], parts[4], parts[5], parts[6], parts[7], parts[8], parts[9], parts[10], parts[11], parts[12], parts[13]);

        }
        return (null, null, null, null, null, null, null, null, null, null, null, null, null, null);
    }

    private (string id, string postal, string latitude, string longitude, string isvpn) GetBlockInfo(string sourceip)
    {
        var result = Find(sourceip, options.BlocksPath, IsInRange);

        if (result == null)
        {
            return default;
        }
        var parts = result.Split(',');
        return (parts[1], parts[6], parts[7], parts[8], parts[4]);
    }

    //ipv4 only
    private static int IsInRange(string sourceIp, string iprange)
    {
        if (string.IsNullOrWhiteSpace(iprange))
        {
            return 0;
        }

        var sourceparts = sourceIp.Split('.', '/');
        var sourceipnumber = (Convert.ToUInt32(sourceparts[0]) << 24) |
                             (Convert.ToUInt32(sourceparts[1]) << 16) |
                             (Convert.ToUInt32(sourceparts[2]) << 8) |
                             Convert.ToUInt32(sourceparts[3]);

        try
        {
            var parts = iprange.Trim().Split('.', '/');

            for (var i = 0; i < parts.Length; i++)
            {
                if (!int.TryParse(parts[i], out var dot))
                {
                    throw new Exception("invalid ip address");
                }
                else if (dot < 0 || dot > 255)
                {
                    throw new Exception("invalid ip address");
                }
            }

            var ipnumber = (Convert.ToUInt32(parts[0]) << 24) | (Convert.ToUInt32(parts[1]) << 16) | (Convert.ToUInt32(parts[2]) << 8) | Convert.ToUInt32(parts[3]);

            var mask = 0xffffffff;
            if (parts.Length == 5)
            {
                var maskbits = Convert.ToInt32(parts[4]);
                mask <<= (32 - maskbits);
            }

            var ipstart = ipnumber & mask;
            var ipend = ipnumber | (mask ^ 0xffffffff);

            var start = ipstart;
            var end = ipend;

            if (sourceipnumber < start)
            {
                return -1;
            }

            if (sourceipnumber > end)
            {
                return 1;
            }

            return 0;
        }
        catch
        {
            //invalid entry
        }

        return -1;
    }

    //TODO add IPv6 support
}

public class GeoIPLocation
{
    public double Latitude { get; private set; }
    public double Longitude { get; private set; }
    public string Continent { get; private set; }
    public string Country { get; private set; }
    public string City { get; private set; }
    public string State { get; private set; }
    public string Postal { get; private set; }
    public string TimeZone { get; private set; }
    public string IsVPN { get; private set; }

    public GeoIPLocation() { }

    public GeoIPLocation(string isvpn, double latitude, double longitude, string continent, string country, string city, string state, string postal, string timezone)
    {
        IsVPN = isvpn;
        Latitude = latitude;
        Longitude = longitude;
        Continent = continent;
        Country = country;
        City = city;
        State = state;
        Postal = postal;
        TimeZone = timezone;
    }

    public void CopyTo(GeoIPLocation location)
    {
        location.IsVPN = this.IsVPN;
        location.Latitude = this.Latitude;
        location.Longitude = this.Longitude;
        location.Continent = this.Continent;
        location.Country = this.Country;
        location.City = this.City;
        location.State = this.State;
        location.Postal = this.Postal;
        location.TimeZone = this.TimeZone;
    }
}