using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tournament_Manager.Logic.Graph.cs;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Tournament_Manager.Logic.Graph
{
    internal class GraphType : IGraphType
    {

        private bool directed;
        private bool weighted;
 

        public GraphType(bool directed, bool weighted)
        {
            this.directed = directed;
            this.weighted = weighted;
        }
        


        public IGraphType AsWeighted()
        {
            return new GraphType(directed, true);
        }


        public bool IsDirected()
        {
            return directed;
        }

        public bool IsWeighted()
        {
            return weighted;
        }
    }
}
