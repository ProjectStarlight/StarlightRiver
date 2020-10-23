using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using StarlightRiver.NPCs;
using StarlightRiver.Core;
using StarlightRiver.Buffs.Summon;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Projectiles.WeaponProjectiles.Summons
{

	public class VitricSummonSword : VitricSummonHammer
	{
		private bool dodamage = false; //wat-So the sword only hits at specific frames-IDG
		private int SwordFrame
		{
			get
			{
				return (int)projectile.localAI[1];
			}
			set
			{
				projectile.localAI[1] = value;
			}
		}
				public VitricSummonSword()
		{
			strikewhere = projectile.Center;
			enemysize = Vector2.One;
		}

		public override bool CanDamage() => dodamage;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Enchanted Vitric Weapons");
			Main.projFrames[projectile.type] = 1;
			ProjectileID.Sets.Homing[projectile.type] = true;
		}

		public sealed override void SetDefaults()
		{
			projectile.width = 64;
			projectile.height = 72;
			projectile.tileCollide = false;
			projectile.friendly = true;
			projectile.hostile = false;
			projectile.minion = true;
			projectile.penetrate = -1;
			projectile.timeLeft = 60;
			projectile.extraUpdates = 1;
			projectile.idStaticNPCHitCooldown = 5;
			projectile.usesIDStaticNPCImmunity = true;
		}

        public override void SendExtraAI(BinaryWriter writer)
        {
			writer.Write(dodamage);
			writer.Write(SwordFrame);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
			dodamage = reader.ReadBoolean();
			SwordFrame = reader.ReadInt32();
		}

        //Gonna use specific radian values here VS %'s of PI
        public override void DoAI()
		{

			oldhitbox = new Vector2(projectile.width, projectile.height);

			if (projectile.localAI[0] > 170)
				projectile.Kill();

			dodamage = false;

			if (projectile.localAI[0] == 1)
			{
				SwordFrame = 1;
				projectile.rotation = projectile.ai[0];
				projectile.spriteDirection = projectile.rotation > 500 ? -1 : 1;

				if (projectile.rotation > 500)
					projectile.rotation -= 1000;

				projectile.ai[0] = 0;
				projectile.netUpdate = true;
			}

			if (enemy != null && enemy.active)
			{
				strikewhere = enemy.Center + new Vector2(enemy.velocity.X * 12, enemy.velocity.Y);
				enemysize = new Vector2(enemy.width, enemy.height);
			}

			Vector2 gothere=Vector2.Zero;

			PrepareToSwing(ref gothere);
			SlashUp(ref gothere);
			DownSlash(ref gothere);
			FinishingSlash(ref gothere);
			
		}

		private void PrepareToSwing(ref Vector2 gothere)
        {
			if (projectile.localAI[0] < 30)//Prepare to swing
			{
				if (projectile.localAI[0] == 15)
					SwordFrame = 0;

				float lerpval = Math.Min(projectile.localAI[0] / 10f, 1f);
				projectile.rotation = projectile.rotation.AngleLerp(0.349066f / 2f, 0.075f * lerpval);
				gothere = (strikewhere + new Vector2(projectile.spriteDirection * -((72 - projectile.localAI[0]) + (enemysize.X / 2f)), projectile.localAI[0] * 3));
				projectile.velocity += ((gothere - projectile.Center) / 100f);

				if (projectile.velocity.Length() > 2f + (10f * lerpval))
					projectile.velocity = Vector2.Normalize(projectile.velocity) * (2f + (10f * lerpval));

				projectile.velocity /= 1f + (0.10f * lerpval);
			}
		}
		private void SlashUp(ref Vector2 gothere)
		{
			if (projectile.localAI[0] < 70 && projectile.localAI[0] >= 30)//Upper Cut swing
			{
				if (projectile.localAI[0] == 36)
					SwordFrame = 1;

				if (projectile.localAI[0] == 42)
				{
					dodamage = true;
					SwordFrame = 2;
					Main.PlaySound(SoundID.Item, (int)projectile.position.X, (int)projectile.position.Y, 7, 0.75f);
				}

				if (projectile.localAI[0] == 50)
					SwordFrame = 3;

				float offset = (60 - projectile.localAI[0]) * 2f + (enemysize.X / 10f);
				float lerpval = Math.Min((projectile.localAI[0] - 30) / 10f, 1f);
				projectile.rotation = projectile.rotation.AngleLerp(-1.39626f / 2f, 0.075f * lerpval);
				gothere = (strikewhere + new Vector2(projectile.spriteDirection * (-32 + offset), -64));
				projectile.velocity += ((gothere - projectile.Center) / 50f);

				if (projectile.velocity.Length() > 14f * lerpval)
					projectile.velocity = Vector2.Normalize(projectile.velocity) * 14 * lerpval;

				projectile.velocity /= 1f + (0.5f * lerpval);
			}
		}
		private void DownSlash(ref Vector2 gothere)
		{

			if (projectile.localAI[0] < 100 && projectile.localAI[0] >= 70)//Down Slash
			{
				if (projectile.localAI[0] == 82)
					SwordFrame = 3;

				if (projectile.localAI[0] == 86)
				{
					dodamage = true;
					SwordFrame = 2;
					Main.PlaySound(SoundID.Item, (int)projectile.position.X, (int)projectile.position.Y, 7, 0.75f);
				}

				//format like this when things line up
				if (projectile.localAI[0] == 90) SwordFrame = 2;
				if (projectile.localAI[0] == 98) SwordFrame = 1;
				if (projectile.localAI[0] == 114) SwordFrame = 0;


				float offset = (80 - projectile.localAI[0]) * 12f;
				float lerpval = Math.Min((projectile.localAI[0] - 70) / 10f, 1f);
				projectile.rotation = projectile.rotation.AngleLerp(1.39626f  /2f, 0.06f * lerpval);
				gothere = (strikewhere + new Vector2(projectile.spriteDirection * (-36 + offset - (enemysize.X / 2f)), 72));
				projectile.velocity += ((gothere - projectile.Center) / 50f);

				if (projectile.velocity.Length() > 14f * lerpval)
					projectile.velocity = Vector2.Normalize(projectile.velocity) * 14 * lerpval;

				projectile.velocity /= 1f + (0.5f * lerpval);
			}
		}
		private void FinishingSlash(ref Vector2 gothere)
		{
			if (projectile.localAI[0] < 200 && projectile.localAI[0] >= 100)//Big Upper Cut swing
			{
				if (projectile.localAI[0] == 136)
					SwordFrame = 1;

				if (projectile.localAI[0] == 142)
				{
					dodamage = true;
					SwordFrame = 2;
					Main.PlaySound(SoundID.Item, (int)projectile.position.X, (int)projectile.position.Y, 7, 0.75f);
				}

				if (projectile.localAI[0] == 150)
					SwordFrame = 3;

				float lerpval = Math.Min((projectile.localAI[0] - 100) / 40f, 1f);

				if (projectile.localAI[0] < 130)
				{
					projectile.rotation = projectile.rotation.AngleLerp(1.74533f / 2f, 0.075f * lerpval);
					gothere = (strikewhere + new Vector2(projectile.spriteDirection * (-96 - (enemysize.X / 2f)), 70));
				}
				else
				{
					float offset = (150 - projectile.localAI[0]) * (6f + enemysize.X / 3f);
					projectile.rotation = projectile.rotation.AngleLerp(-1.39626f / 2f, 0.075f * lerpval);
					gothere = (strikewhere + new Vector2(projectile.spriteDirection * ((-32 + offset) - (enemysize.X / 2f)), -160));

					if (projectile.localAI[0] > 150)
					{
						for (float num315 = 1f; num315 < 3; num315 += 0.5f)
						{
							float angle = Main.rand.NextFloat(-MathHelper.Pi / 4f, MathHelper.Pi / 4f);
							Dust num316 = Dust.NewDustPerfect(projectile.Center + new Vector2(Main.rand.Next(40), Main.rand.Next(60) - 30), mod.DustType("Glass2"), ((projectile.velocity * num315) * 0.25f).RotatedBy(angle), (int)(((projectile.localAI[0] - 150f) / 20f) * 255f), default, (40f - num315) / 40f);
							num316.noGravity = true;
							num316.fadeIn = 0.75f;
						}
					}

				}

				projectile.velocity += ((gothere - projectile.Center) / 50f);

				if (projectile.velocity.Length() > 2 + (14f * lerpval))
					projectile.velocity = Vector2.Normalize(projectile.velocity) * (2f + (14 * lerpval));

				projectile.velocity /= 1f + (0.5f * lerpval);

			}
		}

		public override bool PreKill(int timeLeft)
		{
			if (projectile.localAI[0] < 150)
				return base.PreKill(timeLeft);

			return true;
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			int[] hitboxframe = { 0, (int)(projectile.height / 2f), (int)(projectile.height / 2f), (int)(projectile.height) };
			return base.Colliding(new Rectangle((int)projectile.Center.X - 12, -16 + (int)projectile.position.Y - hitboxframe[SwordFrame], projectile.width + 24, projectile.height + 32), targetHitbox);
		}

		public override void Draw(SpriteBatch spriteBatch, Vector2 drawpos, Color lightColor, float aimframe)
		{
			Texture2D tex = Main.projectileTexture[projectile.type];

			Vector2 pos = VitricSummonOrb.SwordOff[SwordFrame];
			Vector2 drawOrigin = new Vector2((projectile.spriteDirection < 0 ? tex.Width - pos.X : pos.X) / 2f, pos.Y);
			Vector2 drawPos = ((drawpos - Main.screenPosition));
			Color color = lightColor * Math.Min(1f, 1f - ((projectile.localAI[0] - 140f) / 30f));
			spriteBatch.Draw(tex, drawPos, VitricSummonOrb.WhiteFrame(new Rectangle(0, SwordFrame * (tex.Height / 4), tex.Width, tex.Height / 4), false), color, projectile.rotation * projectile.spriteDirection, drawOrigin, projectile.scale, projectile.spriteDirection > 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0f);
			spriteBatch.Draw(tex, drawPos, VitricSummonOrb.WhiteFrame(new Rectangle(0, SwordFrame * (tex.Height / 4), tex.Width, tex.Height / 4), true), VitricSummonOrb.MoltenGlow(moltenglowanim), projectile.rotation * projectile.spriteDirection, drawOrigin, projectile.scale, projectile.spriteDirection > 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0f);
		}
	}

}