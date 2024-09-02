using StarlightRiver.Content.Dusts;
using StarlightRiver.Core.Systems.CameraSystem;
using StarlightRiver.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.Localization;

namespace StarlightRiver.Content.Items.Breacher
{
	public class FlareBreacher : ModItem
	{
		public float shootRotation;
		public int shootDirection;

		public override string Texture => AssetDirectory.BreacherItem + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Flare Breacher");
			Tooltip.SetDefault("Fires explosive flares that embed in enemies, blasting shrapnel through and behind them");
		}

		public override void SetDefaults()
		{
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.useAnimation = 30;
			Item.useTime = 30;
			Item.knockBack = 2f;
			Item.UseSound = SoundID.Item11;
			Item.width = 24;
			Item.height = 28;
			Item.damage = 23;
			Item.rare = ItemRarityID.Orange;
			Item.value = Item.sellPrice(0, 1, 0, 0);
			Item.noMelee = true;
			Item.autoReuse = true;
			Item.useTurn = false;
			Item.useAmmo = AmmoID.Flare;
			Item.DamageType = DamageClass.Ranged;
			Item.shoot = ModContent.ProjectileType<ExplosiveFlare>();
			Item.shootSpeed = 17;
		}

		public override Vector2? HoldoutOffset()
		{
			return new Vector2(2, 0);
		}

		public override bool CanUseItem(Player Player)
		{
			shootRotation = (Player.Center - Main.MouseWorld).ToRotation();
			shootDirection = (Main.MouseWorld.X < Player.Center.X) ? -1 : 1;

			return base.CanUseItem(Player);
		}

		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
		{
			type = ModContent.ProjectileType<ExplosiveFlare>();
			position += new Vector2(0f, -10f * player.direction).RotatedBy(velocity.ToRotation());
		}

		public override void UseStyle(Player player, Rectangle heldItemFrame)
		{
			if (Item.noUseGraphic) // the item draws wrong for the first frame it is drawn when you switch directions for some odd reason, this plus setting it to true in shoot makes it not draw for the first frame.
				Item.noUseGraphic = false;

			float animProgress = 1f - player.itemTime / (float)player.itemTimeMax;

			if (Main.myPlayer == player.whoAmI)
				player.direction = shootDirection;		

			float itemRotation = player.compositeFrontArm.rotation + 1.5707964f * player.gravDir;
			Vector2 itemPosition = player.MountedCenter;

			if (animProgress < 0.05f)
			{
				float lerper = animProgress / 0.05f;
				itemPosition += itemRotation.ToRotationVector2() * MathHelper.Lerp(0f, -4f, EaseBuilder.EaseCircularOut.Ease(lerper));
			}
			else
			{
				float lerper = (animProgress - 0.05f) / 0.95f;
				itemPosition += itemRotation.ToRotationVector2() * MathHelper.Lerp(-4f, 0f, EaseBuilder.EaseBackInOut.Ease(lerper));
			}

			Vector2 itemSize = new Vector2(34f, 26f);
			Vector2 itemOrigin = new Vector2(-20f, 4f);

			Helper.CleanHoldStyle(player, itemRotation, itemPosition, itemSize, new Vector2?(itemOrigin), false, false, true);
		}

		public override void UseItemFrame(Player player)
		{
			if (Main.myPlayer == player.whoAmI)
				player.direction = shootDirection;

			float animProgress = 1f - player.itemTime / (float)player.itemTimeMax;
			float rotation = shootRotation * player.gravDir + 1.5707964f;

			if (animProgress < 0.05f)
			{
				float lerper = animProgress / 0.1f;
				rotation += MathHelper.Lerp(0f, -.35f, EaseBuilder.EaseCircularOut.Ease(lerper)) * player.direction;
			}
			else
			{
				float lerper = (animProgress - 0.1f) / 0.9f;
				rotation += MathHelper.Lerp(-.35f, 0, EaseBuilder.EaseBackInOut.Ease(lerper)) * player.direction;
			}

			Player.CompositeArmStretchAmount stretch = Player.CompositeArmStretchAmount.Full;
			if (animProgress < 0.5f)
				stretch = Player.CompositeArmStretchAmount.None;
			else if (animProgress < 0.75f)
				stretch = Player.CompositeArmStretchAmount.ThreeQuarters;

			player.SetCompositeArmFront(true, stretch, rotation);
		}

