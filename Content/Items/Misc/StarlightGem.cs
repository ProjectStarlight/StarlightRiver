using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using static Terraria.ModLoader.ModContent;

using StarlightRiver.Core;

namespace StarlightRiver.Content.Items.Misc
{
    internal class StarlightGem : ModItem
    {
        public int gemID = 0;
        public override bool CloneNewInstances => true;

        public override string Texture => AssetDirectory.MiscItem + Name;

        public override void SetDefaults()
        {
            item.width = 18;
            item.height = 20;
        }

        public override void SetStaticDefaults()
        {
            ItemID.Sets.ItemNoGravity[item.type] = true;
        }

        public override void PostUpdate()
        {
            Dust.NewDust(item.position - new Vector2(1, 1), 18, 20, DustType<Dusts.Starlight>(), 0, 0, 0, default, 0.5f);
        }

        public override bool CanPickup(Player player)
        {
            return !(player.GetModPlayer<GemHandler>().gems[gemID] == 1);
        }

        public override bool OnPickup(Player player)
        {
            player.GetModPlayer<GemHandler>().gems[gemID] = 1;

            CombatText.NewText(player.Hitbox, new Color(120, 245 - gemID, 175 + gemID), "Starlight Gem #" + (gemID + 1) + " Accquired!");
            for (float k = 0; k <= 6.28f; k += 0.1f)
                Dust.NewDustPerfect(item.Center, DustType<Dusts.Starlight>(), Vector2.One.RotatedBy(k) * (k % 0.79f) * 15, 0, new Color(120, 245 - gemID, 175 + gemID), 3 - k % 0.79f * 3);
            return false;
        }

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            spriteBatch.Draw(GetTexture("StarlightRiver/Assets/Items/Misc/StarlightGem"), item.Center - Main.screenPosition, new Rectangle(0, 0, 18, 20), new Color(120, 245 - gemID, 175 + gemID) * 0.7f,
                rotation, new Vector2(9, 10), 1, 0, 0);
            return false;
        }
    }

    internal class GemHandler : ModPlayer
    {
        public byte[] gems = new byte[100];

        public override TagCompound Save()
        {
            return new TagCompound()
            {
                [nameof(gems)] = gems
            };
        }

        public override void Load(TagCompound tag)
        {
            gems = tag.GetByteArray(nameof(gems));
        }

        public override void PreUpdate()
        {
        }
    }
}