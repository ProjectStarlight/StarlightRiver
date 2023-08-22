﻿using StarlightRiver.Content.Items.BaseTypes;
using StarlightRiver.Content.Items.Gravedigger;
using StarlightRiver.Content.Items.Misc;
using StarlightRiver.Core.Systems.CameraSystem;
using StarlightRiver.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Haunted
{
	public class TaintedGreataxe : CursedAccessory
	{
		public int ProjectileIndex;

		public override string Texture => AssetDirectory.HauntedItem + Name;

		public TaintedGreataxe() : base(ModContent.Request<Texture2D>(AssetDirectory.HauntedItem + "TaintedGreataxe").Value) { }

		public override void Load()
		{
			StarlightPlayer.OnHitNPCEvent += OnHitNPC;
			StarlightPlayer.OnHitNPCWithProjEvent += OnHitNPCWithProj;
			StarlightPlayer.ModifyHitNPCEvent += ModifyHitNPC;
			StarlightPlayer.ModifyHitNPCWithProjEvent += ModifyHitNPCWithProj;
		}

		public override void SetStaticDefaults()
		{
			Tooltip.SetDefault("Cursed : Landing a Critical Strike will inflict Focused on a new, different enemy\n" +
				"<right> on the Greataxe whilst it is Embedded to release it");
		}

		public override void SafeSetDefaults()
		{
			Item.value = Item.sellPrice(gold: 2);
		}

		private void ModifyHitNPCWithProj(Player player, Projectile proj, NPC target, ref NPC.HitModifiers hit)
		{
			if (!Equipped(player))
				return;

			var item = GetEquippedInstance(player) as TaintedGreataxe;

			if (Main.projectile[item.ProjectileIndex].ModProjectile is TaintedGreataxeProjectile greatAxe)
			{
				if (greatAxe.stickyAI)
				{
					if (target == greatAxe.targetNPC)
					{
						float initCrit = proj.CritChance * 2 / 100f;
						if (Main.rand.NextFloat() < initCrit)
							hit.SetCrit();
					}
					else
					{
						hit.DisableCrit();
					}
				}
			}
		}

		private void ModifyHitNPC(Player player, Item Item, NPC target, ref NPC.HitModifiers hit)
		{
			if (!Equipped(player))
				return;

			var item = GetEquippedInstance(player) as TaintedGreataxe;

			if (Main.projectile[item.ProjectileIndex].ModProjectile is TaintedGreataxeProjectile greatAxe)
			{
				if (greatAxe.stickyAI)
				{
					if (target == greatAxe.targetNPC)
					{
						float initCrit = (Item.crit + player.GetTotalCritChance(DamageClass.Generic)) * 2 / 100;

						if (Main.rand.NextFloat() < initCrit)
							hit.SetCrit();
					}
					else
					{
						hit.DisableCrit();
					}
				}
			}
		}

		private void OnHitNPCWithProj(Player player, Projectile proj, NPC target, NPC.HitInfo info, int damageDone)
		{
			if (!Equipped(player))
				return;

			var item = GetEquippedInstance(player) as TaintedGreataxe;

			if (Main.projectile[item.ProjectileIndex].ModProjectile is TaintedGreataxeProjectile greatAxe)
			{
				if (info.Crit && !greatAxe.Embedding)
				{
					//if there is an enemy close to the target that was just critted, the greataxe will go into them
					NPC npc = Main.npc.Where(n => n.active && n != target && target.Distance(n.Center) < 400f)
						.OrderBy(n => target.Distance(n.Center)).FirstOrDefault();

					if (npc != default)
					{
						greatAxe.targetNPC = npc;
						greatAxe.Embedding = true;
					}
					else
					{
						greatAxe.targetNPC = target;
						greatAxe.Embedding = true;
					}
				}
			}
		}

		private void OnHitNPC(Player player, Item Item, NPC target, NPC.HitInfo info, int damageDone)
		{
			if (!Equipped(player))
				return;

			var item = GetEquippedInstance(player) as TaintedGreataxe;

			if (Main.projectile[item.ProjectileIndex].ModProjectile is TaintedGreataxeProjectile greatAxe)
			{
				if (info.Crit && !greatAxe.Embedding)
				{
					//if there is an enemy close to the target that was just critted, the greataxe will go into them
					NPC npc = Main.npc.Where(n => n.active && n != target && target.Distance(n.Center) < 400f)
						.OrderBy(n => target.Distance(n.Center)).FirstOrDefault();

					if (npc != default)
					{
						greatAxe.targetNPC = npc;
						greatAxe.Embedding = true;
					}
					else
					{
						greatAxe.targetNPC = target;
						greatAxe.Embedding = true;
					}
				}
			}
		}

		public override void SafeUpdateEquip(Player Player)
		{
			if (Player.ownedProjectileCounts[ModContent.ProjectileType<TaintedGreataxeProjectile>()] < 1 && Player.whoAmI == Main.myPlayer)
			{
				var proj = Projectile.NewProjectileDirect(Player.GetSource_Accessory(Item), Player.Center, Vector2.One, ModContent.ProjectileType<TaintedGreataxeProjectile>(),
					(int)Player.GetTotalDamage(DamageClass.Generic).ApplyTo(30), 2f, Player.whoAmI);

				ProjectileIndex = proj.whoAmI;
			}

			Main.projectile[ProjectileIndex].timeLeft = 2;
		}

		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient<DullBlade>();
			recipe.AddIngredient<VengefulSpirit>(10);
			recipe.AddIngredient(ItemID.DemoniteBar, 10);
			recipe.AddTile(TileID.DemonAltar);
			recipe.Register();

			recipe = CreateRecipe();
			recipe.AddIngredient<DullBlade>();
			recipe.AddIngredient<VengefulSpirit>(10);
			recipe.AddIngredient(ItemID.CrimtaneBar, 10);
			recipe.AddTile(TileID.DemonAltar);
			recipe.Register();
		}
	}

	class TaintedGreataxeProjectile : ModProjectile
	{
		public bool init = false;
		private List<Vector2> oldPosition = new();
		private List<float> oldRotation = new();

		public NPC targetNPC;
		public bool Embedding;
		public int EmbeddingCooldown;

		public bool stickyAI;
		public Vector2 offset = Vector2.Zero;
		public int enemyWhoAmI;

		public Player Owner => Main.player[Projectile.owner];

		public override string Texture => AssetDirectory.HauntedItem + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Ethereal Greataxe");
			ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
			ProjectileID.Sets.TrailCacheLength[Projectile.type] = 12;
		}

		public override void SetDefaults()
		{
			Projectile.friendly = true;
			Projectile.DamageType = DamageClass.Generic;

			Projectile.tileCollide = false;
			Projectile.penetrate = -1;

			Projectile.width = 14;
			Projectile.height = 14;

			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 20;
		}

		public override bool PreAI()
		{
			if (stickyAI)
			{
				NPC target = Main.npc[enemyWhoAmI];
				Projectile.position = target.position + offset;
			}

			return true;
		}

		public override void AI()
		{
			if (!init)
			{
				init = true;
				oldPosition = new List<Vector2>();
				oldRotation = new List<float>();
			}

			if (Embedding && EmbeddingCooldown <= 0)
				EmbeddingAI();
			else
				IdleMovement();

			if (Owner.dead)
				Projectile.Kill();

			oldRotation.Add(Projectile.rotation);
			oldPosition.Add(Projectile.Center);

			while (oldRotation.Count > 12)
				oldRotation.RemoveAt(0);
			while (oldPosition.Count > 12)
				oldPosition.RemoveAt(0);

			if (EmbeddingCooldown > 0)
				EmbeddingCooldown--;
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
			SpriteEffects spriteEffects = SpriteEffects.None;
			if (Embedding)
				spriteEffects = Projectile.direction == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

			Vector2 origin = tex.Size() / 2f;
			origin.Y = Projectile.spriteDirection == 1 ? tex.Height - 40 : 40;

			if (!stickyAI)
			{
				var green = new Color(85, 220, 55, 0);

				for (int k = 12; k > 0; k--)
				{
					if (k > 0 && k < oldRotation.Count)
						Main.spriteBatch.Draw(tex, oldPosition[k] - Main.screenPosition, null, green * 0.5f, oldRotation[k], origin, Projectile.scale, spriteEffects, 0f);
				}
			}
			else
			{
				//barrier effects but different sorta
				float sin = (float)Math.Sin(Main.GameUpdateCount / 10f);

				for (int k = 0; k < 6; k++)
				{
					Vector2 dir = Vector2.UnitX.RotatedBy(k / 6f * 6.28f) * (5.5f + sin * 3.2f);
					Color color = new Color(25, 175, 55) * (0.85f - sin * 0.1f) * 0.9f;
					color.A = 0;
					Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition + dir, null, color, Projectile.rotation, origin, Projectile.scale, spriteEffects, 0);
				}
			}

			Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, null, Color.White, Projectile.rotation, origin, Projectile.scale, spriteEffects, 0);
			return false;
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			if (!stickyAI && target.life > 0)
			{
				if (Helper.IsFleshy(target))
				{
					for (int i = 0; i < 55; i++)
					{
						Vector2 velocity = Projectile.velocity.RotatedByRandom(MathHelper.ToRadians(45f)) * Main.rand.NextFloat(-0.25f, -0.15f);
						Dust.NewDustDirect(Projectile.position, 12, 12, ModContent.DustType<Dusts.GraveBlood>(), velocity.X, velocity.Y, 0, default, Main.rand.NextFloat(1.55f, 1.95f));
					}

					for (int i = 0; i < 30; i++)
					{
						Vector2 velocity = Projectile.velocity.RotatedByRandom(MathHelper.ToRadians(45f)) * Main.rand.NextFloat(-0.3f, -0.2f);
						Dust.NewDustDirect(Projectile.position, 12, 12, ModContent.DustType<Dusts.Glow>(), velocity.X, velocity.Y, 0, new Color(85, 220, 55), 0.85f);
					}

					Helper.PlayPitched("Impacts/GoreHeavy", 0.85f, Main.rand.NextFloat(-0.05f, 0.05f), Projectile.position);
				}
				else
				{
					for (int i = 0; i < 25; i++)
					{
						Vector2 velocity = Projectile.velocity.RotatedByRandom(MathHelper.ToRadians(45f)) * Main.rand.NextFloat(-0.3f, -0.2f);
						Dust.NewDustDirect(Projectile.position, 12, 12, ModContent.DustType<Dusts.Glow>(), velocity.X, velocity.Y, 0, new Color(85, 220, 55), 0.85f);
					}

					Helper.PlayPitched("Impacts/Clink", 0.7f, Main.rand.NextFloat(-0.05f, 0.05f), Projectile.position);
				}

				for (int i = 0; i < 15; i++)
				{
					Vector2 velocity = Projectile.velocity.RotatedByRandom(MathHelper.ToRadians(35f)) * Main.rand.NextFloat(-0.9f, -0.15f);
					Dust.NewDustDirect(Projectile.position, 4, 4, ModContent.DustType<Dusts.TaintedGreataxeDust>(), velocity.X, velocity.Y, 40 + Main.rand.Next(40), default, Main.rand.NextFloat(0.7f, 1f)).
						rotation = Main.rand.NextFloat(6.18f);
				}

				CameraSystem.shake += 11;

				Projectile.rotation = MathHelper.ToRadians(Main.rand.NextFloat(45f, 60f)) * Projectile.direction;

				stickyAI = true;
				Projectile.friendly = false;
				enemyWhoAmI = target.whoAmI;
				offset = Projectile.position - target.position;
				offset -= Projectile.velocity;
				Projectile.netUpdate = true;
			}
		}

		public override bool? CanHitNPC(NPC target)
		{
			return Embedding && target == targetNPC && !stickyAI;
		}

		public override void SendExtraAI(BinaryWriter writer)
		{
			writer.Write(stickyAI);
			writer.WritePackedVector2(offset);
			writer.Write(enemyWhoAmI);
		}

		public override void ReceiveExtraAI(BinaryReader reader)
		{
			stickyAI = reader.ReadBoolean();
			offset = reader.ReadPackedVector2();
			enemyWhoAmI = reader.ReadInt32();
		}

		private void EmbeddingAI()
		{
			if (!targetNPC.active || Projectile.Distance(Owner.Center) > 1600f) //un embed if too far away from player
			{
				Embedding = false;
				stickyAI = false;
				Projectile.velocity = Vector2.UnitY;
				EmbeddingCooldown = 120;
			}

			if (!stickyAI)
			{
				Projectile.rotation += Projectile.velocity.X * 0.025f;

				Vector2 direction = targetNPC.Center - Projectile.Center;
				if (direction.Length() > 25)
				{
					direction.Normalize();
					Projectile.velocity = Vector2.Lerp(Projectile.velocity, direction * 35f, 0.02f);
				}

				Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<Dusts.GlowFastDecelerate>(), null, 0, new Color(85, 220, 55), 0.35f);
			}
			else
			{
				Dust.NewDust(targetNPC.position, targetNPC.width, targetNPC.height, ModContent.DustType<Dusts.GlowFastDecelerate>(), 0, -2.5f, newColor: new Color(85, 220, 55), Scale: 0.4f);

				if (Main.mouseRight)// right clicking the projectile unembeds it
				{
					if (Main.myPlayer == Projectile.owner)
					{
						if (Projectile.Distance(Main.MouseWorld) < 30f)
						{
							Embedding = false;
							stickyAI = false;
							Projectile.velocity = Vector2.UnitY;
							EmbeddingCooldown = 120;
						}
					}
				}
			}
		}

		private void IdleMovement()
		{
			Projectile.rotation += Projectile.velocity.X * 0.04f;

			//modified movement code from Diane Crecent
			float speed = 15;
			Vector2 direction = Owner.Center - Projectile.Center;
			if (direction.Length() > 1500)
			{
				direction.Normalize();
				Vector2 TeleportPos = Owner.Center + Main.rand.NextVector2Circular(100, 100);
				Projectile.Center = TeleportPos;
				for (int i = 0; i < 10; i++)
				{
					Dust.NewDustPerfect(TeleportPos, ModContent.DustType<Dusts.GlowFastDecelerate>(), null, 0, new Color(200, 200, 205), 0.45f);

					Dust.NewDustPerfect(TeleportPos, ModContent.DustType<Dusts.GlowFastDecelerate>(), null, 0, new Color(85, 220, 55), 0.40f);
				}
			}

			if (direction.Length() > 100)
			{
				direction.Normalize();
				Projectile.velocity = Vector2.Lerp(Projectile.velocity, direction.RotatedByRandom(1.5f) * speed, 0.01f);
			}
			else if (Projectile.velocity == Vector2.Zero)
			{
				Projectile.velocity = Projectile.DirectionTo(Owner.Center);
			}

			Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<Dusts.GlowFastDecelerate>(), null, 0, new Color(85, 220, 55), 0.35f);
		}
	}
}