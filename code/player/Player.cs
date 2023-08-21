using Sandbox;
using System.Collections.Generic;
using System.Linq;


namespace MyGame
{ 
	public partial class Player : BasePlayer
	{
		private TimeSince timeSinceDropped;
		private TimeSince timeSinceJumpReleased;

		private DamageInfo lastDamage;

		[Net, Predicted]
		public bool ThirdPersonCamera { get; set; }

		/// <summary>
		/// The clothing container is what dresses the citizen
		/// </summary>
		public ClothingContainer Clothing = new();

		/// <summary>
		/// How often to payout job wages
		/// </summary>
		private float PayPeriod => 1800f; // in Seconds, 1800 = 30 mins
		public bool FirstTimeJoining { get; internal set; } = true;

		/// <summary>
		/// The current job of the player.
		/// </summary>
		[Net]
		public Job Job { get; set; }

		/// <summary>
		/// The money of the player.
		/// </summary>
		[Net] 
		public long Money { get; protected set; } = 0;

		/// <summary>
		/// The time since last pay period
		/// </summary>
		[Net, Predicted]
		public TimeSince TimeSinceLastPay { get; protected set; }
		/// <summary>
		/// The time since last job switch
		/// </summary>
		[Net, Predicted]
		public TimeSince TimeSinceLastJobSwitch { get; protected set; } = 60f;

		public Player()
		{
			Inventory = new Inventory( this );
		}

		public Player(IClient cl) : this()
		{
			Clothing.LoadFromClient(cl);
		}

		public override void Respawn()
		{
			SetInfo();

			SetModel( "models/citizen/citizen.vmdl" );

			Controller = new WalkController
			{
				WalkSpeed = 60f,
				DefaultSpeed = 180.0f
			};

			if ( DevController is NoclipController )
			{
				DevController = null;
			}

			this.ClearWaterLevel();
			EnableAllCollisions = true;
			EnableDrawing = true;
			EnableHideInFirstPerson = true;
			EnableShadowInFirstPerson = true;

			Clothing.DressEntity( this );

			Inventory.Add( new Pistol(), true );
			Inventory.Add( new MP5());

			base.Respawn();
		}

		public override void OnKilled()
		{
			base.OnKilled();

			if ( lastDamage.HasTag( "vehicle" ) )
			{
				Particles.Create( "particles/impact.flesh.bloodpuff-big.vpcf", lastDamage.Position );
				Particles.Create( "particles/impact.flesh-big.vpcf", lastDamage.Position );
				PlaySound( "kersplat" );
			}

			BecomeRagdollOnClient( Velocity, lastDamage.Position, lastDamage.Force, lastDamage.BoneIndex, lastDamage.HasTag( "bullet" ), lastDamage.HasTag( "blast" ) );

			Controller = null;

			EnableAllCollisions = false;
			EnableDrawing = false;

			foreach ( var child in Children )
			{
				child.EnableDrawing = false;
			}

			Inventory.DropActive();
			Inventory.DeleteContents();
		}

		public override void TakeDamage( DamageInfo info )
		{
			SetInfo();

			//if ( info.Attacker.IsValid() )
			//{
			//	if ( info.Attacker.Tags.Has( $"{PhysGun.GrabbedTag}{Client.SteamId}" ) ) // For when I import the PhysGun
			//		return;
			//}

			if ( info.Hitbox.HasTag( "head" ) ) 
			{
				info.Damage *= 10.0f; // Multiplies the damage if it was a headshot
			}

			lastDamage = info;
			
			base.TakeDamage( info );
		}

		public override void Simulate( IClient cl )
		{
			base.Simulate( cl );

			if ( ActiveChildInput.IsValid() && ActiveChildInput.Owner == this )
			{
				ActiveChild = ActiveChildInput;
			}

			if ( LifeState != LifeState.Alive )
				return;

			var controller = GetActiveController();
			if ( controller != null )
			{
				EnableSolidCollisions = !controller.HasTag( "noclip" );

				SimulateAnimation( controller );
			}

			TickPlayerUse();
			SimulateActiveChild( cl, ActiveChild );

			if ( Input.Pressed( "view" ) )
			{
				ThirdPersonCamera = !ThirdPersonCamera;
			}

			if ( Input.Pressed( "drop" ) )
			{
				var dropped = Inventory.DropActive();
				if ( dropped.IsValid() )
				{
					timeSinceDropped = 0;

					if ( dropped.PhysicsGroup.IsValid() )
					{
						dropped.PhysicsGroup.Velocity = 0;
						dropped.PhysicsGroup.AngularVelocity = 0;
						dropped.PhysicsGroup.ApplyImpulse( Velocity + EyeRotation.Forward * 500.0f + Vector3.Up * 100.0f, true );
						dropped.PhysicsGroup.ApplyAngularImpulse( Vector3.Random * 100.0f, true );
					}
				}
			}


			if ( Input.Released( "noclip" ) )
			{
				if ( DevController is NoclipController )
				{
					DevController = null;
				}
				else
				{
					DevController = new NoclipController();
				}
			}

			if ( InputDirection.y != 0 || InputDirection.x != 0f )
			{
				timeSinceJumpReleased = 1;
			}

			PayPlayer();
		}

