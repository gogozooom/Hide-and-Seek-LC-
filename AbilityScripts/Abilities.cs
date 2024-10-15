using GameNetcodeStuff;
using HideAndSeek.AbilityScripts.Extra;
using HideAndSeek.AudioScripts;
using HideAndSeek.Patches;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.Netcode;
using UnityEngine;
using Debug = Debugger.Debug;

namespace HideAndSeek.AbilityScripts
{
    public static class Abilities
    {
        public static List<AbilityBase> abilities = new List<AbilityBase>
        {
            new AbilityBase(_abilityName:"Money", _abilityDescription:"Give everyone 9999 money!", _abilityCategory: "HIDDEN",
                _abilityCost:0, _abilityDelay:0f, _requriesRoundActive: false, _requiresSeekerActive: false,
                _serverEvent: GiveMoneyServerEvent),

            new AbilityBase(_abilityName:"Remote", _abilityDescription:"Gives you a remote! Not usefull unless you have you have a mod for it. (External Mod Recommended)", _abilityCategory:"HIDDEN",
                _abilityCost:200, _oneTimeUse:true,
                _requiresSeekerActive:false,
                _serverEvent:SpawnRemoteServerEvent),

            new AbilityBase(_abilityName:"Taunt", _abilityDescription:"Let out a little somthin!... You might even get a reward!",
                _abilityDelay:25f, _abilityCost:0, 
                _seekerAbility:false,
                _serverEvent:TauntServerEvent, _clientEvent:TauntClientEvent),

            new AbilityBase(_abilityName:"Key", _abilityDescription:"Gives you a key! Usefull for opening doors, or even, locking them... (External Mod Recommended)", _abilityCategory:"Item",
                _abilityCost:245, _abilityDelay:60f,
                _requiresSeekerActive:false,
                _serverEvent:SpawnKeyServerEvent),

            new AbilityBase(_abilityName:"TZP-Inhalant", _abilityDescription:"Gives you TZP Inhalant! Usefull for gaining some speed and sounding funny.", _abilityCategory:"Item",
                _abilityDelay:60f, _abilityCost:205,
                _requiresSeekerActive:false,
                _serverEvent:SpawnTZPServerEvent),

            new AbilityBase(_abilityName:"Shovel", _abilityDescription:"Gives you a Shovel! Usefull for possibly killing the seeker! Or just trolling your team mates...", _abilityCategory:"Item",
                _oneTimeUse:true, _abilityCost:495,
                _requiresSeekerActive:false,
                _seekerAbility: false,
                _serverEvent:SpawnShovelServerEvent),

            new AbilityBase(_abilityName:"Stun Grenade", _abilityDescription:"Gives you a Stun Grenade! Usefull for blinding the seeker for an escape!", _abilityCategory:"Item",
                _abilityDelay:120f, _abilityCost:205,
                _requiresSeekerActive:false,
                _seekerAbility: false,
                _serverEvent:SpawnStunGServerEvent),

            new AbilityBase(_abilityName:"Flashlight", _abilityDescription:"Lost your flashlight? No worries, have a free flashlight once per round!", _abilityCategory:"Item",
                _oneTimeUse: true, _abilityCost:0,
                _requiresSeekerActive:false,
                _serverEvent:SpawnFlashlightServerEvent),

            new AbilityBase(_abilityName:"Walkie-talkie", _abilityDescription:"Wanna talk to your teammates? Have a free walkie-talkie once per round!", _abilityCategory:"Item",
                _oneTimeUse: true, _abilityCost:0,
                _requiresSeekerActive:false,
                _seekerAbility: false,
                _serverEvent:SpawnWalkieServerEvent),

            new AbilityBase(_abilityName:"Critical Injury", _abilityDescription:"Haunts your nearest enemy, and if they don't react in time, their hp will be set to 1! Making them easily susceptible to death.", _abilityCategory:"Offensive",
                _abilityCost:200, _abilityDelay:120f,
                _serverEvent:CriticalInjuryServerEvent, _clientEvent:CriticalInjuryClientEvent),

            new AbilityBase(_abilityName:"Teleport", _abilityDescription:"Teleport yourself to a random location! Good for getting out of sticky situations! WARNING: This ability has a 3 second startup! This means it will take 3 seconds after using the ability for you to get teleported!", _abilityCategory:"Defensive",
                _abilityCost:400, _abilityDelay:60f,
                _serverEvent:TeleportServerEvent, _clientEvent:TeleportClientEvent,
                _requiresSeekerActive:false,
                _seekerAbility:false),

            new AbilityBase(_abilityName:"Swap", _abilityDescription:"Swap locations with a random player! Good for getting a free hiding spot! I think... WARNING: This ability has a 3 second startup! This means it will take 3 seconds after using the ability for you to get teleported!", _abilityCategory:"Defensive",
                _serverEvent:SwapServerEvent, _clientEvent:SwapClientEvent,
                _abilityCost:450, _abilityDelay:60f,
                _seekerAbility:false),

            new AbilityBase(_abilityName:"Stealth", _abilityDescription:"Sneak past your enemies completely silently for 30 seconds!", _abilityCategory:"Stealth",
                _serverEvent:StealthServerEvent, _clientEvent:StealthClientEvent,
                _abilityCost:30, _abilityDelay:45
                ),

            new AbilityBase(_abilityName:"Long Stealth", _abilityDescription:"Sneak past your enemies completely silently!",  _abilityCategory:"Stealth",
                _serverEvent:LongStealthServerEvent, _clientEvent:StealthClientEvent,
                _abilityCost:259, _oneTimeUse:true
                ),

            new AbilityBase(_abilityName:"Invisibility", _abilityDescription:"Get the ultimate hiding spot in plain sight! Easly trick and sneek around the seeker! Lasts 30 seconds.", _abilityCategory:"Stealth",
                _serverEvent:InvisibilityServerEvent, _clientEvent:InvisibilityClientEvent,
                _abilityCost:500, _abilityDelay:45
                ),

            new AbilityBase(_abilityName:"Spawn Loot Bug", _abilityDescription:"Spawns a little yippee fren! he will find items and put them right at your feet! What a good boi!", _abilityCategory:"Spawn",
                _serverEvent:SpawnLootBugServerEvent, _clientEvent:SpawnClientEvent,
                _abilityCost:99, _abilityDelay:80, _hiderAbility:false),

            new AbilityBase(_abilityName:"Spawn Mimic", _abilityDescription:"Spawns a mimic to help seek out those pesky hiders and multiply for effectiveness! Don't worry, he won't barf on you though!", _abilityCategory:"Spawn",
                _serverEvent:SpawnMimicServerEvent, _clientEvent:SpawnClientEvent,
                _abilityCost:200, _abilityDelay:120, _hiderAbility:false),

            new AbilityBase(_abilityName:"Spawn Thumper", _abilityDescription:"Spawns a flipin' fast boi to quickly search the building and scare off the hiders! Don't worry, he doesn't bite you!", _abilityCategory:"Spawn",
                _serverEvent:SpawnThumperServerEvent, _clientEvent:SpawnClientEvent,
                _abilityCost:500, _abilityDelay:120, _hiderAbility:false),

            new AbilityBase(_abilityName:"Spawn Bracken", _abilityDescription:"Spawns a bracken to help track those terrible hiders! Don't worry, he won't snap your neck~", _abilityCategory:"Spawn",
                _serverEvent:SpawnBrackenServerEvent, _clientEvent:SpawnClientEvent,
                _abilityCost:750, _abilityDelay:120, _hiderAbility:false),


            new AbilityBase(_abilityName:"Spawn Turret", _abilityDescription:"Catch the hiders off guard with perfect turret placement! for only for two-ninety-nine! Don't worry, it wont shoot you!", _abilityCategory:"Spawn",
                _serverEvent:SpawnTurretServerEvent, _clientEvent:SpawnClientEvent,
                _abilityCost:299, _abilityDelay: 30f, _hiderAbility:false),

            new AbilityBase(_abilityName:"Spawn LandMine", _abilityDescription:"Make the hiders go off with a BOOM! for only one-oh-nine! Don't worry, it wont detonate from your feet!", _abilityCategory:"Spawn",
                _serverEvent:SpawnLandmineServerEvent, _clientEvent:SpawnClientEvent,
                _abilityCost:109, _abilityDelay: 5f, _hiderAbility:false),

            new AbilityBase(_abilityName:"Heat Seeking", _abilityDescription:"The ultimate hider seeking device! For only nine-ninety-nine you can pinpoint the location of a random enemy, and get revenge for something they never did! NOTE: Your location will also be revealed to the target!", _abilityCategory:"Offensive",
                _serverEvent:HeatSeekingServerEvent, _clientEvent:HeatSeekingClientEvent,
                _abilityCost:999,
                _oneTimeUse: true, _hiderAbility:false),
        }; 

