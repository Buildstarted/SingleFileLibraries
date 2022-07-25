//Requires: Microsoft.AspNetCore.Http.Abstractions, Microsoft.Extensions.DependencyInjection.Abstractions
//or: Microsoft.AspNetCore.App

using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.DependencyInjection;

namespace OpenStreetMaps;

public class OpenStreetMapsOptions
{
    public int DefaultZoom { get; set; } = 12;
    public string TileServerTemplate { get; set; } = "https://tile.openstreetmap.org/{0}/{1}/{2}.png";
    public MarkerAnchor MarkerAnchor { get; set; }
    public string Marker { get; set; } = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAABkAAAAnCAMAAADNRxOMAAAABGdBTUEAALGPC/xhBQAAAAFzUkdCAK7OHOkAAAAJcEhZcwAACxMAAAsTAQCanBgAAAIlUExURUdwTDMzMzMzMzMzMzMzMzMzMzMzMzMzMzMzMzMzMzMzMzMzMzMzMzQ0NDQ0NDMzMzMzMzMzMzMzMzMzMzMzMzMzMzMzMzMzMzMzMzMzMzMzMzMzMzMzMzMzMzMzMzMzMzMzMzMzMzMzMzMzM////2vbdHXdfm3cdnPdfHDceX/gh3rfgoLhiofijoDgiIzjk4/kljU1NZ3no43jlLe3t+Li4lrXZOnp6XjegF7YaKGhoTg4OHLde+fn503UWGPZbUPRT13YZ0XSUWjacYWFhVvXZX19fT09PcXFxf39/W9vb2FhYampqe7u7kZGRoyMjKLjuVDVW6urq2/ceFBQUPX99klJSUFBQa2trTo6Ov3//YXhjUtLS9bW1j8/P8HBwZrmoGrbc1LVXYKCgl5eXrm5uZLkmWpqamNjY9nZ2dvb28jIyMrKyknKcEzMc1xcXPj4+KSkpKXjvpzkrMTu0azpt+r662facPv++5fhrOv67JXgrVbWYUDRTK/qunvfg9HR0U7UWVPVXtDy2nPdfXPUm/z+/vj9+ZPkm5PdspXlnMDtzUbSUoHYo67pusbv0KvlxK/owmLZbGXab9Dx3G3XhIbglHDWjff99/7//vz+/ErTVVbVZdL015znokjTU/r++lLPc13Yabjpzcrw05DgpVjWYsbu1VXWYMPwyJPct4Lcml7Qgn7ejV/UdY7goVHQbmXVfVLTZlPOePPz83t7e4CAgIu15LQAAAAkdFJOUwA8igxyeHsDXaW3SzmrOZMYLZkwY0ixP6jPutK9RTMVAmZgWo/J8TYAAAHkSURBVBgZjcH1QxoBGAbgDwQO7G5dvUe4id5Gd9rd3Z3r3lx3d3d3/32DExkc/LDnoTCmdNOGjaUUIy+fW/7t+7PM5edRJJFiYvIXeJMTChGFSbp1wPspk8n09e0z6LolFCLr0OPqbc+Y3+9/9/TRdeg7ZMSTdxnR4jLY+qeejFRqbzguw9glpwCGO9Z3oNVpGtnKu2A7cfoMOIaIZON9R4/sY8srQ1bYsTv3xmVEDGd8YHCzrFIb0s56Xa+MHEOiGbx0sAEqtTLApmJZ9s1HzIhIbMFnAxvN+xMWMRUv4ItTJbCIhWIqmsO3UY3AEuaKqGAWP1RqgSXMFlDhPL57lQKLmC8ksQUfXmyOpvkEi5hKpoHnZdHcrzFdQgxnx92bWyKpHz4GxxBJfbh1SVP+j9Zw7b5PSgG5w7jiaa9YU+Y+j+FcCsoZ0uPicWf/tqCKUde5s/qhHOJJBgaBQ45Ws9l82HPqZOfggIRCsnt7sH/v7p0tu/Yc7ERPbzaFZWW2YU1bZhZFyLDascpuzaAo0masapZStPSmWvCa0kkgsQZBNYkkJK9CUJWchBKqtyOgOoFiKHQAdAqKlVQHoC6JYjFcIxo5huJIa0BDGsWTXF9bn0xxpexIofhSrakU3/p19F/+AlDxiLGG8f2uAAAAAElFTkSuQmCC";
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
}

public class OpenStreetMaps
{
    public TilePosition[] GetTiles(double lat, double lon, int zoom)
    {
        var pos = GetPosition(lat, lon, zoom);

        var tiles = new TilePosition[] {
            new TilePosition(pos.X - 1, pos.Y - 1, zoom), new TilePosition(pos.X, pos.Y - 1, zoom), new TilePosition(pos.X + 1, pos.Y - 1, zoom),
            new TilePosition(pos.X - 1, pos.Y , zoom),    new TilePosition(pos.X, pos.Y , zoom),    new TilePosition(pos.X + 1, pos.Y, zoom ),
            new TilePosition(pos.X - 1, pos.Y + 1, zoom), new TilePosition(pos.X, pos.Y + 1, zoom), new TilePosition(pos.X + 1, pos.Y + 1, zoom),
        };

        return tiles;
    }

    public MarkerOffsetFromTile GetMarkerPosition(double lat, double lon, int zoom)
    {
        var pos = GetPosition(lat, lon, zoom);
        var gps = GetGpsFromTile(pos.X, pos.Y, pos.Zoom);
        var offset = GetOffset(lat, lon, gps.Latitude, gps.Longitude, zoom);

        return offset;
    }

