using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace CDP.Core
{
    public partial class RichTextBox : UserControl
    {
        public FlowDocument Document
        {
            get { return (FlowDocument)GetValue(DocumentProperty); }
            set { SetValue(DocumentProperty, value); }
        }

        public static DependencyProperty DocumentProperty = DependencyProperty.Register("Document", typeof(FlowDocument), typeof(RichTextBox), new PropertyMetadata(OnDocumentChanged));

        public RichTextBox()
        {
            InitializeComponent();
        }

        public static void OnDocumentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((RichTextBox)d).childRichTextBox.Document = (FlowDocument)e.NewValue ?? new FlowDocument();
        }
    }
}
