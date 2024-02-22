using ReLogic.Content;
using StarlightRiver.Core.Systems.CameraSystem;
using StarlightRiver.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent.Creative;
using Terraria.Graphics.Effects;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Haunted
{
	public class EchochainWhip : ModItem
	{
		internal int cooldown;
		public override string Texture => AssetDirectory.HauntedItem + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Echochain");
			Tooltip.SetDefault("Chains enemies together, sharing summon damage between them\nHold and release <right> to snare enemies near your mouse to the ground, chaining all effected enemies");
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
		}

		public override void SetDefaults()
		{
			Item.DefaultToWhip(ModContent.ProjectileType<EchochainWhipProjectile>(), 11, 3f, 3.75f, 40);
			Item.SetShopValues(ItemRarityColor.Green2, Item.sellPrice(gold: 1));
		}

		public override bool AltFunctionUse(Player player)
		{
			return cooldown <= 0;
		}

		public override void UpdateInventory(Player player)
		{
			if (cooldown > 0)
				cooldown--;
		}

		public override bool CanUseItem(Player player)
		{
			if (player.altFunctionUse == 2)
			{
				Item.UseSound = SoundID.DD2_BetsyFireballImpact;
				Item.useStyle = ItemUseStyleID.Shoot;
			}
			else
			{
				Item.UseSound = SoundID.Item152;
				Item.useStyle = ItemUseStyleID.Swing;
			}

			return base.CanUseItem(player);
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			if (player.altFunctionUse == 2 && cooldown <= 0)
			{
				Projectile.NewProjectile(source, position, velocity, ModContent.ProjectileType<EchochainWhipAltProjectile>(), damage, knockback, player.whoAmI);
				cooldown = 600;
				return false;
			}

			return !Main.projectile.Any(n => n.active && n.type == type && n.owner == player.whoAmI);
		}

		public override void HoldItem(Player player)
		{
			player.GetModPlayer<ControlsPlayer>().rightClickListener = true;
		}

		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();

			recipe.AddIngredient(ItemID.GoldBar, 15);
			recipe.AddIngredient<VengefulSpirit>(10);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();

			recipe = CreateRecipe();

			recipe.AddIngredient(ItemID.PlatinumBar, 15);
			recipe.AddIngredient<VengefulSpirit>(10);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}
	}

	public class EchochainWhipAltProjectile : ModProjectile
	{
		public bool playedSound;

		public float handRotation;
		public Vector2 oldMouse;

		public Point16?[] tiles = new Point16?[18]; // tiles which will glow visually
		public Point16?[] enemyTiles = new Point16?[6]; // tiles which enemies will be chained onto
		public NPC[] targets = new NPC[6];

		public ref float DeathTimer => ref Projectile.ai[0];

		public Vector2 OwnerMouse => Owner.GetModPlayer<ControlsPlayer>().mouseWorld;
		public Player Owner => Main.player[Projectile.owner];

		public bool CanHold => Owner.GetModPlayer<ControlsPlayer>().mouseRight && !Owner.CCed && !Owner.noItems;

		public override string Texture => AssetDirectory.Invisible;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Echochain Aura");
		}

		public override void SetDefaults()
		{
			Projectile.DamageType = DamageClass.Summon;
			Projectile.width = 10;
			Projectile.height = 10;
			Projectile.friendly = true;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
		}

		public override bool? CanDamage()
		{
			return false;
		}

		public override bool PreAI()
		{
			Projectile.Center = DeathTimer > 0 ? oldMouse : OwnerMouse;
			PopulateTilesAndTargets();

			return true;
		}

		public override void AI()
		{
			if (DeathTimer > 0)
				UpdateDeathAnimation();
			else
				UpdateHeldProjectile();

			UpdateDustVisuals();

			Owner.heldProj = Projectile.whoAmI;
			Owner.itemTime = 2;
			Owner.itemAnimation = 2;

			Owner.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Full, handRotation);
			Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, 0f);
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D bloomTex = ModContent.Request<Texture2D>(AssetDirectory.Keys + "GlowAlpha").Value;

			float fadeOut = 1f;
			if (DeathTimer > 0)
				fadeOut = DeathTimer / 30f;

			Effect effect = Filters.Scene["DistortSprite"].GetShader().Shader;

			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);

			effect.Parameters["time"].SetValue((float)Main.timeForVisualEffects * 0.005f);
			effect.Parameters["uTime"].SetValue((float)Main.timeForVisualEffects * 0.005f);
			effect.Parameters["screenPos"].SetValue(Main.screenPosition * new Vector2(0.5f, 0.1f) / new Vector2(Main.screenWidth, Main.screenHeight));

			effect.Parameters["offset"].SetValue(new Vector2(0.001f));
			effect.Parameters["repeats"].SetValue(2);
			effect.Parameters["uImage1"].SetValue(ModContent.Request<Texture2D>(AssetDirectory.Assets + "Noise/SwirlyNoiseLooping").Value);
			effect.Parameters["uImage2"].SetValue(ModContent.Request<Texture2D>(AssetDirectory.Assets + "Noise/PerlinNoise").Value);
			effect.Parameters["noiseImage1"].SetValue(ModContent.Request<Texture2D>(AssetDirectory.Assets + "Noise/PerlinNoise").Value);

			Color color = new Color(150, 255, 25, 0) * 0.5f * fadeOut;
			effect.Parameters["uColor"].SetValue(color.ToVector4());

			effect.CurrentTechnique.Passes[0].Apply();

			Main.spriteBatch.Draw(bloomTex, Projectile.Center - Main.screenPosition, null, Color.White, 0f, bloomTex.Size() / 2f, 0.65f, 0f, 0f);
			Main.spriteBatch.Draw(bloomTex, Projectile.Center - Main.screenPosition, null, Color.White, 0f, bloomTex.Size() / 2f, 0.45f, 0f, 0f);

			color = new Color(200, 255, 200, 0) * 0.5f * fadeOut;
			effect.Parameters["uColor"].SetValue(color.ToVector4());

			effect.CurrentTechnique.Passes[0].Apply();

			Main.spriteBatch.Draw(bloomTex, Projectile.Center - Main.screenPosition, null, Color.White, 0f, bloomTex.Size() / 2f, 0.25f, 0f, 0f);

			Vector2 pos = Owner.GetBackHandPosition(Player.CompositeArmStretchAmount.Full, handRotation) - Main.screenPosition;

			color = new Color(150, 255, 25, 0) * 0.5f * fadeOut;
			effect.Parameters["uColor"].SetValue(color.ToVector4());

			effect.CurrentTechnique.Passes[0].Apply();

			Main.spriteBatch.Draw(bloomTex, pos, null, Color.White, 0f, bloomTex.Size() / 2f, 0.65f, 0f, 0f);
			Main.spriteBatch.Draw(bloomTex, pos, null, Color.White, 0f, bloomTex.Size() / 2f, 0.45f, 0f, 0f);

			color = new Color(200, 255, 200, 0) * 0.5f * fadeOut;
			effect.Parameters["uColor"].SetValue(color.ToVector4());

			effect.CurrentTechnique.Passes[0].Apply();

			Main.spriteBatch.Draw(bloomTex, pos, null, Color.White, 0f, bloomTex.Size() / 2f, 0.25f, 0f, 0f);

			float mult = MathHelper.Lerp(0.15f, 0.05f, (float)Math.Sin(Main.GlobalTimeWrappedHourly));

			color = new Color(150, 255, 25, 0) * mult * fadeOut;
			effect.Parameters["uColor"].SetValue(color.ToVector4());

			effect.CurrentTechnique.Passes[0].Apply();

			for (int i = 0; i < tiles.Length; i++)
			{
				if (tiles[i] != null)
				{
					Vector2 drawPos = new Vector2(tiles[i].Value.X * 16, tiles[i].Value.Y * 16) - Main.screenPosition + new Vector2(5f);

					Main.spriteBatch.Draw(bloomTex, drawPos, null, Color.White, 0f, bloomTex.Size() / 2f, 0.65f, 0f, 0f);
					Main.spriteBatch.Draw(bloomTex, drawPos, null, Color.White, 0f, bloomTex.Size() / 2f, 0.45f, 0f, 0f);
					Main.spriteBatch.Draw(bloomTex, drawPos, null, Color.White, 0f, bloomTex.Size() / 2f, 0.25f, 0f, 0f);
				}
			}

			Main.spriteBatch.End();
			Main.spriteBatch.Begin(default, default, Main.DefaultSamplerState, default, default, default, Main.GameViewMatrix.TransformationMatrix);

			return false;
		}

		private void UpdateDustVisuals()
		{
			for (int i = 0; i < tiles.Length; i++)
			{
				if (tiles[i] != null)
				{
					var pos = new Vector2(tiles[i].Value.X * 16, tiles[i].Value.Y * 16);
					pos += new Vector2(12f, 0f);
					pos.X += Main.rand.Next(-8, 8);

					if (Main.rand.NextBool(10))
						Dust.NewDustPerfect(pos, ModContent.DustType<Dusts.GlowFastDecelerate>(), Main.rand.NextVector2Circular(1f, 1f) + Vector2.UnitY * -Main.rand.NextFloat(2f), 0, new Color(100, 200, 10), 0.35f);

					if (Main.rand.NextBool(10))
						Dust.NewDustPerfect(pos, ModContent.DustType<Dusts.GlowFastDecelerate>(), Main.rand.NextVector2Circular(2f, 2f) + Vector2.UnitY.RotatedByRandom(1f) * -Main.rand.NextFloat(3f), 0, new Color(150, 255, 25), 0.2f);
				}
			}

			for (int i = 0; i < enemyTiles.Length; i++)
			{
				if (enemyTiles[i] != null)
				{
					var pos = new Vector2(enemyTiles[i].Value.X * 16, enemyTiles[i].Value.Y * 16);
					pos += new Vector2(12f, 0f);

					if (Main.rand.NextBool(20))
						Dust.NewDustPerfect(pos, ModContent.DustType<EchochainChainDust>(), (Vector2.UnitY * -Main.rand.NextFloat(1.5f)).RotatedByRandom(0.5f), Main.rand.Next(150), default, 1.25f).noGravity = true;
				}
			}

			if (Main.rand.NextBool(5))
				Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(15f, 15f), ModContent.DustType<Dusts.GlowFastDecelerate>(), Main.rand.NextVector2Circular(2f, 2f) + Vector2.UnitY * -Main.rand.NextFloat(5f), 0, new Color(150, 255, 25), 0.2f);
		}

		private void UpdateDeathAnimation()
		{
			float progress = 1f - DeathTimer / 30f;

			Projectile.timeLeft = 2;
			DeathTimer--;

			if (DeathTimer == 1)
			{
				Projectile.Kill();
				Owner.itemTime = 0;
				Owner.itemAnimation = 0;
				Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, 0f); // gotta set the composite arm here again otherwise the players front arm appears out for a frame and it looks bad		
				return;
			}

			if (progress < 0.25f)
			{
				handRotation = MathHelper.Lerp(-1f, -3f, EaseBuilder.EaseCircularInOut.Ease(1f - (DeathTimer - 22.5f) / 7.5f)) * Owner.direction;
			}
			else
			{
				handRotation = MathHelper.Lerp(-3f, -0.5f, EaseBuilder.EaseQuarticIn.Ease(1f - DeathTimer / 22.5f)) * Owner.direction;
				if (progress >= 0.9f && !playedSound)
				{
					Helper.PlayPitched("Effects/HeavyWhooshShort", 1f, 0f, Owner.Center);

					CameraSystem.shake += 6;
					playedSound = true;
				}
			}
		}

		private void UpdateHeldProjectile()
		{
			Owner.GetModPlayer<ControlsPlayer>().mouseListener = true;
			Projectile.rotation = Owner.DirectionTo(OwnerMouse).ToRotation();

			if (!CanHold)
			{
				Projectile.timeLeft = 5;
				DeathTimer = 30f;
				oldMouse = OwnerMouse;
				CameraSystem.shake += 4;
				Helper.PlayPitched("Effects/HeavyWhoosh", 1f, 0f, Owner.Center);

				for (int i = 0; i < Main.rand.Next(2, 4); i++)
				{
					Dust.NewDustPerfect(Owner.Center + new Vector2(15f * Owner.direction, -5f) + Main.rand.NextVector2CircularEdge(5f, 5f), ModContent.DustType<Dusts.GlowLine>(), -Vector2.UnitY.RotatedByRandom(0.2f) * 1.5f, 200, new Color(130, 255, 50, 0), 0.35f);
				}

				var targetsToChain = new List<NPC>();

				for (int i = 0; i < enemyTiles.Length; i++)
				{
					if (enemyTiles[i] != null && targets[i] != null && targets[i].active && Collision.CanHitLine(new Vector2(enemyTiles[i].Value.X * 16, enemyTiles[i].Value.Y * 16) + new Vector2(0f, -30f), 2, 2, targets[i].Center, 2, 2))
					{
						targetsToChain.Add(targets[i]);

						if (Main.myPlayer == Owner.whoAmI)
						{
							var pos = new Vector2(enemyTiles[i].Value.X * 16, enemyTiles[i].Value.Y * 16);

							int[] frames = new int[17];
							for (int a = 0; a < frames.Length; a++) // populate array
							{
								frames[a] = Main.rand.Next(3);
							}

							EchochainWhipAltProjectileChain.targetToAssign = targets[i];
							EchochainWhipAltProjectileChain.tilePositionToAssign = pos + new Vector2(0f, 15f);
							EchochainWhipAltProjectileChain.chainFramesToAssign = frames;
							EchochainWhipAltProjectileChain.maxStabTimerToAssign = MathHelper.Lerp(30f, 60f, Vector2.Distance(pos + new Vector2(0f, 15f), targets[i].Center) / 250f);
							Projectile.NewProjectileDirect(null, pos, Vector2.Zero, ModContent.ProjectileType<EchochainWhipAltProjectileChain>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
						}
					}
				}

				if (targetsToChain.Count >= 2)
				{
					EchochainSystem.AddNPCS(targetsToChain);
					for (int i = 0; i < targetsToChain.Count; i++)
					{
						EchochainSystem.ResetTimers(targetsToChain[i]);
					}
				}
			}

			Owner.ChangeDir(OwnerMouse.X < Owner.Center.X ? -1 : 1);

			Projectile.timeLeft = 2;

			handRotation = -1f * Owner.direction;
		}

		private void PopulateTilesAndTargets()
		{
			for (int i = 0; i < tiles.Length; i++)
			{
				tiles[i] = null;
			}

			for (int i = 0; i < targets.Length; i++)
			{
				targets[i] = null;
				enemyTiles[i] = null;
			}

			Vector2 startPos = Projectile.Center;

			for (int x = -9; x < 9; x++) // search 9 tiles each direction
			{
				int index = x + 9;
				for (int y = 0; y < 25; y++) // search 25 tiles down
				{
					Vector2 worldPos = startPos + new Vector2(16f * x, y * 16f);
					var tilePos = new Point16((int)worldPos.X / 16, (int)worldPos.Y / 16);
					Tile tile = Framing.GetTileSafely(tilePos);
					Tile aboveTile = Framing.GetTileSafely(new Point16(tilePos.X, tilePos.Y - 1));
					if (tile.HasTile && !WorldGen.SolidOrSlopedTile(aboveTile) && WorldGen.SolidOrSlopedTile(tile) && !tiles.Contains(tilePos))
					{
						tiles[index] = tilePos;
						if (index % 3 == 0)
						{
							int realIndex = index <= 0 ? 0 : index / 3;

							enemyTiles[realIndex] = tilePos;
							targets[realIndex] = Main.npc.Where(n => n.active && n.CanBeChasedBy() && n.Distance(worldPos) < 250f && !targets.Contains(n)).OrderBy(n => n.Distance(worldPos)).FirstOrDefault();
						}

						break;
					}
				}
			}
		}
	}

	public class EchochainWhipAltProjectileChain : ModProjectile
	{
		public static float maxStabTimerToAssign;
		private float maxStabTimer = 60f;

		public static int[] chainFramesToAssign;
		private int[] chainFrames;

		public static Vector2 tilePositionToAssign;
		private Vector2 tilePosition;

		public static NPC targetToAssign;
		private NPC target;

		private bool hasHit;

		private bool hitGround;

		public ref float StabTimer => ref Projectile.ai[0];

		public Player Owner => Main.player[Projectile.owner];

		public override string Texture => AssetDirectory.Invisible;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Echo Chain");
		}

		public override void SetDefaults()
		{
			Projectile.DamageType = DamageClass.Generic;
			Projectile.width = 16;
			Projectile.height = 16;
			Projectile.friendly = true;
			Projectile.hostile = false;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.timeLeft = 300;
			Projectile.penetrate = -1;
			Projectile.hide = true;
		}

		public override void OnSpawn(IEntitySource source)
		{
			maxStabTimer = maxStabTimerToAssign;
			chainFrames = chainFramesToAssign;
			tilePosition = tilePositionToAssign;
			target = targetToAssign;
		}

		public override bool? CanHitNPC(NPC target)
		{
			if (target != this.target || hasHit)
				return false;

			return base.CanHitNPC(target);
		}

		public override void AI()
		{
			if (target == null || !target.active || tilePosition.Distance(target.Center) > 350f)
			{
				Projectile.Kill();
				return;
			}

			Owner.MinionAttackTargetNPC = target.whoAmI;

			if (StabTimer < maxStabTimer)
				StabTimer++;

			float progress = StabTimer / maxStabTimer;

			if (progress < 0.25f)
			{
				Projectile.Center = Vector2.Lerp(tilePosition + new Vector2(0f, -25f), target.Center + tilePosition.DirectionTo(target.Center) * 50f, EaseBuilder.EaseQuarticInOut.Ease(StabTimer / (maxStabTimer * 0.25f)));

				Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(5f, 5f), ModContent.DustType<Dusts.GlowFastDecelerate>(), Main.rand.NextVector2Circular(1f, 1f), 0, new Color(150, 255, 25), 0.65f);
			}
			else
			{
				Projectile.Center = Vector2.Lerp(Projectile.Center, tilePosition + new Vector2(0f, target.height > 25 ? -target.height : -25f), EaseBuilder.EaseQuarticIn.Ease((StabTimer - maxStabTimer * 0.25f) / (maxStabTimer * 0.75f)));

				if (target.knockBackResist > 0f)
					target.Center = Projectile.Center - tilePosition.DirectionTo(target.Center) * 50f * (1f - progress);

				if (progress >= 0.8f && !hitGround)
				{
					hitGround = true;
					Terraria.Audio.SoundEngine.PlaySound(SoundID.DD2_MonkStaffGroundImpact, Projectile.Center);
					CameraSystem.shake += 4;

					if (Main.myPlayer == Owner.whoAmI)
						target.SimpleStrikeNPC(Projectile.damage * 2, 0);

					for (int i = 0; i < 20; i++)
					{
						Dust.NewDustPerfect(target.Center, ModContent.DustType<Dusts.GlowFastDecelerate>(), Main.rand.NextVector2Circular(5f, 5f), 0, new Color(150, 255, 25), 0.65f);
					}

					if (Helper.IsFleshy(target))
					{
						for (int i = 0; i < 15; i++)
						{
							Dust.NewDustPerfect(target.Center, ModContent.DustType<Dusts.GraveBlood>(), Main.rand.NextVector2Circular(5f, 5f), 0, default, 1.65f);

							Dust.NewDustPerfect(target.Center, DustID.Blood, Main.rand.NextVector2Circular(15f, 15f), 100, default, 1.65f).noGravity = true;
						}
					}
				}
			}

			if (Main.rand.NextBool(5))
				Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(5f, 5f), ModContent.DustType<Dusts.GlowFastDecelerate>(), Main.rand.NextVector2Circular(5f, 5f), 0, new Color(150, 255, 25), 0.5f);
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			Owner.TryGetModPlayer(out StarlightPlayer sp);
			sp.SetHitPacketStatus(true);

			hasHit = true;
			if (target.knockBackResist <= 0f || Main.projectile.Any(p => p.active && p != Projectile && p.type == Type && (p.ModProjectile as EchochainWhipAltProjectileChain).target == target))
				Projectile.Kill();

			CameraSystem.shake += 2;

			for (int i = 0; i < 20; i++)
			{
				Dust.NewDustPerfect(target.Center, ModContent.DustType<Dusts.GlowFastDecelerate>(), tilePosition.DirectionTo(target.Center).RotatedByRandom(0.5f) * Main.rand.NextFloat(15f), 0, new Color(150, 255, 25), 0.65f);
			}

			if (Helper.IsFleshy(target))
			{
				for (int i = 0; i < 15; i++)
				{
					Dust.NewDustPerfect(target.Center, ModContent.DustType<Dusts.GraveBlood>(), tilePosition.DirectionTo(target.Center).RotatedByRandom(0.5f) * Main.rand.NextFloat(15f), 0, default, 1.65f);

					Dust.NewDustPerfect(target.Center, DustID.Blood, tilePosition.DirectionTo(target.Center).RotatedByRandom(0.5f) * Main.rand.NextFloat(15f), 100, default, 1.65f).noGravity = true;
				}
			}

			target.velocity += tilePosition.DirectionTo(target.Center) * 5f * target.knockBackResist;

			Helper.PlayPitched("Impacts/StabTiny", 1f, 0f, Projectile.Center);
		}

		public override void Kill(int timeLeft)
		{
			for (int i = 0; i < 15; i++)
			{
				Dust.NewDustPerfect(Vector2.Lerp(Projectile.Center, tilePosition, i / 15f), ModContent.DustType<Dusts.GlowFastDecelerate>(), Main.rand.NextVector2Circular(3f, 3f), 0, new Color(150, 200, 20), 0.5f);
			}
		}

		public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
		{
			behindNPCsAndTiles.Add(index);
		}

		public override bool PreDraw(ref Color lightColor)
		{
			if (target == null || !target.active)
				return false;

			SpriteBatch spriteBatch = Main.spriteBatch;

			Texture2D tex = ModContent.Request<Texture2D>(AssetDirectory.HauntedItem + "EchochainWhipChain").Value;
			Texture2D texGlow = ModContent.Request<Texture2D>(AssetDirectory.HauntedItem + "EchochainWhipChain_Glow").Value;
			Texture2D texBlur = ModContent.Request<Texture2D>(AssetDirectory.HauntedItem + "EchochainWhipChain_Blur").Value;
			Texture2D bloomTex = ModContent.Request<Texture2D>(AssetDirectory.Keys + "GlowAlpha").Value;

			Vector2 chainEnd = tilePosition;
			Vector2 chainStart = Projectile.Center;

			float rotation = chainStart.DirectionTo(chainEnd).ToRotation() + MathHelper.PiOver2;

			float distance = Vector2.Distance(chainStart, chainEnd);

			spriteBatch.Draw(bloomTex, chainEnd - Main.screenPosition, null, new Color(100, 200, 10, 0) * 0.5f, 0f, bloomTex.Size() / 2f, 1f, 0f, 0f);

			for (int i = 0; i < (distance / 22); i += 1)
			{
				int chainFrame = chainFrames[i];
				var pos = Vector2.Lerp(chainStart, chainEnd, i * 22 / distance);

				spriteBatch.Draw(bloomTex, pos - Main.screenPosition, null, new Color(100, 200, 10, 0) * 0.5f, 0f, bloomTex.Size() / 2f, 0.5f, 0f, 0f);

				Rectangle frame = tex.Frame(verticalFrames: 5, frameY: chainFrame);
				spriteBatch.Draw(tex, pos - Main.screenPosition, frame, Color.White, rotation, frame.Size() / 2f, 1f, 0f, 0f);

				frame = texBlur.Frame(verticalFrames: 5, frameY: chainFrame);
				spriteBatch.Draw(texBlur, pos - Main.screenPosition, frame, Color.White with { A = 0 } * 0.75f, rotation, frame.Size() / 2f, 1f, 0f, 0f);
			}

			Rectangle rect = tex.Frame(verticalFrames: 5, frameY: 3);
			spriteBatch.Draw(tex, Projectile.Center + new Vector2(2f, 2f) - Main.screenPosition, rect, Color.White, rotation + MathHelper.ToRadians(90f), rect.Size() / 2f, 1.5f, 0f, 0f);

			rect = texBlur.Frame(verticalFrames: 5, frameY: 3);
			spriteBatch.Draw(texBlur, Projectile.Center + new Vector2(2f, 2f) - Main.screenPosition, rect, Color.White with { A = 0 } * 0.75f, rotation, rect.Size() / 2f, 1.5f, 0f, 0f);

			return false;
		}

		public override void SendExtraAI(BinaryWriter writer)
		{
			writer.Write(maxStabTimer);
			writer.Write(Array.ConvertAll(chainFrames, b => (byte)b), 0, 17);
			writer.WriteVector2(tilePosition);
			writer.Write(target.whoAmI);
		}

		public override void ReceiveExtraAI(BinaryReader reader)
		{
			maxStabTimer = reader.ReadSingle();
			chainFrames = Array.ConvertAll(reader.ReadBytes(17), i => (int)i);
			tilePosition = reader.ReadVector2();
			int whoAmI = reader.ReadInt32();

			target = Main.npc[whoAmI];
		}
	}

	public class EchochainWhipProjectile : BaseWhip
	{
		public List<Vector2> tipPositions = new();
		public List<float> tipRotations = new();

		public List<NPC> hitTargets = new();
		public override string Texture => AssetDirectory.HauntedItem + Name;

		public EchochainWhipProjectile() : base("Echochain", 40, 1.25f, new Color(150, 255, 20) * 0.5f) { }

		public override void ArcAI()
		{
			if (Projectile.ai[0] > flyTime * 0.45f)
			{
				var points = new List<Vector2>();
				points.Clear();
				SetPoints(points);

				Vector2 tipPos = points[39];

				tipPositions.Add(tipPos);
				if (tipPositions.Count > 15)
					tipPositions.RemoveAt(0);

				Vector2 difference = points[39] - points[38];
				float rotation = difference.ToRotation() - MathHelper.PiOver2;

				tipRotations.Add(rotation);
				if (tipRotations.Count > 15)
					tipRotations.RemoveAt(0);

				Dust.NewDustPerfect(tipPos, ModContent.DustType<Dusts.GlowFastDecelerate>(), Main.rand.NextVector2Circular(1f, 1f), 0, new Color(100, 200, 10), Main.rand.NextFloat(0.3f, 0.5f));

				Dust.NewDustPerfect(tipPos, ModContent.DustType<Dusts.GlowFastDecelerate>(), Main.rand.NextVector2Circular(2f, 2f), 0, new Color(150, 255, 25), Main.rand.NextFloat(0.15f, 0.35f));
			}
		}

		public override void DrawBehindWhip(ref Color lightColor)
		{
			Texture2D texBlur = ModContent.Request<Texture2D>(Texture + "_TipBlur").Value;
			Texture2D texGlow = ModContent.Request<Texture2D>(Texture + "_TipGlow").Value;
			Texture2D bloomTex = ModContent.Request<Texture2D>(AssetDirectory.Keys + "GlowAlpha").Value;

			Asset<Texture2D> texture = ModContent.Request<Texture2D>(Texture);
			Rectangle whipFrame = texture.Frame(1, 5, 0, 0);
			int height = whipFrame.Height;

			float fadeOut = 1f;
			if (Projectile.ai[0] > flyTime * 0.4f && Projectile.ai[0] < flyTime * 0.9f)
				fadeOut *= 1f - (Projectile.ai[0] - flyTime * 0.4f) / (flyTime * 0.5f);
			else if (Projectile.ai[0] >= flyTime * 0.9f)
				fadeOut = 0f;

			for (int i = 15; i > 0; i--)
			{
				float fade = 1 - (15f - i) / 15f;

				if (i > 0 && i < tipPositions.Count)
				{
					whipFrame.Y = height * 4;
					Color color = Color.Lerp(new Color(20, 135, 15, 0), new Color(100, 200, 10, 0), fade) * 0.5f;

					Main.EntitySpriteDraw(texture.Value, tipPositions[i] - Main.screenPosition, whipFrame, Color.White * 0.15f * fade * fadeOut, tipRotations[i], whipFrame.Size() * 0.5f, Projectile.scale, SpriteEffects.None, 0);

					Main.spriteBatch.Draw(texBlur, tipPositions[i] - Main.screenPosition, null, Color.White with { A = 0 } * 0.15f * fade * fadeOut, tipRotations[i], texBlur.Size() / 2f, Projectile.scale, 0f, 0f);

					Main.spriteBatch.Draw(bloomTex, tipPositions[i] - Main.screenPosition, null, color * fade * fadeOut, 0f, bloomTex.Size() / 2f, 1f * fade, 0f, 0f);

					Main.spriteBatch.Draw(bloomTex, tipPositions[i] - Main.screenPosition, null, color * fade * fadeOut * 0.25f, 0f, bloomTex.Size() / 2f, 1.5f * fade, 0f, 0f);

					Main.spriteBatch.Draw(bloomTex, tipPositions[i] - Main.screenPosition, null, Color.White with { A = 0 } * 0.15f * fade * fadeOut * 0.4f, 0f, bloomTex.Size() / 2f, 0.8f * fade, 0f, 0f);

					if (i < 15 && i + 1 < tipPositions.Count)
					{
						var newPosition = Vector2.Lerp(tipPositions[i], tipPositions[i + 1], 0.5f);
						float newRotation = MathHelper.Lerp(tipRotations[i], tipRotations[i + 1], 0.5f);

						Main.EntitySpriteDraw(texture.Value, newPosition - Main.screenPosition, whipFrame, Color.White * 0.15f * fade * fadeOut, newRotation, whipFrame.Size() * 0.5f, Projectile.scale, SpriteEffects.None, 0);

						Main.spriteBatch.Draw(texBlur, newPosition - Main.screenPosition, null, Color.White with { A = 0 } * 0.15f * fade * fadeOut, newRotation, texBlur.Size() / 2f, Projectile.scale, 0f, 0f);

						Main.spriteBatch.Draw(bloomTex, newPosition - Main.screenPosition, null, color * fade * fadeOut, 0f, bloomTex.Size() / 2f, 1f * fade, 0f, 0f);

						Main.spriteBatch.Draw(bloomTex, newPosition - Main.screenPosition, null, color * fade * fadeOut * 0.15f, 0f, bloomTex.Size() / 2f, 1.5f * fade, 0f, 0f);

						Main.spriteBatch.Draw(bloomTex, newPosition - Main.screenPosition, null, Color.White with { A = 0 } * 0.15f * fade * fadeOut * 0.4f, 0f, bloomTex.Size() / 2f, 0.8f * fade, 0f, 0f);
					}
				}
			}
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			Main.player[Projectile.owner].TryGetModPlayer(out StarlightPlayer sp);
			sp.SetHitPacketStatus(true);

			hitTargets.Add(target);

			var points = new List<Vector2>();
			points.Clear();
			SetPoints(points);

			Vector2 tipPos = points[39];

			for (int i = 0; i < 5; i++)
			{
				Dust.NewDustPerfect(target.Center, ModContent.DustType<Dusts.GlowFastDecelerate>(), target.Center.DirectionTo(tipPos).RotatedByRandom(0.5f) * Main.rand.NextFloat(5f, 10f), 0, new Color(100, 200, 20), 0.5f);
			}

			EchochainSystem.ResetTimers(target);
		}

		public override void Kill(int timeLeft)
		{
			if (hitTargets.Count >= 2)
				EchochainSystem.AddNPCS(hitTargets);

			hitTargets.Clear();
		}
	}
}