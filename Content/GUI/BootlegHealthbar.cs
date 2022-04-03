using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Codex;
using StarlightRiver.Core;
using StarlightRiver.Core.Loaders;
using System.Collections.Generic;
using Terraria;
using Terraria.UI;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.GUI
{
    public class BootlegHealthbar : SmartUIState, IOrderedLoadable //PORTTODO: Integrate this into the vanilla health bar somehow
    {
        public override int InsertionIndex(List<GameInterfaceLayer> layers) => layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
        public override bool Visible => visible;

        public static bool visible;

        public static NPC tracked;
        public static string Text;
        public static Texture2D Texture = Request<Texture2D>(AssetDirectory.GUI + "BossBarFrame").Value;
        public static Color glowColor = Color.Transparent;
        public int Timer;

        private static ParticleSystem.Update UpdateShards => UpdateShardsBody;

        

        private static ParticleSystem ShardsSystem;
        private static ParticleSystem ShardsSystem2;

        private Bar bar = new Bar();

        public float Priority => 1f;
        public void Load()
        {
            ShardsSystem = new ParticleSystem("StarlightRiver/Assets/GUI/charm", UpdateShards);
            ShardsSystem2 = new ParticleSystem("StarlightRiver/Assets/GUI/BossbarBack", UpdateShards);
        }

        public void Unload()
        {
            ShardsSystem = null;
            ShardsSystem2 = null;
        }

        public override void OnInitialize()
		{
            bar.Left.Set(-258, 0.5f);
            bar.Top.Set(-80, 1);
            bar.Width.Set(516, 0);
            bar.Height.Set(46, 0);
            Append(bar);
		}

        private static void UpdateShardsBody(Particle particle)
        {
            particle.Color = Color.White;
            particle.Rotation += particle.Velocity.X * 0.1f;
            particle.Position += particle.Velocity;
            particle.Velocity.Y += 0.2f;

            if (particle.Timer < 60)
                particle.Color = Color.White * (particle.Timer / 60f);

            particle.Timer--;
        }

        public override void Update(GameTime gameTime)
		{
            Recalculate();

            if (tracked is null || tracked.life <= 0 || !tracked.active || !tracked.boss)
			{             
                if (Timer == 0)
                {
                    Timer = 120;

                    ShardsSystem.SetTexture(Texture);

                    for (int x = 0; x < Texture.Width; x += 86)
                        for (int y = 0; y < Texture.Height; y += 23)
                        {
                            var pos = bar.GetDimensions().Position() + new Vector2(x + 43, y + 11.5f);
                            var vel = Vector2.UnitY.RotatedByRandom(0.3f) * -Main.rand.NextFloat(1, 4);
                            var frame = new Rectangle(x, y, 86, 23);

                            ShardsSystem.AddParticle(new Particle(pos, vel, 0, 1, Color.White, 120, Vector2.Zero, frame));
                        }

                    var tex = Request<Texture2D>("StarlightRiver/Assets/GUI/BossbarBack").Value;

                    for (int x = 0; x < tex.Width; x += 12)
                        for (int y = 0; y < tex.Height; y += 11)
                        {
                            var pos = bar.GetDimensions().Position() + new Vector2(x + 30, y + 14);
                            var vel = Vector2.UnitY.RotatedByRandom(0.55f) * -Main.rand.NextFloat(1, 4);
                            var frame = new Rectangle(x, y, 12, 11);

                            ShardsSystem2.AddParticle(new Particle(pos, vel, 0, 1, Color.White, 120, Vector2.Zero, frame));
                        }
                }

                if (Timer == 1)
                    visible = false;

                Timer--;
            }

            if (tracked is null)
                visible = false;
        }

		public override void Draw(SpriteBatch spriteBatch)
		{
            if (Timer <= 0)
                base.Draw(spriteBatch);
            else
            {
                ShardsSystem2.DrawParticles(spriteBatch);
                ShardsSystem.DrawParticles(spriteBatch);
            }
		}

		public static void SetTracked(NPC NPC, string text = "", Texture2D tex = default)
        {
            tracked = NPC;
            Text = text;
            visible = true;

            if (tex != default)
                Texture = tex;
        }
    }

    public class Bar : UIElement
	{
		public override void Draw(SpriteBatch spriteBatch)
		{
            var NPC = BootlegHealthbar.tracked;
            var pos = GetDimensions().ToRectangle().TopLeft();
            var off = new Vector2(30, 14);

            if (NPC is null)
                return;

            var texBack = Request<Texture2D>(AssetDirectory.GUI + "BossbarBack").Value;
            var texFill = Request<Texture2D>(AssetDirectory.GUI + "BossbarFill").Value;
            var texEdge = Request<Texture2D>(AssetDirectory.GUI + "BossbarEdge").Value;
            var texGlow = Request<Texture2D>(AssetDirectory.GUI + "BossbarGlow").Value;

            if (NPC.dontTakeDamage || NPC.immortal)
			{
                texFill = Request<Texture2D>(AssetDirectory.GUI + "BossbarFillImmune").Value;
                texEdge = Request<Texture2D>(AssetDirectory.GUI + "BossbarEdgeImmune").Value;
            }

            int progress = (int)(BootlegHealthbar.tracked?.life / (float)BootlegHealthbar.tracked?.lifeMax * texBack.Width);

            spriteBatch.Draw(texBack, pos + off, Color.White);
            spriteBatch.Draw(texFill, new Rectangle((int)(pos.X + off.X), (int)(pos.Y + off.Y), progress, texFill.Height), Color.White);
            spriteBatch.Draw(texEdge, pos + off + Vector2.UnitX * progress, Color.White);

            spriteBatch.End();
            spriteBatch.Begin(default, BlendState.Additive, default, default, default, default, Main.UIScaleMatrix);

            spriteBatch.Draw(texGlow, pos + off, BootlegHealthbar.glowColor * 0.5f);
            spriteBatch.Draw(texGlow, new Rectangle((int)(pos.X + off.X), (int)(pos.Y + off.Y), progress, texFill.Height), new Rectangle(0, 0, progress, texFill.Height), BootlegHealthbar.glowColor);

            spriteBatch.End();
            spriteBatch.Begin(default, default, default, default, default, default, Main.UIScaleMatrix);

            Utils.DrawBorderString(spriteBatch, NPC.FullName + BootlegHealthbar.Text + ": " + NPC.life + "/" + NPC.lifeMax, pos + new Vector2(BootlegHealthbar.Texture.Width / 2, -20), Color.White, 1, 0.5f, 0);

            spriteBatch.Draw(BootlegHealthbar.Texture, pos, Color.White);           

            if (NPC.dontTakeDamage || NPC.immortal)
                spriteBatch.Draw(Request<Texture2D>(AssetDirectory.GUI + "BossbarChains").Value, pos, Color.White);
        }
	}
}