using GameNetcodeStuff;
using HideAndSeek.Patches;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using Debug = Debugger.Debug;

namespace HideAndSeek.AbilityScripts
{
    public class AbilityInstance : MonoBehaviour
    {
        public static AbilityInstance localInstance;
        public int money = 0;
        public PlayerControllerB attachedPlayer;

        public AbilityUI abilityRadialMenu;
        public GameObject tutorialMenu;

        public Action<int> moneyUpdatedAction;

        // Long Ability Used
        public bool stealthActivated;
        public bool invisibilityActivated;

        InputAction sellAction = new("SellScrap");

        static GameObject abilityRadialMenuPrefab;
        static GameObject tutorialMenuPrefab;
        bool emoteModExits;

        bool isOwner = false;
        void Start()
        {
            attachedPlayer = gameObject.GetComponent<PlayerControllerB>();

            isOwner = (attachedPlayer == GameNetworkManager.Instance.localPlayerController);

            Debug.LogWarning($"New AbilityInstance Start(): " +
                $"Player = '{gameObject.name}' " +
                $"AttachedPlayer = '{attachedPlayer}' " +
                $"Local player id = '{GameNetworkManager.Instance.localPlayerController.actualClientId}' " +
                $"AttachedPlayer id = '{attachedPlayer.actualClientId}' " +
                $"Is owner = '{isOwner}'");

            if (!isOwner) return;

            Debug.LogWarning("Action Binding...");
            sellAction = InputConfigs.GetInputClass().SellKey;
            sellAction.performed += SellInputPressed;
            sellAction.canceled += SellInputCanceled;
            sellAction.Enable();

            // Create Tutorial UI
            if (tutorialMenuPrefab == null)
            {
                tutorialMenuPrefab = Plugin.tutorialRadialMenuBundle.LoadAsset<GameObject>("TutorialMenu");
            }
            // Create Radial UI
            if (abilityRadialMenuPrefab == null)
            {
                abilityRadialMenuPrefab = Plugin.abilityRadialMenuBundle.LoadAsset<GameObject>("AbilityRadialMenu");
                abilityRadialMenuPrefab.AddComponent<AbilityUI>();
            }

            GameObject canvas = GameObject.Find("/Systems/UI/Canvas");

            if (!canvas)
            {
                Debug.LogError("Could not find canvas at path '/Systems/UI/Canvas'! Cannot create abilty ui!");
            }
            else
            {
                tutorialMenu = Instantiate(tutorialMenuPrefab, canvas.transform);
                tutorialMenu.transform.localPosition = Vector3.zero;
                tutorialMenu.gameObject.name = "[Hide and Seek] TutorialMenu";
                tutorialMenu.gameObject.SetActive(false);
                Debug.LogMessage("[Ability Instance] Created Tutorial UI!");

                abilityRadialMenu = Instantiate(abilityRadialMenuPrefab, canvas.transform).GetComponent<AbilityUI>();
                abilityRadialMenu.transform.localPosition = Vector3.zero;
                abilityRadialMenu.gameObject.name = "AbilityRadialMenu";
                abilityRadialMenu.gameObject.SetActive(false);
                Debug.LogMessage("[Ability Instance] Created Ability UI!");

            }

            emoteModExits = canvas.transform.FindChild("EmotesRadialMenu")?.gameObject != null;

            localInstance = this;
        }

        bool roundStarted = false;
        void Update()
        {
            if (!roundStarted && !StartOfRound.Instance.inShipPhase && Plugin.seekers.Count > 0)
            {
                // Round started
                roundStarted = true;
            }
            else if (roundStarted && StartOfRound.Instance.inShipPhase)
            {
                // Round ended
                ResetAbilityUse();
                roundStarted = false;
            }
            
            Vector2 movementInput = IngamePlayerSettings.Instance.playerInput.actions.FindAction("Move", false).ReadValue<Vector2>();

            if (movementInput.magnitude > 0.2f)
            {
                if (sellingScrap)
                {
                    SellMoveCanceled();
                }
            }
        }

        // Money/Sell Management
        bool sellingScrap = false;
        public void SellInputPressed(InputAction.CallbackContext context = new())
        {
            Vector2 movementInput = IngamePlayerSettings.Instance.playerInput.actions.FindAction("Move", false).ReadValue<Vector2>();

            GrabbableObject item = attachedPlayer.ItemSlots[attachedPlayer.currentItemSlot];
            if (!item) return;
            if (item.scrapValue == 0) return;

            if (movementInput.magnitude > 0.2f)
            {
                SellMoveCanceled();
                return;
            }

            sellingScrap = true;
            Invoke(nameof(SellSelectedScrap), 3f);
            DisplayTip("Selling Scrap...");
        }
        public void SellInputCanceled(InputAction.CallbackContext context = new())
        {
            if (!sellingScrap) return;
            GrabbableObject item = attachedPlayer.ItemSlots[attachedPlayer.currentItemSlot];
            if (!item) return;
            if (item.scrapValue == 0) return;

            sellingScrap = false;
            CancelInvoke(nameof(SellSelectedScrap));
            DisplayTip("Keep holding to sell scrap!", true);
        }
        void SellMoveCanceled()
        {
            sellingScrap = false;
            CancelInvoke(nameof(SellSelectedScrap));
            DisplayTip("Can't move and sell at the same time!", true);
        }

