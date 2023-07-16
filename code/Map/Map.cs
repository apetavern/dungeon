namespace Dungeon;

public partial class Map
{
	public static Map? Instance;

	public static float TileSize = 64f;
	public static float TileHeight => TileSize;
	public static float HalfTile => TileSize / 2;

	public static Model FloorModel = Model.Load( "models/floor.vmdl" );
	public static Model WallModel = Model.Load( "models/wall.vmdl" );
	public static Model UnbreakableWallModel = Model.Load( "models/wall_unbreakable.vmdl" );

	public int Seed { get; private set; }
	public int Width { get; set; }
	public int Depth { get; set; }
	public BBox Bounds { get; private set; }

	public List<Tile> AllTiles;
	public List<LightActor> Lights;
	public List<Entity> MapEntities = new();

	public int Level { get; private set; }

	[ServerOnly] public Transform? PlayerSpawn { get; private set; }
	[ClientOnly] public bool Initialized;

	private ModelEntity FloorPlane { get; set; }

	public Map( int w, int d )
	{
		Instance = this;
		Width = w;
		Depth = d;

		Seed = DungeonConfig.Seed;
		Bounds = new BBox( new Vector3( TileSize / 2, TileSize / 2, 0 ), new Vector3( (Width - 1) * TileSize - HalfTile, (Width - 1) * TileSize - HalfTile, TileSize ) );

		if ( Game.IsServer )
		{
			var ent = new ModelEntity( "models/dev/plane.vmdl" );
			ent.EnableDrawing = false;
			//ent.Position = Vector3.Down * Map.TileSize / 2;
			float size = 10000;
			ent.SetupPhysicsFromAABB( PhysicsMotionType.Static, new Vector3( -size, -size, -0.1f ), new Vector3( size, size, 0.1f ) );
			Generate();

			SetupNav();
		}

		Event.Register( this );
	}

	public Tile GetTileFromBody( PhysicsBody body )
	{
		// :(
		return AllTiles.Where( x => x.Collider == body ).FirstOrDefault();
	}

	[ServerOnly]
	public void ChangeTile( Tile tile, Tiles newType )
	{
		Game.AssertServer();
		var index = Instance.AllTiles.IndexOf( tile );
		ChangeTile( index, newType, TileFlag.None );
	}

	[ServerOnly]
	public void ChangeTile( int index, Tiles nextTileType, TileFlag nextFlags )
	{
		Game.AssertServer();
		ChangeTileShared( index, nextTileType, nextFlags );
		ChangeTileClient( To.Everyone, index, nextTileType );
	}

	[ClientRpc]
	public static void ChangeTileClient( int index, Tiles newType )
	{
		var tile = Instance.AllTiles[index];

		if ( tile.TileType is Tiles.Wall && newType is Tiles.Floor )
		{
			tile.Collider.Enabled = false;
			tile.SceneObject.Model = FloorModel;
			tile.TileType = Tiles.Floor;
		}
	}

	[GameEvent.Tick]
	public void OnTick()
	{
		if ( DungeonConfig.TileDebug )
		{
			foreach ( var tile in AllTiles )
				tile.DebugDraw();
		}

		if ( Game.IsClient )
			return;

		DebugOverlay.Box(Bounds, Color.Yellow );
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

		if ( Player.Local.SinceMoved >= 0.20f )
			return;

		CullPass();
	}

	private void CullPass()
	{
		if ( !DungeonConfig.EnabledCulling )
			return;

		foreach ( var tile in AllTiles )
		{
			if ( tile.SceneObject.IsValid() )
				tile.SceneObject.RenderingEnabled = Player.Local.Position.Distance( tile.Position ) < DungeonConfig.MapViewDistance;
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
