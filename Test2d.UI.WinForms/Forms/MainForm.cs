// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Test2d;

namespace TestWinForms
{
    /// <summary>
    /// 
    /// </summary>
    public partial class MainForm : Form, IView
    {
        private Drawable _drawable;

        /// <summary>
        /// 
        /// </summary>
        public object DataContext { get; set; }
    
        /// <summary>
        /// 
        /// </summary>
        public MainForm()
        {
            InitializeComponent();

            InitializeContext();
            
            InitializePanel();

            SetContainerInvalidation();
            SetPanelSize();

            HandlePanelShorcutKeys();
            HandleMenuShortcutKeys();

            HandleFileDialogs();

            FormClosing += (s, e) => DeInitializeContext();

            UpdateToolMenu();
            UpdateOptionsMenu();
        }

        /// <summary>
        /// 
        /// </summary>
        private void InitializeContext()
        {
            this.SetStyle(
                ControlStyles.AllPaintingInWmPaint
                | ControlStyles.UserPaint
                | ControlStyles.DoubleBuffer
                | ControlStyles.SupportsTransparentBackColor,
                true);

            var context = new EditorContext()
            {
                View = this,
                Renderers = new IRenderer[] { new EmfRenderer(72.0 / 96.0) },
                ProjectFactory = new ProjectFactory(),
                TextClipboard = new TextClipboard(),
                Serializer = new NewtonsoftSerializer(),
                PdfWriter = new PdfWriter(),
                DxfWriter = new DxfWriter(),
                CsvReader = new CsvHelperReader(),
                CsvWriter = new CsvHelperWriter()
            };
            context.InitializeEditor(new TraceLog());
            context.Editor.Renderers[0].State.DrawShapeState = ShapeState.Visible;
            context.Editor.GetImageKey = async () => await GetImageKey();

            DataContext = context;
        }

