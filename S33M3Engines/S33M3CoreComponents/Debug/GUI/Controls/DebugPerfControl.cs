using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3CoreComponents.GUI.Nuclex.Controls.Arcade;
using S33M3CoreComponents.GUI.Nuclex.Controls;
using S33M3CoreComponents.GUI.Nuclex;
using S33M3DXEngine.Main;
using SharpDX;
using S33M3CoreComponents.GUI.Nuclex.Controls.Desktop;
using S33M3DXEngine.Debug;

namespace S33M3CoreComponents.Debug.GUI.Controls
{

    public class DebugPerfControl : PanelControl
    {
        private enum LabelResultColumn
        {
            Component = 0,
            AvgUpdt = 1,
            MaxUpdt = 2,
            AvgDraw = 3,
            MaxDraw = 4
        }

        #region Private variables
        private Game _game;
        private LabelControl _avgFrameTime;
        private LabelControl[,] _displayedResults = new LabelControl[5, 10];
        private int[] _displayResultsColumnPosi = new int[5];
        private LabelResultColumn _columnSorted = LabelResultColumn.Component;
        private bool _orderDescending = false;
        private bool _showInPercent;
        private VerticalSliderControl _vsc;
        private int _totalItems;
        private int _listOffset;
        #endregion

        #region Public properties/variables

        #endregion

        public DebugPerfControl(Control parent, UniRectangle bounds, Game game)
        {
            _game = game;
            this.Bounds = bounds;
            parent.Children.Add(this);

            BuildWindow();
        }

        #region Private methods
        private void BuildWindow()
        {
            this.Color = Colors.Wheat;

            CloseWindowButtonControl closeBt = ToDispose(new CloseWindowButtonControl() { Bounds = new UniRectangle(this.Bounds.Size.X - 20, 5, 15, 15) });
            closeBt.Pressed += (sender, e) => { this.RemoveFromParent(); };
            this.Children.Add(closeBt);

            InitGameComponents();
        }

