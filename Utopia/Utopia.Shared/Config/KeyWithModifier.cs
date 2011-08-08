using System;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace Utopia.Shared.Config
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
