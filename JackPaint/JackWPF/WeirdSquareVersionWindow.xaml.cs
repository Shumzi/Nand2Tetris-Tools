using System;
using System.IO;
using System.Reflection;
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

namespace JackWPF
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        const int canvXSize = 511;
        const int canvYSize = 255;
        const int LEFT = 0, RIGHT = 1, TOP = 0, BOTTOM = 1;
        #region variables
        //private string jackPath = @"./MyTest.jack";
        private bool isDragging;
        private bool isWhite = false;
        Point startPoint = new Point();
        List<ShapeWrapper> shps = new List<JackWPF.ShapeWrapper>();
        int scnt = -1;
        type t = type.PEN;
        Thickness[,] locationOfStart;
        #endregion

        public Window1()
        {
            this.ResizeMode = ResizeMode.NoResize;
            InitializeComponent();
            locationOfStart = new Thickness[2,2];
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch(e.Key)
            {
                case Key.W:
                    this.isWhite = !this.isWhite;
                    this.isWhiteCheckBox.IsChecked = !this.isWhiteCheckBox.IsChecked;
                    break;
                case Key.U:
                    if (!shps.Any())
                    return;
                    canv.Children.Remove(shps.Last().s);
                    this.shps.RemoveAt(scnt);
                    --scnt;
                    break;
                //case Key.Left:
                //    if (!shps.Any()) return;
                //    //shps.Last().s
                //    break;
            }
            
        }

        #region canvas
        /// <summary>
        /// create appropriate shape, add to wrappershape list, and set starting point for shape.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void canv_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            this.startPoint.X = e.GetPosition(canv).X;
            this.startPoint.Y = (int)e.GetPosition(canv).Y;
            switch (t)
            {
                case type.PEN:
                    
                    break;
                case type.LINE:
                    addLine();
                    Line myLine = shps[scnt].s as Line;
                    myLine.Stroke = isWhite ? Brushes.White : Brushes.Black;
                    canv.Children.Add(myLine);
                    myLine.X1 = startPoint.X;
                    myLine.Y1 = startPoint.Y;
                    myLine.X2 = startPoint.X;
                    myLine.Y2 = startPoint.Y;
                    break;
                case type.SQUARE:
                    addSquare();
                    Rectangle rec = shps[scnt].s as Rectangle;
                    rec.Fill = isWhite ? Brushes.White : Brushes.Black;
                    canv.Children.Add(rec);
                    //locationOfStart[LEFT,TOP] = new Thickness(startPoint.X, startPoint.Y, 0, 0);//
                    //locationOfStart[RIGHT,TOP] = new Thickness(0, startPoint.Y, canvXSize - startPoint.X, 0);
                    //locationOfStart[LEFT,BOTTOM] = new Thickness(startPoint.X, 0, 0, canvYSize - startPoint.Y);
                    //locationOfStart[RIGHT,BOTTOM] = new Thickness(0, 0, canvXSize - startPoint.X, canvYSize - startPoint.Y);
                    //rec.Margin = locationOfStart[LEFT,TOP];
                    Canvas.SetLeft(rec, startPoint.X);
                    Canvas.SetTop(rec, startPoint.Y);
                    rec.Height = 0;
                    rec.Width = 0;
                    break;
                case type.CIRCLE:
                    addCircle();
                    Ellipse el = shps[scnt].s as Ellipse;
                    el.Fill = isWhite ? Brushes.White : Brushes.Black;
                    el.Height = 0;
                    el.Width = 0;
                    Canvas.SetLeft(el, startPoint.X);
                    Canvas.SetTop(el, startPoint.Y);
                    canv.Children.Add(el);
                    break;
            }
            print(startPoint.ToString());
            if (canv.CaptureMouse())
                print("capturedMouse");
            isDragging = true;
        }

        /// <summary>
        /// readjust the shape according to current mouse position.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void canv_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (isDragging)
            {
                Point end = new Point();
                end.X = e.GetPosition(canv).X;
                end.Y = e.GetPosition(canv).Y;
                switch (t)
                {
                    case type.PEN:
                        break;
                    case type.LINE:
                        Line l = shps[scnt].s as Line;
                        l.X2 = (int)end.X;
                        l.Y2 = (int)end.Y;
                        break;
                    case type.SQUARE:
                        Rectangle rec = shps[scnt].s as Rectangle;
                        //set margin to appropriate margin.
                        //rec.Margin = locationOfStart[startPoint.X < end.X?LEFT:RIGHT,startPoint.Y<end.Y?BOTTOM:TOP];
                        rec.Width = (int)Math.Abs(end.X - startPoint.X);
                        rec.Height = (int)Math.Abs(end.Y - startPoint.Y);
                        break;
                    case type.CIRCLE:
                        Ellipse el = shps[scnt].s as Ellipse;
                        int distance = (((int)Math.Sqrt(Math.Pow(end.X - startPoint.X,2)+Math.Pow(end.Y - startPoint.Y,2)))/2)*2;
                        el.Width = distance;
                        el.Height = distance;
                        break;
                }
            }
        }
        /// <summary>
        /// lock shape on screen and move to next shape in vector for next use.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void canv_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (isDragging)
            {
                print("released at: " + loc(e));
                switch (t)
                {
                    case type.PEN:
                        break;
                    case type.LINE:
                        Line ltmp = (shps.Last().s as Line);
                        if (ltmp.X2 > canvXSize)
                            ltmp.X2 = canvXSize;
                        if (ltmp.Y2 > canvYSize)
                            ltmp.Y2 = canvYSize;
                        break;
                    case type.SQUARE:
                        Rectangle r = (shps.Last().s as Rectangle);
                        int height = (int)(Canvas.GetTop(r)+r.Height);
                        int width = (int)(Canvas.GetLeft(r) + r.Width);
                        if (width > canvXSize)
                            r.Width = (int)(canvXSize - Canvas.GetLeft(r));
                        if (height > canvYSize)
                            r.Height = (int)(canvYSize - Canvas.GetTop(r));
                        break;
                    case type.CIRCLE:

                        break;
                }
                canv.ReleaseMouseCapture();
                isDragging = false;
            }
        }

        #endregion

        #region getstuff_addstuff
        private Line getLine()
        {
            Line myLine = new Line();
            myLine.StrokeThickness = 1;
            myLine.Stroke = Brushes.Black;
            myLine.ClipToBounds = true;
            return myLine;
        }
        private Rectangle getSquare()
        {
            Rectangle r = new Rectangle();
            r.Fill = Brushes.Black;
            return r;
        }


        private void addLine()
        {
            ++scnt;
            ShapeWrapper sw = new ShapeWrapper(type.LINE, getLine());
            shps.Add(sw);
        }

        private void addSquare()
        {
            ++scnt;
            ShapeWrapper sw = new ShapeWrapper(type.SQUARE, getSquare());
            shps.Add(sw);
        }

        private void addCircle()
        {
            ++scnt;
            ShapeWrapper sw = new ShapeWrapper(type.CIRCLE, new Ellipse());
            shps.Add(sw);
        }
        #endregion

        #region buttons_and_checkboxes
        private void isWhiteCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            isWhite = true;
            print("iswhite == true");
        }

        private void isWhiteCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            isWhite = false;
            print("iswhite == false");
        }

        private void penButton_Click(object sender, RoutedEventArgs e)
        {
            this.t = type.PEN;
            notifyTypeChange(t.ToString());
        }

        private void lineButton_Click(object sender, RoutedEventArgs e)
        {
            this.t = type.LINE;
            notifyTypeChange(t.ToString());
        }

        private void squareButton_Click(object sender, RoutedEventArgs e)
        {
            this.t = type.SQUARE;
            notifyTypeChange(t.ToString());
        }

        private void circleButton_Click(object sender, RoutedEventArgs e)
        {
            this.t = type.CIRCLE;
            notifyTypeChange(t.ToString());
        }

        private void isGrayCanvas_Checked(object sender, RoutedEventArgs e)
        {
            canv.Background = Brushes.LightGray;
        }

        private void isGrayCanvas_Unchecked(object sender, RoutedEventArgs e)
        {
            canv.Background = Brushes.White;
        }

        #endregion

        #region print_and_export

        private void notifyTypeChange(string type)
        {
            print("type of printing: " + type);
        }

        private string loc(System.Windows.Input.MouseButtonEventArgs e)
        {
            return (e.GetPosition(canv).X.ToString() + "," + e.GetPosition(canv).Y.ToString());
        }

        private void print(string text)
        {
            debugTextBox.AppendText(Environment.NewLine + text);
            debugTextBox.ScrollToEnd();
        }

        private void exportButton_Click(object sender, RoutedEventArgs e)
        {
            clearDebugTextBox();
            bool toggleBW = true;
            Brush b = Brushes.Black;

            foreach (ShapeWrapper sw in shps)
            {
                // need to switch
                switch (sw.t)
                {
                    case type.PEN:
                        //e.GetPosition(canv).X;
                        break;
                    case type.LINE:
                        Line l = sw.s as Line;
                        maybeToggleColor(l.Stroke, ref b, ref toggleBW);
                        print(jackPrintLine(l));
                        printToFile(jackPrintLine(l));
                        break;
                    case type.SQUARE:
                        Rectangle r = sw.s as Rectangle;
                        maybeToggleColor(r.Fill, ref b, ref toggleBW);
                        printToFile(jackPrintRect(r));
                        print(jackPrintRect(r));
                        break;
                    case type.CIRCLE:
                        Ellipse el = sw.s as Ellipse;
                        maybeToggleColor(el.Fill, ref b, ref toggleBW);
                        //printToFile(jackPrintRect(r));
                        print(jackPrintCircle(el));
                        break;
                }
            }
            Clipboard.SetText(debugTextBox.Text);
            codeCopiedLabel.Visibility = Visibility.Visible;
        }



        private void maybeToggleColor(Brush brush, ref Brush b, ref bool toggleBW)
        {
            if (brush != b)
            {
                toggleBW = !toggleBW;
                b = toggleBW ? Brushes.Black : Brushes.White;
                print("do Screen.setColor(" + toggleBW.ToString().ToLower() + ");");
                printToFile("do Screen.setColor(" + toggleBW.ToString().ToLower() + ");");
            }
        }

        private string jackPrintLine(Line line)
        {

            return "do Screen.drawLine(" + line.X1.ToString() + ", " + line.Y1.ToString() + ", " + line.X2.ToString() + ", " + line.Y2.ToString() + ");";
        }

        private string jackPrintRect(Rectangle rec)
        {
            int x = (int)Canvas.GetLeft(rec);
            int y = (int)Canvas.GetTop(rec);
            return "do Screen.drawRectangle(" + x.ToString() + ", " + y.ToString() + ", " + (x+rec.Width).ToString() + ", " + (y+rec.Height).ToString() + ");";
        }


        private string jackPrintCircle(Ellipse el)
        {
            int x = (int)(Canvas.GetLeft(el) + (el.Width / 2));
            int y = (int)(Canvas.GetTop(el) + (el.Height / 2));
            return "do Screen.drawCircle(" + x.ToString() + ", " + y.ToString() + ", " + (el.Width/2).ToString() + ");";
        }

        private void printToFile(string text)
        {
            //if (!File.Exists(jackPath))
            //{
            //    // Create a file to write to.
            //    using (StreamWriter sw = File.CreateText(jackPath))
            //    {
            //        sw.WriteLine(text);
            //    }
            //}
            //else
            //{
            //    using (StreamWriter sw = File.AppendText(jackPath))
            //    {
            //        sw.WriteLine(text);
            //    }
            //}
        }

        #endregion

        private void clearScreenButton_Click(object sender, RoutedEventArgs e)
        {
            canv.Children.Clear();
            shps.Clear();
            scnt = -1;
        }

        private void clearDebugTextBox()
        {
            this.debugTextBox.Clear();
        }




    }
}
