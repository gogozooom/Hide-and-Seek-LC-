using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;
using System.Collections;
using Debug = Debugger.Debug;
using HideAndSeek.AbilityScripts;
using Unity.Netcode;

namespace HideAndSeek.Patches
{
    public class MessageProperties
    {
        public bool _null = true;
        public bool _bool = false;
        public string _string = "";
        public int _int = 0;
        public float _float = 0f;
        public ulong _ulong = 0;
        public Vector3 _Vector3 = new();
        public string _extraMessage;

        public MessageProperties(bool __bool = false, string __string = "", int __int = 0, float __float = 0f, ulong __ulong = 0, Vector3 __Vector3 = new(), bool __null = false, string __extraMessage = null) 
        {
            _bool = __bool;
            _string = __string;
            _int = __int;
            _float = __float;
            _ulong = __ulong;
            _Vector3 = __Vector3;
            _null = __null;
            _extraMessage = __extraMessage;
        }
    }
    class SyncingPatch
    {
        public static void LevelLoading(string eventName, MessageProperties mProps = null)
        {
            if (eventName != ".levelLoading") return;

            Debug.LogMessage("Got LevelLoading Broadcast!");
            RoundManager.Instance.LoadNewLevel(StartOfRound.Instance.randomMapSeed, StartOfRound.Instance.currentLevel);
        }
        public static void PlayerChosen(string eventName, MessageProperties mProps)
        {
            if (eventName != ".playerChosen") return;

            Debug.LogMessage("Got PlayerChosen Broadcast!");
            foreach (var player in GameObject.FindObjectsOfType<PlayerControllerB>())
            {
                if (player.NetworkObjectId == mProps._ulong)
                {
                    Plugin.seekers.Add(player);
                    if (GameNetworkManager.Instance.localPlayerController.actualClientId == player.actualClientId)
                    {
                        HUDManager.Instance.DisplayTip("Hide And Seek", $"You are the seeker!", true);
                    }
                    break;
                }
            }
        }
        public static void SeekersChosen(string eventName, MessageProperties mProps)
        {
            if (eventName != ".seekersChosen") return;

            Debug.LogError("___________________ Got Seekers Chosen Broadcast!");
            PlayerControllerB localPlayer = GameNetworkManager.Instance.localPlayerController;

            if (!Plugin.seekers.Contains(localPlayer))
            {
                HUDManager.Instance.DisplayTip("Hide And Seek", $"Seeker(s) chosen this round [{mProps._string}]");
            }
            foreach (var player in GameObject.FindObjectsByType<PlayerControllerB>(0))
            {
                if (Plugin.seekers.Contains(player))
                {
                    // Seeker
                    player.usernameBillboardText.color = Config.seekerNameColor.Value;
                }
                else
                {
                    // Hider
                    player.usernameBillboardText.color = Config.hiderNameColor.Value;
                }
            }
        }
        public static void LockDoor(string eventName, MessageProperties mProps = null)
        {
            if (eventName != ".lockDoor") return;

            Debug.LogMessage("Got LockDoor Broadcast!");
            HangarShipDoor door = GameObject.FindObjectOfType<HangarShipDoor>();

            door.SetDoorClosed();
            door.PlayDoorAnimation(true);
            door.buttonsEnabled = false;
        }
        public static void OpenDoor(string eventName, MessageProperties mProps = null)
        {
            if (eventName != ".openDoor") return;

            Debug.LogMessage("Got OpenDoor Broadcast!");
            HangarShipDoor door = GameObject.FindObjectOfType<HangarShipDoor>();

            door.buttonsEnabled = true;
            door.SetDoorOpen();
            door.PlayDoorAnimation(false);
        }
        public static void PlayerTeleported(string eventName, MessageProperties mProps)
        {
            if (eventName != ".teleported") return;

            Debug.LogMessage("Got Player Teleported Brodcast!");

            if (mProps._bool) { RoundManagerPatch.playersTeleported = 0; Debug.LogMessage("New Round, Reset Players Teleported!"); return; } // mProps._bool = resetTeleportedPlayers

            if (!mProps._null)
            {
                RoundManagerPatch.itemSpawnPositions.Add((mProps._ulong, mProps._Vector3));
                RoundManagerPatch.playersTeleported += 1;
                Debug.LogMessage($"Added to teleported players! Number is now '{RoundManagerPatch.playersTeleported}' ");
            }
            else
            {
                Debug.LogError("[Player Teleported] mProps == null! Called dry!");
            }
        }
        public static void DisplayTip(string eventName, MessageProperties mProps)
        {
            if (eventName != ".tip") return;

            Debug.LogMessage("Got Display Tip Brodcast!");

            if (mProps._int != -1 || mProps._ulong == GameNetworkManager.Instance.localPlayerController.actualClientId)
            {
                HUDManager.Instance.DisplayTip("Hide And Seek", mProps._string, mProps._bool); // _string = messageContent, _bool = isWarning, _int = -1 (Spesific Message)
            }
        }
        public static void LevelLoaded(string eventName, MessageProperties mProps)
        {
            if (eventName != ".levelLoaded") return;

            Debug.LogMessage("Get Level Done Loading Brodcast!");

            RoundManagerPatch.levelLoading = false;
        }
        public static void LeverFlipped(string eventName, MessageProperties mProps)
        {
            if (eventName != ".leverFlipped") return;

            Debug.LogMessage($"Got Lever Flipped Brodcast! New player id ({mProps._ulong})");

            RoundManagerPatch.leverLastFlippedBy = mProps._ulong;
        }
        public static void SellCurrentItem(string eventName, MessageProperties mProps)
        {
            if (eventName != ".sellCurrentItem" || !GameNetworkManager.Instance.isHostingGame) return;

            Debug.LogMessage($"Got Sell Current Item Brodcast! From player id '{mProps._ulong}'");
            
            AbilityManager.SellCurrentItem(mProps._ulong);
        }
        public static void MoneyChanged(string eventName, MessageProperties mProps)
        {
            if (eventName != ".moneyChanged") return;

            Debug.LogMessage($"Got Money Change Brodcast! For player id: '{mProps._ulong}' is silent: '{mProps._string == "silent"}'");

            PlayerControllerB selectedPlayer = RoundManagerPatch.GetPlayerWithClientId(mProps._ulong);

            if (mProps._null) // Everyone
            {
                foreach (var player in GameObject.FindObjectsOfType<PlayerControllerB>())
                {
                    if (player.isPlayerControlled)
                    {
                        player.GetComponent<AbilityInstance>()?.ServerMoneyUpdated(mProps._int, mProps._bool, mProps._string == "silent");
                    }
                }
                return;
            }

            if (selectedPlayer)
            {
                AbilityInstance playerAbilities = selectedPlayer.GetComponent<AbilityInstance>();

                if (playerAbilities == null)
                {
                    playerAbilities = selectedPlayer.gameObject.AddComponent<AbilityInstance>();
                }

                playerAbilities.ServerMoneyUpdated(mProps._int, mProps._bool, mProps._string == "silent");
            }
        }
        public static void DestroyItem(string eventName, MessageProperties mProps)
        {
            if (eventName != ".destroyItem" || mProps._ulong != GameNetworkManager.Instance.localPlayerController.actualClientId) return;

            Debug.LogMessage($"Got Delete Item Brodcast! For player id '{mProps._ulong}'");

            PlayerControllerB localPlayer = GameNetworkManager.Instance.localPlayerController;

            localPlayer.DestroyItemInSlotAndSync(mProps._int);
        }
        public static void BuyAbility(string eventName, MessageProperties mProps)
        {
            if (eventName != ".buyAbility" || !GameNetworkManager.Instance.isHostingGame) return;

            Debug.LogMessage($"Got Buy Ability Brodcast! Ability name '{mProps._string}' From player id '{mProps._ulong}'");
            
            AbilityManager.BuyAbilityServer(mProps._ulong, Abilities.FindAbilityByName(mProps._string));
        }
        public static void ActivateAbility(string eventName, MessageProperties mProps)
        {
            if (eventName != ".activateAbility") return;

            Debug.LogMessage($"Got Activate Abilty Brodcast! Ability name '{mProps._string}' From player id '{mProps._ulong}' Extra info '{mProps._extraMessage}'");

            AbilityBase Ability = Abilities.FindAbilityByName(mProps._string);

            if (Ability != null)
            {
                Ability.ActivateAbility(mProps._ulong, mProps._extraMessage);
            }
            else
            {
                Debug.LogWarning($"[ActivateAbility] Could not find ability by the name of '{mProps._string}'!");
            }
        }
        public static void SetDayTime(string eventName, MessageProperties mProps)
        {
            if (eventName != ".setDayTime") return;

            Debug.LogMessage($"Got SetDayTime Brodcast! time to set = '{mProps._float}' current time = '{TimeOfDay.Instance.currentDayTime}' global time = '{TimeOfDay.Instance.globalTime}' ");

            TimeOfDay.Instance.globalTime = mProps._float;
            TimeOfDay.Instance.currentDayTime = mProps._float;
        }
        public static void GrabItem(string eventName, MessageProperties mProps)
        {
            if (eventName != ".grabItem" || mProps._ulong != GameNetworkManager.Instance.localPlayerController.actualClientId) return;

            PlayerControllerB localPlayer = GameNetworkManager.Instance.localPlayerController;

            if (mProps._null)
            {
                Debug.LogError($"Got GrabItem Brodcast but mprops is null!");
            }

            Debug.LogMessage($"Got GrabItem Brodcast!");

            foreach (var grabbable in GameObject.FindObjectsOfType<GrabbableObject>())
            {
                if (grabbable.NetworkObjectId == ulong.Parse(mProps._extraMessage))
                {
                    Debug.Log($"SyncingPatch - GrabItem(): Found object to grab! {grabbable}");
                    if (!grabbable.IsOwner)
                    {
                        grabbable.NetworkObject.ChangeOwnership(localPlayer.actualClientId);
                    }

                    if (!grabbable.IsOwner) // still not owner
                    {
                        Debug.LogError("SyncingPatch - GrabItem(): Could not get ownership!");
                        break;
                    }

                    GrabObject(localPlayer, grabbable);

                    break;
                }
            }
        }
        public static void RequestAbilityConfig(string eventName, MessageProperties mProps)
        {
            if (eventName != ".requestAbilityConfig" || !GameNetworkManager.Instance.isHostingGame) return;

            Debug.Log("Got RequestAbilityConfig()");

            if (mProps._string != "")
            {
                AbilityConfig cfg = Abilities.FindAbilityConfigByName(mProps._string, true);

                if (cfg == null) return;
                NetworkHandler.Instance.EventSendRpc(".receiveAbilityConfig", new(__ulong: mProps._ulong, __extraMessage: Abilities.AbilityCfgToData(cfg, false)));
            }
            else
            {
                foreach (var cfg in Abilities.abilityConfigs)
                {
                    NetworkHandler.Instance.EventSendRpc(".receiveAbilityConfig", new(__ulong: mProps._ulong, __extraMessage:Abilities.AbilityCfgToData(cfg, false)));
                }
            }

        }
        public static void ReceiveAbilityConfig(string eventName, MessageProperties mProps)
        {
            if (eventName != ".receiveAbilityConfig" || GameNetworkManager.Instance.localPlayerController.actualClientId != mProps._ulong) return;

            Debug.Log("Got ReceiveAbilityConfig()");

            Debug.Log($"Data = '{mProps._extraMessage}'");

            Abilities.LoadAbilityConfig(Abilities.ADataToCfg(mProps._extraMessage));
        }
        static void GrabObject(PlayerControllerB __this, GrabbableObject currentlyGrabbingObject)
        {
            Traverse.Create(__this).Field("currentlyGrabbingObject").SetValue(currentlyGrabbingObject);

            if (!GameNetworkManager.Instance.gameHasStarted && !currentlyGrabbingObject.itemProperties.canBeGrabbedBeforeGameStart && StartOfRound.Instance.testRoom == null)
            {
                return;
            }
            Traverse.Create(__this).Field("grabInvalidated").SetValue(false);
            if (currentlyGrabbingObject == null || __this.inSpecialInteractAnimation || currentlyGrabbingObject.isHeld || currentlyGrabbingObject.isPocketed)
            {
                return;
            }
            NetworkObject networkObject = currentlyGrabbingObject.NetworkObject;
            if (networkObject == null || !networkObject.IsSpawned)
            {
                return;
            }
            currentlyGrabbingObject.InteractItem();
            if (currentlyGrabbingObject.grabbable)
            {
                __this.playerBodyAnimator.SetBool("GrabInvalidated", false);
                __this.playerBodyAnimator.SetBool("GrabValidated", false);
                __this.playerBodyAnimator.SetBool("cancelHolding", false);
                __this.playerBodyAnimator.ResetTrigger("Throw");
                Traverse.Create(__this).Method("SetSpecialGrabAnimationBool", [true, currentlyGrabbingObject]);
                __this.isGrabbingObjectAnimation = true;
                __this.cursorIcon.enabled = false;
                __this.cursorTip.text = "";
                __this.twoHanded = currentlyGrabbingObject.itemProperties.twoHanded;
                __this.carryWeight = Mathf.Clamp(__this.carryWeight + (currentlyGrabbingObject.itemProperties.weight - 1f), 1f, 10f);
                if (currentlyGrabbingObject.itemProperties.grabAnimationTime > 0f)
                {
                    __this.grabObjectAnimationTime = currentlyGrabbingObject.itemProperties.grabAnimationTime;
                }
                else
                {
                    __this.grabObjectAnimationTime = 0.4f;
                }
                AccessTools.Method(typeof(PlayerControllerB), "GrabObjectServerRpc").Invoke(__this, [(NetworkObjectReference)currentlyGrabbingObject.NetworkObject]);
                __this.StartCoroutine(GrabObjectCoroutine(__this, currentlyGrabbingObject));
            }
        }
        static IEnumerator GrabObjectCoroutine(PlayerControllerB __this, GrabbableObject currentlyGrabbingObject)
        {
            var trav = Traverse.Create(__this);

            yield return new WaitForSeconds(0.1f);
            currentlyGrabbingObject.parentObject = __this.localItemHolder;
            if (currentlyGrabbingObject.itemProperties.grabSFX != null)
            {
                __this.itemAudio.PlayOneShot(currentlyGrabbingObject.itemProperties.grabSFX, 1f);
            }

            trav.Field("grabbedObjectValidated").SetValue(true);
            //currentlyGrabbingObject.GrabItemOnClient();
            __this.isHoldingObject = true;
            yield return new WaitForSeconds(__this.grabObjectAnimationTime - 0.2f);
            __this.playerBodyAnimator.SetBool("GrabValidated", true);
            __this.isGrabbingObjectAnimation = false;
            yield break;
        }

