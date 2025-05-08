using BepInEx.Configuration;
using Debugger;
using LethalCompanyInputUtils.Api;
using LethalCompanyInputUtils.BindingPathEnums;
using UnityEngine;
using UnityEngine.InputSystem;

namespace HideAndSeek
{
    public class ConfigAbility
    {
        // Taunt
        public static ConfigEntry<int> tauntCost;
        public static ConfigEntry<bool> tauntForSeekers;
        public static ConfigEntry<bool> tauntForHiders;
        public static ConfigEntry<bool> tauntOnRoundActivation;
        public static ConfigEntry<bool> tauntWhenSeekerInside;
        public static ConfigEntry<float> tauntDelay;
        public static ConfigEntry<bool> tauntOneTimeUse;

        // Key
        public static ConfigEntry<int> keyCost;
        public static ConfigEntry<bool> keyForSeekers;
        public static ConfigEntry<bool> keyForHiders;
        public static ConfigEntry<bool> keyOnRoundActivation;
        public static ConfigEntry<bool> keyWhenSeekerInside;
        public static ConfigEntry<float> keyDelay;
        public static ConfigEntry<bool> keyOneTimeUse;

        // TZP-Inhalant
        public static ConfigEntry<int> tzpInhalantCost;
        public static ConfigEntry<bool> tzpInhalantForSeekers;
        public static ConfigEntry<bool> tzpInhalantForHiders;
        public static ConfigEntry<bool> tzpInhalantOnRoundActivation;
        public static ConfigEntry<bool> tzpInhalantWhenSeekerInside;
        public static ConfigEntry<float> tzpInhalantDelay;
        public static ConfigEntry<bool> tzpInhalantOneTimeUse;

        // Shovel
        public static ConfigEntry<int> shovelCost;
        public static ConfigEntry<bool> shovelForSeekers;
        public static ConfigEntry<bool> shovelForHiders;
        public static ConfigEntry<bool> shovelOnRoundActivation;
        public static ConfigEntry<bool> shovelWhenSeekerInside;
        public static ConfigEntry<float> shovelDelay;
        public static ConfigEntry<bool> shovelOneTimeUse;

        // Stun Grenade
        public static ConfigEntry<int> stunGrenadeCost;
        public static ConfigEntry<bool> stunGrenadeForSeekers;
        public static ConfigEntry<bool> stunGrenadeForHiders;
        public static ConfigEntry<bool> stunGrenadeOnRoundActivation;
        public static ConfigEntry<bool> stunGrenadeWhenSeekerInside;
        public static ConfigEntry<float> stunGrenadeDelay;
        public static ConfigEntry<bool> stunGrenadeOneTimeUse;

        // Flashlight
        public static ConfigEntry<int> flashlightCost;
        public static ConfigEntry<bool> flashlightForSeekers;
        public static ConfigEntry<bool> flashlightForHiders;
        public static ConfigEntry<bool> flashlightOnRoundActivation;
        public static ConfigEntry<bool> flashlightWhenSeekerInside;
        public static ConfigEntry<float> flashlightDelay;
        public static ConfigEntry<bool> flashlightOneTimeUse;

        // Laser pointer
        public static ConfigEntry<int> laserPointerCost;
        public static ConfigEntry<bool> laserPointerForSeekers;
        public static ConfigEntry<bool> laserPointerForHiders;
        public static ConfigEntry<bool> laserPointerOnRoundActivation;
        public static ConfigEntry<bool> laserPointerWhenSeekerInside;
        public static ConfigEntry<float> laserPointerDelay;
        public static ConfigEntry<bool> laserPointerOneTimeUse;

        // Walkie-Talkie
        public static ConfigEntry<int> walkieTalkieCost;
        public static ConfigEntry<bool> walkieTalkieForSeekers;
        public static ConfigEntry<bool> walkieTalkieForHiders;
        public static ConfigEntry<bool> walkieTalkieOnRoundActivation;
        public static ConfigEntry<bool> walkieTalkieWhenSeekerInside;
        public static ConfigEntry<float> walkieTalkieDelay;
        public static ConfigEntry<bool> walkieTalkieOneTimeUse;

        // Critical injury
        public static ConfigEntry<int> criticalInjuryCost;
        public static ConfigEntry<bool> criticalInjuryForSeekers;
        public static ConfigEntry<bool> criticalInjuryForHiders;
        public static ConfigEntry<bool> criticalInjuryOnRoundActivation;
        public static ConfigEntry<bool> criticalInjuryWhenSeekerInside;
        public static ConfigEntry<float> criticalInjuryDelay;
        public static ConfigEntry<bool> criticalInjuryOneTimeUse;

        // Teleport
        public static ConfigEntry<int> teleportCost;
        public static ConfigEntry<bool> teleportForSeekers;
        public static ConfigEntry<bool> teleportForHiders;
        public static ConfigEntry<bool> teleportOnRoundActivation;
        public static ConfigEntry<bool> teleportWhenSeekerInside;
        public static ConfigEntry<float> teleportDelay;
        public static ConfigEntry<bool> teleportOneTimeUse;

