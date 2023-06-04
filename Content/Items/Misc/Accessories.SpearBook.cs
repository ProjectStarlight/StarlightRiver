using StarlightRiver.Content.Items.BaseTypes;
using StarlightRiver.Core.Systems.CameraSystem;
using StarlightRiver.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics.Effects;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Misc
{
	public class SpearBook : SmartAccessory, IPostLoadable
	{
		public int comboState;
		public static Dictionary<int, bool> spearList;

		public static MethodInfo? AI019SpearsOld_Info;
		public static Action<Projectile>? AI019SpearsOld;
		public static MethodInfo? AI019Spears_Info;
		public static Action<Projectile>? AI019Spears;

		public SpearBook() : base("Snake Technique", "Teaches you the Art of the Spear, granting all spear weapons a new combo attack\nThe last strike in the combo deals increased damage and knockback\n<right> to deter enemies with a flurry of stabs") { }

		public override string Texture => AssetDirectory.MiscItem + "SpearBook";

		public override void Load()
		{
			StarlightItem.CanUseItemEvent += OverrideSpearEffects;
			StarlightItem.AltFunctionUseEvent += AllowRightClick;

			AI019SpearsOld_Info = typeof(Projectile).GetMethod("AI_019_Spears_Old", BindingFlags.NonPublic | BindingFlags.Instance);
			AI019SpearsOld = (Action<Projectile>)Delegate.CreateDelegate(typeof(Action<Projectile>), AI019SpearsOld_Info);

			AI019Spears_Info = typeof(Projectile).GetMethod("AI_019_Spears", BindingFlags.NonPublic | BindingFlags.Instance);
			AI019Spears = (Action<Projectile>)Delegate.CreateDelegate(typeof(Action<Projectile>), AI019Spears_Info);
		}

		public override void Unload()
		{
			StarlightItem.CanUseItemEvent -= OverrideSpearEffects;
			StarlightItem.AltFunctionUseEvent -= AllowRightClick;
		}

		public void PostLoad()
		{
			spearList = new Dictionary<int, bool>();
			var proj = new Projectile();
			for (int i = 0; i < ProjectileLoader.ProjectileCount; i++)
			{
				proj.SetDefaults(i);
				spearList.Add(i, proj.aiStyle == 19);
			}
		}

		public void PostLoadUnload()
		{
			spearList.Clear();
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
				if (item.DamageType.Type == DamageClass.Melee.Type && spearList[item.shoot] && item.noMelee)
					return true;
			}

			return false;
		}

		private bool OverrideSpearEffects(Item item, Player player)
		{
			if (Equipped(player))
			{
				if (item != player.HeldItem)
					return true;

				if (item.DamageType.Type == DamageClass.Melee.Type && spearList[item.shoot] && item.noMelee)
				{
					if (Main.projectile.Any(n => n.active && n.type == ModContent.ProjectileType<SpearBookProjectile>() && n.owner == player.whoAmI))
						return false;

					int i;
					if (Main.mouseRight)
					{
						i = Projectile.NewProjectile(player.GetSource_ItemUse(item), player.Center, Vector2.Zero, ModContent.ProjectileType<SpearBookProjectile>(), item.damage * 2 / 3, item.knockBack, player.whoAmI, 5);

						comboState = 0;
					}
					else
					{
						i = Projectile.NewProjectile(player.GetSource_ItemUse(item), player.Center, Vector2.Zero, ModContent.ProjectileType<SpearBookProjectile>(), item.damage, item.knockBack, player.whoAmI, comboState);

						comboState++;
						comboState %= 5;
					}

					Projectile proj = Main.projectile[i];

					if (proj.ModProjectile is SpearBookProjectile)
					{
						var modProj = proj.ModProjectile as SpearBookProjectile;
						modProj.trailColor = ItemColorUtility.GetColor(item.type);

						Main.instance.LoadProjectile(item.shoot);

						modProj.texture = TextureAssets.Projectile[item.shoot].Value;
						proj.Size = modProj.texture.Size();

						modProj.original = new Projectile();
						modProj.original.SetDefaults(item.shoot);
						modProj.original.owner = player.whoAmI;
						modProj.original.damage = proj.damage;
						modProj.original.knockBack = proj.knockBack;
					}

					return false;
				}
			}

			return true;
		}
	}

	class SpearBookProjectile : ModProjectile, IDrawPrimitive
	{
		const int TRAILLENGTH = 50;
		const int TRAIL2LENGTH = 20;

		enum AttackType : int
		{
			DownSwing,
			Stab,
			Slash,
			UpSwing,
			ChargedStab,
			Flurry
		}

		enum Motion : int
		{
			None,
			Swing,
			Slash,
			Slash2,
			Stab
		}

		public Texture2D texture;
		public Color trailColor;

		public Projectile? original; // Flurry duplicates do not run original AI
		private int originalAITimer = 0;

		private float holdout = 0.8f;
		private float xRotation = 0;
		private float slashRotationOffset = 0;

		private List<Vector2> cache;
		private Trail trail;
		private List<Vector2> cache2;
		private Trail trail2;
		private float progress = 0;
		private float progressAngle = 0;
		private Motion motion = Motion.None;

		private int screenshakeCooldown = 0;

		public override string Texture => AssetDirectory.Invisible;
		public Player Owner => Main.player[Projectile.owner];

		private AttackType CurrentAttack
		{
			get => (AttackType)Projectile.ai[0];
			set => Projectile.ai[0] = (float)value;
		}
		private bool FlurryDuplicate => CurrentAttack == AttackType.Stab && original == null;
		private ref float Timer => ref Projectile.ai[1];
		private ref float TargetAngle => ref Projectile.ai[2];

		public override void SetDefaults()
		{
			Projectile.friendly = true;
			Projectile.DamageType = DamageClass.Melee;
			Projectile.tileCollide = false;
			Projectile.ownerHitCheck = true;
			Projectile.penetrate = -1;
			Projectile.scale = 1.25f;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 18;
			Projectile.extraUpdates = 2;
			Projectile.timeLeft = 3000;
		}

		public override void OnSpawn(IEntitySource source)
		{
			TargetAngle = (Main.MouseWorld - Owner.MountedCenter).ToRotation();
			Owner.direction = TargetAngle > -MathHelper.PiOver2 && TargetAngle < MathHelper.PiOver2 ? 1 : -1;
		}

		public override void AI()
		{

			Projectile.Center = Owner.Center;
			Owner.heldProj = Projectile.whoAmI;
			Owner.itemAnimation = Owner.itemTime = 2;
			Owner.itemAnimationMax = 10;

			OriginalAI();

			switch (CurrentAttack)
			{
				case AttackType.DownSwing:
					DownSwing();
					break;
				case AttackType.Stab:
					Stab();
					break;
				case AttackType.Slash:
					Slash();
					break;
				case AttackType.UpSwing:
					UpSwing();
					break;
				case AttackType.ChargedStab:
					ChargedStab();
					break;
				case AttackType.Flurry:
					Flurry();
					break;
			}

			Timer++;
			screenshakeCooldown--;

			Owner.itemRotation = MathHelper.WrapAngle(Projectile.rotation - ((Owner.direction > 0) ? 0 : MathHelper.Pi));

			ManageCaches();
			ManageTrail();
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			float collisionPoint = 0f;
			Vector2 start = Owner.MountedCenter;
			Vector2 end = start;

			if (CurrentAttack == AttackType.ChargedStab)
				end += 1.5f * GetSpearEndVector();
			else if (CurrentAttack == AttackType.Stab)
				end += 1.1f * GetSpearEndVector();
			else
				end += GetSpearEndVector();

			return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), start, end, 10, ref collisionPoint);
		}

		public override void CutTiles()
		{
			Vector2 start = Owner.MountedCenter;
			Vector2 end = start;

			if (CurrentAttack == AttackType.ChargedStab)
				end += 1.5f * GetSpearEndVector();
			else if (CurrentAttack == AttackType.Stab)
				end += 1.1f * GetSpearEndVector();
			else
				end += GetSpearEndVector();

			Utils.PlotTileLine(start, end, 40 * Projectile.scale, DelegateMethods.CutTiles);
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			Helper.PlayPitched(Helpers.Helper.IsFleshy(target) ? "Impacts/StabFleshy" : "Impacts/Clink", 1, Main.rand.NextFloat(), Owner.Center);

			// cooldown for screenshake to avoid spazzing out
			// flurry duplicates also do not have screenshake for the same reason
			if (screenshakeCooldown < 0 && !FlurryDuplicate)
			{
				CameraSystem.shake += 3;
				screenshakeCooldown = 2;
			}

			original?.StatusNPC(target.whoAmI);

			// run modded OnHit if applicable
			if (original?.ModProjectile is ModProjectile modProj)
				modProj.OnHitNPC(target, hit, damageDone);
		}

		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
		{
			if (CurrentAttack == AttackType.ChargedStab)
			{
				if (Main.rand.NextFloat() * 100 < (Owner.GetTotalCritChance(DamageClass.Melee) + Owner.HeldItem.crit) * 2f)
					modifiers.SetCrit();

				modifiers.CritDamage *= 1.5f;
			}

			// check for flurry or flurry duplicate
			if (CurrentAttack == AttackType.Flurry || FlurryDuplicate)
				modifiers.Knockback *= (1 - (target.Center - Owner.Center).Length() / (Projectile.Size.Length() * Projectile.scale)) * 4;

			modifiers.Knockback *= 0.7f;
			modifiers.HitDirectionOverride = target.position.X > Owner.MountedCenter.X ? 1 : -1;
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Vector2 origin;
			float rotationOffset;
			SpriteEffects effects;

			if (Projectile.spriteDirection < 0)
			{
				origin = new Vector2(texture.Width * holdout, texture.Height * holdout);
				rotationOffset = MathHelper.ToRadians(135f);
				effects = SpriteEffects.None;
			}
			else
			{
				origin = new Vector2(texture.Width * (1 - holdout), texture.Height * holdout);
				rotationOffset = MathHelper.ToRadians(45f);
				effects = SpriteEffects.FlipHorizontally;
			}

			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

			if (CurrentAttack == AttackType.Slash)
			{
				Effect effect = Filters.Scene["SpearDepth"].GetShader().Shader;
				effect.Parameters["rotation"].SetValue(progressAngle);
				effect.Parameters["xRotation"].SetValue(xRotation);
				effect.Parameters["holdout"].SetValue(holdout);
				effect.CurrentTechnique.Passes[0].Apply();
			}

			Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, default, lightColor * Projectile.Opacity, Projectile.rotation + rotationOffset + slashRotationOffset, origin, Projectile.scale, effects, 0);

			Main.spriteBatch.End();
			Main.spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.TransformationMatrix);

			return false;
		}

		public override bool? CanDamage()
		{
			if (motion != Motion.None)
				return base.CanDamage();

			return false;
		}

		private void OriginalAI()
		{
			if (original != null && Timer % 3 == 0 && motion != Motion.None)
			{
				if (originalAITimer < 2)
				{
					original.Center = Projectile.Center;
					original.rotation = Projectile.rotation;
					original.velocity = TargetAngle.ToRotationVector2() * 5;
				}
				else
				{
					original.Center = Projectile.Center;
					original.rotation = Projectile.rotation;
					original.velocity = GetSpearEndVector() / Projectile.Size.Length() * 12.5f;
					original.ai[0] = 0; // seems to relate to extension of spear, so we set to 0 to prevent dusts from flying too far
				}

				// run vanilla AI
				SpearBook.AI019Spears(original);
				SpearBook.AI019SpearsOld(original);

				// run modded AI if applicable
				if (original.ModProjectile is ModProjectile modProj)
				{
					if (modProj.PreAI())
						modProj.AI();

					modProj.PostAI();
				}

				originalAITimer++;
			}
		}

		private void DownSwing()
		{
			float swingRange = MathHelper.Pi * 1.5f;
			float initialRotation = TargetAngle - Owner.direction * swingRange * 2 / 3;
			progress = Timer / 60;
			progressAngle = MathHelper.SmoothStep(0, swingRange, progress);
			motion = Motion.Swing;

			Projectile.rotation = initialRotation + Owner.direction * progressAngle;
			holdout = 0.6f + 0.2f * (float)Math.Sin(Math.PI * progress);

			Projectile.spriteDirection = Owner.direction;

			if (Timer == 0)
				Helper.PlayPitched("Effects/HeavyWhooshShort", 0.8f, 0.8f, Projectile.Center);

			if (progress > 1)
				Projectile.Kill();
		}

		private void Stab()
		{
			progress = Timer / 45;

			Projectile.rotation = TargetAngle;
			holdout = 0.3f + 0.6f * (float)Math.Sin(Math.PI * progress);

			Projectile.spriteDirection = Owner.direction;

			if (progress < 0.5)
				motion = Motion.Stab;
			else
				motion = Motion.None;

			if (Timer == 0)
				Helper.PlayPitched("Effects/HeavyWhooshShort", 0.5f, 1.2f, Projectile.Center);

			if (progress > 1)
				Projectile.Kill();
		}

		private void Slash()
		{
			float swingRange = MathHelper.Pi * 1f;
			progress = Timer / 90;

			if (progress < 0.5f)
			{
				float initialRotation = TargetAngle + Owner.direction * swingRange * 3 / 4;
				progressAngle = MathHelper.SmoothStep(0, swingRange, progress * 2);
				Projectile.rotation = initialRotation - Owner.direction * progressAngle;
				holdout = 0.6f;
				xRotation = 1.3f;
				slashRotationOffset = -MathHelper.Pi / 24;

				Projectile.scale = 1.5f;
				Projectile.spriteDirection = Owner.direction;
				motion = Motion.Slash;
			}
			else
			{
				float initialRotation = TargetAngle - Owner.direction * swingRange * 3 / 4;
				progressAngle = MathHelper.SmoothStep(0, swingRange, (progress - 0.5f) * 2);
				Projectile.rotation = initialRotation + Owner.direction * progressAngle;
				holdout = 0.6f;
				xRotation = 1f;
				slashRotationOffset = MathHelper.Pi / 24;

				Projectile.scale = 1.4f;
				Projectile.spriteDirection = -Owner.direction;
				motion = Motion.Slash2;
			}

			if (Timer == 0)
				Helper.PlayPitched("Effects/HeavyWhooshShort", 0.7f, 1.2f, Projectile.Center);

			if (progress == 0.5f)
			{
				Helper.PlayPitched("Effects/HeavyWhooshShort", 0.7f, 1.1f, Projectile.Center);
				Projectile.ResetLocalNPCHitImmunity();
			}

			if (progress > 1)
				Projectile.Kill();
		}

		private void UpSwing()
		{
			float swingRange = MathHelper.Pi * 1.5f;
			float initialRotation = TargetAngle + Owner.direction * swingRange * 2 / 3;
			progress = Timer / 60;
			progressAngle = MathHelper.SmoothStep(0, swingRange, progress);
			motion = Motion.Swing;

			Projectile.rotation = initialRotation - Owner.direction * progressAngle;
			holdout = 0.6f + 0.2f * (float)Math.Sin(Math.PI * progress);

			Projectile.spriteDirection = Owner.direction;

			if (Timer == 0)
				Helper.PlayPitched("Effects/HeavyWhooshShort", 0.8f, 0.7f, Projectile.Center);

			if (progress > 1)
				Projectile.Kill();
		}

		private void ChargedStab()
		{
			var StabEase = new EaseBuilder();
			StabEase.AddPoint(new Vector2(0, 0.6f), EaseFunction.EaseQuinticOut);
			StabEase.AddPoint(new Vector2(60, 0.3f), EaseFunction.EaseQuinticOut);
			StabEase.AddPoint(new Vector2(65, 0.3f), EaseFunction.EaseQuinticOut);
			StabEase.AddPoint(new Vector2(95, 0.9f), EaseFunction.EaseQuinticOut);
			StabEase.AddPoint(new Vector2(125, 0.9f), EaseFunction.EaseQuinticOut);
			StabEase.AddPoint(new Vector2(145, 0.5f), EaseFunction.EaseQuinticOut);

			Projectile.rotation = TargetAngle;
			holdout = StabEase.Ease(Timer);
			progress = Timer / 145;

			Projectile.spriteDirection = Owner.direction;

			if (Timer > 65 && Timer < 95)
				motion = Motion.Stab;
			else
				motion = Motion.None;

			if (Timer > 125)
				Projectile.Opacity = MathHelper.SmoothStep(1, 0, (Timer - 125) / 20);

			if (Timer == 65)
				Helper.PlayPitched("Effects/HeavyWhooshShort", 1, 0.5f, Projectile.Center);

			if (Timer == 85)
				CameraSystem.shake += 2;

			if (Timer > 145)
				Projectile.Kill();
		}

		private void Flurry()
		{
			ChargedStab();

			Owner.velocity.X *= 0.975f; // slow the player down ever so slightly

			if (Timer > 60 && Timer < 120 && Timer % 10 == 0)
			{
				float randomRotation = TargetAngle + (Main.rand.NextFloat() - 0.5f) * MathHelper.Pi / 4;
				int i = Projectile.NewProjectile(Owner.GetSource_ItemUse(Owner.HeldItem), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<SpearBookProjectile>(), Projectile.damage / 2, Projectile.knockBack, Owner.whoAmI, (float)AttackType.Stab);

				Projectile proj = Main.projectile[i];

				if (proj.ModProjectile is SpearBookProjectile)
				{
					var modProj = proj.ModProjectile as SpearBookProjectile;
					modProj.trailColor = trailColor;
					modProj.texture = texture;
					proj.Size = Projectile.Size;

					proj.ai[2] = randomRotation;
					proj.Opacity = 0.4f;
					modProj.original = null;
				}
			}
		}

		private Vector2 GetSpearEndVector()
		{
			float radius = Projectile.Size.Length() * Projectile.scale * holdout;

			if (CurrentAttack == AttackType.Slash)
			{
				// constant that applies vertical compress to a circle to make it an ellipse
				float c = (float)Math.Abs(1 / Math.Cos(xRotation));
				radius *= (float)(1 / Math.Sqrt(c - (c - 1) * Math.Pow(Math.Cos(progressAngle), 2)));
			}

			return Vector2.UnitX.RotatedBy(Projectile.rotation) * radius;
		}

		private void ManageCaches()
		{
			float length = Projectile.Size.Length() * Projectile.scale * holdout;

			// first trail
			if (motion == Motion.Swing || motion == Motion.Slash)
			{
				if (cache == null)
				{
					cache = new List<Vector2>();

					for (int i = 0; i < TRAILLENGTH; i++)
					{
						cache.Add(GetSpearEndVector() * 0.8f);
					}
				}

				cache.Add(GetSpearEndVector() * 0.8f);

				while (cache.Count > TRAILLENGTH)
				{
					cache.RemoveAt(0);
				}
			}
			else if (motion != Motion.None)
			{
				// repeat for second cache (only when it is needed for second slash or stab)
				if (cache2 == null)
				{
					cache2 = new List<Vector2>();

					for (int i = 0; i < TRAIL2LENGTH; i++)
					{
						if (motion == Motion.Stab)
							cache2.Add(GetSpearEndVector() * (CurrentAttack == AttackType.ChargedStab ? 1.5f : 1.25f));
						else
							cache2.Add(GetSpearEndVector() * 0.8f);
					}
				}

				if (motion == Motion.Stab)
					cache2.Add(GetSpearEndVector() * (CurrentAttack == AttackType.ChargedStab ? 1.5f : 1.25f));
				else
					cache2.Add(GetSpearEndVector() * 0.8f);

				while (cache2.Count > TRAIL2LENGTH)
				{
					cache2.RemoveAt(0);
				}
			}
		}

		private void ManageTrail()
		{
			float length = GetSpearEndVector().Length();

			// first trail
			if (motion == Motion.Swing || motion == Motion.Slash)
			{
				trail ??= new Trail(Main.instance.GraphicsDevice, TRAILLENGTH, new TriangularTip(40 * 4), factor => (float)Math.Min(factor, progress) * length * 0.75f, factor =>
				{
					if (factor.X >= 0.98f)
						return Color.White * 0;

					return trailColor * (float)Math.Min(factor.X, progress) * 0.5f * (float)Math.Sin(progress * 3.14f) * 2;
				});

				var realCache = new Vector2[TRAILLENGTH];

				for (int k = 0; k < TRAILLENGTH; k++)
				{
					realCache[k] = cache[k] + Owner.Center;
				}

				trail.Positions = realCache;
			}
			else if (motion != Motion.None)
			{
				// repeat for second trail (only when it is needed for second slash or stab)
				if (motion == Motion.Stab)
				{
					trail2 ??= new Trail(Main.instance.GraphicsDevice, TRAIL2LENGTH, new TriangularTip(40 * 4), factor => (1f - (float)Math.Pow(2 * factor - 1, 2)) * length * 0.5f, factor =>
					{
						if (factor.X >= 0.98f)
							return Color.White * 0;

						return trailColor * (float)Math.Min(factor.X, progress) * 0.5f * (float)Math.Sin(progress * 3.14f) * 4;
					});
				}
				else
				{
					trail2 ??= new Trail(Main.instance.GraphicsDevice, TRAIL2LENGTH, new TriangularTip(40 * 4), factor => (float)Math.Min(factor, progress) * length * 0.75f, factor =>
					{
						if (factor.X >= 0.98f)
							return Color.White * 0;

						return trailColor * (float)Math.Min(factor.X, progress) * 1.5f * (float)Math.Sin(progress * 3.14f);
					});
				}

				var realCache2 = new Vector2[TRAIL2LENGTH];

				for (int k = 0; k < TRAIL2LENGTH; k++)
				{
					realCache2[k] = cache2[k] + Owner.Center;
				}

				trail2.Positions = realCache2;
			}
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

			if (motion != Motion.Stab)
				trail?.Render(effect);

			if (motion == Motion.Slash2 || motion == Motion.Stab)
			{
				trail2?.Render(effect);
			}
		}
	}
}
