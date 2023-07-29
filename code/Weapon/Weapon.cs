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

		Log.Info( "spawned" );
		ViewmodelEntity = new Viewmodel();
		ViewmodelEntity.SetModel( "models/weapons/v_dng_weapon.vmdl" );

		// I just bonemerge the weapon model to the viewmodel skeleton and have a single
		// animgraph for all weapon types.
		var viewModel = new ModelEntity( ViewmodelPath );
		viewModel.SetParent( ViewmodelEntity, true );
		viewModel.EnableViewmodelRendering = true;

		ViewmodelEntity.Owner = Owner;
		ViewmodelEntity.SetParent( Owner );
		ViewmodelEntity.EnableViewmodelRendering = true;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		OnDestroyClient( To.Single( Owner ) );
	}

	[ClientRpc]
	private void OnDestroyClient()
	{
		Log.Info( "destroyed" );
		ViewmodelEntity.Delete();
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
