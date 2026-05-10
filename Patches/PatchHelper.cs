using GameNetcodeStuff;
using HarmonyLib;
using LCVR.Player;
using System;
using System.Collections;
using UnityEngine;

namespace HideAndSeek.Patches;

public static class PatchHelper
{
    public static Action<ulong> playerRevived;
    public static void ReviveAfterWaitAndCallRpc(PlayerControllerB player, float wait = 5)
    {
        if (!StartOfRound.Instance.shipHasLanded) { Debug.LogError("Can't revive someone while the ship is leaving!"); return; }
        GameNetworkManager.Instance.StartCoroutine(ReviveAfterWaitAndCallRpcC(player, wait));
    }
    static IEnumerator ReviveAfterWaitAndCallRpcC(PlayerControllerB player, float wait = 5)
    {
        yield return new WaitForSeconds(wait);

        RevivePlayerAndCallRpc(player);
    }
    public static void RevivePlayerAndCallRpc(PlayerControllerB player)
    {
        NetworkHandler.Instance.EventSendRpc(".revivePlayerLocal", new(__ulong: player.actualClientId));
    }
    static void ReviveVRPlayerLocal(PlayerControllerB player)
    {
        //SpectatorPlayerPatches.isSpectating = false;
        PlayerControllerB localPlayerController = StartOfRound.Instance.localPlayerController;
        if (localPlayerController.isPlayerDead != player)
        {
            return;
        }
        VRSession.Instance.VolumeManager.Saturation = 0f;
        VRSession.Instance.VolumeManager.VignetteIntensity = 0f;
        localPlayerController.thisPlayerModelArms.enabled = true;
        localPlayerController.isPlayerControlled = false;
        localPlayerController.takingFallDamage = false;
        //localPlayerController.isCameraDisabled = true;
        VRSession.Instance.LocalPlayer.LeftHandInteractor.enabled = true;
        VRSession.Instance.LocalPlayer.RightHandInteractor.enabled = true;
        HangarShipDoor hangarShipDoor = GameObject.FindFirstObjectByType<HangarShipDoor>();
        Transform transform = hangarShipDoor.transform.Find("HangarDoorLeft (1)");
        Transform transform2 = hangarShipDoor.transform.Find("HangarDoorRight (1)");
        Component component = hangarShipDoor.transform.Find("Cube");
        transform.GetComponent<BoxCollider>().isTrigger = false;
        transform2.GetComponent<BoxCollider>().isTrigger = false;
        component.GetComponent<BoxCollider>().isTrigger = false;
        localPlayerController.GetComponent<CharacterController>().excludeLayers = 0;
        VRSession.Instance.HUD.ToggleSpectatorLight(new bool?(false));
    }
    public static IEnumerator GiveZombieItems(PlayerControllerB player)
    {
        if (!string.IsNullOrEmpty(Config.zombieItemSlot1.Value))
        {
            yield return HideAndSeekGM.instance.SpawnNewItemCoroutine(Config.zombieItemSlot1.Value, player);
        }
        if (!string.IsNullOrEmpty(Config.zombieItemSlot2.Value))
        {
            yield return HideAndSeekGM.instance.SpawnNewItemCoroutine(Config.zombieItemSlot2.Value, player);
        }
        if (!string.IsNullOrEmpty(Config.zombieItemSlot3.Value))
        {
            yield return HideAndSeekGM.instance.SpawnNewItemCoroutine(Config.zombieItemSlot3.Value, player);
        }
        if (!string.IsNullOrEmpty(Config.zombieItemSlot4.Value))
        {
            yield return HideAndSeekGM.instance.SpawnNewItemCoroutine(Config.zombieItemSlot4.Value, player);
        }
    }
    static IEnumerator FixTip()
    {
        yield return new WaitForSeconds(2);
        HUDManager.Instance.tipsPanelAnimator.SetTrigger("TriggerHint");
    }
    public static void RevivePlayerLocal(PlayerControllerB player)
    {
        if (player == null)
        {
            Debug.LogError($"RevivePlayer({player}) Tried to revive null player!");
            return;
        }

        StartOfRound _this = StartOfRound.Instance;

        if (_this == null)
        {
            Debug.LogError($"RevivePlayer({player}) No start of round instance!");
            return;
        }

        if (_this.shipIsLeaving)
        {
            Debug.LogError($"RevivePlayer({player}) Tried to revive, but has already left!");
            return;
        }


        GameObject canvas = null;

        if (GameNetworkManager.Instance.localPlayerController == player)
        {
            canvas = GameObject.Find("Systems/UI/Canvas/IngamePlayerHUD");
            canvas.SetActive(false);
        }

        Debug.Log("Reviving players A");

        try
        {
            ReviveVRPlayerLocal(player);
        }
        catch (System.Exception)
        {
            Debug.LogError("ReviveVRPlayerLocal Ran into an error!");
            //throw;
        }

        player.ResetPlayerBloodObjects(player.isPlayerDead);
        if (player.isPlayerDead || player.isPlayerControlled)
        {
            player.isClimbingLadder = false;
            player.clampLooking = false;
            player.inVehicleAnimation = false;
            player.disableMoveInput = false;
            player.ResetZAndXRotation();
            player.thisController.enabled = true;
            player.health = 100;
            player.hasBeenCriticallyInjured = false;
            player.disableLookInput = false;
            player.disableInteract = false;
            Debug.Log("Reviving players B");
            if (player.isPlayerDead)
            {
                player.isPlayerDead = false;
                player.isPlayerControlled = true;
                player.isInElevator = true;
                player.isInHangarShipRoom = true;
                player.isInsideFactory = false;
                player.parentedToElevatorLastFrame = false;
                player.overrideGameOverSpectatePivot = null;
                _this.SetPlayerObjectExtrapolate(false);
                player.TeleportPlayer(_this.playerSpawnPositions[0].position, false, 0f, false, true); // TELEPORT HERE!!!
                player.setPositionOfDeadPlayer = false;
                player.DisablePlayerModel(player.gameObject, true, true);
                player.helmetLight.enabled = false;
                Debug.Log("Reviving players C");
                player.Crouch(false);
                player.criticallyInjured = false;
                if (player.playerBodyAnimator != null)
                {
                    player.playerBodyAnimator.SetBool("Limp", false);
                }
                player.bleedingHeavily = false;
                player.activatingItem = false;
                player.twoHanded = false;
                player.inShockingMinigame = false;
                player.inSpecialInteractAnimation = false;
                player.freeRotationInInteractAnimation = false;
                player.disableSyncInAnimation = false;
                player.inAnimationWithEnemy = null;
                player.holdingWalkieTalkie = false;
                player.speakingToWalkieTalkie = false;
                Debug.Log("Reviving players D");
                player.isSinking = false;
                player.isUnderwater = false;
                player.sinkingValue = 0f;
                player.statusEffectAudio.Stop();
                player.DisableJetpackControlsLocally();
                player.health = 100;
                Debug.Log("Reviving players E");
                player.mapRadarDotAnimator.SetBool("dead", false);
                player.externalForceAutoFade = Vector3.zero;
                if (player.IsOwner)
                {
                    HUDManager.Instance.gasHelmetAnimator.SetBool("gasEmitting", false);
                    player.hasBegunSpectating = false;
                    HUDManager.Instance.RemoveSpectateUI();
                    HUDManager.Instance.gameOverAnimator.SetTrigger("revive");
                    player.hinderedMultiplier = 1f;
                    player.isMovementHindered = 0;
                    player.sourcesCausingSinking = 0;
                    HUDManager.Instance.HideHUD(false);
                    Debug.Log("Reviving players E2");
                    player.reverbPreset = _this.shipReverb;
                }
            }
            Debug.Log("Reviving players F");
            SoundManager.Instance.earsRingingTimer = 0f;
            player.voiceMuffledByEnemy = false;
            SoundManager.Instance.playerVoicePitchTargets[(int)player.actualClientId] = 1f;
            SoundManager.Instance.SetPlayerPitch(1f, (int)player.actualClientId);
            if (player.currentVoiceChatIngameSettings == null)
            {
                _this.RefreshPlayerVoicePlaybackObjects();
            }
            if (player.currentVoiceChatIngameSettings != null)
            {
                if (player.currentVoiceChatIngameSettings.voiceAudio == null)
                {
                    player.currentVoiceChatIngameSettings.InitializeComponents();
                }
                if (player.currentVoiceChatIngameSettings.voiceAudio == null)
                {
                    return;
                }
                player.currentVoiceChatIngameSettings.voiceAudio.GetComponent<OccludeAudio>().overridingLowPass = false;
            }
            Debug.Log("Reviving players G");
        }

        PlayerControllerB playerControllerB = GameNetworkManager.Instance.localPlayerController;
        playerControllerB.bleedingHeavily = false;
        playerControllerB.criticallyInjured = false;
        playerControllerB.playerBodyAnimator.SetBool("Limp", false);
        playerControllerB.health = 100;
        HUDManager.Instance.UpdateHealthUI(100, false);
        playerControllerB.spectatedPlayerScript = null;
        HUDManager.Instance.audioListenerLowPass.enabled = false;
        Debug.Log($"Reviving players H {player.deadBody}");
        _this.SetSpectateCameraToGameOverMode(false, playerControllerB);

        _this.livingPlayers += 1;
        _this.UpdatePlayerVoiceEffects();

        if (canvas) // For "systems online" effect
        {
            canvas.SetActive(true);
            //GameObject.Find("Systems/UI/Canvas/DeathScreen").SetActive(false);
            _this.StartCoroutine(FixTip());
        }

        if (!Plugin.zombies.Contains(player))
        {
            Plugin.zombies.Add(player);
        }
        if (GameNetworkManager.Instance.localPlayerController == player)
        {
            player.thisPlayerModelArms.enabled = true;

            switch (Config.zombieSpawnLocation.Value)
            {
                case "Entrance":
                    EntranceTeleport entranceScript = (EntranceTeleport)AccessTools.Method(typeof(RoundManager), "FindMainEntranceScript").Invoke(null, [false]);

                    entranceScript.TeleportPlayer();
                    break;
                case "Inside":
                    EntranceTeleport insideScript = (EntranceTeleport)AccessTools.Method(typeof(RoundManager), "FindMainEntranceScript").Invoke(null, [true]);

                    insideScript.TeleportPlayer();
                    break;
                default:
                    break;
            }
        }
        if (GameNetworkManager.Instance.isHostingGame)
        {
            Debug.LogError("Giving player items!");
            _this.StartCoroutine(GiveZombieItems(player));
        }
        player.usernameBillboardText.color = Config.zombieNameColor.Value;
        playerRevived?.Invoke(player.actualClientId);
    }
}