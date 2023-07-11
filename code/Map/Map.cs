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
			_needsTransmit = true;
		}

		Event.Register( this );
	}

	public void TransmitToClient()
	{
		using ( var stream = new MemoryStream() )
		using ( var writer = new BinaryWriter( stream ) )
		{
			writer.Write( Cells.Count );
			for ( int i = 0; i < Cells.Count; i++ )
			{
				var c = Cells[i];
				writer.Write( c.Position );
				writer.Write( c.IsWall );
			}

			RebuildClient( To.Everyone, stream.GetBuffer(), false );
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
					IsWall = isWall,
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

	public void DeleteWall( Cell cell )
	{
		Game.AssertServer();
		var index = Current.Cells.IndexOf( cell );
		DeleteWall( index );
	}

	public void DeleteWall( int index )
	{
		Game.AssertServer();

		var cell = Cells[index];
		Log.Info( $"Deleting cell: {index}" );

		if ( cell.IsWall )
			cell.Collider.Enabled = false;

		DeleteWallClient( To.Everyone, index );
	}

	[ClientRpc]
	public static void DeleteWallClient( int index )
	{
		var cell = Current.Cells[index];

		if ( cell.IsWall )
		{
			Log.Info( cell.Collider );
			cell.Collider.Enabled = false;
		}

		cell.Model.Model = FloorModel;
	}

	[GameEvent.Tick]
	public void OnTick()
	{
		if ( Game.IsClient )
			return;

		if ( _needsTransmit )
		{
			TransmitToClient();
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
	public static void RebuildClient( byte[] data, bool firstTime )
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
				var isWall = reader.ReadBoolean();

				var cell = new Cell
				{
					Position = position,
					IsWall = isWall,
					Model = new SceneObject( Game.SceneWorld, isWall ? WallModel : FloorModel, new Transform( position, Rotation.Identity ) )
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
