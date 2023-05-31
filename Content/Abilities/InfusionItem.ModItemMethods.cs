using System;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Abilities
{
	public abstract partial class InfusionItem : ModItem
	{
		public override string Texture => "StarlightRiver/Assets/Invisible";
		public virtual string FrameTexture => AssetDirectory.Debug;

		public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color ItemColor, Vector2 origin, float scale)
		{
			Texture2D back = Request<Texture2D>("StarlightRiver/Assets/Abilities/DefaultFrame", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
			Draw(spriteBatch, position, 1, scale);
			return false;
		}

		/// <summary>
		/// Renders this infusion as a whole, combining it's frame and glyph
		/// </summary>
		/// <param name="spriteBatch">the spriteBatch to draw this infusion with</param>
		/// <param name="position">the center of where the infusion should be drawn</param>
		/// <param name="opacity">the opacity at which the sprites should be drawn, from 0 to 1</param>
		/// ///<param name="scale">the scale at which this infusion should be drawn</param>
		public void Draw(SpriteBatch spriteBatch, Vector2 position, float opacity, float scale = 1)
		{
			Texture2D outlineTex = Request<Texture2D>(FrameTexture).Value;
			spriteBatch.Draw(outlineTex, position, null, Color.White * opacity, 0, outlineTex.Size() / 2, scale, 0, 0);
			Texture2D mainTex = Request<Texture2D>(Texture).Value;
			spriteBatch.Draw(mainTex, position, null, Color.White * opacity, 0, mainTex.Size() / 2, scale, 0, 0);
		}

		public override void Update(ref float gravity, ref float maxFallSpeed)
		{
			Color color;

			if (AbilityType == null)
			{
				color = Color.Gray;
			}
			else
			{
				if (!Main.LocalPlayer.GetHandler().GetAbility(AbilityType, out Ability ability))
					return;
				color = ability.Color;
			}

			float rot = Main.rand.NextFloat((float)Math.PI * 2);
			var d = Dust.NewDustPerfect(Item.Center + Vector2.One.RotatedBy(rot) * 16, 264, Vector2.One.RotatedBy(rot) * -1.25f, 0, color, 0.8f);
			d.noGravity = true;
			d.noLight = true;
		}

		public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
		{
			Draw(spriteBatch, Item.Center - Main.screenPosition, 1);
			return false;
		}

		public override bool CanRightClick()
		{
			return Main.LocalPlayer.GetHandler().CanSetInfusion(this);
		}

		public override void RightClick(Player Player)
		{
			AbilityHandler mp = Player.GetHandler();

			for (int i = 0; i < mp.InfusionLimit; i++)
			{
				if (mp.GetInfusion(i) == null || i == mp.InfusionLimit - 1)
				{
					mp.SetInfusion(Item.Clone().ModItem as InfusionItem, i);
					Item.TurnToAir();
					return;
				}
			}
		}
	}
}