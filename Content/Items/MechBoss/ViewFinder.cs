using StarlightRiver.Content.Dusts;
using StarlightRiver.Content.Projectiles;
using StarlightRiver.Core.Systems.PixelationSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.ID;

namespace StarlightRiver.Content.Items.MechBoss
{
	internal class ViewFinder : ModItem
	{
		/// <summary>
		/// Tracks targets that will have bolts called down when the main fire hits
		/// </summary>
		public List<NPC> targets = [];

		/// <summary>
		/// New targets, to be drawn by the held projectile
		/// </summary>
		public List<NPC> newTargets = [];

		public override string Texture => AssetDirectory.MechBossItem + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Viewfinder");
			Tooltip.SetDefault("<right> to place up to 3 marks\nstrike any foe with the spear to call down lasers on marked enemies");
			ItemID.Sets.Spears[Type] = true;
		}

		public override void Load()
		{
			On_Main.DrawNPCs += DrawMarks;
		}

		public override void SetDefaults()
		{
			Item.damage = 47;
			Item.useTime = 38;
			Item.useAnimation = 38;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.value = Item.gold * 6;
			Item.DamageType = DamageClass.Melee;
			Item.shootSpeed = 1;
			Item.shoot = ModContent.ProjectileType<ViewFinderProjectile>();
			Item.noUseGraphic = true;
			Item.noMelee = true;
		}

