using GameNetcodeStuff;
using HarmonyLib;
using HideAndSeek.AudioScripts;
using HideAndSeek.Patches;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Debug = Debugger.Debug;

namespace HideAndSeek.AbilityScripts
{
    public class AbilityUI : MonoBehaviour
    {
        // UI References
        TextMeshProUGUI creditUI;
        TextMeshProUGUI titleDescriptionUI;
        TextMeshProUGUI titleUI;
        TextMeshProUGUI descriptionUI;
        TextMeshProUGUI versionUI;
        TextMeshProUGUI pageUI;
        TextMeshProUGUI costUI;
        TextMeshProUGUI menuControlTip;
        internal TextMeshProUGUI sellControlTip;

        Image abilitySpriteUI;
        Image fadeSpriteUI;
        RectTransform fadeCooldownUI;

        Button groupTemplateUI;
        Dictionary<string, Button> groupButtons;

        // Ability Lists
        List<string> selectedAbilities;
        int currentIndex = 0;

        List<string> allAbilities;
        Dictionary<string, List<string>> abilityCategories;

        AbilityBase selectedAbility;
        AbilityInstance attachedAbilityInstance;
        PlayerControllerB attachedPlayer;

        // Inputs
        List<InputAction> inputs = new();

        // Other
        bool initalized = false;
        const string FAVORITES = "Favorites";
        const string FAVORITES_FILE_NAME = "favorites.list";
        const string APPDATA_FOLDER_NAME = "LethalCompany.HideAndSeek";
        void Awake()
        {
            Debug.Log("AbilityUI Awake(): Initalizing References!");

            StartCoroutine(Initalize());
        }

