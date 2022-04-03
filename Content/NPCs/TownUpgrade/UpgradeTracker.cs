using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using Terraria;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.NPCs.TownUpgrade
{
	class UpgradeTracker : GlobalNPC
    {
        public override bool PreDraw(NPC NPC, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (NPC.townNPC && StarlightWorld.TownUpgrades.TryGetValue(NPC.TypeName, out bool upgraded) && upgraded)
            {
                Texture2D tex = Request<Texture2D>("StarlightRiver/Assets/NPCs/TownUpgrade/" + NPC.TypeName + "Upgraded").Value;
                Vector2 pos = NPC.Center - screenPos + new Vector2(0, NPC.gfxOffY - 4);

                spriteBatch.Draw(tex, pos, NPC.frame, drawColor, NPC.rotation, NPC.frame.Size() / 2, NPC.scale, NPC.spriteDirection == -1 ? 0 : SpriteEffects.FlipHorizontally, 0);
                return false;
            }

            return base.PreDraw(NPC, spriteBatch, screenPos, drawColor);
        }
    }
}
