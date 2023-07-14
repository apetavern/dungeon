namespace Dungeon;

public partial class Map
{
	public static Map? Current;

	public static float CellHeight => CellSize;
	public static float CellSize = 64f;
	static Model WallModel = Model.Load( "models/wall.vmdl" );
	static Model FloorModel = Model.Load( "models/floor.vmdl" );

	public bool Initialized;

	public int Seed { get; private set; }
	public int Width { get; set; }
	public int Depth { get; set; }
	public BBox Bounds { get; private set; }

	public List<Tile> AllTiles;
	public List<LightActor> Lights;
	public List<Entity> MapEntities = new();

	public int Level { get; private set; }

	[ServerOnly] public Transform? PlayerSpawn { get; private set; }

	public Map( int w, int d )
	{
		Current = this;
		Width = w;
		Depth = d;

		Seed = DungeonConfig.Seed;

		if ( Game.IsServer )
		{
			SetupCells();
			//Bounds = new BBox(Vector3.One * -w,)
		}

		Event.Register( this );
	}

	public void Regenerate()
	{
		if ( Game.IsClient )
			return;

		Level++;
		DeleteMapShared();
		DeleteMapClient( To.Everyone );
		SetupCells();
		TransmitMapData( To.Everyone );
	}

	private void SetupCells()
	{
		Game.SetRandomSeed( Seed + Level );

		var foundSpawn = false;
		var hatchSpawn = false;

		AllTiles ??= new();
		Lights ??= new();
		for ( int x = 0; x < Width; ++x )
		{
			for ( int y = 0; y < Depth; ++y )
			{
				var isWall = Game.Random.Next( 3 ) == 1;
				var cellPos = new Vector3( x * CellSize, y * CellSize, 0 );
				var cell = new Tile
				{
					Position = cellPos,
					TileType = isWall ? Tiles.Wall : Tiles.Floor,
				};

				if ( isWall )
				{
					cell.Collider = new PhysicsBody( Game.PhysicsWorld )
					{
						Position = cell.Position + Vector3.Up * CellSize / 2,
						BodyType = PhysicsBodyType.Static,
						GravityEnabled = false,
					};

					var shape = cell.Collider.AddBoxShape( default, Rotation.Identity, (Vector3.One * 0.5f) * CellSize );
					shape.AddTag( Tag.Tile );
					shape.AddTag( Tag.World );
				}
				else if ( Game.Random.Next( Width ) < 3 )
				{
					Lights.Add( new LightActor( Game.SceneWorld, cellPos, 300, Color.FromRgb( 0xe25822 ) ) );
				}

				AllTiles.Add( cell );

				if ( !foundSpawn && Game.Random.Next( Width ) == 2 && !isWall )
				{
					PlayerSpawn = new Transform( cellPos, Rotation.Identity );
					foundSpawn = true;
				}

				if ( !hatchSpawn && Game.Random.Next( Width ) <= 2 && !isWall )
				{
					var hatch = new Hatch();
					hatch.SetModel( "models/hatch.vmdl" );
					hatch.Position = cell.Position;
					hatchSpawn = true;
					MapEntities.Add( hatch );

				}
			}
		}
	}

	public Tile GetCellFromBody( PhysicsBody body )
	{
		// :(
		return AllTiles.Where( x => x.Collider == body ).FirstOrDefault();
	}

	[ServerOnly]
	public void ChangeCell( Tile cell, Tiles newType )
	{
		Game.AssertServer();
		var index = Current.AllTiles.IndexOf( cell );
		ChangeCell( index, newType );
	}

	[ServerOnly]
	public void ChangeCell( int index, Tiles newType )
	{
		Game.AssertServer();
		ChangeCellShared( index, newType );
		ChangeCellClient( To.Everyone, index, newType );
	}

	[ClientRpc]
	public static void ChangeCellClient( int index, Tiles newType )
	{
		var cell = Current.AllTiles[index];

		if ( cell.TileType is Tiles.Wall && newType is Tiles.Floor )
		{
			cell.Collider.Enabled = false;
			cell.SceneObject.Model = FloorModel;
			cell.TileType = Tiles.Floor;
		}
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
		if ( AllTiles is null || Lights is null )
			return;

		// Do this because when we first spawn we wan't to cull the map at least once.
		if ( !Initialized )
		{
			CullPass();
			Initialized = true;
		}

		// Don't update map culling if we aren't even moving.b
		if ( Player.Local.MoveInput.Length == 0 )
			return;

		CullPass();
	}

	private void CullPass()
	{
		foreach ( var cell in AllTiles )
		{
			cell.SceneObject.RenderingEnabled = Player.Local.Position.Distance( cell.Position ) < DungeonConfig.MapViewDistance;
		}

		foreach ( var light in Lights )
		{
			if ( Player.Local.Position.Distance( light.Info.Position ) >= DungeonConfig.MapLightsViewDistance )
				light.Cull();
			else
				light.UnCull();
		}
	}

}
