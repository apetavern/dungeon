namespace Dungeon;

[Prefab]
public partial class Interaction : EntityComponent
{
	public Action<Interaction, Player> OnInteract;
	
	public void Interact(Player player)
	{
		OnInteract?.Invoke( this, player );
	}
}