        // Swap
        public static ConfigEntry<int> swapCost;
        public static ConfigEntry<bool> swapForSeekers;
        public static ConfigEntry<bool> swapForHiders;
        public static ConfigEntry<bool> swapOnRoundActivation;
        public static ConfigEntry<bool> swapWhenSeekerInside;
        public static ConfigEntry<float> swapDelay;
        public static ConfigEntry<bool> swapOneTimeUse;

        // Stealth
        public static ConfigEntry<int> stealthCost;
        public static ConfigEntry<bool> stealthForSeekers;
        public static ConfigEntry<bool> stealthForHiders;
        public static ConfigEntry<bool> stealthOnRoundActivation;
        public static ConfigEntry<bool> stealthWhenSeekerInside;
        public static ConfigEntry<float> stealthDelay;
        public static ConfigEntry<bool> stealthOneTimeUse;

        // Long Stealth
        public static ConfigEntry<int> longStealthCost;
        public static ConfigEntry<bool> longStealthForSeekers;
        public static ConfigEntry<bool> longStealthForHiders;
        public static ConfigEntry<bool> longStealthOnRoundActivation;
        public static ConfigEntry<bool> longStealthWhenSeekerInside;
        public static ConfigEntry<float> longStealthDelay;
        public static ConfigEntry<bool> longStealthOneTimeUse;

        // Invisibility
        public static ConfigEntry<int> invisibilityCost;
        public static ConfigEntry<bool> invisibilityForSeekers;
        public static ConfigEntry<bool> invisibilityForHiders;
        public static ConfigEntry<bool> invisibilityOnRoundActivation;
        public static ConfigEntry<bool> invisibilityWhenSeekerInside;
        public static ConfigEntry<float> invisibilityDelay;
        public static ConfigEntry<bool> invisibilityOneTimeUse;

        // Spawn Loot Bug
        public static ConfigEntry<int> lootBugCost;
        public static ConfigEntry<bool> lootBugForSeekers;
        public static ConfigEntry<bool> lootBugForHiders;
        public static ConfigEntry<bool> lootBugOnRoundActivation;
        public static ConfigEntry<bool> lootBugWhenSeekerInside;
        public static ConfigEntry<float> lootBugDelay;
        public static ConfigEntry<bool> lootBugOneTimeUse;

        // Spawn Mimic
        public static ConfigEntry<int> mimicCost;
        public static ConfigEntry<bool> mimicForSeekers;
        public static ConfigEntry<bool> mimicForHiders;
        public static ConfigEntry<bool> mimicOnRoundActivation;
        public static ConfigEntry<bool> mimicWhenSeekerInside;
        public static ConfigEntry<float> mimicDelay;
        public static ConfigEntry<bool> mimicOneTimeUse;

        // Spawn Thunper
        public static ConfigEntry<int> thumperCost;
        public static ConfigEntry<bool> thumperForSeekers;
        public static ConfigEntry<bool> thumperForHiders;
        public static ConfigEntry<bool> thumperOnRoundActivation;
        public static ConfigEntry<bool> thumperWhenSeekerInside;
        public static ConfigEntry<float> thumperDelay;
        public static ConfigEntry<bool> thumperOneTimeUse;

        // Spawn Bracken
        public static ConfigEntry<int> brackenCost;
        public static ConfigEntry<bool> brackenForSeekers;
        public static ConfigEntry<bool> brackenForHiders;
        public static ConfigEntry<bool> brackenOnRoundActivation;
        public static ConfigEntry<bool> brackenWhenSeekerInside;
        public static ConfigEntry<float> brackenDelay;
        public static ConfigEntry<bool> brackenOneTimeUse;

        // Spawn Turret
        public static ConfigEntry<int> turretCost;
        public static ConfigEntry<bool> turretForSeekers;
        public static ConfigEntry<bool> turretForHiders;
        public static ConfigEntry<bool> turretOnRoundActivation;
        public static ConfigEntry<bool> turretWhenSeekerInside;
        public static ConfigEntry<float> turretDelay;
        public static ConfigEntry<bool> turretOneTimeUse;

        // Spawn Landmine
        public static ConfigEntry<int> landmineCost;
        public static ConfigEntry<bool> landmineForSeekers;
        public static ConfigEntry<bool> landmineForHiders;
        public static ConfigEntry<bool> landmineOnRoundActivation;
        public static ConfigEntry<bool> landmineWhenSeekerInside;
        public static ConfigEntry<float> landmineDelay;
        public static ConfigEntry<bool> landmineOneTimeUse;

