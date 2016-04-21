using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anarchy_Online_Damage_Meter
{
    class CharacterViewModel
    {
        Character _character;
        public Character Character
        {
            get
            {
                return _character;
            }
            set
            {
                _character = value;
            }
        }

        public string CharacterName
        {
            get { return Character.Name; }
        }
    }
}
