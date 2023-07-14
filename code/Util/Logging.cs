using Sandbox.Diagnostics;

namespace Dungeon;

public static class Logging
{
	public static void Out( this Logger self, object data, LogContext context = LogContext.Info )
	{
		var str = string.Format( "[DNG]::{0}- {1}", context.ToString(), data.ToString() );
		self.Info( str );
	}
}

public enum LogContext
{
	Info = 0,
	Networking = 1,
}
