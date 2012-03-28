using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Utopia.Settings
{
    [Serializable]
    public struct SettingsValue<T>
    {
        [XmlText]
        public T Value;
        [XmlAttribute]
        public string Name;
        [XmlAttribute]
        public string Info;
    }
}
