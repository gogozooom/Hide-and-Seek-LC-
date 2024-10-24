using HideAndSeek.AbilityScripts;
using LCVR.Player;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
using Debug = Debugger.Debug;

namespace HideAndSeek
{
    public class VRAbilityUI
    {
        static GameObject UIObject;
        static AbilityUI abilityUI;
        static GameObject tutorialUI;

        static TrackedPoseDriver leftHand;
        static TrackedPoseDriver rightHand;
        static TrackedPoseDriver camera;
        static Coroutine currentTickLoop = null;
        static bool usingVr = false;

        // Inputs
        public static InputAction AbilityMenuInput;
        public static InputAction ModifierInput;
        public static InputAction SellInput;
        public static InputAction NavigateInput;
        public static InputAction ActivateInput;

        public static void StartTicking()
        {
            CreateInputActions();
            if (currentTickLoop != null)
            {
                GameNetworkManager.Instance.StopCoroutine(currentTickLoop);
                camera = null;
                rightHand = null;
                leftHand = null;
                UIObject = null;
                abilityUI = null;
                currentTickLoop = null;
            }
            currentTickLoop = GameNetworkManager.Instance.StartCoroutine(Tryer());
        }
        static bool menuOpen = false;
        static int turnProvider = -1;
        static void Tick()
        {
            UsingVR();
            if (usingVr)
            {
                if (UIObject == null) CreateUIObject();

                // Using VR

                if (UIObject)
                {
                    Vector3 calculatedPosition = Vector3.zero;

                    if (!leftHand || !rightHand || !camera)
                    {
                        GetTrackedDrivers();
                    }

                    if (leftHand && rightHand)
                    {
                        float distance = (leftHand.transform.position - rightHand.transform.position).magnitude;
                        float functionY = Mathf.Clamp(1 - Mathf.Pow(distance, 2), 0, 1); // 1-x^{2}\left\{0\le x\le1\right\}
                        float yValue = functionY * 0.4f;

                        calculatedPosition = (leftHand.transform.position + rightHand.transform.position) / 2;

                        calculatedPosition += Vector3.up * 0.9f * yValue;
                        Vector3 cameraVector = (calculatedPosition - camera.transform.position);
                        calculatedPosition += new Vector3(cameraVector.x, 0, cameraVector.z).normalized * yValue;
                    }

                    UIObject.GetComponent<RectTransform>().position = calculatedPosition;
                    if (camera)
                    {
                        UIObject.GetComponent<RectTransform>().LookAt(camera.transform, Vector3.up);
                    }

                    if ((abilityUI.gameObject.activeSelf || tutorialUI.gameObject.activeSelf) && !menuOpen)
                    {
                        menuOpen = true;

                        if (Config.disableVRTurningWhileMenuOpen.Value && LCVR.Plugin.Config.TurnProvider.Value != LCVR.Config.TurnProviderOption.Disabled)
                        {
                            turnProvider = (int)LCVR.Plugin.Config.TurnProvider.Value;

                            LCVR.Plugin.Config.TurnProvider.Value = LCVR.Config.TurnProviderOption.Disabled;
                        }
                    }
                    else if (menuOpen)
                    {
                        menuOpen = false;

                        if (Config.disableVRTurningWhileMenuOpen.Value && LCVR.Plugin.Config.TurnProvider.Value == LCVR.Config.TurnProviderOption.Disabled)
                        {
                            LCVR.Plugin.Config.TurnProvider.Value = (LCVR.Config.TurnProviderOption)turnProvider;
                        }
                    }
                }
            }
            else
            {
                // Not Using Vr
            }
        }
        static IEnumerator Tryer()
        {
            try
            {
                Tick();
                // If first works, then it'll continue
                currentTickLoop = GameNetworkManager.Instance.StartCoroutine(Loop());
            }
            catch (System.IO.FileNotFoundException)
            {
                Debug.LogError("[VRAbilityUI] Could not find LCVR DLL! Using default UI...");
                yield break;
            }
        }
        static IEnumerator Loop()
        {
            while (true)
            {
                Tick();
                yield return new WaitForEndOfFrame();
            }
        }

