using HideAndSeek.Patches;
using System;
using Unity.Netcode;
using LethalNetworkAPI;
using UnityEngine;
using Debug = Debugger.Debug;

namespace HideAndSeek; 

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
        NetworkEvent += NetworkEvents.LevelLoading;
        NetworkEvent += NetworkEvents.LevelLoaded;
        NetworkEvent += NetworkEvents.PlayerChosen;
        NetworkEvent += NetworkEvents.SeekersChosen;
        NetworkEvent += NetworkEvents.LockDoor;
        NetworkEvent += NetworkEvents.OpenDoor;
        NetworkEvent += NetworkEvents.PlayerTeleported;
        NetworkEvent += NetworkEvents.DisplayTip;
        NetworkEvent += NetworkEvents.LeverFlipped;
        NetworkEvent += NetworkEvents.SellCurrentItem;
        NetworkEvent += NetworkEvents.MoneyChanged;
        NetworkEvent += NetworkEvents.DestroyItem;
        NetworkEvent += NetworkEvents.BuyAbility;
        NetworkEvent += NetworkEvents.ActivateAbility;
        NetworkEvent += NetworkEvents.SetDayTime;
        NetworkEvent += NetworkEvents.GrabItem;
        NetworkEvent += NetworkEvents.RequestAbilityConfig;
        NetworkEvent += NetworkEvents.ReceiveAbilityConfig;
        NetworkEvent += NetworkEvents.RevivePlayerLocal;
        NetworkEvent += NetworkEvents.RoundEnded;

        NetworkMessage.OnReceivedFromClient += EventRecivedRpc;
    }

    public override void OnDestroy() // This should fix double calling, maybe
    {
        Debug.LogMessage("NetworkHandler OnDestroy(): Disconnecting Events....");
        NetworkEvent -= NetworkEvents.LevelLoading;
        NetworkEvent -= NetworkEvents.LevelLoaded;
        NetworkEvent -= NetworkEvents.PlayerChosen;
        NetworkEvent -= NetworkEvents.SeekersChosen;
        NetworkEvent -= NetworkEvents.LockDoor;
        NetworkEvent -= NetworkEvents.OpenDoor;
        NetworkEvent -= NetworkEvents.PlayerTeleported;
        NetworkEvent -= NetworkEvents.DisplayTip;
        NetworkEvent -= NetworkEvents.LeverFlipped;
        NetworkEvent -= NetworkEvents.SellCurrentItem;
        NetworkEvent -= NetworkEvents.MoneyChanged;
        NetworkEvent -= NetworkEvents.DestroyItem;
        NetworkEvent -= NetworkEvents.BuyAbility;
        NetworkEvent -= NetworkEvents.ActivateAbility;
        NetworkEvent -= NetworkEvents.SetDayTime;
        NetworkEvent -= NetworkEvents.GrabItem;
        NetworkEvent -= NetworkEvents.RequestAbilityConfig;
        NetworkEvent -= NetworkEvents.ReceiveAbilityConfig;
        NetworkEvent -= NetworkEvents.RevivePlayerLocal;
        NetworkEvent -= NetworkEvents.RoundEnded;
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
        Debug.LogMessage("[NetworkHandler] Sending Event! + " + eventName);

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