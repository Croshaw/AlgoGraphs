using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphWPF.Classes
{
    public class GraphEventArgs
    {
        public string? FirstNodeName { get; set; }
        public string? SecondNodeName{ get; set; }

        public GraphEventArgs(string? firstNodeName, string? secondNodeName)
        {
            FirstNodeName = firstNodeName;
            SecondNodeName = secondNodeName;
        }
    }
}
