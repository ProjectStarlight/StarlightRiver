using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using StarlightRiver.Content.Abilities;
using StarlightRiver.Content.GUI;
using StarlightRiver.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Abilities.AbilityContent.Infusions
{
	public abstract class InfusionImprint : InfusionItem
    {
        public override Type AbilityType => null;

        public List<InfusionObjective> objectives = new List<InfusionObjective>();

        public virtual string PreviewVideo => AssetDirectory.Debug;
        public virtual int TransformTo => ItemID.DirtBlock;

		public sealed override void SetStaticDefaults()
        {
            SafeSetStaticDefaults();
            InfusionMaker.infusionOptions.Add(item.type);
        }

        public virtual void SafeSetStaticDefaults() { }

        public override void SetDefaults()
        {
            item.width = 20;
            item.height = 14;
            item.rare = ItemRarityID.Blue;
        }

        public InfusionObjective FindObjective(Player player, string objectiveText)
        {
            for (int k = 0; k < player.inventory.Length; k++)
            {
                var item = player.inventory[k];

                if (item.modItem is InfusionImprint)
                {
                    var objective = (item.modItem as InfusionImprint).objectives.FirstOrDefault(n => n.text == objectiveText);

                    if (objective != null)
                        return objective;
                }
            }

            return null;
        }

        public override void UpdateInventory(Player player)
        {
            bool transform = true;

            if (objectives.Count <= 0)
                return;

            foreach (var objective in objectives)
            {
                if (objective.progress < objective.maxProgress) 
                    transform = false;

                if (objective.progress > objective.maxProgress)
                    objective.progress = objective.maxProgress;
            }

            if (transform)
            {
                item.SetDefaults(TransformTo);
                item.newAndShiny = true;
                Main.NewText("Objectives Complete! You've obtained: " + item.Name);
            }           
        }

        public override bool PreDrawTooltip(ReadOnlyCollection<TooltipLine> lines, ref int x, ref int y)
        {
            var pos = new Vector2(x, y);

            Utils.DrawBorderString(Main.spriteBatch, "Imprinted slate: " + item.Name, pos, new Color(170, 120, 255).MultiplyRGB(Main.mouseTextColorReal));
            pos.Y += 28;

            Utils.DrawBorderString(Main.spriteBatch, "Complete objectives to transform into an infusion", pos, Main.mouseTextColorReal);
            pos.Y += 28;

            foreach (var objective in objectives)
            {
                objective.DrawTextAndBar(Main.spriteBatch, pos);
                pos.Y += 28;
            }

            return false;
        }

		public override ModItem Clone(Item item)
		{
            var newClone = base.Clone(item);

            if (newClone is InfusionImprint)
                (newClone as InfusionImprint).objectives = objectives;

            return newClone;
        }
	}

    public class InfusionObjective
    {
        public float progress;
        public string text;
        public float maxProgress;

        public InfusionObjective(string text, float maxProgress, Color color = default)
        {
            this.text = text;
            this.maxProgress = maxProgress;
        }

        public void DrawBar(SpriteBatch sb, Vector2 pos)
        {
            var tex = GetTexture(AssetDirectory.GUI + "ChungusMeter");
            sb.Draw(tex, pos, Color.White);
        }

        public float DrawText(SpriteBatch sb, Vector2 pos)
		{
            var wrapped = Helpers.Helper.WrapString(text + ": " + progress + "/" + maxProgress, 130, Main.fontItemStack, 0.8f);
            sb.DrawString(Main.fontItemStack, wrapped, pos, Color.White, 0, Vector2.Zero, 0.8f, 0, 0);

            return Main.fontItemStack.MeasureString(wrapped).Y * 0.8f;
        }

        public float DrawTextAndBar(SpriteBatch sb, Vector2 pos) //For the UI only
        {
            var wrapped = (">  " + text + ": " + progress + "/" + maxProgress);
            Utils.DrawBorderString(sb, wrapped, pos, progress >= maxProgress ? new Color(140, 140, 140).MultiplyRGB(Main.mouseTextColorReal) : Main.mouseTextColorReal);
            pos.X += Main.fontMouseText.MeasureString(wrapped).X + 8;
            pos.Y += 2;

            var tex = GetTexture(AssetDirectory.GUI + "ChungusMeter");
            var texFill = GetTexture(AssetDirectory.GUI + "ChungusMeterFill");
            var fillRect = new Rectangle((int)pos.X + 14, (int)pos.Y + 4, (int)(progress / maxProgress * texFill.Width), texFill.Height);
            var fillSource = new Rectangle(0, 0, (int)(progress / maxProgress * texFill.Width), texFill.Height);

            var color = progress >= maxProgress ? Color.Lerp(new Color(80, 180, 255), new Color(120, 255, 255), 0.5f + (float)Math.Sin(Main.GameUpdateCount * 0.2f) * 0.5f) : new Color(80, 180, 255);
            sb.Draw(texFill, fillRect, fillSource, color);
            sb.Draw(tex, pos, Color.White);

            return pos.Y;
        }
    }

}
