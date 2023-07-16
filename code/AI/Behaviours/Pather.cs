using GridAStar;
using System.Net;

namespace Dungeon;

[Prefab]
public partial class Pather : AIBehaviour
{
	public Vector3? Target;
	public List<Cell>? CurrentPath = new();
	public Cell CurrentCell { get; private set; }

	public override void Tick()
	{
		base.Tick();

		CurrentCell = Map.Instance.NavGrid.GetNearestCell( Entity.Position );

		if ( Target is null )
			return;

		var targetCell = Map.Instance.NavGrid.GetNearestCell( Target.Value );

		if ( Time.Tick % DungeonConfig.NavPathComputeRate == 0 )
			CurrentPath = Map.Instance.NavGrid.ComputePath( CurrentCell, targetCell, pathCreator: Entity ).ToList<Cell>();

		if ( CurrentPath.Count <= 0 )
			return;

		var c = CurrentPath.First();
		Entity.Velocity = (c.Position.WithZ( 0 ) - Entity.Position.WithZ( 0 )).Normal;
		Entity.Position += Entity.Velocity * 54 * Time.Delta;
	}
}

