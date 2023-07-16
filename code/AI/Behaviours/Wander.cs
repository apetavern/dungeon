namespace Dungeon;

[Prefab]
public partial class Wander : AIBehaviour
{
	[Prefab] public float WanderRange { get; set; } = 32;

	protected override void OnActivate()
	{
		base.OnActivate();
	}

	public override void Tick()
	{
		base.Tick();

		if ( Game.Random.Next( 0, 35 ) == 15 && Controller.TryGetBehaviour<Pather>( out var pather ) && pather.CurrentPath.Count <= 0 )
		{
			var randomNearPosition = Entity.Position + (Vector3.Random * WanderRange).WithZ( 0 );
			var randomNearbyCell = Map.Instance.NavGrid.GetNearestCell( randomNearPosition );
			pather.Target = randomNearbyCell.Position;
		}
	}

	public override void OnControllerEvent( AIBehaviourEvent ev )
	{
		base.OnControllerEvent( ev );
		if ( Enabled && ev is DetectedPlayer )
		{
			Enabled = false;
		}

		if ( !Enabled &&  ev is LostInterestInTarget )
		{
			Enabled = true;
		}
	}
}
