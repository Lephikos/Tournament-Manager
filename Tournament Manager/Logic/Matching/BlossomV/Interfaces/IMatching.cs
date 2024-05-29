using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tournament_Manager.Logic.Matching.BlossomV.Interfaces
{
    internal interface IMatching<V, E>
    {

        public IGraph<V, E> GetGraph();

        public int GetWeight();

        public HashSet<E> GetEdges();

        public bool IsMatched(V v)
        {
            HashSet<E> edges = GetEdges();
            return GetGraph().EdgesOf(v).Intersect(edges).Any();
        }

        public bool IsPerfect()
        {
            return GetEdges().Count == GetGraph().VertexSet().Count / 2.0;
        }



    }
}
