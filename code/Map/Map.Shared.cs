namespace Dungeon;

partial class Map
{
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
