using ReLogic.Content;
using StarlightRiver.Content.Items.BaseTypes;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Jungle
{
	public class Slitherring : SmartAccessory
	{
		public override string Texture => AssetDirectory.JungleItem + Name;

		public Slitherring() : base("Slitherring", "A small snake occasionally attacks with you when you use a whip") { }

		public override void Load()
		{
			StarlightItem.ShootEvent += ShootSnake;
		}

		public override void SafeSetDefaults()
		{
			Item.value = Item.sellPrice(0, 0, 40, 0);
			Item.rare = ItemRarityID.Orange;
		}

		private bool ShootSnake(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			if (Equipped(player) && ProjectileID.Sets.IsAWhip[type] && Main.rand.NextBool())
			{
				var proj = Projectile.NewProjectileDirect(source, position, velocity * 0.75f, ModContent.ProjectileType<SlitherringWhip>(), (int)(damage * 0.5f), knockback, player.whoAmI, -1);
				proj.originalDamage = damage / 2;
			}

			return true;
		}
	}

	public class SlitherringWhip : BaseWhip
	{
		public override string Texture => AssetDirectory.JungleItem + "SlitherringWhip";

		public SlitherringWhip() : base("Slither Whip", 15, 0.75f, Color.DarkGreen) { }

		public override int SegmentVariant(int segment)
		{
			int variant = segment switch
			{
				5 or 6 or 7 or 8 => 1,
				9 or 10 or 11 or 12 or 13 => 3,
				_ => 2,
			};
			return variant;
		}

		public override bool ShouldDrawSegment(int segment)
		{
			return true;
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			List<Vector2> points = Projectile.WhipPointsForCollision;
			for (int i = 0; i < points.Count - 1; i++)
			{
				float collisionPoint = 0f;
				if (Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), points[i], points[i + 1], 8, ref collisionPoint))
					return true;
			}

			return false;
		}

		public override bool PreAI()
		{
			ProjectileID.Sets.IsAWhip[Projectile.type] = false;
			Player player = Main.player[Projectile.owner];
			flyTime = player.itemAnimationMax * Projectile.MaxUpdates * 2;
			if (Projectile.ai[0] == -1)
				Projectile.ai[0] = flyTime;

			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
			Projectile.ai[0]--;
			Projectile.Center = Main.GetPlayerArmPosition(Projectile) + Projectile.velocity * (Projectile.ai[0] - 1f);
			Projectile.spriteDirection = !(Vector2.Dot(Projectile.velocity, Vector2.UnitX) < 0f) ? 1 : -1;
			if (Projectile.ai[0] <= 0 || player.itemAnimation == 0)
			{
				Projectile.Kill();
				return false;
			}

			Projectile.WhipPointsForCollision.Clear();
			SetPoints(Projectile.WhipPointsForCollision);
			ArcAI();
			return false;
		}

		public override void PostAI()
		{

			base.PostAI();
		}

		public override bool PreDraw(ref Color lightColor)
		{
			DrawBehindWhip(ref lightColor);
			var points = new List<Vector2>();
			points.Clear();
			SetPoints(points);

			//points = Projectile.WhipPointsForCollision;

			//string
			Vector2 stringPoint = points[0];
			for (int i = 0; i < points.Count - 2; i++)
			{
				Vector2 nextPoint = points[i + 1] - points[i];
				Color color = stringColor.MultiplyRGBA(Projectile.GetAlpha(Lighting.GetColor(points[i].ToTileCoordinates())));
				var scale = new Vector2(1f, (nextPoint.Length() + 2f) / TextureAssets.FishingLine.Height());
				Main.EntitySpriteDraw(TextureAssets.FishingLine.Value, stringPoint - Main.screenPosition, null, color, nextPoint.ToRotation() - MathHelper.PiOver2, new Vector2(TextureAssets.FishingLine.Width() * 0.5f, 2f), scale, SpriteEffects.None, 0);
				stringPoint += nextPoint;
			}

			//whip
			Asset<Texture2D> texture = ModContent.Request<Texture2D>(Texture);
			Rectangle whipFrame = texture.Frame(1, 5, 0, 0);
			int height = whipFrame.Height;
			Vector2 firstPoint = points[0];
			for (int i = 0; i < points.Count - 1; i++)
			{
				Vector2 origin = whipFrame.Size() * 0.5f;
				bool draw = true;
				if (i == 0)
				{
					origin.Y += handleOffset;
				}
				else if (i == points.Count - 2)
				{
					whipFrame.Y = height * 4;
				}
				else
				{
					whipFrame.Y = height * SegmentVariant(i);
					draw = ShouldDrawSegment(i);
				}

				Vector2 difference = points[i + 1] - points[i];
				if (draw)
				{
					Color alpha = Projectile.GetAlpha(Lighting.GetColor(points[i].ToTileCoordinates()));
					float rotation = difference.ToRotation() - MathHelper.PiOver2;
					Main.EntitySpriteDraw(texture.Value, points[i] - Main.screenPosition, whipFrame, alpha, rotation, origin, Projectile.scale, SpriteEffects.None, 0);
				}

				firstPoint += difference;
			}

			return false;
		}

		public override Color? GetAlpha(Color lightColor)
		{
			Color minLight = lightColor;
			var minColor = new Color(10, 25, 33);
			if (minLight.R < minColor.R)
				minLight.R = minColor.R;
			if (minLight.G < minColor.G)
				minLight.G = minColor.G;
			if (minLight.B < minColor.B)
				minLight.B = minColor.B;
			return minLight;
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			target.AddBuff(BuffID.Poisoned, Main.rand.Next(60, 240));
		}
	}
}