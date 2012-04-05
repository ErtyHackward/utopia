using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.Windows.Forms;

namespace S33M3CoreComponents.Inputs.KeyboardHandler
{
    [Serializable]
    public struct KeyWithModifier
    {
        [XmlText]
        public Keys MainKey;
        [XmlAttribute]
        public Keys Modifier;
        [XmlAttribute]
        public string Info;

        public static implicit operator KeyWithModifier(Keys k)
        {
            KeyWithModifier kwm;
            kwm.MainKey = k;
            kwm.Modifier = Keys.None;
            kwm.Info = string.Empty;
            return kwm;
        }

        public override bool Equals(object obj)
        {
            return (((KeyWithModifier)obj).MainKey == this.MainKey && ((KeyWithModifier)obj).Modifier == this.Modifier);
        }

        public static bool operator ==(KeyWithModifier a, KeyWithModifier b)
        {
            return (a.MainKey == b.MainKey && a.Modifier == b.Modifier);
        }

        public static bool operator !=(KeyWithModifier a, KeyWithModifier b)
        {
            return !(a == b);
        }
    }
}
