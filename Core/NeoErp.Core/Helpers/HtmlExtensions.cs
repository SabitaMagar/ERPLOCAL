﻿using NeoErp.Core.Integration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Web.WebPages;

namespace NeoErp.Core.Helpers
{
    public static class HtmlExtensions
    {
        #region Admin area extensions

        public static MvcHtmlString Hint(this HtmlHelper helper, string value)
        {
            // Create tag builder
            var builder = new TagBuilder("img");

            // Add attributes
            var urlHelper = new UrlHelper(helper.ViewContext.RequestContext);
            var url = MvcHtmlString.Create(urlHelper.Content("~/Administration/Content/images/ico-help.gif")).ToHtmlString();

            builder.MergeAttribute("src", url);
            builder.MergeAttribute("alt", value);
            builder.MergeAttribute("title", value);

            // Render tag
            return MvcHtmlString.Create(builder.ToString());
        }

      
        //public static MvcHtmlString DeleteConfirmation<T>(this HtmlHelper<T> helper, string buttonsSelector) where T :  BaseNopEntityModel
        //{
        //    return DeleteConfirmation(helper, "", buttonsSelector);
        //}

        //public static MvcHtmlString DeleteConfirmation<T>(this HtmlHelper<T> helper, string actionName,
        //    string buttonsSelector) where T : BaseNopEntityModel
        //{
        //    if (String.IsNullOrEmpty(actionName))
        //        actionName = "Delete";

        //    var modalId =  MvcHtmlString.Create(helper.ViewData.ModelMetadata.ModelType.Name.ToLower() + "-delete-confirmation")
        //        .ToHtmlString();

        //    var deleteConfirmationModel = new DeleteConfirmationModel
        //    {
        //        Id = helper.ViewData.Model.Id,
        //        ControllerName = helper.ViewContext.RouteData.GetRequiredString("controller"),
        //        ActionName = actionName,
        //        WindowId = modalId
        //    };

        //    var window = new StringBuilder();
        //    window.AppendLine(string.Format("<div id='{0}' style='display:none;'>", modalId));
        //    window.AppendLine(helper.Partial("Delete", deleteConfirmationModel).ToHtmlString());
        //    window.AppendLine("</div>");
        //    window.AppendLine("<script>");
        //    window.AppendLine("$(document).ready(function() {");
        //    window.AppendLine(string.Format("$('#{0}').click(function (e) ", buttonsSelector));
        //    window.AppendLine("{");
        //    window.AppendLine("e.preventDefault();");
        //    window.AppendLine(string.Format("var window = $('#{0}');", modalId));
        //    window.AppendLine("if (!window.data('kendoWindow')) {");
        //    window.AppendLine("window.kendoWindow({");
        //    window.AppendLine("modal: true,");
        //    window.AppendLine(string.Format("title: '{0}',", EngineContext.Current.Resolve<ILocalizationService>().GetResource("Admin.Common.AreYouSure")));
        //    window.AppendLine("actions: ['Close']");
        //    window.AppendLine("});");
        //    window.AppendLine("}");
        //    window.AppendLine("window.data('kendoWindow').center().open();");
        //    window.AppendLine("});");
        //    window.AppendLine("});");
        //    window.AppendLine("</script>");

        //    return MvcHtmlString.Create(window.ToString());
        //}

