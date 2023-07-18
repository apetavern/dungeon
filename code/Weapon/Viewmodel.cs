namespace Dungeon;

public partial class Viewmodel : AnimatedEntity
{
	[GameEvent.Client.PostCamera]
	void OnPostCamera()
	{
		if ( !Game.IsClient )
			return;
		
		Position = Camera.Position;
		Rotation = Camera.Rotation;
	}
}
