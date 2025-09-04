using HideAndSeek.AbilityScripts.Enums;
using HideAndSeek.Patches;
using System;
using UnityEngine;

namespace HideAndSeek.AbilityScripts
{
    public class AbilityBase
    {
        // Description
        public string abilityName = EnumAbilitys.GenericNull;
        public string abilityDescription = "genericNullDescription";
        public int abilityCost = 10;
        public string abilityCategory = EnumAbilityCategory.Misc.ToString();

        // Use Restrictions
        public bool seekerAbility = true;
        public bool hiderAbility = true;
        
        public bool requiresRoundActive = true;
        public bool requiresSeekerActive = true;

        public float abilityDelay = 10f;
        public bool oneTimeUse = false;

        // Runtime Variables

        public int timesUsed = 0;
        public bool usedThisRound = false;

        // Events
        public Action<AbilityBase, ulong> serverEvent = null;
        public Action<AbilityBase, ulong, string> clientEvent = null;

        // Runtime Varaibles
        public float lastUsed = -9999f;

        public AbilityBase(string _abilityName = EnumAbilitys.GenericNull,
                           string _abilityDescription = "genericNullDescription",
                           string _abilityCategory = "Misc",
                           int _abilityCost = 10,
                           float _abilityDelay = 10f,
                           bool _oneTimeUse = false,
                           bool _seekerAbility = true,
                           bool _hiderAbility = true,
                           bool _requriesRoundActive = true,
                           bool _requiresSeekerActive = true,
                           Action<AbilityBase, ulong> _serverEvent = null,
                           Action<AbilityBase, ulong, string> _clientEvent = null)
        {
            // Description
            abilityName = _abilityName;
            abilityDescription = _abilityDescription;
            abilityCost = _abilityCost;
            abilityCategory = _abilityCategory;

            // Use Restrictions
            seekerAbility = _seekerAbility;
            hiderAbility = _hiderAbility;

            requiresRoundActive = _requriesRoundActive;
            requiresSeekerActive = _requiresSeekerActive;

            abilityDelay = _abilityDelay;
            oneTimeUse = _oneTimeUse;

            // Events

            if (_serverEvent != null) serverEvent = _serverEvent;
            else serverEvent = Abilities.TemplateServerEvent;

            if (_clientEvent != null) clientEvent = _clientEvent;
            else clientEvent = Abilities.TemplateClientEvent;
        }

        public void ActivateServer(ulong activatorId)
        {
            if (GameNetworkManager.Instance.isHostingGame)
            {
                serverEvent?.Invoke(this, activatorId);
            }
        }
        public void ActivateClient(ulong activatorId, string extraMessage = null, AbilityBase ability = null)
        {
            if (ability == null) { ability = this; Debug.LogWarning("ActivateClient(): Called without ability reference! This could cause problems"); };

            NetworkHandler.Instance.EventSendRpc(".activateAbility", new MessageProperties(__string: ability.abilityName.ToString(), __ulong: activatorId, __extraMessage: extraMessage));
        }
        public void ActivateAbility(ulong activatorId, string extraMessage)
        {
            clientEvent?.Invoke(this, activatorId, extraMessage);
        }

        #region Get abilities properties
        // Methods to get the properties of the abilitys

