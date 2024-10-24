using BepInEx.Configuration;
using Debugger;
using LethalCompanyInputUtils.Api;
using LethalCompanyInputUtils.BindingPathEnums;
using UnityEngine;
using UnityEngine.InputSystem;

namespace HideAndSeek
{
    public class Config
    {
        // Debug
        public static ConfigEntry<bool> debugEnabled;

        // Gamemode.Abilities
        public static ConfigEntry<bool> abilitiesEnabled;
        public static ConfigEntry<bool> creditsResetOnNewRound;
        public static ConfigEntry<int> deadBodySellValue;
        public static ConfigEntry<bool> disableVRTurningWhileMenuOpen;

        // Gamemode.Objective
        public static ConfigEntry<string> objective;
        public static ConfigEntry<float> timeObjectiveAvailable;
        public static ConfigEntry<Color> objectiveNameColor;
        public static ConfigEntry<bool> lockShipLever;

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
        public static ConfigEntry<Color> seekerNameColor;
        public static ConfigEntry<string> seekerChooseBehavior;
        public static ConfigEntry<string> numberOfSeekers;
        public static ConfigEntry<string> extraSeekerChooseBehavior;
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
        public static ConfigEntry<Color> hiderNameColor;
        public static ConfigEntry<bool> teleportHidersToEntrance;
        public static ConfigEntry<bool> forceHidersInside;
        public static ConfigEntry<bool> lockHidersInside;
        public static ConfigEntry<bool> infiniteFlashlightBattery;
        public static ConfigEntry<string> hiderItemSlot1;
        public static ConfigEntry<string> hiderItemSlot2;
        public static ConfigEntry<string> hiderItemSlot3;
        public static ConfigEntry<string> hiderItemSlot4;

        // Players.Zombies
        public static ConfigEntry<Color> zombieNameColor;
        public static ConfigEntry<bool> deadHidersRespawn;
        public static ConfigEntry<bool> deadZombiesRespawn;
        public static ConfigEntry<bool> zombiesCanUseAbilities;
        public static ConfigEntry<float> zombieSpawnDelay;
        public static ConfigEntry<string> zombieSpawnLocation;
        public static ConfigEntry<string> zombieItemSlot1;
        public static ConfigEntry<string> zombieItemSlot2;
        public static ConfigEntry<string> zombieItemSlot3;
        public static ConfigEntry<string> zombieItemSlot4;

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
            deadBodySellValue = cfg.Bind(
                "0:Gamemode.Abilities",
                "Dead Body Value",
                100,
                "How much a dead body is worth when selling"
            );
            disableVRTurningWhileMenuOpen = cfg.Bind(
                "0:Gamemode.Abilities",
                "Disable VR Turning While Menu Open",
                false,
                "(In VR Mode) Disables turning while the ability menu is open to make browsing through the abilities a little less weird. (WARNING: This is a little buggy, as the vr rig rotation gets reset everytime the menu opens! Which is probably not any better than with this off)"
            );

            #endregion

