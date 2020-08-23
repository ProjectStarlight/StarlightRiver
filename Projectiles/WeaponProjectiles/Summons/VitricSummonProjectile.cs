using System;
using System.Linq;
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

namespace StarlightRiver.Projectiles.WeaponProjectiles.Summons
{

	public class VitricSummonOrb : ModProjectile
	{
		private string[] weapontex = { "StarlightRiver/Projectiles/WeaponProjectiles/Summons/Weapon1" };

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Enchanted Vitric Weapons");
			Main.projFrames[projectile.type] = 1;
			ProjectileID.Sets.MinionTargettingFeature[projectile.type] = true;
			Main.projPet[projectile.type] = true;
			ProjectileID.Sets.MinionSacrificable[projectile.type] = true;
			ProjectileID.Sets.Homing[projectile.type] = true;
		}
		public sealed override void SetDefaults()
		{
			projectile.width = 16;
			projectile.height = 16;
			projectile.tileCollide = false;
			projectile.friendly = true;
			projectile.hostile = false;
			projectile.minion = true;
			projectile.minionSlots = 1f;
			projectile.penetrate = -1;
			projectile.timeLeft = 60;
		}
		public override bool CanDamage() => false;
		public override bool MinionContactDamage() => false;

		public override void AI()
		{
			//if (projectile.owner == null || projectile.owner < 0)
			//return;


			Player player = Main.player[projectile.owner];
			if (player.dead || !player.active)
			{
				player.ClearBuff(ModContent.BuffType<VitricSummonBuff>());
			}
			if (player.HasBuff(ModContent.BuffType<VitricSummonBuff>()))
			{
				projectile.timeLeft = 2;
			}
			bool toplayer = true;
			Vector2 gothere = player.Center+new Vector2(player.direction*-40,-12);
			if ((int)projectile.ai[1] > 0)
			{
				projectile.Center = player.Center;
				projectile.localAI[0] = 0f;
			}
			projectile.localAI[0] += 1;
			projectile.ai[1] -= 1;

			List<NPC> closestnpcs = new List<NPC>();
			for(int i = 0; i < Main.maxNPCs; i += 1)
			{
				bool colcheck= Collision.CheckAABBvLineCollision(Main.npc[i].position, new Vector2(Main.npc[i].width, Main.npc[i].height), Main.npc[i].Center,projectile.Center)
					&& Collision.CanHit(Main.npc[i].Center,0,0, projectile.Center,0,0);
				if (Main.npc[i].active && !Main.npc[i].friendly && !Main.npc[i].townNPC && !Main.npc[i].dontTakeDamage && Main.npc[i].CanBeChasedBy() && colcheck
					&& (Main.npc[i].Center-player.Center).Length()<300)
					closestnpcs.Add(Main.npc[i]);
			}

			//int it=player.grappling.OrderBy(n => (Main.projectile[n].active ? 0 : 999999) + Main.projectile[n].timeLeft).ToArray()[0];
			NPC them = closestnpcs.Count<1 ? null : closestnpcs.ToArray().OrderBy(npc => projectile.Distance(npc.Center)).ToList()[0];
			NPC oldthem = null;

			if (player.HasMinionAttackTargetNPC)
			{
				oldthem = them;
				them = Main.npc[player.MinionAttackTargetNPC];
				gothere = them.Center + new Vector2(them.direction * 96, them.direction==0 ? -96 : 0);
			}

			if (them != null && them.active)
			{
				toplayer = false;
				if (!player.HasMinionAttackTargetNPC)
				gothere = them.Center + Vector2.Normalize(projectile.Center- them.Center) *64f;

				DoAttack((byte)projectile.ai[0], them);

			}
			
			float us = 0f;
			float maxus = 0f;

			for (int i = 0; i < Main.maxProjectiles; i++)
			{
				Projectile currentProjectile = Main.projectile[i];
				if (currentProjectile.active
				&& currentProjectile.owner == Main.myPlayer
				&& currentProjectile.type == projectile.type)
				{
					if (i == projectile.whoAmI)
						us = maxus;
					maxus += 1f;
				}
			}
			Vector2 there = player.Center;

			int timer = player.GetModPlayer<StarlightPlayer>().Timer * 2;
			double angles = MathHelper.ToRadians(((float)((us / maxus) * 360.00) - 90f)+ timer);
			float dist = 16f;
			float aval = (float)timer+ (us*83f);
			Vector2 here;
			if (!toplayer)
			{
				here = (new Vector2((float)Math.Sin(aval / 60f) * 6f, 20f * ((float)Math.Sin(aval / 70f)))).RotatedBy((them.Center - gothere).ToRotation());
				projectile.rotation = projectile.rotation.AngleTowards((projectile.velocity.X) * 0.10f, 0.1f);
			}
			else
			{
				float anglz = (float)(Math.Cos(MathHelper.ToRadians(aval)) * player.direction) / 4f;
				projectile.rotation = projectile.rotation.AngleTowards(((player.direction * 0) + anglz)- (projectile.velocity.X*projectile.spriteDirection) * 0.07f, 0.05f);
				here = new Vector2((float)Math.Cos(angles) / 2f, (float)Math.Sin(angles)) * dist;
			}

			Vector2 where = gothere + here;
			Vector2 difference = where - projectile.Center;

			if ((where - projectile.Center).Length() > 0f)
			{
				if (toplayer)
				{
					projectile.velocity += (where - projectile.Center) * 0.25f;
					projectile.velocity *= 0.725f;
					projectile.spriteDirection = player.direction;
				}
				else
				{
					projectile.velocity += (where - projectile.Center) * 0.005f;
					projectile.velocity *= 0.925f;
				}
			}

			float maxspeed = Math.Min(projectile.velocity.Length(), 12+(toplayer ? player.velocity.Length() : 0));
			projectile.velocity.Normalize();
			projectile.velocity *= maxspeed;

		}

