namespace Dungeon;

[Prefab]
public partial class AIController : EntityComponent, ISingletonComponent
{
	public IEnumerable<AIBehaviour> Behaviours => Entity.Components.GetAll<AIBehaviour>( true );
	public IEnumerable<AIData> Data => Entity.Components.GetAll<AIData>( true );

	public Action<AIBehaviourEvent> OnEvent;

	public T GetBehaviour<T>() where T : AIBehaviour
	{
		if ( Entity is null || Entity.Components is null || Entity.Components.Count <= 0 )
			return null;

		foreach ( var mechanic in Behaviours )
		{
			if ( mechanic is T val ) return val;
		}

		return null;
	}

	public bool TryGetBehaviour<T>( out T behaviour ) where T : AIBehaviour
	{
		behaviour = null;
		if ( Entity is null || Entity.Components is null || Entity.Components.Count <= 0 )
			return false;

		foreach ( var m in Behaviours )
		{
			if ( m is T val )
			{
				behaviour = (T)m;
				return true;
			}
		}

		return false;
	}

	public bool TryGetData<T>( out T data ) where T : AIData
	{
		data = null;
		if ( Entity is null || Entity.Components is null || Entity.Components.Count <= 0 )
			return false;

		foreach ( var d in Data )
			if ( d is T found )
			{
				data = (T)d;
				return true;
			}

		return false;
	}

	public void RunEvent( AIBehaviourEvent ev )
	{
		OnEvent?.Invoke( ev );
		foreach ( var x in Behaviours )
		{
			x.OnControllerEvent( ev );
		}
	}

	[GameEvent.Tick.Server]
	void OnTickServer()
	{
		foreach ( var comp in Behaviours.OrderByDescending( x => x.Priority ) )
		{
			if ( !comp.Enabled )
				continue;

			comp.Tick();
		}

		if ( TryGetData<TargetData>( out var targetData ) && targetData.Target is not null)
			targetData.Distance = Entity.Position.Distance( targetData.Target.Position );
	}
}
