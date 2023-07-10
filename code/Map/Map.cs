namespace Dungeon;

public partial class Map : Entity
{
	[Net] public IList<Cell> Cells { get; private set; }
	[Net] public int Width { get; private set; }
	[Net] public int Depth { get; private set; }
	public Transform? PlayerSpawn { get; private set; }

	const string WallPath = "models/wall.vmdl";
	const string FloorPath = "models/floor.vmdl";

	public override void Spawn()
	{
		base.Spawn();
		Transmit = TransmitType.Always;
	}

	public void Build( int width, int depth )
	{
		Width = width;
		Depth = depth;
		var foundSpawn = false;

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

				if ( !isFloor )
				{
					cell.Model = Model.Load( WallPath );
				}
				else
				{
					cell.Model = Model.Load( FloorPath );
					if ( Game.Random.Next( width ) == 2 && !foundSpawn )
					{
						foundSpawn = true;
						PlayerSpawn = new Transform( cell.Position, Rotation.Identity );
					}

				}

				cell.SetupPhysicsFromModel( PhysicsMotionType.Static );
				Cells.Add( cell );
			}
		}
	}
}