		public override string Texture => "StarlightRiver/NPCs/Boss/VitricBoss/CrystalWave";

		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
		{

			if (projectile.localAI[0] < 0 || projectile.ai[1] > 0)
				return false;
			Texture2D tex = ModContent.GetTexture(weapontex[(int)projectile.ai[0]]);
			Player player = Main.player[projectile.owner];

			Vector2 drawOrigin = new Vector2(tex.Width, tex.Height) / 2f;
			Vector2 drawPos = ((projectile.Center - Main.screenPosition));
			Color color = Color.Lerp((projectile.GetAlpha(lightColor) * 0.5f), Color.White*(Math.Min(projectile.localAI[0]/15f,1f)), 1f);
			spriteBatch.Draw(tex, drawPos, null, color, projectile.rotation* projectile.spriteDirection, drawOrigin, projectile.scale,projectile.spriteDirection>0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0f);
			return false;
		}
		public void DoAttack(byte attack, NPC target)
		{
			if (projectile.ai[1] < 1)
			{
				if (attack == 0 && target.Distance(projectile.Center) < 72)
				{
					Projectile proj = Main.projectile[Projectile.NewProjectile(projectile.Center, projectile.velocity / 3f, ModContent.ProjectileType<VitricSummonHammer>(), projectile.damage, projectile.knockBack + 2f, projectile.owner)];
					proj.ai[0] = projectile.rotation;
					proj.ai[1] = target.whoAmI;
					projectile.ai[1] = 80;
					proj.netUpdate = true; projectile.netUpdate = true;
				}
			}
		}

	}

	public class VitricSummonHammer : ModProjectile
	{
		protected Vector2 strikewhere;
		protected Vector2 enemysize;
		protected Player player;
		protected NPC enemy;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Enchanted Vitric Weapons");
			Main.projFrames[projectile.type] = 1;
			ProjectileID.Sets.Homing[projectile.type] = true;
		}
		public override string Texture => "StarlightRiver/NPCs/Boss/VitricBoss/CrystalWave";
		public sealed override void SetDefaults()
		{
			projectile.width = 48;
			projectile.height = 32;
			projectile.tileCollide = false;
			projectile.friendly = true;
			projectile.hostile = false;
			projectile.minion = true;
			projectile.penetrate = -1;
			projectile.timeLeft = 60;
			projectile.extraUpdates = 1;
		}
		public VitricSummonHammer()
		{
			strikewhere = projectile.Center;
			enemysize = Vector2.One;
		}
		public override bool CanDamage() => projectile.localAI[0]>60;

		public override void AI()
		{
			player = Main.player[projectile.owner];
			if (player.HasBuff(ModContent.BuffType<VitricSummonBuff>()))
			{
				projectile.timeLeft = 2;
			}

			projectile.localAI[0] += 1;
			enemy = Main.npc[(int)projectile.ai[1]];
			DoAI();
		}

		public virtual void DoAI()
		{

			if (projectile.localAI[0] == 1)
			{
				projectile.rotation = projectile.ai[0];
				projectile.spriteDirection = player.direction;
				projectile.ai[0] = 0;
			}

			Vector2 gothere;
			if (projectile.localAI[0] < 60)//Swing up
			{

				if (enemy != null && enemy.active) { strikewhere = enemy.Center+new Vector2(enemy.velocity.X, enemy.velocity.Y/2f); enemysize = new Vector2(enemy.width, enemy.height); }

				projectile.rotation = projectile.rotation.AngleLerp(MathHelper.ToRadians(-45), 0.05f);
				gothere = (strikewhere + new Vector2(projectile.spriteDirection * -(75+ (projectile.localAI[0]*3f) + enemysize.X/2f), -200));
				projectile.velocity += (gothere-projectile.Center)/75f;
				if (projectile.velocity.Length() > 14)
					projectile.velocity = Vector2.Normalize(projectile.velocity) * 14;

				projectile.velocity /= 1.5f;
			}
			if (projectile.localAI[0] >= 60)//Swing Down
			{

				projectile.rotation = projectile.rotation.AngleTowards(MathHelper.ToRadians(45), 0.075f);
				gothere = (strikewhere + new Vector2(projectile.spriteDirection * -(32 + enemysize.X / 4f), -32));
				projectile.velocity.X += (MathHelper.Clamp(gothere.X - projectile.Center.X,-80f,80f)) / 24f;
					projectile.velocity.Y += 1f;
				if (projectile.velocity.Length() > 10)
					projectile.velocity = Vector2.Normalize(projectile.velocity) * 8;

				projectile.velocity.X /= 1.20f;

				if (projectile.Center.Y > gothere.Y)//Smashing!
				{
					Point16 point = new Point16((int)((projectile.Center.X+ (projectile.width/3f)*projectile.spriteDirection) / 16), Math.Min(Main.maxTilesY, (int)((projectile.Center.Y) / 16) + 1));
					Tile tile = Framing.GetTileSafely(point.X, point.Y);

					//hard coded dust ids in worldgen.cs, still ew
					//Tile hit!
					if (tile != null && WorldGen.InWorld(point.X, point.Y, 1) && tile.active() && Main.tileSolid[tile.type])
					{
						projectile.height += 32;
						projectile.position.Y -= 16;
						projectile.localAI[0] = 301;
						int dusttype = mod.DustType("Glass2");
							DustHelper.TileDust(tile, ref dusttype);

						for (float num315 = 2f; num315 < 15; num315 += 0.50f)
						{
							float angle = MathHelper.ToRadians(-Main.rand.Next(70, 130));
							Vector2 vecangle = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * num315*3f;
							int num316 = Dust.NewDust(new Vector2(projectile.position.X+ (projectile.spriteDirection*(int)(projectile.width*0.60)), projectile.Center.Y- projectile.height/2f), projectile.width/2, projectile.height, dusttype, 0f, 0f, 50, default, (12f - num315) / 5f);
							Main.dust[num316].noGravity = true;
							Main.dust[num316].velocity = vecangle;
							Main.dust[num316].fadeIn = 0.75f;
						}
					}

				}

				if (projectile.localAI[0] > 300)
					projectile.Kill();

			}
		}
		public override bool PreKill(int timeLeft)
		{
			int dusttype = mod.DustType("Glass2");
			for (float num315 = 2f; num315 < 12; num315 += 0.1f)
			{
				float angle = MathHelper.ToRadians(-Main.rand.Next(40, 140));
				Vector2 vecangle = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * num315;
				int num316 = Dust.NewDust(new Vector2(projectile.Center.X+(projectile.spriteDirection<0 ? -projectile.width : 0), projectile.Center.Y- projectile.height), projectile.width, projectile.height*2, dusttype, 0f, 0f, 50, default, (10f - num315) / 4f);
				Main.dust[num316].noGravity = true;
				Main.dust[num316].velocity = vecangle;
				Main.dust[num316].fadeIn = 0.5f;
			}

			Main.PlaySound(SoundID.Shatter, projectile.Center);
			return true;
		}
		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
		{
			Texture2D tex = ModContent.GetTexture("StarlightRiver/Projectiles/WeaponProjectiles/Summons/Weapon1");

			Vector2 drawOrigin = new Vector2(tex.Width, tex.Height) / 2f;
			Vector2 drawPos = ((projectile.Center - Main.screenPosition));
			Color color = lightColor;
			spriteBatch.Draw(tex, drawPos, null, color, projectile.rotation* projectile.spriteDirection, drawOrigin, projectile.scale, projectile.spriteDirection > 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0f);
			return false;
		}
	}


}