        public static List<AbilityConfig> abilityConfigs = new List<AbilityConfig>();

        // -- Ability Methods --

        #region Templates
        internal static void TemplateServerEvent(AbilityBase ability, ulong activatorId)
        {
            Debug.Log($"[Server] ------------------ Ability Fired by id '{activatorId}' ------------------");

            ability.ActivateClient(activatorId, "Extra Info!!!");
        }

        internal static void TemplateClientEvent(AbilityBase ability, ulong activatorId, string extraMessage = null)
        {
            Debug.Log($"[Client] --- Ability Fired by id '{activatorId}' with '{extraMessage}'! ---");
        }
        #endregion

        #region Events

        #region Hidden
        static void GiveMoneyServerEvent(AbilityBase ability, ulong activatorId)
        {
            foreach (var player in GameObject.FindObjectsByType<PlayerControllerB>(0))
            {
                if (player.isPlayerControlled)
                {
                    NetworkHandler.Instance.EventSendRpc(".moneyChanged", new(__int: 9999, __ulong:player.actualClientId));
                }
            }
        }
        #endregion

        #region CriticalInjury
        static void CriticalInjuryServerEvent(AbilityBase ability, ulong activatorId)
        {
            PlayerControllerB activatorPlayer = RoundManagerPatch.GetPlayerWithClientId(activatorId);

            ulong playerTarget = 10001;

            float closestPlayer = 10000f;
            foreach (var player in GameObject.FindObjectsByType<PlayerControllerB>(0))
            {
                if (!player.isPlayerControlled
                    || player.isPlayerDead
                    ||  Plugin.seekers.Contains(player) == Plugin.seekers.Contains(activatorPlayer) // Invalid Target | 'Plugin.seekers.Contains(player) == seekerUsedAbility' Is of my kind
                    || (Plugin.seekers.Contains(activatorPlayer) && Plugin.zombies.Contains(player))) continue; // Inavlid Target | Seeker dosen't target zombies!

                float distance = (player.transform.position - activatorPlayer.transform.position).magnitude;
                if (distance < closestPlayer)
                {
                    playerTarget = player.actualClientId;
                    closestPlayer = distance;
                }
            }

            if (playerTarget == 10001)
            {
                playerTarget = RoundManagerPatch.GetPlayerWithClientId(activatorId).actualClientId;
                Debug.LogError("CriticalInjuryServerEvent(): Could not find player target!");
            }

            ability.ActivateClient(activatorId, playerTarget.ToString());
        }
        static void CriticalInjuryClientEvent(AbilityBase ability, ulong activatorId, string extraMessage)
        {
            if (GameNetworkManager.Instance.localPlayerController.actualClientId == ulong.Parse(extraMessage)) // Is Target
            {
                GameNetworkManager.Instance.StartCoroutine(CriticalInjuryClientEventC(ability, activatorId));
            }
        }
        static IEnumerator CriticalInjuryClientEventC(AbilityBase ability, ulong activatorId)
        {
            PlayerControllerB localPlayer = GameNetworkManager.Instance.localPlayerController;

            Vector3 audioPos = AudioManager.PlaySound("DistantFireAlarm", position: localPlayer.transform.position + (Vector3.up * 2), spatialBend: 1, volume: 1.4f, maxDistance: 100, minDistance: 20).transform.position;
            AudioManager.PlaySound("ScaryWarning", position: localPlayer.transform.position + (Vector3.up * 2));

            yield return new WaitForSecondsRealtime(1);

            HUDManager.Instance.DisplayTip("Unknown", $"Something is coming...", true);

            yield return new WaitForSecondsRealtime(Random.Range(5f, 7f));

            if ((audioPos - localPlayer.transform.position).magnitude <= 20)
            {
                AudioManager.PlaySound("HeightDamage");

                localPlayer.DamagePlayer(localPlayer.health - 1); // Set them to 1 hp
            }
            else
            {
                HUDManager.Instance.DisplayTip("Unknown", $"Sounds like it left..");
            }
        }
        #endregion

        #region Taunt

        static void TauntServerEvent(AbilityBase ability, ulong activatorId)
        {
            PlayerControllerB activatorPlayer = RoundManagerPatch.GetPlayerWithClientId(activatorId);
            AbilityInstance abilityInstance = activatorPlayer.GetComponent<AbilityInstance>();

            int soundType = 0;

            int randomNumber = UnityEngine.Random.Range(1, 100);
            float range = UnityEngine.Random.RandomRange(0f, 1f);


            if (randomNumber < 5)
            {
                soundType = 2;

                AudioClip sound = AudioManager.GetSound("MegaBoi", random: range);

                Debug.LogError($"Got a total of {Mathf.RoundToInt(25 * sound.length)}$ from using taunt {sound.name}");

                NetworkHandler.Instance.EventSendRpc(".moneyChanged", new(__ulong: activatorId, __int: Mathf.RoundToInt(25 * sound.length), __string: "silent"));
            }
            else if (randomNumber < 12)
            {
                soundType = 1;

                AudioClip sound = AudioManager.GetSound("BigBoi", random: range);

                Debug.LogError($"Got a total of {Mathf.RoundToInt(12 * sound.length)}$ from using taunt {sound.name}");

                NetworkHandler.Instance.EventSendRpc(".moneyChanged", new(__ulong: activatorId, __int: Mathf.RoundToInt(12 * sound.length), __string: "silent"));
            }
            else
            {
                AudioClip sound = AudioManager.GetSound("Taunt", random: range);

                Debug.LogError($"Got a total of {Mathf.RoundToInt(5 * sound.length)}$ from using taunt {sound.name}");
                NetworkHandler.Instance.EventSendRpc(".moneyChanged", new(__ulong: activatorId, __int: Mathf.RoundToInt(5 * sound.length), __string: "silent"));
            }


            ability.ActivateClient(activatorId, $"{soundType},{range}");
        }

