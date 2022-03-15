using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Items.BaseTypes;
using StarlightRiver.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Items.Misc
{
	class SwordBook : SmartAccessory
	{
		public int comboState;

		public SwordBook() : base("Mantis Technique", "Allows execution of combos with broadswords\nRight click to parry with a broadsword") { }

		public override string Texture => AssetDirectory.MiscItem + "SwordBook";

		public override bool Autoload(ref string name)
		{
			StarlightItem.CanUseItemEvent += OverrideSwordEffects;
			return true;
		}

		public override void SafeSetDefaults()
		{
			item.rare = Terraria.ID.ItemRarityID.Orange;
		}

		private bool OverrideSwordEffects(Item item, Player player)
		{
			if (Equipped(player))
			{
				if (item.melee && item.shoot <= 0 && item.useStyle == Terraria.ID.ItemUseStyleID.SwingThrow && !item.noMelee)
				{
					if (Main.projectile.Any(n => n.active && n.type == ModContent.ProjectileType<SwordBookProjectile>() && n.owner == player.whoAmI))
						return false;

					int i = Projectile.NewProjectile(player.Center, Vector2.Zero, ModContent.ProjectileType<SwordBookProjectile>(), item.damage, item.knockBack, player.whoAmI);
					var proj = Main.projectile[i];

					proj.timeLeft = item.useAnimation * 4;
					proj.scale = item.scale;

					if (proj.modProjectile is SwordBookProjectile)
					{
						var modProj = proj.modProjectile as SwordBookProjectile;
						modProj.texture = Main.itemTexture[item.type];
						modProj.length = (float)Math.Sqrt(Math.Pow(modProj.texture.Width, 2) + Math.Pow(modProj.texture.Width, 2)) * item.scale;
						modProj.lifeSpan = item.useAnimation * 4;
						modProj.baseAngle = (Main.MouseWorld - player.Center).ToRotation() + (float)Math.PI / 4f;
						modProj.comboState = comboState;
					}

					float pitch = 1 - item.useAnimation / 60f;
					pitch += comboState * 0.1f;

					if (pitch >= 1)
						pitch = 1;

					Helpers.Helper.PlayPitched("Effects/HeavyWhooshShort", 1, pitch, player.Center);

					comboState++;
					comboState %= 4;

					return false;
				}
			}

			return true;
		}
	}

	class SwordBookProjectile : ModProjectile
	{
		public float length;
		public int comboState;
		public Texture2D texture;
		public int lifeSpan;
		public float baseAngle;
		public float holdOut;

		bool flipSprite = false;

		public float Progress => 1 - projectile.timeLeft / (float)lifeSpan;
		public int Direction => (Math.Abs(baseAngle - (float)Math.PI / 4f) < Math.PI / 2f) ? 1 : -1;
		public Player Owner => Main.player[projectile.owner];

		public override string Texture => AssetDirectory.Invisible;

		public override bool Autoload(ref string name)
		{
			StarlightPlayer.PostUpdateEvent += DoSwingAnimation;
			return true;
		}

		public override void SetDefaults()
		{
			projectile.friendly = true;
			projectile.melee = true;
			projectile.tileCollide = false;
			projectile.penetrate = -1;
			projectile.extraUpdates = 3;
		}

		private void DoSwingAnimation(Player player)
		{
			var instance = Main.projectile.FirstOrDefault(n => n.modProjectile is SwordBookProjectile && n.owner == player.whoAmI);

			if (instance != null && instance.active)
			{
				var mp = instance.modProjectile as SwordBookProjectile;

				switch (mp.comboState)
				{
					case 0:
						player.bodyFrame = player.bodyFrame = new Rectangle(0, 56 * (int)(3 + mp.Progress), 40, 56);
						break;

					case 1:
						player.bodyFrame = player.bodyFrame = new Rectangle(0, 56 * (int)(4 - mp.Progress * 4), 40, 56);
						break;

					case 2:
						player.bodyFrame = player.bodyFrame = new Rectangle(0, 56 * (int)(3 + mp.Progress), 40, 56);
						break;

					case 3:
						player.bodyFrame = player.bodyFrame = new Rectangle(0, 56 * (int)(mp.Progress * 4), 40, 56);
						break;
				}
			}
		}

		public override void AI()
		{
			projectile.Center = Owner.Center;
			Owner.direction = Direction;
			Owner.heldProj = projectile.whoAmI;

			switch (comboState)
			{
				case 0:
					projectile.rotation = baseAngle + (SwingEase(Progress) * 3f - 1.5f) * Direction;
					holdOut = (float)Math.Sin(Progress * 3.14f) * 16;

					break;

				case 1:
					flipSprite = true;
					projectile.rotation = baseAngle - (SwingEase(Progress) * 4f - 2f) * Direction;
					holdOut = (float)Math.Sin(Progress * 3.14f) * 24;

					break;

				case 2:

					if (Progress == 0)
					{
						projectile.timeLeft -= 20;
						lifeSpan -= 20;
					}

					projectile.rotation = baseAngle + (SwingEase(Progress) * 1f - 0.5f) * Direction;
					holdOut = (float)Math.Sin(SwingEase(Progress) * 3.14f) * 32;

					break;

				case 3:

					if (Progress == 0)
					{
						projectile.damage += (int)(projectile.damage * 1.5f);
						projectile.scale += 0.25f;
						projectile.timeLeft += 40;
						lifeSpan += 40;
					}

					projectile.rotation = baseAngle + (Helpers.Helper.BezierEase(Progress) * 6.28f * Direction);
					holdOut = Progress * 32;

					var rot = projectile.rotation + (Direction == 1 ? 0 : -(float)Math.PI / 2f);

					if (Main.rand.Next(6) == 0)
					{
						var pos = Vector2.Lerp(Owner.Center, Owner.Center + Vector2.UnitX.RotatedBy(rot) * (length + holdOut), Main.rand.NextFloat());
						Dust.NewDustPerfect(pos, ModContent.DustType<Dusts.AuroraFast>(), Vector2.Zero, 0, new Color(Main.rand.Next(255), Main.rand.Next(255), Main.rand.Next(255)), Main.rand.NextFloat(0.5f, 1));
					}

					break;
			}
		}

		public float SwingEase(float progress)
		{
			return (float)(3.386f * Math.Pow(progress, 3) - 7.259f * Math.Pow(progress, 2) + 4.873f * progress);
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			var rot = projectile.rotation + (Direction == 1 ? 0 : -(float)Math.PI / 2f);

			var start = Owner.Center;
			var end = Owner.Center + Vector2.UnitX.RotatedBy(rot) * (length + holdOut);

			if (Helpers.Helper.CheckLinearCollision(start, end, targetHitbox, out Vector2 colissionPoint))
			{
				for (int k = 0; k < 20; k++)
				{
					Dust.NewDustPerfect(colissionPoint, Terraria.ID.DustID.Blood, Vector2.Normalize(Owner.Center - colissionPoint).RotatedByRandom(0.5f) * Main.rand.NextFloat(3));
				}

				return true;
			}

			return null;
		}

		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
		{
			Helpers.Helper.PlayPitched(Helpers.Helper.IsFleshy(target) ? "Impacts/StabFleshy" : "Impacts/Clink", 1, Main.rand.NextFloat(), Owner.Center);
			Owner.GetModPlayer<StarlightPlayer>().Shake += 3;
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
		{		
			var origin = Direction == 1 ^ flipSprite ? new Vector2(0, texture.Height) : new Vector2(texture.Width, texture.Height);
			var effects = Direction == 1 ^ flipSprite ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
			var rot = projectile.rotation + (Direction == 1 ^ flipSprite ? 0 : (float)Math.PI / 2f);
			var pos = projectile.Center - Main.screenPosition + Vector2.UnitX.RotatedBy(rot) * holdOut * (flipSprite ? Direction * -1 : Direction);

			spriteBatch.Draw(texture, pos, default, lightColor, rot, origin, projectile.scale, effects, 0);
			return false;
		}
	}
}
