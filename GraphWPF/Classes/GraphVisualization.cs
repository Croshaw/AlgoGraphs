using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using GraphWPF.Classes.ArrowLineControl;
using GraphApi;
using System.Diagnostics.Eventing.Reader;
using System.Reflection;

namespace GraphWPF.Classes {

    
    public class GraphVisualization
    {
        private int increment = 1;

        private bool _isEllipseDragInProg = false;
        private bool _isLineCapture = false;
        private bool _isDirected = true;

        private readonly int _circleWidth = 35;
        private readonly int _supportEllipseWidth = 6;
        private readonly int _lineStrokeThicknes = 2;
        private readonly int _lineSelectedStrokeThicknes = 3;
        private readonly int _ellipseSelectedStrokeThicknes = 3;
        private readonly int _ellipseStrokeThicknes = 2;
        private readonly int _positionCorrectionTextY = 8;
        private readonly int _positionCorrectionTextX = 3;

        private Ellipse? SelectedEllipse { get; set; }
        private Ellipse? FocusEllipse { get; set; }
        private Ellipse? RemovedEllipse { get; set; }
        private Line? LineUnion { get; set; }
        private Ellipse? SelectedSupportEllipse { get; set; }
        private ArrowLine? SelectedLine { get; set; }
        private List<Ellipse> EllipseList { get; set; }


        private Dictionary<Ellipse, List<Ellipse>> SupportElipses { get; set; }
        private Dictionary<Ellipse, List<ArrowLine>> LinesData { get; set; }
        public Dictionary<ArrowLine,List<Ellipse>> EllipseLinks { get; private set; }
        public Dictionary<Ellipse, TextBlock> EllipseData { get; private set; }
        private Dictionary<ArrowLine, int> ArrowsWeight { get; set; }

        private Canvas CanvasControl { get; set; }
        private ContextMenu ContextLineMenu { get; set; }
        private ContextMenu EllipseContextMenu { get; set; }

        public delegate void GraphEventHandler(object sender, GraphEventArgs e);

        public event GraphEventHandler EdgeDirectionChanged;
        public event GraphEventHandler AddNodeComplete;
        public event GraphEventHandler RemoveNodeComplete;
        public event GraphEventHandler RemoveEdgeComplete;
        public event GraphEventHandler AddEdgeComplete;

        public GraphVisualization(Canvas canvas)
        {
            CanvasControl = canvas;
            EllipseList = new List<Ellipse>();
            SupportElipses = new Dictionary<Ellipse, List<Ellipse>>();
            LinesData = new Dictionary<Ellipse, List<ArrowLine>>();
            ContextLineMenu = new ContextMenu();
            EllipseContextMenu = new ContextMenu();
            EllipseLinks = new Dictionary<ArrowLine, List<Ellipse>>();
            EllipseData = new Dictionary<Ellipse, TextBlock>();
            ArrowsWeight = new Dictionary<ArrowLine, int>();
            SetDirectedEdgeContextMenu();
            SetEllipseContextMenu();
        }

        private void SetDirectedEdgeContextMenu()
        {
            ContextLineMenu.Items.Clear();

            MenuItem ChangeDirection_MenuItem = new MenuItem();
            ChangeDirection_MenuItem.Name = "ChangeDirection";
            ChangeDirection_MenuItem.Header = "Изменить направление";
            ChangeDirection_MenuItem.Click += ChangeDirection_MenuItem_Click;

            MenuItem Remove_MenuItem = new MenuItem();
            Remove_MenuItem.Name = "Remove";
            Remove_MenuItem.Header = "Удалить ребро";
            Remove_MenuItem.Click += Remove_MenuItem_Click;

            MenuItem UnidirectionalEdge_MenuItem = new MenuItem();
            UnidirectionalEdge_MenuItem.Name = "UnidirectionalEdge";
            UnidirectionalEdge_MenuItem.Header = "Однонаправленное ребро";
            UnidirectionalEdge_MenuItem.MouseLeftButtonDown += UnidirectionalEdge_MenuItem_MouseLeftButtonDown;
            UnidirectionalEdge_MenuItem.Items.Add(ChangeDirection_MenuItem);

            MenuItem BidirectionalEdge = new MenuItem();
            BidirectionalEdge.Header = "Двунаправленное ребро";
            BidirectionalEdge.Name = "BidirectionalEdge";
            BidirectionalEdge.Click += BidirectionalEdge_Click;

            ContextLineMenu.Items.Add(UnidirectionalEdge_MenuItem);
            ContextLineMenu.Items.Add(BidirectionalEdge);
            ContextLineMenu.Items.Add(Remove_MenuItem);
        }

