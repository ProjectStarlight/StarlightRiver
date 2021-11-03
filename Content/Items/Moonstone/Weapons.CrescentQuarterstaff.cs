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
			item.damage = 20;
			item.melee = true;
			item.width = 36;
			item.height = 44;
			item.useTime = 12;
			item.useAnimation = 12;
			item.reuseDelay = 20;
			item.channel = true;
			item.useStyle = ItemUseStyleID.HoldingOut;
			item.knockBack = 6.5f;
			item.value = Item.sellPrice(0, 1, 0, 0);
			item.crit = 4;
			item.rare = 2;
			item.shootSpeed = 14f;
			item.autoReuse = false;
			item.shoot = ProjectileType<CrescentQuarterstaffProj>();
			item.noUseGraphic = true;
			item.noMelee = true;
			item.autoReuse = false;
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

		private const float SWAY = 10;

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

		private bool clickTimed = false;

		private bool clickCheck = false;

		private int charge;

		private float Charge => (float)charge / (float)MAXCHARGE;
		private bool MouseFacingLeft => (Main.MouseWorld.X < Player.Center.X);
		private bool FirstTickOfSwing
		{
			get => projectile.ai[0] == 0;
			set => projectile.ai[0] = value ? 0 : 1;
		}

		Player Player => Main.player[projectile.owner];


		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Crescent Quarterstaff");
			Main.projFrames[projectile.type] = 1;
		}

		public override void SetDefaults()
		{
			projectile.friendly = true;
			projectile.melee = true;
			projectile.tileCollide = false;
			projectile.Size = new Vector2(130, 130);
			projectile.penetrate = -1;
			projectile.usesLocalNPCImmunity = true;
			projectile.localNPCHitCooldown = 16;
			projectile.ownerHitCheck = true;
		}

        public override bool PreAI()
        {
			Player.heldProj = projectile.whoAmI;
			projectile.velocity = Vector2.Zero;
			projectile.Center = Player.Center + (projectile.rotation.ToRotationVector2() * LENGTH);
			Lighting.AddLight(projectile.Center, new Vector3(0.905f, 0.89f, 1) * Charge);
			projectile.ai[1] += 0.01f;
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
				clickTimed = false;
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
					projectile.rotation = -1.57f;
					initialized = true;
				}
				if (currentAttack == CurrentAttack.Reset)
					projectile.active = false;

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
				rotDifference = ((((direction.ToRotation() - projectile.rotation) % 6.28f) + 9.42f) % 6.28f) - 3.14f;

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
						Vector2 tilePos = Player.Center + ((projectile.rotation).ToRotationVector2() * LENGTH);
						tilePos.Y += 15;
						tilePos /= 16;

						if (angularVelocity != 0)
						{
							if (Main.tile[(int)tilePos.X, (int)tilePos.Y].collisionType == 1 && angularVelocity != 0 && Math.Sign(projectile.rotation.ToRotationVector2().X) == Math.Sign(direction.X))
							{
								Player.GetModPlayer<StarlightPlayer>().Shake += 12;
								angularVelocity = 0;
								attackDuration = 30;
								Projectile.NewProjectile(projectile.Center, Vector2.Zero, ModContent.ProjectileType<GravediggerSlam>(), 0, 0, Player.whoAmI);
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

					if (Main.mouseLeft && !clickCheck)
						clickTimed = true;

					if (clicked && slashWindow <= 16)
						FirstTickOfSwing = true;

					if (clickTimed && slashWindow <= 25)
						FirstTickOfSwing = true;
				}

				if (slashWindow < 0)
					projectile.active = false;
			}

			if (Player == Main.LocalPlayer)
			{
				if (Main.mouseLeft && Player == Main.LocalPlayer)
					clickCheck = true;
				else
					clickCheck = false;
			}
			projectile.rotation += angularVelocity;
		}

        public override bool? CanHitNPC(NPC target)
        {
			if (attackDuration <= 0)
				return false;
            return base.CanHitNPC(target);
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
			target.immune[projectile.owner] = 20;
			if (charge < MAXCHARGE)
				charge++;
			pauseTime = 7;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			// Custom collision so all chains across the flail can cause impact.
			float collisionPoint = 0f;
			if (Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Player.Center, projectile.Center, 24 * projectile.scale, ref collisionPoint))
			{
				return true;
			}
			return false;
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
			Texture2D head = ModContent.GetTexture(Texture + "_Head");
			Texture2D tex = Main.projectileTexture[projectile.type];
			SpriteEffects effects = zRotation > 1.57f && zRotation < 4.71f ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
			Vector2 origin = new Vector2(140, 10);
			origin.X -= LENGTH;
			origin.Y += LENGTH;
			if (effects == SpriteEffects.FlipHorizontally)
				origin.X = 150 - origin.X;

			spriteBatch.End();
			spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

			Effect effect = Filters.Scene["MoonFireAura"].GetShader().Shader;
			effect.Parameters["time"].SetValue(projectile.ai[1]);
			effect.Parameters["fireHeight"].SetValue(0.03f * Charge);
			effect.Parameters["fnoise"].SetValue(GetTexture(AssetDirectory.MoonstoneItem + "DatsuzeiFlameMap1"));
			effect.Parameters["fnoise2"].SetValue(GetTexture(AssetDirectory.MoonstoneItem + "DatsuzeiFlameMap2"));
			effect.Parameters["vnoise"].SetValue(GetTexture(AssetDirectory.MoonstoneItem + "QuarterstaffMap"));
			effect.CurrentTechnique.Passes[0].Apply();

			spriteBatch.Draw(head, Player.Center - Main.screenPosition, null, lightColor, projectile.rotation + 0.78f, origin, new Vector2(projectile.scale * (float)Math.Abs(Math.Cos(zRotation)), projectile.scale), effects, 0);

			spriteBatch.End();
			spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

			effect.Parameters["time"].SetValue(projectile.ai[1]);
			effect.Parameters["fireHeight"].SetValue(0.03f * Charge);
			effect.Parameters["fnoise"].SetValue(GetTexture(AssetDirectory.MoonstoneItem + "DatsuzeiFlameMap1"));
			effect.Parameters["fnoise2"].SetValue(GetTexture(AssetDirectory.MoonstoneItem + "DatsuzeiFlameMap2"));
			effect.Parameters["vnoise"].SetValue(GetTexture(AssetDirectory.MoonstoneItem + "QuarterstaffMap"));
			effect.CurrentTechnique.Passes[1].Apply();

			spriteBatch.Draw(head, Player.Center - Main.screenPosition, null, lightColor, projectile.rotation + 0.78f, origin, new Vector2(projectile.scale * (float)Math.Abs(Math.Cos(zRotation)), projectile.scale), effects, 0);

			spriteBatch.End();
			spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.TransformationMatrix);

			spriteBatch.Draw(tex, Player.Center - Main.screenPosition, null, Color.Lerp(lightColor, Color.White, Charge), projectile.rotation + 0.78f, origin, new Vector2(projectile.scale * (float)Math.Abs(Math.Cos(zRotation)), projectile.scale), effects, 0);
			return false;
        }

        private void AdjustPlayer()
        {
			Player.ChangeDir(FacingRight ? -1 : 1);
			Player.itemRotation = MathHelper.WrapAngle(projectile.rotation - ((Player.direction < 0) ? 0 : MathHelper.Pi));
			Player.itemAnimation = Player.itemTime = 5;
		}
    }
}