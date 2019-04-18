using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Controls;
namespace JackWPF
{
    /// <summary>
    /// suppposed to represent wrapper for rectangle, but doesn't 
    /// add enough functionality to be worth using. :( 
    /// </summary>
    class SquareWrapper:ShapeWrapper
    {
        SquareWrapper()
            : base(type.SQUARE, new Rectangle())
        {
            Rectangle r = this.s as Rectangle;
            r.Fill = Brushes.Black;
            r.Stroke = Brushes.Black;
        }
        public string jackPrintRect(Rectangle rec)
        {
            int x = (int)Canvas.GetLeft(rec);
            int y = (int)Canvas.GetTop(rec);
            return "do Screen.drawRectangle(" + x.ToString() + ", " + y.ToString() + ", " + (x + rec.Width).ToString() + ", " + (y + rec.Height).ToString() + ");";
        }
    }
}
