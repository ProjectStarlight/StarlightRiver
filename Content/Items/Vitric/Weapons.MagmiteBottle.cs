using StarlightRiver.Content.Dusts;
using StarlightRiver.Content.NPCs.Vitric;
using StarlightRiver.Core.Systems.InstancedBuffSystem;
using System.Linq;
using Terraria.GameContent;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Items.Vitric
{
	class MagmiteBottle : ModItem
	{
		public override string Texture => AssetDirectory.VitricItem + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Magmite in a Bottle");
			Tooltip.SetDefault("Why would you do this to him?!");
		}
		public override void SetDefaults()
		{
			Item.damage = 50;
			Item.DamageType = DamageClass.Ranged;
			Item.width = 36;
			Item.height = 38;
			Item.useTime = 18;
			Item.useAnimation = 18;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.noUseGraphic = true;
			Item.value = 0;
			Item.rare = ItemRarityID.Orange;
			Item.UseSound = SoundID.Item1;
			Item.autoReuse = false;
			Item.useTurn = true;
			Item.shoot = ModContent.ProjectileType<MagmiteBottleProjectile>();
			Item.shootSpeed = 8.5f;
			Item.consumable = true;
			Item.maxStack = 999;
		}

		public override void AddRecipes()
		{
			CreateRecipe().
				AddIngredient(ModContent.ItemType<MagmitePassiveItem>()).
				AddIngredient(ItemID.Bottle).
				Register();
		}
	}

	class MagmiteBottleProjectile : ModProjectile
	{
		public override string Texture => AssetDirectory.VitricItem + "MagmiteBottle";

		public override void SetDefaults()
		{
			Projectile.width = 16;
			Projectile.height = 16;
			Projectile.timeLeft = 600;
			Projectile.friendly = true;
			Projectile.damage = 60;
		}

		public override Color? GetAlpha(Color lightColor)
		{
			return Color.White;
		}

		public override void AI()
		{
			Projectile.velocity.Y += 0.4f;
			Projectile.rotation += Projectile.velocity.X * 0.05f;
		}

		public override void Kill(int timeLeft)
		{
			for (int k = 0; k < 60; k++)
				Gore.NewGoreDirect(Projectile.GetSource_FromThis(), Projectile.Center, (Vector2.UnitY * Main.rand.NextFloat(-16, -1)).RotatedByRandom(0.8f), Mod.Find<ModGore>("MagmiteGore").Type, Main.rand.NextFloat(1.0f, 1.4f));

			for (int k = 0; k < 50; k++)
				Dust.NewDust(Projectile.position, 16, 16, DustID.Demonite);

			for (int x = -8; x < 8; x++)
			{
				for (int y = -8; y < 8; y++)
				{
					Tile tile = Main.tile[(int)Projectile.Center.X / 16 + x, (int)Projectile.Center.Y / 16 + y];
					if (tile.HasTile && Main.tileSolid[tile.TileType] && Helpers.Helper.IsEdgeTile((int)Projectile.Center.X / 16 + x, (int)Projectile.Center.Y / 16 + y))
					{
						Vector2 pos = new Vector2((int)Projectile.Center.X / 16 + x, (int)Projectile.Center.Y / 16 + y) * 16 + Vector2.One * 8;

						if (!Main.projectile.Any(n => n.active && n.type == ModContent.ProjectileType<MagmaBottleBurn>() && n.Center == pos))
							Projectile.NewProjectile(Projectile.GetSource_FromThis(), pos, Vector2.Zero, ModContent.ProjectileType<MagmaBottleBurn>(), 25, 0, Projectile.owner);
						else
							Main.projectile.FirstOrDefault(n => n.active && n.type == ModContent.ProjectileType<MagmaBottleBurn>() && n.Center == pos).timeLeft = 180;
					}
				}

				Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.AmberBolt, 0, 0, 0, default, 0.5f);
			}

			Terraria.Audio.SoundEngine.PlaySound(SoundID.Shatter, Projectile.Center);
			Terraria.Audio.SoundEngine.PlaySound(SoundID.DD2_GoblinHurt, Projectile.Center);
		}
	}

	class MagmaBottleBurn : ModProjectile, IDrawAdditive
	{
		public override string Texture => AssetDirectory.Invisible;

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			return false;
		}

		public override void SetDefaults()
		{
			Projectile.width = 18;
			Projectile.height = 18;
			Projectile.friendly = true;
			Projectile.DamageType = DamageClass.Ranged;
			Projectile.penetrate = -1;
			Projectile.timeLeft = 180;
			Projectile.tileCollide = false;
			Projectile.damage = 1;
		}

		public override void AI()
		{
			Tile tile = Main.tile[(int)Projectile.Center.X / 16, (int)Projectile.Center.Y / 16];
			if (!tile.HasTile)
				Projectile.timeLeft = 0;

			Lighting.AddLight(Projectile.Center, new Vector3(1.1f, 0.5f, 0.2f) * (Projectile.timeLeft / 180f));
		}

		public override bool? CanHitNPC(NPC target)
		{
			if (target.Hitbox.Intersects(Projectile.Hitbox))
				BuffInflictor.Inflict<MagmaBurn>(target, 30);
			return false;
		}

		public override void PostDraw(Color lightColor)
		{
			Tile tile = Main.tile[(int)Projectile.Center.X / 16, (int)Projectile.Center.Y / 16];
			Texture2D tex = TextureAssets.Tile[tile.TileType].Value;
			var frame = new Rectangle(tile.TileFrameX, tile.TileFrameY, 16, 16);
			Vector2 pos = Projectile.position + Vector2.One - Main.screenPosition;
			Color color = new Color(255, 140, 50) * 0.2f * (Projectile.timeLeft / 180f);

			Main.spriteBatch.Draw(tex, pos, frame, color, 0, Vector2.Zero, 1, 0, 0);
		}

		public void DrawAdditive(SpriteBatch spriteBatch)
		{
			Texture2D tex = Request<Texture2D>("StarlightRiver/Assets/Keys/Glow").Value;
			Color color = new Color(255, 100, 50) * 0.3f * (Projectile.timeLeft / 180f);
			spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, tex.Frame(), color, 0, tex.Size() / 2, 1.2f * (Projectile.timeLeft / 180f), 0, 0);
		}
	}

	internal class MagmaBurn : StackableBuff
	{
		public override string Name => "MagmaBurn";

		public override string DisplayName => "Magma burn";

		public override string Texture => AssetDirectory.VitricItem + Name;

		public override bool Debuff => true;

		public override string Tooltip => "Liquid glass melts away at you!";

		public override BuffStack GenerateDefaultStack(int duration)
		{
			return new BuffStack()
			{
				duration = duration
			};
		}

		public override void PerStackEffectsNPC(NPC npc, BuffStack stack)
		{
			npc.lifeRegen -= 1;
		}

		public override void AnyStacksUpdateNPC(NPC npc)
		{
			Dust.NewDustDirect(npc.position, npc.width, npc.height, DustID.Torch, 0, 0, 0, default, 2f).noGravity = true;

			if (Main.rand.NextBool(3))
				Dust.NewDustDirect(npc.position, npc.width, npc.height, ModContent.DustType<CoachGunDustGlow>(), 0, 0, 0, default, 2f).velocity *= 0.1f;

			if (Main.rand.NextBool(10))
				Dust.NewDustPerfect(npc.position, ModContent.DustType<Dusts.MagmaSmoke>(), (Vector2.UnitY * Main.rand.NextFloat(-3f, -2f)).RotatedByRandom(MathHelper.ToRadians(75f)), 100, Color.Black, Main.rand.NextFloat(0.7f, 0.9f));
		}
	}
}