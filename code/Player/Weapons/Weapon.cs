namespace Dungeon;

[Prefab]
public partial class Weapon : Entity
{
	public Player Player => (Owner as Player);

	public IEnumerable<WeaponBehaviour> Behaviours => Components.GetAll<WeaponBehaviour>( includeDisabled: true );

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
