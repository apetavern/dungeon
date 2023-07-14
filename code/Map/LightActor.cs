namespace Dungeon;

public class LightActor : IDelete
{
	/// <summary>
	/// If the client is culling this light because its 
	/// out of render distance.
	/// </summary>
	public bool Culled { get; private set; }
	public bool Lit;
	public LightInfo Info;

	public SceneLight Light;

	public LightActor( SceneWorld world, Vector3 position, float radius, Color color )
	{
		Info = new LightInfo
		{
			SceneWorld = world,
			Position = position,
			Radius = radius,
			Color = color
		};
	}

	public void Cull()
	{
		if ( !Light.IsValid() )
			return;

		Culled = true;
		Info.SceneWorld = Light.World;
		Info.Position = Light.Position;
		Info.Color = Light.LightColor;
		Info.Radius = Light.Radius;

		Light.Delete();
	}

	public void UnCull()
	{
		if ( Light.IsValid() )
			return;

		Culled = false;
		Light = new( Info.SceneWorld, Info.Position, Info.Radius, Info.Color );
	}

	public void Delete()
	{
		Light?.Delete();
	}
}

// We use this for simplyfing networking and for caching and recreating the lights when they are culled/unculled.
public struct LightInfo
{
	public SceneWorld SceneWorld;
	public Vector3 Position;
	public Color Color;
	public float Radius;
}
