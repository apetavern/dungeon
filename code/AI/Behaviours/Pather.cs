using GridAStar;
using Sandbox.Diagnostics;
using System.Net;

namespace Dungeon;

[Prefab]
public partial class Pather : AIBehaviour
{
	public Vector3? Target;
	public AStarPath CurrentPath = new();
	public Cell CurrentCell { get; private set; }

	public override void Tick()
	{
		base.Tick();

		CurrentCell = Map.Instance.NavGrid.GetNearestCell( Entity.Position );

		if ( Target is null )
			return;

		var targetCell = Map.Instance.NavGrid.GetNearestCell( Target.Value );

		Assert.True( CurrentCell is not null );
		Assert.True( targetCell is not null );

		if ( Time.Tick % DungeonConfig.NavPathComputeRate != 0 )
			return;

		DebugOverlay.Sphere( CurrentCell.Position, 20, Color.Green, 5 );
		DebugOverlay.Sphere( targetCell.Position, 20, Color.Red, 5 );

		CurrentPath = new AStarPathBuilder( Map.Instance.NavGrid ).WithPathCreator( Entity ).Run( CurrentCell, targetCell );

		if ( CurrentPath.IsEmpty )
			return;

		var c = CurrentPath.Nodes.First();
		DebugOverlay.Sphere( c.StartPosition, 20, Color.Cyan, 5 );
		Entity.Velocity = (c.Current.Position.WithZ( 0 ) - Entity.Position.WithZ( 0 )).Normal;
		Entity.Position += Entity.Velocity * 54 * Time.Delta;
	}
}

