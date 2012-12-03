using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using S33M3CoreComponents.Config;

namespace S33M3CoreComponents.Sprites2D
{
    /// <summary>
    /// Server specific settings
    /// </summary>
    [XmlRoot("LanguageUnicodeRanges")]
    [Serializable]
    public class LanguageUnicodeRanges : IConfigClass
    {
        [XmlIgnore]
        public static XmlSettingsManager<LanguageUnicodeRanges> Current;

        [XmlElement("Language")]
        public Language[] Languages;

        public void Initialize()
        {
        }
    }

    [Serializable]
    public class Language
    {
        public string Name { get; set; }
        [XmlElement("UnicodeRange")]
        public UnicodeRange[] Ranges;

        public List<int> GetUnicodesKeys()
        {
            List<int> unicodeKeys = new List<int>();
            foreach(UnicodeRange unicodeKeyInRange in  Ranges)
            {
                unicodeKeys.AddRange(unicodeKeyInRange.GetUnicodesKeys());
            }
            return unicodeKeys;
        }
    }

    [Serializable]
    public class UnicodeRange
    {
        public int From { get; set; }
        public int To { get; set; }

        public List<int> GetUnicodesKeys()
        {
            List<int> unicodeKeys = new List<int>();
            for (int i = From; i <= To; i++)
            {
                unicodeKeys.Add(i);
            }
            return unicodeKeys;
        }
    }
}
