﻿using Microsoft.AspNetCore.Razor.TagHelpers;
using LIM.TextTranslator.Interfaces;

namespace LessIsMoore.Web.Models
{

    [HtmlTargetElement("*", Attributes = "translate-text")]
    public class ProgressBarTagHelper : TagHelper
    {
        //private const string ProgressValueAttributeName = "bs-progress-value";

        /// <summary>
        /// An expression to be evaluated against the current model.
        /// </summary>
        //[HtmlAttributeName(ProgressValueAttributeName)]
        //public int ProgressValue { get; set; }


        ITextTranslator _ITextTranslator;

        public ProgressBarTagHelper(ITextTranslator TextTranslator)
        {
            _ITextTranslator = TextTranslator;
        }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            string strContent = output.GetChildContentAsync().Result.GetContent();

            output.Content.AppendHtml(_ITextTranslator.TranslateText(strContent));

            //string classValue;
            //if (output.Attributes.ContainsName("class"))
            //{
            //    classValue = string.Format("{0} {1}", output.Attributes["class"].Value, "progress");
            //}
            //else
            //{
            //    classValue = "progress";
            //}

            //output.Attributes.SetAttribute("class", classValue);
        }
    }

}