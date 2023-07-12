namespace Dungeon;

[Prefab]
public partial class ShootProjectileBehaviour : WeaponBehaviour
{
	[Prefab, Net]
	public Prefab Projectile { get; set; }

	public override void Simulate( IClient client )
	{
		base.Simulate( client );
		if ( Game.IsClient )
			return;

		if ( Input.Released( "attack2" ) && PrefabLibrary.TrySpawn<Projectile>( Projectile.ResourcePath, out var projectile ) )
		{
			Log.Info( $"Shot a projectile : {projectile}" );
			projectile.Position = Player.EyePosition + Player.EyeRotation.Forward * 20;
			projectile.Fire( Player.AimRay.Forward, projectile.DefaultMoveSpeed);
		}
	}
}
