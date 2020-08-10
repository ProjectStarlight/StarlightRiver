using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;

namespace StarlightRiver.NPCs.Miniboss.Glassweaver.PathingUtils
{
    class BouncePad
    {
        public Rectangle _hitbox;
        public Rectangle _activeRegion;
        public GlassMiniboss _parent;
        public int _strength;
        public bool _cancelMove;

        public BouncePad(Rectangle hitbox, Rectangle activeRegion, GlassMiniboss parent, int strength, bool cancelMove = false)
        {
            _hitbox = hitbox;
            _activeRegion = activeRegion;
            _parent = parent;
            _strength = strength;
            _cancelMove = cancelMove;
        }

        public void Update()
        {
            //Bounce the boss up if it steps on a bounce pad corresponding to it's targeted region
            if (_parent.targetRectangle == _activeRegion && _parent.npc.Hitbox.Intersects(_hitbox) && _parent.npc.velocity.Y == 0) _parent.Jump(_strength, _cancelMove);
        }

        //debug stuff
        public void DebugDraw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Main.magicPixel, drawRect(_hitbox), null, Color.Yellow * 0.5f);
            if (_parent.targetRectangle == _activeRegion) spriteBatch.Draw(Main.magicPixel, drawRect(_hitbox), null, Color.White);
        }

        private Rectangle drawRect(Rectangle input) => new Rectangle(input.X - (int)Main.screenPosition.X, input.Y - (int)Main.screenPosition.Y, input.Width, input.Height);
    }
}
