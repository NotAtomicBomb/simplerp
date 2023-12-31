using Sandbox;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MyGame
{
	public partial class Job : Entity
	{

		public Job() 
		{
			Transmit = TransmitType.Always;
		}

		/// <summary>
		/// Default job.
		/// </summary>
		public static readonly Job None = new Job {Name = "Unemployed", Description = "No Job", Wage = 0};

		/// <summary>
		/// Name of the job.
		/// </summary>
		[Net]
		public string Name { get; set; } = "Unemployed";

		/// <summary>
		/// Description of the job.
		/// </summary>
		[Net]
		public string Description { get; set; } = "";

		/// <summary>
		/// Description of the job.
		/// </summary>
		[Net]
		public string Category { get; set; } = "Civilian";

		/// <summary>
		/// The max amount of players that can have the job.
		/// </summary>
		[Net]
		public int MaxPlayers { get; set; } = 0;

		/// <summary>
		/// The current number of players in the job.
		/// </summary>
		[Net, Predicted]
		public int CurrentNumberOfPlayers { get; set; } = 0;

		/// <summary>
		/// How much the job will give per pay period.
		/// </summary>
		[Net]
		public int Wage { get; set; } = 0;

		/// <summary>
		/// A bool that gets whether the current job is full
		/// </summary>
		public bool IsFull
		{
			get => CurrentNumberOfPlayers == MaxPlayers;
		}

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
			return $"{Name};{Description};{FormattedWage};{MaxPlayers};{CurrentNumberOfPlayers}";
		}

	}
}
