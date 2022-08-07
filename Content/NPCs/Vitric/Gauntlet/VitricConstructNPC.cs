using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Abilities;
using StarlightRiver.Content.Dusts;
using StarlightRiver.Content.Abilities.ForbiddenWinds;
using StarlightRiver.Core;
using StarlightRiver.Helpers;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

using Terraria.Audio;

using System;
using System.Linq;
using static Terraria.ModLoader.ModContent;
using Terraria.GameContent.ItemDropRules;
using StarlightRiver.Content.Items.Vitric;

namespace StarlightRiver.Content.NPCs.Vitric.Gauntlet
{
    public abstract class VitricConstructNPC : ModNPC
    {
        public bool ableToDoCombo = true;

        public virtual string previewTexturePath => Texture + "_Preview";

        public virtual string previewTextureGlowmaskPath => Texture + "_Preview_Glow";

        public virtual void DrawHealingGlow(SpriteBatch spriteBatch) { }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ItemDropRule.ByCondition(new TempleCondition(), ItemType<TempleKey>(), 4));
        }
    }
}