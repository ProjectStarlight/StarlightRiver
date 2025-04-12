using StarlightRiver.Core.Systems.InstancedBuffSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;

namespace StarlightRiver.Content.Items.BaseTypes
{
	internal abstract class BaseTalisman<T, T2> : ModItem where T : BaseTalismanProjectile<T2> where T2 : BaseTalismanBuff, new()
	{
		public float spread = 0.15f;

		public virtual void SafeSetDefaults() { }

		public sealed override void SetDefaults()
		{
			Item.width = 32;
			Item.height = 32;
			Item.DamageType = DamageClass.Magic;
			Item.consumable = true;
			Item.shoot = ModContent.ProjectileType<T>();
			Item.shootSpeed = 1;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.maxStack = 9999;
			Item.noUseGraphic = true;
			Item.noMelee = true;

			SafeSetDefaults();
		}

		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
		{
			velocity = velocity.RotatedByRandom(spread) * Main.rand.NextFloat(0.8f, 1.1f);
		}
	}

	internal abstract class BaseTalismanProjectile<T> : ModProjectile where T : BaseTalismanBuff, new()
	{
		public int duration = 300;

		public int flipTimer;
		public int maxFlip;
		public int direction;
		public bool hitTile;

		public override void SetStaticDefaults()
		{
			Main.projFrames[Type] = 6;
		}

		public override void SetDefaults()
		{
			Projectile.width = 28;
			Projectile.height = 22;
			Projectile.timeLeft = 300;
			Projectile.friendly = true;
		}

		public override void AI()
		{
			if (Projectile.timeLeft < 60)
				Projectile.alpha = 255 - (int)(Projectile.timeLeft / 60f * 255);

			if (hitTile)
				return;

			if (direction == 0)
				direction = Projectile.velocity.X > 0 ? 1 : -1;

			if (flipTimer > 0)
			{
				Projectile.velocity = Projectile.velocity.RotatedBy(6.28f / (float)maxFlip * -direction);
				flipTimer--;
			}
			else if (Main.rand.NextBool(100))
			{
				maxFlip = Main.rand.Next(26, 48);
				flipTimer = maxFlip;
			}

			Projectile.rotation = Projectile.velocity.ToRotation();

			Projectile.velocity += Vector2.UnitY.RotatedBy(Projectile.rotation) * (float)Math.Sin(Projectile.timeLeft / 5f) * 0.25f;
			Projectile.velocity += Vector2.UnitY.RotatedBy(Projectile.rotation) * (float)Math.Sin(Projectile.timeLeft / 10f + 2) * 0.1f;

			if (Projectile.frameCounter++ > (4 - (int)Projectile.velocity.Length() / 6f))
			{
				Projectile.frameCounter = 0;
				Projectile.frame++;
				Projectile.frame %= 6;
			}
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			BuffInflictor.Inflict<T>(target, duration);
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			if (hitTile)
				return false;

			hitTile = true;

			Projectile.frame = 2;
			Projectile.velocity *= -0.05f;
			Projectile.timeLeft = 60;
			Projectile.extraUpdates = 2;
			return false;
		}
	}

	internal abstract class BaseTalismanBuff : StackableBuff
	{
		public int threshold = 5;

		public int triggerAnimTime;

		public Asset<Texture2D> texture;

		public override void Load()
		{
			texture = ModContent.Request<Texture2D>(Texture);
			StarlightNPC.PostDrawEvent += DrawTalisman;
		}

		private void DrawTalisman(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			if (AnyInflicted(npc))
			{
				var buff = GetInstance(npc) as BaseTalismanBuff;

				Texture2D tex = texture.Value;
				Vector2 pos = npc.Center + Vector2.UnitY * (npc.height / 2 + 32) * -1 - Main.screenPosition;

				var rand = new Random(npc.whoAmI);

				if (buff.triggerAnimTime > 0)
				{
					for (int k = 0; k < buff.threshold; k++)
					{
						Vector2 off = new Vector2(rand.Next(npc.width), rand.Next(npc.height));
						Vector2 pos2 = npc.position + off - Main.screenPosition;
						spriteBatch.Draw(tex, pos2, null, drawColor * (buff.triggerAnimTime / 15f), 0, tex.Size() / 2f, 1f + (1 - buff.triggerAnimTime / 15f), 0, 0);
					}
				}
				else
				{
					for (int k = 0; k < buff.stacks.Count; k++)
					{
						Vector2 off = new Vector2(rand.Next(npc.width), rand.Next(npc.height));
						Vector2 pos2 = npc.position + off - Main.screenPosition;
						spriteBatch.Draw(tex, pos2, null, drawColor, 0, tex.Size() / 2f, 1f, 0, 0);
					}
				}
			}
		}

		public override void AnyStacksUpdateNPC(NPC npc)
		{
			if (triggerAnimTime > 0)
				triggerAnimTime--;

			if (stacks.Count >= threshold)
			{
				OnMaxStacks(npc);

				triggerAnimTime = 15;
				for (int k = 0; k < threshold; k++)
				{
					stacks.RemoveAt(0);
				}
			}
		}

		public virtual void OnMaxStacks(NPC npc)
		{

		}
	}
}