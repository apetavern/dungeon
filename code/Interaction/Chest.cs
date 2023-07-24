namespace Dungeon;

[Prefab]
[Category( "Map Entities" )]
public partial class Chest : AnimatedEntity
{
	[BindComponent] Interaction Interact { get; }

	public override void Spawn()
	{
		base.Spawn();
		AnimateOnServer = true;
		Interact.OnInteract += HandleOnInteract;
	}

	public override void ClientSpawn()
	{
		base.ClientSpawn();
		Interact.OnInteract += HandleOnInteract;
	}

	private void HandleOnInteract( Interaction interaction, Player player )
	{
		SetAnimParameter( "open", true );
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		Interact.OnInteract -= HandleOnInteract;
	}
}
