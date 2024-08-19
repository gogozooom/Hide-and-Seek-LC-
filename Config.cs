using BepInEx.Configuration;
using Debugger;

namespace HideAndSeek
{
    public class Config
    {
        // Debug
        public static ConfigEntry<bool> debugEnabled;

        // Gamemode.Abilities
        public static ConfigEntry<bool> abilitiesEnabled;
        public static ConfigEntry<bool> creditsResetOnNewRound;
        public static ConfigEntry<string> sellKeyBind;
        public static ConfigEntry<string> abilityMenuKeyBind;
        public static ConfigEntry<int> deadBodySellValue;


        // Entities.Daytime
        public static ConfigEntry<bool> disableAllDaytimeEntities;
        public static ConfigEntry<bool> circuitBeeEnabled;
        public static ConfigEntry<bool> manticoilEnabled;
        public static ConfigEntry<bool> roamingLocustEnabled;

        // Entities.Outside
        public static ConfigEntry<bool> disableAllOutsideEntities;
        public static ConfigEntry<bool> baboonHawkEnabled;
        public static ConfigEntry<bool> earthLeviathanEnabled;
        public static ConfigEntry<bool> eyelessDogEnabled;
        public static ConfigEntry<bool> forestKeeperEnabled;
        public static ConfigEntry<bool> tulipSnakeEnabled;
        public static ConfigEntry<bool> mechEnabled;
        public static ConfigEntry<bool> kidnapperFoxEnabled;

        // Entities.Indoor
        public static ConfigEntry<bool> disableAllIndoorEntities;
        public static ConfigEntry<bool> brackenEnabled;
        public static ConfigEntry<bool> bunkerSpiderEnabled;
        public static ConfigEntry<bool> coilHeadEnabled;
        public static ConfigEntry<bool> ghostGirlEnabled;
        public static ConfigEntry<bool> hoardingBugEnabled;
        public static ConfigEntry<bool> hygrodereEnabled;
        public static ConfigEntry<bool> jesterEnabled;
        public static ConfigEntry<bool> maskedEnabled;
        public static ConfigEntry<bool> nutcrackerEnabled;
        public static ConfigEntry<bool> snareFleaEnabled;
        public static ConfigEntry<bool> sporeLizardEnabled;
        public static ConfigEntry<bool> thumperEnabled;
        public static ConfigEntry<bool> butlerEnabled;
        public static ConfigEntry<bool> barberEnabled;
        public static ConfigEntry<bool> turretsEnabled;
        public static ConfigEntry<bool> landminesEnabled;
        public static ConfigEntry<bool> spikeTrapEnabled;
        public static ConfigEntry<bool> maneaterEnabled;

        // Players.Seeker
        public static ConfigEntry<string> seekerChooseBehavior;
        public static ConfigEntry<bool> isSeekerImmune;
        public static ConfigEntry<bool> hostilesIgnoreSeeker;
        public static ConfigEntry<bool> shotgunInfiniteAmmo;
        public static ConfigEntry<bool> shotgunAutoReload;
        public static ConfigEntry<bool> teleportSeekerToEntrance;
        public static ConfigEntry<bool> forceSeekerInside;
        public static ConfigEntry<bool> shipLeaveEarly;
        public static ConfigEntry<float> timeWhenLastHider;
        public static ConfigEntry<float> timeSeekerIsReleased;
        public static ConfigEntry<string> seekerItemSlot1;
        public static ConfigEntry<string> seekerItemSlot2;
        public static ConfigEntry<string> seekerItemSlot3;
        public static ConfigEntry<string> seekerItemSlot4;

        // Players.Hider
        public static ConfigEntry<bool> teleportHidersToEntrance;
        public static ConfigEntry<bool> forceHidersInside;
        public static ConfigEntry<bool> lockHidersInside;
        public static ConfigEntry<bool> infiniteFlashlightBattery;
        public static ConfigEntry<string> hiderItemSlot1;
        public static ConfigEntry<string> hiderItemSlot2;
        public static ConfigEntry<string> hiderItemSlot3;
        public static ConfigEntry<string> hiderItemSlot4;
                
