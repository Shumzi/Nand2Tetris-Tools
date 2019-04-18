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

namespace JackWPF.Shapes
{
    public class myLine : Shapes.shapeCrap
    {
        Line li;
        
        public myLine()
        {
            li = new Line();
            li.StrokeThickness = 1;
            li.Stroke = Brushes.Black;
            li.ClipToBounds = true;
        }
        public override void  setCoordinates(System.Drawing.Point p1, System.Drawing.Point p2)
        {
            li.X1 = p1.X;
            li.X2 = p2.X;
            li.Y1 = p1.Y;
            li.Y2 = p2.Y;
        }
        public override void setStart(System.Drawing.Point p)
        {
            li.X1 = p.Y;
            li.Y1 = p.Y;
        }
        public override void setEnd(System.Drawing.Point p)
        {
            li.X2 = p.Y;
            li.Y2 = p.Y;
        }
        public override type getType()
        {
            return type.LINE;
        }
    }
}
