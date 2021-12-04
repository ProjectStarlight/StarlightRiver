
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
			item.width = 8;
			item.height = 16;
			item.value = 1000;
			item.rare = ItemRarityID.Blue;
			item.value = Item.buyPrice(0, 0, 0, 40);

			item.maxStack = 999;

            item.damage = 15;
			item.knockBack = 1.5f;
			item.ammo = AmmoID.Bullet;

			item.ranged = true;
			item.consumable = true;

			item.shoot = ModContent.ProjectileType<DiceProj>();
			item.shootSpeed = 3f;

		}
	}
    public class DiceProj : ModProjectile
    {

        public override string Texture => AssetDirectory.MiscItem + Name;

        private bool initialized = false;

        private float gravity = 0.05f;
        public override void SetStaticDefaults() => DisplayName.SetDefault("Dice");

        public override void SetDefaults()
        {
            projectile.CloneDefaults(ProjectileID.WoodenArrowFriendly);
            projectile.width = 8;
            projectile.height = 8;
            projectile.friendly = true;
            projectile.ranged = true;
            projectile.penetrate = 1;
            projectile.timeLeft = 600;
            ProjectileID.Sets.TrailCacheLength[projectile.type] = 5;
            ProjectileID.Sets.TrailingMode[projectile.type] = 0;
            projectile.extraUpdates = 1;
        }
        public override void AI()
        {
            Player player = Main.player[projectile.owner];
            DicePlayer mPlayer = player.GetModPlayer<DicePlayer>();
            if (!initialized)
            {
                initialized = true;

                mPlayer.damageMult *= Main.rand.NextFloat(0.5f, 2); //correct these later
                mPlayer.gravityMult *= Main.rand.NextFloat(0.5f, 2);
                mPlayer.velocityMult *= Main.rand.NextFloat(0.5f, 2);
                mPlayer.knockbackMult *= Main.rand.NextFloat(0.5f, 2);

                mPlayer.damageMult /= (float)Math.Sqrt(mPlayer.damageMult);
                mPlayer.gravityMult /= (float)Math.Sqrt(mPlayer.gravityMult);
                mPlayer.velocityMult /= (float)Math.Sqrt(mPlayer.velocityMult);
                mPlayer.knockbackMult /= (float)Math.Sqrt(mPlayer.knockbackMult);

                projectile.damage = (int)(projectile.damage* mPlayer.damageMult);
                gravity *= mPlayer.gravityMult;
                projectile.velocity *= mPlayer.velocityMult;
                projectile.knockBack *= mPlayer.knockbackMult;
            }
            projectile.velocity.X *= 0.995f;
            projectile.velocity.Y += gravity;
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