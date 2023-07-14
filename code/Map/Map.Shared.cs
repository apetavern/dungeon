namespace Dungeon;

partial class Map
{
	private void ChangeCellShared( int index, Tiles newType )
	{
		var cell = AllTiles[index];
		Log.Info( $"Changing cell: {index} from {cell.TileType} to {newType}" );

		if ( newType is Tiles.Floor && cell.TileType is Tiles.Wall )
		{
			cell.Collider.Enabled = false;
		}

		if ( Game.IsClient )
		{
			switch ( newType )
			{
				case Tiles.None:
					cell.SceneObject.Model = Model.Load( "error.vmdl" );
					break;
				case Tiles.Floor:
					cell.SceneObject.Model = FloorModel;
					break;
				case Tiles.Wall:
					cell.SceneObject.Model = WallModel;
					break;
				default:
					cell.SceneObject.Model = FloorModel;
					break;
			}
		}

		cell.TileType = newType;
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
