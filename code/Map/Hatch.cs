namespace Dungeon;

[Category( "Map Entities" )]
public partial class Hatch : ModelEntity
{
	public BBox Bounds { get; set; }
	public bool Triggered { get; private set; }

	public override void Spawn()
	{
		base.Spawn();
		Tags.Clear();
		Bounds = this.CreateBBox( PhysicsMotionType.Static, 32 );
	}

	[GameEvent.Tick]
	void OnTick()
	{
		if ( Triggered )
			return;

		var tr = Trace.Box( Bounds, Position, Position ).WithTag( Tag.Player ).Run();
		if ( tr.Hit && tr.Entity is Player ply )
		{
			Event.Run( MapEvents.FloorCleared );
			Log.Info( "Floor cleared." );
			Triggered = true;
			
		}
		DebugOverlay.Sphere( Position, 20, Color.Yellow, depthTest: false );
	}
}