        /// <summary>
        /// Check if the ability is aviable for the player by their role.
        /// </summary>
        /// <param name="ability"></param>
        /// <param name="isPlayerSeeker"></param>
        /// <returns>[bool] Ability aviability for this role</returns>
        public bool IsAbilityAviableForRole(bool isPlayerSeeker)
        {
            return this.abilityName switch
            {
                EnumAbilitys.Money => isPlayerSeeker ? ConfigAbility.moneyForSeekers.Value : ConfigAbility.moneyForHiders.Value,
                EnumAbilitys.Remote => isPlayerSeeker ? ConfigAbility.remoteForSeekers.Value : ConfigAbility.remoteForHiders.Value,
                EnumAbilitys.Taunt => isPlayerSeeker ? ConfigAbility.tauntForSeekers.Value : ConfigAbility.tauntForHiders.Value,
                EnumAbilitys.Key => isPlayerSeeker ? ConfigAbility.keyForSeekers.Value : ConfigAbility.keyForHiders.Value,
                EnumAbilitys.TzpInhalant => isPlayerSeeker ? ConfigAbility.tzpInhalantForSeekers.Value : ConfigAbility.tzpInhalantForHiders.Value,
                EnumAbilitys.Shovel => isPlayerSeeker ? ConfigAbility.shovelForSeekers.Value : ConfigAbility.shovelForHiders.Value,
                EnumAbilitys.StunGrenade => isPlayerSeeker ? ConfigAbility.stunGrenadeForSeekers.Value : ConfigAbility.stunGrenadeForHiders.Value,
                EnumAbilitys.Flashlight => isPlayerSeeker ? ConfigAbility.flashlightForSeekers.Value : ConfigAbility.flashlightForHiders.Value,
                EnumAbilitys.LaserPointer => isPlayerSeeker ? ConfigAbility.laserPointerForSeekers.Value : ConfigAbility.laserPointerForHiders.Value,
                EnumAbilitys.WalkieTalkie => isPlayerSeeker ? ConfigAbility.walkieTalkieForSeekers.Value : ConfigAbility.walkieTalkieForHiders.Value,
                EnumAbilitys.CriticalInjury => isPlayerSeeker ? ConfigAbility.criticalInjuryForSeekers.Value : ConfigAbility.criticalInjuryForHiders.Value,
                EnumAbilitys.Teleport => isPlayerSeeker ? ConfigAbility.teleportForSeekers.Value : ConfigAbility.teleportForHiders.Value,
                EnumAbilitys.Swap => isPlayerSeeker ? ConfigAbility.swapForSeekers.Value : ConfigAbility.swapForHiders.Value,
                EnumAbilitys.Decoy => isPlayerSeeker ? ConfigAbility.decoyForSeekers.Value : ConfigAbility.decoyForHiders.Value,
                EnumAbilitys.Stealth => isPlayerSeeker ? ConfigAbility.stealthForSeekers.Value : ConfigAbility.stealthForHiders.Value,
                EnumAbilitys.LongStealth => isPlayerSeeker ? ConfigAbility.longStealthForSeekers.Value : ConfigAbility.longStealthForHiders.Value,
                EnumAbilitys.Invisibility => isPlayerSeeker ? ConfigAbility.invisibilityForSeekers.Value : ConfigAbility.invisibilityForHiders.Value,
                EnumAbilitys.SpawnLootBug => isPlayerSeeker ? ConfigAbility.lootBugForSeekers.Value : ConfigAbility.lootBugForHiders.Value,
                EnumAbilitys.SpawnMimic => isPlayerSeeker ? ConfigAbility.mimicForSeekers.Value : ConfigAbility.mimicForHiders.Value,
                EnumAbilitys.SpawnThumper => isPlayerSeeker ? ConfigAbility.thumperForSeekers.Value : ConfigAbility.thumperForHiders.Value,
                EnumAbilitys.SpawnBracken => isPlayerSeeker ? ConfigAbility.brackenForSeekers.Value : ConfigAbility.brackenForHiders.Value,
                EnumAbilitys.SpawnTurret => isPlayerSeeker ? ConfigAbility.turretForSeekers.Value : ConfigAbility.turretForHiders.Value,
                EnumAbilitys.SpawnLandmine => isPlayerSeeker ? ConfigAbility.landmineForSeekers.Value : ConfigAbility.landmineForHiders.Value,
                EnumAbilitys.HeatSeeking => isPlayerSeeker ? ConfigAbility.heatSeekingForSeekers.Value : ConfigAbility.heatSeekingForHiders.Value,
                _ => isPlayerSeeker ? this.seekerAbility : this.hiderAbility,
            };
        }

        /// <summary>
        /// Check if a one time use ability has already be used.
        /// </summary>
        /// <param name="ability"></param>
        /// <returns> [bool] If the ability already used</returns>
        public bool IsAbilityConsumed()
        {
            bool isOneTimeUse = this.abilityName switch
            {
                EnumAbilitys.Money => ConfigAbility.moneyOneTimeUse.Value,
                EnumAbilitys.Remote => ConfigAbility.remoteOneTimeUse.Value,
                EnumAbilitys.Taunt => ConfigAbility.tauntOneTimeUse.Value,
                EnumAbilitys.Key => ConfigAbility.keyOneTimeUse.Value,
                EnumAbilitys.TzpInhalant => ConfigAbility.tzpInhalantOneTimeUse.Value,
                EnumAbilitys.Shovel => ConfigAbility.shovelOneTimeUse.Value,
                EnumAbilitys.StunGrenade => ConfigAbility.stunGrenadeOneTimeUse.Value,
                EnumAbilitys.Flashlight => ConfigAbility.flashlightOneTimeUse.Value,
                EnumAbilitys.LaserPointer => ConfigAbility.laserPointerOneTimeUse.Value,
                EnumAbilitys.WalkieTalkie => ConfigAbility.walkieTalkieOneTimeUse.Value,
                EnumAbilitys.CriticalInjury => ConfigAbility.criticalInjuryOneTimeUse.Value,
                EnumAbilitys.Teleport => ConfigAbility.teleportOneTimeUse.Value,
                EnumAbilitys.Swap => ConfigAbility.swapOneTimeUse.Value,
                EnumAbilitys.Decoy => ConfigAbility.decoyOneTimeUse.Value,
                EnumAbilitys.Stealth => ConfigAbility.stealthOneTimeUse.Value,
                EnumAbilitys.LongStealth => ConfigAbility.longStealthOneTimeUse.Value,
                EnumAbilitys.Invisibility => ConfigAbility.invisibilityOneTimeUse.Value,
                EnumAbilitys.SpawnLootBug => ConfigAbility.lootBugOneTimeUse.Value,
                EnumAbilitys.SpawnMimic => ConfigAbility.mimicOneTimeUse.Value,
                EnumAbilitys.SpawnThumper => ConfigAbility.thumperOneTimeUse.Value,
                EnumAbilitys.SpawnBracken => ConfigAbility.brackenOneTimeUse.Value,
                EnumAbilitys.SpawnTurret => ConfigAbility.turretOneTimeUse.Value,
                EnumAbilitys.SpawnLandmine => ConfigAbility.landmineOneTimeUse.Value,
                _ => this.oneTimeUse,
            };
            return isOneTimeUse && this.usedThisRound;
        }