            #region Gamemode.Objective
            objective = cfg.Bind(
                "0:Gamemode.Objective",
                "Hider Objective",
                "Ship",
                "'None' There will be no objective, 'Ship' The interior exits will be unlocked and the hiders have to get back to the ship, (More may come?)"
            );
            timeObjectiveAvailable = cfg.Bind(
                "0:Gamemode.Objective",
                "Time Objective Available",
                900f,
                "The time the clock would be when the objective comes into play. 900 = 9:00 PM, 960 = 10:00 PM, +60 = +1 hour (Can't be lower than when the seeker is released!)"
            );
            objectiveNameColor = cfg.Bind(
                "2:Players.Hider",
                "Objective Reached Name Color",
                new Color(1, 0, 1),
                "The color the player's name tag will be when they have succsessfully reached the objective."
            );
            lockShipLever = cfg.Bind(
                "2:Players.Seeker",
                "Lock Ship Lever",
                true,
                "Will not allow the ship lever to be pulled during a round (Does not apply to the host or the seekers)"
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
            seekerNameColor = cfg.Bind(
                "2:Players.Seeker",
                "Seeker Name Color",
                new Color(1, 0, 0),
                "The color the player's name tag will be."
            );
            seekerChooseBehavior = cfg.Bind(
                "2:Players.Seeker",
                "Seeker Choose Behavior",
                "Turns",
                "'None' (Just a random range generator), 'No Double' (Next Seeker can't be last seeker), 'Turns' (Will not pick someone that was already seeker, resets when everyone got a chance), 'Lever' (The seeker is the lever puller)"
            );
            numberOfSeekers = cfg.Bind(
                "2:Players.Seeker",
                "Number of Seekers",
                "20%",
                "'1'-[connected players] (Example '3') OR '1%'-'100%' of connected players will be the seeker. (Example 20%). No matter what this is set to, (Example '0' or '100%') there will ALWAYS be at least 1 hider and 1 seeker (Unless there is only one connected player, then they would just be seeker)"
            );
            extraSeekerChooseBehavior = cfg.Bind(
                "2:Players.Seeker",
                "Extra Seeker Choose Behavior",
                "Closest",
                "'None' (Just a random range generator), 'Turns' (Will not pick someone that was already seeker, resets when everyone got a chance), 'Closest' (Players nearest to the first seeker will be seeker)"
            );
            isSeekerImmune = cfg.Bind(
                "2:Players.Seeker",
                "Is Seeker Immune",
                false,
                "Determines if the seeker could be harmed or not (not recommended while abilites are enabled)"
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
                "Skips the time when there is one hider left (Time defined by 'timeWhenLastHider')"
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
                "The id of the item that will spawn in the first slot of the seeker. 'item1, item2' will randomly choose between the items listed (See README.md for list of Item IDs)"
            );
            seekerItemSlot2 = cfg.Bind(
                "2:Players.Seeker",
                "Seeker Item Slot 2",
                "Pro-flashlight",
                "The id of the item that will spawn in the second slot of the seeker. 'item1, item2' will randomly choose between the items listed (See README.md for list of Item IDs)"
            );
            seekerItemSlot3 = cfg.Bind(
                "2:Players.Seeker",
                "Seeker Item Slot 3",
                "",
                "The id of the item that will spawn in the third slot of the seeker. 'item1, item2' will randomly choose between the items listed (See README.md for list of Item IDs)"
            );
            seekerItemSlot4 = cfg.Bind(
                "2:Players.Seeker",
                "Seeker Item Slot 4",
                "",
                "The id of the item that will spawn in the fourth slot of the seeker. 'item1, item2' will randomly choose between the items listed (See README.md for list of Item IDs)"
            );
            #endregion

            #region Players.Hider
            hiderNameColor = cfg.Bind(
                "2:Players.Hider",
                "Hider Name Color",
                new Color(1, 1, 1),
                "The color the player's name tag will be."
            );
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
                "flashlight",
                "The id of the item that will spawn in the first slot of the hider. 'item1, item2' will randomly choose between the items listed (See README.md for list of Item IDs)"
            );
            hiderItemSlot2 = cfg.Bind(
                "2:Players.Hider",
                "Hider Item Slot 2",
                "",
                "The id of the item that will spawn in the second slot of the hider. 'item1, item2' will randomly choose between the items listed (See README.md for list of Item IDs)"
            );
            hiderItemSlot3 = cfg.Bind(
                "2:Players.Hider",
                "Hider Item Slot 3",
                "",
                "The id of the item that will spawn in the third slot of the hider. 'item1, item2' will randomly choose between the items listed (See README.md for list of Item IDs)"
            );
            hiderItemSlot4 = cfg.Bind(
                "2:Players.Hider",
                "Hider Item Slot 4",
                "",
                "The id of the item that will spawn in the fourth slot of the hider. 'item1, item2' will randomly choose between the items listed (See README.md for list of Item IDs)"
            );
            #endregion

