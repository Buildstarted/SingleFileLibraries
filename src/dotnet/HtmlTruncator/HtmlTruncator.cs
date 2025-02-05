namespace SingleFileLibraries;

using AngleSharp.Dom;
using AngleSharp.Html.Parser;

public class HtmlTruncator
{
    /// <summary>
    /// Truncates the given HTML string to the specified length, ensuring that words are not cut off in the middle.
    /// </summary>
    /// <param name="html">The HTML string to truncate.</param>
    /// <param name="length">The maximum length of the truncated HTML.</param>
    /// <returns>The truncated HTML string.</returns>
    public static string TruncateHtml(string html, int length)
    {
        if (string.IsNullOrEmpty(html))
            return string.Empty;

        if (length <= 0)
            return string.Empty;

        var parser = new HtmlParser();
        using var document = parser.ParseDocument(html);
        var body = document.Body;

        var totalTextLength = GetTextContentLength(html);
        var truncatedHtml = TruncateNode(body, length, out bool truncated);

        if (truncated && length < totalTextLength)
        {
            truncatedHtml = AddEllipsisToLastTextNode(truncatedHtml);
        }

        return truncatedHtml;
    }

    /// <summary>
    /// Recursively truncates the given HTML node to the specified length, ensuring that words are not cut off in the middle.
    /// </summary>
    /// <param name="node">The HTML node to truncate.</param>
    /// <param name="length">The maximum length of the truncated HTML.</param>
    /// <param name="truncated">Indicates whether the content was truncated.</param>
    /// <returns>The truncated HTML string.</returns>
    private static string TruncateNode(INode node, int length, out bool truncated)
    {
        truncated = false;
        if (length <= 0)
            return string.Empty;

        var result = string.Empty;
        foreach (var child in node.ChildNodes)
        {
            if (child is IText textNode)
            {
                var words = textNode.TextContent.Split(' ');
                foreach (var word in words)
                {
                    if (word.Length + 1 <= length) // +1 for the space
                    {
                        result += word + " ";
                        length -= word.Length + 1;
                    }
                    else
                    {
                        truncated = true;
                        break;
                    }
                }
            }
            else if (child is IElement element)
            {
                var truncatedChildHtml = TruncateNode(element, length, out bool childTruncated);
                if (!string.IsNullOrEmpty(truncatedChildHtml))
                {
                    result += $"<{element.TagName.ToLower()}>{truncatedChildHtml}</{element.TagName.ToLower()}>";
                    length -= GetTextContentLength(truncatedChildHtml);
                }
                if (childTruncated)
                {
                    truncated = true;
                    break;
                }
            }

            if (length <= 0)
                break;
        }

        return result.TrimEnd();
    }

    /// <summary>
    /// Adds an ellipsis ("...") to the last text node in the given HTML string.
    /// </summary>
    /// <param name="html">The HTML string to modify.</param>
    /// <returns>The modified HTML string with an ellipsis added to the last text node.</returns>
    private static string AddEllipsisToLastTextNode(string html)
    {
        var parser = new HtmlParser();
        using var document = parser.ParseDocument(html);
        var body = document.Body;

        var lastTextNode = GetLastTextNode(body);
        if (lastTextNode != null && !lastTextNode.TextContent.Contains("..."))
        {
            lastTextNode.TextContent += "...";
        }

        return body.InnerHtml;
    }

    /// <summary>
    /// Recursively finds the last text node in the given HTML node.
    /// </summary>
    /// <param name="node">The HTML node to search.</param>
    /// <returns>The last text node found, or null if no text node is found.</returns>
    private static IText GetLastTextNode(INode node)
    {
        IText lastTextNode = null;
        foreach (var child in node.ChildNodes)
        {
            if (child is IText textNode)
            {
                lastTextNode = textNode;
            }
            else if (child is IElement element)
            {
                var nestedTextNode = GetLastTextNode(element);
                if (nestedTextNode != null)
                {
                    lastTextNode = nestedTextNode;
                }
            }
        }
        return lastTextNode;
    }

    /// <summary>
    /// Calculates the length of the text content in the given HTML string.
    /// </summary>
    /// <param name="html">The HTML string to analyze.</param>
    /// <returns>The length of the text content in the HTML string.</returns>
    private static int GetTextContentLength(string html)
    {
        var parser = new HtmlParser();
        using var document = parser.ParseDocument(html);
        return document.Body.TextContent.Length;
    }
}