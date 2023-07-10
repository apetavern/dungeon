namespace Dungeon;

public partial class Cell : ModelEntity
{
	public static float CellSize = 128f;
	[Net] public bool IsFloor { get; set; }
}
