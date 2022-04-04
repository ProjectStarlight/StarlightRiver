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
	class MagmaSword : ModItem, IGlowingItem
    {
        public override string Texture => "StarlightRiver/Assets/Items/Hell/MagmaSword";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("[PH] Magma Sword");
            Tooltip.SetDefault("Launches blobs of burning magma");
        }

        public override void SetDefaults()
        {
            Item.melee = true;
            Item.width = 32;
            Item.height = 32;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.damage = 32;
            Item.crit = 4;
            Item.knockBack = 0.5f;
            Item.useTime = 45;
            Item.useAnimation = 45;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.rare = ItemRarityID.Orange;
            Item.shoot = ProjectileType<MagmaSwordBlob>();
            Item.shootSpeed = 11;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            for (int k = 0; k < 4; k++)
            {
                int i = Projectile.NewProjectile(Player.Center + new Vector2(0, -32), new Vector2(speedX, speedY).RotatedByRandom(0.25f) * ((k + 3) * 0.08f), type, damage, knockBack, Player.whoAmI);
                Main.projectile[i].scale = Main.rand.NextFloat(0.4f, 0.9f);
            }
            return false;
        }

        public void DrawGlowmask(PlayerDrawInfo info)
        {
            Player Player = info.drawPlayer;

            if (Player.ItemAnimation != 0)
            {
                Texture2D tex = Request<Texture2D>("StarlightRiver/Assets/Items/Hell/MagmaSwordHilt").Value;
                Texture2D tex2 = Request<Texture2D>("StarlightRiver/Assets/Items/Hell/MagmaSwordGlow").Value;
                Rectangle frame = new Rectangle(0, 0, 50, 50);
                Color color = Lighting.GetColor((int)Player.Center.X / 16, (int)Player.Center.Y / 16);
                Vector2 origin = new Vector2(Player.direction == 1 ? 0 : frame.Width, frame.Height);

                Main.playerDrawData.Add(new DrawData(tex, info.ItemLocation - Main.screenPosition, frame, color, Player.ItemRotation, origin, Player.HeldItem.scale, info.spriteEffects, 0));
                Main.playerDrawData.Add(new DrawData(tex2, info.ItemLocation - Main.screenPosition, frame, Color.White, Player.ItemRotation, origin, Player.HeldItem.scale, info.spriteEffects, 0));
            }
        }
    }

    class MagmaSwordBlob : ModProjectile
    {
        public override string Texture => "StarlightRiver/Assets/Items/Hell/MagmaSwordBlob";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 20;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 32;
            Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.melee = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 300;
            Projectile.scale = 0.5f;
            Projectile.extraUpdates = 1;
        }

        public override void AI() => Projectile.velocity.Y += 0.1f;

        public override void Kill(int timeLeft)
        {
            for (int x = -3; x < 3; x++)
            {
                for (int y = -3; y < 3; y++)
                {
                    Tile tile = Main.tile[(int)Projectile.Center.X / 16 + x, (int)Projectile.Center.Y / 16 + y];
                    if (tile.HasTile && Main.tileSolid[tile.type])
                    {
                        Vector2 pos = new Vector2((int)Projectile.Center.X / 16 + x, (int)Projectile.Center.Y / 16 + y) * 16 + Vector2.One * 8;
                        if (!Main.projectile.Any(n => n.active && n.type == ProjectileType<MagmaSwordBurn>() && n.Center == pos))
                            Projectile.NewProjectile(pos, Vector2.Zero, ProjectileType<MagmaSwordBurn>(), 5, 0, Projectile.owner);
                        else Main.projectile.FirstOrDefault(n => n.active && n.type == ProjectileType<MagmaSwordBurn>() && n.Center == pos).timeLeft = 180;
                    }
                }
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.AmberBolt, 0, 0, 0, default, 0.5f);
            }
            Terraria.Audio.SoundEngine.PlaySound(SoundID.Drown, Projectile.Center);
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor) => false;

        public override void PostDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            for (int k = 0; k < Projectile.oldPos.Length; k++)
            {
                Color color = new Color(255, 175 + (int)(Math.Sin(StarlightWorld.rottime * 5 + k / 2) * 50), 50) * ((float)(Projectile.oldPos.Length - k) / Projectile.oldPos.Length * 0.4f);
                float scale = Projectile.scale * (Projectile.oldPos.Length - k) / Projectile.oldPos.Length;
                Texture2D tex = Request<Texture2D>(Texture).Value;
                Texture2D tex2 = Request<Texture2D>("StarlightRiver/Assets/Keys/Glow").Value;

                spriteBatch.Draw(tex, Projectile.oldPos[k] + Projectile.Size - Main.screenPosition, null, color, 0, tex.Size() / 2, scale, default, default);
                spriteBatch.Draw(tex2, Projectile.oldPos[k] + Projectile.Size - Main.screenPosition, null, Color.White, 0, tex2.Size() / 2, scale * 0.3f, default, default);
            }
        }
    }

    class MagmaSwordBurn : ModProjectile, IDrawAdditive
    {
        public override string Texture => AssetDirectory.Invisible;

        public override bool OnTileCollide(Vector2 oldVelocity) => false;

        public override void SetDefaults()
        {
            Projectile.width = 18;
            Projectile.height = 18;
            Projectile.friendly = true;
            Projectile.melee = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = false;
            Projectile.damage = 1;
        }

        public override void AI()
        {
            Tile tile = Main.tile[(int)Projectile.Center.X / 16, (int)Projectile.Center.Y / 16];
            if (!tile.HasTile) Projectile.timeLeft = 0;

            Lighting.AddLight(Projectile.Center, new Vector3(1.1f, 0.5f, 0.2f) * (Projectile.timeLeft / 180f));
        }

        public override bool? CanHitNPC(NPC target)
        {
            if (target.Hitbox.Intersects(Projectile.Hitbox)) target.GetGlobalNPC<StarlightNPC>().DoT += (int)((float)Projectile.damage * Projectile.timeLeft / 180f);
            return false;
        }

        public override void PostDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Tile tile = Main.tile[(int)Projectile.Center.X / 16, (int)Projectile.Center.Y / 16];
            Texture2D tex = Main.tileTexture[tile.type];
            Rectangle frame = new Rectangle(tile.TileFrameX, tile.TileFrameY, 16, 16);
            Vector2 pos = Projectile.position + Vector2.One - Main.screenPosition;
            Color color = new Color(255, 140, 50) * 0.2f * (Projectile.timeLeft / 180f);

            spriteBatch.Draw(tex, pos, frame, color, 0, Vector2.Zero, 1, 0, 0);
        }

        public void DrawAdditive(SpriteBatch spriteBatch)
        {
            Texture2D tex = Request<Texture2D>("StarlightRiver/Assets/Keys/Glow").Value;
            Color color = new Color(255, 100, 50) * 0.3f * (Projectile.timeLeft / 180f);
            spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, tex.Frame(), color, 0, tex.Size() / 2, 1.2f * (Projectile.timeLeft / 180f), 0, 0);
        }
    }

}
