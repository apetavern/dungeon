using System.IO;

namespace Dungeon;
public partial class Map
{
	public static Map? Current;
	public static float CellSize = 128f;

	public int Seed { get; private set; }
	public int Width { get; set; }
	public int Depth { get; set; }
	public List<Cell> Cells;

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
			writer.Write( Cells.Count );
			for ( int i = 0; i < Cells.Count; i++ )
			{
				var c = Cells[i];
				writer.Write( c.Position );
				writer.Write( (short)c.CellType );
			}

			ReceiveMapData( to, stream.GetBuffer() );
		}
	}

	private void SetupCells()
	{
		Game.SetRandomSeed( Seed );

		Cells ??= new();
		for ( int x = 0; x < Width; ++x )
		{
			for ( int y = 0; y < Depth; ++y )
			{
				var isWall = Game.Random.Next( 3 ) == 1;
				var cellPos = new Vector3( x * CellSize, y * CellSize, 0 );
				var cell = new Cell
				{
					Position = cellPos,
					CellType = isWall ? CellType.Wall : CellType.Floor,
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

				Cells.Add( cell );

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
		return Cells.Where( x => x.Collider == body ).FirstOrDefault();
	}

	[ServerOnly]
	public void ChangeCell( Cell cell, CellType newType )
	{
		Game.AssertServer();
		var index = Current.Cells.IndexOf( cell );
		ChangeCell( index, newType );
	}

	[ServerOnly]
	public void ChangeCell( int index, CellType newType )
	{
		Game.AssertServer();
		ChangeCellShared( index, newType );
		ChangeCellClient( To.Everyone, index, newType );
	}

	[ClientRpc]
	public static void ChangeCellClient( int index, CellType newType )
	{
		var cell = Current.Cells[index];

		if ( cell.CellType is CellType.Wall )
		{
			Log.Info( cell.Collider );
			cell.Collider.Enabled = false;
		}

		cell.SceneObject.Model = FloorModel;
	}

	private void ChangeCellShared( int index, CellType newType )
	{
		var cell = Cells[index];
		Log.Info( $"Changing cell: {index} from {cell.CellType} to {newType}" );

		if ( newType is CellType.Floor && cell.CellType is CellType.Wall )
		{
			cell.Collider.Enabled = false;
		}

		if ( Game.IsClient )
		{
			switch ( newType )
			{
				case CellType.None:
					cell.SceneObject.Model = Model.Load( "error.vmdl" );
					break;
				case CellType.Floor:
					cell.SceneObject.Model = FloorModel;
					break;
				case CellType.Wall:
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
		if ( Cells is null )
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
			Current.Cells ??= new();
			var cellCount = reader.ReadInt32();
			for ( int i = 0; i < cellCount; i++ )
			{
				var position = reader.ReadVector3();
				var cellType = (CellType)reader.ReadInt16();
				var isWall = cellType is CellType.Wall;

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

				Current.Cells.Add( cell );
			}
		}
	}
}
