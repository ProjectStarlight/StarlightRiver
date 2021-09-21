using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Buffs;
using StarlightRiver.Content.Dusts;
using StarlightRiver.Core;
using StarlightRiver.Helpers;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Items.Astroflora
{
	public class GravediggerItem : ModItem
	{
		public override string Texture => AssetDirectory.GravediggerItem + "GravediggerItem";

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Gravedigger");
			Tooltip.SetDefault("Strikes enemies up into the air \nHit enemies in the air for more damage");
		}

		public override void SetDefaults()
		{
			item.damage = 20;
			item.melee = true;
			item.width = 36;
			item.height = 44;
			item.useTime = 12;
			item.useAnimation = 12;
			item.reuseDelay = 20;
			item.channel = true;
			item.useStyle = ItemUseStyleID.HoldingOut;
			item.knockBack = 6.5f;
			item.value = Item.sellPrice(0, 1, 0, 0);
			item.crit = 4;
			item.rare = 2;
			item.shootSpeed = 14f;
			item.autoReuse = false;
			item.shoot = ModContent.ProjectileType<GravediggerSwing>();
			item.noUseGraphic = true;
			item.noMelee = true;
			item.autoReuse = false;
		}

		public override bool CanUseItem(Player player) => player.GetModPlayer<GravediggerPlayer>().SwingDelay == 0;
	}

	internal class GravediggerSwing : ModProjectile
	{
		private const int FRAMEWIDTH = 128;
		private const int FRAMEHEIGHT = 138;
		private const int COLUMNONEFRAMES = 4;
		private const int COLUMNTWOFRAMES = 2;
		private const int COLUMNTHREEFRAMES = 3;
		private const int COLUMNFOURFRAMES = 4;


		private int frameX = 0;
		private int SlashWindow = 0;
		private Vector2 direction = Vector2.Zero;
		private Vector2 directionTwo = Vector2.Zero;
		public override string Texture => AssetDirectory.GravediggerItem + "GravediggerSwing";
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Grave digger");
			Main.projFrames[projectile.type] = 5;
		}

		public override void SetDefaults()
		{
			projectile.friendly = true;
			projectile.melee = true;
			projectile.tileCollide = false;
			projectile.Size = new Vector2(128, 138);
			projectile.penetrate = -1;
			projectile.usesLocalNPCImmunity = true;
			projectile.localNPCHitCooldown = 16;
			projectile.ownerHitCheck = true;
		}

		Player Player => Main.player[projectile.owner];

		private bool FirstTickOfSwing
		{
			get => projectile.ai[1] == 0;
			set => projectile.ai[1] = value ? 0 : 1;
		}

		private int SwingFrame
		{
			get => frameX / FRAMEWIDTH;
			set => frameX = value * FRAMEWIDTH;
		}

		private Vector2 SwingOrigin
        { 
			get
            {
				switch (SwingFrame)
                {
                    case 0:
						return new Vector2(FRAMEWIDTH * 0.5f, FRAMEHEIGHT * 0.5f);
					case 1:
						return new Vector2(FRAMEWIDTH * 0.33f, FRAMEHEIGHT * 0.4f);
					case 2:
						return new Vector2(FRAMEWIDTH * 0.5f, FRAMEHEIGHT * 0.5f);
					case 3:
						return new Vector2(FRAMEWIDTH * 0.35f, FRAMEHEIGHT * 0.6f);
				}
				return Vector2.Zero;
            }
		}
		public override void AI()
		{
			projectile.velocity = Vector2.Zero;
			if (FirstTickOfSwing)
			{
				directionTwo = Main.MouseWorld - Player.MountedCenter;
				directionTwo.Normalize();
				Player.ChangeDir(Main.MouseWorld.X > Player.MountedCenter.X ? 1 : -1);
				projectile.frame = 0;
				projectile.frameCounter = 0;
				SlashWindow = 30;
				FirstTickOfSwing = false;
				if (Player.controlUp)
				{
					direction = new Vector2(Player.direction, -1);
					SwingFrame = 2;
				}
				else if (Player.controlDown)
				{
					Player.GetModPlayer<GravediggerPlayer>().Combo = 0;
					direction = new Vector2(Player.direction, -1);
					SwingFrame = 3;
				}
				else
				{
					Player.GetModPlayer<GravediggerPlayer>().Combo++;
					SwingFrame = Player.GetModPlayer<GravediggerPlayer>().SwingFrame == 1 ? 0 : 1;
					if (SwingFrame == 0)
					{
						direction = new Vector2(Player.direction, 1);
					}
					else
                    {
						direction = new Vector2(Player.direction, -1);
					}
				}
				direction.Normalize();
				Player.GetModPlayer<GravediggerPlayer>().SwingFrame = SwingFrame;

				DoSFX();
			}
			Player.heldProj = projectile.whoAmI;
			Player.itemTime = 2;
			Player.itemAnimation = 2;
			Player.GetModPlayer<GravediggerPlayer>().SwingDelay = 2;
			Vector2 frameOrigin;
			if (Player.direction < 0)
				frameOrigin = new Vector2(FRAMEWIDTH, FRAMEHEIGHT) - SwingOrigin;
			else
				frameOrigin = SwingOrigin;

			projectile.position = Player.MountedCenter - frameOrigin;
			if (SwingFrame < 2)
			{
				if (!CheckFrameDeath())
				{
					if (SwingFrame == 0 && projectile.frame < 3) 
						direction = direction.RotatedBy(Player.direction * 0.3f);
					else if (SwingFrame == 1) 
						direction = direction.RotatedBy(Player.direction * -0.45f);
				}
				Player.ChangeDir(Main.MouseWorld.X > Player.MountedCenter.X ? 1 : -1);
				projectile.position += (directionTwo * 10);
				Player.itemRotation = MathHelper.WrapAngle(direction.ToRotation()  - ((Player.direction < 0) ? 0 : MathHelper.Pi));
				projectile.rotation = MathHelper.WrapAngle(Player.AngleFrom(projectile.position + frameOrigin) - ((Player.direction < 0) ? 0 : MathHelper.Pi));
			}
			else
			{
				if (!CheckFrameDeath())
				{
					if (SwingFrame == 2) //swing UP
						direction = direction.RotatedBy(Player.direction * -0.3f);
					else if (SwingFrame == 3) //swing DOWN
						direction = direction.RotatedBy(Player.direction * -0.2f);
				}
				Player.itemRotation = MathHelper.WrapAngle(direction.ToRotation() - ((Player.direction < 0) ? 0 : MathHelper.Pi));
				projectile.rotation = 0;
			}

			#region hardcoding
			if (SwingFrame == 3 && Player.direction < 0)
				projectile.position -= new Vector2(5, 20);

			if (SwingFrame == 2)
				projectile.position.X += 14 * Player.direction;

			if (SwingFrame == 1 && Player.direction < 0)
				projectile.position.Y += 12;
			#endregion

			if (!CheckFrameDeath())
				projectile.frameCounter++;

			if (projectile.frameCounter > 4)
			{
				projectile.frame++;
				projectile.frameCounter = 0;
			}
			if (CheckFrameDeath()) //TODO: Sync in multiplayer
			{
				SlashWindow--;
				if (Player == Main.LocalPlayer)
				{
					if (Main.mouseLeft && SlashWindow < 20)
						FirstTickOfSwing = true;
					else if (SlashWindow < 0)
					{
						Player.GetModPlayer<GravediggerPlayer>().Combo = 0;
						projectile.active = false;
					}
				}
			}
		}

		private bool CheckFrameDeath()
		{
			int deathFrame = 0;
			switch (SwingFrame)
			{
				case 0:
					deathFrame = COLUMNONEFRAMES;
					break;
				case 1:
					deathFrame = COLUMNTWOFRAMES;
					break;
				case 2:
					deathFrame = COLUMNTHREEFRAMES;
					break;
				case 3:
					deathFrame = COLUMNFOURFRAMES;
					break;
			}
			return projectile.frame >= deathFrame;
		}

		private void DoSFX()
        {
			Helper.PlayPitched("Effects/HeavyWhooshShort", 0.4f, Main.rand.NextFloat(-0.1f, 0.1f));
		}
		public override Color? GetAlpha(Color lightColor) => Color.Lerp(lightColor, Color.White, 0.2f);

		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
		{
			Texture2D tex = Main.projectileTexture[projectile.type];
			Rectangle frame = new Rectangle(frameX, projectile.frame * FRAMEHEIGHT, FRAMEWIDTH, FRAMEHEIGHT);
			SpriteEffects effects;

			Vector2 frameOrigin = SwingOrigin;
			if (Player.direction < 0)
				frameOrigin.X = FRAMEWIDTH - SwingOrigin.X;

			if (Player.direction < 0)
				effects = SpriteEffects.None;
			else
				effects = SpriteEffects.FlipHorizontally;
			spriteBatch.Draw(tex, projectile.position - Main.screenPosition + frameOrigin, frame, projectile.GetAlpha(lightColor), projectile.rotation, frameOrigin, projectile.scale, effects, 0);
			return false;
		}

		public override bool? CanHitNPC(NPC target)
		{
			if (projectile.frame > 0)
				return false;
			return base.CanHitNPC(target);
		}

		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
		{
			Player player = Main.player[projectile.owner];
			player.GetModPlayer<StarlightPlayer>().Shake += 3;
			Helper.PlayPitched("Impacts/GoreLight", 0.4f, Main.rand.NextFloat(-0.1f, 0.1f));
			if (target.knockBackResist != 0)
			{
				switch (SwingFrame)
				{
					case 0:
						if (!target.noGravity && !target.collideY)
							target.velocity.Y = -3;
						break;
					case 1:
						if (!target.noGravity && !target.collideY)
							target.velocity.Y = -3;
						break;
                    case 2:
						if (!target.noGravity)
						{
							target.AddBuff(ModContent.BuffType<ShovelSlowFall>(), 120);
							target.velocity.Y = -8f;
						}
						break;
                    case 3:
						if (!target.collideY)
						{
							target.AddBuff(ModContent.BuffType<ShovelQuickFall>(), 60);
							target.GetGlobalNPC<GravediggerNPC>().SlamPlayer = Player;
						}
						break;
				}
			}
		}
		public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
		{
			if (target.knockBackResist != 0 && !target.collideY && !target.noGravity && SwingFrame < 2 && target.HasBuff(ModContent.BuffType<ShovelSlowFall>()))
			{
				damage = (int)(damage * 1.5f);
			}
			if (SwingFrame < 2)
				hitDirection = Math.Sign(target.Center.X - Player.Center.X);
			else
				hitDirection = 0;
			CreateBlood(target, hitDirection);
		}

		private void CreateBlood(NPC target, int hitDirection)
        {
			Vector2 direction = Vector2.Zero;
			float variance = 0.5f;
			switch (SwingFrame)
            {
				case 0:
					direction = new Vector2(Math.Sign(hitDirection - 0.5f), 0);
					break;
				case 1:
					direction = new Vector2(Math.Sign(hitDirection - 0.5f), 0);
					break;
				case 2:
					direction = new Vector2(0, -1);
					variance = 0.35f;
					break;
				case 3:
					direction = new Vector2(0, 1);
					break;
			}
			for (int i = 0; i < 30; i++)
			{
				Vector2 direction2 = direction.RotatedBy(Main.rand.NextFloat(0 - variance, variance));
				direction2 *= Main.rand.NextFloat(0.5f, 6f);
				Dust.NewDustPerfect(target.Center, ModContent.DustType<GraveBlood>(), direction2);
			}
		}
    }

	internal class GravediggerSlam : ModProjectile
	{
		public override string Texture => AssetDirectory.GravediggerItem + "GravediggerItem";
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Grave digger");
		}

		public override void SetDefaults()
		{
			projectile.friendly = true;
			projectile.melee = true;
			projectile.tileCollide = false;
			projectile.Size = new Vector2(128, 138);
			projectile.penetrate = -1;
			projectile.timeLeft = 10;
			projectile.alpha = 255;
		}
	}
	internal class GravediggerPlayer : ModPlayer
	{
		public int SwingDelay = 0;
		public int SwingFrame = 0;
		public int Combo = 0;
		public override void ResetEffects() => 
			SwingDelay = Math.Max(SwingDelay - 1, 0);
	}

	public class GravediggerNPC : GlobalNPC
	{
		public override bool InstancePerEntity => true;
		public Player SlamPlayer;
	}

	class ShovelSlowFall : SmartBuff
	{
		public ShovelSlowFall() : base("Slow fall", "Falling Speed Reduced", false) { }

		public override void Update(NPC npc, ref int buffIndex)
		{
			if (npc.velocity.Y > 0.5f && !npc.HasBuff(ModContent.BuffType<ShovelQuickFall>()))
				npc.velocity.Y -= 0.1f;
		}
	}
	class ShovelQuickFall : SmartBuff
	{
		public ShovelQuickFall() : base("Quick fall", "You slammin", false) { }

		public override void Update(NPC npc, ref int buffIndex)
		{
			npc.velocity.Y = 40;
			if (npc.collideY)
            {
				Player player = Main.player[npc.target];
				npc.DelBuff(buffIndex--);
				player.GetModPlayer<StarlightPlayer>().Shake += 10;
				for (int k = 0; k <= 50; k++)
				{
					Dust.NewDustPerfect(npc.Center, ModContent.DustType<Content.Dusts.Stone>(), new Vector2(0, 1).RotatedByRandom(1) * Main.rand.NextFloat(-10, 10));
				}
				Projectile.NewProjectile(npc.Center, Vector2.Zero, ModContent.ProjectileType<GravediggerSlam>(), (int)(30 * npc.GetGlobalNPC<GravediggerNPC>().SlamPlayer.meleeDamage), 0, npc.GetGlobalNPC<GravediggerNPC>().SlamPlayer.whoAmI);
				Main.PlaySound(SoundID.Item70, npc.Center);
				Main.PlaySound(SoundID.NPCHit42, npc.Center);
			}
		}
	}
}