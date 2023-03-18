using StarlightRiver.Content.Buffs;
using StarlightRiver.Content.CustomHooks;
using StarlightRiver.Core.Systems.CameraSystem;
using StarlightRiver.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.DataStructures;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Misc
{
	public class RadculasRapier : ModItem
	{
		public override string Texture => AssetDirectory.MiscItem + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Radcula's Rapier");
			Tooltip.SetDefault("Rapidly stabs enemies, inflicting a deadly bleed\n" +
				"Press <right> to teleport to the cursor, slashing all those in the way\n" +
				"The teleport deals increased damage and heals the player depending on the potency of the bleed on the struck enemy\n" +
				"The more enemies you strike with the teleport, the lower the cooldown");
		}

		public override void SetDefaults()
		{
			Item.damage = 30;
			Item.DamageType = DamageClass.Melee;

			Item.useTime = 30;
			Item.useAnimation = 30; 

			Item.autoReuse = true;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.knockBack = 3.5f;
			Item.shootSpeed = 5f;

			Item.shoot = ModContent.ProjectileType<RadculasRapierSwungBlade>();
			Item.noUseGraphic = true;
			Item.noMelee = true;

			Item.value = Item.sellPrice(gold: 5);
			Item.rare = ItemRarityID.Orange;

			Item.channel = true;
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			if (player.altFunctionUse == 2 && !player.HasBuff<RadculasRapierCooldown>())
				Projectile.NewProjectile(source, position, velocity, ModContent.ProjectileType<RadculasRapierTeleport>(), damage, knockback, player.whoAmI);
			else
				Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);

			return false;
		}

		public override bool CanUseItem(Player player)
		{
			if (player.HasBuff<RadculasRapierCooldown>() && player.altFunctionUse == 2)
				return false;

			return player.ownedProjectileCounts[ModContent.ProjectileType<RadculasRapierSwungBlade>()] <= 0;
		}

		public override bool AltFunctionUse(Player player)
		{
			return true;
		}
	}

	class RadculasRapierPlayer : ModPlayer //required for the dash/teleport
	{

		public List<Vector2> trailPositions = new();

		public int teleportTimer;

		public override void Load()
		{
			StarlightPlayer.PostDrawEvent += StarlightPlayer_PostDrawEvent;
			StarlightPlayer.PreDrawEvent += StarlightPlayer_PreDrawEvent;
		}

		private void StarlightPlayer_PostDrawEvent(Player player, SpriteBatch spriteBatch)
		{
			if (PlayerTarget.canUseTarget)
			{
				spriteBatch.End();
				spriteBatch.Begin(default, blendState: BlendState.Additive, default, default, default, default, Main.GameViewMatrix.TransformationMatrix);

				bool active = player.active && !player.outOfRange && !player.dead;
				if (active && player.GetModPlayer<RadculasRapierPlayer>().teleportTimer > 0)
				{
					Main.spriteBatch.Draw(PlayerTarget.Target, PlayerTarget.getPlayerTargetPosition(player.whoAmI),
								 PlayerTarget.getPlayerTargetSourceRectangle(player.whoAmI), new Color(150, 0, 0) * (player.GetModPlayer<RadculasRapierPlayer>().teleportTimer / 60f), player.fullRotation, Vector2.Zero, 1f, 0f, 0f);

					Texture2D bloomTex = ModContent.Request<Texture2D>(AssetDirectory.Keys + "GlowAlpha").Value;
					Main.spriteBatch.Draw(bloomTex, player.Center - Main.screenPosition, null, new Color(255, 0, 0) * (player.GetModPlayer<RadculasRapierPlayer>().teleportTimer / 60f), 0f, bloomTex.Size() / 2f, 2f, 0f, 0f);
				}

				spriteBatch.End();
				spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.TransformationMatrix);
			}
		}

		private void StarlightPlayer_PreDrawEvent(Player player, SpriteBatch spriteBatch)
		{
			if (PlayerTarget.canUseTarget)
			{
				spriteBatch.End();
				spriteBatch.Begin(default, blendState: BlendState.Additive, default, default, default, default, Main.GameViewMatrix.TransformationMatrix);

				bool active = player.active && !player.outOfRange && !player.dead;
				if (active && player.GetModPlayer<RadculasRapierPlayer>().trailPositions.Count > 0)
				{
					for (int x = player.GetModPlayer<RadculasRapierPlayer>().trailPositions.Count - 1; x > 0; x--)
					{
						Main.spriteBatch.Draw(PlayerTarget.Target, player.GetModPlayer<RadculasRapierPlayer>().trailPositions[x] - Main.screenPosition,
								 PlayerTarget.getPlayerTargetSourceRectangle(player.whoAmI), new Color(100, 0, 0) * (player.GetModPlayer<RadculasRapierPlayer>().teleportTimer / 60f), player.fullRotation, Vector2.Zero, 1f, 0f, 0f);
					}
				}

				spriteBatch.End();
				spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.TransformationMatrix);
			}
		}

		public override void ResetEffects()
		{
			if (teleportTimer > 0)
			{
				teleportTimer--;
				if (teleportTimer == 1)
				{
					Player.AddBuff(BuffID.ParryDamageBuff, 240);
					for (int i = 0; i < 20; i++)
					{
						Dust.NewDustPerfect(Player.Center + Main.rand.NextVector2Circular(Player.width, Player.height), ModContent.DustType<Dusts.GlowFastDecelerate>(), Vector2.UnitY * -Main.rand.NextFloat(1f, 2.5f), 0, new Color(255, 0, 0, 50), Main.rand.NextFloat(0.4f, 0.6f));
					}
				}
			}
			else
				trailPositions.Clear();
		}


		public override void PreUpdateMovement()
		{
			if (teleportTimer > 0)
			{
				trailPositions.Add(PlayerTarget.getPlayerTargetPosition(Player.whoAmI) + Main.screenPosition);

				if (Main.rand.NextBool(5))
				{
					Dust.NewDustPerfect(Player.Center + Main.rand.NextVector2Circular(Player.width, Player.height), ModContent.DustType<Dusts.GlowFastDecelerate>(), Player.velocity * 0.25f, 0, new Color(255, 0, 0, 100), 0.5f);
				}
			}

			if (trailPositions.Count > 30 || (trailPositions.Count > 0 && teleportTimer == 0))
				trailPositions.RemoveAt(0);
		}
	}

	class RadculasRapierNPC : GlobalNPC //TODO: transfer this to stackable buffs
	{
		public override bool InstancePerEntity => true;

		public int duration;

		public Vector2 lastHitPos;

		public bool inflicted => duration > 0;

		public override void ResetEffects(NPC npc)
		{
			duration = Utils.Clamp(--duration, 0, 600);
		}

		public override void AI(NPC npc)
		{
			if (inflicted)
			{
				if (Main.rand.NextBool())
					Dust.NewDustPerfect(npc.Center + npc.DirectionTo(lastHitPos) * (npc.width / 2), DustID.Blood, npc.DirectionTo(lastHitPos).RotatedByRandom(0.35f) * Main.rand.NextFloat(1f, 5f), 0, default, 1.25f);

				if (Main.rand.NextBool(3))
					Dust.NewDustPerfect(npc.Center + npc.DirectionTo(lastHitPos) * (npc.width / 2), ModContent.DustType<Dusts.GraveBlood>(), npc.DirectionTo(lastHitPos).RotatedByRandom(0.35f) * Main.rand.NextFloat(1f, 5f), 0, default, 1.25f);
			}
		}

		public override void UpdateLifeRegen(NPC npc, ref int damage)
		{
			if (inflicted)
			{
				if (npc.lifeRegen > 0)
					npc.lifeRegen = 0;

				npc.lifeRegen -= 20;

				damage = 2;
			}
		}
	}

	class RadculasRapierSwungBlade : ModProjectile
	{
		public struct afterImageStruct
		{
			public Vector2 pos;
			public float rot;
			public int time;

			public afterImageStruct(Vector2 pos, float rot, int time)
			{
				this.pos = pos;
				this.rot = rot;
				this.time = time;
			}
		}

		public int maxTimeLeft;

		public int amountStabs;

		public bool fading;

		public bool playedSound;

		public Vector2 offset;

		public Vector2 stabVec;

		public List<NPC> hitNPCs = new();

		public List<afterImageStruct> afterImages = new(); // position of afterimage, rotation of afterimage, timer for afterimage

		public Player.CompositeArmStretchAmount stretch = Player.CompositeArmStretchAmount.Full;

		public override string Texture => AssetDirectory.MiscItem + "RadculasRapier_Spear";

		public Player Owner => Main.player[Projectile.owner];

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Radculas Rapier");
			ProjectileID.Sets.TrailCacheLength[Type] = 10;
			ProjectileID.Sets.TrailingMode[Type] = 2;
		}

		public override void SetDefaults()
		{
			Projectile.friendly = true;
			Projectile.hostile = false;

			Projectile.DamageType = DamageClass.Melee;
			Projectile.tileCollide = false;

			Projectile.Size = new Vector2(16);

			Projectile.penetrate = -1;
			Projectile.ownerHitCheck = true;

			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 1;
		}

		public override void OnSpawn(IEntitySource source)
		{
			Projectile.timeLeft = (int)(Owner.HeldItem.useAnimation * (1f / Owner.GetTotalAttackSpeed(DamageClass.Melee)));
			maxTimeLeft = Projectile.timeLeft;
		}

		public override void AI()
		{
			Owner.heldProj = Projectile.whoAmI;
			Owner.itemTime = 2;

			Projectile.velocity = Vector2.Normalize(Projectile.velocity);

			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

			int stabTimer = maxTimeLeft - Projectile.timeLeft;

			if (!fading)
			{
				float totalTime = maxTimeLeft * 0.2f; //total amount of frames the stab lasts

				if (stabTimer < totalTime)
				{
					if (stabTimer == 1) //things are initialized here each stab, such as the random rotation, and the randomness of the length of the stab
					{
						Projectile.velocity = Owner.DirectionTo(Main.MouseWorld).RotatedByRandom(0.35f);
						stabVec = new Vector2(Main.rand.NextFloat(90, 150), 0);
						hitNPCs.Clear();
					}
					if (stabTimer < totalTime * 0.5f)
					{
						float lerper = stabTimer / (totalTime * 0.5f);

						offset = Vector2.Lerp(new Vector2(75, 0), stabVec, EaseBuilder.EaseCircularOut.Ease(lerper)).RotatedBy(Projectile.rotation - MathHelper.PiOver2);
					}
					else
					{
						if (!playedSound)
						{
							afterImages.Add(new afterImageStruct(Projectile.Center, Projectile.rotation, 15));
							Helper.PlayPitched("Magic/ShurikenThrow", 1f, Main.rand.NextFloat(-0.3f, 0.3f), Owner.Center);
							playedSound = true;
						}

						float lerper = (stabTimer - totalTime * 0.5f) / (float)(totalTime * 0.5f);

						offset = Vector2.Lerp(stabVec, new Vector2(75, 0), EaseBuilder.EaseCircularOut.Ease(lerper)).RotatedBy(Projectile.rotation - MathHelper.PiOver2);
					}

				}
				else
				{
					playedSound = false;
					amountStabs++;
					Projectile.timeLeft = maxTimeLeft;

					stretch = (Player.CompositeArmStretchAmount)Main.rand.Next(4);
				}

				if (Main.rand.NextBool(5))
					Dust.NewDustPerfect(Vector2.Lerp(Owner.Center, Projectile.Center, Main.rand.NextFloat()), ModContent.DustType<Dusts.GlowFastDecelerate>(), Vector2.Zero, 0, new Color(255, 0, 0, 100), 0.45f);

				if (Main.rand.NextBool(5))
				{
					Vector2 pos = Vector2.Lerp(Owner.Center, Projectile.Center, Main.rand.NextFloat());
					Dust.NewDustPerfect(pos, ModContent.DustType<Dusts.GlowFastDecelerate>(), pos.DirectionTo(Main.MouseWorld).RotatedByRandom(0.35f) * Main.rand.NextFloat(2f, 5f), 0, new Color(255, 0, 0, 100), 0.45f);
				}

				if (Main.rand.NextBool(5))
				{
					Vector2 pos = Vector2.Lerp(Owner.Center, Projectile.Center, Main.rand.NextFloat());
					Dust.NewDustPerfect(pos, ModContent.DustType<Dusts.GlowLineFast>(), pos.DirectionTo(Main.MouseWorld).RotatedByRandom(0.35f) * Main.rand.NextFloat(2f, 5f), 0, new Color(255, 0, Main.rand.Next(255), 100), 1.25f);
				}

				if (!Owner.channel && amountStabs > 5 && Projectile.timeLeft == maxTimeLeft)
				{
					Projectile.timeLeft = 5;
					fading = true;
				}
			}

			for (int i = 0; i < afterImages.Count; i++)
			{
				afterImageStruct afterImage = afterImages[i];
				afterImages[i] = new afterImageStruct(afterImage.pos, afterImage.rot, afterImage.time - 1);

				if (afterImages[i].time <= 0)
					afterImages.RemoveAt(i);
			}


			Projectile.Center = Owner.MountedCenter + offset;

			Owner.SetCompositeArmFront(true, stretch, Projectile.rotation - MathHelper.Pi);
			Owner.ChangeDir(Projectile.direction);
		}

		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
		{
			if (NPCID.Sets.ProjectileNPC[target.type])
				return;

			hitNPCs.Add(target);
		    CameraSystem.shake += 1;

			target.GetGlobalNPC<RadculasRapierNPC>().duration += 30;
			target.GetGlobalNPC<RadculasRapierNPC>().lastHitPos = Owner.Center;

			Vector2 pos = Owner.Center - (Owner.Center - target.Center);

			for (int i = 0; i < 5; i++)
			{
				Dust.NewDustPerfect(pos, ModContent.DustType<Dusts.GlowFastDecelerate>(), Projectile.Center.DirectionTo(Owner.Center).RotatedByRandom(0.35f) * Main.rand.NextFloat(0.5f, 2f), 0, new Color(150, 0, 0, 100), 0.55f);
			}

			if (Helper.IsFleshy(target))
			{
				Helper.PlayPitched("Impale", 1, Main.rand.NextFloat(0.6f, 0.9f), Projectile.Center);

				for (int k = 0; k < 5; k++)
				{
					Dust.NewDustPerfect(pos, DustID.Blood, Projectile.Center.DirectionTo(Owner.Center).RotatedByRandom(0.35f) * Main.rand.NextFloat(2f, 8f), 0, default, 1.5f);

					Dust.NewDustPerfect(pos, ModContent.DustType<Dusts.GraveBlood>(), Projectile.Center.DirectionTo(Owner.Center).RotatedByRandom(0.35f) * Main.rand.NextFloat(2f, 8f), 0, default, 1.5f);
				}
			}
			else
			{
				Helper.PlayPitched("Impacts/Clink", 1, Main.rand.NextFloat(0.1f, 0.3f), Projectile.Center);

				for (int k = 0; k < 5; k++)
				{
					Dust.NewDustPerfect(pos, ModContent.DustType<Dusts.Glow>(), Projectile.Center.DirectionTo(Owner.Center).RotatedByRandom(0.35f) * Main.rand.NextFloat(2f, 8f), 0, new Color(255, Main.rand.Next(130, 255), 80), Main.rand.NextFloat(0.3f, 0.5f));
				}
			}
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			bool original = projHitbox.Intersects(targetHitbox);
			bool line = Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Owner.Center, Projectile.Center);
			return original || line;
		}

		public override bool? CanHitNPC(NPC target)
		{
			return Projectile.friendly && !hitNPCs.Contains(target);
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
			Texture2D texGlow = ModContent.Request<Texture2D>(Texture + "_Glow").Value;

			Vector2 off = new Vector2(0, -20).RotatedBy(Projectile.rotation - MathHelper.PiOver2);

			float fade = 1f;
			if (Projectile.timeLeft <= 5 && fading)
				fade = Projectile.timeLeft / 5f;

			Color color = new Color(100, 0, 0, 0) * fade;
			Effect effect = Terraria.Graphics.Effects.Filters.Scene["AlphaDistort"].GetShader().Shader;
			effect.Parameters["uTime"].SetValue((float)Main.timeForVisualEffects * 0.1f);
			effect.Parameters["power"].SetValue(0.2f);
			effect.Parameters["offset"].SetValue(new Vector2(Main.screenPosition.X / Main.screenWidth * 0.5f, 0));
			effect.Parameters["speed"].SetValue(10f);
			for (int i = 0; i < afterImages.Count; i++) //idk how performance intensive reapplying this shader is
			{
				afterImageStruct afterImage = afterImages[i];

				float opacity = MathHelper.Lerp(0.5f, 0f, 1f - afterImage.time / 15f);

				effect.Parameters["opacity"].SetValue(opacity * fade);
				effect.Parameters["drawColor"].SetValue(Color.Lerp(new Color(50, 0, 150, 0), new Color(200, 0, 0, 0), 1f - afterImage.time / 15f).ToVector4());
				effect.CurrentTechnique.Passes[0].Apply();

				Main.spriteBatch.Draw(texGlow, afterImage.pos - Main.screenPosition + new Vector2(0, Main.player[Projectile.owner].gfxOffY) + new Vector2(-55, 0).RotatedBy(afterImage.rot - MathHelper.PiOver2), null, color * opacity, afterImage.rot, texGlow.Size() / 2f, Projectile.scale, 0f, 0f);

				Main.spriteBatch.Draw(texGlow, afterImage.pos - Main.screenPosition + new Vector2(0, Main.player[Projectile.owner].gfxOffY) + new Vector2(-55, 0).RotatedBy(Projectile.rotation - MathHelper.PiOver2), null, color * opacity, afterImage.rot, texGlow.Size() / 2f, Projectile.scale, 0f, 0f);

				effect.Parameters["drawColor"].SetValue((new Color(255, 255, 255, 0) * 0.25f).ToVector4());
				effect.CurrentTechnique.Passes[0].Apply();

				Main.spriteBatch.Draw(tex, afterImage.pos - Main.screenPosition + new Vector2(0, Main.player[Projectile.owner].gfxOffY) + off, null, Color.White * 0.25f * opacity, afterImage.rot, Vector2.Zero, Projectile.scale, 0f, 0f);
			}

			Main.spriteBatch.End();
			Main.spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.ZoomMatrix); //also dont know if this spritebatch reset is needed

			Main.spriteBatch.Draw(texGlow, Projectile.Center - Main.screenPosition + new Vector2(0, Main.player[Projectile.owner].gfxOffY) + new Vector2(-55, 0).RotatedBy(Projectile.rotation - MathHelper.PiOver2), null, color, Projectile.rotation, texGlow.Size() / 2f, Projectile.scale, 0, 0);
			Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition + new Vector2(0, Main.player[Projectile.owner].gfxOffY) + off, null, Color.White * fade, Projectile.rotation, Vector2.Zero, Projectile.scale, 0, 0);

			return false;
		}
	}

	class RadculasRapierTeleport : ModProjectile
	{
		public Vector2 originalPos;

		public Vector2 offset;

		public float originalRot;

		public bool teleported;

		public List<NPC> hitNPCs = new();

		public List<Vector2> teleportPositions = new();

		public Player Owner => Main.player[Projectile.owner];

		public override string Texture => AssetDirectory.MiscItem + "RadculasRapier_Spear";

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Radculas Rapier");
		}

		public override void SetDefaults()
		{
			Projectile.friendly = true;
			Projectile.hostile = false;

			Projectile.DamageType = DamageClass.Melee;
			Projectile.tileCollide = false;

			Projectile.width = 40;
			Projectile.height = 110;

			Projectile.penetrate = -1;

			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 1;

			Projectile.timeLeft = 30;
		}

		public override void AI()
		{
			Projectile.Center = Owner.MountedCenter;

			Owner.heldProj = Projectile.whoAmI;
			Owner.itemTime = 2;

			Projectile.velocity = Vector2.Normalize(Projectile.velocity);

			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

			if (Projectile.timeLeft < 25 && Projectile.timeLeft > 15)
			{
				float progress = EaseBuilder.EaseQuinticInOut.Ease(1f - (Projectile.timeLeft - 15) / 10f);
				Projectile.rotation = MathHelper.Lerp(0, 3.5f * Projectile.direction, progress);
			}
			else if (Projectile.timeLeft <= 15)
			{
				float progress = 1f - Projectile.timeLeft / 15f;
				Projectile.rotation = MathHelper.Lerp(3.5f * Projectile.direction, 3.65f * Projectile.direction, progress);

				if (Projectile.timeLeft > 5f)
				{
					progress = 1f - (Projectile.timeLeft - 5f) / 10f;
					offset = Vector2.Lerp(new Vector2(0, 0), new Vector2(-30, 0), EaseBuilder.EaseQuinticOut.Ease(progress)).RotatedBy(Projectile.rotation - MathHelper.PiOver2);
				}
				else
				{
					progress = 1f - Projectile.timeLeft / 5f;
					offset = Vector2.Lerp(new Vector2(-30, 0), new Vector2(10, 0), EaseBuilder.EaseQuinticInOut.Ease(progress)).RotatedBy(Projectile.rotation - MathHelper.PiOver2);
				}
			}

			Projectile.Center = Owner.MountedCenter + new Vector2(20f, 0).RotatedBy(Projectile.rotation - MathHelper.PiOver2) + offset;

			Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation - MathHelper.Pi);
			Owner.ChangeDir(Projectile.direction);

			Vector2 pos = Vector2.Lerp(Owner.MountedCenter + new Vector2(-20f, 0).RotatedBy(Projectile.rotation - MathHelper.PiOver2) + offset, Owner.MountedCenter + new Vector2(120f, 0).RotatedBy(Projectile.rotation - MathHelper.PiOver2) + offset, Projectile.timeLeft / 30f);
			for (int i = 0; i < 2; i++)
			{
				Dust.NewDustPerfect(pos + Main.rand.NextVector2Circular(1f, 1f), ModContent.DustType<Dusts.GlowFastDecelerate>(), Main.rand.NextVector2Circular(2f, 2f), 0, new Color(200, 0, 0, 100), 0.35f);
			}
		}

		public override void OnSpawn(IEntitySource source)
		{
			var mp = Owner.GetModPlayer<RadculasRapierPlayer>();

			originalPos = Owner.Center;

			Projectile.velocity = Vector2.Normalize(Projectile.velocity);

			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

			originalRot = Projectile.rotation;

			Vector2 teleportPos = Owner.Center + Owner.DirectionTo(Main.MouseWorld) * 250f;

			Tile tile = Main.tile[(int)teleportPos.X / 16, (int)teleportPos.Y / 16];

			if (tile.HasTile && Main.tileSolid[tile.TileType] && !TileID.Sets.Platforms[tile.TileType])
			{
				Vector2 safePos = teleportPos;

				//cant assign the unsafe tile to a variable since it needs to be updated every time the loop is ran
				while (Main.tile[(int)safePos.X / 16, (int)safePos.Y / 16].HasTile && Main.tileSolid[Main.tile[(int)safePos.X / 16, (int)safePos.Y / 16].TileType] && !TileID.Sets.Platforms[Main.tile[(int)safePos.X / 16, (int)safePos.Y / 16].TileType])
				{
					safePos.Y -= 16;
					if (teleportPos.Y - safePos.Y > 500)
						break;
				}

				if (Vector2.Distance(teleportPos, safePos) < 250f)
				{
					teleportPos = safePos -= new Vector2(0f, Owner.height);
				}
				else
				{
					Projectile.Kill();
					return;
				}
			}

			Owner.Teleport(teleportPos, -1);
			tile = Main.tile[(int)Owner.Bottom.X / 16, (int)Owner.Bottom.Y / 16];
			if (tile.HasTile && Main.tileSolid[tile.TileType] && !TileID.Sets.Platforms[tile.TileType]) //additional check for player teleporting inside of a wall somehow (seemed to be caused by slopes)
				Owner.position -= new Vector2(0f, Owner.height);

			Owner.velocity += originalPos.DirectionTo(Main.MouseWorld) * 5;

			teleported = true;

			float step = 0.1f;
			for (int i = 0; i < (1 / step); i++)
			{
				Vector2 stepPos = Vector2.Lerp(originalPos, Owner.Center, step * i);
				teleportPositions.Add(stepPos);
			}

			step = 0.033f;
			for (int i = (int)(1 / step); i > 0; i--)
			{
				Vector2 stepPos = PlayerTarget.getPlayerTargetPosition(Owner.whoAmI) + Main.screenPosition + (originalPos - Owner.Center) * step * i;
				mp.trailPositions.Add(stepPos);
			}

			mp.teleportTimer = 60;

			Helper.PlayPitched("Magic/ShurikenThrow", 1f, Main.rand.NextFloat(-0.3f, 0.3f), Owner.Center);

			Helper.PlayPitched("Effects/HeavyWhooshShort", 1.5f, Main.rand.NextFloat(-0.3f, 0.3f), Owner.Center);

			CameraSystem.shake += 15;

			for (int i = 0; i < 50; i++)
			{	
				Vector2 pos = Vector2.Lerp(originalPos, Owner.Center, Main.rand.NextFloat());
				Dust.NewDustPerfect(pos + Main.rand.NextVector2Circular(Owner.width, Owner.height), ModContent.DustType<Dusts.GlowLine>(), originalPos.DirectionTo(Owner.Center) * Main.rand.NextFloat(1f, 5f), Main.rand.Next(100), new Color(Main.rand.Next(100, 255), 0, 0), 1.25f).fadeIn = Main.rand.NextFloat(30);

				Dust.NewDustPerfect(pos + Main.rand.NextVector2Circular(Owner.width, Owner.height), DustID.Blood, originalPos.DirectionTo(Owner.Center) * Main.rand.NextFloat(10f, 15f), 0, new Color(255, 0, 0, 100), 2f).noGravity = true;
			}

			bool anyIFramesWouldBeGiven = false;
			for (int j = 0; j < Owner.hurtCooldowns.Length; j++)
			{
				if (Owner.hurtCooldowns[j] < 45)
				{
					anyIFramesWouldBeGiven = true;
				}
			}

			if (anyIFramesWouldBeGiven)
			{
				Owner.immune = true;
				Owner.immuneNoBlink = true;
				Owner.immuneTime = 45;
				for (int i = 0; i < Owner.hurtCooldowns.Length; i++)
				{
					if (Owner.hurtCooldowns[i] < 45)
					{
						Owner.hurtCooldowns[i] = 45;
					}
				}
			}
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			var mp = Owner.GetModPlayer<RadculasRapierPlayer>();

			for (int i = 0; i < teleportPositions.Count; i++)
			{
				if (Vector2.Distance(teleportPositions[i], targetHitbox.Center.ToVector2()) < 50f)
					return true;
			}

			return false;
		}

		public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
		{
			damage = damage * (int)Math.Round(MathHelper.Lerp(1, 3, target.GetGlobalNPC<RadculasRapierNPC>().duration / 600f));
		}

		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
		{
			var gNPC = target.GetGlobalNPC<RadculasRapierNPC>();

			if (hitNPCs.Count <= 0)
				Helper.PlayPitched("Impacts/GoreHeavy", 1.5f, Main.rand.NextFloat(-0.3f, 0.3f), Owner.Center);

			hitNPCs.Add(target);

			Vector2 pos = target.Center;

			for (int i = 0; i < 10; i++)
			{
				Dust.NewDustPerfect(pos, ModContent.DustType<Dusts.GlowFastDecelerate>(), target.Center.DirectionTo(originalPos).RotatedByRandom(0.55f) * Main.rand.NextFloat(2f, 5f), 0, new Color(150, 0, 0, 100), 1f);
			}

			if (Helper.IsFleshy(target))
			{
				for (int k = 0; k < 15; k++)
				{
					Dust.NewDustPerfect(pos, DustID.Blood, target.Center.DirectionTo(originalPos).RotatedByRandom(0.65f) * Main.rand.NextFloat(2f, 8f), 0, default, 2f);

					Dust.NewDustPerfect(pos, ModContent.DustType<Dusts.GraveBlood>(), target.Center.DirectionTo(originalPos).RotatedByRandom(0.65f) * Main.rand.NextFloat(2f, 8f), 0, default, 2f);
				}
			}
			else
			{
				for (int k = 0; k < 15; k++)
				{
					Dust.NewDustPerfect(pos, ModContent.DustType<Dusts.Glow>(), target.Center.DirectionTo(originalPos).RotatedByRandom(0.65f) * Main.rand.NextFloat(2f, 8f), 0, new Color(255, Main.rand.Next(130, 255), 80), Main.rand.NextFloat(0.65f, 1f));
				}
			}

			if (gNPC.duration > 0)
			{
				int healAmount = (int)Math.Round(MathHelper.Lerp(1, 5, gNPC.duration / 600f));

				Owner.Heal(healAmount);
				gNPC.duration = 0;
			}
		}

		public override bool? CanHitNPC(NPC target)
		{
			return Projectile.friendly && !hitNPCs.Contains(target) && Projectile.timeLeft == 30;
		}

		public override void Kill(int timeLeft)
		{
			if (!teleported)
				return;

			Owner.AddBuff(ModContent.BuffType<RadculasRapierCooldown>(), 120); //20 seconds by default, plus up to 15 seconds decrease based on enemies hit

			for (int i = 0; i < 50; i++)
			{
				Vector2 pos = Vector2.Lerp(Owner.MountedCenter + new Vector2(-20f, 0).RotatedBy(Projectile.rotation - MathHelper.PiOver2) + offset, Owner.MountedCenter + new Vector2(120f, 0).RotatedBy(Projectile.rotation - MathHelper.PiOver2) + offset, Main.rand.NextFloat());
				Dust.NewDustPerfect(pos + Main.rand.NextVector2Circular(1f, 1f), ModContent.DustType<Dusts.GlowFastDecelerate>(), Main.rand.NextVector2Circular(2f, 2f), 0, new Color(200, 0, 0, 100), 0.35f);
			}

			Helper.PlayPitched("Impacts/Clink", 1f, Main.rand.NextFloat(-0.3f, 0.3f), Owner.Center);
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
			Texture2D texGlow = ModContent.Request<Texture2D>(Texture + "_Glow").Value;

			float fade = 1f;

			if (Projectile.timeLeft < 10f)
				fade = Projectile.timeLeft / 10f;

			Main.spriteBatch.Draw(texGlow, Projectile.Center - Main.screenPosition + new Vector2(0, Main.player[Projectile.owner].gfxOffY), null, new Color(100, 0, 0, 0) * fade, Projectile.rotation, texGlow.Size() / 2f, Projectile.scale, 0, 0);

			Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition + new Vector2(0, Main.player[Projectile.owner].gfxOffY), null, Color.White * fade, Projectile.rotation, tex.Size() / 2f, Projectile.scale, 0, 0);

			return false;
		}
	}

	class RadculasRapierCooldown : ModBuff
	{
		public override string Texture => AssetDirectory.MiscItem + Name;
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Radcula's Teleport Cooldown");
			Description.SetDefault("The rapier's energy seems deminished");
			Main.debuff[Type] = true;
			Main.pvpBuff[Type] = true;
			Main.buffNoSave[Type] = true;
			BuffID.Sets.NurseCannotRemoveDebuff[Type] = true;
		}
	}

	class RadculasRapierHeart : ModDust
	{
		public override string Texture => AssetDirectory.MiscItem + Name;

		public override void OnSpawn(Dust dust)
		{
			dust.noGravity = true;
			dust.noLight = false;
			dust.frame = new Rectangle(0, 0, 14, 12);
		}

		public override Color? GetAlpha(Dust dust, Color lightColor)
		{
			return new Color(255, 255, 255) * (1f - dust.alpha / 255f);
		}

		public override bool Update(Dust dust)
		{
			dust.position += dust.velocity;
			dust.velocity *= 0.9f;
			dust.color *= 0.96f;

			dust.alpha += 10;
			if (dust.alpha >= 255)
				dust.active = false;

			return false;
		}
	}
}