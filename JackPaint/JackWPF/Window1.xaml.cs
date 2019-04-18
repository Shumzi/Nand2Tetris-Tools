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
        #region variables
        const int canvXSize = 511;
        const int canvYSize = 255;
        const int LEFT = 0, RIGHT = 1, TOP = 0, BOTTOM = 1;
        private bool isDragging;
        private bool isWhite = false;
        Point startPoint = new Point();
        List<ShapeWrapper> shps = new List<JackWPF.ShapeWrapper>();
        type t = type.PEN;
        Thickness[,] locationOfStart;
        Stack<int> penUndoCount = new Stack<int>();// count no. of pixel objects made, in case we want to do undo. (list if we do multiple pen drawings
        #endregion

        public Window1()
        {
            this.ResizeMode = ResizeMode.NoResize;
            InitializeComponent();
            locationOfStart = new Thickness[2, 2];
        }


        #region User Input

        /// <summary>
        /// Do action of respective hotkey shown in GUI.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.E:
                    this.isWhite = !this.isWhite;
                    this.isWhiteCheckBox.IsChecked = !this.isWhiteCheckBox.IsChecked;
                    break;
                default:    // if at all, keys that depend on the existence of a shape on screen.
                    if (!shps.Any()) // if no object exists, done.
                        break;
                    ShapeWrapper last = shps.Last();
                    switch (e.Key)
                    {
                        case Key.U:
                            if (!shps.Any())
                                return;
                            if (last.t == type.PEN) // then delete scribble.
                            {
                                int size = penUndoCount.Peek();
                                for (int i = 0; i < size; ++i)
                                {
                                    removeLastShape();
                                }
                                penUndoCount.Pop();
                            }
                            else
                            {
                                removeLastShape();
                            }
                            break;
                        // move object in direction (doesn't apply for pen tool).
                        // didn't use arrows bc interferes with buttons and crap.
                        case Key.A://left
                            moveShape(last, -1, 0);
                            break;
                        case Key.D://right
                            moveShape(last, 1, 0);
                            break;
                        case Key.W://up
                            moveShape(last, 0, -1);
                            break;
                        case Key.S://down
                            moveShape(last, 0, 1);
                            break;
                        case Key.Q:
                            incDecShapeSize(last, 1);
                            break;
                        case Key.Z:
                            incDecShapeSize(last, -1);
                            break;
                    }
                    break;

            }
        }
        
        /// <summary>
        /// create appropriate shape, add to wrappershape list, and set starting point for shape.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// 
        private void canv_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            this.startPoint.X = e.GetPosition(canv).X;
            this.startPoint.Y = (int)e.GetPosition(canv).Y;
            switch (t)
            {
                case type.PEN:
                    penUndoCount.Push(0);    // add new penCounter
                    addDotToShapesList();
                    Rectangle dot = shps.Last().s as Rectangle;
                    Canvas.SetLeft(dot, startPoint.X);
                    Canvas.SetTop(dot, startPoint.Y);
                    dot.Height = 1;
                    dot.Width = 1;
                    canv.Children.Add(dot);
                    break;
                case type.LINE:
                    addLineToShapesList();
                    Line myLine = shps.Last().s as Line;
                    
                    canv.Children.Add(myLine);
                    myLine.X1 = startPoint.X;
                    myLine.Y1 = startPoint.Y;
                    myLine.X2 = startPoint.X;
                    myLine.Y2 = startPoint.Y;
                    break;
                case type.SQUARE:
                    addSquare();
                    Rectangle rec = shps.Last().s as Rectangle;
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
                    Ellipse el = shps.Last().s as Ellipse;
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
        /// if mouse is out of bounds, will stick to end of canvas.
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
                // deny out of bounds problems.
                if (end.X < 0)
                {
                    end.X = 0;
                }
                else if (end.X > canvXSize)
                {
                    end.X = canvXSize;
                }
                if (end.Y < 0)
                {
                    end.Y = 0;
                }
                else if (end.Y > canvYSize)
                {
                    end.Y = canvYSize;
                }
                switch (t)
                {
                    case type.PEN:
                        Rectangle lastDot = null;
                        Line lastLine = null;
                        if (shps.Any()) // get last dot/line for reference.
                        {
                            lastDot = shps.Last().s as Rectangle;
                            lastLine = shps.Last().s as Line;
                        }
                        Rectangle dot = addDotToShapesList();
                        Canvas.SetLeft(dot, end.X);
                        Canvas.SetTop(dot, end.Y);
                        dot.Height = 1;
                        dot.Width = 1;
                        if ((lastDot != null && isNewDotTooFarAway(lastDot, dot)))   // if last added object is too far away (interrupted to late), swap for a connecting line.
                        {
                            Line fillLine = getLine();
                            fillLine.X1 = Canvas.GetLeft(lastDot);
                            fillLine.Y1 = Canvas.GetTop(lastDot);
                            fillLine.X2 = Canvas.GetLeft(dot);
                            fillLine.Y2 = Canvas.GetTop(dot);
                            addPenLine(fillLine);
                        }
                        else if (lastLine != null && isNewDotTooFarAwayFromLine(lastLine, dot))
                        {
                            Line fillLine = getLine();
                            fillLine.X1 = lastLine.X2;
                            fillLine.Y1 = lastLine.Y2;
                            fillLine.X2 = Canvas.GetLeft(dot);
                            fillLine.Y2 = Canvas.GetTop(dot);
                            addPenLine(fillLine);
                        }
                        else
                            canv.Children.Add(dot);
                        break;
                    case type.LINE:
                        Line l = shps.Last().s as Line;
                        l.X2 = (int)end.X;
                        l.Y2 = (int)end.Y;
                        break;
                    case type.SQUARE:
                        Rectangle rec = shps.Last().s as Rectangle;
                        rec.Width = (int)Math.Abs(end.X - startPoint.X);
                        rec.Height = (int)Math.Abs(end.Y - startPoint.Y);
                        if (end.X < startPoint.X)//left
                        {
                            Canvas.SetLeft(rec,end.X);
                        }
                        if (end.Y < startPoint.Y)
                        {
                            Canvas.SetTop(rec, end.Y);
                        }
                        break;
                    case type.CIRCLE:
                        Ellipse el = shps.Last().s as Ellipse;
                        int distance = (int)(Math.Sqrt((Math.Pow(end.X - startPoint.X, 2) + Math.Pow(end.Y - startPoint.Y, 2))/2));
                        //int distance = (int)Math.Abs(end.X - startPoint.X);
                        el.Width = distance;
                        el.Height = distance;
                        // testing to see that circle doen't go out of bounds (pain in the ass).
                        int minSize = distance;  // min size that will be ok. (start above possible size).
                        if (Canvas.GetTop(el)+el.Width > canvYSize) 
                            minSize = (int)(canvYSize - Canvas.GetTop(el)); // too far out (bc circle is weird), i.e. bad Y
                        if (Canvas.GetTop(el) < 0 && Canvas.GetTop(el) + el.Width < minSize) 
                            minSize = (int)Canvas.GetBottom(el);
                        if (Canvas.GetLeft(el) < 0 && Canvas.GetLeft(el) + el.Width < minSize)
                            minSize = (int)(Canvas.GetLeft(el) + el.Width);
                        if (Canvas.GetLeft(el)+el.Width > canvXSize && canvXSize - Canvas.GetLeft(el) < minSize)
                            minSize = (int)(canvXSize - Canvas.GetLeft(el));
                        if (minSize != distance)
                            el.Width = el.Height = minSize;
                        if (end.X < startPoint.X)//left
                        {
                            Canvas.SetLeft(el, end.X);
                        }
                        if (end.Y < startPoint.Y)
                        {
                            Canvas.SetTop(el, end.Y);
                        }
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
                Point end = new Point();
                end.X = (int)e.GetPosition(canv).X;
                end.Y = (int)e.GetPosition(canv).Y;
                if (end.X == startPoint.X && end.Y == startPoint.Y) // accidental click.
                {
                    print("accidental click");
                    removeLastShape();
                }
                canv.ReleaseMouseCapture();
                isDragging = false;
            }
        }

        #endregion

        #region transform existing Object
        private void incDecShapeSize(ShapeWrapper last, int p)
        {
            if (last.t == type.PEN || last.t == type.LINE)
                return;
            last.s.Height += p;
            last.s.Width += p;
        }

        private void moveShape(ShapeWrapper shapeWrapper, double directionX, double directionY)
        {
            type t = shapeWrapper.t;
            if(isNotLegalMove(shapeWrapper.s,directionX,directionY))
                return;
            switch (t)
            {
                case type.CIRCLE:
                    Ellipse el = (Ellipse)shapeWrapper.s;
                    Canvas.SetLeft(el, Canvas.GetLeft(el) + directionX);
                    Canvas.SetTop(el, Canvas.GetTop(el) + directionY);
                    break;
                case type.LINE:
                    Line l = (Line)shapeWrapper.s;
                    l.X1 += directionX;
                    l.X2 += directionX;
                    l.Y1 += directionY;
                    l.Y2 += directionY;
                    break;
                case type.SQUARE:
                    Rectangle rec = (Rectangle)shapeWrapper.s;
                    double left = Canvas.GetLeft(rec) + directionX;
                    double top = Canvas.GetTop(rec) + directionY;
                    if (left > canvXSize || left < 0
                        || top > canvYSize || top < 0
                        || left + rec.Width > canvXSize || top + rec.Height > canvYSize)
                        break;
                    Canvas.SetLeft(rec, Canvas.GetLeft(rec) + directionX);
                    Canvas.SetTop(rec, Canvas.GetTop(rec) + directionY);
                    print(Canvas.GetTop(rec).ToString());
                    break;
            }
        }

        private bool isNotLegalMove(Shape shape, double directionX, double directionY)
        {
            if (shape is Rectangle || shape is Ellipse)
            {
                if (Canvas.GetLeft(shape) + shape.Width + directionX > canvXSize
                    || Canvas.GetLeft(shape) + directionX < 0
                    || Canvas.GetTop(shape) + shape.Height + directionY > canvYSize
                    || Canvas.GetTop(shape) + directionY < 0)
                    return true;
                return false;
            }
            else // shape is line
            {
                Line l = shape as Line;
                if (l.X1 + directionX > canvXSize
                    || l.X1 + directionX < 0
                    || l.X2 + directionX > canvXSize
                    || l.X2 + directionX < 0
                    || l.Y1 + directionY > canvYSize
                    || l.Y1 + directionY < 0
                    || l.Y2 + directionY > canvYSize
                    || l.Y2 + directionY < 0)
                    return true;
                return false;
            }
        }
        #endregion

        #region Pen Exclusive Stuff
        private void addPenLine(Line fillLine)
        {
            removeLastShape(); // remove last Dot (don't need him anymore, being switched to line).
            shps.Add(new ShapeWrapper(type.PEN, fillLine));
            canv.Children.Add(fillLine);
            penUndoCount.Push(penUndoCount.Pop() + 1);
        }

        private bool isNewDotTooFarAwayFromLine(Line lastLine, Rectangle dot)
        {
            return Math.Abs(lastLine.X2 - Canvas.GetLeft(dot)) > 1
                                        || Math.Abs(lastLine.Y2 - Canvas.GetTop(dot)) > 1;
        }

        private static bool isNewDotTooFarAway(Rectangle lastDot, Rectangle dot)
        {
            return Math.Abs(Canvas.GetLeft(lastDot) - Canvas.GetLeft(dot)) > 1
                                        || Math.Abs(Canvas.GetTop(lastDot) - Canvas.GetTop(dot)) > 1;
        }
        #endregion

        #region getShapes, addShape to list

        /// <summary>
        /// get line object in appropriate color.
        /// </summary>
        /// <returns></returns>
        private Line getLine()
        {
            Line myLine = new Line();
            myLine.StrokeThickness = 1;
            myLine.Stroke = Brushes.Black;
            myLine.ClipToBounds = true;
            myLine.Stroke = isWhite ? Brushes.White : Brushes.Black;
            return myLine;
        }

        private Rectangle getSquare()
        {
            Rectangle r = new Rectangle();
            r.Fill = isWhite ? Brushes.White : Brushes.Black;
            r.Stroke = r.Fill;
            return r;
        }

        private Ellipse getEllipse()
        {
            Ellipse el = new Ellipse();
            el.Fill = isWhite ? Brushes.White : Brushes.Black;
            el.Stroke = el.Fill;
            return el;
        }
        /// <summary>
        /// dots is just a pixel by pixel square, used for the pen tool.
        /// </summary>
        private Rectangle addDotToShapesList()
        {
            ShapeWrapper sw = new ShapeWrapper(type.PEN, getSquare());
            shps.Add(sw);
            penUndoCount.Push(penUndoCount.Pop() + 1);
            return sw.s as Rectangle;
        }

        private void addLineToShapesList()
        {
            ShapeWrapper sw = new ShapeWrapper(type.LINE, getLine());
            shps.Add(sw);
        }

        private void addSquare()
        {
            ShapeWrapper sw = new ShapeWrapper(type.SQUARE, getSquare());
            shps.Add(sw);
        }

        private void addCircle()
        {
            ShapeWrapper sw = new ShapeWrapper(type.CIRCLE, getEllipse());
            shps.Add(sw);
        }

        private void removeLastShape()
        {
            canv.Children.Remove(shps.Last().s);
            if (shps.Last().t == type.PEN)
                penUndoCount.Push(penUndoCount.Pop() - 1);
            this.shps.Remove(shps.Last());
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
                        if (sw.s is Rectangle)
                        {
                            Rectangle dot = sw.s as Rectangle;
                            maybeToggleColor(dot.Fill, ref b, ref toggleBW);
                            print(jackPrintDot(dot));
                        }
                        else
                        {
                            Line penLine = sw.s as Line;
                            maybeToggleColor(penLine.Stroke, ref b, ref toggleBW);
                            print(jackPrintLine(penLine));
                        }
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

        /// <summary>
        /// toggle color from jack api, if current obj color is different than up till now.
        /// </summary>
        /// <param name="brush"></param>
        /// <param name="b"></param>
        /// <param name="toggleBW"></param>
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

        /// <summary>
        /// This and other jackPrintXXX is the jack code for the current shape on the screen. (made according to the JACKOSAPI file).
        /// </summary>
        /// <param name="rectangle"></param>
        /// <returns></returns>
        private string jackPrintDot(Rectangle rectangle)
        {
            return "do Screen.drawPixel(" + ((int)Canvas.GetLeft(rectangle)).ToString() + ", " + ((int)Canvas.GetTop(rectangle)).ToString() + ");";
        }

        private string jackPrintLine(Line line)
        {

            return "do Screen.drawLine(" + line.X1.ToString() + ", " + line.Y1.ToString() + ", " + line.X2.ToString() + ", " + line.Y2.ToString() + ");";
        }

        private string jackPrintRect(Rectangle rec)
        {
            int x = (int)Canvas.GetLeft(rec);
            int y = (int)Canvas.GetTop(rec);
            return "do Screen.drawRectangle(" + x.ToString() + ", " + y.ToString() + ", " + (x + rec.Width).ToString() + ", " + (y + rec.Height).ToString() + ");";
        }

        private string jackPrintCircle(Ellipse el)
        {
            int x = (int)(Canvas.GetLeft(el) + (el.Width / 2));
            int y = (int)(Canvas.GetTop(el) + (el.Height / 2));
            return "do Screen.drawCircle(" + x.ToString() + ", " + y.ToString() + ", " + (el.Width / 2).ToString() + ");";
        }

        /// <summary>
        /// save the jack code to a file. Didn't use this in the end.
        /// </summary>
        /// <param name="text"></param>
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

        private void clearScreenButton_Click(object sender, RoutedEventArgs e)
        {
            canv.Children.Clear();
            shps.Clear();
        }

        private void clearDebugTextBox()
        {
            this.debugTextBox.Clear();
        }

        #endregion

    }
}
