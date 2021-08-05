using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Dusts;
using StarlightRiver.Core;
using StarlightRiver.Helpers;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Items.Vitric
{
	public class Needler : ModItem
	{
		public override string Texture => AssetDirectory.VitricItem + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Needler");
			Tooltip.SetDefault("Stick spikes to enemies to build up heat \nOverheated enemies explode, dealing massive damage");

		}


		//TODO: Adjust rarity sellprice and balance
		public override void SetDefaults()
		{
			item.damage = 8;
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
			//Main.PlaySound(mod.GetLegacySoundSlot(SoundType.Custom, "Sounds/Guns/SMG2"), position);
			Helper.PlayPitched("Guns/SMG2", 0.4f, Main.rand.NextFloat(-0.1f, 0.1f));
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

				if (target.GetGlobalNPC<NeedlerNPC>().needleTimer == 1)
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
				target.GetGlobalNPC<NeedlerNPC>().needleDamage = projectile.damage;
				target.GetGlobalNPC<NeedlerNPC>().needlePlayer = projectile.owner;
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
			if (stuck)
			{
				tex = ModContent.GetTexture(AssetDirectory.VitricItem + "NeedlerBloom");
				color = Color.Lerp(Color.Orange, Color.Red, needleLerp / 20f);
				color.A = 0;
				spriteBatch.Draw(tex, (projectile.Center - Main.screenPosition + new Vector2(0, projectile.gfxOffY)), null, color * 0.66f, projectile.rotation, new Vector2(tex.Width, tex.Height) / 2, ((projectile.scale * (needleLerp / 10f)) + 0.25f) * new Vector2(1f, 1.25f), SpriteEffects.None, 0f);
			}
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
			projectile.width = projectile.height = 200;
			projectile.timeLeft = 20;
			projectile.extraUpdates = 1;
		}
        public override void AI()
        {
			for (int i = 0; i < 2; i++)
				Gore.NewGoreDirect(projectile.Center + Main.rand.NextVector2Circular(25, 25), Main.rand.NextFloat(3.14f,6.28f).ToRotationVector2() * 7, ModGore.GetGoreSlot("StarlightRiver/Assets/NPCs/Vitric/MagmiteGore"), Main.rand.NextFloat(0.4f, 0.8f));
		}
        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
			/*Main.spriteBatch.End();
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
			Main.spriteBatch.Draw(Main.projectileTexture[projectile.type], (projectile.Center - Main.screenPosition), null, Color.White, 0f, new Vector2(200, 200), 1, SpriteEffects.None, 0f);

			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, Main.GameViewMatrix.TransformationMatrix);*/
			return false;
        }
        public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
        {
			crit = true;
            base.ModifyHitNPC(target, ref damage, ref knockback, ref crit, ref hitDirection);
        }
		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
		{
			target.AddBuff(BuffID.OnFire, 180);
			if (target.GetGlobalNPC<NeedlerNPC>().needles >= 1 && target.GetGlobalNPC<NeedlerNPC>().needleTimer <= 0)
            {
				target.GetGlobalNPC<NeedlerNPC>().needleTimer = 60;
			}
				
		}
	}
	public class NeedlerEmber : ModProjectile
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
			projectile.aiStyle = 1;
			projectile.width = projectile.height = 12;
			ProjectileID.Sets.TrailCacheLength[projectile.type] = 9;
			ProjectileID.Sets.TrailingMode[projectile.type] = 0;
			projectile.extraUpdates = 1;
			projectile.alpha = 255;
		}
        public override void AI()
        {
			projectile.scale *= 0.98f;
			if (Main.rand.Next(2) == 0)
			{
				Dust dust = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<NeedlerDustThree>(), Main.rand.NextVector2Circular(1.5f, 1.5f));
				dust.scale = 0.6f * projectile.scale;
				dust.rotation = Main.rand.NextFloatDirection();
			}
		}
    }
	public class NeedlerNPC : GlobalNPC
	{
		public override bool InstancePerEntity => true;
		public int needles = 0;
		public int needleTimer = 0;
		public int needleDamage = 0;
		public int needlePlayer = 0;

        public override void ResetEffects(NPC npc)
        {
			needleTimer--;
			base.ResetEffects(npc);
        }
        public override void AI(NPC npc)
        {
			if (needles >= 8 && needleTimer <= 0)
            {
				needles++;
				needleTimer = 60;
				Main.PlaySound(mod.GetLegacySoundSlot(SoundType.Custom, "Sounds/Magic/FireCast"), npc.Center);
			}
			if (needleTimer == 1)
			{
				Main.PlaySound(mod.GetLegacySoundSlot(SoundType.Custom, "Sounds/Magic/FireHit"), npc.Center);
				Projectile.NewProjectile(npc.Center, Vector2.Zero, ModContent.ProjectileType<NeedlerExplosion>(), (int)(needleDamage * Math.Sqrt(needles)), 0, needlePlayer);
				for (int i = 0; i < 10; i++)
				{
					Dust dust = Dust.NewDustDirect(npc.Center - new Vector2(16, 16), 0, 0, ModContent.DustType<NeedlerDust>());
					dust.velocity = Main.rand.NextVector2Circular(10, 10);
					dust.scale = Main.rand.NextFloat(1.5f, 1.9f);
					dust.alpha = 70 + Main.rand.Next(60);
					dust.rotation = Main.rand.NextFloat(6.28f);
				}
				for (int i = 0; i < 10; i++)
				{
					Dust dust = Dust.NewDustDirect(npc.Center - new Vector2(16, 16), 0, 0, ModContent.DustType<NeedlerDustTwo>());
					dust.velocity = Main.rand.NextVector2Circular(10, 10);
					dust.scale = Main.rand.NextFloat(1.5f, 1.9f);
					dust.alpha = Main.rand.Next(80) + 40;
					dust.rotation = Main.rand.NextFloat(6.28f);

					Dust.NewDustPerfect(npc.Center + Main.rand.NextVector2Circular(25, 25), ModContent.DustType<NeedlerDustFour>()).scale = 0.9f;
				}
				for (int i = 0; i < 5; i++)
                {
					Projectile.NewProjectileDirect(npc.Center, Main.rand.NextFloat(6.28f).ToRotationVector2() * Main.rand.NextFloat(2, 3), ModContent.ProjectileType<NeedlerEmber>(), 0, 0, needlePlayer).scale = Main.rand.NextFloat(0.85f,1.15f);
                }
				Main.player[npc.target].GetModPlayer<StarlightPlayer>().Shake = 20;
				needles = 0;
			}
			if (needleTimer > 30)
            {
				float angle = Main.rand.NextFloat(6.28f);
				Dust dust = Dust.NewDustPerfect((npc.Center - new Vector2(15,15)) - (angle.ToRotationVector2() * 70), ModContent.DustType<NeedlerDustFive>());
				dust.scale = 0.05f;
				dust.velocity = angle.ToRotationVector2() * 0.2f;
			}
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
				effect.Parameters["breakCounter"].SetValue((60 - needleTimer) / 60f);
				effect.Parameters["colorMod"].SetValue(color.ToVector4());
				effect.Parameters["noise"].SetValue(ModContent.GetTexture("StarlightRiver/Assets/Noise/ShaderNoise"));
				effect.CurrentTechnique.Passes[0].Apply();
				Main.spriteBatch.Draw(ModContent.GetTexture(AssetDirectory.VitricItem + "NeedlerShaderMask"), (npc.Center - Main.screenPosition) - new Vector2(0, 6), null, Color.White, 0f, new Vector2(50, 50), new Vector2(npc.width,npc.height) / 50, SpriteEffects.None, 0f);
				Main.spriteBatch.End();
				Main.spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, Main.GameViewMatrix.TransformationMatrix);
			}*/
            return base.PreDraw(npc, spriteBatch, drawColor);
        }
    }
}
