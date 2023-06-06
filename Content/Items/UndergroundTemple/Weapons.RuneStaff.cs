using StarlightRiver.Content.Archaeology;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Items.UndergroundTemple
{
	class RuneStaff : ModItem
	{
		public override string Texture => AssetDirectory.CaveTempleItem + Name;

		public override bool CanUseItem(Player player) => player.ownedProjectileCounts[ProjectileType<RuneStaffHoldout>()] <= 0;
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Shine Staff");
			Tooltip.SetDefault("Hold <left> to search for treasure\nRelease to fire stars toward the cursor");
		}

		public override void SetDefaults()
		{
			Item.DamageType = DamageClass.Magic;
			Item.mana = 1;
			Item.width = 32;
			Item.height = 32;
			Item.damage = 8;
			Item.useStyle = ItemUseStyleID.Shoot;

			Item.useTime = 2;
			Item.useAnimation = 2;
			Item.noUseGraphic = true;
			Item.noMelee = true;

			Item.knockBack = 3f;

			Item.rare = ItemRarityID.Green;
			Item.channel = true;

			Item.shoot = ProjectileType<RuneStaffHoldout>();
		}
	}

	class RuneStaffHoldout : ModProjectile
	{
		public bool playedSound;

		public bool shooting;

		public float TreasureLerp;

		public List<Vector2> oldStarPositions = new();
		public ref float Lifetime => ref Projectile.ai[2];
		public ref float PingTimer => ref Projectile.ai[1];
		public ref float StarRotation => ref Projectile.ai[0];
		public Player Owner => Main.player[Projectile.owner];
		public Vector2? OwnerMouse => (Main.myPlayer == Owner.whoAmI) ? Main.MouseWorld : null;
		public override string Texture => AssetDirectory.CaveTempleItem + "RuneStaff";

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Shine Staff");
		}

		public override void SetDefaults()
		{
			Projectile.width = 44;
			Projectile.height = 44;

			Projectile.timeLeft = 5;
			Projectile.friendly = false;
			Projectile.hostile = false;

			Projectile.tileCollide = false;
			Projectile.DamageType = DamageClass.Magic;

			Projectile.ignoreWater = true;
		}

		public override void OnSpawn(IEntitySource source)
		{
			Projectile.velocity = Owner.DirectionTo(OwnerMouse.Value);
			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;
			Projectile.netUpdate = true;
		}

		public override void AI()
		{
			Lifetime++;

			Vector2 starPos = Projectile.Center + (Projectile.rotation - MathHelper.PiOver4).ToRotationVector2() * 25f + new Vector2(0f, Owner.gfxOffY);

			if (!(Owner.HeldItem.ModItem is RuneStaff))
			{
				Projectile.Kill();
				return;
			}
				

			if ((!Owner.channel || Owner.statMana <= 15) && !shooting && Lifetime >= 30)
			{
				shooting = true;
				Projectile.timeLeft = 45;

				for (int i = 0; i < 4; i++)
				{
					for (int x = 0; x < 5; x++)
					{
						Dust.NewDustPerfect(starPos, ModContent.DustType<Dusts.GlowFastDecelerate>(), Projectile.velocity.RotatedBy(MathHelper.ToRadians(i * 90)).RotatedByRandom(0.3f) * Main.rand.NextFloat(3f), 0, new Color(175, 155, 25), 0.65f);
					}

					Projectile proj = Projectile.NewProjectileDirect(Projectile.GetSource_FromAI(), starPos, Projectile.velocity.RotatedBy(MathHelper.ToRadians(i * 90)) * 1.5f, ModContent.ProjectileType<RuneStaffProjectile>(), Projectile.damage, Projectile.knockBack, Owner.whoAmI);
					(proj.ModProjectile as RuneStaffProjectile).mousePosition = OwnerMouse.Value;
					proj.timeLeft = 240 + i * 10;
				}

				Helpers.Helper.PlayPitched("Magic/HolyCastShort", 1f, 1f, starPos);

				Owner.statMana -= 15;
				Owner.manaRegenDelay += 120;
			}

			if (shooting)
			{
				if (Projectile.timeLeft > 30)
					Projectile.Center = Owner.MountedCenter + (Projectile.rotation - MathHelper.PiOver4).ToRotationVector2() * MathHelper.Lerp(30f, 15f, EaseBuilder.EaseQuinticOut.Ease(1f - (Projectile.timeLeft - 30) / 15f));
				else
					Projectile.Center = Owner.MountedCenter + (Projectile.rotation - MathHelper.PiOver4).ToRotationVector2() * MathHelper.Lerp(15f, 30f, EaseBuilder.EaseCircularInOut.Ease(1f - Projectile.timeLeft / 30f));
			}
			else
			{
				Projectile.timeLeft = 2;
				Projectile.velocity = Vector2.Lerp(Projectile.velocity, Owner.DirectionTo(OwnerMouse.Value), 0.1f);
				Projectile.Center = Owner.MountedCenter + (Projectile.rotation - MathHelper.PiOver4).ToRotationVector2() * 30f;
			}

			TreasureLerp = FindTreasure(starPos);

			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;

			Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation - MathHelper.PiOver2 - MathHelper.PiOver4);

			Owner.heldProj = Projectile.whoAmI;
			Owner.ChangeDir(OwnerMouse.Value.X < Owner.Center.X ? -1 : 1);

			if (shooting)
				return;

			StarRotation += MathHelper.Lerp(0.001f, 0.15f, TreasureLerp);

			Lighting.AddLight(starPos, new Color(150, 150, 10).ToVector3());

			Lighting.AddLight(starPos, (new Color(255, 255, 255) * 0.5f).ToVector3());

			oldStarPositions.Add(starPos);
			if (oldStarPositions.Count > 10)
				oldStarPositions.RemoveAt(0);

			PingTimer += MathHelper.Lerp(0, 25, TreasureLerp);

			if (PingTimer >= 115)
			{
				if (!playedSound)
				{
					(Projectile.NewProjectileDirect(Projectile.GetSource_FromAI(), starPos, Vector2.Zero, ModContent.ProjectileType<RuneStaffHoldoutFlash>(), 0, 0f, Owner.whoAmI).
						ModProjectile as RuneStaffHoldoutFlash).parent = Projectile;
					playedSound = true;
				}

				if (PingTimer >= 120)
				{
					Helpers.Helper.PlayPitched("Effects/BleepLouder", 1.25f, MathHelper.Lerp(0.75f, 2.5f, TreasureLerp), starPos);
					PingTimer = 0;
					playedSound = false;
				}			
			}

			if (Lifetime % 30 == 0)
			{
				Owner.statMana -= 3;
				Owner.manaRegenDelay += 60;
			}
			
			if (Main.rand.NextBool((int)MathHelper.Lerp(20, 2, TreasureLerp)))
				Dust.NewDustPerfect(starPos, ModContent.DustType<Dusts.GlowFastDecelerate>(), Main.rand.NextVector2Circular(5f, 5f), 0, new Color(175, 155, 25), 0.5f);
		}

		private float FindTreasure(Vector2 starPos) // returns a float from 0 - 1 based on how close the nearest treasure is
		{
			float[] closestDistances = new float[4];

			var artifacts = new List<Artifact>();
			
			foreach (KeyValuePair<int, TileEntity> item in TileEntity.ByID)
			{
				if (item.Value is Artifact artifact)
					artifacts.Add(artifact);
			}

			if (artifacts.Count > 0)
			{
				Artifact nearestArtifact = artifacts.OrderBy(n => n.WorldPosition.Distance(starPos)).FirstOrDefault();
				closestDistances[0] = Vector2.Distance(nearestArtifact.WorldPosition, starPos);
			}

			Point16 topLeft = (starPos / 16).ToPoint16();
			topLeft -= new Point16(45, 45);

			List<Vector2> lifeCrystals = new();

			List<Vector2> chests = new();

			for (int x = topLeft.X; x < topLeft.X + 90; x++)
			{
				for (int y = topLeft.Y; y < topLeft.Y + 90; y++)
				{
					Tile tile = Framing.GetTileSafely(x, y);

					if (tile.TileType == TileID.Heart)
					{
						lifeCrystals.Add(new Point16(x, y).ToWorldCoordinates());
					}
					else if (TileID.Sets.BasicChest[tile.TileType])
					{
						chests.Add(new Point16(x, y).ToWorldCoordinates());
					}
				}
			}

			lifeCrystals.OrderBy(n => n.Distance(starPos));
			chests.OrderBy(n => n.Distance(starPos));

			if (lifeCrystals.Count != 0)
				closestDistances[1] = Vector2.Distance(lifeCrystals[0], starPos);
			if (chests.Count != 0)
				closestDistances[2] = Vector2.Distance(chests[0], starPos);

			List<float> finalDistances = new() { closestDistances[0], closestDistances[1], closestDistances[2] };

			//finalDistances.OrderBy(x => (int)x);

			float smallest = 750f;
			int smallestType = -1; //what treasure type the smallest distance is. 0 for artifact, 1 for crystal, 2 for chest

			for (int i = 0; i < finalDistances.Count; i++)
			{
				if (finalDistances[i] < smallest && finalDistances[i] > 0)
				{
					smallest = finalDistances[i];
					smallestType = i;
				}
			}

			float distanceToCheck = 0f;

			if (smallest < 400f) // if there is a treasure within 400 pixels, search for it.
			{
				distanceToCheck = smallest;
			}
			else //otherwise, use priority.
			{
				if (finalDistances[0] < 750f && finalDistances[0] > 0)
				{
					distanceToCheck = finalDistances[0];
				}
				else if (finalDistances[1] < 750f && finalDistances[1] > 0)
				{
					distanceToCheck = finalDistances[1];
				}
				else if (finalDistances[2] < 750f && finalDistances[2] > 0)
				{
					distanceToCheck = finalDistances[2];
				}
			}

			if (distanceToCheck <= 0f)
				return 0f;

			float lerper = 1f - distanceToCheck / 750f;

			return lerper;
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
			Texture2D texGlow = ModContent.Request<Texture2D>(Texture + "_Glow").Value;

			Texture2D starTex = ModContent.Request<Texture2D>(AssetDirectory.Assets + "StarTexture").Value;

			Texture2D bloomTex = ModContent.Request<Texture2D>(AssetDirectory.Keys + "GlowAlpha").Value;

			SpriteEffects flip = Owner.direction == -1 ? SpriteEffects.FlipHorizontally : 0f;

			float fade = 1f;
			if (Lifetime <= 5)
				fade = Lifetime / 5f;

			if (shooting && Projectile.timeLeft <= 20f)
				fade = Projectile.timeLeft / 20f;

			Main.spriteBatch.Draw(texGlow, Projectile.Center + new Vector2(0f, Owner.gfxOffY) - Main.screenPosition, null, new Color(255, 240, 170, 0) * TreasureLerp * fade, Projectile.rotation + (flip == SpriteEffects.FlipHorizontally ? MathHelper.PiOver2 : 0f), texGlow.Size() / 2f, Projectile.scale, flip, 0f);

			Main.spriteBatch.Draw(tex, Projectile.Center + new Vector2(0f, Owner.gfxOffY) - Main.screenPosition, null, lightColor * fade, Projectile.rotation + (flip == SpriteEffects.FlipHorizontally ? MathHelper.PiOver2 : 0f), tex.Size() / 2f, Projectile.scale, flip, 0f);

			Vector2 starPos = Projectile.Center + (Projectile.rotation - MathHelper.PiOver4).ToRotationVector2() * 25f + new Vector2(0f, Owner.gfxOffY);

			float starRot = Projectile.velocity.ToRotation() + StarRotation;

			if (shooting)
				fade = 0f;

			for (int i = 0; i < 10; i++)
			{
				float progress = EaseBuilder.EaseCircularInOut.Ease(i / 10f) * fade;

				if (i > 0 && i < oldStarPositions.Count)
				{
					Main.spriteBatch.Draw(bloomTex, oldStarPositions[i] - Main.screenPosition, null, new Color(150, 150, 10, 0) * 0.25f * progress, 0f, bloomTex.Size() / 2f, 0.2f, 0, 0);

					Main.spriteBatch.Draw(starTex, oldStarPositions[i] - Main.screenPosition, null, new Color(150, 150, 10, 0) * progress, starRot, starTex.Size() / 2f, 0.15f, 0f, 0f);

					Main.spriteBatch.Draw(starTex, oldStarPositions[i] - Main.screenPosition, null, new Color(255, 255, 255, 0) * progress, starRot, starTex.Size() / 2f, 0.1f, 0, 0);
				}
			}

			Main.spriteBatch.Draw(bloomTex, starPos - Main.screenPosition, null, new Color(150, 150, 10, 0) * fade, 0f, bloomTex.Size() / 2f, 0.35f, 0, 0);

			Main.spriteBatch.Draw(starTex, starPos - Main.screenPosition, null, new Color(150, 150, 10, 0) * fade, starRot, starTex.Size() / 2f, 0.3f, 0f, 0f);

			Main.spriteBatch.Draw(starTex, starPos - Main.screenPosition, null, new Color(255, 255, 255, 0) * fade, starRot, starTex.Size() / 2f, 0.2f, 0, 0);
			
			Main.spriteBatch.Draw(bloomTex, starPos - Main.screenPosition, null, new Color(255, 255, 255, 0) * 0.5f * fade, 0f, bloomTex.Size() / 2f, 0.55f, 0, 0);

			return false;
		}
	}

	class RuneStaffProjectile : ModProjectile
	{
		public bool hasHitMouse;

		public Vector2 mousePosition;
		public Player Owner => Main.player[Projectile.owner];
		public override string Texture => AssetDirectory.CaveTempleItem + "RuneStaff";
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Shine Star");
		}

		public override void SetDefaults()
		{
			Projectile.width = 16;
			Projectile.height = 16;

			Projectile.timeLeft = 240;
			Projectile.friendly = true;

			Projectile.penetrate = 1;

			Projectile.hostile = false;
			Projectile.tileCollide = true;
		}

		public override void AI()
		{
			if (Vector2.Distance(Projectile.Center, mousePosition) < 5f && Projectile.timeLeft < 225)
			{
				hasHitMouse = true;
			}

			if (Projectile.timeLeft < 225 && Projectile.timeLeft > 190 && !hasHitMouse)
			{
				Projectile.velocity += Vector2.Normalize(Projectile.Center - mousePosition) * -0.75f;
			}

			Projectile.rotation += 0.15f;

			if (Main.rand.NextBool(10))
				Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<Dusts.GlowFastDecelerate>(), Vector2.Zero, 0, new Color(175, 155, 25), 0.45f);
		}

		public override void Kill(int timeLeft)
		{
			for (int i = 0; i < 5; i++)
			{
				Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<Dusts.GlowFastDecelerate>(), Main.rand.NextVector2Circular(2.5f, 2.5f), 0, new Color(175, 155, 25), 0.45f);
			}
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D starTex = ModContent.Request<Texture2D>(AssetDirectory.Assets + "StarTexture").Value;

			Texture2D bloomTex = ModContent.Request<Texture2D>(AssetDirectory.Keys + "GlowAlpha").Value;

			Main.spriteBatch.Draw(bloomTex, Projectile.Center - Main.screenPosition, null, new Color(150, 150, 10, 0), 0f, bloomTex.Size() / 2f, 0.25f, 0, 0);

			Main.spriteBatch.Draw(starTex, Projectile.Center - Main.screenPosition, null, new Color(150, 150, 10, 0), Projectile.rotation, starTex.Size() / 2f, 0.2f, 0f, 0f);

			Main.spriteBatch.Draw(starTex, Projectile.Center - Main.screenPosition, null, new Color(255, 255, 255, 0), Projectile.rotation, starTex.Size() / 2f, 0.1f, 0f, 0f);

			Main.spriteBatch.Draw(bloomTex, Projectile.Center - Main.screenPosition, null, new Color(255, 255, 255, 0) * 0.5f, 0f, bloomTex.Size() / 2f, 0.35f, 0, 0);

			return false;
		}
	}

	class RuneStaffHoldoutFlash : ModProjectile
	{
		public Projectile parent;
		public Player Owner => Main.player[Projectile.owner];
		public override string Texture => AssetDirectory.CaveTempleItem + "RuneStaff";

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Shine Staff");
		}

		public override void SetDefaults()
		{
			Projectile.width = 32;
			Projectile.height = 32;

			Projectile.timeLeft = 15;
			Projectile.friendly = false;
			Projectile.hostile = false;

			Projectile.tileCollide = false;

			Projectile.ignoreWater = true;
		}

		public override void AI()
		{
			Projectile.Center = parent.Center + (parent.rotation - MathHelper.PiOver4).ToRotationVector2() * 25f + new Vector2(0f, Owner.gfxOffY);
			Projectile.rotation = parent.velocity.ToRotation() + parent.ai[0];
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D starTex = ModContent.Request<Texture2D>(AssetDirectory.Assets + "StarTexture").Value;

			Texture2D bloomTex = ModContent.Request<Texture2D>(AssetDirectory.Keys + "GlowAlpha").Value;

			float lerper = EaseBuilder.EaseCircularInOut.Ease(Projectile.timeLeft / 15f);

			float scale = MathHelper.Lerp(3f, 1f, EaseBuilder.EaseCircularInOut.Ease(lerper));

			Main.spriteBatch.Draw(bloomTex, Projectile.Center - Main.screenPosition, null, new Color(150, 150, 10, 0) * lerper, 0f, bloomTex.Size() / 2f, 0.35f * scale, 0, 0);

			Main.spriteBatch.Draw(starTex, Projectile.Center - Main.screenPosition, null, new Color(150, 150, 10, 0) * lerper, Projectile.rotation, starTex.Size() / 2f, 0.3f * scale, 0f, 0f);

			Main.spriteBatch.Draw(starTex, Projectile.Center - Main.screenPosition, null, new Color(255, 255, 255, 0) * lerper, Projectile.rotation, starTex.Size() / 2f, 0.2f * scale, 0f, 0f);

			Main.spriteBatch.Draw(bloomTex, Projectile.Center - Main.screenPosition, null, new Color(255, 255, 255, 0) * 0.5f * lerper, 0f, bloomTex.Size() / 2f, 0.55f * scale, 0, 0);

			return false;
		}
	}
}