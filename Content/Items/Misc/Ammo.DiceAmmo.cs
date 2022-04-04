
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Buffs;
using StarlightRiver.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using static Terraria.ModLoader.ModContent;
using Terraria.GameContent;

namespace StarlightRiver.Content.Items.Misc
{
	public class DiceAmmo : ModItem
	{
		public override string Texture => AssetDirectory.MiscItem + Name;
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Dice");
			Tooltip.SetDefault("Every shot randomizes");
		}

		public override void SetDefaults()
		{
			Item.width = 8;
			Item.height = 16;
			Item.value = 1000;
			Item.rare = ItemRarityID.Blue;
			Item.value = Item.buyPrice(0, 0, 0, 40);

			Item.maxStack = 999;

            Item.damage = 15;
			Item.knockBack = 1.5f;
			Item.ammo = AmmoID.Bullet;

			Item.DamageType = DamageClass.Ranged;
			Item.consumable = true;

			Item.shoot = ModContent.ProjectileType<DiceProj>();
			Item.shootSpeed = 3f;

		}
	}
    public class DiceProj : ModProjectile
    {

        public override string Texture => AssetDirectory.MiscItem + Name;

        private bool initialized = false;

        private float gravity = 0.05f;
        public override void SetStaticDefaults() => DisplayName.SetDefault("Dice");
        const int frameCount = 6;
        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Texture2D tex = TextureAssets.Projectile[Projectile.type].Value;
            int width = tex.Width / frameCount;
            Rectangle sourceRect = new Rectangle(Projectile.frame * width, 0, width, tex.Height);
            spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, sourceRect, lightColor, Projectile.rotation, new Vector2(width, tex.Height) / 2, 1f, default, default);
            return false;
        }
        public override void SetDefaults()
        {
            Projectile.CloneDefaults(ProjectileID.WoodenArrowFriendly);
            Projectile.width = 8;
            Projectile.height = 8;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 600;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 5;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
            Projectile.extraUpdates = 1;
            Projectile.frame = Main.rand.Next(frameCount);
        }
        public override void AI()
        {
            Player Player = Main.player[Projectile.owner];
            DicePlayer mPlayer = Player.GetModPlayer<DicePlayer>();
            if (!initialized)
            {
                initialized = true;

                mPlayer.damageMult *= Main.rand.NextFloat(0.1f, 2f); //correct these later
                mPlayer.gravityMult *= Main.rand.NextFloat(0.5f, 1.5f);
                mPlayer.velocityMult *= Main.rand.NextFloat(0.75f, 2);
                mPlayer.knockbackMult *= Main.rand.NextFloat(0.5f, 5);

                mPlayer.damageMult /= (float)Math.Sqrt(mPlayer.damageMult);
                mPlayer.gravityMult /= (float)Math.Sqrt(mPlayer.gravityMult);
                mPlayer.velocityMult /= (float)Math.Sqrt(mPlayer.velocityMult);
                mPlayer.knockbackMult /= (float)Math.Sqrt(mPlayer.knockbackMult);

                Projectile.damage = (int)(Projectile.damage * mPlayer.damageMult);
                gravity *= mPlayer.gravityMult;
                Projectile.velocity *= mPlayer.velocityMult;
                Projectile.knockBack *= mPlayer.knockbackMult;
            }
            Projectile.velocity.X *= 0.995f;
            Projectile.velocity.Y += gravity;
        }
    }
    public class DicePlayer : ModPlayer
    {
        public float damageMult = 1f;
        public float gravityMult = 1f;
        public float velocityMult = 1f;
        public float knockbackMult = 1f;
    }
}