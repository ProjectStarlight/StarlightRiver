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

        public override bool Autoload(ref string name)
        {
            mod.AddGore("StarlightRiver/Assets/NPCs/Vitric/MagmiteGore", new MagmiteGore());
            return true;
        }

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
            return Color.White;
        }

        public override void AI()
        {
            if (Main.rand.Next(10) == 0)
                Gore.NewGoreDirect(npc.Center, (Vector2.UnitY * -3).RotatedByRandom(0.2f), ModGore.GetGoreSlot("StarlightRiver/Assets/NPCs/Vitric/MagmiteGore"), Main.rand.NextFloat(0.5f, 0.8f));

            npc.velocity.X = 2f * (Main.player[Main.myPlayer].Center.X > npc.Center.X ? 1 : -1);

            npc.spriteDirection = npc.velocity.X > 0 ? 1 : -1;
        }
    }

    internal class MagmiteGore : ModGore
    {
        public override void OnSpawn(Gore gore)
        {
            gore.timeLeft = 180;
            gore.sticky = true;
            gore.numFrames = 2;
            gore.behindTiles = true;
        }

        public override Color? GetAlpha(Gore gore, Color lightColor) => Color.White * (gore.scale < 0.5f ? gore.scale * 2 : 1);

        public override bool Update(Gore gore)
        {
            Lighting.AddLight(gore.position, new Vector3(0.1f, 0.1f, 0) * gore.scale);
            gore.scale *= 0.99f;

            if (gore.scale < 0.1f)
                gore.active = false;

            if (gore.velocity.Y == 0)
            {
                gore.velocity.X = 0;
                gore.rotation = 0;
                gore.frame = 1;
            }

            //gore.position += gore.velocity;

            if (gore.frame == 0) gore.velocity.Y += 0.5f;

            return true;
        }
    }
}