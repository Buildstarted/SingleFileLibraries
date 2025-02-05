//Uncomment this line if your project can be compiled in unsafe mode
//#define UNSAFE
namespace SingleFileLibraries;

using Microsoft.Extensions.Options;
using System.Net;

public class MaxMindLocationDB
{
    private MaxMindLocationOptions options;

    public MaxMindLocationDB(IOptions<MaxMindLocationOptions> options)
    {
        this.options = options.Value;

        ArgumentNullException.ThrowIfNull(this.options.LocationsPath);
        ArgumentNullException.ThrowIfNull(this.options.IPv4BlocksPath);
        ArgumentNullException.ThrowIfNull(this.options.IPv6BlocksPath);
    }

    public GeoIPLocation GetLocation(IPAddress address, string language = "en")
    {
        ArgumentNullException.ThrowIfNull(address);
        var block = GetBlockInfo(address);

        if (block == null)
        {
            return null;
        }

        var location = GetLocationInfo(block.Id, language);

        return new GeoIPLocation(location.Language, location.Continent, location.ContinentName,
                                 location.CountryISO, location.CountryName, location.Sub1Code,
                                 location.Sub1ISO, location.Sub2Code, location.Sub2ISO, location.CityName,
                                 location.MetroCode, location.TimeZone, location.EUMember, block.IPRange,
                                 block.PostalCode, block.Latitude, block.Longitude, block.IsVPN,
                                 block.IsSatelliteProvider, block.AccuracyRadius, address.ToString());
    }

#if UNSAFE 
    private unsafe string Find<T>(T value, string path, Func<T, string, int> comparer)
#else
    private string Find<T>(T value, string path, Func<T, string, int> comparer)
#endif
    {
        if (!File.Exists(path))
        {
            throw new FileNotFoundException("Unable to find MaxMindGeoIP database", path, null);
        }

        string result = null;
        long high;
        long low;
        const int BufferSize = 256;
#if UNSAFE
        var bufferback = stackalloc char[BufferSize];
        var lineback = stackalloc char[BufferSize];
#else
        var bufferback = new char[BufferSize];
        var lineback = new char[BufferSize];
#endif

        using var filestream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.None, bufferSize: 1);
        low = 0;
        high = filestream.Length - 1;

        using var reader = new StreamReader(filestream);
        while (low <= high)
        {
            var middle = (low + high + 1) / 2;
            filestream.Seek(middle, SeekOrigin.Begin);

            reader.DiscardBufferedData();
#if UNSAFE
            var line = new Span<char>(lineback, BufferSize);
            var buffer = new Span<char>(bufferback, BufferSize);
#else
            var line = new Span<char>(lineback);
            var buffer = new Span<char>(bufferback);
#endif
            var writtenToLine = false;
            int bytesread;

            while ((bytesread = reader.Read(buffer)) != 0)
            {
                var newlineindex = buffer.IndexOf<char>('\n');
                if (newlineindex != -1)
                {
                    buffer = buffer.Slice(newlineindex + 1);

                    if (writtenToLine)
                    {
                        result = string.Concat(line, buffer);
                        break;
                    }
                    else
                    {
                        newlineindex = buffer.IndexOf<char>('\n');

                        if (newlineindex != -1)
                        {
                            buffer = buffer.Slice(0, newlineindex);
                            result = new string(buffer);
                            break;
                        }
                        else
                        {
                            buffer.CopyTo(line);
                            writtenToLine = true;
                        }
                    }
                }
            }

            var column = result[0..result.IndexOf(',')];

            var compare = comparer(value, column);

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
                return result;
            }
        }

        return result;
    }

#if UNSAFE
    private unsafe Location GetLocationInfo(string locationid, string language)
#else
    private Location GetLocationInfo(string locationid, string language)
#endif
    {
        var path = options.LocationsPath;
        if (path.Contains("{0}"))
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
#if UNSAFE
            var ranges = stackalloc Range[14];
            ParseLine(result, ref ranges);
#else
            var ranges = ParseLine(result, 14);
#endif

            var id = result[ranges[0]];
            var lang = result[ranges[1]];
            var continent = result[ranges[2]];
            var countryiso = result[ranges[4]];
            var sub1code = result[ranges[6]];
            var sub1iso = result[ranges[7]];
            var sub2code = result[ranges[8]];
            var sub2iso = result[ranges[9]];
            var metrocode = result[ranges[11]];
            var timezone = result[ranges[12]];
            var eumember = result[ranges[13]] == "1";

            //special cases that may be wrapped in quotes

            var continentname = result[ranges[3]].Length > 0 && result[ranges[3]][0] == '"'
                ? result[ranges[3]][1..^1]
                : result[ranges[3]];

            var countryname = result[ranges[5]].Length > 0 && result[ranges[5]][0] == '"'
                ? result[ranges[5]][1..^1]
                : result[ranges[5]];

            var cityname = result[ranges[10]].Length > 0 && result[ranges[10]][0] == '"'
                ? result[ranges[10]][1..^1]
                : result[ranges[10]];

            return new Location(id, lang, continent, continentname, countryiso, countryname, sub1code, sub1iso, sub2code, sub2iso, cityname, metrocode, timezone, eumember);
        }

        return null;
    }

