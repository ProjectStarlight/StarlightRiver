using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Items.BaseTypes;
using StarlightRiver.Core;
using StarlightRiver.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Items.Misc
{
	class AxeBook : SmartAccessory
	{
		public int comboState;

		public AxeBook() : base("Tiger Technique", "Allows execution of combos with axes\nFinal strike rends enemy armor\nRight click to throw your axe") { }

		public override string Texture => AssetDirectory.MiscItem + "AxeBook";

		public override void Load()
		{
			StarlightItem.CanUseItemEvent += OverrideAxeEffects;
		}

		public override void Unload()
		{
			StarlightItem.CanUseItemEvent -= OverrideAxeEffects;
		}

		public override void SafeSetDefaults()
		{
			Item.rare = Terraria.ID.ItemRarityID.Orange;
		}

		private bool OverrideAxeEffects(Item item, Player player)
		{
			if (Equipped(player))
			{
				if (item != player.HeldItem)
					return true;

				if (item.CountsAsClass(DamageClass.Melee) && item.pick <= 0 && item.axe > 0 && item.hammer <= 0 && item.shoot <= 0 && item.useStyle == Terraria.ID.ItemUseStyleID.Swing && !item.noMelee)
				{
					if (Main.projectile.Any(n => n.active && n.type == ModContent.ProjectileType<AxeBookProjectile>() && n.owner == player.whoAmI))
						return false;

					int i = Projectile.NewProjectile(player.GetSource_ItemUse(item), player.Center, Vector2.Zero, ModContent.ProjectileType<AxeBookProjectile>(), item.damage * 2, item.knockBack, player.whoAmI);
					var proj = Main.projectile[i];

					proj.timeLeft = item.useAnimation * 5;
					proj.scale = item.scale;

					if (proj.ModProjectile is AxeBookProjectile)
					{
						var modProj = proj.ModProjectile as AxeBookProjectile;
						modProj.trailColor = ItemColorUtility.GetColor(item.type);
						modProj.texture = TextureAssets.Item[item.type].Value;
						modProj.length = (float)Math.Sqrt(Math.Pow(modProj.texture.Width, 2) + Math.Pow(modProj.texture.Width, 2)) * item.scale;
						modProj.lifeSpan = item.useAnimation * 5;
						modProj.baseAngle = (Main.MouseWorld - player.Center).ToRotation() + (float)Math.PI / 4f;
						modProj.comboState = comboState;
					}

					float pitch = 1 - item.useAnimation / 60f;
					pitch += comboState * 0.1f;

					if (pitch >= 1)
						pitch = 1;

					Helpers.Helper.PlayPitched("Effects/HeavyWhooshShort", 1, pitch, player.Center);

					if (Item.UseSound.HasValue)
						Terraria.Audio.SoundEngine.PlaySound(Item.UseSound.Value, player.Center);

					comboState++;
					comboState %= 3;

					return false;
				}
			}

			return true;
		}
	}

	class AxeBookProjectile : ModProjectile, IDrawPrimitive
	{
		public float length;
		public int comboState;
		public Texture2D texture;
		public int lifeSpan;
		public float baseAngle;
		public float holdOut;
		public Color trailColor;

		private int freeze = 0;
		private bool flipSprite = false;
		private List<Vector2> cache;
		private Trail trail;

		public float Progress => 1 - Projectile.timeLeft / (float)lifeSpan;
		public int Direction => (Math.Abs(baseAngle - (float)Math.PI / 4f) < Math.PI / 2f) ? 1 : -1;
		public Player Owner => Main.player[Projectile.owner];
		public float FadeOut => Projectile.timeLeft < 60 ? Projectile.timeLeft / 60f : 1;

		public override string Texture => AssetDirectory.Invisible;

		public override void Load()
		{
			StarlightPlayer.PostUpdateEvent += DoSwingAnimation;
		}

		public override void SetDefaults()
		{
			Projectile.friendly = true;
			Projectile.DamageType = DamageClass.Melee;
			Projectile.tileCollide = false;
			Projectile.penetrate = -1;
			Projectile.extraUpdates = 3;
		}

		private void DoSwingAnimation(Player Player)
		{
			var instance = Main.projectile.FirstOrDefault(n => n.ModProjectile is AxeBookProjectile && n.owner == Player.whoAmI);

			if (instance != null && instance.active)
			{
				var mp = instance.ModProjectile as AxeBookProjectile;

				Player.SetCompositeArmFront(true, 0, instance.rotation - 1.57f);
			}
		}

		public override void AI()
		{
			Projectile.Center = Owner.Center;
			Owner.direction = Direction;
			Owner.heldProj = Projectile.whoAmI;

			switch (comboState)
			{
				case 0:

					if (Progress == 0)
					{
						Projectile.timeLeft -= 20;
						lifeSpan -= 20;
					}

					Projectile.rotation = baseAngle + (SwingEase(Progress) * 3f - 1.5f) * Direction;
					holdOut = (float)Math.Sin(SwingEase(Progress) * 3.14f) * length * 0.2f;

					break;

				case 1:

					Projectile.scale = 1.2f;
					Projectile.rotation = baseAngle + (SwingEase(Progress) * 3f - 1.5f) * Direction;
					holdOut = (float)Math.Sin(SwingEase(Progress) * 3.14f) * length * 0.3f;

					break;

				case 2:

					if (Progress == 0)
					{
						Projectile.timeLeft -= 60;
						lifeSpan -= 60;
						Projectile.damage *= 2;
					}

					var rot = Projectile.rotation + (Direction == 1 ? 0 : -(float)Math.PI / 2f);
					var end = Owner.Center + Vector2.UnitX.RotatedBy(rot) * (length * Projectile.scale + holdOut) * .75f;
					Dust.NewDust(end - Vector2.One * 5, 10, 10, ModContent.DustType<Dusts.Cinder>(), 0, 0, 0, GetSwingColor(Progress));

					Projectile.scale = 1.5f;
					Projectile.rotation = baseAngle + (SwingEase(Progress) * 3f - 1.5f) * Direction;
					holdOut = (float)Math.Sin(SwingEase(Progress) * 3.14f) * length * 0.4f;

					break;
			}

			
			ManageTrail();

			if (freeze > 0)
			{
				freeze--;
				Projectile.timeLeft++;
				return;
			}

			ManageCaches();
		}

		public float SwingEase(float progress)
		{
			return (float)(11.904f * Math.Pow(progress, 4) -30.9524f * Math.Pow(progress, 3) + 25.5952f * Math.Pow(progress, 2) - 5.54762f * progress);
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			var rot = Projectile.rotation + (Direction == 1 ? 0 : -(float)Math.PI / 2f);

			var start = Owner.Center;
			var end = Owner.Center + Vector2.UnitX.RotatedBy(rot) * (length * Projectile.scale + holdOut);

			if (freeze <= 0 && Helpers.Helper.CheckLinearCollision(start, end, targetHitbox, out Vector2 colissionPoint))
			{
				for (int k = 0; k < 20; k++)
				{
					Dust.NewDustPerfect(colissionPoint, ModContent.DustType<Dusts.GlowLine>(), Vector2.Normalize(Owner.Center - colissionPoint).RotatedByRandom(0.5f) * -Main.rand.NextFloat(12), 0, GetSwingColor(1));

					for (int n = 0; n < 3; n++)
					{
						Dust.NewDustPerfect(colissionPoint, Terraria.ID.DustID.Blood, Vector2.Normalize(Owner.Center - colissionPoint).RotatedByRandom(1.5f) * -Main.rand.NextFloat(12));
					}
				}

				return true;
			}

			return null;
		}

		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
		{
			Helpers.Helper.PlayPitched(Helpers.Helper.IsFleshy(target) ? "Impacts/StabFleshy" : "Impacts/Clink", 1, Main.rand.NextFloat(), Owner.Center);
			Core.Systems.CameraSystem.Shake += 4;

			if (comboState == 2 && target.defense > 0)
				target.defense--;

			target.velocity += Vector2.Normalize(target.Center - Owner.Center) * Projectile.knockBack * 2 * target.knockBackResist;
			target.immune[0] += 22;

			freeze += 20;
		}

		public override bool PreDraw(ref Color lightColor)
		{		
			var origin = Direction == 1 ^ flipSprite ? new Vector2(0, texture.Height) : new Vector2(texture.Width, texture.Height);
			var effects = Direction == 1 ^ flipSprite ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
			var rot = Projectile.rotation + (Direction == 1 ^ flipSprite ? 0 : (float)Math.PI / 2f);
			var pos = Projectile.Center - Main.screenPosition + Vector2.UnitX.RotatedBy(rot) * holdOut * Projectile.scale * (flipSprite ? Direction * -1 : Direction);

			Main.spriteBatch.Draw(texture, pos, default, lightColor * FadeOut, rot, origin, Projectile.scale, effects, 0);
			return false;
		}

		private void ManageCaches()
		{
			if (cache == null)
			{
				cache = new List<Vector2>();

				for (int i = 0; i < 50; i++)
				{
					cache.Add(Vector2.UnitX.RotatedBy(Projectile.rotation - Math.PI / 4f) * (length + holdOut) * Projectile.scale * 0.7f);
				}
			}

			cache.Add(Vector2.UnitX.RotatedBy(Projectile.rotation - Math.PI / 4f) * (length + holdOut) * Projectile.scale * 0.7f);

			while (cache.Count > 50)
			{
				cache.RemoveAt(0);
			}
		}

		private Color GetSwingColor(float factor)
		{
			if (comboState == 1)
				return Color.Lerp(trailColor, Color.Red, Progress * 0.5f) * (float)Math.Min(factor, Progress) * 0.9f * (float)Math.Sin(Progress * 3.14f) * FadeOut;

			if (comboState == 2)
				return Color.Lerp(trailColor, Color.Red, Progress) * FadeOut;

			return trailColor * (float)Math.Min(factor, Progress) * 0.9f * (float)Math.Sin(Progress * 3.14f) * FadeOut;
		}

		private void ManageTrail()
		{
			trail = trail ?? new Trail(Main.instance.GraphicsDevice, 50, new TriangularTip(40 * 4), factor => (float)Math.Min(factor, Progress) * length * 1.25f, factor =>
			{
				if (factor.X >= 0.98f)
					return Color.White * 0;

				return GetSwingColor(factor.X);
			});

			if (cache != null)
			{
				Vector2[] realCache = new Vector2[50];

				for (int k = 0; k < 50; k++)
				{
					realCache[k] = cache[k] + Owner.Center;
				}

				trail.Positions = realCache;
			}
		}

		public void DrawPrimitives()
		{
			Effect effect = Filters.Scene["DatsuzeiTrail"].GetShader().Shader;

			Matrix world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
			Matrix view = Main.GameViewMatrix.ZoomMatrix;
			Matrix projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

			effect.Parameters["time"].SetValue(Main.GameUpdateCount * 0.02f);
			effect.Parameters["repeats"].SetValue(2f);
			effect.Parameters["transformMatrix"].SetValue(world * view * projection);
			effect.Parameters["sampleTexture"].SetValue(ModContent.Request<Texture2D>("StarlightRiver/Assets/EnergyTrail").Value);
			effect.Parameters["sampleTexture2"].SetValue(ModContent.Request<Texture2D>("StarlightRiver/Assets/MagicPixel").Value);

			trail?.Render(effect);
		}
	}
}
