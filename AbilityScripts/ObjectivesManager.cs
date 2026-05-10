using GameNetcodeStuff;
using System.Collections.Generic;
using UnityEngine;

namespace HideAndSeek;

public class ObjectivesManager : MonoBehaviour
{
    public static ObjectivesManager instance;

    public bool objectiveReleased = false;

    public Dictionary<PlayerControllerB, bool> playersReachedObjective = [];

    public static void Init()
    {
        instance = new GameObject().AddComponent<ObjectivesManager>();
    }

    private void Start()
    {
        HideAndSeekGM.instance.roundStarted += OnRoundStart;
    }
    private void Update()
    {
        if (Config.objective.Value.Equals("Ship", System.StringComparison.CurrentCultureIgnoreCase))
        {
            // Get To Ship Objective

            PlayerControllerB localPlayer = GameNetworkManager.Instance.localPlayerController;

            if (TimeOfDay.Instance.currentDayTime >= Config.timeObjectiveAvailable.Value)
            {
                if (!objectiveReleased)
                {
                    objectiveReleased = true;


                    if (HideAndSeekGM.instance.seekers.Contains(localPlayer))
                    {
                        // Seeker
                        HUDManager.Instance.DisplayTip("Hide And Seek", "The exits have been unlocked, the hiders are escaping!", true);
                    } else if (HideAndSeekGM.instance.zombies.Contains(localPlayer))
                    {
                        // Zombie
                        HUDManager.Instance.DisplayTip("Hide And Seek", "The exits have been unlocked, the hiders are escaping!", true);
                    }
                    else
                    {
                        // Hider
                        HUDManager.Instance.DisplayTip("Hide And Seek", "The exits have been unlocked, escape back to the ship now!");
                    }
                }
                foreach (var player in HideAndSeekGM.GetAllConnectedPlayers("Update Player Objectives"))
                {
                    if (player.isPlayerDead || HideAndSeekGM.instance.seekers.Contains(player) || HideAndSeekGM.instance.zombies.Contains(player)) continue;

                    if (player.isInHangarShipRoom && PlayerReachedObjective(player) == false)
                    {
                        SetPlayerReachedObjective(player, true);

                        player.usernameBillboardText.color = Config.objectiveNameColor.Value;

                        HideAndSeekGM.instance.UpdateGameState("Player Reached Objective");

                        if (localPlayer == player)
                        {
                            HUDManager.Instance.DisplayTip("Hide And Seek", "You have reached the objective!");
                        }
                        else
                        {
                            HUDManager.Instance.DisplayTip("Hide And Seek", $"'{player.playerUsername}' has reached the objective!", true);
                        }
                        if (localPlayer.IsHost)
                        {
                            NetworkHandler.Instance.EventSendRpc(".moneyChanged", new(__ulong: player.actualClientId, __int: 200, __string: "silent")); // Give Hider Reward
                        }
                    }
                }
            }
        }
        else
        {
            Debug.LogError($"[ObjectiveManager] No objective has been set! '{Config.objective.Value}' Disabling Objective...");
            gameObject.SetActive(false);
        }
    }
    private void OnRoundStart()
    {
        objectiveReleased = false;
        playersReachedObjective.Clear();
    }

    public void SetPlayerReachedObjective(PlayerControllerB player, bool b)
    {
        if (!playersReachedObjective.ContainsKey(player))
        {
            playersReachedObjective.Add(player, b);
        }
        else
        {
            playersReachedObjective[player] = b;
        }
    }
    public bool PlayerReachedObjective(PlayerControllerB player)
    {
        if (!playersReachedObjective.ContainsKey(player))
        {
            return false;
        }
        else
        {
            return playersReachedObjective[player];
        }
    }
}
