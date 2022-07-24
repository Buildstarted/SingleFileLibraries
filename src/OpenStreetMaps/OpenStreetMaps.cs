using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace OpenStreetMaps;

public class OpenStreetMapsOptions
{
    public int DefaultZoom { get; set; } = 12;
    public string TileServerTemplate { get; set; } = "https://tile.openstreetmap.org/{0}/{1}/{2}.png";
    public string CachePath { get; set; } = @"c:\data\osm-cache";
    public (int X, int Y) MarkerOffset { get; set; } = (0, 0);

    //can be a remote cdn server like https://tile.openstreetmap.org
    //https://cartodb-basemaps-a.global.ssl.fastly.net/dark_all/{0}/{1}/{2}.png
    private string baseUrl = "/osm";
    public string BaseUrl
    {
        get => baseUrl;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentNullException(nameof(value));
            }

            baseUrl = value.EndsWith("/")
                ? value.Substring(0, value.Length - 1)
                : value;
        }
    }

    public string Marker { get; set; } = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAABkAAAAnCAMAAADNRxOMAAAABGdBTUEAALGPC/xhBQAAAAFzUkdCAK7OHOkAAAAJcEhZcwAACxMAAAsTAQCanBgAAAIlUExURUdwTDMzMzMzMzMzMzMzMzMzMzMzMzMzMzMzMzMzMzMzMzMzMzMzMzQ0NDQ0NDMzMzMzMzMzMzMzMzMzMzMzMzMzMzMzMzMzMzMzMzMzMzMzMzMzMzMzMzMzMzMzMzMzMzMzMzMzMzMzMzMzM////2vbdHXdfm3cdnPdfHDceX/gh3rfgoLhiofijoDgiIzjk4/kljU1NZ3no43jlLe3t+Li4lrXZOnp6XjegF7YaKGhoTg4OHLde+fn503UWGPZbUPRT13YZ0XSUWjacYWFhVvXZX19fT09PcXFxf39/W9vb2FhYampqe7u7kZGRoyMjKLjuVDVW6urq2/ceFBQUPX99klJSUFBQa2trTo6Ov3//YXhjUtLS9bW1j8/P8HBwZrmoGrbc1LVXYKCgl5eXrm5uZLkmWpqamNjY9nZ2dvb28jIyMrKyknKcEzMc1xcXPj4+KSkpKXjvpzkrMTu0azpt+r662facPv++5fhrOv67JXgrVbWYUDRTK/qunvfg9HR0U7UWVPVXtDy2nPdfXPUm/z+/vj9+ZPkm5PdspXlnMDtzUbSUoHYo67pusbv0KvlxK/owmLZbGXab9Dx3G3XhIbglHDWjff99/7//vz+/ErTVVbVZdL015znokjTU/r++lLPc13Yabjpzcrw05DgpVjWYsbu1VXWYMPwyJPct4Lcml7Qgn7ejV/UdY7goVHQbmXVfVLTZlPOePPz83t7e4CAgIu15LQAAAAkdFJOUwA8igxyeHsDXaW3SzmrOZMYLZkwY0ixP6jPutK9RTMVAmZgWo/J8TYAAAHkSURBVBgZjcH1QxoBGAbgDwQO7G5dvUe4id5Gd9rd3Z3r3lx3d3d3/32DExkc/LDnoTCmdNOGjaUUIy+fW/7t+7PM5edRJJFiYvIXeJMTChGFSbp1wPspk8n09e0z6LolFCLr0OPqbc+Y3+9/9/TRdeg7ZMSTdxnR4jLY+qeejFRqbzguw9glpwCGO9Z3oNVpGtnKu2A7cfoMOIaIZON9R4/sY8srQ1bYsTv3xmVEDGd8YHCzrFIb0s56Xa+MHEOiGbx0sAEqtTLApmJZ9s1HzIhIbMFnAxvN+xMWMRUv4ItTJbCIhWIqmsO3UY3AEuaKqGAWP1RqgSXMFlDhPL57lQKLmC8ksQUfXmyOpvkEi5hKpoHnZdHcrzFdQgxnx92bWyKpHz4GxxBJfbh1SVP+j9Zw7b5PSgG5w7jiaa9YU+Y+j+FcCsoZ0uPicWf/tqCKUde5s/qhHOJJBgaBQ45Ws9l82HPqZOfggIRCsnt7sH/v7p0tu/Yc7ERPbzaFZWW2YU1bZhZFyLDascpuzaAo0masapZStPSmWvCa0kkgsQZBNYkkJK9CUJWchBKqtyOgOoFiKHQAdAqKlVQHoC6JYjFcIxo5huJIa0BDGsWTXF9bn0xxpexIofhSrakU3/p19F/+AlDxiLGG8f2uAAAAAElFTkSuQmCC";

    public bool LocalUrl => BaseUrl?.StartsWith("/") == true;
}

