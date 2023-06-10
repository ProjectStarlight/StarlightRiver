using StarlightRiver.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria.DataStructures;
using Terraria.Graphics.Effects;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Items.Vitric
{
	class Coalescence : ModItem
	{
		public int manaCharge = 0;

		public override string Texture => AssetDirectory.VitricItem + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Coalescence");
			Tooltip.SetDefault("Charge for a volley of brilliant magic\nFully charged shots leech mana where their arrows meet");
		}

		public override void SetDefaults()
		{
			Item.damage = 44;
			Item.DamageType = DamageClass.Magic;
			Item.width = 16;
			Item.height = 64;
			Item.useTime = 6;
			Item.useAnimation = 6;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.noMelee = true;
			Item.noUseGraphic = true;
			Item.knockBack = 1;
			Item.rare = ItemRarityID.Orange;
			Item.channel = true;
			Item.shoot = ProjectileType<VitricBowProjectile>();
			Item.shootSpeed = 0f;
			Item.autoReuse = true;
			Item.mana = 40;

			Item.useTurn = true;
		}

		public override void ModifyTooltips(List<TooltipLine> tooltips)
		{
			tooltips.Find(n => n.Name == "Speed" && n.Mod == "Terraria").Text = "Slow charge";
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			if (!Main.projectile.Any(n => n.active && n.owner == player.whoAmI && n.type == ProjectileType<VitricBowProjectile>()))
			{
				Projectile.NewProjectile(source, position, velocity / 4f, Item.shoot, damage, knockback, player.whoAmI);
				manaCharge = 0;
			}

			return false;
		}

		public override void HoldItem(Player Player)
		{
			if (manaCharge >= 5)
			{
				manaCharge = 0;
				Player.statMana += Item.mana;
				CombatText.NewText(Player.Hitbox, CombatText.HealMana, Item.mana);
			}
		}

		public override void ModifyManaCost(Player Player, ref float reduce, ref float mult)
		{
			if (Main.projectile.Any(n => n.active && n.owner == Player.whoAmI && n.type == ProjectileType<VitricBowProjectile>()))
				mult = 0;
		}

		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient<VitricBow>();
			recipe.AddIngredient<SandstoneChunk>(7);
			recipe.AddIngredient<VitricOre>(7);
			recipe.AddIngredient<MagmaCore>();
			recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}
	}

	internal class VitricBowProjectile : ModProjectile, IDrawAdditive
	{
		private int charge = 0;

		public float ChargePercent => charge / 90f;
		Player Owner => Main.player[Projectile.owner];

		public ref float State => ref Projectile.ai[0];
		public ref float Angle => ref Projectile.ai[1];

		public override string Texture => AssetDirectory.VitricItem + "Coalescence";

		public override void SetDefaults()
		{
			Projectile.width = 32;
			Projectile.height = 32;
			Projectile.tileCollide = false;
		}

		public override void AI()
		{
			if (Owner == Main.LocalPlayer)
				Owner.GetModPlayer<ControlsPlayer>().mouseRotationListener = true;

			Angle = (Owner.Center - Owner.GetModPlayer<ControlsPlayer>().mouseWorld).ToRotation() + MathHelper.Pi;

			Projectile.rotation = Angle;
			Projectile.Center = Owner.Center + Vector2.UnitX.RotatedBy(Projectile.rotation) * 24;
			Owner.heldProj = Projectile.whoAmI;

			if (Owner.channel && State == 0)
			{
				float damageMult = 0.25f + ChargePercent * 0.75f;

				if (charge < 75)
					charge++;

				if (charge == 1)
				{
					if (Main.myPlayer == Projectile.owner)
						Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.UnitX, ProjectileType<VitricBowShard>(), (int)(Projectile.damage * damageMult), 1, Projectile.owner, 0, 1);

					Helper.PlayPitched("ImpactHeal", 0.6f, -0.2f);
				}

				if (Main.myPlayer == Projectile.owner)
				{
					for (int k = 2; k < 4; k++)
					{
						if (charge == 19 * k + 1)
						{
							Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.UnitX.RotatedBy((k - 1) * 0.3f), ProjectileType<VitricBowShard>(), (int)(Projectile.damage * damageMult), 1, Projectile.owner, 0, k);
							Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.UnitX.RotatedBy((k - 1) * -0.3f), ProjectileType<VitricBowShard>(), (int)(Projectile.damage * damageMult), 1, Projectile.owner, 0, k);
						}
					}
				}
			}

			else if (charge > 0)
			{
				State = 1;
				Projectile.timeLeft = charge;
				charge -= 4;
			}

			Lighting.AddLight(Owner.Center, new Vector3(0.3f + 0.3f * ChargePercent, 0.6f + 0.2f * ChargePercent, 1) * ChargePercent);
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D tex = Request<Texture2D>(Texture).Value;
			Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, lightColor, Projectile.rotation, tex.Size() / 2, 1, 0, 0);

			return false;
		}

		public void DrawAdditive(SpriteBatch spriteBatch)
		{
			Texture2D texStar = Request<Texture2D>(AssetDirectory.Dust + "Aurora").Value;
			Texture2D texGlow = Request<Texture2D>("StarlightRiver/Assets/Keys/GlowSoft").Value;

			var color1 = new Color(80, 240, 255);
			var color2 = new Color(90, 200, 255);
			var color3 = new Color(180, 220, 255);

			Vector2 offset = Vector2.UnitX.RotatedBy(Projectile.rotation);

			float prog1 = RangeLerp(ChargePercent, 0, 0.3f) + (float)Math.Sin(Main.GameUpdateCount / 20f) * 0.1f;
			float prog2 = RangeLerp(ChargePercent, 0.3f, 0.6f) + (float)Math.Sin(Main.GameUpdateCount / 20f + 1) * 0.1f;
			float prog3 = RangeLerp(ChargePercent, 0.6f, 0.9f) + (float)Math.Sin(Main.GameUpdateCount / 20f + 2) * 0.1f;

			DrawRing(spriteBatch, Projectile.Center + offset * (-30 + prog1 * 40), 1, 1, Main.GameUpdateCount / 40f, prog1, color3);
			DrawRing(spriteBatch, Projectile.Center + offset * (-30 + prog2 * 80), 1.5f, 1.5f, -Main.GameUpdateCount / 30f, prog2, color2);
			DrawRing(spriteBatch, Projectile.Center + offset * (-30 + prog3 * 120), 2, 2, Main.GameUpdateCount / 20f, prog3, color1);

			float prog4 = RangeLerp(ChargePercent, 0.2f, 0.5f) + (float)Math.Sin(Main.GameUpdateCount / 20f + 3) * 0.2f;
			spriteBatch.Draw(texStar, PosRing(Projectile.Center + offset * (-30 + prog4 * 60), 20, 80, -Main.GameUpdateCount / 15f), null, color3 * prog4, Main.GameUpdateCount / 10f, texStar.Size() / 2, prog1 * 0.2f, 0, 0);
			spriteBatch.Draw(texStar, PosRing(Projectile.Center + offset * (-30 + prog4 * 60), 20, 80, Main.GameUpdateCount / 10f), null, color1 * prog4, Main.GameUpdateCount / 15f, texStar.Size() / 2, prog2 * 0.2f, 0, 0);
			spriteBatch.Draw(texStar, PosRing(Projectile.Center + offset * (-30 + prog4 * 60), 20, 80, Main.GameUpdateCount / 25f), null, color3 * prog4, Main.GameUpdateCount / 8f, texStar.Size() / 2, prog1 * 0.2f, 0, 0);
			spriteBatch.Draw(texStar, PosRing(Projectile.Center + offset * (-30 + prog4 * 60), 20, 80, -Main.GameUpdateCount / 35f), null, color2 * prog4, Main.GameUpdateCount / 6f, texStar.Size() / 2, prog2 * 0.2f, 0, 0);
			spriteBatch.Draw(texStar, PosRing(Projectile.Center + offset * (-30 + prog4 * 60), 20, 80, -Main.GameUpdateCount / 20f), null, color1 * prog4, Main.GameUpdateCount / 12f, texStar.Size() / 2, prog3 * 0.2f, 0, 0);

			float prog5 = RangeLerp(ChargePercent, 0.5f, 0.8f) + (float)Math.Sin(Main.GameUpdateCount / 20f + 3) * 0.2f;
			spriteBatch.Draw(texStar, PosRing(Projectile.Center + offset * (-30 + prog5 * 100), 25, 100, Main.GameUpdateCount / 12f), null, color2 * prog5, Main.GameUpdateCount / 18f, texStar.Size() / 2, prog3 * 0.2f * 1.2f, 0, 0);
			spriteBatch.Draw(texStar, PosRing(Projectile.Center + offset * (-30 + prog5 * 100), 25, 100, -Main.GameUpdateCount / 18f), null, color3 * prog5, Main.GameUpdateCount / 12f, texStar.Size() / 2, prog3 * 0.2f * 1.2f, 0, 0);
			spriteBatch.Draw(texStar, PosRing(Projectile.Center + offset * (-30 + prog5 * 100), 25, 100, -Main.GameUpdateCount / 42f), null, color1 * prog5, Main.GameUpdateCount / 10f, texStar.Size() / 2, prog2 * 0.2f * 1.2f, 0, 0);
			spriteBatch.Draw(texStar, PosRing(Projectile.Center + offset * (-30 + prog5 * 100), 25, 100, Main.GameUpdateCount / 32f), null, color3 * prog5, Main.GameUpdateCount / 6f, texStar.Size() / 2, prog2 * 0.2f * 1.2f, 0, 0);
			spriteBatch.Draw(texStar, PosRing(Projectile.Center + offset * (-30 + prog5 * 100), 25, 100, Main.GameUpdateCount / 25f), null, color3 * prog5, Main.GameUpdateCount / 19f, texStar.Size() / 2, prog3 * 0.2f * 1.2f, 0, 0);

			spriteBatch.Draw(texGlow, Projectile.Center + offset * (-40 + prog2 * 90) - Main.screenPosition, null, color3 * (ChargePercent * 0.5f), 0, texGlow.Size() / 2, 3.5f, 0, 0);
		}

		private void DrawRing(SpriteBatch sb, Vector2 pos, float w, float h, float rotation, float prog, Color color) //optimization nightmare. Figure out smth later
		{
			Texture2D texRing = Request<Texture2D>(AssetDirectory.VitricItem + "BossBowRing", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
			Effect effect = Filters.Scene["BowRing"].GetShader().Shader;

			if (effect is null)
				return;

			effect.Parameters["uTime"].SetValue(rotation);
			effect.Parameters["cosine"].SetValue((float)Math.Cos(rotation));
			effect.Parameters["uColor"].SetValue(color.ToVector3());
			effect.Parameters["uImageSize1"].SetValue(new Vector2(Main.screenWidth, Main.screenHeight));
			effect.Parameters["uOpacity"].SetValue(prog);

			sb.End();
			sb.Begin(default, BlendState.Additive, default, default, RasterizerState.CullNone, effect, Main.GameViewMatrix.TransformationMatrix);

			Rectangle target = toRect(pos, (int)(16 * (w + prog)), (int)(60 * (h + prog)));
			sb.Draw(texRing, target, null, color * prog, Projectile.rotation, texRing.Size() / 2, 0, 0);

			sb.End();
			sb.Begin(default, BlendState.Additive, default, default, RasterizerState.CullNone, default, Main.GameViewMatrix.TransformationMatrix);
		}

		private Rectangle toRect(Vector2 pos, int w, int h)
		{
			return new Rectangle((int)(pos.X - Main.screenPosition.X), (int)(pos.Y - Main.screenPosition.Y), w, h);
		}

		private float RangeLerp(float input, float start, float end)
		{
			if (input < start)
				return 0;

			return MathHelper.Clamp(Helper.BezierEase((input - start) / (end - start)), 0, 1);
		}

		private Vector2 PosRing(Vector2 center, float w, float h, float rot)
		{
			return center + new Vector2((float)Math.Cos(rot) * h, (float)Math.Sin(rot) * w).RotatedBy(Projectile.rotation + MathHelper.PiOver2) - Main.screenPosition;
		}
	}

	internal class VitricBowShard : ModProjectile, IDrawAdditive
	{
		private Vector2 startPoint;
		private Vector2 startCenter;
		private Vector2 targetPoint;
		private float storedRotation;

		private float prevRotation;

		public int fadeIn = 15;

		float dist1;
		float dist2;

		public ref float Timer => ref Projectile.ai[0];
		public ref float Rotation => ref Projectile.ai[1];

		public Player Owner => Main.player[Projectile.owner];

		private float TargetRotation => (targetPoint - startCenter).ToRotation();
		private float TargetDistance => Vector2.Distance(targetPoint, startPoint);

		Vector2 Midpoint => startPoint + Vector2.UnitX.RotatedBy(storedRotation - Helper.CompareAngle(storedRotation, TargetRotation) * 0.5f) * TargetDistance / 2f;

		public override string Texture => AssetDirectory.Invisible;

		public override void SetDefaults()
		{
			Projectile.width = 16;
			Projectile.height = 16;
			Projectile.tileCollide = false;
			Projectile.timeLeft = 122;
			Projectile.friendly = true;
			Projectile.hostile = false;
			Projectile.penetrate = 20;
			Projectile.DamageType = DamageClass.Magic;

			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = -1;
		}

		public override bool? CanHitNPC(NPC target)
		{
			if (Projectile.timeLeft < 120)
				return base.CanHitNPC(target);

			return false;
		}

		private Vector2 PointOnSpline(float progress)
		{
			float factor = dist1 / (dist1 + dist2);

			if (progress < factor)
				return Vector2.Hermite(startPoint, Midpoint - startPoint, Midpoint, targetPoint - startPoint, progress * (1 / factor));

			if (progress >= factor)
				return Vector2.Hermite(Midpoint, targetPoint - startPoint, targetPoint, targetPoint - Midpoint, (progress - factor) * (1 / (1 - factor)));

			return Vector2.Zero;
		}

		private float ApproximateSplineLength(int steps, Vector2 start, Vector2 startTan, Vector2 end, Vector2 endTan)
		{
			float total = 0;
			Vector2 prevPoint = start;

			for (int k = 0; k < steps; k++)
			{
				var testPoint = Vector2.Hermite(start, startTan, end, endTan, k / (float)steps);
				total += Vector2.Distance(prevPoint, testPoint);

				prevPoint = testPoint;
			}

			return total;
		}

		public override void AI()
		{
			if (Owner.channel && Projectile.timeLeft >= 120)
			{
				if (Timer < fadeIn)
					Timer++;

				if (Owner == Main.LocalPlayer)
					Owner.GetModPlayer<ControlsPlayer>().mouseRotationListener = true;

				Rotation = Projectile.velocity.ToRotation() + (Owner.Center - Owner.GetModPlayer<ControlsPlayer>().mouseWorld).ToRotation() + MathHelper.Pi;

				Projectile.rotation = Rotation;
				Projectile.timeLeft = 121;
				Projectile.Center = Owner.Center + Vector2.UnitX.RotatedBy(Projectile.rotation) * (80 + (float)Math.Sin(Main.GameUpdateCount / 10f + Projectile.velocity.X * 6) * 10);
			}

			else if (Timer >= fadeIn)
			{
				Timer++;

				if (Timer == 16)
				{

					if (Main.myPlayer == Projectile.owner)
					{
						targetPoint = Main.MouseWorld;
						Rotation = Projectile.velocity.ToRotation() + (Owner.Center - Main.MouseWorld).ToRotation() + 3.14f;

						if (Math.Abs(Projectile.rotation - prevRotation) > 0.2f)
						{
							Projectile.netUpdate = true;
							prevRotation = Projectile.rotation;
						}
					}

					Projectile.rotation = Rotation;
					Projectile.Center = Owner.Center + Vector2.UnitX.RotatedBy(Projectile.rotation) * (80 + (float)Math.Sin(Main.GameUpdateCount / 10f + Projectile.velocity.X * 6) * 10);
				}

				if (startPoint == Vector2.Zero)
				{
					if (Owner == Main.LocalPlayer)
					{
						targetPoint = Main.MouseWorld;
						Projectile.netUpdate = true;
					}

					startPoint = Projectile.Center;
					startCenter = Owner.Center;
					storedRotation = Projectile.rotation;
					dist1 = ApproximateSplineLength(30, startPoint, Midpoint - startPoint, Midpoint, targetPoint - startPoint);
					dist2 = ApproximateSplineLength(30, Midpoint, targetPoint - startPoint, targetPoint, targetPoint - Midpoint);
				}

				int lifeTime = 122 - Projectile.timeLeft;
				int timeToMerge = (int)(Math.Min(0.4f, TargetDistance / 1200f) * 90);

				if (lifeTime < timeToMerge)
				{
					float progress = lifeTime / (float)timeToMerge;
					Projectile.Center = PointOnSpline(progress);
					Projectile.rotation = (Projectile.Center - PointOnSpline(progress + 0.05f)).ToRotation();

					Projectile.velocity = Vector2.Zero;

					if (Main.rand.NextBool(4))
					{
						var color = new Color(20 + (int)(Projectile.ai[1] / 4f * 100), 150, 255);
						var d = Dust.NewDustPerfect(Projectile.Center + Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(10), DustType<Dusts.Aurora>(), Vector2.Zero, 0, color * Main.rand.NextFloat(0.8f, 1.4f));
						d.customData = Main.rand.NextFloat(0.4f, 1.5f);
						d.fadeIn = 30;
					}

					if (Main.rand.NextBool(2))
					{
						var color = new Color(20 + (int)(Projectile.ai[1] / 4f * 100), 150, 255);
						var d = Dust.NewDustPerfect(Projectile.Center + Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(10), DustType<Dusts.Aurora>(), Vector2.Zero, 0, color * Main.rand.NextFloat(0.4f, 0.6f));
						d.customData = Main.rand.NextFloat(0.4f, 0.8f);
						d.fadeIn = 30;
					}
				}
				else
				{
					Projectile.velocity = Vector2.UnitX.RotatedBy(TargetRotation) * 15 * (1 - (lifeTime - timeToMerge) / (122f - timeToMerge));
					Projectile.rotation = TargetRotation;

					var color = new Color(20 + (int)(Projectile.ai[1] / 4f * 100), 150, 255);

					if (Projectile.timeLeft < 30)
						color *= Projectile.timeLeft / 30f;

					if (Main.rand.NextBool(10))
					{
						var d = Dust.NewDustPerfect(Projectile.Center + Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(10), DustType<Dusts.Aurora>(), Vector2.Zero, 0, color * Main.rand.NextFloat(0.8f, 1.4f));
						d.customData = Main.rand.NextFloat(0.4f, 1.5f);
						d.fadeIn = 30;
					}

					if (Main.rand.NextBool(5))
					{
						var d = Dust.NewDustPerfect(Projectile.Center + Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(10), DustType<Dusts.Aurora>(), Vector2.Zero, 0, color * Main.rand.NextFloat(0.4f, 0.6f));
						d.customData = Main.rand.NextFloat(0.4f, 0.8f);
						d.fadeIn = 30;
					}
				}

				if (Projectile.ai[1] != 1 && lifeTime == timeToMerge)
				{
					for (int k = 0; k < 10; k++)
						Dust.NewDustPerfect(Projectile.Center, DustType<Dusts.Glow>(), Vector2.One.RotatedByRandom(6.28f) * 5, 0, new Color(100, 200, 255), 0.25f);
				}
			}
			else
			{
				Timer -= 2;

				if (Timer <= 0)
					Projectile.timeLeft = 0;
			}
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			int lifeTime = 122 - Projectile.timeLeft;
			int timeToMerge = (int)(Math.Min(0.4f, TargetDistance / 1200f) * 90);

			if (Math.Abs(lifeTime - timeToMerge) <= 5 && Owner.HeldItem.ModItem is Coalescence)
			{
				var mi = Owner.HeldItem.ModItem as Coalescence;
				mi.manaCharge++;

				if (mi.manaCharge >= 5)
				{
					Helper.PlayPitched("Magic/HolyCastShort", 1, 0, Projectile.Center);

					var d = Dust.NewDustPerfect(Projectile.Center, DustType<Dusts.Aurora>(), Vector2.Zero, 0, new Color(50, 150, 255), 1);
					d.customData = 3f;
					d.rotation = Main.rand.NextFloat(6.28f);
				}
			}
		}

		public override void SendExtraAI(BinaryWriter writer)
		{
			writer.WritePackedVector2(targetPoint);
		}

		public override void ReceiveExtraAI(BinaryReader reader)
		{
			targetPoint = reader.ReadPackedVector2();
		}

		public void DrawAdditive(SpriteBatch spriteBatch)
		{
			Texture2D tex = Request<Texture2D>(AssetDirectory.MiscTextures + "DirectionalBeam").Value;
			Texture2D tex2 = Request<Texture2D>(AssetDirectory.VitricItem + "BossBowArrow").Value;
			var color = new Color(100 + (int)(Projectile.ai[1] / 4f * 100), 200, 255);

			if (Projectile.timeLeft < 30)
				color *= Projectile.timeLeft / 30f;

			spriteBatch.Draw(tex2, Projectile.Center - Main.screenPosition, null, color * Math.Min(Timer / fadeIn, 1), Projectile.rotation + 1.57f, tex2.Size() / 2, 0.5f, 0, 0);
			spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, color * (Math.Min(Timer / fadeIn, 1) * 0.5f), Projectile.rotation, new Vector2(tex.Width / 4f, tex.Height / 2f), 2, 0, 0);
		}
	}
}