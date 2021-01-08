using System;
using Terraria.ID;

using StarlightRiver.Core;
using StarlightRiver.Content.Abilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Abilities.AbilityContent.Infusions
{
    public abstract class InfusionImprint : InfusionItem
    {
        public override Type AbilityType => null;

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
    }

    public class InfusionObjective
    {
        public float progress;
        private float maxProgress;
        private Action check;

        public InfusionObjective(float maxProgress, Action check, Color color = default)
        {
            this.maxProgress = maxProgress;
            this.check = check;
        }

        public void Draw(SpriteBatch sb, Vector2 pos)
        {
            var tex = GetTexture(AssetDirectory.GUI + "ChungusMeter");
        }
    }

}
