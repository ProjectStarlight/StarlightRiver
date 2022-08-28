using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

		private int tooltipSparkleCounter = 0;

        public override bool InstancePerEntity => true;

		private static ParticleSystem.Update UpdateRelic => UpdateRelicBody;

		private static ParticleSystem.Update UpdateRelicTooltip => UpdateRelicTooltipBody;

		public ParticleSystem RelicParticleSystem = default;
		public ParticleSystem RelicParticleSystemBehind = default;

		public ParticleSystem RelicTooltipParticleSystem = default;

		public Color RelicColor(int offset) => Color.Lerp(Color.Yellow, Color.LimeGreen, 0.5f + (float)(Math.Sin(Main.GameUpdateCount / 20f + offset)) / 2f);
		public Color RelicColorBad(int offset) => Color.Lerp(Color.Yellow, Color.OrangeRed, 0.5f + (float)(Math.Sin(Main.GameUpdateCount / 20f + offset)) / 2f);


        public override void UpdateInventory(Item item, Player player)
        {
			if (isRelic)
			{
				if (RelicParticleSystem == default)
				{
					RelicParticleSystem = new ParticleSystem(AssetDirectory.Keys + "GlowHarshAlpha", UpdateRelic);
					RelicParticleSystemBehind = new ParticleSystem(AssetDirectory.Keys + "GlowHarshAlpha", UpdateRelic);
					RelicTooltipParticleSystem = new ParticleSystem(AssetDirectory.Dust + "GoldSparkle", UpdateRelicTooltip);
				}
			}
			else
			{
				RelicParticleSystem = default;
				RelicParticleSystemBehind = default;
				RelicTooltipParticleSystem = default;
			}
		}

		public override bool PreDrawInInventory(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color ItemColor, Vector2 origin, float scale)
		{
			if (!isRelic)
				return base.PreDrawInInventory(item, spriteBatch, position, frame, drawColor, ItemColor, origin, scale);

			float particleScale = Main.rand.NextFloat(0.3f, 0.45f) * scale;

			float sin = Main.rand.NextFloat(6.28f);

			float backScale = 0.75f;

			Vector2 pos = new Vector2(position.X + (frame.Width * (0.5f * (1 + (float)Math.Sin(sin)))), position.Y + (frame.Height * Main.rand.NextFloat(0.8f, 1f)));
			Vector2 frontPos = pos - new Vector2(80 * particleScale, 80 * particleScale);
			Vector2 backPos = pos - new Vector2(80 * particleScale * backScale, 80 * particleScale * backScale);

			float colorLerper = Main.rand.NextFloat(0.2f);
			Color color = Color.Lerp(Color.Gold, Color.White, colorLerper);
			color.A = 0;

			Color colorBehind = Color.Lerp(Color.Orange, Color.White, colorLerper);
			colorBehind.A = 0;

			int fadeTime = Main.rand.Next(100, 130);
			if (Main.rand.NextBool(18))
			{
				RelicParticleSystem?.AddParticle(new Particle(frontPos, new Vector2(0, 0), 0, particleScale, color, 200, new Vector2(fadeTime, sin), default, 0));
				RelicParticleSystemBehind?.AddParticle(new Particle(backPos, new Vector2(0, 0), 0, particleScale * backScale, colorBehind, 200, new Vector2(fadeTime, sin), default, 0));
			}

			RelicParticleSystemBehind?.DrawParticles(spriteBatch);

			return base.PreDrawInInventory(item, spriteBatch, position, frame, drawColor, ItemColor, origin, scale);
		}


        public override bool PreDrawTooltipLine(Item item, DrawableTooltipLine line, ref int yOffset)
        {
			if (!isRelic || line.Name != "ItemName")
				return base.PreDrawTooltipLine(item, line, ref yOffset);

			float scale = Main.UIScale;
			Vector2 position = new Vector2(line.OriginalX + 7, line.OriginalY + 7) + ((line.Font.MeasureString(line.Text) - new Vector2(14,14)) * scale * new Vector2(Main.rand.NextFloat(), Main.rand.NextFloat()));

			if (tooltipSparkleCounter++ % 14 == 0)
				RelicTooltipParticleSystem?.AddParticle(new Particle(position, Vector2.Zero, 0, Main.UIScale * Main.rand.NextFloat(0.85f,1.15f), Color.White, 20, Vector2.Zero, new Rectangle(0, 0, 14, 14)));

			RelicTooltipParticleSystem?.DrawParticles(Main.spriteBatch);

			return base.PreDrawTooltipLine(item, line, ref yOffset);
		}

        public override void PostDrawInInventory(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color ItemColor, Vector2 origin, float scale)
		{
			if (!isRelic)
				return;

			RelicParticleSystem?.DrawParticles(spriteBatch);
		}

		public override GlobalItem Clone(Item item, Item itemClone)
		{
			return item.TryGetGlobalItem<RelicItem>(out var gi) ? gi : base.Clone(item, itemClone);
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

        public override void UpdateAccessory(Item item, Player Player, bool hideVisual) //re-add vanilla prefixes to double power. This is bad, but its not IL atleast :)
        {
			if (!item.GetGlobalItem<RelicItem>().isRelic)
			{ 
				base.UpdateAccessory(item, Player, hideVisual);
				return;
				}

			if (item.prefix == 62)
			{
				Player.statDefense++;
			}
			if (item.prefix == 63)
			{
				Player.statDefense += 2;
			}
			if (item.prefix == 64)
			{
				Player.statDefense += 3;
			}
			if (item.prefix == 65)
			{
				Player.statDefense += 4;
			}
			if (item.prefix == 66)
			{
				Player.statManaMax2 += 20;
			}
			if (item.prefix == 67)
			{
				Player.GetCritChance(DamageClass.Melee) += 2;
				Player.GetCritChance(DamageClass.Ranged) += 2;
				Player.GetCritChance(DamageClass.Magic) += 2;
				Player.GetCritChance(DamageClass.Throwing) += 2;
			}
			if (item.prefix == 68)
			{
				Player.GetCritChance(DamageClass.Melee) += 4;
				Player.GetCritChance(DamageClass.Ranged) += 4;
				Player.GetCritChance(DamageClass.Magic) += 4;
				Player.GetCritChance(DamageClass.Throwing) += 4;
			}
			if (item.prefix == 69)
			{
				Player.GetDamage(DamageClass.Generic) += 0.01f; 
			}
			if (item.prefix == 70)
			{
				Player.GetDamage(DamageClass.Generic) += 0.02f;
			}
			if (item.prefix == 71)
			{
				Player.GetDamage(DamageClass.Generic) += 0.03f;
			}
			if (item.prefix == 72)
			{
				Player.GetDamage(DamageClass.Generic) += 0.04f;
			}
			if (item.prefix == 73)
			{
				Player.moveSpeed += 0.01f;
			}
			if (item.prefix == 74)
			{
				Player.moveSpeed += 0.02f;
			}
			if (item.prefix == 75)
			{
				Player.moveSpeed += 0.03f;
			}
			if (item.prefix == 76)
			{
				Player.moveSpeed += 0.04f;
			}
			if (item.prefix == 77)
			{
				Player.GetAttackSpeed(DamageClass.Melee) += 0.01f;
			}
			if (item.prefix == 78)
			{
				Player.GetAttackSpeed(DamageClass.Melee) += 0.02f;
			}
			if (item.prefix == 79)
			{
				Player.GetAttackSpeed(DamageClass.Melee) += 0.03f;
			}
			if (item.prefix == 80)
			{
				Player.GetAttackSpeed(DamageClass.Melee) += 0.04f;
			}
		}

        public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
        {
            if (item.GetGlobalItem<RelicItem>().isRelic)
            {
                for(int k = 0; k < tooltips.Count; k++)
                {
                    var line = tooltips[k];

                    if (line.Name == "ItemName")
                    {
                        line.Text = line.Text.Insert(0, "Twice ");
                        line.OverrideColor = RelicColor(k);
                    }

					if (line.IsModifier)
					{
						line.OverrideColor = RelicColor(k);

						if (item.accessory)
							line.Text = DoubleIntValues(line.Text);
					}

                    if (line.IsModifierBad)
                        line.OverrideColor = RelicColorBad(k);
                }

                var newLine = new TooltipLine(Mod, "relicLine", "Cannot be reforged");
                newLine.OverrideColor = new Color(255, 180, 100);
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

        public override void SaveData(Item item, TagCompound tag)
        {
			if(item.GetGlobalItem<RelicItem>().isRelic)
				tag["isRelic"] = true;
		}

        public override void LoadData(Item item, TagCompound tag)
        {
			if (tag.ContainsKey("isRelic"))
				item.GetGlobalItem<RelicItem>().isRelic = tag.GetBool("isRelic");
		}

		private static void UpdateRelicBody(Particle particle)
		{
			float sin = particle.StoredPosition.Y; //abusing storedposition cause theres no other way to pass in special data
			float fadeTime = particle.StoredPosition.X;

			particle.StoredPosition.Y += 0.05f;

			particle.Velocity.Y = -0.2f;
			particle.Velocity.X = 0.7f * (float)Math.Cos(sin);

			if (particle.Timer > 180)
				particle.Alpha += 0.05f;
			else if (particle.Timer < fadeTime)
				particle.Alpha -= 0.025f;

			particle.Alpha = MathHelper.Clamp(particle.Alpha, 0, 1);
			particle.Position += particle.Velocity;
			particle.Timer--;
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
