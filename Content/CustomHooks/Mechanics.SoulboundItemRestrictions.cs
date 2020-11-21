using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using StarlightRiver.Physics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.CustomHooks
{
    class SoulboundItemRestrictions : HookGroup
    {
        //Some vanilla behavior buttfuckery in here, should be wary.
        public override SafetyLevel Safety => SafetyLevel.Questionable;

        public override void Load()
        {
            On.Terraria.Player.DropSelectedItem += DontDropSoulbound;
            On.Terraria.Player.dropItemCheck += SoulboundPriority;
            On.Terraria.Player.ItemFitsItemFrame += NoSoulboundFrame;
            On.Terraria.Player.ItemFitsWeaponRack += NoSoulboundRack;
        }

        private bool NoSoulboundFrame(On.Terraria.Player.orig_ItemFitsItemFrame orig, Player self, Item i) => !(i.modItem is Items.SoulboundItem) && orig(self, i);

        private bool NoSoulboundRack(On.Terraria.Player.orig_ItemFitsWeaponRack orig, Player self, Item i) => !(i.modItem is Items.SoulboundItem) && orig(self, i);

        private void SoulboundPriority(On.Terraria.Player.orig_dropItemCheck orig, Player self)
        {
            if (Main.mouseItem.type > ItemID.None && !Main.playerInventory && Main.mouseItem.modItem != null && Main.mouseItem.modItem is Items.SoulboundItem)
            {
                for (int k = 49; k > 0; k--)
                {
                    Item item = self.inventory[k];

                    if (!(self.inventory[k].modItem is Items.SoulboundItem) || k == 0)
                    {
                        int index = Item.NewItem(self.position, item.type, item.stack, false, item.prefix, false, false);
                        Main.item[index] = item.Clone();
                        Main.item[index].position = self.position;
                        item.TurnToAir();
                        break;
                    }
                }
            }
            orig(self);
        }

        private void DontDropSoulbound(On.Terraria.Player.orig_DropSelectedItem orig, Player self)
        {
            if (self.inventory[self.selectedItem].modItem is Items.SoulboundItem || Main.mouseItem.modItem is Items.SoulboundItem) return;
            else orig(self);
        }
    }
}