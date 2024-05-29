using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;
using Tournament_Manager.Logic.Matching.BlossomV.Interfaces;

namespace Tournament_Manager.Logic.Matching.BlossomV
{
    internal class KolmogorovWeightedPerfectMatching<V, E> : IMatchingAlgorithm<V, E>
    {

        #region member

        private static readonly bool DEBUG = true;

        public static readonly int INFINITY = int.MaxValue;

        public static readonly int NO_PERFECT_MATCHING_THRESHOLD = int.MaxValue;

        private static readonly string NO_PERFECT_MATCHING = "There is no perfect matching in the specified graph";

        private static readonly BlossomVOptions DEFAULT_OPTIONS = new BlossomVOptions();

        internal readonly IGraph<V, E> initialGraph;

        internal readonly IGraph<V, E> graph;

        internal BlossomVState<V, E> state;

        private BlossomVPrimalUpdater<V, E> primalUpdater;

        private BlossomVDualUpdater<V, E> dualUpdater;

        private IMatching<V, E> matching;

        private DualSolution<V, E> dualSolution;

        private BlossomVOptions options;

        private ObjectiveSense objectiveSense;

        #endregion member

        #region constructor



        #endregion constructor




        public IMatching<V, E> GetMatching()
        {
            return null;
        }


    }
}
