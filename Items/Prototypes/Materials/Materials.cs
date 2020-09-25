using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarlightRiver.Items.Prototypes.Materials
{
    class Tubes : QuickMaterial
    {
        public Tubes() : base("Inverted Vacuum Tube", "A strange glass tube containing an inside-out vacuum", 999, 1000000, 3) { }
    }

    class Wire : QuickMaterial
    {
        public Wire() : base("Infinity Cable", "A wire where electrons and holes flow in the same direction", 999, 1000000, 3) { }
    }

    class Frame : QuickMaterial
    {
        public Frame() : base("Adaptive Casing", "A heavy-duty structural unit made of whatever it wants to be made of", 999, 1000000, 3) { }
    }

    class Board : QuickMaterial
    {
        public Board() : base("Data Module", "A storage module containing downloaded RAM", 999, 1000000, 3) { }
    }
}
