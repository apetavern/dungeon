namespace Dungeon;

public partial class Player : AnimatedEntity
{
	[ClientOnly]
	public static Player Local = (Game.LocalPawn as Player);

	[BindComponent]
	public PlayerController Controller { get; }

	[Net] public Weapon? ActiveWeapon { get; private set; }

	[Net] public int FloorsCleared { get; private set; }

	[Net] public TimeSince SinceMoved { get; private set; }

	private PointLightEntity? RPGLight { get; set; }

	public override void Spawn()
	{
		base.Spawn();
		Tags.Add( Tag.Player, Tag.Entity );

		EnableDrawing = true;
		EnableHideInFirstPerson = true;
		EnableShadowInFirstPerson = true;

		Components.Create<PlayerController>();
		Components.RemoveAny<PlayerControllerMechanic>();

		Components.Create<WalkMechanic>();
		Components.Create<AirMoveMechanic>();

		//RPGLight = new PointLightEntity()
		//RPGLight.Color = Color.FromRgb( 0xEBDEAB );

		PrefabLibrary.TrySpawn<Weapon>( "prefabs/weapons/firestaff/firestaff.prefab", out var weapon );
		weapon.Owner = this;
		ActiveWeapon = weapon;
		this.SetupCollision( PhysicsMotionType.Keyframed );
	}

	public override void ClientSpawn()
	{
		base.ClientSpawn();

		RPGLight = new PointLightEntity();
		RPGLight.Color = Color.FromRgb( 0xEBDEAB );
	}

	public override void Simulate( IClient cl )
	{
		base.Simulate( cl );
		Controller?.Simulate( cl );

		if ( Controller.Velocity.WithZ( 0 ).Length > 0 )
			SinceMoved = 0;

		if ( Game.IsServer && Input.Pressed( "attack1" ) )
		{
			var tr = Trace.Ray( AimRay, 200 ).Ignore( this ).WithTag( Tag.Tile ).Run();
			if ( tr.Body is null || !tr.Shape.HasTag( Tag.Tile ) )
				return;

			var tile = Map.Instance.GetTileFromBody( tr.Body );
			if ( !tile.Flags.HasFlag( TileFlag.Unbreakable ) )
			{
				Map.Instance.ChangeTile( tile, Tiles.Floor );
				Particles.Create( "particles/wall_break_rocks.vpcf", tile.Position.WithZ( EyePosition.z ) );

				//// TODO: (Navigation) Maybe just AddCell where we break the wall?
				Map.Instance.ShouldRebuildNav = true;
			}
		}

		ActiveWeapon?.Simulate( cl );
	}

	public override void FrameSimulate( IClient cl )
	{
		base.FrameSimulate( cl );

		Controller?.FrameSimulate( cl );
		ActiveWeapon?.FrameSimulate( cl );

		RPGLight.LightSize = 0.2f;
		RPGLight.Brightness = 0.5f;
		RPGLight.Position = Position;

		Camera.Position = EyePosition;
		Camera.Rotation = EyeRotation;
		Camera.FieldOfView = Screen.CreateVerticalFieldOfView( 75 );
		Camera.FirstPersonViewer = this;
		Camera.ZNear = 0.5f;
	}

	[GameEvent.Tick.Client]
	void OnTickClient()
	{
		if ( IsLocalPawn )
			return;

		DebugOverlay.Text( "Player", Position );
		DebugOverlay.Sphere( Position, 20, Color.White, 0, false );
		DebugOverlay.Axis( EyePosition, EyeRotation, depthTest: false );
	}
}
