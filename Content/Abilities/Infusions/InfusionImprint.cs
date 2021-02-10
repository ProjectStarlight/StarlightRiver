using System;
using Terraria.ID;

using StarlightRiver.Core;
using StarlightRiver.Content.Abilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using static Terraria.ModLoader.ModContent;
using Terraria;
using ReLogic.Graphics;
using System.Collections.Generic;

namespace StarlightRiver.Abilities.AbilityContent.Infusions
{
    public abstract class InfusionImprint : InfusionItem
    {
        public override Type AbilityType => null;
        public List<InfusionObjective> objectives = new List<InfusionObjective>();

        public virtual string PreviewVideo => AssetDirectory.Debug;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("NO_NAME");
            Tooltip.SetDefault("NO_TOOLTIP");
        }

        public override void SetDefaults()
        {
            item.width = 20;
            item.height = 14;
            item.rare = ItemRarityID.Blue;
        }

        public override void UpdateInventory(Player player)
        {
            bool transform = true;

            foreach (var objective in objectives)
            {
                objective.check(objective);
                if (objective.progress < objective.maxProgress) 
                    transform = false;
            }

            if (transform)
            {
                item.TurnToAir();
                Main.NewText("Debug Objectives Complete!");
            }           
        }
    }

    public class InfusionObjective
    {
        public float progress;
        private string text;
        public float maxProgress;
        public Action<InfusionObjective> check;

        public InfusionObjective(string text, float maxProgress, Action<InfusionObjective> check, Color color = default)
        {
            this.text = text;
            this.maxProgress = maxProgress;
            this.check = check;
        }

        public void DrawBar(SpriteBatch sb, Vector2 pos)
        {
            var tex = GetTexture(AssetDirectory.GUI + "ChungusMeter");
            sb.Draw(tex, pos, Color.White);
        }

        public float DrawTextAndBar(SpriteBatch sb, Vector2 pos) //For the UI only
        {
            var wrapped = Helpers.Helper.WrapString(text + ": " + progress + "/" + maxProgress, 140, Main.fontItemStack, 0.8f);
            sb.DrawString(Main.fontItemStack, wrapped, pos, Color.White, 0, Vector2.Zero, 0.8f, 0, 0);
            pos.Y += Main.fontItemStack.MeasureString(wrapped).Y;

            var tex = GetTexture(AssetDirectory.GUI + "ChungusMeter");
            sb.Draw(tex, pos, Color.White);

            return pos.Y;
        }
    }

}
