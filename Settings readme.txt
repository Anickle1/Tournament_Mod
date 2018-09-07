When you press the 'save settings' button in the mod the current mod setup will be saved to the file settings.cfg in this mod folder.
The saved settings will load instead of the default values when you load the game, 
If you wish to go back to defaults on load you can hit 'restore defaults' and then 'save settings' in the mod or just delete the config file.

The config file contains two values that are not configurable in the mod UI -
The settings file generated when you save settings contains the buffer time and speed limit for moving away before you pick up dq time.
You can edit the settings file with a text editor and change these values, they will then be loaded the next time you run the game.

These are currently 3 seconds and 0m/s by default, this means that reasonable recoil wont add dq time if moving towards in-bounds but you will pick up dq time if out of bounds and immobile. Hitting restore defaults will set the values back to these defaults if you want to override whatever you have set in the config file.

Changing the buffer value gives you more/less time to start moving forwards again after going over the speed limit away when out of bounds. 0 will remove the buffer.
Changing the speed value to a positive value will allow you to move that amount in m/s away without triggering dq time, a negative value will require you to be moving towards the target at at least that speed or dq time is picked up. 0 will make any movement away trigger dq. The old Wootness version of the mod is essentially 0 for both values.

The settings file can be edited in a text editor, however be careful to avoid entering invalid data into the file as there is no error checking on load.
The file is json and in the format [value,value,value,..]

[Min altitude, Max altitude, Max distance, Max DQ time, Materials, Distance, Horizontal spawn gap, Vertical spawn offset, Spawn direction, Spawn height location, Default keys on/off, Map tile east-west, Map tile north-south, Forward-back spawn gap, Out of bounds buffer time, Out of bounds away speed limit]