        public Config(ConfigFile cfg)
        {
            #region Debug
            debugEnabled = cfg.Bind<bool>(
                "0:Debug/Other",
                "DebugEnabled",
                false,
                "Used for random feature testing and debug logging. (Should be disabled for end user)"
            );
            #endregion

            #region Gamemode.Abilities
            abilitiesEnabled = cfg.Bind(
                "0:Gamemode.Abilities", 
                "Abilities Enabled",
                true,
                "Enables Abilities! Hold 'c' to sell scrap and press 't' to open the abilities menu to spend your credits on."
            );
            creditsResetOnNewRound = cfg.Bind(
                "0:Gamemode.Abilities",
                "Credits Reset On New Round",
                false,
                "Makes everyone's credits go back to 0 when a new round starts"
            );
            abilityMenuKeyBind = cfg.Bind(
                "0:Gamemode.Abilities",
                "Ability Menu keybind",
                "<Keyboard>/t",
                "Press this to open a menu with a wide range of fun abilities!"
            );
            sellKeyBind = cfg.Bind(
                "0:Gamemode.Abilities",
                "Sell keybind",
                "<Keyboard>/c",
                "Hold this for 3 seconds, and you got your self some cash!"
            );
            deadBodySellValue = cfg.Bind(
                "0:Gamemode.Abilities",
                "Dead Body Value",
                100,
                "How much a dead body is worth when selling"
            );

            #endregion

            #region Entities.Daytime
            disableAllDaytimeEntities = cfg.Bind(
                "1:Entities.Daytime",
                "Disable All Daytime Entities",
                false,
                "Determines if daytime spawning should be disabled. (Usefull for modded maps with unique enemies)"
            );
            circuitBeeEnabled = cfg.Bind(
                "1:Entities.Daytime",
                "Spawn Circut Bees",
                false,
                "Determines if Circuit Bees should spawn."
            );
            manticoilEnabled = cfg.Bind(
                "1:Entities.Daytime",
                "Spawn Manticoils",
                true,
                "Determines if Manticoils should spawn."
            );
            roamingLocustEnabled = cfg.Bind(
                "1:Entities.Daytime",
                "Spawn Roaming Locusts",
                true,
                "Determines if Roaming Locusts should spawn."
            );
            #endregion

            #region Entities.Outside
            disableAllOutsideEntities = cfg.Bind(
                "1:Entities.Outside",
                "Disable All Outside Entities",
                false,
                "Determines if outside spawning should be disabled. (Usefull for modded maps with unique enemies)"
            );
            eyelessDogEnabled = cfg.Bind(
                "1:Entities.Outside",
                "Spawn Eyeless Dogs",
                false,
                "Determines if Eyeless Dogs should spawn."
            );
            forestKeeperEnabled = cfg.Bind(
                "1:Entities.Outside",
                "Spawn Forest Keepers",
                false,
                "Determines if Forest Keepers should spawn."
            );
            earthLeviathanEnabled = cfg.Bind(
                "1:Entities.Outside",
                "Spawn Earth Leviathans",
                false,
                "Determines if Earth Leviathans should spawn."
            );
            baboonHawkEnabled = cfg.Bind(
                "1:Entities.Outside",
                "Spawn Baboon Hawks",
                false,
                "Determines if Baboon Hawks should spawn."
            );
            tulipSnakeEnabled = cfg.Bind(
                "1:Entities.Outside",
                "Spawn Tulip Snakes",
                false,
                "Determines if Tulip Snakes should spawn."
            );
            mechEnabled = cfg.Bind(
                "1:Entities.Outside",
                "Spawn Old Bird",
                false,
                "Determines if Old Birds should spawn."
            );
            kidnapperFoxEnabled = cfg.Bind(
                "1:Entities.Outside",
                "Spawn Foxes",
                false,
                "Determines if Foxes should spawn."
            );
            #endregion

            #region Entities.Indoor
            disableAllIndoorEntities = cfg.Bind(
                "1:Entities.Indoor",
                "Disable All Indoor Entities",
                false,
                "Determines if indoor spawning should be disabled. (Usefull for modded maps with unique enemies)"
            );
            brackenEnabled = cfg.Bind(
                "1:Entities.Indoor",
                "Spawn Brackens",
                false,
                "Determines if Brackens should spawn."
            );
            bunkerSpiderEnabled = cfg.Bind(
                "1:Entities.Indoor",
                "Spawn Bunker Spiders",
                false,
                "Determines if Bunker Spiders should spawn."
            );
            coilHeadEnabled = cfg.Bind(
                "1:Entities.Indoor",
                "Spawn Coil Heads",
                false,
                "Determines if Coil Heads should spawn."
            );
            ghostGirlEnabled = cfg.Bind(
                "1:Entities.Indoor",
                "Spawn Ghost Girls",
                false,
                "Determines if Ghost Girls should spawn."
            );
            hoardingBugEnabled = cfg.Bind(
                "1:Entities.Indoor",
                "Spawn Hoarding Bugs",
                false,
                "Determines if Hoarding Bugs should spawn."
            );
            hygrodereEnabled = cfg.Bind(
                "1:Entities.Indoor",
                "Spawn Hygroderes (Slimes)",
                false,
                "Determines if Hygroderes (Slimes) should spawn."
            );
            jesterEnabled = cfg.Bind(
                "1:Entities.Indoor",
                "Spawn Jesters",
                false,
                "Determines if Jesters should spawn."
            );
            maskedEnabled = cfg.Bind(
                "1:Entities.Indoor",
                "Spawn Masked (Player Mimics)",
                false,
                "Determines if Masked (Player Mimics) should spawn."
            );
            nutcrackerEnabled = cfg.Bind(
                "1:Entities.Indoor",
                "Spawn Nutcrackers",
                false,
                "Determines if Nutcrackers should spawn."
            );
            snareFleaEnabled = cfg.Bind(
                "1:Entities.Indoor",
                "Spawn Snare Fleas",
                false,
                "Determines if Snare Fleas should spawn."
            );
            sporeLizardEnabled = cfg.Bind(
                "1:Entities.Indoor",
                "Spawn Spore Lizards",
                false,
                "Determines if Spore Lizards should spawn."
            );
            thumperEnabled = cfg.Bind(
                "1:Entities.Indoor",
                "Spawn Thumpers",
                false,
                "Determines if Thumpers should spawn."
            );
            butlerEnabled = cfg.Bind(
                "1:Entities.Indoor",
                "Spawn Butlers",
                false,
                "Determines if Butlers should spawn."
            );
            barberEnabled = cfg.Bind(
                "1:Entities.Indoor",
                "Spawn Barbers",
                false,
                "Determines if Barbers should spawn."
            );
            maneaterEnabled = cfg.Bind(
                "1:Entities.Indoor",
                "Spawn Maneaters",
                false,
                "Determines if Maneaters should spawn."
            );
            turretsEnabled = cfg.Bind(
                "1:Entities.Indoor",
                "Spawn Turrets",
                false,
                "Determines if Turrets should spawn."
            );
            landminesEnabled = cfg.Bind(
                "1:Entities.Indoor",
                "Spawn Land Mines",
                false,
                "Determines if Land Mines should spawn."
            );
            spikeTrapEnabled = cfg.Bind(
                "1:Entities.Indoor",
                "Spawn Spike Traps",
                false,
                "Determines if Spike Traps should spawn."
            );
            #endregion

            #region Players.Seeker
            seekerChooseBehavior = cfg.Bind(
                "2:Players.Seeker",
                "Seeker Choose Behavior",
                "Turns",
                "'None' (Just a random range generator), 'No Double' (Next Seeker can't be last seeker), 'Turns' (Will not pick someone that was already seeker, resets when everyone got a chance), 'Lever' (The seeker is the lever puller)"
            );
            isSeekerImmune = cfg.Bind(
                "2:Players.Seeker",
                "Is Seeker Immune",
                false,
                "Determines if the seeker could be harmed or not (god mode basically)"
            );
            hostilesIgnoreSeeker = cfg.Bind(
                "2:Players.Seeker",
                "Is Seeker Ignored",
                true,
                "(To be implemented!) Determines if the seeker should be ignored by hostiles or not."
            );
            shotgunInfiniteAmmo = cfg.Bind(
                "2:Players.Seeker",
                "Shotgun Infinite Ammo",
                true,
                "Disables the need for ammo for the shotgun."
            );
            shotgunAutoReload = cfg.Bind(
                "2:Players.Seeker",
                "Shotgun Auto Reload",
                false,
                "(Only works if 'shotgunInfiniteAmmo' is enabled) Disables the need to reload a new shell, and lets you shoot like normal again."
            );
            shipLeaveEarly = cfg.Bind(
                "2:Players.Seeker",
                "Make Ship Leave Early",
                true,
                "(Only works if 'shotgunInfiniteAmmo' is enabled) Disables the need to reload a new shell, and lets you shoot like normal again."
            );
            timeWhenLastHider = cfg.Bind(
                "2:Players.Seeker",
                "Last Hider Time",
                900f,
                "The time the clock is set to when there is one hider left. 900 = 9:00 PM, 960 = 10:00 PM, +60 = +1 hour (Requires makeShipLeaveEarly = true, to be enabled!)"
            );
            timeSeekerIsReleased = cfg.Bind(
                "2:Players.Seeker",
                "Seeking Time",
                195f,
                "The time the seeker is released to wreak havoc in the land. 180 = 9:00 PM, 240 = 10:00 PM, +60 = +1 hour"
            );
            teleportSeekerToEntrance = cfg.Bind(
                "2:Players.Seeker",
                "Teleport Seeker To Entrance",
                true,
                "Determines if the seeker should be teleported to the entrance on landing."
            );
            forceSeekerInside = cfg.Bind(
                "2:Players.Seeker",
                "Teleport Seeker Inside",
                true,
                "Determines if the seeker should be teleported into the building on landing. (teleportSeekerToEntrance Should Be Enabled!)"
            );
            seekerItemSlot1 = cfg.Bind(
                "2:Players.Seeker",
                "Seeker Item Slot 1",
                "Shotgun",
                "The id of the item that will spawn in the first slot of the seeker. (See README.md for list of Item IDs)"
            );
            seekerItemSlot2 = cfg.Bind(
                "2:Players.Seeker",
                "Seeker Item Slot 2",
                "",
                "The id of the item that will spawn in the second slot of the seeker. (See README.md for list of Item IDs)"
            );
            seekerItemSlot3 = cfg.Bind(
                "2:Players.Seeker",
                "Seeker Item Slot 3",
                "",
                "The id of the item that will spawn in the third slot of the seeker. (See README.md for list of Item IDs)"
            );
            seekerItemSlot4 = cfg.Bind(
                "2:Players.Seeker",
                "Seeker Item Slot 4",
                "",
                "The id of the item that will spawn in the fourth slot of the seeker. (See README.md for list of Item IDs)"
            );
            #endregion

            #region Players.Hider
            teleportHidersToEntrance = cfg.Bind(
                "2:Players.Hider",
                "Teleport Hiders To Entrance",
                true,
                "Determines if all the hiders should be teleported to the entrance on landing."
            );
            forceHidersInside = cfg.Bind(
                "2:Players.Hider",
                "Teleport Hiders Inside",
                true,
                "Determines if all the hiders should be teleported into the building on landing. (teleportHidersToEntrance Should Be Enabled!)"
            );
            lockHidersInside = cfg.Bind(
                "2:Players.Hider",
                "Lock Hiders Inside",
                true,
                "Determines if all the hiders should be locked inside the building. (forceHidersInside Should Be Enabled!)"
            );
            infiniteFlashlightBattery = cfg.Bind(
                "2:Players.Hider",
                "Infinite Flashlight Battery",
                true,
                "Makes the flashlight never run out of battery."
            );
            hiderItemSlot1 = cfg.Bind(
                "2:Players.Hider",
                "Hider Item Slot 1",
                "Pro-flashlight",
                "The id of the item that will spawn in the first slot of the hider. (See README.md for list of Item IDs)"
            );
            hiderItemSlot2 = cfg.Bind(
                "2:Players.Hider",
                "Hider Item Slot 2",
                "",
                "The id of the item that will spawn in the second slot of the hider. (See README.md for list of Item IDs)"
            );
            hiderItemSlot3 = cfg.Bind(
                "2:Players.Hider",
                "Hider Item Slot 3",
                "",
                "The id of the item that will spawn in the third slot of the hider. (See README.md for list of Item IDs)"
            );
            hiderItemSlot4 = cfg.Bind(
                "2:Players.Hider",
                "Hider Item Slot 4",
                "",
                "The id of the item that will spawn in the fourth slot of the hider. (See README.md for list of Item IDs)"
            );
            #endregion
        }
    }
}
