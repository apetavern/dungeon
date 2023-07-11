namespace Dungeon;

public partial class DungeonGame : GameManager
{
	public DungeonGame()
	{
		SetupMap();

		if ( Game.IsServer )
		{

			var ent = new ModelEntity( "models/dev/plane.vmdl" );
			ent.EnableDrawing = false;
			ent.Position = Vector3.Down * Map.CellSize / 2;
			float size = 10000;
			ent.SetupPhysicsFromAABB( PhysicsMotionType.Static, new Vector3( -size, -size, -0.1f ), new Vector3( size, size, 0.1f ) );
		}
	}

	public override void ClientSpawn()
	{
		base.ClientSpawn();
	}

	public override void ClientJoined( IClient client )
	{
		base.ClientJoined( client );

		var pawn = new Player();
		client.Pawn = pawn;

		var spawnpoints = Entity.All.OfType<SpawnPoint>();
		var randomSpawnPoint = spawnpoints.OrderBy( x => Guid.NewGuid() ).FirstOrDefault();

		if ( randomSpawnPoint != null )
		{
			var tx = randomSpawnPoint.Transform;
			tx.Position = tx.Position + Vector3.Up * 50.0f; // raise it up
			pawn.Transform = tx;
		}
	}

	[ConCmd.Admin( "noclip" )]
	private static void Noclip()
	{
		if ( ConsoleSystem.Caller.Pawn is not Player player )
			return;

		if ( player.Controller.TryGetMechanic<NoclipMechanic>( out var noclip ) )
		{
			noclip.Enabled = !noclip.Enabled;
			return;
		}

		var nc = new NoclipMechanic { Enabled = true };
		player.Components.Add( nc );
	}
}
