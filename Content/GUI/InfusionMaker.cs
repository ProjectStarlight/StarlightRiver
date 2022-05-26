using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using StarlightRiver.Abilities.AbilityContent.Infusions;
using StarlightRiver.Content.Abilities;
using StarlightRiver.Content.Abilities.ForbiddenWinds;
using StarlightRiver.Core;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;
using ProjectStarlight.Interchange;
using ProjectStarlight.Interchange.Utilities;
using static Terraria.ModLoader.ModContent;
using System.IO;
using System.Threading.Tasks;
using System.Threading;
using Terraria.GameContent;
using Terraria.Audio;

namespace StarlightRiver.Content.GUI
{
	class InfusionMaker : SmartUIState
    {
        public override int InsertionIndex(List<GameInterfaceLayer> layers) => layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));

        public override bool Visible => visible;

        public static bool visible;

        public static List<int> infusionOptions = new List<int>();

        private Vector2 basePos = new Vector2(700, 100);
        private float previewFade;

        private Vector2 offset;
        private bool dragging;

        private ParticleSystem particles = new ParticleSystem(AssetDirectory.GUI + "Holy", ParticleUpdate);

        public TextureGIF previewGif = null;

        UIList options = new UIList();
        InfusionMakerSlot inSlot = new InfusionMakerSlot();
        UIImageButton craftButton = new UIImageButton(Request<Texture2D>(AssetDirectory.GUI + "BackButton"));
        UIImageButton exitButton = new UIImageButton(Request<Texture2D>(AssetDirectory.GUI + "ExitButton"));
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
            setElement(inSlot, new Vector2(82, 220), new Vector2(32, 32));
            setElement(options, new Vector2(206, 16), new Vector2(180, 390));
            setElement(craftButton, new Vector2(394, 36), new Vector2(32, 32));
            setElement(exitButton, new Vector2(390, 0), new Vector2(32, 32));
        }

		public override void Draw(SpriteBatch spriteBatch)
        {
            if(new Rectangle((int)(basePos.X), (int)(basePos.Y), 384, 478).Contains(Main.MouseScreen.ToPoint()))
			{
                if (!dragging && Main.mouseLeft)
                {
                    offset = Main.MouseScreen - basePos;
                    dragging = true;
                }

                Main.LocalPlayer.mouseInterface = true;         
            }

            if (dragging)
            {
                basePos = Main.MouseScreen - offset;

                Constrain();
                Recalculate();

                if (!Main.mouseLeft)                                       
                    dragging = false;
            }

            var tex = Request<Texture2D>(AssetDirectory.GUI + "InfusionBack").Value;
            spriteBatch.Draw(tex, basePos, Color.White);

            if (selected != null)
            {
                Vector2 pos = basePos + new Vector2(10, 356);

                foreach (var objective in selected.output.objectives)
                {
                    pos.Y += objective.DrawText(spriteBatch, pos);
                }

                spriteBatch.DrawString(FontAssets.ItemStack.Value, Helpers.Helper.WrapString(selected.output.Item.ToolTip.GetLine(0), 130, FontAssets.ItemStack.Value, 0.8f), basePos + new Vector2(10, 10), Color.White, 0, Vector2.Zero, 0.8f, 0, 0);

                if (previewFade < 1 && !crafting)
                    previewFade += 0.05f;
            }
            
            if(crafting || selected is null)
			{
                if (previewFade > 0)
                    previewFade -= 0.1f;
			}

            if (inSlot.Item != null && inSlot.Item.IsAir)
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
                    inSlot.Item.TurnToAir();
                    inSlot.Item = selected.output.Item.Clone();
                    inSlot.Item.SetDefaults(inSlot.Item.type);
                    craftTime = 0;
                    crafting = false;

                    selected = null;
                }
            }

            particles.DrawParticles(spriteBatch);

            base.Draw(spriteBatch);

            //previewGif?.Draw(spriteBatch, new Rectangle((int)basePos.X + 2, (int)basePos.Y + 142, 192, 192), Color.White * previewFade);
            //previewGif?.UpdateGIF();

            if(previewGif is null && selected != null)
			{
                spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle((int)basePos.X + 2, (int)basePos.Y + 142, 192, 192), Color.Black * previewFade);

                for (int k = 0; k < 5; k++)
                {
                    var pos = basePos + new Vector2(94, 232) + Vector2.UnitX.RotatedBy(Main.GameUpdateCount / 8f + k) * 16;
                    var color = Color.White * ((k + 1) / 5f) * previewFade;
                    spriteBatch.Draw(Request<Texture2D>(AssetDirectory.GUI + "charm").Value, pos, null, color, 0, Vector2.Zero, 0.5f, 0, 0);
                }
			}

            spriteBatch.Draw(Request<Texture2D>(AssetDirectory.GUI + "InfusionVideoFrame").Value, basePos + new Vector2(0, 140), Color.White);
        }

        private void SpawnParticles()
        {
            if (craftTime < 40)
            {
                var p = new Particle(Vector2.Zero, Vector2.Zero, Main.rand.NextFloat(6.28f), 0, Color.White, 20, basePos + new Vector2(96, 236));
                particles.AddParticle(p);
            }

            if(craftTime == 60)
            {
                Terraria.Audio.SoundEngine.PlaySound(SoundID.DD2_BetsyFireballImpact);
                for (int k = 0; k < 100; k++)
                {
                    var p = new Particle(Vector2.Zero, Vector2.Zero, Main.rand.NextFloat(6.28f), 0, Color.White, 40, basePos + new Vector2(96, 236));
                    particles.AddParticle(p);
                }
            }
        }

        public void PopulateList() //TODO: Decide if available listing should be ability-specific? with ability specific slates?
        {
            foreach (int n in infusionOptions)
            {
                var imprint = new InfusionRecipieEntry(n);
                imprint.Width.Set(100, 0);
                imprint.Height.Set(28, 0);
                imprint.Left.Set(12, 0);
                options.Add(imprint);
            }

            options._items.Sort((a, b) => (a as InfusionRecipieEntry).output.Visible ? 0 : 1);
        }

        public override void OnInitialize()
        {
            Append(inSlot);
            Append(options);
            craftButton.OnClick += Craft;
            Append(craftButton);
            exitButton.OnClick += (a, b) => visible = false;
            Append(exitButton);                     
        }

        private void Craft(UIMouseEvent evt, UIElement listeningElement)
        {
            if (selected is null) return;

            Terraria.Audio.SoundEngine.PlaySound(SoundID.DD2_DarkMageCastHeal);
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
        public static CancellationTokenSource tokenSource;

        public InfusionImprint output;

        public InfusionRecipieEntry(int type)
        {
            var Item = new Item();
            Item.SetDefaults(type);
            output = (InfusionImprint)Item.ModItem;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            var pos = GetDimensions().ToRectangle().TopLeft();

            if(output != null)
            {
                if (output.Visible)
                {
                    var tex = Request<Texture2D>(output.Texture).Value;
                    output.Draw(spriteBatch, pos + new Vector2(14, 13), 1, false);
                    var color = (Parent.Parent.Parent as InfusionMaker).selected == this ? Color.Yellow : Color.White;
                    spriteBatch.DrawString(Terraria.GameContent.FontAssets.ItemStack.Value, output.Item.Name, pos + new Vector2(32, 6), color, 0, Vector2.Zero, 0.8f, 0, 0);
                }
                else
				{
                    var tex = Request<Texture2D>(output.FrameTexture).Value;
                    spriteBatch.Draw(tex, pos, Color.Gray);
                    spriteBatch.DrawString(Terraria.GameContent.FontAssets.ItemStack.Value, "???", pos + new Vector2(32, 6), Color.Gray, 0, Vector2.Zero, 0.8f, 0, 0);
                }
            }
        }

        public override void Click(UIMouseEvent evt)
        {
            if (!output.Visible)
                return;

            var parent = Parent.Parent.Parent;

            if (parent is InfusionMaker && !(parent as InfusionMaker).crafting)
            {
                (parent as InfusionMaker).selected = this;
                (parent as InfusionMaker).previewGif = null;

                tokenSource?.Cancel();

                tokenSource = new CancellationTokenSource();
                var token = tokenSource.Token;
                Task.Factory.StartNew(n => LoadGif(token), token);
            }

            var soundStyle = new SoundStyle($"{nameof(StarlightRiver)}/Sounds/Slot")
            {
                Volume = 0.5f,
                Pitch = 1f,
            };

            SoundEngine.PlaySound(soundStyle);
        }

        private void LoadGif(CancellationToken token)
		{
            var parent = Parent.Parent.Parent;

            byte[] file = GetFileBytes(output.PreviewVideo + ".gif");
            Stream stream = new MemoryStream(file);

            var gif = GIFBuilder.FromGIFFile(stream, Main.graphics.GraphicsDevice, 2);

            if (token.IsCancellationRequested)
                return;

            //(parent as InfusionMaker).previewGif = gif;
            //(parent as InfusionMaker).previewGif.ShouldLoop = true;
            //(parent as InfusionMaker).previewGif.Play();
        }
    }

    class InfusionMakerSlot : UIElement
    {
        public Item Item;

        public override void Click(UIMouseEvent evt)
        {
            var Player = Main.LocalPlayer;

            if ((Parent as InfusionMaker).crafting)
                return;

            var style = new SoundStyle($"{nameof(StarlightRiver)}/Sounds/Slot")
            {
                Volume = 0.75f
            };

            if (Main.mouseItem.ModItem is InfusionItem)
            {
                Item = Main.mouseItem.Clone();
                Main.mouseItem.TurnToAir();
                SoundEngine.PlaySound(style with { Pitch = 0.5f });
                (Parent as InfusionMaker).PopulateList();
            }
            else if (Main.mouseItem.IsAir && !Item.IsAir)
            {
                Main.mouseItem = Item.Clone();
                Item.TurnToAir();
                SoundEngine.PlaySound(style with { Pitch = 0.1f });
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            var Player = Main.LocalPlayer;
            var pos = GetDimensions().ToRectangle().Center.ToVector2() + Vector2.UnitY;

            if(Item != null && Item.ModItem is InfusionItem)
            {
                (Item.ModItem as InfusionItem).Draw(spriteBatch, pos, 1, false);
            }
            else if (Player.HeldItem.ModItem is InfusionItem)
            {
                (Player.HeldItem.ModItem as InfusionItem).Draw(spriteBatch, pos, 0.35f + (float)Math.Sin(StarlightWorld.rottime) * 0.1f, false);
            }
        }
    }
}
