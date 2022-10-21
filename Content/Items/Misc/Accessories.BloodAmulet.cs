using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Items.BaseTypes;
using StarlightRiver.Content.Items.Gravedigger;
using StarlightRiver.Core;
using StarlightRiver.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Items.Misc
{
	public class BloodAmulet : SmartAccessory
	{
		public int StoredDamage;

		public override string Texture => AssetDirectory.MiscItem + Name;

		public BloodAmulet() : base("Blood Amulet", "Every 25 damage taken releases a homing bloodbolt \nThese bolts damage enemies and guaruntee they drop life hearts on death") { }

		public override void Load()
		{
			StarlightPlayer.ModifyHitByNPCEvent += BloodAmuletOnhit;
			StarlightPlayer.ModifyHitByProjectileEvent += BloodAmuletOnhitProjectile;
		}

		public override void Unload()
		{
			StarlightPlayer.ModifyHitByNPCEvent -= BloodAmuletOnhit;
			StarlightPlayer.ModifyHitByProjectileEvent -= BloodAmuletOnhitProjectile;
		}

		public override void SafeSetDefaults()
		{
			Item.value = Item.sellPrice(0, 2, 0, 0);
			Item.rare = ItemRarityID.LightRed;
		}

		public void BloodAmuletOnhit(Player player, NPC NPC, ref int damage, ref bool crit)
		{
			if (Equipped(player))
			{
				(GetEquippedInstance(player) as BloodAmulet).StoredDamage += damage;
				SpawnBolts(player);
			}
		}

		public void BloodAmuletOnhitProjectile(Player player, Projectile proj, ref int damage, ref bool crit)
		{
			if (Equipped(player))
			{
				(GetEquippedInstance(player) as BloodAmulet).StoredDamage += damage;
				SpawnBolts(player);
			}
		}

		private void SpawnBolts(Player player)
		{
			while (StoredDamage > 25)
			{
				StoredDamage -= 25;
				Projectile.NewProjectile(player.GetSource_Accessory(Item), player.Center, Main.rand.NextVector2Circular(10, 10), ModContent.ProjectileType<BloodAmuletBolt>(), 25, 0, player.whoAmI);
			}
		}

		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ModContent.ItemType<LivingBlood>(), 5);
			recipe.AddIngredient(ItemID.DemoniteBar, 10);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();

			recipe = CreateRecipe();
			recipe.AddIngredient(ModContent.ItemType<LivingBlood>(), 5);
			recipe.AddIngredient(ItemID.CrimtaneBar, 10);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}
	}

	public class BloodAmuletGNPC : GlobalNPC
	{
		public override bool InstancePerEntity => true;

		public bool dropHeart = false;

		public override void OnHitByItem(NPC NPC, Player player, Item item, int damage, float knockback, bool crit)
		{
			if (dropHeart && NPC.life <= 0)
				Item.NewItem(NPC.GetSource_Loot(), NPC.Center, ItemID.Heart);
		}
		public override void OnHitByProjectile(NPC NPC, Projectile projectile, int damage, float knockback, bool crit)
		{
			if (dropHeart && NPC.life <= 0)
				Item.NewItem(NPC.GetSource_Loot(), NPC.Center, ItemID.Heart);
		}
	}

	public class BloodAmuletBolt : ModProjectile, IDrawPrimitive
	{
		public override string Texture => AssetDirectory.Assets + "Invisible";

		private List<Vector2> cache;
		private Trail trail;

		const int TRAILLENGTH = 25;

		public float fade => Math.Min(1, Projectile.timeLeft / 15f);

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Ghoul");
		}

		public override void SetDefaults()
		{
			Projectile.width = 20;
			Projectile.height = 20;
			Projectile.friendly = true;
			Projectile.penetrate = -1;
			Projectile.timeLeft = 450;
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
			target.GetGlobalNPC<BloodAmuletGNPC>().dropHeart = true;

			Projectile.friendly = false;
			if (Projectile.timeLeft > 15)
			{
				Projectile.timeLeft = 15;
			}
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

			if (fade < 1)
				Projectile.velocity = Vector2.Zero;
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
			trail ??= new Trail(Main.instance.GraphicsDevice, TRAILLENGTH, new TriangularTip(1), factor => 20 * factor * fade, factor => Color.Lerp(Color.Black, Color.Red, factor.X));

			trail.Positions = cache.ToArray();
			trail.NextPosition = Projectile.Center + Projectile.velocity;
		}

		public void DrawPrimitives()
		{
			Effect effect = Filters.Scene["CeirosRing"].GetShader().Shader;

			var world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
			Matrix view = Main.GameViewMatrix.ZoomMatrix;
			var projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

			effect.Parameters["time"].SetValue(Main.GameUpdateCount);
			effect.Parameters["repeats"].SetValue(2f);
			effect.Parameters["transformMatrix"].SetValue(world * view * projection);
			effect.Parameters["sampleTexture"].SetValue(ModContent.Request<Texture2D>("StarlightRiver/Assets/FireTrail").Value);

			trail?.Render(effect);
		}
	}
}