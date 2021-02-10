using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using StarlightRiver.Core;


namespace StarlightRiver.Content.Items.Misc
{
    internal class GemFocus : ModItem
    {
        public override string Texture => AssetDirectory.Invisible;

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
            if (!Main.projectile.Any(n => n.active && n.type == ProjectileType<GemFocusProjectile>() && n.owner == player.whoAmI))
                Projectile.NewProjectile(player.Center, Vector2.Zero, ProjectileType<GemFocusProjectile>(), item.damage, item.knockBack, player.whoAmI);
            return true;
        }

        public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            Texture2D over = GetTexture("StarlightRiver/Assets/Items/Misc/GemFocusOver");
            Texture2D under = GetTexture("StarlightRiver/Assets/Items/Misc/GemFocusUnder");
            Texture2D glow = GetTexture("StarlightRiver/Assets/RiftCrafting/Glow0");

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
            Texture2D over = GetTexture("StarlightRiver/Assets/Items/Misc/GemFocusOver");
            Texture2D under = GetTexture("StarlightRiver/Assets/Items/Misc/GemFocusUnder");
            Texture2D glow = GetTexture("StarlightRiver/Assets/RiftCrafting/Glow0");

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

    internal class GemFocusProjectile : ModProjectile
    {
        public override string Texture => AssetDirectory.Invisible;

        public override void SetDefaults()
        {
            projectile.friendly = true;
            projectile.aiStyle = -1;
            projectile.timeLeft = 30;
            projectile.width = 42;
            projectile.height = 42;
            projectile.tileCollide = true;
            projectile.penetrate = -1;
            projectile.netSpam = 1;
        }

        public override bool OnTileCollide(Vector2 oldVelocity) => false;

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            if (projectile.ai[1] == 0)
            {
                projectile.ai[1] = 15;
                Main.PlaySound(SoundID.DD2_WitherBeastAuraPulse, projectile.Center);
            }
        }

        public override void AI()
        {
            if (projectile.ai[1] > 0) projectile.ai[1]--;

            if (Main.player[projectile.owner].channel && Main.player[projectile.owner].statMana > 0)
            {
                projectile.timeLeft = 30;

                if (projectile.ai[0] < 30) projectile.ai[0]++;

                projectile.velocity += Vector2.Normalize(Main.MouseWorld - projectile.Center) * 0.3f;

                if (projectile.velocity.Length() > 5) projectile.velocity = Vector2.Normalize(projectile.velocity) * 5;

                projectile.rotation = projectile.velocity.X * 0.1f;
            }
            else
            {
                projectile.ai[0]--;
                projectile.velocity *= 0;
            }

            projectile.alpha = (int)(projectile.ai[0] / 30f * 255);

            for (int k = 0; k < 6; k++)
            {
                Color color = Color.White;

                switch (k)
                {
                    case 0: color = new Color(255, 150, 150); break;
                    case 1: color = new Color(150, 255, 150); break;
                    case 2: color = new Color(150, 150, 255); break;
                    case 3: color = new Color(255, 240, 150); break;
                    case 4: color = new Color(230, 150, 255); break;
                    case 5: color = new Color(255, 255, 255); break;
                }

                float x = (float)Math.Cos(StarlightWorld.rottime + k) * projectile.ai[0] / 30f * 40;
                float y = (float)Math.Sin(StarlightWorld.rottime + k) * projectile.ai[0] / 30f * 10;
                Vector2 pos = (new Vector2(x, y)).RotatedBy(k / 12f * 6.28f);

                Dust d = Dust.NewDustPerfect(projectile.Center, DustType<Content.Dusts.GemFocusDust>(), pos, 0, color, 1f);
                d.customData = projectile;
            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Texture2D over = GetTexture("StarlightRiver/Assets/Items/Misc/GemFocusOver");
            Texture2D under = GetTexture("StarlightRiver/Assets/Items/Misc/GemFocusUnder");
            Texture2D glow = GetTexture("StarlightRiver/Assets/RiftCrafting/Glow0");

            Vector2 position = projectile.position - Main.screenPosition;
            float scale = projectile.scale;
            float fade = (projectile.alpha / 255f);
            float pulse = 1 - projectile.ai[1] / 15f;
            //Rectangle frame = under.Frame();

            spriteBatch.Draw(under, position + projectile.Size / 2 * scale, under.Frame(), Color.White * fade, projectile.rotation, under.Size() / 2, scale, 0, 0);

            float timer = (float)Math.Sin(StarlightWorld.rottime) * 0.1f;
            spriteBatch.Draw(under, position + projectile.Size / 2 * scale, under.Frame(), Main.DiscoColor * (0.4f + timer) * fade, projectile.rotation, under.Size() / 2, scale * 1.3f + timer, 0, 0);

            spriteBatch.Draw(over, position + projectile.Size / 2 * scale, over.Frame(), lightColor * fade, projectile.rotation, over.Size() / 2, scale, 0, 0);

            spriteBatch.End();
            spriteBatch.Begin(default, BlendState.Additive);

            spriteBatch.Draw(glow, position + projectile.Size / 2 * scale, glow.Frame(), Main.DiscoColor * 0.5f * fade, projectile.rotation, glow.Size() / 2, scale, 0, 0);

            if (projectile.ai[1] > 0)
                spriteBatch.Draw(glow, position + projectile.Size / 2 * scale, glow.Frame(), Main.DiscoColor * (1 - pulse) * fade, projectile.rotation, glow.Size() / 2, scale * (1 + pulse * 2), 0, 0);

            spriteBatch.End();
            spriteBatch.Begin();

            Lighting.AddLight(projectile.Center, Main.DiscoColor.ToVector3() * 0.5f * fade);
            return false;
        }
    }
}