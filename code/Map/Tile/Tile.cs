using System.IO;

namespace Dungeon;

public partial class Tile : IDelete
{
	public Tiles TileType;
	public TileFlag Flags;
	public Vector3 Position;
	public SceneObject SceneObject;
	public PhysicsBody Collider;

	public void Delete()
	{
		SceneObject?.Delete();
		Collider?.ClearShapes();
		Collider?.Remove();
	}

	public void Write( BinaryWriter writer )
	{
		writer.Write( Position );
		writer.Write( (short)TileType );
		writer.Write( (byte)Flags );
	}

	public static Tile? Read( BinaryReader reader )
	{
		try
		{
			var position = reader.ReadVector3();
			var tileType = (Tiles)reader.ReadInt16();
			var flags = (TileFlag)reader.ReadByte();
			var isWall = tileType is Tiles.Wall;

			var tile = new Tile
			{
				Position = position,
				TileType = tileType,
			};

			tile.SceneObject = new SceneObject( Game.SceneWorld, tile.GetModelForTileType(), new Transform( position, Rotation.Identity ) );

			if ( flags.HasFlag( TileFlag.Solid ) )
			{
				tile.Collider = new PhysicsBody( Game.PhysicsWorld )
				{
					Position = tile.Position + Vector3.Up * 20,
					BodyType = PhysicsBodyType.Static,
					GravityEnabled = false,
				};

				tile.Collider.AddBoxShape( default, Rotation.Identity, (Vector3.One * 0.5f) * Map.TileSize );
			}

			return tile;
		}
		catch ( Exception e )
		{
			Log.Error( "Big uh oh trying to read a tile from the net." );
			return null;
		}
	}

	public Model GetModelForTileType() => TileType switch
	{
		Tiles.None => Model.Load( "error.vmdl" ),
		Tiles.Floor => Map.FloorModel,
		Tiles.Wall => Map.WallModel,
		Tiles.UnbreakableWall => Map.UnbreakableWallModel,
		_ => Model.Load( "error.vmdl" )
	};

	public void DebugDraw()
	{
		DebugOverlay.Text( Flags.ToString(), Position );

		if ( !SceneObject.IsValid() )
			return;

		DebugOverlay.Text( SceneObject.Model.ToString(), Position + Vector3.Down * 2 );
	}
}
