namespace Dungeon;

public partial class FireStaff : EntityComponent<Player>, IWeapon
{
	Player Player => Entity;

	public void Shoot()
	{
	}
}
