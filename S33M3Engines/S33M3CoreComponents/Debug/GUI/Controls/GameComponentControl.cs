using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3CoreComponents.GUI.Nuclex.Controls.Arcade;
using SharpDX;
using S33M3CoreComponents.GUI.Nuclex.Controls.Desktop;
using S33M3CoreComponents.GUI.Nuclex;
using S33M3CoreComponents.GUI.Nuclex.Controls;
using S33M3DXEngine.Main;
using S33M3Resources.Structs;
using S33M3DXEngine.Main.Interfaces;

namespace S33M3CoreComponents.Debug.GUI.Controls
{

    public class GameComponentControlRow
    {
        public LabelControl lblGameComp;
        public OptionControl optUpdatable;
        public InputControl updateOrder;
        public OptionControl optDrawable;
        public InputControl drawOrder;
        public Control _parent;
        public IGameComponent _gamecomp;

        private float _y;
        private int _drawOrderindex = -1;

        public GameComponentControlRow(Control parent, float y)
        {
            _parent = parent;
            _y = y;

            lblGameComp = new LabelControl();
            lblGameComp.Bounds = new UniRectangle(10.0f, _y, 110.0f, 18.0f);

            optUpdatable = new OptionControl();
            optUpdatable.Bounds = new UniRectangle(200.0f, _y, 20.0f, 16.0f);
            optUpdatable.Changed += optUpdatable_Changed;

            updateOrder = new InputControl();
            updateOrder.Bounds = new UniRectangle(220.0f, y, 50.0f, 18.0f);
            updateOrder.EnterKeyPressed += updateOrder_EnterKeyPressed;

            optDrawable = new OptionControl();
            optDrawable.Bounds = new UniRectangle(290.0f, y, 20.0f, 16.0f);
            optDrawable.Changed += optDrawable_Changed;

            drawOrder = new InputControl();
            drawOrder.Bounds = new UniRectangle(310.0f, y, 50.0f, 18.0f);
            drawOrder.EnterKeyPressed += drawOrder_EnterKeyPressed;
        }

        public void DetachAll()
        {
            if (lblGameComp.Parent != null) lblGameComp.RemoveFromParent();
            if (optUpdatable.Parent != null) optUpdatable.RemoveFromParent();
            if (updateOrder.Parent != null) updateOrder.RemoveFromParent();
            if (optDrawable.Parent != null) optDrawable.RemoveFromParent();
            if (drawOrder.Parent != null) drawOrder.RemoveFromParent();
        }

        void optUpdatable_Changed(object sender, EventArgs e)
        {
            ((IUpdatableComponent)_gamecomp).Updatable = !((IUpdatableComponent)_gamecomp).Updatable;
        }

        void updateOrder_EnterKeyPressed(object sender, EventArgs e)
        {
            int newOrder;
            if (int.TryParse(updateOrder.Text, out newOrder))
            {
                updateOrder.Color = Colors.Red;
                ((IUpdatableComponent)_gamecomp).UpdateOrder = newOrder;
            }
        }

        void drawOrder_EnterKeyPressed(object sender, EventArgs e)
        {
            int newOrder;
            if (int.TryParse(drawOrder.Text, out newOrder))
            {
                drawOrder.Color = Colors.Red;
                ((IDrawableComponent)_gamecomp).DrawOrders.UpdateIndex(_drawOrderindex, newOrder);
            }
        }

        void optDrawable_Changed(object sender, EventArgs e)
        {
            ((IDrawableComponent)_gamecomp).Visible = !((IDrawableComponent)_gamecomp).Visible;
        }

