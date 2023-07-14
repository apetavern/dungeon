namespace Dungeon;

partial class Player
{
	[MapEvents.FloorCleared]
	void OnFloorCleared()
	{
		FloorsCleared++;
	}
}
