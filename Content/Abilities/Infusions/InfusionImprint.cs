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
using Terraria.ModLoader.IO;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Abilities.AbilityContent.Infusions
{
	public abstract class InfusionImprint : InfusionItem
    {
        public override Type AbilityType => null;

        public override bool Equippable => false;

        public List<InfusionObjective> objectives = new List<InfusionObjective>();

        public virtual string PreviewVideo => AssetDirectory.Debug;
        public virtual int TransformTo => ItemID.DirtBlock;
        public virtual bool Visible => true;

		public sealed override void SetStaticDefaults()
        {
            SafeSetStaticDefaults();
            InfusionMaker.infusionOptions.Add(Item.type);
        }

        public virtual void SafeSetStaticDefaults() { }

        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 14;
            Item.rare = ItemRarityID.Blue;
        }

        public InfusionObjective FindObjective(Player Player, string objectiveText)
        {
            for (int k = 0; k < Player.inventory.Length; k++)
            {
                var Item = Player.inventory[k];

                if (Item.ModItem is InfusionImprint)
                {
                    var objective = (Item.ModItem as InfusionImprint).objectives.FirstOrDefault(n => n.text == objectiveText);

                    if (objective != null)
                        return objective;
                }
            }

            return null;
        }

        public override void UpdateInventory(Player Player)
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
                Item.SetDefaults(TransformTo);
                Item.newAndShiny = true;
                Main.NewText("Objectives Complete! You've obtained: " + Item.Name);
            }           
        }

        public override bool PreDrawTooltip(ReadOnlyCollection<TooltipLine> lines, ref int x, ref int y)
        {
            var pos = new Vector2(x, y);

            Utils.DrawBorderString(Main.spriteBatch, "Imprinted slate: " + Item.Name, pos, new Color(170, 120, 255).MultiplyRGB(Main.mouseTextColorReal));
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

		public override ModItem Clone(Item Item)
		{
            var newClone = base.Clone(Item);

            if (newClone is InfusionImprint)
                (newClone as InfusionImprint).objectives = objectives;

            return newClone;
        }

        public override void SaveData(TagCompound tag)
        {
            List<TagCompound> objectiveTags = new List<TagCompound>();

            foreach (InfusionObjective obj in objectives)
                objectiveTags.Add(obj.Save());

            return new TagCompound()
            {
                ["objectives"] = objectiveTags
            };
        }

        public override void LoadData(TagCompound tag)
        {
            objectives.Clear();

            var tags = tag.GetList<TagCompound>("objectives");

            foreach (TagCompound objectiveTag in tags)
            {
                var objective = new InfusionObjective("Invalid Objective", 1);
                objective.Load(objectiveTag);
                objectives.Add(objective);
            }
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

        public void SaveData(TagCompound tag)
		{
            return new TagCompound()
            {
                ["progress"] = progress,
                ["maxProgress"] = maxProgress,
                ["text"] = text
            };
		}

        public void LoadData(TagCompound tag)
		{
            progress = tag.GetFloat("progress");
            maxProgress = tag.GetFloat("maxProgress");
            text = tag.GetString("text");
        }

        public void DrawBar(SpriteBatch sb, Vector2 pos)
        {
            var tex = Request<Texture2D>(AssetDirectory.GUI + "ChungusMeter").Value;
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
            pos.X += Terraria.GameContent.FontAssets.MouseText.Value.MeasureString(wrapped).X + 8;
            pos.Y += 2;

            var tex = Request<Texture2D>(AssetDirectory.GUI + "ChungusMeter").Value;
            var texFill = Request<Texture2D>(AssetDirectory.GUI + "ChungusMeterFill").Value;
            var fill = (int)(progress / maxProgress * texFill.Width);

            var fillRect = new Rectangle((int)pos.X + 14, (int)pos.Y + 4, fill, texFill.Height);
            var fillSource = new Rectangle(0, 0, (int)(progress / maxProgress * texFill.Width), texFill.Height);

            var color = progress >= maxProgress ? Color.Lerp(new Color(80, 180, 255), new Color(120, 255, 255), 0.5f + (float)Math.Sin(Main.GameUpdateCount * 0.2f) * 0.5f) : new Color(80, 180, 255);
            sb.Draw(texFill, fillRect, fillSource, color);
            sb.Draw(tex, pos, Color.White);

            if(fill > 4)
                sb.Draw(Main.magicPixel, new Rectangle((int)pos.X + 14 + fill, (int)pos.Y + 4, 2, 10), Color.White);

            return pos.Y;
        }
    }

}
