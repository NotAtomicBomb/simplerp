using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyGame
{
	internal class ConsoleCommands
	{
		[ConCmd.Server("switch_job")]
		public static void SwitchJob(string jobName)
		{
			
			Player player = Player.GetPlayer(Game.LocalClient.SteamId);
			Log.Info(player); 
			Job newJob = Job.Jobs.Find( job => job.Name == jobName );
			Log.Info( newJob );
			if ( newJob != null && player != null )
			{
				Job.SwitchJob( player, newJob );
				Log.Info( player );
				Log.Warning( "Job switched" );
			}
				
			
		}

		[ConCmd.Client( "current_job" )]
		public static void CurrentJob()
		{
			Player player = Player.GetPlayer( Game.LocalClient.SteamId );
			Log.Info( player.Job.Name );
		}
	}
}
