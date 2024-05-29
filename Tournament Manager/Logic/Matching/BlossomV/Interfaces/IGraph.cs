using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tournament_Manager.Logic.Matching.BlossomV.Interfaces
{
    internal interface IGraph<V, E>
    {

        public HashSet<E> EdgesOf(V v);

        public HashSet<V> VertexSet();

    }
}
