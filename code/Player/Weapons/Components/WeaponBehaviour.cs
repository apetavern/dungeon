namespace Dungeon;

public partial class WeaponBehaviour : EntityComponent<Weapon>
{
	public Weapon Weapon => Entity;
	public Player Player => Weapon.Player;

	public virtual void Simulate(IClient client)
	{

	}
}
