using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using StarlightRiver.Core;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using Microsoft.Xna.Framework.Graphics;
using Terraria.ID;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using StarlightRiver.Content.Items.Moonstone;

namespace StarlightRiver.Content.Items.BarrierDye
{
	class MoonstoneBarrierDye : BarrierDye
	{
		public override string Texture => AssetDirectory.BarrierDyeItem + Name;

		public override float RechargeAnimationRate => 0.01f;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Moonstone Tincture");
			Tooltip.SetDefault("Causes barrier to reflect the light of the moon\nEquipable\nVanity Item");
		}

		public override void SetDefaults()
		{
			Item.rare = ItemRarityID.Orange;
		}

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ModContent.ItemType<MoonstoneBarItem>(), 6);
            recipe.AddTile(TileID.Anvils);
            recipe.Register();
        }

        public override void PostDrawEffects(SpriteBatch spriteBatch, Player Player)
        {
			if (!CustomHooks.PlayerTarget.canUseTarget)
				return;

			var barrier = Player.GetModPlayer<BarrierPlayer>();

			Texture2D tex = CustomHooks.PlayerTarget.Target;

			var pos = CustomHooks.PlayerTarget.getPositionOffset(Player.whoAmI);

			if (barrier.Barrier <= 0)
				return;
            Effect effect = Filters.Scene["MoonstoneRunes"].GetShader().Shader;
            effect.Parameters["intensity"].SetValue(10f * MathF.Min(barrier.RechargeAnimationTimer, 1));
            effect.Parameters["time"].SetValue((float)Main.timeForVisualEffects * 0.1f);

            effect.Parameters["noiseTexture1"].SetValue(ModContent.Request<Texture2D>(AssetDirectory.Assets + "Noise/MiscNoise3").Value);
            effect.Parameters["noiseTexture2"].SetValue(ModContent.Request<Texture2D>(AssetDirectory.Assets + "Noise/MiscNoise4").Value);
            effect.Parameters["color1"].SetValue(Color.Magenta.ToVector4());
            effect.Parameters["color2"].SetValue(Color.Cyan.ToVector4());
            effect.Parameters["opacity"].SetValue(1);

            effect.Parameters["screenWidth"].SetValue(tex.Width);
            effect.Parameters["screenHeight"].SetValue(tex.Width);
            effect.Parameters["screenPosition"].SetValue(CustomHooks.PlayerTarget.getPlayerTargetPosition(Player.whoAmI));
            effect.Parameters["drawOriginal"].SetValue(false);

            spriteBatch.End();
            spriteBatch.Begin(default, BlendState.Additive, default, default, default, effect);

			Rectangle rect = CustomHooks.PlayerTarget.getPlayerTargetSourceRectangle(Player.whoAmI);
            spriteBatch.Draw(tex, CustomHooks.PlayerTarget.getPlayerTargetPosition(Player.whoAmI), rect, Color.White);

            spriteBatch.End();
            spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.ZoomMatrix);
		}
	}


}