        IEnumerator Initalize()
        {
            bool error = false;

            // Wait for initalization of other scripts
            while (GameNetworkManager.Instance?.localPlayerController == null)
            {
                yield return new WaitForEndOfFrame();
            }

            #region UIReference

            attachedPlayer = GameNetworkManager.Instance.localPlayerController;

            attachedAbilityInstance = attachedPlayer.GetComponent<AbilityInstance>();

            creditUI = GameObject.Find("MoneyText")?.GetComponent<TextMeshProUGUI>();
            if (creditUI == null)
            {
                Debug.LogError("Could not find creditUI!");
                error = true;
            }

            titleUI = GameObject.Find("RadialUI/Title")?.GetComponent<TextMeshProUGUI>();
            if (titleUI == null)
            {
                Debug.LogError("Could not find titleUI!");
                error = true;
            }

            titleDescriptionUI = GameObject.Find("Info/Title")?.GetComponent<TextMeshProUGUI>();
            if (titleDescriptionUI == null)
            {
                Debug.LogError("Could not find titleDescriptionUI!");
                error = true;
            }

            descriptionUI = GameObject.Find("Info/Description")?.GetComponent<TextMeshProUGUI>();
            if (descriptionUI == null)
            {
                Debug.LogError("Could not find descriptionUI!");
                error = true;
            }

            versionUI = GameObject.Find("Info/Version")?.GetComponent<TextMeshProUGUI>();
            if (versionUI == null)
            {
                Debug.LogError("Could not find versionUI!");
                error = true;
            }

            pageUI = GameObject.Find("RadialUI/Page")?.GetComponent<TextMeshProUGUI>();
            if (pageUI == null)
            {
                Debug.LogError("Could not find pageUI!");
                error = true;
            }

            costUI = GameObject.Find("RadialUI/Cost")?.GetComponent<TextMeshProUGUI>();
            if (costUI == null)
            {
                Debug.LogError("Could not find costUI!");
                error = true;
            }

            abilitySpriteUI = GameObject.Find("RadialUI/Sprite")?.GetComponent<Image>();
            if (abilitySpriteUI == null)
            {
                Debug.LogError("Could not find abilitySpriteUI!");
                error = true;
            }

            fadeSpriteUI = GameObject.Find("RadialUI/FadeEffect")?.GetComponent<Image>();
            if (fadeSpriteUI == null)
            {
                Debug.LogError("Could not find FadeEffect!");
                error = true;
            }

            fadeCooldownUI = GameObject.Find("RadialUI/Fade")?.GetComponent<RectTransform>();
            if (fadeCooldownUI == null)
            {
                Debug.LogError("Could not find Fade!");
                error = true;
            }

            groupTemplateUI = GameObject.Find("Groups/Template")?.GetComponent<Button>(); // Make unique groups and implement
            if (groupTemplateUI == null)
            {
                Debug.LogError("Could not find Groups/Template!");
                error = true;
            }
            else
            {
                groupTemplateUI.gameObject.SetActive(false);
            }

            TextMeshProUGUI controlTip = GameObject.Find("Systems/UI/Canvas/IngamePlayerHUD/TopRightCorner/ControlTip1")?.GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI otherControlTip = GameObject.Find("Systems/UI/Canvas/IngamePlayerHUD/TopRightCorner/ControlTip2")?.GetComponent<TextMeshProUGUI>();
            if (controlTip == null)
            {
                Debug.LogError("Could not find ControlTip1!");
            }
            else
            {
                menuControlTip = Instantiate(controlTip);
                menuControlTip.transform.parent = controlTip.transform.parent;
                menuControlTip.text = $"Ability Menu: [{Config.abilityMenuKeyBind.Value.Split(">")[1].Replace("/", "").ToUpper()}]";
                menuControlTip.transform.localScale = Vector3.one;
                menuControlTip.transform.position = controlTip.transform.position + (Vector3.down * (controlTip.transform.position.y - otherControlTip.transform.position.y) * 4);

                sellControlTip = Instantiate(controlTip);
                sellControlTip.transform.parent = controlTip.transform.parent;
                sellControlTip.text = $"Sell Scrap: [{Config.sellKeyBind.Value.Split(">")[1].Replace("/", "").ToUpper()}]";
                sellControlTip.transform.localScale = Vector3.one;
                sellControlTip.transform.position = controlTip.transform.position + (Vector3.down * (controlTip.transform.position.y - otherControlTip.transform.position.y) * 5);
            }

            #endregion

            #region Inputs

            InputAction abilityMenu = new("Menu", InputActionType.Button, Config.abilityMenuKeyBind.Value);
            abilityMenu.performed += ToggleAbilityUIInput;
            abilityMenu.Enable();
            inputs.Add(abilityMenu);

            InputAction exit = new("Exit", InputActionType.Button, "<Keyboard>/escape");
            exit.performed += DisableAbilityUIInput;
            exit.Enable();
            inputs.Add(exit);

            InputAction scroll = new InputAction("Next", InputActionType.Value, "<Mouse>/scroll/y");
            scroll.performed += ScrollInput;
            scroll.Enable();
            inputs.Add(scroll);

            InputAction clickLeft = new InputAction("Left", InputActionType.Value, "<Keyboard>/leftArrow");
            clickLeft.performed += LeftInput;
            clickLeft.Enable();
            inputs.Add(clickLeft);

            InputAction clickRight = new InputAction("Left", InputActionType.Value, "<Keyboard>/rightArrow");
            clickRight.performed += RightInput;
            clickRight.Enable();
            inputs.Add(clickRight);

            InputAction middleClick = new InputAction("Favorite", InputActionType.Button, "<Mouse>/middleButton");
            middleClick.performed += FavoriteInput;
            middleClick.Enable();
            inputs.Add(middleClick);

            Button buyButton = GameObject.Find("RadialUI")?.GetComponent<Button>();
            if (buyButton == null)
            {
                Debug.LogError("Could not find RadialUI!");
            }
            else
            {
                buyButton.onClick.RemoveAllListeners();
                buyButton.onClick.AddListener(BuyInput);
            }

            #endregion

            #region Abilities

            allAbilities = new();

            abilityCategories = new();

            List<string> favorites = new();

            // Read favorites file

            favorites = GetFavorites();

            abilityCategories.Add(FAVORITES, favorites);

            foreach (var ability in Abilities.abilities)
            {
                if (ability.abilityCategory == "HIDDEN") continue;

                allAbilities.Add(ability.abilityName);

                if (!abilityCategories.ContainsKey(ability.abilityCategory))
                {
                    abilityCategories.Add(ability.abilityCategory, new());
                }
                abilityCategories[ability.abilityCategory].Add(ability.abilityName);
            }

            abilityCategories.Add("All", allAbilities);

            groupButtons = new();
            if (groupTemplateUI) foreach (var catigory in abilityCategories.Keys)
                {
                    var newGroupButton = GameObject.Instantiate(groupTemplateUI, groupTemplateUI.transform.parent);

                    newGroupButton.name = catigory;
                    newGroupButton.GetComponentInChildren<TextMeshProUGUI>().text = $"{catigory} [{abilityCategories[catigory].Count}]";
                    newGroupButton.onClick.AddListener(() => SwitchCategory(newGroupButton.name));
                    newGroupButton.gameObject.SetActive(true);

                    groupButtons.Add(catigory, newGroupButton);
                }

            selectedAbilities = allAbilities;
            selectedAbility = GetSeletedAbilityIndex(currentIndex);

            #endregion

            attachedAbilityInstance.moneyUpdatedAction += UpdateCredits;

            UpdateCredits(0);

            if (error == false)
            {
                Debug.LogWarning("AbilityUI Initalized Successfully!");
                initalized = true;
            }
            else
            {
                Debug.LogError("AbilityUI Initalized with an error! Other behavior will not continue!");
            }
        }

