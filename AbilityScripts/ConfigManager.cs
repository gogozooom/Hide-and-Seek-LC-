using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using BepInEx;
using BepInEx.Configuration;
using Debugger;

namespace HideAndSeek.AbilityScripts
{
    /// <summary>
    /// Class who have all the methods to deals with the CFG files.
    /// </summary>
    public static class ConfigManager
    {
        // Old vars
        public const string oldCFGfNAME = "Abilities.Cfg";
        public static List<AbilityConfig> abilityConfigs = new List<AbilityConfig>();

        // New Vars
        public static ConfigFile ConfigFileAbility;
        public const string newCfgAbilityName = "gogozooom.HideAndSeek.ConfigAbilities.cfg";

        public static ConfigFile GetConfigAbility()
        {
            string abilityCfgPath = Path.Combine(Paths.ConfigPath, newCfgAbilityName);
            ConfigFileAbility = new ConfigFile(abilityCfgPath, true);
            return ConfigFileAbility;
        }

        /// <summary>
        /// Method used to read the CFG Files
        /// </summary>
        public static void ReadConfigFile(List<AbilityBase> abilities)
        {
            if (GameNetworkManager.Instance.isHostingGame)
            {
                var dllFolderPath = Path.GetDirectoryName(Plugin.instance.Info.Location);
               var filePath = Path.Combine(dllFolderPath, oldCFGfNAME);
                var newfilePath = Path.Combine(Paths.ConfigPath, newCfgAbilityName);

                if (File.Exists(filePath))
                {
                    string data = File.ReadAllText(filePath);
                    string version = data.Split("]")[0].Replace("[v", "");

                    Debug.LogMessage($"Found File! {version}");
                    if (version != Plugin.PLUGIN_VERSION)
                    {
                        Debug.LogError($"Config file version does not match the current version! cfg = 'v{version}' plugin = 'v{Plugin.PLUGIN_VERSION}' Making backup...");
                        File.Move(filePath, Path.Combine(dllFolderPath, "v" + version + " - " + newCfgAbilityName));
                        ReadConfigFile(abilities);
                        return;
                    }
                    else
                    {
                        abilityConfigs = OldADataToCfgs(abilities, data.Split("]")[1]);
                    }
                }
                else
                {
                    OldAbilitiesToCfg(abilities);

                    string data = $"[v{Plugin.PLUGIN_VERSION}]\r\n" + OldAbilityCfgsToData();

                    Debug.LogWarning("______________ Got Data!: " + data);

                    File.WriteAllText(filePath, data);
                }
            }
            else
            {
                NetworkHandler.Instance.EventSendRpc(".requestAbilityConfig", new(__ulong: GameNetworkManager.Instance.localPlayerController.actualClientId));
            }
        }

        /// <summary>
        /// Write the cfg files.
        /// </summary>
        public static void WriteConfigFile()
        {
            if (GameNetworkManager.Instance.isHostingGame)
            {
                var dllFolderPath = Path.GetDirectoryName(Plugin.instance.Info.Location);
                var filePath = Path.Combine(dllFolderPath, oldCFGfNAME);
                var newFilePath = Path.Combine(dllFolderPath, newCfgAbilityName);

                File.WriteAllText(filePath, OldAbilityCfgsToData());
            }
            else
            {
                Debug.LogWarning("Canceled writing config file because is not host!");
            }
        }

        #region Ability propertys checker methods
        /// <summary>
        /// Get requiresSeekerActive.
        /// </summary>
        /// <param name="ability"></param>
        /// <returns></returns>
        public static bool IsAbilitySeekerActive(AbilityBase ability)
        {
            return ability.abilityName switch
            {
                "Taunt" => ConfigAbility.tauntWhenSeekerInside.Value,
                _ => ability.requiresSeekerActive,
            };
        }

        /// <summary>
        /// Get oneTimeUse.
        /// </summary>
        /// <param name="ability"></param>
        /// <returns></returns>
        public static bool IsAbilityOneTimeUse(AbilityBase ability)
        {
            return ability.abilityName switch
            {
                "Taunt" => ConfigAbility.tauntOneTimeUse.Value,
                _ => ability.oneTimeUse,
            };
        }
        #endregion

        #region Old Ability Config
        public static AbilityConfig OldAbilityToCfg(AbilityBase ability)
        {
            return new(ability.abilityName,
                                ability.GetAbilityCost(), ability.abilityDelay,
                                IsAbilityOneTimeUse(ability), ability.IsAbilityAviableForRole(true),
                                ability.IsAbilityAviableForRole(false), ability.IsAbilityOnRoundOnly(), IsAbilitySeekerActive(ability));
        }
        public static AbilityBase OldApplyCfgToAbility(AbilityBase ability, AbilityConfig cfg)
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

