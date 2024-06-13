using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tournament_Manager.Data
{
    internal enum Result
    {
        WHITE_WIN = 1,
        BLACK_WIN = -2,
        WHITE_LOSS = 2,
        BLACK_LOSS = -1,
        DRAW = 0
    }
}
