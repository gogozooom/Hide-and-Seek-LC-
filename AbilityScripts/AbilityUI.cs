using BepInEx;
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
        public static AbilityUI Instance;

        // Tutorial References
        public GameObject tutorialMenu;

        GameObject tutLeftInfoUI;
        TextMeshProUGUI tutLeftTitleUI;
        TextMeshProUGUI tutLeftDescriptionUI;
        Image tutLeftImageUI;

        GameObject tutRightInfoUI;
        TextMeshProUGUI tutRightTitleUI;
        TextMeshProUGUI tutRightDescriptionUI;
        Image tutRightImageUI;

        Button tutBackButtonUI;
        Button tutNextButtonUI;

        TextMeshProUGUI tutVersionUI;

        // Tutorial Lists
        List<TutorialPage> tutorialPages = new() {
            new("Main", "How to Play!", "", "Config", "TutIcon", false,
                "Welcome to the tutorial screen! If you already know what you're doing, you can just press 'escape' to be on your marry way." +
                "\r\nOtherwise, click the arrow to the right of this menu to continue!"),
            new("Config", "Configuration!", "Main", "Rounds", "TutConfig", true,
                "This mod's behavior heavily depends on its config file, so if you haven't already, go check that out. (By looking somewhere in the BepInEx configuration folder)" +
                "\r\nSome noteworthy things you can change in this config file are the items the seeker and the hider spawn with, and what hostels spawn naturally." +
                "\r\nBut the biggest game changers are, ‘Abilities’, ‘Zombies’, and ‘Objectives’. More on those later.This mod's behavior heavily depends on its config file, so if you haven't already, go check that out. (By looking somewhere in the BepInEx configuration folder)"),
            new("Rounds", "Rounds!", "Config", "Rounds2", "TutSeeker", false,
                "A round starts after simply pulling the lever, and a random seeker will be automatically chosen. (Player choosing behavior can be changed in the config)" +
                "\r\nOnce the ship lands, all the hiders will get teleported into the interior while the seeker is locked in the ship." +
                "\r\nAll players are automatically given their respective items depending on what has been set in the config file."),
            new("Rounds2", "Rounds!", "Rounds", "Abilities", "TutOneHider", false,
                "During seeking hours, all players will automatically be notified when a player dies up until there is one hider left. Then the time will jump to evening." +
                "\r\nThis is also the time that the hider objective is released, so make sure to strategize!"),
            new("Abilities", "Abilities!", "Rounds2", "Credits", "TutAbilities", true,
                "And now the bread and butter of this mod, abilities! (Press 't' to open by default)" +
                "\r\nThis is where you spend your hard-earned credits to gain a temporary advantage!" +
                "\r\nAs to where you get the credits, there are many sources!"),
            new("Credits", "Credits!", "Abilities", "Zombies", "TutSell", false,
                "Credits can be obtained primarily by these methods:" +
                "\r\nTanting, Selling scrap (Hold 'c' by default), Selling dead bodies, Doing well in a round, Winning a round, Spawning a loot bug to get scrap for you :)"),
            new("Zombies", "Zombies!", "Credits", "Zombies2", "TutZombie", true,
                "‘Zombies’ is a special feature that is toggleable in the config. Once enabled, any hiders that die will respawn as zombies with a buffed melee weapon." +
                "\r\n(Note: Zombies have the ability menu disabled by default, can also be changed in the config)"),
            new("Zombies2", "Zombies!", "Zombies", "Objective", "TutZombie2", true,
                "Zombies will assist the hider with finding the last few seekers. However once a zombie dies once, they will not respawn (configurable) until next round."),
            new("Objective", "Objectives!", "Zombies2", "Fun", "TutObjective", false,
                "Objectives can give the hiders extra goals other than simply surviving" +
                "\r\nThis objective will be released at a time specified in the config." +
                "\r\nPlayers will also get an additional reward for reaching the objective take advantage of this!"),
            new("Fun", "Hide and Seek", "Objective", "EXIT", "TutIcon", true,
                "Thank you for reading, through the tutorial! You are now a Hide and Seek master!" +
                "\r\nNow flex all your knowledge and skills to your frens!")
        };
        public TutorialPage selectedTutorialPage;

        // UI References
        TextMeshProUGUI creditUI;
        TextMeshProUGUI titleDescriptionUI;
        TextMeshProUGUI titleUI;
        TextMeshProUGUI descriptionUI;
        TextMeshProUGUI versionUI;
        TextMeshProUGUI pageUI;
        TextMeshProUGUI costUI;
        public TextMeshProUGUI menuControlTip;
        public TextMeshProUGUI sellControlTip;

        Image abilitySpriteUI;
        Image fadeSpriteUI;
        RectTransform fadeCooldownUI;

        Button groupTemplateUI;
        Button helpButton;
        Dictionary<string, Button> groupButtons;

        // Ability Lists
        List<string> selectedAbilities;
        int currentAbilityIndex = 0;
        int currentCategoryIndex = 0;

        List<string> allAbilities;
        Dictionary<string, List<string>> abilityCategories;

        public AbilityBase selectedAbility;
        AbilityInstance attachedAbilityInstance;
        PlayerControllerB attachedPlayer;

        // Inputs
        List<InputAction> inputs = new();

        // Other
        public bool initalized = false;
        const string FAVORITES = "Favorites";
        const string FAVORITES_FILE_NAME = "favorites.list";
        const string APPDATA_FOLDER_NAME = "LethalCompany.HideAndSeek";
        void Awake()
        {
            Instance = this;

            Debug.Log("AbilityUI Awake(): Initalizing References!");

            Debug.LogWarning("Reading Config!");

            Abilities.ReadConfigFile();

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

            helpButton = GameObject.Find("Info/HelpButton")?.GetComponent<Button>();
            if (helpButton == null)
            {
                Debug.LogError("Could not find Info/HelpButton!");
                error = true;
            }
            else
            {
                helpButton.onClick.AddListener(HelpButtonPressed);
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

            groupTemplateUI = GameObject.Find("Groups/Template")?.GetComponent<Button>();
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
                menuControlTip.text = $"Ability Menu: [{InputConfigs.GetInputClass().AbilityMenuKey.GetBindingDisplayString()}]";
                menuControlTip.transform.localScale = Vector3.one;
                menuControlTip.transform.position = controlTip.transform.position + (Vector3.down * (controlTip.transform.position.y - otherControlTip.transform.position.y) * 4);

                sellControlTip = Instantiate(controlTip);
                sellControlTip.transform.parent = controlTip.transform.parent;
                sellControlTip.text = $"Sell Scrap: [{InputConfigs.GetInputClass().SellKey.GetBindingDisplayString()}]";
                sellControlTip.transform.localScale = Vector3.one;
                sellControlTip.transform.position = controlTip.transform.position + (Vector3.down * (controlTip.transform.position.y - otherControlTip.transform.position.y) * 5);
            }

            #endregion

            #region Tutorial Menu

            tutorialMenu = attachedAbilityInstance.tutorialMenu;

            tutLeftInfoUI = tutorialMenu.transform.Find("MenuUI/LeftInfo")?.gameObject;
            if (tutLeftInfoUI == null)
            {
                Debug.LogError($"Could not find {nameof(tutLeftInfoUI)}!");
                error = true;
            }

            tutLeftTitleUI = tutorialMenu.transform.Find("MenuUI/LeftInfo/Side/Title")?.GetComponent<TextMeshProUGUI>();
            if (tutLeftTitleUI == null)
            {
                Debug.LogError($"Could not find {nameof(tutLeftTitleUI)}!");
                error = true;
            }

            tutLeftDescriptionUI = tutorialMenu.transform.Find("MenuUI/LeftInfo/Side/Description")?.GetComponent<TextMeshProUGUI>();
            if (tutLeftDescriptionUI == null)
            {
                Debug.LogError($"Could not find {nameof(tutLeftDescriptionUI)}!");
                error = true;
            }

            tutLeftImageUI = tutorialMenu.transform.Find("MenuUI/LeftInfo/Image")?.GetComponent<Image>();
            if (tutLeftImageUI == null)
            {
                Debug.LogError($"Could not find {nameof(tutLeftImageUI)}!");
                error = true;
            }


            tutRightInfoUI = tutorialMenu.transform.Find("MenuUI/RightInfo")?.gameObject;
            if (tutRightInfoUI == null)
            {
                Debug.LogError($"Could not find {nameof(tutRightInfoUI)}!");
                error = true;
            }

            tutRightTitleUI = tutorialMenu.transform.Find("MenuUI/RightInfo/Side/Title")?.GetComponent<TextMeshProUGUI>();
            if (tutRightTitleUI == null)
            {
                Debug.LogError($"Could not find {nameof(tutRightTitleUI)}!");
                error = true;
            }

            tutRightDescriptionUI = tutorialMenu.transform.Find("MenuUI/RightInfo/Side/Description")?.GetComponent<TextMeshProUGUI>();
            if (tutRightDescriptionUI == null)
            {
                Debug.LogError($"Could not find {nameof(tutRightDescriptionUI)}!");
                error = true;
            }

            tutRightImageUI = tutorialMenu.transform.Find("MenuUI/RightInfo/Image")?.GetComponent<Image>();
            if (tutRightImageUI == null)
            {
                Debug.LogError($"Could not find {nameof(tutRightImageUI)}!");
                error = true;
            }

            tutBackButtonUI = tutorialMenu.transform.Find("MenuUI/BackButton")?.GetComponent<Button>();
            if (tutBackButtonUI == null)
            {
                Debug.LogError($"Could not find {nameof(tutBackButtonUI)}!");
                error = true;
            }
            else
            {
                tutBackButtonUI.onClick.AddListener(TutBackButtonPressed);
            }

            tutNextButtonUI = tutorialMenu.transform.Find("MenuUI/ForwardButton")?.GetComponent<Button>();
            if (tutNextButtonUI == null)
            {
                Debug.LogError($"Could not find {nameof(tutNextButtonUI)}!");
                error = true;
            }
            else
            {
                tutNextButtonUI.onClick.AddListener(TutNextButtonPressed);
            }

            tutVersionUI = tutorialMenu.transform.Find("MenuUI/Version")?.GetComponent<TextMeshProUGUI>();
            if (tutVersionUI == null)
            {
                Debug.LogError($"Could not find {nameof(tutVersionUI)}!");
                error = true;
            }

            #endregion

            #region Inputs

            InputAction activateInput = InputConfigs.GetInputClass().ActivateKey;
            activateInput.performed += ActivateInput;
            activateInput.Enable();
            inputs.Add(activateInput);

            InputAction categoryInput = InputConfigs.GetInputClass().ScrollCategoriesInput;
            categoryInput.performed += ScrollCategoriesInput;
            categoryInput.Enable();
            inputs.Add(categoryInput);

            InputAction categoryInputUp = InputConfigs.GetInputClass().UpKey;
            categoryInputUp.performed += UpInput;
            categoryInputUp.Enable();
            inputs.Add(categoryInputUp);

            InputAction categoryInputDown = InputConfigs.GetInputClass().DownKey;
            categoryInputDown.performed += DownInput;
            categoryInputDown.Enable();
            inputs.Add(categoryInputDown);

            InputAction abilityMenu = InputConfigs.GetInputClass().AbilityMenuKey;
            abilityMenu.performed += ToggleAbilityUIInput;
            abilityMenu.Enable();
            inputs.Add(abilityMenu);

            InputAction exit = InputConfigs.GetInputClass().Escape;
            exit.performed += ExitUIInput;
            exit.Enable();
            inputs.Add(exit);

            InputAction scrollInput = InputConfigs.GetInputClass().ScrollAbilitiesInput;
            scrollInput.performed += ScrollInput;
            scrollInput.Enable();
            inputs.Add(scrollInput);

            InputAction clickLeft = InputConfigs.GetInputClass().BackKey;
            clickLeft.performed += LeftInput;
            clickLeft.Enable();
            inputs.Add(clickLeft);

            InputAction clickRight = InputConfigs.GetInputClass().ForwardKey;
            clickRight.performed += RightInput;
            clickRight.Enable();
            inputs.Add(clickRight);

            InputAction middleClick = InputConfigs.GetInputClass().FavoriteKey;
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
                buyButton.onClick.AddListener(ActivateAbility);
            }

            TextMeshProUGUI tip1 = buyButton.transform.Find("Tip1")?.GetComponent<TextMeshProUGUI>();
            if (tip1 == null)
            {
                Debug.LogError("Could not find Tip1!");
            }

            TextMeshProUGUI tip2 = buyButton.transform.Find("Tip2")?.GetComponent<TextMeshProUGUI>();
            if (tip2 == null)
            {
                Debug.LogError("Could not find Tip2!");
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


            currentCategoryIndex = abilityCategories.Count - 1;
            selectedAbilities = allAbilities;
            selectedAbility = GetSeletedAbilityIndex(currentAbilityIndex);

            #endregion

            attachedAbilityInstance.moneyUpdatedAction += UpdateCredits;

            UpdateCredits(0);

            if (error == false)
            {
                Debug.LogWarning("AbilityUI Initalized Successfully!");
                initalized = true;

                DisplayTutorial("Main");
                if (PlayerPrefs.GetInt("SeenTutorial") != 1)
                {
                    PlayerPrefs.SetInt("SeenTutorial", 1);
                    Invoke(nameof(ToggleTutorialUI), 4f);
                }


                VRAbilityUI.StartTicking();
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
            Debug.LogWarning("[AbilityUI] Destroying!");

            foreach (var input in inputs.ToArray())
            {
                input.performed -= ActivateInput;

                input.performed -= ScrollCategoriesInput;

                input.performed -= UpInput;

                input.performed -= DownInput;

                input.performed -= ToggleAbilityUIInput;

                input.performed -= ExitUIInput;

                input.performed -= ScrollInput;

                input.performed -= LeftInput;

                input.performed -= RightInput;

                input.performed -= FavoriteInput;

                input.Disable();
            }
            inputs.Clear();
        }

        bool roundStarted = false;
        void Update()
        {
            if (!roundStarted && StartOfRound.Instance.shipHasLanded && Plugin.seekers.Count > 0)
            {
                Debug.LogError("[Ability UI] Round Started!");

                roundStarted = true;
                selectedAbilities = GetUsableAbilities(selectedAbilities);
                currentAbilityIndex = 0;
                selectedAbility = GetSeletedAbilityIndex(currentAbilityIndex);
                DisplayAbility(selectedAbility);
            }
            else if(roundStarted && StartOfRound.Instance.inShipPhase)
            {
                Debug.LogError("[Ability UI] Round Ended!");

                roundStarted = false;
                selectedAbilities = allAbilities;
                currentAbilityIndex = 0;
                selectedAbility = GetSeletedAbilityIndex(currentAbilityIndex);
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
            Debug.Log($"UI got event with new amount {newAmount}");

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
        void ExitUIInput(InputAction.CallbackContext context)
        {
            ToggleTutorialUI(false);    
            ToggleAbilityUI(false);
        }
        void ActivateInput(InputAction.CallbackContext context)
        {
            ActivateAbility();
        }
        public void ActivateAbility()
        {
            if (!initalized || !gameObject.activeSelf) return;

            if(selectedAbility != null)
                attachedAbilityInstance.BuyAbility(selectedAbility);

            ToggleAbilityUI(false);
            AudioManager.PlaySound("ActivatedAbility");
        }
        public void LeftInput(InputAction.CallbackContext context = new())
        {
            if (!initalized) return;

            if (gameObject.activeSelf)
            {
                PreviousAbility();
            }
            else if (tutorialMenu.activeSelf)
            {
                TutBackButtonPressed();
            }
            else
            {
                return;
            }

            AudioManager.PlaySound("SwitchAbility", 0.5f, 1.4f);
        }
        public void RightInput(InputAction.CallbackContext context = new())
        {
            if (!initalized) return;

            if (gameObject.activeSelf)
            {
                NextAbility();
            } else if (tutorialMenu.activeSelf)
            {
                TutNextButtonPressed();
            }
            else
            {
                return;
            }

            AudioManager.PlaySound("SwitchAbility", 0.5f, 1.4f);
        }
        public void UpInput(InputAction.CallbackContext context = new())
        {
            if (!initalized || !gameObject.activeSelf) return;

            PreviousCategory();

            AudioManager.PlaySound("SwitchAbility", 0.5f, 1.4f);
        }
        public void DownInput(InputAction.CallbackContext context = new())
        {
            if (!initalized || !gameObject.activeSelf) return;

            NextCategory();

            AudioManager.PlaySound("SwitchAbility", 0.5f, 1.4f);
        }
        void ScrollInput(InputAction.CallbackContext context)
        {
            if (!initalized || !gameObject.activeSelf) return;

            float value = context.ReadValue<Vector2>().y;
            bool scrollUp = value > 0;
            bool scrollDown = value < 0;
            if (scrollUp)
            {
                AudioManager.PlaySound("SwitchAbility", 0.5f, 1.4f);
                PreviousAbility();
            } else if (scrollDown)
            {
                AudioManager.PlaySound("SwitchAbility", 0.5f, 1.4f);
                NextAbility();
            }
        }
        void ScrollCategoriesInput(InputAction.CallbackContext context)
        {
            if (!initalized) return;

            float categoryValue = context.ReadValue<Vector2>().y;
            float abilityValue = context.ReadValue<Vector2>().x;
            bool scrollUp = categoryValue < 0;
            bool scrollDown = categoryValue > 0;
            bool scrollLeft = abilityValue < 0;
            bool scrollRight = abilityValue > 0;

            if (scrollUp)
            {
                if (gameObject.activeSelf)
                {
                    NextCategory();
                } else
                {
                    return;
                }
            }
            else if (scrollDown)
            {
                if (gameObject.activeSelf)
                {
                    PreviousCategory();
                }
                else
                {
                    return;
                }
            }
            else if (scrollRight)
            {
                if (gameObject.activeSelf)
                {
                    NextAbility();
                }
                else if (tutorialMenu.activeSelf)
                {
                    TutNextButtonPressed();
                }
                else
                {
                    return;
                }
            }
            else if (scrollLeft)
            {
                if (gameObject.activeSelf)
                {
                    PreviousAbility();
                }
                else if (tutorialMenu.activeSelf)
                {
                    TutBackButtonPressed();
                }
                else
                {
                    return;
                }
            }
            else
            {
                return;
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

        void PreviousCategory()
        {
            currentCategoryIndex--;

            if (currentCategoryIndex < 0) currentCategoryIndex = abilityCategories.Count - 1;

            SwitchCategory(abilityCategories.Keys.ToArray()[currentCategoryIndex]);
        }

        void NextCategory()
        {
            currentCategoryIndex++;

            if (currentCategoryIndex >= abilityCategories.Count) currentCategoryIndex = 0;

            SwitchCategory(abilityCategories.Keys.ToArray()[currentCategoryIndex]);
        }

        void NextAbility()
        {
            currentAbilityIndex++;

            if (currentAbilityIndex >= selectedAbilities.Count) currentAbilityIndex = 0;

            DisplayAbility(GetSeletedAbilityIndex(currentAbilityIndex));
        }

        void PreviousAbility()
        {
            currentAbilityIndex--;

            if (currentAbilityIndex < 0) currentAbilityIndex = selectedAbilities.Count-1;

            DisplayAbility(GetSeletedAbilityIndex(currentAbilityIndex));
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
                foreach (var item in groupButtons)
                {
                    item.Value.GetComponentInChildren<TextMeshProUGUI>().color = Color.white;
                }

                var button = groupButtons[category];

                button.GetComponentInChildren<TextMeshProUGUI>().color = Color.red;

                currentCategoryIndex = abilityCategories.Keys.ToList().IndexOf(category);

                selectedAbilities = GetUsableAbilities(abilityCategories[category]);
                currentAbilityIndex = 0;
                selectedAbility = GetSeletedAbilityIndex(currentAbilityIndex);
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
                if (!StartOfRound.Instance.inShipPhase)
                    if (attachedPlayer.isPlayerDead || !attachedPlayer.isPlayerControlled || !Config.zombiesCanUseAbilities.Value && Plugin.zombies.Contains(attachedPlayer)) { return; }

                ToggleTutorialUI(false);
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

                pageUI.text = $"Page [{currentAbilityIndex+1}/{selectedAbilities.Count}]";

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
        public void OnApplicationQuit()
        {
            VRAbilityUI.OnApplicationQuit();
        }
        
        // Tutorial Methods
        public void TutNextButtonPressed()
        {
            if (selectedTutorialPage.nextID == "EXIT")
            {
                ToggleTutorialUI(false);
            }
            else
            {
                DisplayTutorial(selectedTutorialPage.nextID);
            }
            AudioManager.PlaySound("SwitchAbility", 0.5f, 1.4f);
        }
        public void TutBackButtonPressed()
        {
            if (selectedTutorialPage.lastID == "EXIT")
            {
                ToggleTutorialUI(false);
            }
            else
            {
                DisplayTutorial(selectedTutorialPage.lastID);
            }
            AudioManager.PlaySound("SwitchAbility", 0.5f, 1.4f);
        }
        public void HelpButtonPressed()
        {
            DisplayTutorial("Main");
            ToggleTutorialUI(true);
        }
        public void ToggleTutorialUI()
        {
            if (!attachedPlayer.gameObject.activeSelf) return;

            bool menuEnabled = FindObjectOfType<QuickMenuManager>().isMenuOpen;

            if (gameObject.activeSelf)
            {
                ToggleAbilityUI(false);
            }

            if (!tutorialMenu.activeSelf && !attachedPlayer.inTerminalMenu && !menuEnabled) // Enable
            {
                if (!StartOfRound.Instance.inShipPhase)
                    if (attachedPlayer.isPlayerDead || !attachedPlayer.isPlayerControlled || !Config.zombiesCanUseAbilities.Value && Plugin.zombies.Contains(attachedPlayer)) { return; }

                tutorialMenu.SetActive(true);
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;

                AccessTools.Method(typeof(PlayerControllerB), "OnDisable").Invoke(attachedPlayer, null); // Player Inputs
            }
            else // Disable
            {
                tutorialMenu.SetActive(false);
                if (!menuEnabled)
                {
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                }

                AccessTools.Method(typeof(PlayerControllerB), "OnEnable").Invoke(attachedPlayer, null); // Player Inputs
            }
        }
        public void ToggleTutorialUI(bool enable)
        {
            if (enable != tutorialMenu.activeSelf) ToggleTutorialUI();
        }
        public void DisplayTutorial(TutorialPage tutorialPage)
        {
            selectedTutorialPage = tutorialPage;
            tutVersionUI.text = Plugin.PLUGIN_GUID + " V" + Plugin.PLUGIN_VERSION;

            if (tutorialPage.isLeftVariant)
            {
                tutLeftInfoUI.SetActive(true);
                tutRightInfoUI.SetActive(false);
            }
            else
            {
                tutLeftInfoUI.SetActive(false);
                tutRightInfoUI.SetActive(true);
            }
            Sprite sprite = AbilitySpriteManager.GetSprite(tutorialPage.image);

            tutLeftTitleUI.text = tutorialPage.title;
            tutLeftDescriptionUI.text = tutorialPage.description;
            tutLeftImageUI.sprite = sprite;

            tutRightTitleUI.text = tutorialPage.title;
            tutRightDescriptionUI.text = tutorialPage.description;
            tutRightImageUI.sprite = sprite;

            if (tutorialPage.nextID.IsNullOrWhiteSpace())
            {
                tutNextButtonUI.gameObject.SetActive(false);
            }
            else
            {
                tutNextButtonUI.gameObject.SetActive(true);
            }

            if(tutorialPage.lastID.IsNullOrWhiteSpace())
            {
                tutBackButtonUI.gameObject.SetActive(false);
            }
            else
            {
                tutBackButtonUI.gameObject.SetActive(true);
            }
        }
        public void DisplayTutorial(string tutorialID)
        {
            foreach (var tutorial in tutorialPages)
            {
                if (tutorial.id == tutorialID)
                {
                    DisplayTutorial(tutorial);
                    return;
                }
            }
            Debug.LogError($"Could not find tutorial page with id '{tutorialID}'");
        }
    
    }
    public class TutorialPage
    {
        // Info
        public string id;
        public string title = "Generic Title!";
        public string description = "This is a generic description!";
        public string image = "NULL";

        public string lastID;
        public string nextID;
        public bool isLeftVariant = true;

        public TutorialPage(string _id = "", string _title = "Generic Title!", string _lastID = "", string _nextID = "", string _image = "NULL", bool _isLeftVariant = true,string _description = "This is a generic description!")
        {
            id = _id;
            title = _title;
            description = _description;
            image = _image;
            isLeftVariant = _isLeftVariant;

            lastID = _lastID;
            nextID = _nextID;
        }
    }
}
