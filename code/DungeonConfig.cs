namespace Dungeon;

public static partial class DungeonConfig
{
	[ConVar.Client("dng_map_view_distance")]
	public static int MapViewDistance { get; set; } = 900;

	[ConVar.Client("dng_map_lights_view_distance")]
	public static int MapLightsViewDistance { get; set; } = 600;
}
