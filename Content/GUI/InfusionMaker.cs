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
using Microsoft.Xna.Framework.Media;
using ProjectStarlight.Interchange;
using ProjectStarlight.Interchange.Utilities;
using Terraria.GameContent.UI.Elements;
using StarlightRiver.Content.Abilities;
using Terraria.ModLoader;
using ReLogic.Graphics;
using StarlightRiver.Abilities.AbilityContent.Infusions;
using StarlightRiver.Content.Abilities.ForbiddenWinds;
using Terraria.ID;

namespace StarlightRiver.Content.GUI
{
    class InfusionMaker : SmartUIState
    {
        public override int InsertionIndex(List<GameInterfaceLayer> layers) => layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));

        public override bool Visible => false; //lol

        private Vector2 basePos;

        private ParticleSystem particles = new ParticleSystem(AssetDirectory.GUI + "Holy", ParticleUpdate);

        UIList options = new UIList();
        InfusionMakerSlot inSlot = new InfusionMakerSlot(true);
        InfusionMakerSlot outSlot = new InfusionMakerSlot(false);
        UIImageButton craftButton = new UIImageButton(GetTexture(AssetDirectory.GUI + "BackButton"));
        TextureGIF previewPlayer = GIFBuilder.FromGIFFile(ModLoader.ModPath + "/TestGif.gif", Main.instance.GraphicsDevice, 2);
        public InfusionRecipieEntry selected;
        public bool crafting;
        public int craftTime;

        private static void ParticleUpdate(Particle particle)
        {
            if (particle.Timer <= 20)
            {
                particle.Position = particle.StoredPosition + Vector2.One.RotatedBy(particle.Rotation) * particle.Timer * 2;
                particle.Scale = (20 - particle.Timer) / 20f;
            }
            else
            {
                particle.Position = particle.StoredPosition + Vector2.One.RotatedBy(particle.Rotation) * (40 - particle.Timer) * 2;
                particle.Scale = (particle.Timer - 20) / 20f;

                if (particle.Timer == 21) 
                    particle.Timer = 0;
            }
            particle.Timer--;
        }

        public void Constrain()
        {
            setElement(inSlot, new Vector2(50, 0), new Vector2(32, 32));
            setElement(outSlot, new Vector2(112, 0), new Vector2(32, 32));
            setElement(options, new Vector2(206, 66), new Vector2(180, 390));
            setElement(craftButton, new Vector2(380, 66), new Vector2(32, 32));
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            previewPlayer.ShouldLoop = true;
            previewPlayer.UpdateGIF();
            previewPlayer.Draw(spriteBatch, new Rectangle((int)basePos.X + 2, (int)basePos.Y + 190, 194, 194), Color.White);

            var tex = GetTexture(AssetDirectory.GUI + "InfusionBack");
            spriteBatch.Draw(tex, basePos, Color.White);

            Constrain();
            basePos = new Vector2(800, 200);
            Recalculate();

            if (selected != null)
            {
                Vector2 pos = basePos + new Vector2(10, 60);
                foreach (var objective in selected.output.objectives)
                {
                    pos.Y += objective.DrawTextAndBar(spriteBatch, pos);
                }

                spriteBatch.DrawString(Main.fontItemStack, Helpers.Helper.WrapString(selected.output.item.ToolTip.GetLine(0), 140, Main.fontItemStack, 0.8f), basePos + new Vector2(10, 410), Color.White, 0, Vector2.Zero, 0.8f, 0, 0);
            }

            if (inSlot.item != null && inSlot.item.IsAir)
            {
                options.Clear();
                selected = null;
            }

            if (crafting)
            {
                craftTime++;

                SpawnParticles();

                if (craftTime >= 60)
                {
                    inSlot.item.TurnToAir();
                    outSlot.item = selected.output.item.Clone();
                    craftTime = 0;
                    crafting = false;
                }
            }

            particles.DrawParticles(spriteBatch);

            base.Draw(spriteBatch);
        }

        private void SpawnParticles()
        {
            if (craftTime < 40)
            {
                var p = new Particle(Vector2.Zero, Vector2.Zero, Main.rand.NextFloat(6.28f), 0, Color.White, 20, basePos + new Vector2(66, 16));
                particles.AddParticle(p);
            }

            if(craftTime == 60)
            {
                Main.PlaySound(SoundID.DD2_BetsyFireballImpact);
                for (int k = 0; k < 100; k++)
                {
                    var p = new Particle(Vector2.Zero, Vector2.Zero, Main.rand.NextFloat(6.28f), 0, Color.White, 40, basePos + new Vector2(128, 16));
                    particles.AddParticle(p);
                }
            }
        }

        public void PopulateList()
        {
            var imprint = new InfusionRecipieEntry(ItemType<AstralImprint>());
            imprint.Width.Set(100, 0);
            imprint.Height.Set(32, 0);
            imprint.Left.Set(12, 0);
            options.Add(imprint);
        }

        public override void OnInitialize()
        {
            Append(inSlot);
            Append(outSlot);
            Append(options);
            craftButton.OnClick += Craft;
            Append(craftButton);

            //previewPlayer.LoadFromPath(ModLoader.ModPath + "/TestGif.gif");
        }

        private void Craft(UIMouseEvent evt, UIElement listeningElement)
        {
            //previewPlayer.LoadFromPath(ModLoader.ModPath + "/TestGif.gif");

            if (selected is null) return;

            Main.PlaySound(SoundID.DD2_DarkMageCastHeal);
            crafting = true;
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
        public InfusionImprint output;

        public InfusionRecipieEntry(int type)
        {
            var item = new Item();
            item.SetDefaults(type);
            output = (InfusionImprint)item.modItem;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            var pos = GetDimensions().ToRectangle().TopLeft();

            if(output != null)
            {
                var tex = GetTexture(output.Texture);
                output.Draw(spriteBatch, pos + Vector2.One * 16, 1, false);
                var color = (Parent.Parent.Parent as InfusionMaker).selected == this ? Color.Yellow : Color.White;
                spriteBatch.DrawString(Main.fontItemStack, output.item.Name, pos + new Vector2(38, 8), color, 0, Vector2.Zero, 0.8f, 0, 0);
            }
        }

        public override void Click(UIMouseEvent evt)
        {
            var parent = Parent.Parent.Parent;
            if (parent is InfusionMaker && !(parent as InfusionMaker).crafting)
            {
                (parent as InfusionMaker).selected = this;
            }

            Main.PlaySound(StarlightRiver.Instance.GetLegacySoundSlot(SoundType.Custom, "Sounds/Slot").SoundId, -1, -1, StarlightRiver.Instance.GetLegacySoundSlot(SoundType.Custom, "Sounds/Slot").Style, 0.5f, 2.5f);
        }
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

            if ((Parent as InfusionMaker).crafting)
                return;

            if (Main.mouseItem.modItem is InfusionItem && acceptInput)
            {
                item = Main.mouseItem.Clone();
                Main.mouseItem.TurnToAir();
                Main.PlaySound(StarlightRiver.Instance.GetLegacySoundSlot(SoundType.Custom, "Sounds/Slot").SoundId, -1, -1, StarlightRiver.Instance.GetLegacySoundSlot(SoundType.Custom, "Sounds/Slot").Style, 0.75f, 0.5f);
                (Parent as InfusionMaker).PopulateList();
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
