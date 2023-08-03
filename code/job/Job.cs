using Sandbox;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MyGame
{
	public class Job : BaseNetworkable
	{
		/// <summary>
		/// Default job.
		/// </summary>
		public static readonly Job None = new Job {Name = "Unemployed", Description = "No Job", Wage = 0};

		/// <summary>
		/// List of jobs.
		/// </summary>
		public static List<Job> Jobs { get; set; } = new List<Job>();


		/// <summary>
		/// Name of the job.
		/// </summary>
		public string Name { get; set; }
		/// <summary>
		/// Description of the job.
		/// </summary>
		public string Description { get; set; }
		/// <summary>
		/// The max amount of players that can have the job.
		/// </summary>
		public int MaxPlayers { get; set;  } = 0;
		/// <summary>
		/// The current number of players in the job.
		/// </summary>
		[JsonIgnore]
		public int CurrentNumberOfPlayer { get; set; } = 0;
		/// <summary>
		/// How much the job will give per pay period.
		/// </summary>
		public int Wage { get; set; }
		/// <summary>
		/// The wage in proper string format.
		/// </summary>
		[JsonIgnore]
		public string FormattedWage 
		{
			get => Wage.ToString("C0");
		}

		public override string ToString()
		{
			return $"{Name};{Description};{FormattedWage};{MaxPlayers};{CurrentNumberOfPlayer}";
		}


	}
}
