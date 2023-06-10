using StarlightRiver.Content.Items.Gravedigger;
using StarlightRiver.Core.Systems.CameraSystem;
using StarlightRiver.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics.Effects;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;
//TODO: sfx
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
			Tooltip.SetDefault("Striking enemies charges the staff with lunar energy\n" + "Condenses collected energy into a lunar orb when the final slam hits the ground");
		}

		public override void SetDefaults()
		{
			Item.damage = 45;
			Item.DamageType = DamageClass.Melee;
			Item.width = 36;
			Item.height = 44;
			Item.useTime = 25;
			Item.useAnimation = 25;
			Item.reuseDelay = 20;
			Item.channel = true;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.knockBack = 6f;
			Item.value = Item.sellPrice(0, 1, 0, 0);
			Item.crit = 4;
			Item.rare = ItemRarityID.Green;
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
			if (Main.projectile.Any(n => n.active && n.owner == player.whoAmI && n.type == ProjectileType<CrescentQuarterstaffProj>()))
				return false; // prevents possibility of duplicate projectiles

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

		public override bool MeleePrefix()
		{
			return true;
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

	internal class CrescentQuarterstaffProj : ModProjectile, IDrawAdditive
	{
		enum AttackType : int
		{
			Stab,
			Spin,
			UpperCut,
			Slam
		}

		private const int MAXCHARGE = 10;

		private List<Vector2> cache;
		private Trail trail;

		private int timer = 0;
		private int freezeTimer = 0;
		private bool active = true;
		private bool curAttackDone = false;

		private float length = 100;
		private float initialRotation = 0;
		private float zRotation = 0;

		private bool slamCharged = false;
		private bool slammed = false;

		private AttackType CurrentAttack
		{
			get => (AttackType)Projectile.ai[0];
			set => Projectile.ai[0] = (float)value;
		}
		private ref float charge => ref Projectile.ai[1];

		private Player Player => Main.player[Projectile.owner];
		private float ArmRotation => Projectile.rotation - ((Player.direction > 0) ? MathHelper.Pi / 3 : MathHelper.Pi * 2 / 3);
		private float Charge => charge / MAXCHARGE;
		private Vector2 StaffEnd => Player.GetFrontHandPosition(Player.CompositeArmStretchAmount.Full, ArmRotation) + Vector2.UnitX.RotatedBy(Projectile.rotation) * length * Projectile.scale;
		private float MeleeSpeed => Player.GetTotalAttackSpeed(DamageClass.Melee);

		private readonly static Func<float, float> StabEase = Helper.CubicBezier(0.09f, 0.71f, 0.08f, 1.62f);
		private readonly static Func<float, float> SpinEase = Helper.CubicBezier(0.6f, -0.3f, .3f, 1f);
		private readonly static Func<float, float> UppercutEase = Helper.CubicBezier(0.6f, -0.3f, 0.5f, 0.8f);
		private readonly static Func<float, float> SlamEase = Helper.CubicBezier(0.5f, -1.6f, 0.9f, -1.6f);

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
			Projectile.scale = Player.GetAdjustedItemScale(Player.HeldItem);
		}

		public override void AI()
		{
			Player.heldProj = Projectile.whoAmI;
			Projectile.velocity = Vector2.Zero;
			Lighting.AddLight(Projectile.Center, new Vector3(0.905f, 0.89f, 1) * Charge);

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

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			if (charge < MAXCHARGE)
				charge++;

			if (CurrentAttack != AttackType.Slam || timer < 50 / MeleeSpeed)
			{
				if (freezeTimer < -8) // prevent procs from multiple enemies overlapping
				{
					CameraSystem.shake += 8;
					freezeTimer = 2;
				}
			}
		}

		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
		{
			if (CurrentAttack == AttackType.Slam)
				modifiers.SourceDamage *= 1.5f;

			if (CurrentAttack == AttackType.Spin)
				modifiers.SourceDamage *= 1.2f;

			modifiers.SourceDamage *= 1 + Charge / 5;

			modifiers.HitDirectionOverride = target.position.X > Player.position.X ? 1 : -1;
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			float collisionPoint = 0f;
			Vector2 start = Player.MountedCenter;
			Vector2 end = start + Vector2.UnitX.RotatedBy(Projectile.rotation) * length * 1.3f * Projectile.scale;
			return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), start, end, 40 * Projectile.scale, ref collisionPoint);
		}

		public override void CutTiles()
		{
			Vector2 start = Player.MountedCenter;
			Vector2 end = start + Vector2.UnitX.RotatedBy(Projectile.rotation) * length * 1.3f * Projectile.scale;
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
			effect.Parameters["vnoise"].SetValue(Request<Texture2D>(AssetDirectory.MoonstoneItem + "CrescentQuarterstaffMap").Value);
			effect.CurrentTechnique.Passes[0].Apply();

			spriteBatch.Draw(head, position - Main.screenPosition, null, lightColor, Projectile.rotation + 0.78f, origin, scale, SpriteEffects.None, 0);

			spriteBatch.End();
			spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

			effect.Parameters["time"].SetValue(StarlightWorld.visualTimer * 0.2f);
			effect.Parameters["fireHeight"].SetValue(0.03f * Charge);
			effect.Parameters["fnoise"].SetValue(Request<Texture2D>(AssetDirectory.MoonstoneItem + "DatsuzeiFlameMap1").Value);
			effect.Parameters["fnoise2"].SetValue(Request<Texture2D>(AssetDirectory.MoonstoneItem + "DatsuzeiFlameMap2").Value);
			effect.Parameters["vnoise"].SetValue(Request<Texture2D>(AssetDirectory.MoonstoneItem + "CrescentQuarterstaffMap").Value);
			effect.CurrentTechnique.Passes[1].Apply();

			spriteBatch.Draw(head, position - Main.screenPosition, null, lightColor, Projectile.rotation + 0.78f, origin, scale, SpriteEffects.None, 0);

			spriteBatch.End();
			spriteBatch.Begin(default, default, default, default, RasterizerState.CullNone, default, Main.GameViewMatrix.TransformationMatrix);

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

		private void StabAttack()
		{
			float swingAngle = Player.direction * -MathHelper.Pi / 10;
			float realInitRot = Player.direction > 0 || initialRotation < 0 ? initialRotation : initialRotation - MathHelper.TwoPi;
			float droop = Player.direction > 0 ? (MathHelper.PiOver2 - Math.Abs(realInitRot)) : (MathHelper.PiOver2 - Math.Abs(realInitRot + MathHelper.Pi));
			float progress = StabEase((float)timer / 15 * MeleeSpeed);

			length = 60 + 40 * progress;
			Projectile.rotation = initialRotation + swingAngle * (1 - progress) * droop;
			active = progress > 0;

			if (timer < 9 / MeleeSpeed && freezeTimer < 0)
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

			if (timer > 15 / MeleeSpeed)
				curAttackDone = true;
		}

		private void SpinAttack()
		{
			float startAngle = Player.direction > 0 || initialRotation < 0 ? initialRotation : initialRotation - MathHelper.TwoPi;
			float finalAngle = Player.direction > 0 ? MathHelper.Pi * 4.25f : -MathHelper.Pi * 5.25f;
			float swingAngle = finalAngle - startAngle;

			float progress = SpinEase(timer / 90f * MeleeSpeed);
			Projectile.rotation = startAngle + swingAngle * progress;
			length = 100 - 40 * progress;
			zRotation = MathHelper.TwoPi * 2 * progress + ((Player.direction > 0) ? MathHelper.Pi : 0);
			Player.UpdateRotation(zRotation);
			active = progress > 0;

			if ((timer == (int)(10 / MeleeSpeed) || timer == (int)(40 / MeleeSpeed) || timer == (int)(70 / MeleeSpeed)) && freezeTimer < 0)
				Projectile.ResetLocalNPCHitImmunity();

			if ((timer == (int)(25 / MeleeSpeed) || timer == (int)(50 / MeleeSpeed)) && freezeTimer < 0)
				Helper.PlayPitched("Effects/HeavyWhoosh", 0.4f, Main.rand.NextFloat(-0.1f, 0.1f));

			if (timer > 90 / MeleeSpeed)
				curAttackDone = true;
		}

		private void UppercutAttack()
		{
			float startAngle = -MathHelper.PiOver2 - Player.direction * MathHelper.Pi * 1.25f;
			float swingAngle = Player.direction * -MathHelper.Pi * 2 / 3;

			float progress = UppercutEase((float)timer / 20 * MeleeSpeed);
			Projectile.rotation = startAngle + swingAngle * progress;
			length = 60 + 40 * progress;
			active = progress > 0;

			if (timer == 0)
			{
				Projectile.ResetLocalNPCHitImmunity();
				Helper.PlayPitched("Effects/HeavyWhooshShort", 0.4f, Main.rand.NextFloat(-0.1f, 0.1f));
			}

			if (timer > 20 / MeleeSpeed)
				curAttackDone = true;
		}

		private void SlamAttack()
		{
			if (timer == 0 && charge > 0 && freezeTimer < 0)
			{
				for (int i = 0; i < 64; i++)
				{
					Vector2 dustOffset = Vector2.UnitX.RotatedBy(MathHelper.TwoPi * i / 64);
					var dust = Dust.NewDustDirect(StaffEnd + dustOffset * 10, 0, 0, ModContent.DustType<Dusts.GlowFastDecelerate>(), 0, 0, 1, new Color(120, 120, 255));
					dust.velocity = 5 * dustOffset;
				}

				freezeTimer = 10;
				timer++; // to prevent repeated freezes
				slamCharged = true;
			}

			Player.direction = Main.MouseWorld.X > Player.position.X ? 1 : -1; // prevents incorrect aiming since it takes so long to charge up

			float progress = 0;
			float startAngle = -MathHelper.PiOver2 + Player.direction * MathHelper.Pi / 12;
			float swingAngle = Player.direction * MathHelper.PiOver2 * 1.05f;

			// stops further movement after slam
			if (!slammed)
			{
				progress = SlamEase((float)timer / 45 * MeleeSpeed);
				Projectile.rotation = startAngle + swingAngle * progress;
			}

			// prevents clipping through blocks when swinging really fast
			if (timer > (20 / MeleeSpeed) && timer <= (45 / MeleeSpeed) && freezeTimer < 0 && !slammed)
			{
				float prevPos = startAngle + swingAngle * SlamEase((timer - 1) / 45f * MeleeSpeed);
				float nextPos = startAngle + swingAngle * progress;

				for (int k = 0; k < 15 * MeleeSpeed; k++)
				{
					Projectile.rotation = MathHelper.Lerp(prevPos, nextPos, k / (10f * MeleeSpeed));

					TentativelyExecuteSlam();

					if (slammed)
						break;
				}
			}

			if (timer == (int)(30 / MeleeSpeed) && freezeTimer < 0)
			{
				Projectile.ResetLocalNPCHitImmunity();
			}

			if (timer > 60 / MeleeSpeed)
				curAttackDone = true;
		}

		// Function that executes slam if conditions are fulfilled
		private void TentativelyExecuteSlam()
		{
			Vector2 tilePos = StaffEnd;
			tilePos.Y += 15;
			tilePos /= 16;

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

				if (slamCharged)
				{
					DustHelper.DrawDustImage(StaffEnd + Vector2.One * 4, ModContent.DustType<Dusts.GlowFastDecelerate>(), 0.05f, ModContent.Request<Texture2D>(AssetDirectory.MoonstoneItem + "MoonstoneHamaxe_Crescent").Value, 0.7f, 0, new Color(120, 120, 255));

					for (int i = 0; i < 64; i++)
					{
						Vector2 dustOffset = Vector2.UnitX.RotatedBy(MathHelper.TwoPi * i / 64);
						var dust = Dust.NewDustDirect(StaffEnd + dustOffset * 50, 0, 0, ModContent.DustType<Dusts.GlowFastDecelerate>(), 0, 0, 1, new Color(120, 120, 255));
						dust.velocity = -5 * dustOffset;
					}

					var proj = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), StaffEnd - Vector2.UnitY * 32 * Projectile.scale, new Vector2(Player.direction * 10, 0) * MeleeSpeed, ProjectileType<CrescentOrb>(), (int)MathHelper.Lerp(Projectile.damage * 0.25f, Projectile.damage, Charge), 0, Projectile.owner, 0, 0);
					proj.scale = (1 + Charge) / 2 * Projectile.scale;

					charge = 0;
				}

				CameraSystem.shake += 16;
				Projectile.NewProjectile(Projectile.GetSource_FromThis(), StaffEnd, Vector2.Zero, ProjectileType<GravediggerSlam>(), 0, 0, Player.whoAmI);
			}
		}

		private void NextAttack()
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
			if (CurrentAttack == AttackType.Slam && slamCharged)
			{
				Main.spriteBatch.End();

				Effect effect = Filters.Scene["CeirosRing"].GetShader().Shader;

				var world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
				Matrix view = Main.GameViewMatrix.TransformationMatrix;
				var projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

				effect.Parameters["time"].SetValue(Main.GameUpdateCount * 0.02f);
				effect.Parameters["repeats"].SetValue(1f);
				effect.Parameters["transformMatrix"].SetValue(world * view * projection);
				effect.Parameters["sampleTexture"].SetValue(Request<Texture2D>("StarlightRiver/Assets/EnergyTrail").Value);

				trail?.Render(effect);

				Main.spriteBatch.Begin(default, default, default, default, RasterizerState.CullNone, default, Main.GameViewMatrix.TransformationMatrix);
			}
		}

		public void DrawAdditive(SpriteBatch spriteBatch)
		{
			if (CurrentAttack == AttackType.Slam && slamCharged)
			{
				Texture2D texFlare = ModContent.Request<Texture2D>(AssetDirectory.Assets + "StarTexture").Value;
				Texture2D texBloom = ModContent.Request<Texture2D>(AssetDirectory.Keys + "GlowAlpha").Value;

				float flareRotation = MathHelper.SmoothStep(0, MathHelper.TwoPi, timer / 40f);
				float flareScale = timer < 20 ? MathHelper.SmoothStep(0, 1, timer / 20f) : MathHelper.SmoothStep(1, 0, (timer - 20) / 20f);

				var color = new Color(78, 87, 191);

				Vector2 pos = StaffEnd + Vector2.UnitX.RotatedBy(Projectile.rotation) * 10 * Projectile.scale;

				spriteBatch.Draw(texBloom, pos - Main.screenPosition, null, color, 0, texBloom.Size() / 2, Projectile.scale * 3 * flareScale, default, default);
				spriteBatch.Draw(texFlare, pos - Main.screenPosition, null, color * 2, flareRotation, texFlare.Size() / 2, Projectile.scale * 0.75f * flareScale, default, default);
			}
		}
	}
}