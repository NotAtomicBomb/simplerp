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
	}
}
