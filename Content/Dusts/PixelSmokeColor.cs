using StarlightRiver.Core.Systems.PixelationSystem;

namespace StarlightRiver.Content.Dusts
{
	public class PixelSmokeColor : ModDust
	{
		private static readonly Asset<Texture2D>[] variants =
		{
			Assets.SmokeTransparent_1,
			Assets.SmokeTransparent_2,
			Assets.SmokeTransparent_3
		};

		public override string Texture => AssetDirectory.Invisible;

		public override void OnSpawn(Dust dust)
		{
			dust.frame = new Rectangle(0, 0, 4, 4);
		}

		public override bool Update(Dust dust)
		{
			if (dust.customData is not object[]) // i LOVE dusts :)
			{
				object[] pair = [dust.customData, 1 + Main.rand.Next(3)];
				dust.customData = pair;
			}

			dust.velocity.Y -= 0.015f;
			dust.position += dust.velocity;
			dust.velocity *= 0.95f;
			dust.rotation += dust.velocity.Length() * 0.01f;

			dust.alpha += 4;

			dust.alpha = (int)(dust.alpha * 1.0075f);

			dust.scale *= 1.03f;

			if (dust.alpha >= 255)
				dust.active = false;

			return false;
		}

		public override bool PreDraw(Dust dust)
		{
			float lerper = 1f - dust.alpha / 255f;

			Color? fadeColor = null;

			object[] pair = (object[])dust.customData;

			int variant = (int)pair[1];

			if (pair[0] is Color color_)
				fadeColor = color_;

			Color color = Color.Lerp(dust.color, fadeColor ?? Color.Black, Eases.EaseQuinticInOut(1f - lerper));

			if (variant < 1 || variant > 3)
				return false;

			Texture2D tex = variants[variant - 1].Value;
			ModContent.GetInstance<PixelationSystem>().QueueRenderAction("Dusts", () =>
			{
				Main.spriteBatch.Draw(tex, dust.position - Main.screenPosition, null, color * lerper, dust.rotation, tex.Size() / 2f, dust.scale, 0f, 0f);
				Main.spriteBatch.Draw(tex, dust.position - Main.screenPosition, null, color * lerper, dust.rotation + MathHelper.PiOver2, tex.Size() / 2f, dust.scale, 0f, 0f);
			});

			return false;
		}
	}
}