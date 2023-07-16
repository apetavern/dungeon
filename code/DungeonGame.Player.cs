namespace Dungeon;

partial class DungeonGame
{
	public Player? GetRandomPlayer()
	{
		var client = Game.Clients.First();

		if ( client.Pawn is Player ply )
			return ply;

		return null;
	}
}
