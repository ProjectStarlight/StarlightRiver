using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Abilities;
using StarlightRiver.Core;
using StarlightRiver.Helpers;

using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Items.Moonstone
{
	public class CrescentQuarterstaff : ModItem
	{
		public override string Texture => AssetDirectory.MoonstoneItem + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Crescent Quarterstaff");
			Tooltip.SetDefault("Update this egshels");
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
			item.shoot = ProjectileType<CrescentQuarterstaffProj>();
			item.noUseGraphic = true;
			item.noMelee = true;
			item.autoReuse = false;
		}
	}

	internal class CrescentQuarterstaffProj : ModProjectile
	{ 
		public override string Texture => AssetDirectory.MoonstoneItem + Name;

		enum CurrentAttack : int
		{
			Down = 0,
			FirstUp = 1,
			Spin = 2,
			SecondUp = 3,
			Slam = 4,
			Reset = 5
		}

		private const int LENGTH = 100;


		private CurrentAttack currentAttack;

		private int slashWindow;

		private bool initialized = false;

		private float angularVelocity = 0;

		private int attackDuration;

		private Vector2 direction;

		private bool FacingRight => (projectile.rotation < -1.57f && projectile.rotation > -4.71f) || (projectile.rotation > 1.57f && projectile.rotation < 4.71f);


		private bool FirstTickOfSwing
		{
			get => projectile.ai[0] == 0;
			set => projectile.ai[0] = value ? 0 : 1;
		}

		Player Player => Main.player[projectile.owner];


		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Crescent Quarterstaff");
			Main.projFrames[projectile.type] = 1;
		}

		public override void SetDefaults()
		{
			projectile.friendly = true;
			projectile.melee = true;
			projectile.tileCollide = false;
			projectile.Size = new Vector2(130, 130);
			projectile.penetrate = -1;
			projectile.usesLocalNPCImmunity = true;
			projectile.localNPCHitCooldown = 16;
			projectile.ownerHitCheck = true;
		}

		public override void AI()
		{
			projectile.velocity = Vector2.Zero;
			projectile.Center = Player.Center + (projectile.rotation.ToRotationVector2() * LENGTH);

			AdjustPlayer();

			if (FirstTickOfSwing)
			{
				direction = Player.DirectionTo(Main.MouseWorld);
				slashWindow = 30;
				angularVelocity = 0;
				if (initialized)
					currentAttack++;
				else
					initialized = true;
				if (currentAttack == CurrentAttack.Reset)
					projectile.active = false;

				switch (currentAttack)
                {
					case CurrentAttack.Down:
					{
						attackDuration = 30;
						break;
					}
					case CurrentAttack.FirstUp:
					{
						attackDuration = 30;
						break;
					}
					case CurrentAttack.Spin:
					{
						attackDuration = 50;
						break;
					}
					case CurrentAttack.SecondUp:
					{
						attackDuration = 30;
						break;
					}
					case CurrentAttack.Slam:
					{
						attackDuration = 15;
						break;
					}
				}
				FirstTickOfSwing = false;
			}

			attackDuration--;
			if (attackDuration > 0)
			{
				switch (currentAttack)
				{
					case CurrentAttack.Down:
					{
						break;
					}
					case CurrentAttack.FirstUp:
					{
						break;
					}
					case CurrentAttack.Spin:
					{
						break;
					}
					case CurrentAttack.SecondUp:
					{
						break;
					}
					case CurrentAttack.Slam:
					{
						break;
					}
				}

				projectile.rotation += angularVelocity;
			}
			else
            {
				slashWindow--;

				if (Player == Main.LocalPlayer)
				{
					if (Main.mouseLeft)
                    {
						FirstTickOfSwing = true;
                    }
				}

				if (slashWindow < 0)
					projectile.active = false;
			}
		}

		private void AdjustPlayer()
        {
			Player.ChangeDir(FacingRight ? 1 : -1);
			Player.itemRotation = MathHelper.WrapAngle(projectile.rotation - ((Player.direction < 0) ? 0 : MathHelper.Pi));
			Player.itemAnimation = Player.itemTime = 5;
		}
    }
}