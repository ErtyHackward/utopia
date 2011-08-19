using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Utopia.Shared.Roleplay;

namespace SpecialClassDesigner
{
    public partial class Form1 : Form
    {
        private int _additinalPoints = 5;

        private CharacterPrimaryAttributes _primary;
        private CharacterSecondaryAttributes _secondary;

        public Form1()
        {
            InitializeComponent();

            _primary = new CharacterPrimaryAttributes { Strength = 5, Perception = 5, Endurance = 5, Charisma = 5, Intellect = 5, Agility = 5, Luck = 5 };

            UpdateSecondary();
            AdditinalPoints = 5;
        }

        public int AdditinalPoints
        {
            get { return _additinalPoints; }
            set { 
                _additinalPoints = value;
                AdditionalLabel.Text = string.Format("Additional points: {0}", value);
            }
        }

        private bool UpdateAdditional(byte was, byte now)
        {
            if (was > now)
            {
                AdditinalPoints += 1;
                return true;
            }
            if (AdditinalPoints > 0)
            {
                AdditinalPoints -= 1;
                return true;
            }
            return false;
        }

        private void UpdateSecondary()
        {
            _secondary = CharacterSecondaryAttributes.GetStartLevel(_primary);

            propertyGrid1.SelectedObject = _secondary;
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            if (UpdateAdditional(_primary.Strength, (byte)numericUpDown1.Value))
            {
                _primary.Strength = (byte)numericUpDown1.Value;
                UpdateSecondary();
            }
            else
            {
                numericUpDown1.Value = _primary.Strength;
            }

        }

        private void numericUpDown2_ValueChanged(object sender, EventArgs e)
        {
            if (UpdateAdditional(_primary.Perception, (byte)numericUpDown2.Value))
            {
                _primary.Perception = (byte)numericUpDown2.Value;
                UpdateSecondary();
            }
            else
            {
                numericUpDown2.Value = _primary.Perception;
                
            }
            
        }

        private void numericUpDown3_ValueChanged(object sender, EventArgs e)
        {
            if (UpdateAdditional(_primary.Endurance, (byte)numericUpDown3.Value))
            {
                _primary.Endurance = (byte)numericUpDown3.Value;
                UpdateSecondary();
            }
            else
            {
                numericUpDown3.Value = _primary.Endurance;
            }
            
        }

        private void numericUpDown4_ValueChanged(object sender, EventArgs e)
        {
            if (UpdateAdditional(_primary.Charisma, (byte)numericUpDown4.Value))
            {
                _primary.Charisma = (byte)numericUpDown4.Value;
                UpdateSecondary();
            }
            else
            {
                numericUpDown4.Value = _primary.Charisma;
            }
            
        }

        private void numericUpDown5_ValueChanged(object sender, EventArgs e)
        {
            if (UpdateAdditional(_primary.Intellect, (byte)numericUpDown5.Value))
            {
                _primary.Intellect = (byte)numericUpDown5.Value;
                UpdateSecondary();
            }
            else
            {
                numericUpDown5.Value = _primary.Intellect; 
            }
            
        }

        private void numericUpDown6_ValueChanged(object sender, EventArgs e)
        {
            if (UpdateAdditional(_primary.Agility, (byte)numericUpDown6.Value))
            {
                _primary.Agility = (byte)numericUpDown6.Value;
                UpdateSecondary();
            }
            else
            {
                numericUpDown6.Value = _primary.Agility;
            }
            
        }

        private void numericUpDown7_ValueChanged(object sender, EventArgs e)
        {
            if (UpdateAdditional(_primary.Luck, (byte)numericUpDown7.Value))
            {
                _primary.Luck = (byte)numericUpDown7.Value;
                UpdateSecondary();
            }
            else
            {
                numericUpDown7.Value = _primary.Luck;
            }
            
        }
    }
}
