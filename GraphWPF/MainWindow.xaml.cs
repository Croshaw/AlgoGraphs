using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using GraphApi;
using GraphWPF.Classes;

namespace GraphWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public GraphVisualization DrawGraph { get; set; }
        public GraphWorker GraphWorkerCpp { get; set; }

        public IList MatrixSize { get; private set; }
        public object Matrix { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            DrawGraph = new GraphVisualization(this.canvas);
            GraphWorkerCpp = new GraphWorker(DrawGraph);

            this.MatrixSize = Enumerable.Range(1, 10).ToArray();
            this.DataContext = this;
        }

        private void canvas_MouseMove(object sender, MouseEventArgs e)
        {
            DrawGraph.MoveEdge();
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            DrawGraph.AddNode();
        }

        private void canvas_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void ButtonMinimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void SettingsMenu_Click(object sender, RoutedEventArgs e)
        {
            settingContextMenu.IsOpen = true;
            //FillWeightMatrix();
        }


        private void SettingsMenu_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        private void SetDirected_Checked(object sender, RoutedEventArgs e)
        {
            if (DrawGraph != null)
            {
                DrawGraph.SetGraphDirected();
            }
        }

        private void SetUndirected_Checked(object sender, RoutedEventArgs e)
        {
            if (DrawGraph != null)
            {
                DrawGraph.SetGraphUndirected();
            }
        }

        private void FillWeightMatrix()
        {
            //List<List<object>> objectWeightMatrix = new List<List<object>>();
            //List<List<int>> weightMatrix = GraphWorkerCpp.GetWeigthMatrix();
            //foreach (var weightLine in weightMatrix)
            //{
            //    objectWeightMatrix.Add(weightLine.Cast<object>().ToList());
            //}


            //if(objectWeightMatrix.Count > 0)
            //{
            //    var max = objectWeightMatrix.Max(f=>f.Count);

            //    for (int i = 0; i < max; i++)
            //    {
            //        WeigthMatrixDataGrid.Columns.Add(
            //            new DataGridTextColumn()
            //            {
            //                Header = string.Format("Column: {0:00}", i),
            //                Binding = new Binding(string.Format("[{0}]", i))
            //            });
            //    }
            //}
            // WeigthMatrixDataGrid.ItemsSource = weightMatrix;
        }
    }
}
