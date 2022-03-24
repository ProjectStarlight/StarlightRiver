using StarlightRiver.Content.Items.BaseTypes;
using StarlightRiver.Core;
using StarlightRiver.Helpers;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Graphics.Effects;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using System;
using System.Collections.Generic;
using System.Linq;

namespace StarlightRiver.Content.Items.Misc
{
    public class BloodAmulet : SmartAccessory
    {
        public override string Texture => AssetDirectory.MiscItem + Name;

		public BloodAmulet() : base("Blood Amulet", "Every 25 damage taken releases a homing bloodbolt \nThese bolts damage enemies and guaruntee they drop life hearts on death") { }

        public override void SafeSetDefaults()
        {
            item.value = Item.sellPrice(0, 2, 0, 0);
            item.rare = ItemRarityID.LightRed;
        }

        public override void SafeUpdateEquip(Player player)
        {
            player.GetModPlayer<BloodAmuletPlayer>().equipped = true;
        }
	}
    public class BloodAmuletPlayer : ModPlayer
    {
        public bool equipped = false;

        public int damageTicker;

        public override void ResetEffects()
        {
            equipped = false;
        }

        public override void ModifyHitByNPC(NPC npc, ref int damage, ref bool crit)
        {
            if (equipped)
            {
                damageTicker += damage;
                SpawnBolts();
            }
        }

        public override void ModifyHitByProjectile(Projectile proj, ref int damage, ref bool crit)
        {
            if (equipped)
            {
                damageTicker += damage;
                SpawnBolts();
            }
        }

        private void SpawnBolts()
        {
            while (damageTicker > 25)
            {
                damageTicker -= 25;
                Projectile.NewProjectile(player.Center, Main.rand.NextVector2Circular(10,10), ModContent.ProjectileType<BloodAmuletBolt>(), 25, 0, player.whoAmI);
            }
        }
    }

	public class BloodAmuletGNPC : GlobalNPC
    {
		public override bool InstancePerEntity => true;

		public bool dropHeart = false;

        public override void NPCLoot(NPC npc)
        {
			if (dropHeart)
				Item.NewItem(npc.Center, ItemID.Heart);
        }
    }


	public class BloodAmuletBolt : ModProjectile, IDrawPrimitive
    {
        public override string Texture => AssetDirectory.Assets + "Invisible";

        private List<Vector2> cache;
        private Trail trail;

		const int TRAILLENGTH = 25;

		public float fade => Math.Min(1, projectile.timeLeft / 15f);

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Ghoul");
		}

		public override void SetDefaults()
		{
			projectile.width = 20;
			projectile.height = 20;
			projectile.friendly = true;
			projectile.penetrate = -1;
			projectile.timeLeft = 450;
			projectile.tileCollide = false;
			projectile.ignoreWater = true;
			projectile.alpha = 255;
		}

		public override void AI()
		{
			Movement();
			ManageCaches();
			ManageTrail();
		}

		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
		{
			target.GetGlobalNPC<BloodAmuletGNPC>().dropHeart = true;

			projectile.friendly = false;
			if (projectile.timeLeft > 15)
            {
				projectile.timeLeft = 15;
            }				
		}

		private void Movement()
		{
			NPC target = Main.npc.Where(n => n.CanBeChasedBy(projectile, false) && Vector2.Distance(n.Center, projectile.Center) < 800).OrderBy(n => Vector2.Distance(n.Center, projectile.Center)).FirstOrDefault();

			if (target != default)
			{
				Vector2 direction = target.Center - projectile.Center;
				direction.Normalize();
				direction *= 10;
				projectile.velocity = Vector2.Lerp(projectile.velocity, direction, 0.03f);
			}
			if (fade < 1)
				projectile.velocity = Vector2.Zero;
		}

		private void ManageCaches()
		{
			if (cache == null)
			{
				cache = new List<Vector2>();
				for (int i = 0; i < TRAILLENGTH; i++)
				{
					cache.Add(projectile.Center);
				}
			}

			cache.Add(projectile.Center);

			while (cache.Count > TRAILLENGTH)
			{
				cache.RemoveAt(0);
			}
		}

		private void ManageTrail()
		{
			trail = trail ?? new Trail(Main.instance.GraphicsDevice, TRAILLENGTH, new TriangularTip(1), factor => 20 * factor * fade, factor =>
			{
				return Color.Lerp(Color.Black, Color.Red, factor.X);
			});

			trail.Positions = cache.ToArray();
			trail.NextPosition = projectile.Center + projectile.velocity;
		}

		public void DrawPrimitives()
		{
			Effect effect = Filters.Scene["CeirosRing"].GetShader().Shader;

			Matrix world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
			Matrix view = Main.GameViewMatrix.ZoomMatrix;
			Matrix projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

			effect.Parameters["time"].SetValue(Main.GameUpdateCount);
			effect.Parameters["repeats"].SetValue(2f);
			effect.Parameters["transformMatrix"].SetValue(world * view * projection);
			effect.Parameters["sampleTexture"].SetValue(ModContent.GetTexture("StarlightRiver/Assets/FireTrail"));

			trail?.Render(effect);
		}

	}
}