		Entity lastWeapon;

		void SimulateAnimation( PawnController controller )
		{
			if ( controller == null )
				return;

			// where should we be rotated to
			var turnSpeed = 0.02f;

			Rotation rotation;

			// If we're a bot, spin us around 180 degrees.
			if ( Client.IsBot )
				rotation = ViewAngles.WithYaw( ViewAngles.yaw + 180f ).ToRotation();
			else
				rotation = ViewAngles.ToRotation();

			var idealRotation = Rotation.LookAt( rotation.Forward.WithZ( 0 ), Vector3.Up );
			Rotation = Rotation.Slerp( Rotation, idealRotation, controller.WishVelocity.Length * Time.Delta * turnSpeed );
			Rotation = Rotation.Clamp( idealRotation, 45.0f, out var shuffle ); // lock facing to within 45 degrees of look direction

			CitizenAnimationHelper animHelper = new CitizenAnimationHelper( this );

			animHelper.WithWishVelocity( controller.WishVelocity );
			animHelper.WithVelocity( controller.Velocity );
			animHelper.WithLookAt( EyePosition + EyeRotation.Forward * 100.0f, 1.0f, 1.0f, 0.5f );
			animHelper.AimAngle = rotation;
			animHelper.FootShuffle = shuffle;
			animHelper.DuckLevel = MathX.Lerp( animHelper.DuckLevel, controller.HasTag( "ducked" ) ? 1 : 0, Time.Delta * 10.0f );
			animHelper.VoiceLevel = Client.Voice.LastHeard < 0.5f ? Client.Voice.CurrentLevel : 0.0f;
			animHelper.IsGrounded = GroundEntity != null;
			animHelper.IsSitting = controller.HasTag( "sitting" );
			animHelper.IsNoclipping = controller.HasTag( "noclip" );
			animHelper.IsClimbing = controller.HasTag( "climbing" );
			animHelper.IsSwimming = this.GetWaterLevel() >= 0.5f;
			animHelper.IsWeaponLowered = false;
			animHelper.MoveStyle = Input.Down( "run" ) ? CitizenAnimationHelper.MoveStyles.Run : CitizenAnimationHelper.MoveStyles.Walk;

			if ( controller.HasEvent( "jump" ) ) animHelper.TriggerJump();
			if ( ActiveChild != lastWeapon ) animHelper.TriggerDeploy();

			if ( ActiveChild is BaseCarriable carry )
			{
				carry.SimulateAnimator( animHelper );

				// We want to use player anim params in viewmodel
				carry.ViewModelEntity?.CopyAnimParameters( this );
			}
			else
			{
				animHelper.HoldType = CitizenAnimationHelper.HoldTypes.None;
				animHelper.AimBodyWeight = 0.5f;
			}

			lastWeapon = ActiveChild;
		}

		public override void StartTouch( Entity other )
		{
			if ( timeSinceDropped < 1 ) return;

			base.StartTouch( other );
		}

		public override float FootstepVolume()
		{
			return Velocity.WithZ( 0 ).Length.LerpInverse( 0.0f, 200.0f ) * 5.0f;
		}

		/// <summary>
		/// The money in string format.
		/// </summary>
		public string FormattedMoney
		{
			get => Money.ToString("C0");
		}

		/// <summary>
		/// Add money to the player.
		/// </summary>
		/// <param name="money">The money to add to the player.</param>
		internal void AddMoney( long money )
		{
			Money += money;
			SetInfo();
		}

		/// <summary>
		/// Subtracts money from the player.
		/// </summary>
		/// <param name="money">The money to subtract from the player.</param>
		internal bool SubtractMoney( long money )
		{
			if( (Money - money) < 0 )
			{
				Log.Warning( "Don't have enough money." );
				return false;
			}

			Money -= money;
			SetInfo();
			return true;
		}

		/// <summary>
		/// Pays the player their job wage.
		/// </summary>
		private void PayPlayer()
		{
			if ( TimeSinceLastPay >= PayPeriod )
			{
				TimeSinceLastPay = 0;
				AddMoney( Job.Wage );
				Log.Info( "You've been paid" );
			}
		}

