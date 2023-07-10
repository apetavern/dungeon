namespace Dungeon;

public partial class Player : AnimatedEntity
{
	[BindComponent]
	public PlayerController Controller { get; }


	public override void Spawn()
	{
		base.Spawn();

		SetModel( "models/sbox_props/watermelon/watermelon.vmdl" );

		EnableDrawing = true;
		EnableHideInFirstPerson = true;
		EnableShadowInFirstPerson = true;

		Components.Create<PlayerController>();
		Components.RemoveAny<PlayerControllerMechanic>();

		Components.Create<WalkMechanic>();
		Components.Create<AirMoveMechanic>();
		Components.Create<JumpMechanic>();
	}

	public override void Simulate( IClient cl )
	{
		base.Simulate( cl );
		Controller?.Simulate( cl );
	}

	public override void FrameSimulate( IClient cl )
	{
		base.FrameSimulate( cl );

		Controller?.FrameSimulate( cl );

		Camera.Position = EyePosition;
		Camera.Rotation = EyeRotation;
	}
}
