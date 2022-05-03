using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Dusts;
using StarlightRiver.Core;
using StarlightRiver.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Graphics.Effects;

namespace StarlightRiver.Content.Items.Vitric
{
	public class IgnitionGauntlets : ModItem
	{
		public override string Texture => AssetDirectory.VitricItem + Name;

		public int handCounter = 0;
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Ignition Gauntlets");
			Tooltip.SetDefault("I will update this later");
		}

		public override void SetDefaults()
		{
			Item.damage = 32;
			Item.DamageType = DamageClass.Melee;
			Item.width = 5;
			Item.height = 5;
			Item.useTime = 3;
			Item.useAnimation = 3;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.noMelee = true;
			Item.knockBack = 0;
			Item.rare = ItemRarityID.Orange;
			Item.shoot = ModContent.ProjectileType<IgnitionPunchPhantom>();
			Item.shootSpeed = 12f;
			Item.autoReuse = true;
			Item.noUseGraphic = true;
		}

		public override bool AltFunctionUse(Player player) => true;

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
			if (player.altFunctionUse == 2)
            {
				if (player.GetModPlayer<IgnitionPlayer>().charge > 0)
				{
					if (!player.GetModPlayer<IgnitionPlayer>().launching)
					{
						Projectile.NewProjectile(source, position, Vector2.Zero, ModContent.ProjectileType<IgnitionGauntletLaunch>(), damage, knockback, player.whoAmI);
						player.velocity = player.DirectionTo(Main.MouseWorld) * 20;
					}
					player.GetModPlayer<IgnitionPlayer>().launching = true;
				}
				return false;
            }
			/*if (player.velocity.Length() < 6 && !(player.controlUp || player.controlDown || player.controlLeft || player.controlRight))
            {
				Vector2 dir = player.DirectionTo(Main.MouseWorld) * 0.6f;
				player.velocity.X += dir.X;
            }*/
			handCounter++;
			if (handCounter % 2 == 0)
			{
				Projectile proj = Projectile.NewProjectileDirect(source, position, Vector2.Zero, type, damage, knockback, player.whoAmI, (handCounter % 4) / 2);
				var mp = proj.ModProjectile as IgnitionPunchPhantom;
				mp.directionVector = player.DirectionTo(Main.MouseWorld).RotatedByRandom(0.2f);
			

				int offset = Main.rand.Next(-20, 20);
				Vector2 vel = new Vector2(7, offset * 0.4f);
				Vector2 offsetV = new Vector2(Main.rand.Next(-20,20), offset);
				float rot = position.DirectionTo(Main.MouseWorld).ToRotation();
				Projectile.NewProjectileDirect(source, position + offsetV.RotatedBy(rot), vel.RotatedBy(rot), ModContent.ProjectileType<IgnitionPunch>(), damage, knockback, player.whoAmI, (handCounter % 4) / 2);
			}
			return false;
        }
    }

	public class IgnitionPunchPhantom : ModProjectile
	{

		public override string Texture => AssetDirectory.Assets + "Invisible";

		public Vector2 directionVector = Vector2.Zero;
		private Player owner => Main.player[Projectile.owner];

		private bool front => Projectile.ai[0] == 0;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Ignition Gauntlets");
		}

		public override void SetDefaults()
		{
			Projectile.penetrate = 1;
			Projectile.tileCollide = false;
			Projectile.hostile = false;
			Projectile.friendly = false;
			Projectile.timeLeft = 13;
			Projectile.width = Projectile.height = 20;
			ProjectileID.Sets.TrailCacheLength[Projectile.type] = 9;
			ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
			Projectile.hide = true;
		}

        public override void AI()
        {
			Projectile.velocity = Vector2.Zero;
			Vector2 direction = Projectile.DirectionTo(Main.MouseWorld);
			Projectile.Center = owner.Center + (direction * 20);

			Player.CompositeArmStretchAmount stretch = Player.CompositeArmStretchAmount.Full;
			float extend = (float)Math.Sin(Projectile.timeLeft / 2f);
			if (extend < 0.25f)
				stretch = Player.CompositeArmStretchAmount.None;
			else if (extend < 0.5f)
				stretch = Player.CompositeArmStretchAmount.Quarter;
			else if (extend < 0.8f)
				stretch = Player.CompositeArmStretchAmount.ThreeQuarters;
			else
				stretch = Player.CompositeArmStretchAmount.Full;
			if (front)
				owner.SetCompositeArmFront(true, stretch, directionVector.ToRotation() - 1.57f); 
			else
				owner.SetCompositeArmBack(true, stretch, directionVector.ToRotation() - 1.57f);

		}

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
			//Projectile.penetrate++;
			//Projectile.friendly = false;
			//owner.velocity = -owner.DirectionTo(Projectile.Center) * 6;
        }
    }
	public class IgnitionPunch : ModProjectile
	{

		public override string Texture => AssetDirectory.VitricItem + Name;

		private Player owner => Main.player[Projectile.owner];

		private bool front => Projectile.ai[0] == 0;

		private List<float> oldRotation = new List<float>();

		private bool initialized = false;

		private Vector2 posToBe = Vector2.Zero;

		private float fade => Math.Min(1, Projectile.timeLeft / 7f);

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Ignition Gauntlets");
			ProjectileID.Sets.TrailCacheLength[Projectile.type] = 3;
			ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
		}

		public override void SetDefaults()
		{
			Projectile.penetrate = 1;
			Projectile.tileCollide = false;
			Projectile.hostile = false;
			Projectile.friendly = true;
			Projectile.timeLeft = 20;
			Projectile.extraUpdates = 1;
			Projectile.width = Projectile.height = 18;
			ProjectileID.Sets.TrailCacheLength[Projectile.type] = 9;
			ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
		}
		public override void AI()
		{
			if (!initialized)
			{
				initialized = true;
				Vector2 direction = owner.DirectionTo(Main.MouseWorld);
				posToBe = owner.Center + (direction * 200);
			}
			Projectile.velocity = Vector2.Lerp(Projectile.velocity, Projectile.DirectionTo(posToBe) * 7, 0.25f);
			Projectile.rotation = Projectile.velocity.ToRotation();

			oldRotation.Add(Projectile.rotation);
			while (oldRotation.Count > Projectile.oldPos.Length)
			{
				oldRotation.RemoveAt(0);
			}
			if (Projectile.timeLeft < 7)
			{
				Projectile.friendly = false;
			}
			else
				Lighting.AddLight(Projectile.Center, Color.OrangeRed.ToVector3() * 0.4f);
		}

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
			int distance = (int)(owner.Center - Projectile.Center).Length();
			float pushback = (float)Math.Sqrt(200 * EaseFunction.EaseCubicIn.Ease((200 - distance) / 200f));
			Vector2 direction = target.DirectionTo(owner.Center);
			owner.velocity += direction * pushback * 0.15f;

			Projectile proj = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<IgnitionGauntletsImpactRing>(), 0, 0, owner.whoAmI, Main.rand.Next(15,25), Projectile.velocity.ToRotation());
			for (int i = 0; i < 7; i++)
            {
				Dust.NewDustPerfect(Projectile.Center, 6, -Projectile.velocity.RotatedByRandom(0.4f) * Main.rand.NextFloat(), 0, default, 1.25f).noGravity = true;
            }

			if (owner.GetModPlayer<IgnitionPlayer>().charge < 75)
				owner.GetModPlayer<IgnitionPlayer>().charge += 1;

			Main.NewText(owner.GetModPlayer<IgnitionPlayer>().charge);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = ModContent.Request<Texture2D>(Texture + (front ? "" : "_Back")).Value;
			Texture2D afterTex = ModContent.Request<Texture2D>(Texture + "_After").Value;

			/*Main.spriteBatch.End();
			Main.spriteBatch.Begin(default, BlendState.Additive, default, default, default, default, Main.GameViewMatrix.TransformationMatrix);*/

			for (int k = Projectile.oldPos.Length - 1; k > 0; k--) 
			{
				Vector2 drawPos = Projectile.oldPos[k] + (new Vector2(Projectile.width, Projectile.height) / 2);

				float progress = (float)(((float)(Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length));
				Color color = Color.White * progress * fade * 0.8f;
				if (k > 0 && k < oldRotation.Count)
					Main.spriteBatch.Draw(tex, drawPos - Main.screenPosition, null, color, oldRotation[k], tex.Size() / 2, Projectile.scale * 0.8f * progress, SpriteEffects.None, 0f);
			}

			/*Main.spriteBatch.End();
			Main.spriteBatch.Begin(default, BlendState.AlphaBlend, default, default, default, default, Main.GameViewMatrix.TransformationMatrix);*/

			Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, Color.White * (float)Math.Sqrt(fade), Projectile.rotation, tex.Size() / 2, Projectile.scale, SpriteEffects.None, 0f);
			return false;
		}
	}
	internal class IgnitionGauntletsImpactRing : ModProjectile, IDrawPrimitive
	{
		public override string Texture => AssetDirectory.Assets + "Invisible";

		private List<Vector2> cache;

		private Trail trail;
		private Trail trail2;

		private float Progress => 1 - (Projectile.timeLeft / 10f);

		private float Radius => Projectile.ai[0] * (float)Math.Sqrt(Math.Sqrt(Progress));

		public override void SetDefaults()
		{
			Projectile.width = 80;
			Projectile.height = 80;
			Projectile.friendly = false;
			Projectile.tileCollide = false;
			Projectile.penetrate = -1;
			Projectile.timeLeft = 10;
		}

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Ignition Gauntlets");
		}

		public override void AI()
		{
			if (Main.netMode != NetmodeID.Server)
			{
				ManageCaches();
				ManageTrail();
			}
		}

		public override bool PreDraw(ref Color lightColor) => false;

		private void ManageCaches()
		{
			cache = new List<Vector2>();
			float radius = Radius;
			for (int i = 0; i < 33; i++) //TODO: Cache offsets, to improve performance
			{
				double rad = (i / 32f) * 6.28f;
				Vector2 offset = new Vector2((float)Math.Sin(rad) * 0.4f, (float)Math.Cos(rad));
				offset *= radius;
				offset = offset.RotatedBy(Projectile.ai[1]);
				cache.Add(Projectile.Center + offset);
			}

			while (cache.Count > 33)
			{
				cache.RemoveAt(0);
			}
		}

		private void ManageTrail()
		{

			trail = trail ?? new Trail(Main.instance.GraphicsDevice, 33, new TriangularTip(1), factor => 28 * (1 - Progress), factor =>
			{
				return Color.Orange;
			});

			trail2 = trail2 ?? new Trail(Main.instance.GraphicsDevice, 33, new TriangularTip(1), factor => 10 * (1 - Progress), factor =>
			{
				return Color.White;
			});
			float nextplace = 33f / 32f;
			Vector2 offset = new Vector2((float)Math.Sin(nextplace), (float)Math.Cos(nextplace));
			offset *= Radius;

			trail.Positions = cache.ToArray();
			trail.NextPosition = Projectile.Center + offset;

			trail2.Positions = cache.ToArray();
			trail2.NextPosition = Projectile.Center + offset;
		}

		public void DrawPrimitives()
		{
			Effect effect = Filters.Scene["OrbitalStrikeTrail"].GetShader().Shader;

			Matrix world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
			Matrix view = Main.GameViewMatrix.ZoomMatrix;
			Matrix projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

			effect.Parameters["transformMatrix"].SetValue(world * view * projection);
			effect.Parameters["sampleTexture"].SetValue(ModContent.Request<Texture2D>("StarlightRiver/Assets/GlowTrail").Value);
			effect.Parameters["alpha"].SetValue(1);

			trail?.Render(effect);
			trail2?.Render(effect);
		}
	}

	public class IgnitionPlayer : ModPlayer
    {
		public int charge = 0;
		public bool launching = false;

		private int rotationCounter = 0;

        public override void PreUpdate() //TODO: some rotdifference shenanagins here to make the rotation transition smoother
        {
            if (launching)
            {
				charge--;
				if (charge <= 0)
				{
					launching = false;
					Player.fullRotation = 0;
					return;
				}

				Player.fullRotationOrigin = Player.Size / 2;

				Player.fullRotation = Player.DirectionTo(Main.MouseWorld).ToRotation() + 1.57f;

				float lerper = MathHelper.Min(rotationCounter / 8f, charge / 15f);
				Player.fullRotation *= lerper;
				Player.velocity = Vector2.Lerp(Player.velocity, Player.DirectionTo(Main.MouseWorld) * 20, 0.1f);

				if (rotationCounter < 8)
					rotationCounter++;

			}
			else
            {
				rotationCounter = 0;
            }
        }
        public override bool PreHurt(bool pvp, bool quiet, ref int damage, ref int hitDirection, ref bool crit, ref bool customDamage, ref bool playSound, ref bool genGore, ref PlayerDeathReason damageSource)
        {
			if (launching)
            {
				return false;
            }
            return base.PreHurt(pvp, quiet, ref damage, ref hitDirection, ref crit, ref customDamage, ref playSound, ref genGore, ref damageSource);
        }
    }

	public class IgnitionGauntletLaunch : ModProjectile
    {
		public override string Texture => AssetDirectory.Assets + "Invisible";
		private Player owner => Main.player[Projectile.owner];

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Ignition Gauntlets");
		}

		public override void SetDefaults()
		{
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.hostile = false;
			Projectile.friendly = true;
			Projectile.timeLeft = 9999;
			Projectile.width = Projectile.height = 50;
			ProjectileID.Sets.TrailCacheLength[Projectile.type] = 9;
			ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
			Projectile.hide = true;
		}

        public override void AI()
        {
			IgnitionPlayer modPlayer = owner.GetModPlayer<IgnitionPlayer>();
			Projectile.Center = owner.Center;
			if (!modPlayer.launching)
			{ 
				Projectile.Kill();
			}
        }
    }
}