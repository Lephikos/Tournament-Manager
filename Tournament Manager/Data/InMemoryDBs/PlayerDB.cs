using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tournament_Manager.Data.InMemoryDBs
{
    internal class PlayerDB
    {

        /// <summary>
        /// Singleton instance
        /// </summary>
        private static PlayerDB? instance;

        /// <summary>
        /// In-Memory player database
        /// </summary>
        private readonly Dictionary<Guid, Player> players = new Dictionary<Guid, Player>();

        
        public PlayerDB GetInstance()
        {
            if (instance == null)
            {
                instance = new PlayerDB();
            }
            return instance;
        }


        public bool AddPlayer(Player player)
        {
            if (players.ContainsKey(player.guid))
            {
                return false;
            }

            players.Add(player.guid, player);
            return true;
        }

        public void RemovePlayer(Player player)
        {
            players.Remove(player.guid);
        }

        public Boolean UpdatePlayer(int guid)
        {
            return true;
        }



    }
}
