namespace Dungeon;

public static class MapEvents
{
	public static string FloorCleared = "map.floor_cleared";
	public class FloorClearedAttribute : EventAttribute
	{
		public FloorClearedAttribute() : base( FloorCleared )
		{
		}
	}
}