        private void InitGameComponents()
        {
            _displayResultsColumnPosi[(int)LabelResultColumn.Component] = 10;
            _displayResultsColumnPosi[(int)LabelResultColumn.AvgUpdt] = 160;
            _displayResultsColumnPosi[(int)LabelResultColumn.MaxUpdt] = 220;
            _displayResultsColumnPosi[(int)LabelResultColumn.AvgDraw] = 290;
            _displayResultsColumnPosi[(int)LabelResultColumn.MaxDraw] = 350;

            int y = 10;

            OptionControl oc = ToDispose(new OptionControl());
            oc.Bounds = new UniRectangle(_displayResultsColumnPosi[(int)LabelResultColumn.Component], y, 70.0f, 16.0f);
            oc.Text = "Perf tracing";
            oc.Changed += (sender, e) =>
            {
                _game.ComponentsPerfMonitor.Updatable = !_game.ComponentsPerfMonitor.Updatable;
            };
            oc.Selected = _game.ComponentsPerfMonitor.Updatable;
            Children.Add(oc);

            oc = ToDispose(new OptionControl());
            oc.Bounds = new UniRectangle(_displayResultsColumnPosi[(int)LabelResultColumn.Component] + 100, y, 70.0f, 16.0f);
            oc.Text = "Avg in %";
            oc.Changed += (sender, e) =>
            {
                _showInPercent = !_showInPercent;
            };
            oc.Selected = _showInPercent;
            Children.Add(oc);


            ButtonControl bc = ToDispose(new ButtonControl());
            bc.Bounds = new UniRectangle(_displayResultsColumnPosi[(int)LabelResultColumn.Component] + 170, y, 70.0f, 16.0f);
            bc.Text = "Reset Max";
            bc.Pressed += (sender, e) =>
            {
                _game.ComponentsPerfMonitor.PerfTimer.ResetMinMax();
            };
            Children.Add(bc);

            y += 15;

            LabelControl ColumnTitles;
            ColumnTitles = ToDispose(new LabelControl());
            ColumnTitles.FontStyle = System.Drawing.FontStyle.Regular;
            ColumnTitles.Bounds = new UniRectangle(_displayResultsColumnPosi[(int)LabelResultColumn.Component], y, 110.0f, 18.0f);
            ColumnTitles.Text = "Avg Frame time :";
            Children.Add(ColumnTitles);

            _avgFrameTime = ToDispose(new LabelControl());
            _avgFrameTime.FontStyle = System.Drawing.FontStyle.Regular;
            _avgFrameTime.Bounds = new UniRectangle(_displayResultsColumnPosi[(int)LabelResultColumn.Component] + 90, y, 110.0f, 18.0f);
            _avgFrameTime.Text = "xxx fps";
            Children.Add(_avgFrameTime);

            _vsc = ToDispose(new VerticalSliderControl());
            _vsc.Bounds = new UniRectangle(this.Bounds.Size.X - 20, y + 25, 15.0f, 150.0f);
            _vsc.LayoutFlags = ControlLayoutFlags.WholeRow;
            _vsc.Moved += new EventHandler(_vsc_Moved);
            Children.Add(_vsc);

            y += 15;

            ColumnTitles = ToDispose(new LabelControl());
            ColumnTitles.FontStyle = System.Drawing.FontStyle.Bold;
            ColumnTitles.Bounds = new UniRectangle(_displayResultsColumnPosi[(int)LabelResultColumn.Component], y, 50.0f, 18.0f);
            ColumnTitles.Text = "Component";
            Children.Add(ColumnTitles);

            ColumnTitles = ToDispose(new LabelControl());
            ColumnTitles.FontStyle = System.Drawing.FontStyle.Bold;
            ColumnTitles.Bounds = new UniRectangle(_displayResultsColumnPosi[(int)LabelResultColumn.AvgUpdt], y, 50.0f, 18.0f);
            ColumnTitles.Text = "Avg Updt";
            ColumnTitles.Color = Colors.DarkBlue;
            ColumnTitles.WithForcedColor = true;
            ColumnTitles.Tag = LabelResultColumn.AvgUpdt;
            ColumnTitles.Clicked += ColumnTitles_Clicked;
            Children.Add(ColumnTitles);

            ColumnTitles = ToDispose(new LabelControl());
            ColumnTitles.FontStyle = System.Drawing.FontStyle.Bold;
            ColumnTitles.Bounds = new UniRectangle(_displayResultsColumnPosi[(int)LabelResultColumn.MaxUpdt], y, 50.0f, 18.0f);
            ColumnTitles.Text = "Max Updt";
            ColumnTitles.Color = Colors.DarkBlue;
            ColumnTitles.WithForcedColor = true;
            ColumnTitles.Tag = LabelResultColumn.MaxUpdt;
            ColumnTitles.Clicked += ColumnTitles_Clicked;
            Children.Add(ColumnTitles);

            ColumnTitles = ToDispose(new LabelControl());
            ColumnTitles.FontStyle = System.Drawing.FontStyle.Bold;
            ColumnTitles.Bounds = new UniRectangle(_displayResultsColumnPosi[(int)LabelResultColumn.AvgDraw], y, 50.0f, 18.0f);
            ColumnTitles.Text = "Avg Draw";
            ColumnTitles.Color = Colors.DarkBlue;
            ColumnTitles.WithForcedColor = true;
            ColumnTitles.Tag = LabelResultColumn.AvgDraw;
            ColumnTitles.Clicked += ColumnTitles_Clicked;
            Children.Add(ColumnTitles);

            ColumnTitles = ToDispose(new LabelControl());
            ColumnTitles.FontStyle = System.Drawing.FontStyle.Bold;
            ColumnTitles.Bounds = new UniRectangle(_displayResultsColumnPosi[(int)LabelResultColumn.MaxDraw], y, 50.0f, 18.0f);
            ColumnTitles.Text = "Max Draw";
            ColumnTitles.Color = Colors.DarkBlue;
            ColumnTitles.WithForcedColor = true;
            ColumnTitles.Tag = LabelResultColumn.MaxDraw;
            ColumnTitles.Clicked += ColumnTitles_Clicked;
            Children.Add(ColumnTitles);
            y += 15;

            //Create the result Array
            LabelControl lc;
            int arrayLinePosition;
            for (int column = 0; column < _displayedResults.GetLength(0); column++)
            {
                arrayLinePosition = y;
                for (int line = 0; line < _displayedResults.GetLength(1); line++)
                {
                    lc = ToDispose(new LabelControl());
                    lc.Bounds = new UniRectangle(_displayResultsColumnPosi[column], arrayLinePosition, 50, 18);
                    lc.Text = "";
                    Children.Add(lc);
                    _displayedResults[column, line] = lc;

                    arrayLinePosition += 15;
                }
            }
        }

        void ColumnTitles_Clicked(object sender, EventArgs e)
        {
            LabelControl lc = (LabelControl)sender;

            if ((LabelResultColumn)lc.Tag != _columnSorted)
            {
                _orderDescending = true;
                _columnSorted = (LabelResultColumn)lc.Tag;
            }
            else
            {
                _orderDescending = !_orderDescending;
            }
        }

