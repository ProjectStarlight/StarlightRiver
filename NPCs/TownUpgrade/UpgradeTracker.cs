using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Terraria.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using static Terraria.ModLoader.ModContent;
using System.Reflection;

namespace StarlightRiver.NPCs.TownUpgrade
{
    class UpgradeTracker : GlobalNPC
    {
        public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Color drawColor)
        {
            if(npc.townNPC && StarlightWorld.TownUpgrades.TryGetValue(npc.TypeName, out bool upgraded) && upgraded)
            {
                Texture2D tex = GetTexture("StarlightRiver/NPCs/TownUpgrade/" + npc.TypeName + "Upgraded");
                Vector2 pos = npc.Center - Main.screenPosition + new Vector2(0, npc.gfxOffY - 4);

                spriteBatch.Draw(tex, pos, npc.frame, drawColor, npc.rotation, npc.frame.Size() / 2, npc.scale, npc.spriteDirection == -1 ? 0 : SpriteEffects.FlipHorizontally, 0);
                return false;
            }

            return base.PreDraw(npc, spriteBatch, drawColor);
        }
    }
}
