using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tournament_Manager.Logic.Matching.BlossomV.Interfaces;

namespace Tournament_Manager.Logic.Matching.BlossomV
{
    internal class BlossomVState<V, E>
    {

        internal readonly int nodeNum;

        internal readonly int edgeNum;

        internal IGraph<V, E> graph;

        internal BlossomVNode[] nodes;




    }
}
