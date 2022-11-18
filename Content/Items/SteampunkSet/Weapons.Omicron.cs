//TODO:
//Sellprice
//Rarity
//Recipe
//Sprites
//Sound effects
//Visuals
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Dusts;
using StarlightRiver.Core;
using StarlightRiver.Helpers;
using System;
using System.Linq;
using System.Collections.Generic;
using Terraria.Graphics.Effects;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Terraria.GameContent;

namespace StarlightRiver.Content.Items.SteampunkSet
{
	public class Omicron : ModItem
	{
		public override string Texture => AssetDirectory.SteampunkItem + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Omicron");
			Tooltip.SetDefault("update later");
		}
		
		public override void SetDefaults()
		{
			Item.damage = 10;
			Item.DamageType = DamageClass.Magic;
			Item.mana = 10;
			Item.width = 24;
			Item.height = 24;
			Item.useTime = 25;
			Item.useAnimation = 25;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.noMelee = true;
			Item.knockBack = 1;
			Item.value = Item.sellPrice(0, 1, 0, 0);
			Item.rare = ItemRarityID.Green;
			Item.shoot = ModContent.ProjectileType<OmicronProj>();
			Item.shootSpeed = 15f;
			Item.autoReuse = true;
			Item.noUseGraphic = true;
		}
	}
	public class OmicronProj : ModProjectile
	{
		public override string Texture => AssetDirectory.SteampunkItem + Name;

		public readonly int MAXSTACKS = 3;

		public int stacks = 0;

		public bool accelerating = false;

		public int hitCooldown = 0;

		public List<Projectile> alreadyHit = new List<Projectile>();

		public Projectile lastHit = default;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Omicron");
		}

		public override void SetDefaults()
		{
			Projectile.penetrate = 1;
			Projectile.tileCollide = true;
			Projectile.timeLeft = 800;
			Projectile.hostile = false;
			Projectile.friendly = true;
			Projectile.width = Projectile.height = 16;
		}

		public override void AI()
		{
			if (!accelerating)
				Projectile.velocity *= 0.92f;
			else if (Projectile.velocity.Length() < 15 + (2 * stacks))
				Projectile.velocity *= 1.05f + (0.02f * stacks);
			else
				accelerating = false;

			if (hitCooldown-- > 0)
				return;

			Projectile pusher = Main.projectile.Where(
				n => n.active && 
				n.type == Projectile.type &&
				n.Hitbox.Intersects(Projectile.Hitbox) &&
				n.velocity.Length() > 1 && n != Projectile && 
				(n.ModProjectile as OmicronProj).hitCooldown < 0 && 
				lastHit != n &&
				(n.ModProjectile as OmicronProj).lastHit != Projectile &&
				Math.Abs(Helper.RotationDifference(n.velocity.ToRotation(), n.DirectionTo(Projectile.Center).ToRotation())) < 2
				).OrderBy(n => n.Distance(Projectile.Center)).FirstOrDefault();
			if (pusher != default)
			{
				lastHit = pusher;

				alreadyHit.Add(pusher);
				(pusher.ModProjectile as OmicronProj).alreadyHit.Add(Projectile);
				//(pusher.ModProjectile as OmicronProj).accelerating = false;
				(pusher.ModProjectile as OmicronProj).lastHit = Projectile;
				accelerating = true;
				Projectile.velocity += pusher.velocity;
				pusher.velocity *= 0.1f;
				if (stacks < MAXSTACKS)
				{
					Projectile.damage += 13;
					stacks++;
				}
			}	
		}

		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
		{

		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			lastHit = default;
			if (Math.Abs(Projectile.velocity.X) < 0.5f)
				Projectile.velocity.X = -Projectile.oldVelocity.X;
			if (Math.Abs(Projectile.velocity.Y) < 0.5f)
				Projectile.velocity.Y = -Projectile.oldVelocity.Y;
			return false;
		}
	}
}