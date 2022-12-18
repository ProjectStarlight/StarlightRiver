using StarlightRiver.Core.Loaders.UILoading;
using StarlightRiver.Core.Systems.CombatMountSystem;
using System.Collections.Generic;
using Terraria.UI;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.GUI
{
	public class CombatMountInfo : SmartUIState
	{
		public override bool Visible => !Main.playerInventory && ModPlayer.activeMount != null;

		CombatMountPlayer ModPlayer => Main.LocalPlayer.GetModPlayer<CombatMountPlayer>();

		public override int InsertionIndex(List<GameInterfaceLayer> layers)
		{
			return layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			var pos = new Vector2(480, 24);
			Texture2D tex = Request<Texture2D>(AssetDirectory.GUI + "MountAbilitySlot").Value;

			Texture2D icon1 = Request<Texture2D>(ModPlayer.activeMount.PrimaryIconTexture).Value;
			Texture2D icon2 = Request<Texture2D>(ModPlayer.activeMount.SecondaryIconTexture).Value;

			spriteBatch.Draw(tex, pos, Color.White);
			spriteBatch.Draw(icon1, pos + Vector2.One * 22, null, Color.White, 0, icon1.Size() / 2, 1, 0, 0);

			var target = new Rectangle((int)pos.X, (int)pos.Y, tex.Width, (int)(tex.Height * (ModPlayer.activeMount.primaryCooldownTimer / (float)ModPlayer.activeMount.MaxPrimaryCooldown)));
			var source = new Rectangle(0, 0, tex.Width, target.Height);
			spriteBatch.Draw(tex, target, source, Color.Black * 0.5f);

			pos.X += 50;

			spriteBatch.Draw(tex, pos, Color.White);
			spriteBatch.Draw(icon2, pos + Vector2.One * 22, null, Color.White, 0, icon2.Size() / 2, 1, 0, 0);

			target = new Rectangle((int)pos.X, (int)pos.Y, tex.Width, (int)(tex.Height * (ModPlayer.activeMount.secondaryCooldownTimer / (float)ModPlayer.activeMount.MaxSecondaryCooldown)));
			source = new Rectangle(0, 0, tex.Width, target.Height);
			spriteBatch.Draw(tex, target, source, Color.Black * 0.5f);
		}

		public override void Update(GameTime gameTime)
		{

		}
	}
}