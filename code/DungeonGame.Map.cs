namespace Dungeon;

partial class DungeonGame
{
	[Net] public Map Map { get; private set; }

	public void SetupMap()
	{
		if(Game.IsServer)
		{
			Map = new();
			Map.Build( 16, 16 );
		}
	}
}
