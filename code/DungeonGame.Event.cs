using Dungeon.UI;

namespace Dungeon;

partial class DungeonGame
{
	[MapEvents.FloorCleared]
	void OnFloorCleared()
	{
		if ( Game.IsClient )
		{
			Hud.Instance.Fade(5);
			return;
		}

		Map.Regenerate();
		foreach ( var client in Game.Clients )
			client.Pawn.Position = Map.PlayerSpawn.Value.Position.WithZ( client.Pawn.Position.z );
	}
}
