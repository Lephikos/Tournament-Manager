using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tournament_Manager.Logic.Matching.BlossomV
{
    /// <summary>
    /// Enum specifying the objective sense of the algorithm. <see cref="MAXIMIZE"/> means the
    /// goal is to maximize the linear programming objective value, <see cref="MINIMIZE"/> - to
    /// minimize the linear programming objective value.
    /// </summary>
    internal enum ObjectiveSense
    {
        MAXIMIZE,
        MINIMIZE
    }
}
