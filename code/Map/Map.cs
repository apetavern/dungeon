namespace Dungeon;

public partial class Map
{
	public static Map? Current;
	public static float CellSize = 128f;

	public int Seed { get; private set; }
	public int Width { get; set; }
	public int Depth { get; set; }

	public List<Cell> AllCells;
	public List<LightActor> Lights;

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

	private void SetupCells()
	{
		Game.SetRandomSeed( Seed );

		AllCells ??= new();
		Lights ??= new();
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
				else if ( Game.Random.Next( Width ) < 3 )
				{
					Lights.Add( new LightActor( Game.SceneWorld, cellPos, 300, Color.FromRgb( 0xe25822 ) ) );
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
	}

	[GameEvent.Client.Frame]
	public void OnFrame()
	{
		if ( AllCells is null || Lights is null )
			return;

		foreach ( var cell in AllCells )
		{
			cell.SceneObject.RenderingEnabled = Player.Local.Position.Distance( cell.Position ) < 1350;
		}

		foreach ( var light in Lights )
		{
			if ( Player.Local.Position.Distance( light.Info.Position ) >= 800 )
				light.Cull();
			else
				light.UnCull();
		}
	}


}
