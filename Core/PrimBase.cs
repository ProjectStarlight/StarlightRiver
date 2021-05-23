using Terraria.ModLoader;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Terraria;
using System.Collections.Generic;
using System.Linq;
using System;
using static Terraria.ModLoader.ModContent;
using System.Reflection;

namespace StarlightRiver.Core
{
    public class PrimTrailManager
    {
        public List<PrimTrail> _trails = new List<PrimTrail>();

        public RenderTarget2D primTarget;
        public void LoadContent(GraphicsDevice GD)
        {
			primTarget = new RenderTarget2D(GD, Main.screenWidth/2, Main.screenHeight/2);
		}
        public void DrawTrails(SpriteBatch spriteBatch, GraphicsDevice gD)
        {
            RenderTargetBinding[] bindings = gD.GetRenderTargets();

			gD.SetRenderTarget(primTarget);
			gD.Clear(Color.Transparent);
            spriteBatch.Begin();
            foreach (PrimTrail trail in _trails.ToArray())
            {
                if (trail.pixellated && !trail.disabled)
                    trail.Draw();
            }
            spriteBatch.End();
            gD.SetRenderTargets(bindings);
        }
        public void DrawTarget(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
            if(primTarget != null)
			    Main.spriteBatch.Draw(primTarget, new Rectangle(0,0,Main.screenWidth, Main.screenHeight), Color.White);
            spriteBatch.End();
            foreach (PrimTrail trail in _trails.ToArray())
            {
                if (!trail.pixellated && !trail.disabled)
                    trail.Draw();
            }
        }
        public void UpdateTrails()
        {
            foreach (PrimTrail trail in _trails.ToArray())
            {
                trail.Update();
            }
        }
        public void CreateTrail(PrimTrail PT) => _trails.Add(PT);

    }
    public partial class PrimTrail
    {
        public bool pixellated = false;
        public bool disabled = false;
        public List<Vector2> _points = new List<Vector2>();

        protected bool _destroyed = false;
        protected Projectile _projectile;
        protected NPC _npc;
        protected float _width;
        protected float _alphaValue;
        protected int _cap;
        protected ITrailShader _trailShader;
        protected int _counter;
        protected int _noOfPoints;
        protected Color _color = new Color(255, 255, 255);

        protected GraphicsDevice _device;
        protected Effect _effect;
        protected BasicEffect _basicEffect;

        protected VertexPositionColorTexture[] vertices;
        protected int currentIndex;
        public PrimTrail(Projectile projectile)
        {
            _trailShader = new DefaultShader();
            _device = Main.graphics.GraphicsDevice;
            _basicEffect = new BasicEffect(_device);
            _basicEffect.VertexColorEnabled = true;
            _projectile = projectile;
            SetDefaults();
            vertices = new VertexPositionColorTexture[_cap];
        }


        public void Dispose()
        {
            StarlightRiver.primitives._trails.Remove(this);
        }
        public void Update()
        {
            OnUpdate();
        }
        public virtual void OnUpdate()
        {

        }
        public void Draw()
        {
            vertices = new VertexPositionColorTexture[_noOfPoints];
            currentIndex = 0;

            PrimStructure(Main.spriteBatch);
            SetShaders();
            if (_noOfPoints >= 1)
            {
                _device.DrawUserPrimitives(PrimitiveType.TriangleList, vertices, 0, _noOfPoints / 3);
            }
        }
        public virtual void PrimStructure(SpriteBatch spriteBatch)
        {

        }
        public virtual void SetShaders()
        {

        }
        public virtual void SetDefaults()
        {

        }

        public virtual void OnDestroy()
        {

        }
        //Helper methods
    }
}