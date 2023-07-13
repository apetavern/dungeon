using System.IO;

namespace Dungeon;

partial class Map
{
	public void TransmitMapData( To to )
	{
		using ( var stream = new MemoryStream() )
		{
			using ( var writer = new BinaryWriter( stream ) )
			{
				writer.Write( AllCells.Count );
				for ( int i = 0; i < AllCells.Count; i++ )
				{
					var c = AllCells[i];
					writer.Write( c.Position );
					writer.Write( (short)c.TileType );
				}

				writer.Write( Lights.Count );
				for ( int l = 0; l < Lights.Count; l++ )
				{
					var lightActor = Lights[l];
					var info = lightActor.Info;

					writer.Write( info.Position );
					writer.Write( info.Radius );
					writer.Write( info.Color );
				}

				ReceiveMapData( to, stream.GetBuffer() );
			}
		}
	}

	[ClientRpc]
	public static void ReceiveMapData( byte[] data )
	{
		if ( Current is null )
			Current = new( 16, 16 );

		using ( var stream = new MemoryStream( data ) )
		{
			using ( var reader = new BinaryReader( stream ) )
			{
				Current.AllCells ??= new();
				var cellCount = reader.ReadInt32();
				for ( int i = 0; i < cellCount; i++ )
				{
					var position = reader.ReadVector3();
					var cellType = (Tiles)reader.ReadInt16();
					var isWall = cellType is Tiles.Wall;
					var cell = new Tile
					{
						Position = position,
						TileType = cellType,
						SceneObject = new SceneObject( Game.SceneWorld, isWall ? WallModel : FloorModel, new Transform( position, Rotation.Identity ) )
					};

					if ( isWall )
					{
						cell.Collider = new PhysicsBody( Game.PhysicsWorld )
						{
							Position = cell.Position + Vector3.Up * CellSize / 2,
							BodyType = PhysicsBodyType.Static,
							GravityEnabled = false,
						};

						cell.Collider.AddBoxShape( default, Rotation.Identity, (Vector3.One * 0.5f) * CellSize );
					}

					Current.AllCells.Add( cell );
				}

				Current.Lights ??= new();
				var lightCount = reader.ReadInt32();
				for ( int l = 0; l < lightCount; l++ )
				{
					var position = reader.ReadVector3();
					var radius = reader.ReadSingle();
					var color = reader.ReadColor();
					var light = new LightActor( Game.SceneWorld, position, radius, color );

					Current.Lights.Add( light );
				}
			}
		}
	}
}
