using GameNetcodeStuff;
using HarmonyLib;
using System.Collections;
using UnityEngine;

namespace HideAndSeek.Patches;

[HarmonyPatch(typeof(ShotgunItem))]
public class ShotgunPatch
{
    [HarmonyPatch("StartReloadGun")]
    [HarmonyPrefix]
    static bool StartReloadGunPatch(ref int ___ammoSlotToUse, ref Animator ___gunAnimator, ref Coroutine ___gunCoroutine)
    {
        ShotgunItem _this = null;
        foreach (var Shotgun in GameObject.FindObjectsOfType<ShotgunItem>())
        {
            if (Shotgun.gunAnimator == ___gunAnimator)
            {
                _this = Shotgun;
                break;
            }
        }

        if (!Traverse.Create(_this).Method("ReloadedGun").GetValue<bool>() && !Config.shotgunInfiniteAmmo.Value)
        {
            _this.gunAudio.PlayOneShot(_this.noAmmoSFX);
            return false;
        }
        if (!_this.IsOwner)
        {
            return false;
        }
        if (___gunCoroutine != null)
        {
            _this.StopCoroutine(___gunCoroutine);
        }
        ___gunCoroutine = _this.StartCoroutine(Traverse.Create(_this).Method("reloadGunAnimation").GetValue<IEnumerator>());
        return false;
    }

    [HarmonyPatch("ItemActivate")]
    [HarmonyPrefix]
    static void ItemActivatePatch(ref int ___shellsLoaded)
    {
        if (Config.shotgunAutoReload.Value && Config.shotgunInfiniteAmmo.Value)
        {
            ___shellsLoaded = 2;
        }
    }
}

[HarmonyPatch(typeof(StartMatchLever))]
public class StartMatchLeverPatch
{
    [HarmonyPatch("LeverAnimation")]
    [HarmonyPrefix]
    public static bool LeverAnimationPatch()
    {
        PlayerControllerB localPlayer = GameNetworkManager.Instance.localPlayerController;

        //Debug.LogError("Ship lever animation! " + System.Environment.StackTrace);

        if (!HideAndSeekGM.instance.seekers.Contains(localPlayer) && !localPlayer.IsHost && !StartOfRound.Instance.inShipPhase && Config.lockShipLever.Value)
        {
            HUDManager.Instance.DisplayTip("Hide And Seek", "You are not allowed to end the round!", true);
            return false;
        }
        return true;
    }
}


[HarmonyPatch(typeof(InteractTrigger))]
public class InteractTriggerPatch
{
    [HarmonyPatch("Interact")]
    [HarmonyPrefix]
    static void InteractPatch(ref InteractTrigger __instance, ref Transform playerTransform)
    {
        if (__instance.name != "StartGameLever")
        {
            return;
        }

        Debug.Log($"LeverFlipped by {playerTransform.gameObject.name}");

        PlayerControllerB playerB = playerTransform.GetComponent<PlayerControllerB>();

        if (playerB)
        {
            Debug.Log($"id found: {playerB.actualClientId}");
            HideAndSeekGM.instance.leverLastFlippedBy = playerB.actualClientId;
            NetworkHandler.Instance.EventSendRpc(".leverFlipped", new MessageProperties() { _ulong = playerB.actualClientId });
        }
    }
}


[HarmonyPatch(typeof(Terminal))]
public class TerminalPatch
{
    [HarmonyPatch("RunTerminalEvents")]
    [HarmonyPostfix]
    static void RunTerminalEvents(ref int ___groupCredits)
    {
        ___groupCredits = 50000;
    }
}

[HarmonyPatch(typeof(GrabbableObject))]
public class GrabbableObjectPatch
{
    [HarmonyPatch("Update")]
    [HarmonyPrefix]
    static void UpdatePatch(ref GrabbableObject __instance)
    {
        if (Config.infiniteFlashlightBattery.Value && __instance.name.Contains("Flashlight"))
        {
            __instance.insertedBattery.charge = 1;
        }
    }
}


[HarmonyPatch(typeof(DeadBodyInfo))]
public class DeadBodyInfoPatch
{
    [HarmonyPatch("Update")]
    [HarmonyPrefix]
    static void UpdatePatch(ref DeadBodyInfo __instance)
    {
        if (Config.abilitiesEnabled.Value)
        {
            if (__instance.grabBodyObject)
            {
                if (__instance.grabBodyObject.scrapValue != Config.deadBodySellValue.Value)
                {
                    __instance.grabBodyObject.SetScrapValue(Config.deadBodySellValue.Value);
                }
            }
        }
    }
}