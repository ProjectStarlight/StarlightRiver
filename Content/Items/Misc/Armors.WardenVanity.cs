using StarlightRiver.Core;
using StarlightRiver.Helpers;
using StarlightRiver.Content.Dusts;
using System;
using System.Linq;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Graphics.Effects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using static Terraria.ModLoader.ModContent;
using System.IO;
using Terraria.GameContent;
using Terraria.DataStructures;

namespace StarlightRiver.Content.Items.Breacher
{
    [AutoloadEquip(EquipType.Head)]
    public class WardenHat : ModItem
    {
        public override string Texture => AssetDirectory.MiscItem + Name;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Warden Hat");
            Tooltip.SetDefault("[PH] someone update this");
        }

        public override void SetDefaults()
        {
            Item.vanity = true;
            Item.width = 28;
            Item.height = 28;
            Item.value = Item.buyPrice(0, 5, 0, 0);
            Item.rare = ItemRarityID.Green;
        }
    }

    [AutoloadEquip(EquipType.Body)]
    public class WardenRobe : ModItem
    {
        public override string Texture => AssetDirectory.MiscItem + Name;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Warden Robe");
            Tooltip.SetDefault("[PH] someone update this");
        }

        public override void SetDefaults()
        {
            Item.vanity = true;
            Item.width = 34;
            Item.height = 20;
            Item.value = Item.buyPrice(0, 5, 0, 0);
            Item.rare = ItemRarityID.Green;
        }
    }

    public class WardenVanityPlayer : ModPlayer
    {
        public const int NUM_MARKS = 12;

        public bool robeEquipped = false;
        public bool hatEquipped = false;

        public bool SetEquipped => robeEquipped && hatEquipped;

        public override void ResetEffects() //Unfortunately as of right now there's no hook in ModItem to check if an ARMOR is equipped in vanity (UpdateVanity only works for accessories) so this'll have to do
        {
            robeEquipped = ((Player.armor[1].type == ItemType<WardenRobe>() && Player.armor[11].type == 0) || Player.armor[11].type == ItemType<WardenRobe>());
            hatEquipped = ((Player.armor[0].type == ItemType<WardenHat>() && Player.armor[10].type == 0) || Player.armor[10].type == ItemType<WardenHat>());
        }

        public override void ModifyDrawInfo(ref PlayerDrawSet drawInfo)
        {
            if (robeEquipped)
                drawInfo.armorHidesArms = true;
        }

        public override void DrawEffects(PlayerDrawSet drawInfo, ref float r, ref float g, ref float b, ref float a, ref bool fullBright)
        {
            if (!SetEquipped)
                return;

            Texture2D tex = ModContent.Request<Texture2D>(AssetDirectory.MiscItem + "WardenQuestionmark").Value;
            Player armorOwner = drawInfo.drawPlayer;

            Vector2 drawPos = (armorOwner.MountedCenter - Main.screenPosition) - new Vector2(0, 3 - armorOwner.gfxOffY);

            for (int i = 0; i < NUM_MARKS; i++)
            {
                float rot = ((i * 6.28f) / (float)NUM_MARKS) + (float)(Main.timeForVisualEffects * 0.05f);
                Vector2 offset = rot.ToRotationVector2() * 54;
                DrawData value = new DrawData(
                    tex,
                    new Vector2((int)(drawPos.X + offset.X), (int)(drawPos.Y + offset.Y)),
                    null,
                    new Color(255,255,255,0),
                    0f,
                    tex.Size() / 2,
                    1,
                    SpriteEffects.None,
                    0
                );
                drawInfo.DrawDataCache.Add(value);
            }
        }

        public override void PostUpdate()
        {
            if (robeEquipped)
            {
                Player.legFrame = new Rectangle(0, 9000, 40, 56);
            }
        }
    }

    public class WardenVanityGNPC : GlobalNPC
    {
        public override void SetupTravelShop(int[] shop, ref int nextSlot)
        {
            if (Main.rand.NextBool(15))
            {
                shop[nextSlot] = ModContent.ItemType<WardenHat>();
                nextSlot++;
                shop[nextSlot] = ModContent.ItemType<WardenRobe>();
                nextSlot++;
            }
        }
    }
}