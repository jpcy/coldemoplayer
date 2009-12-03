using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;

namespace CDP.Gui.CustomControls
{
    /// <summary>
    /// A read-only textbox for displaying messages to the user. Optional background image.
    /// </summary>
    internal class MessageTextBox : TextBox
    {
        public static DependencyProperty BackgroundImageSourceProperty;

        public ImageSource BackgroundImageSource
        {
            get { return (ImageSource)GetValue(BackgroundImageSourceProperty); }
            set { SetValue(BackgroundImageSourceProperty, value); }
        }

        static MessageTextBox()
        {
            BackgroundImageSourceProperty = DependencyProperty.Register("BackgroundImageSource", typeof(ImageSource), typeof(MessageTextBox));   
        }
    }
}
