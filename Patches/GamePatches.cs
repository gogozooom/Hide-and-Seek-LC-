using GameNetcodeStuff;
using HarmonyLib;
using HideAndSeek.AbilityScripts;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Debug = Debugger.Debug;

namespace HideAndSeek.Patches;


[HarmonyPatch(typeof(RoundManager))]
public class RoundManagerPatch
{
    [HarmonyPatch("Awake")]
    [HarmonyPrefix]
    static void AwakePatch()
    {
        HideAndSeekGM.Init();
    }

    [HarmonyPatch("LoadNewLevel")]
    [HarmonyPrefix]
    static bool LoadLevelPatch(ref int randomSeed, ref SelectableLevel newLevel)
    {
        if (HideAndSeekGM.instance.levelLoading == true)
        {
            Debug.LogError("Level got loaded more than once in a row??? Idk man...");
            return false;
        }

        HideAndSeekGM.instance.OnLevelLoaded(newLevel);

        return true;
    }
}

[HarmonyPatch(typeof(PlayerControllerB))]
public class PlayerControllerBPatch
{
    [HarmonyPatch("DestroyItemInSlotAndSync")]
    [HarmonyPrefix]
    static bool DestroyItemInSlotAndSyncPatch(int itemSlot)
    {
        Debug.LogMessage("DestroyItemAndSync + " + itemSlot);
        if (itemSlot < 0)
        {
            Debug.LogMessage("Item slot out of range!");
            return false;
        }
        return true;
    }
    [HarmonyPatch("DamagePlayer")]
    [HarmonyPrefix]
    static bool DamagePlayerPatch(CauseOfDeath causeOfDeath = CauseOfDeath.Unknown)
    {
        Debug.LogMessage($"Damaged recived: {causeOfDeath}");
        PlayerControllerB localPlayer = GameNetworkManager.Instance.localPlayerController;

        if (Objective.PlayerReachedObjective(localPlayer))
        {
            Debug.LogError($"Tried to damage player '{localPlayer.playerUsername}' but he has reached the objective!");
            return false; // Player reached objective!
        }

        if (Plugin.seekers.Contains(localPlayer) || Plugin.zombies.Contains(localPlayer)) // Is seeker
        {
            if (causeOfDeath == CauseOfDeath.Abandoned)
            {
                return false; // Let Seeker die
            }
            if (Config.isSeekerImmune.Value)
            {
                return false; // No seeker damage
            }
            if (causeOfDeath == CauseOfDeath.Gunshots)
            {
                //Debug.LogMessage("You can't die to your own weapon!");
                return false;
            }
        }
        return true;
    }
    [HarmonyPatch("DamagePlayerFromOtherClientClientRpc")]
    [HarmonyPrefix]
    static bool DamagePlayerFromOtherClientClientRpcPatch(ref int damageAmount, ref Vector3 hitDirection, ref int playerWhoHit, ref int newHealthAmount, ref PlayerControllerB __instance)
    {
        PlayerControllerB localPlayer = __instance;
        PlayerControllerB attacker = HideAndSeekGM.instance.GetPlayerWithClientId((ulong)playerWhoHit);

        Debug.LogError($"Hit Damage Recived! local '{Plugin.zombies.Contains(localPlayer) || Plugin.seekers.Contains(localPlayer)}' attacker '{Plugin.zombies.Contains(attacker) || Plugin.seekers.Contains(attacker)}'");
        if((Plugin.zombies.Contains(localPlayer) || Plugin.seekers.Contains(localPlayer)) && (Plugin.zombies.Contains(attacker) || Plugin.seekers.Contains(attacker)))
        {
            return false;
        }

        if(Plugin.zombies.Contains(attacker) || Plugin.seekers.Contains(attacker))
        {
            damageAmount = 90;
            newHealthAmount = localPlayer.health - damageAmount;
        }
        return true;
    }
    [HarmonyPatch("PlayFootstepSound")]
    [HarmonyPrefix]
    static bool PlayFootstepSoundPatch(ref PlayerControllerB __instance)
    {
        if (__instance?.GetComponent<AbilityInstance>()?.stealthActivated == true)
            return false;

        return true;
    }
    [HarmonyPatch("PlayJumpAudio")]
    [HarmonyPrefix]
    static bool PlayJumpAudioPatch(ref PlayerControllerB __instance)
    {
        if (__instance?.GetComponent<AbilityInstance>()?.stealthActivated == true)
            return false;

        return true;
    }

    [HarmonyPatch("PlayHitGroundAudio")]
    [HarmonyPrefix]
    static bool PlayHitGroundAudioPatch(ref PlayerControllerB __instance)
    {
        var abilityInstance = __instance.GetComponent<AbilityInstance>();

        if (!abilityInstance) return true; // Continue

        if (abilityInstance.stealthActivated)
        {
            return false;
        }

        return true;
    }        
}

