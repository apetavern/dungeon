using System.IO;

namespace Dungeon;

partial class Map
{
	public void TransmitMapData( To to )
	{
		using ( var stream = new MemoryStream() )
		{
			using ( var writer = new BinaryWriter( stream ) )
			{
				writer.Write( AllTiles.Count );
				for ( int i = 0; i < AllTiles.Count; i++ )
				{
					AllTiles[i].Write( writer );
				}

				writer.Write( Lights.Count );
				for ( int l = 0; l < Lights.Count; l++ )
				{
					var lightActor = Lights[l];
					var info = lightActor.Info;

					writer.Write( info.Position );
					writer.Write( info.Radius );
					writer.Write( info.Color );
				}

				var bytes = stream.GetBuffer();
				if ( DungeonConfig.UseNetworkCompression )
					bytes = Compression.Compress( bytes );

				Log.Out( $"Sending {bytes.Length}", LogContext.Networking );
				ReceiveMapData( to, bytes );
			}
		}
	}

	[ClientRpc]
	public static void ReceiveMapData( byte[] bytes )
	{
		Log.Out( $"Received: {bytes.Length} bytes.", LogContext.Networking );

		var data = DungeonConfig.UseNetworkCompression ? Compression.Decompress( bytes ) : bytes;

		if ( Instance is null )
			Instance = new( 16, 16 );

		using ( var stream = new MemoryStream( data ) )
		{
			using ( var reader = new BinaryReader( stream ) )
			{
				Instance.AllTiles ??= new();
				var tileCount = reader.ReadInt32();
				for ( int i = 0; i < tileCount; i++ )
				{
					var tile = Tile.Read( reader );
					if ( tile is not null )
						Instance.AllTiles.Add( tile );
				}

				Instance.Lights ??= new();
				var lightCount = reader.ReadInt32();
				for ( int l = 0; l < lightCount; l++ )
				{
					var position = reader.ReadVector3();
					var radius = reader.ReadSingle();
					var color = reader.ReadColor();
					var light = new LightActor( Game.SceneWorld, position, radius, color );

					Instance.Lights.Add( light );
				}
			}
		}
	}

	[ClientRpc]
	public static void DeleteMapClient()
	{
		Instance?.DeleteMapShared();
	}

	[ClientRpc]
	public static void RegenerateClient()
	{
		Instance.CullPass();
	}
}
