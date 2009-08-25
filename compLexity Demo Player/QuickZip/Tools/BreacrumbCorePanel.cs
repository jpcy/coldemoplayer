using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Media.Animation;
using System.Diagnostics;

namespace QuickZip.Tools
{
    public class BreacrumbCorePanel : Panel
    {
        protected override Size MeasureOverride(Size availableSize)
        {
            Size resultSize = new Size(0, 0);

            foreach (UIElement child in Children)
            {
                //Fix:This is a temp fix for ArithException when loading icons for Mds files.
                try { child.Measure(availableSize); }
                catch { }
                resultSize.Width += child.DesiredSize.Width;
                resultSize.Height = Math.Max(resultSize.Height, child.DesiredSize.Height);
            }

            resultSize.Width =
                double.IsPositiveInfinity(availableSize.Width) ?
                resultSize.Width : availableSize.Width;
            resultSize.Width = Math.Min(resultSize.Width, availableSize.Width);

            return resultSize;
        }

        private List<UIElement> GhostFolders = new List<UIElement>();

        protected override Size ArrangeOverride(Size finalSize)
        {
            foreach (UIElement g in GhostFolders)  //03-01-09 : Dirty Ghost Folder Fix.
                g.IsHitTestVisible = true;
            GhostFolders.Clear();

            if (this.Children == null || this.Children.Count == 0)
                return finalSize;

            double finalWidth = finalSize.Width - 30;
            double totalX = 0;

            for (int i = Children.Count - 1; i >= 0; i--)
                if (totalX + Children[i].DesiredSize.Width <= finalWidth || i == Children.Count - 1)
                {
                    totalX += Children[i].DesiredSize.Width;
                }
                else break;
            
            double curX = Math.Min(totalX, finalWidth) + 1;            

            for (int i = Children.Count - 1; i >= 0; i--)
            {
                UIElement child = Children[i];
                if (curX >= child.DesiredSize.Width || i == Children.Count -1)
                {                    
                    if (curX < child.DesiredSize.Width)
                        child.Arrange(new Rect(0, 0, curX, child.DesiredSize.Height));
                    else
                    child.Arrange(new Rect(curX - child.DesiredSize.Width, 0, child.DesiredSize.Width, finalSize.Height));
                    child.IsHitTestVisible = true;
                }
                else
                {                    
                    for (int j = 0; j <= i; j++)
                    {
                        child = Children[j];
                        child.Arrange(new Rect(0, 0, 0, 0));
                        GhostFolders.Add(child);
                        child.IsHitTestVisible = false;
                    }
                    break;
                }                        

                curX -= child.DesiredSize.Width;
            }

            return finalSize;
        }
    }
}
