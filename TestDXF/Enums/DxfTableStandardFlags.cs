﻿// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;

namespace Dxf
{
    // Group code: 70
    public enum DxfTableStandardFlags : int
    {
        Default = 0,
        Xref = 16,
        XrefSuccess = 32,
        Referenced = 64
    }
}