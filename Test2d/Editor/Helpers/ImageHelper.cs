﻿// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test2d
{
    /// <summary>
    /// 
    /// </summary>
    public class ImageHelper : Helper
    {
        private Editor _editor;
        private State _currentState = State.None;
        private XImage _shape;

        private ShapeStyle _style;
        private double _pointEllipseRadius = 3.0;
        private XEllipse _ellipseTopLeft;
        private XEllipse _ellipseBottomRight;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="editor"></param>
        public ImageHelper(Editor editor)
        {
            _editor = editor;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="image"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void TryToConnectTopLeft(XImage image, double x, double y)
        {
            var result = ShapeBounds.HitTest(_editor.Project.CurrentContainer, new Vector2(x, y), _editor.Project.Options.HitTreshold);
            if (result != null && result is XPoint)
            {
                image.TopLeft = result as XPoint;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="image"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void TryToConnectBottomRight(XImage image, double x, double y)
        {
            var result = ShapeBounds.HitTest(_editor.Project.CurrentContainer, new Vector2(x, y), _editor.Project.Options.HitTreshold);
            if (result != null && result is XPoint)
            {
                image.BottomRight = result as XPoint;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public override void LeftDown(double x, double y)
        {
            double sx = _editor.Project.Options.SnapToGrid ? Editor.Snap(x, _editor.Project.Options.SnapX) : x;
            double sy = _editor.Project.Options.SnapToGrid ? Editor.Snap(y, _editor.Project.Options.SnapY) : y;
            switch (_currentState)
            {
                case State.None:
                    {
                        var path = _editor.GetImagePath();
                        if (string.IsNullOrEmpty(path))
                            return;

                        var uri = new Uri(path);

                        _shape = XImage.Create(
                            sx, sy,
                            _editor.Project.CurrentStyleGroup.CurrentStyle,
                            _editor.Project.Options.PointShape,
                            uri,
                            _editor.Project.Options.DefaultIsFilled);
                        if (_editor.Project.Options.TryToConnect)
                        {
                            TryToConnectTopLeft(_shape as XImage, sx, sy);
                        }
                        _editor.Project.CurrentContainer.WorkingLayer.Shapes.Add(_shape);
                        _editor.Project.CurrentContainer.WorkingLayer.Invalidate();
                        ToStateOne();
                        Move(_shape);
                        _editor.Project.CurrentContainer.HelperLayer.Invalidate();
                        _currentState = State.One;
                    }
                    break;
                case State.One:
                    {
                        var image = _shape as XImage;
                        if (image != null)
                        {
                            image.BottomRight.X = sx;
                            image.BottomRight.Y = sy;
                            if (_editor.Project.Options.TryToConnect)
                            {
                                TryToConnectBottomRight(_shape as XImage, sx, sy);
                            }
                            _editor.Project.CurrentContainer.WorkingLayer.Shapes.Remove(_shape);
                            Remove();
                            Finalize(_shape);
                            _editor.History.Snapshot(_editor.Project);
                            _editor.Project.CurrentContainer.CurrentLayer.Shapes.Add(_shape);
                            //_editor.Project.CurrentContainer.Invalidate();
                            _currentState = State.None;
                        }
                    }
                    break;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public override void LeftUp(double x, double y)
        {
            double sx = _editor.Project.Options.SnapToGrid ? Editor.Snap(x, _editor.Project.Options.SnapX) : x;
            double sy = _editor.Project.Options.SnapToGrid ? Editor.Snap(y, _editor.Project.Options.SnapY) : y;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public override void RightDown(double x, double y)
        {
            double sx = _editor.Project.Options.SnapToGrid ? Editor.Snap(x, _editor.Project.Options.SnapX) : x;
            double sy = _editor.Project.Options.SnapToGrid ? Editor.Snap(y, _editor.Project.Options.SnapY) : y;
            switch (_currentState)
            {
                case State.None:
                    break;
                case State.One:
                    {
                        _editor.Project.CurrentContainer.WorkingLayer.Shapes.Remove(_shape);
                        _editor.Project.CurrentContainer.WorkingLayer.Invalidate();
                        Remove();
                        _editor.Project.CurrentContainer.HelperLayer.Invalidate();
                        _currentState = State.None;
                    }
                    break;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public override void RightUp(double x, double y)
        {
            double sx = _editor.Project.Options.SnapToGrid ? Editor.Snap(x, _editor.Project.Options.SnapX) : x;
            double sy = _editor.Project.Options.SnapToGrid ? Editor.Snap(y, _editor.Project.Options.SnapY) : y;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public override void Move(double x, double y)
        {
            double sx = _editor.Project.Options.SnapToGrid ? Editor.Snap(x, _editor.Project.Options.SnapX) : x;
            double sy = _editor.Project.Options.SnapToGrid ? Editor.Snap(y, _editor.Project.Options.SnapY) : y;
            switch (_currentState)
            {
                case State.None:
                    break;
                case State.One:
                    {
                        var image = _shape as XImage;
                        if (image != null)
                        {
                            image.BottomRight.X = sx;
                            image.BottomRight.Y = sy;
                            _editor.Project.CurrentContainer.WorkingLayer.Invalidate();
                            Move(_shape);
                            _editor.Project.CurrentContainer.HelperLayer.Invalidate();
                        }
                    }
                    break;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public override void ToStateOne()
        {
            _style = _editor.Project.Options.HelperStyle;
            _ellipseTopLeft = XEllipse.Create(0, 0, _style, null, true);
            _editor.Project.CurrentContainer.HelperLayer.Shapes.Add(_ellipseTopLeft);
            _ellipseBottomRight = XEllipse.Create(0, 0, _style, null, true);
            _editor.Project.CurrentContainer.HelperLayer.Shapes.Add(_ellipseBottomRight);
        }
        
        /// <summary>
        /// 
        /// </summary>
        public override void ToStateTwo()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        public override void ToStateThree()
        {
        }
        
        /// <summary>
        /// 
        /// </summary>
        public override void ToStateFour()
        {
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="shape"></param>
        public override void Move(BaseShape shape)
        {
            if (_ellipseTopLeft != null)
            {
                _ellipseTopLeft.TopLeft.X = _shape.TopLeft.X - _pointEllipseRadius;
                _ellipseTopLeft.TopLeft.Y = _shape.TopLeft.Y - _pointEllipseRadius;
                _ellipseTopLeft.BottomRight.X = _shape.TopLeft.X + _pointEllipseRadius;
                _ellipseTopLeft.BottomRight.Y = _shape.TopLeft.Y + _pointEllipseRadius;
            }

            if (_ellipseBottomRight != null)
            {
                _ellipseBottomRight.TopLeft.X = _shape.BottomRight.X - _pointEllipseRadius;
                _ellipseBottomRight.TopLeft.Y = _shape.BottomRight.Y - _pointEllipseRadius;
                _ellipseBottomRight.BottomRight.X = _shape.BottomRight.X + _pointEllipseRadius;
                _ellipseBottomRight.BottomRight.Y = _shape.BottomRight.Y + _pointEllipseRadius;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="shape"></param>
        public override void Finalize(BaseShape shape)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Remove()
        {
            if (_ellipseTopLeft != null)
            {
                _editor.Project.CurrentContainer.HelperLayer.Shapes.Remove(_ellipseTopLeft);
                _ellipseTopLeft = null;
            }

            if (_ellipseBottomRight != null)
            {
                _editor.Project.CurrentContainer.HelperLayer.Shapes.Remove(_ellipseBottomRight);
                _ellipseBottomRight = null;
            }
        }
    }
}