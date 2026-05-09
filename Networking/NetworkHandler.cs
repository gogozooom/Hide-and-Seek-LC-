using HideAndSeek.Patches;
using System;
using Unity.Netcode;
using LethalNetworkAPI;
using UnityEngine;
using Debug = Debugger.Debug;

namespace HideAndSeek 
{
    public class NetworkHandler : NetworkBehaviour
    {
        public event Action<String, MessageProperties> NetworkEvent;
        public static NetworkHandler Instance { get; private set; }

        public LethalClientMessage<string> NetworkMessage = new LethalClientMessage<string>("HASMessage");

        public override void OnNetworkSpawn() // Singleton
        {
            NetworkEvent = null;

            NetworkMessage = new("HASMessage");

            if (Instance)
                if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)
                    Instance?.gameObject.GetComponent<NetworkObject>()?.Despawn();

            Instance = this;

            base.OnNetworkSpawn();

            // SyncingPatch

            Debug.LogMessage("NetworkHandler OnNetworkSpawn(): Connecting Events....");
            NetworkEvent += SyncingPatch.LevelLoading;
            NetworkEvent += SyncingPatch.LevelLoaded;
            NetworkEvent += SyncingPatch.PlayerChosen;
            NetworkEvent += SyncingPatch.SeekersChosen;
            NetworkEvent += SyncingPatch.LockDoor;
            NetworkEvent += SyncingPatch.OpenDoor;
            NetworkEvent += SyncingPatch.PlayerTeleported;
            NetworkEvent += SyncingPatch.DisplayTip;
            NetworkEvent += SyncingPatch.LeverFlipped;
            NetworkEvent += SyncingPatch.SellCurrentItem;
            NetworkEvent += SyncingPatch.MoneyChanged;
            NetworkEvent += SyncingPatch.DestroyItem;
            NetworkEvent += SyncingPatch.BuyAbility;
            NetworkEvent += SyncingPatch.ActivateAbility;
            NetworkEvent += SyncingPatch.SetDayTime;
            NetworkEvent += SyncingPatch.GrabItem;
            NetworkEvent += SyncingPatch.RequestAbilityConfig;
            NetworkEvent += SyncingPatch.ReceiveAbilityConfig;
            NetworkEvent += SyncingPatch.RevivePlayerLocal;

            NetworkMessage.OnReceivedFromClient += EventRecivedRpc;
        }

        public override void OnDestroy() // This should fix double calling, maybe
        {
            Debug.LogMessage("NetworkHandler OnDestroy(): Disconnecting Events....");
            NetworkEvent -= SyncingPatch.LevelLoading;
            NetworkEvent -= SyncingPatch.LevelLoaded;
            NetworkEvent -= SyncingPatch.PlayerChosen;
            NetworkEvent -= SyncingPatch.SeekersChosen;
            NetworkEvent -= SyncingPatch.LockDoor;
            NetworkEvent -= SyncingPatch.OpenDoor;
            NetworkEvent -= SyncingPatch.PlayerTeleported;
            NetworkEvent -= SyncingPatch.DisplayTip;
            NetworkEvent -= SyncingPatch.LeverFlipped;
            NetworkEvent -= SyncingPatch.SellCurrentItem;
            NetworkEvent -= SyncingPatch.MoneyChanged;
            NetworkEvent -= SyncingPatch.DestroyItem;
            NetworkEvent -= SyncingPatch.BuyAbility;
            NetworkEvent -= SyncingPatch.ActivateAbility;
            NetworkEvent -= SyncingPatch.SetDayTime;
            NetworkEvent -= SyncingPatch.GrabItem;
            NetworkEvent -= SyncingPatch.RequestAbilityConfig;
            NetworkEvent -= SyncingPatch.ReceiveAbilityConfig;
            NetworkEvent -= SyncingPatch.RevivePlayerLocal;
            base.OnDestroy();
        }

        public void EventSendRpc(string eventName, MessageProperties message = null)
        {
            if (message != null)
            {
                NetworkEvent?.Invoke(eventName, message); // ?.Invoke = If the event has subscribers (does not equal null), invoke the event
            }
            else
            {
                NetworkEvent?.Invoke(eventName, new MessageProperties(__null:true));
            }
            Debug.LogMessage("[Server] Event! + " + eventName + " Info: " + message);

            string data = eventName + "|" + JsonUtility.ToJson(message);

            NetworkMessage.SendAllClients(data, false);
        }
        public void EventRecivedRpc(string data, ulong playerID)
        {
            string eventName = data.Split("|")[0];
            string messageString = data.Split("|")[1];

            MessageProperties message = (MessageProperties)JsonUtility.FromJson(messageString, typeof(MessageProperties));

            if (message != null)
            {
                NetworkEvent?.Invoke(eventName, message); // If the event has subscribers (does not equal null), invoke the event
            }
            else
            {
                NetworkEvent?.Invoke(eventName, new MessageProperties(__null:true));
            }
            //Debug.LogMessage("[Client] Event! + " + eventName + " Info: " + message + " Raw Data: " + data);
        }
    }
}