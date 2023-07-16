using System.Diagnostics;

namespace Dungeon;

[Prefab]
public partial class Enemy : ModelEntity
{
	[Prefab] public float StartingHealth { get; set; }
	[Prefab] public float BoundsSize { get; set; }

	private GridAStar.Cell _currentCell;
	private GridAStar.Cell _nextCell;
	private int _pathIndex;

	public override void Spawn()
	{
		base.Spawn();
		Tags.Add( Tag.Hitbox );
		Tags.Add( Tag.Enemy );
		this.SetupCollision( boundsSize: BoundsSize );
		Health = StartingHealth;
	}

	public override void TakeDamage( DamageInfo info )
	{
		base.TakeDamage( info );
	}

	public override void OnKilled()
	{
		Log.Info( "Bit sad innit" );
		base.OnKilled();
	}

	[GameEvent.Tick.Server]
	void OnTick()
	{
		_currentCell = Map.Instance.NavGrid.GetNearestCell( Position );

		var player = DungeonGame.Instance.GetRandomPlayer();
		var playerCell = Map.Instance.NavGrid.GetNearestCell( player.Position );
		var path = Map.Instance.NavGrid.ComputePath( _currentCell, playerCell, pathCreator: this);
		
		if(path.Length > 0)
		{
			foreach(var c in path)
			DebugOverlay.Sphere( c.Position.WithZ( 2 ), 20, Color.Cyan );
		}
		if ( _pathIndex < path.Length )
			_nextCell = path[_pathIndex];
		else
			return;
		
		if ( Position.Distance( _nextCell.Position ) <= 16 && _pathIndex < path.Length - 1 )
		{
			_pathIndex++;
		}

		DebugOverlay.Sphere( path[_pathIndex].Position.WithZ( 2 ), 20, Color.Cyan );
		Velocity = (_nextCell.Position.WithZ( 0 ) - Position.WithZ( 0 )) * 0.015f;
		DebugOverlay.Line( Position, Position + Velocity, depthTest: false );
		Position += Velocity;

	}
}
