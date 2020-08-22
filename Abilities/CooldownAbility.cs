using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarlightRiver.Abilities
{
    public abstract class CooldownAbility : Ability
    {
        public int Cooldown { get; private set; } = -1;
        public int CooldownBonus { get; set; }
        public abstract int CooldownMax { get; }

        public override bool Available => base.Available && Cooldown <= 0;

        public override void OnActivate()
        {
            Cooldown = CooldownMax + CooldownBonus;
            CooldownBonus = 0;
            base.OnActivate();
        }

        public override void UpdateFixed()
        {
            base.UpdateFixed();

            if (!Active)
            {
                Cooldown--;
            }

            if (Cooldown == 0)
                CooldownFinish();
        }

        public virtual void CooldownFinish() { }
    }
}
