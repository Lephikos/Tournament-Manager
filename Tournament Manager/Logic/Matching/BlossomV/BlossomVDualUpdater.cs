using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tournament_Manager.Logic.Heap;

namespace Tournament_Manager.Logic.Matching.BlossomV
{

    /// <summary>
    /// This class is used by <see cref="KolmogorovWeightedPerfectMatching{V, E}"/> to perform dual updates, thus
    /// increasing the dual objective function value and creating new tight edges.<para/>
    /// 
    /// This class currently supports three types of dual updates: single tree, multiple trees fixed
    /// delta, and multiple tree variable delta. The first one is used to updates duals of a single tree,
    /// when at least one of the <see cref="BlossomVOptions.updateDualsBefore"/> or
    /// <see cref="BlossomVOptions.updateDualsAfter"/> is true. The latter two are used to update the duals
    /// globally and are defined by the <see cref="BlossomVOptions"/>.<para/>
    /// 
    /// There are two type of constraints on a dual change of a tree: in-tree and cross-tree. In-tree
    /// constraints are imposed by the infinity edges, (+, +) in-tree edges and "-" blossoms. Cross-tree
    /// constraints are imposed by (+, +), (+, -) and (-, +) cross-tree edges. With respect to this
    /// classification of constraints the following strategies of changing the duals can be used:
    /// 
    /// <list type="bullet">
    /// <item><description>
    /// Single tree strategy greedily increases the duals of the tree with respect to the in-tree and
    /// cross-tree constraints. This can result in a zero-change update. If a tight (+, +) cross-tree
    /// edge is encountered during this operation, an immediate augmentation is performed
    /// afterwards.
    /// </description></item>
    /// <item><description>
    /// Multiple tree fixed delta approach considers only in-tree constraints and constraints imposed
    /// by the (+, +) cross-tree edges. Since this approach increases the trees' epsilons by the same
    /// amount, it doesn't need to consider other two dual constraints. If a tight (+, +) cross-tree edge
    /// is encountered during this operation, an immediate augmentation is performed afterwards.
    /// </description></item>
    /// <item><description>
    /// Multiple tree variable delta approach considers all types of constraints. It determines a
    /// connected components in the auxiliary graph, where only tight (-, +) and (+, -) cross-tree edges
    /// are present. For these connected components it computes the same dual change, therefore the
    /// constraints imposed by the (-, +) and (+, -) cross-tree edges can't be violated. If a tight (+,+)
    /// cross-tree edge is encountered during this operation, an immediate augmentation is performed
    /// afterwards.
    /// </description></item>
    /// </list>
    /// </summary>
    /// <typeparam name="V">the graph vertex type</typeparam>
    /// <typeparam name="E">the graph edge type</typeparam>
    internal class BlossomVDualUpdater<V, E> where V : notnull where E : notnull
    {

        private readonly static bool DEBUG = KolmogorovWeightedPerfectMatching<V, E>.DEBUG;

        private readonly static double EPS = KolmogorovWeightedPerfectMatching<V, E>.EPS;

        /// <summary>
        /// State information needed for the algorithm
        /// </summary>
        private BlossomVState<V, E> state;

        /// <summary>
        /// Instance of <see cref="BlossomVPrimalUpdater{V, E}"/> for performing immediate augmentations after dual
        /// updates when they are applicable. These speed up the overall algorithm.
        /// </summary>
        private BlossomVPrimalUpdater<V, E> primalUpdater;


        /// <summary>
        /// Creates a new instance of the BlossomVDualUpdater
        /// </summary>
        /// <param name="state">the state common to <see cref="BlossomVPrimalUpdater{V, E}"/>, <see cref="BlossomVDualUpdater{V, E}"/>
        ///                     and <see cref="KolmogorovWeightedPerfectMatching{V, E}"/></param>
        /// <param name="primalUpdater">primal updater used by the algorithm</param>
        public BlossomVDualUpdater(BlossomVState<V, E> state, BlossomVPrimalUpdater<V, E> primalUpdater)
        {
            this.state = state;
            this.primalUpdater = primalUpdater;
        }


