namespace Dungeon;

partial class Player
{
	[MapEvents.FloorCleared]
	void OnFloorCleared()
	{
		Components.Add( new FreezeMovement(0.2f) );
		FloorsCleared++;
	}
}
