using BepInEx;
using BepInEx.Logging;
using GameNetcodeStuff;
using HarmonyLib;
using HideAndSeek;
using HideAndSeek.AbilityScripts;
using HideAndSeek.AudioScripts;
using HideAndSeek.Patches;
using UnityEngine;
using Debug = Debugger.Debug;
using static BepInEx.BepInDependency;
using System.Collections.Generic;

namespace HideAndSeek
{
    [BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public const string PLUGIN_GUID = "gogozooom.HideAndSeek";
        public const string PLUGIN_NAME = "Hide And Seek";
        public const string PLUGIN_VERSION = "1.4.0.2";

        // Instances
        public static ManualLogSource _Logger;
        public static Plugin instance;
        public static Config _Config;
        public static List<PlayerControllerB> seekers = new();
        public static List<PlayerControllerB> zombies = new();
        public static AssetBundle networkHandlerBundle;
        public static AssetBundle abilityRadialMenuBundle;
        readonly Harmony harmony = new Harmony(PLUGIN_GUID);
        private void Awake()
        {
            // Variable Init
            instance = this;
            _Logger = Logger;
            _Config = new(Config);

            // Patches
            Debug.Log("Patching .RoundManagerPatch");
            harmony.PatchAll(typeof(RoundManagerPatch));

            Debug.Log("Patching .TurretPatch");
            harmony.PatchAll(typeof(TurretPatch));

            Debug.Log("Patching .LandminePatch");
            harmony.PatchAll(typeof(LandminePatch));

            Debug.Log("Patching .SpikeRoofTrapPatch");
            harmony.PatchAll(typeof(SpikeRoofTrapPatch));

            Debug.Log("Patching .ShotgunPatch");
            harmony.PatchAll(typeof(ShotgunPatch));

            Debug.Log("Patching .PlayerControllerBPatch");
            harmony.PatchAll(typeof(PlayerControllerBPatch));

            Debug.Log("Patching .EntranceTeleportPatch");
            harmony.PatchAll(typeof(EntranceTeleportPatch));

            Debug.Log("Patching .TerminalPatch");
            harmony.PatchAll(typeof(TerminalPatch));

            Debug.Log("Patching .TimeOfDayPatch");
            harmony.PatchAll(typeof(TimeOfDayPatch));

            Debug.Log("Patching .HUDManagerPatch");
            harmony.PatchAll(typeof(HUDManagerPatch));

            Debug.Log("Patching .GameNetworkManagerPatch");
            harmony.PatchAll(typeof(GameNetworkManagerPatch));

            Debug.Log("Patching .StartOfRoundPatch");
            harmony.PatchAll(typeof(StartOfRoundPatch));

            Debug.Log("Patching .InteractTriggerPatch");
            harmony.PatchAll(typeof(InteractTriggerPatch));

            Debug.Log("Patching .GrabbableObjectPatch");
            harmony.PatchAll(typeof(GrabbableObjectPatch));

            Debug.Log("Patching .DeadBodyInfoPatch");
            harmony.PatchAll(typeof(DeadBodyInfoPatch));

            Debug.Log("Patching .HoarderBugAIPatch");
            harmony.PatchAll(typeof(HoarderBugAIPatch));

            Debug.Log("Patching .CrawlerAIPatch");
            harmony.PatchAll(typeof(CrawlerAIPatch));

            Debug.Log("Patching .FlowermanAIPatch");
            harmony.PatchAll(typeof(FlowermanAIPatch));

            Debug.Log("Patching .MaskedPlayerEnemyPatch");
            harmony.PatchAll(typeof(MaskedPlayerEnemyPatch));

            // Network Assets
            var dllFolderPath = System.IO.Path.GetDirectoryName(Info.Location);
            var networkHandlerPath = System.IO.Path.Combine(dllFolderPath, "networkhandler");
            networkHandlerBundle = AssetBundle.LoadFromFile(networkHandlerPath);

            // Ability Radial Menu
            var abilityRadialMenuPath = System.IO.Path.Combine(dllFolderPath, "abilityradialmenu");
            abilityRadialMenuBundle = AssetBundle.LoadFromFile(abilityRadialMenuPath);

            AbilitySpriteManager.LoadSprites();
            StartCoroutine(AudioManager.LoadAudioCoroutine());

            // Plugin startup logic
            _Logger.LogInfo($"Plugin {PLUGIN_GUID} is loaded!");
        }
    }
}
namespace Debugger
{
    public static class Debug
    {
        static bool warned = false;
        readonly static string WARNmESSAGE = "DebugEnabled is false! (Make sure to turn this on when trying to read Debug.Log()'s messages)";

        public static void Log(object m)
        {
            if (Config.debugEnabled.Value)
                Plugin._Logger.LogInfo(m);
            else if (!warned)
            {
                warned = true;
                Plugin._Logger.LogWarning(WARNmESSAGE);
            }
        }
        public static void LogMessage(object m)
        {
            if (Config.debugEnabled.Value)
                Plugin._Logger.LogMessage(m);
            else if (!warned)
            {
                warned = true;
                Plugin._Logger.LogWarning(WARNmESSAGE);
            }
        }
        public static void LogWarning(object m)
        {
            if (Config.debugEnabled.Value)
                Plugin._Logger.LogWarning(m);
            else if (!warned)
            {
                warned = true;
                Plugin._Logger.LogWarning(WARNmESSAGE);
            }
        }
        public static void LogError(object m)
        {
            if (Config.debugEnabled.Value)
                Plugin._Logger.LogError(m);
            else if (!warned)
            {
                warned = true;
                Plugin._Logger.LogWarning(WARNmESSAGE);
            }
        }
    }
}