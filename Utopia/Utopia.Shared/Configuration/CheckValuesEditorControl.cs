using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace Utopia.Shared.Configuration
{
    public partial class CheckValuesEditorControl<T> : UserControl
    {
        CheckedListBox lvwItems = null;
        ToolStrip bottomMenu = null;
        ToolStripButton buttonReset = null;
        ToolStripButton cancelButton = null;
        ToolStripButton okeyButton = null;
        IWindowsFormsEditorService _Service = null;
        List<T> _valuesList;
        List<T> _selectedValuesList;
        bool cancelFlag = false;

        public CheckValuesEditorControl()
        {
            lvwItems = new System.Windows.Forms.CheckedListBox();
            lvwItems.CheckOnClick = true;
            SuspendLayout();

            lvwItems.Dock = System.Windows.Forms.DockStyle.Fill;
            lvwItems.FormattingEnabled = true;
            lvwItems.IntegralHeight = false;
            lvwItems.BorderStyle = BorderStyle.None;
            Controls.Add(this.lvwItems);

            bottomMenu = new ToolStrip();
            bottomMenu.Dock = DockStyle.Bottom;
            bottomMenu.GripStyle = ToolStripGripStyle.Hidden;
            Controls.Add(bottomMenu);

            okeyButton = new ToolStripButton();
            okeyButton.Click += new EventHandler(okeyButton_Click);
            okeyButton.Text = "Ok";
            bottomMenu.Items.Add(okeyButton);

            buttonReset = new ToolStripButton();
            buttonReset.Click += new EventHandler(buttonReset_Click);
            buttonReset.Text = "Reset";
            bottomMenu.Items.Add(buttonReset);

            cancelButton = new ToolStripButton();
            cancelButton.Click += new EventHandler(cancelButton_Click);
            cancelButton.Text = "Cancel";
            bottomMenu.Items.Add(cancelButton);

            Font = new System.Drawing.Font("Tahoma", 8.25F);
            ResumeLayout(false);
        }

        void okeyButton_Click(object sender, EventArgs e)
        {
            // okey button pressed so CloseDropDown, this will finish the edit operation
            _Service.CloseDropDown();
        }

        void buttonReset_Click(object sender, EventArgs e)
        {
            // reset button pressed, so reload values
            Begin(_Service, _valuesList);
        }

        void cancelButton_Click(object sender, EventArgs e)
        {
            // cancel button pressed, close drop down but
            // also set cancelFlag to true
            cancelFlag = true;
            _Service.CloseDropDown();
        }

        // begin edit operation
        public void Begin(IWindowsFormsEditorService service, List<T> ValuesList, List<T> SelectedValuesList = null)
        {
            _valuesList = ValuesList;
            _selectedValuesList = SelectedValuesList;

            _Service = service;
            lvwItems.Items.Clear();

            // prepare list
            foreach (var v in ValuesList)
            {
                bool isChecked = false;
                if (SelectedValuesList != null && SelectedValuesList.Contains(v)) isChecked = true;
                lvwItems.Items.Add(v, isChecked);
            }
        }

        // end edit operation
        public void End()
        {
            cancelFlag = false;
            _Service = null;
        }

        // value which will be calculated from the checked items list
        public IEnumerable<T> GetSelectedValues()
        {
            // if cancel flag set, return original value
            if (cancelFlag)
            {
                if (_selectedValuesList != null)
                {
                    foreach (var i in _selectedValuesList) yield return i;
                }
            }
            else
            {
                for (int i = 0; i < lvwItems.CheckedItems.Count; i++)
                {
                    yield return (T)lvwItems.CheckedItems[i];
                }
            }
        }

        protected override bool ProcessDialogKey(Keys keyData)
        {
            if (((keyData & Keys.KeyCode) == Keys.Return)
                && ((keyData & (Keys.Alt | Keys.Control)) == Keys.None))
            {
                _Service.CloseDropDown();
                return true;
            }
            if (((keyData & Keys.KeyCode) == Keys.Escape)
                && ((keyData & (Keys.Alt | Keys.Control)) == Keys.None))
            {
                cancelFlag = true;
                _Service.CloseDropDown();
                return true;
            }

            return base.ProcessDialogKey(keyData);
        }
        
    }
}
