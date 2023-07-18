namespace Dungeon;

[Prefab]
[Icon( "auto_fix_high" )]
public partial class Weapon : Entity
{
	public Player Player => Owner as Player;

	public IEnumerable<WeaponBehaviour> Behaviours => Components.GetAll<WeaponBehaviour>( includeDisabled: true );

	public Viewmodel ViewmodelEntity { get; set; }

	[Prefab, ResourceType( "vmdl" ), Net]
	public string ViewmodelPath { get; set; }

	[Prefab]
	public float BaseDaamge { get; set; } = 10;

	public override void Spawn()
	{
		base.Spawn();
		Transmit = TransmitType.Always;
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
