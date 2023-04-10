using StarlightRiver.Helpers;
using System;
using System.IO;
using Terraria.GameContent;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Items.Gravedigger
{
	class PoltergeistMinion : ModProjectile
	{
		public Item Item;

		private float targetRotation = 0;
		private Vector2 targetPos;
		private float opacity;

		public Player owner => Main.player[Projectile.owner];

		public ref float Timer => ref Projectile.ai[0];
		public ref float State => ref Projectile.ai[1];

		public override string Texture => AssetDirectory.Invisible;

		public override bool? CanCutTiles()
		{
			return false;
		}

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Haunted Weapon");
		}

		public override void SetDefaults()
		{
			Projectile.friendly = true;
			Projectile.minion = true;
			Projectile.minionSlots = 0;
			Projectile.tileCollide = false;
			Projectile.DamageType = DamageClass.Magic;
		}

		public override void Kill(int timeLeft)
		{
			if (owner?.armor[0]?.ModItem != null && owner.armor[0].type == ModContent.ItemType<PoltergeistHead>())
				(owner.armor[0].ModItem as PoltergeistHead).minions.Remove(Projectile);
		}

		public override void AI()
		{

			if (owner.dead && opacity > 0)
				opacity -= 0.05f;
			else if (opacity < 1)
				opacity += 0.05f;

			if (owner.armor[0].type != ItemType<PoltergeistHead>() || !owner.armor[0].ModItem.IsArmorSet(owner.armor[0], owner.armor[1], owner.armor[2]) || !(owner.armor[0].ModItem as PoltergeistHead).minions.Contains(Projectile))
			{
				Projectile.Kill();
				return;
			}

			if (Item != null && !Item.IsAir)
			{
				if (Timer % 120 == 0)
					Projectile.netUpdate = true;

				Timer++;

				var helm = owner.armor[0].ModItem as PoltergeistHead;

				int sleepTimer = helm.sleepTimer;
				int index = helm.minions.IndexOf(Projectile);
				float progress = helm.timer * 0.02f + index / (float)helm.minions.Count * 6.28f;

				Projectile.timeLeft = 2;
				targetPos = owner.Center + new Vector2(0, -100 + (float)Math.Sin(progress * 3.4f) * 20) + new Vector2((float)Math.Cos(progress) * 100, (float)Math.Sin(progress) * 40);

				if (sleepTimer <= 0) //fall asleep
				{
					targetRotation = 1.57f + (Item.staff[Item.type] ? 1.57f / 2 : 0);
					targetPos = owner.Center + new Vector2(0, -70) + new Vector2((float)Math.Cos(progress) * 35, (float)Math.Sin(progress) * 10);
				}

				Projectile.Center += (targetPos - Projectile.Center) * 0.05f;

				if (Math.Abs(Helper.CompareAngle(targetRotation, Projectile.rotation)) > 0.1f)
					Projectile.rotation += Helper.CompareAngle(targetRotation, Projectile.rotation) * 0.05f;
				else
					Projectile.rotation = targetRotation;

				if (sleepTimer > 0 && !owner.dead && (int)Timer % (Item.useTime * 2) == 0 && TryFindTarget(out NPC target))
				{
					float rot = (target.Center - Projectile.Center).ToRotation();

					if (Main.myPlayer == owner.whoAmI)
						Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.UnitX.RotatedBy(rot) * Item.shootSpeed, Item.shoot, Item.damage / 2, Item.knockBack, Projectile.owner);

					if (Item.UseSound.HasValue)
						Terraria.Audio.SoundEngine.PlaySound(Item.UseSound.Value, Projectile.Center);

					targetRotation = rot + (Item.staff[Item.type] ? 1.57f / 2 : 0);
				}
			}
		}

		public override void SendExtraAI(BinaryWriter writer)
		{
			if (Item != null)
				writer.Write(Item.type);
			else
				writer.Write(-1);

			writer.Write((owner.armor[0].ModItem as PoltergeistHead).sleepTimer);

		}

		public override void ReceiveExtraAI(BinaryReader reader)
		{
			int ItemType = reader.ReadInt32();
			if (ItemType == -1)
			{
				Item = null;
			}
			else
			{
				Item = new Item();
				Item.SetDefaults(ItemType);
			}

			int sleepTimer = reader.ReadInt32();

			if (owner?.armor[0]?.ModItem != null)
			{
				(owner.armor[0].ModItem as PoltergeistHead).sleepTimer = sleepTimer;
				if (Projectile.active && !(owner.armor[0].ModItem as PoltergeistHead).minions.Contains(Projectile))
					(owner.armor[0].ModItem as PoltergeistHead).minions.Add(Projectile);
			}
		}

		private bool TryFindTarget(out NPC NPC)
		{
			for (int k = 0; k < Main.maxNPCs; k++)
			{
				NPC thisNPC = Main.npc[k];

				if (thisNPC.active && thisNPC.CanBeChasedBy(this) && Vector2.Distance(thisNPC.Center, owner.Center) < 500 && Collision.CanHitLine(Projectile.Center, 1, 1, thisNPC.position, thisNPC.width, thisNPC.height))
				{
					NPC = thisNPC;
					return true;
				}
			}

			NPC = null;
			return false;
		}

		public override bool PreDraw(ref Color lightColor)
		{
			if (Item != null && !Item.IsAir)
			{

				Texture2D tex = TextureAssets.Item[Item.type].Value;
				//var frames = Main.itemFrame[Item.type];

				Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, lightColor * opacity, Projectile.rotation, tex.Size() / 2, 1, 0, 0);
			}

			return false;
		}
	}
}