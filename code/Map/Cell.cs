namespace Dungeon;

public class Cell
{
	public Vector3 Position { get; set; }
	public bool IsFloor { get; set; }
	public SceneModel Modl { get; set; }
	public PhysicsBody Collider { get; set; }
}
