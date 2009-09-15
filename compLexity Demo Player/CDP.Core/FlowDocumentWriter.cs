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
            Write(text, Brushes.Black);
        }

        public void Write(string text, SolidColorBrush brush)
        {
            Write(text, brush, null);
        }

        public void Write(string text, SolidColorBrush brush, TextDecorationCollection decorations)
        {
            // FIXME: probably more invalid characters...
            text = text.Replace((Char)0x17, ' ');
            //text = System.Security.SecurityElement.Escape(text).Replace((Char)0x17, ' ');

            XmlElement run = document.CreateElement("Run", namespaceUri);
            run.InnerText = text;

            if (brush != Brushes.Black)
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
    }
}
