using StarlightRiver.Core.Systems.CameraSystem;
using StarlightRiver.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.GameContent;
using Terraria.Graphics.Effects;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Breacher
{
	public class Scrapshot : ModItem
	{
		public ScrapshotHook hook;

		public override string Texture => AssetDirectory.BreacherItem + Name;

		public override bool AltFunctionUse(Player Player)
		{
			return true;
		}

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Scrapshot");
			Tooltip.SetDefault("Right click to hook your enemies and pull closer\nFire while hooked to reduce spread and go flying");
		}

		public override void SetDefaults()
		{
			Item.width = 24;
			Item.height = 28;
			Item.damage = 9;
			Item.useAnimation = 30;
			Item.useTime = 30;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.knockBack = 2f;
			Item.rare = ItemRarityID.Orange;
			Item.value = Item.sellPrice(0, 10, 0, 0);
			Item.noMelee = true;
			Item.useAmmo = AmmoID.Bullet;
			Item.DamageType = DamageClass.Ranged;
			Item.shoot = ProjectileID.None;
			Item.shootSpeed = 17;
		}

		public override Vector2? HoldoutOffset()
		{
			return new Vector2(-15, 0);
		}

		public override bool CanUseItem(Player Player)
		{
			if (Player.altFunctionUse == 2)
			{
				Item.useTime = 14;
				Item.useAnimation = 14;
				Item.noUseGraphic = true;

				return !Main.projectile.Any(n => n.active && n.owner == Player.whoAmI && n.type == ModContent.ProjectileType<ScrapshotHook>());
			}
			else
			{
				Item.useTime = 30;
				Item.useAnimation = 30;
				Item.noUseGraphic = false;

				if (Main.netMode == NetmodeID.MultiplayerClient && Main.myPlayer != Player.whoAmI && (hook == null || !hook.Projectile.active || hook.Projectile.type != ModContent.ProjectileType<ScrapshotHook>() || hook.Projectile.owner != Player.whoAmI))
					FindHook(Player);

				return hook is null || hook != null && (!hook.Projectile.active || hook.Projectile.type != ModContent.ProjectileType<ScrapshotHook>() || hook.isHooked && !hook.struck);
			}
		}

		public override void UseStyle(Player Player, Rectangle rect)
		{
			//only know rotation for self
			if (Player.whoAmI == Main.myPlayer)
			{
				if (Player.altFunctionUse != 2)
				{
					Player.direction = (Player.Center - Main.MouseWorld).ToRotation().ToRotationVector2().X < 0 ? 1 : -1;
					Player.itemRotation = (Player.Center - Main.MouseWorld).ToRotation() + (Player.direction == 1 ? 3.14f : 0);
				}
			}
		}

		public override bool? UseItem(Player player)
		{
			//even though this is a "gun" we are using useItem so that it runs on all clients. need to deconstruct the useammo and damage modifiers ourselves here

			int damage = 0;
			float speed = Item.shootSpeed;
			float speedX = 0;
			float speedY = 0;
			float knockback = 0;

			if (Main.myPlayer == player.whoAmI)
			{
				damage = (int)(Item.damage * player.GetDamage(DamageClass.Ranged).Multiplicative);
				float rotation = (player.Center - Main.MouseWorld).ToRotation() - 1.57f;
				speedX = speed * (float)Math.Sin(rotation);
				speedY = speed * -(float)Math.Cos(rotation);
				knockback = Item.knockBack;
			}

			if (player.altFunctionUse == 2)
			{
				if (Main.myPlayer == player.whoAmI)
				{
					int i = Projectile.NewProjectile(player.GetSource_ItemUse(Item), player.Center, new Vector2(speedX, speedY), ModContent.ProjectileType<ScrapshotHook>(), Item.damage, (int)Item.knockBack, player.whoAmI);
					hook = Main.projectile[i].ModProjectile as ScrapshotHook;
				}

				Helper.PlayPitched("Guns/ChainShoot", 0.5f, 0, player.Center);
			}
			else
			{
				float spread = 0.5f;

				int type = ProjectileID.Bullet;
				var sample = new Item();
				sample.SetDefaults(type);
				sample.useAmmo = AmmoID.Bullet;

				bool shoot = player.HasAmmo(player.HeldItem);

				if (!shoot)
					return false;

				if (type == ProjectileID.Bullet)
					type = ModContent.ProjectileType<ScrapshotShrapnel>();

				if (Main.myPlayer == player.whoAmI)
					CameraSystem.shake += 8;

				if (hook != null && hook.Projectile.type == ModContent.ProjectileType<ScrapshotHook>() && hook.Projectile.active && hook.isHooked)
				{
					hook.struck = true;

					NPC hooked = Main.npc[hook.hookedNpcIndex];
					hook.Projectile.timeLeft = 20;
					player.velocity = Vector2.Normalize(hook.startPos - hooked.Center) * 12;

					Helper.PlayPitched("ChainHit", 0.5f, 0, player.Center);

					for (int k = 0; k < 20; k++)
					{
						Vector2 direction = Vector2.One.RotatedByRandom(6.28f);
						Dust.NewDustPerfect(player.Center + direction * 10, ModContent.DustType<Dusts.Glow>(), direction * Main.rand.NextFloat(2, 4), 0, new Color(150, 80, 40), Main.rand.NextFloat(0.2f, 0.5f));
					}

					if (Main.myPlayer == player.whoAmI)
					{
						spread = 0.05f;
						damage += 4;

						CameraSystem.shake += 12;
					}
				}

				float rot = new Vector2(speedX, speedY).ToRotation();

				if (Main.myPlayer != player.whoAmI)
					hook = null;

				for (int k = 0; k < 6; k++)
				{
					Vector2 offset = Vector2.UnitX.RotatedBy(rot);
					Vector2 direction = offset.RotatedByRandom(spread);

					if (Main.myPlayer == player.whoAmI)
					{
						int i = Projectile.NewProjectile(player.GetSource_ItemUse_WithPotentialAmmo(Item, Item.useAmmo), player.Center + offset * 25, direction * Item.shootSpeed, type, damage, (int)Item.knockBack, player.whoAmI);

						if (type != ModContent.ProjectileType<ScrapshotShrapnel>())
							Main.projectile[i].timeLeft = 30;

						//don't know direction for other Players so we only add these for self.
						Dust.NewDustPerfect(player.Center + direction * 60, ModContent.DustType<Dusts.Glow>(), direction * Main.rand.NextFloat(20), 0, new Color(150, 80, 40), Main.rand.NextFloat(0.2f, 0.5f));
						Dust.NewDustPerfect(player.Center + direction * 60, ModContent.DustType<Dusts.Smoke>(), Vector2.UnitY * -2 + direction * 5, 0, new Color(60, 55, 50) * 0.5f, Main.rand.NextFloat(0.5f, 1));
					}
				}

				Helper.PlayPitched("Guns/Scrapshot", 0.4f, 0, player.Center);
			}

			return true;
		}

		public override bool CanConsumeAmmo(Item ammo, Player Player)
		{
			return Player.altFunctionUse != 2;
		}

		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ItemID.Boomstick);
			recipe.AddIngredient(ModContent.ItemType<Content.Items.SpaceEvent.Astroscrap>(), 5);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();

			recipe = CreateRecipe();
			recipe.AddIngredient(ItemID.IllegalGunParts);
			recipe.AddIngredient(ModContent.ItemType<Content.Items.SpaceEvent.Astroscrap>(), 5);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}

		/// <summary>
		/// we are using the precondition that only 1 scrapshot hook can exist for a Player in order to find and assign the hook in multiPlayer
		/// </summary>
		/// <returns></returns>
		private void FindHook(Player Player)
		{
			for (int i = 0; i < Main.maxProjectiles; i++)
			{
				Projectile proj = Main.projectile[i];

				if (proj.active && proj.owner == Player.whoAmI && proj.type == ModContent.ProjectileType<ScrapshotHook>())
				{
					hook = proj.ModProjectile as ScrapshotHook;
					return;
				}
			}
		}
	}

	public class ScrapshotHook : ModProjectile
	{
		public bool isHooked = false;
		public Vector2 startPos;
		public bool struck;

		public int timer;

		ref float Progress => ref Projectile.ai[0];
		ref float Distance => ref Projectile.ai[1];
		bool Retracting => Projectile.timeLeft < 30;

		Player Player => Main.player[Projectile.owner];

		public byte hookedNpcIndex = 0;

		public override string Texture => AssetDirectory.BreacherItem + Name;

		public override void SetDefaults()
		{
			Projectile.width = 16;
			Projectile.height = 16;
			Projectile.friendly = true;
			Projectile.timeLeft = 60;
			Projectile.aiStyle = -1;
			Projectile.penetrate = -1;
		}

		private void FindIfHit()
		{
			foreach (NPC NPC in Main.npc.Where(n => n.active && !n.dontTakeDamage && !n.townNPC && n.life > 0 && n.Hitbox.Intersects(Projectile.Hitbox)))
			{
				if (Player.HeldItem.ModItem is Scrapshot)
				{
					Player.itemAnimation = 1;
					Player.itemTime = 1;
				}

				isHooked = true;
				Projectile.velocity *= 0;
				startPos = Player.Center;
				Distance = Vector2.Distance(startPos, NPC.Center);
				hookedNpcIndex = (byte)NPC.whoAmI;
			}
		}

		public override void AI()
		{
			Projectile.rotation = Projectile.velocity.ToRotation();

			if (Projectile.timeLeft < 40)//slows down the Projectile by 8%, for about 10 ticks before it retracts
				Projectile.velocity *= 0.92f;

			if (Projectile.timeLeft == 30)
			{
				startPos = Projectile.Center;
				Projectile.velocity *= 0;
			}

			if (Retracting)
				Projectile.Center = Vector2.Lerp(Player.Center, startPos, Projectile.timeLeft / 30f);

			if (!isHooked && !Retracting && Main.myPlayer != Projectile.owner)
			{
				Projectile.friendly = true; //otherwise it will stop just short of actually intersecting the hitbox
				FindIfHit(); //since onhit hooks are client side only, all other clients will manually check for collisions
			}

			if (isHooked && !struck)
			{
				timer++;
				NPC hooked = Main.npc[hookedNpcIndex];
				Player.direction = startPos.X > hooked.Center.X ? -1 : 1;

				if (timer == 1)
					Helper.PlayPitched("Guns/ChainPull", 1, 0, Player.Center);

				if (timer < 10)
				{
					Player.velocity *= 0.96f;
					return;
				}

				if (timer >= 10)
					startPos = Player.Center;

				Projectile.timeLeft = 52;

				if (Vector2.Distance(Projectile.Center, hooked.Center) > 128 || Player.dead) //break the hook if the enemy is too fast or teleports, or if the Player is dead
				{
					hooked = null;
					Projectile.timeLeft = 30;
					return;
				}

				Projectile.Center = hooked.Center;

				Progress += 10f / Distance * (0.8f + Progress * 1.5f);

				if (Player.velocity.Y == 0 && hooked.Center.Y > Player.Center.Y)
					Player.Center = new Vector2(Vector2.Lerp(startPos, hooked.Center, Progress).X, Player.Center.Y);
				else
					Player.Center = Vector2.Lerp(startPos, hooked.Center, Progress);

				Player.velocity *= 0;

				if (Player.Hitbox.Intersects(hooked.Hitbox) || Progress > 1)
				{
					struck = true;
					Projectile.timeLeft = 20;

					Player.immune = true;
					Player.immuneTime = 20;
					Player.velocity = Vector2.Normalize(startPos - hooked.Center) * 15;
					CameraSystem.shake += 15;

					hooked.SimpleStrikeNPC(Projectile.damage, Player.Center.X < hooked.Center.X ? -1 : 1);
					Helper.PlayPitched("Guns/ChainPull", 0.001f, 0, Player.Center);
				}
			}

			if (struck)
			{
				Player.fullRotation = Projectile.timeLeft / 20f * 3.14f * Player.direction;
				Player.fullRotationOrigin = Player.Size / 2;
				Player.velocity *= 0.95f;

				if (Projectile.timeLeft == 1)
					Player.fullRotation = 0;
			}
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			if (target.life <= 0 || Retracting)
				return;

			if (Player.HeldItem.ModItem is Scrapshot)
			{
				Player.itemAnimation = 1;
				Player.itemTime = 1;
			}

			hookedNpcIndex = (byte)target.whoAmI;
			isHooked = true;
			Projectile.velocity *= 0;
			startPos = Player.Center;
			Distance = Vector2.Distance(startPos, target.Center);
			Projectile.friendly = false;
		}

		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
		{
			modifiers.FinalDamage *= 0.25f;
			modifiers.Knockback *= 0.25f;
			modifiers.DisableCrit();
		}

		public override bool PreDraw(ref Color lightColor)
		{
			if (struck)
				return false;

			Texture2D chainTex1 = ModContent.Request<Texture2D>(AssetDirectory.BreacherItem + "ScrapshotHookChain1").Value;
			Texture2D chainTex2 = ModContent.Request<Texture2D>(AssetDirectory.BreacherItem + "ScrapshotHookChain2").Value;
			Player Player = Main.player[Projectile.owner];

			float dist = Vector2.Distance(Player.Center, Projectile.Center);
			float rot = (Player.Center - Projectile.Center).ToRotation() + (float)Math.PI / 2f;

			float length = 1f / dist * chainTex1.Height;

			for (int k = 0; k * length < 1; k++)
			{
				var pos = Vector2.Lerp(Projectile.Center, Player.Center, k * length);

				if (k % 2 == 0)
					Main.spriteBatch.Draw(chainTex1, pos - Main.screenPosition, null, lightColor, rot, chainTex1.Size() / 2, 1, 0, 0);
				else
					Main.spriteBatch.Draw(chainTex2, pos - Main.screenPosition, null, lightColor, rot, chainTex1.Size() / 2, 1, 0, 0);
			}

			Texture2D hook = TextureAssets.Projectile[Projectile.type].Value;

			Main.spriteBatch.Draw(hook, Projectile.Center - Main.screenPosition, null, lightColor, rot + (float)Math.PI * 0.75f, hook.Size() / 2, 1, 0, 0);

			return false;
		}
	}

	public class ScrapshotShrapnel : ModProjectile, IDrawPrimitive
	{
		private List<Vector2> cache;
		private Trail trail;

		public override string Texture => AssetDirectory.BreacherItem + "ExplosiveFlare";

		public override void SetDefaults()
		{
			Projectile.width = 16;
			Projectile.height = 16;
			Projectile.DamageType = DamageClass.Ranged;
			Projectile.friendly = true;
			Projectile.penetrate = 1;
			Projectile.timeLeft = 100;
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
			if (Projectile.timeLeft == 100)
				Projectile.velocity *= Main.rand.NextFloat(1.5f, 2);

			Projectile.velocity *= 0.95f;

			if (Main.netMode != NetmodeID.Server)
			{
				ManageCaches();
				ManageTrail();
			}
		}

		private void ManageCaches()
		{
			if (cache == null)
			{
				cache = new List<Vector2>();
				for (int i = 0; i < 10; i++)
				{
					cache.Add(Projectile.Center);
				}
			}

			cache.Add(Projectile.Center);

			while (cache.Count > 10)
			{
				cache.RemoveAt(0);
			}
		}

		private void ManageTrail()
		{
			trail ??= new Trail(Main.instance.GraphicsDevice, 10, new TriangularTip(40 * 4), factor => factor * 5, factor => new Color(255, 170, 80) * factor.X * (Projectile.timeLeft / 100f));

			trail.Positions = cache.ToArray();
			trail.NextPosition = Projectile.Center;
		}

		public void DrawPrimitives()
		{
			Effect effect = Filters.Scene["CeirosRing"].GetShader().Shader;

			var world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
			Matrix view = Main.GameViewMatrix.TransformationMatrix;
			var projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

			effect.Parameters["time"].SetValue(Main.GameUpdateCount * 0.05f);
			effect.Parameters["repeats"].SetValue(2f);
			effect.Parameters["transformMatrix"].SetValue(world * view * projection);
			effect.Parameters["sampleTexture"].SetValue(ModContent.Request<Texture2D>("StarlightRiver/Assets/GlowTrail").Value);

			trail?.Render(effect);
		}
	}
}