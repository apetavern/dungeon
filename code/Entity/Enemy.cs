namespace Dungeon;

[Prefab]
public partial class Enemy : ModelEntity
{
	[Prefab] public float StartingHealth { get; set; }
	[Prefab] public float BoundsSize { get; set; }

	public override void Spawn()
	{
		base.Spawn();
		Tags.Add( Tag.Hitbox );
		Tags.Add( Tag.Enemy );
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
		base.OnKilled();
	}
}
