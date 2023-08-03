using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace MyGame
{
	public partial class Player : BaseNetworkable
	{
		[Net]
		public long SteamId { get; set; }
		[Net]
		public Job Job { get; set; }
		public Player(){ }
		
		public static Player GetPlayer(long steamId)
		{
			IList<Player> players = (GameManager.Current as MyGame).Players;
			Log.Info( players[0].SteamId );
			Log.Info( players[0] );

			Player player = players.FirstOrDefault( player => player.SteamId == steamId );
			Log.Info( player.GetHashCode() );
			return player;
			
			
		}

		public void SwitchJob( Job job )
		{
			
			bool isJobFull = (job.CurrentNumberOfPlayer + 1) > job.MaxPlayers;

			if ( job.MaxPlayers != 0 )
			{
				if ( !isJobFull )
				{
					Job.CurrentNumberOfPlayer -= 1;
					Job = job;
					job.CurrentNumberOfPlayer++;
				}
				else
				{
					Log.Warning( "Job is full." );
				}
			}
			else
			{
				Job = job;
				Log.Info( Job );
				job.CurrentNumberOfPlayer++;
			}
		}

		public void SetSteamId(long steamId )
		{
			SteamId = steamId;
		}

	}
}
