namespace Dungeon;

public partial class Viewmodel : AnimatedEntity
{
	public static string Attack = "fire";

	[GameEvent.Client.PostCamera]
	void OnPostCamera()
	{
		if ( !Game.IsClient )
			return;
		
		Position = Camera.Position;
		Rotation = Camera.Rotation;
	}
}
