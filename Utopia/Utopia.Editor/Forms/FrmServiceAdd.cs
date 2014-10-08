using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using Utopia.Shared.Configuration;
using Utopia.Shared.Entities.Concrete;
using Utopia.Shared.Helpers;
using Utopia.Shared.Services;

namespace Utopia.Editor.Forms
{
    public partial class FrmServiceAdd : Form
    {
        private readonly WorldConfiguration _conf;

        private class TypeWrapper
        {
            public Type Type { get; set; }
            
            public override string ToString()
            {
                return Type.Name;
            }
        }

        private static List<TypeWrapper> _possibleTypes;

        public Type SelectedType { get; set; }

        static FrmServiceAdd()
        {
            var type = typeof(Service);

            var query = from assembly in AppDomain.CurrentDomain.GetAssemblies()
                        from t in assembly.GetLoadableTypes()
                        where
                            type.IsAssignableFrom(t) && !t.IsAbstract &&
                            t.GetCustomAttributes(typeof(EditorHideAttribute), true).Length == 0
                        select t;
            
            _possibleTypes = query.Select(t => new TypeWrapper{ Type = t }).ToList();
        }

        public FrmServiceAdd(WorldConfiguration conf)
        {
            _conf = conf;
            InitializeComponent();

            foreach (var wrp in _possibleTypes.Where(t => _conf.Services.All(s => s.GetType() != t.Type)))
            {
                listBox1.Items.Add(wrp);
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            SelectedType = ((TypeWrapper)listBox1.SelectedItem).Type;

            var desc = SelectedType.GetCustomAttributes(typeof(DescriptionAttribute), false);

            if (desc.Length > 0)
            {
                var description = (DescriptionAttribute)desc[0];
                label1.Text = description.Description;
            }
            else
            {
                label1.Text = "This type don't have a description";
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem == null)
            {
                MessageBox.Show("Select the type to add", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
