using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using Utopia.Shared.Configuration;
using Utopia.Shared.Entities.Concrete;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Helpers;

namespace Utopia.Editor.Forms
{
    public partial class FrmEntityChoose : Form
    {
        private readonly WorldConfiguration _conf;

        private class TypeWrapper
        {
            public Type Type { get; set; }
            public int TrimChars { get; set; }

            public override string ToString()
            {
                return Type.ToString().Remove(0, TrimChars);
            }
        }

        private static List<TypeWrapper> _possibleTypes;

        static FrmEntityChoose()
        {
            var type = typeof(IEntity);

            var query = from assembly in AppDomain.CurrentDomain.GetAssemblies()
                        from t in assembly.GetLoadableTypes()
                        where
                            type.IsAssignableFrom(t) && !t.IsAbstract &&
                            t.GetCustomAttributes(typeof(EditorHideAttribute), true).Length == 0
                        select t;
            
            _possibleTypes = query.Select(t => new TypeWrapper{ Type = t }).ToList();
        }

        public Type SelectedType { get; set; }

        public FrmEntityChoose(WorldConfiguration conf)
        {
            _conf = conf;
            InitializeComponent();

            string commonPart = _possibleTypes[0].Type.ToString();

            foreach (var t in _possibleTypes)
            {
                var name = t.Type.ToString();

                for (int i = commonPart.Length - 1; i >= 0; i--)
                {
                    if (!name.StartsWith(commonPart))
                        commonPart = commonPart.Remove(i, commonPart.Length - i);
                    else
                        break;
                }
            }

            foreach (var typeWrapper in _possibleTypes)
            {
                typeWrapper.TrimChars = commonPart.Length;
            }

            listBoxTypes.Items.AddRange(_possibleTypes.ToArray<object>());
        }

        private void buttonOk_Click(object sender, EventArgs e)
        {
            if (listBoxTypes.SelectedIndex == -1)
            {
                System.Media.SystemSounds.Beep.Play();
                return;
            }

            SelectedType = ((TypeWrapper)listBoxTypes.SelectedItem).Type;
            DialogResult = DialogResult.OK;
            Close();
        }

        private void FrmEntityChoose_Load(object sender, EventArgs e)
        {
            if (SelectedType != null)
            {
                listBoxTypes.SelectedItem = listBoxTypes.Items.OfType<TypeWrapper>().First(tw => tw.Type == SelectedType);
            }
        }

        private void listBoxTypes_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBoxTypes.SelectedItem == null)
            {
                labelDescription.Text = "Select an item to see the description";
                return;
            }

            var type = ( (TypeWrapper)listBoxTypes.SelectedItem ).Type;
            var desc = type.GetCustomAttributes(typeof(DescriptionAttribute), false);

            if (desc.Length > 0)
            {
                var description = (DescriptionAttribute)desc[0];
                labelDescription.Text = description.Description;
            }
            else
            {
                labelDescription.Text = "This type don't have a description";
            }

            // find examples of usage
            var samples = _conf.BluePrints.Values.Where(t => t.GetType() == type).ToList();

            if (samples.Count > 0)
            {
                labelDescription.Text += string.Format("{0}{0}Example: {1}", Environment.NewLine , string.Join(", ", samples.Take(3)));
            }
        }
    }
}
