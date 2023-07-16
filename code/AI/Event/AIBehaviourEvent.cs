namespace Dungeon;

public abstract class AIBehaviourEvent
{
	public AIBehaviour Source;
	public AIBehaviourEvent( AIBehaviour source)
	{
		Source = source;
	}
}

public sealed class DetectedPlayer : AIBehaviourEvent
{
	public DetectedPlayer( AIBehaviour source ) : base( source ) { }
}

public sealed class LostInterestInTarget : AIBehaviourEvent
{
	public LostInterestInTarget( AIBehaviour source ) : base( source ) { }
}

public sealed class StartedFollowingTarget : AIBehaviourEvent
{
	public StartedFollowingTarget( AIBehaviour source ) : base( source ) { }
}
