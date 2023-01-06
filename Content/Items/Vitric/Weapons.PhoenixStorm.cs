//TODO:
//Usestyle
//Make phoenix able to target NPCs
//Visuals
//Balance
//Sound effects
//Improve circling behavior
//Make the phoenixes teleport if they get too far away
//Bloom particles on hit

using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Buffs;
using StarlightRiver.Content.Buffs.Summon;
using StarlightRiver.Core;
using StarlightRiver.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.DataStructures;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;
using Terraria.Graphics.Effects;

namespace StarlightRiver.Content.Items.Vitric
{
	public class PhoenixStorm : ModItem
	{
		public int stormTimer = 0;
		public override string Texture => AssetDirectory.VitricItem + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Phoenix Storm");
			Tooltip.SetDefault("Summons a storm of phoenixes that periodically swoops in on enemies");
			ItemID.Sets.GamepadWholeScreenUseRange[Item.type] = true; // This lets the Player target anywhere on the whole screen while using a controller.
			ItemID.Sets.LockOnIgnoresCollision[Item.type] = true;
		}

		public override void SetDefaults()
		{
			Item.damage = 40;
			Item.knockBack = 3f;
			Item.mana = 10;
			Item.width = 32;
			Item.height = 32;
			Item.useTime = 36;
			Item.useAnimation = 36;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.UseSound = SoundID.Item44;

			Item.noMelee = true;
			Item.DamageType = DamageClass.Magic;
			Item.buffType = BuffType<PhoenixStormSummonBuff>();
			Item.shoot = ProjectileType<PhoenixStormMinion>();
			Item.rare = ItemRarityID.Orange;
			Item.value = Item.sellPrice(0, 2, 0, 0);
		}

