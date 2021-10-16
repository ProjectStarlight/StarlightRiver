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
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Items.Gravedigger
{
	class PoltergeistMinion : ModProjectile
	{
		public Item item;

		private float targetRotation = 0;
		private Vector2 targetPos;
		private float opacity;

		public Player owner => Main.player[projectile.owner];

		public ref float Timer => ref projectile.ai[0];
		public ref float State => ref projectile.ai[1];

		public override string Texture => AssetDirectory.Invisible;

		public override bool? CanCutTiles() => false;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Haunted Weapon");
		}

		public override void SetDefaults()
		{
			projectile.friendly = true;
			projectile.minion = true;
			projectile.minionSlots = 0;
			projectile.tileCollide = false;
			projectile.magic = true;
		}

		public override void AI()
		{
			Timer++;

			if (owner.dead && opacity > 0)
				opacity -= 0.05f;
			else if (opacity < 1)
				opacity += 0.05f;

			if (owner.armor[0].type != ItemType<PoltergeistHead>() || !(owner.armor[0].modItem as PoltergeistHead).minions.Contains(projectile))
			{
				projectile.active = false;
				return;
			}

			if (item != null && !item.IsAir)
			{
				var helm = (owner.armor[0].modItem as PoltergeistHead);

				int sleepTimer = helm.sleepTimer;
				float progress = owner.GetModPlayer<StarlightPlayer>().Timer * 0.02f + helm.minions.IndexOf(projectile) / (float)helm.minions.Count * 6.28f;

				projectile.timeLeft = 2;
				targetPos = owner.Center + new Vector2(0, -100 + (float)Math.Sin(progress * 3.4f) * 20) + new Vector2((float)Math.Cos(progress) * 100, (float)Math.Sin(progress) * 40);

				if (sleepTimer <= 0) //fall asleep
				{
					targetRotation = 1.57f + (Item.staff[item.type] ? 1.57f / 2 : 0);
					targetPos = owner.Center + new Vector2(0, -70) + new Vector2((float)Math.Cos(progress) * 35, (float)Math.Sin(progress) * 10);
				}

				projectile.Center += (targetPos - projectile.Center) * 0.05f;

				if (Math.Abs(Helper.CompareAngle(targetRotation, projectile.rotation)) > 0.1f)
					projectile.rotation += Helper.CompareAngle(targetRotation, projectile.rotation) * 0.05f;
				else
					projectile.rotation = targetRotation;

				if (sleepTimer > 0 && !owner.dead && (int)Timer % (item.useTime * 2) == 0 && TryFindTarget(out NPC target))
				{
					float rot = (target.Center - projectile.Center).ToRotation();

					Projectile.NewProjectile(projectile.Center, Vector2.UnitX.RotatedBy(rot) * item.shootSpeed, item.shoot, item.damage / 2, item.knockBack, projectile.owner);
					Main.PlaySound(item.UseSound, projectile.Center);
					targetRotation = rot + (Item.staff[item.type] ? 1.57f / 2 : 0);
				}
			}
		}

		private bool TryFindTarget(out NPC npc)
		{
			for(int k = 0; k < Main.maxNPCs; k++)
			{
				var thisNPC = Main.npc[k];

				if (thisNPC.active && thisNPC.CanBeChasedBy(this) && Vector2.Distance(thisNPC.Center, owner.Center) < 500 && Collision.CanHitLine(projectile.Center, 1, 1, thisNPC.position, thisNPC.width, thisNPC.height))
				{
					npc = thisNPC;
					return true;
				}
			}

			npc = null;
			return false;
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
		{
			if (item != null && !item.IsAir)
			{
				var tex = Main.itemTexture[item.type];
				//var frames = Main.itemFrame[item.type];

				spriteBatch.Draw(tex, projectile.Center - Main.screenPosition, null, lightColor * opacity, projectile.rotation, tex.Size() / 2, 1, 0, 0);
			}

			return false;
		}
	}
}