        static void TauntClientEvent(AbilityBase ability, ulong activatorId, string extraMessage = null)
        {
            bool isUser = activatorId == GameNetworkManager.Instance.localPlayerController.actualClientId;
            AbilityInstance abilityInstance = GameNetworkManager.Instance.localPlayerController.GetComponent<AbilityInstance>();
            Vector3 playerPosition = RoundManagerPatch.GetPlayerWithClientId(activatorId).transform.position;

            float randomN = float.Parse(extraMessage.Split(",")[1]);

            int tauntType = int.Parse(extraMessage.Split(",")[0]);

            if (tauntType == 2)
            {
                AudioManager.PlaySound("MegaBoi", position:playerPosition, spatialBend:1, minDistance: 20, maxDistance:800, random: randomN);
                if (isUser)
                {
                    abilityInstance.DisplayTip("You let out a massiv boi!", true);
                }
            }
            else if (tauntType == 1)
            {
                AudioManager.PlaySound("BigBoi", position: playerPosition, spatialBend: 1, minDistance: 12, maxDistance:400, random: randomN);
                if (isUser)
                {
                    abilityInstance.DisplayTip("You let out a big boi!", false);
                }
            }
            else
            {
                AudioManager.PlaySound("Taunt", position: playerPosition, spatialBend: 1, minDistance: 5, maxDistance: 200, random: randomN);
                if (isUser)
                {
                    abilityInstance.DisplayTip("You let out a lil boi.", false);
                }
            }
        }

        #endregion

        #region Spawn
        static void SpawnKeyServerEvent(AbilityBase ability, ulong activatorId)
        {
            RoundManagerPatch.SpawnNewItem("Key", RoundManagerPatch.GetPlayerWithClientId(activatorId), true);
        }
        static void SpawnTZPServerEvent(AbilityBase ability, ulong activatorId)
        {
            RoundManagerPatch.SpawnNewItem("TZP-Inhalant", RoundManagerPatch.GetPlayerWithClientId(activatorId), true);
        }
        static void SpawnFlashlightServerEvent(AbilityBase ability, ulong activatorId)
        {
            RoundManagerPatch.SpawnNewItem("Flashlight", RoundManagerPatch.GetPlayerWithClientId(activatorId), true);
        }
        static void SpawnWalkieServerEvent(AbilityBase ability, ulong activatorId)
        {
            RoundManagerPatch.SpawnNewItem("Walkie-talkie", RoundManagerPatch.GetPlayerWithClientId(activatorId), true);
        }
        static void SpawnShovelServerEvent(AbilityBase ability, ulong activatorId)
        {
            RoundManagerPatch.SpawnNewItem("Shovel", RoundManagerPatch.GetPlayerWithClientId(activatorId), true);
        }
        static void SpawnStunGServerEvent(AbilityBase ability, ulong activatorId)
        {
            RoundManagerPatch.SpawnNewItem("Stun grenade", RoundManagerPatch.GetPlayerWithClientId(activatorId), true);
        }
        static void SpawnRemoteServerEvent(AbilityBase ability, ulong activatorId)
        {
            RoundManagerPatch.SpawnNewItem("Remote", RoundManagerPatch.GetPlayerWithClientId(activatorId), true);
        }
        
        static void SpawnLootBugServerEvent(AbilityBase ability, ulong activatorId)
        {
            SpawnInsideMonster(ability, activatorId, "Hoarding bug");
        }
        static void SpawnMimicServerEvent(AbilityBase ability, ulong activatorId)
        {
            SpawnInsideMonster(ability, activatorId, "Masked");
        }
        static void SpawnBrackenServerEvent(AbilityBase ability, ulong activatorId)
        {
            SpawnInsideMonster(ability, activatorId, "Flowerman");
        }
        static void SpawnThumperServerEvent(AbilityBase ability, ulong activatorId)
        {
            SpawnInsideMonster(ability, activatorId, "Crawler");
        }
        public static List<NetworkObject> objectsToDespawnNextRound = new();