        /// <summary>
        /// Get infos from the config file
        /// </summary>
        /// <param name="ability"></param>
        /// <param name="cfg"></param>
        /// <returns></returns>
        public static AbilityBase OldApplyCfgToAbility(AbilityBase ability)
        {
            switch (ability.abilityName)
            {
                case "Taunt":
                    ability.abilityCost = ConfigAbility.tauntCost.Value;
                    ability.abilityDelay = ConfigAbility.tauntDelay.Value;
                    ability.oneTimeUse = ConfigAbility.tauntOneTimeUse.Value;
                    ability.seekerAbility = ConfigAbility.tauntForSeekers.Value;
                    ability.hiderAbility = ConfigAbility.tauntForHiders.Value;
                    ability.requiresRoundActive = ConfigAbility.tauntOnRoundActivation.Value;
                    ability.requiresSeekerActive = ConfigAbility.tauntWhenSeekerInside.Value;
                    break;
            }
            return ability;
        }
        public static string OldAbilityCfgToData(AbilityConfig aCfg, bool format = true)
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
        public static string OldAbilityCfgsToData(bool format = true)
        {
            if (abilityConfigs.Count <= 0) { Debug.LogError("Tried to ACfgToData but there is no ability config data!"); return null; }

            string data = string.Empty;

            foreach (var aCfg in abilityConfigs)
            {
                if (data != string.Empty && format)
                {
                    data += "\r\n";
                }

                data += OldAbilityCfgToData(aCfg, format);
            }

            return data;
        }
        public static AbilityConfig OldADataToCfg(List<AbilityBase> abilities, string d)
        {
            string[] s = d.Split("{");
            if (s.Length < 2)
            {
                Debug.LogError($"[OldADataToCfg] Ligne mal formée : {d}");
                return null;
            }
            string aName = s[0].Trim();
            string data = s[1];

            AbilityBase ability = FindAbilityByName(abilities, aName, true);

            if (ability == null)
            {
                Debug.LogError($"[OldADataToCfg] Ability '{aName}' not found in the list. Skipping this entry.");
                return null;
            }
            AbilityConfig aCfg = OldAbilityToCfg(ability);

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

        public static AbilityBase FindAbilityByName(List<AbilityBase> abilities, string name, bool raw = false)
        {
            AbilityBase ability = null;

            Debug.Log("Abilities disponibles dans la liste transmise :");
            foreach (var _ability in abilities)
            {
                Debug.Log($" - {_ability.abilityName}");
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
                    AbilityConfig cfg = FindAbilityConfigByName(abilities, name);

                    if (cfg != null)
                    {
                        if (cfg.syncedWithHost || GameNetworkManager.Instance.isHostingGame)
                        {
                            ability = OldApplyCfgToAbility(ability, cfg);
                        }
                    }
                }
            }

            return ability;
        }

        public static AbilityConfig FindAbilityConfigByName(List<AbilityBase> abilities, string name, bool check = false)
        {
            AbilityConfig abilityCfg = null;
            AbilityBase ability = FindAbilityByName(abilities, name, true);

            if (ability == null) { return null; }

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
                if (GameNetworkManager.Instance.isHostingGame && !check)
                {
                    abilityConfigs.Add(OldAbilityToCfg(ability));
                }
                if (!GameNetworkManager.Instance.isHostingGame && GameNetworkManager.Instance?.localPlayerController != null && !check)
                {
                    // Client
                    Debug.LogMessage("Attempting request...");

                    NetworkHandler.Instance.EventSendRpc(".requestAbilityConfig", new(__ulong: GameNetworkManager.Instance.localPlayerController.actualClientId, __string: name));
                }
            }

            return abilityCfg;
        }

        public static List<AbilityConfig> OldADataToCfgs(List<AbilityBase> abilities, string data)
        {
            List<AbilityConfig> newACfgs = new();

            Debug.LogWarning("_______ Cfg Input! \r\n" + data.Replace("\t", "").Replace("\r\n", ""));
            foreach (var aCfgS in data.Replace("\t", "").Replace("\r\n", "").Split('}'))
            {
                if (string.IsNullOrEmpty(aCfgS)) continue;

                var sCfData = OldADataToCfg(abilities, aCfgS);

                if (sCfData != null)
                { 
                    newACfgs.Add(sCfData);
                }

            }

            return newACfgs;
        }
        public static void OldAbilitiesToCfg(List<AbilityBase> abilities)
        {
            abilityConfigs = new();
            foreach (var ability in abilities)
            {
                if (ability.abilityCategory != "HIDDEN")
                {
                    abilityConfigs.Add(OldAbilityToCfg(ability));
                }
            }
        }

        #endregion

        #region New Ability Config
        
        #endregion
    }
}