using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Abilities;
using StarlightRiver.Core;
using StarlightRiver.Content.Items.Gravedigger;
using StarlightRiver.Helpers;

using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Terraria.GameContent;

namespace StarlightRiver.Content.Items.Moonstone
{
	public class CrescentQuarterstaff : ModItem
	{
		public override string Texture => AssetDirectory.MoonstoneItem + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Crescent Quarterstaff");
			Tooltip.SetDefault("Update this egshels");
		}

		public override void SetDefaults()
		{
			Item.damage = 32;
			Item.DamageType = DamageClass.Melee;
			Item.width = 36;
			Item.height = 44;
			Item.useTime = 12;
			Item.useAnimation = 12;
			Item.reuseDelay = 20;
			Item.channel = true;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.knockBack = 6.5f;
			Item.value = Item.sellPrice(0, 1, 0, 0);
			Item.crit = 4;
			Item.rare = 2;
			Item.shootSpeed = 14f;
			Item.autoReuse = false;
			Item.shoot = ProjectileType<CrescentQuarterstaffProj>();
			Item.noUseGraphic = true;
			Item.noMelee = true;
			Item.autoReuse = false;
		}

		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ModContent.ItemType<MoonstoneBar>(), 12);
			recipe.AddTile(TileID.Anvils);
		}
	}

	internal class CrescentQuarterstaffProj : ModProjectile
	{ 
		public override string Texture => AssetDirectory.MoonstoneItem + Name;

		enum CurrentAttack : int
		{
			Down = 0,
			FirstUp = 1,
			Spin = 2,
			SecondUp = 3,
			Slam = 4,
			Reset = 5
		}

		private float LENGTH = 100;

		private const float SWAY = 20;

		private const int MAXCHARGE = 10;


		private CurrentAttack currentAttack;

		private int pauseTime;

		private int slashWindow;

		private bool initialized = false;

		private float angularVelocity = 0;

		private int attackDuration;

		private int currentAttackDuration;

		private Vector2 direction;

		private float rotDifference;

		private float zRotation = 0;

		private bool FacingRight = false;

		private bool clicked = false;

		private int charge;

		private float Charge => (float)charge / (float)MAXCHARGE;
		private bool MouseFacingLeft => (Main.MouseWorld.X < Player.Center.X);
		private bool FirstTickOfSwing
		{
			get => Projectile.ai[0] == 0;
			set => Projectile.ai[0] = value ? 0 : 1;
		}

		Player Player => Main.player[Projectile.owner];


		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Crescent Quarterstaff");
			Main.projFrames[Projectile.type] = 1;
		}

		public override void SetDefaults()
		{
			Projectile.friendly = true;
			Projectile.DamageType = DamageClass.Melee;
			Projectile.tileCollide = false;
			Projectile.Size = new Vector2(130, 130);
			Projectile.penetrate = -1;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 16;
			Projectile.ownerHitCheck = true;
		}

        public override bool PreAI()
        {
			Player.heldProj = Projectile.whoAmI;
			Projectile.velocity = Vector2.Zero;

			Vector2 offset = (Projectile.rotation.ToRotationVector2() * LENGTH);
			Projectile.Center = Player.Center + offset;
			Lighting.AddLight(Projectile.Center, new Vector3(0.905f, 0.89f, 1) * Charge);
			Projectile.ai[1] += 0.01f;
			AdjustPlayer();

			if (pauseTime-- > 0)
				return false;
			return true;
        }

        public override void AI()
		{
			if (FirstTickOfSwing)
			{
				clicked = false;
				LENGTH = 100;
				zRotation = 0;
				float rotDifferenceDivider = 15;
				direction = Player.DirectionTo(Main.MouseWorld);
				FacingRight = direction.X < 0;
				slashWindow = 30;
				angularVelocity = 0;
				if (initialized)
					currentAttack++;
				else
				{
					Projectile.rotation = -1.57f;
					Projectile.Center = Player.Center + (Projectile.rotation.ToRotationVector2() * LENGTH);
					initialized = true;
				}
				if (currentAttack == CurrentAttack.Reset)
					Projectile.active = false;

				switch (currentAttack)
                {
					case CurrentAttack.Down:
					{
						direction = direction.RotatedBy(MouseFacingLeft ? -0.5f : 0.5f);
						currentAttackDuration = attackDuration = 30;
						rotDifferenceDivider = 35f;
						break;
					}
					case CurrentAttack.FirstUp:
					{
						direction = direction.RotatedBy(MouseFacingLeft ? 1f : -1f);
							currentAttackDuration = attackDuration = 30;
						rotDifferenceDivider = 35f;
						break;
					}
					case CurrentAttack.Spin:
					{
						rotDifferenceDivider = 25f;
						currentAttackDuration = attackDuration = 50;
						break;
					}
					case CurrentAttack.SecondUp:
					{
						rotDifferenceDivider = 35f;
						direction = direction.RotatedBy(MouseFacingLeft ? 1.8f : -1.8f);
						currentAttackDuration = attackDuration = 30;
						break;
					}
					case CurrentAttack.Slam:
					{
						rotDifferenceDivider = 25f;
						direction = direction.RotatedBy(MouseFacingLeft ? -0.8f : 0.8f);
						currentAttackDuration = attackDuration = 30;
						break;
					}
				}
				rotDifference = ((((direction.ToRotation() - Projectile.rotation) % 6.28f) + 9.42f) % 6.28f) - 3.14f;

				if (currentAttack == CurrentAttack.Slam)
					rotDifference = MouseFacingLeft ? -1 : 1;
				angularVelocity = rotDifference / rotDifferenceDivider;
				FirstTickOfSwing = false;
			}

			attackDuration--;
			if (attackDuration > 0)
			{
				float sinValue = ((float)currentAttackDuration - (float)attackDuration) / (float)currentAttackDuration;
				sinValue *= 3.14f;
				if (currentAttack != CurrentAttack.Slam)
					LENGTH = 100 + (float)(SWAY * Math.Sin(sinValue));
				switch (currentAttack)
				{
					case CurrentAttack.Down:
					{
						if (currentAttackDuration / 1.18f < attackDuration)
							angularVelocity *= 1.33f;
						else
							angularVelocity *= 0.93f;
						break;
					}
					case CurrentAttack.FirstUp:
					{
						if (currentAttackDuration / 1.18f < attackDuration)
							angularVelocity *= 1.33f;
						else
							angularVelocity *= 0.93f;
						break;
					}
					case CurrentAttack.Spin:
					{
						if (currentAttackDuration / 2 < attackDuration)
						{
							zRotation += 6.28f / (currentAttackDuration / 2);
							Player.UpdateRotation(zRotation);
						}
						else
						{
							angularVelocity *= 0.85f;
							Player.UpdateRotation(0);
						}
						break;
					}
					case CurrentAttack.SecondUp:
					{
						if (currentAttackDuration / 1.18f < attackDuration)
							angularVelocity *= 1.33f;
						else
							angularVelocity *= 0.93f;
						break;
					}
					case CurrentAttack.Slam:
					{
						if (angularVelocity < 0.6f)
							angularVelocity *= 1.13f;
						Vector2 tilePos = Player.Center + ((Projectile.rotation + (angularVelocity / 2)).ToRotationVector2() * LENGTH);
						tilePos.Y += 15;
						tilePos /= 16;

						if (angularVelocity != 0)
						{
							if (Main.tile[(int)tilePos.X, (int)tilePos.Y].collisionType == 1 && angularVelocity != 0 && Math.Sign(Projectile.rotation.ToRotationVector2().X) == Math.Sign(direction.X))
							{
									for (int i = 0; i < 13; i++)
									{
										Vector2 dustVel = Vector2.UnitY.RotatedBy(Main.rand.NextFloat(-0.9f, 0.9f)) * Main.rand.NextFloat(-2, -0.5f);
										dustVel.X *= 10;
										if (Math.Abs(dustVel.X) < 6)
											dustVel.X += Math.Sign(dustVel.X) * 6;
										Dust.NewDustPerfect((tilePos * 16) - new Vector2(Main.rand.Next(-20, 20), 17), ModContent.DustType<Dusts.CrescentSmoke>(), dustVel, 0, new Color(236, 214, 146) * 0.15f, Main.rand.NextFloat(0.5f, 1));
									}
									if (Charge > 0)
									{
										Projectile proj = Projectile.NewProjectileDirect(Projectile.Center, new Vector2(0, 7), ModContent.ProjectileType<QuarterOrb>(), (int)MathHelper.Lerp(0, Projectile.damage, Charge), 0, Projectile.owner, 0, 0);

										if (proj.ModProjectile is QuarterOrb modproj)
										{
											modproj.moveDirection = new Vector2(-Player.direction, -1);
										}
									}
								Player.GetModPlayer<StarlightPlayer>().Shake += 12;
								angularVelocity = 0;
								attackDuration = 30;
								Projectile.NewProjectile(Projectile.Center, Vector2.Zero, ModContent.ProjectileType<GravediggerSlam>(), 0, 0, Player.whoAmI);
							}

							if (LENGTH < LENGTH + SWAY)
								LENGTH++;
						}
						break;
					}
				}

			}
			else
            {
				angularVelocity *= 0.85f;
				Player.UpdateRotation(0);
				slashWindow--;

				if (Player == Main.LocalPlayer)
				{
					if (Main.mouseLeft && !clicked)
						clicked = true;

					if (clicked && slashWindow <= 25)
						FirstTickOfSwing = true;

				}

				if (slashWindow < 0)
					Projectile.active = false;
			}

			Projectile.rotation += angularVelocity;

			if (Main.rand.NextBool())
				Dust.NewDustPerfect(Projectile.Center + Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(25), DustType<Dusts.Aurora>(), Vector2.Zero, 0, new Color(20, 20, 100), 1.3f);
			else if (Math.Abs(angularVelocity) > 0.07f || (zRotation > 0 && zRotation < 6.2f))
				Dust.NewDustPerfect(Projectile.Center + Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(15), DustType<Dusts.Glow>(), Vector2.Zero, 0, new Color(50, 50, 255), 0.4f);
		}

        public override bool? CanHitNPC(NPC target)
        {
			if (attackDuration <= 0 || pauseTime > 0 || target.immune[Projectile.owner] > 0)
				return false;
            return base.CanHitNPC(target);
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
			target.immune[Projectile.owner] = 30;
			if (charge < MAXCHARGE)
				charge++;
			Player.GetModPlayer<StarlightPlayer>().Shake += 8;

			if (currentAttack != CurrentAttack.Slam)
				pauseTime = 7;
        }

        public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
        {
			if (currentAttack == CurrentAttack.Slam)
				damage = (int)(damage * 1.5f);

			if (currentAttack == CurrentAttack.Spin)
				damage = (int)(damage * 1.5f);
		}

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			// Custom collision so all chains across the flail can cause impact.
			float collisionPoint = 0f;
			if (Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Player.Center, Projectile.Center, 24 * Projectile.scale, ref collisionPoint))
			{
				return true;
			}
			return false;
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
			Texture2D head = ModContent.Request<Texture2D>(Texture + "_Head").Value;
			Texture2D tex = TextureAssets.Projectile[Projectile.type].Value;
			SpriteEffects effects = zRotation > 1.57f && zRotation < 4.71f ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

			if (effects == SpriteEffects.FlipHorizontally && Player.direction < 0)
				effects = SpriteEffects.FlipVertically;

			Vector2 origin = new Vector2(140, 10);
			origin.X -= LENGTH;
			origin.Y += LENGTH;
			if (effects == SpriteEffects.FlipHorizontally)
				origin.X = 150 - origin.X;

			if (effects == SpriteEffects.FlipVertically)
				origin.Y = 150 - origin.Y;

			Vector2 scale = new Vector2(Projectile.scale, Projectile.scale);

			if (Player.direction > 0)
				scale.X *= (float)Math.Abs(Math.Cos(zRotation));
			else
				scale.Y *= (float)Math.Abs(Math.Cos(zRotation));

			spriteBatch.End();
			spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

			Effect effect = Filters.Scene["MoonFireAura"].GetShader().Shader;
			effect.Parameters["time"].SetValue(Projectile.ai[1]);
			effect.Parameters["fireHeight"].SetValue(0.03f * Charge);
			effect.Parameters["fnoise"].SetValue(Request<Texture2D>(AssetDirectory.MoonstoneItem + "DatsuzeiFlameMap1").Value);
			effect.Parameters["fnoise2"].SetValue(Request<Texture2D>(AssetDirectory.MoonstoneItem + "DatsuzeiFlameMap2").Value);
			effect.Parameters["vnoise"].SetValue(Request<Texture2D>(AssetDirectory.MoonstoneItem + "QuarterstaffMap").Value);
			effect.CurrentTechnique.Passes[0].Apply();

			spriteBatch.Draw(head, Player.Center - Main.screenPosition, null, lightColor, Projectile.rotation + 0.78f, origin, scale, effects, 0);

			spriteBatch.End();
			spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

			effect.Parameters["time"].SetValue(Projectile.ai[1]);
			effect.Parameters["fireHeight"].SetValue(0.03f * Charge);
			effect.Parameters["fnoise"].SetValue(Request<Texture2D>(AssetDirectory.MoonstoneItem + "DatsuzeiFlameMap1").Value);
			effect.Parameters["fnoise2"].SetValue(Request<Texture2D>(AssetDirectory.MoonstoneItem + "DatsuzeiFlameMap2").Value);
			effect.Parameters["vnoise"].SetValue(Request<Texture2D>(AssetDirectory.MoonstoneItem + "QuarterstaffMap").Value);
			effect.CurrentTechnique.Passes[1].Apply();

			spriteBatch.Draw(head, Player.Center - Main.screenPosition, null, lightColor, Projectile.rotation + 0.78f, origin, scale, effects, 0);

			spriteBatch.End();
			spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.TransformationMatrix);

			spriteBatch.Draw(tex, Player.Center - Main.screenPosition, null, Color.Lerp(lightColor, Color.White, Charge), Projectile.rotation + 0.78f, origin, scale, effects, 0);
			return false;
        }

		private void AdjustPlayer()
        {
			Player.ChangeDir(FacingRight ? -1 : 1);
			Player.ItemRotation = MathHelper.WrapAngle(Projectile.rotation - ((Player.direction > 0) ? 0 : MathHelper.Pi));
			Player.ItemAnimation = Player.ItemTime = 5;
		}
    }
	public class QuarterOrb : ModProjectile, IDrawAdditive
	{
		public override string Texture => AssetDirectory.MoonstoneItem + Name;

		public override void SetStaticDefaults()
		{
			ProjectileID.Sets.TrailCacheLength[Projectile.type] = 2;
			ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
			DisplayName.SetDefault("Lunar Orb");
		}

		public Vector2 moveDirection;
		public Vector2 newVelocity = Vector2.Zero;
		public float speed = 7f;

		bool collideX = false;
		bool collideY = false;
		public override void SetDefaults()
		{
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.hostile = true;
			Projectile.friendly = false;
			Projectile.width = Projectile.height = 36;
			Projectile.timeLeft = 150;
			Projectile.ignoreWater = true;
		}
		public override void AI()
		{
			Projectile.scale = 0.5f;
			newVelocity = Collide();
			if (Math.Abs(newVelocity.X) < 0.5f)
				collideX = true;
			else
				collideX = false;
			if (Math.Abs(newVelocity.Y) < 0.5f)
				collideY = true;
			else
				collideY = false;

			if (Projectile.ai[1] == 0f)
			{
				Projectile.rotation += (float)(moveDirection.X * moveDirection.Y) * 0.43f;
				if (collideY)
				{
					Projectile.ai[0] = 2f;
				}
				if (!collideY && Projectile.ai[0] == 2f)
				{
					moveDirection.X = -moveDirection.X;
					Projectile.ai[1] = 1f;
					Projectile.ai[0] = 1f;
				}
				if (collideX)
				{
					moveDirection.Y = -moveDirection.Y;
					Projectile.ai[1] = 1f;
				}
			}
			else
			{
				Projectile.rotation -= (float)(moveDirection.X * moveDirection.Y) * 0.13f;
				if (collideX)
				{
					Projectile.ai[0] = 2f;
				}
				if (!collideX && Projectile.ai[0] == 2f)
				{
					moveDirection.Y = -moveDirection.Y;
					Projectile.ai[1] = 0f;
					Projectile.ai[0] = 1f;
				}
				if (collideY)
				{
					moveDirection.X = -moveDirection.X;
					Projectile.ai[1] = 0f;
				}
			}
			Projectile.velocity = speed * moveDirection;
			Projectile.velocity = Collide();
		}
		protected virtual Vector2 Collide()
		{
			return Collision.noSlopeCollision(Projectile.position, Projectile.velocity, Projectile.width, Projectile.height, true, true);
		}

		public void DrawAdditive(SpriteBatch spriteBatch)
		{
			Texture2D texGlow = Request<Texture2D>("StarlightRiver/Assets/Keys/Glow").Value;

			int sin = (int)(Math.Sin(StarlightWorld.rottime * 3) * 40f);
			var color = new Color(72 + sin, 30 + (sin / 2), 127);

			spriteBatch.Draw(texGlow, Projectile.Center - Main.screenPosition, null, color * Projectile.scale, 0, texGlow.Size() / 2, Projectile.scale * 1.0f, default, default);
			spriteBatch.Draw(texGlow, Projectile.Center - Main.screenPosition, null, color * Projectile.scale * 1.2f, 0, texGlow.Size() / 2, Projectile.scale * 1.6f, default, default);

			var effect1 = Filters.Scene["CrescentOrb"].GetShader().Shader;
			effect1.Parameters["sampleTexture"].SetValue(Request<Texture2D>("StarlightRiver/Assets/Items/Moonstone/QuarterstaffMap").Value);
			effect1.Parameters["sampleTexture2"].SetValue(Request<Texture2D>("StarlightRiver/Assets/Bosses/VitricBoss/LaserBallDistort").Value);
			effect1.Parameters["uTime"].SetValue(Main.GameUpdateCount * 0.01f);

			spriteBatch.End();
			spriteBatch.Begin(default, BlendState.NonPremultiplied, default, default, default, effect1, Main.GameViewMatrix.ZoomMatrix);

			spriteBatch.Draw(TextureAssets.Projectile[Projectile.type].Value, Projectile.Center - Main.screenPosition, null, Color.White * Projectile.scale, 0, Projectile.Size / 2, Projectile.scale * 1.7f, 0, 0);

			spriteBatch.End();
			spriteBatch.Begin(default, BlendState.Additive, SamplerState.PointWrap, default, default, default, Main.GameViewMatrix.ZoomMatrix);

		}
		}
}