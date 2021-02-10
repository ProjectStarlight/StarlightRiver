using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using System.Collections.Generic;
using Terraria;
using Terraria.UI;
using static Terraria.ModLoader.ModContent;

using StarlightRiver.Core;
using StarlightRiver.Content.Abilities;

namespace StarlightRiver.Content.GUI
{
    public class Collection : SmartUIState
    {
        public override int InsertionIndex(List<GameInterfaceLayer> layers) => layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));

        public static Ability ActiveAbility;
        public static bool ShouldReset = false;

        private void AddAbility(Ability ability, Vector2 off)
        {
            AbilityDisplay element = new AbilityDisplay(ability);
            element.Left.Set((int)off.X - 16, 0);
            element.Top.Set((int)off.Y - 16, 0);
            element.Width.Set(32, 0);
            element.Height.Set(32, 0);
            Append(element);
        }

        public override void Update(GameTime gameTime)
        {
            if (!Main.gameMenu && Elements.Count == 0 && Main.LocalPlayer.GetHandler() != null || ShouldReset)
            {
                RemoveAllChildren();
                var abilities = Ability.GetAbilityInstances();

                for (int k = 0; k < abilities.Length; k++)
                {
                    Ability ability = abilities[k];
                    Vector2 pos = new Vector2(100, 300) + new Vector2(-60, 0).RotatedBy(-k / (float)(abilities.Length - 1) * 3.14f);
                    AddAbility(ability, pos);
                }
                ShouldReset = false;
            }
        }
    }

    public class AbilityDisplay : UIElement
    {
        private readonly Ability ability;

        public AbilityDisplay(Ability ability) => this.ability = ability;

        public override void Click(UIMouseEvent evt)
        {
            if (ability != null) Collection.ActiveAbility = ability;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (IsMouseHovering)
                Main.LocalPlayer.mouseInterface = true;

            Vector2 pos = GetDimensions().Center() - Vector2.One;
            Texture2D tex = !Main.LocalPlayer.GetHandler().Unlocked(ability.GetType()) ? GetTexture("StarlightRiver/Assets/GUI/blank") : GetTexture(ability.Texture);

            spriteBatch.Draw(tex, pos, tex.Frame(), Color.White, 0, tex.Size() / 2, 1, 0, 0);

            /*if (Collection.ActiveAbility == Ability) //extra VFX
            {
                if (Ability is Dash)
                {
                    Texture2D dustex = ModContent.GetTexture("StarlightRiver/Assets/GUI/Fire");

                    for (int k = 0; k < 3; k++)
                    {
                        float timer = (float)Math.Sin(LegendWorld.rottime);
                        Vector2 duspos = pos + new Vector2(2, 0) + new Vector2((float)Math.Sin(LegendWorld.rottime * 2 + k * 2) * (10 - timer * 5), timer * 15);
                        Collection.dust.Add(new ExpertDust(dustex, duspos, Vector2.Zero, new Color(200, 240, 255), 1.8f, 30));
                    }
                }

                if (Ability is Wisp)
                {
                    Texture2D dustex = ModContent.GetTexture("StarlightRiver/Assets/GUI/Fire");

                    Vector2 duspos = pos + new Vector2((float)Math.Cos(LegendWorld.rottime) * 2, (float)Math.Sin(LegendWorld.rottime)) * 8f;
                    Collection.dust.Add(new ExpertDust(dustex, duspos, Vector2.Zero, new Color(255, 255, 150), 1.8f, 30));

                    Vector2 duspos2 = pos + new Vector2((float)Math.Cos(LegendWorld.rottime + 1), (float)Math.Sin(LegendWorld.rottime + 1) * 2) * 8f;
                    Collection.dust.Add(new ExpertDust(dustex, duspos2, Vector2.Zero, new Color(255, 255, 150), 1.8f, 30));

                    Vector2 duspos3 = pos + new Vector2((float)Math.Cos(LegendWorld.rottime + 2), (float)Math.Sin(LegendWorld.rottime + 2)) * 16f;
                    Collection.dust.Add(new ExpertDust(dustex, duspos3, Vector2.Zero, new Color(255, 255, 150), 1.8f, 30));
                }
            }*/
        }
    }
}