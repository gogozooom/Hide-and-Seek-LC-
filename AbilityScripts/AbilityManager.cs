using GameNetcodeStuff;
using HideAndSeek.Patches;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Debug = Debugger.Debug;

namespace HideAndSeek.AbilityScripts
{
    public static class AbilityManager
    {
        // Player Manager
        public static IEnumerator ConnectStart() // Called at start of RoundManagerPatch
        {
            Debug.Log($"Step One; GameNetworkManager Exits?: {GameNetworkManager.Instance != null}");

            while (GameNetworkManager.Instance == null)
            {
                yield return new WaitForEndOfFrame();
            }

            Debug.Log($"Step One; localPlayer Exits?: {GameNetworkManager.Instance.localPlayerController != null}");

            while (GameNetworkManager.Instance.localPlayerController == null)
            {
                yield return new WaitForEndOfFrame();
            }

            Debug.Log($"We are done! localPlayer Exits?: {GameNetworkManager.Instance.localPlayerController != null}");

            foreach (var player in GameObject.FindObjectsOfType<PlayerControllerB>())
            {
                if (player.isPlayerControlled)
                {
                    PlayerJoined(player.actualClientId);
                }
            }
        }

        public static void PlayerJoined(ulong playerId)
        {
            PlayerControllerB player = RoundManagerPatch.GetPlayerWithClientId(playerId);

            if (!player)
            {
                Debug.LogWarning($"AbilityManager: Could not find player id {playerId}");
                return;
            }

            if (player.GetComponent<AbilityInstance>())
            {
                GameObject.Destroy(player.GetComponent<AbilityInstance>());
            }

            Debug.LogMessage($"AbilityManager: {player.playerUsername} got AbilityInstance script!");
            player.gameObject.AddComponent<AbilityInstance>();
        }

        // Money Manager
        public static void SellCurrentItem(ulong playerId)
        {
            PlayerControllerB player = RoundManagerPatch.GetPlayerWithClientId(playerId);
            AbilityInstance playerAbilities = player.GetComponent<AbilityInstance>();

            if (playerAbilities == null)
            {
                Debug.LogWarning("Tried to sell items for a player that does not posses a 'AbilityInstance' script! Adding one...");
                playerAbilities = player.gameObject.AddComponent<AbilityInstance>();
            }

            GrabbableObject item = player.ItemSlots[player.currentItemSlot];
            Debug.Log($"[Server] Attempting to sell selected object... '{item}'");
            if (item)
            {
                int value = item.scrapValue;
                if (value > 0) // Might need other checks
                {
                    Debug.Log($"[Server] Passed local checks! scrapvalue: '{value}'");

                    NetworkHandler.Instance.EventSendRpc(".destroyItem", new MessageProperties(__int: player.currentItemSlot, __ulong: playerId));

                    NetworkHandler.Instance.EventSendRpc(".moneyChanged", new MessageProperties(__int: value, __ulong: playerId));
                }
                else
                {
                    Debug.Log($"[Server] Could not pass value check! scrapvalue: '{value}'");
                }
            }
        }
    
        public static void BuyAbilityServer(ulong playerId, AbilityBase ability)
        {
            PlayerControllerB player = RoundManagerPatch.GetPlayerWithClientId(playerId);
            AbilityInstance playerAbilities = player.GetComponent<AbilityInstance>();

            if (playerAbilities == null)
            {
                Debug.LogWarning("Tried to buy ability for a player that does not posses a 'AbilityInstance' script! Adding one...");
                playerAbilities = player.gameObject.AddComponent<AbilityInstance>();
            }

            Debug.Log($"[Server] Attempting to buy seleted ability... '{ability.abilityName}'");
            if (ability != null)
            {
                // Valid check

                bool isSeeker = player == Plugin.seekerPlayer;

                if (ability.abilityCost > playerAbilities.money)
                {
                    Debug.Log($"[Server] Could not pass value check! AbilityCost: '{ability.abilityCost}'");
                    return;
                }

                if (!(ability.seekerAbility && isSeeker || ability.hiderAbility && !isSeeker)) // If neither are true
                {
                    Debug.Log($"[Server] Could not pass proper ability user check! AbilityName: '{ability.abilityName}'");
                    return;
                }

                if (ability.requiresRoundActive && !StartOfRound.Instance.shipHasLanded)
                {
                    Debug.Log($"[Server] Could not pass requiersRoundActive check! AbilityName: '{ability.abilityName}'");
                    return;
                }

                if (ability.requiresSeekerActive && TimeOfDay.Instance.currentDayTime <= Config.timeSeekerIsReleased.Value)
                {
                    Debug.Log($"[Server] Could not pass requiersRoundActive check! AbilityName: '{ability.abilityName}'");
                    return;
                }

                // Passed All Checks!

                Debug.Log($"[Server] Passed local checks! AbilityCost: '{ability.abilityCost}'");

                NetworkHandler.Instance.EventSendRpc(".moneyChanged", new MessageProperties(__int: -ability.abilityCost, __ulong: playerId));

                ability.ActivateServer(playerId);
            }
            else
            {
                Debug.LogWarning("[Server] Tried to buy a null ability!");
            }
        }
    }
}