using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tournament_Manager
{
    internal class Player
    {

        public Guid guid = Guid.NewGuid();

        /// <summary>
        /// Displayed player name
        /// </summary>
        public string name;

        /// <summary>
        /// Player rating
        /// </summary>
        public int dwz;

        public Player(string name, int dwz)
        {
            this.name = name;
            this.dwz = dwz;
        }

    }
}