		/// <summary>
		/// Gets the <see cref="Player"/> via the SteamId.
		/// </summary>
		/// <param name="steamId">The SteamId of the player you are trying to get. </param>
		/// <returns><see cref="Player"/></returns>
		public static Player GetPlayer( long steamId )
		{
			IDictionary<string ,Player> players = (GameManager.Current as SimpleRp).Players;

			Player player = players[steamId.ToString()];
			return player;
		}

		/// <summary>
		/// Gets the <see cref="Player"/> via the SteamId.
		/// </summary>
		/// <param name="playerName">The Player name of the player you are trying to get. </param>
		/// <returns><see cref="Player"/></returns>
		public static Player GetPlayer( string playerName )
		{
			IDictionary<string, Player> players = (GameManager.Current as SimpleRp).Players;

			Player player = players.FirstOrDefault(x => x.Value.Client.Name.ToLower() == playerName.ToLower()).Value;
			return player;
		}

		/// <summary>
		/// Sets the current info onto the Client.
		/// </summary>
		private void SetInfo()
		{
			if ( Client != null )
			{
				PlayerInfo.UpdateInfo(To.Single(this)); // Ignore the error, it works.
				PlayerList.UpdateInfo(); // Ignore the error, it works.
				JobMenu.UpdateInfo(); // Ignore the error, it works.
				WeaponInfo.UpdateInfo(To.Single(this));
			}
		}

		public override void OnChildAdded( Entity child )
		{
			Inventory?.OnChildAdded(child);
		}

		public override void OnChildRemoved( Entity child )
		{ 
			Inventory?.OnChildRemoved(child);
		}

		/// <summary>
		/// Switches the players job to the given job.
		/// </summary>
		/// <param name="job">The job you want to switch to.</param>
		/// <param name="skipCooldown">Whether or not to skip the switch job cooldown.</param>
		public void SwitchJob( Job job, bool skipCooldown = false )
		{
			if ( job == Job ) 
			{
				Log.Warning( "You are already that job." );
				return;
			}

			if (TimeSinceLastJobSwitch < 60f )
			{
				Log.Info( $"You must wait {(60f - TimeSinceLastJobSwitch):0}s before switching jobs." );
				return;
			}

			if ( job.MaxPlayers != 0 )
			{
				if ( !job.IsFull )
				{
					if ( Job != null )
					{
						Job.CurrentNumberOfPlayers -= 1;
					}
					Job = job;
					Job.CurrentNumberOfPlayers++;
					TimeSinceLastJobSwitch *= skipCooldown ? 1 : 0;
					Log.Info( $"{Client.Name} switched jobs to {Job.Name}." );
					SetInfo();
				}
				else
				{
					Log.Warning( "Job is full." );
				}
			}
			else
			{
				if ( Job != null )
				{
					Job.CurrentNumberOfPlayers -= 1;
				}
				Job = job;
				Job.CurrentNumberOfPlayers++;
				TimeSinceLastJobSwitch *= skipCooldown ? 1 : 0;
				SetInfo();
			}
		}

		/// <summary>
		/// Simulates every frame on the client.
		/// </summary>
		/// <param name="cl">The client to simulate on.</param>
		public override void FrameSimulate( IClient cl )
		{
			Camera.Rotation = ViewAngles.ToRotation();
			Camera.FieldOfView = Screen.CreateVerticalFieldOfView( Game.Preferences.FieldOfView );

			if ( ThirdPersonCamera )
			{
				Camera.FirstPersonViewer = null;

				Vector3 targetPos;
				var center = Position + Vector3.Up * 64;

				var pos = center;
				var rot = Camera.Rotation * Rotation.FromAxis( Vector3.Up, -16 );

				float distance = 130.0f * Scale;
				targetPos = pos + rot.Right * ((CollisionBounds.Mins.x + 32) * Scale);
				targetPos += rot.Forward * -distance;

				var tr = Trace.Ray( pos, targetPos )
					.WithAnyTags( "solid" )
					.Ignore( this )
					.Radius( 8 )
					.Run();

				Camera.Position = tr.EndPosition;
			}
			else if ( LifeState != LifeState.Alive && Corpse.IsValid() )
			{
				Corpse.EnableDrawing = true;

				var pos = Corpse.GetBoneTransform( 0 ).Position + Vector3.Up * 10;
				var targetPos = pos + Camera.Rotation.Backward * 100;

				var tr = Trace.Ray( pos, targetPos )
					.WithAnyTags( "solid" )
					.Ignore( this )
					.Radius( 8 )
					.Run();

				Camera.Position = tr.EndPosition;
				Camera.FirstPersonViewer = null;
			}
			else
			{
				Camera.Position = EyePosition;
				Camera.FirstPersonViewer = this;
				Camera.Main.SetViewModelCamera( 90f );
			}
		}

	}
}
