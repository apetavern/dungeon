namespace Dungeon;

public abstract class AIBehaviourEvent
{
	public AIBehaviourEvent( AIBehaviour _ ) { }
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
