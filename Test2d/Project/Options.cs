﻿// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;

namespace Test2d
{
    /// <summary>
    /// 
    /// </summary>
    public class Options : ObservableObject
    {
        private bool _snapToGrid = true;
        private double _snapX = 15.0;
        private double _snapY = 15.0;
        private double _hitTreshold = 7.0;
        private MoveMode _moveMode = MoveMode.Point;
        private bool _defaultIsFilled = false;
        private bool _tryToConnect = false;
        private int _cycleResolution;
        private BaseShape _pointShape;
        private ShapeStyle _selectionStyle;
        private ShapeStyle _helperStyle;

        /// <summary>
        /// Gets or sets how grid snapping is handled. 
        /// </summary>
        public bool SnapToGrid
        {
            get { return _snapToGrid; }
            set { Update(ref _snapToGrid, value); }
        }

        /// <summary>
        /// Gets or sets how much grid X axis is snapped.
        /// </summary>
        public double SnapX
        {
            get { return _snapX; }
            set { Update(ref _snapX, value); }
        }

        /// <summary>
        /// Gets or sets how much grid Y axis is snapped.
        /// </summary>
        public double SnapY
        {
            get { return _snapY; }
            set { Update(ref _snapY, value); }
        }

        /// <summary>
        /// Gets or sets hit test treshold radius.
        /// </summary>
        public double HitTreshold
        {
            get { return _hitTreshold; }
            set { Update(ref _hitTreshold, value); }
        }

        /// <summary>
        /// Gets or sets how selected shapes are moved.
        /// </summary>
        public MoveMode MoveMode
        {
            get { return _moveMode; }
            set { Update(ref _moveMode, value); }
        }

        /// <summary>
        /// Gets or sets value indicating whether shape is filled during creation.
        /// </summary>
        public bool DefaultIsFilled
        {
            get { return _defaultIsFilled; }
            set { Update(ref _defaultIsFilled, value); }
        }

        /// <summary>
        /// Gets or sets how point connection is handled.
        /// </summary>
        public bool TryToConnect
        {
            get { return _tryToConnect; }
            set { Update(ref _tryToConnect, value); }
        }

        /// <summary>
        /// Gets or sets simulation clock cycle resolution in milliseconds.
        /// </summary>
        public int CycleResolution
        {
            get { return _cycleResolution; }
            set { Update(ref _cycleResolution, value); }
        }

        /// <summary>
        /// Gets or sets shape used to draw points.
        /// </summary>
        public BaseShape PointShape
        {
            get { return _pointShape; }
            set { Update(ref _pointShape, value); }
        }
        
        /// <summary>
        /// Gets or sets selection rectangle style.
        /// </summary>
        public ShapeStyle SelectionStyle
        {
            get { return _selectionStyle; }
            set { Update(ref _selectionStyle, value); }
        }
        
        /// <summary>
        /// Gets or sets editor helper shapes style.
        /// </summary>
        public ShapeStyle HelperStyle
        {
            get { return _helperStyle; }
            set { Update(ref _helperStyle, value); }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static Options Create()
        {
            var options = new Options()
            {
                SnapToGrid = true,
                SnapX = 15.0,
                SnapY = 15.0,
                HitTreshold = 7.0,
                MoveMode = MoveMode.Point,
                DefaultIsFilled = false,
                TryToConnect = false,
                CycleResolution = 100
            };

            options.SelectionStyle =
                ShapeStyle.Create(
                    "Selection",
                    0x7F, 0x33, 0x33, 0xFF,
                    0x4F, 0x33, 0x33, 0xFF,
                    1.0);

            options.HelperStyle =
                ShapeStyle.Create(
                    "Helper",
                    0xFF, 0xFF, 0x00, 0x00,
                    0xFF, 0xFF, 0x00, 0x00,
                    1.0);

            var pss = 
                ShapeStyle.Create(
                    "PointShape",
                    0xFF, 0xFF, 0x00, 0x00,
                    0xFF, 0xFF, 0x00, 0x00, 
                    2.0);

            options.PointShape = CrossPointShape(pss);
  
            return options;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pss"></param>
        /// <returns></returns>
        public static BaseShape EllipsePointShape(ShapeStyle pss)
        {
            return XEllipse.Create(-4, -4, 4, 4, pss, null, false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pss"></param>
        /// <returns></returns>
        public static BaseShape FilledEllipsePointShape(ShapeStyle pss)
        {
            return XEllipse.Create(-3, -3, 3, 3, pss, null, true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pss"></param>
        /// <returns></returns>
        public static BaseShape RectanglePointShape(ShapeStyle pss)
        {
            return XRectangle.Create(-4, -4, 4, 4, pss, null, false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pss"></param>
        /// <returns></returns>
        public static BaseShape FilledRectanglePointShape(ShapeStyle pss)
        {
            return XRectangle.Create(-3, -3, 3, 3, pss, null, true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pss"></param>
        /// <returns></returns>
        public static BaseShape CrossPointShape(ShapeStyle pss)
        {
            var g = XGroup.Create("PointShape");

            var builder = g.Shapes.ToBuilder();
            builder.Add(XLine.Create(-4, 0, 4, 0, pss, null));
            builder.Add(XLine.Create(0, -4, 0, 4, pss, null));
            g.Shapes = builder.ToImmutable();

            return g;
        }
    }
}
