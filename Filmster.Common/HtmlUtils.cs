using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace Filmster.Common
{
    public class HtmlUtils
    {
        private HtmlUtils() { }

        /// <summary>
        /// Reorders the child nodes of the head element, so page tile, description and keywords comes first. This is good SEO practice.
        /// </summary>
        /// <param name="html">The html document to perform the sorting on</param>
        /// <returns>An html document with reordered head child nodes</returns>
        public static string ReorderHeadChildNodes(string html)
        {
            int headStartIndex = html.IndexOf("<head>");
            int headEndIndex = html.IndexOf("</head>");
            if (headStartIndex >= 0 && headEndIndex >= 0)
            {
                string head = html.Substring(headStartIndex, headEndIndex - headStartIndex + 7);

                XmlDocument doc = new XmlDocument();
                doc.LoadXml(head);
                XmlElement root = doc.DocumentElement;
                if (root != null)
                {
                    XmlNode titleNode = root.SelectSingleNode("title");
                    XmlNode descriptionNode = root.SelectSingleNode("meta[@name='description']");
                    XmlNode keywordsNode = root.SelectSingleNode("meta[@name='keywords']");

                    if (titleNode != null) root.RemoveChild(titleNode);
                    if (descriptionNode != null) root.RemoveChild(descriptionNode);
                    if (keywordsNode != null) root.RemoveChild(keywordsNode);

                    if (keywordsNode != null) root.PrependChild(keywordsNode);
                    if (descriptionNode != null) root.PrependChild(descriptionNode);
                    if (titleNode != null) root.PrependChild(titleNode);

                    StringBuilder stringBuilder = new StringBuilder();
                    XmlWriter xmlWriter = XmlWriter.Create(stringBuilder);

                    root.WriteTo(xmlWriter);
                    if (xmlWriter != null) xmlWriter.Flush();

                    html = html.Remove(headStartIndex, headEndIndex - headStartIndex + 7);
                    html = html.Insert(headStartIndex, stringBuilder.ToString());
                }
            }
            return html;
        }

        /// <summary>
        /// Removes white space from the passed in html document making the document smaller
        /// </summary>
        /// <param name="html">The html document to remove white space from</param>
        /// <returns>An html document without useless whitespace</returns>
        public static string Minify(string html)
        {
            Regex reg = new Regex(@"(?<=[^])\t{2,}|(?<=[>])\s{2,}(?=[<])|(?<=[>])\s{2,11}(?=[<])|(?=[\n])\s{2,}");
            return reg.Replace(html, string.Empty);
        }

        /// <summary>
        /// Moves the ASP.NET viewstate field to the bottom of the page. This is good SEO practice.
        /// </summary>
        /// <param name="html">The html document to perform re-ordering on</param>
        /// <returns>An html document that has the viewstate field as the last child node of the body tag.</returns>
        public static string MoveViewstateToBottom(string html)
        {
            string s = html;
            int startIndex = html.IndexOf("<input type=\"hidden\" name=\"__VIEWSTATE\"");
            if (startIndex >= 0)
            {
                int endIndex = html.IndexOf("/>", startIndex) + 2;
                string viewstateInput = html.Substring(startIndex, endIndex - startIndex);
                html = html.Remove(startIndex, endIndex - startIndex);
                int formEndStart = html.IndexOf("</form>");
                if (formEndStart >= 0)
                {
                    s = html.Insert(formEndStart, viewstateInput);
                }
            }
            return s;
        }
    }
}