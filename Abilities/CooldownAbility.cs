using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarlightRiver.Abilities
{
    public abstract class CooldownAbility : Ability
    {
        public int Cooldown { get; private set; }
        public int CooldownReduction { get; set; }
        public abstract int CooldownMax { get; }

        public override bool Available => base.Available && Cooldown <= 0;

        public override void OnActivate()
        {
            base.OnActivate();
        }

        public override void UpdateFixed()
        {
            base.UpdateFixed();

            if (CooldownReduction > CooldownMax)
                CooldownReduction = CooldownMax;

            if (!Active)
                Cooldown--;
            else
                Cooldown = CooldownMax - CooldownReduction;
            CooldownReduction = 0;

            if (Cooldown == 0)
                CooldownFinish();
        }

        public virtual void CooldownFinish() { }
    }
}
