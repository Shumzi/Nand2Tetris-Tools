using System;
using System.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Shapes;

namespace JackWPF.Shapes
{

    public abstract class shapeCrap : UIElement 
    {
        public abstract void setCoordinates(System.Drawing.Point p1, System.Drawing.Point p2);
        public abstract void setStart(System.Drawing.Point p);
        public abstract void setEnd(System.Drawing.Point p);
        public abstract type getType();
    }
}
