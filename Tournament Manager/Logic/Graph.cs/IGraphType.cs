using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tournament_Manager.Logic.Graph.cs
{
    internal interface IGraphType
    {

        public bool IsDirected();

        public bool IsUndirected();

        public bool IsMixed();

        public bool IsAllowingMultipleEdges();

        public bool IsAllowingSelfLoops();

        public bool IsAllowingCycles();

        public bool IsWeighted();

        public bool IsSimple();

        public bool IsPseudograph();

        public bool IsMultigraph();

        public bool IsModifiable();


        public IGraphType AsDirected();

        public IGraphType AsUndirected();

        public IGraphType AsMixed();

        public IGraphType AsUnweighted();

        public IGraphType AsWeighted();

        public IGraphType AsModifiable();

        public IGraphType AsUnmodifiable();
    }
}
