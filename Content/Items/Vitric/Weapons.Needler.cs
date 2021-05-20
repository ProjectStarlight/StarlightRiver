using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using Terraria.Graphics.Effects;
using StarlightRiver.Helpers;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Items.Vitric
{
	public class Needler : ModItem
	{
		public override string Texture => AssetDirectory.VitricItem + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Needler");
			Tooltip.SetDefault("Stick spikes to enemies to build up heat \nOverheated enemies explode");

		}

		public override void SetDefaults()
		{
			item.damage = 15;
			item.ranged = true;
			item.width = 24;
			item.height = 24;
			item.useTime = 5;
			item.useAnimation = 5;
			item.useStyle = ItemUseStyleID.HoldingOut;
			item.noMelee = true;
			item.knockBack = 0;
			item.rare = ItemRarityID.Orange;
			item.shoot = ModContent.ProjectileType<NeedlerProj>();
			item.shootSpeed = 14f;
			item.autoReuse = true;
		}
		//TODO: Add glowmask to weapon
		//TODO: Add holdoffset
		public override bool Shoot(Player player, ref Microsoft.Xna.Framework.Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
		{
			//TODO: Add sound effects
			Vector2 direction = new Vector2(speedX, speedY);
			float itemRotation = Main.rand.NextFloat(-0.1f, 0.1f);
			direction = direction.RotatedBy(itemRotation * 2);
			Projectile.NewProjectile(position, direction, type, damage, knockBack, player.whoAmI);

			direction = new Vector2(speedX, speedY).RotatedBy(itemRotation);
			for (int i = 0; i < 15; i++)
			{
				Dust dust = Dust.NewDustPerfect(position + (direction * 4.4f), 6, (direction.RotatedBy(Main.rand.NextFloat(-1, 1)) / 5f) * Main.rand.NextFloat());
				dust.noGravity = true;
			}
			player.itemRotation = direction.ToRotation(); //TODO: Wrap properly when facing left
			if (player.direction != 1)
			{
				player.itemRotation -= 3.14f;
			}
			return false;
		}
	}
	public class NeedlerProj : ModProjectile
	{

		public override string Texture => AssetDirectory.VitricItem + Name;
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Needle");
		}
		public override void SetDefaults()
		{
			projectile.penetrate = 1;
			projectile.tileCollide = true;
			projectile.hostile = false;
			projectile.friendly = true;
			projectile.aiStyle = 113;
			projectile.width = projectile.height = 20;
			ProjectileID.Sets.TrailCacheLength[projectile.type] = 9;
			ProjectileID.Sets.TrailingMode[projectile.type] = 0;
		}
		int enemyID;
		bool stuck = false;
		Vector2 offset = Vector2.Zero;

		float needleLerp = 0f;

		//TODO: Move methods to top + method breaks

		//TODO: Turn needles into getnset
		public override bool PreAI()
		{
			if (stuck)
			{
				NPC target = Main.npc[enemyID];
				int needles = target.GetGlobalNPC<NeedlerNPC>().needles;

				if (Main.rand.Next(Math.Max(((10 - needles) * 30) + 300, 50)) == 0)
					Gore.NewGoreDirect(projectile.Center, Vector2.Zero, ModGore.GetGoreSlot("StarlightRiver/Assets/NPCs/Vitric/MagmiteGore"), Main.rand.NextFloat(0.4f, 0.8f));

				if (needles == 0)
				{
					//Projectile.NewProjectile(projectile.Center, Vector2.Zero, ModContent.ProjectileType<NeedlerExplosion>(), projectile.damage * 3, projectile.knockBack, projectile.owner);
					projectile.active = false;
				}
				if (needles > needleLerp)
				{
					if (needleLerp < 10)
					{
						needleLerp += 0.2f;
						if (needles > needleLerp + 3)
                        {
							needleLerp += 0.4f;
                        }
					}
				}
				else
					needleLerp = needles;
				Color lightColor = Color.Lerp(Color.Orange, Color.Red, needleLerp / 20f);
				Lighting.AddLight(projectile.Center, lightColor.R * needleLerp / 2000f, lightColor.G * needleLerp / 2000f, lightColor.B * needleLerp / 2000f);
				if (!target.active)
				{
					if (projectile.timeLeft > 5)
						projectile.timeLeft = 5;
					projectile.velocity = Vector2.Zero;
				}
				else
				{
					projectile.position = target.position + offset;
				}
				if (projectile.timeLeft == 2)
					target.GetGlobalNPC<NeedlerNPC>().needles--;
				return false;
			}
			else
			{
				projectile.rotation = projectile.velocity.ToRotation();
            }
			return true;
		}
		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
		{
			if (!stuck && target.life > 0)
			{
				projectile.penetrate++;
				target.GetGlobalNPC<NeedlerNPC>().needles++;
				stuck = true;
				projectile.friendly = false;
				projectile.tileCollide = false;
				enemyID = target.whoAmI;
				offset = projectile.position - target.position;
				offset -= projectile.velocity;
				projectile.timeLeft = 400;
			}
		}
		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
		{
			NPC target = Main.npc[enemyID];
			Color color;
			if (stuck)
				color = VitricSummonOrb.MoltenGlow(100 - (needleLerp * 10));

			else
				color = VitricSummonOrb.MoltenGlow(100);
			Texture2D tex = Main.projectileTexture[projectile.type];
			spriteBatch.Draw(tex, (projectile.Center - Main.screenPosition + new Vector2(0, projectile.gfxOffY)), VitricSummonOrb.WhiteFrame(tex.Size().ToRectangle(), false), lightColor, projectile.rotation, new Vector2(tex.Width / 2, tex.Height / 2), projectile.scale, SpriteEffects.None, 0);
			spriteBatch.Draw(tex, (projectile.Center - Main.screenPosition + new Vector2(0, projectile.gfxOffY)), VitricSummonOrb.WhiteFrame(tex.Size().ToRectangle(), true), color * (needleLerp / 10f), projectile.rotation, new Vector2(tex.Width / 2, tex.Height / 2), projectile.scale, SpriteEffects.None, 0);

			tex = ModContent.GetTexture(AssetDirectory.VitricItem + "NeedlerBloom");
			color = Color.Lerp(Color.Orange, Color.Red, needleLerp / 20f);
			color.A = 0;
			spriteBatch.Draw(tex, (projectile.Center - Main.screenPosition + new Vector2(0, projectile.gfxOffY)), null, color * 0.66f, projectile.rotation, new Vector2(tex.Width, tex.Height) / 2, ((projectile.scale * (needleLerp / 10f)) + 0.25f) * new Vector2(1f,1.25f), SpriteEffects.None, 0f);
			return false;
		}
	}

	public class NeedlerExplosion : ModProjectile
	{

		public override string Texture => AssetDirectory.VitricItem + Name;
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Needle");
		}
		public override void SetDefaults()
		{
			projectile.penetrate = -1;
			projectile.tileCollide = false;
			projectile.hostile = false;
			projectile.friendly = true;
			projectile.width = projectile.height = 300;
			projectile.timeLeft = 20;
			projectile.extraUpdates = 1;
		}

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);
			float progress = ((float)((20 - projectile.timeLeft) / 20f) - 1.5f) * 0.66f;
			Color color = Color.Orange;
			color.A = 0;
			Effect effect = Filters.Scene["NeedlerExplosion"].GetShader().Shader;
			effect.Parameters["colorMod"].SetValue(color.ToVector4());
			effect.Parameters["progress"].SetValue(progress);
			effect.Parameters["progress2"].SetValue((20 - projectile.timeLeft) / 20f);
			effect.Parameters["noise"].SetValue(ModContent.GetTexture("StarlightRiver/Assets/Noise/ShaderNoise"));
			effect.CurrentTechnique.Passes[0].Apply();
			Main.spriteBatch.Draw(Main.projectileTexture[projectile.type], (projectile.Center - Main.screenPosition), null, Color.White, 0f, new Vector2(150, 150), 1, SpriteEffects.None, 0f);

			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, Main.GameViewMatrix.TransformationMatrix);
			return false;
        }
    }
	public class NeedlerNPC : GlobalNPC
	{
		public override bool InstancePerEntity => true;
		public int needles = 0;
		public int needleTimer = 0;

        public override void ResetEffects(NPC npc)
        {
			needleTimer--;
			base.ResetEffects(npc);
        }
        public override void AI(NPC npc)
        {
			if (needles == 10)
            {
				needles++;
				needleTimer = 60;
            }
			if (needleTimer == 1)
				needles = 0;
			base.AI(npc);
        }
        public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Color drawColor)
        {
			/*if (needleTimer > 1)
            {
				Main.spriteBatch.End();
				Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);

				Color color = Color.Lerp(Color.Orange, Color.White, (60 - needleTimer) / 60f);
				Effect effect = Filters.Scene["NeedlerRays"].GetShader().Shader;
				effect.Parameters["breakCounter"].SetValue((90 - needleTimer) / 90f);
				effect.Parameters["colorMod"].SetValue(color.ToVector4());
				effect.Parameters["noise"].SetValue(ModContent.GetTexture("StarlightRiver/Assets/Noise/ShaderNoise"));
				effect.CurrentTechnique.Passes[0].Apply();
				Main.spriteBatch.Draw(ModContent.GetTexture(AssetDirectory.VitricItem + "NeedlerShaderMask"), (npc.Center - Main.screenPosition) - new Vector2(0, 6), null, Color.White, 0f, new Vector2(50, 50), 1, SpriteEffects.None, 0f);
				Main.spriteBatch.End();
				Main.spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, Main.GameViewMatrix.TransformationMatrix);
			}*/
			if (needleTimer == 1)
				Projectile.NewProjectile(npc.Center, Vector2.Zero, ModContent.ProjectileType<NeedlerExplosion>(), 50, 0, npc.target);
            return base.PreDraw(npc, spriteBatch, drawColor);
        }
    }
}
