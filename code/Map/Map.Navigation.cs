using GridAStar;

namespace Dungeon;

partial class Map
{
	public Grid NavGrid { get; private set; }

	public TimeUntil NextRebuild { get; private set; }

	public bool ShouldRebuildNav { get; set; }

	[GameEvent.Tick.Server]
	void OnServerTick()
	{
		if ( Time.Tick % 16 != 0 )
			return;

		if ( ShouldRebuildNav )
		{
			BuildNav();
			ShouldRebuildNav = false;
		}
	}

	public async void BuildNav()
	{
		NavGrid = await new GridBuilder()
			.WithBounds( Vector3.Zero, Bounds, Rotation.Identity )
			.WithCellSize( TileSize / 4 )
			.Create();
	}

	[ConCmd.Server( "build_nav" )]
	private static void DebugRebuildNav()
	{
		Map.Instance.BuildNav();
	}
}
