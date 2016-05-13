#Salvage
Plugin for Rust Experimental servers using Oxide Mod

###Description:
Allows a player to salvage items **they know how to make** at any repair bench. A salvaged item yields half of the resource cost to
make the item. If the item has a condition, the yeild is affected by the item's remaining condition.

###Usage:
A player wishing to salvage items uses the "/salvage" slash command before visiting a repair bench.

While salvaging is enabled, salvageable items placed in a repair bench are immediately removed and the salvaged parts are placed in
the player's inventory.

Salvaged parts respect maximum stack sizes. If there is not enough room in the player's inventory, salvaged parts will be spilled on
the ground.

Items that are not salvageable will remain in the repair bench for the player to either remove or repair.

Salvaging will remain enabled until the "/salvage" slash command is used again.

###Installation:
Simply drop Salvage.cs into your plugin directory:

    rust_server\server\server_identity\oxide\plugins\

There are no configuration files or additional setup steps.

###Uninstallation:
Remove Salvage.cs from your plugin directory. The plugin does not create any other files.

###Additional Notes:
* This plugin is tiny and does not persist any data between server instances - currently salvaging players have to re-enable salvaging if the server is restarted or if the plugin is reloaded.
* Any ammo in a salvaged weapon is currently discarded. Please unload ammo from weapons you wish to salvage.
