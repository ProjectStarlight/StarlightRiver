using StarlightRiver.Content.Buffs;
using StarlightRiver.Core.Systems.PixelationSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.ID;
using Terraria.Utilities;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Items.Misc
{
	public class ElectroArrow : ModItem
	{
		public override string Texture => AssetDirectory.MiscItem + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Electro Arrow");
			Tooltip.SetDefault("Chains to nearby enemies\nInflicts {{BUFF:Overcharge}}");
		}

		public override void SetDefaults()
		{
			Item.damage = 1;
			Item.DamageType = DamageClass.Ranged;
			Item.width = 8;
			Item.height = 8;
			Item.maxStack = 9999;
			Item.consumable = true;
			Item.crit = -4;
			Item.knockBack = 0f;
			Item.value = 10;
			Item.rare = ItemRarityID.Blue;
			Item.shoot = ProjectileType<ElectroArrowProjectile>();
			Item.shootSpeed = 1f;
			Item.ammo = AmmoID.Arrow;
		}

		public override void ModifyTooltips(List<TooltipLine> tooltips)
		{
			tooltips.FirstOrDefault(n => n.Name == "Damage").Text = "Deals 25% bow damage";
		}

		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe(10);
			recipe.AddIngredient(ItemID.WoodenArrow, 10);
			recipe.AddIngredient(ItemID.Wire);
			recipe.AddTile(TileID.WorkBenches);
			recipe.Register();
		}
	}

	internal class ElectroArrowProjectile : ModProjectile
	{
		Vector2 savedPos = Vector2.Zero;

		readonly List<NPC> hitNPCs = new();
		readonly List<Vector2> nodes = new();

		public ref float State => ref Projectile.ai[0];

		public override string Texture => AssetDirectory.MiscItem + Name;

		public override void SetDefaults()
		{
			Projectile.width = 8;
			Projectile.height = 8;
			Projectile.friendly = true;
			Projectile.DamageType = DamageClass.Ranged;
			Projectile.penetrate = 3;
			Projectile.timeLeft = 180;
			Projectile.tileCollide = true;
			Projectile.ignoreWater = false;

			Projectile.extraUpdates = 3;
		}

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Electro Arrow");
		}

		public override void AI()
		{
			if (Projectile.extraUpdates != 0)
				Projectile.rotation = Projectile.velocity.ToRotation() - MathHelper.PiOver2;
			else
				Projectile.Opacity = Projectile.timeLeft > 8 ? 1 : Projectile.timeLeft / 7f;

			if (Projectile.timeLeft == 180)
			{
				savedPos = Projectile.Center;
				Terraria.Audio.SoundEngine.PlaySound(SoundID.DD2_LightningBugZap, Projectile.Center);
			}

			if (State == 1)
			{
				Projectile.extraUpdates = 0;
				Projectile.velocity *= 0;

				if (Projectile.timeLeft > 15)
					Projectile.timeLeft = 15;

				Projectile.Opacity = MathF.Sin(Projectile.timeLeft / 15f * 3.14f);

				for (int k = 0; k < hitNPCs.Count; k++)
				{
					Vector2 start = k == 0 ? savedPos : hitNPCs[k - 1].Center;
					Vector2 end = hitNPCs[k].Center;

					var vel = end.DirectionTo(start).RotatedBy(Main.rand.NextBool() ? 0.5f : -0.5f) * Main.rand.NextFloat(2f, 8f);
					Dust.NewDustPerfect(Vector2.Lerp(start, end, Main.rand.NextFloat()), ModContent.DustType<Dusts.GlowLineFast>(), vel, 0, new Color(50, 80, 200), Main.rand.NextFloat(0.4f, 0.5f));

					if (Main.rand.NextBool(3))
					{
						vel = end.DirectionTo(start).RotatedBy(Main.rand.NextBool() ? 0.3f : -0.3f) * Main.rand.NextFloat(6f, 12f);
						Dust.NewDustPerfect(Vector2.Lerp(start, end, Main.rand.NextFloat()), ModContent.DustType<Dusts.GlowLineFast>(), vel, 0, new Color(150, 200, 255), Main.rand.NextFloat(0.5f, 0.7f));
					}
				}
			}

			if (Projectile.timeLeft == 1)
				PreKill(Projectile.timeLeft);
		}

		public override void PostDraw(Color lightColor)
		{
			if (State == 1)
			{
				for (int k = 0; k < hitNPCs.Count; k++)
				{
					Vector2 start = k == 0 ? savedPos : hitNPCs[k - 1].Center;
					Vector2 end = hitNPCs[k].Center;

					ModContent.GetInstance<PixelationSystem>().QueueRenderAction("UnderProjectiles", () =>
					{
						DrawLightning(Main.spriteBatch, start, end);
					});
				}
			}
		}

		private void DrawLightning(SpriteBatch spritebatch, Vector2 point1, Vector2 point2)
		{
			Texture2D tex = Assets.GlowTrailNoEnd.Value;
			Texture2D tex2 = Assets.GlowTrail.Value;
			Texture2D glow = Assets.StarTexture.Value;

			float dist = Vector2.Distance(point1, point2);
			float rot = point1.DirectionTo(point2).ToRotation();

			Rectangle target = new Rectangle((int)(point1.X - Main.screenPosition.X), (int)(point1.Y - Main.screenPosition.Y), (int)dist, (int)(60 * Projectile.Opacity));
			Rectangle target2 = new Rectangle((int)(point1.X - Main.screenPosition.X), (int)(point1.Y - Main.screenPosition.Y), (int)dist, (int)(90 * Projectile.Opacity));

			spritebatch.Draw(tex, target, null, new Color(50, 150, 200, 0) * 0.45f * Projectile.Opacity, rot, new Vector2(0, tex.Height / 2f), 0, 0);
			spritebatch.Draw(tex, target2, null, new Color(30, 100, 200, 0) * 0.25f * Projectile.Opacity, rot, new Vector2(0, tex.Height / 2f), 0, 0);

			UnifiedRandom rand = new UnifiedRandom((int)Main.GameUpdateCount / 3 ^ 901273125);

			float lastOffset = 0;

			int segments = (int)(dist / 42) + 1;
			int step = (int)(dist / segments);

			for (int k = 0; k < step * segments; k += step)
			{
				Vector2 segStart = Vector2.Lerp(point1, point2, k / dist);
				segStart += Vector2.UnitX.RotatedBy(rot + 1.57f) * lastOffset;

				lastOffset = rand.NextFloat(-10, 10);

				if (k == step * segments - step)
					lastOffset = 0;

				Vector2 segEnd = Vector2.Lerp(point1, point2, (k + step) / dist);
				segEnd += Vector2.UnitX.RotatedBy(rot + 1.57f) * lastOffset;

				float segDist = Vector2.Distance(segStart, segEnd);
				float segWidth = 6 + MathF.Sin(k / dist * 3.14f) * 10;

				Rectangle segTarget = new Rectangle((int)(segStart.X - Main.screenPosition.X), (int)(segStart.Y - Main.screenPosition.Y), (int)segDist + 2, (int)(segWidth * Projectile.Opacity));
				spritebatch.Draw(tex2, segTarget, null, new Color(200, 220, 255, 0) * Projectile.Opacity, segStart.DirectionTo(segEnd).ToRotation(), new Vector2(0, tex2.Height / 2f), 0, 0);
			}

			spritebatch.Draw(glow, point2 - Main.screenPosition, null, new Color(150, 180, 200, 0) * 0.85f * Projectile.Opacity, 0, glow.Size() / 2f, 0.2f * Projectile.Opacity, 0, 0);
			spritebatch.Draw(glow, point2 - Main.screenPosition, null, new Color(30, 100, 200, 0) * 0.25f * Projectile.Opacity, 0, glow.Size() / 2f, 0.4f * Projectile.Opacity, 0, 0);
		}

		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
		{
			modifiers.FinalDamage *= 0;
			modifiers.DisableCrit();
			modifiers.DisableKnockback();
			modifiers.HideCombatText();
		}

		public override bool? CanHitNPC(NPC target)
		{
			return State == 0;
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			hitNPCs.Add(target);

			CheckAboutNPC(target, 3);

			State = 1;

			hitNPCs.ForEach(n =>
			{
				n.AddBuff(BuffType<Overcharge>(), 300);
				n.SimpleStrikeNPC((int)(Projectile.damage * 0.25f), 0, false, 0, DamageClass.Ranged);
			});

			PreKill(Projectile.timeLeft);
		}

		public void CheckAboutNPC(NPC origin, int remaining)
		{
			if (remaining <= 0)
				return;

			Random rng = new Random();
			var indicies = Enumerable.Range(0, Main.maxNPCs + 1).OrderBy(_ => rng.Next()).ToArray();

			for (int k = 0; k < indicies.Length; k++)
			{
				NPC scan = Main.npc[indicies[k]];
				if (scan.active && scan.chaseable && !hitNPCs.Contains(scan) && Vector2.Distance(scan.Center, origin.Center) < 500)
				{
					hitNPCs.Add(scan);
					CheckAboutNPC(scan, remaining - 1);
					break;
				}
			}
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			State = 1;

			return false;
		}

		public override bool PreKill(int timeLeft)
		{
			if (Projectile.extraUpdates == 0)
				return true;

			Projectile.velocity *= 0;
			Projectile.friendly = false;
			Projectile.timeLeft = 15;
			Projectile.extraUpdates = 0;

			return false;
		}
	}
}