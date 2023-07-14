namespace Dungeon;

partial class Map
{
	public void Regenerate()
	{
		if ( Game.IsClient )
			return;

		Level++;
		DeleteMapShared();
		DeleteMapClient( To.Everyone );
		Generate();
		TransmitMapData( To.Everyone );
		RegenerateClient( To.Everyone );
	}

	private void Generate()
	{
		Game.SetRandomSeed( Seed + Level );

		AllTiles ??= new();
		Lights ??= new();

		for ( int x = 0; x < Width; ++x )
			for ( int y = 0; y < Depth; ++y )
			{
				var isUnbreakable = (x <= Width && y == 0) || (x == 0 && y <= Depth) || (x <= Width && y == Depth - 1) || (x == Width - 1 && y <= Depth);
				var isWall = Game.Random.Next( 3 ) == 1 || isUnbreakable;
				var tilePos = new Vector3( x * TileSize, y * TileSize, TileSize / 2 );

				var tile = new Tile
				{
					Position = tilePos,
					TileType = isWall ? Tiles.Wall : Tiles.Floor,
					Flags = TileFlag.None
				};

				if ( isWall )
				{
					tile.Flags |= TileFlag.Solid;
					tile.Collider = new PhysicsBody( Game.PhysicsWorld )
					{
						Position = tile.Position,
						BodyType = PhysicsBodyType.Static,
						GravityEnabled = false,
					};

					var shape = tile.Collider.AddBoxShape( default, Rotation.Identity, (Vector3.One * 0.5f) * TileSize );
					shape.AddTag( Tag.Tile );
					shape.AddTag( Tag.World );
				}
				else if ( Game.Random.Next( Width ) < 3 )
				{
					Lights.Add( new LightActor( Game.SceneWorld, tilePos, 300, Color.FromRgb( 0xe25822 ) ) );
				}

				if ( isUnbreakable )
				{
					tile.TileType = Tiles.UnbreakableWall;
					tile.Flags |= TileFlag.Unbreakable;
				}

				AllTiles.Add( tile );

			}

		if ( TryGetRandomTile( out var spawnTile, TileFlag.Solid ) )
		{
			PlayerSpawn = new Transform( spawnTile.Position, Rotation.Identity );
			Log.Info( $"Found a spawn: {PlayerSpawn.Value.Position}" );
		}

		if ( TryGetRandomTile( out var hatchSpawnTile, TileFlag.Solid ) )
		{
			var hatch = new Hatch();
			hatch.SetModel( "models/hatch.vmdl" );
			hatch.Position = hatchSpawnTile.Position.WithZ( 6 );
			MapEntities.Add( hatch );
		}
	}
}
