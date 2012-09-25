﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Utopia.Shared.Entities.Interfaces;

namespace Utopia.Editor
{
    public partial class FrmEntityChoose : Form
    {
        private static List<Type> _possibleTypes;

        static FrmEntityChoose()
        {
            var type = typeof(IEntity);
            _possibleTypes = AppDomain.CurrentDomain.GetAssemblies().ToList()
                .SelectMany(s => s.GetTypes())
                .Where(p => type.IsAssignableFrom(p) && !p.IsAbstract).ToList();
        }

        public Type SelectedType { get; set; }

        public FrmEntityChoose()
        {
            InitializeComponent();

            comboBoxTypes.Items.AddRange(_possibleTypes.ToArray());
        }

        private void buttonOk_Click(object sender, EventArgs e)
        {
            SelectedType = (Type)comboBoxTypes.SelectedItem;
        }
    }
}