        /// <summary>
        /// Performs global dual update. Operates on the whole graph and updates duals according to the
        /// strategy defined by <see cref="BlossomVOptions.DualUpdateStrategy"/>.
        /// </summary>
        /// <param name="type">the strategy to use for updating the duals</param>
        /// <returns>the sum of all changes of dual variables of the trees</returns>
        public double UpdateDuals(BlossomVOptions.DualUpdateStrategy type)
        {
            if (DEBUG)
            {
                Console.WriteLine("Start updating duals");
            }

            long start = GetNanoTime();

            BlossomVEdge? augmentEdge = null;

            // go through all tree roots and determine the initial tree dual change wrt. in-tree
            // constraints
            // the cross-tree constraints are handles wrt. dual update strategy
            for (BlossomVNode? root = state.nodes[state.nodeNum].treeSiblingNext; root != null; root = root.treeSiblingNext)
            {
                BlossomVTree tree = root.tree!;
                double eps = GetEps(tree);
                tree.accumulatedEps = eps - tree.eps;
            }
            if (type == BlossomVOptions.DualUpdateStrategy.MULTIPLE_TREE_FIXED_DELTA)
            {
                augmentEdge = MultipleTreeFixedDelta();
            }
            else if (type == BlossomVOptions.DualUpdateStrategy.MULTIPLE_TREE_CONNECTED_COMPONENTS)
            {
                augmentEdge = UpdateDualsConnectedComponents();
            }

            double dualChange = 0;
            // add tree.accumulatedEps to the tree.eps
            for (BlossomVNode? root = state.nodes[state.nodeNum].treeSiblingNext; root != null; root = root.treeSiblingNext)
            {
                if (root.tree!.accumulatedEps > EPS)
                {
                    dualChange += root.tree.accumulatedEps;
                    root.tree.eps += root.tree.accumulatedEps;
                }
            }

            if (DEBUG)
            {
                for (BlossomVNode? root = state.nodes[state.nodeNum].treeSiblingNext; root != null;
                    root = root.treeSiblingNext)
                {
                    Console.WriteLine("Updating duals: now eps of " + root.tree + " is " + (root.tree!.eps));
                }
            }

            state.statistics.dualUpdatesTime += GetNanoTime() - start;

            if (augmentEdge != null)
            {
                primalUpdater.Augment(augmentEdge);
            }

            return dualChange;
        }

        /// <summary>
        /// Updates the duals of the single tree. This method takes into account both in-tree and
        /// cross-tree constraints. If possible, it also finds a cross-tree (+, +) edge of minimum slack
        /// and performs an augmentation.
        /// </summary>
        /// <param name="tree">the tree to update duals of</param>
        /// <returns>true if some progress was made and there was no augmentation performed, false otherwise</returns>
        public bool UpdateDualsSingle(BlossomVTree tree)
        {
            long start = GetNanoTime();

            double eps = GetEps(tree);
            double epsAugment = KolmogorovWeightedPerfectMatching<V, E>.INFINITY;

            BlossomVEdge? augmentEdge = null;
            double delta = 0;

            for (BlossomVTree.TreeEdgeEnumerator enumerator = tree.GetTreeEdgeEnumerator(); enumerator.MoveNext();)
            {
                BlossomVTreeEdge treeEdge = enumerator.Current;
                BlossomVTree opposite = treeEdge.head[enumerator.GetCurrentDirection()]!;

                if (!treeEdge.plusPlusEdges.IsEmpty())
                {
                    BlossomVEdge plusPlusEdge = treeEdge.plusPlusEdges.FindMin().GetValue()!;
                    if (plusPlusEdge.slack - opposite.eps < epsAugment)
                    {
                        epsAugment = plusPlusEdge.slack - opposite.eps;
                        augmentEdge = plusPlusEdge;
                    }
                }

                IMergeableAddressableHeap<Double, BlossomVEdge> currentPlusMinusHeap = treeEdge.GetCurrentPlusMinusHeap(opposite.currentDirection);

                if (!currentPlusMinusHeap.IsEmpty())
                {
                    BlossomVEdge edge = currentPlusMinusHeap.FindMin().GetValue()!;
                    if (edge.slack + opposite.eps < eps)
                    {
                        eps = edge.slack + opposite.eps;

                    }
                }
            }

            if (eps > epsAugment)
            {
                eps = epsAugment;
            }

            // now eps takes into account all the constraints
            if (eps > KolmogorovWeightedPerfectMatching<V, E>.NO_PERFECT_MATCHING_THRESHOLD)
            {
                throw new ArgumentException(KolmogorovWeightedPerfectMatching<V, E>.NO_PERFECT_MATCHING);
            }

            if (eps > tree.eps)
            {
                delta = eps - tree.eps;
                tree.eps = eps;
                if (DEBUG)
                {
                    Console.WriteLine("Updating duals: now eps of " + tree + " is " + eps);
                }
            }

            state.statistics.dualUpdatesTime += GetNanoTime() - start;

            if (augmentEdge != null && epsAugment <= tree.eps)
            {
                primalUpdater.Augment(augmentEdge);
                return false; // can't proceed with the same tree
            }
            else
            {
                return delta > EPS;
            }
        }