        public void ResetAbilityUse()
        {
            foreach (var ability in Abilities.abilities)
            {
                ability.lastUsed = 0f;
                ability.usedThisRound = false;
            }
        }
        public void SellSelectedScrap()
        {
            if (!Config.abilitiesEnabled.Value) return;
            sellingScrap = false;
            if (attachedPlayer.isPlayerDead || !attachedPlayer.isPlayerControlled) { Debug.LogWarning($"Ur ded bruh, ca't du dat. Dead:{attachedPlayer.isPlayerDead} Controlled:{attachedPlayer.isPlayerControlled}"); return; }

            GrabbableObject item = attachedPlayer.ItemSlots[attachedPlayer.currentItemSlot];
            Debug.Log($"[Client] Attempting to sell selected object... '{item}'");
            if (item)
            {
                int value = item.scrapValue;
                if (value > 0) // Might need other checks
                {
                    Debug.Log($"[Client] Passed local checks! scrapvalue: '{item.scrapValue}'");

                    abilityRadialMenu.sellControlTip.alpha = Mathf.Max(abilityRadialMenu.sellControlTip.alpha - 0.25f, 0);

                    NetworkHandler.Instance.EventSendRpc(".sellCurrentItem", new MessageProperties(__ulong:attachedPlayer.actualClientId));
                }
                else
                {
                    Debug.Log($"[Client] Could not pass value check! scrapvalue: '{item.scrapValue}'");
                }
            }
        }

        public void ServerMoneyUpdated(int valueChange, bool set = false, bool silent = false)
        {
            if (!Config.abilitiesEnabled.Value) return;
            Debug.LogWarning($"Got ServerMoneyUpdated({valueChange})!");

            money += valueChange;

            if (set)
                money = valueChange;

            moneyUpdatedAction?.Invoke(money);

            if(!silent)
                StartCoroutine(SendInfoMessage(valueChange));
        }

        public IEnumerator SendInfoMessage(int valueChange)
        {
            int tries = 10;
            while (attachedPlayer == null && tries > 0)
            {
                tries--;
                yield return new WaitForEndOfFrame();
            }

            if (GameNetworkManager.Instance.isHostingGame)
                if (valueChange > 0)
                {
                    NetworkHandler.Instance.EventSendRpc(".tip", new MessageProperties(__string: $"Sold item for '{valueChange}', Current Credit Balance: '{money}'", __int: -1, __ulong: attachedPlayer.actualClientId)); // _int = -1 (Spesific Message)
                }
                else if (valueChange < 0)
                {
                    NetworkHandler.Instance.EventSendRpc(".tip", new MessageProperties(__string: $"Bought item for '{MathF.Abs(valueChange)}', Current Credit Balance: '{money}'", __int: -1, __ulong: attachedPlayer.actualClientId)); // _int = -1 (Spesific Message)
                }
        }
        
        public void DisplayTip(string s, bool warn = false)
        {
            HUDManager.Instance.DisplayTip("Hide And Seek", s, warn);
        }

        public void BuyAbility(AbilityBase ability)
        {
            if (!Config.abilitiesEnabled.Value) return;
            Debug.Log($"[Client] Attempting to buy seleted ability... '{ability.abilityName}'");
            if (ability != null)
            {
                // Valid check
                bool isSeeker = Plugin.seekers.Contains(attachedPlayer);

                if (ability.abilityCost > money)
                {
                    Debug.Log($"[Client] Could not pass value check! AbilityCost: '{ability.abilityCost}'");
                    DisplayTip($"Cannot afford this ability! You need '{ability.abilityCost - money}' more credits.", true);
                    return;
                }

                if (ability.usedThisRound && ability.oneTimeUse) // Can only use once
                {
                    Debug.Log($"[Client] Ability already used! AbilityName: '{ability.abilityName}' TimeLeft: '{ability.abilityDelay - (Time.time - ability.lastUsed)}'");
                    DisplayTip($"You can only use {ability.abilityName} once a round!", true);
                    return;
                }

                if (Time.time - ability.lastUsed <= ability.abilityDelay) // Time since last used < the time delay
                {
                    Debug.Log($"[Client] Ability on cooldown! AbilityName: '{ability.abilityName}' TimeLeft: '{ability.abilityDelay - (Time.time - ability.lastUsed)}'");
                    DisplayTip($"Ability on cooldown! Seconds left: '{(int)(ability.abilityDelay - (Time.time - ability.lastUsed))+1}'", true);
                    return;
                }

                if (ability.requiresRoundActive && !StartOfRound.Instance.shipHasLanded)
                {
                    Debug.Log($"[Client] Could not pass requiersRoundActive check! AbilityName: '{ability.abilityName}'");
                    DisplayTip("Round has not started yet! Can't use this ability.", true);
                    return;
                }

                if (ability.requiresSeekerActive && TimeOfDay.Instance.currentDayTime <= Config.timeSeekerIsReleased.Value)
                {
                    Debug.Log($"[Client] Could not pass requiersRoundActive check! AbilityName: '{ability.abilityName}'");
                    DisplayTip("Seeker not active yet! Can't use this ability.", true);
                    return;
                }

                if (!(ability.seekerAbility && isSeeker || ability.hiderAbility && !isSeeker)) // If neither are true
                {
                    Debug.Log($"[Client] Could not pass proper ability user check! AbilityName: '{ability.abilityName}'");
                    if(isSeeker)
                    {
                        DisplayTip($"This ability is only for hiders!!");
                    }
                    else
                    {
                        DisplayTip($"This ability is only for seekers!!");
                    }
                    return;
                }

                // Passed Checks

                Debug.Log($"[Client] Passed local checks! AbilityCost: '{ability.abilityCost}'");

                ability.lastUsed = Time.time;
                ability.usedThisRound = true;
                ability.timesUsed++;
                NetworkHandler.Instance.EventSendRpc(".buyAbility", new MessageProperties(__string: ability.abilityName, __ulong: attachedPlayer.actualClientId));
            }
            else
            {
                Debug.LogWarning("[Client] Tried to buy a null ability!");
            }
        }
    }
}