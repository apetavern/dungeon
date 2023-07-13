namespace Dungeon;

public static partial class DungeonConfig
{
	[ConVar.Client]
	public static int MapViewDistance { get; set; } = 1350;

	[ConVar.Client]
	public static int MapLightsViewDistance { get; set; } = 900;
}