        // Heat seeking
        public static ConfigEntry<int> heatSeekingCost;
        public static ConfigEntry<bool> heatSeekingForSeekers;
        public static ConfigEntry<bool> heatSeekingForHiders;
        public static ConfigEntry<bool> heatSeekingOnRoundActivation;
        public static ConfigEntry<bool> heatSeekingWhenSeekerInside;
        public static ConfigEntry<float> heatSeekingDelay;
        public static ConfigEntry<bool> heatSeekingOneTimeUse;

        public ConfigAbility(ConfigFile cfg) 
        {
            #region Taunt
            tauntCost = cfg.Bind<int>(
                "Taunt Ability",
                "TauntCost",
                0,
                "The cost of that ability."
            );
            tauntDelay = cfg.Bind<float>(
                "Taunt Ability",
                "TauntDelay",
                25,
                "The delay between uses of that ability (In seconds)."
            );
            tauntForSeekers = cfg.Bind<bool>(
                "Taunt Ability",
                "TauntForSeekers",
                false,
                "Is that ability can be use by the seekers ?"
            );
            tauntForHiders = cfg.Bind<bool>(
                "Taunt Ability",
                "TauntForHiders",
                true,
                "Is that ability can be use by the seekers ?"
            );
            tauntOnRoundActivation = cfg.Bind<bool>(
                "Taunt Ability",
                "TauntOnRoundActivation",
                true,
                "Can the ability only be used in one round?"
            );
            tauntWhenSeekerInside = cfg.Bind<bool>(
                "Taunt Ability",
                "TauntWhenSeekerInside",
                true,
                "Can the ability only be used when the seekers was ready to seek?"
            );
            tauntOneTimeUse = cfg.Bind<bool>(
                "Taunt Ability",
                "TauntOneTimeUse",
                false,
                "Can the ability only be used one time?"
            );
            #endregion
            #region Key
            keyCost = cfg.Bind<int>(
                "Key Ability",
                "KeyCost",
                245,
                "The cost of that ability."
            );
            keyDelay = cfg.Bind<float>(
                "Key Ability",
                "KeyDelay",
                60,
                "The delay between uses of that ability (In seconds)."
            );
            keyForSeekers = cfg.Bind<bool>(
                "Key Ability",
                "KeyForSeekers",
                true,
                "Is that ability can be use by the seekers ?"
            );
            keyForHiders = cfg.Bind<bool>(
                "Key Ability",
                "KeyForHiders",
                true,
                "Is that ability can be use by the seekers ?"
            );
            keyOnRoundActivation = cfg.Bind<bool>(
                "Key Ability",
                "KeyOnRoundActivation",
                true,
                "Can the ability only be used in one round?"
            );
            keyWhenSeekerInside = cfg.Bind<bool>(
                "Key Ability",
                "KeyWhenSeekerInside",
                false,
                "Can the ability only be used when the seekers was ready to seek?"
            );
            keyOneTimeUse = cfg.Bind<bool>(
                "Key Ability",
                "KeyOneTimeUse",
                false,
                "Can the ability only be used one time?"
            );
            #endregion
            #region Tzp-Inhalant
            tzpInhalantCost = cfg.Bind<int>(
                "Tzp-Inhalant Ability",
                "TzpInhalantCost",
                205,
                "The cost of that ability."
            );
            tzpInhalantDelay = cfg.Bind<float>(
                "Tzp-Inhalant Ability",
                "TzpInhalantDelay",
                60,
                "The delay between uses of that ability (In seconds)."
            );
            tzpInhalantForSeekers = cfg.Bind<bool>(
                "Tzp-Inhalant Ability",
                "TzpInhalantForSeekers",
                true,
                "Is that ability can be use by the seekers ?"
            );
            tzpInhalantForHiders = cfg.Bind<bool>(
                "Tzp-Inhalant Ability",
                "TzpInhalantForHiders",
                true,
                "Is that ability can be use by the seekers ?"
            );
            tzpInhalantOnRoundActivation = cfg.Bind<bool>(
                "Tzp-Inhalant Ability",
                "TzpInhalantOnRoundActivation",
                true,
                "Can the ability only be used in one round?"
            );
            tzpInhalantWhenSeekerInside = cfg.Bind<bool>(
                "Tzp-Inhalant Ability",
                "TzpInhalantWhenSeekerInside",
                false,
                "Can the ability only be used when the seekers was ready to seek?"
            );
            tzpInhalantOneTimeUse = cfg.Bind<bool>(
                "Tzp-Inhalant Ability",
                "TzpInhalantOneTimeUse",
                false,
                "Can the ability only be used one time?"
            );
            #endregion
            #region Shovel
            shovelCost = cfg.Bind<int>(
                "Shovel Ability",
                "ShovelCost",
                495,
                "The cost of that ability."
            );
            shovelDelay = cfg.Bind<float>(
                "Shovel Ability",
                "ShovelDelay",
                10,
                "The delay between uses of that ability (In seconds)."
            );
            shovelForSeekers = cfg.Bind<bool>(
                "Shovel Ability",
                "ShovelForSeekers",
                false,
                "Is that ability can be use by the seekers ?"
            );
            shovelForHiders = cfg.Bind<bool>(
                "Shovel Ability",
                "ShovelForHiders",
                true,
                "Is that ability can be use by the seekers ?"
            );
            shovelOnRoundActivation = cfg.Bind<bool>(
                "Shovel Ability",
                "ShovelOnRoundActivation",
                true,
                "Can the ability only be used in one round?"
            );
            shovelWhenSeekerInside = cfg.Bind<bool>(
                "Shovel Ability",
                "ShovelWhenSeekerInside",
                false,
                "Can the ability only be used when the seekers was ready to seek?"
            );
            shovelOneTimeUse = cfg.Bind<bool>(
                "Shovel Ability",
                "ShovelOneTimeUse",
                false,
                "Can the ability only be used one time?"
            );
            #endregion
            #region Stun Grenade
            stunGrenadeCost = cfg.Bind<int>(
                "Stun Grenade Ability",
                "StunGrenadeCost",
                205,
                "The cost of that ability."
            );
            stunGrenadeDelay = cfg.Bind<float>(
                "Stun Grenade Ability",
                "StunGrenadeDelay",
                120,
                "The delay between uses of that ability (In seconds)."
            );
            stunGrenadeForSeekers = cfg.Bind<bool>(
                "Stun Grenade Ability",
                "StunGrenadeForSeekers",
                false,
                "Is that ability can be use by the seekers ?"
            );
            stunGrenadeForHiders = cfg.Bind<bool>(
                "Stun Grenade Ability",
                "StunGrenadeForHiders",
                true,
                "Is that ability can be use by the seekers ?"
            );
            stunGrenadeOnRoundActivation = cfg.Bind<bool>(
                "Stun Grenade Ability",
                "StunGrenadeOnRoundActivation",
                true,
                "Can the ability only be used in one round?"
            );
            stunGrenadeWhenSeekerInside = cfg.Bind<bool>(
                "Stun Grenade Ability",
                "StunGrenadeWhenSeekerInside",
                false,
                "Can the ability only be used when the seekers was ready to seek?"
            );
            stunGrenadeOneTimeUse = cfg.Bind<bool>(
                "Stun Grenade Ability",
                "StunGrenadeOneTimeUse",
                false,
                "Can the ability only be used one time?"
            );
            #endregion
            #region Flashlight
            flashlightCost = cfg.Bind<int>(
                "Flashlight Ability",
                "FlashlightCost",
                0,
                "The cost of that ability."
            );
            flashlightDelay = cfg.Bind<float>(
                "Flashlight Ability",
                "FlashlightDelay",
                10,
                "The delay between uses of that ability (In seconds)."
            );
            flashlightForSeekers = cfg.Bind<bool>(
                "Flashlight Ability",
                "FlashlightForSeekers",
                true,
                "Is that ability can be use by the seekers ?"
            );
            flashlightForHiders = cfg.Bind<bool>(
                "Flashlight Ability",
                "FlashlightForHiders",
                true,
                "Is that ability can be use by the seekers ?"
            );
            flashlightOnRoundActivation = cfg.Bind<bool>(
                "Flashlight Ability",
                "FlashlightOnRoundActivation",
                true,
                "Can the ability only be used in one round?"
            );
            flashlightWhenSeekerInside = cfg.Bind<bool>(
                "Flashlight Ability",
                "FlashlightWhenSeekerInside",
                false,
                "Can the ability only be used when the seekers was ready to seek?"
            );
            flashlightOneTimeUse = cfg.Bind<bool>(
                "Flashlight Ability",
                "FlashlightOneTimeUse",
                false,
                "Can the ability only be used one time?"
            );
            #endregion
            #region Laser Pointer
            laserPointerCost = cfg.Bind<int>(
                "Laser Pointer Ability",
                "LaserPointerCost",
                250,
                "The cost of that ability."
            );
            laserPointerDelay = cfg.Bind<float>(
                "Laser Pointer Ability",
                "LaserPointerDelay",
                10,
                "The delay between uses of that ability (In seconds)."
            );
            laserPointerForSeekers = cfg.Bind<bool>(
                "Laser Pointer Ability",
                "LaserPointerForSeekers",
                true,
                "Is that ability can be use by the seekers ?"
            );
            laserPointerForHiders = cfg.Bind<bool>(
                "Laser Pointer Ability",
                "LaserPointerForHiders",
                true,
                "Is that ability can be use by the seekers ?"
            );
            laserPointerOnRoundActivation = cfg.Bind<bool>(
                "Laser Pointer Ability",
                "LaserPointerOnRoundActivation",
                true,
                "Can the ability only be used in one round?"
            );
            laserPointerWhenSeekerInside = cfg.Bind<bool>(
                "Laser Pointer Ability",
                "LaserPointerWhenSeekerInside",
                false,
                "Can the ability only be used when the seekers was ready to seek?"
            );
            laserPointerOneTimeUse = cfg.Bind<bool>(
                "Laser Pointer Ability",
                "LaserPointerOneTimeUse",
                true,
                "Can the ability only be used one time?"
            );
            #endregion
            #region Walkie Talkie
            walkieTalkieCost = cfg.Bind<int>(
                "Walkie Talkie Ability",
                "WalkieTalkieCost",
                0,
                "The cost of that ability."
            );
            walkieTalkieDelay = cfg.Bind<float>(
                "Walkie Talkie Ability",
                "WalkieTalkieDelay",
                10,
                "The delay between uses of that ability (In seconds)."
            );
            walkieTalkieForSeekers = cfg.Bind<bool>(
                "Walkie Talkie Ability",
                "WalkieTalkieForSeekers",
                false,
                "Is that ability can be use by the seekers ?"
            );
            walkieTalkieForHiders = cfg.Bind<bool>(
                "Walkie Talkie Ability",
                "WalkieTalkieForHiders",
                true,
                "Is that ability can be use by the seekers ?"
            );
            walkieTalkieOnRoundActivation = cfg.Bind<bool>(
                "Walkie Talkie Ability",
                "WalkieTalkieOnRoundActivation",
                true,
                "Can the ability only be used in one round?"
            );
            walkieTalkieWhenSeekerInside = cfg.Bind<bool>(
                "Walkie Talkie Ability",
                "WalkieTalkieWhenSeekerInside",
                false,
                "Can the ability only be used when the seekers was ready to seek?"
            );
            walkieTalkieOneTimeUse = cfg.Bind<bool>(
                "Walkie Talkie Ability",
                "WalkieTalkieOneTimeUse",
                false,
                "Can the ability only be used one time?"
            );
            #endregion
            #region Critical Injury
            criticalInjuryCost = cfg.Bind<int>(
                "Critical Injury Ability",
                "CriticalInjuryCost",
                200,
                "The cost of that ability."
            );
            criticalInjuryDelay = cfg.Bind<float>(
                "Critical Injury Ability",
                "CriticalInjuryDelay",
                120,
                "The delay between uses of that ability (In seconds)."
            );
            criticalInjuryForSeekers = cfg.Bind<bool>(
                "Critical Injury Ability",
                "CriticalInjuryForSeekers",
                true,
                "Is that ability can be use by the seekers ?"
            );
            criticalInjuryForHiders = cfg.Bind<bool>(
                "Critical Injury Ability",
                "CriticalInjuryForHiders",
                true,
                "Is that ability can be use by the seekers ?"
            );
            criticalInjuryOnRoundActivation = cfg.Bind<bool>(
                "Critical Injury Ability",
                "CriticalInjuryOnRoundActivation",
                true,
                "Can the ability only be used in one round?"
            );
            criticalInjuryWhenSeekerInside = cfg.Bind<bool>(
                "Critical Injury Ability",
                "CriticalInjuryWhenSeekerInside",
                true,
                "Can the ability only be used when the seekers was ready to seek?"
            );
            criticalInjuryOneTimeUse = cfg.Bind<bool>(
                "Critical Injury Ability",
                "CriticalInjuryOneTimeUse",
                false,
                "Can the ability only be used one time?"
            );
            #endregion
            #region Teleport
            teleportCost = cfg.Bind<int>(
                "Teleport Ability",
                "TeleportCost",
                400,
                "The cost of that ability."
            );
            teleportDelay = cfg.Bind<float>(
                "Teleport Ability",
                "TeleportDelay",
                60,
                "The delay between uses of that ability (In seconds)."
            );
            teleportForSeekers = cfg.Bind<bool>(
                "Teleport Ability",
                "TeleportForSeekers",
                false,
                "Is that ability can be use by the seekers ?"
            );
            teleportForHiders = cfg.Bind<bool>(
                "Teleport Ability",
                "TeleportForHiders",
                true,
                "Is that ability can be use by the seekers ?"
            );
            teleportOnRoundActivation = cfg.Bind<bool>(
                "Teleport Ability",
                "TeleportOnRoundActivation",
                true,
                "Can the ability only be used in one round?"
            );
            teleportWhenSeekerInside = cfg.Bind<bool>(
                "Teleport Ability",
                "TeleportWhenSeekerInside",
                false,
                "Can the ability only be used when the seekers was ready to seek?"
            );
            teleportOneTimeUse = cfg.Bind<bool>(
                "Teleport Ability",
                "TeleportOneTimeUse",
                false,
                "Can the ability only be used one time?"
            );
            #endregion
            #region Swap
            swapCost = cfg.Bind<int>(
                "Swap Ability",
                "SwapCost",
                450,
                "The cost of that ability."
            );
            swapDelay = cfg.Bind<float>(
                "Swap Ability",
                "SwapDelay",
                60,
                "The delay between uses of that ability (In seconds)."
            );
            swapForSeekers = cfg.Bind<bool>(
                "Swap Ability",
                "SwapForSeekers",
                false,
                "Is that ability can be use by the seekers ?"
            );
            swapForHiders = cfg.Bind<bool>(
                "Swap Ability",
                "SwapForHiders",
                true,
                "Is that ability can be use by the seekers ?"
            );
            swapOnRoundActivation = cfg.Bind<bool>(
                "Swap Ability",
                "SwapOnRoundActivation",
                true,
                "Can the ability only be used in one round?"
            );
            swapWhenSeekerInside = cfg.Bind<bool>(
                "Swap Ability",
                "SwapWhenSeekerInside",
                false,
                "Can the ability only be used when the seekers was ready to seek?"
            );
            swapOneTimeUse = cfg.Bind<bool>(
                "Swap Ability",
                "SwapOneTimeUse",
                false,
                "Can the ability only be used one time?"
            );
            #endregion
            #region Stealth
            stealthCost = cfg.Bind<int>(
                "Stealth Ability",
                "StealthCost",
                30,
                "The cost of that ability."
            );
            stealthDelay = cfg.Bind<float>(
                "Stealth Ability",
                "StealthDelay",
                45,
                "The delay between uses of that ability (In seconds)."
            );
            stealthForSeekers = cfg.Bind<bool>(
                "Stealth Ability",
                "StealthForSeekers",
                true,
                "Is that ability can be use by the seekers ?"
            );
            stealthForHiders = cfg.Bind<bool>(
                "Stealth Ability",
                "StealthForHiders",
                true,
                "Is that ability can be use by the seekers ?"
            );
            stealthOnRoundActivation = cfg.Bind<bool>(
                "Stealth Ability",
                "StealthOnRoundActivation",
                true,
                "Can the ability only be used in one round?"
            );
            stealthWhenSeekerInside = cfg.Bind<bool>(
                "Stealth Ability",
                "StealthWhenSeekerInside",
                true,
                "Can the ability only be used when the seekers was ready to seek?"
            );
            stealthOneTimeUse = cfg.Bind<bool>(
                "Stealth Ability",
                "StealthOneTimeUse",
                false,
                "Can the ability only be used one time?"
            );
            #endregion
            #region Long Stealth
            longStealthCost = cfg.Bind<int>(
                "Long Stealth Ability",
                "LongStealthCost",
                259,
                "The cost of that ability."
            );
            longStealthDelay = cfg.Bind<float>(
                "Long Stealth Ability",
                "LongStealthDelay",
                10,
                "The delay between uses of that ability (In seconds)."
            );
            longStealthForSeekers = cfg.Bind<bool>(
                "Long Stealth Ability",
                "LongStealthForSeekers",
                true,
                "Is that ability can be use by the seekers ?"
            );
            longStealthForHiders = cfg.Bind<bool>(
                "Long Stealth Ability",
                "LongStealthForHiders",
                true,
                "Is that ability can be use by the seekers ?"
            );
            longStealthOnRoundActivation = cfg.Bind<bool>(
                "Long Stealth Ability",
                "LongStealthOnRoundActivation",
                true,
                "Can the ability only be used in one round?"
            );
            longStealthWhenSeekerInside = cfg.Bind<bool>(
                "Long Stealth Ability",
                "LongStealthWhenSeekerInside",
                true,
                "Can the ability only be used when the seekers was ready to seek?"
            );
            longStealthOneTimeUse = cfg.Bind<bool>(
                "Long Stealth Ability",
                "LongStealthOneTimeUse",
                true,
                "Can the ability only be used one time?"
            );
            #endregion
            #region Invisibility
            invisibilityCost = cfg.Bind<int>(
                "Invisibility Ability",
                "InvisibilityCost",
                500,
                "The cost of that ability."
            );
            invisibilityDelay = cfg.Bind<float>(
                "Invisibility Ability",
                "InvisibilityDelay",
                45,
                "The delay between uses of that ability (In seconds)."
            );
            invisibilityForSeekers = cfg.Bind<bool>(
                "Invisibility Ability",
                "InvisibilityForSeekers",
                true,
                "Is that ability can be use by the seekers ?"
            );
            invisibilityForHiders = cfg.Bind<bool>(
                "Invisibility Ability",
                "InvisibilityForHiders",
                true,
                "Is that ability can be use by the seekers ?"
            );
            invisibilityOnRoundActivation = cfg.Bind<bool>(
                "Invisibility Ability",
                "InvisibilityOnRoundActivation",
                true,
                "Can the ability only be used in one round?"
            );
            invisibilityWhenSeekerInside = cfg.Bind<bool>(
                "Invisibility Ability",
                "InvisibilityWhenSeekerInside",
                true,
                "Can the ability only be used when the seekers was ready to seek?"
            );
            invisibilityOneTimeUse = cfg.Bind<bool>(
                "Invisibility Ability",
                "InvisibilityOneTimeUse",
                false,
                "Can the ability only be used one time?"
            );
            #endregion
            #region Spawn Loot Bug
            lootBugCost = cfg.Bind<int>(
                "Spawn Loot Bug Ability",
                "LootBugCost",
                99,
                "The cost of that ability."
            );
            lootBugDelay = cfg.Bind<float>(
                "Spawn Loot Bug Ability",
                "LootBugDelay",
                80,
                "The delay between uses of that ability (In seconds)."
            );
            lootBugForSeekers = cfg.Bind<bool>(
                "Spawn Loot Bug Ability",
                "LootBugForSeekers",
                true,
                "Is that ability can be use by the seekers ?"
            );
            lootBugForHiders = cfg.Bind<bool>(
                "Spawn Loot Bug Ability",
                "LootBugForHiders",
                false,
                "Is that ability can be use by the seekers ?"
            );
            lootBugOnRoundActivation = cfg.Bind<bool>(
                "Spawn Loot Bug Ability",
                "LootBugOnRoundActivation",
                true,
                "Can the ability only be used in one round?"
            );
            lootBugWhenSeekerInside = cfg.Bind<bool>(
                "Spawn Loot Bug Ability",
                "LootBugWhenSeekerInside",
                true,
                "Can the ability only be used when the seekers was ready to seek?"
            );
            lootBugOneTimeUse = cfg.Bind<bool>(
                "Spawn Loot Bug Ability",
                "LootBugOneTimeUse",
                false,
                "Can the ability only be used one time?"
            );
            #endregion
            #region Spawn Mimic
            mimicCost = cfg.Bind<int>(
                "Spawn Mimic Ability",
                "MimicCost",
                200,
                "The cost of that ability."
            );
            mimicDelay = cfg.Bind<float>(
                "Spawn Mimic Ability",
                "MimicDelay",
                120,
                "The delay between uses of that ability (In seconds)."
            );
            mimicForSeekers = cfg.Bind<bool>(
                "Spawn Mimic Ability",
                "MimicForSeekers",
                true,
                "Is that ability can be use by the seekers ?"
            );
            mimicForHiders = cfg.Bind<bool>(
                "Spawn Mimic Ability",
                "MimicForHiders",
                false,
                "Is that ability can be use by the seekers ?"
            );
            mimicOnRoundActivation = cfg.Bind<bool>(
                "Spawn Mimic Ability",
                "MimicOnRoundActivation",
                true,
                "Can the ability only be used in one round?"
            );
            mimicWhenSeekerInside = cfg.Bind<bool>(
                "Spawn Mimic Ability",
                "MimicWhenSeekerInside",
                true,
                "Can the ability only be used when the seekers was ready to seek?"
            );
            mimicOneTimeUse = cfg.Bind<bool>(
                "Spawn Mimic Ability",
                "MimicOneTimeUse",
                false,
                "Can the ability only be used one time?"
            );
            #endregion
            #region Spawn Thumper
            thumperCost = cfg.Bind<int>(
                "Spawn Thumper Ability",
                "ThumperCost",
                500,
                "The cost of that ability."
            );
            thumperDelay = cfg.Bind<float>(
                "Spawn Thumper Ability",
                "ThumperDelay",
                120,
                "The delay between uses of that ability (In seconds)."
            );
            thumperForSeekers = cfg.Bind<bool>(
                "Spawn Thumper Ability",
                "ThumperForSeekers",
                true,
                "Is that ability can be use by the seekers ?"
            );
            thumperForHiders = cfg.Bind<bool>(
                "Spawn Thumper Ability",
                "ThumperForHiders",
                false,
                "Is that ability can be use by the seekers ?"
            );
            thumperOnRoundActivation = cfg.Bind<bool>(
                "Spawn Thumper Ability",
                "ThumperOnRoundActivation",
                true,
                "Can the ability only be used in one round?"
            );
            thumperWhenSeekerInside = cfg.Bind<bool>(
                "Spawn Thumper Ability",
                "ThumperWhenSeekerInside",
                true,
                "Can the ability only be used when the seekers was ready to seek?"
            );
            thumperOneTimeUse = cfg.Bind<bool>(
                "Spawn Thumper Ability",
                "ThumperOneTimeUse",
                false,
                "Can the ability only be used one time?"
            );
            #endregion
            #region Spawn Bracken
            brackenCost = cfg.Bind<int>(
                "Spawn Bracken Ability",
                "BrackenCost",
                750,
                "The cost of that ability."
            );
            brackenDelay = cfg.Bind<float>(
                "Spawn Bracken Ability",
                "BrackenDelay",
                120,
                "The delay between uses of that ability (In seconds)."
            );
            brackenForSeekers = cfg.Bind<bool>(
                "Spawn Bracken Ability",
                "BrackenForSeekers",
                true,
                "Is that ability can be use by the seekers ?"
            );
            brackenForHiders = cfg.Bind<bool>(
                "Spawn Bracken Ability",
                "BrackenForHiders",
                false,
                "Is that ability can be use by the seekers ?"
            );
            brackenOnRoundActivation = cfg.Bind<bool>(
                "Spawn Bracken Ability",
                "BrackenOnRoundActivation",
                true,
                "Can the ability only be used in one round?"
            );
            brackenWhenSeekerInside = cfg.Bind<bool>(
                "Spawn Bracken Ability",
                "BrackenWhenSeekerInside",
                true,
                "Can the ability only be used when the seekers was ready to seek?"
            );
            brackenOneTimeUse = cfg.Bind<bool>(
                "Spawn Bracken Ability",
                "BrackenOneTimeUse",
                false,
                "Can the ability only be used one time?"
            );
            #endregion
            #region Spawn Turret
            turretCost = cfg.Bind<int>(
                "Spawn Turret Ability",
                "TurretCost",
                299,
                "The cost of that ability."
            );
            turretDelay = cfg.Bind<float>(
                "Spawn Turret Ability",
                "TurretDelay",
                30,
                "The delay between uses of that ability (In seconds)."
            );
            turretForSeekers = cfg.Bind<bool>(
                "Spawn Turret Ability",
                "TurretForSeekers",
                true,
                "Is that ability can be use by the seekers ?"
            );
            turretForHiders = cfg.Bind<bool>(
                "Spawn Turret Ability",
                "TurretForHiders",
                false,
                "Is that ability can be use by the seekers ?"
            );
            turretOnRoundActivation = cfg.Bind<bool>(
                "Spawn Turret Ability",
                "TurretOnRoundActivation",
                true,
                "Can the ability only be used in one round?"
            );
            turretWhenSeekerInside = cfg.Bind<bool>(
                "Spawn Turret Ability",
                "TurretWhenSeekerInside",
                true,
                "Can the ability only be used when the seekers was ready to seek?"
            );
            turretOneTimeUse = cfg.Bind<bool>(
                "Spawn Turret Ability",
                "TurretOneTimeUse",
                false,
                "Can the ability only be used one time?"
            );
            #endregion
            #region Spawn Landmine
            landmineCost = cfg.Bind<int>(
                "Spawn Landmine Ability",
                "LandmineCost",
                109,
                "The cost of that ability."
            );
            landmineDelay = cfg.Bind<float>(
                "Spawn Landmine Ability",
                "LandmineDelay",
                5,
                "The delay between uses of that ability (In seconds)."
            );
            landmineForSeekers = cfg.Bind<bool>(
                "Spawn Landmine Ability",
                "LandmineForSeekers",
                true,
                "Is that ability can be use by the seekers ?"
            );
            landmineForHiders = cfg.Bind<bool>(
                "Spawn Landmine Ability",
                "LandmineForHiders",
                false,
                "Is that ability can be use by the seekers ?"
            );
            landmineOnRoundActivation = cfg.Bind<bool>(
                "Spawn Landmine Ability",
                "LandmineOnRoundActivation",
                true,
                "Can the ability only be used in one round?"
            );
            landmineWhenSeekerInside = cfg.Bind<bool>(
                "Spawn Landmine Ability",
                "LandmineWhenSeekerInside",
                true,
                "Can the ability only be used when the seekers was ready to seek?"
            );
            landmineOneTimeUse = cfg.Bind<bool>(
                "Spawn Landmine Ability",
                "LandmineOneTimeUse",
                false,
                "Can the ability only be used one time?"
            );
            #endregion
            #region Heat Seeking
            heatSeekingCost = cfg.Bind<int>(
                "Heat Seeking Ability",
                "HeatSeekingCost",
                999,
                "The cost of that ability."
            );
            heatSeekingDelay = cfg.Bind<float>(
                "Heat Seeking Ability",
                "HeatSeekingDelay",
                10,
                "The delay between uses of that ability (In seconds)."
            );
            heatSeekingForSeekers = cfg.Bind<bool>(
                "Heat Seeking Ability",
                "HeatSeekingForSeekers",
                true,
                "Is that ability can be use by the seekers ?"
            );
            heatSeekingForHiders = cfg.Bind<bool>(
                "Heat Seeking Ability",
                "HeatSeekingForHiders",
                false,
                "Is that ability can be use by the seekers ?"
            );
            heatSeekingOnRoundActivation = cfg.Bind<bool>(
                "Heat Seeking Ability",
                "HeatSeekingOnRoundActivation",
                true,
                "Can the ability only be used in one round?"
            );
            heatSeekingWhenSeekerInside = cfg.Bind<bool>(
                "Heat Seeking Ability",
                "HeatSeekingWhenSeekerInside",
                true,
                "Can the ability only be used when the seekers was ready to seek?"
            );
            heatSeekingOneTimeUse = cfg.Bind<bool>(
                "Heat Seeking Ability",
                "HeatSeekingOneTimeUse",
                false,
                "Can the ability only be used one time?"
            );
            #endregion
        }
    }
}