            #region Players.Zombies
            zombieNameColor = cfg.Bind(
                "2:Players.Zombie",
                "Zombie Name Color",
                new Color(0, 1, 0),
                "The color the player's name tag will be."
            );
            deadHidersRespawn = cfg.Bind(
                "2:Players.Zombie",
                "Dead Hiders Respawn",
                true,
                "(If true) : When a hider is killed, they will turn into a zombie, assisting the seeker."
            );
            deadZombiesRespawn = cfg.Bind(
                "2:Players.Zombie",
                "Dead Zombies Respawn",
                false,
                "(If true) : When a zombie dies, they will respawn again."
            );
            zombiesCanUseAbilities = cfg.Bind(
                "2:Players.Zombie",
                "Zombies Can Use Abilities",
                false,
                "(If true) : The zombies will be able to use seeker abilities."
            );
            zombieSpawnDelay = cfg.Bind(
                "2:Players.Zombie",
                "Zombie Spawn Delay",
                8f,
                "When a player dies, the thread will yield for the spesified amount of seconds before attempting to respawn them as a zombie."
            );
            zombieSpawnLocation = cfg.Bind(
                "2:Players.Zombie",
                "Zombie Spawn Location",
                "Inside",
                "'Inside' : Teleports Zombies inside when spawning, 'Entrance' : Teleports Zombies to the main entrance when spawning, 'Ship' : Spawns Zombies in the ship"
            );
            zombieItemSlot1 = cfg.Bind(
                "2:Players.Zombie",
                "Zombie Item Slot 1",
                "Stop sign, Yield sign, Shovel",
                "The id of the item that will spawn in the first slot of the zombie. 'item1, item2' will randomly choose between the items listed (See README.md for list of Item IDs)"
            );
            zombieItemSlot2 = cfg.Bind(
                "2:Players.Zombie",
                "Zombie Item Slot 2",
                "",
                "The id of the item that will spawn in the second slot of the zombie. 'item1, item2' will randomly choose between the items listed (See README.md for list of Item IDs)"
            );
            zombieItemSlot3 = cfg.Bind(
                "2:Players.Zombie",
                "Zombie Item Slot 3",
                "",
                "The id of the item that will spawn in the third slot of the zombie. 'item1, item2' will randomly choose between the items listed (See README.md for list of Item IDs)"
            );
            zombieItemSlot4 = cfg.Bind(
                "2:Players.Zombie",
                "Zombie Item Slot 4",
                "",
                "The id of the item that will spawn in the fourth slot of the zombie. 'item1, item2' will randomly choose between the items listed (See README.md for list of Item IDs)"
            );

            #endregion
        }
    }
    public class InputConfigs : LcInputActions
    {
        static InputConfigs instance;

        [InputAction(KeyboardControl.Escape, Name = "Escape", GamepadControl = GamepadControl.Start)]
        public InputAction Escape { get; set; }

        [InputAction(KeyboardControl.T, Name = "Ability Menu", GamepadControl = GamepadControl.Select)]
        public InputAction AbilityMenuKey { get; set; }

        [InputAction(KeyboardControl.C, Name = "Sell Scrap", GamepadControl = GamepadControl.RightShoulder)]
        public InputAction SellKey { get; set; }

        [InputAction(MouseControl.Scroll, Name = "Scroll")]
        public InputAction ScrollAbilitiesInput { get; set; }

        [InputAction(MouseControl.MiddleButton, Name = "Favorite Ability")]
        public InputAction FavoriteKey { get; set; }

        [InputAction(KeyboardControl.LeftArrow, Name = "Previous Ability")]
        public InputAction BackKey { get; set; }

        [InputAction(KeyboardControl.RightArrow, Name = "Next Ability")]
        public InputAction ForwardKey { get; set; }

        [InputAction(MouseControl.None, Name = "Scroll Categories", GamepadControl = GamepadControl.Dpad)]
        public InputAction ScrollCategoriesInput { get; set; }

        [InputAction(KeyboardControl.UpArrow, Name = "Change Category Up")]
        public InputAction UpKey { get; set; }

        [InputAction(KeyboardControl.DownArrow, Name = "Change Category Down")]
        public InputAction DownKey { get; set; }

        [InputAction(KeyboardControl.Enter, Name = "Activate Ability", GamepadControl = GamepadControl.RightTrigger)]
        public InputAction ActivateKey { get; set; }

        public static InputConfigs GetInputClass()
        {
            if (instance == null)
            {
                instance = new InputConfigs();
            }

            return instance;
        }
    }
}