        // Object creator/reference
        static void CreateInputActions()
        {
            AbilityMenuInput = new InputAction("OpenAbilityMenu", InputActionType.Button, "<XRController>{LeftHand}/thumbstickClicked");
            AbilityMenuInput.performed += ProssesAbilityMenuInput;
            AbilityMenuInput.canceled += ProssesAbilityMenuInput;
            AbilityMenuInput.Enable();

            ModifierInput = new InputAction("InputModifier", InputActionType.Value, "<XRController>{LeftHand}/gripPressed");
            ModifierInput.performed += ProssesModifierInput;
            ModifierInput.canceled += ProssesModifierInput;
            ModifierInput.Enable();

            SellInput = new InputAction("SellItem", InputActionType.Value, "<XRController>{LeftHand}/trigger");
            SellInput.performed += ProssesSellInput;
            SellInput.canceled += ProssesSellInput;
            SellInput.Enable();

            NavigateInput = new InputAction("Navigate", InputActionType.Value, "<XRController>{RightHand}/joystick");
            NavigateInput.performed += ProssesNavigateInput;
            NavigateInput.canceled += ProssesNavigateInput;
            NavigateInput.Enable();

            ActivateInput = new InputAction("Activate", InputActionType.Button, "<XRController>{RightHand}/primaryButton");
            ActivateInput.performed += ProssesActivateInput;
            ActivateInput.canceled += ProssesActivateInput;
            ActivateInput.Enable();
        }
        static void ProssesAbilityMenuInput(InputAction.CallbackContext context)
        {
            float v = context.ReadValue<float>();

            if (v > 0 && modifierInputDown)
            {
                abilityUI.ToggleAbilityUI();
            }
        }
        static bool modifierInputDown = false;
        static void ProssesModifierInput(InputAction.CallbackContext context)
        {
            float v = context.ReadValue<float>();

            if (v > 0.2f && !modifierInputDown)
            {
                modifierInputDown = true;
            }
            else if (v < 0.2f && modifierInputDown)
            {
                modifierInputDown = false;
            }
        }
        static bool sellInputDown = false;
        static void ProssesSellInput(InputAction.CallbackContext context)
        {
            float v = context.ReadValue<float>();

            if (v >= 0.5f && !sellInputDown)
            {
                sellInputDown = true;
                AbilityInstance.localInstance.SellInputPressed();
            }
            else if (v < 0.5f && sellInputDown)
            {
                sellInputDown = false;
                AbilityInstance.localInstance.SellInputCanceled();
            }
        }
        static bool leftInput;
        static bool rightInput;
        static bool upInput;
        static bool downInput;

        static void ProssesNavigateInput(InputAction.CallbackContext context)
        {
            Vector2 v = context.ReadValue<Vector2>();

            if (v.y > 0.2f && !upInput)
            {
                abilityUI.UpInput();
                upInput = true;
            }
            else if (v.y < 0.2f && upInput)
            {
                upInput = false;
            }

            if (v.y < -0.2f && !downInput)
            {
                abilityUI.DownInput();
                downInput = true;
            }
            else if (v.y > -0.2f && downInput)
            {
                downInput = false;
            }

            if (v.x > 0.2f && !rightInput)
            {
                abilityUI.RightInput();
                rightInput = true;
            }
            else if (v.x < 0.2f && rightInput)
            {
                rightInput = false;
            }

            if (v.x < -0.2f && !leftInput)
            {
                abilityUI.LeftInput();
                leftInput = true;
            }
            else if (v.x > -0.2f && leftInput)
            {
                leftInput = false;
            }
        }
        static void ProssesActivateInput(InputAction.CallbackContext context)
        {
            float v = context.ReadValue<float>();

            if (v > 0)
            {
                abilityUI.ActivateAbility();
            }
        }
        static void CreateUIObject()
        {
            UIObject = new GameObject();
            UIObject.name = "VRUICanvas";

            Canvas canvas = UIObject.GetComponent<Canvas>();
            if (!canvas) canvas = UIObject.AddComponent<Canvas>();

            canvas.renderMode = RenderMode.WorldSpace;
            abilityUI = GameObject.FindAnyObjectByType<AbilityUI>();
            tutorialUI = abilityUI.tutorialMenu;
            if (abilityUI == null)
            {
                GameObject.Destroy(UIObject);
                Debug.LogError("abilityUI is == null!");
                return;
            }
            
            abilityUI.transform.parent = UIObject.transform;
            tutorialUI.transform.parent = UIObject.transform;
            abilityUI.transform.Rotate(Vector3.up, 180, Space.Self);
            tutorialUI.transform.Rotate(Vector3.up, 180, Space.Self);
            tutorialUI.transform.localPosition = Vector3.zero;
            abilityUI.transform.Find("Background").gameObject.SetActive(false);
            tutorialUI.transform.Find("Background").gameObject.SetActive(false);
            // TODO: Remove the faded background on the ui

            UIObject.transform.localScale = Vector3.one / 6;
        }
        static void GetTrackedDrivers()
        {
            foreach (var item in GameObject.FindObjectsByType<TrackedPoseDriver>(0))
            {
                if (item.trackingStateInput.action.name.ToLower().Contains("left"))
                {
                    leftHand = item;
                }
                else if (item.trackingStateInput.action.name.ToLower().Contains("right"))
                {
                    rightHand = item;
                }
                else if (item.gameObject.name == "MainCamera")
                {
                    camera = item;
                }
            }
        }

        // Can use vr check
        static void Try()
        {
            usingVr = VRSession.InVR;
        }
        public static void UsingVR()
        {
            try
            {
                Try();
            }
            catch (System.Exception)
            {
                //Debug.Log("CheckError " + e);
            }
        }

        // Exiting
        public static void OnApplicationQuit() 
        {
            if (turnProvider != -1)
            {
                LCVR.Plugin.Config.TurnProvider.Value = (LCVR.Config.TurnProviderOption)turnProvider;
            }
        }
    }
}