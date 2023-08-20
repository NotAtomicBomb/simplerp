using Sandbox;
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
public partial class SimpleRp : GameManager
{
	[Net]
	public IDictionary<string, Player> Players { get; set; }

	[Net]
	public IDictionary<string, Job> Jobs { get; set; }

	/// <summary>
	/// Called when the game is created (on both the server and client)
	/// </summary>
	public SimpleRp()
	{
		if ( Game.IsClient )
		{
			Game.RootPanel = new Hud();
			Game.RootPanel = new PlayerList();
			Game.RootPanel = new JobMenu();
			Game.RootPanel = new Crosshair();
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
		player.DressFromClient( client );
		player.SwitchJob( Jobs["unemployed"] );
		player.Respawn();

		Players.Add(player.Client.SteamId.ToString() , player );
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
		Player player = cl.Pawn as Player;
		Players.Remove( player.Client.SteamId.ToString() );

		base.ClientDisconnect( cl, reason );
		
	}

	public override void DoPlayerDevCam( IClient client )
	{
		return;
	}

	/// <summary>
	/// Loads all of the <see cref="Job"/>s in jobs.json into the list <see cref="Jobs"/>
	/// </summary>
	private void LoadJobs()
	{
		Jobs =  FileSystem.Mounted.ReadJson<Dictionary<string, Job>>( "jobs.json" );
	}
}

