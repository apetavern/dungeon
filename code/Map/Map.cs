using System.ComponentModel;
using System.Diagnostics;

namespace Dungeon;

public partial class Map : Entity
{
	[Net, HideInEditor] public IList<Cell> Cells { get; private set; }
	[Net] public int Width { get; private set; }
	[Net] public int Depth { get; private set; }
	[Net] public ModelEntity MapModelEntity { get; private set; }
	[Net] public bool NeedsUpdate { get; set; }

	public override void Spawn()
	{
		base.Spawn();
		Transmit = TransmitType.Always;
		MapModelEntity = new ModelEntity();
	}

	public override void ClientSpawn()
	{
		base.ClientSpawn();
		var (mesh, verts, indices) = SetupMesh();
		var model = Model.Builder.AddMesh( mesh ).Create();
		MapModelEntity.Model = model;
	}

	public void Build( int width, int depth )
	{
		Width = width;
		Depth = depth;

		for ( int x = 0; x < width; ++x )
		{
			for ( int y = 0; y < depth; ++y )
			{
				var isFloor = Game.Random.Next( 3 ) == 1;
				var cellPos = new Vector3( x * Cell.CellSize, y * Cell.CellSize, 0 );
				var cell = new Cell
				{
					Position = cellPos,
					IsFloor = isFloor,
				};

				Cells.Add( cell );
			}
		}

		NeedsUpdate = true;
	}

	public void UpdateCell( Cell c, bool collision = true )
	{
		if ( !collision )
			return;

		c.Collider = new PhysicsBody( Game.PhysicsWorld )
		{
			Position = c.Position / 8,
			BodyType = PhysicsBodyType.Static,
			GravityEnabled = false,
		};

		c.Collider.AddBoxShape( c.Position, Rotation.Identity, (Vector3.One * 0.5f) * Cell.CellSize  );
	}

	private (Mesh, List<SimpleVertex>, List<int>) SetupMesh()
	{
		var mat = Material.Load( "models/citizen_props/beachball.vmat" );
		var mesh = new Mesh( mat );

		var faceIndices = new int[]
		{
				0, 1, 2, 3,
				7, 6, 5, 4,
				0, 4, 5, 1,
				1, 5, 6, 2,
				2, 6, 7, 3,
				3, 7, 4, 0,
		};

		var uAxis = new Vector3[]
		{
				Vector3.Forward,
				Vector3.Left,
				Vector3.Left,
				Vector3.Forward,
				Vector3.Right,
				Vector3.Backward,
		};

		var vAxis = new Vector3[]
		{
				Vector3.Left,
				Vector3.Forward,
				Vector3.Down,
				Vector3.Down,
				Vector3.Down,
				Vector3.Down,
		};

		var positions = new Vector3[]
		{
				new Vector3(-0.5f, -0.5f, 0.5f) * Cell.CellSize,
				new Vector3(-0.5f, 0.5f, 0.5f) * Cell.CellSize,
				new Vector3(0.5f, 0.5f, 0.5f) * Cell.CellSize,
				new Vector3(0.5f, -0.5f, 0.5f) * Cell.CellSize,
				new Vector3(-0.5f, -0.5f, -0.5f) * Cell.CellSize,
				new Vector3(-0.5f, 0.5f, -0.5f) * Cell.CellSize,
				new Vector3(0.5f, 0.5f, -0.5f) * Cell.CellSize,
				new Vector3(0.5f, -0.5f, -0.5f) * Cell.CellSize,
		};

		List<SimpleVertex> verts = new();
		List<int> indices = new();

		foreach ( var c in Cells )
		{
			if ( c.IsFloor )
				continue;

			for ( var i = 0; i < 6; ++i )
			{
				var tangent = uAxis[i];
				var binormal = vAxis[i];
				var normal = Vector3.Cross( tangent, binormal );

				for ( var j = 0; j < 4; ++j )
				{
					var vertexIndex = faceIndices[(i * 4) + j];
					var pos = positions[vertexIndex];

					verts.Add( new SimpleVertex()
					{
						position = pos,
						normal = normal,
						tangent = tangent,
						texcoord = Planar( pos / 32, uAxis[i], vAxis[i] )
					} );
				}

				indices.Add( i * 4 + 0 );
				indices.Add( i * 4 + 2 );
				indices.Add( i * 4 + 1 );
				indices.Add( i * 4 + 2 );
				indices.Add( i * 4 + 0 );
				indices.Add( i * 4 + 3 );
			}
		}

		mesh.CreateVertexBuffer<SimpleVertex>( verts.Count, SimpleVertex.Layout, verts.ToArray() );
		mesh.CreateIndexBuffer( indices.Count, indices.ToArray() );
		return (mesh, verts, indices);
	}

	[GameEvent.Tick]
	void OnTick()
	{
		if ( NeedsUpdate )
		{
			foreach ( var c in Cells )
				UpdateCell( c, !c.IsFloor );
			
			if(Game.IsServer)
				DelayClearUpdate();
		}
	}

	private async void DelayClearUpdate()
	{
		await GameTask.Delay( 10 );
		NeedsUpdate = false;
	}

	[GameEvent.Client.Frame]
	void OnFrame()
	{
		if ( !MapModelEntity.IsValid() )
			return;

		DebugOverlay.Text( "Map", Position );
		foreach ( var c in Cells )
		{
			DebugOverlay.Sphere( c.Position, 20, c.IsFloor ? Color.Green : Color.Red, depthTest: false );
		}
	}

	[Event.Hotload]
	void OnHotload()
	{
		if ( Game.IsServer )
			NeedsUpdate = true;
	}

	protected static Vector2 Planar( Vector3 pos, Vector3 uAxis, Vector3 vAxis )
	{
		return new Vector2()
		{
			x = Vector3.Dot( uAxis, pos ),
			y = Vector3.Dot( vAxis, pos )
		};
	}
}