		public override void UpdateInventory(Player player)
		{
			stormTimer++;
			if (player.HasMinionAttackTargetNPC)
				stormTimer++;
			if (stormTimer % 240 < 2)
			{
				List<Projectile> toSwoop = Main.projectile.Where(n => n.active && n.owner == player.whoAmI && n.ModProjectile is PhoenixStormMinion modProj && modProj.syncItem == Item).ToList();
				toSwoop.ForEach(n => (n.ModProjectile as PhoenixStormMinion).swoopDelay = Main.rand.Next(10, 40));
			}
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			// This is needed so the buff that keeps your minion alive and allows you to despawn it properly applies
			player.AddBuff(Item.buffType, 2);

			// Here you can change where the minion is spawned. Most vanilla minions spawn at the cursor position.
			position = Main.MouseWorld;

			Projectile proj = Projectile.NewProjectileDirect(source, position, velocity, type, damage, knockback, player.whoAmI);
			proj.originalDamage = damage;
			(proj.ModProjectile as PhoenixStormMinion).syncItem = Item;
			return false;
		}

		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ItemID.Book, 1);
			recipe.AddIngredient(ItemID.HellstoneBar, 10);
			recipe.AddIngredient(ModContent.ItemType<MagmaCore>(), 1);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}
	}

	public class PhoenixStormMinion : ModProjectile
	{
		private readonly int AFTER_IMAGE_LENGTH = 6;


		public Item syncItem;
		public int swoopDelay = -1;
		private float circleCounter = 0;
		private bool swooping = false;
		private float swoopSpeed = 0.1f;

		private List<NPC> alreadyHit = new List<NPC>();

		private List<Vector2> oldPos = new List<Vector2>();
		private List<float> oldRot = new List<float>();

		private List<Vector2> swoopTrajectory = new List<Vector2>();

		private NPC target;

		private Trail trail;
		private List<Vector2> cache;

		private Player player => Main.player[Projectile.owner];

		public override string Texture => AssetDirectory.VitricItem + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Phoenix");
			Main.projFrames[Projectile.type] = 7;

			Main.projPet[Projectile.type] = true; // Denotes that this Projectile is a pet or minion
			ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true; // This is necessary for right-click targeting
			ProjectileID.Sets.MinionSacrificable[Projectile.type] = true; // This is needed so your minion can properly spawn when summoned and replaced when other minions are summoned			 
			ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true; // Don't mistake this with "if this is true, then it will automatically home". It is just for damage reduction for certain NPCs
		}

		public sealed override void SetDefaults()
		{
			Projectile.width = 32;
			Projectile.height = 32;

			Projectile.tileCollide = false;

			Projectile.friendly = true;

			Projectile.minion = true;

			Projectile.minionSlots = 0.5f;

			Projectile.penetrate = 1;
			swoopSpeed = Main.rand.NextFloat(0.025f, 0.055f);
		}

		public override bool? CanCutTiles()
		{
			return false;
		}

		public override void AI()
		{
			#region Active check
			if (player.dead || !player.active) // This is the "active check", makes sure the minion is alive while the Player is alive, and despawns if not
				player.ClearBuff(BuffType<PhoenixStormSummonBuff>());

			if (player.HasBuff(BuffType<PhoenixStormSummonBuff>()))
				Projectile.timeLeft = 2;
			#endregion

			#region Find Target
			if (player.HasMinionAttackTargetNPC)
				target = Main.npc[player.MinionAttackTargetNPC];
			else
				target = Main.npc.Where(n => n.active && n.Distance(player.Center) < 800 && n.CanBeChasedBy(Projectile)).OrderBy(n => n.Distance(player.Center)).FirstOrDefault();

			#endregion

			#region trail stuff
			ManageCaches();
			ManageTrail();
			#endregion

			if (swooping)
			{
				if (Main.rand.NextBool(2))
					Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(20, 20), ModContent.DustType<Dusts.Glow>(), Main.rand.NextVector2Circular(2, 2), 0, Color.Lerp(Color.Orange, Color.Red, Main.rand.NextFloat()), 0.45f);
				
				if (swoopTrajectory.Count == 0)
				{
					swooping = false;
					return;
				}
				while (Projectile.Distance(swoopTrajectory[0]) < 31)
				{
					swoopTrajectory.RemoveAt(0);
					if (swoopTrajectory.Count == 0)
					{
						swooping = false;
						return;
					}
				}

				float progress = 1 - ((10 - swoopTrajectory.Count()) / 10f);
				Projectile.velocity = Projectile.DirectionTo(swoopTrajectory[0]) * (1 + ((0.5f - MathF.Abs(progress - 0.5f)) * 60));
				Projectile.rotation = Projectile.velocity.ToRotation();
				Projectile.frame = Main.projFrames[Projectile.type] - 1;

				oldPos.Add(Projectile.Center);
				oldRot.Add(Projectile.rotation);

				if (oldPos.Count() > AFTER_IMAGE_LENGTH)
				{
					oldPos.RemoveAt(0);
					oldRot.RemoveAt(0);
				}
			}
			else
			{
				Projectile.frameCounter++;
				if (Projectile.frameCounter % 3 == 0)
					Projectile.frame++;
				Projectile.frame %= Main.projFrames[Projectile.type] - 1;

					swoopDelay--;
				if (swoopDelay == 0)
					Swoop();

				circleCounter += swoopSpeed;
				Vector2 posToBe = player.Center;
				if (target != default)
					posToBe = target.Center;

				posToBe -= new Vector2(0, 200);
				Vector2 posOffset = circleCounter.ToRotationVector2();
				posOffset.X *= 100;
				posOffset.Y *= 37;
				posToBe += posOffset;

				Projectile.velocity = Projectile.DirectionTo(posToBe) * MathF.Sqrt(Projectile.Distance(posToBe)) * 0.35f;
				Projectile.velocity.Y += MathF.Cos(circleCounter * 3.9f) * 2;
				Projectile.rotation = 0;
			}
		}

		public override bool? CanHitNPC(NPC target)
		{
			if (alreadyHit.Contains(target))
				return false;
			return base.CanHitNPC(target);
		}

		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
		{ 
			for (int i = 0; i < 12; i++)
				Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(20, 20), ModContent.DustType<Dusts.Glow>(), Main.rand.NextVector2Circular(5, 5), 0, Color.Lerp(Color.Orange, Color.Red, Main.rand.NextFloat()), 0.55f);
			alreadyHit.Add(target);
			target.AddBuff(BuffID.OnFire, 240);
			Projectile.penetrate++;
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
			Texture2D whiteTex = Request<Texture2D>(Texture + "_White").Value;
			Texture2D glowTex = Request<Texture2D>(Texture + "_Glow").Value;
			int frameHeight = tex.Height / Main.projFrames[Projectile.type];
			Rectangle frame = new Rectangle(0, frameHeight * Projectile.frame, tex.Width, frameHeight);
			
			SpriteEffects spriteEffects = SpriteEffects.FlipHorizontally;
			float rotation = Projectile.rotation;
			if (Projectile.velocity.X < 0)
			{
				spriteEffects = SpriteEffects.None;
			}
			
			if (player.HasMinionAttackTargetNPC || swooping)
			{
				int glowFrameHeight = glowTex.Height / Main.projFrames[Projectile.type];
				Rectangle glowFrame = new Rectangle(0, glowFrameHeight * Projectile.frame, glowTex.Width, glowFrameHeight);
				Color glowColor = Color.OrangeRed;
				glowColor.A = 0;
				Main.spriteBatch.Draw(glowTex, Projectile.Center - Main.screenPosition, glowFrame, glowColor, rotation, glowFrame.Size() / 2, Projectile.scale * 1.2f, spriteEffects, 0f);
			}

			if (swooping)
			{
				Main.spriteBatch.End();
				DrawTrail();
				Main.spriteBatch.Begin(default, BlendState.Additive, default, default, default, default, Main.GameViewMatrix.TransformationMatrix);
				for (int i = 0; i < oldPos.Count(); i++)
				{
					float opacity = i / (float)oldPos.Count();
					Color color = Color.Lerp(Color.Orange, Color.Red, opacity);
					Main.spriteBatch.Draw(tex, oldPos[i] - Main.screenPosition, frame, color * opacity, oldRot[i], frame.Size() / 2, Projectile.scale, spriteEffects, 0f);
				}

				Main.spriteBatch.End();
				Main.spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.TransformationMatrix);
			}
			Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, frame, lightColor, rotation, frame.Size() / 2, Projectile.scale, spriteEffects, 0f);
			return false;
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
			trail ??= new Trail(Main.instance.GraphicsDevice, 10, new TriangularTip(4), factor => 13 * factor, factor =>
			{
				return Color.OrangeRed;
			});

			trail.Positions = cache.ToArray();
			trail.NextPosition = Projectile.Center + Projectile.velocity;
		}

		public override bool MinionContactDamage() => swooping;

		private void Swoop()
		{
			oldPos.Clear();
			oldRot.Clear();
			alreadyHit.Clear();
			if (target == default || swooping)
				return;

			Vector2 endPoint = Projectile.Center + new Vector2((target.Center.X - Projectile.Center.X) * 3, -70) + Main.rand.NextVector2Circular(50, 25);
			Vector2 control1 = new Vector2(Projectile.Center.X, target.Center.Y + 100) + Main.rand.NextVector2Circular(50, 55);
			Vector2 control2 = new Vector2(endPoint.X, target.Center.Y + 100) + Main.rand.NextVector2Circular(50, 55);
			BezierCurve curve = new BezierCurve(Projectile.Center, control1, control2, endPoint);
			swoopTrajectory = curve.GetPoints(10);
			swooping = true;
		}

		private void DrawTrail()
		{
			Effect effect = Filters.Scene["CeirosRing"].GetShader().Shader;

			var world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
			Matrix view = Main.GameViewMatrix.ZoomMatrix;
			var projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

			effect.Parameters["time"].SetValue(Main.GameUpdateCount);
			effect.Parameters["repeats"].SetValue(2f);
			effect.Parameters["transformMatrix"].SetValue(world * view * projection);
			effect.Parameters["sampleTexture"].SetValue(Request<Texture2D>("StarlightRiver/Assets/FireTrail").Value);

			trail?.Render(effect);
		}
	}

	public class PhoenixStormSummonBuff : SmartBuff
	{
		public override string Texture => AssetDirectory.VitricItem + Name;

		public PhoenixStormSummonBuff() : base("Phoenix Minion", "A fiery bird follows you", false, true) { }

		public override void Update(Player Player, ref int buffIndex)
		{
			if (Player.ownedProjectileCounts[ModContent.ProjectileType<PhoenixStormMinion>()] > 0)
			{
				Player.buffTime[buffIndex] = 18000;
			}
			else
			{
				Player.DelBuff(buffIndex);
				buffIndex--;
			}
		}
	}
}