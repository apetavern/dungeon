using GridAStar;

namespace Dungeon;

partial class Map
{
	public Grid NavGrid { get; private set; }

	public TimeUntil NextRebuild { get; private set; }

	public bool ShouldRebuildNav { get; set; }

	private void SetupNav()
	{
		BuildNav();
	}

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

	private async void BuildNav()
	{
		NavGrid = await Grid.Create( Vector3.Zero, Bounds, Rotation.Identity, cellSize: TileSize / 2.2f, save: false );
	}
}
