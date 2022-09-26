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

namespace StarlightRiver.Content.Items.Vanity
{
    [AutoloadEquip(EquipType.Head)]
    public class WardenHat : ModItem
    {
        public override string Texture => AssetDirectory.VanityItem + Name;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Warden Hat");
            Tooltip.SetDefault("[c/198E12:???]");
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

    public class WardenRobe : ModItem
    {
        public override string Texture => AssetDirectory.VanityItem + Name;

        public override void Load()
        {
            if (Main.netMode != NetmodeID.Server)
                EquipLoader.AddEquipTexture(Mod, AssetDirectory.Invisible, EquipType.Body, null, "WardenRobe_Body");
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Warden Robe");
            Tooltip.SetDefault("[c/198E12:???]");

            if (Main.netMode != NetmodeID.Server)
                ArmorIDs.Body.Sets.HidesArms[EquipLoader.GetEquipSlot(Mod, "WardenRobe_Body", EquipType.Body)] = true;
        }

        public override void SetDefaults()
        {
            Item.bodySlot = EquipLoader.GetEquipSlot(Mod, "WardenRobe_Body", EquipType.Body);
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

        public int yFrame = 0;
        public int frameCounter = 0;

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

            if (!SetEquipped)
                return;

            Texture2D tex = ModContent.Request<Texture2D>(AssetDirectory.VanityItem + "WardenQuestionmark").Value;
            Player armorOwner = drawInfo.drawPlayer;
            Vector2 drawPos = (armorOwner.MountedCenter - Main.screenPosition) - new Vector2(0, 3 - armorOwner.gfxOffY);

            for (int i = 0; i < NUM_MARKS; i++)
            {
                float rot = ((i * 6.28f) / (float)NUM_MARKS) + (float)(Main.timeForVisualEffects * 0.05f);
                Vector2 offset = rot.ToRotationVector2() * 54;
                DrawData data2 = new DrawData(
                    tex,
                    new Vector2((int)(drawPos.X + offset.X), (int)(drawPos.Y + offset.Y)),
                    null,
                    new Color(255, 255, 255, 0),
                    0f,
                    tex.Size() / 2,
                    1,
                    SpriteEffects.None,
                    0
                )
                {
                    shader = drawInfo.cHead
                };
                drawInfo.DrawDataCache.Add(data2);
            }
        }

        public override void PostUpdate()
        {
            if (robeEquipped)
            {
                Player.legFrame = new Rectangle(0, 9000, 40, 56);
                Player.bodyFrame = new Rectangle(0, 0, 40, 56);
                frameCounter++;
            }
        }
    }
}