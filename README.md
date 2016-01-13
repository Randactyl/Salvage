# Salvage
Plugin for Rust Experimental servers using Oxide Mod

###Description:
Allows a player to salvage items at any repair bench. A salvaged item yields half of the resource cost to make the item. If the item has
a condition, the yeild is affected by the item's remaining condition.

###Usage:
A player wishing to salvage items uses the "/salvage" slash command before visiting a repair bench. While salvaging is enabled, items
placed in a repair bench are immediately removed and the salvaged parts are placed in the player's inventory. Salvaging will remain
enabled until the "/salvage" slash command is used again.

###Installation:
Simply drop Salvage.cs into your plugin directory:

    rust_server\server\server_identity\oxide\plugins\

There are no configuration files or additional setup steps.

###Uninstallation:
Remove Salvage.cs from your plugin directory. The plugin does not create any other files.

###Additional Notes:
* This plugin is tiny and does not persist any data between instances - currently salvaging players would have to re-enable salvaging if
the server was restarted or if the plugin was reloaded.
* Any ammo in a salvaged weapon is currently discarded. Please unload ammo from weapons you wish to salvage.
