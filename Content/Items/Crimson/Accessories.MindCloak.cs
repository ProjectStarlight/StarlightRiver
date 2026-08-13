using StarlightRiver.Content.Items.BaseTypes;
using StarlightRiver.Content.Items.Vitric;
using StarlightRiver.Core.Loaders;
using StarlightRiver.Core.Systems.BarrierSystem;
using StarlightRiver.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Crimson
{
	internal class MindCloak : SmartAccessory
	{
		public int lastMana;

		public override string Texture => AssetDirectory.CrimsonItem + Name;

		public MindCloak() : base("Cloak of the Mind", "Increases to maximum mana also apply to maximum {{barrier}}") { }

		public override void Load()
		{
			StarlightPlayer.PostUpdateEquipsEvent += RecordMana;
		}

		public override void SafeSetDefaults()
		{
			Item.expert = true;
			Item.rare = ItemRarityID.Expert;
			Item.accessory = true;
			Item.width = 32;
			Item.height = 32;

			Item.value = Item.sellPrice(gold: 2);
		}

		private void RecordMana(StarlightPlayer Player)
		{
			if (Equipped(Player.Player))
				(GetEquippedInstance(Player.Player) as MindCloak).lastMana = Player.Player.statManaMax2 - Player.Player.statManaMax;
		}

		public override void SafeUpdateEquip(Player Player)
		{
			Player.GetModPlayer<BarrierPlayer>().maxBarrier += lastMana;
		}

		public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
		{
			Texture2D tex = Assets.Items.Crimson.MindCloakGlow.Value;

			Effect effect = ShaderLoader.GetShader("MirageItemFilter").Value;

			if (effect != null)
			{
				effect.Parameters["u_color"].SetValue(Vector3.One);
				effect.Parameters["u_fade"].SetValue(Vector3.One);
				effect.Parameters["u_resolution"].SetValue(tex.Size());
				effect.Parameters["u_time"].SetValue(Main.GameUpdateCount * 0.1f);

				spriteBatch.End();
				spriteBatch.Begin(default, BlendState.Additive, SamplerState.LinearClamp, default, default, effect, Main.UIScaleMatrix);

				spriteBatch.Draw(tex, position, frame, drawColor, 0, origin, scale, 0, 0);

				spriteBatch.End();
				spriteBatch.Begin(default, default, SamplerState.LinearClamp, default, default, default, Main.UIScaleMatrix);
			}

			return true;
		}

		public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
		{
			Texture2D tex = Assets.Items.Crimson.MindCloakGlow.Value;

			Effect effect = ShaderLoader.GetShader("MirageItemFilter").Value;

			if (effect != null)
			{
				effect.Parameters["u_color"].SetValue(Vector3.One);
				effect.Parameters["u_fade"].SetValue(Vector3.One);
				effect.Parameters["u_resolution"].SetValue(tex.Size());
				effect.Parameters["u_time"].SetValue(Main.GameUpdateCount * 0.05f);

				spriteBatch.End();
				spriteBatch.Begin(default, BlendState.Additive, SamplerState.LinearClamp, default, default, effect, Main.UIScaleMatrix);

				spriteBatch.Draw(tex, Item.Center - Main.screenPosition, null, Color.White, rotation, Item.Size / 2f, scale, 0, 0);

				spriteBatch.End();
				spriteBatch.Begin(default, default, SamplerState.LinearClamp, default, default, default, Main.UIScaleMatrix);
			}

			return true;
		}
	}
}