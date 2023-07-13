namespace Sandbox;

public static partial class EventTransmitter
{
	public static void RunGlobalEvent( To to, string eventName )
	{
		Event.Run( eventName );

		Log.Info( $"Transmitting global event:: {eventName}" );
		RunGlobalEventRPC( to, eventName );
	}

	[ClientRpc]
	public static void RunGlobalEventRPC( string eventName )
	{
		Log.Info( $"Running global event RPC :: {eventName}" );
		Event.Run( eventName );
	}
}
