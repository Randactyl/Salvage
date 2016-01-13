using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Oxide.Plugins {

    [Info("Salvage", "Randactyl", "1.0.0")]
    [Description("Use /salvage to enable/disable salvage mode at repair benches.")]
    public class Salvage : RustPlugin {
        Dictionary<BasePlayer, bool> salvagers = new Dictionary<BasePlayer, bool>(); //[player] = boolean
        Dictionary<BasePlayer, BaseEntity> benchMaps = new Dictionary<BasePlayer, BaseEntity>(); //[player] = entity
        Dictionary<string, ItemBlueprint> blueprints = new Dictionary<string, ItemBlueprint>(); //[shortname] = itemBp

        void OnServerInitialized() {
            blueprints = ItemManager.bpList.ToDictionary(itemBp => itemBp.targetItem.shortname, itemBp => itemBp);
        }

        [ChatCommand("salvage")]
        void cmdSalvage(BasePlayer player, string command, string[] args) {
            bool state;
            salvagers.TryGetValue(player, out state);

            if(state) {
                salvagers.Remove(player);
                benchMaps.Remove(player);
                player.ChatMessage("Salvaging is now OFF.");
            } else {
                salvagers[player] = true;
                player.ChatMessage("Salvaging is now ON. Visit a repair bench to salvage items.");
            }
        }

        void OnLootEntity(BaseEntity source, BaseEntity target) {
            if(source == null || target == null) return;
            if(target.LookupShortPrefabName() != "repairbench_deployed.prefab") return;

            BasePlayer player = (BasePlayer) source;
            bool isSalvager;
            salvagers.TryGetValue(player, out isSalvager);

            if(isSalvager) {
                benchMaps[player] = target;
                player.ChatMessage("Remember, you're salvaging!");
            }
        }

        void OnItemAddedToContainer(ItemContainer container, Item item) {
            BaseEntity entity = container.entityOwner as BaseEntity;
            
            if(entity == null) return;
            if(entity.LookupShortPrefabName() != "repairbench_deployed.prefab") return;

            BasePlayer player = null;
            foreach(BasePlayer key in benchMaps.Keys)
                if(benchMaps[key] == entity) {
                    player = key;
                    break;
                }

            if(player == null) return;

            SalvageItem(player, item);
        }

        void SalvageItem(BasePlayer player, Item item) {
        	bool canCraft = player.blueprints.CanCraft(item.info.itemid, 0);

        	item.RemoveFromContainer();

        	if(!canCraft || (item.hasCondition && item.condition == 0)) {
        		player.GiveItem(item);
        		//Puts("Giving " + player.ToString() + " " + item.ToString());
        		player.ChatMessage("This item is not salvageable.");
        	} else {
        		ItemBlueprint itemBp = blueprints[item.info.shortname];
        		double refundRatio = 0.5;
                double conditionRatio = item.hasCondition ? item.conditionNormalized : 1;

                foreach(ItemAmount ingredient in itemBp.ingredients) {
                    double refundAmount = ingredient.amount / itemBp.amountToCreate;
                    refundAmount = refundAmount * refundRatio * conditionRatio * item.amount;
                    refundAmount = Math.Ceiling(refundAmount);
                    if(refundAmount < 1)
                        refundAmount = 1;

                    Item newItem = ItemManager.Create(ingredient.itemDef, (int)refundAmount);
                    player.GiveItem(newItem);
                    //Puts("Giving " + player.ToString() + " " + newItem.ToString());
                }
        	}
        }
    }
}
