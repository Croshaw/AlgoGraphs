using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
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
        private GraphVisualization DrawGraph { get; set; }
        private GraphWorker GraphWorkerCpp { get; set; }

        private bool _menuIsOpen = false;

        public MainWindow()
        {
            InitializeComponent();
            DrawGraph = new GraphVisualization(this.canvas);
            GraphWorkerCpp = new GraphWorker(DrawGraph);
        }

        private void canvas_MouseMove(object sender, MouseEventArgs e)
        {
            DrawGraph.MoveEdge();
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            DrawGraph.AddNode();
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

        private void SetMinWidth_WeightMatrix(object sender, RoutedEventArgs e)
        {
            foreach (var column in WeigthMatrixDataGrid.Columns)
            {
                column.MinWidth = column.ActualWidth;
                column.Width = new DataGridLength(1, DataGridLengthUnitType.Star);
            }
        }

        private void SetMinWidth_AdjacencyMatrix(object sender, RoutedEventArgs e)
        {
            foreach (var column in AdjacencyMatrixDataGrid.Columns)
            {
                column.MinWidth = column.ActualWidth;
                column.Width = new DataGridLength(1, DataGridLengthUnitType.Star);
            }
        }

        private void SetMinWidth_IncidenceMatrix(object sender, RoutedEventArgs e)
        {
            foreach (var column in IncidenceMatrixDataGrid.Columns)
            {
                column.MinWidth = column.ActualWidth;
                column.Width = new DataGridLength(1, DataGridLengthUnitType.Star);
            }
        }

        private void WeigthMatrixDataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = (e.Row.GetIndex() + 1).ToString();
        }

        private void AdjacencyMatrixDataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = (e.Row.GetIndex() + 1).ToString();
        }

        private void IncidenceMatrixDataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = (e.Row.GetIndex() + 1).ToString();
        }

        private void OpenMatrixMenu_Click(object sender, RoutedEventArgs e)
        {
            ((Button)sender).Margin= new Thickness(-100,0,0,0);
            CloseMatrixMenu.Margin = new Thickness(0, 0, 0, 0);
        }

        private void refreshMatrixData_Click(object sender, RoutedEventArgs e)
        {
            FillTables();
        }

        private void CloseMatrixMenu_Click(object sender, RoutedEventArgs e)
        {
            ((Button)sender).Margin = new Thickness(-100, 0, 0, 0);
            OpenMatrixMenu.Margin = new Thickness(0, 0, 0, 0);
        }

        private void FillTables()
        {
            FillAdjacencyMatrix();
            FillWeightMatrix();
            FillIncidenceMatrix();
        }

        private void FillWeightMatrix()
        {
            DataTable weightTable = new DataTable("WeightTable");
            List<List<int>> weightMatrix = GraphWorkerCpp.GetWeigthMatrix();
            if (weightMatrix.Count == 0) return;
            for (int i = 0; i < weightMatrix[0].Count; i++)
            {
                DataColumn dataColumn = new DataColumn(Convert.ToString(i + 1), typeof(string));
                weightTable.Columns.Add(dataColumn);
            }
            for (int i = 0; i < weightMatrix.Count; i++)
            {
                DataRow dataRow = weightTable.NewRow();
                for (int j = 0; j < weightMatrix[i].Count; j++)
                {
                    if (weightMatrix[i][j] == -1)
                    {
                        dataRow[j] = "inf";
                    }
                    else 
                    {
                        dataRow[j] = weightMatrix[i][j];
                    }
                }
                weightTable.Rows.Add(dataRow);
            }
            this.WeigthMatrixDataGrid.ItemsSource = weightTable.DefaultView;
        }

        private void FillAdjacencyMatrix()
        {
            DataTable adjacencyTable = new DataTable("AdjacencyTable");
            List<List<int>> adjacencyMatrix = GraphWorkerCpp.GetAdjacencyMatrix();
            if (adjacencyMatrix.Count == 0) return;
            for (int i = 0; i < adjacencyMatrix[0].Count; i++)
            {
                DataColumn dataColumn = new DataColumn(Convert.ToString(i+1), typeof(int));
                adjacencyTable.Columns.Add(dataColumn);
            }
            for (int i = 0; i < adjacencyMatrix.Count; i++)
            {
                DataRow dataRow = adjacencyTable.NewRow();
                for (int j = 0; j < adjacencyMatrix[i].Count; j++)
                {
                    dataRow[j] = adjacencyMatrix[i][j];
                }
                adjacencyTable.Rows.Add(dataRow);
            }
            this.AdjacencyMatrixDataGrid.ItemsSource = adjacencyTable.DefaultView;
        }

        private void FillIncidenceMatrix()
        {
            DataTable incidenceTable = new DataTable("IncidenceTable");
            List<List<short>> incidenceMatrix = GraphWorkerCpp.GetIncidenceMatrix();
            if(incidenceMatrix.Count == 0) return;  
            for (int i = 0; i < incidenceMatrix[0].Count; i++)
            {
                DataColumn dataColumn = new DataColumn(Convert.ToString(i+1), typeof(int));
                incidenceTable.Columns.Add(dataColumn);
            }
            for (int i = 0; i < incidenceMatrix.Count; i++)
            {
                DataRow dataRow = incidenceTable.NewRow();
                for (int j = 0; j < incidenceMatrix[i].Count; j++)
                {
                    dataRow[j] = incidenceMatrix[i][j];
                }
                incidenceTable.Rows.Add(dataRow);
            }
            this.IncidenceMatrixDataGrid.ItemsSource = incidenceTable.DefaultView;
        }

        private void MenuItem_Click_ClearField(object sender, RoutedEventArgs e)
        {
            DrawGraph.ClearVisualization();
        }
    }
}
