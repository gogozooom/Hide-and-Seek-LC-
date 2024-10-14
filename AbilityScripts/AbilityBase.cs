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
    }
}