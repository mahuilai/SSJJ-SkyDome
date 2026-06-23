using System;
using NetData;
namespace SkyDome
{
	public static class EventBroadcaster
	{
		public static void InvokePlayerHit(GameServerSetupData data)
		{
			Action<GameServerSetupData> onPlayerHit = EventBroadcaster.OnPlayerHit;
			if (onPlayerHit == null)
			{
				return;
			}
			onPlayerHit(data);
		}
		public static event Action<GameServerSetupData> OnPlayerHit;
	}
}