public static class OpenStreetMapExtensions
{
    public static IServiceCollection AddOpenStreetMaps(this IServiceCollection collection) => AddOpenStreetMaps(collection, new OpenStreetMapsOptions());

    public static IServiceCollection AddOpenStreetMaps(this IServiceCollection collection, OpenStreetMapsOptions options)
    {
        collection.AddSingleton<OpenStreetMapsOptions>(options);
        collection.AddSingleton<OpenStreetMaps>();

        return collection;
    }

    public static IApplicationBuilder UseOpenStreetMaps(this IApplicationBuilder builder)
    {
        var osm = builder.ApplicationServices.GetService<OpenStreetMaps>();
        var options = builder.ApplicationServices.GetService<OpenStreetMapsOptions>();

        if (osm == null || options == null)
        {
            throw new Exception("Unable to use OpenStreetMaps. Must call AddOpenStreetMaps first.");
        }

        if (options.LocalUrl)
        {
            //local
            (builder as IEndpointRouteBuilder).MapGet($"{options.BaseUrl}/{{z}}/{{x}}/{{y}}.png", async (HttpContext context) =>
            {
                var osm = context.RequestServices.GetService<OpenStreetMaps>();

                var z = (string)context.Request.RouteValues["z"];
                var x = (string)context.Request.RouteValues["x"];
                var y = (string)context.Request.RouteValues["y"];
                var mode = (string)context.Request.RouteValues["mode"];

                using var file = await osm.GetTile(z, x, y, mode);
                context.Response.ContentType = "image/png";
                await file.CopyToAsync(context.Response.Body);
            });
        }

        return builder;
    }
}

public class OpenStreetMaps
{
    private static string useragent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/87.0.4280.88 Safari/537.36";
    private readonly OpenStreetMapsOptions options;

    public static string UserAgent => useragent;

    public OpenStreetMaps(OpenStreetMapsOptions options)
    {
        this.options = options;
    }

    public async Task<Stream> GetTile(string z, string x, string y, string mode)
    {
        if (!Directory.Exists($"{options.CachePath}/{z}/{x}"))
        {
            Directory.CreateDirectory($"{options.CachePath}/{z}/{x}");
        }

        var localpath = $"{options.CachePath}/{z}/{x}/{y}.png";

        var fileinfo = new FileInfo(localpath);
        if (!fileinfo.Exists || fileinfo.LastWriteTime.AddDays(180) < DateTime.UtcNow)
        {
            //a, b, c
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", UserAgent);

            client.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
            client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");
            client.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.5");
            client.DefaultRequestHeaders.Add("Cache-Control", "no-cache");
            //DefaultRequestHeaders.Add("Connection", "Keep-Alive");
            client.DefaultRequestHeaders.Add("Pragma", "no-cache");
            client.DefaultRequestHeaders.Add("Upgrade-Insecure-Requests", "1");
            client.DefaultRequestHeaders.Add("TE", "Trailers");
            client.DefaultRequestHeaders.Add("DNT", "1");

            client.DefaultRequestHeaders.ExpectContinue = false;
            client.DefaultRequestHeaders.CacheControl.NoCache = true;
            client.DefaultRequestHeaders.ConnectionClose = true;

            try
            {
                var c = 0;
                string url = null;

                url = string.Format(options.TileServerTemplate, z, x, y);

                await using var data = await client.GetStreamAsync(url);
                await using var filestream = new FileStream(localpath, FileMode.OpenOrCreate);
                await data.CopyToAsync(filestream);
                filestream.Close();
            }
            catch (Exception e)
            {
                fileinfo.Delete();
            }
        }

        return new FileStream(localpath, FileMode.Open);
    }

    public ((int X, int Y)[] Tiles, double MarkerOffsetX, double MarkerOffsetY, int Zoom) GetTileCenterAndOffset(double lat, double lon, int zoom)
    {
        var mapcenter = GetTileId(lat, lon, zoom);
        var pos = GetPosition(lat, lon, zoom);
        var gps = GetGpsFromTile(mapcenter.X, mapcenter.Y, pos.Zoom);
        var offset = GetOffset(lat, lon, gps.Lat, gps.Lon, zoom);

        var tiles = new[] {
            (mapcenter.X - 1, mapcenter.Y - 1),(mapcenter.X, mapcenter.Y - 1),(mapcenter.X + 1, mapcenter.Y - 1),
            (mapcenter.X - 1, mapcenter.Y ),(mapcenter.X, mapcenter.Y ),(mapcenter.X + 1, mapcenter.Y ),
            (mapcenter.X - 1, mapcenter.Y + 1),(mapcenter.X, mapcenter.Y + 1),(mapcenter.X + 1, mapcenter.Y + 1),
        };

        return (tiles, offset.X, offset.Y, zoom);
    }

