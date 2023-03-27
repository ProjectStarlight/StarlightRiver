using System.Collections.Generic;
using Terraria.GameContent.Creative;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Misc
{
	public class CopperCoil : ModItem
	{
		public override string Texture => AssetDirectory.MiscItem + "CopperCoil";

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Copper Coil");
			Tooltip.SetDefault("Strikes nearby enemies with static electricity");
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
		}

		public override void SetDefaults()
		{
			Item.DefaultToWhip(ModContent.ProjectileType<CopperCoilWhip>(), 5, 1.2f, 5f, 25);
			Item.SetShopValues(ItemRarityID.White, Item.sellPrice(0, 0, 50));
		}

		public override void AddRecipes()
		{
			CreateRecipe()
				.AddIngredient(ItemID.CopperBar, 7)
				.AddIngredient(ItemID.Cobweb, 5)
				.AddTile(TileID.WorkBenches)
				.Register();
		}
	}

	public class CopperCoilWhip : BaseWhip
	{
		public override string Texture => AssetDirectory.MiscItem + "CopperCoilWhip";

		public CopperCoilWhip() : base("Copper Coil", 15, 0.57f, new Color(153, 122, 97)) { }

		public override int SegmentVariant(int segment)
		{
			int variant = segment switch
			{
				5 or 6 or 7 or 8 => 2,
				9 or 10 or 11 or 12 or 13 => 3,
				_ => 1,
			};
			return variant;
		}

		public override bool ShouldDrawSegment(int segment)
		{
			return true;// segment % 2 == 0;
		}

		public override void ArcAI()
		{
			if (Projectile.ai[0] > MiddleOfArc - 5 && Projectile.ai[0] < MiddleOfArc + 5)
			{
				float scale = Utils.GetLerpValue(MiddleOfArc - 5, MiddleOfArc, Projectile.ai[0], true) * Utils.GetLerpValue(MiddleOfArc + 5, MiddleOfArc, Projectile.ai[0], true);
				var d = Dust.NewDustPerfect(EndPoint, DustID.Electric, Vector2.Zero, 0, Color.GhostWhite, scale * 0.5f);
				d.noGravity = true;
				d.position += Main.rand.NextVector2Circular(1, 8).RotatedBy(Projectile.rotation);
				d.velocity += new Vector2(0f, -Main.rand.Next(1, 3)).RotatedBy(Projectile.rotation).RotatedByRandom(0.5f);
			}

			if (Projectile.ai[0] >= MiddleOfArc - 1 && Projectile.ai[0] < MiddleOfArc)
			{
				int id = Projectile.FindTargetWithLineOfSight();

				if (id >= 0)
				{
					NPC target = Main.npc[id];
					if (EndPoint.Distance(target.Center) < 300)
					{
						Vector2 vel = Main.player[Projectile.owner].DirectionTo(EndPoint) * 5f;
						var bolt = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), EndPoint, vel, ModContent.ProjectileType<CoilStaticShock>(), (int)(Projectile.damage * 1.2f), 0.5f, Main.player[Projectile.owner].whoAmI);
						bolt.ai[0] = id;
						bolt.localAI[0] = vel.ToRotation();
					}
				}
			}
		}

		public override Color? GetAlpha(Color lightColor)
		{
			Color minLight = lightColor;
			var minColor = new Color(10, 25, 33);

			if (minLight.R < minColor.R)
				minLight.R = minColor.R;

			if (minLight.G < minColor.G)
				minLight.G = minColor.G;

			if (minLight.B < minColor.B)
				minLight.B = minColor.B;

			return minLight;
		}
	}

	public class CoilStaticShock : ModProjectile
	{
		public override string Texture => AssetDirectory.MiscItem + "CopperCoilWhip";

		private readonly List<Vector2> points = new();
		private Vector2 startPoint;
		private bool canHit = true;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Static Shock");
			ProjectileID.Sets.CultistIsResistantTo[Type] = true;
		}

		public override void SetDefaults()
		{
			Projectile.width = 32;
			Projectile.height = 32;
			Projectile.friendly = true;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.extraUpdates = 10;
			Projectile.timeLeft = 120;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 5;
			Projectile.penetrate = -1;
			canHit = true;
		}

		public override void AI()
		{
			NPC target = Main.npc[(int)Projectile.ai[0]];

			if (Projectile.ai[1] == 0)
			{
				startPoint = Projectile.Center;
				Helpers.Helper.PlayPitched("Impacts/ShockHit", 0.9f, Main.rand.NextFloat(-0.3f, 0.8f), Projectile.Center);
			}

			Projectile.ai[1]++;
			Projectile.velocity = Vector2.Zero;

			Vector2 nextPoint = startPoint;
			points.Clear();
			int nodeCount = Main.rand.Next(5, 12) - (int)(1f / startPoint.Distance(target.Center) * 0.2f);

			for (int i = 0; i < nodeCount; i++)
			{
				Vector2 velocity = Vector2.UnitX.RotatedBy(Projectile.localAI[0]) * nextPoint.Distance(target.Center) / nodeCount;
				float lerp = Utils.GetLerpValue(0, nodeCount / 1.2f, i, true);
				velocity = Vector2.Lerp(velocity, nextPoint.DirectionTo(target.Center) * nextPoint.Distance(target.Center), lerp);
				points.Add(nextPoint);
				nextPoint += velocity;
			}

			for (int i = 1; i < points.Count - 2; i++)
				points[i] += Main.rand.NextVector2Circular(30, 20).RotatedBy(points[i + 1].AngleTo(points[i]));

			if (Main.rand.NextBool(8))
			{
				var d = Dust.NewDustPerfect(Main.rand.Next(points), DustID.Electric, Vector2.Zero, 0, Color.GhostWhite, 0.5f);
				d.noGravity = true;
				d.position += Main.rand.NextVector2Circular(5, 5);
				d.velocity += Main.rand.NextVector2Circular(2, 2);
			}

			Projectile.Center = target.Center;
		}

		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
		{
			target.AddBuff(ModContent.BuffType<Buffs.StaticShock>(), 180);
			Projectile.timeLeft = 100;
			canHit = false;
		}

		public override void OnHitPvp(Player target, int damage, bool crit)/* tModPorter Note: Removed. Use OnHitPlayer and check info.PvP */
		{
			target.AddBuff(ModContent.BuffType<Buffs.StaticShock>(), 180);
			Projectile.timeLeft = 100;
			canHit = false;
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			if (!canHit)
				return false;

			return base.Colliding(projHitbox, targetHitbox);
		}

		public override bool PreDraw(ref Color lightColor)
		{
			ReLogic.Content.Asset<Texture2D> texture = ModContent.Request<Texture2D>("StarlightRiver/Assets/GlowTrail");

			float t = Utils.GetLerpValue(0, 80, Projectile.timeLeft, true);
			var glowColor = Color.Lerp(new Color(120, 230, 255), Color.AliceBlue, 0.5f);
			glowColor.A = 0;

			for (int i = 0; i < points.Count - 1; i++)
			{
				Vector2 difference = points[i + 1] - points[i];
				float directionalRot = difference.ToRotation();
				var scale = new Vector2((difference.Length() + 2f) / texture.Width(), 16f / texture.Height() * t);
				Main.EntitySpriteDraw(texture.Value, points[i] - Main.screenPosition, null, glowColor, directionalRot, texture.Size() * new Vector2(0f, 0.5f), scale, SpriteEffects.None, 0);
			}

			return false;
		}
	}
}