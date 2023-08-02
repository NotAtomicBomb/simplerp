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
		public int CurrentNumberOfPlayer { get; set; } = 0;
		/// <summary>
		/// How much the job will give per pay period.
		/// </summary>
		public int Wage { get; set; }
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
			bool isJobFull = (job.CurrentNumberOfPlayer + 1) > job.MaxPlayers;

			if (pawn != null && job.MaxPlayers != 0 ) {
				if (!isJobFull)
				{
					pawn.Job.CurrentNumberOfPlayer -= 1;
					pawn.Job = job;
					job.CurrentNumberOfPlayer++;
				}
				else
				{
					Log.Warning( "Job is full." );
				}
			}
			else if (pawn != null)
			{
				pawn.Job = job;
				job.CurrentNumberOfPlayer++;
			}
		}

	}
}