        public static GameObject turretPrefab;
        static void SpawnTurretServerEvent(AbilityBase ability, ulong activatorId)
        {
            Transform playerT = RoundManagerPatch.GetPlayerWithClientId(activatorId).transform;
            Vector3 positionToSpawn = playerT.position + (playerT.forward * 1.3f);

            if (!turretPrefab)
            {
                if (!RoundManagerPatch.currentLevel)
                {
                    Debug.LogError("No current level loaded!");
                    return;
                }

                foreach (var item in RoundManagerPatch.currentLevel.spawnableMapObjects)
                {
                    if (item.prefabToSpawn.GetComponentInChildren<Turret>() != null)
                    {
                        turretPrefab = item.prefabToSpawn;
                    }
                }
            }

            if (!turretPrefab) // Still Null
            {
                Debug.LogError("Could not get turret prefab!");
                NetworkHandler.Instance.EventSendRpc(".moneyChanged", new(__string: "silent", __int: ability.abilityCost)); // Refund
                NetworkHandler.Instance.EventSendRpc(".tip", new(__string: "Could not get turret prefab! Play a map with a turret first!", __bool: true, __ulong:activatorId, __int:-1));

                return;
            }

            GameObject turret = GameObject.Instantiate(turretPrefab, positionToSpawn, playerT.rotation);

            NetworkObject networkObject = turret.GetComponent<NetworkObject>();
            networkObject.Spawn(true);

            objectsToDespawnNextRound.Add(networkObject);

            ability.ActivateClient(activatorId, $"Turret~{JsonUtility.ToJson(positionToSpawn)}~{networkObject.NetworkObjectId}");
        }
        public static GameObject landminePrefab;
        static void SpawnLandmineServerEvent(AbilityBase ability, ulong activatorId)
        {
            Transform playerT = RoundManagerPatch.GetPlayerWithClientId(activatorId).transform;
            Vector3 positionToSpawn = playerT.position + (playerT.forward * 1.3f);

            if (!landminePrefab)
            {
                if (!RoundManagerPatch.currentLevel)
                {
                    Debug.LogError("No current level loaded!");
                    return;
                }

                foreach (var item in RoundManagerPatch.currentLevel.spawnableMapObjects)
                {
                    if (item.prefabToSpawn.GetComponentInChildren<Landmine>() != null)
                    {
                        landminePrefab = item.prefabToSpawn;
                    }
                }
            }

            if (!landminePrefab) // Still Null
            {
                Debug.LogError("Could not get landmine prefab!");
                NetworkHandler.Instance.EventSendRpc(".moneyChanged", new(__string: "silent", __int: ability.abilityCost)); // Refund
                NetworkHandler.Instance.EventSendRpc(".tip", new(__string: "Could not get landmine prefab! Play a map with a landmine first!", __bool: true, __ulong: activatorId, __int: -1));
                return;
            }


            GameObject landmine = GameObject.Instantiate(landminePrefab, positionToSpawn, playerT.rotation);

            NetworkObject networkObject = landmine.GetComponent<NetworkObject>();
            networkObject.Spawn(true);

            objectsToDespawnNextRound.Add(networkObject);

            ability.ActivateClient(activatorId, $"Landmine~{JsonUtility.ToJson(positionToSpawn)}~{networkObject.NetworkObjectId}");
        }
        static void SpawnInsideMonster(AbilityBase ability, ulong activatorId, string monsterName)
        {
            PlayerControllerB userPlayer = RoundManagerPatch.GetPlayerWithClientId(activatorId);

            Vector3 randomPosition = new Vector3();

            int i = UnityEngine.Random.RandomRangeInt(0, RoundManager.Instance.insideAINodes.Length - 1);

            randomPosition = RoundManager.Instance.insideAINodes[i].transform.position;

            EnemyType enemyType = null;

            foreach (var enemy in Resources.FindObjectsOfTypeAll<EnemyType>())
            {
                if (enemy.enemyName == monsterName)
                {
                    enemyType = enemy;
                    break;
                }
            }

            if (enemyType == null)
            {
                Debug.LogError($"Could not find {monsterName} for some reason? Enemy list = '{JsonUtility.ToJson(Resources.FindObjectsOfTypeAll<EnemyType>())}'");
            }

            // TMP
            randomPosition = userPlayer.transform.position + (userPlayer.transform.forward*3);

            NetworkObject spawnedObject = RoundManager.Instance.SpawnEnemyGameObject(randomPosition, 0, -1, enemyType);
            RoundManager.Instance.SpawnMapObjects();

            ability.ActivateClient(activatorId, $"{monsterName}~{JsonUtility.ToJson(randomPosition)}~{spawnedObject.NetworkObjectId}");
        }
        static void SpawnClientEvent(AbilityBase ability, ulong activatorId, string extraMessage = null)
        {
            AudioManager.PlaySound("Teleported", position: JsonUtility.FromJson<Vector3>(extraMessage.Split("~")[1]), maxDistance: 50, spatialBend: 1);

            PlayerControllerB localPlayer = GameNetworkManager.Instance.localPlayerController;
            PlayerControllerB userPlayer = RoundManagerPatch.GetPlayerWithClientId(activatorId);
            bool isUserSeeker = Plugin.seekers.Contains(userPlayer);

            if(localPlayer.actualClientId == activatorId)
            {
                localPlayer.GetComponent<AbilityInstance>().DisplayTip($"A {extraMessage.Split("~")[0]} has been spawned!");
            }

            bool gotSpawnInfo = false;

            if (extraMessage.Split("~").Length < 3)
            {
                Debug.LogError($"SpawnClientEvent(): '{extraMessage.Split('~')[0]}' Did not send network object id!");
                return;
            }

            foreach (var networkObject in GameObject.FindObjectsOfType<NetworkObject>())
            {
                if(networkObject.NetworkObjectId == ulong.Parse(extraMessage.Split("~")[2]))
                {
                    // Found target monster
                    SpawnAbilityInfo info = networkObject.gameObject.AddComponent<SpawnAbilityInfo>();
                    info.creatorPlayer = userPlayer;

                    if (isUserSeeker)
                    {
                        info.otherFriendlies = Plugin.seekers;
                        foreach (var zombie in Plugin.zombies)
                        {
                            info.otherFriendlies.Add(zombie);
                        }
                    }
                    else
                    {
                        info.otherFriendlies = new();
                        foreach (var player in GameObject.FindObjectsByType<PlayerControllerB>(0))
                        {
                            if (!Plugin.seekers.Contains(player) && !Plugin.zombies.Contains(player))
                            {
                                info.otherFriendlies.Add(player);
                            }
                        }
                    }

                    gotSpawnInfo = true;
                    break;
                }
            }

            if (gotSpawnInfo)
            {
                Debug.LogMessage($"SpawnClientEvent(): '{extraMessage.Split('~')[0]}' Got Spawn Info!");
            }
            else
            {
                Debug.LogError($"SpawnClientEvent(): '{extraMessage.Split('~')[0]}' Could not find network object! id = {extraMessage.Split('~')[2]}");
            }
        }

        #endregion

        #region Teleport
        static void TeleportServerEvent(AbilityBase ability, ulong activatorId)
        {
            PlayerControllerB activatorPlayer = RoundManagerPatch.GetPlayerWithClientId(activatorId);

            Vector3 randomPosition = new Vector3();

            int i = UnityEngine.Random.RandomRangeInt(0, RoundManager.Instance.insideAINodes.Length - 1);

            randomPosition = RoundManager.Instance.insideAINodes[i].transform.position;

            ability.ActivateClient(activatorId, $"{JsonUtility.ToJson(activatorPlayer.transform.position)}W{JsonUtility.ToJson(randomPosition)}"); // Before,After
        }

        static void TeleportClientEvent(AbilityBase ability, ulong activatorId, string extraMessage = null)
        {
            GameNetworkManager.Instance.StartCoroutine(TeleportClientEventC(ability, activatorId, extraMessage));
        }
        static IEnumerator TeleportClientEventC(AbilityBase ability, ulong activatorId, string extraMessage = null)
        {
            PlayerControllerB localPlayer = GameNetworkManager.Instance.localPlayerController;
            PlayerControllerB activatorPlayer = RoundManagerPatch.GetPlayerWithClientId(activatorId);

            Vector3 beforePosition = JsonUtility.FromJson<Vector3>(extraMessage.Split("W")[0]);
            Vector3 randomPosition = JsonUtility.FromJson<Vector3>(extraMessage.Split("W")[1]);

            activatorPlayer.beamUpParticle.Play();

            AudioManager.PlaySound("BeamCharging", position: beforePosition, maxDistance: 50, spatialBend: 1, parent: activatorPlayer.transform);
            AudioManager.PlaySound("Spark", position: beforePosition, maxDistance: 50, spatialBend: 1, parent: activatorPlayer.transform);

            yield return new WaitForSeconds(3);

            if (activatorPlayer.isPlayerDead)
            {
                yield break;
            }

            if (activatorId == localPlayer.actualClientId)
            {
                localPlayer.TeleportPlayer(randomPosition);
            }

            AudioManager.PlaySound("Teleporting", position: activatorPlayer.transform.position, maxDistance: 50, spatialBend: 1);
            AudioManager.PlaySound("Teleported", position: randomPosition, maxDistance: 50, spatialBend: 1);
        }
        #endregion

