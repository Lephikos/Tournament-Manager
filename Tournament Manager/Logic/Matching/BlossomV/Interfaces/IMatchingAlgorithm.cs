using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tournament_Manager.Logic.Matching.BlossomV.Interfaces
{
    internal interface IMatchingAlgorithm<V, E>
    {

        public IMatching<V, E> GetMatching();

    }
}
