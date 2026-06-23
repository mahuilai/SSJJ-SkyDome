using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Sources.Chat;
using Assets.Sources.Framework;
using Assets.Sources.Framework.System;
using Assets.Sources.Modules.Ui.Chat;
using NetData;
using SkyDome.Cfg;
using SkyDome.Extension;
using UnityEngine;
namespace SkyDome.Feature
{
	public class MessageDispatcher : MonoBehaviour
	{
		public static void SendServerMessage(string message, string chatChannel = "battle_all")
		{
			ChatJobSystem chatJobSystem = MessageDispatcher.GetChatJobSystem();
			if (chatJobSystem == null)
			{
				return;
			}
			ChatInputData chatInputData = new ChatInputData
			{
				SenderInputContent = message,
				SenderType = chatChannel,
				ReceiverName = string.Empty,
				ReceiverCid = string.Empty
			};
			object obj = chatJobSystem;
			string text = "SendChatInfo";
			object[] array = new object[1];
			array[0] = chatInputData;
			obj.InvokeMethod(text, array);
		}
		private void Update()
		{
			if (SettingsStore.MessageDispatcher && Time.time - this._lastSendTime >= 3f)
			{
				MessageDispatcher.SendServerMessage(SettingsStore.SendMsg, "battle_all");
				this._lastSendTime = Time.time;
			}
        }

        // 你他妈毕业于肇庆技师学院还敢自称“生来强大”？强大个鸡巴，你就配被我们天天羞辱
        public static void SendLocalMessage(MessageDispatcher.MessageType type, string senderName, string messageContent)
		{
			ChatJobSystem chatJobSystem = MessageDispatcher.GetChatJobSystem();
			if (chatJobSystem == null)
			{
				return;
			}
			ChatHistroyData chatHistroyData = default(ChatHistroyData);
			chatHistroyData.MsgType = MessageDispatcher.GetMsgTypeString(type);
			chatHistroyData.ReceiverName = string.Empty;
			chatHistroyData.ReceiverCid = string.Empty;
			chatHistroyData.SenderName = senderName;
			chatHistroyData.SenderBody = messageContent;
			chatHistroyData.AlphaData.RemainTime = 6000;
			chatHistroyData.AlphaData.AlphaRemainTime = 100;
			object obj = chatJobSystem;
			string text = "OnRecvChatInfo";
			object[] array = new object[1];
			array[0] = chatHistroyData;
			obj.InvokeMethod(text, array);
		}

        private static ChatJobSystem GetChatJobSystem()
		{
			GameModuleFeature instance = GameModuleFeature.Instance;
			if (instance == null)
			{
				return null;
			}
			PlaybackSystem fieldValue = instance.GetFieldValue<PlaybackSystem>("_playbackSystem");
			if (fieldValue == null)
			{
				return null;
			}
			List<IPlaybackSystem> fieldValue2 = fieldValue.GetFieldValue<List<IPlaybackSystem>>("_systems");
			if (fieldValue2 == null)
			{
				return null;
			}
			return fieldValue2.FirstOrDefault((IPlaybackSystem s) => s.GetType() == typeof(ChatJobSystem)) as ChatJobSystem;
		}
		private static string GetMsgTypeString(MessageDispatcher.MessageType type)
		{
			switch (type)
			{
			case MessageDispatcher.MessageType.Vip:
				return "vip";
			case MessageDispatcher.MessageType.BattleAll:
				return "battle_all";
			case MessageDispatcher.MessageType.BattleObserverAll:
				return "battle_observer_all";
			case MessageDispatcher.MessageType.BattleTeam:
				return "battle_team";
			case MessageDispatcher.MessageType.Team:
				return "team";
			case MessageDispatcher.MessageType.Personal:
				return "personal";
			case MessageDispatcher.MessageType.System:
				return "system";
			case MessageDispatcher.MessageType.Prompt:
				return "prompt";
			case MessageDispatcher.MessageType.TacticsSound:
				return "tacticsSound";
			case MessageDispatcher.MessageType.PlayerLogin:
				return "playerlogin";
			case MessageDispatcher.MessageType.PlayerLogout:
				return "playerlogout";
			case MessageDispatcher.MessageType.BigHorn2:
				return "big_horn2";
			case MessageDispatcher.MessageType.BigHorn3:
				return "big_horn3";
			case MessageDispatcher.MessageType.LiveBarrage:
				return "live_barrage";
			case MessageDispatcher.MessageType.LiveGift:
				return "live_gift";
			case MessageDispatcher.MessageType.Hononary1:
				return "hononary1";
			case MessageDispatcher.MessageType.Hononary2:
				return "hononary2";
			default:
				return "system";
			}
        }

        // 你他妈毕业于肇庆技师学院还敢自称“生来强大”？强大个鸡巴，你就配被我们天天羞辱
        private void Start()
		{
			EventBroadcaster.OnPlayerHit += this.OnHitCallback;
		}
		private void OnHitCallback(GameServerSetupData data)
		{
			MessageDispatcher.SendLocalMessage(-190 + MessageDispatcher.MessageType.TacticsSound + 188, "", "击中目标");
		}
		public MessageDispatcher()
		{
		}

        private void OnDestroy()
		{
			EventBroadcaster.OnPlayerHit -= this.OnHitCallback;
		}
		private float _lastSendTime;
		public enum MessageType
		{
			Vip,
			BattleAll,
			BattleObserverAll,
			BattleTeam,
			Team,
			Personal,
			System,
			Prompt,
			TacticsSound,
			PlayerLogin,
			PlayerLogout,
			BigHorn2,
			BigHorn3,
			LiveBarrage,
			LiveGift,
			Hononary1,
			Hononary2
		}
	}
}