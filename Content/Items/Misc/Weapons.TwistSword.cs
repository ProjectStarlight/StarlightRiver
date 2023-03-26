using StarlightRiver.Content.CustomHooks;
using StarlightRiver.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria.DataStructures;
using Terraria.Graphics.Effects;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Items.Misc
{
	class TwistSword : ModItem
	{
		public int charge = 0;
		int timer = 0;
		bool noItemLastFrame = false;

		public override string Texture => AssetDirectory.MiscItem + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Twisted Greatsword");
			Tooltip.SetDefault("Hold to unleash a whirling slash\nHold jump while slashing to accelerate upward");
		}

		public override void SetDefaults()
		{
			Item.damage = 28;
			Item.crit = 5;
			Item.DamageType = DamageClass.Melee;
			Item.width = 40;
			Item.height = 20;
			Item.useTime = 10;
			Item.useAnimation = 10;
			Item.useStyle = ItemUseStyleID.HoldUp;
			Item.noMelee = true;
			Item.knockBack = 8;
			Item.rare = ItemRarityID.Orange;
			Item.channel = true;
			Item.noUseGraphic = true;
			Item.autoReuse = true;
		}

		public override ModItem Clone(Item Item)
		{
			ModItem clone = base.Clone(Item);

			if (!(Item.ModItem is TwistSword))
				return clone;

			if (Main.mouseItem.type == ItemType<TwistSword>())
				Item.ModItem.HoldItem(Main.player[Main.myPlayer]);

			(clone as TwistSword).charge = (Item.ModItem as TwistSword).charge;
			(clone as TwistSword).timer = (Item.ModItem as TwistSword).timer;

			return clone;
		}

		public override bool CanUseItem(Player Player)
		{
			return charge > 40 && !Player.channel;
		}

		public override bool? UseItem(Player Player)
		{
			if (Player.whoAmI == Main.myPlayer)
			{
				Projectile.NewProjectile(Player.GetSource_ItemUse(Item), Player.Center, Vector2.Zero, ProjectileType<TwistSwordProjectile>(), Item.damage, Item.knockBack, Player.whoAmI);
				return true;
			}

			return false;
		}

		public override void NetSend(BinaryWriter writer)
		{
			writer.Write(charge);
			writer.Write(timer);
		}

		public override void NetReceive(BinaryReader reader)
		{
			charge = reader.ReadInt32();
			timer = reader.ReadInt32();
		}

		public override void HoldItem(Player Player)
		{
			if (noItemLastFrame && Player.whoAmI == Main.myPlayer && !Player.noItems && Player.channel && CanUseItem(Player))
			{
				//if the Player gets hit by a noItem effect like cursed or forbidden winds dash, the twist sword Projectile will die, but if they continue to hold left click through it we want to resummon the twist sword at the end
				//alteratively we could change the logic to have noItem set Player.channel to false so they have to manually reclick once the effect ends but I think this feels more polished
				bool doesntOwnTwistSwordProj = true;
				for (int i = 0; i < Main.maxProjectiles; i++)
				{
					Projectile proj = Main.projectile[i];
					if (proj.active && proj.owner == Player.whoAmI && proj.type == ModContent.ProjectileType<TwistSwordProjectile>())
					{
						doesntOwnTwistSwordProj = false;
						break;
					}
				}

				if (doesntOwnTwistSwordProj)
					UseItem(Player);
			}

			if (Player.channel && !Player.noItems)
			{
				timer++;

				if (Player.controlJump && timer % 2 == 1)
				{
					charge--;
					timer++;
				}

				Player.fallStart = (int)Player.position.Y / 16;

				if (Player.velocity.Y > 2)
					Player.velocity.Y = 2;

				if (Player.velocity.X < 5 && Player.controlRight)
					Player.velocity.X += 0.2f;

				if (Player.velocity.X > -5 && Player.controlLeft)
					Player.velocity.X -= 0.2f;

				charge--;
			}
			else
			{
				timer = 0;
			}

			if (timer % 20 == 0 && timer > 0)
				Helper.PlayPitched("Magic/WaterWoosh", 0.3f, Main.rand.NextFloat(0.2f, 0.4f), Player.Center);

			if (timer % 20 == 10 && timer > 0)
				Helper.PlayPitched("Magic/WaterWoosh", 0.3f, -0.4f, Player.Center);

			if (charge <= 0)
				Player.channel = false;

			if (charge < 600 && (!Player.channel || Player.noItems))
			{
				if (Player.velocity.Y == 0)
					charge += 10;
				else
					charge += 2;
			}

			if (charge > 600)
				charge = 600;

			noItemLastFrame = Player.noItems;
		}

		public override void UpdateInventory(Player Player)
		{
			if (Player.HeldItem != Item)
			{
				if (charge < 600)
				{
					if (Player.velocity.Y == 0)
						charge += 10;
					else
						charge += 2;
				}
			}
		}
	}

	class TwistSwordChargeBarLayer : PlayerDrawLayer
	{
		public override Position GetDefaultPosition()
		{
			return new AfterParent(PlayerDrawLayers.IceBarrier);
		}

		protected override void Draw(ref PlayerDrawSet drawInfo)
		{

			Player drawPlayer = drawInfo.drawPlayer;

			if (drawPlayer != null && !drawPlayer.HeldItem.IsAir && drawPlayer.HeldItem.type == ItemType<TwistSword>() && PlayerTarget.canUseTarget)
			{
				int charge = (drawPlayer.HeldItem.ModItem as TwistSword).charge;
				Texture2D tex = Request<Texture2D>(AssetDirectory.GUI + "SmallBar1").Value;
				Texture2D tex2 = Request<Texture2D>(AssetDirectory.GUI + "SmallBar0").Value;
				var pos = (drawPlayer.Center + new Vector2(-tex.Width / 2, -40) + Vector2.UnitY * drawPlayer.gfxOffY - Main.screenPosition).ToPoint();
				var target = new Rectangle(pos.X, pos.Y, (int)(charge / 600f * tex.Width), tex.Height);
				var source = new Rectangle(0, 0, (int)(charge / 600f * tex.Width), tex.Height);
				var target2 = new Rectangle(pos.X, pos.Y + 2, tex2.Width, tex2.Height);
				var color = Vector3.Lerp(Color.Red.ToVector3(), Color.Aqua.ToVector3(), charge / 800f);

				Main.spriteBatch.Draw(tex2, target2, new Color(40, 40, 40));
				Main.spriteBatch.Draw(tex, target, source, new Color(color.X, color.Y, color.Z));
			}
		}
	}

	class TwistSwordProjectile : ModProjectile, IDrawPrimitive
	{
		private List<Vector2> cache;
		private Trail trail;

		public override string Texture => AssetDirectory.MiscItem + Name;

		public ref float Rotation => ref Projectile.ai[0];
		public ref float Spinup => ref Projectile.ai[1];

		public override void SetDefaults()
		{
			Projectile.width = 250;
			Projectile.height = 50;
			Projectile.friendly = true;
			Projectile.tileCollide = false;
			Projectile.penetrate = -1;
			Projectile.timeLeft = 2;
			Projectile.extraUpdates = 3;
			Projectile.DamageType = DamageClass.Melee;
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			float rot = Rotation % 80 / 80f * 6.28f;
			float x = (float)Math.Cos(-rot) * 160;
			float y = (float)Math.Sin(-rot) * 70;
			var off = new Vector2(x, y);

			return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center, Projectile.Center + off);
		}

		private void findIfHit()
		{
			foreach (NPC NPC in Main.npc.Where(n => n.active && !n.dontTakeDamage && !n.townNPC && n.life > 0 && n.immune[Projectile.owner] <= 0 && Colliding(Projectile.Hitbox, n.Hitbox) == true))
			{
				OnHitNPC(NPC, 0, 0, false);
			}
		}

		public override void AI()
		{
			Player player = Main.player[Projectile.owner];

			float attackSpeedMult = player.GetAttackSpeed<MeleeDamageClass>();
			float maxSpinup = 400f * attackSpeedMult;

			if (Spinup < maxSpinup)
				Spinup += attackSpeedMult;

			if (Spinup > maxSpinup)
				Spinup = maxSpinup;

			if (!player.controlJump && Spinup > 200f)
				Spinup = 200f;

			Rotation += 0.4f + Spinup * 0.0025f;

			Projectile.Center = player.Center + new Vector2(0, player.gfxOffY);

			if (player.channel && player.HeldItem.type == ItemType<TwistSword>() && !player.noItems)
				Projectile.timeLeft = 2;

			if (Spinup > 200 && player.velocity.Y > -4)
				player.velocity.Y -= 0.0004f * Spinup;

			//visuals
			float rot = Rotation % 80 / 80f * 6.28f;
			float x = (float)Math.Cos(-rot) * 120;
			float y = (float)Math.Sin(-rot) * 40;
			var off = new Vector2(x, y);

			if (rot > 3.14f)
				player.heldProj = Projectile.whoAmI;

			if (Main.rand.NextBool(3))
				Dust.NewDustPerfect(player.Center + off, DustType<Dusts.Glow>(), off * Main.rand.NextFloat(0.01f), 0, new Color(10, 30, 255), Main.rand.NextFloat(0.2f, 0.4f));

			if (Main.rand.NextBool((int)Math.Round(25.0 / attackSpeedMult)))
				Dust.NewDustPerfect(player.Center + off, DustType<Dusts.WaterBubble>(), off * Main.rand.NextFloat(0.01f), 0, new Color(160, 180, 255), Main.rand.NextFloat(0.2f, 0.4f));

			if (player.channel && player.HeldItem.type == ItemType<TwistSword>() && !player.noItems)
				player.UpdateRotation(rot);
			else
				player.UpdateRotation(0);

			Lighting.AddLight(Projectile.Center + off, new Vector3(0.1f, 0.25f, 0.6f));

			if (Main.netMode != NetmodeID.Server)
			{
				ManageCaches();
				ManageTrail();
			}

			if (Main.myPlayer != Projectile.owner)
				findIfHit();
		}

		public override void Kill(int timeLeft)
		{
			//have to reset rotation in multiPlayer when proj is gone
			Player player = Main.player[Projectile.owner];
			player.UpdateRotation(0);
		}

		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
		{
			float rot = Rotation % 80 / 80f * 6.28f;
			Vector2 away = Vector2.UnitX.RotatedBy(rot);

			target.immune[Projectile.owner] = 10; //same as regular pierce Projectile but explicit for multiPlayer compatibility

			target.velocity += away * 8 * target.knockBackResist;

			if (Main.netMode != NetmodeID.Server)
				OnHitEffect(target);
		}

		public void OnHitEffect(NPC target)
		{
			Helper.PlayPitched("Magic/WaterSlash", 0.4f, 0.2f, Projectile.Center);
			Helper.PlayPitched("Magic/WaterWoosh", 0.3f, 0.6f, Projectile.Center);

			float rot = Rotation % 80 / 80f * 6.28f;
			Vector2 away = Vector2.UnitX.RotatedBy(rot);

			for (int k = 0; k < 20; k++)
				Dust.NewDustPerfect(target.Center, DustType<Dusts.Glow>(), away.RotatedByRandom(0.2f) * Main.rand.NextFloat(4), 0, new Color(50, 110, 255), 0.4f);
		}

		public override bool PreDraw(ref Color lightColor)
		{
			float rot = Rotation % 80 / 80f * 6.28f;
			float x = (float)Math.Cos(-rot) * 120;

			Texture2D tex = Request<Texture2D>(Texture).Value;
			Player owner = Main.player[Projectile.owner];

			var target = new Rectangle((int)(owner.Center.X - Main.screenPosition.X), (int)(owner.Center.Y - Main.screenPosition.Y), (int)Math.Abs(x / 120f * tex.Size().Length()), 40);

			Main.spriteBatch.Draw(tex, target, null, lightColor, -rot, new Vector2(0, tex.Height), SpriteEffects.None, default);

			return false;
		}

		private void ManageCaches()
		{
			float rot = Rotation % 80 / 80f * 6.28f;
			float x = (float)Math.Cos(-rot) * 120;
			float y = (float)Math.Sin(-rot) * 40;
			var off = new Vector2(x, y);

			if (cache == null)
			{
				cache = new List<Vector2>();

				for (int i = 0; i < 50; i++)
				{
					cache.Add(Projectile.Center + off);
				}
			}

			cache.Add(Projectile.Center + off);

			while (cache.Count > 50)
			{
				cache.RemoveAt(0);
			}
		}

		private void ManageTrail()
		{
			float rot = Rotation % 80 / 80f * 6.28f;
			float x = (float)Math.Cos(-rot) * 120;
			float y = (float)Math.Sin(-rot) * 40;
			var off = new Vector2(x, y);

			trail ??= new Trail(Main.instance.GraphicsDevice, 50, new TriangularTip(40 * 4), factor => factor * 25, factor =>
			{
				if (factor.X >= 0.98f)
					return Color.White * 0;

				return new Color(50, 30 + (int)(100 * factor.X), 255) * factor.X;
			});

			trail.Positions = cache.ToArray();
			trail.NextPosition = Projectile.Center + Projectile.velocity + off;
		}

		public void DrawPrimitives()
		{
			Effect effect = Filters.Scene["CeirosRing"].GetShader().Shader;

			var world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
			Matrix view = Main.GameViewMatrix.ZoomMatrix;
			var projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

			effect.Parameters["time"].SetValue(Main.GameUpdateCount);
			effect.Parameters["repeats"].SetValue(2f);
			effect.Parameters["transformMatrix"].SetValue(world * view * projection);
			effect.Parameters["sampleTexture"].SetValue(Request<Texture2D>("StarlightRiver/Assets/ShadowTrail").Value);

			trail?.Render(effect);
		}
	}
}
