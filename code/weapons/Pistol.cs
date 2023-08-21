using Sandbox;

namespace MyGame;

public partial class Pistol : Weapon
{
	public override string ViewModelPath => "weapons/rust_pistol/v_rust_pistol.vmdl";
	public override int MagSize => 7;
	public override float ReloadTime => 2.5f;

	public override void Spawn()
	{
		base.Spawn();
		CurrentAmmo = 7;

		Tags.Add( "weapon" );
		SetModel( "weapons/rust_pistol/rust_pistol.vmdl" );
	}


	[ClientRpc]
	protected override void ShootEffects()
	{
		Game.AssertClient();

		Particles.Create( "particles/pistol_muzzleflash.vpcf", EffectEntity, "muzzle" );

		
		ViewModelEntity?.SetAnimParameter( "fire", true );
	}

	public override void AttackPrimary()
	{
		if ( CurrentAmmo > 0 )
		{
			(Owner as AnimatedEntity)?.SetAnimParameter( "b_attack", true );
			ShootEffects();
			PlaySound( "rust_pistol.shoot" );
			ShootBullet( 0.1f, 100, 20, 1 );

			CurrentAmmo--;
		}
		else
		{
			PlaySound( "pistol_empty" );
		}
		WeaponInfo.UpdateInfo( To.Single( Owner as Player ) ); // Ignore error, it works.
	}
}
