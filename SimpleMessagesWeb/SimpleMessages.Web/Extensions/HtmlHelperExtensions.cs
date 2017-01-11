using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;

namespace SimpleMessages.Web.Extensions
{
    public static class HtmlHelperExtensions
    {
        /// <summary>
        /// An Html helper for Require.js
        /// </summary>
        /// <see cref="https://web.archive.org/web/20150502094922/http://tech.pro/tutorial/1156/using-requirejs-in-an-aspnet-mvc-application"/>
        /// <param name="helper"></param>
        /// <param name="common">Location of the common js file.</param>
        /// <param name="module">Location of the main.js file.</param>
        /// <returns></returns>
        public static MvcHtmlString RequireJs(this HtmlHelper helper, string common, string module)
        {
            var sb = new StringBuilder();
            sb.AppendFormat("require(['{0}'], function() {{ require(['{1}']); }});", common, module);

            return new MvcHtmlString(sb.ToString());
        }

        /// <summary>
        /// An Html helper for "simple" checkbox (that can e.g. support bootstrap-switch)
        /// </summary>
        public static MvcHtmlString BasicCheckBoxFor<T>(this HtmlHelper<T> html, Expression<Func<T, bool>> expression, object htmlAttributes = null)
        {
            var tag = new TagBuilder("input");

            tag.Attributes["type"] = "checkbox";
            tag.Attributes["id"] = html.IdFor(expression).ToString();
            tag.Attributes["name"] = html.NameFor(expression).ToString();
            tag.Attributes["value"] = "true";

            // set the "checked" attribute if true
            ModelMetadata metadata = ModelMetadata.FromLambdaExpression(expression, html.ViewData);
            if (metadata.Model != null)
            {
                bool modelChecked;
                if (Boolean.TryParse(metadata.Model.ToString(), out modelChecked))
                {
                    if (modelChecked)
                    {
                        tag.Attributes["checked"] = "checked";
                    }
                }
            }

            // merge custom attributes
            tag.MergeAttributes(HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes));

            var tagString = tag.ToString(TagRenderMode.SelfClosing);

            var finalHtml = MvcHtmlString.Create(tagString);

            return finalHtml;
        }

    }
}