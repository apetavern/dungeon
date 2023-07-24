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
	private const float InteractRange = 150f;

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

		if ( Input.Down( InputActions.Interact ) )
			TryInteract();

		Controller?.Simulate( cl );
		ActiveWeapon?.Simulate( cl );

		if ( Controller.Velocity.WithZ( 0 ).Length > 0 )
			SinceMoved = 0;
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

		Camera.Main.SetViewModelCamera( 75 );
	}

	public void SetActiveWeapon( Weapon weapon )
	{
		ActiveWeapon = weapon;
		weapon.SetParent( this );
		weapon.Owner = this;
	}

	private void TryInteract()
	{
		var tr = Trace.Ray( AimRay, InteractRange ).Ignore( this ).WithTag( Tag.Interactable ).Run();
		DebugOverlay.TraceResult( tr );
		if ( !tr.Hit || !tr.Entity.IsValid() )
			return;

		if ( !tr.Entity.Components.TryGet<Interaction>( out var interactComp ) )
			return;

		interactComp.Interact( this );
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
