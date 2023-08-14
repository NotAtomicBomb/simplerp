using Sandbox;
using System.Collections.Generic;

namespace MyGame;

public partial class Weapon : AnimatedEntity
{
	/// <summary>
	/// The View Model's entity, only accessible clientside.
	/// </summary>
	public WeaponViewModel ViewModelEntity { get; protected set; }

	/// <summary>
	/// An accessor to grab our Pawn.
	/// </summary>
	public Pawn Pawn => Owner as Pawn;

	/// <summary>
	/// This'll decide which entity to fire effects from. If we're in first person, the View Model, otherwise, this.
	/// </summary>
	public AnimatedEntity EffectEntity => Camera.FirstPersonViewer == Owner ? ViewModelEntity : this;

	public virtual string ViewModelPath => null;
	public virtual string ModelPath => null;

	/// <summary>
	/// How often you can shoot this gun.
	/// </summary>
	public virtual float PrimaryRate => 5.0f;

	/// <summary>
	/// The max amount of ammout you can have in the gun at any given time.
	/// </summary>
	public virtual int MagSize => 12;

	/// <summary>
	/// The time it takes to reload
	/// </summary>
	public virtual float ReloadTime => 2.5f;

	/// <summary>
	/// The current ammo of the weapon
	/// </summary>
	[Net]
	public int CurrentAmmo { get; set; } = 12;
	/// <summary>
	/// The current reserveammo of the weapon
	/// </summary>
	[Net]
	public int ReserveAmmo { get; set; } = 32;

	/// <summary>
	/// How long since we last shot this gun.
	/// </summary>
	[Net, Predicted] public TimeSince TimeSincePrimaryAttack { get; set; }
	/// <summary>
	/// How long since reloading
	/// </summary>
	[Net, Predicted] public TimeSince TimeSinceReload { get; set; }
	/// <summary>
	/// If the weapon is currently reloading
	/// </summary>
	[Net, Predicted] public bool IsReloading { get; set; }


	public override void Spawn()
	{
		EnableHideInFirstPerson = true;
		EnableShadowInFirstPerson = true;
		EnableDrawing = false;

		if ( ModelPath != null )
		{
			SetModel( ModelPath );
		}
	}

	/// <summary>
	/// Called when <see cref="Pawn.SetActiveWeapon(Weapon)"/> is called for this weapon.
	/// </summary>
	/// <param name="pawn"></param>
	public void OnEquip( Pawn pawn )
	{
		Owner = pawn;
		SetParent( pawn, true );
		EnableDrawing = true;
		CreateViewModel( To.Single( pawn ) );
	}

	public virtual void Reload()
	{
		if ( ReserveAmmo == 0 ) return;
		if(IsReloading) return;

		TimeSinceReload = 0;
		IsReloading = true;

		(Owner as AnimatedEntity)?.SetAnimParameter( "b_reload", true );
		StartReloadEffects();

	}

	public virtual void OnReloadFinish()
	{
		var AmmoNeeded = (MagSize - CurrentAmmo);
		if ( (ReserveAmmo - AmmoNeeded) < 0 && ReserveAmmo > 0 )
		{
			CurrentAmmo = ReserveAmmo;
			ReserveAmmo = 0;
		}
		else
		{
			ReserveAmmo -= AmmoNeeded;
			CurrentAmmo += AmmoNeeded;
		}
		IsReloading = false;
	}

	[ClientRpc]
	public virtual void StartReloadEffects()
	{
		ViewModelEntity?.SetAnimParameter( "reload", true );
	}

	/// <summary>
	/// Called when the weapon is either removed from the player, or holstered.
	/// </summary>
	public void OnHolster()
	{
		EnableDrawing = false;
		DestroyViewModel( To.Single( Owner ) );
	}

