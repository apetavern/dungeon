namespace Dungeon;

[Prefab]
partial class FaceTowardsTarget : AIBehaviour
{
	public override void Tick()
	{
		base.Tick();

		if ( !Controller.TryGetData<TargetData>( out var targetter ) )
			return;

		if ( targetter.Target is null )
			return;

		Entity.Rotation = Rotation.LookAt( targetter.Target.Position - Entity.Position, Vector3.Up );
	}
}
