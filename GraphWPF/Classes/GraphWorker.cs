using GraphApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GraphApi;
using System.Windows.Shapes;
using GraphWPF.Classes.ArrowLineControl;

namespace GraphWPF.Classes
{
    public class GraphWorker
    {
        private Graph GraphCpp { get; set; }

        private GraphVisualization GraphVisualization { get; set; }

        public GraphWorker(GraphVisualization drawGraph) {
            GraphCpp = new Graph();
            GraphVisualization = drawGraph;
            EventsHandlerSetter();
        }

        private void EventsHandlerSetter()
        {
            GraphVisualization.RemoveNodeComplete += GraphVisualization_RemoveNodeComplete;
            GraphVisualization.RemoveEdgeComplete += GraphVisualization_RemoveEdgeComplete;
            GraphVisualization.AddEdgeComplete += GraphVisualization_AddEdgeComplete;
            GraphVisualization.AddNodeComplete += GraphVisualization_AddNodeComplete;
            GraphVisualization.EdgeDirectionChanged += GraphVisualization_EdgeDirectionChanged;
        }

        private void GraphVisualization_EdgeDirectionChanged(object sender, GraphEventArgs e)
        {
            if (e.FirstNodeName != null && e.SecondNodeName != null)
            {
                var nodes = GraphCpp.getNodes();
                Node? firstNode = nodes.FirstOrDefault(f => f.name == e.FirstNodeName);
                Node? secondNode = nodes.FirstOrDefault(f => f.name == e.SecondNodeName);
                
                if (firstNode != null && secondNode != null)
                {
                    if (((ArrowLine)sender).ArrowEnds == ArrowEnds.None)
                    {
                        GraphCpp.changeDirection(firstNode, secondNode, 0);
                    }
                    else if (((ArrowLine)sender).ArrowEnds == ArrowEnds.End)
                    {
                        GraphCpp.changeDirection(firstNode, secondNode, 1);
                    }
                    else if(((ArrowLine)sender).ArrowEnds == ArrowEnds.Start)
                    {
                        GraphCpp.changeDirection(firstNode, secondNode, -1);
                    }
                    else
                    {
                        GraphCpp.changeDirection(firstNode, secondNode, 2);
                    }
                }
            }
        }

        private void GraphVisualization_AddNodeComplete(object sender, GraphEventArgs e)
        {
            if (e.FirstNodeName != null)
            {
                GraphCpp.addNode(e.FirstNodeName);
            }
        }

        private void GraphVisualization_AddEdgeComplete(object sender, GraphEventArgs e)
        {
            if (e.FirstNodeName != null&&e.SecondNodeName!=null)
            {
                var nodes = GraphCpp.getNodes();
                Node? firstNode = nodes.FirstOrDefault(f => f.name == e.FirstNodeName);
                Node? secondNode = nodes.FirstOrDefault(f => f.name == e.SecondNodeName);
                if (firstNode != null && secondNode != null)
                {
                    GraphCpp.addEdge(firstNode, secondNode);
                }
            }
        }

        private void GraphVisualization_RemoveEdgeComplete(object sender, GraphEventArgs e)
        {
            if (e.FirstNodeName!=null&&e.SecondNodeName!=null)
            {
                var nodes = GraphCpp.getNodes();
                Node? firstNode = nodes.FirstOrDefault(f => f.name == e.FirstNodeName);
                Node? secondNode = nodes.FirstOrDefault(f => f.name == e.SecondNodeName);
                if (firstNode != null && secondNode != null)
                {
                    GraphCpp.removeEdge(firstNode, secondNode);
                }
            }
        }

        private void GraphVisualization_RemoveNodeComplete(object sender, GraphEventArgs e)
        {
            if (e.FirstNodeName != null)
            {
                var nodes = GraphCpp.getNodes();
                Node? nodeToRemove = nodes.FirstOrDefault(f => f.name == e.FirstNodeName);
                if (nodeToRemove != null)
                {
                    GraphCpp.removeNode(nodeToRemove);
                }
            }
        }

        public List<List<int>> GetWeigthMatrix()
        {
            GraphHelper graphHelper = new GraphHelper(GraphCpp);
            return graphHelper.weightMatrix();
        }

        public List<List<int>> GetAdjacencyMatrix()
        {
            GraphHelper graphHelper = new GraphHelper(GraphCpp);
            return graphHelper.adjacencyMatrix();
        }

        public List<List<short>> GetIncidenceMatrix()
        {
            GraphHelper graphHelper = new GraphHelper(GraphCpp);
            return graphHelper.incidenceMatrix();
        }
    }
}
