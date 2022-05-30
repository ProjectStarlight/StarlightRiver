using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Graphics.CameraModifiers;
using Terraria.ModLoader;

namespace StarlightRiver.Core.Systems
{
    internal class MoveModifier : ICameraModifier
    {
        public Func<Vector2, Vector2, float, Vector2> EaseFunction = Vector2.SmoothStep;

        public int MovementDuration = 0;
        public int Timer = 0;
        public Vector2 Target = new Vector2(0, 0);
        public bool Returning = false;

        public string UniqueIdentity => "Starlight River Move";

        public bool Finished => false;

        public void PassiveUpdate()
        {
            if (MovementDuration > 0 && Target != Vector2.Zero)
            {
                if (Timer < MovementDuration)
                    Timer++;
            }
        }

        public void Update(ref CameraInfo cameraPosition)
        {
            if (MovementDuration > 0 && Target != Vector2.Zero)
            {
                if(Returning)
                    cameraPosition.CameraPosition = EaseFunction(Target, cameraPosition.OriginalCameraPosition, Timer / (float)MovementDuration);
                else
                    cameraPosition.CameraPosition = EaseFunction(cameraPosition.OriginalCameraPosition, Target, Timer / (float)MovementDuration);
            }
        }

        public void Reset()
        {
            MovementDuration = 0;
            Target = Vector2.Zero;
        }
    }
}
