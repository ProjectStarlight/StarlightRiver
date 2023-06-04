using StarlightRiver.Content.Items.Vitric;
using StarlightRiver.Core.Systems.BarrierSystem;
using Terraria.ID;

namespace StarlightRiver.Content.Items.BarrierDye
{
	class VitricBossBarrierDye : BarrierDye
	{
		public override string Texture => AssetDirectory.BarrierDyeItem + Name;

		public override float RechargeAnimationRate => 0.01f;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Sentinel's Tincture");
			Tooltip.SetDefault("Causes barrier effect to mimic the shield of the Shattered Sentinel\nEquipable\nVanity Item");
		}

		public override void SetDefaults()
		{
			Item.rare = ItemRarityID.Orange;
		}

		public override void LoseBarrierEffects(Player Player)
		{
			Terraria.Audio.SoundEngine.PlaySound(Terraria.ID.SoundID.Item107, Player.Center);

			for (int k = 0; k < 20; k++)
				Dust.NewDustPerfect(Player.Center, ModContent.DustType<Dusts.GlassGravity>(), Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(6f), 0, default, 2);
		}

		public override void PostDrawEffects(SpriteBatch spriteBatch, Player Player)
		{
			if (!CustomHooks.PlayerTarget.canUseTarget)
				return;

			BarrierPlayer barrier = Player.GetModPlayer<BarrierPlayer>();

			Texture2D tex = CustomHooks.PlayerTarget.Target;

			Vector2 pos = CustomHooks.PlayerTarget.getPositionOffset(Player.whoAmI);

			Effect effect = Terraria.Graphics.Effects.Filters.Scene["MoltenFormAndColor"].GetShader().Shader;
			effect.Parameters["sampleTexture2"].SetValue(ModContent.Request<Texture2D>("StarlightRiver/Assets/Bosses/VitricBoss/ShieldMap").Value);
			effect.Parameters["uTime"].SetValue(barrier.rechargeAnimationTimer * 2 + (barrier.rechargeAnimationTimer >= 1 ? Main.GameUpdateCount / 30f % 2f : 0));
			effect.Parameters["sourceFrame"].SetValue(new Vector4((int)pos.X - 30, (int)pos.Y - 60, 60, 120));
			effect.Parameters["texSize"].SetValue(tex.Size());

			spriteBatch.End();
			spriteBatch.Begin(default, BlendState.NonPremultiplied, SamplerState.PointClamp, default, default, effect, Main.GameViewMatrix.ZoomMatrix);

			spriteBatch.Draw(tex, CustomHooks.PlayerTarget.getPlayerTargetPosition(Player.whoAmI), CustomHooks.PlayerTarget.getPlayerTargetSourceRectangle(Player.whoAmI), Color.White);

			spriteBatch.End();
			spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.TransformationMatrix);
		}
		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient<VitricOre>(5);
			recipe.AddTile(TileID.DyeVat);
			recipe.Register();
		}
	}
}