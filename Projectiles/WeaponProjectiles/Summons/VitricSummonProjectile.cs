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

	internal class VKnife
	{
		public Vector2 pos;
		public int index;
		public int max;
		public float rotation = 0;
		public int direction = 1;

		internal VKnife(int index, int max)
		{
			this.index = index;
			this.max = max;
		}

		public void Update(float rotation, Player owner)
		{
            float angle = (float)index / max * MathHelper.TwoPi * direction;
			pos = new Vector2((float)Math.Cos(rotation + angle) * 8, (float)Math.Sin(rotation + angle) * 16);
			this.rotation = (float)Math.Sin(rotation + angle) / 2f;
		}
	}

	public class VitricSummonOrb : ModProjectile
	{
		private float startchase = 0;
		private float reversechase = 0;
		private float moltenglowanim = 0f;

		private float AnimationTimer
		{
			get => projectile.localAI[0];
			set => projectile.localAI[0] = value;
		}
		private float MovementLerp
		{
			get => projectile.localAI[1];
			set => projectile.localAI[1] = value;
		}
		private List<VKnife> knives;
		public static readonly Vector2[] swordlocs = { new Vector2(4, 4), new Vector2(4, 9), new Vector2(4, 5), new Vector2(4, 38) };
		public static readonly Vector2[] holdweaponsoffset = { new Vector2(-32, -24), new Vector2(10, -6), new Vector2(-32, -16), new Vector2(30, -32) };
		private enum WeaponType
		{
			Hammer,Sword,Knives,Javelin
		}
		private int Weapon
		{
			get => (int)projectile.ai[0];
			set => projectile.ai[0] = (int)value;
		}
		private int DisabledTime
		{
			get => (int)projectile.ai[1];
			set => projectile.ai[1] = (int)value;
		}

		public VitricSummonOrb()
		{
			knives = new List<VKnife>();
			for (int i = 0; i < 3; i += 1)
				knives.Add(new VKnife(i, 3));
		}

		public override void SendExtraAI(BinaryWriter writer)
		{
			writer.Write((int)startchase);
			writer.Write((int)reversechase);
			writer.Write((int)AnimationTimer);
		}

		public override void ReceiveExtraAI(BinaryReader reader)
		{
			startchase = reader.ReadInt32();
			reversechase = reader.ReadInt32();
			AnimationTimer = (float)reader.ReadInt32();
		}

		public int NextWeapon
		{
			get
			{
				int projcount = 0;
				List<int> findweapon = new List<int>();
				for (int i = 0; i < 4; i++)
					findweapon.Add(i);

				for (int i = 0; i < Main.maxProjectiles; i++)
				{
					Projectile currentProjectile = Main.projectile[i];
					if (currentProjectile.active
					&& currentProjectile.owner == projectile.owner
					&& currentProjectile.type == projectile.type)
					{
						if (i == currentProjectile.whoAmI)
						{
							projcount += 1;
							for (int j = 0; j < 20; j++)
								findweapon.Add((int)currentProjectile.ai[0]);
						}
					}
				}

				if (projcount < 3)
				{
					for (int i = 0; i < 4; i++)
						for (int j = Weapon; j >= 0; j--)
							findweapon.Add(j);
				}

				//Find least common
				return (findweapon.ToArray().GroupBy(i => i).OrderBy(g => g.Count()).Select(g => g.Key).ToList()).First();
			}

		}

		public static Rectangle WhiteFrame(Rectangle tex,bool white)
		{
			return new Rectangle(white ? tex.Width / 2 : 0, tex.Y, tex.Width / 2, tex.Height);
		}

		public static Color MoltenGlow(float time)
		{
			Color MoltenGlowc = Color.White;
			if (time > 30 && time < 60)
				MoltenGlowc = Color.Lerp(Color.White, Color.Orange, Math.Min((time - 30f) / 20f,1f));
			else if (time >= 60)
				MoltenGlowc = Color.Lerp(Color.Orange, Color.Lerp(Color.Red,Color.Transparent, Math.Min((time - 60f) / 50f, 1f)), Math.Min((time - 60f) / 30f,1f));
			return MoltenGlowc;
		}

        public override bool CanDamage() => false;

        public override bool MinionContactDamage() => false;

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

		public override void AI()
		{
			//if (projectile.owner == null || projectile.owner < 0)
			//return;

			Player player = projectile.Owner();

			if (player.dead || !player.active)
				player.ClearBuff(ModContent.BuffType<VitricSummonBuff>());

			if (player.HasBuff(ModContent.BuffType<VitricSummonBuff>()))
				projectile.timeLeft = 2;

			bool toplayer = true;
			Vector2 gothere = player.Center+new Vector2(player.direction*(holdweaponsoffset[Weapon].X), holdweaponsoffset[Weapon].Y);

			if (DisabledTime > 0)
			{
				projectile.Center = player.Center;
				AnimationTimer = 0f;
				projectile.spriteDirection = player.direction;
				moltenglowanim = 0;
				if (DisabledTime == 1)
				Weapon = NextWeapon;
			}

			AnimationTimer += 1;
			moltenglowanim += Weapon == (int)WeaponType.Knives ? 2f : 1f;
			DisabledTime -= 1;

			List<NPC> closestnpcs = new List<NPC>();

			for(int i = 0; i < Main.maxNPCs; i += 1)
			{
				bool colcheck= Collision.CheckAABBvLineCollision(Main.npc[i].position, new Vector2(Main.npc[i].width, Main.npc[i].height), Main.npc[i].Center,projectile.Center)
					&& Collision.CanHit(Main.npc[i].Center,0,0, projectile.Center,0,0);

				if (Main.npc[i].active && !Main.npc[i].friendly && !Main.npc[i].townNPC && !Main.npc[i].dontTakeDamage && Main.npc[i].CanBeChasedBy() && colcheck
					&& (Main.npc[i].Center-player.Center).Length()<300)
					closestnpcs.Add(Main.npc[i]);
			}

			NPC them = closestnpcs.Count<1 ? null : closestnpcs.ToArray().OrderBy(npc => projectile.Distance(npc.Center)).ToList()[0];
			NPC oldthem = null;

			if (player.HasMinionAttackTargetNPC)
			{
				oldthem = them;
				them = Main.npc[player.MinionAttackTargetNPC];
				gothere = them.Center + new Vector2(them.direction * 120,-64);
			}

			if (them != null && them.active && AnimationTimer > 15)
			{
				toplayer = false;
				if (!player.HasMinionAttackTargetNPC)
				gothere = them.Center + Vector2.Normalize(projectile.Center- them.Center) *64f;

				DoAttack((byte)Weapon, them);
			}
			
			float thisprojectile = 0f; //These names are completely nondescript. Please change them. -IDG-ok
			float maxmatchingprojectiles = 0f;

			for (int i = 0; i < Main.maxProjectiles; i++)
			{
				Projectile currentProjectile = Main.projectile[i]; //Smells like there should be a helper method for checking this validity. -IDG-I do have an idea on what do about that later
				if (currentProjectile.active
				&& currentProjectile.owner == player.whoAmI
				&& currentProjectile.type == projectile.type)
				{
					if (i == projectile.whoAmI)
						thisprojectile = maxmatchingprojectiles;
					maxmatchingprojectiles += 1f;
				}
			}

			float timer = player.GetModPlayer<StarlightPlayer>().Timer * (MathHelper.TwoPi/180f);
			double angles = (float)(thisprojectile / maxmatchingprojectiles * MathHelper.TwoPi) - ((float)Math.PI/2f) + (timer*projectile.spriteDirection); //again, please work in radians.IDG-yes
			float dist = 16f;
			float aval = timer + (thisprojectile*MathHelper.Pi*0.465f) * projectile.spriteDirection;
			Vector2 here;

			if (!toplayer)
			{
				here = (new Vector2((float)Math.Sin(aval / 60f) * 6f, 20f * ((float)Math.Sin(aval / 70f)))).RotatedBy((them.Center - gothere).ToRotation());
				projectile.rotation = projectile.rotation.AngleTowards((MovementLerp * projectile.spriteDirection) * 0.10f, 0.1f);
			}
			else
			{
				float anglz = (float)(Math.Cos(aval)) / 4f;
				projectile.rotation = projectile.rotation.AngleTowards(((player.direction * 0) + anglz) - (MovementLerp * projectile.spriteDirection) * 0.07f, 0.05f);
				here = new Vector2((float)Math.Cos(angles) / 2f, (float)Math.Sin(angles)) * dist;
			}

			foreach(VKnife knife in knives)
				knife.Update(aval, player);

			Vector2 where = gothere + here;
			Vector2 difference = where - projectile.Center;

			if ((where - projectile.Center).Length() > 0f)
			{
				Vector2 diff = where - projectile.Center;

				if (toplayer)
				{
					startchase = 0f;
					reversechase += 1f;
					projectile.velocity += (diff/5f) * Math.Min(reversechase / 405f, 1f);
					projectile.velocity *= (0.725f*Math.Max(diff.Length()/100f,1f));
					projectile.spriteDirection = player.direction;
				}
				else
				{
					startchase += 1f;
					reversechase = 0f;
					projectile.velocity += diff * 0.005f * Math.Min(startchase / 45f, 1f);
					projectile.velocity *= 0.925f;
				}
			}

			float maxspeed = Math.Min(projectile.velocity.Length(), 12+(toplayer ? player.velocity.Length() : 0));
			projectile.velocity.Normalize();
			projectile.velocity *= maxspeed;

			MovementLerp += (projectile.velocity.X - MovementLerp)/20f;
		}

		public override string Texture => "StarlightRiver/NPCs/Boss/VitricBoss/CrystalWave";

		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
		{

			if (Weapon < 0 || DisabledTime > 0)
				return false;

			Texture2D tex = ModContent.GetTexture("StarlightRiver/Projectiles/WeaponProjectiles/Summons/Weapon"+(Weapon+1));

			float scale = Math.Min(AnimationTimer / 15f, 1f);
			Rectangle Rect = WhiteFrame(tex.Size().ToRectangle(), false);
			Rectangle Rect2 = WhiteFrame(tex.Size().ToRectangle(), true);


			Vector2 drawPos = ((projectile.Center - Main.screenPosition));
			Color color = lightColor * scale;
			Vector2 drawOrigin;

            //Local variables. Use them. Please. IDG-ok
			if ((int)Weapon == (int)WeaponType.Hammer || Weapon == (int)WeaponType.Javelin)
			{
				drawOrigin = new Vector2(tex.Width/2f, tex.Height) / 2f;
				spriteBatch.Draw(tex, drawPos, Rect, color, (projectile.rotation+ (Weapon == 3 ? MathHelper.Pi/2f : 0)) * projectile.spriteDirection, drawOrigin, projectile.scale*scale, projectile.spriteDirection > 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0f);
				spriteBatch.Draw(tex, drawPos, Rect2, MoltenGlow(moltenglowanim), (projectile.rotation+ (Weapon == 3 ? MathHelper.Pi/2f : 0)) * projectile.spriteDirection, drawOrigin, projectile.scale*scale, projectile.spriteDirection > 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0f);
			}

			if (Weapon == (int)WeaponType.Sword)
			{
				drawOrigin = new Vector2((projectile.spriteDirection<0 ? tex.Width-swordlocs[1].X : swordlocs[1].X)/2f, swordlocs[1].Y);
				spriteBatch.Draw(tex, drawPos, WhiteFrame(new Rectangle(0,tex.Height / 4,tex.Width, tex.Height/4),false), color, projectile.rotation * projectile.spriteDirection, drawOrigin, projectile.scale * scale, projectile.spriteDirection > 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0f);
				spriteBatch.Draw(tex, drawPos, WhiteFrame(new Rectangle(0,tex.Height / 4,tex.Width, tex.Height/4),true), MoltenGlow(moltenglowanim), projectile.rotation * projectile.spriteDirection, drawOrigin, projectile.scale * scale, projectile.spriteDirection > 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0f);
			}

			if (Weapon == (int)WeaponType.Knives)
			{
				foreach (VKnife knife in knives)
				{
					drawOrigin = new Vector2(tex.Width/2f, tex.Height) / 2f;
					float rotoffset = projectile.rotation + MathHelper.Pi/4f + knife.rotation;
					spriteBatch.Draw(tex, drawPos+ knife.pos * scale, Rect, color, rotoffset * projectile.spriteDirection, drawOrigin, projectile.scale * scale, projectile.spriteDirection > 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0f);
					spriteBatch.Draw(tex, drawPos+ knife.pos * scale, Rect2, MoltenGlow(moltenglowanim), rotoffset * projectile.spriteDirection, drawOrigin, projectile.scale * scale, projectile.spriteDirection > 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0f);
				}
			}

			return false;
		}

		public void DoAttack(byte attack, NPC target)
		{
			//something tells me attack should be an enum type here. Or at the very least commented
			//idg-done
			bool didattack = false;
			if (DisabledTime < 1 && AnimationTimer > 15)
			{
				float targetdistance = target.Distance(projectile.Center);
				if (attack == (int)WeaponType.Hammer && targetdistance < 72)
				{
					Projectile proj = Main.projectile[Projectile.NewProjectile(projectile.Center, projectile.velocity / 3f, ModContent.ProjectileType<VitricSummonHammer>(), projectile.damage, projectile.knockBack + 2f, projectile.owner)];
					proj.ai[0] = projectile.rotation + (projectile.spriteDirection > 0 ? 0 : 1000);
					proj.ai[1] = target.whoAmI;
					projectile.ai[1] = 80;
					(proj.modProjectile as VitricSummonHammer).moltenglowanim = moltenglowanim;
					proj.netUpdate = true;
					didattack = true;
				}

				if (attack == (int)WeaponType.Sword && targetdistance < 80)
				{
					Projectile proj = Main.projectile[Projectile.NewProjectile(projectile.Center, projectile.velocity * 10.50f, ModContent.ProjectileType<VitricSummonSword>(), projectile.damage, projectile.knockBack + 1f, projectile.owner)];
					proj.ai[0] = projectile.rotation + (projectile.spriteDirection > 0 ? 0 : 1000);
					proj.ai[1] = target.whoAmI;
					projectile.ai[1] = 80;
					(proj.modProjectile as VitricSummonHammer).moltenglowanim = moltenglowanim;
					proj.netUpdate = true;
					didattack = true;
				}

				if (attack == (int)WeaponType.Knives && targetdistance < 300)
				{
					int index = 0;
					foreach (VKnife knife in knives)
					{
						Projectile proj = Main.projectile[Projectile.NewProjectile(projectile.Center + knife.pos, projectile.velocity * 2, ModContent.ProjectileType<VitricSummonKnife>(), projectile.damage, projectile.knockBack, projectile.owner)];
						proj.ai[0] = (projectile.rotation + knife.rotation) + (projectile.spriteDirection > 0 ? 0 : 1000);
						proj.ai[1] = target.whoAmI;
						(proj.modProjectile as VitricSummonKnife).offset = new Vector2(0, -10 + (index * 10));
						projectile.ai[1] = 80;
						(proj.modProjectile as VitricSummonHammer).moltenglowanim = moltenglowanim;
						proj.netUpdate = true;
						index += 1;
						didattack = true;
					}
				}

				if (attack == (int)WeaponType.Javelin && targetdistance < 300)
				{
					Projectile proj = Main.projectile[Projectile.NewProjectile(projectile.Center, projectile.velocity * -5f, ModContent.ProjectileType<VitricSummonJavelin>(), projectile.damage, projectile.knockBack, projectile.owner)];
					proj.ai[0] = (projectile.rotation) + (projectile.spriteDirection > 0 ? 0 : 1000);
					proj.ai[1] = target.whoAmI;
					projectile.ai[1] = 80;
					(proj.modProjectile as VitricSummonHammer).moltenglowanim = moltenglowanim;
					proj.netUpdate = true;
					didattack = true;
				}

				if (didattack)
					projectile.netUpdate = true;
			}
		}

	}

	public class VitricSummonJavelin : VitricSummonHammer
	{
		internal Vector2 offset;

		public VitricSummonJavelin()
		{
			strikewhere = projectile.Center;
			enemysize = Vector2.One;
			Vector2 offset = Vector2.Zero;
		}

        public override string Texture => "StarlightRiver/NPCs/Boss/VitricBoss/CrystalWave"; //why were these before the ctor?

        public override bool CanDamage() => offset.X > 0;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Enchanted Vitric Weapons");
            Main.projFrames[projectile.type] = 1;
            ProjectileID.Sets.Homing[projectile.type] = true;
        }

        public sealed override void SetDefaults()
        {
            projectile.width = 24;
            projectile.height = 24;
            projectile.tileCollide = false;
            projectile.friendly = true;
            projectile.hostile = false;
            projectile.minion = true;
            projectile.penetrate = 100;
            projectile.timeLeft = 60;
            projectile.extraUpdates = 3;
        }

		public override void DoAI()
		{
			Player player = Main.player[projectile.owner];
			oldhitbox = new Vector2(projectile.width, projectile.height);
			projectile.tileCollide = offset.X > 0;

			if (projectile.localAI[0] > 1000)
				projectile.Kill();

			if (projectile.localAI[0] == 1)
			{
				projectile.localAI[1] = 1;
				projectile.rotation = projectile.ai[0];
				projectile.spriteDirection = projectile.rotation > 500 ? -1 : 1;

				if (projectile.rotation > 500)
					projectile.rotation -= 1000;

				projectile.netUpdate = true;
			}

			if (enemy != null && enemy.active)
			{
				strikewhere = enemy.Center + enemy.velocity*4;
				enemysize = new Vector2(enemy.width, enemy.height);
			}

			if (offset.X < 1)
			{
				Vector2 gothere = projectile.Center;
				Vector2 aimvector = strikewhere - projectile.Center;
				float animlerp = Math.Min(projectile.localAI[0] / 200f, 1f);
				float turnto = aimvector.ToRotation();

				if (projectile.localAI[0] < 200)
				{
					gothere = strikewhere - new Vector2(projectile.spriteDirection * 96, 0);
					projectile.velocity += ((gothere - projectile.Center) / 80f) * animlerp;
					projectile.velocity *= 0.65f;
				}
				else
				{
					projectile.velocity -= (projectile.rotation * projectile.spriteDirection).ToRotationVector2()* 0.40f * projectile.spriteDirection;
					projectile.velocity *= 0.95f/(1f+ (projectile.localAI[0] - 200f)/150f);
				}

				projectile.rotation = projectile.rotation.AngleTowards(turnto * projectile.spriteDirection + (projectile.spriteDirection < 0 ? (float)Math.PI : 0), animlerp * (projectile.localAI[0]<200 ? 0.01f : 0.005f));

				if ((int)projectile.localAI[0] == 400)
				{
					for (float num315 = 0.75f; num315 < 8; num315 += 2f)
					{
						for (float i = -80; i < 41; i += 8f)
						{
							float angle = MathHelper.ToRadians(-Main.rand.Next(-10, 10));
							Vector2 velo = Vector2.Normalize((((projectile.rotation) + angle) + (float)Math.PI).ToRotationVector2()) * ((i + 82f) / 40f);
							Dust.NewDustPerfect(projectile.Center + Vector2.Normalize(projectile.velocity)*(i * 1.5f) + new Vector2(Main.rand.NextFloat(-10, 10), Main.rand.NextFloat(-6, 6)), DustID.LavaMoss, //local varrsss
								new Vector2(velo.X*projectile.spriteDirection, velo.Y), 100, default, num315 / 2f);
						}
					}

					offset.X = 1;
					projectile.velocity = (projectile.rotation * projectile.spriteDirection).ToRotationVector2() * 10f * projectile.spriteDirection;
					Main.PlaySound(SoundID.Item, (int)projectile.Center.X, (int)projectile.Center.Y, 75, 0.75f, -0.50f);

				}
			}

		}

		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
		{
			projectile.penetrate -= 10; //10?
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			projectile.penetrate -= 1;
			projectile.velocity = oldVelocity;
			return projectile.penetrate < 1;
		}

		public override bool PreKill(int timeLeft)
		{
			int dusttype = DustType<Dusts.Glass2>();
			for (float num315 = 0.75f; num315 < 8; num315 += 2f)
			{
				for (float i = -80; i < 41; i += 8f)
				{
					float angle = MathHelper.ToRadians(-Main.rand.Next(-30, 30));
					Vector2 velo = projectile.rotation.ToRotationVector2()* num315;
					Dust.NewDustPerfect(projectile.Center + Vector2.Normalize(projectile.velocity) * (i * 1.5f) + new Vector2(Main.rand.NextFloat(-10, 10), Main.rand.NextFloat(-20, -8)), dusttype,
						new Vector2(velo.X * projectile.spriteDirection, velo.Y), 100, default, num315 / 2f);
				}
			}

			Main.PlaySound(SoundID.Shatter, (int)projectile.Center.X, (int)projectile.Center.Y, 0, 0.75f);
			return true;
		}

		public override void Draw(SpriteBatch spriteBatch, Vector2 drawpos, Color lightColor,float aimframe)
		{
			Texture2D tex = GetTexture("StarlightRiver/Projectiles/WeaponProjectiles/Summons/Weapon4");

			Vector2 drawOrigin = new Vector2(tex.Width/2f,tex.Height)/2f;
			Rectangle Rect = VitricSummonOrb.WhiteFrame(tex.Size().ToRectangle(), false);
			Rectangle Rect2 = VitricSummonOrb.WhiteFrame(tex.Size().ToRectangle(), true);
			float rotoffset = projectile.rotation + MathHelper.ToRadians(90f);
			float themath = Math.Min((projectile.localAI[0] - 200f) / 300f, 1f);
			spriteBatch.Draw(tex, drawpos - Main.screenPosition, Rect, lightColor, rotoffset * projectile.spriteDirection, drawOrigin, projectile.scale, projectile.spriteDirection > 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0f);
			spriteBatch.Draw(tex, drawpos - Main.screenPosition, Rect2, VitricSummonOrb.MoltenGlow(moltenglowanim), rotoffset * projectile.spriteDirection, drawOrigin, projectile.scale, projectile.spriteDirection > 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0f);
		}
	}

	public class VitricSummonKnife : VitricSummonHammer
	{
		private bool closetoplayer = false;
		internal Vector2 offset;

        public override string Texture => "StarlightRiver/NPCs/Boss/VitricBoss/CrystalWave";

        public override bool CanDamage() => offset.X > 0;

        public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Enchanted Vitric Weapons");
			Main.projFrames[projectile.type] = 1;
			ProjectileID.Sets.Homing[projectile.type] = true;
		}

		public sealed override void SetDefaults()
		{
			projectile.width = 24;
			projectile.height = 24;
			projectile.tileCollide = false;
			projectile.friendly = true;
			projectile.hostile = false;
			projectile.minion = true;
			projectile.penetrate = 1;
			projectile.timeLeft = 60;
			projectile.extraUpdates = 1;
		}

		public override void SendExtraAI(BinaryWriter writer)
		{
			writer.Write((int)offset.X);
			writer.Write((int)offset.Y);
		}

		public override void ReceiveExtraAI(BinaryReader reader)
		{
			offset.X = reader.ReadInt32();
			offset.Y = reader.ReadInt32();
		}

		public VitricSummonKnife()
		{
			strikewhere = projectile.Center;
			enemysize = Vector2.One;
		}

		public override void DoAI()
		{
			Player player = projectile.Owner();
			oldhitbox = new Vector2(projectile.width, projectile.height);
			projectile.tileCollide = offset.X>0;

			if (projectile.localAI[0] > 600)
				projectile.Kill();

			if (projectile.localAI[0] == 1)
			{
				projectile.localAI[1] = 1;
				projectile.rotation = projectile.ai[0];
				projectile.spriteDirection = projectile.rotation > 500 ? -1 : 1;

				if (projectile.rotation > 500)
					projectile.rotation -= 1000;

				projectile.ai[0] = Main.rand.NextFloat(MathHelper.ToRadians(-20), MathHelper.ToRadians(20));

				if (player.Distance(projectile.Center) < 96)
					closetoplayer = true;

				projectile.netUpdate = true;
			}

			if (Helper.IsTargetValid(enemy))
			{ 
				strikewhere = enemy.Center + new Vector2(enemy.velocity.X * 4, enemy.velocity.Y*4);
				enemysize = new Vector2(enemy.width, enemy.height);
			}

			Vector2 aimvector = strikewhere - projectile.Center;
			float turnto = aimvector.ToRotation();

			if (offset.X < 1)
			{
				Vector2 gothere;
				float animlerp = Math.Min(projectile.localAI[0] / 40f, 1f);

				if (closetoplayer)
				{
					gothere = player.Center - new Vector2(player.direction * 32, 72) + (offset*3f);
					projectile.velocity += ((gothere - projectile.Center) / 30f) * animlerp;
					projectile.velocity *= 0.65f;
				}
				else
				{
					projectile.velocity -= Vector2.Normalize(strikewhere - projectile.Center).RotatedBy(MathHelper.ToRadians(offset.Y*9f*projectile.spriteDirection)) * animlerp*0.10f;
					projectile.velocity *= 0.92f;
				}

				projectile.rotation = projectile.rotation.AngleTowards(turnto* projectile.spriteDirection + (projectile.spriteDirection < 0 ? (float)Math.PI : 0), animlerp * 0.04f);

				if ((int)projectile.localAI[0] == 120 + (int)offset.Y)
				{
					offset.X = 1;
					projectile.velocity = (projectile.rotation*projectile.spriteDirection).ToRotationVector2() * 10f*projectile.spriteDirection;
					Main.PlaySound(SoundID.Item, (int)projectile.position.X, (int)projectile.position.Y, 1, 0.75f,-0.5f);
					projectile.localAI[0] = 300;
				}

			}
			else
			{
				projectile.rotation = projectile.rotation.AngleTowards(turnto * projectile.spriteDirection + (projectile.spriteDirection < 0 ? (float)Math.PI : 0), 0.04f/(1f+(projectile.localAI[0]-300f)/60f));
				projectile.velocity = (projectile.rotation * projectile.spriteDirection).ToRotationVector2() * projectile.velocity.Length() * projectile.spriteDirection;
			}

		}

		public override bool PreKill(int timeLeft)
		{
			int dusttype = DustType<Dusts.Glass2>(); //use the generics my dude.

			for (float num315 = 0.75f; num315 < 5; num315 += 0.4f)
			{
				float angle = MathHelper.ToRadians(-Main.rand.Next(-30, 30));
				Vector2 vari = new Vector2(Main.rand.NextFloat(-2f, 2), Main.rand.NextFloat(-2f, 2));
				Dust.NewDustPerfect(projectile.position+new Vector2(Main.rand.NextFloat(projectile.width), Main.rand.NextFloat(projectile.width)),mod.DustType("Glass2"),((projectile.velocity+vari)/num315).RotatedBy(angle),100,default, num315/4f);
			}

			Main.PlaySound(SoundID.Item, (int)projectile.Center.X, (int)projectile.Center.Y, 27, 0.75f);
			return true;
		}

        public override void Draw(SpriteBatch spriteBatch, Vector2 drawpos, Color lightColor, float aimframe)
        {
            Texture2D tex = ModContent.GetTexture("StarlightRiver/Projectiles/WeaponProjectiles/Summons/Weapon3");

            Vector2 drawOrigin = new Vector2(tex.Width / 2, tex.Height) / 2f;
            float rotoffset = projectile.rotation + MathHelper.ToRadians(45f);
            spriteBatch.Draw(tex, drawpos - Main.screenPosition, VitricSummonOrb.WhiteFrame(tex.Size().ToRectangle(), false), lightColor, rotoffset * projectile.spriteDirection, drawOrigin, projectile.scale, projectile.spriteDirection > 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0f);
            spriteBatch.Draw(tex, drawpos - Main.screenPosition, VitricSummonOrb.WhiteFrame(tex.Size().ToRectangle(), true), VitricSummonOrb.MoltenGlow(moltenglowanim), rotoffset * projectile.spriteDirection, drawOrigin, projectile.scale, projectile.spriteDirection > 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0f);
        }
	}

	public class VitricSummonSword : VitricSummonHammer
	{
        private bool dodamage = false; //wat

        public VitricSummonSword()
        {
            strikewhere = projectile.Center;
            enemysize = Vector2.One;
        }

        public override string Texture => "StarlightRiver/NPCs/Boss/VitricBoss/CrystalWave";

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

		public override void DoAI()
		{

			oldhitbox = new Vector2(projectile.width, projectile.height);

			if (projectile.localAI[0] > 170)
				projectile.Kill();

			dodamage = false;

			if (projectile.localAI[0] == 1)
			{
				projectile.localAI[1] = 1;
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

			Vector2 gothere;

			if (projectile.localAI[0] < 30)//Prepare to swing
			{
				if (projectile.localAI[0] == 15)
					projectile.localAI[1] = 0;

				float lerpval = Math.Min(projectile.localAI[0] / 10f, 1f);
				projectile.rotation = projectile.rotation.AngleLerp(MathHelper.ToRadians(20), 0.075f * lerpval);
				gothere = (strikewhere + new Vector2(projectile.spriteDirection * -((72 - projectile.localAI[0]) + (enemysize.X / 2f)), projectile.localAI[0] * 3));
				projectile.velocity += ((gothere - projectile.Center) / 100f);

				if (projectile.velocity.Length() > 2f + (10f * lerpval))
					projectile.velocity = Vector2.Normalize(projectile.velocity) * (2f + (10f * lerpval));

				projectile.velocity /= 1f + (0.10f * lerpval);
			}

			if (projectile.localAI[0] < 70 && projectile.localAI[0] >= 30)//Upper Cut swing
			{
				if (projectile.localAI[0] == 36)
					projectile.localAI[1] = 1;

				if (projectile.localAI[0] == 42)
				{
					dodamage = true;
					projectile.localAI[1] = 2;
					Main.PlaySound(SoundID.Item, (int)projectile.position.X, (int)projectile.position.Y, 7, 0.75f);
				}

				if (projectile.localAI[0] == 50)
					projectile.localAI[1] = 3;

				float offset = (60 - projectile.localAI[0]) * 2f + (enemysize.X / 10f);
				float lerpval = Math.Min((projectile.localAI[0] - 30) / 10f, 1f);
				projectile.rotation = projectile.rotation.AngleLerp(MathHelper.ToRadians(-80), 0.075f * lerpval);
				gothere = (strikewhere + new Vector2(projectile.spriteDirection * (-32 + offset), -64));
				projectile.velocity += ((gothere - projectile.Center) / 50f);

				if (projectile.velocity.Length() > 14f * lerpval)
					projectile.velocity = Vector2.Normalize(projectile.velocity) * 14 * lerpval;

				projectile.velocity /= 1f + (0.5f * lerpval);
			}

			if (projectile.localAI[0] < 100 && projectile.localAI[0] >= 70)//Down Slash
			{
				if (projectile.localAI[0] == 82)
					projectile.localAI[1] = 3;

				if (projectile.localAI[0] == 86)
				{
					dodamage = true;
					projectile.localAI[1] = 2;
					Main.PlaySound(SoundID.Item, (int)projectile.position.X, (int)projectile.position.Y, 7, 0.75f);
				}

                //format like this when things line up
				if (projectile.localAI[0] == 90) projectile.localAI[1] = 2;
				if (projectile.localAI[0] == 98) projectile.localAI[1] = 1;
				if (projectile.localAI[0] == 114) projectile.localAI[1] = 0;


				float offset = (80 - projectile.localAI[0]) * 12f;
				float lerpval = Math.Min((projectile.localAI[0] - 70) / 10f, 1f);
				projectile.rotation = projectile.rotation.AngleLerp(MathHelper.ToRadians(80), 0.06f * lerpval);
				gothere = (strikewhere + new Vector2(projectile.spriteDirection * (-36 + offset - (enemysize.X / 2f)), 72));
				projectile.velocity += ((gothere - projectile.Center) / 50f);

				if (projectile.velocity.Length() > 14f * lerpval)
					projectile.velocity = Vector2.Normalize(projectile.velocity) * 14 * lerpval;

				projectile.velocity /= 1f + (0.5f * lerpval);
			}

			if (projectile.localAI[0] < 200 && projectile.localAI[0] >= 100)//Big Upper Cut swing
			{
				if (projectile.localAI[0] == 136)
					projectile.localAI[1] = 1;

				if (projectile.localAI[0] == 142)
				{
					dodamage = true;
					projectile.localAI[1] = 2;
					Main.PlaySound(SoundID.Item, (int)projectile.position.X, (int)projectile.position.Y, 7, 0.75f);
				}

				if (projectile.localAI[0] == 150)
					projectile.localAI[1] = 3;

				float lerpval = Math.Min((projectile.localAI[0] - 100) / 40f, 1f);

				if (projectile.localAI[0] < 130)
				{
					projectile.rotation = projectile.rotation.AngleLerp(MathHelper.ToRadians(100), 0.075f * lerpval);
					gothere = (strikewhere + new Vector2(projectile.spriteDirection * (-96 - (enemysize.X / 2f)), 70));
				}
				else
				{
					float offset = (150 - projectile.localAI[0]) * (6f + enemysize.X / 3f);
					projectile.rotation = projectile.rotation.AngleLerp(MathHelper.ToRadians(-80), 0.075f * lerpval);
					gothere = (strikewhere + new Vector2(projectile.spriteDirection * ((-32 + offset) - (enemysize.X / 2f)), -160));

					if (projectile.localAI[0] > 150)
					{
						for (float num315 = 1f; num315 < 3; num315 += 0.5f)
						{
							float angle = MathHelper.ToRadians(Main.rand.Next(-40, 40));
							Dust num316 = Dust.NewDustPerfect(projectile.Center + new Vector2(Main.rand.Next(40), Main.rand.Next(60) - 30), mod.DustType("Glass2"), ((projectile.velocity * num315) * 0.25f).RotatedBy(angle), (int)(((projectile.localAI[0]-150f)/20f)*255f), default, (40f - num315) / 40f);
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
			return base.Colliding(new Rectangle((int)projectile.Center.X-12, -16+(int)projectile.position.Y - hitboxframe[(int)projectile.localAI[1]], projectile.width+24,projectile.height+32), targetHitbox);
		}

		public override void Draw(SpriteBatch spriteBatch, Vector2 drawpos, Color lightColor, float aimframe)
		{
			Texture2D tex = ModContent.GetTexture("StarlightRiver/Projectiles/WeaponProjectiles/Summons/Weapon2");

			Vector2 pos = VitricSummonOrb.swordlocs[(int)projectile.localAI[1]];
			Vector2 drawOrigin = new Vector2((projectile.spriteDirection < 0 ? tex.Width - pos.X : pos.X)/2f, pos.Y);
			Vector2 drawPos = ((drawpos - Main.screenPosition));
			Color color = lightColor*Math.Min(1f,1f-((projectile.localAI[0]-140f)/30f));
			spriteBatch.Draw(tex, drawPos, VitricSummonOrb.WhiteFrame(new Rectangle(0, (int)projectile.localAI[1]*(tex.Height / 4), tex.Width,tex.Height/4),false), color, projectile.rotation * projectile.spriteDirection, drawOrigin, projectile.scale, projectile.spriteDirection > 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0f);
			spriteBatch.Draw(tex, drawPos, VitricSummonOrb.WhiteFrame(new Rectangle(0, (int)projectile.localAI[1]*(tex.Height / 4), tex.Width,tex.Height/4),true), VitricSummonOrb.MoltenGlow(moltenglowanim), projectile.rotation * projectile.spriteDirection, drawOrigin, projectile.scale, projectile.spriteDirection > 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0f);
		}
	}

	public class VitricSummonHammer : ModProjectile
	{
		protected Vector2 strikewhere;
		protected Vector2 enemysize;
		protected Player player;
		protected NPC enemy;
		protected Vector2 oldhitbox;
		internal float moltenglowanim = 0f;

        public VitricSummonHammer()
        {
            strikewhere = projectile.Center;
            enemysize = Vector2.One;
        }

        public override string Texture => "StarlightRiver/NPCs/Boss/VitricBoss/CrystalWave";

        public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Enchanted Vitric Weapons");
			Main.projFrames[projectile.type] = 1;
			ProjectileID.Sets.Homing[projectile.type] = true;
		}

		public override void SetDefaults()
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

		public override bool CanDamage() => projectile.localAI[0]>60;

		public override void AI()
		{

			player = projectile.Owner();

			if (player.HasBuff(ModContent.BuffType<VitricSummonBuff>()))
				projectile.timeLeft = 2;

			projectile.localAI[0] += 1;
			enemy = Main.npc[(int)projectile.ai[1]];
			float cooloffrate = (GetType() == typeof(VitricSummonSword) ? 1.50f : (GetType() == typeof(VitricSummonKnife) ? 2f : 1f));
			 moltenglowanim += cooloffrate / (1f+(float)projectile.extraUpdates);

			DoAI();
		}

		public virtual void DoAI()
		{
			if (projectile.localAI[0] > 300)
			{
				projectile.Kill();
				return;
			}

			oldhitbox = new Vector2(projectile.width, projectile.height);

			if (projectile.localAI[0] == 1)
			{
				projectile.rotation = projectile.ai[0];
				projectile.spriteDirection = projectile.rotation > 500 ? -1 : 1;

				if (projectile.rotation > 500)
					projectile.rotation -= 1000;

				projectile.ai[0] = 0;
				projectile.netUpdate = true;
			}

			Vector2 gothere;
			if (projectile.localAI[0] < 70)//Swing up
			{
				float lerpval = Math.Min(projectile.localAI[0] / 50f, 1f);
				if (Helper.IsTargetValid(enemy))
				{
					strikewhere = enemy.Center+new Vector2(enemy.velocity.X, enemy.velocity.Y/2f);
					enemysize = new Vector2(enemy.width, enemy.height);
				}

				projectile.rotation = projectile.rotation.AngleLerp(-MathHelper.Pi/2f, 0.075f* lerpval);
				gothere = (strikewhere + new Vector2(projectile.spriteDirection * -(75+ (float)Math.Pow(projectile.localAI[0]*2f,0.80) + enemysize.X/2f), -200));
				projectile.velocity += ((gothere-projectile.Center)/75f);

				if (projectile.velocity.Length() > 14f * lerpval)
					projectile.velocity = Vector2.Normalize(projectile.velocity) * 14 * lerpval;

				projectile.velocity /= 1.5f;
			}

			if (projectile.localAI[0] >= 70)//Swing Down
			{
				if (Helper.IsTargetValid(enemy))
					strikewhere.X = enemy.Center.X + enemy.velocity.X*1.50f;

				projectile.velocity.X += Math.Min(Math.Abs(projectile.velocity.X),0) * projectile.spriteDirection;

				float lerpval = Math.Min((projectile.localAI[0]-70f) / 30f, 1f);

				projectile.rotation = projectile.rotation.AngleTowards(MathHelper.ToRadians(45), 0.075f* lerpval);
				gothere = (strikewhere + new Vector2(projectile.spriteDirection * -(32 + enemysize.X / 4f), -32));
				projectile.velocity.X += (MathHelper.Clamp(gothere.X - projectile.Center.X,-80f,80f)) / 24f;
				projectile.velocity.Y += 1f;

				if (projectile.velocity.Length() > 10* lerpval)
					projectile.velocity = Vector2.Normalize(projectile.velocity) * 10* lerpval;

				projectile.velocity.X /= 1.20f;

				if (projectile.Center.Y > gothere.Y)//Smashing!
				{
					Point16 point = new Point16((int)((projectile.Center.X+ (projectile.width/3f)*projectile.spriteDirection) / 16), Math.Min(Main.maxTilesY, (int)((projectile.Center.Y) / 16) + 1));
					Tile tile = Framing.GetTileSafely(point.X, point.Y);

                    //hard coded dust ids in worldgen.cs, still ew
                    //Tile hit!
                    if (tile != null && WorldGen.InWorld(point.X, point.Y, 1) && tile.active() && Main.tileSolid[tile.type])
                    {
                        projectile.localAI[0] = 301;
                        int dusttype = mod.DustType("Glass2");
                        DustHelper.TileDust(tile, ref dusttype);

                        Projectile.NewProjectile(new Vector2(point.X, point.Y - 1) * 16, Vector2.Zero, ModContent.ProjectileType<ShockwaveSummon>(), (int)(projectile.damage * 0.25), 0, Main.myPlayer, (int)tile.type, 16 * projectile.spriteDirection);
                        Main.LocalPlayer.GetModPlayer<StarlightPlayer>().Shake += 10;

                        for (float num315 = 2f; num315 < 15; num315 += 0.50f)
                        {
                            float angle = MathHelper.ToRadians(-Main.rand.Next(70, 130));
                            Vector2 vecangle = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * num315 * 3f;
                            int num316 = Dust.NewDust(new Vector2(projectile.position.X + (projectile.spriteDirection * (int)(projectile.width * 0.60)), projectile.Center.Y - projectile.height / 2f), projectile.width / 2, projectile.height, dusttype, 0f, 0f, 50, default, (12f - num315) / 5f);
                            Main.dust[num316].noGravity = true;
                            Main.dust[num316].velocity = vecangle;
                            Main.dust[num316].fadeIn = 0.25f;
                        }

                        projectile.height += 32; projectile.position.Y -= 16;
                        projectile.width += 40; projectile.position.X -= 20;
                    }

				}

			}
		}

		public override bool PreKill(int timeLeft)
		{
			int dusttype = mod.DustType("Glass2");

			for (float num315 = 2f; num315 < 12; num315 += 0.25f)
			{
				float angle = MathHelper.ToRadians(-Main.rand.Next(40, 140));
				Vector2 vecangle = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * num315;
				int num316 = Dust.NewDust(new Vector2(projectile.Center.X+(projectile.spriteDirection<0 ? -oldhitbox.X : 0), projectile.Center.Y- oldhitbox.Y), (int)oldhitbox.X, (int)oldhitbox.Y*2, dusttype, 0f, 0f, 50, default, (40f - num315) / 40f);
				Main.dust[num316].noGravity = true;
				Main.dust[num316].velocity = vecangle;
				Main.dust[num316].fadeIn = 0.75f;
			}

			Main.PlaySound(SoundID.Shatter, projectile.Center);
			return true;
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
		{
			Vector2 pos = projectile.Center;

			for (float xx = Math.Min(1f,(projectile.velocity.Length() - 4f) / 2f); xx > 0 ; xx -= 0.10f)
			{
				Vector2 drawPos = (pos) - ((projectile.velocity*6f) * xx);
				Draw(spriteBatch, drawPos, lightColor* (1f-xx)*0.5f,xx);
			}

			Draw(spriteBatch, projectile.Center, lightColor,0);
			return false;
		}

        public virtual void Draw(SpriteBatch spriteBatch, Vector2 drawpos, Color lightColor, float aimframe)
        {
            Texture2D tex = ModContent.GetTexture("StarlightRiver/Projectiles/WeaponProjectiles/Summons/Weapon1");

            Vector2 drawOrigin = new Vector2(tex.Width / 2f, tex.Height) / 2f;
            Vector2 drawPos = ((drawpos - Main.screenPosition));
            Color color = lightColor;
            spriteBatch.Draw(tex, drawPos, VitricSummonOrb.WhiteFrame(tex.Size().ToRectangle(), false), color, projectile.rotation * projectile.spriteDirection, drawOrigin, projectile.scale, projectile.spriteDirection > 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0f);
            spriteBatch.Draw(tex, drawPos, VitricSummonOrb.WhiteFrame(tex.Size().ToRectangle(), true), VitricSummonOrb.MoltenGlow(moltenglowanim), projectile.rotation * projectile.spriteDirection, drawOrigin, projectile.scale, projectile.spriteDirection > 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0f);
        }
	}

	class ShockwaveSummon : NPCs.Miniboss.Glassweaver.Shockwave
	{
		public override string Texture => "StarlightRiver/Tiles/Vitric/Blocks/AncientSandstone";

		public override void SetStaticDefaults() => DisplayName.SetDefault("Shockwave");
		private int TileType => (int)projectile.ai[0];
		private int ShockwavesLeft => (int)projectile.ai[1];//Positive and Negitive

		public override void SetDefaults()
		{
			base.SetDefaults();
			projectile.hostile = false;
			projectile.friendly = true;
			projectile.minion = true;
			projectile.timeLeft = 1060;
			projectile.tileCollide = true;
			projectile.width = 16;
			projectile.height = 16;
			projectile.idStaticNPCHitCooldown = 20;
			projectile.usesIDStaticNPCImmunity = true;
		}

		public override void AI()
		{
			if (projectile.timeLeft > 1000)
			{
				if (projectile.timeLeft < 1002 && projectile.timeLeft > 80)
					projectile.Kill();

				projectile.velocity.Y = 4f;
			}
			else
			{
				projectile.velocity.Y = projectile.timeLeft <= 10 ? 1f : -1f;

				if (projectile.timeLeft == 19 && Math.Abs(ShockwavesLeft) > 0) //what the fuck is this. Local vars. please.
					Projectile.NewProjectile(new Vector2(((int)projectile.Center.X / 16) * 16 + 16*Math.Sign(ShockwavesLeft)
					, (((int)projectile.Center.Y / 16) * 16) - 32),
					Vector2.Zero, projectile.type, projectile.damage, 0, Main.myPlayer, TileType, projectile.ai[1] - Math.Sign(ShockwavesLeft));

			}
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
		{
			if (projectile.timeLeft < 21)
				spriteBatch.Draw(Main.tileTexture[TileType], projectile.position - Main.screenPosition, new Rectangle(18, 0, 16, 16), lightColor);

			return false;
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			if (projectile.timeLeft > 800) //smells like boilerplate. IDG-it kinda is
			{
				Point16 point = new Point16((int)((projectile.Center.X + (projectile.width / 3f) * projectile.spriteDirection) / 16), Math.Min(Main.maxTilesY, (int)((projectile.Center.Y) / 16) + 1));
				Tile tile = Framing.GetTileSafely(point.X, point.Y);

				//hard coded dust ids in worldgen.cs, still ew
				//Tile hit!
				if (tile != null && WorldGen.InWorld(point.X, point.Y, 1) && tile.active() && Main.tileSolid[tile.type])
				{
					projectile.timeLeft = 20;
					projectile.ai[0] = tile.type;
					projectile.tileCollide = false;
					projectile.position.Y += 16;

					int dusttype = 0;

					DustHelper.TileDust(tile, ref dusttype);

					for (float num315 = 0.50f; num315 < 3; num315 += 0.25f)
					{
						float angle = MathHelper.ToRadians(-Main.rand.Next(70, 130));
						Vector2 vecangle = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * num315 * 3f;
						int num316 = Dust.NewDust(new Vector2(projectile.position.X, projectile.position.Y), projectile.width, (int)(projectile.height/2f), dusttype, 0f, 0f, 50, default, (4f - num315) / 3f);
						Main.dust[num316].noGravity = true;
						Main.dust[num316].velocity = vecangle*2f;
						Main.dust[num316].fadeIn = 0.25f;
					}
				}
			}
			return false;
		}
	}
}

/*Concluding comments:
 * - Please try to minimize the amount of vars you use in projectiles, use the ai fields when possible. You also did not implement any netsync for the ones you did use. that needs to be done.
 * - please use local vars over splitting a call into multiple lines, its very hard to read and looks abhorrently messy.
 * - some of your var names are very nondescriptive, like "us". use VS rename tool to change this to something more appropriate for what it represents
 * - fields => properties => ctor => lambda methods => lambda overrides => overrides and methods. please.
 * - remove redundant parenthesis once you're done thinking through something, and please actually make an attempt to format things consistently. put whitespace lines between methods, before conditional statements, etc.
 * - note that you can also re-type the closing parenthesis on a statement or definition to have VS auto-format the tabulation for you. You had alot of incorrect tabulation.
 * - you have adapted vanilla code where you didnt re-name the local vars or comment that it was adapted vanilla code. dont do that.
 * - each weapon here belongs in it's own file. a 1.1k line long file is not OK.
 */

    