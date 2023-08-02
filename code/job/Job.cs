using System.Collections.Generic;


namespace MyGame
{
	public class Job
	{
		public static readonly Job None = new Job {Name = "Unemployed", Description = "No Job", Wage = 0};

		public static List<Job> Jobs { get; set; } = new List<Job>();

		/// <summary>
		/// Name of the job.
		/// </summary>
		public virtual string Name { get; set; }
		/// <summary>
		/// Description of the job.
		/// </summary>
		public virtual string Description { get; set; }
		/// <summary>
		/// The max amount of players that can have the job.
		/// </summary>
		public virtual int MaxPlayers { get; } = 0;
		/// <summary>
		/// The current number of players in the job.
		/// </summary>
		public int CurrentNumberOfPlayer { get; set; } = 0;
		/// <summary>
		/// How much the job will give per pay period.
		/// </summary>
		public virtual int Wage { get; set; }
		/// <summary>
		/// The wage in proper string format.
		/// </summary>
		public string FormattedWage 
		{
			get => Wage.ToString("C0");
		}

		public override string ToString()
		{
			return $"{Name} {Description} {FormattedWage}";
		}

		public static void SwitchJob(Pawn pawn, Job job)
		{
			if (pawn != null) { 

			}
		}

	}
}
