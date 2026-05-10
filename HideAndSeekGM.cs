using GameNetcodeStuff;
using HarmonyLib;
using HideAndSeek.AbilityScripts;
using HideAndSeek.AudioScripts;
using HideAndSeek.Patches;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using Debug = Debugger.Debug;

namespace HideAndSeek;

public class HideAndSeekGM : MonoBehaviour
{
    public static HideAndSeekGM instance;
    public RoundManager roundManager;

    // States
    public int playersTeleported;
    public int playersAlive;
    public ulong leverLastFlippedBy = 999;
    public ulong lastSeekerId = 10001;
    public List<(ulong playerId, Vector3 position)> itemSpawnPositions = [];
    public List<ulong> pastSeekers = [];

    public System.Action<ulong> playerRevived;
    public System.Action roundStarted;
    public System.Action backInOrbit;
    public System.Action shipLeaving;

    public List<PlayerControllerB> seekers = new();
    public List<PlayerControllerB> zombies = new();
    List<PlayerControllerB> rewardedPlayers = new();

    public bool levelLoading = false;
    public SelectableLevel currentLevel;

    bool seekersWon = false;

    public static void Init()
    {
        instance = new GameObject().AddComponent<HideAndSeekGM>();
        instance.name = "HideAndSeekGM";
    }

    private void Start()
    {
        // Ability Sync
        if (!AudioManager.LoadedAudio)
        {
            Debug.LogWarning($"Loading AudioManager Audio!");
            StartCoroutine(AudioManager.LoadAudioCoroutine());
        }

        Debug.Log($"StartPatch, AbilityManager: Enabled = {Config.abilitiesEnabled.Value}");

        if (!Config.abilitiesEnabled.Value) { return; }

        StartCoroutine(AbilityManager.ConnectStart());

        backInOrbit += OnReturnToOrbit;
    }

    private void Update()
    {
        if (!GameNetworkManager.Instance.localPlayerController) return; // No localPlayer, Exiting...

        bool isHost = GameNetworkManager.Instance.localPlayerController.IsServer;
        if (!isHost) return;

        int alivePlayerCount = StartOfRound.Instance.livingPlayers;

        if (playersAlive < alivePlayerCount)
        {
            Debug.LogWarning("Player count reset to: " + alivePlayerCount);
            playersAlive = alivePlayerCount;
        }
        else if (playersAlive > alivePlayerCount)
        {
            // Player Died!
            Debug.LogWarning("Player died! New Count: " + alivePlayerCount);
            playersAlive = alivePlayerCount;
            if (TimeOfDay.Instance.currentDayTime != 0)
            {
                UpdateGameState("Player Died");
            }
        }
    }


    private void OnReturnToOrbit()
    {
        Debug.LogError("[HideAndSeekGM] OnRoundEnd()");
        if (seekers.Count > 0)
        {
            foreach (var player in HideAndSeekGM.GetAllConnectedPlayers("Award On Round End"))
            {
                if (!seekers.Contains(player) && !seekersWon)
                {
                    Debug.LogMessage("Award Hider");
                    NetworkHandler.Instance.EventSendRpc(".moneyChanged", new(__ulong: player.actualClientId, __int: 250, __string: "silent")); // Give Hiders Money
                    NetworkHandler.Instance.EventSendRpc(".tip", new(__ulong: player.actualClientId, __string: "You won, and got a reward!", __int: -1));
                }
                else if (seekers.Contains(player) && seekersWon)
                {
                    Debug.LogMessage("Award Seeker");
                    NetworkHandler.Instance.EventSendRpc(".moneyChanged", new(__ulong: player.actualClientId, __int: 400, __string: "silent")); // Give Seekers Money
                    NetworkHandler.Instance.EventSendRpc(".tip", new(__ulong: player.actualClientId, __string: "You won, and got a reward!", __int: -1));
                }
            }
        }
    }

