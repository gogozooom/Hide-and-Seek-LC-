using BepInEx.Configuration;
using HideAndSeek.AbilityScripts;
using System;
using System.Collections.Generic;
using System.Text;

namespace HideAndSeek
{
    public class AbilityConfig
    {
        public bool syncedWithHost = false;

        // Description
        public string abilityName = "genericNull";
        public int abilityCost = 10;

        // Use Restrictions
        public bool seekerAbility = true;
        public bool hiderAbility = true;

        public bool requiresRoundActive = true;
        public bool requiresSeekerActive = true;

        public float abilityDelay = 10f;
        public bool oneTimeUse = false;

        public AbilityConfig(string _abilityName = "genericNull",
            int _abilityCost = 10,
            float _abilityDelay = 10f,
            bool _oneTimeUse = false,
            bool _seekerAbility = true,
            bool _hiderAbility = true,
            bool _requriesRoundActive = true,
            bool _requiresSeekerActive = true) 
        {
            // Description
            abilityName = _abilityName;
            abilityCost = _abilityCost;

            // Use Restrictions
            seekerAbility = _seekerAbility;
            hiderAbility = _hiderAbility;

            requiresRoundActive = _requriesRoundActive;
            requiresSeekerActive = _requiresSeekerActive;

            abilityDelay = _abilityDelay;
            oneTimeUse = _oneTimeUse;
        }
    }
}
