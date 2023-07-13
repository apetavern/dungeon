﻿namespace Dungeon;

public partial class Player : AnimatedEntity
{
	[ClientOnly]
	public static Player Local = (Game.LocalPawn as Player);

	[BindComponent]
	public PlayerController Controller { get; }

	[Net] public Weapon? ActiveWeapon { get; private set; }

	private PointLightEntity? RPGLight { get; set; }

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

		//RPGLight = new PointLightEntity();
		//RPGLight.Color = Color.FromRgb( 0xEBDEAB );

		PrefabLibrary.TrySpawn<Weapon>( "prefabs/weapons/firestaff/firestaff.prefab", out var weapon );
		weapon.Owner = this;
		ActiveWeapon = weapon;
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

		if ( Game.IsServer && Input.Down( "attack1" ) )
		{
			var tr = Trace.Ray( AimRay, 200 ).Run();
			if ( tr.Body is null )
				return;

			var cell = Map.Current.GetCellFromBody( tr.Body );
			Map.Current.ChangeCell( cell, Tiles.Floor );
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
		Camera.FieldOfView = Screen.CreateVerticalFieldOfView( 60 );
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
