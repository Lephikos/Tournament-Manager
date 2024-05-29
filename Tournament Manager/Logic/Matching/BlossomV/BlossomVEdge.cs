using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tournament_Manager.Logic.Matching.BlossomV
{
    internal class BlossomVEdge
    {

        internal BlossomVEdge[] next;

        internal BlossomVEdge[] prev;

        internal BlossomVNode[] head;

        public BlossomVNode GetOpposite(BlossomVNode node)
        {
            return new BlossomVNode(0);
        }

    }
}
