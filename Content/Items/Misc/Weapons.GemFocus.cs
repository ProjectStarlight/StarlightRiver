using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Items.Misc
{
    internal class GemFocus : ModItem
    {
        public override string Texture => "StarlightRiver/Invisible";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Focusing Gem");
            Tooltip.SetDefault("Control a magical gemstone focus");
        }

        public override void SetDefaults()
        {
            item.width = 28;
            item.height = 30;
            item.useStyle = ItemUseStyleID.HoldingOut;
            item.useAnimation = 10;
            item.useTime = 10;
            item.knockBack = 1f;
            item.damage = 17;
            item.rare = ItemRarityID.Green;
            item.noMelee = true;
            item.magic = true;
            item.mana = 4;
            item.channel = true;
            item.autoReuse = true;
        }

        public override bool UseItem(Player player)
        {
            if (!Main.projectile.Any(n => n.active && n.type == ProjectileType<Projectiles.WeaponProjectiles.GemFocusProjectile>() && n.owner == player.whoAmI))
            {
                Projectile.NewProjectile(player.Center, Vector2.Zero, ProjectileType<Projectiles.WeaponProjectiles.GemFocusProjectile>(), item.damage, item.knockBack, player.whoAmI);
            }
            return true;
        }

        public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            Texture2D over = GetTexture("StarlightRiver/Items/Misc/GemFocusOver");
            Texture2D under = GetTexture("StarlightRiver/Items/Misc/GemFocusUnder");
            Texture2D glow = GetTexture("StarlightRiver/RiftCrafting/Glow0");

            spriteBatch.Draw(under, position + frame.Size() / 2 * scale, under.Frame(), Color.White, 0, under.Size() / 2, scale, 0, 0);

            float timer = (float)Math.Sin(StarlightWorld.rottime) * 0.1f;
            spriteBatch.Draw(under, position + frame.Size() / 2 * scale, under.Frame(), Main.DiscoColor * (0.4f + timer), 0, under.Size() / 2, scale * 1.3f + timer, 0, 0);

            spriteBatch.Draw(over, position + frame.Size() / 2 * scale, over.Frame(), Color.White, 0, over.Size() / 2, scale, 0, 0);

            spriteBatch.End();
            spriteBatch.Begin(default, BlendState.Additive);

            spriteBatch.Draw(glow, position + frame.Size() / 2 * scale, glow.Frame(), Main.DiscoColor * 0.5f, 0, glow.Size() / 2, scale, 0, 0);

            spriteBatch.End();
            spriteBatch.Begin();

            return false;
        }

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            Texture2D over = GetTexture("StarlightRiver/Items/Misc/GemFocusOver");
            Texture2D under = GetTexture("StarlightRiver/Items/Misc/GemFocusUnder");
            Texture2D glow = GetTexture("StarlightRiver/RiftCrafting/Glow0");

            Vector2 position = item.position - Main.screenPosition;
            Rectangle frame = item.Hitbox;

            spriteBatch.Draw(under, position, under.Frame(), Color.White, 0, Vector2.Zero, scale, 0, 0);

            float timer = (float)Math.Sin(StarlightWorld.rottime) * 0.1f;
            spriteBatch.Draw(under, position + frame.Size() / 2 * scale, under.Frame(), Main.DiscoColor * (0.4f + timer), 0, under.Size() / 2, scale * 1.3f + timer, 0, 0);

            spriteBatch.Draw(over, position, over.Frame(), lightColor, 0, Vector2.Zero, scale, 0, 0);

            spriteBatch.End();
            spriteBatch.Begin(default, BlendState.Additive);

            spriteBatch.Draw(glow, position + frame.Size() / 2 * scale, glow.Frame(), Main.DiscoColor * 0.5f, 0, glow.Size() / 2, scale, 0, 0);

            spriteBatch.End();
            spriteBatch.Begin();

            Lighting.AddLight(item.Center, Main.DiscoColor.ToVector3());
            return false;
        }
    }
}