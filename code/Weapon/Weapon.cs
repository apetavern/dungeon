namespace Dungeon;

[Prefab]
[Icon( "auto_fix_high" )]
public partial class Weapon : Entity
{
	[Net] public Player Player { get; private set; }

	public IEnumerable<WeaponBehaviour> Behaviours => Components.GetAll<WeaponBehaviour>( includeDisabled: true );

	public Viewmodel ViewmodelEntity { get; set; }

	[Prefab, ResourceType( "vmdl" ), Net]
	public string ViewmodelPath { get; set; }

	public override void Spawn()
	{
		base.Spawn();
		Transmit = TransmitType.Always;
		Player = Owner as Player;
	}

	public override void ClientSpawn()
	{
		base.ClientSpawn();

		ViewmodelEntity = new Viewmodel();
		ViewmodelEntity.SetModel( ViewmodelPath );
		ViewmodelEntity.Owner = Owner;
		ViewmodelEntity.SetParent( Owner );
		ViewmodelEntity.EnableViewmodelRendering = true;
	}

	public override void Simulate( IClient client )
	{
		foreach ( var behaviour in Behaviours )
		{
			if ( !behaviour.Enabled )
				continue;

			behaviour.Simulate( client );
		}
	}

	public override void FrameSimulate( IClient cl )
	{
	}
}