        /// <summary>Updates the size and position of the list's slider</summary>
        private void updateSlider()
        {
            if ((Screen != null))
            {
                _vsc.ThumbSize = Math.Min(1.0f, _displayedResults.GetLength(1) / (float)_totalItems);
            }
        }

        void _vsc_Moved(object sender, EventArgs e)
        {
            _listOffset = (int)Math.Min(S33M3CoreComponents.Maths.MathHelper.Lerp(0, _totalItems, _vsc.ThumbPosition), _totalItems - _displayedResults.GetLength(1));
        }

        #endregion

        #region Public Methods
        public double UpdateData()
        {
            //Perfom a data update
            double totalTimeFrame = 0;

            IOrderedEnumerable<PerfTimerResult> MainResult = null;
            Dictionary<string, PerfTimerResult> linkedResult = new Dictionary<string, PerfTimerResult>();
            bool mainResultIsUpdate = false;

            //Get List of all components
            HashSet<string> compList = new HashSet<string>(_game.ComponentsPerfMonitor.PerfTimer.PerfTimerResults.Values.Select(x => x.Name));

            double total = 1;
            totalTimeFrame = _game.ComponentsPerfMonitor.PerfTimer.PerfTimerResults.Values.Sum(x => x.AvgInMS);
            if (_showInPercent)
            {
                total = totalTimeFrame / 100;
            }

            switch (_columnSorted)
            {
                case LabelResultColumn.Component:
                    mainResultIsUpdate = true;
                    if (_orderDescending)
                    {
                        MainResult = _game.ComponentsPerfMonitor.PerfTimer.PerfTimerResults.Values.Where(x => x.Suffix == "Update").OrderByDescending(x => x.PerfSamplingName);
                    }
                    else
                    {
                        MainResult = _game.ComponentsPerfMonitor.PerfTimer.PerfTimerResults.Values.Where(x => x.Suffix == "Update").OrderBy(x => x.PerfSamplingName);
                    }

                    foreach (var itemResult in _game.ComponentsPerfMonitor.PerfTimer.PerfTimerResults.Values.Where(x => x.Suffix == "Draw"))
                    {
                        linkedResult.Add(itemResult.Name, itemResult);
                    }
                    break;
                case LabelResultColumn.AvgUpdt:
                    mainResultIsUpdate = true;
                    if (_orderDescending)
                    {
                        MainResult = _game.ComponentsPerfMonitor.PerfTimer.PerfTimerResults.Values.Where(x => x.Suffix == "Update").OrderByDescending(x => x.AvgInMS);
                    }
                    else
                    {
                        MainResult = _game.ComponentsPerfMonitor.PerfTimer.PerfTimerResults.Values.Where(x => x.Suffix == "Update").OrderBy(x => x.AvgInMS);
                    }

                    foreach (var itemResult in _game.ComponentsPerfMonitor.PerfTimer.PerfTimerResults.Values.Where(x => x.Suffix == "Draw"))
                    {
                        linkedResult.Add(itemResult.Name, itemResult);
                    }
                    break;
                case LabelResultColumn.MaxUpdt:
                    mainResultIsUpdate = true;
                    if (_orderDescending)
                    {
                        MainResult = _game.ComponentsPerfMonitor.PerfTimer.PerfTimerResults.Values.Where(x => x.Suffix == "Update").OrderByDescending(x => x.MaxInMS);
                    }
                    else
                    {
                        MainResult = _game.ComponentsPerfMonitor.PerfTimer.PerfTimerResults.Values.Where(x => x.Suffix == "Update").OrderBy(x => x.MaxInMS);
                    }

                    foreach (var itemResult in _game.ComponentsPerfMonitor.PerfTimer.PerfTimerResults.Values.Where(x => x.Suffix == "Draw"))
                    {
                        linkedResult.Add(itemResult.Name, itemResult);
                    }
                    break;
                case LabelResultColumn.AvgDraw:
                    mainResultIsUpdate = false;
                    if (_orderDescending)
                    {
                        MainResult = _game.ComponentsPerfMonitor.PerfTimer.PerfTimerResults.Values.Where(x => x.Suffix == "Draw").OrderByDescending(x => x.AvgInMS);
                    }
                    else
                    {
                        MainResult = _game.ComponentsPerfMonitor.PerfTimer.PerfTimerResults.Values.Where(x => x.Suffix == "Draw").OrderBy(x => x.AvgInMS);
                    }

                    foreach (var itemResult in _game.ComponentsPerfMonitor.PerfTimer.PerfTimerResults.Values.Where(x => x.Suffix == "Update"))
                    {
                        linkedResult.Add(itemResult.Name, itemResult);
                    }
                    break;
                case LabelResultColumn.MaxDraw:
                    mainResultIsUpdate = false;
                    if (_orderDescending)
                    {
                        MainResult = _game.ComponentsPerfMonitor.PerfTimer.PerfTimerResults.Values.Where(x => x.Suffix == "Draw").OrderByDescending(x => x.MaxInMS);
                    }
                    else
                    {
                        MainResult = _game.ComponentsPerfMonitor.PerfTimer.PerfTimerResults.Values.Where(x => x.Suffix == "Draw").OrderBy(x => x.MaxInMS);
                    }

                    foreach (var itemResult in _game.ComponentsPerfMonitor.PerfTimer.PerfTimerResults.Values.Where(x => x.Suffix == "Update"))
                    {
                        linkedResult.Add(itemResult.Name, itemResult);
                    }
                    break;
            }

            int LineToDisplay = 0;
            int MaxLineToDisplay = _displayedResults.GetLength(1);
            int lineNbr = 0;

            _totalItems = MainResult.Count();

            foreach (var data in MainResult)
            {
                compList.Remove(data.Name);
                //Skip the first X in case of offset

                if (lineNbr >= _listOffset)
                {
                    if (LineToDisplay < MaxLineToDisplay)
                    {
                        refreshArray(data, linkedResult, LineToDisplay, mainResultIsUpdate, total, false);
                        LineToDisplay++;
                    }
                }
                lineNbr++;
            }

            _totalItems += compList.Count;

            foreach (var data in compList)
            {
                if (LineToDisplay < MaxLineToDisplay)
                {
                    PerfTimerResult perfData = linkedResult[data];
                    refreshArray(perfData, linkedResult, LineToDisplay, mainResultIsUpdate, total, true);
                    LineToDisplay++;
                }
            }

            if (totalTimeFrame > 0)
            {
                _avgFrameTime.Text = totalTimeFrame.ToString("0.000 ms (") + (1 / totalTimeFrame * 1000.0f).ToString("0.00 fps)");
            }
            else
            {
                _avgFrameTime.Text = "Performance tracking disabled";
            }

            updateSlider();

            if (totalTimeFrame == 0) return 0;
            else return (1 / totalTimeFrame * 1000.0f);
        }

