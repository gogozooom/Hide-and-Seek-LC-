using BepInEx;
using GameNetcodeStuff;
using HarmonyLib;
using HideAndSeek.AbilityScripts;
using HideAndSeek.AbilityScripts.Extra;
using HideAndSeek.AudioScripts;
using LCVR.Player;
using LethalNetworkAPI;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using Unity.Netcode;
using UnityEngine;
using Debug = Debugger.Debug;

namespace HideAndSeek.Patches
{
    public static class PatchesManager
    {
        public static System.Action<ulong> playerRevived;
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
            HangarShipDoor hangarShipDoor = Object.FindObjectOfType<HangarShipDoor>();
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
                yield return RoundManagerPatch.SpawnNewItemCoroutine(Config.zombieItemSlot1.Value, player);
            }
            if (!string.IsNullOrEmpty(Config.zombieItemSlot2.Value))
            {
                yield return RoundManagerPatch.SpawnNewItemCoroutine(Config.zombieItemSlot2.Value, player);
            }
            if (!string.IsNullOrEmpty(Config.zombieItemSlot3.Value))
            {
                yield return RoundManagerPatch.SpawnNewItemCoroutine(Config.zombieItemSlot3.Value, player);
            }
            if (!string.IsNullOrEmpty(Config.zombieItemSlot4.Value))
            {
                yield return RoundManagerPatch.SpawnNewItemCoroutine(Config.zombieItemSlot4.Value, player);
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
    [HarmonyPatch(typeof(RoundManager))]
    public class RoundManagerPatch
    {
        public static RoundManager instance;
        public static int playersTeleported;
        public static int playersAlive;
        public static ulong leverLastFlippedBy = 999;
        public static List<(ulong playerId, Vector3 position)> itemSpawnPositions = new List<(ulong playerId, Vector3 position)>();

        [HarmonyPatch("Awake")]
        [HarmonyPrefix]
        static void AwakePatch()
        {
            Debug.LogMessage("RoundManager Awake started with variables..:");
            Debug.LogMessage($"instance = {instance}");
            Debug.LogMessage($"playersTeleported = {playersTeleported}");
            Debug.LogMessage($"playersAlive = {playersAlive}");
            Debug.LogMessage($"leverLastFlippedBy = {leverLastFlippedBy}");
            Debug.LogMessage($"itemSpawnPositions = {itemSpawnPositions}");

            if (leverLastFlippedBy != 999)
            {
                Debug.LogError("Player left half way through! Fixing variables");
                playersTeleported = 0;
                playersAlive = 0;
                leverLastFlippedBy = 999;
                levelLoading = false;
                lastSeekerId = 10001;
                pastSeekers.Clear();
                itemSpawnPositions.Clear();
                revivedPlayers.Clear();
                Plugin.seekers.Clear();
                Plugin.zombies.Clear();
            }
        }

        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        static void StartPatch(ref RoundManager __instance)
        {
            // Ability Sync
            if (!AudioManager.LoadedAudio)
            {
                Debug.LogWarning($"Loading AudioManager Audio!");
                __instance.StartCoroutine(AudioManager.LoadAudioCoroutine());
            }

            Debug.Log($"StartPatch, AbilityManager: Enabled = {Config.abilitiesEnabled.Value}");

            if (!Config.abilitiesEnabled.Value) { return; }

            GameObject.FindObjectOfType<RoundManager>().StartCoroutine(AbilityManager.ConnectStart());
            Objective.StartTicking();
        }

        static bool shipLeaving = false;

        [HarmonyPatch("Update")]
        [HarmonyPostfix]
        static void UpdatePatch()
        {
            if (!GameNetworkManager.Instance.localPlayerController) return; // No localPlayer, Exiting...

            bool isHost = GameNetworkManager.Instance.localPlayerController.IsServer;
            if (!isHost) return;

            int alivePlayerCount = StartOfRound.Instance.livingPlayers;

            if (playersAlive < alivePlayerCount)
            {
                Debug.LogWarning("Player count reset to: " + alivePlayerCount);
                playersAlive = alivePlayerCount;
            } else if (playersAlive > alivePlayerCount)
            {
                // Player Died!
                Debug.LogWarning("Player died! New Count: " + alivePlayerCount);
                playersAlive = alivePlayerCount;
                if (TimeOfDay.Instance.currentDayTime != 0)
                {
                    PlayerDied("Player Died!");
                }
            }
            if (StartOfRound.Instance.shipIsLeaving && shipLeaving == false)
            {
                // Round Starting
                shipLeaving = true;
            }
            else if (StartOfRound.Instance.inShipPhase && shipLeaving == true)
            {
                // Round Ended
                shipLeaving = false;

                if (Plugin.seekers.Count > 0)
                {
                    foreach (var player in GameObject.FindObjectsByType<PlayerControllerB>(0))
                    {
                        if (player.gameObject.activeSelf)
                        {
                            if (!Plugin.seekers.Contains(player) && !seekersWon)
                            {
                                NetworkHandler.Instance.EventSendRpc(".moneyChanged", new(__ulong: player.actualClientId, __int: 250, __string: "silent")); // Give Hiders Money
                                NetworkHandler.Instance.EventSendRpc(".tip", new(__ulong: player.actualClientId, __string: "You won, and got a reward!", __int: -1));
                            } else if (Plugin.seekers.Contains(player) && seekersWon)
                            {
                                NetworkHandler.Instance.EventSendRpc(".moneyChanged", new(__ulong: player.actualClientId, __int: 400, __string: "silent")); // Give Seekers Money
                                NetworkHandler.Instance.EventSendRpc(".tip", new(__ulong: player.actualClientId, __string: "You won, and got a reward!", __int: -1));
                            }
                        }
                    }
                }
            }
        }

        public static bool levelLoading = false;
        public static SelectableLevel currentLevel;
        [HarmonyPatch("LoadNewLevel")]
        [HarmonyPrefix]
        static bool LoadLevelPatch(ref int randomSeed, ref SelectableLevel newLevel)
        {
            Debug.LogMessage($"[LoadLevelPatch] LoadLevel Start!");
            if (levelLoading) { Debug.LogError("levelLoading is true! This should be abnormal!"); return false; }
            levelLoading = true;

            instance = GameObject.FindFirstObjectByType<RoundManager>();
            revivedPlayers.Clear();
            Plugin.seekers.Clear();
            Plugin.zombies.Clear();
            playersTeleported = 0;

            SyncingPatch.TeleportPlayer();

            seekersWon = false;
            currentLevel = newLevel;
            bool isHost = GameNetworkManager.Instance.isHostingGame;
            if (!isHost) return true;

            if (!Abilities.turretPrefab || !Abilities.landminePrefab)
            {
                foreach (var item in RoundManagerPatch.currentLevel.spawnableMapObjects)
                {
                    if (item.prefabToSpawn.GetComponentInChildren<Turret>() != null)
                    {
                        Abilities.turretPrefab = item.prefabToSpawn;
                    }
                    if (item.prefabToSpawn.GetComponentInChildren<Landmine>() != null)
                    {
                        Abilities.landminePrefab = item.prefabToSpawn;
                    }
                }
            }

            foreach (var item in Abilities.objectsToDespawnNextRound)
            {
                if (!item.IsSpawned) continue;

                item.Despawn();
                GameObject.Destroy(item.gameObject);
            }
            Abilities.objectsToDespawnNextRound = new();

            NetworkHandler.Instance.EventSendRpc(".levelLoading");

            if (Config.abilitiesEnabled.Value && Config.creditsResetOnNewRound.Value)
                NetworkHandler.Instance.EventSendRpc(".moneyChanged", new(__bool: true, __null: true));

            // -- Entitiy Removal --
            // Daytime Enemies
            foreach (var enemy in newLevel.DaytimeEnemies)
            {
                Debug.Log("Checking DaytimeEnemey: " + enemy.enemyType.enemyName);

                if (Config.disableAllDaytimeEntities.Value)
                {
                    Debug.Log("DisableAllDaytimeEntities is true! " + enemy.enemyType.enemyName);
                    enemy.rarity = 0;
                    continue;
                }

                switch (enemy.enemyType.enemyName)
                {
                    case ("Red Locust Bees"):
                        if (!Config.circuitBeeEnabled.Value)
                        {
                            enemy.rarity = 0;
                        }
                        break;
                    case ("Manticoil"):
                        if (!Config.manticoilEnabled.Value)
                        {
                            enemy.rarity = 0;
                        }
                        break;
                    case ("Docile Locust Bees"):
                        if (!Config.roamingLocustEnabled.Value)
                        {
                            enemy.rarity = 0;
                        }
                        break;
                }

                Debug.Log(enemy.enemyType.enemyName + ".Rarity = " + enemy.rarity);
            }
            // Outside Enemies
            foreach (var enemy in newLevel.OutsideEnemies)
            {
                Debug.Log("Checking OutsideEnemey: " + enemy.enemyType.enemyName);

                if (Config.disableAllOutsideEntities.Value)
                {
                    Debug.Log("DisableAllOutsideEntities is true! " + enemy.enemyType.enemyName);
                    enemy.rarity = 0;
                    continue;
                }

                switch (enemy.enemyType.enemyName)
                {
                    case ("MouthDog"):
                        if (!Config.eyelessDogEnabled.Value)
                        {
                            enemy.rarity = 0;
                        }
                        break;
                    case ("ForestGiant"):
                        if (!Config.forestKeeperEnabled.Value)
                        {
                            enemy.rarity = 0;
                        }
                        break;
                    case ("Baboon hawk"):
                        if (!Config.baboonHawkEnabled.Value)
                        {
                            enemy.rarity = 0;
                        }
                        break;
                    case ("Earth Leviathan"):
                        if (!Config.earthLeviathanEnabled.Value)
                        {
                            enemy.rarity = 0;
                        }
                        break;
                    case ("RadMech"):
                        if (!Config.mechEnabled.Value)
                        {
                            enemy.rarity = 0;
                        }
                        break;
                    case ("Tulip Snake"):
                        if (!Config.tulipSnakeEnabled.Value)
                        {
                            enemy.rarity = 0;
                        }
                        break;
                    case ("BushWolf"):
                        if (!Config.kidnapperFoxEnabled.Value)
                        {
                            enemy.rarity = 0;
                        }
                        break;
                }

                Debug.Log(enemy.enemyType.enemyName + ".Rarity = " + enemy.rarity);
            }
            // Enemies
            foreach (var enemy in newLevel.Enemies)
            {
                Debug.Log("Checking Enemey: " + enemy.enemyType.enemyName);

                if (Config.disableAllIndoorEntities.Value)
                {
                    Debug.Log("DisableAllIndoorEntities is true! " + enemy.enemyType.enemyName);
                    enemy.rarity = 0;
                    continue;
                }

                switch (enemy.enemyType.enemyName)
                {
                    case ("Centipede"):
                        if (!Config.snareFleaEnabled.Value)
                        {
                            enemy.rarity = 0;
                        }
                        break;
                    case ("Bunker Spider"):
                        if (!Config.bunkerSpiderEnabled.Value)
                        {
                            enemy.rarity = 0;
                        }
                        break;
                    case ("Hoarding bug"):
                        if (!Config.hoardingBugEnabled.Value)
                        {
                            enemy.rarity = 0;
                        }
                        break;
                    case ("Flowerman"):
                        if (!Config.brackenEnabled.Value)
                        {
                            enemy.rarity = 0;
                        }
                        break;
                    case ("Crawler"):
                        if (!Config.thumperEnabled.Value)
                        {
                            enemy.rarity = 0;
                        }
                        break;
                    case ("Blob"):
                        if (!Config.hygrodereEnabled.Value)
                        {
                            enemy.rarity = 0;
                        }
                        break;
                    case ("Girl"):
                        if (!Config.ghostGirlEnabled.Value)
                        {
                            enemy.rarity = 0;
                        }
                        break;
                    case ("Puffer"):
                        if (!Config.sporeLizardEnabled.Value)
                        {
                            enemy.rarity = 0;
                        }
                        break;
                    case ("Nutcracker"):
                        if (!Config.nutcrackerEnabled.Value)
                        {
                            enemy.rarity = 0;
                        }
                        break;
                    case ("Jester"):
                        if (!Config.jesterEnabled.Value)
                        {
                            enemy.rarity = 0;
                        }
                        break;
                    case ("Spring"):
                        if (!Config.coilHeadEnabled.Value)
                        {
                            enemy.rarity = 0;
                        }
                        break;
                    case ("Masked"):
                        if (!Config.maskedEnabled.Value)
                        {
                            enemy.rarity = 0;
                        }
                        break;
                    case ("Butler"):
                        if (!Config.butlerEnabled.Value)
                        {
                            enemy.rarity = 0;
                        }
                        break;
                    case ("Clay Surgeon"):
                        if (!Config.barberEnabled.Value)
                        {
                            enemy.rarity = 0;
                        }
                        break;
                    case ("CaveDweller"):
                        if (!Config.maneaterEnabled.Value)
                        {
                            enemy.rarity = 0;
                        }
                        break;
                }

                Debug.Log(enemy.enemyType.enemyName + ".Rarity = " + enemy.rarity);
            }

            // -- Hide And Seek --

            // Init Players
            List<PlayerControllerB> players = new();

            foreach (var player in GameObject.FindObjectsByType<PlayerControllerB>(0))
            {
                if (player.isPlayerControlled)
                {
                    Debug.LogMessage($"Found {player.name}! Adding to active list.");
                    players.Add(player);
                }
            }

            // Pick Seeker
            int seekersThisRound = 1;

            if (Config.numberOfSeekers.Value.Contains("%"))
            {
                seekersThisRound = Mathf.FloorToInt(float.Parse(Config.numberOfSeekers.Value.Replace("%", "")) / 100 * players.Count); // [config]% or players.Count
            }
            else
            {
                seekersThisRound = int.Parse(Config.numberOfSeekers.Value);
            }

            if (seekersThisRound >= players.Count)
            {
                seekersThisRound = players.Count - 1;
            }
            if (seekersThisRound <= 0)
            {
                seekersThisRound = 1;
            }

            Debug.LogWarning($"Number of seekers this round! = '{seekersThisRound}'");

            string seekersChosenS = "";

            for (int i = 0; i < seekersThisRound; i++)
            {
                PlayerControllerB player = PickRandomSeeker();

                Plugin.seekers.Add(player);
                if (seekersChosenS != "")
                {
                    seekersChosenS += ", ";
                }
                seekersChosenS += player.playerUsername;

                NetworkHandler.Instance.EventSendRpc(".playerChosen", new MessageProperties(__ulong: player.NetworkObjectId));
            }

            NetworkHandler.Instance.EventSendRpc(".seekersChosen", new MessageProperties(__string: seekersChosenS));


            itemSpawnPositions.Clear();

            instance.StartCoroutine(GivePlayersItems());

            levelLoading = false;

            NetworkHandler.Instance.EventSendRpc(".levelLoaded", new MessageProperties());

            // TMP Print Items
            /*

            Debugger.LogMessage("Logging Items!");
            var items = Resources.FindObjectsOfTypeAll<Item>();
            foreach (var item in items) 
            {
                Debugger.LogMessage("ItemFound! = " + item.itemName);
            }*/
            return true;
        }
        public static ulong lastSeekerId = 10001;
        public static List<ulong> pastSeekers = new List<ulong>();
        public static PlayerControllerB PickRandomSeeker()
        {
            string pickType = Config.seekerChooseBehavior.Value.ToLower().Trim().Replace(" ", "");
            if (Plugin.seekers.Count > 0)
            {
                pickType = Config.extraSeekerChooseBehavior.Value.ToLower().Trim().Replace(" ", "");
            }
            ulong seekerPlayerId = 10001;

            Debug.Log($"Random Type ['{pickType}']");
            if (pickType == "nodouble")
            {
                List<ulong> currentPool = new();
                foreach (var player in GameObject.FindObjectsByType<PlayerControllerB>(0))
                {
                    Debug.Log($"Searching {player}, {player.isPlayerControlled}, {player.actualClientId}");
                    if (player.isPlayerControlled && player.actualClientId != lastSeekerId && !Plugin.seekers.Contains(player))
                    {
                        Debug.Log($"Added {player}!");
                        currentPool.Add(player.actualClientId);
                    }
                }

                if (currentPool.Count > 0)
                {
                    int r = Random.Range(0, currentPool.Count);
                    Debug.Log("[NoDouble] RandomPlayerNumber = " + r);
                    seekerPlayerId = currentPool[r];
                }
                else
                {
                    seekerPlayerId = GameNetworkManager.Instance.localPlayerController.actualClientId;
                }
            }
            else if (pickType == "turns")
            {
                List<ulong> currentPool = new();

                // Update Pools
                foreach (var player in GameObject.FindObjectsByType<PlayerControllerB>(0))
                {
                    if (player.isPlayerControlled && !pastSeekers.Contains(player.actualClientId) && !Plugin.seekers.Contains(player)) // New player appears
                    {
                        currentPool.Add(player.actualClientId);
                    }
                }

                if (currentPool.Count <= 0)
                {
                    pastSeekers.Clear();
                    foreach (var player in GameObject.FindObjectsByType<PlayerControllerB>(0))
                    {
                        if (player.isPlayerControlled && !Plugin.seekers.Contains(player))
                        {
                            currentPool.Add(player.actualClientId);
                        }
                    }
                }

                // Generate
                int r = Random.Range(0, currentPool.Count);
                Debug.Log($"[Turns] RandomPlayerNumber = '{r}' Number In Current Pool '{currentPool.Count}'");
                seekerPlayerId = currentPool[r];

                // Upate Pools
                if (currentPool.Contains(seekerPlayerId))
                {
                    currentPool.Remove(seekerPlayerId);
                    pastSeekers.Add(seekerPlayerId);
                }


                Debug.LogWarning("---- New Pools! ----");
                Debug.LogWarning("CurrentPool = ");
                Debug.Log(currentPool.ToArray());
                Debug.LogWarning("PastSeekers = ");
                Debug.Log(pastSeekers.ToArray());
            }
            else if (pickType == "lever" && leverLastFlippedBy != 999)
            {
                Debug.Log($"It using new lever thingy hehehe; Player id ({leverLastFlippedBy})");
                seekerPlayerId = leverLastFlippedBy;
            }
            else if (pickType == "closest" && Plugin.seekers.Count > 0)
            {
                float closestDistance = float.PositiveInfinity;

                foreach (var player in GameObject.FindObjectsByType<PlayerControllerB>(0))
                {
                    if (player.isPlayerControlled && !Plugin.seekers.Contains(player))
                    {
                        if ((player.transform.position - Plugin.seekers[0].transform.position).magnitude < closestDistance)
                        {
                            seekerPlayerId = player.actualClientId;
                        }
                    }
                }

                Debug.Log("[Closets] Closest player number = " + seekerPlayerId);
            }
            else
            {
                List<ulong> currentPool = new();

                foreach (var player in GameObject.FindObjectsByType<PlayerControllerB>(0))
                {
                    if (player.isPlayerControlled && !Plugin.seekers.Contains(player))
                    {
                        currentPool.Add(player.actualClientId);
                    }
                }

                int r = Random.Range(0, currentPool.Count);
                Debug.Log("[Random] RandomPlayerNumber = " + r);
                seekerPlayerId = currentPool[r];
            }

            PlayerControllerB playerChosen = GetPlayerWithClientId(seekerPlayerId);

            if (playerChosen == null)
            {
                Debug.LogWarning($"Could not find player with id: '{seekerPlayerId}' using id: '0' instead");
                playerChosen = GetPlayerWithClientId(0);
            }

            lastSeekerId = seekerPlayerId;

            return playerChosen;
        }
        public static PlayerControllerB GetPlayerWithClientId(ulong playerId)
        {
            PlayerControllerB playerController = null;

            foreach (var player in GameObject.FindObjectsByType<PlayerControllerB>(0))
            {
                Debug.Log($"Looped through {player.name}; Client id {player.actualClientId}; Is controlled; {player.isPlayerControlled}");
                if (playerId == 0) // Strange First time fix
                {
                    if (GetFRFRId(player) == playerId)
                    {
                        playerController = player;

                        break;
                    }
                }
                else
                {
                    if (player.actualClientId == playerId)
                    {
                        playerController = player;

                        break;
                    }
                }
            }

            if (playerController == null)
            {
                bool bugFound = false;
                foreach (var player in GameObject.FindObjectsByType<PlayerControllerB>(0))
                {
                    if (player.OwnerClientId != player.actualClientId)
                    {
                        bugFound = true;
                        player.actualClientId = player.OwnerClientId;
                        Debug.LogWarning($"'{player.playerUsername}' found with an incorrect actualClientId!");
                    }
                }
                if (bugFound)
                {
                    return GetPlayerWithClientId(playerId);
                }
            }

            Debug.Log($"GetPlayerWithClientId({playerId}) Got player '{playerController}' with '{playerId}'");

            return playerController;
        }
        static ulong GetFRFRId(PlayerControllerB player)
        {
            string playerIDString = player.name.Replace("Player", "").Replace(" ", "").Replace("(", "").Replace(")", "");

            if (playerIDString == "")
            {
                return 0;
            }

            return ulong.Parse(playerIDString);
        }
        static bool seekersWon = false;
        static List<PlayerControllerB> revivedPlayers = new();
        public static void PlayerDied(string reason = "", bool checking = false)
        {
            int aliveHidersCount = 0;
            int hidersObjectivesCompleted = 0;
            int aliveSeekersCount = 0;
            int aliveZombieCount = 0;

            foreach (var player in GameObject.FindObjectsByType<PlayerControllerB>(0))
            {
                if (!player.isPlayerDead && player.gameObject.tag != "Decoy")
                {
                    if (Plugin.zombies.Contains(player))
                    {
                        // Alive Zombie
                        aliveZombieCount++;
                    } else if (Plugin.seekers.Contains(player))
                    {
                        // Alive Seeker
                        aliveSeekersCount++;
                    }
                    else if (Objective.PlayerReachedObjective(player))
                    {
                        hidersObjectivesCompleted++;
                    }
                    else
                    {
                        // Alive Hider
                        aliveHidersCount++;
                    }
                }
            }

            //if (checking) Debug.Log($"Checking dead people... Seekers dead: {aliveSeekersCount <= 0} Alive hider count: {aliveHidersCount} Is ship leaving: {StartOfRound.Instance.shipIsLeaving}");
            
            if (StartOfRound.Instance.shipIsLeaving) { return; }

            StartMatchLever lever = GameObject.FindAnyObjectByType<StartMatchLever>();
            if (aliveSeekersCount <= 0)
            {
                if (!checking)
                {
                    NetworkHandler.Instance.EventSendRpc(".tip", new MessageProperties() { _string = "Seeker Died; Hiders Win!", _bool = true });
                }
                Debug.LogMessage("_________ SEEKER DIED! _________");
                if (StartOfRound.Instance.shipHasLanded)
                {
                    lever.EndGame();
                    lever.LeverAnimation();
                    foreach (var player in GameObject.FindObjectsByType<PlayerControllerB>(0))
                    {
                        if (!player.isPlayerDead && player.isPlayerControlled && !Plugin.seekers.Contains(player) && !Plugin.zombies.Contains(player))
                        {
                            // Last alive hiders

                            int reward = Mathf.RoundToInt((1080 - Config.timeSeekerIsReleased.Value) * 12 / 60); // Total Reward

                            NetworkHandler.Instance.EventSendRpc(".moneyChanged", new(__ulong: player.actualClientId, __int: reward, __string: "silent")); // Give Hider Money
                        }
                    }
                }

                return;
            }
            else
            {
                if (!checking)
                {
                    Debug.LogMessage("_________ PLAYER DIED! _________");
                }

                if (aliveHidersCount <= 0 && GameNetworkManager.Instance.connectedPlayers != 1) // Last part is for testing in solo, so I don't get immidiently kicked out
                {
                    if (hidersObjectivesCompleted > 0)
                    {
                        if (!checking)
                        {
                            NetworkHandler.Instance.EventSendRpc(".tip", new MessageProperties() { _string = "Objective Reached; Hiders Win!", _bool = true });
                        }
                        Debug.LogMessage($"_________ HIDERS WON! _________ Objectives Complete? '{hidersObjectivesCompleted}'");
                        lever.EndGame();
                        lever.LeverAnimation();

                        foreach (var player in GameObject.FindObjectsByType<PlayerControllerB>(0))
                        {
                            if (!player.isPlayerDead && player.isPlayerControlled && !Plugin.seekers.Contains(player) && !Plugin.zombies.Contains(player))
                            {
                                // Last alive hiders

                                int reward = Mathf.RoundToInt((1080 - Config.timeSeekerIsReleased.Value) * 12 / 60); // Total Reward

                                NetworkHandler.Instance.EventSendRpc(".moneyChanged", new(__ulong: player.actualClientId, __int: reward, __string: "silent")); // Give Hider Money
                            }
                        }
                    }
                    else
                    {
                        if (!seekersWon)
                        {
                            seekersWon = true;
                        }
                        if (!checking)
                        {
                            NetworkHandler.Instance.EventSendRpc(".tip", new MessageProperties() { _string = "Seeker Won!", _bool = true });
                        }
                        Debug.LogMessage($"_________ SEEKER WON! _________ {GameNetworkManager.Instance.connectedPlayers} Connected player amount. != 1?: {StartOfRound.Instance.connectedPlayersAmount != 1} ");
                        lever.EndGame();
                        lever.LeverAnimation();
                    }
                }
                else if (aliveHidersCount >= 1 && !checking)
                {
                    if (aliveHidersCount == 1) // One hider left
                    {
                        if (Config.shipLeaveEarly.Value && Config.timeWhenLastHider.Value > TimeOfDay.Instance.currentDayTime)
                            NetworkHandler.Instance.EventSendRpc(".setDayTime", new(__float: Config.timeWhenLastHider.Value));

                        if (reason != "Objective")
                            NetworkHandler.Instance.EventSendRpc(".tip", new(__string: "1 Hider Remains..."));
                    }
                    else
                    {
                        if (reason != "Objective")
                            NetworkHandler.Instance.EventSendRpc(".tip", new(__string: $"{aliveHidersCount} Hiders Remain..."));
                    }
                }
            }

            if (!checking)
            {
                foreach (var player in GameObject.FindObjectsByType<PlayerControllerB>(0))
                {
                    if (player.isPlayerDead && !Plugin.zombies.Contains(player) && !Plugin.seekers.Contains(player))
                    {
                        if (revivedPlayers.Contains(player) && Config.deadZombiesRespawn.Value)
                        {
                            continue;
                        }

                        int reward = Mathf.RoundToInt((TimeOfDay.Instance.currentDayTime - Config.timeSeekerIsReleased.Value) * 12 / 60);

                        if (reward < 0) reward = 0;

                        Debug.LogError($"Hider {player.playerUsername} recived a reward of {reward}, survived for = {TimeOfDay.Instance.currentDayTime - Config.timeSeekerIsReleased.Value}");
                        NetworkHandler.Instance.EventSendRpc(".moneyChanged", new(__ulong: player.actualClientId, __int: reward, __string: "silent")); // Give Hider Money

                        foreach (var seeker in Plugin.seekers)
                        {
                            NetworkHandler.Instance.EventSendRpc(".moneyChanged", new(__ulong: seeker.actualClientId, __int: 50, __string: "silent")); // Give Seeker Money
                        }

                        Plugin.zombies.Add(player);
                        revivedPlayers.Add(player);

                        if (aliveHidersCount > 0 && Config.deadHidersRespawn.Value)
                            PatchesManager.ReviveAfterWaitAndCallRpc(player, Config.zombieSpawnDelay.Value);
                    }
                }
            }

            if (!checking)
            {
                Debug.LogMessage($"_________ ({aliveHidersCount+aliveSeekersCount}) Players Left! _________");
            }
        }
        public static IEnumerator GivePlayersItems()
        {
            List<PlayerControllerB> players = new();

            foreach (var player in GameObject.FindObjectsByType<PlayerControllerB>(0))
            {
                if (player.isPlayerControlled)
                {
                    players.Add(player);
                }
            }

            Debug.LogMessage("Game Has Started = " + GameNetworkManager.Instance.gameHasStarted);

            while (itemSpawnPositions.Count == 0 || playersTeleported == 0 || !StartOfRound.Instance.shipHasLanded)
            {
                if (GameNetworkManager.Instance.localPlayerController.IsHost)
                {
                    PlayerDied("Before Item Give", checking: true);
                }
                if (StartOfRound.Instance.shipIsLeaving)
                {
                    yield break;
                }

                //Debug.Log($"[Progress] Players Teleported: {playersTeleported}, item Spawn Positions: {itemSpawnPositions.Count}");
                yield return new WaitForSeconds(1);
            }

            int tries = 5;
            while (itemSpawnPositions.Count < players.Count || playersTeleported < players.Count || !StartOfRound.Instance.shipHasLanded)
            {
                if (tries <= 0)
                {
                    break;
                }
                Debug.Log($"[Progress Inter] Not fully there! We have: {tries} left before ending automatically! {playersTeleported}, {itemSpawnPositions.Count}");
                yield return new WaitForSeconds(1);
                tries--;
            }
            Debug.Log($"[Final] Players Teleported: {playersTeleported}, item Spawn Positions: {itemSpawnPositions.Count}");

            // Spawn Items
            foreach (var player in players)
            {
                if (Plugin.seekers.Contains(player))
                {
                    // Seeker
                    if (!string.IsNullOrEmpty(Config.seekerItemSlot1.Value))
                    {
                        yield return SpawnNewItemCoroutine(Config.seekerItemSlot1.Value, player);
                    }
                    if (!string.IsNullOrEmpty(Config.seekerItemSlot2.Value))
                    {
                        yield return SpawnNewItemCoroutine(Config.seekerItemSlot2.Value, player);
                    }
                    if (!string.IsNullOrEmpty(Config.seekerItemSlot3.Value))
                    {
                        yield return SpawnNewItemCoroutine(Config.seekerItemSlot3.Value, player);
                    }
                    if (!string.IsNullOrEmpty(Config.seekerItemSlot4.Value))
                    {
                        yield return SpawnNewItemCoroutine(Config.seekerItemSlot4.Value, player);
                    }
                }
                else
                {
                    // Hider
                    if (!string.IsNullOrEmpty(Config.hiderItemSlot1.Value))
                    {
                        yield return SpawnNewItemCoroutine(Config.hiderItemSlot1.Value, player);
                    }
                    if (!string.IsNullOrEmpty(Config.hiderItemSlot2.Value))
                    {
                        yield return SpawnNewItemCoroutine(Config.hiderItemSlot2.Value, player);
                    }
                    if (!string.IsNullOrEmpty(Config.hiderItemSlot3.Value))
                    {
                        yield return SpawnNewItemCoroutine(Config.hiderItemSlot3.Value, player);
                    }
                    if (!string.IsNullOrEmpty(Config.hiderItemSlot4.Value))
                    {
                        yield return SpawnNewItemCoroutine(Config.hiderItemSlot4.Value, player);
                    }
                }
            }
        }
        public static void SpawnNewItem(string itemName, PlayerControllerB player, bool forceSamePosition = false)
        {
            RoundManager.Instance.StartCoroutine(SpawnNewItemCoroutine(itemName, player, forceSamePosition));
        }
        public static IEnumerator SpawnNewItemCoroutine(string itemName, PlayerControllerB player, bool forceSamePosition = false)
        {
            Debug.LogMessage("SpawnNewItem()!");
            Item[] items = Resources.FindObjectsOfTypeAll<Item>();

            string targetItem = itemName;

            if (itemName.Contains(","))
            {
                string[] itemNames = itemName.Split(",");

                int r = Random.Range(0, itemNames.Length);

                targetItem = itemNames[r];
            }

            int i = 0;
            foreach (var item in items)
            {
                if (item.itemName.ToLower().Trim() == targetItem.ToLower().Trim())
                {
                    break;
                }
                i++;
            }

            if (i == items.Length)
            {
                Debug.LogWarning($"Could not find {targetItem} in items id list! (Look at README.md to see item IDs)");
                yield break;
            }

            Vector3 itemSpawnPosition = player.transform.position;
            bool newPositionFound = false;
            Debug.Log($"Spawing Item with spawn positions: {itemSpawnPositions.Count}");
            foreach (var idVector in itemSpawnPositions)
            {
                Debug.Log($"Scanning {idVector.playerId} with position {idVector.position}");
                if (idVector.playerId == player.actualClientId)
                {
                    newPositionFound = true;
                    itemSpawnPosition = idVector.position;
                    Debug.Log($"New Item Spawn Position! {itemSpawnPosition}");
                }
            }

            if (!newPositionFound)
            {
                Debug.LogError($"Could not find Spawn Position for player");
            }

            if (forceSamePosition)
                itemSpawnPosition = player.transform.position;

            itemSpawnPosition += Vector3.up * 0.3f;

            GrabbableObject newItem = GameObject.Instantiate(
                items[i].spawnPrefab,
                itemSpawnPosition,
                Quaternion.identity)
                .GetComponent<GrabbableObject>();

            newItem.fallTime = 0f;
            newItem.GetComponent<NetworkObject>().Spawn(false);
            newItem.NetworkObject.ChangeOwnership(player.actualClientId);

            Debug.LogMessage($"Spawning {newItem.name} for {player.playerUsername} at position {newItem.transform.position}");

            yield return new WaitForEndOfFrame();

            int totalItems = 0;
            foreach (var item in player.ItemSlots)
            {
                if (item)
                {
                    totalItems++;
                }
            }

            if (totalItems < 4) // Inventory not full
                NetworkHandler.Instance.EventSendRpc(".grabItem", new(__ulong: player.actualClientId, __extraMessage: newItem.NetworkObjectId.ToString()));

            yield return new WaitForEndOfFrame();
        }

        public static bool IsRoundActive()
        {
            List<PlayerControllerB> players = new();

            foreach (var player in GameObject.FindObjectsByType<PlayerControllerB>(0))
            {
                if (player.isPlayerControlled)
                {
                    players.Add(player);
                }
            }

            return playersTeleported >= players.Count && StartOfRound.Instance.shipHasLanded;
        }
    }

    [HarmonyPatch(typeof(Turret))]
    public class TurretPatch
    {
        [HarmonyPatch("CheckForPlayersInLineOfSight")]
        [HarmonyPrefix]
        static bool CheckForPlayersInLineOfSightPatch(ref Turret __instance, ref PlayerControllerB __result, ref float radius, ref bool angleRangeCheck)
        {
            if (Config.abilitiesEnabled.Value)
            {
                SpawnAbilityInfo spawnAbilityInfo = __instance.NetworkObject?.gameObject.GetComponent<SpawnAbilityInfo>();
                if (spawnAbilityInfo)
                {
                    Vector3 vector = __instance.aimPoint.forward;
                    vector = Quaternion.Euler(0f, (float)((int)(-(int)__instance.rotationRange)) / radius, 0f) * vector;
                    float num = __instance.rotationRange / radius * 2f;
                    int i = 0;
                    while (i <= 6)
                    {
                        Ray shootRay = new Ray(__instance.centerPoint.position, vector);
                        RaycastHit hit;
                        bool enteringBerserkMode = (bool)AccessTools.Field(typeof(Turret), "enteringBerserkMode").GetValue(__instance);

                        if (!Physics.Raycast(shootRay, out hit, 30f, 1051400, QueryTriggerInteraction.Ignore))
                        {
                            goto IL_168;
                        }

                        AccessTools.Field(typeof(Turret), "shootRay").SetValue(__instance, shootRay); // shootRay = out shootRay
                        AccessTools.Field(typeof(Turret), "hit").SetValue(__instance, hit); // hit = out hit

                        if (hit.transform.CompareTag("Player"))
                        {
                            PlayerControllerB component = hit.transform.GetComponent<PlayerControllerB>();
                            if (!(component == null))
                            {
                                if (angleRangeCheck && Vector3.Angle(component.transform.position + Vector3.up * 1.75f - __instance.centerPoint.position, __instance.forwardFacingPos.forward) > __instance.rotationRange)
                                {
                                    __result = null;
                                    return false;
                                }
                                if (component == spawnAbilityInfo.creatorPlayer || spawnAbilityInfo.otherFriendlies.Contains(component))
                                {
                                    Debug.LogError("Turret: Canceled behavior because is friendly!");
                                    __result = null;
                                    return false;
                                }


                                __result = component;
                                return false;
                            }
                        }
                        else
                        {
                            if ((__instance.turretMode != TurretMode.Firing && (__instance.turretMode != TurretMode.Berserk || enteringBerserkMode)) || !hit.transform.tag.StartsWith("PlayerRagdoll"))
                            {
                                goto IL_168;
                            }
                            Rigidbody component2 = hit.transform.GetComponent<Rigidbody>();
                            if (component2 != null)
                            {
                                component2.AddForce(vector.normalized * 42f, ForceMode.Impulse);
                                goto IL_168;
                            }
                            goto IL_168;
                        }
                    IL_185:
                        i++;
                        continue;
                    IL_168:
                        vector = Quaternion.Euler(0f, num / 6f, 0f) * vector;
                        goto IL_185;
                    }
                    __result = null;
                    return false;
                }
            }
            return true;
        }

        [HarmonyPatch("Start")]
        [HarmonyPrefix]
        static void StartPatch(Turret __instance)
        {
            if (!Config.turretsEnabled.Value && TimeOfDay.Instance.currentDayTime <= Config.timeSeekerIsReleased.Value) // When seeker is not active
            {
                Debug.LogWarning("Turret Found! Deleteing...");
                __instance.NetworkObject.Despawn();
                Debug.Log("Deleteing...");
            }
        }
    }
    [HarmonyPatch(typeof(Landmine))]
    public class LandminePatch
    {
        [HarmonyPatch("OnTriggerEnter")]
        [HarmonyPrefix]
        static bool OnTriggerEnterPatch(ref Landmine __instance, ref Collider other)
        {
            if (Config.abilitiesEnabled.Value)
            {
                SpawnAbilityInfo spawnAbilityInfo = __instance.NetworkObject?.gameObject.GetComponent<SpawnAbilityInfo>();
                if (spawnAbilityInfo)
                {
                    float pressMineDebounceTimer = (float)AccessTools.Field(typeof(Landmine), "pressMineDebounceTimer").GetValue(__instance);

                    if (__instance.hasExploded)
                    {
                        return false;
                    }
                    if (pressMineDebounceTimer > 0f)
                    {
                        return false;
                    }
                    if (other.CompareTag("Player"))
                    {
                        PlayerControllerB component = other.gameObject.GetComponent<PlayerControllerB>();
                        if (component != GameNetworkManager.Instance.localPlayerController)
                        {
                            return false;
                        }
                        if (component != null && !component.isPlayerDead)
                        {
                            if (component == spawnAbilityInfo.creatorPlayer || spawnAbilityInfo.otherFriendlies.Contains(component))
                            {
                                Debug.LogError("Landmine: Canceled behavior because is friendly!");
                                return false;
                            }

                            AccessTools.Field(typeof(Landmine), "localPlayerOnMine").SetValue(__instance, true); // localPlayerOnMine = true;
                            AccessTools.Field(typeof(Landmine), "pressMineDebounceTimer").SetValue(__instance, 0.5f); //pressMineDebounceTimer = 0.5f;
                            __instance.PressMineServerRpc();
                            return false;
                        }
                    }
                    else if (other.CompareTag("PhysicsProp") || other.tag.StartsWith("PlayerRagdoll"))
                    {
                        if (other.GetComponent<DeadBodyInfo>())
                        {
                            if (other.GetComponent<DeadBodyInfo>().playerScript != GameNetworkManager.Instance.localPlayerController)
                            {
                                return false;
                            }
                        }
                        else if (other.GetComponent<GrabbableObject>() && !other.GetComponent<GrabbableObject>().NetworkObject.IsOwner)
                        {
                            return false;
                        }
                        AccessTools.Field(typeof(Landmine), "pressMineDebounceTimer").SetValue(__instance, 0.5f); //pressMineDebounceTimer = 0.5f;
                        __instance.PressMineServerRpc();
                    }
                }
            }

            return true;
        }

        [HarmonyPatch("OnTriggerExit")]
        [HarmonyPrefix]
        static bool OnTriggerExitPatch(ref Landmine __instance, ref Collider other)
        {
            if (Config.abilitiesEnabled.Value)
            {
                SpawnAbilityInfo spawnAbilityInfo = __instance.NetworkObject?.gameObject.GetComponent<SpawnAbilityInfo>();
                if (spawnAbilityInfo)
                {
                    if (__instance.hasExploded)
                    {
                        return false;
                    }
                    if (!(bool)AccessTools.Field(typeof(Landmine), "mineActivated").GetValue(__instance)) // mineActivated == false
                    {
                        return false;
                    }
                    if (other.CompareTag("Player"))
                    {
                        PlayerControllerB component = other.gameObject.GetComponent<PlayerControllerB>();
                        if (component != null && !component.isPlayerDead)
                        {
                            if (component != GameNetworkManager.Instance.localPlayerController)
                            {
                                return false;
                            }
                            if (component == spawnAbilityInfo.creatorPlayer || spawnAbilityInfo.otherFriendlies.Contains(component))
                            {
                                Debug.LogError("Landmine: Canceled behavior because is friendly!");
                                return false;
                            }
                            AccessTools.Field(typeof(Landmine), "localPlayerOnMine").SetValue(__instance, false); //localPlayerOnMine = false;
                            AccessTools.Method(typeof(Landmine), "TriggerMineOnLocalClientByExiting").Invoke(__instance, null); // TriggerMineOnLocalClientByExiting();
                            return false;
                        }
                    }
                    else if (other.tag.StartsWith("PlayerRagdoll") || other.CompareTag("PhysicsProp"))
                    {
                        if (other.GetComponent<DeadBodyInfo>())
                        {
                            if (other.GetComponent<DeadBodyInfo>().playerScript != GameNetworkManager.Instance.localPlayerController)
                            {
                                return false;
                            }
                        }
                        else if (other.GetComponent<GrabbableObject>() && !other.GetComponent<GrabbableObject>().NetworkObject.IsOwner)
                        {
                            return false;
                        }
                        AccessTools.Method(typeof(Landmine), "TriggerMineOnLocalClientByExiting").Invoke(__instance, null); // TriggerMineOnLocalClientByExiting();
                    }
                }
            }

            return true;
        }

        [HarmonyPatch("Start")]
        [HarmonyPrefix]
        static void StartPatch(Landmine __instance)
        {
            if (!Config.landminesEnabled.Value && TimeOfDay.Instance.currentDayTime <= Config.timeSeekerIsReleased.Value) // When seeker is not active
            {
                Debug.LogWarning("Landmine Found!");
                __instance.NetworkObject.Despawn();
                Debug.Log("Deleteing...");
            }
        }
    }

    public class EnemyAIPatch
    {
        public static void UpdatePatch(EnemyAI _this)
        {
            var trav = Traverse.Create(_this);

            if (_this.enemyType.isDaytimeEnemy && !_this.daytimeEnemyLeaving)
            {
                AccessTools.Method(typeof(CrawlerAI), "CheckTimeOfDayToLeave").Invoke(_this, null); //_this.CheckTimeOfDayToLeave();
            }
            if (_this.stunnedIndefinitely <= 0)
            {
                if (_this.stunNormalizedTimer >= 0f)
                {
                    _this.stunNormalizedTimer -= Time.deltaTime / _this.enemyType.stunTimeMultiplier;
                }
                else
                {
                    _this.stunnedByPlayer = null;
                    if (_this.postStunInvincibilityTimer >= 0f)
                    {
                        _this.postStunInvincibilityTimer -= Time.deltaTime * 5f;
                    }
                }
            }
            if (!_this.ventAnimationFinished && _this.timeSinceSpawn < _this.exitVentAnimationTime + 0.005f * (float)RoundManager.Instance.numberOfEnemiesInScene)
            {
                _this.timeSinceSpawn += Time.deltaTime;
                if (!_this.IsOwner)
                {
                    Vector3 vector = _this.serverPosition;
                    if (_this.serverPosition != Vector3.zero)
                    {
                        _this.transform.position = _this.serverPosition;
                        _this.transform.eulerAngles = new Vector3(_this.transform.eulerAngles.x, (float)trav.Field("targetYRotation").GetValue(), _this.transform.eulerAngles.z);
                        return;
                    }
                }
                else
                {
                    float updateDestinationInterval = (float)trav.Field("updateDestinationInterval").GetValue();

                    if (updateDestinationInterval >= 0f)
                    {
                        updateDestinationInterval -= Time.deltaTime;
                        return;
                    }
                    _this.SyncPositionToClients();
                    updateDestinationInterval = 0.1f;

                    trav.Field("updateDestinationInterval").SetValue(updateDestinationInterval);
                }
                return;
            }
            if (!_this.inSpecialAnimation && !_this.ventAnimationFinished)
            {
                _this.ventAnimationFinished = true;
                if (_this.creatureAnimator != null)
                {
                    _this.creatureAnimator.SetBool("inSpawningAnimation", false);
                }
            }
            if (!_this.IsOwner)
            {
                if (_this.currentSearch.inProgress)
                {
                    _this.StopSearch(_this.currentSearch, true);
                }
                _this.SetClientCalculatingAI(false);
                if (!_this.inSpecialAnimation)
                {
                    Vector3 tempVelocity = (Vector3)trav.Field("tempVelocity").GetValue();

                    _this.transform.position = Vector3.SmoothDamp(_this.transform.position, _this.serverPosition, ref tempVelocity, _this.syncMovementSpeed);
                    _this.transform.eulerAngles = new Vector3(_this.transform.eulerAngles.x, Mathf.LerpAngle(_this.transform.eulerAngles.y, (float)trav.Field("targetYRotation").GetValue(), 15f * Time.deltaTime), _this.transform.eulerAngles.z);

                    trav.Field("tempVelocity").SetValue(tempVelocity);
                }
                _this.timeSinceSpawn += Time.deltaTime;
                return;
            }
            if (_this.isEnemyDead)
            {
                _this.SetClientCalculatingAI(false);
                return;
            }
            if (!_this.inSpecialAnimation)
            {
                _this.SetClientCalculatingAI(true);
            }
            if (_this.movingTowardsTargetPlayer && _this.targetPlayer != null)
            {
                float setDestinationToPlayerInterval = (float)trav.Field("setDestinationToPlayerInterval").GetValue();

                if (setDestinationToPlayerInterval <= 0f)
                {
                    setDestinationToPlayerInterval = 0.25f;
                    _this.destination = RoundManager.Instance.GetNavMeshPosition(_this.targetPlayer.transform.position, RoundManager.Instance.navHit, 2.7f, -1);
                }
                else
                {
                    _this.destination = new Vector3(_this.targetPlayer.transform.position.x, _this.destination.y, _this.targetPlayer.transform.position.z);
                    setDestinationToPlayerInterval -= Time.deltaTime;
                }
                if (_this.addPlayerVelocityToDestination > 0f)
                {
                    if (_this.targetPlayer == GameNetworkManager.Instance.localPlayerController)
                    {
                        _this.destination += Vector3.Normalize(_this.targetPlayer.thisController.velocity * 100f) * _this.addPlayerVelocityToDestination;
                    }
                    else if (_this.targetPlayer.timeSincePlayerMoving < 0.25f)
                    {
                        _this.destination += Vector3.Normalize((_this.targetPlayer.serverPlayerPosition - _this.targetPlayer.oldPlayerPosition) * 100f) * _this.addPlayerVelocityToDestination;
                    }
                }

                trav.Field("setDestinationToPlayerInterval").SetValue(setDestinationToPlayerInterval);
            }
            if (!_this.inSpecialAnimation)
            {
                float updateDestinationInterval = (float)trav.Field("updateDestinationInterval").GetValue();

                if (updateDestinationInterval >= 0f)
                {
                    updateDestinationInterval -= Time.deltaTime;
                }
                else
                {
                    _this.DoAIInterval();
                    updateDestinationInterval = _this.AIIntervalTime;
                }
                float previousYRotation = (float)trav.Field("previousYRotation").GetValue();
                if (Mathf.Abs(previousYRotation - _this.transform.eulerAngles.y) > 6f)
                {
                    trav.Field("previousYRotation").SetValue(_this.transform.eulerAngles.y);
                    trav.Field("targetYRotation").SetValue(_this.transform.eulerAngles.y); //_this.targetYRotation = _this.previousYRotation;
                    if (_this.IsServer)
                    {
                        AccessTools.Method(typeof(CrawlerAI), "UpdateEnemyRotationClientRpc").Invoke(_this, [(short)previousYRotation]); //_this.UpdateEnemyRotationClientRpc((short)previousYRotation);
                        return;
                    }
                    AccessTools.Method(typeof(CrawlerAI), "UpdateEnemyRotationServerRpc").Invoke(_this, [(short)previousYRotation]); //_this.UpdateEnemyRotationClientRpc((short)previousYRotation);
                }

                trav.Field("updateDestinationInterval").SetValue(updateDestinationInterval);
            }
        }
    }
    [HarmonyPatch(typeof(HoarderBugAI))]
    public class HoarderBugAIPatch
    {
        static bool GrabTargetItemIfClose(HoarderBugAI _this, SpawnAbilityInfo sInfo)
        {
            if (_this.targetItem != null && _this.heldItem == null && Vector3.Distance(_this.transform.position, _this.targetItem.transform.position) < 1f)
            {
                _this.nestPosition = sInfo.creatorPlayer.transform.position;

                if (!_this.SetDestinationToPosition(_this.nestPosition, true))
                {
                    _this.nestPosition = _this.ChooseClosestNodeToPosition(_this.transform.position, false, 0).position;
                    _this.SetDestinationToPosition(_this.nestPosition, false);
                }
                Debug.LogMessage($"FOUDN ITEM! '{_this.targetItem}' setting nest position to player...");

                NetworkObject component = _this.targetItem.GetComponent<NetworkObject>();
                _this.SwitchToBehaviourStateOnLocalClient(1);
                AccessTools.Method(typeof(HoarderBugAI), "GrabItem").Invoke(_this, [component]);
                Traverse.Create(_this).Field("sendingGrabOrDropRPC").SetValue(true);
                _this.GrabItemServerRpc(component);
                return true;
            }
            return false;
        }

        static bool IsHoarderBugAngry(HoarderBugAI _this)
        {
            if (_this.stunNormalizedTimer > 0f)
            {
                _this.angryTimer = 4f;
                if (_this.stunnedByPlayer)
                {
                    _this.angryAtPlayer = _this.stunnedByPlayer;
                }
                //Debug.LogWarning("Bug angy because you stun him!");
                return true;
            }
            int num = 0;
            int num2 = 0;
            for (int i = 0; i < HoarderBugAI.HoarderBugItems.Count; i++)
            {
                if (HoarderBugAI.HoarderBugItems[i].status == HoarderBugItemStatus.Stolen)
                {
                    num2++;
                }
                else if (HoarderBugAI.HoarderBugItems[i].status == HoarderBugItemStatus.Returned)
                {
                    num++;
                }
            }
            if (_this.angryTimer > 0f)
            {
                //Debug.LogWarning($"Bug angy because you near him/ his nest!  angy timer = {_this.angryTimer}");
            }
            else if (num2 > 0)
            {
                //Debug.LogWarning("Bug angy because you took his item");
            }

            return _this.angryTimer > 0f || num2 > 0;
        }

        [HarmonyPatch("IsHoarderBugAngry")]
        [HarmonyPrefix]
        static bool IsHoarderBugAngryPatch(ref HoarderBugAI __instance, ref bool __result)
        {
            __result = IsHoarderBugAngry(__instance);
            return false;
        }

        [HarmonyPatch("DoAIInterval")]
        [HarmonyPrefix]
        static bool DoAIIntervalPatch(ref HoarderBugAI __instance)
        {
            if (Config.abilitiesEnabled.Value)
            {
                SpawnAbilityInfo spawnAbilityInfo = __instance.NetworkObject?.gameObject.GetComponent<SpawnAbilityInfo>();
                var trav = Traverse.Create(__instance);

                if (spawnAbilityInfo)
                {
                    //Debug.Log($"'{__instance.currentBehaviourStateIndex}'");

                    if (__instance.moveTowardsDestination) // base.DoAIInterval()
                    {
                        __instance.agent.SetDestination(__instance.destination);
                    }
                    __instance.SyncPositionToClients();

                    if (__instance.isEnemyDead || StartOfRound.Instance.allPlayersDead)
                    {
                        return false;
                    }
                    if (!(bool)AccessTools.Field(typeof(HoarderBugAI), "choseNestPosition").GetValue(__instance))
                    {
                        AccessTools.Field(typeof(HoarderBugAI), "choseNestPosition").SetValue(__instance, true);
                        AccessTools.Method(typeof(HoarderBugAI), "ChooseNestPosition").Invoke(__instance, []);
                        return false;
                    }

                    if (__instance.CheckLineOfSightForPosition(__instance.nestPosition, 60f, 40, 0.5f, null))
                    {
                        for (int i = 0; i < HoarderBugAI.HoarderBugItems.Count; i++)
                        {
                            if (HoarderBugAI.HoarderBugItems[i].itemGrabbableObject.isHeld && HoarderBugAI.HoarderBugItems[i].itemNestPosition == __instance.nestPosition)
                            {
                                HoarderBugAI.HoarderBugItems[i].status = HoarderBugItemStatus.Stolen;
                            }
                        }
                    }
                    HoarderBugItem hoarderBugItem = __instance.CheckLineOfSightForItem(HoarderBugItemStatus.Stolen, 60f, 30, 3f);
                    if (hoarderBugItem != null && !hoarderBugItem.itemGrabbableObject.isHeld)
                    {
                        hoarderBugItem.status = HoarderBugItemStatus.Returned;
                        if (!HoarderBugAI.grabbableObjectsInMap.Contains(hoarderBugItem.itemGrabbableObject.gameObject))
                        {
                            HoarderBugAI.grabbableObjectsInMap.Add(hoarderBugItem.itemGrabbableObject.gameObject);
                        }
                    }
                    switch (__instance.currentBehaviourStateIndex)
                    {
                        case 0: // Roaming
                            trav.Field("inReturnToNestMode").SetValue(false);
                            AccessTools.Method(typeof(HoarderBugAI), "ExitChaseMode").Invoke(__instance, null);

                            if (!GrabTargetItemIfClose(__instance, spawnAbilityInfo))
                            {
                                if (__instance.targetItem == null && !__instance.searchForItems.inProgress)
                                {
                                    __instance.StartSearch(__instance.nestPosition, __instance.searchForItems);
                                    return false;
                                }
                                if (__instance.targetItem != null)
                                {
                                    Debug.Log($"Found target item! {__instance.targetItem.gameObject}");
                                    AccessTools.Method(typeof(HoarderBugAI), "SetGoTowardsTargetObject").Invoke(__instance, [__instance.targetItem.gameObject]);
                                    return false;
                                }
                                GameObject gameObject = __instance.CheckLineOfSight(HoarderBugAI.grabbableObjectsInMap, 60f, 40, 5f);
                                if (gameObject)
                                {
                                    GrabbableObject component = gameObject.GetComponent<GrabbableObject>();
                                    if (component && (!component.isHeld || (Random.Range(0, 100) < 4 && !component.isPocketed)))
                                    {
                                        Debug.Log($"Found target item! {gameObject}");
                                        AccessTools.Method(typeof(HoarderBugAI), "SetGoTowardsTargetObject").Invoke(__instance, [gameObject]);
                                        return false;
                                    }
                                }
                            }
                            break;
                        case 1: // Waiting / go to nest
                            /*
                            if ((bool)trav.Field("waitingAtNest").GetValue())
                                Debug.Log($"Waiting at nest!");
                            else
                                Debug.Log($"Go To nest! distance to nest = {Vector3.Distance(__instance.transform.position, __instance.agent.destination)}"); */

                            AccessTools.Method(typeof(HoarderBugAI), "ExitChaseMode").Invoke(__instance, null);
                            if (!(bool)trav.Field("inReturnToNestMode").GetValue())
                            {
                                trav.Field("inReturnToNestMode").SetValue(true);
                                AccessTools.Method(typeof(HoarderBugAI), "SetReturningToNest").Invoke(__instance, null);
                                Debug.Log(__instance.gameObject.name + ": Abandoned current search and returning to nest empty-handed");
                            }
                            GrabTargetItemIfClose(__instance, spawnAbilityInfo);
                            if ((bool)trav.Field("waitingAtNest").GetValue())
                            {
                                if (Vector3.Distance(__instance.transform.position, spawnAbilityInfo.creatorPlayer.transform.position) > 2.2f)
                                {
                                    Debug.LogMessage("Master running away!");
                                    trav.Field("waitingAtNest").SetValue(false);
                                    __instance.SwitchToBehaviourState(0);
                                    return false;
                                }

                                if (__instance.heldItem != null)
                                {
                                    AccessTools.Method(typeof(HoarderBugAI), "DropItemAndCallDropRPC").Invoke(__instance, [__instance.heldItem.itemGrabbableObject.GetComponent<NetworkObject>(), true]);
                                }
                                else
                                {
                                    GameObject gameObject2 = __instance.CheckLineOfSight(HoarderBugAI.grabbableObjectsInMap, 60f, 40, 5f);
                                    if (gameObject2 && Vector3.Distance(__instance.eye.position, gameObject2.transform.position) < 6f)
                                    {
                                        __instance.targetItem = gameObject2.GetComponent<GrabbableObject>();
                                        if (__instance.targetItem != null && !__instance.targetItem.isHeld)
                                        {
                                            trav.Field("waitingAtNest").SetValue(false);
                                            __instance.SwitchToBehaviourState(0);
                                            return false;
                                        }
                                    }
                                }
                                if ((float)trav.Field("waitingAtNestTimer").GetValue() <= 0f)
                                {
                                    if (!__instance.watchingPlayerNearPosition || __instance.watchingPlayer == spawnAbilityInfo.creatorPlayer)
                                    {
                                        Debug.Log("Gave item! Will return to search");
                                        trav.Field("waitingAtNest").SetValue(false);
                                        __instance.SwitchToBehaviourStateOnLocalClient(0);
                                    }
                                    return false;
                                }
                                return false;
                            }
                            else if (Vector3.Distance(__instance.transform.position, __instance.agent.destination) < 1.25f)
                            {
                                if (Vector3.Distance(__instance.transform.position, spawnAbilityInfo.creatorPlayer.transform.position) < 1.25f)
                                {
                                    trav.Field("waitingAtNest").SetValue(true);
                                    trav.Field("waitingAtNestTimer").SetValue(5f);
                                    return false;
                                }
                                else
                                {
                                    __instance.nestPosition = spawnAbilityInfo.creatorPlayer.transform.position; // Update nest Position

                                    if (!__instance.SetDestinationToPosition(__instance.nestPosition, true))
                                    {
                                        __instance.nestPosition = __instance.ChooseClosestNodeToPosition(__instance.transform.position, false, 0).position;
                                        __instance.SetDestinationToPosition(__instance.nestPosition, false);
                                    }
                                }
                            }
                            break;
                        case 2: // chasing player
                            trav.Field("inReturnToNestMode").SetValue(false);
                            if (__instance.heldItem != null)
                            {
                                AccessTools.Method(typeof(HoarderBugAI), "DropItemAndCallDropRPC").Invoke(__instance, [__instance.heldItem.itemGrabbableObject.GetComponent<NetworkObject>(), false]);
                            }
                            if ((bool)trav.Field("lostPlayerInChase").GetValue())
                            {
                                if (!__instance.searchForPlayer.inProgress)
                                {
                                    __instance.searchForPlayer.searchWidth = 30f;
                                    __instance.StartSearch(__instance.targetPlayer.transform.position, __instance.searchForPlayer);
                                    Debug.Log(__instance.gameObject.name + ": Lost player in chase; beginning search where the player was last seen");
                                    return false;
                                }
                            }
                            else
                            {
                                if (__instance.targetPlayer == null)
                                {
                                    Debug.LogError("TargetPlayer is null even though bug is in chase; setting targetPlayer to watchingPlayer");
                                    if (__instance.watchingPlayer != null)
                                    {
                                        __instance.targetPlayer = __instance.watchingPlayer;
                                    }
                                }
                                if (__instance.searchForPlayer.inProgress)
                                {
                                    __instance.StopSearch(__instance.searchForPlayer, true);
                                    Debug.Log(__instance.gameObject.name + ": Found player during chase; stopping search coroutine and moving after target player");
                                }
                                __instance.movingTowardsTargetPlayer = true;
                            }
                            break;
                        case 3:
                            break;
                        default:
                            return false;
                    }
                    return false;
                }
            }
            return true;
        }

        [HarmonyPatch("DetectAndLookAtPlayers")]
        [HarmonyPrefix]
        static bool DetectAndLookAtPlayersPatch(ref HoarderBugAI __instance)
        {
            // Owned Checks
            if (!Config.abilitiesEnabled.Value) return true;

            SpawnAbilityInfo spawnAbilityInfo = __instance.NetworkObject?.gameObject.GetComponent<SpawnAbilityInfo>();
            var trav = Traverse.Create(__instance);

            if (!spawnAbilityInfo) return true;
            // Owned Checks

            Vector3 position;
            if (__instance.currentBehaviourStateIndex == 1)
            {
                position = __instance.nestPosition;
            }
            else
            {
                position = __instance.transform.position;
            }
            PlayerControllerB[] allPlayersInLineOfSight = __instance.GetAllPlayersInLineOfSight(70f, 30, __instance.eye, 1.2f, -1);
            if (allPlayersInLineOfSight != null)
            {
                PlayerControllerB y = __instance.watchingPlayer;
                Traverse.Create(__instance).Field("timeSinceSeeingAPlayer").SetValue(0f); //__instance.timeSinceSeeingAPlayer = 0f;
                float num = 500f;
                bool flag = false;
                if (__instance.stunnedByPlayer != null && __instance.stunnedByPlayer != spawnAbilityInfo.creatorPlayer)
                {
                    flag = true;
                    __instance.angryAtPlayer = __instance.stunnedByPlayer;
                }
                for (int i = 0; i < allPlayersInLineOfSight.Length; i++)
                {
                    bool isFriendly = spawnAbilityInfo.otherFriendlies.Contains(allPlayersInLineOfSight[i]) || spawnAbilityInfo.creatorPlayer == allPlayersInLineOfSight[i];

                    if (!flag && allPlayersInLineOfSight[i].currentlyHeldObjectServer != null)
                    {
                        foreach (var hItem in HoarderBugAI.HoarderBugItems.ToArray())
                        {
                            if (hItem.itemGrabbableObject == allPlayersInLineOfSight[i].currentlyHeldObjectServer)
                            {
                                if (!isFriendly)
                                {
                                    Debug.LogWarning("Bug angy because you stol his stuff!");

                                    hItem.status = HoarderBugItemStatus.Stolen;
                                    __instance.angryAtPlayer = allPlayersInLineOfSight[i];
                                    flag = true;
                                }
                                else
                                {
                                    //Debug.LogMessage("Bug angy because you stol his stuff, but fren!");
                                    __instance.SwitchToBehaviourState(0);
                                    HoarderBugAI.HoarderBugItems.Remove(hItem);
                                }
                            }
                        }
                    }
                    if (IsHoarderBugAngry(__instance) && allPlayersInLineOfSight[i] == __instance.angryAtPlayer)
                    {
                        __instance.watchingPlayer = __instance.angryAtPlayer;
                    }
                    else
                    {
                        float num2 = Vector3.Distance(allPlayersInLineOfSight[i].transform.position, position);
                        if (num2 < num)
                        {
                            num = num2;
                            __instance.watchingPlayer = allPlayersInLineOfSight[i];
                        }
                    }
                    float distanceFromNest = Vector3.Distance(allPlayersInLineOfSight[i].transform.position, __instance.nestPosition);
                    if (HoarderBugAI.HoarderBugItems.Count > 0)
                    {
                        if ((distanceFromNest < 4f || ((bool)Traverse.Create(__instance).Field("inChase").GetValue() && distanceFromNest < 8f)) && __instance.angryTimer < 3.25f)
                        {
                            if (!isFriendly)
                            {
                                Debug.LogWarning("Bug angy because you close to nest!");
                                __instance.angryAtPlayer = allPlayersInLineOfSight[i];
                                __instance.watchingPlayer = allPlayersInLineOfSight[i];
                                __instance.angryTimer = 3.25f;
                                break;
                            }
                            else
                            {
                                //Debug.LogMessage("Bug angy because you close to nest, but fren!");
                                if (Vector3.Distance(__instance.transform.position, spawnAbilityInfo.creatorPlayer.transform.position) <= 1.25f && __instance.heldItem != null) // Close to nest, drop item
                                {
                                    AccessTools.Method(typeof(HoarderBugAI), "DropItemAndCallDropRPC").Invoke(__instance, [__instance.heldItem.itemGrabbableObject.GetComponent<NetworkObject>(), true]);
                                    trav.Field("waitingAtNest").SetValue(true);
                                    trav.Field("waitingAtNestTimer").SetValue(5f);
                                }
                                float creatorDistanceToClosestItem = Vector3.Distance(HoarderBugAI.HoarderBugItems[0].itemGrabbableObject.transform.position, spawnAbilityInfo.creatorPlayer.transform.position);

                                if (__instance.heldItem == null && creatorDistanceToClosestItem > 5f)
                                {
                                    __instance.SetDestinationToPosition(HoarderBugAI.HoarderBugItems[0].itemGrabbableObject.transform.position, false);
                                    __instance.targetItem = HoarderBugAI.HoarderBugItems[0].itemGrabbableObject;
                                }
                            }
                        }
                        if (!(bool)Traverse.Create(__instance).Field("isAngry").GetValue() && __instance.currentBehaviourStateIndex == 0 && distanceFromNest < 8f && (__instance.targetItem == null || Vector3.Distance(__instance.targetItem.transform.position, __instance.transform.position) > 7.5f) && __instance.IsOwner)
                        {
                            Debug.Log("Switching state to 1, Guarding Items!");
                            __instance.SwitchToBehaviourState(1);
                        }
                    }
                    if (__instance.currentBehaviourStateIndex != 2 && Vector3.Distance(__instance.transform.position, allPlayersInLineOfSight[i].transform.position) < 2.5f)
                    {
                        if (!isFriendly)
                        {
                            Debug.LogWarning("Bug angy because you touch him!");
                            float annoyanceMeter = (float)Traverse.Create(__instance).Field("annoyanceMeter").GetValue();

                            annoyanceMeter += 0.2f;
                            if (annoyanceMeter > 2.5f)
                            {
                                __instance.angryAtPlayer = allPlayersInLineOfSight[i];
                                __instance.watchingPlayer = allPlayersInLineOfSight[i];
                                __instance.angryTimer = 3.25f;
                            }

                            Traverse.Create(__instance).Field("annoyanceMeter").SetValue(annoyanceMeter);
                        }
                        else
                        {
                            //Debug.LogMessage("Bug angy because you touch him, but is fren!");
                        }
                    }
                }
                __instance.watchingPlayerNearPosition = (num < 6f);
                if (__instance.watchingPlayer != y)
                {
                    RoundManager.PlayRandomClip(__instance.creatureVoice, __instance.chitterSFX, true, 1f, 0, 1000);
                }
                if (!__instance.IsOwner)
                {
                    return false;
                }

                bool lostPlayerInChase = (bool)Traverse.Create(__instance).Field("lostPlayerInChase").GetValue();

                if (__instance.currentBehaviourStateIndex != 2)
                {
                    if (IsHoarderBugAngry(__instance))
                    {
                        lostPlayerInChase = false;
                        __instance.targetPlayer = __instance.watchingPlayer;
                        __instance.SwitchToBehaviourState(2);
                        return false;
                    }
                }
                else
                {
                    __instance.targetPlayer = __instance.watchingPlayer;
                    if (lostPlayerInChase)
                    {
                        lostPlayerInChase = false;
                        return false;
                    }
                }

                Traverse.Create(__instance).Field("lostPlayerInChase").SetValue(lostPlayerInChase);
            }
            else
            {
                float timeSinceSeeingAPlayer = (float)Traverse.Create(__instance).Field("timeSinceSeeingAPlayer").GetValue();
                timeSinceSeeingAPlayer += 0.2f;
                Traverse.Create(__instance).Field("timeSinceSeeingAPlayer").SetValue(timeSinceSeeingAPlayer);

                __instance.watchingPlayerNearPosition = false;
                if (__instance.currentBehaviourStateIndex != 2)
                {
                    if (timeSinceSeeingAPlayer > 1.5f)
                    {
                        __instance.watchingPlayer = null;
                        return false;
                    }
                }
                else
                {
                    if (timeSinceSeeingAPlayer > 1.25f)
                    {
                        __instance.watchingPlayer = null;
                    }
                    if (!__instance.IsOwner)
                    {
                        return false;
                    }
                    if (timeSinceSeeingAPlayer > 15f)
                    {
                        __instance.SwitchToBehaviourState(1);
                        return false;
                    }
                    if (timeSinceSeeingAPlayer > 2.5f)
                    {
                        Traverse.Create(__instance).Field("lostPlayerInChase").SetValue(true);
                    }
                }
            }
            return false;
        }

        [HarmonyPatch("OnCollideWithPlayer")]
        [HarmonyPrefix]
        static bool OnCollideWithPlayerPatch(ref HoarderBugAI __instance, Collider other)
        {
            if (!Config.abilitiesEnabled.Value) return true;

            SpawnAbilityInfo spawnAbilityInfo = __instance.NetworkObject?.gameObject.GetComponent<SpawnAbilityInfo>();
            var trav = Traverse.Create(__instance);

            if (!spawnAbilityInfo) return true;

            PlayerControllerB playerControllerB = __instance.MeetsStandardPlayerCollisionConditions(other, false, false);

            if (spawnAbilityInfo.creatorPlayer == playerControllerB || spawnAbilityInfo.otherFriendlies.Contains(playerControllerB))
            {
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(CrawlerAI))]
    public class CrawlerAIPatch
    {

        [HarmonyPatch("Update")]
        [HarmonyPrefix]
        static bool UpdatePatch(ref CrawlerAI __instance)
        {
            // Checks
            if (!Config.abilitiesEnabled.Value) return true;

            SpawnAbilityInfo spawnAbilityInfo = __instance.NetworkObject?.gameObject.GetComponent<SpawnAbilityInfo>();
            var trav = Traverse.Create(__instance);

            if (!spawnAbilityInfo) return true;

            // Modified Script
            EnemyAIPatch.UpdatePatch(__instance);
            if (__instance.isEnemyDead)
            {
                return false;
            }
            if (!__instance.IsOwner)
            {
                __instance.inSpecialAnimation = false;
            }
            AccessTools.Method(typeof(CrawlerAI), "CalculateAgentSpeed").Invoke(__instance, null); //__instance.CalculateAgentSpeed();
            trav.Field("timeSinceHittingPlayer").SetValue((float)trav.Field("timeSinceHittingPlayer").GetValue() + Time.deltaTime); //__instance.timeSinceHittingPlayer += Time.deltaTime;
            if (GameNetworkManager.Instance.localPlayerController.HasLineOfSightToPosition(__instance.transform.position + Vector3.up * 0.25f, 80f, 25, 5f)
                && !(GameNetworkManager.Instance.localPlayerController == spawnAbilityInfo.creatorPlayer || spawnAbilityInfo.otherFriendlies.Contains(GameNetworkManager.Instance.localPlayerController))) // Local player is not friendly
            {
                if (__instance.currentBehaviourStateIndex == 1)
                {
                    GameNetworkManager.Instance.localPlayerController.IncreaseFearLevelOverTime(0.8f, 1f);
                }
                else
                {
                    GameNetworkManager.Instance.localPlayerController.IncreaseFearLevelOverTime(0.8f, 0.5f);
                }
            }
            int currentBehaviourStateIndex = __instance.currentBehaviourStateIndex;
            if (currentBehaviourStateIndex != 0)
            {
                if (currentBehaviourStateIndex != 1)
                {
                    return false;
                }
                if (!(bool)trav.Field("hasEnteredChaseMode").GetValue()) //__instance.hasEnteredChaseMode
                {
                    trav.Field("hasEnteredChaseMode").SetValue(true); //__instance.hasEnteredChaseMode = true;
                    trav.Field("lostPlayerInChase").SetValue(false); //__instance.lostPlayerInChase = false;
                    trav.Field("checkLineOfSightInterval").SetValue(0f); //__instance.checkLineOfSightInterval = 0f;
                    __instance.noticePlayerTimer = 0f;
                    trav.Field("beginningChasingThisClient").SetValue(false); //__instance.beginningChasingThisClient = false;
                    __instance.useSecondaryAudiosOnAnimatedObjects = true;
                    __instance.openDoorSpeedMultiplier = 1.5f;
                    __instance.agent.stoppingDistance = 0.5f;
                    __instance.agent.speed = 0f;
                }
                if (!__instance.IsOwner)
                {
                    return false;
                }
                if (__instance.stunNormalizedTimer > 0f)
                {
                    return false;
                }
                float checkLineOfSightInterval = (float)trav.Field("checkLineOfSightInterval").GetValue();
                if (checkLineOfSightInterval <= 0.075f)
                {
                    checkLineOfSightInterval += Time.deltaTime;
                    trav.Field("checkLineOfSightInterval").SetValue(checkLineOfSightInterval);
                    return false;
                }
                checkLineOfSightInterval = 0f;
                trav.Field("checkLineOfSightInterval").SetValue(checkLineOfSightInterval);

                /*
                if (!(bool)trav.Field("ateTargetPlayerBody").GetValue()//__instance.ateTargetPlayerBody 
                    && __instance.targetPlayer != null 
                    && __instance.targetPlayer.deadBody != null 
                    && __instance.targetPlayer.deadBody.grabBodyObject != null
                    && __instance.targetPlayer.deadBody.grabBodyObject.grabbableToEnemies 
                    && (Coroutine)trav.Field("eatPlayerBodyCoroutine").GetValue()  == null 
                    && Vector3.Distance(__instance.transform.position, __instance.targetPlayer.deadBody.bodyParts[0].transform.position) < 3.3f) // Eat Dead Body Animation
                {
                    Debug.Log("Crawler: Eat player body start");
                    __instance.ateTargetPlayerBody = true;
                    __instance.inSpecialAnimation = true;
                    trav.Field("eatPlayerBodyCoroutine").SetValue(
                        __instance.StartCoroutine(__instance.EatPlayerBodyAnimation((int)__instance.targetPlayer.playerClientId))
                        ); //__instance.eatPlayerBodyCoroutine = __instance.StartCoroutine(__instance.EatPlayerBodyAnimation((int)__instance.targetPlayer.playerClientId));
                    __instance.EatPlayerBodyServerRpc((int)__instance.targetPlayer.playerClientId);
                } */
                // Not worth it hehe.

                bool lostPlayerInChase = (bool)trav.Field("lostPlayerInChase").GetValue();

                if (lostPlayerInChase)
                {
                    PlayerControllerB playerControllerB = __instance.CheckLineOfSightForPlayer(55f, 60, -1);
                    if (playerControllerB)
                    {
                        __instance.noticePlayerTimer = 0f;
                        lostPlayerInChase = false;
                        __instance.MakeScreechNoiseServerRpc();
                        if (playerControllerB != __instance.targetPlayer)
                        {
                            __instance.SetMovingTowardsTargetPlayer(playerControllerB);
                            //__instance.ateTargetPlayerBody = false; Not worth it hehe.
                            __instance.ChangeOwnershipOfEnemy(playerControllerB.actualClientId);
                            return false;
                        }
                    }
                    else
                    {
                        __instance.noticePlayerTimer -= 0.075f;
                        if (__instance.noticePlayerTimer < -15f)
                        {
                            __instance.SwitchToBehaviourState(0);
                            return false;
                        }
                    }
                }
                else
                {
                    PlayerControllerB playerControllerB2 = __instance.CheckLineOfSightForPlayer(65f, 80, -1);
                    if (playerControllerB2 != null)
                    {
                        __instance.noticePlayerTimer = 0f;
                        trav.Field("lastPositionOfSeenPlayer").SetValue(playerControllerB2.transform.position);
                        if (playerControllerB2 != __instance.targetPlayer)
                        {
                            __instance.targetPlayer = playerControllerB2;
                            //__instance.ateTargetPlayerBody = false; not worth it hehe
                            __instance.ChangeOwnershipOfEnemy(__instance.targetPlayer.actualClientId);
                            return false;
                        }
                    }
                    else
                    {
                        __instance.noticePlayerTimer += 0.075f;
                        if (__instance.noticePlayerTimer > 1.8f)
                        {
                            lostPlayerInChase = true;
                        }
                    }
                }

                trav.Field("lostPlayerInChase").SetValue(lostPlayerInChase);
            }
            else
            {
                if ((bool)trav.Field("hasEnteredChaseMode").GetValue()) //__instance.hasEnteredChaseMode
                {
                    trav.Field("hasEnteredChaseMode").SetValue(false); //__instance.hasEnteredChaseMode = false;
                    __instance.searchForPlayers.searchWidth = 25f;
                    trav.Field("beginningChasingThisClient").SetValue(false); //__instance.beginningChasingThisClient = false;
                    __instance.noticePlayerTimer = 0f;
                    __instance.useSecondaryAudiosOnAnimatedObjects = false;
                    __instance.openDoorSpeedMultiplier = 0.6f;
                    __instance.agent.stoppingDistance = 0f;
                    __instance.agent.speed = 7f;
                }

                float checkLineOfSightInterval = (float)trav.Field("checkLineOfSightInterval").GetValue();
                if (checkLineOfSightInterval <= 0.05f)
                {
                    checkLineOfSightInterval += Time.deltaTime;
                    trav.Field("checkLineOfSightInterval").SetValue(checkLineOfSightInterval);
                    return false;
                }
                checkLineOfSightInterval = 0f;
                trav.Field("checkLineOfSightInterval").SetValue(checkLineOfSightInterval);
                PlayerControllerB currentTargetPlayer;
                if (__instance.stunnedByPlayer != null)
                {
                    currentTargetPlayer = __instance.stunnedByPlayer;
                    __instance.noticePlayerTimer = 1f;
                }
                else
                {
                    currentTargetPlayer = __instance.CheckLineOfSightForPlayer(55f, 60, -1);
                }

                if (currentTargetPlayer == spawnAbilityInfo.creatorPlayer || spawnAbilityInfo.otherFriendlies.Contains(currentTargetPlayer))
                {
                    Debug.Log("Crawler: Found Friendly, Canceled Chasing!");
                    currentTargetPlayer = null;
                }

                if (!(currentTargetPlayer == GameNetworkManager.Instance.localPlayerController))
                {
                    __instance.noticePlayerTimer -= Time.deltaTime;
                    return false;
                }
                __instance.noticePlayerTimer = Mathf.Clamp(__instance.noticePlayerTimer + 0.05f, 0f, 10f);
                bool beginningChasingThisClient = (bool)trav.Field("beginningChasingThisClient").GetValue();
                if (__instance.noticePlayerTimer > 0.2f && !beginningChasingThisClient)
                {
                    beginningChasingThisClient = true;
                    __instance.BeginChasingPlayerServerRpc((int)GameNetworkManager.Instance.localPlayerController.playerClientId);
                    __instance.ChangeOwnershipOfEnemy(currentTargetPlayer.actualClientId);
                    Debug.Log("Begin chasing on local client");
                    return false;
                }
                trav.Field("beginningChasingThisClient").SetValue(beginningChasingThisClient);
            }
            return false;
        }

        [HarmonyPatch("OnCollideWithPlayer")]
        [HarmonyPrefix]
        static bool OnCollideWithPlayerPatch(ref CrawlerAI __instance, Collider other)
        {
            if (!Config.abilitiesEnabled.Value) return true;

            SpawnAbilityInfo spawnAbilityInfo = __instance.NetworkObject?.gameObject.GetComponent<SpawnAbilityInfo>();

            if (!spawnAbilityInfo) return true;

            PlayerControllerB playerControllerB = __instance.MeetsStandardPlayerCollisionConditions(other, false, false);

            if (spawnAbilityInfo.creatorPlayer == playerControllerB || spawnAbilityInfo.otherFriendlies.Contains(playerControllerB))
            {
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(FlowermanAI))]
    public class FlowermanAIPatch
    {
        static bool PathIsIntersectedByLineOfSight(EnemyAI _this, Vector3 targetPos, bool calculatePathDistance = false, bool avoidLineOfSight = true)
        {
            _this.pathDistance = 0f;
            if (_this.agent.isOnNavMesh && !_this.agent.CalculatePath(targetPos, _this.path1))
            {
                return true;
            }
            if (_this.path1 == null || _this.path1.corners.Length == 0)
            {
                return true;
            }
            if (Vector3.Distance(_this.path1.corners[_this.path1.corners.Length - 1], RoundManager.Instance.GetNavMeshPosition(targetPos, RoundManager.Instance.navHit, 2.7f, -1)) > 1.5f)
            {
                if (_this.DebugEnemy)
                {
                    Debug.Log("Path is not complete; final waypoint of path was too far from target position");
                }
                return true;
            }
            if (calculatePathDistance)
            {
                for (int j = 1; j < _this.path1.corners.Length; j++)
                {
                    _this.pathDistance += Vector3.Distance(_this.path1.corners[j - 1], _this.path1.corners[j]);
                    if (avoidLineOfSight && Physics.Linecast(_this.path1.corners[j - 1], _this.path1.corners[j], 262144))
                    {
                        return true;
                    }
                }
            }
            else if (avoidLineOfSight)
            {
                for (int k = 1; k < _this.path1.corners.Length; k++)
                {
                    UnityEngine.Debug.DrawLine(_this.path1.corners[k - 1], _this.path1.corners[k], Color.green);
                    if (Physics.Linecast(_this.path1.corners[k - 1], _this.path1.corners[k], 262144))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        static bool TargetClosestPlayer(FlowermanAI _this, float bufferDistance = 1.5f, bool requireLineOfSight = false, float viewWidth = 70f)
        {
            SpawnAbilityInfo spawnAbilityInfo = _this.NetworkObject.gameObject.GetComponent<SpawnAbilityInfo>();

            _this.mostOptimalDistance = 2000f;
            PlayerControllerB playerControllerB = _this.targetPlayer;
            _this.targetPlayer = null;
            for (int i = 0; i < StartOfRound.Instance.connectedPlayersAmount + 1; i++)
            {
                if (_this.PlayerIsTargetable(StartOfRound.Instance.allPlayerScripts[i])
                    && !_this.PathIsIntersectedByLineOfSight(StartOfRound.Instance.allPlayerScripts[i].transform.position, calculatePathDistance: false, avoidLineOfSight: false)
                    && (!requireLineOfSight || _this.CheckLineOfSightForPosition(StartOfRound.Instance.allPlayerScripts[i].gameplayCamera.transform.position, viewWidth, 40))
                    && !(StartOfRound.Instance.allPlayerScripts[i] == spawnAbilityInfo.creatorPlayer || spawnAbilityInfo.otherFriendlies.Contains(StartOfRound.Instance.allPlayerScripts[i]))) // Is not friendly!
                {
                    _this.tempDist = Vector3.Distance(_this.transform.position, StartOfRound.Instance.allPlayerScripts[i].transform.position);
                    if (_this.tempDist < _this.mostOptimalDistance)
                    {
                        _this.mostOptimalDistance = _this.tempDist;
                        _this.targetPlayer = StartOfRound.Instance.allPlayerScripts[i];
                    }
                }
            }

            if (_this.targetPlayer != null && bufferDistance > 0f && playerControllerB != null && Mathf.Abs(_this.mostOptimalDistance - Vector3.Distance(_this.transform.position, playerControllerB.transform.position)) < bufferDistance)
            {
                _this.targetPlayer = playerControllerB;
            }

            return _this.targetPlayer != null;
        }

        [HarmonyPatch("DoAIInterval")]
        [HarmonyPrefix]
        static bool DoAIIntervalPatch(ref FlowermanAI __instance)
        {
            // Checks
            if (!Config.abilitiesEnabled.Value) return true;

            SpawnAbilityInfo spawnAbilityInfo = __instance.NetworkObject?.gameObject.GetComponent<SpawnAbilityInfo>();
            var trav = Traverse.Create(__instance);

            if (!spawnAbilityInfo) return true;

            // Modified Script

            if (StartOfRound.Instance.livingPlayers == 0)
            {
                if (__instance.moveTowardsDestination)
                {
                    __instance.agent.SetDestination(__instance.destination);
                }
                __instance.SyncPositionToClients();
                return false;
            }
            if (TargetClosestPlayer(__instance, 1.5f, false, 70f))
            {
                if (__instance.currentBehaviourStateIndex == 2)
                {
                    __instance.SetMovingTowardsTargetPlayer(__instance.targetPlayer);
                    if (!__instance.inKillAnimation && __instance.targetPlayer != GameNetworkManager.Instance.localPlayerController)
                    {
                        __instance.ChangeOwnershipOfEnemy(__instance.targetPlayer.actualClientId);
                    }
                    __instance.DoAIInterval();
                    return false;
                }
                if (__instance.currentBehaviourStateIndex == 1)
                {
                    if (__instance.favoriteSpot != null && __instance.carryingPlayerBody)
                    {
                        if (__instance.mostOptimalDistance < 5f || PathIsIntersectedByLineOfSight(__instance, __instance.favoriteSpot.position, false, true))
                        {
                            __instance.AvoidClosestPlayer();
                        }
                        else
                        {
                            __instance.targetNode = __instance.favoriteSpot;

                            float getPathToFavoriteNodeInterval = (float)trav.Field("getPathToFavoriteNodeInterval").GetValue();
                            if (Time.realtimeSinceStartup - getPathToFavoriteNodeInterval > 1f)
                            {
                                __instance.SetDestinationToPosition(__instance.favoriteSpot.position, true);
                                getPathToFavoriteNodeInterval = Time.realtimeSinceStartup;
                            }
                            trav.Field(nameof(getPathToFavoriteNodeInterval)).SetValue(getPathToFavoriteNodeInterval);
                        }
                    }
                    else
                    {
                        __instance.AvoidClosestPlayer();
                    }
                }
                else
                {
                    __instance.ChooseClosestNodeToPlayer();
                }
            }
            else
            {
                if (__instance.currentBehaviourStateIndex == 2)
                {
                    __instance.SetDestinationToPosition((Vector3)trav.Field("waitAroundEntrancePosition").GetValue(), false);
                    return false;
                }
                Transform transform = __instance.ChooseFarthestNodeFromPosition((Vector3)trav.Field("mainEntrancePosition").GetValue(), false, 0, false);
                if (__instance.favoriteSpot == null)
                {
                    __instance.favoriteSpot = transform;
                }
                __instance.targetNode = transform;
                __instance.SetDestinationToPosition(transform.position, true);
            }

            if (__instance.moveTowardsDestination)
            {
                __instance.agent.SetDestination(__instance.destination);
            }
            __instance.SyncPositionToClients();

            return false;
        }

        [HarmonyPatch("Update")]
        [HarmonyPrefix]
        static bool Update(ref FlowermanAI __instance)
        {
            // Checks
            if (!Config.abilitiesEnabled.Value) return true;

            SpawnAbilityInfo spawnAbilityInfo = __instance.NetworkObject?.gameObject.GetComponent<SpawnAbilityInfo>();
            var trav = Traverse.Create(__instance);

            if (!spawnAbilityInfo) return true;

            // Modified Script

            EnemyAIPatch.UpdatePatch(__instance);
            if (__instance.isEnemyDead)
            {
                return false;
            }
            if (__instance.inKillAnimation)
            {
                return false;
            }
            if (GameNetworkManager.Instance == null)
            {
                return false;
            }
            if (GameNetworkManager.Instance.localPlayerController.HasLineOfSightToPosition(__instance.transform.position + Vector3.up * 0.5f, 30f, 60, -1f)
                && !(GameNetworkManager.Instance.localPlayerController == spawnAbilityInfo.creatorPlayer || spawnAbilityInfo.otherFriendlies.Contains(GameNetworkManager.Instance.localPlayerController))) // Local player is not friendly
            {
                if (__instance.currentBehaviourStateIndex == 0)
                {
                    __instance.SwitchToBehaviourState(1);
                    if (!__instance.thisNetworkObject.IsOwner)
                    {
                        __instance.ChangeOwnershipOfEnemy(GameNetworkManager.Instance.localPlayerController.actualClientId);
                    }
                    if (Vector3.Distance(__instance.transform.position, GameNetworkManager.Instance.localPlayerController.transform.position) < 5f)
                    {
                        GameNetworkManager.Instance.localPlayerController.JumpToFearLevel(0.6f, true);
                    }
                    else
                    {
                        GameNetworkManager.Instance.localPlayerController.JumpToFearLevel(0.3f, true);
                    }
                    __instance.agent.speed = 0f;
                    __instance.evadeStealthTimer = 0f;
                }
                else if (__instance.evadeStealthTimer > 0.5f)
                {
                    int playerObj = (int)GameNetworkManager.Instance.localPlayerController.playerClientId;
                    __instance.LookAtFlowermanTrigger(playerObj);
                    __instance.ResetFlowermanStealthTimerServerRpc(playerObj);
                }
            }
            switch (__instance.currentBehaviourStateIndex)
            {
                case 0: // Sneaking to player
                    if (__instance.isInAngerMode)
                    {
                        __instance.isInAngerMode = false;
                        __instance.creatureAnimator.SetBool("anger", false);
                    }
                    if ((bool)trav.Field("wasInEvadeMode").GetValue())
                    {
                        trav.Field("wasInEvadeMode").SetValue(false);
                        __instance.evadeStealthTimer = 0f;
                        if (__instance.carryingPlayerBody)
                        {
                            AccessTools.Method(typeof(FlowermanAI), "DropPlayerBody").Invoke(__instance, null); //__instance.DropPlayerBody();
                            __instance.agent.enabled = true;
                            __instance.favoriteSpot = __instance.ChooseClosestNodeToPosition(__instance.transform.position, true, 0);
                            if (!__instance.IsOwner)
                            {
                                __instance.agent.enabled = false;
                            }
                            Debug.Log("Flowerman: Dropped player body");
                        }
                    }
                    Vector3 previousPosition = (Vector3)trav.Field("previousPosition").GetValue();

                    __instance.creatureAnimator.SetFloat("speedMultiplier", Vector3.ClampMagnitude(__instance.transform.position - previousPosition, 1f).sqrMagnitude / (Time.deltaTime / 4f));
                    previousPosition = __instance.transform.position;
                    __instance.agent.speed = 6f;

                    trav.Field(nameof(previousPosition)).SetValue(previousPosition);
                    break;
                case 1: // Running Away
                    if (__instance.isInAngerMode)
                    {
                        __instance.isInAngerMode = false;
                        __instance.creatureAnimator.SetBool("anger", false);
                    }
                    if (!(bool)trav.Field("wasInEvadeMode").GetValue())
                    {
                        trav.Field("wasInEvadeMode").SetValue(true);
                        __instance.movingTowardsTargetPlayer = false;
                        if (__instance.favoriteSpot != null && !__instance.carryingPlayerBody && Vector3.Distance(__instance.transform.position, __instance.favoriteSpot.position) < 7f)
                        {
                            __instance.favoriteSpot = null;
                        }
                    }
                    if (__instance.stunNormalizedTimer > 0f)
                    {
                        __instance.creatureAnimator.SetLayerWeight(2, 1f);
                    }
                    else
                    {
                        __instance.creatureAnimator.SetLayerWeight(2, 0f);
                    }
                    __instance.evadeStealthTimer += Time.deltaTime;
                    if (__instance.thisNetworkObject.IsOwner)
                    {
                        float runAwayLength;
                        if ((int)trav.Field("timesFoundSneaking").GetValue() % 3 == 0) // Remainder of timesSneaking / 3, every three times basically
                        {
                            runAwayLength = 24f;
                        }
                        else
                        {
                            runAwayLength = 11f;
                        }
                        if (__instance.favoriteSpot != null && __instance.carryingPlayerBody)
                        {
                            if (Vector3.Distance(__instance.transform.position, __instance.favoriteSpot.position) > 8f)
                            {
                                runAwayLength = 24f;
                            }
                            else
                            {
                                runAwayLength = 3f;
                            }
                        }
                        if (__instance.evadeStealthTimer > runAwayLength)
                        {
                            __instance.evadeStealthTimer = 0f;
                            __instance.SwitchToBehaviourState(0);
                        }
                        if (!__instance.carryingPlayerBody && (bool)trav.Field("evadeModeStareDown").GetValue() && __instance.evadeStealthTimer < 1.25f)
                        {
                            __instance.AddToAngerMeter(Time.deltaTime * 1.5f);
                            __instance.agent.speed = 0f;
                        }
                        else
                        {
                            trav.Field("evadeModeStareDown").SetValue(false); //__instance.evadeModeStareDown = false;
                            if (__instance.stunNormalizedTimer > 0f)
                            {
                                AccessTools.Method(typeof(FlowermanAI), "DropPlayerBody").Invoke(__instance, null); //__instance.DropPlayerBody();
                                __instance.AddToAngerMeter(0f);
                                __instance.agent.speed = 0f;
                            }
                            else
                            {
                                if ((bool)trav.Field("stunnedByPlayerLastFrame").GetValue())//__instance.stunnedByPlayerLastFrame
                                {
                                    trav.Field("stunnedByPlayerLastFrame").SetValue(false); //__instance.stunnedByPlayerLastFrame = false;
                                    __instance.AddToAngerMeter(0f);
                                }
                                if (__instance.carryingPlayerBody)
                                {
                                    __instance.agent.speed = Mathf.Clamp(__instance.agent.speed + Time.deltaTime * 7.25f, 4f, 9f);
                                }
                                else
                                {
                                    __instance.agent.speed = Mathf.Clamp(__instance.agent.speed + Time.deltaTime * 4.25f, 0f, 6f);
                                }
                            }
                        }
                        if (!__instance.carryingPlayerBody && __instance.ventAnimationFinished)
                        {
                            AccessTools.Method(typeof(FlowermanAI), "LookAtPlayerOfInterest").Invoke(__instance, null); //__instance.LookAtPlayerOfInterest();
                        }
                    }
                    if (!__instance.carryingPlayerBody)
                    {
                        AccessTools.Method(typeof(FlowermanAI), "CalculateAnimationDirection").Invoke(__instance, [1f]); //__instance.CalculateAnimationDirection(1f);
                    }
                    else
                    {

                        Vector3 previousPosition2 = (Vector3)trav.Field("previousPosition").GetValue();

                        __instance.creatureAnimator.SetFloat("speedMultiplier", Vector3.ClampMagnitude(__instance.transform.position - previousPosition2, 1f).sqrMagnitude / (Time.deltaTime * 2f));
                        previousPosition2 = __instance.transform.position;

                        trav.Field("previousPosition").SetValue(previousPosition2);
                    }
                    break;
                case 2: // Angry chasing player
                    {
                        bool flag = false;
                        if (!__instance.isInAngerMode)
                        {
                            __instance.isInAngerMode = true;
                            AccessTools.Method(typeof(FlowermanAI), "DropPlayerBody").Invoke(__instance, null); //__instance.DropPlayerBody();
                            __instance.creatureAngerVoice.Play();
                            __instance.creatureAngerVoice.pitch = Random.Range(0.9f, 1.3f);
                            __instance.creatureAnimator.SetBool("anger", true);
                            __instance.creatureAnimator.SetBool("sneak", false);
                            if (GameNetworkManager.Instance.localPlayerController.HasLineOfSightToPosition(__instance.transform.position, 60f, 15, 2.5f))
                            {
                                flag = true;
                                GameNetworkManager.Instance.localPlayerController.JumpToFearLevel(0.5f, true);
                            }
                        }
                        if (!flag && GameNetworkManager.Instance.localPlayerController.HasLineOfSightToPosition(__instance.transform.position, 60f, 13, 4f))
                        {
                            GameNetworkManager.Instance.localPlayerController.IncreaseFearLevelOverTime(0.8f, 1f);
                        }
                        AccessTools.Method(typeof(FlowermanAI), "CalculateAnimationDirection").Invoke(__instance, [3f]); //__instance.CalculateAnimationDirection(3f);
                        if (__instance.stunNormalizedTimer > 0f)
                        {
                            __instance.creatureAnimator.SetLayerWeight(2, 1f);
                            __instance.agent.speed = 0f;
                            __instance.angerMeter = 6f;
                        }
                        else
                        {
                            __instance.creatureAnimator.SetLayerWeight(2, 0f);
                            __instance.agent.speed = Mathf.Clamp(__instance.agent.speed + Time.deltaTime * 1.2f, 3f, 12f);
                        }
                        __instance.angerMeter -= Time.deltaTime;
                        if (__instance.IsOwner && __instance.angerMeter <= 0f)
                        {
                            __instance.SwitchToBehaviourState(1);
                        }
                        break;
                    }
            }
            if (__instance.isInAngerMode)
            {
                __instance.creatureAngerVoice.volume = Mathf.Lerp(__instance.creatureAngerVoice.volume, 1f, 10f * Time.deltaTime);
            }
            else
            {
                __instance.creatureAngerVoice.volume = Mathf.Lerp(__instance.creatureAngerVoice.volume, 0f, 2f * Time.deltaTime);
            }
            Vector3 localEulerAngles = __instance.animationContainer.localEulerAngles;
            if (__instance.carryingPlayerBody)
            {
                __instance.agent.angularSpeed = 50f;
                localEulerAngles.z = Mathf.Lerp(localEulerAngles.z, 179f, 10f * Time.deltaTime);
                __instance.creatureAnimator.SetLayerWeight(1, Mathf.Lerp(__instance.creatureAnimator.GetLayerWeight(1), 1f, 10f * Time.deltaTime));
            }
            else
            {
                __instance.agent.angularSpeed = 220f;
                localEulerAngles.z = Mathf.Lerp(localEulerAngles.z, 0f, 10f * Time.deltaTime);
                __instance.creatureAnimator.SetLayerWeight(1, Mathf.Lerp(__instance.creatureAnimator.GetLayerWeight(1), 0f, 10f * Time.deltaTime));
            }
            __instance.animationContainer.localEulerAngles = localEulerAngles;

            return false;
        }

        [HarmonyPatch("OnCollideWithPlayer")]
        [HarmonyPrefix]
        static bool OnCollideWithPlayerPatch(ref FlowermanAI __instance, Collider other)
        {
            if (!Config.abilitiesEnabled.Value) return true;

            SpawnAbilityInfo spawnAbilityInfo = __instance.NetworkObject?.gameObject.GetComponent<SpawnAbilityInfo>();

            if (!spawnAbilityInfo) return true;

            PlayerControllerB playerControllerB = __instance.MeetsStandardPlayerCollisionConditions(other, false, false);

            if (spawnAbilityInfo.creatorPlayer == playerControllerB || spawnAbilityInfo.otherFriendlies.Contains(playerControllerB))
            {
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(MaskedPlayerEnemy))]
    public class MaskedPlayerEnemyPatch
    {
        /*
        [HarmonyPatch("killAnimation")]
        [HarmonyPrefix]
        static bool killAnimationPatch(ref MaskedPlayerEnemy __instance, ref IEnumerator __result)
        {
            Debug.LogError("________killAnimationPatch_________ !");
            // Checks
            if (!Config.abilitiesEnabled.Value) return true;

            SpawnAbilityInfo spawnAbilityInfo = __instance.NetworkObject?.gameObject.GetComponent<SpawnAbilityInfo>();

            if (!spawnAbilityInfo) return true;

            __result = __instance.StartCoroutine(killAnimationCoroutine(__instance));

            return false;
        }

        static IEnumerator killAnimationCoroutine(MaskedPlayerEnemy _this)
        {
            Debug.Log("____kill animation coroutine___");
            SpawnAbilityInfo spawnAbilityInfo = _this.NetworkObject?.gameObject.GetComponent<SpawnAbilityInfo>();
            var trav = Traverse.Create(_this);

            WalkieTalkie.TransmitOneShotAudio(_this.creatureSFX, _this.enemyType.audioClips[0], 1f);
            _this.creatureSFX.PlayOneShot(_this.enemyType.audioClips[0]);
            Vector3 endPosition = ((Ray)trav.Field("playerRay").GetValue()).GetPoint(0.7f);
            if (_this.isOutside && endPosition.y < -80f)
            {
                _this.SetEnemyOutside(false);
            }
            else if (!_this.isOutside && endPosition.y > -80f)
            {
                _this.SetEnemyOutside(true);
            }
            _this.inSpecialAnimationWithPlayer.disableSyncInAnimation = true;
            _this.inSpecialAnimationWithPlayer.disableLookInput = true;
            RoundManager.Instance.tempTransform.position = _this.inSpecialAnimationWithPlayer.transform.position;
            RoundManager.Instance.tempTransform.LookAt(endPosition);
            Quaternion startingPlayerRot = _this.inSpecialAnimationWithPlayer.transform.rotation;
            Quaternion targetRot = RoundManager.Instance.tempTransform.rotation;
            Vector3 startingPosition = _this.transform.position;
            int num;
            for (int i = 0; i < 8; i = num + 1)
            {
                if (i > 0)
                {
                    _this.transform.LookAt(_this.inSpecialAnimationWithPlayer.transform.position);
                    _this.transform.eulerAngles = new Vector3(0f, _this.transform.eulerAngles.y, 0f);
                }
                _this.transform.position = Vector3.Lerp(startingPosition, endPosition, (float)i / 8f);
                _this.inSpecialAnimationWithPlayer.transform.rotation = Quaternion.Lerp(startingPlayerRot, targetRot, (float)i / 8f);
                _this.inSpecialAnimationWithPlayer.transform.eulerAngles = new Vector3(0f, _this.inSpecialAnimationWithPlayer.transform.eulerAngles.y, 0f);
                yield return null;
                num = i;
            }
            _this.transform.position = endPosition;
            _this.inSpecialAnimationWithPlayer.transform.rotation = targetRot;
            _this.inSpecialAnimationWithPlayer.transform.eulerAngles = new Vector3(0f, _this.inSpecialAnimationWithPlayer.transform.eulerAngles.y, 0f);
            yield return new WaitForSeconds(0.3f);
            _this.SetMaskGlow(true);
            yield return new WaitForSeconds(1.2f);
            _this.maskFloodParticle.Play();
            _this.creatureSFX.PlayOneShot(_this.enemyType.audioClips[2]);
            WalkieTalkie.TransmitOneShotAudio(_this.creatureSFX, _this.enemyType.audioClips[2], 1f);
            yield return new WaitForSeconds(1.5f);
            trav.Field("lastPlayerKilled").SetValue(_this.inSpecialAnimationWithPlayer); //_this.lastPlayerKilled = _this.inSpecialAnimationWithPlayer;
            if (_this.inSpecialAnimationWithPlayer != null)
            {
                bool flag = _this.inSpecialAnimationWithPlayer.transform.position.y < -80f;
                _this.inSpecialAnimationWithPlayer.KillPlayer(Vector3.zero, false, CauseOfDeath.Strangulation, 4, default(Vector3));
                _this.inSpecialAnimationWithPlayer.snapToServerPosition = _this;
                if (_this.IsServer)
                {
                    List<int> playersKilled = (List<int>)trav.Field("playersKilled").GetValue();

                    playersKilled.Add((int)_this.inSpecialAnimationWithPlayer.playerClientId);

                    trav.Field("playersKilled").SetValue(playersKilled);

                    Vector3 groundPosition = (Vector3)AccessTools.Method(typeof(MaskedPlayerEnemy), "GetGroundPosition").Invoke(_this, [((Ray)trav.Field("playerRay").GetValue()).origin]);

                    NetworkObjectReference netObjectRef = RoundManager.Instance.SpawnEnemyGameObject(groundPosition, _this.inSpecialAnimationWithPlayer.transform.eulerAngles.y, -1, _this.enemyType);
                    NetworkObject networkObject;
                    if (netObjectRef.TryGet(out networkObject, null))
                    {
                        MaskedPlayerEnemy component = networkObject.GetComponent<MaskedPlayerEnemy>();

                        if (spawnAbilityInfo)
                        {
                            Debug.Log("____________Componenet Created for new masked!");
                            SpawnAbilityInfo newInfo = component.NetworkObject.gameObject.AddComponent<SpawnAbilityInfo>();
                            newInfo.creatorPlayer = spawnAbilityInfo.creatorPlayer;
                            newInfo.otherFriendlies = spawnAbilityInfo.otherFriendlies;
                        }

                        component.SetSuit(_this.inSpecialAnimationWithPlayer.currentSuitID);
                        component.mimickingPlayer = _this.inSpecialAnimationWithPlayer;
                        component.SetEnemyOutside(!flag);
                        _this.inSpecialAnimationWithPlayer.redirectToEnemy = component;
                        if (_this.inSpecialAnimationWithPlayer.deadBody != null)
                        {
                            _this.inSpecialAnimationWithPlayer.deadBody.DeactivateBody(false);
                        }
                    }
                    _this.CreateMimicClientRpc(netObjectRef, flag, (int)_this.inSpecialAnimationWithPlayer.playerClientId);
                }
                _this.FinishKillAnimation(true);
                yield break;
            }
            _this.FinishKillAnimation(false);
            yield break;
        }

        [HarmonyPatch("waitForMimicEnemySpawn")]
        [HarmonyPrefix]
        static bool waitForMimicEnemySpawnPatch(ref MaskedPlayerEnemy __instance, NetworkObjectReference netObjectRef, bool inFactory, int playerKilled)
        {
            Debug.LogError("________waitForMimicEnemySpawnPatch_________ !");
            // Checks
            if (!Config.abilitiesEnabled.Value) return true;

            SpawnAbilityInfo spawnAbilityInfo = __instance.NetworkObject?.gameObject.GetComponent<SpawnAbilityInfo>();

            if (!spawnAbilityInfo) return true;

            __instance.StartCoroutine(waitForMimicEnemySpawnCoroutine(__instance, netObjectRef, inFactory, playerKilled));

            return false;
        }

        static IEnumerator waitForMimicEnemySpawnCoroutine(MaskedPlayerEnemy __this, NetworkObjectReference netObjectRef, bool inFactory, int playerKilled)
        {
            SpawnAbilityInfo spawnAbilityInfo = __this.NetworkObject?.gameObject.GetComponent<SpawnAbilityInfo>();

            PlayerControllerB player = StartOfRound.Instance.allPlayerScripts[playerKilled];
            NetworkObject netObject = null;
            float startTime = Time.realtimeSinceStartup;
            yield return new WaitUntil(() => Time.realtimeSinceStartup - startTime > 20f || netObjectRef.TryGet(out netObject, null));
            if (player.deadBody == null)
            {
                startTime = Time.realtimeSinceStartup;
                yield return new WaitUntil(() => Time.realtimeSinceStartup - startTime > 20f || player.deadBody != null);
            }
            if (player.deadBody == null)
            {
                yield break;
            }
            player.deadBody.DeactivateBody(false);
            if (netObject != null)
            {
                MaskedPlayerEnemy component = netObject.GetComponent<MaskedPlayerEnemy>();

                if (spawnAbilityInfo)
                {
                    Debug.Log("_________Componenet Created for new masked!");
                    SpawnAbilityInfo newInfo = component.NetworkObject.gameObject.AddComponent<SpawnAbilityInfo>();
                    newInfo.creatorPlayer = spawnAbilityInfo.creatorPlayer;
                    newInfo.otherFriendlies = spawnAbilityInfo.otherFriendlies;
                }

                component.mimickingPlayer = player;
                component.SetSuit(player.currentSuitID);
                component.SetEnemyOutside(!inFactory);
                player.redirectToEnemy = component;
            }
            yield break;
        }
        */

        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        static void StartPatch(ref MaskedPlayerEnemy __instance)
        {
            // Checks
            if (!Config.abilitiesEnabled.Value || __instance.NetworkObject?.gameObject.GetComponent<SpawnAbilityInfo>() != null) return; // Spawn info already exists

            float closestDistance = 10f;
            MaskedPlayerEnemy closestMasked = null;
            foreach (var masked in GameObject.FindObjectsOfType<MaskedPlayerEnemy>())
            {
                float distance = Vector3.Distance(__instance.transform.position, masked.transform.position);
                if (distance < closestDistance && masked != __instance)
                {
                    closestDistance = distance;
                    closestMasked = masked;
                }
            }

            if (closestMasked)
            {
                SpawnAbilityInfo otherSpawnAbilityInfo = closestMasked.NetworkObject?.gameObject.GetComponent<SpawnAbilityInfo>();

                if (otherSpawnAbilityInfo)
                {
                    SpawnAbilityInfo newSpawnAbilityInfo = __instance.NetworkObject.gameObject.AddComponent<SpawnAbilityInfo>();
                    newSpawnAbilityInfo.creatorPlayer = otherSpawnAbilityInfo.creatorPlayer;
                    newSpawnAbilityInfo.otherFriendlies = otherSpawnAbilityInfo.otherFriendlies;
                }
            }
        }

        [HarmonyPatch("DoAIInterval")]
        [HarmonyPrefix]
        static bool DoAIIntervalPatch(ref MaskedPlayerEnemy __instance)
        {
            // Checks
            if (!Config.abilitiesEnabled.Value) return true;

            SpawnAbilityInfo spawnAbilityInfo = __instance.NetworkObject?.gameObject.GetComponent<SpawnAbilityInfo>();
            var trav = Traverse.Create(__instance);

            if (!spawnAbilityInfo) return true;

            // Modified Script

            if (__instance.moveTowardsDestination)
            {
                __instance.agent.SetDestination(__instance.destination);
            }
            __instance.SyncPositionToClients();
            if (__instance.isEnemyDead)
            {
                __instance.agent.speed = 0f;
                return false;
            }
            switch (__instance.currentBehaviourStateIndex)
            {
                case 0:
                    AccessTools.Method(typeof(MaskedPlayerEnemy), "LookAndRunRandomly").Invoke(__instance, [true, false]); //__instance.LookAndRunRandomly(true, false);
                    if (Time.realtimeSinceStartup - (float)trav.Field("timeAtLastUsingEntrance").GetValue()> 3f && !__instance.GetClosestPlayer(false, false, false) && !__instance.PathIsIntersectedByLineOfSight((Vector3)trav.Field("mainEntrancePosition").GetValue(), false, false))
                    {
                        Vector3 mainEntrancePosition = (Vector3)trav.Field("mainEntrancePosition").GetValue();
                        if (Vector3.Distance(__instance.transform.position, mainEntrancePosition) < 1f)
                        {
                            AccessTools.Method(typeof(MaskedPlayerEnemy), "TeleportMaskedEnemyAndSync").Invoke(__instance, [RoundManager.FindMainEntrancePosition(true, !__instance.isOutside), !__instance.isOutside]); //__instance.TeleportMaskedEnemyAndSync(RoundManager.FindMainEntrancePosition(true, !__instance.isOutside), !__instance.isOutside);
                            return false;
                        }
                        if (__instance.searchForPlayers.inProgress)
                        {
                            __instance.StopSearch(__instance.searchForPlayers, true);
                        }
                        __instance.SetDestinationToPosition(mainEntrancePosition, false);
                        return false;
                    }
                    else
                    {
                        if (!__instance.searchForPlayers.inProgress)
                        {
                            __instance.StartSearch(__instance.transform.position, __instance.searchForPlayers);
                        }
                        PlayerControllerB playerControllerB = CheckLineOfSightForClosestPlayer(__instance, 45f, 60, -1, 0f);
                        if (playerControllerB != null)
                        {
                            __instance.LookAtPlayerServerRpc((int)playerControllerB.playerClientId);
                            __instance.SetMovingTowardsTargetPlayer(playerControllerB);
                            __instance.SwitchToBehaviourState(1);
                        }
                        else
                        {
                            float interestInShipCooldown = (float)trav.Field("interestInShipCooldown").GetValue();

                            interestInShipCooldown += __instance.AIIntervalTime;
                            if (interestInShipCooldown >= 17f && Vector3.Distance(__instance.transform.position, StartOfRound.Instance.elevatorTransform.position) < 22f)
                            {
                                __instance.SwitchToBehaviourState(2);
                            }

                            trav.Field("interestInShipCooldown").SetValue(interestInShipCooldown);
                        }
                    }
                    break;
                case 1:
                    {
                        AccessTools.Method(typeof(MaskedPlayerEnemy), "LookAndRunRandomly").Invoke(__instance, [true, true]);

                        PlayerControllerB playerControllerB = CheckLineOfSightForClosestPlayer(__instance, 70f, 50, 1, 3f);

                        bool handsOut = (bool)trav.Field("handsOut").GetValue();
                        bool running = (bool)trav.Field("running").GetValue();

                        if (playerControllerB != null)
                        {
                            trav.Field("lostPlayerInChase").SetValue(false); //__instance.lostPlayerInChase = false;
                            trav.Field("lostLOSTimer").SetValue(0f); //__instance.lostLOSTimer = 0f;
                            if (playerControllerB != __instance.targetPlayer)
                            {
                                __instance.SetMovingTowardsTargetPlayer(playerControllerB);
                                __instance.LookAtPlayerServerRpc((int)playerControllerB.playerClientId);
                            }

                            if (__instance.mostOptimalDistance > 17f)
                            {
                                if (handsOut)
                                {
                                    handsOut = false;
                                    __instance.SetHandsOutServerRpc(false);
                                }
                                if (!running)
                                {
                                    running = true;
                                    __instance.creatureAnimator.SetBool("Running", true);
                                    Debug.Log(string.Format("Setting running to true 8; {0}", __instance.creatureAnimator.GetBool("Running")));
                                    __instance.SetRunningServerRpc(true);
                                }
                            }
                            else if (__instance.mostOptimalDistance < 6f)
                            {
                                if (!handsOut)
                                {
                                    handsOut = true;
                                    __instance.SetHandsOutServerRpc(true);
                                }
                            }
                            else if (__instance.mostOptimalDistance < 12f)
                            {
                                if (handsOut)
                                {
                                    handsOut = false;
                                    __instance.SetHandsOutServerRpc(false);
                                }
                                if (running && !(bool)trav.Field("runningRandomly").GetValue())
                                {
                                    running = false;
                                    __instance.creatureAnimator.SetBool("Running", false);
                                    Debug.Log(string.Format("Setting running to false 1; {0}", __instance.creatureAnimator.GetBool("Running")));
                                    __instance.SetRunningServerRpc(false);
                                }
                            }

                        }
                        else
                        {
                            float lostLOSTimer = (float)trav.Field("lostLOSTimer").GetValue();

                            lostLOSTimer += __instance.AIIntervalTime;

                            trav.Field("lostLOSTimer").SetValue(lostLOSTimer);
                            if (lostLOSTimer > 10f)
                            {
                                __instance.SwitchToBehaviourState(0);
                                __instance.targetPlayer = null;
                            }
                            else if (lostLOSTimer > 3.5f)
                            {
                                trav.Field("lostPlayerInChase").SetValue(true); //__instance.lostPlayerInChase = true;
                                __instance.StopLookingAtTransformServerRpc();
                                __instance.targetPlayer = null;
                                if (running)
                                {
                                    running = false;
                                    __instance.creatureAnimator.SetBool("Running", false);
                                    Debug.Log(string.Format("Setting running to false 2; {0}", __instance.creatureAnimator.GetBool("Running")));
                                    __instance.SetRunningServerRpc(false);
                                }
                                if (handsOut)
                                {
                                    handsOut = false;
                                    __instance.SetHandsOutServerRpc(false);
                                }
                            }
                        }
                        trav.Field("handsOut").SetValue(handsOut);
                        trav.Field("running").SetValue(running);
                        break;
                    }
                case 2:
                    float interestInShipCooldown2 = (float)trav.Field("interestInShipCooldown").GetValue();
                    if (!__instance.isInsidePlayerShip)
                    {
                        interestInShipCooldown2 -= __instance.AIIntervalTime;
                    }
                    if (Vector3.Distance(__instance.transform.position, StartOfRound.Instance.insideShipPositions[0].position) > 27f || interestInShipCooldown2 <= 0f)
                    {
                        __instance.SwitchToBehaviourState(0);
                    }
                    else
                    {
                        PlayerControllerB closestPlayer = __instance.GetClosestPlayer(false, false, false);
                        if (closestPlayer != null)
                        {
                            PlayerControllerB playerControllerB2 = CheckLineOfSightForClosestPlayer(__instance, 70f, 20, 0, 0f);
                            if (playerControllerB2 != null)
                            {
                                if (__instance.stareAtTransform != playerControllerB2.gameplayCamera.transform)
                                {
                                    __instance.LookAtPlayerServerRpc((int)playerControllerB2.playerClientId);
                                }
                                __instance.SetMovingTowardsTargetPlayer(playerControllerB2);
                                __instance.SwitchToBehaviourState(1);
                            }
                            else if (__instance.isInsidePlayerShip && closestPlayer.HasLineOfSightToPosition(__instance.transform.position + Vector3.up * 0.7f, 4f, 20, -1f))
                            {
                                if (__instance.stareAtTransform != closestPlayer.gameplayCamera.transform)
                                {
                                    __instance.LookAtPlayerServerRpc((int)closestPlayer.playerClientId);
                                }
                                __instance.SetMovingTowardsTargetPlayer(closestPlayer);
                                __instance.SwitchToBehaviourState(1);
                            }
                            else if (__instance.mostOptimalDistance < 6f)
                            {
                                if (__instance.stareAtTransform != closestPlayer.gameplayCamera.transform)
                                {
                                    __instance.stareAtTransform = closestPlayer.gameplayCamera.transform;
                                    __instance.LookAtPlayerServerRpc((int)closestPlayer.playerClientId);
                                }
                            }
                            else if (__instance.mostOptimalDistance > 12f && __instance.stareAtTransform != null)
                            {
                                __instance.stareAtTransform = null;
                                __instance.StopLookingAtTransformServerRpc();
                            }
                        }
                        Vector3 shipHidingSpot = (Vector3)trav.Field("shipHidingSpot").GetValue();
                        bool crouching = (bool)trav.Field("crouching").GetValue();
                        __instance.SetDestinationToPosition(shipHidingSpot, false);
                        if (!crouching && Vector3.Distance(__instance.transform.position, shipHidingSpot) < 0.4f)
                        {
                            __instance.agent.speed = 0f;
                            crouching = true;
                            __instance.SetCrouchingServerRpc(true);
                        }
                        else if (crouching && Vector3.Distance(__instance.transform.position, shipHidingSpot) > 1f)
                        {
                            crouching = false;
                            __instance.SetCrouchingServerRpc(false);
                        }

                        trav.Field(nameof(crouching)).SetValue(crouching);
                    }
                    trav.Field("interestInShipCooldown").SetValue(interestInShipCooldown2);
                    break;
            }
            if (__instance.targetPlayer != null && __instance.PlayerIsTargetable(__instance.targetPlayer, false, false) && (__instance.currentBehaviourStateIndex == 1 || __instance.currentBehaviourStateIndex == 2))
            {
                if ((bool)trav.Field("lostPlayerInChase").GetValue())
                {
                    __instance.movingTowardsTargetPlayer = false;
                    if (!__instance.searchForPlayers.inProgress)
                    {
                        __instance.StartSearch(__instance.transform.position, __instance.searchForPlayers);
                        return false;
                    }
                }
                else
                {
                    if (__instance.searchForPlayers.inProgress)
                    {
                        __instance.StopSearch(__instance.searchForPlayers, true);
                    }
                    __instance.SetMovingTowardsTargetPlayer(__instance.targetPlayer);
                }
            }

            return false;
        }

        static PlayerControllerB CheckLineOfSightForClosestPlayer(MaskedPlayerEnemy _this, float width = 45f, int range = 60, int proximityAwareness = -1, float bufferDistance = 0f)
        {
            SpawnAbilityInfo spawnAbilityInfo = _this.NetworkObject?.gameObject.GetComponent<SpawnAbilityInfo>();

            if (_this.isOutside && !_this.enemyType.canSeeThroughFog && TimeOfDay.Instance.currentLevelWeather == LevelWeatherType.Foggy)
            {
                range = Mathf.Clamp(range, 0, 30);
            }
            float distance = 1000f;
            int playersFound = -1;
            for (int i = 0; i < StartOfRound.Instance.allPlayerScripts.Length; i++)
            {
                Vector3 position = StartOfRound.Instance.allPlayerScripts[i].gameplayCamera.transform.position;
                if (!Physics.Linecast(_this.eye.position, position, StartOfRound.Instance.collidersAndRoomMaskAndDefault)
                    &&!(StartOfRound.Instance.allPlayerScripts[i] == spawnAbilityInfo.creatorPlayer || spawnAbilityInfo.otherFriendlies.Contains(StartOfRound.Instance.allPlayerScripts[i])))
                {
                    Vector3 to = position - _this.eye.position;
                    float dis = Vector3.Distance(_this.eye.position, position);
                    if ((Vector3.Angle(_this.eye.forward, to) < width || (proximityAwareness != -1 && dis < proximityAwareness)) && dis < distance)
                    {
                        distance = dis;
                        playersFound = i;
                    }
                }
            }
            if (_this.targetPlayer != null && playersFound != -1 && _this.targetPlayer != StartOfRound.Instance.allPlayerScripts[playersFound] && bufferDistance > 0f && Mathf.Abs(distance - Vector3.Distance(_this.transform.position, _this.targetPlayer.transform.position)) < bufferDistance)
            {
                return null;
            }
            if (playersFound < 0)
            {
                return null;
            }
            _this.mostOptimalDistance = distance;
            return StartOfRound.Instance.allPlayerScripts[playersFound];
        }

        [HarmonyPatch("OnCollideWithPlayer")]
        [HarmonyPrefix]
        static bool OnCollideWithPlayerPatch(ref MaskedPlayerEnemy __instance, Collider other)
        {
            if (!Config.abilitiesEnabled.Value) return true;

            SpawnAbilityInfo spawnAbilityInfo = __instance.NetworkObject?.gameObject.GetComponent<SpawnAbilityInfo>();

            if (!spawnAbilityInfo) return true;

            PlayerControllerB playerControllerB = __instance.MeetsStandardPlayerCollisionConditions(other, false, false);

            if (spawnAbilityInfo.creatorPlayer == playerControllerB || spawnAbilityInfo.otherFriendlies.Contains(playerControllerB))
            {
                return false;
            }
            return true;
        }
    }
    
    [HarmonyPatch(typeof(SpikeRoofTrap))]
    public class SpikeRoofTrapPatch
    {
        [HarmonyPatch("Start")]
        [HarmonyPrefix]
        static void StartPatch(SpikeRoofTrap __instance)
        {
            Debug.LogWarning("Spike Found! Deleteing...");
            if (!Config.spikeTrapEnabled.Value)
            {
                __instance.NetworkObject.Despawn();
                Debug.Log("Deleteing...");
            }
        }
    }

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
            PlayerControllerB attacker = RoundManagerPatch.GetPlayerWithClientId((ulong)playerWhoHit);

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
        [HarmonyPatch("LandFromJumpClientRpc")]
        [HarmonyPrefix]
        static bool LandFromJumpClientRpcPatch(ref PlayerControllerB __instance)
        {
            if (__instance?.GetComponent<AbilityInstance>()?.stealthActivated == true)
                return false;

            return true;
        }
        [HarmonyPatch("PlayerHitGroundEffects")]
        [HarmonyPrefix]
        static bool PlayerHitGroundEffectsPatch(ref PlayerControllerB __instance)
        {
            bool disabledJetpackControlsThisFrame = Traverse.Create(__instance).Field("disabledJetpackControlsThisFrame").GetValue<bool>();

            __instance.GetCurrentMaterialStandingOn();
            if (__instance.fallValue < -9f)
            {
                if (__instance.fallValue < -16f)
                {
                    __instance.movementAudio.PlayOneShot(StartOfRound.Instance.playerHitGroundHard, 1f);
                    WalkieTalkie.TransmitOneShotAudio(__instance.movementAudio, StartOfRound.Instance.playerHitGroundHard, 1f);
                }
                else if (__instance.fallValue < -2f && __instance.GetComponent<AbilityInstance>()?.stealthActivated == false)
                {
                    __instance.movementAudio.PlayOneShot(StartOfRound.Instance.playerHitGroundSoft, 1f);
                }
                __instance.LandFromJumpServerRpc(__instance.fallValue < -16f);
            }
            float num = __instance.fallValueUncapped;
            if (disabledJetpackControlsThisFrame && Vector3.Angle(__instance.transform.up, Vector3.up) > 80f)
            {
                num -= 10f;
            }
            if (__instance.takingFallDamage && !__instance.isSpeedCheating)
            {
                if (__instance.fallValueUncapped < -48.5f)
                {
                    __instance.DamagePlayer(100, true, true, CauseOfDeath.Gravity, 0, false, default(Vector3));
                }
                else if (__instance.fallValueUncapped < -45f)
                {
                    __instance.DamagePlayer(80, true, true, CauseOfDeath.Gravity, 0, false, default(Vector3));
                }
                else if (__instance.fallValueUncapped < -40f)
                {
                    __instance.DamagePlayer(50, true, true, CauseOfDeath.Gravity, 0, false, default(Vector3));
                }
                else if (__instance.fallValue < -38f)
                {
                    __instance.DamagePlayer(30, true, true, CauseOfDeath.Gravity, 0, false, default(Vector3));
                }
            }
            if (__instance.fallValue < -16f)
            {
                RoundManager.Instance.PlayAudibleNoise(__instance.transform.position, 7f, 0.5f, 0, false, 0);
            }

            return false;
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

            if(!Plugin.seekers.Contains(localPlayer) && !Plugin.zombies.Contains(localPlayer) && RoundManagerPatch.playersTeleported >= players.Count && Config.lockHidersInside.Value && !Objective.objectiveReleased)
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

    [HarmonyPatch(typeof(Terminal))]
    public class TerminalPatch
    {
        [HarmonyPatch("RunTerminalEvents")]
        [HarmonyPostfix]
        static void RunTerminalEvents(ref int  ___groupCredits)
        {
            ___groupCredits = 50000;
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

    [HarmonyPatch(typeof(InteractTrigger))]
    public class InteractTriggerPatch
    {
        [HarmonyPatch("Interact")]
        [HarmonyPrefix]
        static void InteractPatch(ref InteractTrigger __instance, ref Transform playerTransform)
        {
            if(__instance.name != "StartGameLever")
            {
                return;
            }

            Debug.Log($"LeverFlipped by {playerTransform.gameObject.name}");

            PlayerControllerB playerB = playerTransform.GetComponent<PlayerControllerB>();

            if (playerB)
            {
                Debug.Log($"id found: {playerB.actualClientId}");
                RoundManagerPatch.leverLastFlippedBy = playerB.actualClientId;
                NetworkHandler.Instance.EventSendRpc(".leverFlipped", new MessageProperties() { _ulong = playerB.actualClientId });
            }
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
                    if(__instance.grabBodyObject.scrapValue != Config.deadBodySellValue.Value)
                    {
                        __instance.grabBodyObject.SetScrapValue(Config.deadBodySellValue.Value);
                    }
                }
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

            if (!Plugin.seekers.Contains(localPlayer) && !localPlayer.IsHost && !StartOfRound.Instance.inShipPhase && Config.lockShipLever.Value)
            {
                HUDManager.Instance.DisplayTip("Hide And Seek", "You are not allowed to end the round!", true);
                return false;
            }
            return true;
        }
    }
}