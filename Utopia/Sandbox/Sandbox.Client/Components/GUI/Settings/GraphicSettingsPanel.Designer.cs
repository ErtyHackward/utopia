﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3CoreComponents.GUI.Nuclex.Controls;
using S33M3Resources.Structs;
using S33M3CoreComponents.GUI.Nuclex.Visuals.Flat;
using S33M3CoreComponents.GUI.Nuclex;
using SharpDX.Direct3D11;
using Utopia.Settings;
using System.Reflection;
using S33M3CoreComponents.GUI.Nuclex.Controls.Desktop;
using S33M3CoreComponents.Tools;

namespace Sandbox.Client.Components.GUI.Settings
{
    partial class GraphicSettingsPanel
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        #region Private Variables
        protected LabelControl _panelLabel;

        protected List<ParamRow> Parameters;
        #endregion

        #region Public Variables
        #endregion

        private void InitializeComponent()
        {
            CreateComponents();
            Resize();
            BindComponents();
        }

        #region Public Methods
        #endregion

        #region Private Methods
        private void CreateComponents()
        {
            _panelLabel = new LabelControl()
            {
                Text = "Graphical settings",
                Color = new ByteColor(255, 255, 255),
                CustomFont = SandboxMenuComponent.FontBebasNeue25
            };

            Parameters = new List<ParamRow>(Settings2Components.CreateComponentsRows(ClientSettings.Current.Settings.GraphicalParameters));            
        }

        private void BindComponents()
        {
            this.Children.Add(_panelLabel);
            foreach (ParamRow row in Parameters)
            {
                this.Children.Add(ToDispose(row.ParamName));
                this.Children.Add(ToDispose(row.InputingComp));
                if (row.LabelInfo != null) this.Children.Add(ToDispose(row.LabelInfo));
                if (row.InputingComp is SliderControl)
                {
                    int valueThumb = (int)row.InputingComp.Tag;
                    row.InputingComp.Tag = row;
                    ((SliderControl)row.InputingComp).Moved += SliderControl_Moved;
                    SliderTextResfresh((SliderControl)row.InputingComp);
                    SetSliderThumb((SliderControl)row.InputingComp, valueThumb);
                }
            }
        }

        private void SliderControl_Moved(object sender, EventArgs e)
        {
            SliderTextResfresh((SliderControl)sender);
        }

        private void SetSliderThumb(SliderControl ctr, int value)
        {
            ParamRow row = (ParamRow)ctr.Tag;
            ParameterAttribute attrib = (ParameterAttribute)row.LabelInfo.Tag;
            ctr.ThumbPosition = (float)(value - attrib.MinSliderValue) / (float)((int)attrib.MaxSliderValue - (int)attrib.MinSliderValue);
            SliderTextResfresh(ctr);
        }

        private void SliderTextResfresh(SliderControl ctr)
        {
            ParamRow row = (ParamRow)ctr.Tag;
            ParameterAttribute attrib = (ParameterAttribute)row.LabelInfo.Tag;
            row.LabelInfo.Text = ((int)S33M3CoreComponents.Maths.MathHelper.Lerp((int)attrib.MinSliderValue, (int)attrib.MaxSliderValue, ctr.ThumbPosition)).ToString();
        }

        public void Resize()
        {
            if(this.Parent != null) this.Bounds = new UniRectangle(0,0,this.Parent.Bounds.Size.X.Offset,this.Parent.Bounds.Size.Y.Offset);

            float BorderMargin = 15;
            _panelLabel.Bounds = new UniRectangle(BorderMargin, BorderMargin, 0, 0);

            float lineHeight = 50;

            foreach (ParamRow row in Parameters)
            {
                row.ParamName.Bounds = new UniRectangle(BorderMargin, lineHeight, 1 , 0);
                if (row.InputingComp is HorizontalSliderControl)
                {
                    row.InputingComp.Bounds = new UniRectangle(BorderMargin + 200, lineHeight - 5, 150, 25);
                }
                else if (row.InputingComp is InputControl)
                {
                    row.InputingComp.Bounds = new UniRectangle(BorderMargin + 200, lineHeight - 5, 50, 25);
                }
                if (row.LabelInfo != null) row.LabelInfo.Bounds = new UniRectangle(row.InputingComp.Bounds.Location.X + row.InputingComp.Bounds.Size.X + 10, lineHeight, 1, 0);
                lineHeight += 30;
            }
        }
        #endregion
    }
}
