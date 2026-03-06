using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.DataStructures;
using Terraria.ModLoader.IO;
using Terraria.UI.Chat;
using Terraria.Utilities;

namespace StarlightRiver.Content.Items.BaseTypes
{
	class RelicItem : GlobalItem
	{
		public bool isRelic = false;

		public bool doubled = false;

		public override bool InstancePerEntity => true;

		public Color RelicColor(int offset)
		{
			return Color.Lerp(Color.Gold, Color.Orange, 0.5f + (float)Math.Sin(Main.GameUpdateCount / 20f + offset) / 2f);
		}

		public override bool PreDrawInInventory(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color ItemColor, Vector2 origin, float scale)
		{
			if (!isRelic)
				return base.PreDrawInInventory(item, spriteBatch, position, frame, drawColor, ItemColor, origin, scale);

			var pos = new Vector2(position.X, position.Y) + Main.rand.NextVector2Circular(16, 16);

			if (Main.rand.NextBool(18))
			{
				RelicAccessorySystem.RelicParticleSystem?.AddParticle(pos, Vector2.Zero, 0, Main.UIScale * Main.rand.NextFloat(0.85f, 1.15f), Color.White, 20, Vector2.Zero, new Rectangle(0, 0, 14, 14));
			}

			return base.PreDrawInInventory(item, spriteBatch, position, frame, drawColor, ItemColor, origin, scale);
		}

		public override bool PreDrawTooltipLine(Item item, DrawableTooltipLine line, ref int yOffset)
		{
			if (!isRelic)
				return base.PreDrawTooltipLine(item, line, ref yOffset);

			if (line.Visible && (line.Name == "ItemName" || line.IsModifier && !line.IsModifierBad))
			{
				

				

				for(int k = 0; k < 4; k++)
				{
					Vector2 off = Vector2.UnitX.RotatedBy(k / 4f * 6.28f + Main.GameUpdateCount * 0.01f) * 4;

					var snippets = ChatManager.ParseMessage(line.Text, RelicColor(20 * k));
					foreach (var item1 in snippets)
					{
						item1.Color *= 0.5f;
					}

					ChatManager.DrawColorCodedString(Main.spriteBatch, line.Font, snippets.ToArray(), new Vector2(line.X, line.Y) + off, RelicColor(20 * k), 0, Vector2.Zero, Vector2.One, out int hovered, -1);
				}
				Utils.DrawBorderString(Main.spriteBatch, line.Text, new Vector2(line.X, line.Y), Color.Lerp(RelicColor(0), Color.White, 0.5f));

				if (Main.rand.NextBool(20))
				{
					float scale = Main.UIScale;
					Vector2 position = new Vector2(line.OriginalX + 7, line.OriginalY + 7) + (ChatManager.GetStringSize(line.Font, line.Text, Vector2.One) - new Vector2(14, 14)) * scale * new Vector2(Main.rand.NextFloat(), Main.rand.NextFloat());

					RelicAccessorySystem.RelicTooltipParticleSystem?.AddParticle(position, Vector2.Zero, 0, Main.UIScale * Main.rand.NextFloat(0.85f, 1.15f), Color.White, 20, Vector2.Zero, new Rectangle(0, 0, 14, 14));
				}

				return false;
			}

			return base.PreDrawTooltipLine(item, line, ref yOffset);
		}

		public override GlobalItem Clone(Item item, Item itemClone)
		{
			return item.TryGetGlobalItem<RelicItem>(out RelicItem gi) ? gi : base.Clone(item, itemClone);
		}

		public override bool? PrefixChance(Item item, int pre, UnifiedRandom rand)
		{
			if (item.GetGlobalItem<RelicItem>().isRelic)
			{
				if (pre == -3)
					return false;

				if (pre == -1)
					return true;
			}

			return base.PrefixChance(item, pre, rand);
		}

		public override int ChoosePrefix(Item item, UnifiedRandom rand)
		{
			if (item.GetGlobalItem<RelicItem>().isRelic)
			{
				int result = base.ChoosePrefix(item, rand);
				return result != 0 ? result : ChoosePrefix(item, rand);
			}

			return base.ChoosePrefix(item, rand);
		}

		public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
		{
			if (item.GetGlobalItem<RelicItem>().isRelic)
			{
				if (item.material)
				{
					var newLine2 = new TooltipLine(Mod, "relicLine2", "Items crafted with this one will inherit it's prefix")
					{
						OverrideColor = new Color(160, 160, 160)
					};
					tooltips.Add(newLine2);
				}

				var newLine = new TooltipLine(Mod, "relicLine", "Cannot be reforged")
				{
					OverrideColor = new Color(160, 160, 160)
				};
				tooltips.Add(newLine);
			}
		}

		public override void SaveData(Item item, TagCompound tag)
		{
			if (item.GetGlobalItem<RelicItem>().isRelic)
				tag["isRelic"] = true;
		}

		public override void LoadData(Item item, TagCompound tag)
		{
			if (tag.ContainsKey("isRelic"))
				item.GetGlobalItem<RelicItem>().isRelic = tag.GetBool("isRelic");
		}

		public override void OnCreated(Item item, ItemCreationContext context)
		{
			if (context is RecipeItemCreationContext recipe)
			{
				// Inherit the first relic prefix used
				if (recipe.ConsumedItems.Any(n => n.GetGlobalItem<RelicItem>().isRelic))
				{
					Item parentRelic = recipe.ConsumedItems.First(n => n.GetGlobalItem<RelicItem>().isRelic);
					item.GetGlobalItem<RelicItem>().isRelic = true;
					item.prefix = parentRelic.prefix;
				}
			}
		}
	}

	class RelicAccessorySystem : ModSystem
	{
		public static ParticleSystem RelicParticleSystem = default;
		public static ParticleSystem RelicTooltipParticleSystem = default;

		public override void Load()
		{
			RelicParticleSystem = new ParticleSystem(AssetDirectory.Dust + "GoldSparkle", UpdateRelicBody, ParticleSystem.AnchorOptions.UI);
			RelicTooltipParticleSystem = new ParticleSystem(AssetDirectory.Dust + "GoldSparkle", UpdateRelicTooltipBody, ParticleSystem.AnchorOptions.UI);

			if (!Main.dedServ)
			{
				On_Main.DrawInterface_27_Inventory += DrawInventoryParticles;
				On_Main.DrawInterface_36_Cursor += DrawTooltipParticles;
			}
		}

		public override void PostUpdateEverything()
		{
			RelicParticleSystem?.UpdateParticles();
			RelicTooltipParticleSystem?.UpdateParticles();
		}

		private void DrawInventoryParticles(On_Main.orig_DrawInterface_27_Inventory orig, Main self)
		{
			orig(self);
			RelicParticleSystem.DrawParticles(Main.spriteBatch);
		}

		private void DrawTooltipParticles(On_Main.orig_DrawInterface_36_Cursor orig)
		{
			orig();
			RelicTooltipParticleSystem.DrawParticles(Main.spriteBatch);
		}

		private static void UpdateRelicBody(Particle particle)
		{
			particle.Position += particle.Velocity;
			particle.Timer--;
			if (particle.Timer % 5 == 0 && particle.Timer != 0)
				particle.Frame.Y += 14;
		}

		private static void UpdateRelicTooltipBody(Particle particle)
		{
			particle.Position += particle.Velocity;
			particle.Timer--;
			if (particle.Timer % 5 == 0 && particle.Timer != 0)
				particle.Frame.Y += 14;
		}
	}
}