using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System.Linq;
using System;
using System.Collections.Generic;
using StarlightRiver.Core;
using static Terraria.ModLoader.ModContent;
using StarlightRiver.Content.Buffs.Summon;
using Microsoft.Xna.Framework.Graphics;

namespace StarlightRiver.Content.Items.Palestone
{
    public class PalestoneNail : ModItem
    {
        public override string Texture => AssetDirectory.PalestoneItem + Name;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Palenail");
            Tooltip.SetDefault("Summons the little man.");
            ItemID.Sets.GamepadWholeScreenUseRange[item.type] = true; // This lets the player target anywhere on the whole screen while using a controller.
            ItemID.Sets.LockOnIgnoresCollision[item.type] = true;
        }

        public override void SetDefaults()
        {
            item.damage = 7;
            item.knockBack = 3f;
            item.mana = 10;
            item.width = 32;
            item.height = 32;
            item.useTime = 36;
            item.useAnimation = 36;
            item.useStyle = ItemUseStyleID.SwingThrow;
            item.value = Item.buyPrice(0, 0, 12, 0);
            item.rare = ItemRarityID.White;
            item.UseSound = SoundID.Item44;

            item.noMelee = true;
            item.summon = true;
            item.buffType = BuffType<PalestoneSummonBuff>();
			item.shoot = ProjectileType<PaleKnight>();
        }

        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            // This is needed so the buff that keeps your minion alive and allows you to despawn it properly applies
            player.AddBuff(item.buffType, 2);