        void OnEnable()
        {
            if (initalized)
            {
                DisplayAbility(selectedAbility);
                versionUI.text = Plugin.PLUGIN_GUID + " V" + Plugin.PLUGIN_VERSION;
            }
        }

        void OnDestroy()
        {
            foreach (var input in inputs)
            {
                input.Disable();
            }
        }

        bool roundStarted = false;
        void Update()
        {
            if (!roundStarted && StartOfRound.Instance.shipHasLanded && Plugin.seekers.Count > 0)
            {
                Debug.LogError("Round Started!");

                roundStarted = true;
                selectedAbilities = GetUsableAbilities(selectedAbilities);
                currentIndex = 0;
                selectedAbility = GetSeletedAbilityIndex(currentIndex);
                DisplayAbility(selectedAbility);
            }
            else if(roundStarted && StartOfRound.Instance.inShipPhase)
            {
                Debug.LogError("Round Ended!");

                roundStarted = false;
                selectedAbilities = allAbilities;
                currentIndex = 0;
                selectedAbility = GetSeletedAbilityIndex(currentIndex);
                DisplayAbility(selectedAbility);
            }

            UpdateCoolDownFade();
            UpdateTextColor();
        }

        List<string> GetUsableAbilities(List<string> abilities)
        {
            if (roundStarted)
            {
                List<string> newAbilities = new();

                if (Plugin.seekers.Contains(attachedPlayer))
                {
                    foreach (var aName in abilities)
                    {
                        AbilityBase ab = Abilities.FindAbilityByName(aName);

                        if (ab.seekerAbility)
                        {
                            newAbilities.Add(aName);
                        }
                    }
                }
                else
                {
                    foreach (var aName in abilities)
                    {
                        AbilityBase ab = Abilities.FindAbilityByName(aName);

                        if (ab.hiderAbility)
                        {
                            newAbilities.Add(aName);
                        }
                    }
                }

                return newAbilities;
            }
            else
            {
                return abilities;
            }
        }
        void UpdateCredits(int newAmount)
        {
            Debug.Log($"UI got event with new amout {newAmount}");

            #region CreateSpaceEveryThousand

            string amountString = "";

            char[] s = newAmount.ToString().ToCharArray();

            int counted = 0;
            for (int i = s.Length-1; i >= 0; i--)
            {
                counted++;
                if (counted > 3)
                {
                    counted = 0;
                    amountString += " ";
                }
                amountString += s[i];
            }

            var newAmountString = "";
            foreach (var c in amountString.ToCharArray().Reverse())
            {
                newAmountString += c;
            }

            #endregion

            creditUI.text = $"Credits: \n {newAmountString}$";
        }
        void ToggleAbilityUIInput(InputAction.CallbackContext context)
        {
            ToggleAbilityUI();
        }
        void DisableAbilityUIInput(InputAction.CallbackContext context)
        {
            ToggleAbilityUI(false);
        }
        void BuyInput()
        {
            if (!initalized || !gameObject.activeSelf) return;

            if(selectedAbility != null)
                attachedAbilityInstance.BuyAbility(selectedAbility);

            ToggleAbilityUI(false);
            AudioManager.PlaySound("ActivatedAbility");
        }
        void LeftInput(InputAction.CallbackContext context)
        {
            if (!initalized || !gameObject.activeSelf) return;

            PreviousAbility();

            AudioManager.PlaySound("SwitchAbility", 0.5f, 1.4f);
        }
        void RightInput(InputAction.CallbackContext context)
        {
            if (!initalized || !gameObject.activeSelf) return;

            NextAbility();

            AudioManager.PlaySound("SwitchAbility", 0.5f, 1.4f);
        }
        void ScrollInput(InputAction.CallbackContext context)
        {
            if (!initalized || !gameObject.activeSelf) return;
            
            float value = context.ReadValue<float>();
            bool scrollUp = value > 0;
            bool scrollDown = value < 0;
            if (scrollUp)
            {
                PreviousAbility();
            } else if (scrollDown)
            {
                NextAbility();
            }
            AudioManager.PlaySound("SwitchAbility", 0.5f, 1.4f);
        }
        void FavoriteInput(InputAction.CallbackContext context)
        {
            if (abilityCategories[FAVORITES].Contains(selectedAbility.abilityName))
            {
                RemoveFavoriteAbility(selectedAbility.abilityName);
            }
            else
            {
                AddFavoriteAbility(selectedAbility.abilityName);
            }
        }

