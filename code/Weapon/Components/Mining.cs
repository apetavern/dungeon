namespace Dungeon;

[Prefab]
public partial class Mining : WeaponBehaviour
{
	public override void Simulate( IClient client )
	{
		base.Simulate( client );

		if ( Game.IsServer && Input.Pressed( "attack1" ) )
		{
			var tr = Trace.Ray( Player.AimRay, 200 ).Ignore( Player ).WithTag( Tag.Tile ).Run();
			if ( tr.Body is null || !tr.Shape.HasTag( Tag.Tile ) )
				return;

			var tile = Map.Instance.GetTileFromBody( tr.Body );
			if ( !tile.Flags.HasFlag( TileFlag.Unbreakable ) )
			{
				Map.Instance.ChangeTile( tile, Tiles.Floor );
				Particles.Create( "particles/wall_break_rocks.vpcf", tile.Position.WithZ( Player.EyePosition.z ) );

				//// TODO: (Navigation) Maybe just AddCell where we break the wall?
				Map.Instance.ShouldRebuildNav = true;
			}
		}
	}
}
