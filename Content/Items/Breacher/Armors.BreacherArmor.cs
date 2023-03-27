using StarlightRiver.Content.Dusts;
using StarlightRiver.Core.Systems.CameraSystem;
using StarlightRiver.Core.Systems.ScreenTargetSystem;
using StarlightRiver.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria.GameContent;
using Terraria.Graphics.Effects;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Items.Breacher
{
	[AutoloadEquip(EquipType.Head)]
	public class BreacherHead : ModItem
	{
		public override string Texture => AssetDirectory.BreacherItem + "BreacherHead";

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Breacher Visor");
			Tooltip.SetDefault("15% increased ranged critical strike damage");
		}

		public override void SetDefaults()
		{
			Item.width = 28;
			Item.height = 28;
			Item.value = 8000;
			Item.defense = 5;
			Item.rare = ItemRarityID.Orange;
		}

		public override void UpdateEquip(Player Player)
		{
			Player.GetModPlayer<CritMultiPlayer>().RangedCritMult += 0.15f;
		}

		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ItemType<SpaceEvent.Astroscrap>(), 15);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}
	}

	[AutoloadEquip(EquipType.Body)]
	public class BreacherChest : ModItem
	{
		public override string Texture => AssetDirectory.BreacherItem + "BreacherChest";

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Breacher Chestplate");
			Tooltip.SetDefault("10% increased ranged damage");
		}

		public override void SetDefaults()
		{
			Item.width = 34;
			Item.height = 20;
			Item.value = 6000;
			Item.defense = 6;
			Item.rare = ItemRarityID.Orange;
		}

		public override void UpdateEquip(Player Player)
		{
			Player.GetDamage(DamageClass.Ranged) += 0.1f;
		}

		public override bool IsArmorSet(Item head, Item body, Item legs)
		{
			return head.type == ItemType<BreacherHead>() && legs.type == ItemType<BreacherLegs>();
		}

		public override void UpdateArmorSet(Player Player)
		{
			Player.setBonus = "A spotter drone follows you, building energy with kills\nDouble tap DOWN to consume it and call down an orbital strike on an enemy";

			if (Player.ownedProjectileCounts[ProjectileType<SpotterDrone>()] < 1 && !Player.dead)
				Projectile.NewProjectile(Player.GetSource_Accessory(Item), Player.Center, Vector2.Zero, ProjectileType<SpotterDrone>(), (int)(50 * Player.GetDamage(DamageClass.Ranged).Multiplicative), 1.5f, Player.whoAmI);
		}

		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ItemType<SpaceEvent.Astroscrap>(), 20);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}
	}

	[AutoloadEquip(EquipType.Legs)]
	public class BreacherLegs : ModItem
	{
		public override string Texture => AssetDirectory.BreacherItem + "BreacherLegs";

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Breacher Leggings");
			Tooltip.SetDefault("up to 20% ranged critical strike damage based on speed");
		}

		public override void SetDefaults()
		{
			Item.width = 30;
			Item.height = 20;
			Item.value = 4000;
			Item.defense = 5;
			Item.rare = ItemRarityID.Orange;
		}

		public override void UpdateEquip(Player Player)
		{
			Player.GetModPlayer<CritMultiPlayer>().RangedCritMult += Math.Min(0.2f, Player.velocity.Length() / 16f * 0.2f);
		}

		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ItemType<SpaceEvent.Astroscrap>(), 15);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}
	}

	public class SpotterDrone : ModProjectile
	{
		public const int SCAN_TIME = 230;

		const float attackRange = 200;

		private NPC target;

		private Vector2 enemyOffset = Vector2.Zero;

		private int attackDelay;

		private List<float> rotations;
		private List<float> rotations2;

		private int batteryCharge = 0;
		private float batteryFade;

		public override string Texture => AssetDirectory.BreacherItem + Name;

		public ref float ScanTimer => ref Projectile.ai[0];

		public ref float Charges => ref Projectile.ai[1];

		public bool CanScan => ScanTimer <= 0;

		private float CurrentRotation => (TargetPos - Projectile.Center).ToRotation();
		private float CurrentRotation2 => (TargetPos2 - Projectile.Center).ToRotation();

		private Vector2 TargetPos => Vector2.Lerp(target.Bottom, target.Top, 0.5f + (float)Math.Cos((ScanTimer - 100) * 2 * 6.28f / (SCAN_TIME - 100)) / 2f);
		private Vector2 TargetPos2 => Vector2.Lerp(target.Top, target.Bottom, 0.5f + (float)Math.Cos((ScanTimer - 100) * 2 * 6.28f / (SCAN_TIME - 100)) / 2f);

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Breacher Drone");
			Projectile.netImportant = true;
			Main.projPet[Projectile.type] = true;
			Main.projFrames[Projectile.type] = 1;
			ProjectileID.Sets.TrailCacheLength[Projectile.type] = 1;
			ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
			ProjectileID.Sets.MinionSacrificable[Projectile.type] = false;
			ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true;
			ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true;
		}

		public override void SetDefaults()
		{
			Projectile.netImportant = true;
			Projectile.width = 20;
			Projectile.height = 20;
			Projectile.friendly = false;
			Projectile.DamageType = DamageClass.Ranged;
			Projectile.penetrate = -1;
			Projectile.timeLeft = 216000;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
		}

		public override void AI()
		{
			Player Player = Main.player[Projectile.owner];
			BatteryCharge(Player);

			if (Player.dead)
				Projectile.active = false;

			if (Player.GetModPlayer<BreacherPlayer>().SetBonusActive)
				Projectile.timeLeft = 2;

			if (ScanTimer <= 0)
			{
				if (Player.GetModPlayer<BreacherPlayer>().ticks < BreacherPlayer.CHARGETIME * 5)
					Player.GetModPlayer<BreacherPlayer>().ticks++;

				IdleMovement(Player);
				Vector2 direction = Player.Center - Projectile.Center;
				Projectile.rotation = direction.ToRotation() + 3.14f;
				target = null;
			}
			else
			{
				AttackBehavior(Player);
			}

			if (MathHelper.WrapAngle(Projectile.rotation) < -1.57f || MathHelper.WrapAngle(Projectile.rotation) > 1.57f)
			{
				Projectile.rotation -= 3.14f;
				Projectile.spriteDirection = -1;
			}
			else
			{
				Projectile.spriteDirection = 1;
			}
		}

		public override void Kill(int timeLeft)
		{
			if (target == null || !target.active)
				return;

			target.GetGlobalNPC<BreacherGNPC>().targetted = false;
		}

		public override void SendExtraAI(BinaryWriter writer)
		{
			if (target != null && target.active)
				writer.Write(target.whoAmI);
			else
				writer.Write(-1);
		}

		public override void ReceiveExtraAI(BinaryReader reader)
		{
			int targetIndex = reader.ReadInt32();
			if (targetIndex < 0)
				target = null;
			else
				target = Main.npc[targetIndex];
		}

		public override bool PreDraw(ref Color lightColor)
		{
			#region battery drawing

			Color scanColor = Color.Lerp(Color.Red, Color.Green, batteryCharge / 5f) * MathHelper.Min(batteryFade, 1);
			scanColor.A = 0;

			Texture2D tex = Request<Texture2D>(Texture + "_Display").Value;
			Vector2 position = Projectile.Center - Main.screenPosition;
			var origin = new Vector2(tex.Width / 2, tex.Height);
			Main.spriteBatch.Draw(tex, position, null, scanColor * 2, 0, origin, new Vector2(0.46f, 0.85f), SpriteEffects.None, 0);

			tex = Request<Texture2D>(Texture + "_Battery").Value;
			position = Projectile.Center - Main.screenPosition - new Vector2(0, 30);
			origin = tex.Size() / 2;

			Main.spriteBatch.Draw(tex, position, null, scanColor, 0, origin, 1, SpriteEffects.None, 0);

			tex = Request<Texture2D>(Texture + "_BatteryCharge").Value;
			var frame = new Rectangle(0, 0, 11 + 4 * batteryCharge, tex.Height);

			Main.spriteBatch.Draw(tex, position, frame, scanColor, 0, origin, 1, SpriteEffects.None, 0);

			#endregion

			if (ScanTimer == SCAN_TIME || ScanTimer <= 100 || rotations == null || rotations.Count < 2 || rotations2.Count < 2)
				return true;

			var color = Color.Lerp(new Color(255, 0, 0), Color.Red, 0.65f);
			color.A = 0;

			float oldRot = rotations[0];
			float currentRotation = rotations[Math.Max(rotations.Count - 1, 0)];
			float rotDifference = ((currentRotation - oldRot) % 6.28f + 9.42f) % 6.28f - 3.14f;

			Main.spriteBatch.Draw(TextureAssets.MagicPixel.Value, Projectile.Center - Main.screenPosition, new Rectangle(0, 0, 1, 1), color, currentRotation, Vector2.Zero, new Vector2((TargetPos - Projectile.Center).Length(), 2), SpriteEffects.None, 0);

			if (rotDifference > 0)
			{
				for (float k = 0; k < rotDifference; k += 0.02f * Math.Sign(rotDifference))
				{
					DrawLine(Main.spriteBatch, k, oldRot, rotDifference, TargetPos);
				}
			}
			else
			{
				for (float k = 0; k > rotDifference; k += 0.02f * Math.Sign(rotDifference))
				{
					DrawLine(Main.spriteBatch, k, oldRot, rotDifference, TargetPos);
				}
			}

			oldRot = rotations2[0];
			currentRotation = rotations2[Math.Max(rotations2.Count - 1, 0)];
			rotDifference = ((currentRotation - oldRot) % 6.28f + 9.42f) % 6.28f - 3.14f;

			Main.spriteBatch.Draw(TextureAssets.MagicPixel.Value, Projectile.Center - Main.screenPosition, new Rectangle(0, 0, 1, 1), color, currentRotation, Vector2.Zero, new Vector2((TargetPos2 - Projectile.Center).Length(), 2), SpriteEffects.None, 0);

			if (rotDifference > 0)
			{
				for (float k = 0; k < rotDifference; k += 0.01f * Math.Sign(rotDifference))
				{
					DrawLine(Main.spriteBatch, k, oldRot, rotDifference, TargetPos2);
				}
			}
			else
			{
				for (float k = 0; k > rotDifference; k += 0.01f * Math.Sign(rotDifference))
				{
					DrawLine(Main.spriteBatch, k, oldRot, rotDifference, TargetPos2);
				}
			}

			return true;
		}

		private void BatteryCharge(Player Player)
		{
			BreacherPlayer modPlayer = Player.GetModPlayer<BreacherPlayer>();

			if (modPlayer.Charges != batteryCharge)
			{
				if (modPlayer.Charges > batteryCharge)
					batteryFade = 2f;

				batteryCharge = modPlayer.Charges;
			}

			if (batteryFade > 0)
			{
				batteryFade -= 0.02f;
				Lighting.AddLight(Projectile.Center - new Vector2(0, 24), Color.Lerp(Color.Red, Color.Green, batteryCharge / 5f).ToVector3());
			}
		}

		private void DrawLine(SpriteBatch spriteBatch, float k, float oldRot, float rotDifference, Vector2 targetPosition)
		{
			float rot = k + oldRot;
			float lerper = Math.Abs(k / rotDifference);
			lerper *= lerper * lerper;
			var color = Color.Lerp(Color.Red, new Color(255, 0, 0), lerper * lerper / 2);
			color.A = 0;
			spriteBatch.Draw(TextureAssets.MagicPixel.Value, Projectile.Center - Main.screenPosition, new Rectangle(0, 0, 1, 1), color * lerper * 0.5f, rot, Vector2.Zero, new Vector2((targetPosition - Projectile.Center).Length(), 2), SpriteEffects.None, 0);
		}

		private void IdleMovement(Entity entity)
		{
			Vector2 toEntity = entity.Center - new Vector2((entity.width + 50) * entity.direction, entity.height + 25) - Projectile.Center;

			if (toEntity.Length() > 1000)
				Projectile.Center = entity.Center - new Vector2((entity.width + 50) * entity.direction, entity.height + 25);

			toEntity.Normalize();
			toEntity *= 10;
			Projectile.velocity = Vector2.Lerp(Projectile.velocity, toEntity, 0.02f);
		}

		private void AttackMovement(Entity entity)
		{
			Vector2 toEntity = entity.Center + enemyOffset - Projectile.Center;
			toEntity.Normalize();
			toEntity *= 15;
			Projectile.velocity = Vector2.Lerp(Projectile.velocity, toEntity, 0.06f);

			if (ScanTimer % 40 == 0)
			{
				enemyOffset = new Vector2(Main.rand.Next(-entity.width * 2, entity.width * 2), Main.rand.Next(-entity.height, 0));
				enemyOffset.X += Math.Sign(enemyOffset.X) * 50;
			}
		}

		private void AttackBehavior(Player Player)
		{
			if (target == null || !target.active)
			{
				ScanTimer = SCAN_TIME;
				NPC testtarget = Main.npc.Where(n => n.CanBeChasedBy(Projectile, false) && Vector2.Distance(n.Center, Projectile.Center) < 800).OrderBy(n => Vector2.Distance(n.Center, Main.MouseWorld)).FirstOrDefault();

				if (testtarget != default)
				{
					if (Vector2.Distance(testtarget.Center, Projectile.Center) < attackRange)
					{
						if (Main.myPlayer == Projectile.owner)
							Projectile.netUpdate = true;

						Helper.PlayPitched("Effects/Scan", 0.5f, 0);
						target = testtarget;
						ScanTimer--;
						rotations = new List<float>();
						rotations2 = new List<float>();
					}
					else
					{
						AttackMovement(testtarget);
					}
				}
				else
				{
					IdleMovement(Player);
				}

				return;
			}

			if (ScanTimer > Charges)
			{
				target.GetGlobalNPC<BreacherGNPC>().targetted = true;
				target.GetGlobalNPC<BreacherGNPC>().targetDuration = 10;
				BreacherArmorHelper.anyScanned = Main.npc.Any(n => n.GetGlobalNPC<BreacherGNPC>().targetted);

				if (rotations == null)
				{
					rotations = new List<float>();
					rotations2 = new List<float>();
				}

				rotations.Add(CurrentRotation);

				while (rotations.Count > 8)
				{
					rotations.RemoveAt(0);
				}

				rotations2.Add(CurrentRotation2);

				while (rotations2.Count > 8)
				{
					rotations2.RemoveAt(0);
				}

				if (ScanTimer < 150)
					CameraSystem.shake = (int)MathHelper.Lerp(0, 2, 1 - (float)ScanTimer / 150f);

				if (ScanTimer == 125)
					Helper.PlayPitched("AirstrikeIncoming", 0.6f, 0);

				ScanTimer--;
			}
			else
			{
				target.GetGlobalNPC<BreacherGNPC>().targetted = false;

				if (attackDelay == 0)
					SummonStrike();

				attackDelay--;
			}

			if (ScanTimer == 100)
				Helper.PlayPitched("Effects/ScanComplete", 0.5f, 0);

			if (ScanTimer > 100)
			{
				target.GetGlobalNPC<BreacherGNPC>().alpha = 1;
				AttackMovement(target);
				Vector2 direction = TargetPos - Projectile.Center;
				Projectile.rotation = direction.ToRotation() + 3.14f;
			}
			else
			{
				target.GetGlobalNPC<BreacherGNPC>().alpha = (float)ScanTimer / 100f;
				IdleMovement(Player);
				Vector2 direction = Player.Center - Projectile.Center;
				Projectile.rotation = direction.ToRotation() + 3.14f;
			}
		}

		private void SummonStrike()
		{
			attackDelay = 6;

			if (Projectile.owner == Main.myPlayer)
			{
				var direction = new Vector2(0, -1);
				direction = direction.RotatedBy(Main.rand.NextFloat(-0.3f, 0.3f));
				Projectile.NewProjectile(Projectile.GetSource_FromThis(), target.Center + direction * 800, direction * -10, ProjectileType<OrbitalStrike>(), Projectile.damage, Projectile.knockBack, Projectile.owner, target.whoAmI);
			}

			Charges--;
		}
	}

	internal class OrbitalStrike : ModProjectile, IDrawPrimitive
	{
		private List<Vector2> cache;

		private Trail trail;
		private Trail trail2;

		private bool hit = false;

		private NPC Target => Main.npc[(int)Projectile.ai[0]];

		private float Alpha => hit ? (Projectile.timeLeft / 50f) : 1;

		public override string Texture => AssetDirectory.BreacherItem + Name;

		public override void SetDefaults()
		{
			Projectile.width = 80;
			Projectile.height = 20;
			Projectile.DamageType = DamageClass.Ranged;
			Projectile.friendly = true;
			Projectile.tileCollide = false;
			Projectile.penetrate = 1;
			Projectile.timeLeft = 300;
			Projectile.extraUpdates = 4;
			Projectile.scale = 0.6f;
		}

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Orbital Strike");
			Main.projFrames[Projectile.type] = 2;
			ProjectileID.Sets.TrailCacheLength[Projectile.type] = 30;
			ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
		}

		private void findIfHit()
		{
			//for other Players to determine if this has hit
			foreach (NPC NPC in Main.npc.Where(n => n.active && !n.dontTakeDamage && !n.townNPC && n.life > 0 && n.immune[Projectile.owner] <= 0 && n.Hitbox.Intersects(Projectile.Hitbox)))
			{
				OnHitNPC(NPC, 0, 0f, false);
			}
		}

		public override void AI()
		{
			if (!hit)
			{
				Vector2 direction = Target.Center - Projectile.Center;
				direction.Normalize();
				direction *= 10;

				if (direction.Y > 0)
					Projectile.velocity = direction;

				if (Main.netMode != NetmodeID.Server)
					ManageCaches();
			}
			else if (Main.myPlayer != Projectile.owner)
			{
				findIfHit();
			}

			if (Main.netMode != NetmodeID.Server)
				ManageTrail();
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D tex = TextureAssets.Projectile[Projectile.type].Value;

			Color color = Color.Cyan;
			color.A = 0;

			Color color2 = Color.White;
			color2.A = 0;

			Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null,
							 color * Alpha * 0.33f, Projectile.rotation, tex.Size() / 2, Projectile.scale * 2, SpriteEffects.None, 0);

			Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null,
							 color * Alpha, Projectile.rotation, tex.Size() / 2, Projectile.scale, SpriteEffects.None, 0);

			Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null,
							 color2 * Alpha, Projectile.rotation, tex.Size() / 2, Projectile.scale * 0.75f, SpriteEffects.None, 0);

			return false;
		}

		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
		{
			if (Main.myPlayer == Projectile.owner)
				CameraSystem.shake += 9;

			Projectile.friendly = false;
			Projectile.penetrate++;
			hit = true;
			Projectile.timeLeft = 50;
			Projectile.extraUpdates = 3;
			Projectile.velocity = Vector2.Zero;

			Explode(target);
		}

		private void ManageCaches()
		{
			if (cache == null)
			{
				cache = new List<Vector2>();

				for (int i = 0; i < 100; i++)
				{
					cache.Add(Projectile.Center);
				}
			}

			if (Projectile.oldPos[0] != Vector2.Zero)
				cache.Add(Projectile.oldPos[0] + new Vector2(Projectile.width / 2, Projectile.height / 2));

			while (cache.Count > 100)
			{
				cache.RemoveAt(0);
			}
		}

		private void ManageTrail()
		{
			trail ??= new Trail(Main.instance.GraphicsDevice, 100, new TriangularTip(16), factor => factor * MathHelper.Lerp(11, 22, factor), factor => Color.Cyan);
			trail2 ??= new Trail(Main.instance.GraphicsDevice, 100, new TriangularTip(16), factor => factor * MathHelper.Lerp(6, 12, factor), factor => Color.White);

			trail.Positions = cache.ToArray();
			trail.NextPosition = Projectile.Center;

			trail2.Positions = cache.ToArray();
			trail2.NextPosition = Projectile.Center;
		}

		public void DrawPrimitives()
		{
			Effect effect = Filters.Scene["OrbitalStrikeTrail"].GetShader().Shader;

			var world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
			Matrix view = Main.GameViewMatrix.ZoomMatrix;
			var projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

			effect.Parameters["transformMatrix"].SetValue(world * view * projection);
			effect.Parameters["sampleTexture"].SetValue(Request<Texture2D>("StarlightRiver/Assets/GlowTrail").Value);
			effect.Parameters["alpha"].SetValue(Alpha);

			trail?.Render(effect);
			trail2?.Render(effect);
		}

		private void Explode(NPC target)
		{
			Helper.PlayPitched("Impacts/AirstrikeImpact", 0.4f, Main.rand.NextFloat(-0.1f, 0.1f));

			for (int i = 0; i < 5; i++)
			{
				Dust.NewDustPerfect(Projectile.Center + new Vector2(20, 70), DustType<BreacherDustLine>(), Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(12, 26), 0, new Color(48, 242, 96), Main.rand.NextFloat(0.7f, 0.9f));
				Dust.NewDustPerfect(Projectile.Center, DustType<BreacherDustStar>(), Main.rand.NextFloat(6.28f).ToRotationVector2() * Main.rand.NextFloat(8), 0, new Color(48, 242, 96), Main.rand.NextFloat(0.1f, 0.2f));
			}

			Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ProjectileType<OrbitalStrikeRing>(), Projectile.damage, Projectile.knockBack, Projectile.owner, target.whoAmI);
		}
	}

	internal class OrbitalStrikeRing : ModProjectile, IDrawPrimitive
	{
		private List<Vector2> cache;

		private Trail trail;
		private Trail trail2;

		private float Progress => 1 - Projectile.timeLeft / 10f;

		private float Radius => 66 * (float)Math.Sqrt(Math.Sqrt(Progress));

		public override string Texture => AssetDirectory.BreacherItem + "OrbitalStrike";

		public override void SetDefaults()
		{
			Projectile.width = 80;
			Projectile.height = 80;
			Projectile.DamageType = DamageClass.Ranged;
			Projectile.friendly = true;
			Projectile.tileCollide = false;
			Projectile.penetrate = -1;
			Projectile.timeLeft = 10;
		}

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Orbital Strike");
		}

		public override void AI()
		{
			if (Main.netMode != NetmodeID.Server)
			{
				ManageCaches();
				ManageTrail();
			}
		}

		public override bool PreDraw(ref Color lightColor)
		{
			return false;
		}

		public override bool? CanHitNPC(NPC target)
		{
			if (target.whoAmI == (int)Projectile.ai[0])
				return false;

			return base.CanHitNPC(target);
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			Vector2 line = targetHitbox.Center.ToVector2() - Projectile.Center;
			line.Normalize();
			line *= Radius;

			if (Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center, Projectile.Center + line))
				return true;

			return false;
		}

		private void ManageCaches()
		{
			cache = new List<Vector2>();
			float radius = Radius;

			for (int i = 0; i < 33; i++) //TODO: Cache offsets, to improve performance
			{
				double rad = i / 32f * 6.28f;
				var offset = new Vector2((float)Math.Sin(rad), (float)Math.Cos(rad));
				offset *= radius;
				cache.Add(Projectile.Center + offset);
			}

			while (cache.Count > 33)
			{
				cache.RemoveAt(0);
			}
		}

		private void ManageTrail()
		{
			trail ??= new Trail(Main.instance.GraphicsDevice, 33, new TriangularTip(1), factor => 38 * (1 - Progress), factor => Color.Cyan);
			trail2 ??= new Trail(Main.instance.GraphicsDevice, 33, new TriangularTip(1), factor => 20 * (1 - Progress), factor => Color.White);

			float nextplace = 33f / 32f;
			var offset = new Vector2((float)Math.Sin(nextplace), (float)Math.Cos(nextplace));
			offset *= Radius;

			trail.Positions = cache.ToArray();
			trail.NextPosition = Projectile.Center + offset;

			trail2.Positions = cache.ToArray();
			trail2.NextPosition = Projectile.Center + offset;
		}

		public void DrawPrimitives()
		{
			Effect effect = Filters.Scene["OrbitalStrikeTrail"].GetShader().Shader;

			var world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
			Matrix view = Main.GameViewMatrix.ZoomMatrix;
			var projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

			effect.Parameters["transformMatrix"].SetValue(world * view * projection);
			effect.Parameters["sampleTexture"].SetValue(Request<Texture2D>("StarlightRiver/Assets/GlowTrail").Value);
			effect.Parameters["alpha"].SetValue(1);

			trail?.Render(effect);
			trail2?.Render(effect);
		}
	}

	public class BreacherGNPC : GlobalNPC
	{
		public bool targetted = false;
		public int targetDuration = 0;

		public float alpha;

		public override bool InstancePerEntity => true;

		public override void ResetEffects(NPC NPC)
		{
			targetDuration--;

			if (targetted && targetDuration < 0)
			{
				targetted = false;
				BreacherArmorHelper.anyScanned = Main.npc.Any(n => n.active && n.GetGlobalNPC<BreacherGNPC>().targetted);
			}
		}
	}

	public class BreacherPlayer : ModPlayer
	{
		public const int CHARGETIME = 150;

		public int ticks;

		public int Charges => ticks / CHARGETIME;

		public bool SetBonusActive => Player.armor[0].type == ItemType<BreacherHead>() && Player.armor[1].type == ItemType<BreacherChest>() && Player.armor[2].type == ItemType<BreacherLegs>();

		public override void OnHitNPCWithProj(Projectile proj, NPC target, int damage, float knockback, bool crit)
		{
			if (target.life <= 0 && ticks < CHARGETIME * 5)
				ticks += CHARGETIME / 3;
		}

		public override void OnHitNPC(Item Item, NPC target, int damage, float knockback, bool crit)
		{
			if (target.life <= 0 && ticks < CHARGETIME * 5)
				ticks += CHARGETIME / 3;
		}
	}

	public class BreacherArmorHelper : IOrderedLoadable
	{
		public static ScreenTarget NPCTarget = new(DrawTargeted, () => anyScanned, 1);

		public static bool anyScanned;

		private static float alpha = 1f;

		public float Priority => 1.1f;

		public void Load()
		{
			if (Main.dedServ)
				return;

			Terraria.On_Main.DrawNPCs += DrawBreacherOverlay;
		}

		public void Unload() { }

		private void DrawBreacherOverlay(Terraria.On_Main.orig_DrawNPCs orig, Main self, bool behindTiles)
		{
			orig(self, behindTiles);

			if (anyScanned)
				DrawNPCTarget();
		}

		private static void DrawTargeted(SpriteBatch spriteBatch)
		{
			Main.graphics.GraphicsDevice.Clear(Color.Transparent);

			spriteBatch.End();
			spriteBatch.Begin(default, default, default, default, default, null, Main.GameViewMatrix.ZoomMatrix);

			for (int i = 0; i < Main.npc.Length; i++)
			{
				NPC NPC = Main.npc[i];

				if (NPC.active && NPC.GetGlobalNPC<BreacherGNPC>().targetted)
				{
					alpha = NPC.GetGlobalNPC<BreacherGNPC>().alpha;

					if (NPC.ModNPC != null)
					{
						if (NPC.ModNPC != null && NPC.ModNPC is ModNPC ModNPC)
						{
							if (ModNPC.PreDraw(spriteBatch, Main.screenPosition, NPC.GetAlpha(Color.White)))
								Main.instance.DrawNPC(i, false);

							ModNPC.PostDraw(spriteBatch, Main.screenPosition, NPC.GetAlpha(Color.White));
						}
					}
					else
					{
						Main.instance.DrawNPC(i, false);
					}
				}
			}

			spriteBatch.End();
			spriteBatch.Begin();
		}

		private static void DrawNPCTarget()
		{
			GraphicsDevice gD = Main.graphics.GraphicsDevice;
			SpriteBatch spriteBatch = Main.spriteBatch;

			if (Main.dedServ || spriteBatch == null || NPCTarget == null || gD == null)
				return;

			spriteBatch.End();
			spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, null);

			Effect effect = Filters.Scene["BreacherScan"].GetShader().Shader;
			effect.Parameters["uImageSize0"].SetValue(new Vector2(Main.screenWidth, Main.screenHeight));
			effect.Parameters["alpha"].SetValue(alpha);
			effect.Parameters["red"].SetValue(new Color(1, 0.1f, 0.1f, 1).ToVector4());
			effect.Parameters["red2"].SetValue(new Color(1, 0.1f, 0.1f, 0.9f).ToVector4());

			float flickerTime = 100 - alpha * 100;

			if (flickerTime > 0 && flickerTime < 16)
			{
				float flickerTime2 = (float)(flickerTime / 20f);
				float whiteness = 1.5f - (flickerTime2 * flickerTime2 / 2 + 2f * flickerTime2);
				effect.Parameters["whiteness"].SetValue(whiteness);
			}
			else
			{
				effect.Parameters["whiteness"].SetValue(0);
			}

			effect.CurrentTechnique.Passes[0].Apply();
			spriteBatch.Draw(NPCTarget.RenderTarget, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.White);

			spriteBatch.End();
			spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.TransformationMatrix);
		}
	}
}