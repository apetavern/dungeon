namespace Dungeon;

[Prefab]
public partial class Melee : WeaponBehaviour
{
	[Prefab] public float Range { get; set; } = 200;

	public override void Simulate( IClient client )
	{
		base.Simulate( client );
		if ( SinceActivated < Cooldown )
			return;
		
		if ( Input.Pressed( InputActions.PrimaryAttack ) )
		{
			if ( Game.IsServer )
			{
				var tr = Trace.Ray( Player.AimRay, Range ).WithTag( Tag.Hitbox ).Run();
				if ( tr.Hit )
				{
					tr.Entity.TakeDamage( DamageInfo.Generic( Weapon.BaseDaamge ) );
				}
			}

			if ( Game.IsClient )
			{
				Weapon.ViewmodelEntity.SetAnimParameter( "fire", true );
			}

			SinceActivated = 0;
		}
	}
}
