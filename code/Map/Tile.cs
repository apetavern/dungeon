namespace Dungeon;

public class Tile : IDelete
{
	public Tiles TileType;
	public Vector3 Position;
	public SceneObject SceneObject;
	public PhysicsBody Collider;

	public void Delete()
	{
		SceneObject?.Delete();
		Collider?.ClearShapes();
		Collider?.Remove();
	}
}
