namespace Dungeon;

[Flags]
public enum TileFlag : byte
{
	None,
	Solid, // Player can't walk on this tile (usually a wall). We will create a collider for it.
	Unbreakable // Player can't break this wall.
}
