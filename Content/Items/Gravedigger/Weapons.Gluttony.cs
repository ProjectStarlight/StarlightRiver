using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Buffs;
using StarlightRiver.Content.Dusts;
using StarlightRiver.Core;
using StarlightRiver.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Graphics.Effects;
using Terraria.DataStructures;
using Terraria.GameContent;

namespace StarlightRiver.Content.Items.Gravedigger
{
	public class Gluttony : ModItem
	{
		public override string Texture => AssetDirectory.GravediggerItem + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Gluttony");
			Tooltip.SetDefault("Suck the soul out of your enemies!");
		}

		public override void SetDefaults()
		{
			Item.width = 24;
			Item.height = 28;
			Item.useTurn = true;
			Item.autoReuse = true;
			Item.value = Item.buyPrice(0, 6, 0, 0);
			Item.rare = ItemRarityID.Green;
			Item.damage = 20;
			Item.mana = 9;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.useTime = 10;
			Item.useAnimation = 10;
			Item.DamageType = DamageClass.Magic;
			Item.channel = true;
			Item.noMelee = true;
			Item.shoot = ModContent.ProjectileType<GluttonyHandle>();
			Item.shootSpeed = 0f;
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			return !Main.projectile.Any(n => n.active && n.owner == player.whoAmI && n.type == ModContent.ProjectileType<GluttonyHandle>());
		}
	}

	public class GluttonyHandle : ModProjectile, IDrawAdditive, IDrawPrimitive
	{
		public override string Texture => AssetDirectory.GravediggerItem + Name;

		public Vector2 direction = Vector2.Zero;

		public List<NPC> targets = new List<NPC>();

		private List<Vector2> cache;
		private List<Vector2> cache2;
		private List<Vector2> cache3;
		private List<Vector2> cache4;
		private List<Vector2> cache5;

		private Trail trail;
		private Trail trail2;
		private Trail trail3;
		private Trail trail4;
		private Trail trail5;

		float damageDone = 0;

		int timer;

		bool released = false;

		const int RANGE = 300;
		const float CONE = 0.7f;
		public const int DPS = 40;

		const int TRAILLENGTH = 15;
		const float CIRCLES = 0.5f;
		const int STARTRAD = 5;
		const int ENDRAD = 100;
		const float SQUISH = 0.35f;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Gluttony");
		}

		public override void SetDefaults()
		{
			Projectile.width = 14;
			Projectile.height = 18;
			Projectile.friendly = false;
			Projectile.hostile = false;
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.DamageType = DamageClass.Magic;
			Projectile.ignoreWater = true;
		}

        public override void AI()
        {
			timer++;

			Player Player = Main.player[Projectile.owner];
			Projectile.damage = (int)(Player.HeldItem.damage * Player.GetDamage(DamageClass.Magic).Multiplicative);

			direction = Main.MouseWorld - (Player.Center);
			direction.Normalize();

			Projectile.Center = Player.Center;
			Projectile.rotation = direction.ToRotation();

			if (Player.statMana < Player.HeldItem.mana)
				released = true;

			if (Player.channel && Player.HeldItem.type == ModContent.ItemType<Gluttony>() && !released)
            {
				Player.ChangeDir(Main.MouseWorld.X > Player.position.X ? 1 : -1);

				//Player.itemTime = Player.itemAnimation = 2;
				Projectile.timeLeft = 2;

				Player.itemRotation = direction.ToRotation();

				if (Player.direction != 1)
					Player.itemRotation -= 3.14f;

				if(timer > 10 && Main.rand.Next(4) == 0)
				{
					float prog = Helper.SwoopEase(Math.Min(1, timer / 50f));
					float dustRot = Projectile.rotation + 0.1f + Main.rand.NextFloat(-0.3f, 0.3f);
					Dust.NewDustPerfect(Projectile.Center + Vector2.UnitX.RotatedBy(dustRot) * 300 * prog + new Vector2(0, 48), ModContent.DustType<Dusts.GlowLine>(), Vector2.UnitX.RotatedBy(dustRot) * Main.rand.NextFloat(-9.5f, -8f), 0, new Color(255, 40, 80) * 0.8f, 0.8f);
				}
			}

			else if (timer > 80)
			{
				timer = 79;
				Projectile.timeLeft = 2;
				released = true;
			}
			else
			{
				timer -= 3;

				if(timer > 0)
					Projectile.timeLeft = 2;
			}

			UpdateTargets(Player);
			SuckEnemies(Player);
			ManageCaches();
			ManageTrails();

		}

		private void ManageCaches()
		{
			float rot = (float)Math.PI / 2.5f;
			ManageCache(ref cache, 0);
			ManageCache(ref cache2, rot);
			ManageCache(ref cache3, rot * 2);
			ManageCache(ref cache4, rot * 3);
			ManageCache(ref cache5, rot * 4);
		}

		private void ManageCache(ref List<Vector2> localCache, float rotationStart)
        {
			localCache = new List<Vector2>();

			float rotation = (timer / 50f) + rotationStart;
			float prog = Helper.SwoopEase(Math.Min(1, timer / 80f));

			for (int i = 0; i < TRAILLENGTH; i++)
			{
				float lerper = (float)i / (float)TRAILLENGTH * prog;
				float radius = MathHelper.Lerp(STARTRAD, ENDRAD, lerper) * prog;

				float rotation2 = (lerper * 6.28f * CIRCLES) + rotation;
				Vector2 pole = Projectile.Center + (lerper * direction * RANGE);

				Vector2 pointX = direction * (float)Math.Sin(rotation2) * SQUISH;
				Vector2 pointy = direction.RotatedBy(1.57f) * (float)Math.Cos(rotation2);
				Vector2 point = (pointX + pointy) * radius;
				localCache.Add(pole + point);
			}

			while (localCache.Count > TRAILLENGTH)
			{
				localCache.RemoveAt(0);
			}
		}

		private void ManageTrails()
		{
			float rot = (float)Math.PI / 2.5f;
			ManageTrail(ref trail, ref cache, 0);
			ManageTrail(ref trail2, ref cache2, rot);
			ManageTrail(ref trail3, ref cache3, rot * 2);
			ManageTrail(ref trail4, ref cache4, rot * 3);
			ManageTrail(ref trail5, ref cache5, rot * 4);
		}

		private void ManageTrail(ref Trail localTrail, ref List<Vector2> localCache, float rotationStart)
        {
			localTrail = localTrail ?? new Trail(Main.instance.GraphicsDevice, TRAILLENGTH, new TriangularTip(1), factor => MathHelper.Lerp(10,40,factor), factor =>
			{
				float rotProg = (0.6f + (float)Math.Sin((timer / 50f) + rotationStart - 0.5f) * 0.4f);

				if (factor.X > 0.95f)
					return Color.Transparent;

				return new Color(255, 80 - (byte)(factor.X * 70), 80 + (byte)(rotProg * 20)) * rotProg;
			});
			localTrail.Positions = localCache.ToArray();
			localTrail.NextPosition = Projectile.Center + (direction * RANGE);
		}

		public void DrawPrimitives()
		{
			Effect effect = Filters.Scene["GluttonyTrail"].GetShader().Shader;

			Matrix world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
			Matrix view = Main.GameViewMatrix.ZoomMatrix;
			Matrix projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

			effect.Parameters["time"].SetValue(0.05f * Main.GameUpdateCount);
			effect.Parameters["repeats"].SetValue(3f);
			effect.Parameters["transformMatrix"].SetValue(world * view * projection);
			effect.Parameters["sampleTexture"].SetValue(ModContent.Request<Texture2D>("StarlightRiver/Assets/EnergyTrail").Value);
			effect.Parameters["sampleTexture2"].SetValue(ModContent.Request<Texture2D>("StarlightRiver/Assets/Bosses/VitricBoss/LaserBallDistort").Value);

			effect.Parameters["row"].SetValue(0);
			trail?.Render(effect);
			effect.Parameters["row"].SetValue(0.2f);
			trail2?.Render(effect);
			effect.Parameters["row"].SetValue(0.4f);
			trail3?.Render(effect);
			effect.Parameters["row"].SetValue(0.6f);
			trail4?.Render(effect);
			effect.Parameters["row"].SetValue(0.8f);
			trail5?.Render(effect);
		}

		private void UpdateTargets(Player Player)
        {
			for (int i = 0; i < Main.npc.Length; i++)
			{
				NPC NPC = Main.npc[i];
				Vector2 toNPC = NPC.Center - Player.Center;

				if (toNPC.Length() < RANGE && NPC.active && AnglesWithinCone(toNPC.ToRotation(), direction.ToRotation()) && (!NPC.townNPC || !NPC.friendly) && !NPC.immortal)
				{
					bool targetted = false;

					foreach (var NPC2 in targets)
					{
						if (NPC2.active)
						{
							if (NPC2 == NPC)
								targetted = true;
						}
					}

					if (!targetted)
						targets.Add(NPC);
				}
				else
					targets.Remove(NPC);
			}

			foreach (var NPC2 in targets.ToArray())
			{
				if (!NPC2.active)
					targets.Remove(NPC2);
			}
		}

		private void SuckEnemies(Player Player)
        {
			foreach(NPC NPC in targets)
			{
				NPC.AddBuff(ModContent.BuffType<SoulSuck>(), 2);
				damageDone += DPS / 30f;
			}

			if (damageDone > 150 && Player.ownedProjectileCounts[ModContent.ProjectileType<GluttonyGhoul>()] < 10)
            {
				damageDone = 0;
				Projectile.NewProjectile(Projectile.GetSource_FromThis(), Player.Center + (direction * 15), direction.RotatedBy(Main.rand.NextFloat(-1.57f,1.57f) + 3.14f) * 5, ModContent.ProjectileType<GluttonyGhoul>(), Projectile.damage / 2, Projectile.knockBack, Player.whoAmI);
            }

        }

		private bool AnglesWithinCone(float angle1, float angle2)
		{
			if (Math.Abs(MathHelper.WrapAngle(angle1) % 6.28f - MathHelper.WrapAngle(angle2) % 6.28f) < CONE)
				return true;
			if (Math.Abs((MathHelper.WrapAngle(angle1) % 6.28f - MathHelper.WrapAngle(angle2) % 6.28f) + 6.28f) < CONE)
				return true;
			if (Math.Abs((MathHelper.WrapAngle(angle1) % 6.28f - MathHelper.WrapAngle(angle2) % 6.28f) - 6.28f) < CONE)
				return true;
			return false;
		}

		public override bool PreDraw(ref Color lightColor)
		{
			var tex = ModContent.Request<Texture2D>("StarlightRiver/Assets/Items/Gravedigger/GluttonyBG").Value;
			float prog = Helper.SwoopEase(Math.Min(1, timer / 80f));

			var effect1 = Filters.Scene["Cyclone"].GetShader().Shader;
			effect1.Parameters["NoiseOffset"].SetValue(Vector2.One * Main.GameUpdateCount * -0.02f);
			effect1.Parameters["brightness"].SetValue(10);
			effect1.Parameters["MainScale"].SetValue(1.0f);
			effect1.Parameters["CenterPoint"].SetValue(new Vector2(0.5f, 1f));
			effect1.Parameters["TrailDirection"].SetValue(new Vector2(0, -1));
			effect1.Parameters["width"].SetValue(0.85f);
			effect1.Parameters["distort"].SetValue(0.75f);
			effect1.Parameters["Resolution"].SetValue(tex.Size());
			effect1.Parameters["mainColor"].SetValue(new Vector3(0.8f, 0.03f, 0.18f));

			effect1.Parameters["sampleTexture2"].SetValue(ModContent.Request<Texture2D>("StarlightRiver/Assets/Bosses/VitricBoss/LaserBallDistort").Value);

			Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, Color.Black * prog * 0.8f, Projectile.rotation + 1.57f + 0.1f, new Vector2(tex.Width / 2, tex.Height), prog * 0.55f, 0, 0);

			Main.spriteBatch.End();
			Main.spriteBatch.Begin(default, BlendState.Additive, default, default, default, effect1, Main.GameViewMatrix.ZoomMatrix);

			Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, new Color(220, 50, 90) * prog, Projectile.rotation + 1.57f + 0.1f, new Vector2(tex.Width / 2, tex.Height), prog * 0.55f, 0, 0);
			//spriteBatch.Draw(Terraria.GameContent.TextureAssets.MagicPixel.Value, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.White);

			Main.spriteBatch.End();
			Main.spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.ZoomMatrix);

			return false;
		}

		public void DrawAdditive(SpriteBatch spriteBatch)
		{
			float prog = Helper.SwoopEase(Math.Min(1, timer / 50f));
			var tex = ModContent.Request<Texture2D>(AssetDirectory.VitricBoss + "ConeTell").Value;
			//spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, new Color(255, 60, 80) * prog * 0.4f, Projectile.rotation + 1.57f + 0.1f, new Vector2(tex.Width / 2, tex.Height), prog * 0.55f, 0, 0);
		}
	}

	public class GluttonyGhoul : ModProjectile, IDrawPrimitive
	{
		public override string Texture => AssetDirectory.GravediggerItem + Name;

		private List<Vector2> cache;

		private Trail trail;

		const int TRAILLENGTH = 25;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Ghoul");
		}

		public override void SetDefaults()
		{
			Projectile.width = 20;
			Projectile.height = 20;
			Projectile.friendly = true;
			Projectile.minion = true;
			Projectile.minionSlots = 0;
			Projectile.penetrate = -1;
			Projectile.timeLeft = 150;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.alpha = 255;
		}

		public override void AI()
		{
			Movement();
			ManageCaches();
			ManageTrail();
		}

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
			if (target.life <= 0)
				Projectile.timeLeft = 150;
        }

        private void Movement()
		{
			NPC target = Main.npc.Where(n => n.CanBeChasedBy(Projectile, false) && Vector2.Distance(n.Center, Projectile.Center) < 800).OrderBy(n => Vector2.Distance(n.Center, Projectile.Center)).FirstOrDefault();

			if (target != default)
			{
				Vector2 direction = target.Center - Projectile.Center;
				direction.Normalize();
				direction *= 10;
				Projectile.velocity = Vector2.Lerp(Projectile.velocity, direction, 0.03f);
			}
		}

		private void ManageCaches()
		{
			if (cache == null)
			{
				cache = new List<Vector2>();
				for (int i = 0; i < TRAILLENGTH; i++)
				{
					cache.Add(Projectile.Center);
				}
			}

			cache.Add(Projectile.Center);

			while (cache.Count > TRAILLENGTH)
			{
				cache.RemoveAt(0);
			}
		}

		private void ManageTrail()
        {
			trail = trail ?? new Trail(Main.instance.GraphicsDevice, TRAILLENGTH, new TriangularTip(1), factor => 20, factor =>
			{
				return Color.Red;
			});

			trail.Positions = cache.ToArray();
			trail.NextPosition = Projectile.Center + Projectile.velocity;
		}

		public void DrawPrimitives()
		{
			Effect effect = Filters.Scene["GhoulTrail"].GetShader().Shader;

			Matrix world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
			Matrix view = Main.GameViewMatrix.ZoomMatrix;
			Matrix projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

			effect.Parameters["transformMatrix"].SetValue(world * view * projection);
			effect.Parameters["sampleTexture"].SetValue(TextureAssets.Projectile[Projectile.type].Value);

			trail?.Render(effect);
		}
	}

	public class SoulSuck : SmartBuff
	{
		public override string Texture => AssetDirectory.Debug;

		public SoulSuck() : base("Soul Suck", "You getting sucked", false) { }

		public override void Update(NPC NPC, ref int buffIndex)
		{
			if (!NPC.friendly)
			{
				if (NPC.lifeRegen > 0)
				{
					NPC.lifeRegen = 0;
				}

				NPC.lifeRegen -= GluttonyHandle.DPS;
			}
		}
	}
}