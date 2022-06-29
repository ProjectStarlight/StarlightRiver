using StarlightRiver.Items.Armor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace StarlightRiver.Core.PlayerLayers
{
    public class ChestMaskLayer : PlayerDrawLayer
    {
        private const int CHESTVANITYSLOT = 11;
        private const int CHESTARMORSLOT = 1;

        public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.Torso);

        protected override void Draw(ref PlayerDrawSet drawInfo)
        {
            if (drawInfo.drawPlayer.armor[CHESTVANITYSLOT].IsAir && drawInfo.drawPlayer.armor[CHESTARMORSLOT].ModItem is IArmorLayerDrawable) 
                (drawInfo.drawPlayer.armor[CHESTARMORSLOT].ModItem as IArmorLayerDrawable).DrawArmorLayer(drawInfo);
            else if (drawInfo.drawPlayer.armor[CHESTVANITYSLOT].ModItem is IArmorLayerDrawable) 
                (drawInfo.drawPlayer.armor[CHESTVANITYSLOT].ModItem as IArmorLayerDrawable).DrawArmorLayer(drawInfo);
        }
    }
}
