namespace Dungeon;

[Prefab]
public partial class WeaponPickupController : AnimatedEntity
{
	[Prefab] public Prefab Weapon { get; set; }

	private float _spinSpeed = 42;

	[GameEvent.Tick.Server]
	void OnTickServer()
	{
		Rotation = Rotation.FromAxis( Vector3.Up, Time.Now * _spinSpeed );

		var tr = Trace.Box( this.CreateBBox( boundsSize: 8 ), Position, Position ).WithTag( Tag.Player ).Run();

		if ( tr.Hit )
		{
			if ( tr.Entity is Player player && PrefabLibrary.TrySpawn<Weapon>( Weapon.ResourcePath, out var weapon ) )
			{
				weapon.SetParent( player );
				player.ActiveWeapon = weapon;
			}

			Delete();
		}
	}
}
