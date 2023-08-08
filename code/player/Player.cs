using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MyGame
{ 
	public partial class Player : Pawn
	{
		public bool FirstTimeJoining { get; internal set; } = true;

		/// <summary>
		/// The current job of the player.
		/// </summary>
		[Net, JsonIgnore]
		public Job Job { get; set; }

		/// <summary>
		/// The money of the player.
		/// </summary>
		[Net] 
		public long Money { get; protected set; } = 0;

		/// <summary>
		/// The money in string format.
		/// </summary>
		[JsonIgnore]
		public string FormattedMoney
		{
			get => Money.ToString("C0");
		}

		/// <summary>
		/// Add money to the player.
		/// </summary>
		/// <param name="money">The money to add to the player.</param>
		internal void AddMoney( long money )
		{
			Money += money;
			SetInfo();
		}

		/// <summary>
		/// Subtracts money from the player.
		/// </summary>
		/// <param name="money">The money to subtract from the player.</param>
		internal bool SubtractMoney( long money )
		{
			if( (Money - money) < 0 )
			{
				Log.Warning( "Don't have enough money." );
				return false;
			}

			Money -= money;
			SetInfo();
			return true;
		}

		/// <summary>
		/// Gets the <see cref="Player"/> via the SteamId.
		/// </summary>
		/// <param name="steamId">The SteamId of the player you are trying to get. </param>
		/// <returns><see cref="Player"/></returns>
		public static Player GetPlayer( long steamId )
		{
			IDictionary<string ,Player> players = (GameManager.Current as MyGame).Players;

			Player player = players[steamId.ToString()];
			return player;
		}

		/// <summary>
		/// Sets the current info onto the Client.
		/// </summary>
		private void SetInfo()
		{
			if ( Client != null )
			{
				PlayerInfo.UpdateInfo(To.Single(this)); // Ignore the error, it works.
				PlayerList.UpdateInfo(); // Ignore the error, it works.
				JobMenu.UpdateInfo(); // Ignore the error, it works.
			}
		}

		/// <summary>
		/// Switches the players job to the given job.
		/// </summary>
		/// <param name="job">The job you want to switch to.</param>
		public void SwitchJob( Job job )
		{
			
			bool isJobFull = (job.CurrentNumberOfPlayers + 1) > job.MaxPlayers;

			if ( job.MaxPlayers != 0 )
			{
				if ( job != Job )
				{
					if ( !isJobFull )
					{
						Job.CurrentNumberOfPlayers -= 1;
						Job = job;
						job.CurrentNumberOfPlayers++;
						Log.Info( $"{Client.Name} switched jobs to {job.Name}." );
						SetInfo();
					}
					else
					{
						Log.Warning( "Job is full." );
					}
				}
				else
				{
					Log.Warning( "You are already that job." );
				}
			}
			else
			{
				Log.Info( job.ToString() );
				Job = job;
				job.CurrentNumberOfPlayers++;
				SetInfo();
			}
		}

	}
}