        /// <summary>
        /// Computes and returns the value which can be assigned to the <c>tree.eps</c> so that it
        /// doesn't violate in-tree constraints. In other words, <c>getEps(tree) - tree.eps</c> is the
        /// resulting dual change wrt. in-tree constraints. The computed value is always greater than or
        /// equal to the <c>tree.eps</c>, can violate the cross-tree constraints, and can be equal to
        /// <see cref="KolmogorovWeightedPerfectMatching{V, E}.INFINITY"/>.
        /// </summary>
        /// <param name="tree">the tree to process</param>
        /// <returns>a value which can be safely assigned to tree.eps</returns>
        private static double GetEps(BlossomVTree tree)
        {
            double eps = KolmogorovWeightedPerfectMatching<V, E>.INFINITY;

            // check minimum slack of the plus-infinity edges
            if (!tree.plusInfinityEdges.IsEmpty())
            {
                BlossomVEdge edge = tree.plusInfinityEdges.FindMin().GetValue()!;
                if (edge.slack < eps)
                {
                    eps = edge.slack;
                }
            }
            // check minimum dual variable of the "-" blossoms
            if (!tree.minusBlossoms.IsEmpty())
            {
                BlossomVNode node = tree.minusBlossoms.FindMin().GetValue()!;
                if (node.dual < eps)
                {
                    eps = node.dual;

                }
            }
            // check minimum slack of the (+, +) edges
            if (!tree.plusPlusEdges.IsEmpty())
            {
                BlossomVEdge edge = tree.plusPlusEdges.FindMin().GetValue()!;
                if (2 * eps > edge.slack)
                {
                    eps = edge.slack / 2;
                }
            }

            return eps;
        }

