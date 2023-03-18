﻿using System;
using System.Linq;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Gravedigger
{
	internal class StaffOfSelfDissassembly : ModItem
	{
		public override string Texture => AssetDirectory.GravediggerItem + Name;

		public override void Load()
		{
			StarlightPlayer.PreUpdateBuffsEvent += UpdateMinions;
		}

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Staff of Self Dissassembly");
			Tooltip.SetDefault("Reserve 40 life to summon a flesh apparation\n" +
				"apparations grant increased life regeneration\n" +
				"life regeneration boost is increased when apparitions strike an enemy");
		}

		public override void SetDefaults()
		{
			Item.damage = 38;
			Item.DamageType = DamageClass.Summon;
			Item.rare = ItemRarityID.Orange;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.useAnimation = 20;
			Item.useTime = 20;
			Item.shoot = ModContent.ProjectileType<FleshApparation>();
			Item.shootSpeed = 1;
		}

		public override bool CanUseItem(Player player)
		{
			return player.GetModPlayer<ResourceReservationPlayer>().TryReserveLife(40);
		}

		private void UpdateMinions(Player player)
		{
			foreach (Projectile proj in Main.projectile.Where(n => n.active && n.type == ModContent.ProjectileType<FleshApparation>() && n.owner == player.whoAmI))
			{
				if (proj.ModProjectile is FleshApparation mp)
				{
					player.GetModPlayer<ResourceReservationPlayer>().ReserveLife(40);
					player.lifeRegen += (int)(6 * mp.healPower);
				}
			}
		}
	}

	internal class FleshApparation : ModProjectile
	{
		public float healPower = 1;

		public Vector2 target;
		public float randomRot;

		public NPC targetNPC;

		public Player Owner => Main.player[Projectile.owner];

		public ref float Timer => ref Projectile.ai[0];
		public ref float State => ref Projectile.ai[1];

		public override string Texture => AssetDirectory.GravediggerItem + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Flesh apparation");
			ProjectileID.Sets.TrailCacheLength[Projectile.type] = 40;
			ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
		}

		public override void SetDefaults()
		{
			Projectile.DamageType = DamageClass.Summon;
			Projectile.width = 42;
			Projectile.height = 42;
			Projectile.friendly = true;
			Projectile.minion = true;
			Projectile.minionSlots = 1;
			Projectile.tileCollide = false;
			Projectile.penetrate = -1;
		}

		public override void AI()
		{
			Projectile.timeLeft = 2;

			Timer++;

			if (healPower > 1)
				healPower -= 0.005f;

			if (healPower > 3)
				healPower = 3;

			Projectile.friendly = true; //We should only do damage while dashing!

			switch (State)
			{
				case 0:
					PassiveMovement();
					break;

				case 1:
					Attack();
					break;
			}

			if (Main.rand.NextBool(3))
				Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Blood, 0, 2, 0, default, Main.rand.NextFloat(1.25f));
		}

		public void PassiveMovement()
		{
			if (Timer % 80 == 0)
				target = Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(60, 90);

			Vector2 moveTarget = Owner.Center + target;

			Projectile.velocity = (moveTarget - Projectile.Center) * 0.05f; //hover around random points while idle

			if (targetNPC != null && targetNPC.active) //If we have a target, engage attack mode!
			{
				Timer = Main.rand.Next(60, 100);
				State = 1;
				target = Projectile.Center; //Set target to the current position so we can use it to attack from
			}
			else //Otherwise look for a target
			{
				FindTarget();
			}
		}

		public void FindTarget()
		{
			foreach (NPC npc in Main.npc) //scan all targets for anything less than 600 units away
			{
				if (!npc.friendly && npc.CanBeChasedBy(Projectile) && Vector2.Distance(npc.Center, Projectile.Center) < 600)
					targetNPC = npc;
			}
		}

		public void Attack()
		{
			if (targetNPC is null || !targetNPC.active || Vector2.Distance(Owner.Center, targetNPC.Center) > 800) //If we lost the target, abandon attack
			{
				State = 0;
				Timer = 0;
				target = Vector2.Zero;
				targetNPC = null;
				return;
			}

			if (Timer < 60) //Position away from target
			{
				Projectile.velocity *= 0;

				Vector2 diff = Vector2.Normalize(target - targetNPC.Center).RotatedBy(randomRot);
				Projectile.Center = Vector2.Lerp(target, targetNPC.Center + diff * 200f, Helpers.Helper.SwoopEase(Timer / 60f));
			}

			if (Timer == 60) //Dash!
				Projectile.velocity = Vector2.Normalize(targetNPC.Center - Projectile.Center) * 45f;

			if (Timer >= 60 && Timer < 100) //slow down
			{
				Projectile.velocity *= 0.92f;
				Projectile.friendly = true;

				Lighting.AddLight(Projectile.Center, new Vector3(0.02f, 0.01f, 0.01f) * Projectile.velocity.Length());
			}

			if (Timer >= 100) //reset
			{
				target = Projectile.Center;
				randomRot = Main.rand.NextFloat(2);
				Timer = 0;
			}
		}

		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
		{
			healPower += 0.5f;

			for (int k = 0; k < 20; k++)
			{
				Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<Dusts.BloodMetaballDust>(), Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(2.5f), 0, Color.Red, 0.3f);

				for (int i = 0; i < 5; i++)
				{
					Dust.NewDustPerfect(Projectile.Center + Vector2.One.RotatedByRandom(6.28f) * 20, DustID.Blood, Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(5f));
				}
			}

			Helpers.Helper.PlayPitched("Impacts/ArrowFleshy", 1, Main.rand.NextFloat(-0.4f, 0.2f), Projectile.Center);
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Vector2 targetPoint = Owner.Center;

			Main.spriteBatch.End();
			Main.spriteBatch.Begin(default, BlendState.Additive, SamplerState.PointWrap, default, default, default, Main.GameViewMatrix.TransformationMatrix);

			Texture2D tex = Terraria.GameContent.TextureAssets.MagicPixel.Value;
			Texture2D tex2 = ModContent.Request<Texture2D>("StarlightRiver/Assets/EnergyTrail").Value;
			Texture2D tex3 = ModContent.Request<Texture2D>("StarlightRiver/Assets/MotionTrail").Value;

			float strength = (0.2f + (float)Math.Sin(Main.GameUpdateCount * 0.1f) * 0.05f) * (healPower * 2);

			float dist = Vector2.Distance(Projectile.Center, targetPoint);
			float width = 3f + strength * 8f;
			float rot = (targetPoint - Projectile.Center).ToRotation();

			var target = new Rectangle((int)Projectile.Center.X, (int)Projectile.Center.Y, (int)dist, (int)width);
			target.Offset((-Main.screenPosition).ToPoint());

			var target2 = new Rectangle((int)Projectile.Center.X, (int)Projectile.Center.Y, (int)dist, (int)width * 2);
			target2.Offset((-Main.screenPosition).ToPoint());

			var target3 = new Rectangle((int)Projectile.Center.X, (int)Projectile.Center.Y, 2, (int)dist);
			target3.Offset((-Main.screenPosition).ToPoint());

			var source = new Rectangle(-(int)Main.GameUpdateCount * 5, 0, tex2.Width, tex2.Height);

			var source2 = new Rectangle(tex3.Width / 2, -(int)Main.GameUpdateCount * 2, 2, (int)dist);

			Main.spriteBatch.Draw(tex, target, null, new Color(200, 0, 80) * (strength * 0.25f), rot, new Vector2(0, tex.Height / 2f), 0, 0);
			Main.spriteBatch.Draw(tex2, target2, source, new Color(200, 40, 55) * (strength * 0.8f), rot, new Vector2(0, tex2.Height / 2f), 0, 0);
			Main.spriteBatch.Draw(tex3, target3, source2, new Color(255, 80, 80) * strength, rot - 1.57f, new Vector2(0, 0), 0, 0);

			Main.spriteBatch.End();
			Main.spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.TransformationMatrix);

			Texture2D mainTex = ModContent.Request<Texture2D>(Texture).Value;

			if (State == 1)
			{
				for (int k = 0; k < (Timer - 60); k++)
				{
					Color color = Color.Lerp(new Color(255, 100, 100), Color.Red * 0.1f, k / 20f) * (Projectile.velocity.Length() / 20f);
					Main.spriteBatch.Draw(mainTex, Projectile.oldPos[k] - Main.screenPosition + Projectile.Size / 2f, null, color, Projectile.oldRot[k], mainTex.Size() / 2f, 1, 0, 0);
				}
			}

			Main.spriteBatch.Draw(mainTex, Projectile.Center - Main.screenPosition, null, Color.White, Projectile.rotation, mainTex.Size() / 2f, 1, 0, 0);

			return false;
		}
	}
}
