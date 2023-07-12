﻿namespace Dungeon;

public partial class Player : AnimatedEntity
{
	[BindComponent]
	public PlayerController Controller { get; }

	private PointLightEntity RPGLight { get; set; }

	public override void Spawn()
	{
		base.Spawn();

		EnableDrawing = true;
		EnableHideInFirstPerson = true;
		EnableShadowInFirstPerson = true;

		Components.Create<PlayerController>();
		Components.RemoveAny<PlayerControllerMechanic>();

		Components.Create<WalkMechanic>();
		Components.Create<AirMoveMechanic>();
		Components.Create<JumpMechanic>();

		RPGLight = new PointLightEntity();
		RPGLight.Color = Color.FromRgb( 0xEBDEAB );
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
		RPGLight.Position = EyePosition;

		if ( Game.IsServer && Input.Down( "attack1" ) )
		{
			var tr = Trace.Ray( AimRay, 200 ).Run();
			if ( tr.Body is null )
				return;

			var cell = Map.Current.GetCellFromBody( tr.Body );
			Map.Current.ChangeCell( cell, CellType.Floor );
		}
	}

	public override void FrameSimulate( IClient cl )
	{
		base.FrameSimulate( cl );
		Controller?.FrameSimulate( cl );

		RPGLight.Position = EyePosition;
		Camera.Position = EyePosition;
		Camera.Rotation = EyeRotation;
		Camera.FieldOfView = Screen.CreateVerticalFieldOfView( 60 );
		Camera.FirstPersonViewer = this;
		Camera.ZNear = 0.5f;
	}

	[GameEvent.Tick]
	void OnTick()
	{
		DebugOverlay.Text( "Player", Position );
		DebugOverlay.Sphere( Position, 20, Color.White, 0, false );
		DebugOverlay.Axis( EyePosition, EyeRotation, depthTest: false );
	}
}
