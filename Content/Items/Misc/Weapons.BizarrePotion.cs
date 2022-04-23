using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using StarlightRiver.Content.Dusts;
using StarlightRiver.Content.Buffs;
using StarlightRiver.Content.Items.SteampunkSet;
using StarlightRiver.Helpers;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System;
using System.Linq;
using System.Collections.Generic;
using Terraria.Graphics.Effects;
using Terraria.DataStructures;
using Terraria.GameContent;

namespace StarlightRiver.Content.Items.Misc
{
	public class BizarrePotion : ModItem
	{
		public override string Texture => AssetDirectory.MiscItem + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Bizarre Potion");
			Tooltip.SetDefault("Throws a random potion");

		}

		public override void SetDefaults()
		{
			Item.damage = 20;
			Item.DamageType = DamageClass.Generic;
			Item.width = 24;
			Item.height = 24;
			Item.useTime = 25;
			Item.useAnimation = 25;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.noMelee = true;
			Item.knockBack = 0;
			Item.rare = ItemRarityID.Blue;
			Item.shoot = ModContent.ProjectileType<BizarrePotionProj>();
			Item.shootSpeed = 12f;
			Item.autoReuse = true;
			Item.noUseGraphic = true;
			Item.consumable = true;
			Item.maxStack = 999;
		}
    }

	internal enum BottleType
    { 
		Regular = 0,
		NoGravity = 1,
		Float = 2,
		Slide = 3
	}

	internal enum LiquidType
    {
		Fire = 0,
		Ice = 1,
		Lightning = 2,
		Poison = 3
    }
	public class BizarrePotionProj : ModProjectile
	{
		public override string Texture => AssetDirectory.MiscItem + Name;

		private bool initialized = false;

		private LiquidType liquidType;

		private BottleType bottleType;

		private Player owner => Main.player[Projectile.owner];
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Bizarre Potion");
		}

		public override void SetDefaults()
		{
			Projectile.width = 18;
			Projectile.height = 24;

			Projectile.aiStyle = -1;

			Projectile.friendly = true;
			Projectile.hostile = false;
			Projectile.DamageType = DamageClass.Generic;
		}

        public override bool PreDraw(ref Color lightColor)
        {
			Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
			int xFrameSize = texture.Width / 4;
			int yFrameSize = texture.Height / 4;

			int xFrame = (int)bottleType;
			int yFrame = (int)liquidType;
			Rectangle frame = new Rectangle(xFrame * xFrameSize, yFrame * yFrameSize, xFrameSize, yFrameSize);

			Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, frame, lightColor, Projectile.rotation, new Vector2(xFrameSize, yFrameSize) / 2, Projectile.scale, SpriteEffects.None, 0f);

            Texture2D glowTexture = ModContent.Request<Texture2D>(Texture + "_Glow").Value;
			Main.spriteBatch.Draw(glowTexture, Projectile.Center - Main.screenPosition, frame, Color.White, Projectile.rotation, new Vector2(xFrameSize, yFrameSize) / 2, Projectile.scale, SpriteEffects.None, 0f);
			return false;
		}

        public override void AI()
		{
			if (!initialized)
			{
				initialized = true;
				liquidType = (LiquidType)Main.rand.Next(4);
				bottleType = (BottleType)Main.rand.Next(4);

				switch (bottleType)
				{
					case BottleType.Float:
						Projectile.velocity.Y += 5;
						break;
					case BottleType.Slide:
						Projectile.velocity /= 1.5f;
						break;
				}
			}

			Lighting.AddLight(Projectile.Center, GetColor().ToVector3() * 0.5f);

			switch (bottleType)
			{
				case BottleType.Regular:
					Projectile.aiStyle = 2;
					break;
				case BottleType.Float:
					Projectile.rotation = 0f;
					Projectile.velocity.Y -= 0.4f;
					break;
				case BottleType.Slide:
					Projectile.rotation = 0f;
					Projectile.velocity.Y += 0.3f;
					break;
				case BottleType.NoGravity:
					Projectile.rotation = Projectile.velocity.ToRotation() + 1.57f;
					break;
			}
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			if (bottleType == BottleType.Slide)
            {
				if (oldVelocity.Y != Projectile.velocity.Y && oldVelocity.X == Projectile.velocity.X)
					return false;
            }
			return true;
		}

        public override void Kill(int timeLeft)
        {
			Terraria.Audio.SoundEngine.PlaySound(SoundID.Item, (int)Projectile.position.X, (int)Projectile.position.Y, 107);
			switch (liquidType)
            {
                case LiquidType.Fire:
					Explode();
					break;
            }
        }

		private void Explode()
        {
			owner.GetModPlayer<StarlightPlayer>().Shake += 8;

			Terraria.Audio.SoundEngine.PlaySound(SoundLoader.GetLegacySoundSlot(Mod, "Sounds/Magic/FireHit"), Projectile.Center);
			Helper.PlayPitched("Impacts/AirstrikeImpact", 0.4f, Main.rand.NextFloat(-0.1f, 0.1f));

			for (int i = 0; i < 4; i++)
			{
				Dust dust = Dust.NewDustDirect(Projectile.Center - new Vector2(16, 16), 0, 0, ModContent.DustType<JetwelderDust>());
				dust.velocity = Main.rand.NextVector2Circular(2, 2);
				dust.scale = Main.rand.NextFloat(1f, 1.5f);
				dust.alpha = Main.rand.Next(80) + 40;
				dust.rotation = Main.rand.NextFloat(6.28f);

				Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(25, 25), ModContent.DustType<CoachGunDustFour>()).scale = 0.9f;
			}

			for (int i = 0; i < 3; i++)
			{
				var velocity = Main.rand.NextFloat(6.28f).ToRotationVector2() * Main.rand.NextFloat(1, 2);
				Projectile.NewProjectileDirect(Projectile.GetProjectileSource_FromThis(), Projectile.Center, velocity, ModContent.ProjectileType<CoachGunEmber>(), 0, 0, owner.whoAmI).scale = Main.rand.NextFloat(0.85f, 1.15f);
			}

			Projectile.NewProjectileDirect(Projectile.GetProjectileSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<JetwelderJumperExplosion>(), Projectile.damage, 0, owner.whoAmI, -1);
			for (int i = 0; i < 2; i++)
			{
				Vector2 vel = Main.rand.NextFloat(6.28f).ToRotationVector2();
				Dust dust = Dust.NewDustDirect(Projectile.Center - new Vector2(16, 16) + (vel * Main.rand.Next(20)), 0, 0, ModContent.DustType<JetwelderDustTwo>());
				dust.velocity = vel * Main.rand.Next(2);
				dust.scale = Main.rand.NextFloat(0.3f, 0.7f);
				dust.alpha = 70 + Main.rand.Next(60);
				dust.rotation = Main.rand.NextFloat(6.28f);
			}
		}

        private Color GetColor()
        {
			switch (liquidType)
            {
				case LiquidType.Fire:
					return Color.Orange;
				case LiquidType.Ice:
					return Color.Cyan;
				case LiquidType.Lightning:
					return Color.Yellow;
				case LiquidType.Poison:
					return Color.Purple;
            }
			return Color.White;
        }
	}
}