        public void SetGameComponent(IGameComponent gamecomp, int drawIndex)
        {
            DetachAll();

            _gamecomp = gamecomp;
            _drawOrderindex = drawIndex;

            lblGameComp.Text = _gamecomp.Name;
            _parent.Children.Add(lblGameComp);                  //Show the Label

            if (drawIndex == 0)
            {
                IUpdatableComponent updateableComponent = _gamecomp as IUpdatableComponent;
                if (updateableComponent != null)
                {
                    optUpdatable.Selected = updateableComponent.Updatable;
                    _parent.Children.Add(optUpdatable);

                    updateOrder.Text = updateableComponent.UpdateOrder.ToString();
                    _parent.Children.Add(updateOrder);
                }
            }

            IDrawableComponent drawableComponent = _gamecomp as IDrawableComponent;
            if (drawableComponent != null)
            {
                if (drawIndex == 0)
                {
                    optDrawable.Selected = drawableComponent.Visible;
                    _parent.Children.Add(optDrawable);
                }

                lblGameComp.Text += " " + drawableComponent.DrawOrders.DrawOrdersCollection[drawIndex].Name;

                drawOrder.Text = drawableComponent.DrawOrders.DrawOrdersCollection[drawIndex].Order.ToString();
                _parent.Children.Add(drawOrder);
            }
        }
    }

    public class GameCompHolder
    {
        public IGameComponent GameComp;
        public int IndexId;
        public int OrderId;
    }

    public class GameComponentControl : PanelControl
    {
        #region Private variables
        private Game _game;
        private int _nbrRowsToShow = 17;
        private VerticalSliderControl _vsc;

        private const float Step = 20f;
        private int _listOffset = 0;
        private int _totalItems;
        private bool _isDrawingSorted = true;
        private bool _sortDescending = true;
        private List<GameComponentControlRow> Rows = new List<GameComponentControlRow>();
        #endregion

        #region Public variables
        #endregion

        public GameComponentControl(Control parent, UniRectangle bounds, Game game)
        {
            this.Bounds = bounds;
            parent.Children.Add(this);

            _game = game;
            _game.GameComponents.ComponentAdded += GameComponents_Changed;
            _game.GameComponents.ComponentRemoved += GameComponents_Changed;

            BuildWindow();
        }

        void GameComponents_Changed(object sender, GameComponentCollectionEventArgs e)
        {
            RefreshDataGrid();
        }

        public override void Dispose()
        {
            _game.GameComponents.ComponentAdded -= GameComponents_Changed;
            _game.GameComponents.ComponentRemoved -= GameComponents_Changed;
            base.Dispose();
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
            LabelControl ColumnTitles;
            ColumnTitles = ToDispose(new LabelControl());
            ColumnTitles.FontStyle = System.Drawing.FontStyle.Bold;
            ColumnTitles.Bounds = new UniRectangle(10.0f, 5.0f, 110.0f, 18.0f);
            ColumnTitles.Text = "Comp. type";
            Children.Add(ColumnTitles);

            ColumnTitles = ToDispose(new LabelControl());
            ColumnTitles.FontStyle = System.Drawing.FontStyle.Bold;
            ColumnTitles.Bounds = new UniRectangle(200.0f, 5.0f, 90.0f, 18.0f);
            ColumnTitles.Text = "Updating";
            ColumnTitles.Color = Colors.DarkBlue;
            ColumnTitles.WithForcedColor = true;
            ColumnTitles.Clicked += ColumnTitlesUpdating_Clicked;
            Children.Add(ColumnTitles);

            ColumnTitles = ToDispose(new LabelControl());
            ColumnTitles.FontStyle = System.Drawing.FontStyle.Bold;
            ColumnTitles.Bounds = new UniRectangle(290.0f, 5.0f, 110.0f, 18.0f);
            ColumnTitles.Text = "Drawing";
            ColumnTitles.Color = Colors.DarkBlue;
            ColumnTitles.WithForcedColor = true;
            ColumnTitles.Clicked += ColumnTitlesDrawing_Clicked;
            Children.Add(ColumnTitles);

            _vsc = ToDispose(new VerticalSliderControl());
            _vsc.Bounds = new UniRectangle(this.Bounds.Size.X - 20, 30.0f, 15.0f, this.Bounds.Size.Y - 35);
            _vsc.LayoutFlags = ControlLayoutFlags.WholeRow;
            _vsc.Moved += new EventHandler(_vsc_Moved);
            Children.Add(_vsc);

            float y = 25f;
            //Create the 20 components lists
            for (int i = 0; i < _nbrRowsToShow; i++)
            {
                Rows.Add(new GameComponentControlRow(this, y));
                y = y + Step;
            }

            RefreshDataGrid();
        }