    public GpsPosition GetGpsFromTile(int x, int y, int zoom)
    {
        zoom = Math.Min(14, Math.Max(1, zoom));

        var lon = x / (double)(1 << zoom) * 360 - 180;

        double n = Math.PI - 2.0 * Math.PI * y / (double)(1 << zoom);
        var lat = 180 / Math.PI * Math.Atan(.5 * (Math.Exp(n) - Math.Exp(-n)));

        return new GpsPosition(lat, lon, zoom);
    }

    public TilePosition GetPosition(double latitude, double longitude, int zoom)
    {
        zoom = Math.Min(14, Math.Max(1, zoom));

        double lonrad = (longitude * Math.PI) / 180;
        double latrad = (latitude * Math.PI) / 180;

        var n = 1 << zoom;

        var x = (int)Math.Floor(((longitude + 180) / 360) * n);
        var y = (int)Math.Floor(((1 - Math.Log(Math.Tan(latrad) + 1 / Math.Cos(latrad)) / Math.PI) / 2 * n));

        return new TilePosition(x, y, zoom);
    }

    private MarkerOffsetFromTile GetOffset(double latitude1, double longitude1, double latitude2, double longitude2, int zoom)
    {
        var result1 = GetPosition(latitude1, longitude1, zoom);
        var result2 = GetPosition(latitude2, longitude2, zoom);

        var x = (int)((result1.X - result2.X) * 256);
        var y = (int)((result1.Y - result2.Y) * 256);

        return new MarkerOffsetFromTile(x, y);
    }
}

[HtmlTargetElement("osm")]
[OutputElementHint("div")]
public class OpenStreetMapsTagHelper : TagHelper
{
    private const int TileSize = 256;
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
        var tiles = osm.GetTiles(Latitude, Longitude, Zoom);

        output.TagName = "div";
        output.Attributes.Add("class", "osm-map");

        output.Content.AppendHtml("<div>");
        output.Content.AppendHtml($@"  <div><img src=""{string.Format(options.TileServerTemplate, Zoom, tiles[0].X, tiles[0].Y)}"" /></div>");
        output.Content.AppendHtml($@"  <div><img src=""{string.Format(options.TileServerTemplate, Zoom, tiles[1].X, tiles[1].Y)}"" /></div>");
        output.Content.AppendHtml($@"  <div><img src=""{string.Format(options.TileServerTemplate, Zoom, tiles[2].X, tiles[2].Y)}"" /></div>");
        output.Content.AppendHtml("</div>");

        output.Content.AppendHtml("<div>");
        output.Content.AppendHtml($@"  <div><img src=""{string.Format(options.TileServerTemplate, Zoom, tiles[3].X, tiles[3].Y)}"" /></div>");
        output.Content.AppendHtml($@"  <div><img src=""{string.Format(options.TileServerTemplate, Zoom, tiles[4].X, tiles[4].Y)}"" /></div>");
        if(ShowMarker)
        {
            var offset = osm.GetMarkerPosition(Latitude, Longitude, Zoom);

            var percentagex = (offset.X * 100 / TileSize);
            var percentagey = (offset.Y * 100 / TileSize);

            output.Content.AppendHtml($@"<img style=""z-index: 99999; position: absolute; top: calc({percentagey}% - {options.MarkerAnchor.Y}px); left: calc({percentagex}% - {options.MarkerAnchor.X}px);"" src=""{options.Marker}"" />");
        }
        output.Content.AppendHtml($@"  <div><img src=""{string.Format(options.TileServerTemplate, Zoom, tiles[5].X, tiles[5].Y)}"" /></div>");
        output.Content.AppendHtml("</div>");

        output.Content.AppendHtml("<div>");
        output.Content.AppendHtml($@"  <div><img src=""{string.Format(options.TileServerTemplate, Zoom, tiles[6].X, tiles[6].Y)}"" /></div>");
        output.Content.AppendHtml($@"  <div><img src=""{string.Format(options.TileServerTemplate, Zoom, tiles[7].X, tiles[7].Y)}"" /></div>");
        output.Content.AppendHtml($@"  <div><img src=""{string.Format(options.TileServerTemplate, Zoom, tiles[8].X, tiles[8].Y)}"" /></div>");
        output.Content.AppendHtml("</div>");
        output.Content.AppendHtml("</div>");

        output.TagMode = TagMode.StartTagAndEndTag;
    }
}

public struct GpsPosition
{
    public double Latitude { get; }
    public double Longitude { get; }
    public int Zoom { get; }

    public GpsPosition(double latitude, double longitude, int zoom)
    {
        Latitude = latitude;
        Longitude = longitude;
        Zoom = zoom;
    }
}

public struct MarkerOffsetFromTile
{
    public int X { get; }
    public int Y { get; }

    public MarkerOffsetFromTile(int x, int y)
    {
        X = x;
        Y = y;
    }
}

public struct TilePosition
{
    public int X { get; }
    public int Y { get; }
    public int Zoom { get; }

    public TilePosition(int x, int y, int zoom)
    {
        X = x;
        Y = y;
        Zoom = zoom;
    }
}

public struct MarkerAnchor
{
    public int X { get; }
    public int Y { get; }

    public MarkerAnchor(int x, int y)
    {
        X = x;
        Y = y;
    }
}