        #region Swap
        static void SwapServerEvent(AbilityBase ability, ulong activatorId)
        {
            Debug.Log($"[Server] ------------------ Ability Fired by id '{activatorId}' ------------------");

            ulong selectedPlayerId = 10001;

            List<ulong> playerList = new();

            foreach (var player in GameObject.FindObjectsByType<PlayerControllerB>(0))
            {
                if (player.isPlayerControlled && !player.isPlayerDead && player.actualClientId != activatorId)
                {
                    playerList.Add(player.actualClientId);
                }
            }

            if (playerList.Count > 0)
            {
                int i = UnityEngine.Random.RandomRangeInt(0, playerList.Count);

                selectedPlayerId = playerList[i];
            }

            Debug.LogError($"Possible Players = {JsonUtility.ToJson(playerList)}; player chosen = {selectedPlayerId}");

            if (selectedPlayerId == 10001 || selectedPlayerId == activatorId)
            {
                Debug.LogError("Not enough players to teleport! Teleporting to self...");
                selectedPlayerId = activatorId;

                if(ability.abilityCost != 0)
                    NetworkHandler.Instance.EventSendRpc(".moneyChanged", new(__string:"silent", __int:ability.abilityCost, __ulong:activatorId)); // Refund!
            }

            ability.ActivateClient(activatorId, selectedPlayerId.ToString());
        }
        static void SwapClientEvent(AbilityBase ability, ulong activatorId, string extraMessage = null)
        {
            GameNetworkManager.Instance.StartCoroutine(SwapClientEventC(ability, activatorId, extraMessage));
        }
        static IEnumerator SwapClientEventC(AbilityBase ability, ulong activatorId, string extraMessage = null)
        {
            Debug.Log($"[Client] --- Ability Fired by id '{activatorId}' with '{extraMessage}'! ---");

            ulong targetPlayerId = ulong.Parse(extraMessage);
            PlayerControllerB localPlayer = GameNetworkManager.Instance.localPlayerController;
            PlayerControllerB activatorPlayer = RoundManagerPatch.GetPlayerWithClientId(activatorId);
            PlayerControllerB targetPlayer = RoundManagerPatch.GetPlayerWithClientId(targetPlayerId);

            targetPlayer.beamOutBuildupParticle.Play();
            activatorPlayer.beamOutBuildupParticle.Play();

            AudioManager.PlaySound("BeamCharging", position: activatorPlayer.transform.position, maxDistance: 50, spatialBend: 1, parent: activatorPlayer.transform);
            AudioManager.PlaySound("Spark", position: activatorPlayer.transform.position, maxDistance: 50, spatialBend: 1, parent: activatorPlayer.transform);

            AudioManager.PlaySound("BeamCharging", position: targetPlayer.transform.position, maxDistance: 50, spatialBend: 1, parent: targetPlayer.transform);
            AudioManager.PlaySound("Spark", position: targetPlayer.transform.position, maxDistance: 50, spatialBend: 1, parent: targetPlayer.transform);

            Vector3 targetPosition = activatorPlayer.transform.position;

            if (targetPlayerId != activatorId) 
            {
                if (localPlayer.actualClientId == activatorId)
                {
                    // User
                    targetPosition = targetPlayer.transform.position;
                    localPlayer.GetComponent<AbilityInstance>().DisplayTip($"Switching places with '{targetPlayer.playerUsername}'", false);
                }
                else if (localPlayer.actualClientId == targetPlayerId)
                {
                    // Target
                    targetPosition = activatorPlayer.transform.position;
                    localPlayer.GetComponent<AbilityInstance>().DisplayTip($"'{activatorPlayer.playerUsername}' is switching places with you!", true);
                }
            }

            yield return new WaitForSeconds(3);

            if (activatorPlayer.isPlayerDead || targetPlayer.isPlayerDead)
            {
                if (localPlayer.actualClientId == activatorId)
                {
                    // User
                    localPlayer.GetComponent<AbilityInstance>().DisplayTip($"Failed to switch places with '{RoundManagerPatch.GetPlayerWithClientId(targetPlayerId).playerUsername}'", false);
                }
                yield break;
            }

            if (localPlayer.actualClientId == activatorId || localPlayer.actualClientId == targetPlayerId)
            {
                // User
                localPlayer.TeleportPlayer(targetPosition);
            }

            activatorPlayer.beamOutParticle.Play();
            targetPlayer.beamOutParticle.Play();

            if (targetPlayerId == activatorId)
            {
                if (localPlayer.actualClientId == activatorId)
                {
                    ability.lastUsed = 0f;
                    localPlayer.GetComponent<AbilityInstance>().DisplayTip("Not enough targets!", true);
                }
            }
            else
            {
                AudioManager.PlaySound("Teleported", position: activatorPlayer.transform.position, maxDistance: 20, spatialBend: 1);
                AudioManager.PlaySound("Teleported", position: targetPlayer.transform.position, maxDistance: 20, spatialBend: 1);
            }
        }
        #endregion

        #region Heat Seeking

        static void HeatSeekingServerEvent(AbilityBase ability, ulong activatorId)
        {
            GameNetworkManager.Instance.StartCoroutine(HeatSeekingServerEventCoroutine(ability, activatorId));
        }
        static IEnumerator HeatSeekingServerEventCoroutine(AbilityBase ability, ulong activatorId)
        {
            PlayerControllerB activatorPlayer = RoundManagerPatch.GetPlayerWithClientId(activatorId);

            List<PlayerControllerB> targets = new List<PlayerControllerB>();
            foreach (var player in GameObject.FindObjectsByType<PlayerControllerB>(0))
            {
                if (player.isPlayerControlled && !player.isPlayerDead && Plugin.seekers.Contains(player) != Plugin.seekers.Contains(activatorPlayer) && !Plugin.zombies.Contains(player))
                {
                    Debug.LogWarning("Adding player: " + player + " To targets!");
                    targets.Add(player);
                }
            }

            Debug.LogMessage($"[Heat Seeking] Target count = '{targets.Count}'");

            int r = Random.Range(0, targets.Count);

            if (targets.Count > 0)
            {
                Debug.LogWarning("Player id chosen: " + targets[r]);
                while (StartOfRound.Instance.shipHasLanded)
                {
                    ability.ActivateClient(activatorId, "TG:"+targets[r].actualClientId.ToString());
                    foreach (var item in dots.ToArray())
                    {
                        if(item.targetedPlayer.isPlayerDead == true)
                        {
                            dots.Remove(item);
                            GameObject.Destroy(item.gameObject);
                        }
                    }
                    if (dots.Count <= 0)
                    {
                        break;
                    }
                    yield return new WaitForSecondsRealtime(12f); // Update Time
                }
                ability.ActivateClient(activatorId, "CLEAN");
            }
            else
            {
                Debug.LogWarning("None To Target!");
                NetworkHandler.Instance.EventSendRpc(".moneyChanged", new(__string:"silent", __int: ability.abilityCost, __ulong: activatorId)); // Refund!
            }
            ability.usedThisRound = false;
        }