    public void OnLevelLoaded(SelectableLevel newLevel)
    {
        Debug.LogMessage($"[LoadLevelPatch] LoadLevel Start! ------------------------------------- ");
        levelLoading = true;

        roundManager = GameObject.FindFirstObjectByType<RoundManager>();
        rewardedPlayers.Clear();
        seekers.Clear();
        zombies.Clear();
        playersTeleported = 0;

        NetworkEvents.TeleportPlayer();

        seekersWon = false;
        currentLevel = newLevel;
        bool isHost = GameNetworkManager.Instance.isHostingGame;
        if (!isHost) return;

        if (!Abilities.turretPrefab || !Abilities.landminePrefab)
        {
            foreach (var item in currentLevel.spawnableMapObjects)
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

        var disabledEntities = Config.disabledEntities.Value.ToLower().Split(',').ToList();

        List<SpawnableEnemyWithRarity> allEnemies =
        [
            .. newLevel.DaytimeEnemies,
            .. newLevel.OutsideEnemies,
            .. newLevel.Enemies,
        ];

        foreach (var enemy in allEnemies)
        {
            Debug.Log($"[{enemy.enemyType.enemyName}] Checking Enemy");

            if (Config.disableAllEntities.Value)
            {
                Debug.Log($"[{enemy.enemyType.enemyName}] DisableAllIndoorEntities is true!");
                enemy.rarity = 0;
                continue;
            }

            if (disabledEntities.Contains(enemy.enemyType.enemyName.ToLower()))
            {
                Debug.Log($"[{enemy.enemyType.enemyName}] Entity found in blacklist! Removing...");
                enemy.rarity = 0;
                continue;
            }

            Debug.Log($"{enemy.enemyType.enemyName}.Rarity = {enemy.rarity}");
        }

        // -- Hide And Seek --

        // Init Players
        List<PlayerControllerB> players = HideAndSeekGM.GetAllConnectedPlayers("Round Start");

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

            seekers.Add(player);
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

        NetworkHandler.Instance.EventSendRpc(".levelLoaded");

        // TMP Print Items
        /*

        Debugger.LogMessage("Logging Items!");
        var items = Resources.FindObjectsOfTypeAll<Item>();
        foreach (var item in items) 
        {
            Debugger.LogMessage("ItemFound! = " + item.itemName);
        }*/
    }
    public PlayerControllerB PickRandomSeeker()
    {
        string pickType = Config.seekerChooseBehavior.Value.ToLower().Trim().Replace(" ", "");
        if (seekers.Count > 0)
        {
            pickType = Config.extraSeekerChooseBehavior.Value.ToLower().Trim().Replace(" ", "");
        }
        ulong seekerPlayerId = 10001;

        Debug.Log($"Random Type ['{pickType}']");
        if (pickType == "nodouble")
        {
            List<ulong> currentPool = new();
            foreach (var player in HideAndSeekGM.GetAllConnectedPlayers("Chosing Seeker, (NoDouble)"))
            {
                if (player.actualClientId != lastSeekerId && !seekers.Contains(player))
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

            // Update Pool
            foreach (var player in HideAndSeekGM.GetAllConnectedPlayers("Chosing Seeker (Turns)"))
            {
                if (!pastSeekers.Contains(player.actualClientId) && !seekers.Contains(player))
                {
                    currentPool.Add(player.actualClientId);
                }
            }

            if (currentPool.Count <= 0)
            {
                pastSeekers.Clear();
                foreach (var player in HideAndSeekGM.GetAllConnectedPlayers("Chosing Seeker (Turns Exausted)"))
                {
                    if (!seekers.Contains(player))
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
        else if (pickType == "closest" && seekers.Count > 0)
        {
            float closestDistance = float.PositiveInfinity;

            foreach (var player in HideAndSeekGM.GetAllConnectedPlayers("Chosing Seeker (Closest)"))
            {
                if (player.isPlayerControlled && !seekers.Contains(player))
                {
                    if ((player.transform.position - seekers[0].transform.position).magnitude < closestDistance)
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

            foreach (var player in HideAndSeekGM.GetAllConnectedPlayers("Chosing Seeker (Random)"))
            {
                if (!seekers.Contains(player))
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
    public PlayerControllerB GetPlayerWithClientId(ulong playerId) // TODO: Remove bloat and find real fix to ID issue
    {
        PlayerControllerB playerController = null;

        foreach (var player in GameObject.FindObjectsByType<PlayerControllerB>(0))
        {
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
                    Debug.LogError($"'{player.playerUsername}' found with an incorrect actualClientId! ------------------------");
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
    
    public (int aliveHiders, int aliveSeekers, int hidersObjectiveCompleted, int aliveZombies) GetAlivePlayerCount()
    {
        int aliveHidersCount = 0;
        int aliveSeekersCount = 0;
        int hidersObjectiveCompleted = 0;
        int aliveZombieCount = 0;

        foreach (var player in HideAndSeekGM.GetAllConnectedPlayers("Get Alive Player Count"))
        {
            if (!player.isPlayerDead && player.gameObject.tag != "Decoy") // Maybe this is detecting the inactive stored players???
            {
                if (zombies.Contains(player))
                {
                    // Alive Zombie
                    aliveZombieCount++;
                }
                else if (seekers.Contains(player))
                {
                    // Alive Seeker
                    aliveSeekersCount++;
                }
                else if (ObjectivesManager.instance.PlayerReachedObjective(player))
                {
                    // Hiders With Objective Completed
                    hidersObjectiveCompleted++;
                }
                else
                {
                    // Alive Hider
                    aliveHidersCount++;
                }
            }
        }

        return (aliveHidersCount, aliveSeekersCount, hidersObjectiveCompleted, aliveSeekersCount);
    }

    public void UpdateGameState(string reason)
    {
        Debug.LogError($"UpdateGameState being called because '{reason}'");

        var (aliveHiders, aliveSeekers, hidersObjectiveCompleted, aliveZombies) = GetAlivePlayerCount();

        if (aliveSeekers <= 0) // All seekers dead?
        {
            NetworkHandler.Instance.EventSendRpc(".tip", new MessageProperties() { _string = "Seeker Died; Hiders Win!", _bool = true });

            if (StartOfRound.Instance.shipHasLanded)
            {
                EndRound();

                foreach (var player in HideAndSeekGM.GetAllConnectedPlayers("Reward Hiders"))
                {
                    if (!player.isPlayerDead && player.isPlayerControlled && !seekers.Contains(player) && !zombies.Contains(player))
                    {
                        // Give hiders rewards for "killing" seeker

                        int reward = Mathf.RoundToInt((1080 - Config.timeSeekerIsReleased.Value) * 12 / 60); // Total Reward

                        NetworkHandler.Instance.EventSendRpc(".moneyChanged", new(__ulong: player.actualClientId, __int: reward, __string: "silent"));
                    }
                }
            }

            return;
        }
        // All hiders dead?
        else if (aliveHiders <= 0 && GameNetworkManager.Instance.connectedPlayers != 1) // Last part is for testing in solo, so I don't get immediately kicked out
        {
            if (hidersObjectiveCompleted > 0) // Someone reached objective
            {
                NetworkHandler.Instance.EventSendRpc(".tip", new MessageProperties() { _string = "Objective Reached; Hiders Win!", _bool = true });

                EndRound();

                foreach (var player in HideAndSeekGM.GetAllConnectedPlayers("Reward Objective Reachers"))
                {
                    if (!player.isPlayerDead && player.isPlayerControlled && !seekers.Contains(player) && !zombies.Contains(player))
                    {
                        // Last alive hiders

                        int reward = Mathf.RoundToInt((1080 - Config.timeSeekerIsReleased.Value) * 12 / 60); // Total Reward

                        NetworkHandler.Instance.EventSendRpc(".moneyChanged", new(__ulong: player.actualClientId, __int: reward, __string: "silent")); // Give Hider Money
                    }
                }
            }
            else // All properly dead
            {
                if (!seekersWon)
                {
                    seekersWon = true;
                }

                NetworkHandler.Instance.EventSendRpc(".tip", new MessageProperties() { _string = "Seeker Won!", _bool = true });

                EndRound();
            }
        }
        else if (aliveHiders > 0) // Hiders still alive
        {
            // Advance time with last hider
            if (aliveHiders == 1 && Config.shipLeaveEarly.Value && Config.timeWhenLastHider.Value > TimeOfDay.Instance.currentDayTime)
                NetworkHandler.Instance.EventSendRpc(".setDayTime", new(__float: Config.timeWhenLastHider.Value));

            // Brodcast the amount of hiders remaining
            NetworkHandler.Instance.EventSendRpc(".tip", new(__string: $"{aliveHiders} Hiders Remain..."));
        }

        // Revive and reward players who just died
        foreach (var player in HideAndSeekGM.GetAllConnectedPlayers("Reward Players Who Just Died"))
        {
            if (!player.isPlayerDead) continue;
            // Player must be dead

            if (zombies.Contains(player)) continue;
            if (seekers.Contains(player)) continue;
            // Player must be hider

            if (rewardedPlayers.Contains(player)) continue;
            // Player must be not checked yet

            rewardedPlayers.Add(player);

            int reward = Mathf.RoundToInt((TimeOfDay.Instance.currentDayTime - Config.timeSeekerIsReleased.Value) * 12 / 60);

            if (reward < 0) reward = 0;

            // Reward hider for surviving as long as they have
            Debug.LogError($"Hider {player.playerUsername} recived a reward of {reward}, survived for = {TimeOfDay.Instance.currentDayTime - Config.timeSeekerIsReleased.Value}");
            NetworkHandler.Instance.EventSendRpc(".moneyChanged", new(__ulong: player.actualClientId, __int: reward, __string: "silent")); // Give Hider Money

            // Reward all seekers for killing a hider
            foreach (var seeker in seekers)
            {
                NetworkHandler.Instance.EventSendRpc(".moneyChanged", new(__ulong: seeker.actualClientId, __int: 50, __string: "silent")); // Give Seeker Money
            }

            // Revive if zombies allowed
            if (aliveHiders > 0 && Config.deadHidersRespawn.Value)
                PatchHelper.ReviveAfterWaitAndCallRpc(player, Config.zombieSpawnDelay.Value);
        }
    }

    public void EndRound()
    {
        StartMatchLever lever = GameObject.FindAnyObjectByType<StartMatchLever>();

        lever.EndGame();
        lever.LeverAnimation();

        if (GameNetworkManager.Instance.localPlayerController.IsServer) NetworkHandler.Instance.EventSendRpc(".roundEnded");
    }

    public IEnumerator GivePlayersItems()
    {
        List<PlayerControllerB> players = HideAndSeekGM.GetAllConnectedPlayers("GivePlayersItems");

        Debug.LogMessage("Game Has Started = " + GameNetworkManager.Instance.gameHasStarted);

        while (itemSpawnPositions.Count == 0 || playersTeleported == 0 || !StartOfRound.Instance.shipHasLanded)
        {
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
            if (seekers.Contains(player))
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
    public void SpawnNewItem(string itemName, PlayerControllerB player, bool forceSamePosition = false)
    {
        RoundManager.Instance.StartCoroutine(SpawnNewItemCoroutine(itemName, player, forceSamePosition));
    }
    public IEnumerator SpawnNewItemCoroutine(string itemName, PlayerControllerB player, bool forceSamePosition = false)
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

    /// <summary>
    /// Stupid way to get the "real" id of a player, because I had issues with ids in the past
    /// </summary>
    static ulong GetFRFRId(PlayerControllerB player)
    {
        string playerIDString = player.name.Replace("Player", "").Replace(" ", "").Replace("(", "").Replace(")", "");

        if (playerIDString == "")
        {
            return 0;
        }

        return ulong.Parse(playerIDString);
    }
    public bool IsRoundActive()
    {
        List<PlayerControllerB> players = HideAndSeekGM.GetAllConnectedPlayers("Is Round Active");

        return playersTeleported >= players.Count && StartOfRound.Instance.shipHasLanded;
    }

    public void MakeZombie(PlayerControllerB player)
    {
        if (zombies.Contains(player)) return;

        zombies.Add(player);

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
            Debug.LogError("Giving zombie items!");
            StartCoroutine(GiveZombieItems(player));
        }
        player.usernameBillboardText.color = Config.zombieNameColor.Value;
        playerRevived?.Invoke(player.actualClientId);
    }
    public IEnumerator GiveZombieItems(PlayerControllerB player)
    {
        if (!string.IsNullOrEmpty(Config.zombieItemSlot1.Value))
        {
            yield return SpawnNewItemCoroutine(Config.zombieItemSlot1.Value, player);
        }
        if (!string.IsNullOrEmpty(Config.zombieItemSlot2.Value))
        {
            yield return SpawnNewItemCoroutine(Config.zombieItemSlot2.Value, player);
        }
        if (!string.IsNullOrEmpty(Config.zombieItemSlot3.Value))
        {
            yield return SpawnNewItemCoroutine(Config.zombieItemSlot3.Value, player);
        }
        if (!string.IsNullOrEmpty(Config.zombieItemSlot4.Value))
        {
            yield return SpawnNewItemCoroutine(Config.zombieItemSlot4.Value, player);
        }
    }

    public static List<PlayerControllerB> GetAllConnectedPlayers(string reason)
    {
        Debug.LogError($"[HideAndSeekGM] GetAllConnectedPlayers('{reason}')");
        List<PlayerControllerB> result = [];

        foreach (var player in FindObjectsByType<PlayerControllerB>(0))
        {
            Debug.LogMessage("Looping Through Player: " + player);
            Debug.LogMessage($" player.isPlayerControlled - {player.isPlayerControlled}");
            Debug.LogMessage($" player.isPlayerDead - {player.isPlayerDead}");
            if (player.isPlayerControlled || player.isPlayerDead) result.Add(player);
        }

        return result;
    }
}