        private double refreshArray(PerfTimerResult data, Dictionary<string, PerfTimerResult> linkedResult, int lineNbr, bool mainResultIsUpdate, double total, bool withEmptyMain = false)
        {
            double totalActionTime = 0;
            PerfTimerResult linkedr;
            int MainAvg, MainMax, linkAvg, linkMax;
            if (mainResultIsUpdate)
            {
                MainAvg = (int)LabelResultColumn.AvgUpdt;
                MainMax = (int)LabelResultColumn.MaxUpdt;
                linkAvg = (int)LabelResultColumn.AvgDraw;
                linkMax = (int)LabelResultColumn.MaxDraw;
            }
            else
            {
                MainAvg = (int)LabelResultColumn.AvgDraw;
                MainMax = (int)LabelResultColumn.MaxDraw;
                linkAvg = (int)LabelResultColumn.AvgUpdt;
                linkMax = (int)LabelResultColumn.MaxUpdt;
            }

            _displayedResults[(int)LabelResultColumn.Component, lineNbr].Text = data.Name;
            if (withEmptyMain == false)
            {
                _displayedResults[MainAvg, lineNbr].Text = (data.AvgInMS / total).ToString("00.000") + (total != 1 ? "%" : "");
                _displayedResults[MainMax, lineNbr].Text = data.MaxInMS.ToString("0.000");
                totalActionTime += data.AvgInMS;
            }
            else
            {
                _displayedResults[MainAvg, lineNbr].Text = "";
                _displayedResults[MainMax, lineNbr].Text = "";
            }

            string AvgInMS = string.Empty;
            string MaxInMS = string.Empty;

            //Get the other linked Draw data
            if (linkedResult.TryGetValue(data.Name, out linkedr))
            {
                AvgInMS = (linkedr.AvgInMS / total).ToString("00.000") + (total != 1 ? "%" : "");
                MaxInMS = linkedr.MaxInMS.ToString("0.000");
                totalActionTime += linkedr.AvgInMS;
            }

            _displayedResults[linkAvg, lineNbr].Text = AvgInMS;
            _displayedResults[linkMax, lineNbr].Text = MaxInMS;

            return totalActionTime;
        }
        #endregion
    }
}
