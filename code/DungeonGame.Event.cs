namespace Dungeon;

partial class DungeonGame
{
	[MapEvents.FloorCleared]
	void OnFloorCleared()
	{
		Map.Regenerate();
		foreach ( var client in Game.Clients )
			client.Pawn.Position = Map.PlayerSpawn.Value.Position;
	}
}
