using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Items.BaseTypes
{
	internal abstract class InfectedAccessory : SmartAccessory
    {
        public InfectedAccessory() : base("Unnamed Infected Accessory", "you forgot to set a display name/tooltip dingus!") { }
        public override bool CanEquipAccessory(Player Player, int slot)
        {
            //Main.NewText("Slot: " + slot, 255, 255, 0);//debug?
            if (slot == 3) return false;
            if (!Player.armor[slot - 1].IsAir) return false;
            if (slot > 7 + Player.extraAccessorySlots) return false;

            Item blocker = new Item
            {
                type = ItemType<Blocker>()
            };
            blocker.SetDefaults(ItemType<Blocker>());
            (blocker.ModItem as Blocker).Parent = Item;
            Player.armor[slot - 1] = blocker;
            return true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            TooltipLine line = new TooltipLine(Mod, "StarlightRiverInfectedWarning", "Infected! Requires 2 accessory slots")
            {
                overrideColor = new Color(100, 160, 120)
            };
            tooltips.Add(line);
        }

        public virtual bool SafePreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color ItemColor, Vector2 origin, float scale)
        {
            return true;
        }

        public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color ItemColor, Vector2 origin, float scale)
        {
            if (Main.LocalPlayer.armor.Any(n => n == Item))
            {
                Texture2D tex = Request<Texture2D>("StarlightRiver/Assets/GUI/InfectedGoop").Value;
                spriteBatch.Draw(tex, position + new Vector2(-10, -35), tex.Frame(), Color.White);
            }

            return SafePreDrawInInventory(spriteBatch, position, frame, drawColor, ItemColor, origin, scale);
        }
    }

    internal class Blocker : ModItem
    {
        public override string Texture => AssetDirectory.Invisible;
        public Item Parent { get; set; }

        public override void SetDefaults()
        {
            Item.accessory = true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Clear();
        }

        public override void UpdateEquip(Player Player)
        {
            if (!Player.armor.Any(n => n.type == Parent.type)) Item.TurnToAir();
        }

        public override void SaveData(TagCompound tag)
        {
            return new TagCompound
            {
                [nameof(Parent)] = Parent
            };
        }

        public override void LoadData(TagCompound tag)
        {
            Parent = tag.Get<Item>(nameof(Parent));
        }
    }
}