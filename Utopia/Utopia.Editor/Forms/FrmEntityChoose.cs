using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Utopia.Shared.Entities.Concrete;
using Utopia.Shared.Entities.Interfaces;

namespace Utopia.Editor.Forms
{
    public partial class FrmEntityChoose : Form
    {
        private static List<Type> _possibleTypes;

        static FrmEntityChoose()
        {
            var type = typeof(IEntity);

            var query = from assembly in AppDomain.CurrentDomain.GetAssemblies()
                        from t in assembly.GetTypes()
                        where
                            type.IsAssignableFrom(t) && !t.IsAbstract &&
                            t.GetCustomAttributes(typeof(EditorHideAttribute), true).Length == 0
                        select t;



            // 

            _possibleTypes = query.ToList();

            //_possibleTypes = AppDomain.CurrentDomain.GetAssemblies().ToList()
            //    .SelectMany(s => s.GetTypes())
            //    .Where(p => type.IsAssignableFrom(p) && !p.IsAbstract).ToList();
        }

        public Type SelectedType { get; set; }

        public FrmEntityChoose()
        {
            InitializeComponent();

            comboBoxTypes.Items.AddRange(_possibleTypes.ToArray());
        }

        private void buttonOk_Click(object sender, EventArgs e)
        {
            if (comboBoxTypes.SelectedIndex == -1)
            {
                System.Media.SystemSounds.Beep.Play();
                return;
            }

            SelectedType = (Type)comboBoxTypes.SelectedItem;
            DialogResult = DialogResult.OK;
            Close();
        }

        private void FrmEntityChoose_Load(object sender, EventArgs e)
        {
            if (SelectedType != null)
            {
                comboBoxTypes.SelectedItem = SelectedType;
            }
        }
    }
}