        static List<SeekerDotVisuals> dots = new();
        static void HeatSeekingClientEvent(AbilityBase ability, ulong activatorId, string extraMessage = null)
        {
            PlayerControllerB localPlayer = GameNetworkManager.Instance.localPlayerController;
            PlayerControllerB activatorPlayer = RoundManagerPatch.GetPlayerWithClientId(activatorId);
            AbilityInstance localAbilityInstance = localPlayer.GetComponent<AbilityInstance>();

            if(extraMessage == "CLEAN" || localPlayer.isPlayerDead)
            {
                foreach (var dot in dots)
                {
                    GameObject.Destroy(dot.gameObject);
                }
                dots.Clear();
                if (localPlayer.actualClientId == activatorId)
                {
                    localAbilityInstance.DisplayTip("Target(s) eliminated! Disengaging...", true);
                }
                return;
            }

            if (dots.Count == 0 && extraMessage.Split(":")[0] == "TG")
            {
                ulong targetId = ulong.Parse(extraMessage.Split(":")[1]);
                if (localPlayer.actualClientId == activatorId)
                {
                    // User
                    var newDot = new GameObject().AddComponent<SeekerDotVisuals>();

                    foreach (var player in GameObject.FindObjectsByType<PlayerControllerB>(0))
                    {
                        if (player.actualClientId == targetId)
                        {
                            newDot.gameObject.name = player.playerUsername;
                            newDot.targetedPlayer = player;
                        }
                    }

                    dots.Add(newDot);
                }
                else if(localPlayer.actualClientId == targetId)
                {
                    // Target
                    dots.Add(new GameObject().AddComponent<SeekerDotVisuals>());

                    dots[0].gameObject.name = activatorPlayer.playerUsername;
                    dots[0].targetedPlayer = activatorPlayer;

                    localAbilityInstance.DisplayTip($"You're getting tracked by '{activatorPlayer.playerUsername}'", true);
                }
            }

            // Update Dot Position
            foreach (var dot in dots.ToArray())
            {
                if (dot.targetedPlayer.isPlayerDead)
                {
                    dots.Remove(dot);
                    GameObject.Destroy(dot.gameObject);
                }
                else
                {
                    dot.targetPosition = dot.targetedPlayer.transform.position + (Vector3.up * 1.5f);
                }
            }

            if(localPlayer.actualClientId == activatorId && dots.Count > 0)
            {
                localAbilityInstance.DisplayTip("Updating Tracking...", false);
            }
        }

        #endregion

        #region Stealth
        static void LongStealthServerEvent(AbilityBase ability, ulong activatorId)
        {
            PlayerControllerB activatedPlayer = RoundManagerPatch.GetPlayerWithClientId(activatorId);
            AbilityInstance activatedInstance = activatedPlayer.GetComponent<AbilityInstance>();

            if (activatedInstance.stealthActivated)
            {
                NetworkHandler.Instance.EventSendRpc(".moneyChanged", new(__int: ability.abilityCost, __string: "silent", __ulong: activatorId));
                ability.ActivateClient(activatorId, "Cancel");
                return;
            }

            GameNetworkManager.Instance.StartCoroutine(StealthServerEventCoroutine(ability, activatorId, true));
        }
        static void StealthServerEvent(AbilityBase ability, ulong activatorId)
        {
            PlayerControllerB activatedPlayer = RoundManagerPatch.GetPlayerWithClientId(activatorId);
            AbilityInstance activatedInstance = activatedPlayer.GetComponent<AbilityInstance>();

            if (activatedInstance.stealthActivated)
            {
                NetworkHandler.Instance.EventSendRpc(".moneyChanged", new(__int: ability.abilityCost, __string:"silent", __ulong:activatorId));
                ability.ActivateClient(activatorId, "Cancel");
                return;
            }

            GameNetworkManager.Instance.StartCoroutine(StealthServerEventCoroutine(ability, activatorId));
        }
        static IEnumerator StealthServerEventCoroutine(AbilityBase ability, ulong activatorId, bool forever = false)
        {
            ability.ActivateClient(activatorId, true.ToString());

            PlayerControllerB activatedPlayer = RoundManagerPatch.GetPlayerWithClientId(activatorId);

            if (forever)
            {
                while (!activatedPlayer.isPlayerDead && !StartOfRound.Instance.inShipPhase)
                {
                    yield return new WaitForSecondsRealtime(1);
                }
            }
            else
            {
                int timer = 30;
                while (timer > 0 && !activatedPlayer.isPlayerDead && !StartOfRound.Instance.inShipPhase)
                {
                    yield return new WaitForSecondsRealtime(1);
                    timer--;
                    if (timer == 5)
                    {
                        ability.ActivateClient(activatorId, "Warn");
                    }
                }
            }

            ability.ActivateClient(activatorId, false.ToString());
        }
        static void StealthClientEvent(AbilityBase ability, ulong activatorId, string extraMessage = null)
        {
            PlayerControllerB activatedPlayer = RoundManagerPatch.GetPlayerWithClientId(activatorId);
            AbilityInstance activatedInstance = activatedPlayer.GetComponent<AbilityInstance>();

            if (activatedInstance == null)
            {
                activatedInstance = activatedPlayer.gameObject.AddComponent<AbilityInstance>();
            }

            bool isLocalPlayer = activatedPlayer == GameNetworkManager.Instance.localPlayerController;

            if (extraMessage == "Cancel")
            {
                if (isLocalPlayer)
                    Abilities.FindAbilityByName(ability.abilityName).usedThisRound = false;

                return;
            } else if (extraMessage == "Warn")
            {
                if (isLocalPlayer)
                    activatedInstance.DisplayTip("Stealth: 5 Seconds left!", true);

                return;
            }

            activatedInstance.stealthActivated = bool.Parse(extraMessage);

            if (isLocalPlayer)
            {
                if (bool.Parse(extraMessage)) // Activate
                {
                    activatedInstance.DisplayTip("Stealth: Activated stelth!");
                }
                else
                {
                    activatedInstance.DisplayTip("Stealth: Ability expired!");
                }
            }
        }

        #endregion

