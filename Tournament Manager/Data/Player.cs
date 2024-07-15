using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tournament_Manager.Data
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
		public int rating;

		public Player(string name, int rating)
		{
			this.name = name;
			this.rating = rating;
		}



		public override bool Equals(object? obj)
		{
			if (obj == null || obj is not Player) return false;
			if (obj == this) return true;

			Player other = (Player)obj;

			return guid.Equals(other.guid);
		}

		public override int GetHashCode()
		{
			return guid.GetHashCode();
		}
	}
}
