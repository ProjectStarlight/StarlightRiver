using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.Utilities;

namespace StarlightRiver.Content.Items.BaseTypes
{
    class RelicItem : GlobalItem
    {
        public bool isRelic = false;

        public bool doubled = false;

        public override bool InstancePerEntity => true;

        public override bool CloneNewInstances => true;

        public override bool NeedsSaving(Item item) => isRelic;

        public Color RelicColor(int offset) => Color.Lerp(Color.Yellow, Color.LimeGreen, 0.5f + (float)(Math.Sin(Main.GameUpdateCount / 20f + offset)) / 2f);
        public Color RelicColorBad(int offset) => Color.Lerp(Color.Yellow, Color.OrangeRed, 0.5f + (float)(Math.Sin(Main.GameUpdateCount / 20f + offset)) / 2f);

        public override bool? PrefixChance(Item item, int pre, UnifiedRandom rand)
        {
            if (isRelic)
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
            if (isRelic)
            {
                int result = base.ChoosePrefix(item, rand);
                return result != 0 ? result : ChoosePrefix(item, rand);
            }

            return base.ChoosePrefix(item, rand);
        }

        public override void UpdateAccessory(Item item, Player player, bool hideVisual) //re-add vanilla prefixes to double power. This is bad, but its not IL atleast :)
        {
			if (!isRelic)
			{ 
				base.UpdateAccessory(item, player, hideVisual);
				return;
				}

			if (item.prefix == 62)
			{
				player.statDefense++;
			}
			if (item.prefix == 63)
			{
				player.statDefense += 2;
			}
			if (item.prefix == 64)
			{
				player.statDefense += 3;
			}
			if (item.prefix == 65)
			{
				player.statDefense += 4;
			}
			if (item.prefix == 66)
			{
				player.statManaMax2 += 20;
			}
			if (item.prefix == 67)
			{
				player.meleeCrit += 2;
				player.rangedCrit += 2;
				player.magicCrit += 2;
				player.thrownCrit += 2;
			}
			if (item.prefix == 68)
			{
				player.meleeCrit += 4;
				player.rangedCrit += 4;
				player.magicCrit += 4;
				player.thrownCrit += 4;
			}
			if (item.prefix == 69)
			{
				player.allDamage += 0.01f;
			}
			if (item.prefix == 70)
			{
				player.allDamage += 0.02f;
			}
			if (item.prefix == 71)
			{
				player.allDamage += 0.03f;
			}
			if (item.prefix == 72)
			{
				player.allDamage += 0.04f;
			}
			if (item.prefix == 73)
			{
				player.moveSpeed += 0.01f;
			}
			if (item.prefix == 74)
			{
				player.moveSpeed += 0.02f;
			}
			if (item.prefix == 75)
			{
				player.moveSpeed += 0.03f;
			}
			if (item.prefix == 76)
			{
				player.moveSpeed += 0.04f;
			}
			if (item.prefix == 77)
			{
				player.meleeSpeed += 0.01f;
			}
			if (item.prefix == 78)
			{
				player.meleeSpeed += 0.02f;
			}
			if (item.prefix == 79)
			{
				player.meleeSpeed += 0.03f;
			}
			if (item.prefix == 80)
			{
				player.meleeSpeed += 0.04f;
			}
		}

        public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
        {
            if (isRelic)
            {
                for(int k = 0; k < tooltips.Count; k++)
                {
                    var line = tooltips[k];

                    if (line.Name == "ItemName")
                    {
                        line.text = line.text.Insert(0, "Twice ");
                        line.overrideColor = RelicColor(k);
                    }

					if (line.isModifier)
					{
						line.overrideColor = RelicColor(k);

						if (item.accessory)
							line.text = DoubleIntValues(line.text);
					}

                    if (line.isModifierBad)
                        line.overrideColor = RelicColorBad(k);
                }

                var newLine = new TooltipLine(mod, "relicLine", "Cannot be reforged");
                newLine.overrideColor = new Color(255, 180, 100);
                tooltips.Add(newLine);
            }
        }

		public string DoubleIntValues(string input)
        {
			for (int k = 0; k < input.Length; k++)
			{
				int result = 0;
				if (int.TryParse(input[k].ToString(), out result))
				{
					input = input.Remove(k, 1);
					input = input.Insert(k, (result * 2).ToString());
				}
			}

			return input;
        }

        public override TagCompound Save(Item item)
        {
            return new TagCompound()
            {
                ["isRelic"] = isRelic
            };
        }

        public override void Load(Item item, TagCompound tag)
        {
            isRelic = tag.GetBool("isRelic");
        }
    }
}