    public (int X, int Y, int Zoom) GetTileId(double lat, double lon, int zoom)
    {
        zoom = Math.Min(14, Math.Max(1, zoom));

        var latradians = (lat * Math.PI) / 180;
        var n = 1 << zoom;

        var x = (int)((lon + 180) / 360 * n);
        var y = (int)((1 - Math.Log(Math.Tan(latradians) + 1 / Math.Cos(latradians)) / Math.PI) / 2 * n);

        return (x, y, zoom);
    }

    public (double Lat, double Lon, int Zoom) GetGpsFromTile(int x, int y, int zoom)
    {
        zoom = Math.Min(14, Math.Max(1, zoom));

        var lon = x / (double)(1 << zoom) * 360 - 180;

        double n = Math.PI - 2.0 * Math.PI * y / (double)(1 << zoom);
        var lat = 180 / Math.PI * Math.Atan(.5 * (Math.Exp(n) - Math.Exp(-n)));

        return (lat, lon, zoom);
    }

    public (int X, int Y, int Zoom) GetOffset(double lat1, double lon1, double lat2, double lon2, int zoom)
    {
        var result1 = GetPosition(lat1, lon1, zoom);
        var result2 = GetPosition(lat2, lon2, zoom);

        var x = (int)((result1.X - result2.X) * 256);
        var y = (int)((result1.Y - result2.Y) * 256);

        return (x, y, zoom);
    }

    public (double X, double Y, int Zoom) GetPosition(double lat, double lon, int zoom)
    {
        zoom = Math.Min(14, Math.Max(1, zoom));

        double lonrad = (lon * Math.PI) / 180;
        double latrad = (lat * Math.PI) / 180;

        var n = 1 << zoom;

        var x = ((lon + 180) / 360) * n;
        var y = ((1 - Math.Log(Math.Tan(latrad) + 1 / Math.Cos(latrad)) / Math.PI) / 2 * n);

        return (x, y, zoom);
    }
}

[HtmlTargetElement("osm")]
[OutputElementHint("div")]
public class OpenStreetMapsTagHelper : TagHelper
{
    private readonly OpenStreetMaps osm;
    private readonly OpenStreetMapsOptions options;

    [HtmlAttributeName("Latitude")]
    public double Latitude { get; set; }

    [HtmlAttributeName("Longitude")]
    public double Longitude { get; set; }

    [HtmlAttributeName("Zoom")]
    public int Zoom { get; set; }

    [HtmlAttributeName("ShowMarker")]
    public bool ShowMarker { get; set; }

    public OpenStreetMapsTagHelper(OpenStreetMaps osm, OpenStreetMapsOptions options)
    {
        this.osm = osm;
        this.options = options;
        this.Zoom = options.DefaultZoom;
        ShowMarker = true;
    }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var mapinfo = osm.GetTileCenterAndOffset(Latitude, Longitude, Zoom);

        var percentagex = (mapinfo.MarkerOffsetX * 100 / 256);
        var percentagey = (mapinfo.MarkerOffsetY * 100 / 256);

        output.TagName = "div";
        output.Attributes.Add("class", "osm-map");
        output.Content.SetHtmlContent(
$@"
    <div>
        <div><img src=""{options.BaseUrl}/{Zoom}/{mapinfo.Tiles[0].X}/{mapinfo.Tiles[0].Y}.png"" /></div>
        <div><img src=""{options.BaseUrl}/{Zoom}/{mapinfo.Tiles[1].X}/{mapinfo.Tiles[1].Y}.png"" /></div>
        <div><img src=""{options.BaseUrl}/{Zoom}/{mapinfo.Tiles[2].X}/{mapinfo.Tiles[2].Y}.png"" /></div>
    </div>
    <div>
        <div><img src=""{options.BaseUrl}/{Zoom}/{mapinfo.Tiles[3].X}/{mapinfo.Tiles[3].Y}.png"" /></div>
        <div>
            <img src=""{options.BaseUrl}/{Zoom}/{mapinfo.Tiles[4].X}/{mapinfo.Tiles[4].Y}.png"" />" +
            (ShowMarker ? $@"<img style=""z-index: 99999; position: absolute; top: calc({percentagey}% - {options.MarkerOffset.Y}px); left: calc({percentagex}% - {options.MarkerOffset.X}px);"" src=""{options.Marker}"" />" : "") +
        $@"</div>
        <div><img src=""{options.BaseUrl}/{Zoom}/{mapinfo.Tiles[5].X}/{mapinfo.Tiles[5].Y}.png"" /></div>
    </div>
    <div>
        <div><img src=""{options.BaseUrl}/{Zoom}/{mapinfo.Tiles[6].X}/{mapinfo.Tiles[6].Y}.png"" /></div>
        <div><img src=""{options.BaseUrl}/{Zoom}/{mapinfo.Tiles[7].X}/{mapinfo.Tiles[7].Y}.png"" /></div>
        <div><img src=""{options.BaseUrl}/{Zoom}/{mapinfo.Tiles[8].X}/{mapinfo.Tiles[8].Y}.png"" /></div>
    </div>
");

        output.TagMode = TagMode.StartTagAndEndTag;
    }
}
