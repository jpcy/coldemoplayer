using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace CDP.Core
{
    public partial class RichTextBox : UserControl
    {
        public FlowDocument Document
        {
            get { return (FlowDocument)GetValue(DocumentProperty); }
            set { SetValue(DocumentProperty, value); }
        }

        public Brush Background
        {
            get { return (Brush)GetValue(BackgroundProperty); }
            set { SetValue(BackgroundProperty, value); }
        }

        public FontFamily FontFamily
        {
            get { return (FontFamily)GetValue(FontFamilyProperty); }
            set { SetValue(FontFamilyProperty, value); }
        }

        public static DependencyProperty DocumentProperty = DependencyProperty.Register("Document", typeof(FlowDocument), typeof(RichTextBox), new PropertyMetadata(OnDocumentChanged));

        public static DependencyProperty BackgroundProperty = DependencyProperty.Register("Background", typeof(Brush), typeof(RichTextBox), new PropertyMetadata(OnBackgroundChanged));

        public static DependencyProperty FontFamilyProperty = DependencyProperty.Register("FontFamily", typeof(FontFamily), typeof(RichTextBox), new PropertyMetadata(OnFontFamilyChanged));

        public RichTextBox()
        {
            InitializeComponent();
        }

        public static void OnDocumentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((RichTextBox)d).childRichTextBox.Document = (FlowDocument)e.NewValue ?? new FlowDocument();
        }

        public static void OnBackgroundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((RichTextBox)d).childRichTextBox.Background = (Brush)e.NewValue;
        }

        public static void OnFontFamilyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((RichTextBox)d).childRichTextBox.FontFamily = (FontFamily)e.NewValue;
        }
    }
}
