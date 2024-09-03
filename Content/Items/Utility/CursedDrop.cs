using StarlightRiver.Content.Items.BaseTypes;
using StarlightRiver.Content.Prefixes.Accessory.Cursed;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.Audio;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.UI;

namespace StarlightRiver.Content.Items.Utility
{
	class CursedDrop: ModItem
	{
		public static float tooltipProgress;

		public override string Texture => AssetDirectory.Assets + "Items/Utility/" + Name;

		public override void Load()
		{
			On_ItemSlot.RightClick_refItem_int += ApplyDrop;
			On_ItemSlot.RightClick_ItemArray_int_int += ApplyDrop2;
		}

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Drop of darkness");
			Tooltip.SetDefault("Reforges an accessory with a cursed prefix\nRight Click an accessory while holding this on your cursor to use\nReforge the item at the goblin tinkerer to remove it");
		}

		public override void SetDefaults()
		{
			Item.width = 32;
			Item.height = 32;
			Item.maxStack = 9999;
			Item.rare = ItemRarityID.LightRed;
		}

		public override void ModifyTooltips(List<TooltipLine> tooltips)
		{
			tooltips.Remove(tooltips.FirstOrDefault(n => n.Mod == "Terraria" && n.Name == "Equipable"));
		}

		private void ApplyDrop(On_ItemSlot.orig_RightClick_refItem_int orig, ref Item inv, int context)
		{
			if (Main.mouseRight && Main.mouseItem.type == ModContent.ItemType<CursedDrop>() && inv.accessory && !(inv.ModItem is CursedAccessory))
			{
				var pool = CursedPrefixPool.GetCursedPrefixes();
				inv.prefix = pool[Main.rand.Next(pool.Count)];

				SoundEngine.PlaySound(SoundID.NPCHit55.WithPitchOffset(0.2f));
				SoundEngine.PlaySound(SoundID.Item123.WithPitchOffset(0.2f));

				for (int k = 0; k <= 50; k++)
					CursedAccessory.CursedSystem.AddParticle(new Particle(Main.MouseScreen, Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(0.75f), 0, 1, new(25, 17, 49), 60, Vector2.Zero));

				Main.mouseItem.stack--;

				if (Main.mouseItem.stack <= 0)
					Main.mouseItem.TurnToAir();
			}
			else
			{
				orig(ref inv, context);
			}	
		}

		private void ApplyDrop2(On_ItemSlot.orig_RightClick_ItemArray_int_int orig, Item[] inv, int context, int slot)
		{
			if (Main.mouseRight && Main.mouseItem.type == ModContent.ItemType<CursedDrop>() && inv[slot].accessory && !(inv[slot].ModItem is CursedAccessory))
			{
				var pool = CursedPrefixPool.GetCursedPrefixes();
				inv[slot].prefix = pool[Main.rand.Next(pool.Count)];

				SoundEngine.PlaySound(SoundID.NPCHit55.WithPitchOffset(0.2f));
				SoundEngine.PlaySound(SoundID.Item123.WithPitchOffset(0.2f));

				for (int k = 0; k <= 50; k++)
					CursedAccessory.CursedSystem.AddParticle(new Particle(Main.MouseScreen, Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(0.75f), 0, 1, new(25, 17, 49), 60, Vector2.Zero));

				Main.mouseItem.stack--;

				if (Main.mouseItem.stack <= 0)
					Main.mouseItem.TurnToAir();
			}
			else
			{
				orig(inv, context, slot);
			}
		}

		// Copied from cursed acc, TODO: should really set up a rarity for this...
		public override void UpdateInventory(Player Player)
		{
			if (!(Main.HoverItem.ModItem is CursedDrop))
				tooltipProgress = 0;
		}

		public override bool PreDrawTooltipLine(DrawableTooltipLine line, ref int yOffset)
		{
			if (line.Mod == "Terraria" && line.Name == "ItemName")
			{
				Effect effect = Filters.Scene["CursedTooltip"].GetShader().Shader;
				Texture2D tex = Assets.Keys.Glow.Value;

				if (effect is null)
					return true;

				effect.Parameters["speed"].SetValue(1);
				effect.Parameters["power"].SetValue(0.011f * tooltipProgress);
				effect.Parameters["uTime"].SetValue(Main.GameUpdateCount / 10f);

				int measure = (int)(line.Font.MeasureString(line.Text).X * 1.1f);
				int offset = (int)(Math.Sin(Main.GameUpdateCount / 25f) * 5);
				var target = new Rectangle(line.X + measure / 2, line.Y + 10, (int)(measure * 1.5f) + offset, 34 + offset);
				Main.spriteBatch.Draw(tex, target, new Rectangle(4, 4, tex.Width - 4, tex.Height - 4), Color.Black * (0.675f * tooltipProgress + (float)Math.Sin(Main.GameUpdateCount / 25f) * -0.1f), 0, (tex.Size() - Vector2.One * 8) / 2, 0, 0);

				Main.spriteBatch.End();
				Main.spriteBatch.Begin(default, default, SamplerState.PointClamp, default, default, effect, Main.UIScaleMatrix);

				Utils.DrawBorderString(Main.spriteBatch, line.Text, new Vector2(line.X, line.Y), Color.Lerp(Color.White, new Color(180, 100, 225), tooltipProgress), 1.1f);

				Main.spriteBatch.End();
				Main.spriteBatch.Begin(default, default, SamplerState.PointClamp, default, default, default, Main.UIScaleMatrix);

				if (tooltipProgress < 1)
					tooltipProgress += 0.05f;

				return false;
			}

			return base.PreDrawTooltipLine(line, ref yOffset);
		}
	}
}