        void ColumnTitlesUpdating_Clicked(object sender, EventArgs e)
        {
            if (_isDrawingSorted)
            {
                _isDrawingSorted = false;
                _sortDescending = true;
            }
            else
            {
                _sortDescending = !_sortDescending;
            }

            RefreshDataGrid();
        }

        void ColumnTitlesDrawing_Clicked(object sender, EventArgs e)
        {
            if (_isDrawingSorted == false)
            {
                _isDrawingSorted = true;
                _sortDescending = true;
            }
            else
            {
                _sortDescending = !_sortDescending;
            }

            RefreshDataGrid();
        }

        /// <summary>Updates the size and position of the list's slider</summary>
        private void updateSlider()
        {
            if ((Screen != null))
            {
                _vsc.ThumbSize = Math.Min(1.0f, _nbrRowsToShow / (float)_totalItems);
            }
        }

        private void _vsc_Moved(object sender, EventArgs e)
        {
            _listOffset = (int)Math.Min(S33M3CoreComponents.Maths.MathHelper.Lerp(0, _totalItems, _vsc.ThumbPosition), _totalItems - _nbrRowsToShow);

            RefreshDataGrid();
        }

        private void RefreshDataGrid()
        {
            _totalItems = _game.GameComponents.Count;
            int rowid = 0;

            List<GameCompHolder> GameCompHolders = new List<GameCompHolder>();

            //Get full list of gamecomponent, making the drawcall things "flat"
            foreach (var gc in _game.GameComponents)
            {
                IDrawableComponent drawable = gc as IDrawableComponent;
                if (drawable == null)
                {
                    int order = ((IUpdatableComponent)gc).UpdateOrder;
                    if (_isDrawingSorted)
                    {
                        if (_sortDescending) order = int.MinValue;
                        else order = int.MinValue;
                    }

                    //It's an uptabable components, add it as is
                    GameCompHolders.Add(new GameCompHolder()
                    {
                        GameComp = gc,
                        IndexId = 0,
                        OrderId = order
                    });
                }
                else
                {
                    //ITs an drawable components, need to add each index of it separalty
                    foreach (var draworder in drawable.DrawOrders.DrawOrdersCollection)
                    {
                        int order = drawable.UpdateOrder;
                        if (_isDrawingSorted)
                        {
                            order = draworder.Order;
                        }

                        //It's an uptabable components, add it as is
                        GameCompHolders.Add(new GameCompHolder()
                        {
                            GameComp = gc,
                            IndexId = draworder.DrawID,
                            OrderId = order
                        });

                    }

                }
            }

            if (_sortDescending)
            {
                GameCompHolders = GameCompHolders.OrderByDescending(x => x.OrderId).ToList();
            }
            else
            {
                GameCompHolders = GameCompHolders.OrderBy(x => x.OrderId).ToList();
            }

            for (int i = _listOffset; i < GameCompHolders.Count; i++)
            {
                if (rowid < _nbrRowsToShow)
                {
                    Rows[rowid].SetGameComponent(GameCompHolders[i].GameComp, GameCompHolders[i].IndexId);
                    rowid++;
                }
            }

            for (rowid++; rowid < _nbrRowsToShow; rowid++)
            {
                Rows[rowid].DetachAll();
            }

            updateSlider();
        }

        //Form events
        private void _closeBt_Pressed(object sender, EventArgs e)
        {
            this.RemoveFromParent();
        }
        #endregion

        #region Public methods
        #endregion
    }
}