		public override bool AltFunctionUse(Player player)
		{
			return true;
		}

		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ItemID.HallowedBar, 15);
			recipe.AddIngredient(ItemID.SoulofSight, 15);
			recipe.AddTile(TileID.MythrilAnvil);
			recipe.Register();
		}

		/// <summary>
		/// Renders the marks over marked targets
		/// </summary>
		/// <param name="orig"></param>
		/// <param name="self"></param>
		/// <param name="behindTiles"></param>
		private void DrawMarks(On_Main.orig_DrawNPCs orig, Main self, bool behindTiles)
		{
			orig(self, behindTiles);

			if (!behindTiles && Main.LocalPlayer.HeldItem.ModItem is ViewFinder finder)
			{
				Texture2D soul = Terraria.GameContent.TextureAssets.Item[ItemID.SoulofSight].Value;
				var soulFrame = new Rectangle(0, 28 * (int)(Main.GameUpdateCount * 0.2f % 4), 22, 22);

				foreach (NPC target in finder.targets)
				{
					Main.spriteBatch.Draw(soul, target.Center - Main.screenPosition, soulFrame, Color.White, 0, Vector2.One * 11, 1, 0, 0);
				}
			}
		}

		/// <summary>
		/// Adds a target to the targets list and the newTargets list, as well as playing some effects
		/// </summary>
		/// <param name="player"></param>
		/// <param name="target"></param>
		public void AddTarget(Player player, NPC target)
		{
			if (targets.Count < 3)
			{
				if (target != null && !targets.Contains(target))
				{
					targets.Add(target);
					newTargets.Add(target);

					Vector2 start = player.HandPosition.Value + new Vector2(0, -75);

					SoundHelper.PlayPitched("Effects/BleepLouder", 1, 0, player.Center);
					SoundHelper.PlayPitched("Effects/Chirp1", 1, 0.5f, player.Center);

					for (int k = 0; k < 30; k++)
					{
						int duration = Main.rand.Next(30, 120);
						Vector2 mid = Vector2.Lerp(start, target.Center, 0.5f).RotatedBy(Main.rand.NextFloat(0.18f, 0.22f), start);
						ViewfinderSplineDust.Spawn(start, mid, duration, target, player, Main.rand.NextFloat(0.05f, 0.15f), new Color(100, Main.rand.Next(150, 255), 100, 0) * (1f - (duration - 30) / 90f));
					}
				}
			}
		}

		public override void UpdateInventory(Player player)
		{
			targets.RemoveAll(n => n is null || !n.active);

			foreach(NPC target in targets)
			{
				Lighting.AddLight(target.Center, new Vector3(0.2f, 0.4f, 0.2f));
			}
		}

		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
		{
			if (player.altFunctionUse == 2)
				type = ModContent.ProjectileType<ViewFinderRightProjectile>();
		}

		public override bool? UseItem(Player player)
		{
			if (player.altFunctionUse == 2)
			{
				var mouseBox = new Rectangle((int)Main.MouseWorld.X - 128, (int)Main.MouseWorld.Y - 128, 256, 256);
				foreach (NPC npc in Main.npc.Where(n => n.active && n.CanBeChasedBy() && n.Hitbox.Intersects(mouseBox)))
				{
					AddTarget(player, npc);
				}
			}

			return true;
		}
	}

	internal class ViewFinderProjectile : SpearProjectile
	{
		public override string Texture => AssetDirectory.MechBossItem + Name;

		public ViewFinderProjectile() : base(30, 60, 140)
		{
			motionFunction = Eases.EaseCircularInOut;
			fadeDuration = 10;
		}

		public override void SafeSetDefaults()
		{
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 20;
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			Player owner = Main.player[Projectile.owner];

			if (owner != null && owner.HeldItem != null && owner.HeldItem.ModItem is ViewFinder item)
			{
				foreach (NPC beamTarget in item.targets)
				{
					if (beamTarget != null && beamTarget.active)
					{
						float rot = Main.rand.NextFloat(-0.2f, 0.2f);
						Projectile.NewProjectile(null, beamTarget.Center - Vector2.UnitY.RotatedBy(rot) * 1000, Vector2.UnitY.RotatedBy(rot) * 12, ModContent.ProjectileType<ViewFinderLaser>(), item.Item.damage * 2, item.Item.knockBack, Projectile.owner);
					}
				}

				item.targets.Clear();
			}
		}

		public override bool PreDraw(ref Color lightColor)
		{
			return base.PreDraw(ref lightColor);
		}

		public override void PostDraw(Color lightColor)
		{

		}
	}

	internal class ViewFinderRightProjectile : ModProjectile
	{
		public override string Texture => AssetDirectory.Invisible;

		public override void SetDefaults()
		{
			Projectile.width = 2;
			Projectile.height = 2;
			Projectile.tileCollide = false;
			Projectile.hostile = false;
			Projectile.friendly = false;
			Projectile.penetrate = -1;
			Projectile.timeLeft = 38;
			Projectile.aiStyle = -1;
		}

		public override void AI()
		{
			Player owner = Main.player[Projectile.owner];
			owner.heldProj = Projectile.whoAmI;

			Projectile.Center = owner.Center + new Vector2(0, owner.gfxOffY);
			owner.compositeFrontArm.enabled = true;
			owner.compositeFrontArm.rotation = -1.57f * owner.direction;
			owner.compositeFrontArm.stretch = Player.CompositeArmStretchAmount.Full;

			int time = 38 - Projectile.timeLeft;
			float opacity = time < 10 ? time / 10f : time > 33 ? 1f - (time - 33) / 5f : 1;

			Vector2 pos = owner.HandPosition.Value;
			pos.Y -= time < 10 ? Eases.EaseQuadInOut(time / 10f) * 46 : 46;
			Vector2 soulPos = pos + new Vector2(-6 * owner.direction, -29);

			Lighting.AddLight(pos, new Vector3(0.4f, 0.75f, 0.4f) * opacity);

			if (Projectile.timeLeft <= 1)
			{
				if (owner != null && owner.HeldItem != null && owner.HeldItem.ModItem is ViewFinder item)
				{
					item.newTargets.Clear();
				}
			}
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Player owner = Main.player[Projectile.owner];

			Texture2D tex = Assets.Items.MechBoss.ViewFinderProjectile.Value;
			Texture2D soul = Terraria.GameContent.TextureAssets.Item[ItemID.SoulofSight].Value;

			Main.Assets.Request<Texture2D>(Terraria.GameContent.TextureAssets.Item[ItemID.SoulofSight].Name);

			var soulFrame = new Rectangle(0, 28 * (int)(Main.GameUpdateCount * 0.2f % 4), 22, 22);

			int time = 38 - Projectile.timeLeft;

			Vector2 pos = owner.HandPosition.Value - Main.screenPosition;
			pos.Y -= time < 10 ? Eases.EaseQuadInOut(time / 10f) * 46 : 46;
			Vector2 soulPos = pos + new Vector2(-6 * owner.direction, -29);

			float opacity = time < 10 ? time / 10f : time > 33 ? 1f - (time - 33) / 5f : 1;

			Main.spriteBatch.Draw(soul, soulPos, soulFrame, Color.White * opacity, 0, Vector2.One * 11, 1f, 0, 0);
			Main.spriteBatch.Draw(tex, pos, null, lightColor * opacity, 1.57f / 2f * owner.direction, tex.Size() / 2f, 1, owner.direction == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0);

			if (owner != null && owner.HeldItem != null && owner.HeldItem.ModItem is ViewFinder item)
			{
				foreach (NPC beamTarget in item.newTargets)
				{
					if (beamTarget != null && beamTarget.active)
					{
						DrawTether(soulPos, beamTarget, opacity);
					}
				}
			}

			return false;
		}

		/// <summary>
		/// Draws a spline tether between a target NPC and the top of the spear
		/// </summary>
		/// <param name="start"></param>
		/// <param name="target"></param>
		/// <param name="opacity"></param>
		public void DrawTether(Vector2 start, NPC target, float opacity)
		{
			Player owner = Main.player[Projectile.owner];
			Vector2 mid = Vector2.Lerp(start, target.Center - Main.screenPosition, 0.5f).RotatedBy(0.2f, start);
			SplineHelper.SplineData spline = new(start, mid, target.Center - Main.screenPosition);

			Texture2D tex = Assets.Masks.GlowAlpha.Value;

			int time = 38 - Projectile.timeLeft;
			float len = SplineHelper.ApproximateSplineLength(spline);

			for (int k = 0; k < len; k += 16)
			{
				float prog = k / len;

				float opMult = 1f / MathF.Abs(prog - (time / 38f));

				if (opMult < 2)
					opMult = 2;

				Vector2 pos = SplineHelper.PointOnSpline(prog, spline);
				float dir = SplineHelper.TangentOfSpline(prog, spline);
				Main.spriteBatch.Draw(tex, pos, null, new Color(1, 2, 1, 0) * opacity * opMult, dir, tex.Size() / 2f, new Vector2(0.5f, 0.1f), 0f, 0f);
			}



			Main.spriteBatch.Draw(tex, SplineHelper.PointOnSpline(time / 38f, spline), null, new Color(100, 255, 100, 0) * opacity, 0, tex.Size() / 2f, 0.2f, 0f, 0f);
		}
	}

	public class ViewfinderSplineDust : Glow
	{
		public struct ViewfinderSplineDustData
		{
			public SplineHelper.SplineData spline;
			public int duration;
			public int timer;
			public NPC target;
			public Player owner;

			public ViewfinderSplineDustData(Vector2 midPoint, int duration, NPC target, Player owner)
			{
				Vector2 startPoint = owner.HandPosition.Value + new Vector2(0, -75);
				Vector2 endPoint = target.Center;
				this.spline = new(startPoint, midPoint, endPoint);
				this.duration = duration;
				this.target = target;
				this.owner = owner;
			}
		}

		public override bool Update(Dust dust)
		{
			if (dust.customData is ViewfinderSplineDustData data)
			{
				dust.color.A = 0;
				dust.fadeIn++;

				if (data.target is null || !data.target.active)
				{
					dust.active = false;
					return false;
				}

				data.spline.StartPoint = data.owner.HandPosition.Value + new Vector2(0, -75);
				data.spline.MidPoint = Vector2.Lerp(data.spline.StartPoint, data.target.Center, 0.5f).RotatedBy(0.2f, data.spline.StartPoint);
				data.spline.EndPoint = data.target.Center;

				dust.position = SplineHelper.PointOnSpline(dust.fadeIn / data.duration, data.spline);
				Lighting.AddLight(dust.position, dust.color.ToVector3());

				if (dust.fadeIn < 20)
					dust.alpha = 255 - (int)(dust.fadeIn / 20f * 255);

				if (dust.fadeIn > data.duration - 20)
				{
					float ttd = dust.fadeIn - (data.duration - 20);
					dust.alpha = (int)(ttd / 20f * 255);
				}

				if (dust.fadeIn > data.duration)
					dust.active = false;
			}
			else
			{
				Main.NewText("Dust spawned with invalid custom data. Did you call NewDust instead of SplineGlow.Spawn?", Color.Red);
				dust.active = false;
			}

			return false;
		}

		public override bool PreDraw(Dust dust)
		{
			float lerper = 1f - dust.alpha / 255f;

			Texture2D tex = Assets.Masks.GlowAlpha.Value;

			ModContent.GetInstance<PixelationSystem>().QueueRenderAction("Dusts", () =>
			{
				Main.spriteBatch.Draw(tex, dust.position - Main.screenPosition, null, dust.color * lerper, dust.rotation, tex.Size() / 2f, dust.scale * lerper, 0f, 0f);
			});

			return false;
		}

		public static void Spawn(Vector2 position, Vector2 mid, int duration, NPC target, Player owner, float scale, Color color)
		{
			var d = Dust.NewDustPerfect(position, ModContent.DustType<ViewfinderSplineDust>(), Vector2.Zero, 0, color, scale);
			d.customData = new ViewfinderSplineDustData(mid, duration, target, owner);
		}
	}

	internal class ViewFinderLaser : ModProjectile
	{
		public override string Texture => AssetDirectory.Invisible;

		public override void SetDefaults()
		{
			Projectile.width = 48;
			Projectile.height = 48;
			Projectile.timeLeft = 1200;
			Projectile.extraUpdates = 3;
			Projectile.friendly = true;
			Projectile.tileCollide = false;
			Projectile.penetrate = 1;
			Projectile.aiStyle = -1;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = -1;
		}

		public override void AI()
		{
			Projectile.rotation = Projectile.velocity.ToRotation();
			Lighting.AddLight(Projectile.Center, new Vector3(0.3f, 0.6f, 0.3f));

			var dustColor = Color.Lerp(new Color(50, 100, 50), Color.Orange, Main.rand.NextFloat());
			dustColor.A = 0;

			Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<Dusts.PixelatedEmber>(), Projectile.velocity.RotatedByRandom(0.2f) * -Main.rand.NextFloat(), 0, dustColor, Main.rand.NextFloat(0.25f));
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			for (int k = 0; k < 16; k++)
			{
				Dust.NewDustPerfect(target.Center, ModContent.DustType<Dusts.PixelatedImpactLineDustGlow>(), Vector2.UnitX.RotatedByRandom(6.28f) * Main.rand.NextFloat(16), 0, new Color(50, Main.rand.Next(100, 255), 70, 0), Main.rand.NextFloat(0.2f, 0.3f));

				var dustColor = Color.Lerp(new Color(50, 100, 50), Color.Orange, Main.rand.NextFloat());
				dustColor.A = 0;

				Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<Dusts.PixelatedGlow>(), Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(12), 0, dustColor, Main.rand.NextFloat(0.1f, 0.4f));
			}

			SoundHelper.PlayPitched("Effects/ScanComplete", 1f, 0.25f, target.Center);
			SoundHelper.PlayPitched("Effects/Chirp2", 2f, -0.5f, target.Center);

			Projectile.active = false;
		}

		public override bool PreDraw(ref Color lightColor)
		{
			ModContent.GetInstance<PixelationSystem>().QueueRenderAction("UnderProjectiles", () =>
			{
				Texture2D tex = Assets.Masks.GlowAlpha.Value;
				Texture2D spike = Assets.Misc.SpikeTell.Value;

				var spikeFrame = new Rectangle(spike.Width / 2, 0, spike.Width / 2, spike.Height - 16);
				var opacity = Projectile.timeLeft > 1100 ? 1f - (Projectile.timeLeft - 1100) / 100f : 1f;

				Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, new Color(60, 255, 60, 0) * opacity, Projectile.rotation - 1.57f, tex.Size() / 2f, new Vector2(0.35f, 1.5f), 0, 0);
				Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, new Color(200, 255, 200, 0) * opacity, Projectile.rotation - 1.57f, tex.Size() / 2f, new Vector2(0.15f, 0.75f), 0, 0);
				Main.spriteBatch.Draw(spike, Projectile.Center - Main.screenPosition, spikeFrame, new Color(100, 255, 100, 0) * opacity, Projectile.rotation - 1.57f, new Vector2(spike.Width / 4, spike.Height - 16), new Vector2(0.3f, 8.5f), 0, 0);
			});

			return false;
		}
	}
}
