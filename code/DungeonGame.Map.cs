namespace Dungeon;

partial class DungeonGame
{
	public Map Map { get; private set; }

	public void SetupMap()
	{
		Map = new( 32, 32 );
	}
}
