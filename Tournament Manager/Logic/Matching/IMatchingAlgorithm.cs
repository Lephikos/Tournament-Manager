using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tournament_Manager.Logic.Graph.cs;

namespace Tournament_Manager.Logic.Matching
{

    /// <summary>
    /// Allows to derive a <a href="http://en.wikipedia.org/wiki/Matching_(graph_theory)">matching</a> of
    /// a given graph.
    /// </summary>
    /// <typeparam name="V">the graph vertex type</typeparam>
    /// <typeparam name="E">the graph edge type</typeparam>
    internal interface IMatchingAlgorithm<V, E>
    {

        /// <summary>
        /// Default tolerance used by algorithms comparing floating point values.
        /// </summary>
        static double DEFAULT_EPSILON = 1e-9;

        /// <summary>
        /// Compute a matching for a given graph.
        /// </summary>
        /// <returns>a matching</returns>
        public IMatching<V, E> GetMatching();

    }
}
