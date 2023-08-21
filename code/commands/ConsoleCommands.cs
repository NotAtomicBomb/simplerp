using Sandbox;

namespace MyGame
{
	internal class ConsoleCommands
	{
		/// <summary>
		/// A command that allows the player to switch jobs.
		/// </summary>
		/// <param name="jobName">The job's string name you want to switch to.</param>
		[ConCmd.Server("switch_job")]
		public static void SwitchJob(string jobName)
		{
			// The client calling the command.
			IClient callingClient = ConsoleSystem.Caller;
			Player player = Player.GetPlayer(callingClient.SteamId );
			Job newJob = (GameManager.Current as SimpleRp).Jobs[jobName];
			if ( newJob != null && player != null )
			{
				player.SwitchJob( newJob );
			}
			else
			{
				Log.Warning( "Job not found." );
			}
			
		}

		[ConCmd.Admin( "switch_player_job" )]
		public static void SwitchJob( string playerName, string jobName )
		{
			Player player = Player.GetPlayer( playerName );
			Job newJob = (GameManager.Current as SimpleRp).Jobs[jobName];
			if ( newJob != null && player != null )
			{
				player.SwitchJob( newJob );
			}
			else
			{
				Log.Warning( "Job not found." );
			}

		}

		[ConCmd.Admin( "noclip" )]
		static void DoPlayerNoclip()
		{
			if ( ConsoleSystem.Caller.Pawn is Player player )
			{
				if ( player.DevController is NoclipController )
				{
					player.DevController = null;
				}
				else
				{
					player.DevController = new NoclipController();
				}
			}
		}

		[ConCmd.Admin( "kill" )]
		static void DoPlayerSuicide()
		{
			if ( ConsoleSystem.Caller.Pawn is Player player )
			{
				player.TakeDamage( new DamageInfo { Damage = player.Health * 99 } );
			}
		}

	}
}