        /// <summary>
        /// Updates the duals via connected components. The connected components are a set of trees which
        /// are connected via tight (+, -) cross tree edges. For these components the same dual change is
        /// chosen. As a result, the circular constraints are guaranteed to be avoided. This is the point
        /// where the <see cref="BlossomVDualUpdater{V, E}.UpdateDualsSingle(BlossomVTree)"/> approach can fail.
        /// </summary>
        /// <returns>edge which can be augmented if possible, null otherwise</returns>
        private BlossomVEdge? UpdateDualsConnectedComponents()
        {

            BlossomVTree dummyTree = new BlossomVTree();
            BlossomVEdge? augmentEdge = null;

            double augmentEps = KolmogorovWeightedPerfectMatching<V, E>.INFINITY;
            double oppositeEps;

            for (BlossomVNode? root = state.nodes[state.nodeNum].treeSiblingNext; root != null; root = root.treeSiblingNext)
            {
                root.tree!.nextTree = null;
            }

            for (BlossomVNode? root = state.nodes[state.nodeNum].treeSiblingNext; root != null; root = root.treeSiblingNext)
            {
                BlossomVTree startTree = root.tree!;
                if (startTree.nextTree != null)
                {
                    // this tree is present in some connected component and has been processed already
                    continue;
                }
                double eps = startTree.accumulatedEps;

                startTree.nextTree = startTree;
                BlossomVTree connectedComponentLast = startTree;

                BlossomVTree currentTree = startTree;
                while (true)
                {
                    for (BlossomVTree.TreeEdgeEnumerator enumerator = currentTree!.GetTreeEdgeEnumerator(); enumerator.MoveNext();)
                    {
                        BlossomVTreeEdge currentEdge = enumerator.Current;
                        int dir = enumerator.GetCurrentDirection();
                        BlossomVTree opposite = currentEdge.head[dir]!;
                        double plusPlusEps = KolmogorovWeightedPerfectMatching<V, E>.INFINITY;
                        int dirRev = 1 - dir;

                        if (!currentEdge.plusPlusEdges.IsEmpty())
                        {
                            plusPlusEps = currentEdge.plusPlusEdges.FindMin().GetKey() - currentTree.eps
                                - opposite.eps;
                            if (augmentEps > plusPlusEps)
                            {
                                augmentEps = plusPlusEps;
                                augmentEdge = currentEdge.plusPlusEdges.FindMin().GetValue();
                            }
                        }
                        if (opposite.nextTree != null && opposite.nextTree != dummyTree)
                        {
                            // opposite tree is in the same connected component
                            // since the trees in the same connected component have the same dual change
                            // we don't have to check (-, +) edges in this tree edge
                            if (2 * eps > plusPlusEps)
                            {
                                eps = plusPlusEps / 2;
                            }
                            continue;
                        }

                        double[] plusMinusEps = new double[2];
                        plusMinusEps[dir] = KolmogorovWeightedPerfectMatching<V, E>.INFINITY;
                        if (!currentEdge.GetCurrentPlusMinusHeap(dir).IsEmpty())
                        {
                            plusMinusEps[dir] =
                                currentEdge.GetCurrentPlusMinusHeap(dir).FindMin().GetKey()
                                    - currentTree.eps + opposite.eps;
                        }
                        plusMinusEps[dirRev] = KolmogorovWeightedPerfectMatching<V, E>.INFINITY;
                        if (!currentEdge.GetCurrentPlusMinusHeap(dirRev).IsEmpty())
                        {
                            plusMinusEps[dirRev] =
                                currentEdge.GetCurrentPlusMinusHeap(dirRev).FindMin().GetKey()
                                    - opposite.eps + currentTree.eps;
                        }
                        if (opposite.nextTree == dummyTree)
                        {
                            // opposite tree is in another connected component and has valid accumulated
                            // eps
                            oppositeEps = opposite.accumulatedEps;
                        }
                        else if (plusMinusEps[0] > 0 && plusMinusEps[1] > 0)
                        {
                            // this tree edge doesn't contain any tight (-, +) cross-tree edge and
                            // opposite tree
                            // hasn't been processed yet.
                            oppositeEps = 0;
                        }
                        else
                        {
                            // opposite hasn't been processed and there is a tight (-, +) cross-tree
                            // edge between
                            // current tree and opposite tree => we add opposite to the current
                            // connected component
                            connectedComponentLast.nextTree = opposite;
                            connectedComponentLast = opposite.nextTree = opposite;
                            if (eps > opposite.accumulatedEps)
                            {
                                // eps of the connected component can't be greater than the minimum
                                // accumulated eps among trees in the connected component
                                eps = opposite.accumulatedEps;
                            }
                            continue;
                        }
                        if (eps > plusPlusEps - oppositeEps)
                        {
                            // eps is bounded by the resulting slack of a (+, +) cross-tree edge
                            eps = plusPlusEps - oppositeEps;
                        }
                        if (eps > plusMinusEps[dir] + oppositeEps)
                        {
                            // eps is bounded by the resulting slack of a (+, -) cross-tree edge in the
                            // current direction
                            eps = plusMinusEps[dir] + oppositeEps;
                        }
                    }
                    if (currentTree.nextTree == currentTree)
                    {
                        // the end of the connected component
                        break;
                    }
                    currentTree = currentTree.nextTree!;
                }

                if (eps > KolmogorovWeightedPerfectMatching<V, E>.NO_PERFECT_MATCHING_THRESHOLD)
                {
                    throw new ArgumentException(KolmogorovWeightedPerfectMatching<V, E>.NO_PERFECT_MATCHING);
                }

                // apply dual change to all trees in the connected component
                BlossomVTree nextTree = startTree;
                do
                {
                    currentTree = nextTree!;
                    nextTree = nextTree.nextTree!;
                    currentTree.nextTree = dummyTree;
                    currentTree.accumulatedEps = eps;
                } while (currentTree != nextTree);
            }

            if (augmentEdge != null && augmentEps - augmentEdge.head[0].tree!.accumulatedEps - augmentEdge.head[1].tree!.accumulatedEps <= 0)
            {
                return augmentEdge;
            }

            return null;
        }

