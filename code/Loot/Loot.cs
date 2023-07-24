namespace Dungeon;

[GameResource("Loot", "loot","Define a loot item for the loot table.", Icon = "attach_money" )]
public class Loot : GameResource
{
	public LootTier Tier { get; set; }
	public Prefab Drop { get; set; }
}
