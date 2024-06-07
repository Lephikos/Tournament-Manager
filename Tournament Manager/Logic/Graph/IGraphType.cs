using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tournament_Manager.Logic.Graph
{
    internal interface IGraphType
    {

        public bool IsDirected();

        public bool IsWeighted();


        public IGraphType AsWeighted();

    }
}
