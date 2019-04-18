using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Shapes;

namespace JackWPF
{
    /// <summary>
    /// Container for Shapes, to include their type as well as shape.
    /// possible option to add functions or virtual functions to class. 
    /// </summary>
    public class ShapeWrapper
    {
        public type t { get; set; }
        public Shape s { get; set; }
        public ShapeWrapper(type t, Shape s)
        {
            this.t = t;
            this.s = s;
        }
    }
}
