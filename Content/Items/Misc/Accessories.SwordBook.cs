using StarlightRiver.Content.Items.BaseTypes;
using StarlightRiver.Core.Systems.CameraSystem;
using StarlightRiver.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.GameContent;
using Terraria.Graphics.Effects;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Misc
{
	class SwordBook : SmartAccessory
	{
		public int comboState;

		public SwordBook() : base("Mantis Technique", "Allows execution of combos with broadswords\nRight click to parry with a broadsword") { }

		public override string Texture => AssetDirectory.MiscItem + "SwordBook";

		public override void Load()
		{
			StarlightItem.CanUseItemEvent += OverrideSwordEffects;
		}

		public override void Unload()
		{
			StarlightItem.CanUseItemEvent -= OverrideSwordEffects;
		}

		public override void SafeSetDefaults()
		{
			Item.rare = ItemRarityID.Orange;
		}

		private bool OverrideSwordEffects(Item item, Player player)
		{
			if (Equipped(player))
			{
				if (item != player.HeldItem)
					return true;

				if (item.DamageType.Type == DamageClass.Melee.Type && item.pick <= 0 && item.axe <= 0 && item.hammer <= 0 && item.shoot <= ProjectileID.None && item.useStyle == Terraria.ID.ItemUseStyleID.Swing && !item.noMelee)
				{
					if (Main.projectile.Any(n => n.active && n.type == ModContent.ProjectileType<SwordBookProjectile>() && n.owner == player.whoAmI))
						return false;

					int i = Projectile.NewProjectile(player.GetSource_ItemUse(item), player.Center, Vector2.Zero, ModContent.ProjectileType<SwordBookProjectile>(), item.damage, item.knockBack, player.whoAmI);
					Projectile proj = Main.projectile[i];

					proj.timeLeft = item.useAnimation * 4;
					proj.scale = item.scale;

					if (proj.ModProjectile is SwordBookProjectile)
					{
						var modProj = proj.ModProjectile as SwordBookProjectile;
						modProj.trailColor = ItemColorUtility.GetColor(item.type);
						modProj.texture = TextureAssets.Item[item.type].Value;
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

					if (Item.UseSound.HasValue)
						Terraria.Audio.SoundEngine.PlaySound(Item.UseSound.Value, player.Center);

					comboState++;
					comboState %= 4;

					return false;
				}
			}

			return true;
		}
	}

	class SwordBookProjectile : ModProjectile, IDrawPrimitive
	{
		public float length;
		public int comboState;
		public Texture2D texture;
		public int lifeSpan;
		public float baseAngle;
		public float holdOut;
		public Color trailColor;

		private bool flipSprite = false;
		private List<Vector2> cache;
		private Trail trail;

		public float Progress => 1 - Projectile.timeLeft / (float)lifeSpan;
		public int Direction => (Math.Abs(baseAngle - (float)Math.PI / 4f) < Math.PI / 2f) ? 1 : -1;
		public Player Owner => Main.player[Projectile.owner];

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
			Projectile instance = Main.projectile.FirstOrDefault(n => n.ModProjectile is SwordBookProjectile && n.owner == Player.whoAmI);

			if (instance != null && instance.active)
			{
				var mp = instance.ModProjectile as SwordBookProjectile;

				switch (mp.comboState)
				{
					case 0:
						Player.bodyFrame = Player.bodyFrame = new Rectangle(0, 56 * (int)(3 + mp.Progress), 40, 56);
						break;

					case 1:
						Player.bodyFrame = Player.bodyFrame = new Rectangle(0, 56 * (int)(4 - mp.Progress * 4), 40, 56);
						break;

					case 2:
						Player.bodyFrame = Player.bodyFrame = new Rectangle(0, 56 * (int)(3 + mp.Progress), 40, 56);
						break;

					case 3:
						Player.bodyFrame = Player.bodyFrame = new Rectangle(0, 56 * (int)(mp.Progress * 4), 40, 56);
						break;
				}
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
					Projectile.rotation = baseAngle + (SwingEase(Progress) * 3f - 1.5f) * Direction;
					holdOut = (float)Math.Sin(Progress * 3.14f) * 16;

					break;

				case 1:
					flipSprite = true;
					Projectile.rotation = baseAngle - (SwingEase(Progress) * 4f - 2f) * Direction;
					holdOut = (float)Math.Sin(Progress * 3.14f) * 24;

					break;

				case 2:

					if (Progress == 0)
					{
						Projectile.timeLeft -= 20;
						lifeSpan -= 20;
					}

					Projectile.rotation = baseAngle + (SwingEase(Progress) * 1f - 0.5f) * Direction;
					holdOut = (float)Math.Sin(SwingEase(Progress) * 3.14f) * 32;

					break;

				case 3:

					if (Progress == 0)
					{
						Projectile.damage += (int)(Projectile.damage * 1.5f);
						Projectile.scale += 0.25f;
						length += length * 0.25f;
						Projectile.timeLeft += 40;
						lifeSpan += 40;
					}

					Projectile.rotation = baseAngle + Direction + Helpers.Helper.BezierEase(Progress) * 6.28f * Direction;
					holdOut = Progress * 32;

					float rot = Projectile.rotation + (Direction == 1 ? 0 : -(float)Math.PI / 2f);

					if (Main.rand.NextBool(6))
					{
						var pos = Vector2.Lerp(Owner.Center, Owner.Center + Vector2.UnitX.RotatedBy(rot) * (length + holdOut), Main.rand.NextFloat());
						Dust.NewDustPerfect(pos, ModContent.DustType<Dusts.AuroraFast>(), Vector2.Zero, 0, new Color(Main.rand.Next(255), 0, Main.rand.Next(255)), Main.rand.NextFloat(0.5f, 1));
					}

					break;
			}

			ManageCaches();
			ManageTrail();
		}

		public float SwingEase(float progress)
		{
			return (float)(3.386f * Math.Pow(progress, 3) - 7.259f * Math.Pow(progress, 2) + 4.873f * progress);
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			float rot = Projectile.rotation + (Direction == 1 ? 0 : -(float)Math.PI / 2f);

			Vector2 start = Owner.Center;
			Vector2 end = Owner.Center + Vector2.UnitX.RotatedBy(rot) * (length + holdOut);

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
			CameraSystem.shake += 3;

			target.velocity += Vector2.Normalize(target.Center - Owner.Center) * Projectile.knockBack * 2 * target.knockBackResist;
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Vector2 origin = Direction == 1 ^ flipSprite ? new Vector2(0, texture.Height) : new Vector2(texture.Width, texture.Height);
			SpriteEffects effects = Direction == 1 ^ flipSprite ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
			float rot = Projectile.rotation + (Direction == 1 ^ flipSprite ? 0 : (float)Math.PI / 2f);
			Vector2 pos = Projectile.Center - Main.screenPosition + Vector2.UnitX.RotatedBy(rot) * holdOut * (flipSprite ? Direction * -1 : Direction);

			Main.spriteBatch.Draw(texture, pos, default, lightColor, rot, origin, Projectile.scale, effects, 0);
			return false;
		}

		private void ManageCaches()
		{
			if (cache == null)
			{
				cache = new List<Vector2>();

				for (int i = 0; i < 50; i++)
				{
					cache.Add(Vector2.UnitX.RotatedBy(Projectile.rotation - Math.PI / 4f) * length * 0.75f);
				}
			}

			cache.Add(Vector2.UnitX.RotatedBy(Projectile.rotation - Math.PI / 4f) * length * 0.75f);

			while (cache.Count > 50)
			{
				cache.RemoveAt(0);
			}
		}

		private void ManageTrail()
		{
			trail ??= new Trail(Main.instance.GraphicsDevice, 50, new TriangularTip(40 * 4), factor => (float)Math.Min(factor, Progress) * length * 0.75f, factor =>
			{
				if (factor.X >= 0.98f)
					return Color.White * 0;

				return trailColor * (float)Math.Min(factor.X, Progress) * 0.5f * (float)Math.Sin(Progress * 3.14f);
			});

			var realCache = new Vector2[50];

			for (int k = 0; k < 50; k++)
			{
				realCache[k] = cache[k] + Owner.Center;
			}

			trail.Positions = realCache;
		}

		public void DrawPrimitives()
		{
			Effect effect = Filters.Scene["DatsuzeiTrail"].GetShader().Shader;

			var world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
			Matrix view = Main.GameViewMatrix.ZoomMatrix;
			var projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

			effect.Parameters["time"].SetValue(Main.GameUpdateCount * 0.02f);
			effect.Parameters["repeats"].SetValue(8f);
			effect.Parameters["transformMatrix"].SetValue(world * view * projection);
			effect.Parameters["sampleTexture"].SetValue(ModContent.Request<Texture2D>("StarlightRiver/Assets/GlowTrail").Value);
			effect.Parameters["sampleTexture2"].SetValue(ModContent.Request<Texture2D>("StarlightRiver/Assets/Items/Moonstone/DatsuzeiFlameMap2").Value);

			trail?.Render(effect);
		}
	}
}