        private void SetUndirectedEdgeContextMenu()
        {
            ContextLineMenu.Items.Clear();

            MenuItem Remove_MenuItem = new MenuItem();
            Remove_MenuItem.Name = "Remove";
            Remove_MenuItem.Header = "Удалить ребро";
            Remove_MenuItem.Click += Remove_MenuItem_Click;

            ContextLineMenu.Items.Add(Remove_MenuItem);
        }

        private void SetEllipseContextMenu()
        {
            MenuItem RemoveNode_MenuItem = new MenuItem();
            RemoveNode_MenuItem.Name = "RemoveNode";
            RemoveNode_MenuItem.Header = "Удалить узел";
            RemoveNode_MenuItem.Click += RemoveNode_MenuItem_Click;

            EllipseContextMenu.Items.Add(RemoveNode_MenuItem);  
        }
        
        private void RemoveNode_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (RemovedEllipse != null)
            {
                RemoveNode(RemovedEllipse);
                RemovedEllipse = null;
            }
            else if(SelectedEllipse!= null)
            {
                RemoveNode(SelectedEllipse);
                SelectedEllipse = null;
            }
            FocusEllipse = null;
        }

        private void Remove_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            if(SelectedLine != null)
            {
                RemoveEdge(SelectedLine);
            }
        }

        private void UnidirectionalEdge_MenuItem_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (SelectedLine != null)
            {
                SelectedLine.ArrowEnds = ArrowEnds.End;
                if (EdgeDirectionChanged != null) EdgeDirectionChanged(SelectedLine, new GraphEventArgs(EllipseData[EllipseLinks[SelectedLine][0]].Text, EllipseData[EllipseLinks[SelectedLine][1]].Text));
            }
        }

        private void BidirectionalEdge_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedLine != null)
            {
                SelectedLine.ArrowEnds = ArrowEnds.Both;
                if (EdgeDirectionChanged != null) EdgeDirectionChanged(SelectedLine, new GraphEventArgs(EllipseData[EllipseLinks[SelectedLine][0]].Text, EllipseData[EllipseLinks[SelectedLine][1]].Text));
            }
        }

        private void ChangeDirection_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedLine != null)
            {
                if (SelectedLine.ArrowEnds == ArrowEnds.End)
                {
                    SelectedLine.ArrowEnds = ArrowEnds.Start;
                    if (EdgeDirectionChanged != null) EdgeDirectionChanged(SelectedLine, new GraphEventArgs(EllipseData[EllipseLinks[SelectedLine][0]].Text, EllipseData[EllipseLinks[SelectedLine][1]].Text));
                }
                else
                {
                    SelectedLine.ArrowEnds = ArrowEnds.End;
                    if (EdgeDirectionChanged != null) EdgeDirectionChanged(SelectedLine, new GraphEventArgs(EllipseData[EllipseLinks[SelectedLine][0]].Text, EllipseData[EllipseLinks[SelectedLine][1]].Text));
                }
            }

        }

        private Ellipse CreateMainEllipse()
        {
            Ellipse ellipse = new Ellipse();
            ellipse.Width = _circleWidth;
            ellipse.Height = _circleWidth;
            ellipse.VerticalAlignment = VerticalAlignment.Top;
            ellipse.Fill = Brushes.LightGray;
            ellipse.StrokeThickness = _ellipseStrokeThicknes;

            ellipse.Stroke = Brushes.Black;
            ellipse.MouseLeftButtonDown += Ellipse_MouseLeftButtonDown;
            ellipse.MouseLeftButtonUp += Ellipse_MouseLeftButtonUp;
            ellipse.MouseMove += Ellipse_MouseMove;
            ellipse.MouseRightButtonDown += Ellipse_MouseRightButtonDown;

            ellipse.ContextMenu = EllipseContextMenu;


            LinesData.Add(ellipse, new List<ArrowLine>());
            return ellipse;
        }

        private void Ellipse_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            SelectedEllipse = null;
            SelectedEllipse = (Ellipse)sender;
        }

        private void CreateSupportEllipse(Point mousePosition, Ellipse mainEllipse)
        {
            Ellipse ellipseLeft = new Ellipse();
            SetSupportEllipseConfig(ellipseLeft);
            Canvas.SetTop(ellipseLeft, mousePosition.Y - _supportEllipseWidth / 2);
            Canvas.SetLeft(ellipseLeft, mousePosition.X + _supportEllipseWidth / 2 + _circleWidth / 2);
            CanvasControl.Children.Add(ellipseLeft);

            Ellipse ellipseTop = new Ellipse();
            SetSupportEllipseConfig(ellipseTop);
            Canvas.SetTop(ellipseTop, mousePosition.Y - 1.4 * _supportEllipseWidth - _circleWidth / 2);
            Canvas.SetLeft(ellipseTop, mousePosition.X - _supportEllipseWidth / 2);
            CanvasControl.Children.Add(ellipseTop);

            Ellipse ellipseBottom = new Ellipse();
            SetSupportEllipseConfig(ellipseBottom);
            Canvas.SetTop(ellipseBottom, mousePosition.Y + _supportEllipseWidth / 2 + _circleWidth / 2);
            Canvas.SetLeft(ellipseBottom, mousePosition.X - _supportEllipseWidth / 2);
            CanvasControl.Children.Add(ellipseBottom);

            Ellipse ellipseRight = new Ellipse();
            SetSupportEllipseConfig(ellipseRight);
            Canvas.SetTop(ellipseRight, mousePosition.Y - _supportEllipseWidth / 2);
            Canvas.SetLeft(ellipseRight, mousePosition.X - 1.4 * _supportEllipseWidth - _circleWidth / 2);
            CanvasControl.Children.Add(ellipseRight);

            SupportElipses.Add(mainEllipse, new List<Ellipse>() { ellipseLeft, ellipseTop, ellipseRight, ellipseBottom });
        }

        private void CreateLineUnion(Ellipse supportEllipse)
        {
            LineUnion = new Line();
            LineUnion.StrokeThickness = 2;
            LineUnion.Fill = Brushes.Black;
            LineUnion.Stroke = Brushes.Black;
            LineUnion.X1 = Canvas.GetLeft(supportEllipse) + _supportEllipseWidth / 2;
            LineUnion.Y1 = Canvas.GetTop(supportEllipse) + _supportEllipseWidth / 2;

            Point mousePosition = Mouse.GetPosition(CanvasControl);
            LineUnion.X2 = mousePosition.X;
            LineUnion.Y2 = mousePosition.Y;

            CanvasControl.Children.Add(LineUnion);
        }

        private Ellipse? SearchEllipse(Point mousePoint)
        {
            foreach (var item in EllipseList)
            {
                double x = Canvas.GetLeft(item);
                double y = Canvas.GetTop(item);
                if (mousePoint.X >= Canvas.GetLeft(item) && mousePoint.X <= Canvas.GetLeft(item) + _circleWidth && mousePoint.Y >= Canvas.GetTop(item) && mousePoint.Y <= Canvas.GetTop(item) + _circleWidth)
                {
                    return item;
                }
            }
            return null;
        }

        private Ellipse? AdvancedSearchEllipse(Point mousePoint)
        {
            foreach (var item in EllipseList)
            {
                if (mousePoint.X >= Canvas.GetLeft(item) - _circleWidth / 2 && mousePoint.X <= Canvas.GetLeft(item) + _circleWidth + _circleWidth / 2 && mousePoint.Y >= Canvas.GetTop(item) - _circleWidth / 2 && mousePoint.Y <= Canvas.GetTop(item) + _circleWidth + _circleWidth / 2)
                {
                    return item;
                }
            }
            return null;
        }

        private void RemoveEdge(ArrowLine arrowLine)
        {
            foreach (var item in LinesData)
            {
                if (item.Value.Contains(arrowLine))
                {
                    item.Value.Remove(arrowLine);

                    CanvasControl.Children.Remove(arrowLine);
                }
            }
            if (RemoveNodeComplete != null && EllipseLinks[arrowLine].Count == 2) RemoveEdgeComplete(arrowLine, new GraphEventArgs(EllipseData[EllipseLinks[arrowLine][0]].Text, EllipseData[EllipseLinks[arrowLine][1]].Text));
        EllipseLinks.Remove(arrowLine);
            
        }

        private void RemoveNode(Ellipse ellipse)
        {
            CanvasControl.Children.Remove(ellipse);
            CanvasControl.Children.Remove(EllipseData[ellipse]);
            RemoveSupportEllipse(ellipse);
            LinesDataDeleteLinks(ellipse);
            EllipseList.Remove(ellipse);
            if(RemoveNodeComplete!=null) RemoveNodeComplete(ellipse, new GraphEventArgs(EllipseData[ellipse].Text,null));
            EllipseData.Remove(ellipse);
        }

        private void LinesDataDeleteLinks(Ellipse ellipse)
        {
            List<ArrowLine> linesToDelete = new List<ArrowLine>(LinesData[ellipse]);
            foreach (var item in linesToDelete)
            {
                RemoveEdge(item);
            }
            LinesData.Remove(ellipse);
        }

        private void RemoveSupportEllipse(Ellipse ellipse)
        {
            foreach (var item in SupportElipses[ellipse])
            {
                CanvasControl.Children.Remove(item);
            }
            SupportElipses.Remove(ellipse);
        }

        private void MainEllipseMove(Point mousePos)
        {
            if (SelectedEllipse != null)
            {
                if (!_isEllipseDragInProg) return;

                double left = mousePos.X - SelectedEllipse.ActualWidth / 2;
                double top = mousePos.Y - SelectedEllipse.ActualHeight / 2;
                Canvas.SetLeft(SelectedEllipse, left);
                Canvas.SetTop(SelectedEllipse, top);
                SupportEllipseMove(mousePos);
                LinesMove();
                DataMove(SelectedEllipse, mousePos);
            }
        }

        private void DataMove(Ellipse ellipse, Point mousePos)
        {
            TextBlock data = EllipseData[ellipse];
            Canvas.SetLeft(data, mousePos.X-_positionCorrectionTextX);
            Canvas.SetTop(data, mousePos.Y-_positionCorrectionTextY);
        }

        private void SupportEllipseMove(Point mousePos)
        {
            if (SelectedEllipse != null)
            {
                List<Ellipse> supportEllipses = SupportElipses[SelectedEllipse];
                UpdateSupportEllipsesPosition(supportEllipses, mousePos);
            }
        }

        private void UpdateSupportEllipsesPosition(List<Ellipse> supportEllipse, Point mousePosition)
        {
            for (int i = 0; i < supportEllipse.Count; i++)
            {
                switch (i)
                {
                    case 0:
                        Canvas.SetTop(supportEllipse[i], mousePosition.Y - _supportEllipseWidth / 2);
                        Canvas.SetLeft(supportEllipse[i], mousePosition.X + _supportEllipseWidth / 2 + _circleWidth / 2);
                        break;
                    case 1:
                        Canvas.SetTop(supportEllipse[i], mousePosition.Y - 1.4 * _supportEllipseWidth - _circleWidth / 2);
                        Canvas.SetLeft(supportEllipse[i], mousePosition.X - _supportEllipseWidth / 2);
                        break;
                    case 2:
                        Canvas.SetTop(supportEllipse[i], mousePosition.Y + _supportEllipseWidth / 2 + _circleWidth / 2);
                        Canvas.SetLeft(supportEllipse[i], mousePosition.X - _supportEllipseWidth / 2);
                        break;
                    case 3:
                        Canvas.SetTop(supportEllipse[i], mousePosition.Y - _supportEllipseWidth / 2);
                        Canvas.SetLeft(supportEllipse[i], mousePosition.X - 1.4 * _supportEllipseWidth - _circleWidth / 2);
                        break;
                }
            }
        }

        private void LinesMove()
        {
            List<ArrowLine> lines = LinesData[SelectedEllipse];
            foreach (ArrowLine line in lines)
            {
                Ellipse secondEllipse = new Ellipse();
                foreach (var item in LinesData)
                {
                    if (item.Value.Contains(line) && item.Key != SelectedEllipse)
                    {
                        secondEllipse = item.Key;
                    }
                }
               
                Point firstCircleCenter = new Point(Canvas.GetLeft(SelectedEllipse) + _circleWidth / 2, Canvas.GetTop(SelectedEllipse) + _circleWidth / 2);
                Point secondCircleCenter = new Point(Canvas.GetLeft(secondEllipse) + _circleWidth / 2, Canvas.GetTop(secondEllipse) + _circleWidth / 2);

                List<Ellipse>? link = FindLink(SelectedEllipse, secondEllipse);
                if (link != null)
                {
                    if(link.IndexOf(secondEllipse) != 1)
                    {
                        Point temp = new Point(secondCircleCenter.X, secondCircleCenter.Y);
                        secondCircleCenter = new Point(firstCircleCenter.X, firstCircleCenter.Y);
                        firstCircleCenter = new Point(temp.X, temp.Y);
                    }
                }
                (Point contactPointFirstCircle, Point contactPointSecondCircle, bool isReverse) contactPoints = CalculateContactPoints(firstCircleCenter, secondCircleCenter);
                
                    line.X1 = contactPoints.contactPointFirstCircle.X;
                    line.Y1 = contactPoints.contactPointFirstCircle.Y;
                    line.X2 = contactPoints.contactPointSecondCircle.X;
                    line.Y2 = contactPoints.contactPointSecondCircle.Y;
            }
        }

        private List<Ellipse>? FindLink(Ellipse first,Ellipse second)
        {
            foreach(var item in EllipseLinks.Values)
            {
                if (item.Contains(first) && item.Contains(second))
                {
                    return item;
                }
            }
            return null;
        }

        private void UpdateLineUnionPosition()
        {
            bool mouseIsDown = Mouse.LeftButton == MouseButtonState.Pressed;
            if (mouseIsDown)
            {
                RotateLine();
            }
            else
            {
                SetLine();
            }
        }

        private void RotateLine()
        {
            Point mousePosition = Mouse.GetPosition(CanvasControl);
            LineUnion.X2 = mousePosition.X;
            LineUnion.Y2 = mousePosition.Y;
        }

        private void SetLine()
        {
            Ellipse? firstEllipse = AdvancedSearchEllipse(new Point(LineUnion.X1, LineUnion.Y1));
            Ellipse? secondEllipse = AdvancedSearchEllipse(Mouse.GetPosition(CanvasControl));
            if (!(firstEllipse == secondEllipse || firstEllipse == null || secondEllipse == null))
            {
                Point firstCircleCenter = new Point(Canvas.GetLeft(firstEllipse) + _circleWidth / 2, Canvas.GetTop(firstEllipse) + _circleWidth / 2);
                Point secondCircleCenter = new Point(Canvas.GetLeft(secondEllipse) + _circleWidth / 2, Canvas.GetTop(secondEllipse) + _circleWidth / 2);
                ArrowLine line = GetLinePosition(firstCircleCenter, secondCircleCenter);
                if (!_isDirected)
                {
                    line.ArrowEnds = ArrowEnds.None;
                }
                if (LinesData[firstEllipse].FirstOrDefault(f => f.X1 == line.X1 && f.X2 == line.X2 && f.Y1 == line.Y1 && f.Y2 == line.Y2) == null&&
                    LinesData[firstEllipse].FirstOrDefault(f => f.X2 == line.X1 && f.X1 == line.X2 && f.Y2 == line.Y1 && f.Y1 == line.Y2) == null)
                {
                    CanvasControl.Children.Add(line);
                    LinesData[firstEllipse].Add(line);
                    LinesData[secondEllipse].Add(line);
                    EllipseLinks.Add(line,new List<Ellipse> { firstEllipse, secondEllipse });
                    ArrowsWeight.Add(line, 1);
                    if(AddEdgeComplete!=null) AddEdgeComplete(line,new GraphEventArgs(EllipseData[firstEllipse].Text, EllipseData[secondEllipse].Text));
                    if (EdgeDirectionChanged != null) EdgeDirectionChanged(line, new GraphEventArgs(EllipseData[firstEllipse].Text, EllipseData[secondEllipse].Text));
                }

            }
            CanvasControl.Children.Remove(LineUnion);
            _isLineCapture = false;
            LineUnion = null;
            SelectedSupportEllipseUnFocus();
        }

        private void SelectedSupportEllipseUnFocus()
        {
            SelectedSupportEllipse.Fill = Brushes.Gray;
            SelectedSupportEllipse.Stroke = Brushes.Gray;
            SelectedSupportEllipse = null;
        }

        private void SetVisibleToSupportEllipses(Ellipse? ellipseCheked)
        {
            if (ellipseCheked != null)
            {
                FocusEllipse = ellipseCheked;
                List<Ellipse> supportEllipses = SupportElipses[FocusEllipse];
                foreach (Ellipse ellipse in supportEllipses)
                {
                    ellipse.Visibility = Visibility.Visible;
                }
            }
        }

        private void SetHiddenToSupportEllipses()
        {
            if (FocusEllipse != null)
            {
                List<Ellipse> supportEllipses = SupportElipses[FocusEllipse];
                foreach (Ellipse ellipse in supportEllipses)
                {
                    ellipse.Visibility = Visibility.Hidden;
                }
            }
            FocusEllipse = null;
        }

        private void SetSupportEllipseConfig(Ellipse ellipse)
        {
            ellipse.Width = _supportEllipseWidth;
            ellipse.Height = _supportEllipseWidth;
            ellipse.VerticalAlignment = VerticalAlignment.Top;
            ellipse.Fill = Brushes.Gray;
            ellipse.StrokeThickness = 1;
            ellipse.Stroke = Brushes.Gray;
            ellipse.Visibility = Visibility.Hidden;
            ellipse.MouseLeftButtonDown += SupportEllipse_MouseLeftButtonDown;
            ellipse.MouseLeftButtonUp += SupportEllipse_MouseLeftButtonUp;
        }

        private bool СollisionСhecking()
        {
            Point mousePoint = Mouse.GetPosition(CanvasControl);
            foreach (var item in EllipseList)
            {
                if (mousePoint.X >= Canvas.GetLeft(item) - _circleWidth && mousePoint.X <= Canvas.GetLeft(item) + 2 * _circleWidth && mousePoint.Y >= Canvas.GetTop(item) - _circleWidth && mousePoint.Y <= Canvas.GetTop(item) + 2 * _circleWidth)
                {
                    return true;
                }
            }
            return false;
        }

        private (Point contactPointFirstCircle, Point contactPointSecondCircle,bool isReverse) CalculateContactPoints(Point firstCircleCenter, Point secondCircleCenter)
        {
            bool isReverse = false;
            if (firstCircleCenter.X > secondCircleCenter.X)
            {
                Point temp = firstCircleCenter;
                firstCircleCenter = secondCircleCenter;
                secondCircleCenter = temp;
                isReverse = true;
            }
            double A1B = Math.Sqrt(Math.Pow(secondCircleCenter.X - firstCircleCenter.X, 2));
            double BC = Math.Sqrt(Math.Pow(secondCircleCenter.Y - firstCircleCenter.Y, 2));
            double AC = Math.Sqrt(Math.Pow(A1B, 2) + Math.Pow(BC, 2));
            double cornerAlpha = Math.Acos(A1B / AC);

            double K1M1 = Math.Sin(cornerAlpha) * (_circleWidth / 2);
            double A1M1 = Math.Cos(cornerAlpha) * (_circleWidth / 2);
            if (firstCircleCenter.X <= secondCircleCenter.X && firstCircleCenter.Y <= secondCircleCenter.Y)
            {
                Point M1 = new Point(firstCircleCenter.X + A1M1, firstCircleCenter.Y);
                Point K1 = new Point(M1.X, M1.Y + K1M1);

                double cornerBeta = Math.Acos(BC / AC);
                double A2M2 = Math.Cos(cornerBeta) * (_circleWidth / 2);
                double K2M2 = Math.Sin(cornerBeta) * (_circleWidth / 2);

                Point M2 = new Point(secondCircleCenter.X, secondCircleCenter.Y - A2M2);
                Point K2 = new Point(M2.X - K2M2, M2.Y);
                if (isReverse)
                {
                    return (K2, K1, isReverse);
                }
                return (K1, K2,isReverse);

            }
            else if (firstCircleCenter.X < secondCircleCenter.X && firstCircleCenter.Y > secondCircleCenter.Y)
            {
                Point M1 = new Point(firstCircleCenter.X + A1M1, firstCircleCenter.Y);
                Point K1 = new Point(M1.X, M1.Y - K1M1);


                double cornerBeta = Math.Acos(BC / AC);
                double A2M2 = Math.Cos(cornerBeta) * (_circleWidth / 2);
                double K2M2 = Math.Sin(cornerBeta) * (_circleWidth / 2);

                Point M2 = new Point(secondCircleCenter.X, secondCircleCenter.Y + A2M2);
                Point K2 = new Point(M2.X - K2M2, M2.Y);
                if (isReverse)
                {
                    return (K2, K1, isReverse);
                }
                return (K1, K2, isReverse);
            }
            else
            {
                return (new Point(0, 0), new Point(0, 0),false);
            }
        }

        private void Ellipse_MouseMove(object sender, MouseEventArgs e)
        {
            MainEllipseMove(e.GetPosition(CanvasControl));
            SetHiddenToSupportEllipses();
        }

        private void Ellipse_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (SelectedEllipse != null)
            {
                _isEllipseDragInProg = false;
                SelectedEllipse.ReleaseMouseCapture();
                SelectedEllipse = null;
                SetHiddenToSupportEllipses();
            }
        }

        private void Ellipse_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
           
            SelectedEllipse = SearchEllipse(Mouse.GetPosition(CanvasControl));
            if (SelectedEllipse != null)
            {
                _isEllipseDragInProg = true;
                SetHiddenToSupportEllipses();
                SelectedEllipse.CaptureMouse();
            }
        }

        private void SupportEllipse_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _isLineCapture = true;
            ((Ellipse)sender).Fill = Brushes.Black;
            ((Ellipse)sender).Stroke = Brushes.Black;
            SelectedSupportEllipse = (Ellipse)sender;
            CreateLineUnion((Ellipse)sender);
        }

        private void SupportEllipse_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {

        }

        private ArrowLine GetLinePosition(Point firstCircleCenter, Point secondCircleCenter)
        {
            (Point contactPointFirstCircle, Point contactPointSecondCircle, bool isReverse) contactPoints = CalculateContactPoints(firstCircleCenter, secondCircleCenter);

            ArrowLine line = new ArrowLine();
            line.StrokeThickness = _lineStrokeThicknes;
            line.Fill = Brushes.Black;
            line.Stroke = Brushes.Black;
            line.X1 = contactPoints.contactPointFirstCircle.X;
            line.Y1 = contactPoints.contactPointFirstCircle.Y;
            line.X2 = contactPoints.contactPointSecondCircle.X;
            line.Y2 = contactPoints.contactPointSecondCircle.Y;
            line.MouseRightButtonDown += Line_MouseRightButtonDown;
            line.MouseEnter += Line_MouseEnter;
            line.MouseLeave += Line_MouseLeave;
            
            line.ContextMenu = ContextLineMenu;

            return line;
        }

        private void Line_MouseLeave(object sender, MouseEventArgs e)
        {
            ((ArrowLine)sender).StrokeThickness = _lineStrokeThicknes;
        }

        private void Line_MouseEnter(object sender, MouseEventArgs e)
        {
            ((ArrowLine)sender).StrokeThickness = _lineSelectedStrokeThicknes;
        }

        private void Line_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            SelectedLine = null;
            SelectedLine = (ArrowLine)sender;
        }

        private TextBlock SetData(double X, double Y)
        {
            var text = new TextBlock()
            {
                Text = Convert.ToString(increment),
            };
            increment++;
            text.Background = Brushes.LightGray;
            Canvas.SetLeft(text, X + _circleWidth / 2 - _positionCorrectionTextX);
            Canvas.SetTop(text, Y+ _circleWidth / 2 - _positionCorrectionTextY);
            CanvasControl.Children.Add(text);
            text.MouseLeftButtonDown += Ellipse_MouseLeftButtonDown;
            text.MouseRightButtonDown += Text_MouseRightButtonDown;
            text.ContextMenu = EllipseContextMenu;
            return text;
        }

        private void Text_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            RemovedEllipse = SearchEllipse(Mouse.GetPosition(CanvasControl));
        }

        private void DisablingDirectionSelection()
        {
            SetUndirectedEdgeContextMenu();
        }

        private void EnableDirectionSelection()
        {
            SetDirectedEdgeContextMenu();
        }

        public void AddNode()
        {
            if (LineUnion != null) return;
            if (!СollisionСhecking())
            {
                Point mousePosition = Mouse.GetPosition(CanvasControl);

                Ellipse ellipse = CreateMainEllipse();
                Canvas.SetTop(ellipse, mousePosition.Y - _circleWidth / 2);
                Canvas.SetLeft(ellipse, mousePosition.X - _circleWidth / 2);

                CanvasControl.Children.Add(ellipse);
                EllipseList.Add(ellipse);

                CreateSupportEllipse(mousePosition, ellipse);
                EllipseData.Add(ellipse, SetData(Canvas.GetLeft(ellipse), Canvas.GetTop(ellipse)));
                if (AddNodeComplete != null) AddNodeComplete(ellipse, new GraphEventArgs(EllipseData[ellipse].Text,null));
            }
        }

        public void MoveEdge()
        {
            if (LineUnion != null)
            {
                UpdateLineUnionPosition();
            }
            Ellipse? ellipseCheked = AdvancedSearchEllipse(Mouse.GetPosition(CanvasControl));
            if (ellipseCheked != null && !ellipseCheked.IsMouseCaptured)
            {
                SetVisibleToSupportEllipses(ellipseCheked);
            }
            else
            {
                SetHiddenToSupportEllipses();
            }
        }

        public void SetGraphUndirected()
        {
            DisablingDirectionSelection();
            foreach (var lineList in LinesData.Values)
            {
                foreach (var line in lineList)
                {
                    line.ArrowEnds = ArrowEnds.None;
                    if (EdgeDirectionChanged != null) EdgeDirectionChanged(line, new GraphEventArgs(EllipseData[EllipseLinks[line][0]].Text, EllipseData[EllipseLinks[line][1]].Text));
                }
            }
            _isDirected =false;
        }

        public void SetGraphDirected()
        {
            EnableDirectionSelection();
            foreach(var lineList in LinesData.Values)
            {
                foreach(var line in lineList)
                {
                    line.ArrowEnds = ArrowEnds.End;
                    if (EdgeDirectionChanged != null) EdgeDirectionChanged(line, new GraphEventArgs(EllipseData[EllipseLinks[line][0]].Text, EllipseData[EllipseLinks[line][1]].Text));
                }
            }
            _isDirected =true;  
        }

        public void ClearVisualization()
        {
            List<Ellipse> deleteTemp = new List<Ellipse>(EllipseList);
            foreach(var item in deleteTemp)
            {
                RemoveNode(item);
            }
            increment = 1;
        }

        public void TurnOnSearchMode()
        {
          
           // _searchMode = true;
        }
        public void TurnOffSearchMode()
        {

           // _searchMode = true;
        }

        public void DrawingEdge(ArrowLine line, Ellipse firstNode, Ellipse secondNode)
        {
            line.Stroke = Brushes.Red;
            line.Fill = Brushes.Red;
            firstNode.Stroke = Brushes.Red;
            secondNode.Stroke = Brushes.Red;
        }

        public void SelectionClear()
        {
            foreach(var item in EllipseLinks)
            {
                item.Key.Stroke = Brushes.Black;
                item.Key.Fill = Brushes.Black;
                item.Value[0].Stroke = Brushes.Black;
                item.Value[1].Stroke = Brushes.Black;
            }
        }
       
    }
}
