namespace Dungeon;

public partial class WeaponBehaviour : EntityComponent<Weapon>
{
	public Weapon Weapon => Entity;
	public Player Player => Weapon.Player;

	[Prefab]
	public float Cooldown { get; set; } = 0;

	public TimeSince SinceActivated { get; set; }

	public virtual void Simulate(IClient client)
	{

	}
}
