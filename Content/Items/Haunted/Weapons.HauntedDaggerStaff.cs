using NetEasy;
using StarlightRiver.Content.Buffs.Summon;
using StarlightRiver.Content.Items.BarrierDye;
using StarlightRiver.Core.Systems.BarrierSystem;
using StarlightRiver.Core.Systems.CameraSystem;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.DataStructures;
using Terraria.Graphics.Effects;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Haunted
{
	public class HauntedDaggerStaff : ModItem
	{
		public override string Texture => AssetDirectory.HauntedItem + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Haunted Dagger Staff");
			Tooltip.SetDefault("Summons haunted daggers, embedding themselves in your foes\nChange summon targets or whip them to violently tear the daggers from their flesh" +
				"\n'These aren't enchanted... they're haunted!'\n'It's different?'\n'It's different.'");
			ItemID.Sets.GamepadWholeScreenUseRange[Item.type] = true; // This lets the Player target anywhere on the whole screen while using a controller.
			ItemID.Sets.LockOnIgnoresCollision[Item.type] = true;
		}

		public override void SetDefaults()
		{
			Item.damage = 12;
			Item.knockBack = 5f;
			Item.mana = 10;
			Item.width = 32;
			Item.height = 32;
			Item.useTime = 36;
			Item.useAnimation = 36;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.value = Item.buyPrice(gold: 1);
			Item.rare = ItemRarityID.Blue;
			Item.UseSound = SoundID.Item44;

			Item.noMelee = true;
			Item.DamageType = DamageClass.Summon;
			Item.buffType = ModContent.BuffType<HauntedDaggerSummonBuff>();
			Item.shoot = ModContent.ProjectileType<HauntedDaggerProjectile>();
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			player.AddBuff(Item.buffType, 2);
			Projectile.NewProjectileDirect(source, Main.MouseWorld, velocity, type, damage, knockback, Main.myPlayer).originalDamage = Item.damage;

			for (int i = 0; i < 15; i++)
			{
				Dust.NewDustPerfect(Main.MouseWorld, ModContent.DustType<Dusts.GlowFastDecelerate>(),
					Main.rand.NextVector2CircularEdge(3f, 3f), 0, new Color(70, 200, 100), 0.5f);
			}

			return false;
		}

		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();

			recipe.AddIngredient(ItemID.RichMahogany, 30);
			recipe.AddIngredient(ItemID.GoldBar, 15);
			recipe.AddIngredient<VengefulSpirit>(10);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();

			recipe = CreateRecipe();

			recipe.AddIngredient(ItemID.RichMahogany, 30);
			recipe.AddIngredient(ItemID.PlatinumBar, 15);
			recipe.AddIngredient<VengefulSpirit>(10);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}
	}

	class HauntedDaggerGlobalNPC : GlobalNPC
	{
		public bool Unembed = false;
		public int UnembedCount = -1;
		public int UnembeddingPlayerWhoAmI;

		public override bool InstancePerEntity => true;

		public int GetDaggerCount(NPC npc)
		{
			return Main.projectile.Where(p => p.active && p.type == ModContent.ProjectileType<HauntedDaggerProjectile>() && (p.ModProjectile as HauntedDaggerProjectile).TargetWhoAmI == npc.whoAmI && (p.ModProjectile as HauntedDaggerProjectile).embedded).Count();
		}

		public static void UnembedDaggers(NPC npc, int count, Player strikingPlayer)
		{
			if (Main.myPlayer == strikingPlayer.whoAmI && Main.netMode == NetmodeID.MultiplayerClient)
			{
				var packet = new UnembedPacket(npc, strikingPlayer);
				packet.Send(-1, strikingPlayer.whoAmI, false);
			}

			for (int i = 0; i < Main.maxProjectiles; i++)
			{
				Projectile proj = Main.projectile[i];
				var dagger = proj.ModProjectile as HauntedDaggerProjectile;

				if (proj.active && dagger != null && dagger.TargetWhoAmI == npc.whoAmI && dagger.embedded)
					dagger.Unembed(false, npc);
			}

			if (strikingPlayer.whoAmI == Main.myPlayer) // Only player with the whip does the damage and gets the shake
			{
				npc.SimpleStrikeNPC(count * 10, 0, false, 0, DamageClass.Summon, true, strikingPlayer.luck);

				CameraSystem.shake += 8;
			}

			bool fleshy = Helpers.Helper.IsFleshy(npc);

			Helpers.Helper.PlayPitched("Impacts/GoreLight", 1f, Main.rand.NextFloat(-0.1f, 0.1f), npc.Center);

			if (!fleshy)
				Helpers.Helper.PlayPitched("Impacts/Clink", 1f, Main.rand.NextFloat(-0.1f, 0.1f), npc.Center);

			for (int i = 0; i < 15; i++)
			{
				Dust.NewDustPerfect(npc.Center, ModContent.DustType<Dusts.GlowFastDecelerate>(),
					Main.rand.NextVector2CircularEdge(10f, 10f), 0, new Color(70, 200, 100), 0.5f);

				for (int d = 0; d < 2; d++)
				{
					Dust.NewDustPerfect(npc.Center + Main.rand.NextVector2Circular(npc.width, npc.height), ModContent.DustType<Dusts.GlowFastDecelerate>(),
						-Vector2.UnitY * Main.rand.NextFloat(5), 0, new Color(70, 200, 100), 0.5f);
				}

				if (fleshy)
				{
					Dust.NewDustPerfect(npc.Center + Main.rand.NextVector2Circular(npc.width, npc.height), DustID.Blood,
						Main.rand.NextVector2Circular(10f, 10f), Main.rand.Next(100), default, 2.5f).noGravity = true;

					Dust.NewDustPerfect(npc.Center + Main.rand.NextVector2Circular(npc.width, npc.height), ModContent.DustType<Dusts.GraveBlood>(),
						Main.rand.NextVector2Circular(10f, 10f), Main.rand.Next(100), default, 1.5f);
				}
				else
				{
					Dust.NewDustPerfect(npc.Center + Main.rand.NextVector2Circular(npc.width, npc.height), ModContent.DustType<Dusts.GlowFastDecelerate>(),
					Main.rand.NextVector2Circular(10f, 10f), 0, new Color(150, 80, 30), 0.5f);
				}
			}
		}

		public override void OnHitByProjectile(NPC npc, Projectile projectile, NPC.HitInfo hit, int damageDone)
		{
			int count = GetDaggerCount(npc);

			if (ProjectileID.Sets.IsAWhip[projectile.type] && projectile.CountsAsClass(DamageClass.Summon) && count > 0)
				UnembedDaggers(npc, count, Main.player[projectile.owner]);
		}

		public override void AI(NPC npc)
		{
			if (Unembed && UnembedCount > 0)
			{
				UnembedDaggers(npc, UnembedCount, Main.player[UnembeddingPlayerWhoAmI]);

				Unembed = false;
				UnembeddingPlayerWhoAmI = -1;
				UnembedCount = -1;
			}
		}
	}

	class HauntedDaggerProjectile : ModProjectile
	{
		public const int MAX_ATTACK_DELAY = 5;

		public int rotTimer;
		public int lifetime;

		public Vector2 rotationalVelocity;

		public Vector2 enemyOffset;

		public NPC embeddedTarget;

		public bool embedded = false;

		public ref float AttackTimer => ref Projectile.ai[0];
		public ref float TargetWhoAmI => ref Projectile.ai[1];
		public ref float AttackDelay => ref Projectile.ai[2];

		public NPC Target => TargetWhoAmI > -1 ? Main.npc[(int)TargetWhoAmI] : null;
		public bool FoundTarget => Target != null;

		public Player Owner => Main.player[Projectile.owner];

		public NPC MinionTarget
		{
			get
			{
				if (Owner.HasMinionAttackTargetNPC && Main.npc[Owner.MinionAttackTargetNPC].Distance(Projectile.Center) < 1000f)
					return Main.npc[Owner.MinionAttackTargetNPC];

				return null;
			}
		}

		public int Lifetime
		{
			get
			{
				if (Projectile.minionPos <= 0)
				{
					return lifetime;
				}
				else
				{
					Projectile proj = Main.projectile.Where(p => p.active && p.type == Type && p.owner == Projectile.owner && p.minionPos == 0).FirstOrDefault();
					if (proj != null)
					{
						var dagger = proj.ModProjectile as HauntedDaggerProjectile;
						if (dagger != null)
							return dagger.lifetime;
					}
				}

				return 0;
			}
		}

		public override string Texture => AssetDirectory.HauntedItem + Name;
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Haunted Dagger");
			Main.projPet[Projectile.type] = true; // Denotes that this Projectile is a pet or minion
			ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true; // This is necessary for right-click targeting
			ProjectileID.Sets.MinionSacrificable[Projectile.type] = true; // This is needed so your minion can properly spawn when summoned and replaced when other minions are summoned			 
			ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true; // Don't mistake this with "if this is true, then it will automatically home". It is just for damage reduction for certain NPCs
		}

		public override void SetDefaults()
		{
			Projectile.Size = new(20);
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.minion = true;
			Projectile.friendly = true;
			Projectile.hostile = false;
			Projectile.minionSlots = 1f;
			Projectile.penetrate = -1;
			Projectile.DamageType = DamageClass.Summon;
		}

		public override void OnSpawn(IEntitySource source)
		{
			TargetWhoAmI = -1f;
		}

		public override bool PreAI()
		{
			if (embedded)
			{
				Projectile.position = embeddedTarget.position + enemyOffset;

				bool wrongTarget = MinionTarget != null && embeddedTarget != MinionTarget;
				bool deadTarget = !embeddedTarget.active;

				if (wrongTarget || deadTarget)
					Unembed(wrongTarget && !deadTarget, embeddedTarget);

				UpdateProjectileLifetime();

				if (Main.rand.NextBool(15))
				{
					Dust.NewDustPerfect(Projectile.Center + Projectile.velocity + Main.rand.NextVector2Circular(2f, 2f), ModContent.DustType<Dusts.GlowFastDecelerate>(),
						Main.rand.NextVector2Circular(2f, 2f), 0, new Color(70, 200, 100), 0.45f);
				}

				return false;
			}

			return true;
		}

		public override void AI()
		{
			UpdateProjectileLifetime();

			if (AttackDelay > 0)
				AttackDelay--;

			if (rotTimer > 0)
				rotTimer--;

			if (MinionTarget != null && AttackDelay <= 0)
				TargetWhoAmI = MinionTarget.whoAmI;

			if (!FoundTarget)
			{
				if (Main.rand.NextBool(60))
				{
					Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(2f, 2f), ModContent.DustType<Dusts.GlowFastDecelerate>(),
						Main.rand.NextVector2Circular(2f, 2f), 0, new Color(70, 200, 100), 0.45f);
				}

				DoIdleMovement();

				if (AttackDelay <= 0)
				{
					NPC target = FindTarget();
					if (target != default)
						TargetWhoAmI = target.whoAmI;
				}
			}
			else if (AttackDelay <= 0)
			{
				AttackTimer++;

				if (AttackTimer < 45)
				{
					DoIdleMovement();

					rotTimer += (int)MathHelper.Lerp(1f, 50f, EaseFunction.EaseCubicIn.Ease(AttackTimer / 45f));

					float lerper = MathHelper.Lerp(30f, 2f, EaseFunction.EaseCubicIn.Ease(AttackTimer / 45f));
					if (Main.rand.NextBool(3))
						Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2CircularEdge(lerper, lerper), ModContent.DustType<Dusts.GlowFastDecelerate>(), Main.rand.NextVector2Circular(0.25f, 0.25f), 0, new Color(70, 200, 100), 0.5f);
				}
				else
				{
					if (Main.rand.NextBool())
					{
						Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(2f, 2f), ModContent.DustType<Dusts.GlowFastDecelerate>(),
							Main.rand.NextVector2Circular(2f, 2f), 0, new Color(70, 200, 100), 0.45f);
					}

					rotationalVelocity = Vector2.Lerp(rotationalVelocity, Projectile.DirectionTo(Target.Center), 0.15f);
					Projectile.rotation = rotationalVelocity.ToRotation() + MathHelper.PiOver2;

					if (AttackTimer == 45)
					{
						Projectile.velocity += Projectile.DirectionTo(Target.Center) * 22f;

						Helpers.Helper.PlayPitched("Effects/HeavyWhooshShort", 0.65f, Main.rand.NextFloat(-0.1f, 0.1f), Projectile.Center);
					}

					Vector2 direction = Target.Center - Projectile.Center;
					direction.Normalize();
					direction *= 23.5f;
					Projectile.velocity = Vector2.Lerp(Projectile.velocity, direction, 0.075f);

					if (rotTimer > 0)
						rotTimer = 0;
				}

				if (!Target.active || Target.Distance(Owner.Center) > 1000f)
				{
					TargetWhoAmI = -1;
					AttackDelay = MAX_ATTACK_DELAY;
					AttackTimer = 0;
				}
			}
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			Owner.TryGetModPlayer(out StarlightPlayer starlightPlayer);
			starlightPlayer.SetHitPacketStatus(shouldRunProjMethods: true);

			if (!embedded && target.life > 0)
			{
				embeddedTarget = target;
				embedded = true;
				Projectile.friendly = false;
				enemyOffset = Projectile.position - target.position;
				enemyOffset -= Projectile.velocity;
				Projectile.netUpdate = true;
			}

			Helpers.Helper.PlayPitched("Impacts/StabTiny", 1f, Main.rand.NextFloat(-0.1f, 0.1f), Projectile.Center);

			for (int i = 0; i < 6; i++)
			{
				Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(2f, 2f), ModContent.DustType<Dusts.GlowFastDecelerate>(),
					-Projectile.velocity.RotatedByRandom(0.5f) * Main.rand.NextFloat(0.5f), 0, new Color(70, 200, 100), 0.5f);

				if (Helpers.Helper.IsFleshy(target))
				{
					Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(3f, 3f), DustID.Blood,
						-Projectile.velocity.RotatedByRandom(0.75f) * Main.rand.NextFloat(0.25f), Main.rand.Next(100), default, 2.5f).noGravity = true;

					Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(3f, 3f), ModContent.DustType<Dusts.GraveBlood>(),
						-Projectile.velocity.RotatedByRandom(0.75f) * Main.rand.NextFloat(0.25f), Main.rand.Next(100), default, 1.5f);
				}
				else
				{
					Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(2f, 2f), ModContent.DustType<Dusts.GlowFastDecelerate>(),
					-Projectile.velocity.RotatedByRandom(0.5f) * Main.rand.NextFloat(0.5f), 0, new Color(150, 80, 30), 0.5f);

				}
			}

			CameraSystem.shake += 2;
		}

		public override bool? CanHitNPC(NPC target)
		{
			return target.whoAmI == TargetWhoAmI;
		}

		public override bool MinionContactDamage()
		{
			return !embedded && AttackDelay <= 0 && AttackTimer >= 45;
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
			Texture2D texGlow = ModContent.Request<Texture2D>(Texture + "_Glow").Value;
			Texture2D bloomTex = ModContent.Request<Texture2D>(AssetDirectory.Keys + "GlowAlpha").Value;
			Texture2D starTex = ModContent.Request<Texture2D>(AssetDirectory.Assets + "StarTexture").Value;

			Main.spriteBatch.Draw(bloomTex, Projectile.Center - Main.screenPosition, null, new Color(70, 200, 100, 0) * 0.25f, Projectile.rotation + MathHelper.ToRadians(rotTimer), bloomTex.Size() / 2f, 1f, 0f, 0f);

			Main.spriteBatch.Draw(texGlow, Projectile.Center - Main.screenPosition, null, new Color(70, 200, 100, 0), Projectile.rotation + MathHelper.ToRadians(rotTimer), texGlow.Size() / 2f, Projectile.scale, 0f, 0f);

			Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, Color.White, Projectile.rotation + MathHelper.ToRadians(rotTimer), tex.Size() / 2f, Projectile.scale, 0f, 0f);

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

			Color color = new Color(70, 200, 100, 0) * 0.4f * Utils.Clamp((float)Math.Sin(Main.GlobalTimeWrappedHourly * 2f), 0.5f, 1f);

			effect.Parameters["uColor"].SetValue(color.ToVector4());
			effect.Parameters["noiseImage1"].SetValue(ModContent.Request<Texture2D>(AssetDirectory.Assets + "Noise/PerlinNoise").Value);

			effect.CurrentTechnique.Passes[0].Apply();

			Main.spriteBatch.Draw(texGlow, Projectile.Center - Main.screenPosition, null, new Color(70, 200, 100, 0), Projectile.rotation + MathHelper.ToRadians(rotTimer), texGlow.Size() / 2f, Projectile.scale * 1.5f, 0f, 0f);

			Main.spriteBatch.Draw(bloomTex, Projectile.Center - Main.screenPosition, null, new Color(70, 200, 100, 0), Projectile.rotation + MathHelper.ToRadians(rotTimer), bloomTex.Size() / 2f, 1f, 0f, 0f);

			Main.spriteBatch.End();
			Main.spriteBatch.Begin(default, default, Main.DefaultSamplerState, default, default, default, Main.GameViewMatrix.TransformationMatrix);

			return false;
		}

		public void Unembed(bool dealDamage, NPC target)
		{
			HauntedDaggerGlobalNPC gNPC = target.GetGlobalNPC<HauntedDaggerGlobalNPC>();

			gNPC.Unembed = dealDamage;
			gNPC.UnembeddingPlayerWhoAmI = Owner.whoAmI;

			int count = gNPC.GetDaggerCount(target);

			if (gNPC.UnembedCount <= 0)
				gNPC.UnembedCount = count;

			embedded = false;
			TargetWhoAmI = -1f;
			AttackDelay = MAX_ATTACK_DELAY;

			Projectile.velocity *= -1f;
			AttackTimer = 0;

			for (int i = 0; i < 6; i++)
			{
				Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(2f, 2f), ModContent.DustType<Dusts.GlowFastDecelerate>(),
					Projectile.Center.DirectionTo(Projectile.Center + Projectile.velocity).RotatedByRandom(1f) * Main.rand.NextFloat(16f), 0, new Color(70, 200, 100), 0.5f);
			}
		}

		internal void DoIdleMovement()
		{
			float totalCount = Owner.ownedProjectileCounts[Type] > 0 ? Owner.ownedProjectileCounts[Type] : 1;

			Vector2 idlePos = Owner.Center + new Vector2(0f, -100) + new Vector2(0, 35).RotatedBy(MathHelper.ToRadians(Projectile.minionPos / totalCount * 360f + (Projectile.minionPos == totalCount ? 360f / totalCount : 0f)) + MathHelper.ToRadians(Lifetime));

			float dist = Vector2.Distance(Projectile.Center, idlePos);

			Vector2 toIdlePos = idlePos - Projectile.Center;
			if (toIdlePos.Length() < 0.0001f)
			{
				toIdlePos = Vector2.Zero;
			}
			else
			{
				float speed = 35f;
				if (dist < 1000f)
					speed = MathHelper.Lerp(5f, 35f, dist / 1000f);

				if (dist < 100f)
					speed = MathHelper.Lerp(0.1f, 5f, dist / 100f);

				toIdlePos.Normalize();
				toIdlePos *= speed;
			}

			Projectile.velocity = (Projectile.velocity * (25f - 1) + toIdlePos) / 25f;

			if (dist > 2000f)
			{
				Projectile.Center = idlePos;
				Projectile.velocity = Vector2.Zero;
				Projectile.netUpdate = true;
			}

			Projectile.rotation = Projectile.velocity.X * 0.05f;
		}

		internal void UpdateProjectileLifetime()
		{
			if (Owner.HasBuff<HauntedDaggerSummonBuff>())
				Projectile.timeLeft = 2;

			lifetime++;
		}

		internal NPC FindTarget()
		{
			return Main.npc.Where(n => n.CanBeChasedBy() && n.Distance(Owner.Center) < 1000f).OrderBy(n => n.Distance(Projectile.Center)).FirstOrDefault();
		}

		public override void SendExtraAI(BinaryWriter writer)
		{
			writer.Write(embedded);
			writer.WritePackedVector2(enemyOffset);

			if (embeddedTarget != null)
				writer.Write(embeddedTarget.whoAmI);
			else
				writer.Write(-1);
		}

		public override void ReceiveExtraAI(BinaryReader reader)
		{
			embedded = reader.ReadBoolean();
			enemyOffset = reader.ReadPackedVector2();
			int embeddedTargetId = reader.ReadInt32();

			if (embeddedTargetId >= 0)
				embeddedTarget = Main.npc[embeddedTargetId];
			else 
				embeddedTarget = null;
		}
	}

	[Serializable]
	public class UnembedPacket : Module
	{
		public readonly byte strikingPlayerWhoAmI;
		public readonly int npcWhoAmI;

		public UnembedPacket(NPC npc, Player strikingPlayer)
		{
			strikingPlayerWhoAmI = (byte)strikingPlayer.whoAmI;
			npcWhoAmI = npc.whoAmI;
		}

		protected override void Receive()
		{
			Player player = Main.player[strikingPlayerWhoAmI];
			NPC npc = Main.npc[npcWhoAmI];

			// "Count" doesn't matter for the other players since only the striker deals damage
			HauntedDaggerGlobalNPC.UnembedDaggers(npc, 0, player);

			if (Main.netMode == NetmodeID.Server)
			{
				Send(-1, strikingPlayerWhoAmI, false);
				return;
			}
		}
	}
}