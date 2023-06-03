using StarlightRiver.Content.Buffs;
using System.Collections.Generic;
using System.Linq;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Items.Misc
{
	public class ElectroArrow : ModItem
	{
		public override string Texture => AssetDirectory.MiscItem + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Electro Arrow");
			Tooltip.SetDefault("Chains to nearby enemies\nInflicts Overcharged, greatly lowering enemy defense");
		}

		public override void SetDefaults()
		{
			Item.damage = 1;
			Item.DamageType = DamageClass.Ranged;
			Item.width = 8;
			Item.height = 8;
			Item.maxStack = 999;
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
			tooltips.FirstOrDefault(n => n.Name == "CritChance").Text = "Cannot critically strike";
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

	internal class ElectroArrowProjectile : ModProjectile, IDrawAdditive
	{
		Vector2 savedPos = Vector2.Zero;
		int blacklistNPC = -1;
		readonly List<Vector2> nodes = new();

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

			Projectile.extraUpdates = 6;
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

			if (Main.GameUpdateCount % 3 == 0) //rebuild electricity nodes
			{
				nodes.Clear();

				Vector2 point1 = savedPos;
				Vector2 point2 = Projectile.Center;
				int nodeCount = (int)Vector2.Distance(point1, point2) / 30;

				for (int k = 1; k < nodeCount; k++)
				{
					nodes.Add(Vector2.Lerp(point1, point2, k / (float)nodeCount) +
						(k == nodes.Count - 1 ? Vector2.Zero : Vector2.Normalize(point1 - point2).RotatedBy(1.58f) * (Main.rand.NextFloat(2) - 1) * 30 / 3));
				}

				nodes.Add(point2);
			}

			if (Projectile.timeLeft == 1)
				PreKill(Projectile.timeLeft);
		}

		public void DrawAdditive(SpriteBatch sb)
		{
			Vector2 point1 = savedPos;
			Vector2 point2 = Projectile.Center;

			if (point1 == Vector2.Zero || point2 == Vector2.Zero)
				return;

			Texture2D tex = Request<Texture2D>("StarlightRiver/Assets/GlowTrail").Value;

			for (int k = 1; k < nodes.Count; k++)
			{
				Vector2 prevPos = k == 1 ? point1 : nodes[k - 1];

				var target = new Rectangle((int)(prevPos.X - Main.screenPosition.X), (int)(prevPos.Y - Main.screenPosition.Y), (int)Vector2.Distance(nodes[k], prevPos) + 1, 10);
				var origin = new Vector2(0, tex.Height / 2);
				float rot = (nodes[k] - prevPos).ToRotation();
				Color color = new Color(200, 230, 255) * (Projectile.extraUpdates == 0 ? Projectile.timeLeft / 15f : 1);

				sb.Draw(tex, target, null, color, rot, origin, 0, 0);

				if (Main.rand.NextBool(30))
					Dust.NewDustPerfect(prevPos + new Vector2(0, 32), DustType<Dusts.GlowLine>(), Vector2.Normalize(nodes[k] - prevPos) * Main.rand.NextFloat(-6, -4), 0, new Color(100, 150, 200), 0.5f);
			}

			Color glowColor = new Color(100, 150, 200) * 0.45f * (Projectile.extraUpdates == 0 ? Projectile.timeLeft / 15f : 1);
			sb.Draw(tex, new Rectangle((int)(point1.X - Main.screenPosition.X), (int)(point1.Y - Main.screenPosition.Y), (int)Vector2.Distance(point1, point2), 100), null, glowColor, (point2 - point1).ToRotation(), new Vector2(0, tex.Height / 2), 0, 0);
		}

		public override bool? CanHitNPC(NPC target)
		{
			return target.whoAmI != blacklistNPC;
		}

		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
		{
			modifiers.FinalDamage *= 0.25f;
			modifiers.DisableCrit();
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			target.AddBuff(BuffType<Overcharge>(), 300);

			if (Projectile.penetrate <= 1)
				return;

			for (int k = 0; k < Main.maxNPCs; k++)
			{
				NPC NPC = Main.npc[k];
				if (NPC.active && NPC.chaseable && !NPC.HasBuff(BuffType<Overcharge>()) && Vector2.Distance(NPC.Center, target.Center) < 500)
				{
					var proj = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), target.Center, Vector2.Normalize(target.Center - NPC.Center) * -6, ProjectileType<ElectroArrowProjectile>(), 20, 0, Projectile.owner, 2, 100);
					proj.penetrate = Projectile.penetrate - 1;
					proj.tileCollide = false;
					(proj.ModProjectile as ElectroArrowProjectile).blacklistNPC = target.whoAmI;
					break;
				}
			}

			PreKill(Projectile.timeLeft);
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			if (Projectile.extraUpdates == 0)
				return true;

			Projectile.velocity *= 0;
			Projectile.friendly = false;
			Projectile.timeLeft = 15;
			Projectile.extraUpdates = 0;

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