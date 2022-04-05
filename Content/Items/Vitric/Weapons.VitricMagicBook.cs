using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Dusts;
using StarlightRiver.Core;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Items.Vitric
{
	public class VitricMagicBook : ModItem
    {
        public override string Texture => AssetDirectory.VitricItem + Name;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Vitric Book");
            Tooltip.SetDefault("Summons stabbing spikes on nearby ground");
        }

        public override void SetDefaults()
        {
            Item.width = 38;
            Item.height = 34;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.staff[Item.type] = true;
            Item.useAnimation = 10;
            Item.useTime = 10;
            Item.reuseDelay = 20;
            Item.autoReuse = true;
            Item.shootSpeed = 12f;
            Item.knockBack = 0f;
            Item.damage = 15;
            Item.shoot = ProjectileType<VitricBookProjectile>();
            Item.rare = ItemRarityID.Green;
            Item.noMelee = true;
            Item.DamageType = DamageClass.Magic;
            Item.mana = 15;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            for (int i = 0; i < 181; i += 180)
            {
                Vector2 muzzleOffset = MathHelper.ToRadians(i).ToRotationVector2() * 35f;

                if (Collision.CanHit(position, 0, 0, position + muzzleOffset, 0, 0))
                    position += muzzleOffset;

                Terraria.Audio.SoundEngine.PlaySound(SoundID.Item, (int)position.X, (int)position.Y, 8, 1);
                muzzleOffset.Normalize();
                Projectile.NewProjectile(source, position.X, position.Y, muzzleOffset.X * 16f, 0, Item.shoot, damage, knockback, player.whoAmI);
            }

            return false;
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemType<Misc.Sandscript>());
            recipe.AddIngredient(ItemType<VitricOre>(), 10);
            recipe.AddTile(TileID.Bookcases);
        }
    }

    internal class VitricBookSpikeTrap : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 32;
            Projectile.height = 52;
            Projectile.friendly = true;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 500;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = -1;
            Projectile.localNPCHitCooldown = 1;
            Projectile.usesLocalNPCImmunity = true;

        }
        public override string Texture => "StarlightRiver/Assets/Bosses/VitricBoss/BossSpike";

        public override void SetStaticDefaults() => DisplayName.SetDefault("Vitric Spike Trap");

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = TextureAssets.Projectile[Projectile.type].Value;
            Rectangle rect = new Rectangle(0, 0, tex.Width, (int)Projectile.localAI[0]);
            var pos = (Projectile.Center + new Vector2(0, 12 + (tex.Height / 2) - Projectile.localAI[0])) - Main.screenPosition;

            Main.spriteBatch.Draw(tex, pos, rect, lightColor, Projectile.rotation, new Vector2(tex.Width / 2f, tex.Height / 2), Projectile.scale, SpriteEffects.None, 0f);
            return false;
        }

		public override bool? CanHitNPC(NPC target)
		{
			return Projectile.ai[0] == 0;
        }

        public override void AI()
        {
            Projectile.ai[0] -= 1;
            Projectile.localAI[1] -= 1;

            if (Projectile.localAI[1] > 0)
                Projectile.localAI[0] += 8;

            if (Projectile.localAI[0] > 10)
                Projectile.timeLeft = Math.Max(Projectile.timeLeft, 1);

            Projectile.localAI[0] = MathHelper.Clamp(Projectile.localAI[0] - 4, 10, 36);

            if (Projectile.ai[0] > 10)
                SpikeUp();
            if (Projectile.ai[0] < -40)
            {
                for (int zz = 0; zz < Main.maxNPCs; zz += 1)
                {
                    NPC NPC = Main.npc[zz];
                    if (!NPC.dontTakeDamage && !NPC.townNPC && NPC.active && NPC.life > 0)
                    {
                        Rectangle rech = new Rectangle((int)NPC.position.X, (int)NPC.position.Y, NPC.width, NPC.height);
                        Rectangle rech2 = new Rectangle((int)Projectile.position.X, (int)Projectile.position.Y, Projectile.width, Projectile.height);
                        if (rech.Intersects(rech2))
                        {
                            SpikeUp();
                            break;
                        }
                    }
                }
            }
        }

        public override void Kill(int timeLeft)
        {
            for (int i = -3; i < 7; i += 1)
                Dust.NewDust(new Vector2(Projectile.Center.X, Projectile.Center.Y + i), Projectile.width, 0, DustType<GlassGravity>(), 0f, 0f, 75, default(Color), 0.85f);
        }

        private void SpikeUp()
        {
            Terraria.Audio.SoundEngine.PlaySound(SoundID.Item, (int)Projectile.Center.X, (int)Projectile.Center.Y, 24, 0.75f, 0.5f);
            Projectile.ai[0] = 10;
            Projectile.localAI[1] = 20;
            Projectile.netUpdate = true;
        }
    }

    internal class VitricBookProjectile : ModProjectile
    {
        public override string Texture => "StarlightRiver/Assets/Bosses/VitricBoss/BossSpike";

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 10;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Vitric Book 1");
        }

        public override bool PreDraw(ref Color lightColor) => false;

        public override void AI()
        {
            for (float num315 = 0.2f; num315 < 6; num315 += 0.25f)
            {
                float angle = Projectile.velocity.ToRotation() + MathHelper.ToRadians(Main.rand.Next(40, 140));
                Vector2 vecangle = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * num315;
                int num316 = Dust.NewDust(new Vector2(Projectile.Center.X, Projectile.Center.Y + Main.rand.Next(-30, 90)), Projectile.width, Projectile.height, DustType<Dusts.GlassGravity>(), 0f, 0f, 50, default, ((10f - num315) / 5f) * Projectile.timeLeft / 20f);
                Main.dust[num316].noGravity = true;
                Main.dust[num316].velocity = vecangle;
                Main.dust[num316].fadeIn = 0.5f;
            }

            if (Projectile.ai[0] % 2 == 0)
                Projectile.NewProjectile(Projectile.GetProjectileSource_FromThis(), Projectile.Center.X, Projectile.Center.Y, 0, 2, ProjectileType<VitricBookProjectiletileCheck>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
            Projectile.ai[0] += 1;
        }
    }

    internal class VitricBookProjectiletileCheck : ModProjectile
    {
        public override string Texture => "StarlightRiver/Assets/Bosses/VitricBoss/BossSpike";

        public override void SetDefaults()
        {
            Projectile.width = 2;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 40;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 40;
        }

        public override void SetStaticDefaults() => DisplayName.SetDefault("Vitric Book 2");

        public override bool PreDraw(ref Color lightColor) => false;

		public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac)
		{
            fallThrough = false;
            return true;
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (!Collision.CanHit(Projectile.Center, 4, 4, Projectile.Center - new Vector2(0, 24), 0, 0))
                return true;

            for (int zz = 0; zz < Main.maxProjectiles; zz += 1)
            {
                Projectile proj = Main.projectile[zz];
                if (proj.active && proj.type == ProjectileType<VitricBookSpikeTrap>())
                {
                    if (proj.Distance(Projectile.Center) < 24)
                        return true;
                }
            }
            Point16 point = new Point16((int)(Projectile.Center.X / 16), Math.Min(Main.maxTilesY, (int)(Projectile.Center.Y / 16) + 1));
            Tile tile = Framing.GetTileSafely(point.X, point.Y);
            int dusttype = DustType<GlassGravity>();

            //hard coded dust ids in worldgen.cs, ew
            if (tile != null && WorldGen.InWorld(point.X, point.Y, 1))
            {
                if (tile.HasTile)
                    Helpers.DustHelper.TileDust(tile, ref dusttype);
            }

            for (float num315 = 0.2f; num315 < 8; num315 += 0.50f)
            {
                float angle = MathHelper.ToRadians(-Main.rand.Next(60, 120));
                Vector2 vecangle = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * num315;
                int num316 = Dust.NewDust(new Vector2(Projectile.Center.X, Projectile.Center.Y), Projectile.width, Projectile.height, dusttype, 0f, 0f, 50, default, (10f - num315) / 5f);
                Main.dust[num316].noGravity = true;
                Main.dust[num316].velocity = vecangle;
                Main.dust[num316].fadeIn = 0.5f;
            }

            Projectile.NewProjectile(Projectile.GetProjectileSource_FromThis(), Projectile.Center.X, Projectile.Center.Y, 0, 0, ProjectileType<VitricBookSpikeTrap>(), Projectile.damage, 0, Projectile.owner, 12);

            return false;
        }
    }
}