using StarlightRiver.Core.Systems.CameraSystem;
using StarlightRiver.Helpers;
using System;
using System.Collections.Generic;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics.Effects;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Astroflora
{
	public class AstrofloraBow : ModItem
	{
		public override string Texture => AssetDirectory.Astroflora + "AstrofloraBow";

		private List<NPC> locks;

		private const int maxLocks = 3;

		private const string SoundPath = $"{nameof(StarlightRiver)}/Sounds/Custom/Astroflora/";

		public bool CursorShouldBeRed { get; private set; }

		private int counter;

		public override void SetStaticDefaults()
		{
			Tooltip.SetDefault("TBA");
		}

		public override void SetDefaults()
		{
			// Balance requiered on all stats (I have no idea what point in progression this is).
			Item.damage = 200;
			Item.DamageType = DamageClass.Ranged;

			Item.useTime = 30;
			Item.useAnimation = 30;

			Item.shootSpeed = 1;
			Item.shoot = ProjectileID.PurificationPowder; // Dummy since Shoot hook changes the result.
			Item.useAmmo = AmmoID.Arrow;

			Item.noMelee = true;

			Item.useStyle = ItemUseStyleID.Shoot;
			Item.UseSound = SoundID.Item5;

			Item.rare = ItemRarityID.Blue;
			Item.value = Item.sellPrice(0, 1, 0, 0);
		}

		public override void HoldItem(Player Player)
		{
			if (CursorShouldBeRed && --counter <= 0)
			{
				counter = 0;
				CursorShouldBeRed = false;
			}

			locks ??= new List<NPC>();

			for (int i = 0; i < Main.maxNPCs; i++)
			{
				NPC NPC = Main.npc[i];

				if (locks.Contains(NPC) && (!NPC.CanBeChasedBy() || NPC.CanBeChasedBy() && !NPC.GetGlobalNPC<AstrofloraLocksGlobalNPC>().Locked))
					locks.Remove(NPC);

				Rectangle generousHitbox = NPC.Hitbox;
				generousHitbox.Inflate(NPC.Hitbox.Width / 3, NPC.Hitbox.Height / 3);

				if (NPC.CanBeChasedBy() && !NPC.GetGlobalNPC<AstrofloraLocksGlobalNPC>().Locked && locks.Count < maxLocks && !locks.Contains(NPC) && generousHitbox.Contains(Main.MouseWorld.ToPoint()))
				{
					Say("Target Locked!", Player);

					locks.Add(NPC);

					NPC.GetGlobalNPC<AstrofloraLocksGlobalNPC>().Locked = true;

					// TODO: Play some kind of lock-on sound effect?
				}
			}
		}

		private void Say(string text, Player Player)
		{
			// Main.fontCombatText[0] is just the variant used when dramatic == false.
			Vector2 textSize = FontAssets.CombatText[0].Value.MeasureString(text);

			var textRectangle = new Rectangle((int)Player.MountedCenter.X, (int)(Player.MountedCenter.Y + Player.height), (int)textSize.X, (int)textSize.Y);

			CombatText.NewText(textRectangle, Main.cursorColor, text);
		}

		public override bool CanUseItem(Player Player)
		{
			locks ??= new List<NPC>();

			if (locks.Count > 0)
			{
				return true;
			}
			else
			{
				CameraSystem.Shake = 5;

				SoundEngine.PlaySound(new SoundStyle($"{SoundPath}Failure"), Player.Center);

				Say("No Locks!", Player);

				CursorShouldBeRed = true;
				counter = 30;

				return false;
			}
		}

		public override bool Shoot(Player Player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			for (int i = 0; i < maxLocks; i++)
			{
				int index;

				if (locks.Count == 0)
					index = -1;
				else
					index = i > locks.Count - 1 ? Main.rand.Next(locks).whoAmI : locks[i].whoAmI;

				Vector2 shotOffset = Vector2.Normalize(velocity) * 32;

				Projectile.NewProjectile(source, position + shotOffset, velocity.RotatedBy((i - 1) * (MathHelper.PiOver4 / 2)) * 24, ModContent.ProjectileType<AstrofloraBolt>(), damage, knockback, Player.whoAmI, index);
			}

			locks.Clear();

			return false;
		}
	}

	public class AstrofloraBolt : ModProjectile, IDrawPrimitive
	{
		private const int oldPositionCacheLength = 120;

		private const int trailMaxWidth = 8;

		public override string Texture => AssetDirectory.Invisible;

		private Trail trail;

		private List<Vector2> cache;

		private int TargetNPCIndex
		{
			get => (int)Projectile.ai[0];
			set => Projectile.ai[0] = value;
		}

		private bool HitATarget
		{
			get => (int)Projectile.ai[1] == 1;
			set => Projectile.ai[1] = value ? 1 : 0;
		}

		public override void SetStaticDefaults()
		{
			ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true;
		}

		public override void SetDefaults()
		{
			Projectile.width = 16;
			Projectile.height = 16;

			Projectile.damage = 100;
			Projectile.knockBack = 8;

			Projectile.friendly = true;

			Projectile.timeLeft = 300;

			Projectile.tileCollide = false;

			Projectile.penetrate = -1;
		}

		public override void AI()
		{
			// Sync its target.
			Projectile.netUpdate = true;

			ManageCaches();

			ManageTrail();

			if (Projectile.timeLeft < 30)
				Projectile.alpha += 8;

			if (!HitATarget)
			{
				Projectile.velocity.Y = Math.Min(Projectile.velocity.Y + 0.1f, 10);

				if (TargetNPCIndex == -1)
					return;

				NPC target = Main.npc[TargetNPCIndex];

				if (!target.CanBeChasedBy())
				{
					// Stop homing if the target NPC is no longer a valid target.
					TargetNPCIndex = -1;

					return;
				}

				Homing(target);
			}
		}

		private void Homing(NPC target)
		{
			Vector2 move = target.Center - Projectile.Center;

			AdjustMagnitude(ref move);

			Projectile.velocity = (10 * Projectile.velocity + move) / 11f;

			AdjustMagnitude(ref Projectile.velocity);
		}

		private void AdjustMagnitude(ref Vector2 vector)
		{
			float adjustment = 24;

			float magnitude = vector.Length();

			if (magnitude > adjustment)
				vector *= adjustment / magnitude;
		}

		private void ManageCaches()
		{
			if (cache == null)
			{
				cache = new List<Vector2>();

				for (int i = 0; i < oldPositionCacheLength; i++)
				{
					cache.Add(Projectile.Center);
				}
			}

			cache.Add(Projectile.Center);

			while (cache.Count > oldPositionCacheLength)
			{
				cache.RemoveAt(0);
			}
		}

		private void ManageTrail()
		{
			trail ??= new Trail(Main.instance.GraphicsDevice, oldPositionCacheLength, new TriangularTip(trailMaxWidth * 4), factor => factor * trailMaxWidth, factor =>
			{
				// 1 = full opacity, 0 = transparent.
				float normalisedAlpha = 1 - Projectile.alpha / 255f;

				// Scales opacity with the Projectile alpha as well as the distance from the beginning of the trail.
				return new Color(31, 250, 131) * normalisedAlpha * factor.X;
			});

			trail.Positions = cache.ToArray();
			trail.NextPosition = Projectile.Center + Projectile.velocity;
		}

		public void DrawPrimitives()
		{
			Effect effect = Filters.Scene["Primitives"].GetShader().Shader;

			var world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
			Matrix view = Main.GameViewMatrix.ZoomMatrix;
			var projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

			effect.Parameters["transformMatrix"].SetValue(world * view * projection);

			trail?.Render(effect);
		}

		public override bool? CanHitNPC(NPC target)
		{
			return TargetNPCIndex != -1 && !HitATarget && Main.npc[TargetNPCIndex] == target;
		}

		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
		{
			target.GetGlobalNPC<AstrofloraLocksGlobalNPC>().Locked = false;

			Projectile.timeLeft = 30;

			HitATarget = true;

			// This is hacky, but it lets the Projectile keep its rotation without having to make an extra variable to cache it after it hits a target and "stops".
			Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.Zero) * 0.0001f;
		}

		public override void Kill(int timeLeft)
		{
			trail?.Dispose();

			if (TargetNPCIndex > -1)
			{
				NPC NPC = Main.npc[TargetNPCIndex];

				if (NPC.active)
					NPC.GetGlobalNPC<AstrofloraLocksGlobalNPC>().Locked = false;
			}
		}
	}

	public class AstrofloraLocksGlobalNPC : GlobalNPC
	{
		public override bool InstancePerEntity => true;

		public const int MaxLockDuration = 5 * 60; // 5 seconds, subject to change (1 second feels a bit short).

		public bool Locked
		{
			get => locked;
			set
			{
				if (value)
					remainingLockDuration = MaxLockDuration;

				locked = value;
			}
		}

		private bool locked;

		public int remainingLockDuration;

		public override bool PreAI(NPC NPC)
		{
			if (--remainingLockDuration <= 0)
			{
				Locked = false;
				remainingLockDuration = 0;
			}

			return base.PreAI(NPC);
		}
	}
}
