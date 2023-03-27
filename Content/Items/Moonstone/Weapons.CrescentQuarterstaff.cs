using StarlightRiver.Content.Items.Gravedigger;
using StarlightRiver.Core.Systems.CameraSystem;
using StarlightRiver.Helpers;
using System;
using System.Linq;
using System.IO;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics.Effects;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;
using System.Collections.Generic;
using Terraria.Audio;

namespace StarlightRiver.Content.Items.Moonstone
{
	public class CrescentQuarterstaff : ModItem
	{
		public int charge = 0;
		private int chargeDepletionTimer = 0;
		public int combo = 0;
		private int comboResetTimer = 0;

		public override string Texture => AssetDirectory.MoonstoneItem + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Crescent Quarterstaff");
			Tooltip.SetDefault("Update this egshels");
		}

		public override void SetDefaults()
		{
			Item.damage = 100;
			Item.DamageType = DamageClass.Melee;
			Item.width = 36;
			Item.height = 44;
			Item.useTime = 12;
			Item.useAnimation = 12;
			Item.reuseDelay = 20;
			Item.channel = true;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.knockBack = 10f;
			Item.value = Item.sellPrice(0, 1, 0, 0);
			Item.crit = 4;
			Item.rare = 2;
			Item.shootSpeed = 14f;
			Item.autoReuse = false;
			Item.shoot = ProjectileType<CrescentQuarterstaffProj>();
			Item.noUseGraphic = true;
			Item.noMelee = true;
			Item.autoReuse = true;
		}

		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ItemType<MoonstoneBarItem>(), 12);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			int proj = Projectile.NewProjectile(source, position, velocity, type, damage, knockback, Main.myPlayer, combo, charge);
			return false;
		}

		public override void UpdateInventory(Player player)
		{
			if (Main.projectile.Any(n => n.active && n.owner == player.whoAmI && n.type == ProjectileType<CrescentQuarterstaffProj>()))
			{
				chargeDepletionTimer = 0;
				comboResetTimer = 0;
			}
			else
			{
				chargeDepletionTimer++;
				comboResetTimer++;

				if (chargeDepletionTimer > 30 && charge > 0)
				{
					charge--;
					chargeDepletionTimer = 0;
				}

				if (comboResetTimer > 60)
					combo = 0;
			}
		}

		public override void NetSend(BinaryWriter writer)
		{
			writer.Write(charge);
			writer.Write(chargeDepletionTimer);
			writer.Write(combo);
			writer.Write(comboResetTimer);
		}

		public override void NetReceive(BinaryReader reader)
		{
			charge = reader.ReadInt32();
			chargeDepletionTimer = reader.ReadInt32();
			combo = reader.ReadInt32();
			comboResetTimer = reader.ReadInt32();
		}
	}

	internal class CrescentQuarterstaffProj : ModProjectile
	{
		enum AttackType : int
		{
			Stab,
			Spin,
			UpperCut,
			Slam
		}

		private const int MAXCHARGE = 10;

		private AttackType CurrentAttack
		{
			get => (AttackType)Projectile.ai[0];
			set => Projectile.ai[0] = (float)value;
		}

		private List<Vector2> cache;
		private Trail trail;

		private ref float charge => ref Projectile.ai[1];
		private int freezeTimer = 0;
		private bool active = true;
		private bool curAttackDone = false;

		private float length = 100;
		private float initialRotation = 0;
		private float zRotation = 0;

		private int timer = 0;
		private bool slammed = false;

		private Player Player => Main.player[Projectile.owner];
		private float ArmRotation => Projectile.rotation - ((Player.direction > 0) ? MathHelper.Pi / 3 : MathHelper.Pi * 2 / 3);
		private float Charge => charge / (float)MAXCHARGE;
		private Vector2 StaffEnd => Player.GetFrontHandPosition(Player.CompositeArmStretchAmount.Full, ArmRotation) + Vector2.UnitX.RotatedBy(Projectile.rotation) * length;


		private Func<float, float> StabEase = Helper.CubicBezier(0.09f, 0.71f, 0.08f, 1.62f);
		private Func<float, float> SpinEase = Helper.CubicBezier(0.6f, -0.3f, .3f, 1f);
		private Func<float, float> UppercutEase = Helper.CubicBezier(0.6f, -0.3f, 0.5f, 0.8f);
		private Func<float, float> SlamEase = Helper.CubicBezier(0.5f, -1.6f, 0.9f, -1.6f);

		public override string Texture => AssetDirectory.MoonstoneItem + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Crescent Quarterstaff");
		}

		public override void SetDefaults()
		{
			Projectile.friendly = true;
			Projectile.DamageType = DamageClass.Melee;
			Projectile.tileCollide = false;
			Projectile.Size = new Vector2(150, 150);
			Projectile.penetrate = -1;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = -1;
			Projectile.ownerHitCheck = true;

		}

		public override void OnSpawn(IEntitySource source)
		{
			initialRotation = (Main.MouseWorld - Player.MountedCenter).ToRotation();
		}

		public override bool PreAI()
		{
			Player.heldProj = Projectile.whoAmI;
			Projectile.velocity = Vector2.Zero;
			Lighting.AddLight(Projectile.Center, new Vector3(0.905f, 0.89f, 1) * Charge);
			return true;
		}

		public override void AI()
		{
			if (!Player.active || Player.dead || Player.noItems || Player.CCed)
			{
				Projectile.Kill();
				return;
			}

			switch (CurrentAttack)
			{
				case AttackType.Spin:
					SpinAttack();
					break;
				case AttackType.UpperCut:
					UppercutAttack();
					break;
				case AttackType.Slam:
					SlamAttack();
					break;
				default:
					StabAttack();
					break;
			}

			Projectile.Center = Player.Center;

			AdjustPlayer();
			if (freezeTimer < 0)
				timer++;

			freezeTimer--;

			if (curAttackDone)
				NextAttack();

			if (Main.netMode != NetmodeID.Server)
			{
				ManageCaches();
				ManageTrail();
			}
		}

		public override void Kill(int timeleft)
		{
			Player.itemAnimation = Player.itemTime = 0;
			Player.UpdateRotation(0);
		}

		public override bool? CanHitNPC(NPC target)
		{
			if (!active)
				return false;

			return base.CanHitNPC(target);
		}

		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
		{
			if (charge < MAXCHARGE)
				charge++;

			if (CurrentAttack != AttackType.Slam || timer < 50)
			{
				if (freezeTimer < -8) // prevent procs from multiple enemies overlapping
				{ 
					CameraSystem.shake += 8;
					freezeTimer = 2;
				}
			}
		}

		public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
		{
			if (CurrentAttack == AttackType.Slam)
				damage = (int)(damage * 1.5f);

			if (CurrentAttack == AttackType.Spin)
				damage = (int)(damage * 1.2f);

			damage = (int)(damage * (1 + Charge / 5));

			hitDirection = target.position.X > Player.position.X ? 1 : -1;
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			float collisionPoint = 0f;
			Vector2 start = Player.MountedCenter;
			Vector2 end = start + Vector2.UnitX.RotatedBy(Projectile.rotation) * length * 1.5f;
			return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), start, end, 40 * Projectile.scale, ref collisionPoint);
		}

		public override void CutTiles()
		{
			Vector2 start = Player.MountedCenter;
			Vector2 end = start + Vector2.UnitX.RotatedBy(Projectile.rotation) * length * 1.5f;
			Utils.PlotTileLine(start, end, 40 * Projectile.scale, DelegateMethods.CutTiles);
		}

		public override bool PreDraw(ref Color lightColor)
		{
			SpriteBatch spriteBatch = Main.spriteBatch;
			Texture2D head = Request<Texture2D>(Texture + "_Head").Value;
			Texture2D tex = TextureAssets.Projectile[Projectile.type].Value;

			var origin = new Vector2(140, 10);
			origin.X -= length;
			origin.Y += length;

			var scale = new Vector2(Projectile.scale, Projectile.scale);

			Vector2 position = Player.GetFrontHandPosition(Player.CompositeArmStretchAmount.Full, ArmRotation);

			spriteBatch.End();
			spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

			Effect effect = Filters.Scene["MoonFireAura"].GetShader().Shader;
			effect.Parameters["time"].SetValue(StarlightWorld.visualTimer * 0.2f);
			effect.Parameters["fireHeight"].SetValue(0.03f * Charge);
			effect.Parameters["fnoise"].SetValue(Request<Texture2D>(AssetDirectory.MoonstoneItem + "DatsuzeiFlameMap1").Value);
			effect.Parameters["fnoise2"].SetValue(Request<Texture2D>(AssetDirectory.MoonstoneItem + "DatsuzeiFlameMap2").Value);
			effect.Parameters["vnoise"].SetValue(Request<Texture2D>(AssetDirectory.MoonstoneItem + "QuarterstaffMap").Value);
			effect.CurrentTechnique.Passes[0].Apply();

			spriteBatch.Draw(head, position - Main.screenPosition, null, lightColor, Projectile.rotation + 0.78f, origin, scale, SpriteEffects.None, 0);

			spriteBatch.End();
			spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

			effect.Parameters["time"].SetValue(StarlightWorld.visualTimer * 0.2f);
			effect.Parameters["fireHeight"].SetValue(0.03f * Charge);
			effect.Parameters["fnoise"].SetValue(Request<Texture2D>(AssetDirectory.MoonstoneItem + "DatsuzeiFlameMap1").Value);
			effect.Parameters["fnoise2"].SetValue(Request<Texture2D>(AssetDirectory.MoonstoneItem + "DatsuzeiFlameMap2").Value);
			effect.Parameters["vnoise"].SetValue(Request<Texture2D>(AssetDirectory.MoonstoneItem + "QuarterstaffMap").Value);
			effect.CurrentTechnique.Passes[1].Apply();

			spriteBatch.Draw(head, position - Main.screenPosition, null, lightColor, Projectile.rotation + 0.78f, origin, scale, SpriteEffects.None, 0);

			spriteBatch.End();
			spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.TransformationMatrix);

			DrawPrimitives();

			spriteBatch.Draw(tex, position - Main.screenPosition, null, Color.Lerp(lightColor, Color.White, Charge), Projectile.rotation + 0.78f, origin, scale, SpriteEffects.None, 0);
			return false;
		}

		public override void SendExtraAI(BinaryWriter writer)
		{
			writer.Write(initialRotation);
		}

		public override void ReceiveExtraAI(BinaryReader reader)
		{
			initialRotation = reader.ReadSingle();
		}

		private void AdjustPlayer()
		{
			if (CurrentAttack != AttackType.Spin)
				Player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, ArmRotation);
			else
				Player.itemRotation = ArmRotation;

			Player.itemAnimation = Player.itemTime = 2;
		}

		public void StabAttack()
		{
			float swingAngle = Player.direction * -MathHelper.Pi / 10;
			float realInitRot = Player.direction > 0 || initialRotation < 0 ? initialRotation : initialRotation - MathHelper.TwoPi;
			float droop = Player.direction > 0 ? (MathHelper.PiOver2 - Math.Abs(realInitRot)) : (MathHelper.PiOver2 - Math.Abs(realInitRot + MathHelper.Pi));
			float progress = StabEase((float)timer / 15);

			length = 60 + 40 * progress;
			Projectile.rotation = initialRotation + swingAngle * (1 - progress) * droop;
			active = progress > 0;

			if (timer < 9 && freezeTimer < 0)
			{
				Vector2 vel = Vector2.UnitX.RotatedBy(Projectile.rotation) * progress * progress / 2;
				vel.Y *= 0.4f;
				Player.velocity += vel;
			}

			if (timer == 0)
			{
				Projectile.ResetLocalNPCHitImmunity();
				Helper.PlayPitched("Effects/HeavyWhooshShort", 0.3f, Main.rand.NextFloat(-0.1f, 0.1f));
			}

			if (timer > 15)
				curAttackDone = true;
		}

		public void SpinAttack()
		{
			float startAngle = Player.direction > 0 || initialRotation < 0 ? initialRotation : initialRotation - MathHelper.TwoPi;
			float finalAngle = Player.direction > 0 ? MathHelper.Pi * 4.25f : -MathHelper.Pi * 5.25f;
			float swingAngle = finalAngle - startAngle;

			float progress = SpinEase((float)timer / 90);
			Projectile.rotation = startAngle + swingAngle * progress;
			length = 100 - 40 * progress;
			zRotation = MathHelper.TwoPi * 2 * progress + ((Player.direction > 0) ? MathHelper.Pi : 0);
			Player.UpdateRotation(zRotation);
			active = progress > 0;

			if ((timer == 10 || timer == 40 || timer == 70) && freezeTimer < 0)
				Projectile.ResetLocalNPCHitImmunity();

			if ((timer == 25 || timer == 50) && freezeTimer < 0)
				Helper.PlayPitched("Effects/HeavyWhoosh", 0.4f, Main.rand.NextFloat(-0.1f, 0.1f));

			if (timer > 90)
				curAttackDone = true;
		}

		public void UppercutAttack()
		{
			float startAngle = -MathHelper.PiOver2 - Player.direction * MathHelper.Pi * 1.25f;
			float swingAngle = Player.direction * -MathHelper.Pi * 2 / 3;

			float progress = UppercutEase((float)timer / 20);
			Projectile.rotation = startAngle + swingAngle * progress;
			length = 60 + 40 * progress;
			active = progress > 0;

			if (timer == 0)
			{
				Projectile.ResetLocalNPCHitImmunity();
				Helper.PlayPitched("Effects/HeavyWhooshShort", 0.4f, Main.rand.NextFloat(-0.1f, 0.1f));
			}

			if (timer > 20)
				curAttackDone = true;
		}

		public void SlamAttack()
		{
			if (timer == 0 && charge > 0 && freezeTimer < 0)
			{
				for (int i = 0; i < 64; i++)
				{
					Vector2 dustOffset = Vector2.UnitX.RotatedBy(MathHelper.TwoPi * i / 64);
					Dust dust = Dust.NewDustDirect(StaffEnd + dustOffset * 10, 0, 0, ModContent.DustType<Dusts.GlowFastDecelerate>(), 0, 0, 1, new Color(120, 120, 255));
					dust.velocity = 5 * dustOffset;
				}

				freezeTimer = 5;
				timer++; // to prevent repeated freezes
			}

			if (!slammed)
			{
				float startAngle = -MathHelper.PiOver2 + Player.direction * MathHelper.Pi / 12;
				float swingAngle = Player.direction * MathHelper.PiOver2 * 1.05f;

				float progress = SlamEase((float)timer / 45);
				Projectile.rotation = startAngle + swingAngle * progress;
			}

			Vector2 tilePos = StaffEnd;
			tilePos.Y += 15;
			tilePos /= 16;

			if (timer <= 45 && freezeTimer < 0 && !slammed)
			{
				Tile tile = Main.tile[(int)tilePos.X, (int)tilePos.Y];
				if (tile.HasTile && Main.tileSolid[tile.TileType] && Math.Sign(Projectile.rotation.ToRotationVector2().X) == Player.direction)
				{
					slammed = true;

					for (int i = 0; i < 13; i++)
					{
						Vector2 dustVel = Vector2.UnitY.RotatedBy(Main.rand.NextFloat(-0.9f, 0.9f)) * Main.rand.NextFloat(-2, -0.5f);
						dustVel.X *= 10;

						if (Math.Abs(dustVel.X) < 6)
							dustVel.X += Math.Sign(dustVel.X) * 6;

						Dust.NewDustPerfect(tilePos * 16 - new Vector2(Main.rand.Next(-20, 20), 17), ModContent.DustType<Dusts.CrescentSmoke>(), dustVel, 0, new Color(236, 214, 146) * 0.15f, Main.rand.NextFloat(0.5f, 1));
					}

					if (Charge > 0)
					{
						DustHelper.DrawDustImage(StaffEnd + Vector2.One * 4, ModContent.DustType<Dusts.GlowFastDecelerate>(), 0.05f, ModContent.Request<Texture2D>("StarlightRiver/Assets/Items/Moonstone/MoonstoneHamaxe_Crescent").Value, 0.7f, 0, new Color(120, 120, 255));
						SoundEngine.PlaySound(SoundID.MaxMana, Projectile.Center);

						for (int i = 0; i < 64; i++)
						{
							Vector2 dustOffset = Vector2.UnitX.RotatedBy(MathHelper.TwoPi * i / 64);
							Dust dust = Dust.NewDustDirect(StaffEnd + dustOffset * 50, 0, 0, ModContent.DustType<Dusts.GlowFastDecelerate>(), 0, 0, 1, new Color(120, 120, 255));
							dust.velocity = -5 * dustOffset;
						}

						var proj = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center, new Vector2(0, 7), ProjectileType<QuarterOrb>(), (int)MathHelper.Lerp(Projectile.damage * 0.25f, Projectile.damage, Charge), 0, Projectile.owner, 0, 0);
						proj.scale = (1 + Charge) / 2;

						if (proj.ModProjectile is QuarterOrb modproj)
							modproj.moveDirection = new Vector2(-Player.direction, -1);

						charge = 0;
					}

					CameraSystem.shake += 12;
					Projectile.NewProjectile(Projectile.GetSource_FromThis(), StaffEnd, Vector2.Zero, ProjectileType<GravediggerSlam>(), 0, 0, Player.whoAmI);
				}
			}

			if (timer == 30 && freezeTimer < 0)
			{
				Projectile.ResetLocalNPCHitImmunity();
			}

			if (timer > 60)
				curAttackDone = true;
		}

		public void NextAttack()
		{
			timer = 0;
			freezeTimer = 0;

			zRotation = 0;
			Player.UpdateRotation(0);

			if (CurrentAttack < AttackType.Slam)
			{
				CurrentAttack++;
			}
			else
			{
				CurrentAttack = AttackType.Stab;
				initialRotation = (Main.MouseWorld - Player.MountedCenter).ToRotation();
			}

			Player.direction = Main.MouseWorld.X > Player.position.X ? 1 : -1;
			curAttackDone = false;
			slammed = false;

			if (Player.HeldItem.ModItem is CrescentQuarterstaff staff)
			{
				staff.charge = (int)charge;
				staff.combo = (int)CurrentAttack;
			}

			if (!Player.channel)
			{
				Projectile.Kill();
			}
		}

		private void ManageCaches()
		{
			if (cache is null)
			{
				cache = new List<Vector2>();
				for (int i = 0; i < 16; i++)
				{
					cache.Add(StaffEnd);
				}
			}

			for (int i = 0; i < 16; i++)
			{
				cache[i] = cache[i] - Vector2.UnitY * 8;
			}

			cache.Add(StaffEnd);

			while (cache.Count > 16)
			{
				cache.RemoveAt(0);
			}
		}

		private void ManageTrail()
		{
			trail ??= new Trail(Main.instance.GraphicsDevice, 16, new TriangularTip(8), factor => 50f * Charge, factor => new Color(78, 87, 191) * Charge * 0.7f * factor.X);

			trail.Positions = cache.ToArray();

			trail.NextPosition = (StaffEnd - Player.MountedCenter).RotatedBy(Player.direction - Math.PI / 6) + Player.MountedCenter;
		}

		public void DrawPrimitives()
		{
			if (CurrentAttack == AttackType.Slam)
			{
			Main.spriteBatch.End();

			Effect effect = Filters.Scene["CeirosRing"].GetShader().Shader;

			var world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
			Matrix view = Main.GameViewMatrix.ZoomMatrix;
			var projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

			effect.Parameters["time"].SetValue(Main.GameUpdateCount * 0.02f);
			effect.Parameters["repeats"].SetValue(1f);
			effect.Parameters["transformMatrix"].SetValue(world * view * projection);
			effect.Parameters["sampleTexture"].SetValue(Request<Texture2D>("StarlightRiver/Assets/EnergyTrail").Value);

			trail?.Render(effect);

			Main.spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.TransformationMatrix);
			}
		}
	}

	public class QuarterOrb : ModProjectile, IDrawAdditive
	{
		public Vector2 moveDirection;
		public Vector2 newVelocity = Vector2.Zero;
		public float speed = 7f;

		bool collideX = false;
		bool collideY = false;

		public override string Texture => AssetDirectory.MoonstoneItem + Name;

		public override void SetStaticDefaults()
		{
			ProjectileID.Sets.TrailCacheLength[Projectile.type] = 2;
			ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
			DisplayName.SetDefault("Lunar Orb");
		}

		public override void SetDefaults()
		{
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.friendly = true;
			Projectile.width = Projectile.height = 64;
			Projectile.timeLeft = 180;
			Projectile.ignoreWater = true;
		}
		public override void AI()
		{
			Projectile.width = Projectile.height = (int)(64 * Projectile.scale);
			newVelocity = Collide();

			if (Math.Abs(newVelocity.X) < 0.5f)
				collideX = true;
			else
				collideX = false;

			if (Math.Abs(newVelocity.Y) < 0.5f)
				collideY = true;
			else
				collideY = false;

			if (Projectile.ai[1] == 0f)
			{
				Projectile.rotation += (float)(moveDirection.X * moveDirection.Y) * 0.1f;

				if (collideY)
					Projectile.ai[0] = 2f;

				if (!collideY && Projectile.ai[0] == 2f)
				{
					moveDirection.X = -moveDirection.X;
					Projectile.ai[1] = 1f;
					Projectile.ai[0] = 1f;
				}

				if (collideX)
				{
					moveDirection.Y = -moveDirection.Y;
					Projectile.ai[1] = 1f;
				}
			}
			else
			{
				Projectile.rotation -= (float)(moveDirection.X * moveDirection.Y) * 0.1f;

				if (collideX)
					Projectile.ai[0] = 2f;

				if (!collideX && Projectile.ai[0] == 2f)
				{
					moveDirection.Y = -moveDirection.Y;
					Projectile.ai[1] = 0f;
					Projectile.ai[0] = 1f;
				}

				if (collideY)
				{
					moveDirection.X = -moveDirection.X;
					Projectile.ai[1] = 0f;
				}
			}

			Projectile.velocity = speed * moveDirection;
			Projectile.velocity = Collide();
		}

		protected virtual Vector2 Collide()
		{
			return Collision.noSlopeCollision(Projectile.position, Projectile.velocity, Projectile.width, Projectile.height, true, true);
		}

		public override bool PreDraw(ref Color lightColor)
		{
			return false;
		}

		public void DrawAdditive(SpriteBatch spriteBatch)
		{
			Texture2D texGlow = Request<Texture2D>("StarlightRiver/Assets/Keys/Glow").Value;

			int sin = (int)(Math.Sin(StarlightWorld.visualTimer * 3) * 40f);
			float opacity = Projectile.timeLeft > 20 ? 1 : (float)Projectile.timeLeft / 20f;
			var color = new Color(72 + sin, 30 + sin / 2, 127);

			spriteBatch.Draw(texGlow, Projectile.Center - Main.screenPosition, null, color * Projectile.scale * opacity, 0, texGlow.Size() / 2, Projectile.scale / 2, default, default);
			spriteBatch.Draw(texGlow, Projectile.Center - Main.screenPosition, null, color * Projectile.scale * 1.2f * opacity, 0, texGlow.Size() / 2, Projectile.scale * 0.8f, default, default);

			Effect effect1 = Filters.Scene["CrescentOrb"].GetShader().Shader;
			effect1.Parameters["sampleTexture"].SetValue(Request<Texture2D>("StarlightRiver/Assets/Items/Moonstone/QuarterstaffMap").Value);
			effect1.Parameters["sampleTexture2"].SetValue(Request<Texture2D>("StarlightRiver/Assets/Bosses/VitricBoss/LaserBallDistort").Value);
			effect1.Parameters["uTime"].SetValue(Main.GameUpdateCount * 0.01f);
			effect1.Parameters["opacity"].SetValue(opacity);

			spriteBatch.End();
			spriteBatch.Begin(default, BlendState.NonPremultiplied, default, default, default, effect1, Main.GameViewMatrix.ZoomMatrix);

			spriteBatch.Draw(TextureAssets.Projectile[Projectile.type].Value, Projectile.Center - Main.screenPosition, null, Color.White * Projectile.scale, Projectile.rotation, Vector2.One * 32, Projectile.scale, 0, 0);

			spriteBatch.End();
			spriteBatch.Begin(default, BlendState.Additive, SamplerState.PointWrap, default, default, default, Main.GameViewMatrix.ZoomMatrix);
		}
	}
}