        /// <summary>
        /// Updates duals by iterating through trees and greedily increasing their dual variables.
        /// </summary>
        /// <returns>edge which can be augmented if possible, null otherwise</returns>
        private BlossomVEdge? MultipleTreeFixedDelta()
        {
            if (DEBUG)
            {
                Console.WriteLine("Multiple tree fixed delta approach");
            }

            BlossomVEdge? augmentEdge = null;
            double eps = KolmogorovWeightedPerfectMatching<V, E>.INFINITY;
            double augmentEps = KolmogorovWeightedPerfectMatching<V, E>.INFINITY;

            for (BlossomVNode? root = state.nodes[state.nodeNum].treeSiblingNext; root != null; root = root.treeSiblingNext)
            {
                BlossomVTree tree = root.tree!;
                double treeEps = tree.eps;
                eps = Math.Min(eps, tree.accumulatedEps);

                // iterate only through outgoing tree edges so that every edge is considered only once
                for (BlossomVTreeEdge outgoingTreeEdge = tree.first[0]; outgoingTreeEdge != null;
                    outgoingTreeEdge = outgoingTreeEdge.next[0])
                {
                    // since all epsilons are equal we don't have to check (+, -) cross tree edges
                    if (!outgoingTreeEdge.plusPlusEdges.IsEmpty())
                    {
                        BlossomVEdge varEdge = outgoingTreeEdge.plusPlusEdges.FindMin().GetValue()!;
                        double slack = varEdge.slack - treeEps - outgoingTreeEdge.head[0]!.eps;
                        eps = Math.Min(eps, slack / 2);
                        if (augmentEps > slack)
                        {
                            augmentEps = slack;
                            augmentEdge = varEdge;
                        }
                    }
                }
            }

            if (eps > KolmogorovWeightedPerfectMatching<V, E>.NO_PERFECT_MATCHING_THRESHOLD)
            {
                throw new ArgumentException(KolmogorovWeightedPerfectMatching<V, E>.NO_PERFECT_MATCHING);
            }
            for (BlossomVNode? root = state.nodes[state.nodeNum].treeSiblingNext; root != null; root = root.treeSiblingNext)
            {
                root.tree!.accumulatedEps = eps;
            }
            if (augmentEps <= 2 * eps)
            {
                return augmentEdge;
            }

            return null;
        }

        /// <summary>
        /// Gets time in nano seconds
        /// </summary>
        /// <returns>time in nanoseconds</returns>
        private static long GetNanoTime()
        {
            long nano = 10000L * Stopwatch.GetTimestamp();
            nano /= TimeSpan.TicksPerMillisecond;
            nano *= 100L;
            return nano;
        }

    }
}
