using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Items.BaseTypes;
using StarlightRiver.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;
using Terraria;
using Terraria.ID;
using Microsoft.Xna.Framework;
using StarlightRiver.Helpers;

namespace StarlightRiver.Content.Items.Misc
{
	public class ULTRAPILLS : CursedAccessory
	{
		public override string Texture => AssetDirectory.MiscItem + Name;
		public ULTRAPILLS() : base(ModContent.Request<Texture2D>(AssetDirectory.MiscItem + "ULTRAPILLS").Value) { }

		public override void Load()
		{
			StarlightPlayer.UpdateLifeRegenEvent += RemoveLifeRegen;
			StarlightPlayer.NaturalLifeRegenEvent += RemoveNaturalLifeRegen;
			StarlightPlayer.CanUseItemEvent += PreventHealingItems;
			StarlightPlayer.OnHitNPCEvent += OnHitNPCItem;
			StarlightPlayer.OnHitNPCWithProjEvent += OnHitNPCProj;
			StarlightItem.GetHealLifeEvent += PreventHealingItemsAgain;
			StarlightItem.OnPickupEvent += PreventHeartPickups;
		}

		private void OnHitNPCProj(Player player, Projectile proj, NPC target, int damage, float knockback, bool crit)
		{
			if (!Equipped(player))
				return;

			if (Main.rand.NextBool(4))
				player.Heal(1);

			if (target.life <= 0)
			{
				for (int i = 0; i < Main.rand.Next(7, 14); i++)
				{
					Projectile.NewProjectile(player.GetSource_OnHit(target), target.Center, Vector2.UnitY.RotatedByRandom(0.5f) * -Main.rand.NextFloat(5f, 10f), ModContent.ProjectileType<UltrapillBlood>(), 0, 0f, player.whoAmI);
				}
			}
		}

		private void OnHitNPCItem(Player player, Item Item, NPC target, int damage, float knockback, bool crit)
		{
			if (!Equipped(player))
				return;

			if (Main.rand.NextBool(4))
				player.Heal(1);

			if (target.lifeMax <= 0)
			{
				for (int i = 0; i < Main.rand.Next(7, 14); i++)
				{
					Projectile.NewProjectile(player.GetSource_OnHit(target), target.Center, Vector2.UnitY.RotatedByRandom(0.5f) * -Main.rand.NextFloat(5f, 10f), ModContent.ProjectileType<UltrapillBlood>(), 0, 0f, player.whoAmI);
				}
			}
		}

		private bool PreventHeartPickups(Item Item, Player Player)
		{
			if (Equipped(Player) && (Item.type == ItemID.Heart || Item.type == ItemID.CandyApple || Item.type == ItemID.CandyCane))
				return false;

			return true;
		}

		private void PreventHealingItemsAgain(Item Item, Player Player, bool quickHeal, ref int healValue)
		{
			if (Equipped(Player))
				healValue = 0;
		}

		private bool PreventHealingItems(Player player, Item item)
		{
			if (Equipped(player) && item.healLife > 0)
				return false;

			return true;
		}

		private void RemoveNaturalLifeRegen(Player player, ref float regen)
		{
			if (Equipped(player))
				regen *= 0;
		}

		private void RemoveLifeRegen(Player player)
		{
			if (Equipped(player) && player.lifeRegen > 0)
				player.lifeRegen = 0;
		}

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("ULTRAPILLS");
			Tooltip.SetDefault("Cursed: You cannot regenerate health by normal means\nStriking enemies occasionally leeches life\nKilling enemies causes them to explode into blood which heals and provides a substantial boost to movement speed\n'Don't do drugs kids'");
		}
	}

	class UltrapillBlood : ModProjectile, IDrawPrimitive
	{
		private List<Vector2> cache;
		private Trail trail;
		public Player Owner => Main.player[Projectile.owner];
		public override string Texture => AssetDirectory.MiscItem + Name;

		public override void SetDefaults()
		{
			Projectile.width = Projectile.height = 16;
			Projectile.tileCollide = true;

			Projectile.timeLeft = 120;
			Projectile.hostile = false;
			Projectile.friendly = false;
		}

		public override void AI()
		{
			if (!Main.dedServ)
			{
				ManageCaches();
				ManageTrail();
			}

			if (Projectile.velocity.Y > 16f)
				Projectile.velocity.Y = 16f;
			else
				Projectile.velocity.Y += 0.25f;

			Projectile.velocity.X *= 0.99f;

			Projectile.rotation = Projectile.velocity.ToRotation();

			if (Projectile.Hitbox.Intersects(Owner.Hitbox))
			{
				Owner.Heal(Main.rand.Next(3, 9));
				Projectile.Kill();
			}
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
			Main.spriteBatch.Draw(tex, Projectile.Center - Projectile.velocity - Main.screenPosition, null, new Color(150, 10, 10), Projectile.rotation, tex.Size() / 2f, 0.65f, SpriteEffects.None, 0f);
			return false;
		}

		private void ManageCaches()
		{
			if (cache == null)
			{
				cache = new List<Vector2>();
				for (int i = 0; i < 20; i++)
				{
					cache.Add(Projectile.Center);
				}
			}

			cache.Add(Projectile.Center);

			while (cache.Count > 20)
			{
				cache.RemoveAt(0);
			}
		}

		private void ManageTrail()
		{
			trail = trail ?? new Trail(Main.instance.GraphicsDevice, 20, new RoundedTip(2), factor => 4.5f * factor, factor =>
			{
				return Color.Lerp(new Color(50, 10, 10), new Color(35, 1, 1), factor.X);
			});

			trail.Positions = cache.ToArray();
			trail.NextPosition = Projectile.Center;
		}
		public void DrawPrimitives()
		{
			Effect effect = Terraria.Graphics.Effects.Filters.Scene["pixelationFull"].GetShader().Shader;

			Matrix world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
			Matrix view = Main.GameViewMatrix.ZoomMatrix;
			Matrix projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

			effect.Parameters["pixelation"].SetValue(0.1f);
			effect.Parameters["resolution"].SetValue(1f);

			trail?.Render(effect);
		}
	}
}
