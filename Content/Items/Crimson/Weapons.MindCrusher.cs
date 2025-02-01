using StarlightRiver.Content.Biomes;
using StarlightRiver.Content.Buffs;
using StarlightRiver.Content.Items.Vitric;
using StarlightRiver.Content.Projectiles;
using StarlightRiver.Core.Systems.InstancedBuffSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Crimson
{
	internal class MindCrusher : ModItem
	{
		public override string Texture => AssetDirectory.Debug;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Mind crusher");
			Tooltip.SetDefault("Click and hold to create a growing circle\nRelease to crush all enemies inside the circle\nEnemies with defense take 2 additional damage per defense\nInflicts 5 stacks of {{BUFF:Neurosis}}");
		}

		public override void SetDefaults()
		{
			Item.damage = 32;
			Item.DamageType = DamageClass.Magic;
			Item.width = 32;
			Item.height = 32;
			Item.useTime = 40;
			Item.useAnimation = 40;
			Item.useStyle = ItemUseStyleID.HoldUp;
			Item.noMelee = true;
			Item.knockBack = 1;
			Item.rare = ItemRarityID.Orange;
			Item.value = Item.sellPrice(0, 5, 0, 0);
			Item.shoot = ModContent.ProjectileType<MindCursherProjectile>();
			Item.shootSpeed = 0f;
			Item.useTurn = true;
			Item.channel = true;
			Item.mana = 100;
		}

		public override bool CanUseItem(Player player)
		{
			return !Main.projectile.Any(n => n.active && n.owner == player.whoAmI && n.type == Item.shoot);
		}

		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
		{
			position = Main.MouseWorld;
		}

		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ModContent.ItemType<DendriteBar>(), 10);
			recipe.AddIngredient(ModContent.ItemType<ImaginaryTissue>(), 5);
			recipe.AddIngredient(ItemID.CrimsonRod);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}
	}

	internal class MindCursherProjectile : ModProjectile
	{
		private readonly int largest = 250;

		public override string Texture => AssetDirectory.Invisible;

		public ref float Radius => ref Projectile.ai[0];
		public ref float State => ref Projectile.ai[1];
		public ref float MaxRadius => ref Projectile.ai[2];

		public override void Load()
		{
			GraymatterBiome.onDrawHallucinationMap += DrawHallucinations;
		}

		public override void SetDefaults()
		{
			Projectile.penetrate = -1;
			Projectile.timeLeft = 300;
			Projectile.width = 1;
			Projectile.height = 1;
			Projectile.tileCollide = false;
			Projectile.friendly = true;
		}

		public override void AI()
		{
			GraymatterBiome.forceGrayMatter = true;
			Player player = Main.player[Projectile.owner];

			if (State == 0)
			{
				if (player.channel)
				{
					Projectile.timeLeft = 300;

					int oldWidth = Projectile.width;

					Projectile.width = (int)Radius * 2;
					Projectile.height = (int)Radius * 2;
					Projectile.position -= Vector2.One * (Projectile.width - oldWidth) / 2f;

					Projectile.Center += (Main.MouseWorld - Projectile.Center) * 0.05f;

					if (Radius < largest)
						Radius += (largest - Radius) / (float)largest * 5f;
				}
				else
				{
					MaxRadius = Radius;
					State = 1;
				}
			}

			if (State == 1)
			{
				Radius -= 20;
			}

			if (Radius <= 0)
			{
				Projectile.timeLeft = 0;
				for (int k = 0; k < 50; k++)
				{
					var color = new Color(Main.rand.NextFloat(0.5f, 1f), Main.rand.NextFloat(0.5f, 1f), Main.rand.NextFloat(0.5f, 1f));
					float rot = Main.rand.NextFloat(6.28f);

					Dust.NewDustPerfect(Projectile.Center + Vector2.One.RotatedBy(rot) * 45, ModContent.DustType<Dusts.GlowLineFast>(), Vector2.One.RotatedBy(rot) * Main.rand.NextFloat(8) * MaxRadius / (float)largest, 0, color, Main.rand.NextFloat(0.5f, 1f) * MaxRadius / (float)largest);
					Dust.NewDustPerfect(Projectile.Center + Vector2.One.RotatedBy(rot) * 5, ModContent.DustType<Dusts.GraymatterDust>(), Vector2.One.RotatedBy(rot) * Main.rand.NextFloat(8) * MaxRadius / (float)largest, 0, color, Main.rand.NextFloat(0.35f, 0.75f) * MaxRadius / (float)largest);
				}
			}

			Lighting.AddLight(Projectile.Center, Color.White.ToVector3() * 1.5f);
		}

		public override bool? CanHitNPC(NPC target)
		{
			return MaxRadius > 1 && Vector2.Distance(target.Center, Projectile.Center) < Radius && State == 1 ? null : false;
		}

		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
		{
			if (target.defense > 0)
				modifiers.FinalDamage += target.defense * 2;
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			for (int k = 0; k < 5; k++)
			{
				BuffInflictor.Inflict<Neurosis>(target, 300);
			}
		}

		public override void PostDraw(Color lightColor)
		{
			Texture2D tex = Assets.Misc.GlowRing.Value;
			Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, new Color(255, 255, 255, 0) * 0.1f, 0, tex.Size() / 2f, Radius * 2f / tex.Width, 0, 0);
		}

		private void DrawHallucinations(SpriteBatch batch)
		{
			foreach (Projectile proj in Main.ActiveProjectiles)
			{
				if (proj.type == Type)
				{
					Texture2D tex = Assets.Misc.StarView.Value;
					batch.Draw(tex, proj.Center - Main.screenPosition, null, new Color(255, 255, 255, 0), 0, tex.Size() / 2f, (proj.ModProjectile as MindCursherProjectile).Radius * 7f / tex.Width, 0, 0);
				}
			}
		}
	}
}