using Sandbox;

namespace Dungeon;
public static class VertexBufferExtensions
{
	//public static void AddCubeReal(this VertexBuffer self, Vector3 center, Vector3 size, Rotation rotation, Color32 color = default )
	//{
	//	var oldColor = self.Default.Color;
	//	self.Default.Color = color;

	//	var f = rot.Forward * size.x * 0.5f;
	//	var l = rot.Left * size.y * 0.5f;
	//	var u = rot.Up * size.z * 0.5f;

	//	AddQuad( self, new Ray( center + f, f.Normal ), l, u );
	//	AddQuad( self, new Ray( center - f, -f.Normal ), l, -u );

	//	AddQuad( self, new Ray( center + l, l.Normal ), -f, u );
	//	AddQuad( self, new Ray( center - l, -l.Normal ), f, u );

	//	AddQuad( self, new Ray( center + u, u.Normal ), f, l );
	//	AddQuad( self, new Ray( center - u, -u.Normal ), f, -l );

	//	self.Default.Color = oldColor;
	//}
}
