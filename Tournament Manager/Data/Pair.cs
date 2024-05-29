using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tournament_Manager.Data
{
    internal class Pair<E>
    {

        public E left;
        public E right;

        public Pair(E left, E right)
        {
            this.left = left;
            this.right = right;
        }

    }
}
