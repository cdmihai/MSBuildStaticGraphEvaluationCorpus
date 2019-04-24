using Microsoft.Build.Experimental.Graph;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Build.Shared;

namespace GraphGen
{
    public class GraphVisEdge
    {
        public GraphVisNode From { get; }
        public GraphVisNode To { get;  }

        public GraphVisEdge(GraphVisNode from, GraphVisNode to)
        {
            From = from;
            To = to;
        }

        public string Create()
        {
            return $"  {From.Name} -> {To.Name};";
        }
    }
}