        /// <summary>
        /// Check the ability was on cooldown.
        /// </summary>
        /// <param name="ability"></param>
        /// <returns> [bool] If the ability was in cooldown</returns>
        public bool IsAbilityOnCooldown()
        {
            return Time.time - this.lastUsed <= this.abilityDelay;
        }

        /// <summary>
        /// Check the ability requires round active to be buyed.
        /// </summary>
        /// <param name="ability"></param>
        /// <returns> [bool] If he was only aviable on a round.</returns>
        public bool IsAbilityOnRoundOnly()
        {
            return this.abilityName switch
            {
                EnumAbilitys.Money => ConfigAbility.moneyOnRoundActivation.Value,
                EnumAbilitys.Remote => ConfigAbility.remoteOnRoundActivation.Value,
                EnumAbilitys.Taunt => ConfigAbility.tauntOnRoundActivation.Value,
                EnumAbilitys.Key => ConfigAbility.keyOnRoundActivation.Value,
                EnumAbilitys.TzpInhalant => ConfigAbility.tzpInhalantOnRoundActivation.Value,
                EnumAbilitys.Shovel => ConfigAbility.shovelOnRoundActivation.Value,
                EnumAbilitys.StunGrenade => ConfigAbility.stunGrenadeOnRoundActivation.Value,
                EnumAbilitys.Flashlight => ConfigAbility.flashlightOnRoundActivation.Value,
                EnumAbilitys.LaserPointer => ConfigAbility.laserPointerOnRoundActivation.Value,
                EnumAbilitys.WalkieTalkie => ConfigAbility.walkieTalkieOnRoundActivation.Value,
                EnumAbilitys.CriticalInjury => ConfigAbility.criticalInjuryOnRoundActivation.Value,
                EnumAbilitys.Teleport => ConfigAbility.teleportOnRoundActivation.Value,
                EnumAbilitys.Swap => ConfigAbility.swapOnRoundActivation.Value,
                EnumAbilitys.Decoy => ConfigAbility.decoyOnRoundActivation.Value,
                EnumAbilitys.Stealth => ConfigAbility.stealthOnRoundActivation.Value,
                EnumAbilitys.LongStealth => ConfigAbility.longStealthOnRoundActivation.Value,
                EnumAbilitys.Invisibility => ConfigAbility.invisibilityOnRoundActivation.Value,
                EnumAbilitys.SpawnLootBug => ConfigAbility.lootBugOnRoundActivation.Value,
                EnumAbilitys.SpawnMimic => ConfigAbility.mimicOnRoundActivation.Value,
                EnumAbilitys.SpawnThumper => ConfigAbility.thumperOnRoundActivation.Value,
                EnumAbilitys.SpawnBracken => ConfigAbility.brackenOnRoundActivation.Value,
                EnumAbilitys.SpawnTurret => ConfigAbility.turretOnRoundActivation.Value,
                EnumAbilitys.SpawnLandmine => ConfigAbility.landmineOnRoundActivation.Value,
                _ => this.requiresRoundActive,
            };
        }

