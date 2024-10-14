## v1.4.0.1 Major update
- Added:
    - Multiple Seekers!
        - There is now a config file for specifying the number of seekers each round, both static (1-4... etc) and scaling (20%-55.55%... etc).
    - Ship Teleporter effects to the Teleport and Swap ability.
    - Username colors for each role (Configurable)
    - Configure the prices, delay, and more with the 'Abilities.Cfg' config file! (Automatically generated once game is launched for the first time. Last config is incompatible with different version numbers)
- Changed:
    - Starting item slots can now be randomized! (Config Example, 'Shovel, Yeild Sign, Stop Sign' will have a 1 out of 3 chance for each item)
- Fixed:
    - GetPlayerWithClientId() being inconsistant with some clients.
    - Game soft locking when the seeker dies before the ship lands.
    - Unable to spawn Turrets or Landmines when natural spaning of them is disabled in the config.
    - "You have zero days left to meet the profit quota." What profit quota?
- Balancing:
    - Teleport and Swap
        - Startup cooldown = 3
- More to be added...

## v1.3.3 Small Update
- Added:
    - Tooltips and version to the Ability UI
    - New Category "Item"
    - TZP-Inhanlant Spawnable Ability
    - Shovel Spawnable Ability
    - Stun Grenade Spawnable Ability
    - Flashlight Spawnable Ability
    - Walkie-talkie Spawnable Ability
- Changed:
    - "Spawn Key" Ability Name -> "Key"
    - Default 'Hider Item Slot 1' Config 'Pro-flashlight' -> 'flashlight'
    - Default 'Seeker Item Slot 2' Config '' -> 'Pro-flashlight'
- Fixed:
    - Config Description on 'shipLeaveEarly'
- Heat-Seeking Ability Changes:
    - Ability reworked to only target one player at a time
	- Fixed the seeker's location to be revealed instead of the target tracking themselves
    - Update Time: 10s -> 12s
    - The ability will refresh once the target has been eliminated, allowing you to buy the ability again

## v1.3.2 Hotfix
- Added link to manifest

## v1.3.1 Patch
- Added:
    - 'Maneater Enabled' as a config option
- Changed:
    - 'Teleport seeker to entrance', and 'teleport seeker inside' are now true by default (Changed in config)
    - 'last hider time' changed to 9:00 PM(Changed in config)
    - Descriptions of a few abilities
    - Ability UI is now overall easier to read
- Fixed:
    - Warning the client at 10 seconds rather than 5 (Invisibility + Stealth)
    - "Collision Cube" appearing on client invisibility expires
    - Flashlight and Walkie sometimes being visible while invisible ability is active (Due to reserved slot mod)
    - Spawning a turret or landmine when the prefab wasn't loaded will now refund your credits
    - spawned turrets or landmines being persistent to the next round
    - Being able to get ejected sometimes
- Balanced The Abilities:
    - 'Stealth' Ability Cost: $100 -> $30
    - 'Long Stealth' Ability Cost: $500 -> $259
    - 'Spawn Mimic' Ability Cost: $250 -> $200
    - 'Spawn Loot Bug' Ability Cost: $300 -> $99; Ability Delay: 60s -> 80s
    - 'Spawn Thumper' Ability Cost: $750 -> $500
    - 'Spawn Bracken' Ability Cost: $500 -> $750
    - 'Spawn Turret' Ability Cost: $599 -> $299; Ability Delay: 10s -> 30s
    - 'Spawn LandMine' Ability Cost: $199 -> $109; Ability Delay: 10s -> 5s

## v1.3.0 Ability Update
- Added config option to make ship leave earlier (true and 10:00PM by default)
- Added config option to enable infinite flashlight battery (true by default)
- Added a few more taunt sound effects
- Added 'Stealth' and 'Invisibility' as new abilities
- Added categories to keep all the abilities sorted (plus a favorites tab)

- Turrets and Landmines now no longer activate to their creator
- Hoarding Bugs will now give items to their creator and fend of anyone else trying to hinder this process
- Brackens will now ignore their creator
- The Masked will now ignore their creator
- Thumpers will now ignore their creator
- Spellchecked the Changelog and Readme
- Items spawned now enter inventory automatically
- Changed a few ability prices to make more balanced
- Winners now get a good wad of cash after each round
- Dead bodies now have more scrap value (100 by default in config)

- Fixed a few errors when using abilities with certain clients
- Fixed more issues related to the host leaving and creating a new lobby
- Fixed the swap ability breaking when there is only one hider left
- Fixed spawning Turrets/Landmines sometimes having an error
- Fixed tip being able to exclaim "'1' hiders remain"; changed to "'1' hider remains"
- Fixed the first and last taunt sound being less probable to play
- Fixed being able to open the ability menu when you're not supposed to (Using the terminal, Quick menu open)
- Temporarily removed 'Spawn Remote' ability

## v1.2.0 Update Patch + Ability Update
- Implemented Lever-Seeker picking functionality in the config
- Fixed functionality breaking when host leaves and rejoins
- Added abilities to both the seeker and hiders (Toggled in the config)
    - Pick up scrap around the map and sell it with 'c'
    - And Press 't' to open your abilities tab
    - There are currently 13 abilities in the game (Taunt, Spawn Key, Spawn Remote, Critical Injury, Teleport, Swap, Spawn Loot Bug, Spawn Mimic, Spawn Bracken, Spawn Thumper, Spawn Turret, Spawn LandMine, Heat Seeking)
    - More are planned to be added, and exiting ones planned to be polished; plus categories in the menu (To keep things sorted, ya know)

## v1.1.4 Hotfix
- Added v55 enemies
- Added config options to disable all enemies (Useful for modded entities)
- Updated dependencies

## v1.1.3 Hotfix
- Fixed the bug promised in the last patch (Bruh, why do I always get to exited)

## v1.1.2 Hotfix
- Finally Fixed LevelLoaded being true despite the level being finished loading on the client (Aka inconsistent teleporting)
- Fixed a few other small things

## v1.1.1 Hotfix
- Hopefully fixed inconsistent player teleporting after round one
- Made the seeker only teleport to the entrance if they are holding at least one item (Preventing a empty handed seeker)
- Made the seeker unable to die from gunshots (More relevant for vr players shooting their hands)
- Fixed a few other small things (Like the game breaking when the seeker jumps off the ship early, etc.)

## v1.1.0 Update Patch + More Config
- Fixed death messages and ship leaving early being inconsistent
- Fixed Turrets and Landmine spawn config options
- Fixed item spawning being inconsistent sometimes
- Added seeker randomizer types (CONFIG: None, NoDouble, Turns)
- Added Tulip Snakes, Butlers, Old Birds, and Spike Traps as config spawn options
- Added teleporting seeker to entrance config options
- Made SeekerImmune config option actually do something (Still can die from a few things)

## v1.0.2 Update Patch
- V50 support!
- Now depends on LethalNetworkingApi
- Reworked the plugin to work with the new version and without LC_API (As it is now deprecated for some reason)

## v1.0.1 Hotfix
- Fixed the item list display in README

## v1.0.0 Release
- Release!
- Config allows changing which items are given to the seeker and the hiders
- Also allows disabling individual hostiles (Including turrets and landmines)
