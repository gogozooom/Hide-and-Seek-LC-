using GameNetcodeStuff;
using HarmonyLib;
using HideAndSeek.AbilityScripts;
using Unity.Netcode;
using UnityEngine;
using Debug = Debugger.Debug;

namespace HideAndSeek.Patches;

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
                if (Time.realtimeSinceStartup - (float)trav.Field("timeAtLastUsingEntrance").GetValue() > 3f && !__instance.GetClosestPlayer(false, false, false) && !__instance.PathIsIntersectedByLineOfSight((Vector3)trav.Field("mainEntrancePosition").GetValue(), false, false))
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
                && !(StartOfRound.Instance.allPlayerScripts[i] == spawnAbilityInfo.creatorPlayer || spawnAbilityInfo.otherFriendlies.Contains(StartOfRound.Instance.allPlayerScripts[i])))
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