        /// <summary>
        /// 
        /// </summary>
        private void DeInitializeContext()
        {
            var context = DataContext as EditorContext;
            if (context == null)
                return;

            context.Dispose();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private void InitializePanel()
        {
            var context = DataContext as EditorContext;
            if (context == null)
                return;

            _drawable = new Drawable();

            _drawable.Context = context;
            _drawable.InvalidateContainer = InvalidateContainer;
            _drawable.Initialize();

            _drawable.Anchor = (AnchorStyles.Top|AnchorStyles.Left);
            _drawable.Dock = DockStyle.Fill;
            _drawable.Name = "containerPanel";
            _drawable.TabIndex = 0;

            this.SuspendLayout();
            this.Controls.Add(_drawable);
            this.ResumeLayout(false);

            _drawable.Select();
        }

        /// <summary>
        /// 
        /// </summary>
        private void InvalidateContainer()
        {
            SetContainerInvalidation();
            SetPanelSize();

            var context = DataContext as EditorContext;
            if (context == null)
                return;

            var container = context.Editor.Project.CurrentContainer;
            if (container != null)
            {
                container.Invalidate();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void HandleFileDialogs()
        {
            // open container
            this.openFileDialog1.FileOk += (sender, e) =>
            {
                var context = DataContext as EditorContext;
                if (context == null)
                    return;

                string path = openFileDialog1.FileName;
                int filterIndex = openFileDialog1.FilterIndex;
                context.Open(path);
                InvalidateContainer();
            };

            // save container
            this.saveFileDialog1.FileOk += (sender, e) =>
            {
                var context = DataContext as EditorContext;
                if (context == null)
                    return;

                string path = saveFileDialog1.FileName;
                int filterIndex = saveFileDialog1.FilterIndex;
                context.Save(path);
            };

            // export container
            this.saveFileDialog2.FileOk += (sender, e) =>
            {
                var context = DataContext as EditorContext;
                if (context == null)
                    return;

                string path = saveFileDialog2.FileName;
                int filterIndex = saveFileDialog2.FilterIndex;
                switch (filterIndex)
                {
                    case 1:
                        context.ExportAsPdf(path, context.Editor.Project);
                        Process.Start(path);
                        break;
                    case 2:
                        context.ExportAsDxf(path);
                        Process.Start(path);
                        break;
                    default:
                        break;
                }
            };
        }

        /// <summary>
        /// 
        /// </summary>
        private void SetPanelSize()
        {
            var context = DataContext as EditorContext;
            if (context == null)
                return;

            var container = context.Editor.Project.CurrentContainer;
            if (container == null)
                return;

            int width = (int)container.Width;
            int height = (int)container.Height;

            int x = (this.Width - width) / 2;
            int y = (((this.Height) - height) / 2) - (this.menuStrip1.Height / 3);

            _drawable.Location = new System.Drawing.Point(x, y);
            _drawable.Size = new System.Drawing.Size(width, height);
        }

        /// <summary>
        /// 
        /// </summary>
        private void SetContainerInvalidation()
        {
            var context = DataContext as EditorContext;
            if (context == null)
                return;

            var container = context.Editor.Project.CurrentContainer;
            if (container == null)
                return;

            foreach (var layer in container.Layers)
            {
                layer.InvalidateLayer += (s, e) => _drawable.Invalidate();
            }

            if (container.WorkingLayer != null)
            {
                container.WorkingLayer.InvalidateLayer += (s, e) => _drawable.Invalidate();
            }
            
            if (container.HelperLayer != null)
            {
                container.HelperLayer.InvalidateLayer += (s, e) => _drawable.Invalidate();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void UpdateToolMenu()
        {
            var context = DataContext as EditorContext;
            if (context == null)
                return;

            var tool = context.Editor.CurrentTool;

            noneToolStripMenuItem.Checked = tool == Tool.None;
            selectionToolStripMenuItem.Checked = tool == Tool.Selection;
            pointToolStripMenuItem.Checked = tool == Tool.Point;
            lineToolStripMenuItem.Checked = tool == Tool.Line;
            arcToolStripMenuItem.Checked = tool == Tool.Arc;
            bezierToolStripMenuItem.Checked = tool == Tool.Bezier;
            qBezierToolStripMenuItem.Checked = tool == Tool.QBezier;
            pathToolStripMenuItem.Checked = tool == Tool.Path;
            rectangleToolStripMenuItem.Checked = tool == Tool.Rectangle;
            ellipseToolStripMenuItem.Checked = tool == Tool.Ellipse;
            textToolStripMenuItem.Checked = tool == Tool.Text;
            imageToolStripMenuItem.Checked = tool == Tool.Image;
        }

        /// <summary>
        /// 
        /// </summary>
        private void UpdateOptionsMenu()
        {
            var context = DataContext as EditorContext;
            if (context == null)
                return;

            var options = context.Editor.Project.Options;

            defaultIsFilledToolStripMenuItem.Checked = options.DefaultIsFilled;
            snapToGridToolStripMenuItem.Checked = options.SnapToGrid;
            tryToConnectToolStripMenuItem.Checked = options.TryToConnect;
        }

        /// <summary>
        /// 
        /// </summary>
        private void HandleMenuShortcutKeys()
        {
            // File
            newToolStripMenuItem.Click += (sender, e) => OnNew();
            openToolStripMenuItem.Click += (sender, e) => OnOpen();
            saveAsToolStripMenuItem.Click += (sender, e) => OnSave();
            exportToolStripMenuItem.Click += (sender, e) => OnExport();
            exitToolStripMenuItem.Click += (sender, e) => Close();

            // Tool
            noneToolStripMenuItem.Click += (sender, e) => OnSetToolToNone();
            selectionToolStripMenuItem.Click += (sender, e) => OnSetToolToSelection();
            pointToolStripMenuItem.Click += (sender, e) => OnSetToolToPoint();
            lineToolStripMenuItem.Click += (sender, e) => OnSetToolToLine();
            arcToolStripMenuItem.Click += (sender, e) => OnSetToolToArc();
            bezierToolStripMenuItem.Click += (sender, e) => OnSetToolToBezier();
            qBezierToolStripMenuItem.Click += (sender, e) => OnSetToolToQBezier();
            pathToolStripMenuItem.Click += (sender, e) => OnSetToolToPath();
            rectangleToolStripMenuItem.Click += (sender, e) => OnSetToolToRectangle();
            ellipseToolStripMenuItem.Click += (sender, e) => OnSetToolToEllipse();
            textToolStripMenuItem.Click += (sender, e) => OnSetToolToText();
            imageToolStripMenuItem.Click += (sender, e) => OnSetToolToImage();

            // Options
            defaultIsFilledToolStripMenuItem.Click += (sender, e) => OnSetDefaultIsFilled();
            snapToGridToolStripMenuItem.Click += (sender, e) => OnSetSnapToGrid();
            tryToConnectToolStripMenuItem.Click += (sender, e) => OnSetTryToConnect();
        }

        /// <summary>
        /// 
        /// </summary>
        private void HandlePanelShorcutKeys()
        {
            _drawable.KeyDown += (sender, e) =>
            {
                if (e.Control || e.Alt || e.Shift)
                    return;

                var context = DataContext as EditorContext;
                if (context == null)
                    return;

                switch (e.KeyCode)
                {
                    case Keys.Delete:
                        context.Commands.DeleteCommand.Execute(null);
                        break;
                    case Keys.N:
                        OnSetToolToNone();
                        break;
                    case Keys.S:
                        OnSetToolToSelection();
                        break;
                    case Keys.P:
                        OnSetToolToPoint();
                        break;
                    case Keys.L:
                        OnSetToolToLine();
                        break;
                    case Keys.A:
                        OnSetToolToArc();
                        break;
                    case Keys.B:
                        OnSetToolToBezier();
                        break;
                    case Keys.Q:
                        OnSetToolToQBezier();
                        break;
                    case Keys.H:
                        OnSetToolToPath();
                        break;
                    case Keys.M:
                        OnSetToolToMove();
                        break;
                    case Keys.R:
                        OnSetToolToRectangle();
                        break;
                    case Keys.E:
                        OnSetToolToEllipse();
                        break;
                    case Keys.T:
                        OnSetToolToText();
                        break;
                    case Keys.I:
                        OnSetToolToImage();
                        break;
                    case Keys.F:
                        OnSetDefaultIsFilled();
                        break;
                    case Keys.G:
                        OnSetSnapToGrid();
                        break;
                    case Keys.C:
                        OnSetTryToConnect();
                        break;
                    case Keys.Z:
                        _drawable.ResetZoom();
                        break;
                    case Keys.X:
                        _drawable.AutoFit();
                        break;
                }
            };
        }

        /// <summary>
        /// 
        /// </summary>
        private void OnNew()
        {
            var context = DataContext as EditorContext;
            if (context == null)
                return;

            context.Commands.NewCommand.Execute(null);
            InvalidateContainer();
        }

        /// <summary>
        /// 
        /// </summary>
        private void OnOpen()
        {
            openFileDialog1.Filter = "Project (*.project)|*.project|All (*.*)|*.*";
            openFileDialog1.FilterIndex = 0;
            openFileDialog1.ShowDialog(this);
        }

        /// <summary>
        /// 
        /// </summary>
        private void OnSave()
        {
            var context = DataContext as EditorContext;
            if (context == null)
                return;

            saveFileDialog1.Filter = "Project (*.project)|*.project|All (*.*)|*.*";
            saveFileDialog1.FilterIndex = 0;
            saveFileDialog1.FileName = context.Editor.Project.Name;
            saveFileDialog1.ShowDialog(this);
        }

        /// <summary>
        /// 
        /// </summary>
        private void OnExport()
        {
            var context = DataContext as EditorContext;
            if (context == null)
                return;

            saveFileDialog2.Filter = "Pdf (*.pdf)|*.pdf|Dxf (*.dxf)|*.dxf|All (*.*)|*.*";
            saveFileDialog2.FilterIndex = 0;
            saveFileDialog2.FileName = context.Editor.Project.Name;
            saveFileDialog2.ShowDialog(this);
        }

        /// <summary>
        /// 
        /// </summary>
        private void OnSetToolToNone()
        {
            var context = DataContext as EditorContext;
            if (context == null)
                return;

            context.Commands.ToolNoneCommand.Execute(null);
            UpdateToolMenu();
        }

        /// <summary>
        /// 
        /// </summary>
        private void OnSetToolToSelection()
        {
            var context = DataContext as EditorContext;
            if (context == null)
                return;

            context.Commands.ToolSelectionCommand.Execute(null);
            UpdateToolMenu();
        }

        /// <summary>
        /// 
        /// </summary>
        private void OnSetToolToPoint()
        {
            var context = DataContext as EditorContext;
            if (context == null)
                return;

            context.Commands.ToolPointCommand.Execute(null);
            UpdateToolMenu();
        }

        /// <summary>
        /// 
        /// </summary>
        private void OnSetToolToLine()
        {
            var context = DataContext as EditorContext;
            if (context == null)
                return;

            context.Commands.ToolLineCommand.Execute(null);
            UpdateToolMenu();
        }

        /// <summary>
        /// 
        /// </summary>
        private void OnSetToolToArc()
        {
            var context = DataContext as EditorContext;
            if (context == null)
                return;

            context.Commands.ToolArcCommand.Execute(null);
            UpdateToolMenu();
        }

        /// <summary>
        /// 
        /// </summary>
        private void OnSetToolToBezier()
        {
            var context = DataContext as EditorContext;
            if (context == null)
                return;

            context.Commands.ToolBezierCommand.Execute(null);
            UpdateToolMenu();
        }

        /// <summary>
        /// 
        /// </summary>
        private void OnSetToolToQBezier()
        {
            var context = DataContext as EditorContext;
            if (context == null)
                return;

            context.Commands.ToolQBezierCommand.Execute(null);
            UpdateToolMenu();
        }

        /// <summary>
        /// 
        /// </summary>
        private void OnSetToolToPath()
        {
            var context = DataContext as EditorContext;
            if (context == null)
                return;

            context.Commands.ToolPathCommand.Execute(null);
            UpdateToolMenu();
        }
                
        /// <summary>
        /// 
        /// </summary>
        private void OnSetToolToMove()
        {
            var context = DataContext as EditorContext;
            if (context == null)
                return;

            context.Commands.ToolMoveCommand.Execute(null);
            UpdateToolMenu();
        }

        /// <summary>
        /// 
        /// </summary>
        private void OnSetToolToRectangle()
        {
            var context = DataContext as EditorContext;
            if (context == null)
                return;

            context.Commands.ToolRectangleCommand.Execute(null);
            UpdateToolMenu();
        }

        /// <summary>
        /// 
        /// </summary>
        private void OnSetToolToEllipse()
        {
            var context = DataContext as EditorContext;
            if (context == null)
                return;

            context.Commands.ToolEllipseCommand.Execute(null);
            UpdateToolMenu();
        }

        /// <summary>
        /// 
        /// </summary>
        private void OnSetToolToText()
        {
            var context = DataContext as EditorContext;
            if (context == null)
                return;

            context.Commands.ToolTextCommand.Execute(null);
            UpdateToolMenu();
        }

        /// <summary>
        /// 
        /// </summary>
        private void OnSetToolToImage()
        {
            var context = DataContext as EditorContext;
            if (context == null)
                return;

            context.Commands.ToolImageCommand.Execute(null);
            UpdateToolMenu();
        }

        /// <summary>
        /// 
        /// </summary>
        private void OnSetDefaultIsFilled()
        {
            var context = DataContext as EditorContext;
            if (context == null)
                return;

            context.Commands.DefaultIsFilledCommand.Execute(null);
            UpdateOptionsMenu();
        }

        /// <summary>
        /// 
        /// </summary>
        private void OnSetSnapToGrid()
        {
            var context = DataContext as EditorContext;
            if (context == null)
                return;

            context.Commands.SnapToGridCommand.Execute(null);
            UpdateOptionsMenu();
        }

        /// <summary>
        /// 
        /// </summary>
        private void OnSetTryToConnect()
        {
            var context = DataContext as EditorContext;
            if (context == null)
                return;

            context.Commands.TryToConnectCommand.Execute(null);
            UpdateOptionsMenu();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private async Task<string> GetImageKey()
        {
            var context = DataContext as EditorContext;
            if (context == null)
                return null;

            openFileDialog2.Filter = "All (*.*)|*.*";
            openFileDialog2.FilterIndex = 0;
            var result = openFileDialog2.ShowDialog(this);
            if (result == DialogResult.OK)
            {
                var path = openFileDialog2.FileName;
                var bytes = System.IO.File.ReadAllBytes(path);
                var key = context.Editor.Project.AddImageFromFile(path, bytes);
                return await Task.Run(() => key);
            }
            return null;
        }

        #region [Testing]
        public void _testrendering()
        {
            List<double> x = new List<double>(),
                         y = new List<double>(),
                         z = new List<double>();

            x.Add(5.31E-04F);
            x.Add(25.581153F);
            x.Add(52.471346F);
            x.Add(79.532937F);
            x.Add(106.8682F);
            x.Add(146.13019F);
            x.Add(173.5059F);
            x.Add(206.2034F);
            x.Add(236.15716F);
            x.Add(266.12751F);
            x.Add(296.10766F);

            y.Add(0F);
            y.Add(23.073F);
            y.Add(47.316F);
            y.Add(70.958F);
            y.Add(95.561F);
            y.Add(129.898F);
            y.Add(154.082F);
            y.Add(183.04F);
            y.Add(209.667F);
            y.Add(236.482F);
            y.Add(262.677F);

            var context = DataContext as EditorContext;
            var container = context.Editor.Project.CurrentContainer;
            //Find the radius of the control by dividing the width by 2 
            double radius = 189.12;

            //Find the origin of the circle by dividing the width and height of the control 
            XPoint origin = XPoint.Create(container.Width / 2, container.Height / 2);

            if (context == null)
                Debug.Write("---Failed--- Context is null");
            var factory = new Factory(context);
            var line1style = ShapeStyle.Create("line1", 255, 0, 0, 00, 255, 0, 0, 0, 2.0, TextStyle.Create("linet1", "Swis721 Ex BT", "SWZ721BE.TTF", 26.0, (Test2d.FontStyle.Bold | Test2d.FontStyle.Bold)), null, null, null, Test2d.LineCap.Square);
            var line2style = ShapeStyle.Create("line2", 255, 0, 0, 00, 255, 0, 0, 0, 20.0, TextStyle.Create("linet2", "Swis721 Ex BT", "SWZ721BE.TTF", 14.0, (Test2d.FontStyle.Bold | Test2d.FontStyle.Italic)), null, null, null, Test2d.LineCap.Square);
            var line3style = ShapeStyle.Create("line3", 255, 0, 0, 00, 255, 0, 0, 0, 10.0, TextStyle.Create("linet3", "Swis721 Ex BT", "SWZ721BE.TTF", 14.0, (Test2d.FontStyle.Bold | Test2d.FontStyle.Italic)), null, null, null, Test2d.LineCap.Square);
            var line4style = ShapeStyle.Create("line4", 255, 0, 0, 00, 255, 0, 0, 0, 1.0, TextStyle.Create("linet4", "Swis721 BlkCn BT", "SWZ721BE.TTF", 26.0, (Test2d.FontStyle.Bold)), null, null, null, Test2d.LineCap.Square);

            //Draw the Major segments for the clock 
            //Why pan and zoom mark is all weird...
            foreach (float note in y)
            {
                var tick = factory.Line(PointOnCircle(radius - 1, note, origin), PointOnCircle(radius - 34.944, note, origin));
                tick.Style = line1style;
                var tick2 = factory.Line(PointOnCircle(radius - 4.005, note, origin), PointOnCircle(radius - 6.32, note, origin));
                tick2.Style = line2style;
                var tick3 = factory.Line(PointOnCircle(radius - 4.005, note, origin), PointOnCircle(radius - 20, note, origin));
                tick3.Style = line3style;
                var tick4 = factory.Text(PointOnCircle(radius - 1, note, origin), PointOnCircle(radius - 170, note, origin), note.ToString("#"), false);
                tick4.Style = line4style;
            }

            //Draw the minor segments for the control 
            for (float i = 0f; i != 360f; i += 10f)
            {
                var tick5 = factory.Line(PointOnCircle(radius - 1, i, origin), PointOnCircle(radius - 19.584, i, origin));
                tick5.Style = line1style;
            }

        }

        /// <summary> 
        /// Find the point on the circumference of a circle 
        /// </summary> 
        /// <param name="radius">The radius of the circle</param> 
        /// <param name="angleInDegrees">The angle of the point to origin</param> 
        /// <param name="origin">The origin of the circle</param> 
        /// <returns>Return the point</returns> 
        private XPoint PointOnCircle(double radius, double angleInDegrees, XPoint origin)
        {
            //Find the x and y using the parametric equation for a circle 
            double x = (radius * Math.Cos((angleInDegrees - 90f) * Math.PI / 180F)) + origin.X;
            double y = (radius * Math.Sin((angleInDegrees - 90f) * Math.PI / 180F)) + origin.Y;

            /*Note : The "- 90f" is only for the proper rotation of the clock. 
             * It is not part of the parament equation for a circle*/

            //Return the point 
            return XPoint.Create(x, y);
        }

        #endregion

        private void renderTestToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _testrendering();
        }

    }
}
