using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace Tournament_Manager.Logic.Matching.BlossomV
{

    /// <summary>
    /// BlossomVOptions that define the strategies to use during the algorithm for updating duals and
    /// initializing the matching.<para/>
    /// 
    /// According to the experimental results, the greedy initialization substantially speeds up the
    /// algorithm.
    /// </summary>
    internal class BlossomVOptions
    {

        /// <summary>
        /// Enum for types of matching initialization
        /// </summary>
        public enum InitializationType
        {
            GREEDY,
            NONE,
            FRACTIONAL
        }

        /// <summary>
        /// Enum for chossing dual updates strategy
        /// </summary>
        public enum DualUpdateStrategy
        {
            MULTIPLE_TREE_FIXED_DELTA,
            MULTIPLE_TREE_CONNECTED_COMPONENTS
        }

        /// <summary>
        /// Returns the name of the initialization type
        /// </summary>
        /// <param name="initializationType">the initialization type</param>
        /// <returns>the name of the initialization type</returns>
        private static string InitializationTypeToString(InitializationType initializationType)
        {
            switch (initializationType)
            {
                case InitializationType.NONE: return "None";
                case InitializationType.GREEDY: return "Greedy initialization";
                default: return "Fractional matching initialization"; //InitializationType.FRACTIONAL
            }
        }

        /// <summary>
        /// Returns the name of the dual updates strategy
        /// </summary>
        /// <param name="dualUpdateStrategy">the dual strategy</param>
        /// <returns>the name of the dual updates strategy</returns>
        private static string DualUpdateStrategyToString(DualUpdateStrategy dualUpdateStrategy)
        {
            switch (dualUpdateStrategy)
            {
                case DualUpdateStrategy.MULTIPLE_TREE_FIXED_DELTA: return "Multiple tree fixed delta";
                default: return "Multiple tree connected components"; //MULTIPLE_TREE_CONNECTED_COMPONENTS
            }
        }


        /// <summary>
        /// All possible strategies
        /// </summary>
        public static readonly BlossomVOptions[] ALL_OPTIONS = new BlossomVOptions[] 
        {
            new BlossomVOptions(InitializationType.NONE, DualUpdateStrategy.MULTIPLE_TREE_CONNECTED_COMPONENTS, true, true), //[0]
            new BlossomVOptions(InitializationType.NONE, DualUpdateStrategy.MULTIPLE_TREE_CONNECTED_COMPONENTS, true, false), //[1]
            new BlossomVOptions(InitializationType.NONE, DualUpdateStrategy.MULTIPLE_TREE_CONNECTED_COMPONENTS, false, true), //[2]
            new BlossomVOptions(InitializationType.NONE, DualUpdateStrategy.MULTIPLE_TREE_CONNECTED_COMPONENTS, false, false), //[3]
            new BlossomVOptions(InitializationType.NONE, DualUpdateStrategy.MULTIPLE_TREE_FIXED_DELTA, true, true), //[4]
            new BlossomVOptions(InitializationType.NONE, DualUpdateStrategy.MULTIPLE_TREE_FIXED_DELTA, true, false), //[5]
            new BlossomVOptions(InitializationType.NONE, DualUpdateStrategy.MULTIPLE_TREE_FIXED_DELTA, false, true), //[6]
            new BlossomVOptions(InitializationType.NONE, DualUpdateStrategy.MULTIPLE_TREE_FIXED_DELTA, false, false), //[7]
            new BlossomVOptions(InitializationType.GREEDY, DualUpdateStrategy.MULTIPLE_TREE_CONNECTED_COMPONENTS, true, true), //[8]
            new BlossomVOptions(InitializationType.GREEDY, DualUpdateStrategy.MULTIPLE_TREE_CONNECTED_COMPONENTS, true, false), //[9]
            new BlossomVOptions(InitializationType.GREEDY, DualUpdateStrategy.MULTIPLE_TREE_CONNECTED_COMPONENTS, false, true), //[10]
            new BlossomVOptions(InitializationType.GREEDY, DualUpdateStrategy.MULTIPLE_TREE_CONNECTED_COMPONENTS, false, false), //[11]
            new BlossomVOptions(InitializationType.GREEDY, DualUpdateStrategy.MULTIPLE_TREE_FIXED_DELTA, true, true), //[12]
            new BlossomVOptions(InitializationType.GREEDY, DualUpdateStrategy.MULTIPLE_TREE_FIXED_DELTA, true, false), //[13]
            new BlossomVOptions(InitializationType.GREEDY, DualUpdateStrategy.MULTIPLE_TREE_FIXED_DELTA, false, true), //[14]
            new BlossomVOptions(InitializationType.GREEDY, DualUpdateStrategy.MULTIPLE_TREE_FIXED_DELTA, false, false), //[15]
            new BlossomVOptions(InitializationType.FRACTIONAL, DualUpdateStrategy.MULTIPLE_TREE_CONNECTED_COMPONENTS, true, true), //[16]
            new BlossomVOptions(InitializationType.FRACTIONAL, DualUpdateStrategy.MULTIPLE_TREE_CONNECTED_COMPONENTS, true, false), //[17]
            new BlossomVOptions(InitializationType.FRACTIONAL, DualUpdateStrategy.MULTIPLE_TREE_CONNECTED_COMPONENTS, false, true), //[18]
            new BlossomVOptions(InitializationType.FRACTIONAL, DualUpdateStrategy.MULTIPLE_TREE_CONNECTED_COMPONENTS, false, false), //[19]
            new BlossomVOptions(InitializationType.FRACTIONAL, DualUpdateStrategy.MULTIPLE_TREE_FIXED_DELTA, true, true), //[20]
            new BlossomVOptions(InitializationType.FRACTIONAL, DualUpdateStrategy.MULTIPLE_TREE_FIXED_DELTA, true, false), //[21]
            new BlossomVOptions(InitializationType.FRACTIONAL, DualUpdateStrategy.MULTIPLE_TREE_FIXED_DELTA, false, true), //[22]
            new BlossomVOptions(InitializationType.FRACTIONAL, DualUpdateStrategy.MULTIPLE_TREE_FIXED_DELTA, false, false), //[23]
        };

        /// <summary>
        /// Default algorithm initialization type
        /// </summary>
        private static readonly InitializationType DEFAULT_INITIALIZATION_TYPE = InitializationType.FRACTIONAL;

        /// <summary>
        /// Default dual updates strategy
        /// </summary>
        private static readonly DualUpdateStrategy DEFAULT_DUAL_UPDATE_TYPE = DualUpdateStrategy.MULTIPLE_TREE_FIXED_DELTA;

        /// <summary>
        /// Default value for the flag <see cref="DEFAULT_UPDATE_DUALS_BEFORE"/>
        /// </summary>
        private static readonly bool DEFAULT_UPDATE_DUALS_BEFORE = true;

        /// <summary>
        /// Default value for the flag <see cref="DEFAULT_UPDATE_DUALS_AFTER"/>
        /// </summary>
        private static readonly bool DEFAULT_UPDATE_DUALS_AFTER = false;


        /// <summary>
        /// What greedy strategy to use to perform a global dual update
        /// </summary>
        internal DualUpdateStrategy dualUpdateStrategy;

        /// <summary>
        /// What strategy to choose to initialize the matching before the main phase of the algorithm
        /// </summary>
        internal InitializationType initializationType;

        /// <summary>
        /// Whether to update duals of the tree before growth
        /// </summary>
        internal bool updateDualsBefore;

        /// <summary>
        /// Whether to update duals of the tree after growth
        /// </summary>
        internal bool updateDualsAfter;


        /// <summary>
        /// Constructs a custom set of options for the algorithm
        /// </summary>
        /// <param name="initializationType">strategy for initializing the matching</param>
        /// <param name="dualUpdateStrategy">greedy strategy to update dual variables globally</param>
        /// <param name="updateDualsBefore">whether to update duals of the tree before growth</param>
        /// <param name="updateDualsAfter">whether to update duals of the tree after growth</param>
        public BlossomVOptions(InitializationType initializationType, DualUpdateStrategy dualUpdateStrategy,
            bool updateDualsBefore, bool updateDualsAfter)
        {
            this.dualUpdateStrategy = dualUpdateStrategy;
            this.initializationType = initializationType;
            this.updateDualsBefore = updateDualsBefore;
            this.updateDualsAfter = updateDualsAfter;
        }

        /// <summary>
        /// Constructs a new options instance with an <c>initializationType</c>
        /// </summary>
        /// <param name="initializationType">defines a strategy to use to initialize the matching</param>
        public BlossomVOptions(InitializationType initializationType) : 
            this(initializationType, DEFAULT_DUAL_UPDATE_TYPE, DEFAULT_UPDATE_DUALS_BEFORE, DEFAULT_UPDATE_DUALS_AFTER)
        {
        }

        /// <summary>
        /// Constructs a default set of options for the algorithm
        /// </summary>
        public BlossomVOptions() :
            this(DEFAULT_INITIALIZATION_TYPE, DEFAULT_DUAL_UPDATE_TYPE, DEFAULT_UPDATE_DUALS_BEFORE, DEFAULT_UPDATE_DUALS_AFTER)
        {
        }


        public override string ToString()
        {
            return "BlossomVOptions{initializationType=" + InitializationTypeToString(this.initializationType) +
                ", dualUpdateStrategy=" + DualUpdateStrategyToString(this.dualUpdateStrategy) +
                ", updateDualsBefore=" + updateDualsBefore + ", updateDualsAfter=" + updateDualsAfter + "}";
        }

        /// <summary>
        /// Returns the <see cref="updateDualsBefore"/> flag
        /// </summary>
        /// <returns>the flag <see cref="updateDualsBefore"/></returns>
        public bool IsUpdateDualsBefore()
        {
            return this.updateDualsBefore;
        }

        /// <summary>
        /// Returns the <see cref="updateDualsAfter"/> flag
        /// </summary>
        /// <returns>the flag <see cref="updateDualsAfter"/></returns>
        public bool IsUpdateDualsAfter()
        {
            return this.updateDualsAfter;
        }

        /// <summary>
        /// Returns dual updates strategy
        /// </summary>
        /// <returns>dual updates strategy</returns>
        public DualUpdateStrategy GetDualUpdateStrategy()
        {
            return this.dualUpdateStrategy;
        }

        /// <summary>
        /// Returns initialization type
        /// </summary>
        /// <returns>initialization type</returns>
        public InitializationType GetInitializationType()
        {
            return this.initializationType;
        }
    }
}
