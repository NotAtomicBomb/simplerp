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
			Job newJob = Job.Jobs.Find( job => job.Name == jobName );
			if ( newJob != null && player != null )
			{
				player.SwitchJob( newJob );
			}
				
			
		}

		/// <summary>
		/// A command that allows the player the check their current job.
		/// </summary>
		[ConCmd.Server( "current_job" )]
		public static void CurrentJob()
		{
			
			IClient callingClient = ConsoleSystem.Caller;
			Player player = Player.GetPlayer( callingClient.SteamId);
			Log.Info( player.Job );
		}
	}
}
