namespace Dungeon;

public static partial class DungeonConfig
{
	[ConVar.Replicated( "dng_net_compress" )]
	public static bool UseNetworkCompression { get; set; } = true;

	[ConVar.Replicated( "dng_seed" )]
	public static int Seed { get; set; } = 6221998;

	[ConVar.Client( "dng_map_view_distance" )]
	public static int MapViewDistance { get; set; } = 900;

	[ConVar.Client( "dng_map_lights_view_distance" )]
	public static int MapLightsViewDistance { get; set; } = 600;

	[ConVar.Client( "dng_culling" )]
	public static bool EnabledCulling { get; set; } = true;

	[ConVar.Server( "dng_tile_debug" )]
	public static bool TileDebug { get; set; }

	[ConVar.Client( "dng_draw_hud" )]
	public static bool DrawHud { get; set; } = true;
}
