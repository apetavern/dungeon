namespace Dungeon;

public static partial class DungeonConfig
{
	[ConVar.Replicated( "dng_seed" )]
	public static int Seed { get; set; } = 6221998;

	[ConVar.Client( "dng_map_view_distance" )]
	public static int MapViewDistance { get; set; } = 900;

	[ConVar.Client( "dng_map_lights_view_distance" )]
	public static int MapLightsViewDistance { get; set; } = 600;
}
