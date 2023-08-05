
using Sandbox;
using Sandbox.Internal;
using System;
using System.Collections.Generic;
using System.Linq;

//
// You don't need to put things in a namespace, but it doesn't hurt.
//
namespace MyGame;

/// <summary>
/// This is your game class. This is an entity that is created serverside when
/// the game starts, and is replicated to the client.
///
/// You can use this to create things like HUDs and declare which player class
/// to use for spawned players.
/// </summary>
public partial class MyGame : Sandbox.GameManager
{
	[Net]
	public IList<Player> Players { get; set; }
	/// <summary>
	/// Called when the game is created (on both the server and client)
	/// </summary>
	public MyGame()
	{
		if ( Game.IsClient )
		{
			Game.RootPanel = new Hud();
			Game.RootPanel = new JobMenu();
			Game.RootPanel = new PlayerList();
		}

		LoadJobs();
	}

	/// <summary>
	/// A client has joined the server. Make them a pawn to play with
	/// </summary>
	public override void ClientJoined( IClient client )
	{
		base.ClientJoined( client );
		// Create a pawn for this client to play with
		Player player = new();
		client.Pawn = player;
		player.Username = client.Name;
		player.Ping = client.Ping;
		player.SwitchJob( Job.None );
		player.Respawn();
		player.DressFromClient( client );
		Players.Add( player );
		Log.Info( player );

		if ( player.FirstTimeJoining )
		{
			player.AddMoney( 500 );
			player.FirstTimeJoining = false;
		}

		// Get all of the spawnpoints
		var spawnpoints = Entity.All.OfType<SpawnPoint>();
		// chose a random one
		var randomSpawnPoint = spawnpoints.OrderBy( x => Guid.NewGuid() ).FirstOrDefault();

		// if it exists, place the pawn there
		if ( randomSpawnPoint != null )
		{
			var tx = randomSpawnPoint.Transform;
			tx.Position = tx.Position + Vector3.Up * 50.0f; // raise it up
			player.Transform = tx;
		}
	}

	public override void ClientDisconnect( IClient cl, NetworkDisconnectionReason reason )
	{
		base.ClientDisconnect( cl, reason );
		Player player = Player.GetPlayer( cl.SteamId );
		Players.Remove( player );
	}

	public override void DoPlayerDevCam( IClient client )
	{
		return;
	}

	/// <summary>
	/// Loads all of the jobs in jobs.json into the list <see cref="Job.Jobs"/>
	/// </summary>
	private void LoadJobs()
	{
		Job.Jobs.AddRange(FileSystem.Mounted.ReadJson<List<Job>>( "jobs.json" ));
	}
}

