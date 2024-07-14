using StarlightRiver.Content.Items.BaseTypes;
using StarlightRiver.Core.Systems.CameraSystem;
using StarlightRiver.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Terraria;
using Terraria.DataStructures;
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
			base.SetStaticDefaults();
			blackListedSwords = new() { ModContent.ItemType<Moonstone.Moonfury>() };

			ItemID.Sets.ShimmerTransformToItem[Type] = ModContent.ItemType<SpearBook>();
		}

		public override void SafeSetDefaults()
		{
			Item.rare = ItemRarityID.Orange;
			Item.value = Item.sellPrice(gold: 2);
		}

		/// <summary>
		/// Allows the player to right click with swords that don't normally have them
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

					if (Main.mouseRight && Main.myPlayer == player.whoAmI) // Parry on right click
					{
						Projectile.NewProjectile(player.GetSource_ItemUse(item), player.Center, Vector2.Normalize(Main.MouseWorld - player.Center) * 20, ModContent.ProjectileType<SwordBookParry>(), item.damage, item.knockBack, player.whoAmI);
					}
					else if (Main.myPlayer == player.whoAmI) // Combo on left click
					{
						float baseAngle = (Main.MouseWorld - player.Center).ToRotation() + (float)Math.PI / 4f;
						Projectile.NewProjectile(player.GetSource_ItemUse(item), player.Center, Vector2.Zero, ModContent.ProjectileType<SwordBookProjectile>(), item.damage, item.knockBack, player.whoAmI, ai0: baseAngle, ai1: comboState);

						comboState++;
						comboState %= 4;
					}

					return false;
				}
			}

			return true;
		}

		public override bool? CanHitNPC(Player player, NPC target)
		{
			if (player.ownedProjectileCounts[ModContent.ProjectileType<SwordBookProjectile>()] > 0 || player.ownedProjectileCounts[ModContent.ProjectileType<SwordBookParry>()] > 0)
				return false; //prevent regular melee damage while a swordbook is active so we can use itemanimation

			return base.CanHitNPC(player, target);
		}
	}

	class SwordBookProjectile : ModProjectile, IDrawPrimitive
	{
		// Info about the sword that created this projectile
		public float length;
		public Texture2D texture;
		public int lifeSpan;
		public float holdOut;
		public Color trailColor;

		private bool flipSprite = false;
		private List<Vector2> cache;
		private Trail trail;

		private bool hasDoneSwingSound = false;
		private bool hasDoneOnSpawn = false;

		// These handle replicating the vanilla effects which we must do via reflection
		public static MethodInfo? playerItemCheckEmitUseVisuals_Info;
		public static Func<Player, Item, Rectangle, Rectangle>? playerItemCheckEmitUseVisuals;

		public static MethodInfo? ApplyNPCOnHitEffects_Info;
		public static Action<Player, Item, Rectangle, int, float, int, int, int>? ApplyNPCOnHitEffects;

		public Item itemSnapshot; //lock in the item on creation incase they bypass the item switching prevention

		// Properties
		public float Progress => 1 - Projectile.timeLeft / (float)lifeSpan;
		public int Direction => (Math.Abs(BaseAngle - (float)Math.PI / 4f) < Math.PI / 2f) ? 1 : -1;
		public Player Owner => Main.player[Projectile.owner];

		public ref float BaseAngle => ref Projectile.ai[0];

		public ref float ComboState => ref Projectile.ai[1];

		public override string Texture => AssetDirectory.Invisible;

		public override void Load()
		{
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

		public override void OnSpawn(IEntitySource source)
		{
			SpawnLogic();
		}

		private void SpawnLogic()
		{
			if (!hasDoneOnSpawn)
			{
				//call by all clients to setup
				hasDoneOnSpawn = true;

				itemSnapshot = Owner.HeldItem;

				Projectile.timeLeft = itemSnapshot.useAnimation * 4;
				lifeSpan = Projectile.timeLeft;
				Projectile.scale = itemSnapshot.scale;

				if (Main.netMode != NetmodeID.Server)
				{
					trailColor = ItemColorUtility.GetColor(itemSnapshot.type);
					texture = TextureAssets.Item[itemSnapshot.type].Value;
					length = (float)Math.Sqrt(Math.Pow(texture.Width, 2) + Math.Pow(texture.Width, 2)) * itemSnapshot.scale;
				}

				PlaySwingSound();
			}
		}

		private void PlaySwingSound()
		{
			if (!hasDoneSwingSound && Main.netMode != NetmodeID.Server)
			{
				hasDoneSwingSound = true;

				float pitch = 1 - itemSnapshot.useAnimation / 60f;
				pitch += ComboState * 0.1f;

				if (pitch >= 1)
					pitch = 1;

				Helper.PlayPitched("Effects/HeavyWhooshShort", 1, pitch, Owner.Center);

				if (itemSnapshot.UseSound.HasValue)
					Terraria.Audio.SoundEngine.PlaySound(itemSnapshot.UseSound.Value, Owner.Center);
			}
		}

		public override void AI()
		{
			Owner.itemAnimation = Owner.itemTime = Projectile.timeLeft; //lock inventory while this is active
			Owner.itemAnimationMax = 0; //make sure the regular weapon holdout doesn't render (makes an invisible super sword so you need to disable onhit elsewhere)

			Projectile.Center = Owner.Center;
			Owner.direction = Direction;
			Owner.heldProj = Projectile.whoAmI;

			Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation + 1.57f * 2.5f);

			SpawnLogic();

			if (Projectile.timeLeft % 4 == 0)
			{
				Vector2 itemRectStart = Projectile.Center + Projectile.rotation.ToRotationVector2() * length * 0.5f;
				var itemRect = new Rectangle((int)itemRectStart.X, (int)itemRectStart.Y, 2, 2);
				itemRect.Inflate((int)length / 2, (int)length / 2);
				playerItemCheckEmitUseVisuals(Owner, itemSnapshot, itemRect);
			}

			if (ComboState < 3 && Progress == 0 && itemSnapshot.shoot > ProjectileID.None && Projectile.owner == Main.myPlayer) //spawn projectile if relevant
				Projectile.NewProjectile(null, Owner.Center, Vector2.Normalize(Main.MouseWorld - Owner.Center) * itemSnapshot.shootSpeed, itemSnapshot.shoot, Projectile.damage, Projectile.knockBack, Projectile.owner);

			switch (ComboState)
			{
				case 0:
					Projectile.rotation = BaseAngle + (SwingEase(Progress) * 3f - 1.5f) * Direction;
					holdOut = (float)Math.Sin(Progress * 3.14f) * 16;

					break;

				case 1:
					flipSprite = true;
					Projectile.rotation = BaseAngle - (SwingEase(Progress) * 4f - 2f) * Direction;
					holdOut = (float)Math.Sin(Progress * 3.14f) * 24;

					break;

				case 2:

					if (Progress == 0) //this swing is slightly faster
					{
						Projectile.timeLeft -= 20;
						lifeSpan -= 20;
					}

					Projectile.rotation = BaseAngle + (SwingEase(Progress) * 2f - 1f) * Direction;
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

					Projectile.rotation = BaseAngle + Direction + Helpers.Helper.BezierEase(Progress) * 6.28f * Direction;
					holdOut = Progress * 32;

					float rot = Projectile.rotation + (Direction == 1 ? 0 : -(float)Math.PI / 2f);

					if (itemSnapshot.shoot > ProjectileID.None && Projectile.owner == Main.myPlayer) //create projectile ring on circular slash
					{
						for (int k = 0; k < 12; k++)
						{
							if (Projectile.timeLeft == (int)(lifeSpan * (k / 12f)))
							{
								int i = Projectile.NewProjectile(null, Owner.Center, Vector2.UnitX.RotatedBy(Projectile.rotation) * itemSnapshot.shootSpeed, itemSnapshot.shoot, Projectile.damage / 4, Projectile.knockBack, Projectile.owner);
								Main.projectile[i].scale *= 0.75f; //scale and timeleft aren't set for other players but this could be any projectile type. TODO: find a solution for this

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

			if (Main.netMode != NetmodeID.Server)
			{
				ManageCaches();
				ManageTrail();
			}
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

		public override void CutTiles()
		{
			float rot = Projectile.rotation + (Direction == 1 ? 0 : -(float)Math.PI / 2f);

			Vector2 start = Owner.Center;
			Vector2 end = Owner.Center + Vector2.UnitX.RotatedBy(rot) * (length + holdOut);

			Utils.PlotTileLine(start, end, 40 * Projectile.scale, DelegateMethods.CutTiles);
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			Helpers.Helper.PlayPitched(Helpers.Helper.IsFleshy(target) ? "Impacts/StabFleshy" : "Impacts/Clink", 1, Main.rand.NextFloat(), Owner.Center);
			CameraSystem.shake += 3;

			// Simulate on-hit effects
			ItemLoader.OnHitNPC(itemSnapshot, Owner, target, hit, damageDone);
			NPCLoader.OnHitByItem(target, Owner, itemSnapshot, hit, damageDone);
			PlayerLoader.OnHitNPC(Owner, target, hit, damageDone);
			Owner.StatusToNPC(itemSnapshot.type, target.whoAmI);
			float knockback = hit.Knockback;
			ApplyNPCOnHitEffects(Owner, itemSnapshot, Projectile.Hitbox, Projectile.damage, knockback, target.whoAmI, Main.DamageVar(damageDone, Owner.luck), damageDone);

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
			if (trail is null || trail.IsDisposed)
			{
				trail = new Trail(Main.instance.GraphicsDevice, 50, new NoTip(), factor => (float)Math.Min(factor, Progress) * length * 0.75f, factor =>
							{
								if (factor.X == 1)
									return Color.Transparent;

								return trailColor * (float)Math.Min(factor.X, Progress) * 0.5f * (float)Math.Sin(Progress * 3.14f);
							});
			}

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
			effect.Parameters["sampleTexture"].SetValue(Assets.GlowTrail.Value);
			effect.Parameters["sampleTexture2"].SetValue(Assets.Items.Moonstone.DatsuzeiFlameMap2.Value);

			trail?.Render(effect);
		}
	}

	class SwordBookParry : ModProjectile
	{
		public float length;
		public Texture2D texture;

		public float oldRot;
		public Vector2 oldPos;

		private bool hasDoneOnSpawn = false;

		public Item itemSnapshot; //lock in the item on creation incase they bypass the item switching prevention

		public ref float State => ref Projectile.ai[0];

		public Player Owner => Main.player[Projectile.owner];

		public override string Texture => AssetDirectory.Invisible;

		public override void SetDefaults()
		{
			Projectile.friendly = true;
			Projectile.DamageType = DamageClass.Melee;
			Projectile.tileCollide = false;
			Projectile.penetrate = -1;
			Projectile.timeLeft = 30;
		}

		public override void OnSpawn(IEntitySource source)
		{
			SetTextureAndLength();
		}

		private void SetTextureAndLength()
		{
			if (!hasDoneOnSpawn)
			{
				hasDoneOnSpawn = true;
				itemSnapshot = Owner.HeldItem;

				if (Main.netMode != NetmodeID.Server)
				{
					texture = TextureAssets.Item[itemSnapshot.type].Value;
					length = (float)Math.Sqrt(Math.Pow(texture.Width, 2) + Math.Pow(texture.Width, 2)) * itemSnapshot.scale;
				}
			}
		}

		public override void AI()
		{
			Owner.itemAnimation = Owner.itemTime = Projectile.timeLeft; //lock inventory while this is active
			Owner.itemAnimationMax = 0; //make sure the regular weapon holdout doesn't render (makes an invisible super sword so you need to disable onhit elsewhere)

			Owner.heldProj = Projectile.whoAmI;

			SetTextureAndLength();

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

						if (proj.damage <= itemSnapshot.damage * 2)
						{
							CameraSystem.shake += 20;
							CombatText.NewText(Owner.Hitbox, Color.Yellow, "Parry");

							if (itemSnapshot.shoot != ProjectileID.None && Projectile.owner == Main.myPlayer)
							{
								Projectile.NewProjectile(null, proj.Center, proj.velocity * -1, itemSnapshot.shoot, Projectile.damage, Projectile.knockBack, Projectile.owner);

								for (int i = 0; i < 4; i++)
								{
									Projectile.NewProjectile(null, proj.Center, proj.velocity.RotatedByRandom(0.5f) * -Main.rand.NextFloat(0.8f, 1.0f), itemSnapshot.shoot, Projectile.damage / 4, Projectile.knockBack, Projectile.owner);
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