	/// <summary>
	/// Called from <see cref="Pawn.Simulate(IClient)"/>.
	/// </summary>
	/// <param name="player"></param>
	public override void Simulate( IClient player )
	{
		Animate();

		if ( CanPrimaryAttack() && !IsReloading )
		{
			using ( LagCompensation() )
			{
				TimeSincePrimaryAttack = 0;
				PrimaryAttack();
			}
		}

		if ( Input.Pressed( "reload" ) )
		{
			Reload();
		}

		if( IsReloading && TimeSinceReload > ReloadTime ) 
		{ 
			OnReloadFinish();
		}

	}

	/// <summary>
	/// Called every <see cref="Simulate(IClient)"/> to see if we can shoot our gun.
	/// </summary>
	/// <returns></returns>
	public virtual bool CanPrimaryAttack()
	{
		if ( !Owner.IsValid() || !Input.Down( "attack1" ) ) return false;

		var rate = PrimaryRate;
		if ( rate <= 0 ) return true;

		return TimeSincePrimaryAttack > (1 / rate);
	}

	/// <summary>
	/// Called when your gun shoots.
	/// </summary>
	public virtual void PrimaryAttack()
	{

	}

	/// <summary>
	/// Useful for setting anim parameters based off the current weapon.
	/// </summary>
	protected virtual void Animate()
	{
	}

	/// <summary>
	/// Does a trace from start to end, does bullet impact effects. Coded as an IEnumerable so you can return multiple
	/// hits, like if you're going through layers or ricocheting or something.
	/// </summary>
	public virtual IEnumerable<TraceResult> TraceBullet( Vector3 start, Vector3 end, float radius = 2.0f )
	{
		bool underWater = Trace.TestPoint( start, "water" );

		var trace = Trace.Ray( start, end )
				.UseHitboxes()
				.WithAnyTags( "solid", "player", "npc" )
				.Ignore( this )
				.Size( radius );

		//
		// If we're not underwater then we can hit water
		//
		if ( !underWater )
			trace = trace.WithAnyTags( "water" );

		var tr = trace.Run();

		if ( tr.Hit )
			yield return tr;
	}

	/// <summary>
	/// Shoot a single bullet
	/// </summary>
	public virtual void ShootBullet( Vector3 pos, Vector3 dir, float spread, float force, float damage, float bulletSize )
	{
		var forward = dir;
		forward += (Vector3.Random + Vector3.Random + Vector3.Random + Vector3.Random) * spread * 0.25f;
		forward = forward.Normal;

		//
		// ShootBullet is coded in a way where we can have bullets pass through shit
		// or bounce off shit, in which case it'll return multiple results
		//
		foreach ( var tr in TraceBullet( pos, pos + forward * 5000, bulletSize ) )
		{
			tr.Surface.DoBulletImpact( tr );

			if ( !Game.IsServer ) continue;
			if ( !tr.Entity.IsValid() ) continue;

			//
			// We turn predictiuon off for this, so any exploding effects don't get culled etc
			//
			using ( Prediction.Off() )
			{
				var damageInfo = DamageInfo.FromBullet( tr.EndPosition, forward * 100 * force, damage )
					.UsingTraceResult( tr )
					.WithAttacker( Owner )
					.WithWeapon( this );

				tr.Entity.TakeDamage( damageInfo );
			}
		}
	}

	/// <summary>
	/// Shoot a single bullet from owners view point
	/// </summary>
	public virtual void ShootBullet( float spread, float force, float damage, float bulletSize )
	{
		Game.SetRandomSeed( Time.Tick );

		var ray = Owner.AimRay;
		ShootBullet( ray.Position, ray.Forward, spread, force, damage, bulletSize );
	}

	[ClientRpc]
	public void CreateViewModel()
	{
		if ( ViewModelPath == null ) return;

		var vm = new WeaponViewModel( this );
		vm.Model = Model.Load( ViewModelPath );
		ViewModelEntity = vm;
	}

	[ClientRpc]
	public void DestroyViewModel()
	{
		if ( ViewModelEntity.IsValid() )
		{
			ViewModelEntity.Delete();
		}
	}
}