        public static MvcHtmlString NopLabelFor<TModel, TValue>(this HtmlHelper<TModel> helper, Expression<Func<TModel, TValue>> expression, bool displayHint = true)
        {
            var result = new StringBuilder();
            var metadata = ModelMetadata.FromLambdaExpression(expression, helper.ViewData);
            var hintResource = string.Empty;
            object value;
            if (metadata.AdditionalValues.TryGetValue("NopResourceDisplayName", out value))
            {
                //var resourceDisplayName = value as NeoResourceDisplayName;
                //if (resourceDisplayName != null && displayHint)
                //{
                //    var langId = _Settings.LanguageID; // EngineContext.Current.Resolve<IWorkContext>().WorkingLanguage.Id;

                //    hintResource =  new MultiLanguageServices().GetResourceValue(langId, resourceDisplayName.ResourceKey + ".Hint");

                //    result.Append(helper.Hint(hintResource).ToHtmlString());
                //}
            }
            result.Append(helper.LabelFor(expression, new { title = hintResource }));
            return MvcHtmlString.Create(result.ToString());
        }

      
        /// <summary>
        /// Render CSS styles of selected index 
        /// </summary>
        /// <param name="helper">HTML helper</param>
        /// <param name="currentIndex">Current tab index (where appropriate CSS style should be rendred)</param>
        /// <param name="indexToSelect">Tab index to select</param>
        /// <returns>MvcHtmlString</returns>
        public static MvcHtmlString RenderSelectedTabIndex(this HtmlHelper helper, int currentIndex, int indexToSelect)
        {
            if (helper == null)
                throw new ArgumentNullException("helper");

            //ensure it's not negative
            if (indexToSelect < 0)
                indexToSelect = 0;
            
            //required validation
            if (indexToSelect == currentIndex)
            {
            return new MvcHtmlString(" class='k-state-active'");
            }

            return new MvcHtmlString("");
        }

        #endregion

        #region Common extensions

        public static MvcHtmlString RequiredHint(this HtmlHelper helper, string additionalText = null)
        {
            // Create tag builder
            var builder = new TagBuilder("span");
            builder.AddCssClass("required");
            var innerText = "*";
            //add additional text if specified
            if (!String.IsNullOrEmpty(additionalText))
                innerText += " " + additionalText;
            builder.SetInnerText(innerText);
            // Render tag
            return MvcHtmlString.Create(builder.ToString());
        }

        public static string FieldNameFor<T, TResult>(this HtmlHelper<T> html, Expression<Func<T, TResult>> expression)
        {
            return html.ViewData.TemplateInfo.GetFullHtmlFieldName(ExpressionHelper.GetExpressionText(expression));
        }
        public static string FieldIdFor<T, TResult>(this HtmlHelper<T> html, Expression<Func<T, TResult>> expression)
        {
            var id = html.ViewData.TemplateInfo.GetFullHtmlFieldId(ExpressionHelper.GetExpressionText(expression));
            // because "[" and "]" aren't replaced with "_" in GetFullHtmlFieldId
            return id.Replace('[', '_').Replace(']', '_');
        }

