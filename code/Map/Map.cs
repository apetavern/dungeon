using System.IO;

namespace Dungeon;
public partial class Map
{
	public static Map? Current;
	public static float CellSize = 128f;

	public int Seed { get; private set; }
	public int Width { get; set; }
	public int Depth { get; set; }
	public List<Cell> AllCells;

	[ServerOnly] public Transform? PlayerSpawn { get; private set; }
	[ServerOnly] private bool _foundSpawn = false;

	private bool _needsTransmit;

	static Model WallModel = Model.Load( "models/wall.vmdl" );
	static Model FloorModel = Model.Load( "models/floor.vmdl" );

	public Map( int w, int d )
	{
		Current = this;
		Width = w;
		Depth = d;

		Seed = Game.Random.Next();
		if ( Game.IsServer )
		{
			SetupCells();
		}

		Event.Register( this );
	}

	public void TransmitToClient( To to )
	{
		using ( var stream = new MemoryStream() )
		using ( var writer = new BinaryWriter( stream ) )
		{
			writer.Write( AllCells.Count );
			for ( int i = 0; i < AllCells.Count; i++ )
			{
				var c = AllCells[i];
				writer.Write( c.Position );
				writer.Write( (short)c.CellType );
			}

			ReceiveMapData( to, stream.GetBuffer() );
		}
	}

	private void SetupCells()
	{
		Game.SetRandomSeed( Seed );

		AllCells ??= new();
		for ( int x = 0; x < Width; ++x )
		{
			for ( int y = 0; y < Depth; ++y )
			{
				var isWall = Game.Random.Next( 3 ) == 1;
				var cellPos = new Vector3( x * CellSize, y * CellSize, 0 );
				var cell = new Cell
				{
					Position = cellPos,
					CellType = isWall ? Cells.Wall : Cells.Floor,
				};

				if ( isWall )
				{
					cell.Collider = new PhysicsBody( Game.PhysicsWorld )
					{
						Position = cell.Position,
						BodyType = PhysicsBodyType.Static,
						GravityEnabled = false,
					};

					cell.Collider.AddBoxShape( default, Rotation.Identity, (Vector3.One * 0.5f) * CellSize );
				}

				AllCells.Add( cell );

				if ( !_foundSpawn && Game.Random.Next( Width ) == 2 && !isWall )
				{
					PlayerSpawn = new Transform( cellPos, Rotation.Identity );
					_foundSpawn = true;
				}
			}
		}
	}

	public Cell GetCellFromBody( PhysicsBody body )
	{
		// :(
		return AllCells.Where( x => x.Collider == body ).FirstOrDefault();
	}

	[ServerOnly]
	public void ChangeCell( Cell cell, Cells newType )
	{
		Game.AssertServer();
		var index = Current.AllCells.IndexOf( cell );
		ChangeCell( index, newType );
	}

	[ServerOnly]
	public void ChangeCell( int index, Cells newType )
	{
		Game.AssertServer();
		ChangeCellShared( index, newType );
		ChangeCellClient( To.Everyone, index, newType );
	}

	[ClientRpc]
	public static void ChangeCellClient( int index, Cells newType )
	{
		var cell = Current.AllCells[index];

		if ( cell.CellType is Cells.Wall && newType is Cells.Floor )
		{
			cell.Collider.Enabled = false;
			cell.SceneObject.Model = FloorModel;
			cell.CellType = Cells.Floor;
		}
	}

	private void ChangeCellShared( int index, Cells newType )
	{
		var cell = AllCells[index];
		Log.Info( $"Changing cell: {index} from {cell.CellType} to {newType}" );

		if ( newType is Cells.Floor && cell.CellType is Cells.Wall )
		{
			cell.Collider.Enabled = false;
		}

		if ( Game.IsClient )
		{
			switch ( newType )
			{
				case Cells.None:
					cell.SceneObject.Model = Model.Load( "error.vmdl" );
					break;
				case Cells.Floor:
					cell.SceneObject.Model = FloorModel;
					break;
				case Cells.Wall:
					cell.SceneObject.Model = WallModel;
					break;
				default:
					cell.SceneObject.Model = FloorModel;
					break;
			}
		}

		cell.CellType = newType;
	}

	[GameEvent.Tick]
	public void OnTick()
	{
		if ( Game.IsClient )
			return;

		if ( _needsTransmit )
		{
			TransmitToClient( To.Everyone );
			_needsTransmit = false;
		}
	}

	[GameEvent.Client.Frame]
	public void OnFrame()
	{
		if ( AllCells is null )
			return;
	}

	[ClientRpc]
	public static void ReceiveMapData( byte[] data )
	{
		if ( Current is null )
			Current = new( 16, 16 );

		using ( var stream = new MemoryStream( data ) )
		using ( var reader = new BinaryReader( stream ) )
		{
			Current.AllCells ??= new();
			var cellCount = reader.ReadInt32();
			for ( int i = 0; i < cellCount; i++ )
			{
				var position = reader.ReadVector3();
				var cellType = (Cells)reader.ReadInt16();
				var isWall = cellType is Cells.Wall;

				var cell = new Cell
				{
					Position = position,
					CellType = cellType,
					SceneObject = new SceneObject( Game.SceneWorld, isWall ? WallModel : FloorModel, new Transform( position, Rotation.Identity ) )
				};

				if ( isWall )
				{
					cell.Collider = new PhysicsBody( Game.PhysicsWorld )
					{
						Position = cell.Position,
						BodyType = PhysicsBodyType.Static,
						GravityEnabled = false,
					};

					cell.Collider.AddBoxShape( default, Rotation.Identity, (Vector3.One * 0.5f) * CellSize );
				}

				Current.AllCells.Add( cell );
			}
		}
	}
}
