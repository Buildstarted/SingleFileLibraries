﻿using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Collections.Generic;
using System.Reflection;
using System.Text.Encodings.Web;

namespace SingleFileLibraries;

[HtmlTargetElement(Attributes = ClassApplicatorTagHelper.Prefix + "*")]
public class ClassApplicatorTagHelper : TagHelper
{
    private const string Prefix = "class:";
    private readonly HtmlEncoder htmlEncoder;

    [ViewContext, HtmlAttributeNotBound]
    public ViewContext ViewContext { get; set; }

    private const BindingFlags DefaultLookup = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.IgnoreCase;

    public ClassApplicatorTagHelper(HtmlEncoder htmlEncoder)
    {
        this.htmlEncoder = htmlEncoder;
    }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        foreach (var attribute in context.AllAttributes)
        {
            //if an attribute has a value of true in razor then the value of the attribute
            //will be the name of the attribute itself. As such we just need to compare
            //the value of the attribute with the name and if they're the same and they
            //start with the prefix then we're ok

            if (attribute.Name.Length > Prefix.Length
                && attribute.Name[0..Prefix.Length] == Prefix)
            {
                //remove the attribute from the element
                //as it would end up with
                //class:text-danger="class:text-danger"
                //in addition to the class value

                output.Attributes.Remove(attribute);

                var classNames = GetAttributeClassnames(attribute);

                foreach (var name in classNames)
                {
                    output.AddClass(name, htmlEncoder);
                }
            }
        }
        base.Process(context, output);
    }

    private IEnumerable<string> GetAttributeClassnames(TagHelperAttribute attribute)
    {
        var model = ViewContext.ViewData.Model;
        var modelType = model?.GetType();
        var names = attribute.Name[PrefixLength..].Split("|");

        foreach (var name in names)
        {
            if (modelType is not null && attribute.ValueStyle == HtmlAttributeValueStyle.Minimized)
            {
                //Support for converting kebab case to camelcase
                //css classes tend to separate words using a `-`
                //is-admin -> isadmin

                var namelookup = name.Replace("-", "");

                //TODO(ben): Look into caching this for better performance
                var modelProperty = modelType.GetProperty(namelookup, DefaultLookup);
                var value = modelProperty?.GetValue(model);

                if (value is null) continue;

                //if the property is a boolean only return the class name if the value
                //is true
                if (modelProperty?.PropertyType == typeof(bool) && value is true)
                {
                    yield return name;
                }

                //if the property is not a boolean and we got here because the value
                //is not null then return the class name
                else
                {
                    yield return name;
                }
            }

            //by default razor attributes that are set to a value of true
            //have their name set as the value instead
            else if (attribute.Name == attribute.Value)
            {
                yield return name;
            }
        }
    }
}