using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;

namespace Molinos.Orquest.Web.Helpers
{
    public static class ExtensionesSelectList
    {
        public static List<SelectListItem> ToSelectList(this IEnumerable<string> source, string selected = "")
        {
            return source.Select(item => new SelectListItem { Value = item, Text = item, Selected = item == selected }).ToList();
        }

        public static List<SelectListItem> ToSelectList<TSource>(this IEnumerable<TSource> source,
            Func<TSource, string> valueSelector, Func<TSource, string> textSelector, string selected = "")
        {
            return source.Select(item => new SelectListItem
            {
                Value = valueSelector(item),
                Text = textSelector(item),
                Selected = valueSelector(item) == selected
            })
                    .ToList();
        }

        public static List<SelectListItem> ToSelectList<TSource>(this IEnumerable<TSource> source)
        {
            return source.Select(item => new SelectListItem
            {
                Value = item.ToString(),
                Text = item.ToString(),
            }).ToList();
        }


        public static IEnumerable<SelectListItem> ToSelectList(this Enum enumValue)
        {
            return from Enum e in Enum.GetValues(enumValue.GetType())
                   select new SelectListItem
                   {
                       Selected = e.Equals(enumValue),
                       Text = e.ToDescription(),
                       Value = Convert.ToInt32(e).ToString(CultureInfo.InvariantCulture)
                   };
        }
        private static string ToDescription(this Enum value)
        {
            var attributes = (DescriptionAttribute[])value.GetType().GetField(value.ToString()).GetCustomAttributes(typeof(DescriptionAttribute), false);
            return attributes.Length > 0 ? attributes[0].Description : value.ToString();
        }
    }
}