## Description
- v60! *New Abilities* adding spice to the classic hide and seek game!
- A Hide And Seek mod dedicated to making your life easier when it comes to rules and finding the right mods.
- A config allowing a wide range of behaviors allowing the seeker or the hiders to have a slightly better advantage
- Infinite money so you don't have to get a few things back just to go to Titan for a few rounds

## v1.4.0.2 Major update
- Added:
    - Multiple Seekers!
        - There is now a config file for specifying the number of seekers each round, both static (1-4... etc) and scaling (20%-55.55%... etc).
    - Zombie Players!
        - There is now a config file for enabling zombie players! After a hider dies, they will respawn with their own items and help the seeker find the last few hiders!
    - Ship Teleporter effects to the Teleport and Swap ability.
    - Special sounds and effects to the Critical Injury ability.
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
    - Teleport
        - Startup cooldown = 3
    - Swap
        - Startup cooldown = 3
        - Can now target any player
    - Critical Injury
        - Delay 60 -> 120
        - Target can now avoid concequences
- More to be added...

## Known Mod Conflict Issues
- Currently, using having the 'ModelReplacementApi' mod will break the Invisibility ability, as the models are handled differently. This makes it hard to disable the custom models without a hard and unnecessary dependency on the mod
- It is impossible to use the ability ui in the 'LethalCompanyVR' mod. This is planned to be fixed in a future update though. (I love playing in VR)

## Known Compatible Mods
- More Company
- Late Company
- Reserved Item Slot mods (Just press 'e' a few times to fix the items being locked up)
- More Suits

## Recommended Mods
- More Company (Bigger lobbies)
- Lock_Doors_Mod (Give keys a cool extra mechanic)
- LC Better Clock (Make the time visible at all times)
- Too Many Emotes (Overall makes the game more fun, you wont get rewarded for this type of taunting though)

## Item Slot IDs (Not Case Sensitive)
# Weapons
- Shotgun
- Shovel
- Homemade flashbang
- Extension ladder
- Stop sign
- Yield sign
- Stun grenade
- Zap gun
- Kitchen knife

# Items
- Boombox
- Extension ladder
- Flashlight
- Pro-flashlight
- Ammo
- Jetpack
- Key
- Lockpicker
- Radar-booster
- Laser pointer
- Remote
- Spray paint
- TZP-Inhalant
- Walkie-talkie

# Scrap
- Apparatus
- Magic 7 ball
- Airhorn
- Bell
- Big bolt
- Bottles
- Brush
- Candy
- Cash register
- Chemical jug
- Clown horn
- Large axle
- Comedy
- Teeth
- Dust pan
- Egg beater
- V-type engine
- Golden cup
- Fancy lamp
- Painting
- Plastic fish
- Laser pointer
- Flask
- Gift
- Gold bar
- Hairdryer
- Magnifying glass
- Metal sheet
- Cookie mold pan
- Mug
- Perfume bottle
- Old phone
- Jar of pickles
- Pill bottle
- Hive
- Remote
- Ring
- Toy robot
- Rubber Ducky
- Red soda
- Steering wheel
- Stop sign
- Tea kettle
- Toothpaste
- Toy cube
- Tragedy
- Whoopie cushion
- Yield sign
- Easter egg

# Other
- Binoculars
- clipboard
- Mapper
- Body
- Sticky note
- box