        #region Invisibility
        static void InvisibilityServerEvent(AbilityBase ability, ulong activatorId)
        {
            GameNetworkManager.Instance.StartCoroutine(InvisibilityServerEventCoroutine(ability, activatorId));
        }
        static IEnumerator InvisibilityServerEventCoroutine(AbilityBase ability, ulong activatorId)
        {
            ability.ActivateClient(activatorId, true.ToString());

            PlayerControllerB activatedPlayer = RoundManagerPatch.GetPlayerWithClientId(activatorId);

            int timer = 30;
            while (timer > 0 && !StartOfRound.Instance.inShipPhase)
            {
                yield return new WaitForSecondsRealtime(1);
                timer--;
                if (timer == 5)
                {
                    ability.ActivateClient(activatorId, "Warn");
                }
            }

            ability.ActivateClient(activatorId, false.ToString());
        }
        static void InvisibilityClientEvent(AbilityBase ability, ulong activatorId, string extraMessage = null)
        {
            PlayerControllerB activatedPlayer = RoundManagerPatch.GetPlayerWithClientId(activatorId);

            AbilityInstance activatedInstance = activatedPlayer.GetComponent<AbilityInstance>();


            if (activatedInstance == null)
            {
                activatedInstance = activatedPlayer.gameObject.AddComponent<AbilityInstance>();
            }

            bool isLocalPlayerActivator = activatedPlayer == GameNetworkManager.Instance.localPlayerController;

            if (extraMessage == "Warn")
            {
                if (isLocalPlayerActivator)
                    activatedInstance.DisplayTip("Invisibility: 5 Seconds left!", true);

                return;
            }


            if (bool.Parse(extraMessage)) // Activate
            {
                activatedInstance.invisibilityActivated = true;

                foreach (var item in GameObject.FindObjectsOfType<GrabbableObject>())
                {
                    if (item.OwnerClientId == activatorId && item.mainObjectRenderer)
                    {
                        item.mainObjectRenderer.gameObject.SetActive(false);
                    }
                }

                Canvas canvas = activatedPlayer.gameObject.GetComponentInChildren<Canvas>();

                if (canvas)
                {
                    canvas.enabled = false;
                }

                if (isLocalPlayerActivator)
                {
                    activatedPlayer.thisPlayerModelArms.enabled = false;
                    activatedInstance.DisplayTip("Invisibility: Activated Invisibility!");
                }
                else
                {
                    foreach (var skinnedMesh in activatedPlayer.gameObject.GetComponentsInChildren<SkinnedMeshRenderer>())
                    {
                        skinnedMesh.enabled = false;
                    }
                    foreach (var mesh in activatedPlayer.gameObject.GetComponentsInChildren<MeshRenderer>())
                    {
                        mesh.enabled = false;
                    }
                }
            }
            else // Deactivate
            {
                activatedInstance.invisibilityActivated = false;
                                
                foreach (var item in GameObject.FindObjectsOfType<GrabbableObject>())
                {
                    if (item.OwnerClientId == activatorId && item.mainObjectRenderer)
                    {
                        item.mainObjectRenderer.gameObject.SetActive(true);
                    }
                }

                Canvas canvas = activatedPlayer.gameObject.GetComponentInChildren<Canvas>();

                if (canvas)
                {
                    canvas.enabled = true;
                }
                
                if (isLocalPlayerActivator)
                {
                    activatedPlayer.thisPlayerModelArms.enabled = true;
                    activatedInstance.DisplayTip("Invisibility: Ability expired!");
                }
                else
                {
                    foreach (var skinnedMesh in activatedPlayer.gameObject.GetComponentsInChildren<SkinnedMeshRenderer>())
                    {
                        skinnedMesh.enabled = true;
                    }
                    foreach (var mesh in activatedPlayer.gameObject.GetComponentsInChildren<MeshRenderer>())
                    {
                        if (mesh.gameObject.name == "PlayerPhysicsBox") continue;

                        mesh.enabled = true;
                    }

                    activatedPlayer.thisPlayerModelArms.enabled = false;
                }
            }
        }
        #endregion

        #endregion

