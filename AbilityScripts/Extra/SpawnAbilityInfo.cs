using GameNetcodeStuff;
using HideAndSeek.Patches;
using System.Collections.Generic;
using UnityEngine;

namespace HideAndSeek.AbilityScripts.Extra
{
    public class SpawnAbilityInfo : MonoBehaviour
    {
        public PlayerControllerB creatorPlayer;
        public List<PlayerControllerB> otherFriendlies = new();

        void Start()
        {
            PatchesManager.playerRevived += PlayerRevived;
        }

        void PlayerRevived(ulong id)
        {
            PlayerControllerB newZombie = RoundManagerPatch.GetPlayerWithClientId(id);
            if (Plugin.seekers.Contains(creatorPlayer))
            {
                // Creator is seeker
                if(!otherFriendlies.Contains(newZombie))
                    otherFriendlies.Add(newZombie);
            }
            else
            {
                // Creator is hider
                if (otherFriendlies.Contains(newZombie))
                    otherFriendlies.Remove(newZombie);
            }
        }
    }
}
