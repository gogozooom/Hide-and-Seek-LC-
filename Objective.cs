using GameNetcodeStuff;
using HideAndSeek.Patches;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using UnityEngine;

namespace HideAndSeek
{
    public class Objective
    {
        static Dictionary<ulong, bool> objectiveReached = new();

        public static bool roundStarted = false;
        public static bool objectiveReleased = false;
        static Coroutine currentTickLoop = null;

        public static void StartTicking()
        {
            if (currentTickLoop != null)
            {
                TimeOfDay.Instance.StopCoroutine(currentTickLoop);
                currentTickLoop = null;
            }
            currentTickLoop = TimeOfDay.Instance.StartCoroutine(Tick());
        }
        static IEnumerator Tick()
        {
            while (true)
            {
                if (StartOfRound.Instance.inShipPhase && roundStarted)
                {
                    roundStarted = false;
                    objectiveReleased = false;
                    objectiveReached.Clear();
                }
                else if (!StartOfRound.Instance.shipHasLanded && !roundStarted)
                {
                    roundStarted = true;
                }

                if (Config.objective.Value.Equals("Ship", System.StringComparison.CurrentCultureIgnoreCase))
                {
                    // Get To Ship Objective

                    PlayerControllerB localPlayer = GameNetworkManager.Instance.localPlayerController;

                    if (TimeOfDay.Instance.currentDayTime >= Config.timeObjectiveAvailable.Value)
                    {
                        if (!objectiveReleased)
                        {
                            objectiveReleased = true;


                            if (Plugin.seekers.Contains(localPlayer))
                            {
                                // Seeker
                                HUDManager.Instance.DisplayTip("Hide And Seek", "The exits have been unlocked, the hiders are escaping!", true);
                            } else if (Plugin.zombies.Contains(localPlayer))
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
                        foreach (var player in GameObject.FindObjectsByType<PlayerControllerB>(0))
                        {
                            if (player.isPlayerDead || !player.isPlayerControlled || Plugin.seekers.Contains(player) || Plugin.zombies.Contains(player)) continue;

                            if (player.isInHangarShipRoom && PlayerReachedObjective(player) == false)
                            {
                                SetPlayerReachedObjective(player, true);

                                player.usernameBillboardText.color = Config.objectiveNameColor.Value;

                                RoundManagerPatch.PlayerDied("Objective");

                                if (localPlayer == player)
                                {
                                    HUDManager.Instance.DisplayTip("Hide And Seek", "You have reached the objective!");
                                }
                                else
                                {
                                    HUDManager.Instance.DisplayTip("Hide And Seek", $"'{player.playerUsername}' has reached the objective!", true);
                                }
                            }
                        }
                    }
                }
                else
                {
                    Debug.LogError($"[ObjectiveManager] No objective has been set! '{Config.objective.Value}' Returing...");
                    yield break;
                }

                yield return new WaitForEndOfFrame();
            }
        }
        public static void SetPlayerReachedObjective(PlayerControllerB player, bool b)
        {
            if (!objectiveReached.ContainsKey(player.actualClientId))
            {
                objectiveReached.Add(player.actualClientId, b);
            }
            else
            {
                objectiveReached[player.actualClientId] = b;
            }
        }
        public static bool PlayerReachedObjective(PlayerControllerB player)
        {
            if (!objectiveReached.ContainsKey(player.actualClientId))
            {
                return false;
            }
            else
            {
                return objectiveReached[player.actualClientId];
            }
        }
    }
}
