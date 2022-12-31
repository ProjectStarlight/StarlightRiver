using System;

namespace StarlightRiver.Content.ArmorEnchantment
{
	class DebugEnchant2 : ArmorEnchantment
	{
		public DebugEnchant2() : base() { }

		public DebugEnchant2(Guid guid) : base(guid) { }

		public override string Texture => AssetDirectory.ArmorEnchant + "DebugEnchant";

		public override bool IsAvailable(Item head, Item chest, Item legs)
		{
			return true;
		}

		public override void UpdateSet(Player Player)
		{
			Player.setBonus = "Acts as a second button to test the UI";
		}

		public override void DrawInInventory(Item Item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color ItemColor, Vector2 origin, float scale)
		{

		}

		public override bool PreDrawInInventory(Item Item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color ItemColor, Vector2 origin, float scale)
		{
			spriteBatch.End();
			spriteBatch.Begin(default, BlendState.Additive, SamplerState.PointClamp, default, default, default, Main.UIScaleMatrix);

			Texture2D tex = Terraria.GameContent.TextureAssets.Item[Item.type].Value;

			for (int k = 0; k < 3; k++)
			{
				spriteBatch.Draw(tex, position + tex.Size() * 0.5f * scale, frame, new Color(0.5f, 0.8f, 1f) * 0.55f, 0, frame.Size() * 0.5f, scale * 1.3f + 0.1f * (float)Math.Sin(StarlightWorld.visualTimer + k), SpriteEffects.None, 0);
			}

			spriteBatch.End();
			spriteBatch.Begin(default, default, SamplerState.PointClamp, default, default, default, Main.UIScaleMatrix);

			return true;
		}
	}
}
