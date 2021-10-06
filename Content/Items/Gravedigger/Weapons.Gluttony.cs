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
	public class GluttonyHandle : ModProjectile, IDrawPrimitive
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
			projectile.hide = true;
		}

        public override void AI()
        {
			Player player = Main.player[projectile.owner];
			projectile.damage = (int)(player.inventory[player.selectedItem].damage * player.magicDamage);
			timer++;
			direction = Main.MouseWorld - (player.Center);
			direction.Normalize();

			if (player.channel)
            {
				projectile.Center = player.Center;
				player.ChangeDir(Main.MouseWorld.X > player.position.X ? 1 : -1);

				player.itemTime = player.itemAnimation = 2;
				projectile.timeLeft = 2;

				player.itemRotation = direction.ToRotation();

				if (player.direction != 1)
					player.itemRotation -= 3.14f;
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

			for (int i = 0; i < TRAILLENGTH; i++)
			{
				float lerper = (float)i / (float)TRAILLENGTH;
				float radius = MathHelper.Lerp(STARTRAD, ENDRAD, lerper);

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
			ManageTrail(ref trail, ref cache);
			ManageTrail(ref trail2, ref cache2);
			ManageTrail(ref trail3, ref cache3);
			ManageTrail(ref trail4, ref cache4);
			ManageTrail(ref trail5, ref cache5);
		}

		private void ManageTrail(ref Trail localTrail, ref List<Vector2> localCache)
        {
			localTrail = localTrail ?? new Trail(Main.instance.GraphicsDevice, TRAILLENGTH, new TriangularTip(1), factor => MathHelper.Lerp(5,60,factor), factor =>
			{
				return Color.Red;
			});
			localTrail.Positions = localCache.ToArray();
			localTrail.NextPosition = projectile.Center + (direction * RANGE);
		}

		public void DrawPrimitives()
		{
			Effect effect = Filters.Scene["CeirosRing"].GetShader().Shader;

			Matrix world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
			Matrix view = Main.GameViewMatrix.ZoomMatrix;
			Matrix projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

			effect.Parameters["time"].SetValue(0.05f * Main.GameUpdateCount);
			effect.Parameters["repeats"].SetValue(3f);
			effect.Parameters["transformMatrix"].SetValue(world * view * projection);
			effect.Parameters["sampleTexture"].SetValue(ModContent.GetTexture("StarlightRiver/Assets/FireTrail"));

			trail?.Render(effect);
			trail2?.Render(effect);
			trail3?.Render(effect);
			trail4?.Render(effect);
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

				npc.lifeRegen -= GluttonyHandle.DPS;
			}
		}
	}
}