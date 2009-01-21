using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Documents;
using System.Collections;
using System.Xml;
using System.IO;
using System.Windows.Threading;

namespace compLexity_Demo_Player
{
    public interface IAnalysisWindow
    {
        void GameLogWrite(String text);
        void GameLogWrite(String text, SolidColorBrush brush);
        void GameLogWrite(String text, SolidColorBrush brush, TextDecorationCollection decorations);
        void ParsingFinished(Object playersItemSource, Object scoreboardRoundsItemSource, Object networkGraphPlayersItemSource);
    }

    public partial class AnalysisWindow : IAnalysisWindow
    {
        public void GameLogWrite(String text)
        {
            GameLogWrite(text, Brushes.Black);
        }

        public void GameLogWrite(String text, SolidColorBrush brush)
        {
            GameLogWrite(text, brush, null);
        }

        public void GameLogWrite(String text, SolidColorBrush brush, TextDecorationCollection decorations)
        {
            // FIXME: probably more invalid characters...
            text = text.Replace((Char)0x17, ' ');
            //text = System.Security.SecurityElement.Escape(text).Replace((Char)0x17, ' ');

            XmlElement run = GameLogCreateXmlElement("Run");
            run.InnerText = text;

            Procedure<String, String> addXmlAttribute = (name, value) =>
            {
                XmlAttribute attribute = gameLog.CreateAttribute(name);
                attribute.InnerXml = value;
                run.Attributes.Append(attribute);
            };

            if (brush != Brushes.Black)
            {
                addXmlAttribute("Foreground", brush.ToString());
            }

            if (decorations != null)
            {
                // TODO: fixme for multiple attributes, what's the XML for that?
                addXmlAttribute("TextDecorations", decorations[0].Location.ToString());
            }

            gameLogParagraph.AppendChild(run);
        }

        public void ParsingFinished(Object playersItemSource, Object scoreboardRoundsItemSource, Object networkGraphPlayersItemSource)
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, new Procedure(() =>
            {
                progressWindowInterface.CloseWithResult(true);

                using (MemoryStream ms = new MemoryStream())
                {
                    gameLog.Save(ms);

                    TextRange textRange = new TextRange(uiGameLogRichTextBox.Document.ContentStart, uiGameLogRichTextBox.Document.ContentEnd);
                    textRange.Load(ms, DataFormats.Xaml);
                }

                uiPlayersListView.ItemsSource = playersItemSource as IEnumerable;
                uiScoreboardRoundsListView.ItemsSource = scoreboardRoundsItemSource as IEnumerable;
                uiNetworkGraphPlayersListView.ItemsSource = networkGraphPlayersItemSource as IEnumerable;

                uiPlayersListView.Sort();
                uiScoreboardRoundsListView.Sort();
                uiNetworkGraphPlayersListView.Sort();
            }));
        }
    }
}
