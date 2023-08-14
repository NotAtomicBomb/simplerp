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
		/// <summary>
		/// How often to payout job wages
		/// </summary>
		private float PayPeriod => 1800f; // in Seconds, 1800 = 30 mins
		public bool FirstTimeJoining { get; internal set; } = true;

		/// <summary>
		/// The current job of the player.
		/// </summary>
		[Net]
		public Job Job { get; set; }

		/// <summary>
		/// The money of the player.
		/// </summary>
		[Net] 
		public long Money { get; protected set; } = 0;

		/// <summary>
		/// The time since last pay period
		/// </summary>
		[Net, Predicted]
		public TimeSince TimeSinceLastPay { get; protected set; }
		/// <summary>
		/// The time since last job switch
		/// </summary>
		[Net, Predicted]
		public TimeSince TimeSinceLastJobSwitch { get; protected set; } = 60f;

		public override void Simulate( IClient cl )
		{
			base.Simulate( cl );
			PayPlayer();
		}

		/// <summary>
		/// The money in string format.
		/// </summary>
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
		/// Pays the player their job wage.
		/// </summary>
		private void PayPlayer()
		{
			if ( TimeSinceLastPay >= PayPeriod )
			{
				TimeSinceLastPay = 0;
				AddMoney( Job.Wage );
				Log.Info( "You've been paid" );
			}
		}

		/// <summary>
		/// Gets the <see cref="Player"/> via the SteamId.
		/// </summary>
		/// <param name="steamId">The SteamId of the player you are trying to get. </param>
		/// <returns><see cref="Player"/></returns>
		public static Player GetPlayer( long steamId )
		{
			IDictionary<string ,Player> players = (GameManager.Current as SimpleRp).Players;

			Player player = players[steamId.ToString()];
			return player;
		}

		/// <summary>
		/// Gets the <see cref="Player"/> via the SteamId.
		/// </summary>
		/// <param name="playerName">The Player name of the player you are trying to get. </param>
		/// <returns><see cref="Player"/></returns>
		public static Player GetPlayer( string playerName )
		{
			IDictionary<string, Player> players = (GameManager.Current as SimpleRp).Players;

			Player player = players.FirstOrDefault(x => x.Value.Client.Name.ToLower() == playerName.ToLower()).Value;
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
			if ( job == Job ) 
			{
				Log.Warning( "You are already that job." );
				return;
			}

			if (TimeSinceLastJobSwitch < 60f )
			{
				Log.Info( $"You must wait {(60f - TimeSinceLastJobSwitch):0}s before switching jobs." );
				return;
			}

			if ( job.MaxPlayers != 0 )
			{
				if ( !job.IsFull )
				{
					if ( Job != null )
					{
						Job.CurrentNumberOfPlayers -= 1;
					}
					Job = job;
					Job.CurrentNumberOfPlayers++;
					TimeSinceLastJobSwitch = 0;
					Log.Info( $"{Client.Name} switched jobs to {Job.Name}." );
					SetInfo();
				}
				else
				{
					Log.Warning( "Job is full." );
				}
			}
			else
			{
				if ( Job != null )
				{
					Job.CurrentNumberOfPlayers -= 1;
				}
				Job = job;
				Job.CurrentNumberOfPlayers++;
				TimeSinceLastJobSwitch = 0;
				SetInfo();
			}
		}

	}
}
