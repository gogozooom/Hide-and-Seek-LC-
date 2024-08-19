using GameNetcodeStuff;
using System.Collections.Generic;
using UnityEngine;

namespace HideAndSeek.AbilityScripts.Extra
{
    public class SpawnAbilityInfo : MonoBehaviour
    {
        public PlayerControllerB creatorPlayer;
        public List<PlayerControllerB> otherFriendlies = new();
    }
}
