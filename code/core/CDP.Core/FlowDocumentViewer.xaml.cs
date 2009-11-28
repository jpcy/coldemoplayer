using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace CDP.Core
{
    public partial class FlowDocumentViewer : UserControl
    {
        public FlowDocument Document
        {
            get { return (FlowDocument)GetValue(DocumentProperty); }
            set { SetValue(DocumentProperty, value); }
        }

        public new Brush Background
        {
            get { return (Brush)GetValue(BackgroundProperty); }
            set { SetValue(BackgroundProperty, value); }
        }

        public new FontFamily FontFamily
        {
            get { return (FontFamily)GetValue(FontFamilyProperty); }
            set { SetValue(FontFamilyProperty, value); }
        }

        public new double FontSize
        {
            get { return (double)GetValue(FontSizeProperty); }
            set { SetValue(FontSizeProperty, value); }
        }

        public static DependencyProperty DocumentProperty = DependencyProperty.Register("Document", typeof(FlowDocument), typeof(FlowDocumentViewer), new PropertyMetadata(OnDocumentChanged));

        public static new DependencyProperty BackgroundProperty = DependencyProperty.Register("Background", typeof(Brush), typeof(FlowDocumentViewer));

        public static new DependencyProperty FontFamilyProperty = DependencyProperty.Register("FontFamily", typeof(FontFamily), typeof(FlowDocumentViewer));

        public static new DependencyProperty FontSizeProperty = DependencyProperty.Register("FontSize", typeof(double), typeof(FlowDocumentViewer));

        public FlowDocumentViewer()
        {
            InitializeComponent();
        }

        public static void OnDocumentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            FlowDocumentViewer viewer = (FlowDocumentViewer)d;
            FlowDocument document = (FlowDocument)e.NewValue ?? new FlowDocument();
            document.Background = viewer.Background ?? document.Background;
            document.FontFamily = viewer.FontFamily ?? document.FontFamily;
            document.FontSize = viewer.FontSize == 0 ? 14 : viewer.FontSize;
            document.PagePadding = new Thickness(4);
            viewer.scrollViewer.Document = document;
        }
    }
}
