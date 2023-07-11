namespace Dungeon;

partial class DungeonGame
{
	[Net] public Map Map { get; private set; }

	public void SetupMap()
	{
		if ( Game.IsClient )
			return;

		Map = new();
		Map.Width = 16;
		Map.Depth = 16;

		Map.Build();
		Map.NeedsUpdate = true;
	}

	[GameEvent.Tick]
	void OnTick()
	{
		if ( Map is null )
			return;

		Map.OnTick();
	}

	[GameEvent.Client.Frame]
	void OnFrame()
	{
		if ( Map is null )
			return;

		Map.OnFrame();
	}
}
