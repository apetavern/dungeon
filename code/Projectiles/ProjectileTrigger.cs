namespace Dungeon;

public partial class ProjectileTrigger : BaseTrigger
{
	public override void Spawn()
	{
		base.Spawn();
		Tags.Add( "hitbox" );
		EnableTouch = true;
	}
}
