namespace Dungeon;

public partial class DungeonGame : GameManager
{
	public static DungeonGame Instance => (Current as DungeonGame);
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

		var spawnpoint = Map.PlayerSpawn ?? new Transform( Vector3.One.WithZ( 64 ), Rotation.Identity );
		var tx = spawnpoint.WithPosition( spawnpoint.Position + Vector3.Up * 1.5f );
		pawn.Transform = spawnpoint;
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
