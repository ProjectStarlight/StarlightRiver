using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Terraria;
using System.Collections.Generic;

namespace StarlightRiver.Core
{
    public class PrimTrailManager
    {
        public List<PrimTrail> _trails = new List<PrimTrail>();

        public RenderTarget2D primTarget; // We'll use a rendertarget to draw primitives pixellated. 

        public BasicEffect basicEffect; //The basiceffect is the same for all trails
        public void LoadContent(GraphicsDevice GD) //Load the rendertarget and the basic effect
        {
            basicEffect = new BasicEffect(GD); //Load the basic effect that all primtrails in this list will use.
            basicEffect.VertexColorEnabled = true; //If we don't have this, the prims will be black and white!

            primTarget = new RenderTarget2D(GD, Main.screenWidth/2, Main.screenHeight/2); //The rendertarget is half the size of the screen, so we can upscale it to the full screen
		}
        public void DrawTrails(SpriteBatch spriteBatch, GraphicsDevice gD)
        {
            RenderTargetBinding[] bindings = gD.GetRenderTargets();

			gD.SetRenderTarget(primTarget);
			gD.Clear(Color.Transparent); //Reset the rendertarget

            spriteBatch.Begin();
            foreach (PrimTrail trail in _trails.ToArray()) //Each trail that is pixellated is drawn to the rendertarget
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
            if(primTarget != null) //Draw the rendertarget to the entire screen
			    Main.spriteBatch.Draw(primTarget, new Rectangle(0,0,Main.screenWidth, Main.screenHeight), Color.White);
            spriteBatch.End();
            foreach (PrimTrail trail in _trails.ToArray()) //Draw the non pixellated rendertargets normally
            {
                if (!trail.pixellated && !trail.disabled)
                    trail.Draw();
            }
        }
        public void UpdateTrails() //Update primtrails
        {
            foreach (PrimTrail trail in _trails.ToArray())
            {
                trail.Update();
            }
        }
        public void CreateTrail(PrimTrail PT)
        {
            _trails.Add(PT); //If a trail is created, add it to the list
            PT._basicEffect = basicEffect;
        }

    }
    public partial class PrimTrail
    {
        public bool pixellated = false; //Is it pixellated?
        public bool disabled = false; //Set to true to make it not drawn (If you want to, for example, use an interface to draw it, you'll want this)

        public List<Vector2> _points = new List<Vector2>(); //Every primtrail includes a list of points. This is important

        protected bool _destroyed = false; //Set this to true when you START to destroy the primtrail. That way, the primtrail doesn't bind itself to a new projectile/npc

        protected Projectile _projectile; //You can either use a Projectile for the trail to follow...

        protected NPC _npc; //Or you can use an NPC

        protected float _width; //The width of the primtrail

        protected float _alphaValue; //The transparency of the primtrail. Set from 0 to 1. 1 is opaque, 0 is transparent

        protected int _cap; //Maximum number of points. Use this to dispose of points beyond the cap.

        protected ITrailShader _trailShader;

        protected int _counter; //If we want to keep track of updates, we can increment this.

        protected int _noOfPoints; //TODO: Get rid of this baloney

        protected Color _color = new Color(255, 255, 255); //The color of the primtrail

        protected GraphicsDevice _device; //The device the primtrail is registered to.

        protected Effect _effect; //A blank effect, if we want to apply shaders to the primtrails.

        public BasicEffect _basicEffect; //This is set when the trail is added to the list

        protected VertexPositionColorTexture[] vertices; //The vertices

        protected int currentIndex; //When we increment through the array of vertices, we'll use this as the index.
        public PrimTrail(Projectile projectile) //ALWAYS call base if you're using a projectile
        {
            _trailShader = new DefaultShader();
            _device = Main.graphics.GraphicsDevice;
            _projectile = projectile;
            vertices = new VertexPositionColorTexture[_cap];
        }


        public void Dispose() //Call this to remove the primtrail from the manager's list
        {
            StarlightRiver.primitives._trails.Remove(this);
        }
        public void Update() //This is called every tick. Keeping Update and OnUpdate separate incase I want to add anything else to Update in the future
        {
            OnUpdate();
        }

        public void Draw() //This is called every frame, after projectiles draw
        {
            vertices = new VertexPositionColorTexture[_noOfPoints]; //Reset out list of vertices and our index
            currentIndex = 0;

            PrimStructure(Main.spriteBatch); //The structure of the primtrail will be different for every trail. 

            SetShaders(); //Set the shaders on the primtrail

            if (_noOfPoints >= 1) //Can't have too many safety checks
            {
                _device.DrawUserPrimitives(PrimitiveType.TriangleList, vertices, 0, _noOfPoints / 3); //Draw the trail!
            }
        }

        public virtual void OnUpdate() //Override this for actions such as adding points, disposal, or anything else not related to drawing.
        {

        }
        public virtual void PrimStructure(SpriteBatch spriteBatch) //Inherit from this to add your vertices.
        {

        }
        public virtual void SetShaders()
        {

        }

        public virtual void OnDestroy()
        {

        }
    }
}