#if UNSAFE
    private unsafe Block GetBlockInfo(IPAddress address)
#else
    private Block GetBlockInfo(IPAddress address)
#endif
    {
        var path = address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6 ? options.IPv6BlocksPath : options.IPv4BlocksPath;

        var result = Find(address, path, IsInRange);

        if (result == null)
        {
            return default;
        }

#if UNSAFE
        var ranges = stackalloc Range[14];
        ParseLine(result, ref ranges);
#else
        var ranges = ParseLine(result, 10);
#endif

        var iprange = result[ranges[0]];
        var id = result[ranges[1]];
        var isvpn = result[ranges[4]] == "1";
        var issatelliteprovider = result[ranges[5]] == "1";
        var postalcode = result[ranges[6]];
        var latitude = double.Parse(result[ranges[7]]);
        var longitude = double.Parse(result[ranges[8]]);
        var accuracy = int.Parse(result[ranges[9]]);

        return new Block(iprange, id, postalcode, latitude, longitude, isvpn, issatelliteprovider, accuracy);
    }

    private static int IsInRange(IPAddress address, string cidrRange)
    {
        //check what type of ip
        if (address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
        {
            return IsInRangeIPv6(address, cidrRange);
        }

        return IsInRangeIPv4(address, cidrRange);
    }

    private static int IsInRangeIPv6(IPAddress address, string cidrRange)
    {
        var addressbytes = address.GetAddressBytes();
        ulong addressupper = ConvertIPv6ToUlong(addressbytes);
        ulong addresslower = ConvertIPv6ToUlong(addressbytes, sizeof(ulong));

        var suffixstart = cidrRange.IndexOf("/");
        ulong uppermask = 0;
        ulong lowermask = 0;
        if (suffixstart != -1)
        {
            byte suffix = byte.Parse(cidrRange[(suffixstart + 1)..]);

            if (suffix > 128 || suffix < 0)
            {
                throw new InvalidOperationException($"/{suffix} is invalid for a IPv6 CIDR");
            }

            if (suffix <= 64)
            {
                uppermask = ~(((ulong)1 << (64 - suffix)) - 1);
                lowermask = ulong.MinValue;
            }
            else
            {
                uppermask = ulong.MaxValue;
                lowermask = ~(((ulong)1 << (128 - suffix)) - 1);
            }
        }

        var rangeaddress = IPAddress.Parse(cidrRange[0..(suffixstart != -1 ? suffixstart : ^0)]);
        var rangeaddressbytes = rangeaddress.GetAddressBytes();
        ulong rangeupper = ConvertIPv6ToUlong(rangeaddressbytes);
        ulong rangelower = ConvertIPv6ToUlong(rangeaddressbytes, sizeof(ulong));

        ulong startupper = rangeupper & uppermask;
        ulong startlower = rangelower & lowermask;

        ulong endupper = rangeupper | ~uppermask;
        ulong endlower = rangelower | ~lowermask;


        //TODO find out if this is valid
        if (addressupper < startupper || addresslower < startlower)
        {
            return -1;
        }

        if (addressupper > endupper || addresslower > endlower)
        {
            return 1;
        }

        return 0;

        static ulong ConvertIPv6ToUlong(byte[] address, int offset = 0)
        {
            ulong result = 0;

            for (var i = 0; i < sizeof(ulong); i++)
            {
                result |= (ulong)address[i + offset] << ((sizeof(ulong) - i - 1) * 8);
            }

            return result;
        }
    }

    private static int IsInRangeIPv4(IPAddress address, string cidrRange)
    {
        if (string.IsNullOrWhiteSpace(cidrRange))
        {
            throw new InvalidOperationException("An invalid IP Range was provided.");
        }

        var addressbytes = address.GetAddressBytes();
        var sourceipnumber = ((ulong)addressbytes[0] << 24) | ((ulong)addressbytes[1] << 16) | ((ulong)addressbytes[2] << 8) | (ulong)addressbytes[3];

        var suffixstart = cidrRange.IndexOf("/");
        var mask = 0xffffffff;
        if (suffixstart != -1)
        {
            var suffix = byte.Parse(cidrRange[(suffixstart + 1)..]);
            if (suffix > 32 || suffix < 0)
            {
                throw new InvalidOperationException($"/{suffix} is invalid for a IPv4 CIDR");
            }
            mask <<= (32 - suffix);
        }

        var rangeaddress = IPAddress.Parse(cidrRange[0..(suffixstart != -1 ? suffixstart : ^0)]);
        var rangebytes = rangeaddress.GetAddressBytes();
        var rangenumber = ((ulong)rangebytes[0] << 24) | ((ulong)rangebytes[1] << 16) | ((ulong)rangebytes[2] << 8) | (ulong)rangebytes[3];

        var start = rangenumber & mask;
        var end = rangenumber | (0xffffffff ^ mask);

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

    private record class Location(string Id, string Language, string Continent, string ContinentName, string CountryISO, string CountryName, string Sub1Code, string Sub1ISO, string Sub2Code, string Sub2ISO, string CityName, string MetroCode, string TimeZone, bool EUMember);
    private record class Block(string IPRange, string Id, string PostalCode, double Latitude, double Longitude, bool IsVPN, bool IsSatelliteProvider, int AccuracyRadius);

#if UNSAFE
    unsafe void ParseLine(string line, ref Range* ranges)
    {
#else
    Range[] ParseLine(string line, int columns)
    {
        var ranges = new Range[columns];
#endif
        var index = 0;
        var i = 0;
        while (true)
        {
            if (i >= line.Length) break;

            if (line[i] != '"')
            {
                //non escaped entry
                var startpos = i;
                var endpos = -1;
                while (true)
                {
                    if (i >= line.Length)
                    {
                        endpos = i;
                        break;
                    }

                    if (line[i] == ',')
                    {
                        endpos = i;
                        i++;
                        break;
                    }
                    i++;
                }
                ranges[index++] = new Range(startpos, endpos);
            }
            else
            {
                //escaped entry
                var startpos = i + 1;
                var endpos = -1;
                while (true)
                {
                    if (i >= line.Length)
                    {
                        endpos = i;
                        break;
                    }
                    if (line[i] == '"')
                    {
                        i++;
                        if (i >= line.Length)
                        {
                            endpos = i;
                            break;
                        }

                        if (line[i] == ',')
                        {
                            endpos = i - 1;
                            i++;
                            break;
                        }
                        if (line[i] == '"')
                        {
                            //double quotes so not escaped
                        }
                    }
                    i++;
                }

                ranges[index++] = new Range(startpos, endpos);
            }
        }

#if !UNSAFE
        return ranges;
#endif
    }
}

public class MaxMindLocationOptions
{
    public string IPv4BlocksPath { get; set; }
    public string IPv6BlocksPath { get; set; }
    public string LocationsPath { get; set; }
}


public record class GeoIPLocation(string Language, string Continent, string ContinentName, string CountryISO, string CountryName, string Sub1Code, string Sub1ISO, string Sub2Code, string Sub2ISO, string CityName, string MetroCode, string TimeZone, bool EUMember, string IPRange, string PostalCode, double Latitude, double Longitude, bool IsVPN, bool IsSatelliteProvider, int AccuracyRadius, string IPAddress)
{
    static GeoIPLocation unknown;
    public static GeoIPLocation Unknown => unknown ??= new GeoIPLocation("Unknown", "Unknown", "Unknown", "Unknown", "Unknown", "Unknown", "Unknown", "Unknown", "Unknown", "Unknown", "Unknown", "Unknown", false, "Unknown", "Unknown", 0, 0, false, false, 0, "Unknown");

    public override string ToString()
    {
        return $"Country: {CountryName}, State: {Sub1ISO}, City: {CityName}, Postal Code: {PostalCode}, IsVPN: {IsVPN}, AccuracyRadius: {AccuracyRadius}";
    }
}

public class MaxMindLocationMiddleware
{
    public const string XMaxMindLocationKey = "X-MaxMindGeoIP-Location";
    private const string XForwardedForHeader = "X-Forwarded-For";
    private readonly RequestDelegate next;
    private readonly MaxMindLocationDB maxMindDB;

    public MaxMindLocationMiddleware(RequestDelegate next, MaxMindLocationDB maxMindDB)
    {
        this.next = next;
        this.maxMindDB = maxMindDB;
    }

    public async Task Invoke(HttpContext context)
    {
        IPAddress? address;
        context.Request.HttpContext.Request.Headers.TryGetValue(XForwardedForHeader, out var ipvalue);

        if (ipvalue.Any())
        {
            address = IPAddress.Parse(ipvalue);
        }
        else
        {
            address = context.Request.HttpContext.Connection.RemoteIpAddress;
        }

        var location = maxMindDB.GetLocation(address);

        context.Items[XMaxMindLocationKey] = location;

        await next(context);
    }
}

public static class MaxMindLocationExtensions
{
    public static IServiceCollection AddMaxMindLocation(this IServiceCollection services)
    {
        services.AddSingleton<MaxMindLocationDB>();

        return services;
    }

    public static IServiceCollection AddMaxMindLocationMiddleware(this IServiceCollection services)
    {
        services.AddMaxMindLocation();

        return services;
    }

    public static IApplicationBuilder UseMaxMindLocationMiddleware(this IApplicationBuilder builder)
    {
        builder.UseMiddleware<MaxMindLocationMiddleware>();

        return builder;
    }
}
