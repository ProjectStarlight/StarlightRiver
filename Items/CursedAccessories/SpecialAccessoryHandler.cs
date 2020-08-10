using Microsoft.Xna.Framework;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Items.CursedAccessories
{
    internal class SpecialAccessoryHandler : ModPlayer
    {
        public override void DrawEffects(PlayerDrawInfo drawInfo, ref float r, ref float g, ref float b, ref float a, ref bool fullBright)
        {
            /*
            //updates visuals for cursed accessories
            CursedAccessory.Bootlegdust.ForEach(BootlegDust => BootlegDust.Update());
            CursedAccessory.Bootlegdust.RemoveAll(BootlegDust => BootlegDust.time <= 0);
            //updates visuals for blessed accessories
            BlessedAccessory.Bootlegdust.ForEach(BootlegDust => BootlegDust.Update());
            BlessedAccessory.Bootlegdust.RemoveAll(BootlegDust => BootlegDust.time <= 0);

            BlessedAccessory.Bootlegdust2.ForEach(BootlegDust => BootlegDust.Update());
            BlessedAccessory.Bootlegdust2.RemoveAll(BootlegDust => BootlegDust.time <= 0);
            */
        }

        private void CheckFail(Item item)
        {
            if ((item.modItem as BlessedAccessory).TestCondition())
            {
                Main.NewText("Your " + item.modItem.Name + " shattered...", 255, 242, 161);
                Main.PlaySound(SoundID.Shatter, player.Center);
                for (int k = 0; k <= 30; k++)
                {
                    Dust.NewDustPerfect(player.Center, DustType<Dusts.Gold>(), Vector2.One.RotatedByRandom(6.28) * k / 10, 0, default, 2);
                }

                item.TurnToAir();
            }
        }

        public override void PreUpdate()
        {
            foreach (Item item in player.inventory.Where(item => item.modItem is BlessedAccessory))
            {
                CheckFail(item);
            }
            foreach (Item item in player.armor.Where(item => item.modItem is BlessedAccessory))
            {
                CheckFail(item);
            }
        }
    }
}