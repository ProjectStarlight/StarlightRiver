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
			item.width = 24;
			item.height = 28;
			item.useTurn = false;
			item.value = Item.buyPrice(0, 6, 0, 0);
			item.rare = ItemRarityID.Pink;
			item.damage = 34;
			item.mana = 9;
			item.useStyle = ItemUseStyleID.HoldingOut;
			item.useTime = 10;
			item.useAnimation = 7;
			//item.scale = 0.9f;
			item.reuseDelay = 5;
			item.magic = true;
			item.channel = true;
			item.noMelee = true;
			//item.noUseGraphic = true;
			item.shoot = ModContent.ProjectileType<GluttonyHandle>();
			item.shootSpeed = 0f;
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

		int damageDone = 0;

		int timer;

		const int RANGE = 300;
		const float CONE = 0.7f;
		public const int DPS = 4;

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
			projectile.width = 14;
			projectile.height = 18;
			projectile.friendly = false;
			projectile.hostile = false;
			projectile.penetrate = -1;
			projectile.tileCollide = false;
			projectile.magic = true;
			projectile.ignoreWater = true;
		}

        public override void AI()
        {
			Player player = Main.player[projectile.owner];
			projectile.damage = (int)(player.inventory[player.selectedItem].damage * player.magicDamage);
			timer++;
			direction = Main.MouseWorld - (player.Center);
			direction.Normalize();

			projectile.Center = player.Center;
			projectile.rotation = direction.ToRotation();

			if (player.channel && player.HeldItem.type == ModContent.ItemType<Gluttony>())
            {
				player.ChangeDir(Main.MouseWorld.X > player.position.X ? 1 : -1);

				player.itemTime = player.itemAnimation = 2;
				projectile.timeLeft = 2;

				player.itemRotation = direction.ToRotation();

				if (player.direction != 1)
					player.itemRotation -= 3.14f;

				if(timer > 10 && Main.rand.Next(4) == 0)
				{
					float prog = Helper.SwoopEase(Math.Min(1, timer / 50f));
					float dustRot = projectile.rotation + 0.1f + Main.rand.NextFloat(-0.3f, 0.3f);
					Dust.NewDustPerfect(projectile.Center + Vector2.UnitX.RotatedBy(dustRot) * 300 * prog + new Vector2(0, 48), ModContent.DustType<Dusts.GlowLine>(), Vector2.UnitX.RotatedBy(dustRot) * Main.rand.NextFloat(-9.5f, -8f), 0, new Color(255, 40, 80) * 0.8f, 0.8f);
				}
			}

			else if (timer > 80)
			{
				timer = 79;
				projectile.timeLeft = 2;
			}
			else
			{
				timer -= 3;

				if(timer > 0)
					projectile.timeLeft = 2;
			}

			UpdateTargets(player);
			SuckEnemies(player);
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
				Vector2 pole = projectile.Center + (lerper * direction * RANGE);

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
			localTrail.NextPosition = projectile.Center + (direction * RANGE);
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
			effect.Parameters["sampleTexture"].SetValue(ModContent.GetTexture("StarlightRiver/Assets/EnergyTrail"));
			effect.Parameters["sampleTexture2"].SetValue(ModContent.GetTexture("StarlightRiver/Assets/Bosses/VitricBoss/LaserBallDistort"));

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

		private void UpdateTargets(Player player)
        {
			for (int i = 0; i < Main.npc.Length; i++)
			{
				NPC npc = Main.npc[i];
				Vector2 toNPC = npc.Center - player.Center;

				if (toNPC.Length() < RANGE && npc.active && AnglesWithinCone(toNPC.ToRotation(), direction.ToRotation()) && (!npc.townNPC || !npc.friendly) && !npc.immortal)
				{
					bool targetted = false;

					foreach (var npc2 in targets)
					{
						if (npc2.active)
						{
							if (npc2 == npc)
								targetted = true;
						}
					}

					if (!targetted)
						targets.Add(npc);
				}
				else
					targets.Remove(npc);
			}

			foreach (var npc2 in targets.ToArray())
			{
				if (!npc2.active)
					targets.Remove(npc2);
			}
		}

		private void SuckEnemies(Player player)
        {

			foreach(NPC npc in targets)
			{
				npc.AddBuff(ModContent.BuffType<SoulSuck>(), 2);
				if (Main.GlobalTime % 30 == 0)
					damageDone += DPS;
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

		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
		{
			var tex = ModContent.GetTexture("StarlightRiver/Assets/Items/Gravedigger/GluttonyBG");
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

			effect1.Parameters["sampleTexture2"].SetValue(ModContent.GetTexture("StarlightRiver/Assets/Bosses/VitricBoss/LaserBallDistort"));

			spriteBatch.Draw(tex, projectile.Center - Main.screenPosition, null, Color.Black * prog * 0.8f, projectile.rotation + 1.57f + 0.1f, new Vector2(tex.Width / 2, tex.Height), prog * 0.55f, 0, 0);

			spriteBatch.End();
			spriteBatch.Begin(default, BlendState.Additive, default, default, default, effect1, Main.GameViewMatrix.ZoomMatrix);

			spriteBatch.Draw(tex, projectile.Center - Main.screenPosition, null, new Color(220, 50, 90) * prog * 0f, projectile.rotation + 1.57f + 0.1f, new Vector2(tex.Width / 2, tex.Height), prog * 0.55f, 0, 0);
			//spriteBatch.Draw(Main.magicPixel, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.White);

			spriteBatch.End();
			spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.ZoomMatrix);

			return false;
		}

		public void DrawAdditive(SpriteBatch spriteBatch)
		{
			float prog = Helper.SwoopEase(Math.Min(1, timer / 50f));
			var tex = ModContent.GetTexture(AssetDirectory.VitricBoss + "ConeTell");
			//spriteBatch.Draw(tex, projectile.Center - Main.screenPosition, null, new Color(255, 60, 80) * prog * 0.4f, projectile.rotation + 1.57f + 0.1f, new Vector2(tex.Width / 2, tex.Height), prog * 0.55f, 0, 0);
		}
	}

	public class SoulSuck : SmartBuff
	{
		public SoulSuck() : base("Soul Suck", "You getting sucked", false) { }

		public override void Update(NPC npc, ref int buffIndex)
		{
			if (!npc.friendly)
			{
				if (npc.lifeRegen > 0)
				{
					npc.lifeRegen = 0;
				}

				npc.lifeRegen -= GluttonyHandle.DPS * 10;
			}
		}
	}
}