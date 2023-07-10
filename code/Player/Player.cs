using Sandbox;

namespace Dungeon;

public partial class Player : AnimatedEntity
{
	[BindComponent]
	public PlayerController Controller { get; }

	private PointLightEntity _light;

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
		_light = new PointLightEntity();
	}

	public override void Simulate( IClient cl )
	{
		base.Simulate( cl );
		Controller?.Simulate( cl );
		if(Game.IsServer)
			_light.Position = EyePosition;
	}

	public override void FrameSimulate( IClient cl )
	{
		base.FrameSimulate( cl );
		Controller?.FrameSimulate( cl );

		Camera.Position = EyePosition;
		Camera.Rotation = EyeRotation;
		Camera.FieldOfView = Screen.CreateVerticalFieldOfView( Game.Preferences.FieldOfView );
		Camera.FirstPersonViewer = this;
		Camera.ZNear = 0.5f;
	}
}
