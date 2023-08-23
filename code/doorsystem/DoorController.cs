using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

namespace MyGame
{
	public class DoorController : EntityComponent<DoorEntity>
	{
		public Player Owner { get; set; } = null;

		public string Title => Entity.Name;

		public void Simulate(IClient cl )
		{
			Log.Info( Entity.Name );
		}

	}
}
