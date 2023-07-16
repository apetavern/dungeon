namespace Dungeon;

[Prefab]
public partial class Enemy : ModelEntity
{
	[Prefab] public float StartingHealth { get; set; }
	[Prefab] public float BoundsSize { get; set; }

	[BindComponent] AIController Controller { get; }

	public override void Spawn()
	{
		base.Spawn();
		this.SetupCollision( boundsSize: BoundsSize );
		Health = StartingHealth;
	}

	public override void TakeDamage( DamageInfo info )
	{
		base.TakeDamage( info );
	}

	public override void OnKilled()
	{
		Log.Info( "Bit sad innit" );
		Rotation = Rotation.LookAt( Velocity.WithZ( 0 ), Vector3.Up );
		base.OnKilled();
	}
}
