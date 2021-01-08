using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.UI;
using Terraria;
using static Terraria.ModLoader.ModContent;
using Microsoft.Xna.Framework;
using Terraria.GameContent.UI.Elements;
using StarlightRiver.Content.Abilities;
using Terraria.ModLoader;
using ReLogic.Graphics;

namespace StarlightRiver.Content.GUI
{
    class InfusionMaker : SmartUIState
    {
        public override int InsertionIndex(List<GameInterfaceLayer> layers) => layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));

        public override bool Visible => true; //lol

        private Vector2 basePos;

        UIList options = new UIList();
        InfusionMakerSlot inSlot = new InfusionMakerSlot(true);
        InfusionMakerSlot outSlot = new InfusionMakerSlot(false);
        InfusionRecipieEntry selected = null;

        public void Constrain()
        {
            setElement(inSlot, new Vector2(50, 0), new Vector2(32, 32));
            setElement(outSlot, new Vector2(112, 0), new Vector2(32, 32));
            setElement(options, new Vector2(206, 50), new Vector2(180, 410));
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(GetTexture(AssetDirectory.Debug), new Rectangle((int)basePos.X + 2, (int)basePos.Y + 190, 194, 194), Color.White);

            var tex = GetTexture(AssetDirectory.GUI + "InfusionBack");
            spriteBatch.Draw(tex, basePos, Color.White);

            Constrain();
            basePos = new Vector2(800, 200);
            Recalculate();

            spriteBatch.DrawString(Main.fontItemStack, Helpers.Helper.WrapString("Storm the Capital 0/1", 140, Main.fontItemStack, 0.8f), basePos + new Vector2(10, 60), Color.White, 0, Vector2.Zero, 0.8f, 0, 0);
            spriteBatch.DrawString(Main.fontItemStack, "Political Infusion", basePos + new Vector2(240, 70), Color.Yellow, 0, Vector2.Zero, 0.8f, 0, 0);
            spriteBatch.DrawString(Main.fontItemStack, Helpers.Helper.WrapString("Makes your dick longer", 140, Main.fontItemStack, 0.8f), basePos + new Vector2(10, 410), Color.White, 0, Vector2.Zero, 0.8f, 0, 0);

            base.Draw(spriteBatch);
        }

        public override void OnInitialize()
        {
            Append(inSlot);
            Append(outSlot);
            Append(options);
        }

        private void setElement(UIElement element, Vector2 offset, Vector2 size = default)
        {
            element.Left.Set(basePos.X + offset.X, 0);
            element.Top.Set(basePos.Y + offset.Y, 0);

            if(size != default)
            {
                element.Width.Set(size.X, 0);
                element.Height.Set(size.Y, 0);
            }
        }
    }

    class InfusionRecipieEntry : UIElement
    {

    }

    class InfusionMakerSlot : UIElement
    {
        public Item item;
        private bool acceptInput;

        public InfusionMakerSlot(bool acceptInput)
        {
            this.acceptInput = acceptInput;
        }

        public override void Click(UIMouseEvent evt)
        {
            var player = Main.LocalPlayer;

            if(Main.mouseItem.modItem is InfusionItem && acceptInput)
            {
                item = Main.mouseItem.Clone();
                Main.mouseItem.TurnToAir();
                Main.PlaySound(StarlightRiver.Instance.GetLegacySoundSlot(SoundType.Custom, "Sounds/Slot").SoundId, -1, -1, StarlightRiver.Instance.GetLegacySoundSlot(SoundType.Custom, "Sounds/Slot").Style, 0.75f, 0.5f);
            }
            else if (Main.mouseItem.IsAir && !item.IsAir)
            {
                Main.mouseItem = item.Clone();
                item.TurnToAir();
                Main.PlaySound(StarlightRiver.Instance.GetLegacySoundSlot(SoundType.Custom, "Sounds/Slot").SoundId, -1, -1, StarlightRiver.Instance.GetLegacySoundSlot(SoundType.Custom, "Sounds/Slot").Style, 0.75f, 0.1f);
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            var player = Main.LocalPlayer;
            var pos = GetDimensions().ToRectangle().Center.ToVector2() + Vector2.UnitY;

            if(item != null && item.modItem is InfusionItem)
            {
                (item.modItem as InfusionItem).Draw(spriteBatch, pos, 1, false);
            }
            else if (player.HeldItem.modItem is InfusionItem && acceptInput)
            {
                (player.HeldItem.modItem as InfusionItem).Draw(spriteBatch, pos, 0.35f + (float)Math.Sin(StarlightWorld.rottime) * 0.1f, false);
            }
        }
    }
}
