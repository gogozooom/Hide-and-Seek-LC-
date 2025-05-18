using HideAndSeek.Patches;
using System;
using UnityEngine;

namespace HideAndSeek.AbilityScripts
{
    public class AbilityBase
    {
        // Description
        public string abilityName = "genericNull";
        public string abilityDescription = "genericNullDescription";
        public int abilityCost = 10;
        public string abilityCategory = "Misc";

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

        public AbilityBase(string _abilityName = "genericNull",
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

            NetworkHandler.Instance.EventSendRpc(".activateAbility", new MessageProperties(__string: ability.abilityName, __ulong: activatorId, __extraMessage: extraMessage));
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
                "Taunt" => isPlayerSeeker ? ConfigAbility.tauntForSeekers.Value : ConfigAbility.tauntForHiders.Value,
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
                "Taunt" => ConfigAbility.tauntOneTimeUse.Value,
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
            float timeFromLastUse = this.abilityName switch
            {
                "Taunt" => Time.time - this.lastUsed,
                _ => Time.time - this.lastUsed,
            };
            return timeFromLastUse <= this.abilityDelay;
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
                "Taunt" => ConfigAbility.tauntOnRoundActivation.Value,
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
                "Taunt" => ConfigAbility.tauntWhenSeekerInside.Value,
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
            return this.abilityName switch
            {
                "Taunt" => ConfigAbility.tauntCost.Value,
                _ => this.abilityCost,
            };
        }
        #endregion
    }
}