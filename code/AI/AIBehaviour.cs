namespace Dungeon;

public partial class AIBehaviour : EntityComponent
{
	public AIController? Controller => Entity.Components.Get<AIController>() ?? null;
	public virtual int Priority => 0;

	public virtual void Start() { }
	public virtual void Tick() { }
	public virtual void OnControllerEvent( AIBehaviourEvent ev) { }
}
