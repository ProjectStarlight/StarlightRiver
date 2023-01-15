//TODO:
//Obtainment
//Sellprice
//Rarity
//Balance
using ReLogic.Content;
using System.Collections.Generic;
using Terraria.Audio;
using Terraria.GameContent.Creative;
using Terraria.ID;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;

namespace StarlightRiver.Content.Items.Misc
{
	public class ThunderBeads : ModItem
	{
		public override string Texture => AssetDirectory.MiscItem + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Thunder beads");
			Tooltip.SetDefault("Whip enemies to stick the beads in them \nRepeatedly click to shock affected enemies");
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
		}

		public override void SetDefaults()
		{
			Item.DefaultToWhip(ModContent.ProjectileType<ThunderBeads_Whip>(), 15, 1.2f, 5f, 25);
			Item.value = Item.sellPrice(0, 1, 0, 0);
			Item.rare = ItemRarityID.Green;
		}
	}

	public class ThunderBeads_Whip : BaseWhip
	{
		public NPC target = default;

		public bool embedded = false;

		public int embedTimer = 150;

		public bool ableToHit = false;
		public bool leftClick = false;

		public override string Texture => AssetDirectory.MiscItem + Name;

		public ThunderBeads_Whip() : base("Thunder Beads", 15, 0.87f, Color.Cyan)
		{
			xFrames = 1;
			yFrames = 5;
		}

		public override int SegmentVariant(int segment)
		{
			return 1;
		}

		public override bool PreAI()
		{
			Projectile.localNPCHitCooldown = 1;
			if (embedded)
			{
				if (!leftClick && Main.mouseLeft)
				{
					ableToHit = true;
					leftClick = true;
				}

				if (!Main.mouseLeft)
					leftClick = false;

				Player player = Main.player[Projectile.owner];
				flyTime = player.itemAnimationMax * Projectile.MaxUpdates;
				Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
				Projectile.Center = Main.GetPlayerArmPosition(Projectile) + Projectile.velocity * (Projectile.ai[0] - 1f);
				Projectile.spriteDirection = (!(Vector2.Dot(Projectile.velocity, Vector2.UnitX) < 0f)) ? 1 : -1;

				player.heldProj = Projectile.whoAmI;
				player.itemAnimation = player.itemAnimationMax - (int)(Projectile.ai[0] / Projectile.MaxUpdates);
				player.itemTime = player.itemAnimation;

				embedTimer--;
				if (embedTimer < 0 || !target.active)
				{
					Projectile.friendly = false;
					embedded = false;
					return false;
				}

				Projectile.WhipPointsForCollision.Clear();
				SetPoints(Projectile.WhipPointsForCollision);
				return false;
			}
			return base.PreAI();
		}

		public override void ArcAI()
		{
			xFrame = 0;
		}

		public override bool ShouldDrawSegment(int segment)
		{
			return segment % 3 == 0;
		}

		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
		{
			ableToHit = false;
			if (!embedded)
			{
				this.target = target;
				embedded = true;
			}
		}

		public override bool? CanHitNPC(NPC target)
		{
			if (embedded)
				return target == this.target && ableToHit;
			return base.CanHitNPC(target);
		}

		public override void SetPoints(List<Vector2> controlPoints)
		{
			if (embedded)
			{
				Player player = Main.player[Projectile.owner];
				Item heldItem = player.HeldItem;
				Vector2 playerArmPosition = Main.GetPlayerArmPosition(Projectile);
				for (int i = 0; i < segments + 1; i++)
				{
					float lerper = i / (float)segments;
					controlPoints.Add(Vector2.Lerp(playerArmPosition, target.Center, lerper));
				}
			}
			else
				base.SetPoints(controlPoints);
		}
	}
}