        #region _FinderMethods_
        public static AbilityBase FindAbilityByName(string name, bool raw = false)
        {
            AbilityBase ability = null;

            foreach (var _ability in abilities)
            {
                if (_ability.abilityName.Equals(name, System.StringComparison.CurrentCultureIgnoreCase))
                {
                    ability = _ability;
                    break;
                }
            }

            if (ability == null) Debug.LogWarning($"FindAbilityByName:'{name}' Could not find ability!");
            else
            {
                if (!raw)
                {
                    AbilityConfig cfg = FindAbilityConfigByName(name);

                    if (cfg != null)
                    {
                        if (cfg.syncedWithHost || GameNetworkManager.Instance.isHostingGame)
                        {
                            ability = ApplyCfgToAbility(ability, cfg);
                        }
                    }
                }
            }

            return ability;
        }
        public static AbilityConfig FindAbilityConfigByName(string name, bool check = false)
        {
            AbilityConfig abilityCfg = null;

            foreach (var _ability in abilityConfigs)
            {
                if (_ability.abilityName.Equals(name.Trim(), System.StringComparison.CurrentCultureIgnoreCase))
                {
                    abilityCfg = _ability;
                    break;
                }
            }

            if (abilityCfg == null)
            {
                Debug.LogWarning($"FindAbilityConfigByName:'{name}' Could not find ability config!!");
                if (!GameNetworkManager.Instance.isHostingGame && GameNetworkManager.Instance?.localPlayerController != null && !check)
                {
                    // Client
                    Debug.LogMessage("Attempting request...");

                    NetworkHandler.Instance.EventSendRpc(".requestAbilityConfig", new(__ulong: GameNetworkManager.Instance.localPlayerController.actualClientId, __string: name));
                }
            }

            return abilityCfg;
        }
        public static bool AbilityExists(string name)
        {
            foreach (var _ability in abilities)
            {
                if (_ability.abilityName.Equals(name, System.StringComparison.CurrentCultureIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }
        public static bool AbilityConfigExists(string name)
        {
            foreach (var _ability in abilityConfigs)
            {
                if (_ability.abilityName.Equals(name, System.StringComparison.CurrentCultureIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }
        public static void LoadAbilityConfig(AbilityConfig cfg)
        {
            if (cfg == null) return;

            foreach (var item in abilityConfigs.ToArray())
            {
                if (item.abilityName.Equals(cfg.abilityName, System.StringComparison.CurrentCultureIgnoreCase))
                {
                    abilityConfigs.Remove(item);
                }
            }
            cfg.syncedWithHost = true;
            abilityConfigs.Add(cfg);
        }
        #endregion

        #region _OtherMethods_
        public const string CFGfNAME = "Abilities.Cfg";
        public static void AbilitiesToCfg()
        {
            abilityConfigs = new();
            foreach (var ability in abilities)
            {
                if (ability.abilityCategory != "HIDDEN")
                {
                    abilityConfigs.Add(AbilityToCfg(ability));
                }
            }
        }
        public static AbilityConfig AbilityToCfg(AbilityBase ability)
        {
            return new(ability.abilityName,
                                ability.abilityCost, ability.abilityDelay,
                                ability.oneTimeUse, ability.seekerAbility,
                                ability.hiderAbility, ability.requiresRoundActive, ability.requiresSeekerActive);
        }
        public static AbilityBase ApplyCfgToAbility(AbilityBase ability, AbilityConfig cfg)
        {
            ability.abilityCost = cfg.abilityCost;
            ability.abilityDelay = cfg.abilityDelay;
            ability.oneTimeUse = cfg.oneTimeUse;
            ability.seekerAbility = cfg.seekerAbility;
            ability.hiderAbility = cfg.hiderAbility;
            ability.requiresRoundActive = cfg.requiresRoundActive;
            ability.requiresSeekerActive = cfg.requiresSeekerActive;

            return ability;
        }
        public static string AbilityCfgToData(AbilityConfig aCfg, bool format = true)
        {
            string data = string.Empty;
            if (format)
            {
                data += aCfg.abilityName + " {\r\n" +
                        $"\t{nameof(aCfg.abilityCost)} = {aCfg.abilityCost};\r\n" +
                        $"\t{nameof(aCfg.seekerAbility)} = {aCfg.seekerAbility};\r\n" +
                        $"\t{nameof(aCfg.hiderAbility)} = {aCfg.hiderAbility};\r\n" +
                        $"\t{nameof(aCfg.requiresRoundActive)} = {aCfg.requiresRoundActive};\r\n" +
                        $"\t{nameof(aCfg.requiresSeekerActive)} = {aCfg.requiresSeekerActive};\r\n" +
                        $"\t{nameof(aCfg.abilityDelay)} = {aCfg.abilityDelay};\r\n" +
                        $"\t{nameof(aCfg.oneTimeUse)} = {aCfg.oneTimeUse};\r\n" + "}";
            }
            else
            {
                data += aCfg.abilityName + "{" +
                        $"{nameof(aCfg.abilityCost)}={aCfg.abilityCost};" +
                        $"{nameof(aCfg.seekerAbility)}={aCfg.seekerAbility};" +
                        $"{nameof(aCfg.hiderAbility)}={aCfg.hiderAbility};" +
                        $"{nameof(aCfg.requiresRoundActive)}={aCfg.requiresRoundActive};" +
                        $"{nameof(aCfg.requiresSeekerActive)}={aCfg.requiresSeekerActive};" +
                        $"{nameof(aCfg.abilityDelay)}={aCfg.abilityDelay};" +
                        $"{nameof(aCfg.oneTimeUse)}={aCfg.oneTimeUse};" + "}";
            }
            return data;
        }
        public static string AbilityCfgsToData(bool format = true)
        {
            if (abilityConfigs.Count <= 0) { Debug.LogError("Tried to ACfgToData but there is no ability config data!"); return null; }

            string data = string.Empty;

            foreach (var aCfg in abilityConfigs)
            {
                if (data != string.Empty && format)
                {
                    data += "\r\n";
                }

                data += AbilityCfgToData(aCfg, format);
            }

            return data;
        }
        public static AbilityConfig ADataToCfg(string d)
        {
            string[] s = d.Split("{");
            string aName = s[0].Trim();
            string data = s[1];

            AbilityConfig aCfg = AbilityToCfg(FindAbilityByName(aName, true));

            Debug.Log($"Reading Ability '{aName}'");

            foreach (var item in data.Replace("}", "").Split(";"))
            {
                if (string.IsNullOrEmpty(item)) continue;

                string name = item.Split("=")[0].Trim();
                string value = item.Split("=")[1].Trim();

                Debug.Log($"Reading Value '{name}' = '{value}'");

                switch (name)
                {
                    case nameof(aCfg.abilityCost):
                        aCfg.abilityCost = int.Parse(value);
                        break;
                    case nameof(aCfg.seekerAbility):
                        aCfg.seekerAbility = bool.Parse(value);
                        break;
                    case nameof(aCfg.hiderAbility):
                        aCfg.hiderAbility = bool.Parse(value);
                        break;
                    case nameof(aCfg.requiresRoundActive):
                        aCfg.requiresRoundActive = bool.Parse(value);
                        break;
                    case nameof(aCfg.requiresSeekerActive):
                        aCfg.requiresSeekerActive = bool.Parse(value);
                        break;
                    case nameof(aCfg.abilityDelay):
                        aCfg.abilityDelay = float.Parse(value);
                        break;
                    case nameof(aCfg.oneTimeUse):
                        aCfg.oneTimeUse = bool.Parse(value);
                        break;
                    default:
                        Debug.LogError($"Could not read {name}!");
                        break;
                }
            }
            return aCfg;
        }
        public static List<AbilityConfig> ADataToCfgs(string data)
        {
            List<AbilityConfig> newACfgs = new();

            Debug.LogWarning("_______ Cfg Input! \r\n" + data.Replace("\t", "").Replace("\r\n", ""));
            foreach (var aCfgS in data.Replace("\t", "").Replace("\r\n", "").Split('}'))
            {
                if (string.IsNullOrEmpty(aCfgS)) continue;

                var sCfData = ADataToCfg(aCfgS);

                newACfgs.Add(sCfData);
            }

            return newACfgs;
        }
        public static void ReadConfigFile()
        {
            if (GameNetworkManager.Instance.isHostingGame)
            {
                var dllFolderPath = Path.GetDirectoryName(Plugin.instance.Info.Location);
                var filePath = Path.Combine(dllFolderPath, CFGfNAME);

                if (File.Exists(filePath))
                {
                    string data = File.ReadAllText(filePath);

                    string version = data.Split("]")[0].Replace("[v", "");

                    Debug.LogMessage($"Found File! {version}");
                    if (version != Plugin.PLUGIN_VERSION)
                    {
                        Debug.LogError($"Config file version does not match the current version! cfg = 'v{version}' plugin = 'v{Plugin.PLUGIN_VERSION}' Making backup...");
                        File.Move(filePath, Path.Combine(dllFolderPath, "v" + version + " - " + CFGfNAME));
                        ReadConfigFile();
                        return;
                    }
                    else
                    {
                        abilityConfigs = ADataToCfgs(data.Split("]")[1]);
                    }
                }
                else
                {
                    AbilitiesToCfg();

                    string data = $"[v{Plugin.PLUGIN_VERSION}]\r\n" + AbilityCfgsToData();

                    Debug.LogWarning("______________ Got Data!: " + data);

                    File.WriteAllText(filePath, data);
                }
            }
            else
            {
                NetworkHandler.Instance.EventSendRpc(".requestAbilityConfig", new(__ulong:GameNetworkManager.Instance.localPlayerController.actualClientId));
            }
        }
        public static void WriteConfigFile()
        {
            if (GameNetworkManager.Instance.isHostingGame)
            {
                var dllFolderPath = Path.GetDirectoryName(Plugin.instance.Info.Location);
                var filePath = Path.Combine(dllFolderPath, CFGfNAME);

                File.WriteAllText(filePath, AbilityCfgsToData());
            }
            else
            {
                Debug.LogWarning("Canceled writing config file because is not host!");
            }
        }
        #endregion
    }
}
