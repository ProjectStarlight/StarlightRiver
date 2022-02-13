using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using System;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Items.Hell
{
	/*class MagmaSword : ModItem, IGlowingItem
    {
        public override string Texture => "StarlightRiver/Assets/Items/Hell/MagmaSword";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("[PH] Magma Sword");
            Tooltip.SetDefault("Launches blobs of burning magma");
        }

        public override void SetDefaults()
        {
            item.melee = true;
            item.width = 32;
            item.height = 32;
            item.noMelee = true;
            item.noUseGraphic = true;
            item.damage = 32;
            item.crit = 4;
            item.knockBack = 0.5f;
            item.useTime = 45;
            item.useAnimation = 45;
            item.useStyle = ItemUseStyleID.SwingThrow;
            item.rare = ItemRarityID.Orange;
            item.shoot = ProjectileType<MagmaSwordBlob>();
            item.shootSpeed = 11;
        }

        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            for (int k = 0; k < 4; k++)
            {
                int i = Projectile.NewProjectile(player.Center + new Vector2(0, -32), new Vector2(speedX, speedY).RotatedByRandom(0.25f) * ((k + 3) * 0.08f), type, damage, knockBack, player.whoAmI);
                Main.projectile[i].scale = Main.rand.NextFloat(0.4f, 0.9f);
            }
            return false;
        }

        public void DrawGlowmask(PlayerDrawInfo info)
        {
            Player player = info.drawPlayer;

            if (player.itemAnimation != 0)
            {
                Texture2D tex = GetTexture("StarlightRiver/Assets/Items/Hell/MagmaSwordHilt");
                Texture2D tex2 = GetTexture("StarlightRiver/Assets/Items/Hell/MagmaSwordGlow");
                Rectangle frame = new Rectangle(0, 0, 50, 50);
                Color color = Lighting.GetColor((int)player.Center.X / 16, (int)player.Center.Y / 16);
                Vector2 origin = new Vector2(player.direction == 1 ? 0 : frame.Width, frame.Height);

                Main.playerDrawData.Add(new DrawData(tex, info.itemLocation - Main.screenPosition, frame, color, player.itemRotation, origin, player.HeldItem.scale, info.spriteEffects, 0));
                Main.playerDrawData.Add(new DrawData(tex2, info.itemLocation - Main.screenPosition, frame, Color.White, player.itemRotation, origin, player.HeldItem.scale, info.spriteEffects, 0));
            }
        }
    }*/

    class MagmaSwordBlob : ModProjectile
    {
        public override string Texture => "StarlightRiver/Assets/Items/Hell/MagmaSwordBlob";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[projectile.type] = 20;
            ProjectileID.Sets.TrailingMode[projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            projectile.width = 32;
            projectile.height = 32;
            projectile.friendly = true;
            projectile.melee = true;
            projectile.penetrate = -1;
            projectile.timeLeft = 300;
            projectile.scale = 0.5f;
            projectile.extraUpdates = 1;
        }

        public override void AI() => projectile.velocity.Y += 0.1f;

        public override void Kill(int timeLeft)
        {
            for (int x = -3; x < 3; x++)
            {
                for (int y = -3; y < 3; y++)
                {
                    Tile tile = Main.tile[(int)projectile.Center.X / 16 + x, (int)projectile.Center.Y / 16 + y];
                    if (tile.active() && Main.tileSolid[tile.type])
                    {
                        Vector2 pos = new Vector2((int)projectile.Center.X / 16 + x, (int)projectile.Center.Y / 16 + y) * 16 + Vector2.One * 8;
                        if (!Main.projectile.Any(n => n.active && n.type == ProjectileType<MagmaSwordBurn>() && n.Center == pos))
                            Projectile.NewProjectile(pos, Vector2.Zero, ProjectileType<MagmaSwordBurn>(), 5, 0, projectile.owner);
                        else Main.projectile.FirstOrDefault(n => n.active && n.type == ProjectileType<MagmaSwordBurn>() && n.Center == pos).timeLeft = 180;
                    }
                }
                Dust.NewDust(projectile.position, projectile.width, projectile.height, DustID.AmberBolt, 0, 0, 0, default, 0.5f);
            }
            Main.PlaySound(SoundID.Drown, projectile.Center);
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor) => false;

        public override void PostDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            for (int k = 0; k < projectile.oldPos.Length; k++)
            {
                Color color = new Color(255, 175 + (int)(Math.Sin(StarlightWorld.rottime * 5 + k / 2) * 50), 50) * ((float)(projectile.oldPos.Length - k) / projectile.oldPos.Length * 0.4f);
                float scale = projectile.scale * (projectile.oldPos.Length - k) / projectile.oldPos.Length;
                Texture2D tex = GetTexture(Texture);
                Texture2D tex2 = GetTexture("StarlightRiver/Assets/Keys/Glow");

                spriteBatch.Draw(tex, projectile.oldPos[k] + projectile.Size - Main.screenPosition, null, color, 0, tex.Size() / 2, scale, default, default);
                spriteBatch.Draw(tex2, projectile.oldPos[k] + projectile.Size - Main.screenPosition, null, Color.White, 0, tex2.Size() / 2, scale * 0.3f, default, default);
            }
        }
    }

    class MagmaSwordBurn : ModProjectile, IDrawAdditive
    {
        public override string Texture => AssetDirectory.Invisible;

        public override bool OnTileCollide(Vector2 oldVelocity) => false;

        public override void SetDefaults()
        {
            projectile.width = 18;
            projectile.height = 18;
            projectile.friendly = true;
            projectile.melee = true;
            projectile.penetrate = -1;
            projectile.timeLeft = 180;
            projectile.tileCollide = false;
            projectile.damage = 1;
        }

        public override void AI()
        {
            Tile tile = Main.tile[(int)projectile.Center.X / 16, (int)projectile.Center.Y / 16];
            if (!tile.active()) projectile.timeLeft = 0;

            Lighting.AddLight(projectile.Center, new Vector3(1.1f, 0.5f, 0.2f) * (projectile.timeLeft / 180f));
        }

        public override bool? CanHitNPC(NPC target)
        {
            if (target.Hitbox.Intersects(projectile.Hitbox)) target.GetGlobalNPC<StarlightNPC>().DoT += (int)((float)projectile.damage * projectile.timeLeft / 180f);
            return false;
        }

        public override void PostDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Tile tile = Main.tile[(int)projectile.Center.X / 16, (int)projectile.Center.Y / 16];
            Texture2D tex = Main.tileTexture[tile.type];
            Rectangle frame = new Rectangle(tile.frameX, tile.frameY, 16, 16);
            Vector2 pos = projectile.position + Vector2.One - Main.screenPosition;
            Color color = new Color(255, 140, 50) * 0.2f * (projectile.timeLeft / 180f);

            spriteBatch.Draw(tex, pos, frame, color, 0, Vector2.Zero, 1, 0, 0);
        }

        public void DrawAdditive(SpriteBatch spriteBatch)
        {
            Texture2D tex = GetTexture("StarlightRiver/Assets/Keys/Glow");
            Color color = new Color(255, 100, 50) * 0.3f * (projectile.timeLeft / 180f);
            spriteBatch.Draw(tex, projectile.Center - Main.screenPosition, tex.Frame(), color, 0, tex.Size() / 2, 1.2f * (projectile.timeLeft / 180f), 0, 0);
        }
    }

}
