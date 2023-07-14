namespace Dungeon;

partial class Map
{
	public bool TryGetRandomTile( out Tile? randomTile, TileFlag toFilter = TileFlag.None)
	{
		randomTile = null;
		for ( int i = 0; i < Width * Depth; i++ )
		{
			var (x, y) = GetRandomCoords();
			randomTile = GetTileShared( x, y );
			if ( !randomTile.Flags.HasFlag( toFilter ) )
				break;
		}

		return true;
	}

	public (int, int) GetRandomCoords()
	{
		return (Game.Random.Next( 0, Width ), Game.Random.Next( 0, Depth ));
	}

	public Tile? GetTileShared( int x, int y )
	{
		if ( x < 0 || x > Width || y < 0 || y > Depth )
			return null;

		var index = x * Width + y;
		return GetTileShared( index );
	}

	public Tile? GetTileShared( int index )
	{
		if ( index > AllTiles.Count || index < 0 )
			return null;

		return AllTiles[index];
	}

	private void ChangeTileShared( int index, Tiles nextTileType, TileFlag nextFlags )
	{
		var tile = AllTiles[index];
		Log.Info( $"Changing tile: {index} from {tile.TileType} to {nextTileType}" );

		tile.Collider.Enabled = nextFlags.HasFlag( TileFlag.Solid );

		if ( Game.IsClient )
			tile.SceneObject.Model = tile.GetModelForTileType();

		tile.TileType = nextTileType;
	}

	private void DeleteMapShared()
	{
		if ( Game.IsServer )
		{
			foreach ( var ent in MapEntities )
				ent.Delete();
		}

		foreach ( var t in AllTiles )
			t.Delete();
		foreach ( var l in Lights )
			l.Delete();

		AllTiles.Clear();
		Lights.Clear();
	}
}