            // Here you can change where the minion is spawned. Most vanilla minions spawn at the cursor position.
            position = Main.MouseWorld;
            return true;
        }

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.UlyssesButterfly, 25);
            recipe.AddTile(TileID.UlyssesButterflyJar);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
    }

    public class PaleKnight : ModProjectile
    {
		public override string Texture => AssetDirectory.PalestoneItem + "PalestoneKnight";
		public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Tiny Killer");
            Main.projFrames[projectile.type] = 20;

			Main.projPet[projectile.type] = true; // Denotes that this projectile is a pet or minion
			ProjectileID.Sets.MinionTargettingFeature[projectile.type] = true; // This is necessary for right-click targeting
			ProjectileID.Sets.MinionSacrificable[projectile.type] = true; // This is needed so your minion can properly spawn when summoned and replaced when other minions are summoned			 
			ProjectileID.Sets.Homing[projectile.type] = true; // Don't mistake this with "if this is true, then it will automatically home". It is just for damage reduction for certain NPCs
		}

        public sealed override void SetDefaults()
        {
            projectile.width = 18;
            projectile.height = 28;

            projectile.tileCollide = true;

            //projectile.friendly = true;

            projectile.minion = true;

            projectile.minionSlots = 1f;

            projectile.penetrate = -1;
        }

		public override bool? CanCutTiles() => false;
		//public override bool MinionContactDamage() => true;	// This is mandatory if your minion deals contact damage (further related stuff in AI() in the Movement region)

		const int RunFrames = 8;

		const int FlyFrames = 6;
		const int FlyOffset = RunFrames;

		const int AttackFrames = 6;
		const int AttackOffset = FlyFrames + RunFrames;

        public override void PostDraw(SpriteBatch spriteBatch, Color lightColor)
        {
			Utils.DrawBorderString(spriteBatch, projectile.frame.ToString(), projectile.position + new Vector2(0, -60) - Main.screenPosition, Color.LightCoral);
			Utils.DrawBorderString(spriteBatch, projectile.ai[0].ToString(), projectile.position + new Vector2(0, -40) - Main.screenPosition, Color.LightGoldenrodYellow);
			Utils.DrawBorderString(spriteBatch, projectile.velocity.Length().ToString(), projectile.position + new Vector2(0, -20) - Main.screenPosition, Color.LightBlue);
		}

		public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough)
        {
			return true;
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit) => 
			projectile.ai[1] = target.whoAmI;//TODO: possible desync, needs testing

		const float defaultFrameSpeed = 10f;
		const float defaultRunSpeed = 8f;
		const float tiltAmount = 0.02f;
		const float enemyCheckRadius = 700f;
		const int validZoneSize = 24;
		const int minionSpacing = 32;
		const int maxMinionChaseRange = 2000;

		public override void AI()
		{
			Player player = Main.player[projectile.owner];
			PaleKnightPlayer modPlayer = player.GetModPlayer<PaleKnightPlayer>();

			#region Active check
			if (player.dead || !player.active) // This is the "active check", makes sure the minion is alive while the player is alive, and despawns if not
				player.ClearBuff(BuffType<PalestoneSummonBuff>());
			if (player.HasBuff(BuffType<PalestoneSummonBuff>()))
				projectile.timeLeft = 2;
			#endregion

			#region General behavior

			#endregion

			#region Find target
			// Starting search distance
			float distanceFromTarget = enemyCheckRadius;
			Vector2 targetCenter = projectile.position;
			bool foundTarget = projectile.ai[1] >= 0;

			// This code is required if your minion weapon has the targeting feature
			if (player.HasMinionAttackTargetNPC)
			{
				NPC npc = Main.npc[player.MinionAttackTargetNPC];
				float between = Vector2.Distance(npc.Center, projectile.Center);
				// Reasonable distance away so it doesn't target across multiple screens
				if (between < maxMinionChaseRange)
				{
					distanceFromTarget = between;
					targetCenter = npc.Center;
					projectile.ai[1] = npc.whoAmI;
					foundTarget = true;
				}
			}
            else if (foundTarget)
            {
				NPC npc = Main.npc[(int)projectile.ai[1]];
				float between = Vector2.Distance(npc.Center, player.Center);
				if (Helpers.Helper.IsTargetValid(npc) && npc.CanBeChasedBy() && between < maxMinionChaseRange)
				{
					distanceFromTarget = Vector2.Distance(npc.Center, projectile.Center);
					targetCenter = npc.Center;
				}
				else
				{
					projectile.ai[1] = -1;
					foundTarget = false;
				}
			}

			if(!foundTarget)
			{
				if (projectile.ai[1] < -10)
				{
					// This code is required either way, used for finding a target
					for (int i = 0; i < Main.maxNPCs; i++)
					{
						NPC npc = Main.npc[i];
						if (npc.CanBeChasedBy())
						{
							float between = Vector2.Distance(npc.Center, projectile.Center);
							bool closest = Vector2.Distance(projectile.Center, targetCenter) > between;
							bool inRange = between < distanceFromTarget;
							bool lineOfSight = Collision.CanHitLine(projectile.position, projectile.width, projectile.height, npc.position, npc.width, npc.height);
							// Additional check for this specific minion behavior, otherwise it will stop attacking once it dashed through an enemy while flying though tiles afterwards
							// The number depends on various parameters seen in the movement code below. Test different ones out until it works alright
							bool closeThroughWall = between < 100f;
							if (((closest && inRange) || !foundTarget) && (lineOfSight || closeThroughWall))
							{
								distanceFromTarget = between;
								targetCenter = npc.Center;
								projectile.ai[1] = npc.whoAmI;
								foundTarget = true;
							}
						}
					}
				}
				else
					projectile.ai[1]--;
			}

			// friendly needs to be set to true so the minion can deal contact damage
			// friendly needs to be set to false so it doesn't damage things like target dummies while idling
			// Both things depend on if it has a target or not, so it's just one assignment here
			// You don't need this assignment if your minion is shooting things instead of dealing contact damage
			//projectile.friendly = foundTarget;
			#endregion

			#region Movement
			Rectangle validZone;
            if (!foundTarget)
            {
				Vector2 pos = player.Center + new Vector2((((validZoneSize * 2) + modPlayer.KnightCount * minionSpacing) * player.direction), 16);
				validZone = new Rectangle((int)(pos.X - validZoneSize), (int)(pos.Y - validZoneSize), validZoneSize * 2, validZoneSize * 2);
				for(int i = 0; i < 10; i++)
					Dust.NewDustPerfect(validZone.TopLeft() + new Vector2(Main.rand.Next(validZone.Width), Main.rand.Next(validZone.Height)), DustType<Dusts.AirDash>(), Vector2.Zero, 0, Color.Blue);
				Dust.NewDustPerfect(validZone.Center(), DustType<Dusts.AirDash>(), Vector2.Zero, 0, Color.Red);
			}

			if (Vector2.Distance(projectile.position, player.position) > 200f && !foundTarget)
				projectile.ai[0] = 1;
			else if (projectile.ai[0] == 1)
				projectile.ai[0] = 0;

			projectile.velocity.Y += 0.35f;
			if (projectile.ai[0] == 1)
				projectile.velocity += Vector2.Normalize(player.position - projectile.position) * 0.5f;

			projectile.velocity = Vector2.Clamp(projectile.velocity, -Vector2.One * 10, Vector2.One * 10);
			#endregion

			#region Animation and visuals
			// So it will lean slightly towards the direction it's moving

			projectile.spriteDirection = projectile.velocity.X > 0 ? 1 : -1;

			int startOffset = 0;
			int frameCount = FlyFrames;
			float frameSpeed = defaultFrameSpeed;//this value is in fps

			switch (projectile.ai[0])
            {
				case 0:
					{
						projectile.rotation = 0;
						frameCount = RunFrames;
						frameSpeed = Math.Abs(projectile.velocity.X * defaultRunSpeed);
					} break;
				case 1:
                    {
						projectile.rotation = projectile.velocity.X * tiltAmount;
						startOffset = FlyOffset;
					} break;
				case 2:
					startOffset = AttackOffset; break;
			}

			projectile.frameCounter++;
			if ((int)(projectile.frameCounter * frameSpeed) >= 60)
			{
				projectile.frameCounter = 0;
				projectile.frame++;
			}

			if (projectile.frame >= startOffset + frameCount || projectile.frame < startOffset)
				projectile.frame = startOffset;

			// Some visuals here
			Lighting.AddLight(projectile.Center, Color.White.ToVector3() * 0.78f);
			#endregion

			modPlayer.KnightCount++;
		}
    }

	internal class PaleKnightPlayer : ModPlayer
	{
		public int KnightCount = 0;
		public override void ResetEffects() =>
			KnightCount = 0;
	}
}