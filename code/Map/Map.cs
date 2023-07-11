namespace Dungeon;

public partial class Map : BaseNetworkable, INetworkSerializer
{
	public static Map Current;
	public static float CellSize = 128f;

	public int Seed { get; private set; }
	public int Width { get; set; }
	public int Depth { get; set; }
	public List<Cell> Cells { get; private set; }
	public bool NeedsUpdate { get; set; }

	[ServerOnly] public Transform? PlayerSpawn { get; private set; }
	[ServerOnly] private bool _foundSpawn = false;

	private bool _builtOnClient = false;

	public const string WallPath = "models/wall.vmdl";
	public const string FloorPath = "models/floor.vmdl";

	public void Build()
	{
		Game.AssertServer();
		if ( Game.IsClient )
			return;

		Seed = Game.Random.Next();
		SetupCells();

		Current = this;
	}

	public void BuildClient()
	{
		Game.AssertClient();
		Current = this;
	}

	public void DeleteRandomCell()
	{
		Game.AssertServer();
		if ( Game.IsClient )
			return;

		var index = Game.Random.Next( 0, Cells.Count );
		DeleteCell( index );
	}

	private void SetupCells()
	{
		Game.SetRandomSeed( Seed );

		Cells = new();
		for ( int x = 0; x < Width; ++x )
		{
			for ( int y = 0; y < Depth; ++y )
			{
				var isFloor = Game.Random.Next( 3 ) == 1;
				var cellPos = new Vector3( x * CellSize, y * CellSize, 0 );
				var cell = new Cell
				{
					Position = cellPos,
					IsFloor = isFloor,
				};

				Cells.Add( cell );

				if ( !_foundSpawn && Game.Random.Next( Width ) == 2 && isFloor )
				{
					PlayerSpawn = new Transform( cellPos, Rotation.Identity );
					_foundSpawn = true;
				}
			}
		}
	}

	public void BuildCollsionForCell( Cell cell )
	{
		if ( cell.IsFloor )
			return;

		// Cell already has collision setup.
		if ( cell.Collider.IsValid() )
			return;

		cell.Collider = new PhysicsBody( Game.PhysicsWorld )
		{
			Position = cell.Position,
			BodyType = PhysicsBodyType.Static,
			GravityEnabled = false,
		};

		cell.Collider.AddBoxShape( default, Rotation.Identity, (Vector3.One * 0.5f) * CellSize );
	}

	public void DeleteCell( int index )
	{
		var cell = Cells[index];
		Log.Info( $"Deleting cell: {index}" );

		if ( !cell.IsFloor )
		{
			cell.Collider.Enabled = false;
			Log.Info( cell.Collider.Enabled );
		}

		DeleteCellClient( To.Everyone, index );
	}

	[ClientRpc]
	public static void DeleteCellClient( int index )
	{
		var cell = Current.Cells[index];
		DebugOverlay.Sphere( cell.Position, 20, Color.Random, 20, depthTest: false );

		if ( !cell.IsFloor )
			cell.Collider.Enabled = false;

		cell.Model.RenderingEnabled = false;
	}

	public void OnTick()
	{
		if ( Game.IsClient )
			return;

		if ( NeedsUpdate )
		{
			foreach ( var c in Cells )
				BuildCollsionForCell( c );
		}

		foreach ( var c in Cells )
		{
			if ( c.Collider.IsValid() && c.Collider.Enabled )
			{
				DebugOverlay.Sphere( c.Position, 5, Color.Red, depthTest: false );
			}
		}

		WriteNetworkData();
		NeedsUpdate = false;
	}

	public void OnFrame()
	{
		if ( !_builtOnClient )
			BuildClient();

		DebugOverlay.Text( "Map", default );
		foreach ( var c in Cells )
		{
			//DebugOverlay.Sphere( c.Position, 20, c.IsFloor ? Color.Green : Color.Red, depthTest: false );
			if ( c.Collider.IsValid() && c.Collider.Enabled )
				DebugOverlay.Box( c.Collider.GetBounds(), Color.White );
		}
	}

	protected static Vector2 Planar( Vector3 pos, Vector3 uAxis, Vector3 vAxis )
	{
		return new Vector2()
		{
			x = Vector3.Dot( uAxis, pos ),
			y = Vector3.Dot( vAxis, pos )
		};
	}

	public void Read( ref NetRead net )
	{
		NeedsUpdate = net.Read<bool>();
		var cellCount = net.Read<int>();
		Cells = new( cellCount );
		for ( int i = 0; i < cellCount; i++ )
		{
			var position = net.Read<Vector3>();
			var IsFloor = net.Read<bool>();

			var cell = new Cell
			{
				Position = position,
				IsFloor = IsFloor
			};

			cell.Model = new SceneObject( Game.SceneWorld, cell.IsFloor ? Model.Load( FloorPath ) :
				Model.Load( WallPath ), new Transform( cell.Position, Rotation.Identity ) );
			BuildCollsionForCell( cell );
			Cells.Add( cell );
		}

		Seed = net.Read<int>();
		Width = net.Read<int>();
		Depth = net.Read<int>();
	}

	public void Write( NetWrite net )
	{
		net.Write( NeedsUpdate );
		if ( NeedsUpdate )
		{
			net.Write( Cells.Count );
			foreach ( var c in Cells )
			{
				net.Write( c.Position );
				net.Write( c.IsFloor );
			}
		}

		net.Write( Seed );
		net.Write( Width );
		net.Write( Depth );
	}
}
