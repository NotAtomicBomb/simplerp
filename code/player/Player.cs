using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace MyGame
{
	public partial class Player : Pawn
	{
		/// <summary>
		/// The current job of the player.
		/// </summary>
		public Job Job { get; set; }

		/// <summary>
		/// The money of the player.
		/// </summary>
		[Net]
		public long Money { get; protected set; } = 0;

		private string FormattedMoney 
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
			IList<Player> players = (GameManager.Current as MyGame).Players;

			Player player = players.FirstOrDefault( player => player.Client.SteamId == steamId );
			return player;
		}

		/// <summary>
		/// Sets the current info onto the Client.
		/// </summary>
		private void SetInfo()
		{
			if ( Client != null )
			{
				Client.SetValue( "job", Job.ToString() );
				Client.SetValue( "money", FormattedMoney );
			}
		}

		/// <summary>
		/// Switches the players job to the given job.
		/// </summary>
		/// <param name="job">The job you want to switch to.</param>
		public void SwitchJob( Job job )
		{
			
			bool isJobFull = (job.CurrentNumberOfPlayer + 1) > job.MaxPlayers;

			if ( job.MaxPlayers != 0 )
			{
				if ( job != Job )
				{
					if ( !isJobFull )
					{
						Job.CurrentNumberOfPlayer -= 1;
						Job = job;
						job.CurrentNumberOfPlayer++;
						Log.Info( $"{Client.Name} switch jobs to {job.Name}." );
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
				job.CurrentNumberOfPlayer++;
				SetInfo();
			}
		}

	}
}
