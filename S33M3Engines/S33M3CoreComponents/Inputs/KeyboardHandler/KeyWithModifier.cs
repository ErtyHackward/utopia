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
    }
}