[HarmonyPatch(typeof(EntranceTeleport))]
public class EntranceTeleportPatch
{
    [HarmonyPatch("TeleportPlayer")]
    [HarmonyPrefix]
    public static bool TeleportPlayerPatch()
    {
        List<PlayerControllerB> players = new List<PlayerControllerB>();
        foreach (var player in GameObject.FindObjectsByType<PlayerControllerB>(0))
        {
            if (player.isPlayerControlled)
            {
                players.Add(player);
            }
        }

        PlayerControllerB localPlayer = GameNetworkManager.Instance.localPlayerController;

        if(!Plugin.seekers.Contains(localPlayer) && !Plugin.zombies.Contains(localPlayer) && HideAndSeekGM.instance.playersTeleported >= players.Count && Config.lockHidersInside.Value && !Objective.objectiveReleased)
        {
            HUDManager.Instance.DisplayTip("???", "The entrance appears to be blocked.");
            return false;
        }
        else
        {
            return true;
        }
    }
}

[HarmonyPatch(typeof(TimeOfDay))]
public class TimeOfDayPatch
{
    [HarmonyPatch("SetBuyingRateForDay")]
    [HarmonyPostfix]
    public static void SetBuyingRateForDayPatch()
    {
        StartOfRound.Instance.companyBuyingRate = 1f;
    }
    [HarmonyPatch("UpdateProfitQuotaCurrentTime")]
    [HarmonyPrefix]
    public static bool UpdateProfitQuotaCurrentTimePatch()
    {
        HUDManagerPatch.UpdateRoundDisplay();
        return false;
    }
}

[HarmonyPatch(typeof(HUDManager))]
public class HUDManagerPatch
{
    public static int CurrentRound = 1;

    [HarmonyPatch("DisplayDaysLeft")]
    [HarmonyPostfix]
    public static void DisplayDaysLeftPatch()
    {
        CurrentRound++;

        UpdateRoundDisplay();
    }

    public static void UpdateRoundDisplay()
    {
        HUDManager.Instance.profitQuotaDaysLeftText.text = $"Round {CurrentRound}";
        HUDManager.Instance.profitQuotaDaysLeftText2.text = $"Round {CurrentRound}";
        StartOfRound.Instance.deadlineMonitorText.text = $"Round:\n {CurrentRound}";
        TimeOfDay.Instance.quotaVariables.deadlineDaysAmount = 3;
        TimeOfDay.Instance.profitQuota = 200;
        TimeOfDay.Instance.quotaFulfilled = 0;
        TimeOfDay.Instance.timeUntilDeadline = 3;
        TimeOfDay.Instance.daysUntilDeadline = 3;
        TimeOfDay.Instance.hoursUntilDeadline = 3;
    }
}

[HarmonyPatch(typeof(GameNetworkManager))]
public class GameNetworkManagerPatch
{
    public static GameObject networkPrefab;

    [HarmonyPatch("Start")]
    [HarmonyPostfix]
    public static void StartPatch()
    {
        if (networkPrefab)
        {
            NetworkManager.Singleton.RemoveNetworkPrefab(networkPrefab);
        }

        networkPrefab = (GameObject)Plugin.networkHandlerBundle.LoadAsset("NetworkHandler");
        networkPrefab.AddComponent<NetworkHandler>();

        NetworkManager.Singleton.AddNetworkPrefab(networkPrefab);
    }
}

[HarmonyPatch(typeof(StartOfRound))]
public class StartOfRoundPatch
{
    [HarmonyPatch("Awake")]
    [HarmonyPostfix]
    static void SpawnNetworkHandler()
    {
        if(NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)
        {
            var networkHandlerHost = GameObject.Instantiate(GameNetworkManagerPatch.networkPrefab, Vector3.zero, Quaternion.identity);
            networkHandlerHost.GetComponent<NetworkObject>().Spawn();
        }
    }
    [HarmonyPatch("FirePlayersAfterDeadlineClientRpc")]
    [HarmonyPrefix]
    static bool FirePlayersPatch()
    {
        Debug.LogError("Tried to fire players! Canceling");
        return false;
    }
    [HarmonyPatch("SetTimeAndPlanetToSavedSettings")]
    [HarmonyPrefix]
    static bool SetTimeAndPlanetToSavedSettingsPatch(ref StartOfRound __instance)
    {
        string currentSaveFileName = GameNetworkManager.Instance.currentSaveFileName;
        __instance.ChangeLevel(ES3.Load("CurrentPlanetID", currentSaveFileName, __instance.defaultPlanet));
        __instance.ChangePlanet();

        if (__instance.isChallengeFile)
        {
            TimeOfDay.Instance.totalTime = TimeOfDay.Instance.lengthOfHours * (float)TimeOfDay.Instance.numberOfHours;
            TimeOfDay.Instance.timeUntilDeadline = TimeOfDay.Instance.totalTime;
            TimeOfDay.Instance.profitQuota = 200;
        }
        else
        {
            HUDManagerPatch.UpdateRoundDisplay();
        }
        TimeOfDay.Instance.UpdateProfitQuotaCurrentTime();
        __instance.LoadPlanetsMoldSpreadData();
        __instance.SetPlanetsWeather(0);
        Object.FindObjectOfType<Terminal>().SetItemSales();
        if (__instance.gameStats.daysSpent == 0 && !__instance.isChallengeFile)
        {
            //__instance.PlayFirstDayShipAnimation(true); No
        }
        if (TimeOfDay.Instance.timeUntilDeadline > 0f && TimeOfDay.Instance.daysUntilDeadline <= 0 && TimeOfDay.Instance.timesFulfilledQuota <= 0)
        {
            //__instance.StartCoroutine(__instance.playDaysLeftAlertSFXDelayed()); No
        }
        return false;
    }
}