        void NextAbility()
        {
            currentIndex++;

            if (currentIndex >= selectedAbilities.Count) currentIndex = 0;

            DisplayAbility(GetSeletedAbilityIndex(currentIndex));
        }

        void PreviousAbility()
        {
            currentIndex--;

            if (currentIndex < 0) currentIndex = selectedAbilities.Count-1;

            DisplayAbility(GetSeletedAbilityIndex(currentIndex));
        }

        List<string> GetFavorites()
        {
            var appdataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), APPDATA_FOLDER_NAME);
            if (!Directory.Exists(appdataPath))
            {
                Debug.LogError($"GetFavorites(): Directory doesn't exist! {appdataPath}");
                return new();
            }
            var favoritesPath = Path.Combine(appdataPath, FAVORITES_FILE_NAME);
            if (!File.Exists(favoritesPath))
            {
                Debug.LogError($"GetFavorites(): File doesn't exist! {favoritesPath}");
                return new();
            }

            List<string> newList = new();

            string fileData = File.ReadAllText(favoritesPath);

            foreach (var item in fileData.Split(","))
            {
                if (Abilities.AbilityExists(item))
                    newList.Add(item);
            }

            Debug.LogMessage($"GetFavorites(): Returned List {fileData}");
            return newList;
        }
        void SaveFavorites()
        {
            var appdataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), APPDATA_FOLDER_NAME);
            if (!Directory.Exists(appdataPath))
            {
                Directory.CreateDirectory(appdataPath);
            }
            var favoritesPath = Path.Combine(appdataPath, FAVORITES_FILE_NAME);

            string savefileText = "";

            foreach (var favorite in abilityCategories[FAVORITES])
            {
                if(savefileText != "")
                {
                    savefileText += ",";
                }
                savefileText += favorite;
            }

            File.WriteAllText(favoritesPath, savefileText);
        }
        
        void SwitchCategory(string category)
        {
            if (abilityCategories.ContainsKey(category))
            {
                selectedAbilities = GetUsableAbilities(abilityCategories[category]);
                currentIndex = 0;
                selectedAbility = GetSeletedAbilityIndex(currentIndex);
                DisplayAbility(selectedAbility);
            }
        }
        
        AbilityBase GetSeletedAbilityIndex(int index)
        {
            AbilityBase foundAbility = null;

            if (selectedAbilities.Count != 0 && selectedAbilities.Count > index)
            {
                foundAbility = Abilities.FindAbilityByName(selectedAbilities[index]);
            }

            return foundAbility;
        }
        // Public Methods
        public void ToggleAbilityUI()
        {
            if (!attachedPlayer.gameObject.activeSelf) return;

            bool menuEnabled = FindObjectOfType<QuickMenuManager>().isMenuOpen;

            if (!gameObject.activeSelf && !attachedPlayer.inTerminalMenu && !menuEnabled) // Enable
            {
                if (attachedPlayer.isPlayerDead || !attachedPlayer.isPlayerControlled) { Debug.LogWarning($"Ur ded bruh, ca't du dat. Dead:{attachedPlayer.isPlayerDead} Controlled:{attachedPlayer.isPlayerControlled}"); return; }

                gameObject.SetActive(true);
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;

                menuControlTip.alpha = MathF.Max(menuControlTip.alpha - 0.2f, 0);

                AccessTools.Method(typeof(PlayerControllerB), "OnDisable").Invoke(attachedPlayer, null); // Player Inputs
            }
            else // Disable
            {
                gameObject.SetActive(false);
                if (!menuEnabled)
                {
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                }

                AccessTools.Method(typeof(PlayerControllerB), "OnEnable").Invoke(attachedPlayer, null); // Player Inputs
            }
        }
        public void ToggleAbilityUI(bool enable)
        {
            if (enable != gameObject.activeSelf) ToggleAbilityUI();
        }

        public void DisplayAbility(AbilityBase ability)
        {
            //Debug.Log($"AbilityUI DisplayAbility({ability.abilityName}): Got display ability!");
            if (ability != null)
            {
                titleDescriptionUI.text = ability.abilityName;
                if (abilityCategories[FAVORITES].Contains(ability.abilityName))
                    titleUI.text = $"{ability.abilityName}*";
                else
                    titleUI.text = ability.abilityName;
                descriptionUI.text = ability.abilityDescription;
                costUI.text = $"{ability.abilityCost}$";

                abilitySpriteUI.sprite = AbilitySpriteManager.GetSprite(ability.abilityName);

                pageUI.text = $"Page [{currentIndex+1}/{selectedAbilities.Count}]";

                selectedAbility = ability;
                UpdateTextColor();
                UpdateCoolDownFade();
            }
            else
            {
                Debug.LogError($"AbilityUI DisplayAbility(): Reference == null!");

                titleDescriptionUI.text = "[Null]";
                titleUI.text = "[Null]";
                descriptionUI.text = "[Null]";
                costUI.text = "0$";

                abilitySpriteUI.sprite = AbilitySpriteManager.GetSprite("[Null]");

                pageUI.text = $"Page [0/0]";
                UpdateTextColor();
                UpdateCoolDownFade();
            }
        }
        public void UpdateTextColor()
        {
            if (selectedAbility == null)
            {
                titleUI.color = Color.red;
                titleDescriptionUI.color = Color.red;
                costUI.color = Color.red;
                return;
            }

            bool isSeeker = Plugin.seekers.Contains(attachedPlayer);
            if (!(selectedAbility.seekerAbility && isSeeker || selectedAbility.hiderAbility && !isSeeker) || // Not your ability type
                (selectedAbility.oneTimeUse && selectedAbility.usedThisRound) || // Already Used This Round
                (Time.time - selectedAbility.lastUsed <= selectedAbility.abilityDelay) || // On Cooldown
                selectedAbility.requiresRoundActive && !RoundManagerPatch.IsRoundActive() || // Requires Round Active
                selectedAbility.requiresSeekerActive && TimeOfDay.Instance.currentDayTime <= Config.timeSeekerIsReleased.Value) // Requires Seeker Active 
            {
                titleDescriptionUI.color = Color.red;
                titleUI.color = Color.red;
            }
            else
            {
                titleDescriptionUI.color = Color.green;
                titleUI.color = Color.green;
            }

            if (selectedAbility.abilityCost > attachedAbilityInstance.money)
            {
                costUI.color = Color.red;
            }
            else
            {
                costUI.color = Color.green;
            }
        }
        public void UpdateCoolDownFade()
        {
            if (selectedAbility == null)
            {
                fadeCooldownUI.localPosition = new(0, 0, 0);
                fadeCooldownUI.gameObject.SetActive(true);
                fadeSpriteUI.gameObject.SetActive(false);
                return;
            }

            float timeSinceUsed = Time.time - selectedAbility.lastUsed;

            if (timeSinceUsed <= selectedAbility.abilityDelay) // On Delay
            {
                float animationTime = timeSinceUsed / selectedAbility.abilityDelay; // starts at 0, goes to 1

                fadeCooldownUI.localPosition = new(0, -160 * animationTime, 0);

                if (fadeSpriteUI.gameObject.active)
                {
                    fadeSpriteUI.gameObject.SetActive(false);
                }
                if (!fadeCooldownUI.gameObject.active)
                {
                    fadeCooldownUI.gameObject.SetActive(true);
                }
            } else if (selectedAbility.oneTimeUse && selectedAbility.usedThisRound)
            {
                fadeCooldownUI.localPosition = new(0, 0, 0);

                if (fadeSpriteUI.gameObject.active)
                {
                    fadeSpriteUI.gameObject.SetActive(false);
                }
                if (!fadeCooldownUI.gameObject.active)
                {
                    fadeCooldownUI.gameObject.SetActive(true);
                }
            }
            else // Can use
            {
                if (!fadeSpriteUI.gameObject.active)
                {
                    fadeSpriteUI.gameObject.SetActive(true);
                }
                if (fadeCooldownUI.gameObject.active)
                {
                    fadeCooldownUI.gameObject.SetActive(false);
                }
            }
        }
        public void UpdateAbilityCount(Button selectedButton = null)
        {
            if (selectedButton)
            {
                int abilityCount = abilityCategories[selectedButton.name].Count;

                selectedButton.GetComponentInChildren<TextMeshProUGUI>().text = $"{selectedButton.name} [{abilityCount}]";

                if (selectedButton.name != FAVORITES && selectedButton.name != "All" && abilityCount == 0)
                {
                    selectedButton.gameObject.SetActive(false);
                }
                else
                {
                    selectedButton.gameObject.SetActive(true);
                }
            }
            else
            {
                foreach (Button groupButton in groupButtons.Values)
                {
                    int abilityCount = abilityCategories[groupButton.name].Count;

                    groupButton.GetComponentInChildren<TextMeshProUGUI>().text = $"{groupButton.name} [{abilityCount}]";

                    if (groupButton.name != FAVORITES && groupButton.name != "All" && abilityCount == 0)
                    {
                        groupButton.gameObject.SetActive(false);
                    }
                    else
                    {
                        groupButton.gameObject.SetActive(true);
                    }

                }
            }
        }
        public void AddFavoriteAbility(string ability)
        {
            abilityCategories[FAVORITES].Add(ability);
            titleUI.text = $"{selectedAbility.abilityName}*";
            UpdateAbilityCount(groupButtons[FAVORITES]);

            SaveFavorites();
        }
        public void RemoveFavoriteAbility(string ability)
        {
            abilityCategories[FAVORITES].Remove(ability);
            titleUI.text = selectedAbility.abilityName;
            UpdateAbilityCount(groupButtons[FAVORITES]);

            SaveFavorites();
        }

    }
}
