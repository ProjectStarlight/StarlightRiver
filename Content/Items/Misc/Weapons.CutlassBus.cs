using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.DataStructures;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Misc
{
	//TODO: Leaving this because its unfinished
	public class CutlassBus : ModItem
	{
		public override string Texture => AssetDirectory.MiscItem + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Cutlassbus");
			Tooltip.SetDefault("egshels update this lol");
		}

		public override void SetDefaults()
		{
			Item.damage = 15;
			Item.DamageType = DamageClass.Summon;
			Item.width = 24;
			Item.height = 24;
			Item.useTime = 4;
			Item.useAnimation = 4;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.noMelee = true;
			Item.knockBack = 0;
			Item.rare = ItemRarityID.Blue;
			Item.shoot = ModContent.ProjectileType<CutlassBusProj>();
			Item.shootSpeed = 12f;
			Item.autoReuse = true;
			Item.noUseGraphic = true;
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			for (int i = 0; i < 3; i++)
			{
				var proj = Projectile.NewProjectileDirect(source, position, velocity, type, damage, knockback, player.whoAmI);
				proj.originalDamage = damage;
			}

			Projectile.NewProjectile(source, position, Vector2.Zero, ModContent.ProjectileType<CutlassBusHeldProj>(), 0, 0, player.whoAmI, velocity.ToRotation());
			Projectile.NewProjectile(source, position, Vector2.Normalize(velocity) * 1.5f + player.velocity, ModContent.ProjectileType<CutlassBusFlash>(), 0, 0, player.whoAmI, velocity.ToRotation());
			return false;
		}
	}

	internal class CutlassBusHeldProj : ModProjectile
	{

		public override string Texture => AssetDirectory.MiscItem + Name;

		private Player owner => Main.player[Projectile.owner];

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Cutlassbus");
			Main.projFrames[Projectile.type] = 10;
		}

		public override void SetDefaults()
		{
			Projectile.friendly = false;
			Projectile.tileCollide = false;
			Projectile.Size = new Vector2(32, 32);
			Projectile.penetrate = -1;
		}

		public override void AI()
		{
			Main.projFrames[Projectile.type] = 10;
			Projectile.velocity = Vector2.Zero;
			Projectile.Center = owner.Center;
			owner.heldProj = Projectile.whoAmI;
			owner.itemTime = owner.itemAnimation = 2;

			Vector2 direction = Projectile.ai[0].ToRotationVector2();
			owner.direction = Math.Sign(direction.X);

			Projectile.rotation = Projectile.ai[0];

			int frameTicker = 3;
			if (Projectile.frame == Main.projFrames[Projectile.type] - 1)
				frameTicker = 30;
			Projectile.frameCounter++;
			if (Projectile.frameCounter >= frameTicker)
			{
				Projectile.frame++;
				Projectile.frameCounter = 0;
			}

			if (Projectile.frame >= Main.projFrames[Projectile.type])
				Projectile.active = false;
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
			Texture2D glowTex = ModContent.Request<Texture2D>(Texture + "_Glow").Value;
			int frameHeight = tex.Height / Main.projFrames[Projectile.type];
			var frame = new Rectangle(0, frameHeight * Projectile.frame, tex.Width, frameHeight);

			var origin = new Vector2(10, frameHeight * 0.75f);
			float rotation = Projectile.rotation;
			SpriteEffects effects = SpriteEffects.None;
			if (owner.direction == -1)
			{
				origin = new Vector2(tex.Width - origin.X, origin.Y);
				rotation += 3.14f;
				effects = SpriteEffects.FlipHorizontally;
			}

			Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, frame, lightColor, rotation, origin, Projectile.scale, effects, 0f);
			Main.spriteBatch.Draw(glowTex, Projectile.Center - Main.screenPosition, frame, Color.White, rotation, origin, Projectile.scale, effects, 0f);
			return false;
		}
	}
	internal class CutlassBusFlash : ModProjectile
	{

		public override string Texture => AssetDirectory.MiscItem + Name;

		private Player owner => Main.player[Projectile.owner];

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Cutlassbus");
			Main.projFrames[Projectile.type] = 6;
		}

		public override void SetDefaults()
		{
			Projectile.friendly = false;
			Projectile.tileCollide = false;
			Projectile.Size = new Vector2(32, 32);
			Projectile.penetrate = -1;
		}
		public override void AI()
		{
			Projectile.rotation = Projectile.ai[0];
			Projectile.frameCounter++;
			if (Projectile.frameCounter % 3 == 0)
				Projectile.frame++;
			if (Projectile.frame >= Main.projFrames[Projectile.type])
				Projectile.active = false;
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
			int frameHeight = tex.Height / Main.projFrames[Projectile.type];
			var frame = new Rectangle(0, frameHeight * Projectile.frame, tex.Width, frameHeight);

			var origin = new Vector2(0, frameHeight);
			if (owner.direction != 1)
				origin = new Vector2(0, 0);
			Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, frame, Color.White, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0f);
			return false;
		}
	}
	internal class CutlassBusProj : ModProjectile
	{

		public override string Texture => AssetDirectory.MiscItem + Name;

		private Player owner => Main.player[Projectile.owner];

		private NPC target = default;

		private readonly List<NPC> potentialVictims = new();

		private readonly List<NPC> victims = new();

		private bool stuck = false;
		private Vector2 stuckOffset = Vector2.Zero;
		private NPC stuckTarget = default;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Cutlassbus");
			Main.projFrames[Projectile.type] = 8;
		}

		public override void SetDefaults()
		{
			Projectile.friendly = true;
			Projectile.DamageType = DamageClass.Summon;
			Projectile.tileCollide = false;
			Projectile.Size = new Vector2(16, 16);
			Projectile.penetrate = 6;
			Projectile.timeLeft = 400;
		}
		public override void AI()
		{
			Projectile.frameCounter++;
			if (Projectile.frameCounter % 3 == 0)
				Projectile.frame++;
			if (Projectile.frame >= Main.projFrames[Projectile.type])
				Projectile.frame = 0;

			if (stuck)
			{
				if (!stuckTarget.active)
				{
					Projectile.Kill();
				}
				else
				{
					Projectile.Center = stuckTarget.Center + stuckOffset;
				}

				return;
			}

			if (target == default)
				target = Main.npc.Where(n => n.active && n.CanBeChasedBy(Projectile, false) && Vector2.Distance(n.Center, Projectile.Center) < 500 && n.GetGlobalNPC<CutlassBusGNPC>().targettable && !n.GetGlobalNPC<CutlassBusGNPC>().skewered && !victims.Contains(n)).OrderBy(x => Vector2.Distance(x.Center, Projectile.Center)).FirstOrDefault();
			if (target != default)
			{
				if (target == null || !target.active)
				{
					target = default;
					return;
				}

				target.GetGlobalNPC<CutlassBusGNPC>().targettable = false;

				if (!potentialVictims.Contains(target))
					potentialVictims.Add(target);

				float velMult = 1.6f;
				if (Projectile.velocity.Length() > 10)
					velMult = 1;

				if (Projectile.penetrate < 6)
					velMult = 0.7f;

				velMult *= Projectile.velocity.Length();

				Projectile.velocity = Vector2.Lerp(Projectile.velocity, Projectile.DirectionTo(target.Center) * velMult, 0.1f);

				Projectile.rotation = Projectile.velocity.ToRotation();
				if (Projectile.velocity.Length() < 0.1f)
					Projectile.Kill();
			}
		}

		public override bool? CanHitNPC(NPC localtarget)
		{
			if (victims.Contains(localtarget) || localtarget.GetGlobalNPC<CutlassBusGNPC>().skewered)
				return false;
			return base.CanHitNPC(localtarget);
		}

		public override void OnHitNPC(NPC localtarget, int damage, float knockback, bool crit)
		{
			CutlassBusGNPC gnpc = localtarget.GetGlobalNPC<CutlassBusGNPC>();
			if (localtarget.knockBackResist > 0.1f)
			{
				gnpc.targettable = false;
				Projectile.velocity *= Math.Max(1, localtarget.knockBackResist);
				gnpc.skewered = true;
				gnpc.skewerOffset = localtarget.Center - Projectile.Center;
				gnpc.skewerer = Projectile;

				victims.Add(localtarget);
			}
			else
			{
				Projectile.timeLeft = 100;
				stuck = true;
				Projectile.friendly = false;
				stuckTarget = localtarget;
				stuckOffset = Projectile.Center - localtarget.Center;
				Projectile.velocity = Vector2.Zero;
			}

			base.OnHitNPC(localtarget, damage, knockback, crit);
		}

		public override void Kill(int timeLeft)
		{
			Main.NewText("HYe");
			foreach (NPC npc in victims)
			{
				npc.GetGlobalNPC<CutlassBusGNPC>().skewered = false;
				npc.GetGlobalNPC<CutlassBusGNPC>().targettable = true;
			}

			foreach (NPC npc2 in potentialVictims)
			{
				npc2.GetGlobalNPC<CutlassBusGNPC>().targettable = true;
			}
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
			int frameHeight = tex.Height / Main.projFrames[Projectile.type];
			var frame = new Rectangle(0, frameHeight * Projectile.frame, tex.Width, frameHeight);

			var origin = new Vector2(tex.Width, frameHeight / 2);
			Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, frame, Color.White, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0f);
			return false;
		}
	}

	public class CutlassBusGNPC : GlobalNPC
	{
		public override bool InstancePerEntity => true;

		public bool targettable = true;

		public bool skewered = false;
		public Vector2 skewerOffset = Vector2.Zero;
		public Projectile skewerer = default;

		public override void PostAI(NPC npc)
		{
			if (skewered)
			{
				if (!skewerer.active)
				{
					skewered = false;
					targettable = true;
					return;
				}

				npc.Center = skewerer.Center + skewerOffset;
			}
		}
	}
}