namespace Dungeon;

[Prefab]
public partial class Projectile : Entity
{
	[Prefab] public float Radius { get; set; }
	[Prefab] public float DefaultMoveSpeed { get; set; } = 7;
	[Prefab] public float LifeTime { get; set; } = 10;
	[Prefab] public ParticleSystem Particle { get; set; }

	private TimeSince SinceCreated { get; set; }
	private Particles? ParticleEffect { get; set; }

	public override void Spawn()
	{
		base.Spawn();
		Transmit = TransmitType.Always;
		ParticleEffect = Particles.Create( Particle.ResourcePath );
		SinceCreated = 0;
	}

	public override void ClientSpawn()
	{
		base.ClientSpawn();
	}

	public void Fire( Vector3 dir, float force )
	{
		Velocity = dir * force;
	}

	[GameEvent.Tick.Server]
	void Tick()
	{
		var tr = Trace.Sphere( Radius, Position, Position + Velocity ).WithoutTags( "player" ).Run();
		//DebugOverlay.Sphere( Position, Radius, Color.Green, 2, true );
		Position += Velocity;

		ParticleEffect?.SetPosition( 0, Position );
		if ( Game.IsServer && tr.Hit || SinceCreated >= LifeTime )
		{
			ParticleEffect?.Destroy( true );
			Delete();
		}

		//DebugOverlay.Line( Position + Vector3.Random.WithZ(0) * 16, Position + Vector3.Down * 82, 5, false );
		//DebugOverlay.Line( Position, Position + Vector3.Down * 82, 5, false );
	}
}
