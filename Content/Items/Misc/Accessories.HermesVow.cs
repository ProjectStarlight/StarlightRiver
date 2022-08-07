using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Items.BaseTypes;
using StarlightRiver.Core;
using System;
using Terraria;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Misc
{
    public class HermesVow : CursedAccessory
    {
        public override string Texture => AssetDirectory.MiscItem + Name;

        public HermesVow() : base(ModContent.Request<Texture2D>(AssetDirectory.MiscItem + "HermesVow").Value) { }

        public override void Load()
        {
            StarlightPlayer.PostUpdateRunSpeedsEvent += AddRunSpeeds;
            StarlightItem.CanEquipAccessoryEvent += PreventWingUse;
        }

        public override void Unload()
        {
            StarlightPlayer.PostUpdateRunSpeedsEvent -= AddRunSpeeds;
            StarlightItem.CanEquipAccessoryEvent -= PreventWingUse;
        }

        private bool PreventWingUse(Item item, Player player, int slot, bool modded)
        {
            if (Equipped(player))
                if (item.wingSlot > 0)
                    return false;

            return true;
        }

        private void AddRunSpeeds(Player player)
        {
            if (Equipped(player))
            {
                player.accRunSpeed += 3.5f;
                player.moveSpeed += 0.3f;
                player.maxRunSpeed += 0.75f;
            }
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Hermes' Vow");
            Tooltip.SetDefault("Cursed\nMassively increased acceleration and movement speed\nIncreased jump height and max movement speed\nYou are unable to use wings");
        }

        public override void OnEquip(Player Player, Item item)
        {
            for (int i = 3; i < 10; i++)
            {
                if (Player.IsAValidEquipmentSlotForIteration(i))
                {
                    Item wingItem = Player.armor[i];
                    if (wingItem.wingSlot > 0)
                    {
                        Player.QuickSpawnClonedItem(Player.GetSource_Accessory(Item), wingItem);
                        wingItem.wingSlot = 0;
                        wingItem.TurnToAir();
                    }
                }
            }
        }

        public override void SafeUpdateEquip(Player player)
        {
            player.jumpSpeedBoost += 2f;
            player.extraFall += 10;
        }
    }   
}
