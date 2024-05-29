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
        private readonly Dictionary<int, Player> players = new Dictionary<int, Player>();

        
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
            if (players.ContainsKey(player.id))
            {
                return false;
            }

            players.Add(player.id, player);
            return true;
        }

        public void RemovePlayer(Player player)
        {
            players.Remove(player.id);
        }

        public Boolean UpdatePlayer(int id, )



    }
}
