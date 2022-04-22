using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using StarlightRiver.Content.Dusts;
using StarlightRiver.Content.Buffs;
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
			Item.maxStack = 9999;
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
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Bizarre Potion");
		}

		public override void SetDefaults()
		{
			Projectile.width = 16;
			Projectile.height = 14;

			Projectile.aiStyle = -1;

			Projectile.friendly = true;
			Projectile.hostile = false;
			Projectile.DamageType = DamageClass.Generic;
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

			Lighting.AddLight(Projectile.Center, GetColor().ToVector3());

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
				if (oldVelocity.Y != Projectile.velocity.Y)
					return false;
            }
			return true;
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