        // Other Methods
        public static void TeleportPlayer()
        {
            Debug.LogMessage("Got Teleport Player Brodcast!");

            RoundManagerPatch.playersTeleported = 0;

            RoundManager.Instance.StartCoroutine(TeleportSelf(true));
        }
        public static IEnumerator TeleportSelf(bool waitForShipToLand)
        {
            PlayerControllerB localPlayer = GameNetworkManager.Instance.localPlayerController;
            TimeOfDay timeOfDay = GameObject.FindObjectOfType<TimeOfDay>();
            bool isHost = localPlayer.IsServer;

            if (waitForShipToLand)
            {
                while (timeOfDay.currentDayTime <= 130) // Basically the exact time the ship lands
                {
                    yield return new WaitForSecondsRealtime(0.5f);
                    Debug.Log($"[Client TeleportSelf] Tick {timeOfDay.currentDayTime}");
                }
            }

            if (isHost)
            {
                RoundManagerPatch.PlayerDied("Teleport Self", checking: true);
                NetworkHandler.Instance.EventSendRpc(".lockDoor");
            }

            Debug.Log($"TIME TO TELEPORT! Local Player '{localPlayer}' Seekers '{Plugin.seekers}'");

            if (Plugin.seekers.Contains(localPlayer))
            {
                Debug.LogMessage("[SEEKER] Attempted to teleport " + localPlayer.playerUsername + " but they are the seeker!");

                if (GameObject.FindObjectOfType<AudioReverbPresets>())
                {
                    GameObject.FindObjectOfType<AudioReverbPresets>().audioPresets[3].ChangeAudioReverbForPlayer(localPlayer);
                }
                localPlayer.isInElevator = true;
                localPlayer.isInHangarShipRoom = true;
                localPlayer.isInsideFactory = false;
                localPlayer.averageVelocity = 0f;
                localPlayer.velocityLastFrame = Vector3.zero;
                
                localPlayer.TeleportPlayer(StartOfRound.Instance.playerSpawnPositions[0].position);

                NetworkHandler.Instance.EventSendRpc(".teleported", new MessageProperties() { _null = false, _Vector3 = StartOfRound.Instance.playerSpawnPositions[0].position, _ulong = localPlayer.actualClientId });
            }
            else
            {
                Debug.LogMessage("[HIDER] Teleporting to entrance... inside" + Config.forceHidersInside.Value);
                localPlayer.DropAllHeldItems(true, false);
                Vector3 entrancePosition = (Vector3)AccessTools.Method(typeof(RoundManager), "FindMainEntrancePosition", null, null).Invoke(null, [true, !Config.forceHidersInside.Value]);
                EntranceTeleport entranceScript = (EntranceTeleport)AccessTools.Method(typeof(RoundManager), "FindMainEntranceScript", null, null).Invoke(null, [Config.forceHidersInside.Value]);


                if (entranceScript != null)
                {
                    if (Config.teleportHidersToEntrance.Value)
                    {
                        entranceScript.TeleportPlayer();

                        Debug.LogMessage($"Player [{localPlayer.playerUsername}] teleported to entrance...");
                    }
                    else
                    {
                        entranceScript.TeleportPlayer();

                        Debug.LogError($"teleportPlayerOutOfShip NOT IMPLEMENTED! [{localPlayer.playerUsername}] teleported to entrance...");
                    }
                    NetworkHandler.Instance.EventSendRpc(".teleported", new MessageProperties() { _null = false, _Vector3 = entrancePosition, _ulong = localPlayer.actualClientId });
                }
                else
                {
                    Debug.LogMessage("Failed to find entrance");
                }
            }


            while (timeOfDay.currentDayTime < Config.timeSeekerIsReleased.Value)
            {
                int aliveSeekers = 0;
                foreach (var player in Plugin.seekers)
                {
                    if (!player.isPlayerDead && player.gameObject.activeSelf)
                    {
                        aliveSeekers++;
                    }
                }
                if (aliveSeekers <= 0)
                {
                    GameObject.FindAnyObjectByType<StartMatchLever>().EndGame();
                    Debug.LogError("All Seekers Died Before The Round Started!");
                    yield break;
                }

                if (!timeOfDay.timeHasStarted)
                {
                    yield break;
                }
                //Debug.LogMessage("Current Time: " + timeOfDay.currentDayTime + " Target Time: " + Config.timeSeekerIsReleased.Value);
                yield return new WaitForSeconds(1);
            }

            if (isHost)
            {
                NetworkHandler.Instance.EventSendRpc(".openDoor");
            }

            if (Plugin.seekers.Contains(localPlayer))
            {
                Debug.LogMessage("Teleporting to entrance...");
                EntranceTeleport entranceScript = (EntranceTeleport)AccessTools.Method(typeof(RoundManager), "FindMainEntranceScript", null, null).Invoke(null, [Config.forceSeekerInside.Value]);


                int itemAmount = 0;
                foreach (var slot in localPlayer.ItemSlots)
                {
                    if (slot != null)
                    {
                        itemAmount++;
                    }
                }
                if (entranceScript != null && itemAmount != 0)
                {
                    if (Config.teleportSeekerToEntrance.Value)
                    {
                        entranceScript.TeleportPlayer();
                        Debug.LogMessage($"Player [{localPlayer.playerUsername}] teleported to entrance...");
                    }
                }
                else
                {
                    Debug.LogMessage("Failed to find entrance / Player did not pick up items");
                }
            }
        }
    }
}