		public override bool Shoot(Player Player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			Vector2 barrelPos = position + new Vector2(30f, -4f * Player.direction).RotatedBy(velocity.ToRotation());

			for (int i = 0; i < 10; i++)
			{
				Dust.NewDustPerfect(barrelPos + Main.rand.NextVector2Circular(5f, 5f), DustID.Torch, velocity.RotatedByRandom(0.5f).RotatedByRandom(0.5f) * Main.rand.NextFloat(0.25f), 0, default, 1.2f).noGravity = true;

				Dust.NewDustPerfect(barrelPos + Main.rand.NextVector2Circular(5f, 5f), ModContent.DustType<Dusts.GlowFastDecelerate>(), velocity.RotatedByRandom(0.5f).RotatedByRandom(0.5f) * Main.rand.NextFloat(0.2f), 0, new Color(255, 50, 200), 0.3f).noGravity = true;

				Dust.NewDustPerfect(barrelPos + Main.rand.NextVector2Circular(5f, 5f), ModContent.DustType<Dusts.GlowFastDecelerate>(), velocity.RotatedByRandom(0.5f).RotatedByRandom(0.5f) * Main.rand.NextFloat(0.2f), 0, new Color(30, 230, 255), 0.3f).noGravity = true;
			}

			Dust.NewDustPerfect(barrelPos + Main.rand.NextVector2Circular(5f, 5f), ModContent.DustType<FlareBreacherSmokeDust>(), velocity * 0.025f, 50, new Color(255, 50, 200), 0.1f);

			Dust.NewDustPerfect(barrelPos + Main.rand.NextVector2Circular(5f, 5f), ModContent.DustType<FlareBreacherSmokeDust>(), velocity * 0.05f, 150, new Color(255, 50, 200), 0.2f);

			Dust.NewDustPerfect(barrelPos, ModContent.DustType<FlareBreacherStarDust>(), Vector2.Zero, 0, new Color(30, 230, 255, 0), 0.35f).customData = Player;

			Vector2 flashPos = barrelPos - new Vector2(5f, 0f).RotatedBy(velocity.ToRotation());

			Dust.NewDustPerfect(flashPos, ModContent.DustType<FlareBreacherMuzzleFlashDust>(), Vector2.Zero, 0, default, 0.75f).rotation = velocity.ToRotation();

			Helper.PlayPitched("Guns/FlareFire", 0.6f, Main.rand.NextFloat(-0.1f, 0.1f), position);
			CameraSystem.shake += 1;
			Item.noUseGraphic = true;

			return true;
		}

		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ModContent.ItemType<Content.Items.SpaceEvent.Astroscrap>(), 12);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}

		public static Condition getMerchantFlareCondition()
		{
			return new Condition(Language.GetText("Conditions.PlayerCarriesItem").WithFormatArgs(Lang.GetItemName(ModContent.ItemType<FlareBreacher>())), () => Main.LocalPlayer.HasItem(ModContent.ItemType<FlareBreacher>()) && !Main.LocalPlayer.HasItem(ItemID.FlareGun));
		}
	}

	internal class ExplosiveFlare : ModProjectile
	{
		bool red;
		bool stuck;

		int explosionTimer = 100;
		int enemyID;
		int blinkCounter;

		Vector2 offset = Vector2.Zero;

		public override string Texture => AssetDirectory.BreacherItem + Name;

		public override void SetDefaults()
		{
			Projectile.width = 10;
			Projectile.height = 10;
			Projectile.DamageType = DamageClass.Ranged;
			Projectile.friendly = true;
			Projectile.penetrate = -1;
			Projectile.aiStyle = 1;
			AIType = 163;
		}

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Explosive Flare");
			Main.projFrames[Projectile.type] = 2;
		}

		public override bool PreAI()
		{
			Lighting.AddLight(Projectile.Center, Color.Purple.ToVector3());
			Vector2 direction = (Projectile.rotation + 1.57f + Main.rand.NextFloat(-0.2f, 0.2f)).ToRotationVector2();

			if (stuck)
			{
				var dust = Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<BreacherDust>(), direction * Main.rand.NextFloat(3, 4));
				dust.scale = 1.15f;
				dust.noGravity = true;
				NPC target = Main.npc[enemyID];
				Projectile.position = target.position + offset;
				explosionTimer--;

				blinkCounter++;
				int timerVal = 3 + (int)Math.Sqrt(explosionTimer);

				if (blinkCounter > timerVal)
				{
					if (!red)
					{
						red = true;
						Helper.PlayPitched("Effects/Bleep", 1, 1 - explosionTimer / 100f, Projectile.Center);
						blinkCounter = 0;
					}
					else
					{
						red = false;
						blinkCounter = 0;
					}
				}

				if (explosionTimer <= 0 || !target.active)
					Explode(target);

				return false;
			}
			else
			{
				if (Projectile.timeLeft < 3598)
				{
					for (float i = -1; i < 0; i += 0.25f)
					{
						var dust = Dust.NewDustPerfect(Projectile.Center - Projectile.velocity * i, ModContent.DustType<BreacherDustFastFade>(), direction * Main.rand.NextFloat(3, 4));
						dust.scale = 0.85f;
						dust.noGravity = true;
					}
				}

				Projectile.rotation = Projectile.velocity.ToRotation() + 1.57f;
			}

			return base.PreAI();
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			Dust.NewDustPerfect(Projectile.Center + oldVelocity, ModContent.DustType<FlareBreacherDust>(), Vector2.Zero, 60, default, 0.7f).rotation = Main.rand.NextFloat(6.28f);
			return base.OnTileCollide(oldVelocity);
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			if (!stuck && target.life > 0)
			{
				stuck = true;
				Projectile.friendly = false;
				Projectile.tileCollide = false;
				enemyID = target.whoAmI;
				offset = Projectile.position - target.position;
				offset -= Projectile.velocity;
				Projectile.netUpdate = true;
			}
		}

		public override void SendExtraAI(BinaryWriter writer)
		{
			writer.Write(stuck);
			writer.WriteVector2(offset);
			writer.Write(enemyID);
		}

		public override void ReceiveExtraAI(BinaryReader reader)
		{
			stuck = reader.ReadBoolean();
			offset = reader.ReadVector2();
			enemyID = reader.ReadInt32();
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D tex = TextureAssets.Projectile[Projectile.type].Value;
			var source = new Rectangle(0, 0, Projectile.width, 16);

			if (stuck)
				source.Y += 16 * (red ? 1 : 0);

			Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, source, lightColor, Projectile.rotation, tex.Size() / 2 * new Vector2(1, 0.5f), Projectile.scale, 0, 0);

			return false;
		}

		private void Explode(NPC target)
		{
			Helper.PlayPitched("Guns/FlareBoom", 0.4f, Main.rand.NextFloat(-0.1f, 0.1f), Projectile.Center);

			if (!target.immortal && !target.dontTakeDamage)
				target.SimpleStrikeNPC(Projectile.damage, 0);

			CameraSystem.shake = 10;
			int numberOfProjectiles = Main.rand.Next(4, 6);

			if (Main.myPlayer == Projectile.owner)
			{
				for (int i = 0; i < numberOfProjectiles; i++)
				{
					float offsetRad = MathHelper.Lerp(0, 0.5f, i / (float)numberOfProjectiles);
					Vector2 pos = Projectile.Center + Vector2.UnitX.RotatedBy(Projectile.rotation - 1.57f) * target.width;
					Vector2 velocity = Vector2.UnitX.RotatedBy(Projectile.rotation + Main.rand.NextFloat(0 - offsetRad, offsetRad) - 1.57f) * Main.rand.NextFloat(9, 11);

					Projectile.NewProjectile(Projectile.GetSource_FromThis(), pos, velocity, ModContent.ProjectileType<FlareShrapnel>(), Projectile.damage / 4, Projectile.knockBack, Projectile.owner, target.whoAmI);
				}
			}

			for (int i = 0; i < 4; i++)
			{
				var dust = Dust.NewDustDirect(Projectile.Center + Vector2.UnitX.RotatedBy(Projectile.rotation - 1.57f), 0, 0, ModContent.DustType<FlareBreacherDust>());
				dust.velocity = Vector2.UnitX.RotatedBy(Projectile.rotation + Main.rand.NextFloat(-0.3f, 0.3f) - 1.57f) * Main.rand.NextFloat(5, 10);
				dust.scale = Main.rand.NextFloat(0.4f, 0.7f);
				dust.alpha = 40 + Main.rand.Next(40);
				dust.rotation = Main.rand.NextFloat(6.28f);
			}

			for (int i = 0; i < 8; i++)
			{
				var dust = Dust.NewDustDirect(Projectile.Center + Vector2.UnitX.RotatedBy(Projectile.rotation - 1.57f), 0, 0, ModContent.DustType<FlareBreacherDust>());
				dust.velocity = Vector2.UnitX.RotatedBy(Projectile.rotation + Main.rand.NextFloat(-0.3f, 0.3f) - 1.57f) * Main.rand.NextFloat(10, 20);
				dust.scale = Main.rand.NextFloat(0.75f, 1f);
				dust.alpha = 40 + Main.rand.Next(40);
				dust.rotation = Main.rand.NextFloat(6.28f);
			}

			for (int i = 0; i < 24; i++)
			{
				var dust = Dust.NewDustDirect(Projectile.Center + Vector2.UnitX.RotatedBy(Projectile.rotation - 1.57f), 0, 0, ModContent.DustType<BreacherDust>());
				dust.velocity = Vector2.UnitX.RotatedBy(Projectile.rotation + Main.rand.NextFloat(-0.3f, 0.3f) - 1.57f) * Main.rand.NextFloat(1, 5);
				dust.scale = Main.rand.NextFloat(0.75f, 1.1f);
			}

			Projectile.active = false;
		}
	}

	internal class FlareShrapnel : ModProjectile, IDrawPrimitive
	{
		private List<Vector2> cache;
		private Trail trail;

		private NPC source => Main.npc[(int)Projectile.ai[0]];

		public override string Texture => AssetDirectory.BreacherItem + "ExplosiveFlare";

		public override void SetDefaults()
		{
			Projectile.width = 4;
			Projectile.height = 4;
			Projectile.DamageType = DamageClass.Ranged;
			Projectile.friendly = true;
			Projectile.penetrate = 1;
			Projectile.timeLeft = Main.rand.Next(50, 70);
			Projectile.extraUpdates = 4;
			Projectile.alpha = 255;
		}

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Explosive Shrapnel");
			Main.projFrames[Projectile.type] = 2;
		}

		public override void AI()
		{
			Projectile.velocity *= 0.96f;
			if (Main.netMode != NetmodeID.Server)
			{
				ManageCaches();
				ManageTrail();
			}
		}

		public override bool? CanHitNPC(NPC target)
		{
			if (target == source)
				return false;

			return base.CanHitNPC(target);
		}

		private void ManageCaches()
		{
			if (cache == null)
			{
				cache = new List<Vector2>();
				for (int i = 0; i < 20; i++)
				{
					cache.Add(Projectile.Center);
				}
			}

			cache.Add(Projectile.Center);

			while (cache.Count > 20)
			{
				cache.RemoveAt(0);
			}
		}

		private void ManageTrail()
		{
			if (trail is null || trail.IsDisposed)
				trail = new Trail(Main.instance.GraphicsDevice, 20, new NoTip(), factor => factor * 6, factor => new Color(255, 50, 180));

			trail.Positions = cache.ToArray();
			trail.NextPosition = Projectile.Center + Projectile.velocity;
		}

		public void DrawPrimitives()
		{
			Effect effect = Filters.Scene["ShrapnelTrail"].GetShader().Shader;

			var world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
			Matrix view = Main.GameViewMatrix.TransformationMatrix;
			var projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

			effect.Parameters["transformMatrix"].SetValue(world * view * projection);
			effect.Parameters["sampleTexture"].SetValue(Assets.GlowTrail.Value);
			effect.Parameters["progress"].SetValue(MathHelper.Lerp(Projectile.timeLeft / 60f, 0, 0.3f));

			trail?.Render(effect);
		}
	}

	public class FlareBreacherMuzzleFlashDust : ModDust
	{
		public override string Texture => AssetDirectory.Invisible;

		public override void OnSpawn(Dust dust)
		{
			dust.frame = new Rectangle(0, 0, 4, 4);
		}

		public override bool Update(Dust dust)
		{
			dust.alpha += 15;
			dust.alpha = (int)(dust.alpha * 1.05f);

			if (dust.alpha >= 255)
				dust.active = false;

			return false;
		}

		public override bool PreDraw(Dust dust)
		{
			float lerper = 1f - dust.alpha / 255f;

			Texture2D tex = ModContent.Request<Texture2D>(AssetDirectory.BreacherItem  + Name).Value;
			Texture2D texBlur = ModContent.Request<Texture2D>(AssetDirectory.BreacherItem + Name + "_Blur").Value;
			Texture2D texGlow = ModContent.Request<Texture2D>(AssetDirectory.BreacherItem + Name + "_Glow").Value;
			Texture2D bloomTex = ModContent.Request<Texture2D>(AssetDirectory.Keys + "GlowAlpha").Value;
			Texture2D starTex = ModContent.Request<Texture2D>(AssetDirectory.BreacherItem + "SupplyBeaconProj_Star").Value;

			Color color = Color.Lerp(new Color(255, 50, 200, 0), new Color(50, 230, 255, 0), EaseBuilder.EaseCircularInOut.Ease(1f - lerper));

			Main.spriteBatch.Draw(bloomTex, dust.position - Main.screenPosition, null, color * 0.25f * lerper, dust.rotation, bloomTex.Size() / 2f, dust.scale * 1.25f, 0f, 0f);

			Main.spriteBatch.Draw(texGlow, dust.position - Main.screenPosition, null, color * lerper, dust.rotation, texGlow.Size() / 2f, dust.scale, 0f, 0f);

			Main.spriteBatch.Draw(tex, dust.position - Main.screenPosition, null, color with { A = 255 } * lerper, dust.rotation, tex.Size() / 2f, dust.scale, 0f, 0f);

			Main.spriteBatch.Draw(texBlur, dust.position - Main.screenPosition, null, color * 0.5f * lerper, dust.rotation, texBlur.Size() / 2f, dust.scale, 0f, 0f);

			Main.spriteBatch.Draw(starTex, dust.position - Main.screenPosition, null, color * lerper, dust.rotation, starTex.Size() / 2f, dust.scale * 0.75f * EaseBuilder.EaseCircularInOut.Ease(lerper), 0f, 0f);

			return false;
		}
	}

	public class FlareBreacherStarDust : ModDust
	{
		public override string Texture => AssetDirectory.Invisible;

		public override void OnSpawn(Dust dust)
		{
			dust.frame = new Rectangle(0, 0, 4, 4);
		}

		public override bool Update(Dust dust)
		{
			Player player = dust.customData as Player;
			if (player == null)
			{
				dust.active = false;
				return false;
			}

			float itemRotation = player.compositeFrontArm.rotation + 1.5707964f * player.gravDir;
			Vector2 itemPosition = player.MountedCenter;
			dust.position = itemPosition + new Vector2(player.direction == -1 ? 3f : 1f, -10f * player.direction).RotatedBy(itemRotation);

			dust.alpha += 10;
			dust.alpha = (int)(dust.alpha * 1.05f);

			float lerper = 1f - dust.alpha / 255f;

			dust.rotation += 0.2f * lerper;

			if (dust.alpha >= 255)
				dust.active = false;

			return false;
		}

		public override bool PreDraw(Dust dust)
		{
			Player player = dust.customData as Player;
			if (player == null)
				return false;

			float lerper = 1f - dust.alpha / 255f;

			Texture2D tex = ModContent.Request<Texture2D>(AssetDirectory.BreacherItem + "SupplyBeaconProj_Star").Value; // star texture
			Texture2D bloomTex = ModContent.Request<Texture2D>(AssetDirectory.Keys + "GlowAlpha").Value;

			Main.spriteBatch.Draw(bloomTex, dust.position + new Vector2(0f, player.gfxOffY) - Main.screenPosition, null, Color.Lerp(Color.White with { A = 0 }, dust.color, 1f - lerper) * 0.5f, 0f, bloomTex.Size() / 2f, dust.scale * 2f * lerper, 0f, 0f);

			Main.spriteBatch.Draw(tex, dust.position + new Vector2(0f, player.gfxOffY) - Main.screenPosition, null, Color.Lerp(Color.White with { A = 0 }, dust.color, 1f - lerper), dust.rotation, tex.Size() / 2f, dust.scale * lerper, 0f, 0f);

			return false;
		}
	}

	public class FlareBreacherSmokeDust : ModDust
	{
		public override string Texture => AssetDirectory.Invisible;

		public override void OnSpawn(Dust dust)
		{
			dust.frame = new Rectangle(0, 0, 4, 4);
			dust.customData = 1 + Main.rand.Next(2);
			dust.rotation = Main.rand.NextFloat(6.28f);
		}

		public override bool Update(Dust dust)
		{
			dust.position.Y -= 0.1f;
			if (dust.noGravity)
				dust.position.Y -= 0.5f;

			dust.position += dust.velocity;

			if (!dust.noGravity)
			{
				dust.velocity *= 0.99f;
			}
			else
			{
				dust.velocity *= 0.975f;
				dust.velocity.X *= 0.99f;
			}

			dust.rotation += dust.velocity.Length() * 0.01f;

			if (dust.noGravity)
				dust.alpha += 2;
			else
				dust.alpha += 5;

			dust.alpha = (int)(dust.alpha * 1.005f);

			if (!dust.noGravity)
				dust.scale *= 1.02f;
			else
				dust.scale *= 0.99f;

			if (dust.alpha >= 255)
				dust.active = false;

			return false;
		}

		public override bool PreDraw(Dust dust)
		{
			float lerper = 1f - dust.alpha / 255f;

			Texture2D tex = ModContent.Request<Texture2D>(AssetDirectory.Assets + "SmokeTransparent_" + dust.customData).Value;

			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);

			Main.spriteBatch.Draw(tex, dust.position - Main.screenPosition, null, Color.Lerp(dust.color, new Color(50, 50, 50), EaseBuilder.EaseCircularIn.Ease(1f - lerper)) * lerper, dust.rotation, tex.Size() / 2f, dust.scale, 0f, 0f);

			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

			return false;
		}
	}
}