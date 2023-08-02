using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyGame
{
	public partial class Player
	{
		public static List<Player> Players { get; set; } =  new List<Player>();
		public IClient Client { get; set; }
		public Job Job { get; set; }

		public Player(IClient client) 
		{ 
			Client = client;
			Players.Add( this );
		}

		public static Player GetPlayer(long steamId)
		{
			Player player = Player.Players.FirstOrDefault( player => player.Client.SteamId == steamId );
			Log.Info( steamId );
			Log.Info( Player.Players.Count);
			return player;
		}

	}
}
