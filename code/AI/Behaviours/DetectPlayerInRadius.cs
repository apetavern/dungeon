namespace Dungeon;

[Prefab]
public partial class DetectPlayerInRadius : AIBehaviour
{
	[Prefab] public float Radius { get; set; } = 20;

	public override void Tick()
	{
		base.Tick();

		var tr = Trace.Sphere( Radius, Entity.Position, Entity.Position )
			.WithTag( Tag.Player )
			.Run();

		DebugOverlay.Sphere( Entity.Position, Radius, tr.Hit ? Color.Red : Color.Green );
		if ( tr.Hit && Controller.TryGetData<TargetData>( out var targetter ) )
		{
			targetter.Target = tr.Entity;
			Controller.RunEvent( new StartedFollowingTarget( this ) );
			Controller.RunEvent( new DetectedPlayer( this ) );
		}
	}

	public override void OnControllerEvent( AIBehaviourEvent ev )
	{
		base.OnControllerEvent( ev );
		if ( !Enabled && ev is LostInterestInTarget )
			Enabled = true;
	}
}
