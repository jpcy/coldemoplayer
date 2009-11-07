using System;
using System.Xml;
using System.Windows.Media;
using System.Windows;
using System.Windows.Documents;
using System.IO;

namespace CDP.Core
{
    public interface IFlowDocumentWriter
    {
        void Write(string text);
        void Write(string text, SolidColorBrush brush);
        void Write(string text, SolidColorBrush brush, TextDecorationCollection decorations);
        void WriteLine();
        void WriteLine(string text);
        void WriteLine(string text, SolidColorBrush brush);
        void WriteLine(string text, SolidColorBrush brush, TextDecorationCollection decorations);
        void Save(FlowDocument flowDocument);
    }

    public class FlowDocumentWriter : IFlowDocumentWriter
    {
        private readonly XmlDocument document;
        private readonly XmlElement paragraph;
        private readonly string namespaceUri = @"http://schemas.microsoft.com/winfx/2006/xaml/presentation";

        public FlowDocumentWriter()
        {
            document = new XmlDocument() { PreserveWhitespace = true };

            // Create root element.
            XmlElement root = document.CreateElement("Section", namespaceUri);
            document.AppendChild(root);
            AddXmlAttribute(root, "xml:space", "preserve");

            // Create paragraph element.
            paragraph = document.CreateElement("Paragraph", namespaceUri);
            root.AppendChild(paragraph);
        }

        public void Write(string text)
        {
            Write(text, null);
        }

        public void Write(string text, SolidColorBrush brush)
        {
            Write(text, brush, null);
        }

        public void Write(string text, SolidColorBrush brush, TextDecorationCollection decorations)
        {
            if (text == null)
            {
                throw new ArgumentNullException("text");
            }

            XmlElement run = document.CreateElement("Run", namespaceUri);
            run.InnerText = ValidateString(text);

            if (brush != null)
            {
                AddXmlAttribute(run, "Foreground", brush.ToString());
            }

            if (decorations != null)
            {
                // TODO: fixme for multiple attributes, what's the XML for that?
                AddXmlAttribute(run, "TextDecorations", decorations[0].Location.ToString());
            }

            paragraph.AppendChild(run);
        }

        public void WriteLine()
        {
            Write(Environment.NewLine);
        }

        public void WriteLine(string text)
        {
            Write(text + Environment.NewLine, null);
        }

        public void WriteLine(string text, SolidColorBrush brush)
        {
            Write(text + Environment.NewLine, brush, null);
        }

        public void WriteLine(string text, SolidColorBrush brush, TextDecorationCollection decorations)
        {
            Write(text + Environment.NewLine, brush, decorations);
        }

        public void Save(FlowDocument flowDocument)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                document.Save(ms);
                TextRange textRange = new TextRange(flowDocument.ContentStart, flowDocument.ContentEnd);
                textRange.Load(ms, DataFormats.Xaml);
            }
        }

        private void AddXmlAttribute(XmlElement element, string attributeName, string attributeValue)
        {
            XmlAttribute attribute = document.CreateAttribute(attributeName);
            attribute.InnerXml = attributeValue;
            element.Attributes.Append(attribute);
        }

        /// <summary>
        /// Removes invalid characters from a string.
        /// </summary>
        /// <param name="s">The string to remove invalid characters from.</param>
        /// <returns>A new string with invalid characters removed.</returns>
        private string ValidateString(string s)
        {
            int nInvalidChars = 0;

            foreach (char ch in s)
            {
                if (!IsValidChar(ch))
                {
                    nInvalidChars++;
                }
            }

            if (nInvalidChars == 0)
            {
                return s;
            }

            int newLength = s.Length - nInvalidChars;

            if (newLength <= 0)
            {
                return string.Empty;
            }

            char[] result = new char[newLength];
            int i = 0;

            foreach (char ch in s)
            {
                if (IsValidChar(ch))
                {
                    result[i] = ch;
                    i++;
                }
            }

            return new string(result);
        }

        // http://www.w3.org/TR/REC-xml/#charsets
        private bool IsValidChar(char ch)
        {
            return (ch == 0x9 || ch == 0xA || ch == 0xD || (ch >= 0x20 && ch <= 0xD7FF) || (ch >= 0xE000 && ch <= 0xFFFD));
        }
    }
}
