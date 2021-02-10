using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

using StarlightRiver.Core;
using StarlightRiver.Content.Abilities;
using StarlightRiver.Content.Abilities.ForbiddenWinds;

namespace StarlightRiver.Content.NPCs.Vitric
{
    internal class MagmitePassive : ModNPC
    {
        public override string Texture => "StarlightRiver/Assets/NPCs/Vitric/MagmitePassive";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Small Magmite");
        }

        public override void SetDefaults()
        {
            npc.width = 16;
            npc.height = 16;
            npc.damage = 0;
            npc.defense = 0;
            npc.lifeMax = 25;
            npc.aiStyle = -1;
        }

        public override Color? GetAlpha(Color drawColor)
        {
            return Lighting.GetColor((int)npc.position.X / 16, (int)npc.position.Y / 16) * 0.75f;
        }

        public override void AI()
        {

        }
    }

    internal class MagmiteGore : ModGore
    {
        public override Color? GetAlpha(Gore gore, Color lightColor) => Color.White;

        public override bool Update(Gore gore)
        {
            Lighting.AddLight(gore.position, new Vector3(0.1f, 0.1f, 0) * gore.scale);
            gore.scale *= 0.99f;

            if (gore.scale < 0.1f)
                gore.active = false;

            if (gore.velocity.Y == 0)
                gore.frame = 1;

            return false;
        }
    }
}