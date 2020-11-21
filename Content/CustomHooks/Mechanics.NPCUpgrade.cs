using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using StarlightRiver.NPCs.TownUpgrade;
using StarlightRiver.Physics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.CustomHooks
{
    class NPCUpgrade : HookGroup
    {
        //justs sets some values on a UI when a vanilla event
        public override SafetyLevel Safety => SafetyLevel.Safe;

        public override void Load()
        {
            On.Terraria.NPC.GetChat += SetUpgradeUI;
        }

        private string SetUpgradeUI(On.Terraria.NPC.orig_GetChat orig, NPC self)
        {
            if (StarlightWorld.TownUpgrades.TryGetValue(self.TypeName, out bool unlocked))
            {
                if (unlocked)
                    StarlightRiver.Instance.Chatbox.SetState(TownUpgrade.FromString(self.TypeName));
                else
                    StarlightRiver.Instance.Chatbox.SetState(new LockedUpgrade());
            }
            else
                StarlightRiver.Instance.Chatbox.SetState(null);

            return orig(self);
        }
    }
}