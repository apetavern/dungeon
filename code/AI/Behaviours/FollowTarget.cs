namespace Dungeon;

[Prefab]
public partial class FollowTarget : AIBehaviour
{
	[Prefab] float LoseInterestRange { get; set; } = 0;
	[Prefab] bool GivePersonalSpace { get; set; } = true;

	public static float PersonalSpace = Map.TileSize / 2;

	public override void Tick()
	{
		base.Tick();

		if ( !Controller.TryGetData<TargetData>( out var targetter ) )
			return;

		var target = targetter.Target;

		if ( target is null || !Controller.TryGetBehaviour<Pather>( out var pather ) )
			return;

		if ( GivePersonalSpace )
			pather.Target = target.Position.Distance( Entity.Position ) >= Map.TileSize / 2 ? target.Position : null;
		else
			pather.Target = target.Position;

		if ( target.Position.Distance( Entity.Position ) >= LoseInterestRange && target is not null )
		{
			Controller.RunEvent( new LostInterestInTarget( this ) );
			targetter.Target = null;
		}
	}

	public override void OnControllerEvent( AIBehaviourEvent ev )
	{
		base.OnControllerEvent( ev );
		if ( ev is DetectedPlayer detectedPlayer )
			detectedPlayer.Source.Enabled = false;
	}
}
