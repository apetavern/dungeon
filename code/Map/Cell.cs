namespace Dungeon;

public partial class Cell : BaseNetworkable
{
	public static float CellSize = 128f;
	[Net] public Vector3 Position { get; set; }
	[Net] public bool IsFloor { get; set; }
	[Net] public BBox Bounds { get; set; }
	public PhysicsBody Collider { get; set; }
}
