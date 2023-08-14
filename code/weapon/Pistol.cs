using Sandbox;

namespace MyGame;

public partial class Pistol : Weapon
{
	public override string ModelPath => "weapons/rust_pistol/rust_pistol.vmdl";
	public override string ViewModelPath => "weapons/rust_pistol/v_rust_pistol.vmdl";
	public override int MagSize => 7;
	public override float ReloadTime => 2.5f;

	public override void Spawn()
	{
		base.Spawn();
		CurrentAmmo = 7;
	}


	[ClientRpc]
	protected virtual void ShootEffects()
	{
		Game.AssertClient();

		Particles.Create( "particles/pistol_muzzleflash.vpcf", EffectEntity, "muzzle" );

		
		ViewModelEntity?.SetAnimParameter( "fire", true );
	}

	public override void PrimaryAttack()
	{
		if ( CurrentAmmo > 0 )
		{
			Pawn.SetAnimParameter( "b_attack", true );
			ShootEffects();
			Pawn.PlaySound( "rust_pistol.shoot" );
			ShootBullet( 0.1f, 100, 20, 1 );

			CurrentAmmo--;
		}
		else
		{
			Pawn.PlaySound( "pistol_empty" );
		}
		WeaponInfo.UpdateInfo( To.Single( Pawn ) ); // Ignore error, it works.
	}

	protected override void Animate()
	{
		Pawn.SetAnimParameter( "holdtype", (int)CitizenAnimationHelper.HoldTypes.Pistol );
	}
}