        /// <summary>
        /// Check the ability requires the seekers to be active to be buyed.
        /// </summary>
        /// <param name="ability"></param>
        /// <returns> [bool] Aviability of the ability</returns>
        public bool IsAbilityWhenSeekerActive()
        {
            bool needSeekerActive = this.abilityName switch
            {
                EnumAbilitys.Money => ConfigAbility.moneyWhenSeekerInside.Value,
                EnumAbilitys.Remote => ConfigAbility.remoteWhenSeekerInside.Value,
                EnumAbilitys.Taunt => ConfigAbility.tauntWhenSeekerInside.Value,
                EnumAbilitys.Key => ConfigAbility.keyWhenSeekerInside.Value,
                EnumAbilitys.TzpInhalant => ConfigAbility.tzpInhalantWhenSeekerInside.Value,
                EnumAbilitys.Shovel => ConfigAbility.shovelWhenSeekerInside.Value,
                EnumAbilitys.StunGrenade => ConfigAbility.stunGrenadeWhenSeekerInside.Value,
                EnumAbilitys.Flashlight => ConfigAbility.flashlightWhenSeekerInside.Value,
                EnumAbilitys.LaserPointer => ConfigAbility.laserPointerWhenSeekerInside.Value,
                EnumAbilitys.WalkieTalkie => ConfigAbility.walkieTalkieWhenSeekerInside.Value,
                EnumAbilitys.CriticalInjury => ConfigAbility.criticalInjuryWhenSeekerInside.Value,
                EnumAbilitys.Teleport => ConfigAbility.teleportWhenSeekerInside.Value,
                EnumAbilitys.Swap => ConfigAbility.swapWhenSeekerInside.Value,
                EnumAbilitys.Decoy => ConfigAbility.decoyWhenSeekerInside.Value,
                EnumAbilitys.Stealth => ConfigAbility.stealthWhenSeekerInside.Value,
                EnumAbilitys.LongStealth => ConfigAbility.longStealthWhenSeekerInside.Value,
                EnumAbilitys.Invisibility => ConfigAbility.invisibilityWhenSeekerInside.Value,
                EnumAbilitys.SpawnLootBug => ConfigAbility.lootBugWhenSeekerInside.Value,
                EnumAbilitys.SpawnMimic => ConfigAbility.mimicWhenSeekerInside.Value,
                EnumAbilitys.SpawnThumper => ConfigAbility.thumperWhenSeekerInside.Value,
                EnumAbilitys.SpawnBracken => ConfigAbility.brackenWhenSeekerInside.Value,
                EnumAbilitys.SpawnTurret => ConfigAbility.turretWhenSeekerInside.Value,
                EnumAbilitys.SpawnLandmine => ConfigAbility.landmineWhenSeekerInside.Value,
                _ => this.requiresSeekerActive,
            };
            return needSeekerActive && TimeOfDay.Instance.currentDayTime <= Config.timeSeekerIsReleased.Value;
        }

        /// <summary>
        /// Check the ability requires the seekers to be active to be buyed.
        /// </summary>
        /// <param name="ability"></param>
        /// <returns>[int] Cost of the ability</returns>
        public int GetAbilityCost()
        {
            Debug.LogWarning($"Ability reading cost :  {this.abilityName}");
            return this.abilityName switch
            {
                EnumAbilitys.Money => ConfigAbility.moneyCost.Value,
                EnumAbilitys.Remote => ConfigAbility.remoteCost.Value,
                EnumAbilitys.Taunt => ConfigAbility.tauntCost.Value,
                EnumAbilitys.Key => ConfigAbility.keyCost.Value,
                EnumAbilitys.TzpInhalant => ConfigAbility.tzpInhalantCost.Value,
                EnumAbilitys.Shovel => ConfigAbility.shovelCost.Value,
                EnumAbilitys.StunGrenade => ConfigAbility.stunGrenadeCost.Value,
                EnumAbilitys.Flashlight => ConfigAbility.flashlightCost.Value,
                EnumAbilitys.LaserPointer => ConfigAbility.laserPointerCost.Value,
                EnumAbilitys.WalkieTalkie => ConfigAbility.walkieTalkieCost.Value,
                EnumAbilitys.CriticalInjury => ConfigAbility.criticalInjuryCost.Value,
                EnumAbilitys.Teleport => ConfigAbility.teleportCost.Value,
                EnumAbilitys.Swap => ConfigAbility.swapCost.Value,
                EnumAbilitys.Decoy => ConfigAbility.decoyCost.Value,
                EnumAbilitys.Stealth => ConfigAbility.stealthCost.Value,
                EnumAbilitys.LongStealth => ConfigAbility.longStealthCost.Value,
                EnumAbilitys.Invisibility => ConfigAbility.invisibilityCost.Value,
                EnumAbilitys.SpawnLootBug => ConfigAbility.lootBugCost.Value,
                EnumAbilitys.SpawnMimic => ConfigAbility.mimicCost.Value,
                EnumAbilitys.SpawnThumper => ConfigAbility.thumperCost.Value,
                EnumAbilitys.SpawnBracken => ConfigAbility.brackenCost.Value,
                EnumAbilitys.SpawnTurret => ConfigAbility.turretCost.Value,
                EnumAbilitys.SpawnLandmine => ConfigAbility.landmineCost.Value,
                _ => this.abilityCost,
            };
        }
        #endregion
    }
}