        /// <summary>
        /// Creates a days, months, years drop down list using an HTML select control. 
        /// The parameters represent the value of the "name" attribute on the select control.
        /// </summary>
        /// <param name="html">HTML helper</param>
        /// <param name="dayName">"Name" attribute of the day drop down list.</param>
        /// <param name="monthName">"Name" attribute of the month drop down list.</param>
        /// <param name="yearName">"Name" attribute of the year drop down list.</param>
        /// <param name="beginYear">Begin year</param>
        /// <param name="endYear">End year</param>
        /// <param name="selectedDay">Selected day</param>
        /// <param name="selectedMonth">Selected month</param>
        /// <param name="selectedYear">Selected year</param>
        /// <param name="localizeLabels">Localize labels</param>
        /// <returns></returns>
        public static MvcHtmlString DatePickerDropDowns(this HtmlHelper html,
            string dayName, string monthName, string yearName,
            int? beginYear = null, int? endYear = null,
            int? selectedDay = null, int? selectedMonth = null, int? selectedYear = null, bool localizeLabels = true)
        {
            var daysList = new TagBuilder("select");
            var monthsList = new TagBuilder("select");
            var yearsList = new TagBuilder("select");

            daysList.Attributes.Add("name", dayName);
            monthsList.Attributes.Add("name", monthName);
            yearsList.Attributes.Add("name", yearName);

            var days = new StringBuilder();
            var months = new StringBuilder();
            var years = new StringBuilder();

            string dayLocale, monthLocale, yearLocale;
            if (localizeLabels)
            {
                dayLocale = "Day";
                monthLocale = "Month";
                yearLocale = "Year";
                //var locService = new MultiLanguageServices(); // EngineContext.Current.Resolve<ILocalizationService>();
                //dayLocale = locService.GetResource("Common.Day");
                //monthLocale = locService.GetResource("Common.Month");
                //yearLocale = locService.GetResource("Common.Year");
            }
            else
            {
                dayLocale = "Day";
                monthLocale = "Month";
                yearLocale = "Year";
            }

            days.AppendFormat("<option value='{0}'>{1}</option>", "0", dayLocale);
            for (int i = 1; i <= 31; i++)
                days.AppendFormat("<option value='{0}'{1}>{0}</option>", i,
                    (selectedDay.HasValue && selectedDay.Value == i) ? " selected=\"selected\"" : null);


            months.AppendFormat("<option value='{0}'>{1}</option>", "0", monthLocale);
            for (int i = 1; i <= 12; i++)
            {
                months.AppendFormat("<option value='{0}'{1}>{2}</option>",
                                    i, 
                                    (selectedMonth.HasValue && selectedMonth.Value == i) ? " selected=\"selected\"" : null,
                                    CultureInfo.CurrentUICulture.DateTimeFormat.GetMonthName(i));
            }


            years.AppendFormat("<option value='{0}'>{1}</option>", "0", yearLocale);

            if (beginYear == null)
                beginYear = DateTime.UtcNow.Year - 100;
            if (endYear == null)
                endYear = DateTime.UtcNow.Year;

            if (endYear > beginYear)
            {
                for (int i = beginYear.Value; i <= endYear.Value; i++)
                    years.AppendFormat("<option value='{0}'{1}>{0}</option>", i,
                        (selectedYear.HasValue && selectedYear.Value == i) ? " selected=\"selected\"" : null);
            }
            else
            {
                for (int i = beginYear.Value; i >= endYear.Value; i--)
                    years.AppendFormat("<option value='{0}'{1}>{0}</option>", i,
                        (selectedYear.HasValue && selectedYear.Value == i) ? " selected=\"selected\"" : null);
            }

            daysList.InnerHtml = days.ToString();
            monthsList.InnerHtml = months.ToString();
            yearsList.InnerHtml = years.ToString();

            return MvcHtmlString.Create(string.Concat(daysList, monthsList, yearsList));
        }

        public static MvcHtmlString Widget(this HtmlHelper helper, string widgetZone, object additionalData = null)
        {
            return helper.Action("WidgetsByZone", "Widget", new { widgetZone = widgetZone, additionalData = additionalData });
        }

        /// <summary>
        /// Renders the standard label with a specified suffix added to label text
        /// </summary>
        /// <typeparam name="TModel">Model</typeparam>
        /// <typeparam name="TValue">Value</typeparam>
        /// <param name="html">HTML helper</param>
        /// <param name="expression">Expression</param>
        /// <param name="htmlAttributes">HTML attributes</param>
        /// <param name="suffix">Suffix</param>
        /// <returns>Label</returns>
        public static MvcHtmlString LabelFor<TModel, TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression, object htmlAttributes, string suffix)
        {
            string htmlFieldName = ExpressionHelper.GetExpressionText(expression);
            var metadata = ModelMetadata.FromLambdaExpression(expression, html.ViewData);
            string resolvedLabelText = metadata.DisplayName ?? (metadata.PropertyName ?? htmlFieldName.Split(new [] { '.' }).Last());
            if (string.IsNullOrEmpty(resolvedLabelText))
            {
                return MvcHtmlString.Empty;
            }
            var tag = new TagBuilder("label");
            tag.Attributes.Add("for", TagBuilder.CreateSanitizedId(html.ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldId(htmlFieldName)));
            if (!String.IsNullOrEmpty(suffix))
            {
                resolvedLabelText = String.Concat(resolvedLabelText, suffix);
            }
            tag.SetInnerText(resolvedLabelText);

            var dictionary = ((IDictionary<string, object>)HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes));
            tag.MergeAttributes(dictionary, true);

            return MvcHtmlString.Create(tag.ToString(TagRenderMode.Normal));
        }

        #endregion
    }
}

