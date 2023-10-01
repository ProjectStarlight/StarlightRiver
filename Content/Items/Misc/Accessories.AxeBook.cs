using StarlightRiver.Content.Items.BaseTypes;
using StarlightRiver.Core.Systems.CameraSystem;
using StarlightRiver.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics.Effects;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Misc
{
	internal class AxeBook : SmartAccessory
	{
		public int comboState;

		public override string Texture => AssetDirectory.MiscItem + "AxeBook";

		public AxeBook() : base("Tiger Technique", "Teaches you the Art of Axes, granting all axe weapons a new combo attack\nThe final strike will {{rend}} enemies' armor\n<right> to throw your axe") { }

		public override void Load()
		{
			StarlightItem.CanUseItemEvent += OverrideAxeEffects;
			StarlightItem.AltFunctionUseEvent += AllowRightClick;
		}

		public override void Unload()
		{
			StarlightItem.CanUseItemEvent -= OverrideAxeEffects;
			StarlightItem.AltFunctionUseEvent -= AllowRightClick;
		}

		public override void SafeSetDefaults()
		{
			Item.rare = Terraria.ID.ItemRarityID.Orange;
			Item.value = Item.sellPrice(gold: 2);
		}

		/// <summary>
		/// Allows the player to right click with axes that dont normally have them
		/// </summary>
		/// <param name="item"></param>
		/// <param name="player"></param>
		/// <returns></returns>
		private bool AllowRightClick(Item item, Player player)
		{
			if (Equipped(player))
			{
				if (item.CountsAsClass(DamageClass.Melee) && item.pick <= 0 && item.axe > 0 && item.hammer <= 0 && item.shoot <= ProjectileID.None && item.useStyle == Terraria.ID.ItemUseStyleID.Swing && !item.noMelee)
					return true;
			}

			return false;
		}

		/// <summary>
		/// Changes the effect of using an item with axe power, melee damage, no projectile, and a swing usestyle to the intended alternative effect
		/// </summary>
		/// <param name="item"></param>
		/// <param name="player"></param>
		/// <returns></returns>
		private bool OverrideAxeEffects(Item item, Player player)
		{
			if (Equipped(player))
			{
				if (item != player.HeldItem)
					return true;

				if (item.CountsAsClass(DamageClass.Melee) && item.pick <= 0 && item.axe > 0 && item.hammer <= 0 && item.shoot <= ProjectileID.None && item.useStyle == Terraria.ID.ItemUseStyleID.Swing && !item.noMelee)
				{
					if (Main.projectile.Any(n => n.active && (n.type == ModContent.ProjectileType<AxeBookProjectile>() || n.type == ModContent.ProjectileType<ThrownAxeProjectile>()) && n.owner == player.whoAmI))
						return false;

					if (Main.mouseRight && Main.myPlayer == player.whoAmI)
					{
						Vector2 vel = Vector2.Normalize(Main.MouseWorld - player.Center) * Math.Clamp(item.damage * 0.12f, 5, 6.5f);

						Projectile.NewProjectile(player.GetSource_ItemUse(item), player.Center, vel, ModContent.ProjectileType<ThrownAxeProjectile>(), item.damage, item.knockBack, player.whoAmI);
					}
					else if (Main.myPlayer == player.whoAmI)
					{
						float baseAngle = (Main.MouseWorld - player.Center).ToRotation() + (float)Math.PI / 4f;
						Projectile.NewProjectile(player.GetSource_ItemUse(item), player.Center, Vector2.Zero, ModContent.ProjectileType<AxeBookProjectile>(), item.damage * 2, item.knockBack, player.whoAmI, ai0: comboState, ai2: baseAngle);

						comboState++;
						comboState %= 3;
					}

					return false;
				}
			}

			return true;
		}

		public override bool? CanHitNPC(Player player, NPC target)
		{
			if (player.ownedProjectileCounts[ModContent.ProjectileType<AxeBookProjectile>()] > 0 || player.ownedProjectileCounts[ModContent.ProjectileType<ThrownAxeProjectile>()] > 0)
				return false; //prevent regular melee damage while a axe is active so we can use itemanimation

			return base.CanHitNPC(player, target);
		}
	}

	internal class AxeBookProjectile : ModProjectile, IDrawPrimitive
	{
		public Texture2D texture;
		public int lifeSpan;
		public float holdOut;
		public Color trailColor;

		private int freeze = 0;
		private List<Vector2> cache;
		private Trail trail;

		private bool doneOnSpawn = false;

		private bool hitTree;

		private readonly bool flipSprite = false;

		public float Progress => 1 - Projectile.timeLeft / (float)lifeSpan;
		public int Direction => (Math.Abs(BaseAngle - (float)Math.PI / 4f) < Math.PI / 2f) ? 1 : -1;
		public Player Owner => Main.player[Projectile.owner];
		public float FadeOut => Projectile.timeLeft < 60 ? Projectile.timeLeft / 60f : 1;

		public ref float ComboState => ref Projectile.ai[0];
		public ref float Length => ref Projectile.ai[1];
		public ref float BaseAngle => ref Projectile.ai[2];

		public Item Item => Owner.HeldItem;

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

		public override void OnSpawn(IEntitySource source)
		{
			SpawnLogic();
		}

		private void SpawnLogic()
		{
			//we just call this directly by all clients and server since it doesn't require any additional data being sent beyond ai fields
			if (doneOnSpawn)
				return;

			Projectile.timeLeft = Item.useAnimation * 5;
			Projectile.scale = Item.scale * (1.2f + ComboState * 0.3f);
			lifeSpan = Item.useAnimation * 5;

			if (Main.netMode != NetmodeID.Server)
			{
				trailColor = ItemColorUtility.GetColor(Item.type);
				texture = TextureAssets.Item[Item.type].Value;
				Length = (float)Math.Sqrt(Math.Pow(texture.Width, 2) + Math.Pow(texture.Width, 2)) * Item.scale;

				PlaySound();
			}

			doneOnSpawn = true;
		}

		private void PlaySound()
		{
			float pitch = -0.3f;
			pitch += ComboState * 0.1f;

			if (pitch >= 1)
				pitch = 1;

			Helper.PlayPitched("Effects/HeavyWhoosh", 1, pitch, Owner.MountedCenter);
			Helper.PlayPitched("GlassMiniboss/GlassShatter", 1, pitch, Owner.MountedCenter);

			if (Item.UseSound.HasValue)
				Terraria.Audio.SoundEngine.PlaySound(Item.UseSound.Value, Owner.MountedCenter);
		}

		private void DoSwingAnimation(Player player)
		{
			Projectile instance = Main.projectile.FirstOrDefault(n => n.ModProjectile is AxeBookProjectile && n.owner == player.whoAmI);
			var modProj = instance?.ModProjectile as AxeBookProjectile;

			if (modProj != null && instance.active)
				player.bodyFrame = new Rectangle(0, (int)(1 + modProj.Progress * 4) * 56, 40, 56);
		}

		public override void AI()
		{
			Projectile.Center = Owner.Center;
			Owner.direction = Direction;
			Owner.heldProj = Projectile.whoAmI;

			SpawnLogic();

			Owner.itemAnimation = Owner.itemTime = Projectile.timeLeft; //lock inventory while this is active
			Owner.itemAnimationMax = 0; //make sure the regular weapon holdout doesn't render (makes an invisible super axe so you need to disable onhit elsewhere)

			// Cut trees
			if (Progress > 0 && !hitTree)
			{
				float rot = Projectile.rotation + (Direction == 1 ? 0 : -(float)Math.PI / 2f);
				Vector2 center = Owner.Center + Vector2.UnitX.RotatedBy(rot) * (Length * Projectile.scale + holdOut);

				for (int x = -1; x <= 1; x++)
				{
					for (int y = -1; y <= 1; y++)
					{
						Tile tile = Framing.GetTileSafely((int)center.X / 16 + x, (int)center.Y / 16 + y);

						if (tile.HasTile && Main.tileAxe[tile.TileType])
						{
							Owner.PickTile((int)center.X / 16 + x, (int)center.Y / 16 + y, Owner.HeldItem.axe);
							hitTree = true;
						}
					}
				}
			}

			// Combat logic
			switch (ComboState)
			{
				case 0:
					if (Progress == 0)
					{
						Projectile.timeLeft -= 20;
						lifeSpan -= 20;
					}

					Projectile.rotation = BaseAngle + (SwingEase(Progress) * 3f - 1.5f) * Direction;
					holdOut = (float)Math.Sin(SwingEase(Progress) * 3.14f) * Length * 0.2f;

					break;

				case 1:
					Projectile.rotation = BaseAngle + (SwingEase(Progress) * 3f - 1.5f) * Direction;
					holdOut = (float)Math.Sin(SwingEase(Progress) * 3.14f) * Length * 0.3f;

					break;

				case 2:
					if (Progress == 0)
					{
						Projectile.timeLeft -= 60;
						lifeSpan -= 60;
						Projectile.damage *= 2;
					}

					float rot = Projectile.rotation + (Direction == 1 ? 0 : -(float)Math.PI / 2f);
					Vector2 end = Owner.Center + Vector2.UnitX.RotatedBy(rot) * (Length * Projectile.scale + holdOut) * .75f;
					Dust.NewDust(end - Vector2.One * 5, 10, 10, ModContent.DustType<Dusts.Cinder>(), 0, 0, 0, GetSwingColor(Progress));

					Projectile.rotation = BaseAngle + (SwingEase(Progress) * 3f - 1.5f) * Direction;
					holdOut = (float)Math.Sin(SwingEase(Progress) * 3.14f) * Length * 0.4f;

					break;
			}

			if (Main.netMode != NetmodeID.Server)
				ManageTrail();

			if (freeze > 1)
			{
				freeze--;
				Projectile.timeLeft++;
				return;
			}

			if (Main.netMode != NetmodeID.Server)
				ManageCaches();
		}

		public float SwingEase(float progress)
		{
			return (float)(11.904f * Math.Pow(progress, 4) - 30.9524f * Math.Pow(progress, 3) + 25.5952f * Math.Pow(progress, 2) - 5.54762f * progress);
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			float rot = Projectile.rotation + (Direction == 1 ? 0 : -(float)Math.PI / 2f);

			Vector2 start = Owner.Center;
			Vector2 end = Owner.Center + Vector2.UnitX.RotatedBy(rot) * (Length * Projectile.scale + holdOut) * 1.15f;

			if (freeze <= 1 && Helper.CheckLinearCollision(start, end, targetHitbox, out Vector2 colissionPoint))
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

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			Helper.PlayPitched(Helper.IsFleshy(target) ? "Impacts/GoreLight" : "Impacts/Clink", 1, -Main.rand.NextFloat(0.25f), Owner.Center);
			CameraSystem.shake += 4;

			if (ComboState == 2 && target.defense > 0)
				target.defense--;

			target.velocity += Vector2.Normalize(target.Center - Owner.Center) * Projectile.knockBack * 2 * target.knockBackResist;
			target.immune[0] += 22;

			if (freeze == 0)
				freeze += 24;

			Owner.TryGetModPlayer<StarlightPlayer>(out StarlightPlayer starlightPlayer);
			starlightPlayer.SetHitPacketStatus(shouldRunProjMethods: true);
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Vector2 origin = Direction == 1 ^ flipSprite ? new Vector2(0, texture.Height) : new Vector2(texture.Width, texture.Height);
			SpriteEffects effects = Direction == 1 ^ flipSprite ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
			float rot = Projectile.rotation + (Direction == 1 ^ flipSprite ? 0 : (float)Math.PI / 2f);
			Vector2 pos = Projectile.Center - Main.screenPosition + Vector2.UnitX.RotatedBy(rot) * holdOut * Projectile.scale * (flipSprite ? Direction * -1 : Direction);

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
					cache.Add(Vector2.UnitX.RotatedBy(Projectile.rotation - Math.PI / 4f) * (Length + holdOut) * Projectile.scale * 0.7f);
				}
			}

			cache.Add(Vector2.UnitX.RotatedBy(Projectile.rotation - Math.PI / 4f) * (Length + holdOut) * Projectile.scale * 0.7f);

			while (cache.Count > 50)
			{
				cache.RemoveAt(0);
			}
		}

		private Color GetSwingColor(float factor)
		{
			if (ComboState == 1)
				return Color.Lerp(trailColor, Color.Red, Progress * 0.5f) * (float)Math.Min(factor, Progress) * 0.9f * (float)Math.Sin(Progress * 3.14f) * FadeOut;

			if (ComboState == 2)
				return Color.Lerp(trailColor, Color.Red, Progress) * FadeOut;

			return trailColor * (float)Math.Min(factor, Progress) * 0.9f * (float)Math.Sin(Progress * 3.14f) * FadeOut;
		}

		private void ManageTrail()
		{
			trail ??= new Trail(Main.instance.GraphicsDevice, 50, new TriangularTip(40 * 4), factor => (float)Math.Min(factor, Progress) * Length * 1.25f, factor =>
			{
				if (factor.X >= 0.98f)
					return Color.White * 0;

				return GetSwingColor(factor.X);
			});

			if (cache != null)
			{
				var realCache = new Vector2[50];

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

			var world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
			Matrix view = Main.GameViewMatrix.TransformationMatrix;
			var projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

			effect.Parameters["time"].SetValue(Main.GameUpdateCount * 0.02f);
			effect.Parameters["repeats"].SetValue(2f);
			effect.Parameters["transformMatrix"].SetValue(world * view * projection);
			effect.Parameters["sampleTexture"].SetValue(ModContent.Request<Texture2D>("StarlightRiver/Assets/EnergyTrail").Value);
			effect.Parameters["sampleTexture2"].SetValue(ModContent.Request<Texture2D>("StarlightRiver/Assets/MagicPixel").Value);

			trail?.Render(effect);
		}
	}

	internal class ThrownAxeProjectile : ModProjectile, IDrawPrimitive
	{
		public const int LIFE_SPAN = 300;

		public Texture2D texture;
		public Color trailColor;

		private int freeze = 0;
		private List<Vector2> cache;
		private Trail trail;
		private Vector2 freezePoint;
		private float storedScale;
		private bool doneOnSpawn;

		public float Progress => 1 - Projectile.timeLeft / (float)LIFE_SPAN;
		public Player Owner => Main.player[Projectile.owner];
		public float FadeOut => Projectile.timeLeft < 60 ? Projectile.timeLeft / 60f : 1;
		public Item Item => Owner.HeldItem;

		public ref float Length => ref Projectile.ai[1];

		public override string Texture => AssetDirectory.Invisible;

		public override void SetDefaults()
		{
			Projectile.friendly = true;
			Projectile.DamageType = DamageClass.Melee;
			Projectile.tileCollide = false;
			Projectile.penetrate = -1;
			Projectile.extraUpdates = 3;
			Projectile.timeLeft = LIFE_SPAN;
		}

		public override void OnSpawn(IEntitySource source)
		{
			SpawnLogic();
		}

		private void SpawnLogic()
		{
			//we just call this directly by all clients and server since it doesn't require any additional data being sent beyond ai fields
			if (doneOnSpawn)
				return;

			Projectile.scale = Item.scale * 1.25f;

			if (Main.netMode != NetmodeID.Server)
			{
				trailColor = ItemColorUtility.GetColor(Item.type);
				texture = TextureAssets.Item[Item.type].Value;
				Length = (float)Math.Sqrt(Math.Pow(texture.Width, 2) + Math.Pow(texture.Width, 2)) * Item.scale;
			}

			doneOnSpawn = true;
		}

		public override void AI()
		{
			Owner.heldProj = Projectile.whoAmI;

			SpawnLogic();

			Owner.itemAnimation = Owner.itemTime = Projectile.timeLeft; //lock inventory while this is active
			Owner.itemAnimationMax = 0; //make sure the regular weapon holdout doesn't render (makes an invisible super axe so you need to disable onhit elsewhere)

			if (storedScale == 0)
				storedScale = Projectile.scale;

			if (Projectile.timeLeft % 40 == 0 && Projectile.friendly)
				Helper.PlayPitched("Effects/HeavyWhoosh", 0.45f, 0.5f, Projectile.Center);

			if (Progress < 0.2f)
			{
				Projectile.Center = Owner.Center;
				Projectile.rotation += 0.01f + Progress / 0.2f * 0.11f;

				Owner.SetCompositeArmFront(true, 0, Owner.direction * 1.57f + Owner.direction * (Progress / 0.2f * 4.71f));
				Projectile.scale = Progress / 0.2f * storedScale;

				return;
			}

			if (!Projectile.friendly)
			{
				Projectile.Center = Owner.Center;
				Projectile.scale = 0;
				trailColor = Color.Transparent;
			}

			if (Main.netMode != NetmodeID.Server)
				ManageTrail();

			if (freeze > 0)
			{
				freeze--;
				Projectile.timeLeft++;
				Projectile.position -= Projectile.velocity;
				return;
			}

			if (Main.netMode != NetmodeID.Server)
				ManageCaches();

			Projectile.rotation += 0.12f;

			if (Progress > 0.4f)
			{
				Projectile.velocity += -Vector2.Normalize(Projectile.Center - Owner.Center) * 0.1f;

				if (Vector2.Distance(Projectile.Center, Owner.Center) < 30 && Projectile.friendly)
				{
					Projectile.friendly = false;
					freezePoint = Projectile.Center;
				}
			}
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			return Helper.CheckCircularCollision(Projectile.Center, (int)(Length * Projectile.scale) / 2, targetHitbox);
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			for (int k = 0; k < 10; k++)
			{
				Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<Dusts.Cinder>(), Main.rand.NextVector2Circular(6, 6), 0, GetSwingColor());

				for (int n = 0; n < 3; n++)
				{
					Dust.NewDustPerfect(Projectile.Center, Terraria.ID.DustID.Blood, Main.rand.NextVector2Circular(6, 6));
				}
			}

			Helper.PlayPitched(Helper.IsFleshy(target) ? "Impacts/GoreLight" : "Impacts/Clink", 1, -Main.rand.NextFloat(0.25f), Owner.Center);
			CameraSystem.shake += 4;

			target.velocity += Vector2.Normalize(target.Center - Projectile.Center) * Projectile.knockBack * 2 * target.knockBackResist;
			target.immune[0] += 22;

			if (freeze == 0)
				freeze += 12;

			Owner.TryGetModPlayer<StarlightPlayer>(out StarlightPlayer starlightPlayer);
			starlightPlayer.SetHitPacketStatus(shouldRunProjMethods: true);
		}

		public override bool PreDraw(ref Color lightColor)
		{
			float rot = Projectile.rotation;
			Vector2 pos = Projectile.Center - Main.screenPosition;

			Main.spriteBatch.Draw(texture, pos, default, lightColor * FadeOut, rot, texture.Size() / 2, Projectile.scale, 0, 0);
			return false;
		}

		private void ManageCaches()
		{
			if (cache == null)
			{
				cache = new List<Vector2>();

				for (int i = 0; i < 50; i++)
				{
					cache.Add(Projectile.Center + Vector2.UnitX.RotatedBy(Projectile.rotation - Math.PI / 4f) * Length / 2 * Projectile.scale * 0.7f);
				}
			}

			if (Projectile.friendly)
				cache.Add(Projectile.Center + Vector2.UnitX.RotatedBy(Projectile.rotation - Math.PI / 4f) * Length / 2 * Projectile.scale * 0.7f);
			else
				cache.Add(freezePoint + Vector2.UnitX.RotatedBy(Projectile.rotation - Math.PI / 4f) * Length / 2 * Projectile.scale * 0.7f);

			while (cache.Count > 50)
			{
				cache.RemoveAt(0);
			}
		}

		private Color GetSwingColor()
		{
			return Color.Lerp(trailColor, Color.Red, Progress) * FadeOut;
		}

		private void ManageTrail()
		{
			trail ??= new Trail(Main.instance.GraphicsDevice, 50, new TriangularTip(40 * 4), factor => (float)Math.Min(factor, Progress) * 64, factor =>
			{
				if (factor.X >= 0.98f)
					return Color.White * 0;

				return GetSwingColor();
			});

			if (cache != null)
				trail.Positions = cache.ToArray();
		}

		public void DrawPrimitives()
		{
			Effect effect = Filters.Scene["DatsuzeiTrail"].GetShader().Shader;

			var world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
			Matrix view = Main.GameViewMatrix.TransformationMatrix;
			var projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

			effect.Parameters["time"].SetValue(Main.GameUpdateCount * 0.02f);
			effect.Parameters["repeats"].SetValue(2f);
			effect.Parameters["transformMatrix"].SetValue(world * view * projection);
			effect.Parameters["sampleTexture"].SetValue(ModContent.Request<Texture2D>("StarlightRiver/Assets/EnergyTrail").Value);
			effect.Parameters["sampleTexture2"].SetValue(ModContent.Request<Texture2D>("StarlightRiver/Assets/MagicPixel").Value);

			trail?.Render(effect);
		}
	}
}