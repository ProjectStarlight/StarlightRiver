using Mono.Cecil;
using StarlightRiver.Content.Biomes;
using StarlightRiver.Content.Buffs;
using StarlightRiver.Content.Items.UndergroundTemple;
using StarlightRiver.Content.Projectiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Crimson
{
	internal class Lobotomizer : ModItem
	{
		public override string Texture => AssetDirectory.CrimsonItem + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Lobotomizer");
			Tooltip.SetDefault("Right click to throw a hallucinatory spear\nAttacks faster while hallucinating\nLonger reach while not hallucinating");
		}

		public override void SetDefaults()
		{
			Item.DamageType = DamageClass.Melee;
			Item.width = 32;
			Item.height = 32;
			Item.damage = 24;
			Item.crit = 6;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.useTime = 30;
			Item.useAnimation = 30;
			Item.noUseGraphic = true;
			Item.noMelee = true;
			Item.knockBack = 2f;
			Item.rare = ItemRarityID.Orange;
			Item.shoot = ModContent.ProjectileType<LobotomizerProjectile>();
			Item.shootSpeed = 1;

			Item.value = Item.sellPrice(gold: 1);
		}

		public override bool AltFunctionUse(Player player)
		{
			return true;
		}

		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
		{
			if (player.HasBuff<CrimsonHallucination>())
				type = ModContent.ProjectileType<LobotomizerFastProjectile>();
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			if (player.altFunctionUse == 2)
			{
				foreach(Projectile proj in Main.ActiveProjectiles)
				{
					if (proj.type == ModContent.ProjectileType<LobotomizerHallucination>() && proj.owner == player.whoAmI)
						proj.timeLeft = 30;
				}

				Projectile.NewProjectile(source, position, velocity * 20f, ModContent.ProjectileType<LobotomizerHallucination>(), 5, 0, player.whoAmI);
				return false;
			}

			return true;
		}
	}

	public class LobotomizerProjectile : SpearProjectile
	{
		public override string Texture => AssetDirectory.CrimsonItem + Name;

		public LobotomizerProjectile() : base(26, 60, 180) { }

		public override void SafeAI()
		{
			GraymatterBiome.forceGrayMatter = true;
		}

		public override void PostDraw(Color lightColor)
		{
			var tex = Assets.Misc.SpikeTell.Value;
			var source = new Rectangle(tex.Width / 2, 0, tex.Width / 2, tex.Height);

			float opacity = 1f;
			var realDuration = (Projectile.ModProjectile as SpearProjectile).RealDuration;

			if (Projectile.timeLeft < 6)
				opacity = Projectile.timeLeft / 6f;

			if (Projectile.timeLeft > realDuration - 6)
				opacity = (realDuration - Projectile.timeLeft) / 6f;

			Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, source, new Color(255, 255, 255, 0) * opacity * 0.1f, Projectile.rotation - 1.57f * 0.5f, new Vector2(tex.Width / 4f, tex.Height * 0.8f), 0.5f, 0, 0);

			Lighting.AddLight(Projectile.Center, Color.White.ToVector3() * 0.4f);
		}
	}

	public class LobotomizerFastProjectile : SpearProjectile
	{
		public override string Texture => AssetDirectory.CrimsonItem + Name;

		public LobotomizerFastProjectile() : base(15, 0, 140) { }

		public override void SafeAI()
		{
			Projectile.usesLocalNPCImmunity = true;

			var player = Main.player[Projectile.owner];

			if (Projectile.timeLeft == 5 && Projectile.ai[2] < 2)
			{
				float rot = Projectile.ai[2] == 0 ? 0.05f : -0.1f;
				Projectile.NewProjectile(Projectile.GetSource_FromThis(), player.Center, Projectile.velocity.RotatedBy(rot), ModContent.ProjectileType<LobotomizerFastProjectile>(), Projectile.damage, 0, player.whoAmI, 0, 0, Projectile.ai[2] + 1);
			}
		}

		public override void PostDraw(Color lightColor)
		{
			var tex = Assets.Misc.SpikeTell.Value;
			var source = new Rectangle(tex.Width / 2, 0, tex.Width / 2, tex.Height);

			float opacity = 1f;
			var realDuration = (Projectile.ModProjectile as SpearProjectile).RealDuration;

			if (Projectile.timeLeft < 6)
				opacity = Projectile.timeLeft / 6f;

			if (Projectile.timeLeft > realDuration - 6)
				opacity = (realDuration - Projectile.timeLeft) / 6f;

			Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, source, new Color(255, 255, 255, 0) * opacity * 0.5f, Projectile.rotation - 1.57f * 0.5f, new Vector2(tex.Width / 4f, tex.Height * 0.7f), 0.8f, 0, 0);
		}
	}

	public class LobotomizerHallucination : ModProjectile
	{
		public Vector2 savedOff;
		public NPC embedded;

		public override string Texture => AssetDirectory.CrimsonItem + "LobotomizerProjectile";

		public ref float Radius => ref Projectile.ai[0];
		public ref float State => ref Projectile.ai[1];

		public override void Load()
		{
			GraymatterBiome.onDrawHallucinationMap += DrawSpearHallucinations;
		}

		public override void SetStaticDefaults()
		{
			ProjectileID.Sets.TrailCacheLength[Type] = 10;
			ProjectileID.Sets.TrailingMode[Type] = 1;
		}

		public override void SetDefaults()
		{
			Projectile.penetrate = -1;
			Projectile.timeLeft = 400;
			Projectile.width = 15;
			Projectile.height = 15;
			Projectile.tileCollide = true;
			Projectile.friendly = true;
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			Projectile.position += Projectile.velocity * 2;
			Projectile.velocity *= 0;
			Projectile.friendly = false;

			State = 1;

			return false;
		}

		public override void AI()
		{
			GraymatterBiome.forceGrayMatter = true;

			if (State == 0)
			{
				Projectile.velocity.Y += 0.4f;
				Projectile.rotation = Projectile.velocity.ToRotation() + 1.57f * 1.5f;
			}
			else if (State == 1)
			{
				Projectile.velocity *= 0;
			}
			else if (State == 2)
			{
				if (!embedded.active || embedded is null)
				{
					State = 0;
					Projectile.timeLeft = 30;
				}

				Projectile.Center = embedded.Center + savedOff;
			}

			if (State > 0 && Radius < 300 && Projectile.timeLeft > 30)
				Radius += 10;

			if (Radius > 0 && Projectile.timeLeft <= 30)
				Radius -= 10;

			// Allows this to make it's owner hallucinate
			var player = Main.player[Projectile.owner];

			if (Vector2.Distance(player.Center, Projectile.Center) <= Radius)
				player.AddBuff(ModContent.BuffType<CrimsonHallucination>(), 2);

			Lighting.AddLight(Projectile.Center, Color.White.ToVector3() * 1.5f);
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			Projectile.position += Projectile.velocity * 2;
			Projectile.velocity *= 0;
			Projectile.friendly = false;

			savedOff = Projectile.Center - target.Center;
			embedded = target;
			State = 2;
		}

		public override bool PreDraw(ref Color lightColor)
		{
			var tex = Assets.Items.Crimson.LobotomizerProjectile.Value;
			Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition + new Vector2(0, Main.player[Projectile.owner].gfxOffY), tex.Frame(), lightColor, Projectile.rotation, Vector2.Zero, Projectile.scale, 0, 0);
			return false;
		}

		public override void PostDraw(Color lightColor)
		{
			var tex = Assets.Misc.GlowRing.Value;
			Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, new Color(255, 255, 255, 0) * 0.1f, 0, tex.Size() / 2f, Radius * 2f / tex.Width, 0, 0);
		}

		private void DrawSpearHallucinations(SpriteBatch batch)
		{
			foreach(Projectile proj in Main.ActiveProjectiles)
			{
				if (proj.type == Type)
				{
					var tex = Assets.Misc.StarView.Value;
					batch.Draw(tex, proj.Center - Main.screenPosition, null, new Color(255, 255, 255, 0), 0, tex.Size() / 2f, (proj.ModProjectile as LobotomizerHallucination).Radius * 7f / tex.Width, 0, 0);
				}

				if (proj.type == ModContent.ProjectileType<LobotomizerProjectile>() || proj.type == ModContent.ProjectileType<LobotomizerFastProjectile>())
				{
					var tex = Assets.Misc.SpikeTell.Value;
					var source = new Rectangle(tex.Width / 2, 0, tex.Width / 2, tex.Height);

					float opacity = 1f;
					var realDuration = (proj.ModProjectile as SpearProjectile).RealDuration;

					if (proj.timeLeft < 6)
						opacity = Projectile.timeLeft / 6f;

					if (Projectile.timeLeft > realDuration - 6)
						opacity = (realDuration - proj.timeLeft) / 6f;

					batch.Draw(tex, proj.Center - Main.screenPosition, source, new Color(255, 255, 255, 0) * opacity, proj.rotation - 1.57f * 0.5f, new Vector2(tex.Width / 4f, tex.Height * 0.8f), 1f, 0, 0);
				}
			}
		}
	}
}
