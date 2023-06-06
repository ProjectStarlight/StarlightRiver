using StarlightRiver.Content.Items.BaseTypes;
using StarlightRiver.Core.Systems.CameraSystem;
using StarlightRiver.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Terraria.GameContent;
using Terraria.Graphics.Effects;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Misc
{
	class SwordBook : SmartAccessory
	{
		public int comboState;

		private static List<int> blackListedSwords;

		public SwordBook() : base("Mantis Technique", "Teaches you the Art of the Sword, granting all sword weapons a new combo attack\n<right> to parry, reflecting projectiles") { }

		public override string Texture => AssetDirectory.MiscItem + "SwordBook";

		public override void Load()
		{
			StarlightItem.CanUseItemEvent += OverrideSwordEffects;
			StarlightItem.AltFunctionUseEvent += AllowRightClick;
		}

		public override void Unload()
		{
			StarlightItem.CanUseItemEvent -= OverrideSwordEffects;
			StarlightItem.AltFunctionUseEvent -= AllowRightClick;
		}

		public override void SetStaticDefaults()
		{
			blackListedSwords = new() { ModContent.ItemType<Moonstone.Moonfury>() };
		}

		public override void SafeSetDefaults()
		{
			Item.rare = ItemRarityID.Orange;
		}

		/// <summary>
		/// Allows the player to right click with spears that don't normally have them
		/// </summary>
		/// <param name="item"></param>
		/// <param name="player"></param>
		/// <returns></returns>
		private bool AllowRightClick(Item item, Player player)
		{
			if (Equipped(player))
			{
				if (item.DamageType.Type == DamageClass.Melee.Type && item.pick <= 0 && item.axe <= 0 && item.hammer <= 0 && item.useStyle == ItemUseStyleID.Swing && !item.noMelee)
					return true;
			}

			return false;
		}

		private bool OverrideSwordEffects(Item item, Player player)
		{
			if (Equipped(player))
			{
				if (item != player.HeldItem || blackListedSwords.Contains(item.type))
					return true;

				// Items that deal melee damage, are not tools, have the swung use style, and have a melee hitbox count as swords
				if (item.DamageType.Type == DamageClass.Melee.Type && item.pick <= 0 && item.axe <= 0 && item.hammer <= 0 && item.useStyle == ItemUseStyleID.Swing && !item.noMelee)
				{
					if (Main.projectile.Any(n => n.active && (n.type == ModContent.ProjectileType<SwordBookProjectile>() || n.type == ModContent.ProjectileType<SwordBookParry>()) && n.owner == player.whoAmI))
						return false;

					if (Main.mouseRight) // Parry on right click
					{
						int i = Projectile.NewProjectile(player.GetSource_ItemUse(item), player.Center, Vector2.Normalize(Main.MouseWorld - player.Center) * 20, ModContent.ProjectileType<SwordBookParry>(), item.damage, item.knockBack, player.whoAmI);
						Projectile proj = Main.projectile[i];

						proj.timeLeft = 30;

						if (proj.ModProjectile is SwordBookParry)
						{
							var modProj = proj.ModProjectile as SwordBookParry;
							modProj.texture = TextureAssets.Item[item.type].Value;
							modProj.length = (float)Math.Sqrt(Math.Pow(modProj.texture.Width, 2) + Math.Pow(modProj.texture.Width, 2)) * item.scale;
						}
					}
					else // Combo on left click
					{
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
					}

					return false;
				}
			}

			return true;
		}
	}

	class SwordBookProjectile : ModProjectile, IDrawPrimitive
	{
		// Info about the sword that created this projectile
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

		// These handle replicating the vanilla effects which we must do via reflection
		public static MethodInfo? playerItemCheckEmitUseVisuals_Info;
		public static Func<Player, Item, Rectangle, Rectangle>? playerItemCheckEmitUseVisuals;

		public static MethodInfo? ApplyNPCOnHitEffects_Info;
		public static Action<Player, Item, Rectangle, int, float, int, int, int>? ApplyNPCOnHitEffects;

		// Properties
		public float Progress => 1 - Projectile.timeLeft / (float)lifeSpan;
		public int Direction => (Math.Abs(baseAngle - (float)Math.PI / 4f) < Math.PI / 2f) ? 1 : -1;
		public Player Owner => Main.player[Projectile.owner];
		public Item Item => Owner.HeldItem; // The owner cant switch off untill this dies anyways since its a heldProj

		public override string Texture => AssetDirectory.Invisible;

		public override void Load()
		{
			StarlightPlayer.PostUpdateEvent += DoSwingAnimation;

			// We cache the MethodInfo of the methods we need to simulate vanilla effects here
			playerItemCheckEmitUseVisuals_Info = typeof(Player).GetMethod("ItemCheck_EmitUseVisuals", BindingFlags.NonPublic | BindingFlags.Instance);
			playerItemCheckEmitUseVisuals = (Func<Player, Item, Rectangle, Rectangle>)Delegate.CreateDelegate(
				typeof(Func<Player, Item, Rectangle, Rectangle>), playerItemCheckEmitUseVisuals_Info);

			ApplyNPCOnHitEffects_Info = typeof(Player).GetMethod("ApplyNPCOnHitEffects", BindingFlags.NonPublic | BindingFlags.Instance);
			ApplyNPCOnHitEffects = (Action<Player, Item, Rectangle, int, float, int, int, int>)Delegate.CreateDelegate(
				typeof(Action<Player, Item, Rectangle, int, float, int, int, int>), ApplyNPCOnHitEffects_Info);

		}

		public override void SetDefaults()
		{
			Projectile.friendly = true;
			Projectile.DamageType = DamageClass.Melee;
			Projectile.tileCollide = false;
			Projectile.penetrate = -1;
			Projectile.extraUpdates = 3;
		}

		/// <summary>
		/// Handles the player's body animation for the sword swings
		/// </summary>
		/// <param name="Player">The player to animate</param>
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

			Vector2 itemRectStart = Projectile.Center + Projectile.rotation.ToRotationVector2() * length * 0.5f;
			var itemRect = new Rectangle((int)itemRectStart.X, (int)itemRectStart.Y, 2, 2);
			itemRect.Inflate((int)length / 2, (int)length / 2);
			playerItemCheckEmitUseVisuals(Owner, Item, itemRect);

			if (comboState < 3 && Progress == 0 && Item.shoot > ProjectileID.None) //spawn projectile if relevant
				Projectile.NewProjectile(null, Owner.Center, Vector2.Normalize(Main.MouseWorld - Owner.Center) * Item.shootSpeed, Item.shoot, Projectile.damage, Projectile.knockBack, Projectile.owner);

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

					if (Progress == 0) //this swing is slightly faster
					{
						Projectile.timeLeft -= 20;
						lifeSpan -= 20;
					}

					Projectile.rotation = baseAngle + (SwingEase(Progress) * 2f - 1f) * Direction;
					holdOut = (float)Math.Sin(SwingEase(Progress) * 3.14f) * 32;

					break;

				case 3:

					if (Progress == 0) //this swing is slower, bigger, and more powerful
					{
						Projectile.damage += (int)(Projectile.damage * 1.5f);
						Projectile.scale += 0.25f;
						length += length * 0.25f;
						Projectile.timeLeft += 20;
						lifeSpan += 20;
					}

					Projectile.rotation = baseAngle + Direction + Helpers.Helper.BezierEase(Progress) * 6.28f * Direction;
					holdOut = Progress * 32;

					float rot = Projectile.rotation + (Direction == 1 ? 0 : -(float)Math.PI / 2f);

					if (Item.shoot > ProjectileID.None) //create projectile ring on circular slash
					{
						for (int k = 0; k < 12; k++)
						{
							if (Projectile.timeLeft == (int)(lifeSpan * (k / 12f)))
							{
								int i = Projectile.NewProjectile(null, Owner.Center, Vector2.UnitX.RotatedBy(Projectile.rotation) * Item.shootSpeed, Item.shoot, Projectile.damage / 4, Projectile.knockBack, Projectile.owner);
								Main.projectile[i].scale *= 0.75f;

								if (Main.projectile[i].extraUpdates > 0)
									Main.projectile[i].timeLeft = 20 * Main.projectile[i].extraUpdates;
								else
									Main.projectile[i].timeLeft = 20;
							}
						}
					}

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

		/// <summary>
		/// Simple easing function used for the sword swings
		/// </summary>
		/// <param name="progress">Input X</param>
		/// <returns>Output Y</returns>
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

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			Helpers.Helper.PlayPitched(Helpers.Helper.IsFleshy(target) ? "Impacts/StabFleshy" : "Impacts/Clink", 1, Main.rand.NextFloat(), Owner.Center);
			CameraSystem.shake += 3;

			// Simulate on-hit effects
			ItemLoader.OnHitNPC(Item, Owner, target, hit, damageDone);
			NPCLoader.OnHitByItem(target, Owner, Item, hit, damageDone);
			PlayerLoader.OnHitNPC(Owner, target, hit, damageDone);
			Owner.StatusToNPC(Item.type, target.whoAmI);
			float knockback = hit.Knockback;
			ApplyNPCOnHitEffects(Owner, Item, Projectile.Hitbox, Projectile.damage, knockback, target.whoAmI, Main.DamageVar(damageDone, Owner.luck), damageDone);

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
			Matrix view = Main.GameViewMatrix.TransformationMatrix;
			var projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

			effect.Parameters["time"].SetValue(Main.GameUpdateCount * 0.02f);
			effect.Parameters["repeats"].SetValue(8f);
			effect.Parameters["transformMatrix"].SetValue(world * view * projection);
			effect.Parameters["sampleTexture"].SetValue(ModContent.Request<Texture2D>("StarlightRiver/Assets/GlowTrail").Value);
			effect.Parameters["sampleTexture2"].SetValue(ModContent.Request<Texture2D>("StarlightRiver/Assets/Items/Moonstone/DatsuzeiFlameMap2").Value);

			trail?.Render(effect);
		}
	}

	class SwordBookParry : ModProjectile
	{
		public float length;
		public Texture2D texture;

		public float oldRot;
		public Vector2 oldPos;

		public ref float State => ref Projectile.ai[0];

		public Player Owner => Main.player[Projectile.owner];
		public Item Item => Owner.HeldItem;

		public override string Texture => AssetDirectory.Invisible;

		public override void SetDefaults()
		{
			Projectile.friendly = true;
			Projectile.DamageType = DamageClass.Melee;
			Projectile.tileCollide = false;
			Projectile.penetrate = -1;
		}

		public override void AI()
		{
			if (Projectile.velocity.Length() > 0) // Orient on spawn
			{
				Projectile.rotation = Projectile.velocity.ToRotation();
				Projectile.velocity *= 0;
			}

			Projectile.Center = Owner.Center + Vector2.UnitX.RotatedBy(Projectile.rotation) * (float)Math.Sin(Projectile.timeLeft / 20f * 1.57f) * 32;

			if (State == 0)
			{
				for (int k = 0; k < Main.maxProjectiles; k++)
				{
					Projectile proj = Main.projectile[k];

					if (proj.active && proj.hostile)
					{
						Vector2 first = Projectile.Center + Vector2.UnitX.RotatedBy(Projectile.rotation + 1.57f) * length / 2;
						Vector2 second = Projectile.Center + Vector2.UnitX.RotatedBy(Projectile.rotation - 1.57f) * length / 2;
						bool colliding = Helper.CheckLinearCollision(first, second, proj.Hitbox, out Vector2 collisionPoint);

						var normal = Vector2.Normalize(Projectile.Center - Owner.Center);

						first = Projectile.Center + normal * 16 + Vector2.UnitX.RotatedBy(Projectile.rotation + 1.57f) * length / 2;
						second = Projectile.Center + normal * 16 + Vector2.UnitX.RotatedBy(Projectile.rotation - 1.57f) * length / 2;
						colliding |= Helper.CheckLinearCollision(first, second, proj.Hitbox, out Vector2 collisionPoint2);

						if (!colliding)
							continue;

						if (proj.damage <= Item.damage * 2)
						{
							CameraSystem.shake += 20;
							CombatText.NewText(Owner.Hitbox, Color.Yellow, "Parry");

							if (Item.shoot != ProjectileID.None)
							{
								Projectile.NewProjectile(null, proj.Center, proj.velocity * -1, Item.shoot, Projectile.damage, Projectile.knockBack, Projectile.owner);

								for (int i = 0; i < 4; i++)
								{
									Projectile.NewProjectile(null, proj.Center, proj.velocity.RotatedByRandom(0.5f) * -Main.rand.NextFloat(0.8f, 1.0f), Item.shoot, Projectile.damage / 4, Projectile.knockBack, Projectile.owner);
								}
							}

							proj.active = false;
							State = 1;
							Projectile.timeLeft = 20;
							oldRot = Projectile.rotation;
							oldPos = Projectile.Center;
						}
						else
						{
							CameraSystem.shake += 20;
							CombatText.NewText(Owner.Hitbox, Color.Red, "Cant block!");
							State = 1;
							Projectile.timeLeft = 20;
							oldRot = Projectile.rotation;
							oldPos = Projectile.Center;
						}
					}
				}
			}

			if (State == 1)
			{
				Projectile.rotation -= 0.1f;
			}
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Vector2 origin = texture.Size() / 2;
			float rot = Projectile.rotation + 1.57f * 1.5f;
			Vector2 pos = Projectile.Center - Main.screenPosition;

			float opacity = 1;

			if (Projectile.timeLeft <= 5)
				opacity = Projectile.timeLeft / 5f;

			if (Projectile.timeLeft > 15)
				rot -= (Projectile.timeLeft - 15) * 0.05f;

			Main.spriteBatch.Draw(texture, pos, default, lightColor * opacity, rot, origin, Projectile.scale, 0, 0);

			if (State == 1)
			{
				float progress = Projectile.timeLeft / 20f;
				Main.spriteBatch.Draw(texture, oldPos - Main.screenPosition, default, Color.White * progress, oldRot + 1.57f * 1.5f, origin, Projectile.scale * (1 - progress) * 3, 0, 0);
			}

			return false;
		}
	}
}