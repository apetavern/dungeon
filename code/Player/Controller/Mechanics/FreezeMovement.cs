namespace Dungeon;

public class FreezeMovement : PlayerControllerMechanic
{
	private float? _duration;
	private RealTimeSince _sinceAdded;
	private WalkMechanic _walk;

	public FreezeMovement() { }

	public FreezeMovement( float duration )
	{
		_duration = duration;
		_sinceAdded = 0;
	}

	protected override void OnStart()
	{
		base.OnStart();
		Controller.AllowMovement = false;
	}

	protected override bool ShouldStart()
	{
		return true;
	}

	protected override void Tick()
	{
		base.Tick();

		if ( _sinceAdded >= _duration )
		{
			Controller.AllowMovement = true;
			Remove();
		}
	}
}
