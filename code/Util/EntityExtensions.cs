namespace Dungeon;

public static class EntityExtensions
{
	public static void SetupCollision( this ModelEntity self, PhysicsMotionType motionType = PhysicsMotionType.Keyframed, float boundsSize = 16 )
	{
		self.SetupPhysicsFromAABB( motionType, new Vector3( -boundsSize, -boundsSize, 0 ), new Vector3( boundsSize, boundsSize, Map.TileHeight ) );
	}

	public static BBox CreateBBox( this Entity self, PhysicsMotionType motionType = PhysicsMotionType.Keyframed, float boundsSize = 16 )
	{
		return new BBox( new Vector3( -boundsSize, -boundsSize, 0 ), new Vector3( boundsSize, boundsSize, Map.TileHeight ) );
	}
}
