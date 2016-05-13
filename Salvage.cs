using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Oxide.Plugins {

    [Info("Salvage", "Randactyl", "1.0.0")]
    [Description("Use /salvage to enable/disable salvage mode at repair benches.")]
    public class Salvage : RustPlugin {
        Dictionary<ulong, BasePlayer> salvagers = new Dictionary<ulong, BasePlayer>(); //[steamId] = player
        Dictionary<string, ItemBlueprint> blueprints = new Dictionary<string, ItemBlueprint>(); //[shortname] = itemBp

        void OnServerInitialized() {
            //build bp lookup
            blueprints = ItemManager.bpList.ToDictionary(itemBp => itemBp.targetItem.shortname, itemBp => itemBp);
        }

        [ChatCommand("salvage")]
        void cmdSalvage(BasePlayer player, string command, string[] args) {
            ulong steamId = player.userID;
            
            //if player is a salvager then remove them else add them
            if (salvagers.ContainsKey(steamId)) {
                salvagers.Remove(steamId);
                player.ChatMessage("Salvaging is now OFF.");
            } else {
                salvagers[steamId] = player;
                player.ChatMessage("Salvaging is now ON. Visit a repair bench to salvage items.");
            }
        }

        void OnLootEntity(BaseEntity source, BaseEntity target) {
            //check entities
            if(source == null || target == null) return;
            if(target.LookupShortPrefabName() != "repairbench_deployed.prefab") return;
            
            //store the last player to open the repair bench
            BasePlayer player = (BasePlayer)source;
            ulong steamId = player.userID;
            
            //if player is a salvager then send reminder message
            if(salvagers.ContainsKey(steamId)) {
                player.ChatMessage("Remember, you're salvaging!");
            }
        }

        void OnItemAddedToContainer(ItemContainer container, Item item) {
            //get container entity
            BaseEntity entity = container.entityOwner as BaseEntity;
            
            //check entity is repair bench
            if(entity == null) return;
            if(entity.LookupShortPrefabName() != "repairbench_deployed.prefab") return;
            
            //get player steamId
            ulong steamId = entity.OwnerID;
            
            //attempt salvage
            if(salvagers.ContainsKey(steamId))
                SalvageItem(salvagers[steamId], item);
        }

        void SalvageItem(BasePlayer player, Item item) {
            //check player can craft item
        	bool canCraft = player.blueprints.CanCraft(item.info.itemid, 0);

            //if can't craft or broken, else salvage
        	if(!canCraft || (item.hasCondition && item.condition == 0)) {
        		player.ChatMessage("This item is not salvageable.");
        	} else {
                //remove the item from the container
        	    item.RemoveFromContainer();
                
                //get bp
        		ItemBlueprint itemBp = blueprints[item.info.shortname];
                
                //set ratios
        		double refundRatio = 0.5;
                //if item.hasCondition then conditionRatio == item.conditionNormalized else conditionRatio = 1
                double conditionRatio = item.hasCondition ? item.conditionNormalized : 1;
                
                /*
                    itemBp.amountToCreate is the number of items made by the act of crafting (3x 5.56)
                    ingredient.amount is the number of that material required (100 wood for campfire)
                    ingredient.itemDef.stackable is the max stack size of that ingredient
                */
                foreach(ItemAmount ingredient in itemBp.ingredients) {
                    //calculate refund amount
                    double refundAmount = ingredient.amount / itemBp.amountToCreate;
                    refundAmount = refundAmount * refundRatio * conditionRatio * item.amount;
                    refundAmount = Math.Ceiling(refundAmount);
                    if(refundAmount < 1)
                        refundAmount = 1;
                        
                    //calculate how many stacks to split the refund into (no super stacks)
                    double refundStacks = refundAmount / ingredient.itemDef.stackable;
                    if(refundStacks < 1)
                        refundStacks = 1;
                    
                    //refund ingredient
                    for(int i = 0; i < refundStacks; i++) {
                        int amt;
                        
                        //calculate how much to give in this stack
                        if (refundAmount <= ingredient.itemDef.stackable)
                            amt = (int)refundAmount;
                        else
                            amt = ingredient.itemDef.stackable;
                        
                        //reduce the pending refund amount by how much we're about to give out
                        refundAmount = refundAmount - amt;
                        
                        //create and give this stack
                        Item newItem = ItemManager.Create(ingredient.itemDef, amt);
                        player.GiveItem(newItem);
                    }
                }
        	}
        }
    }
}
