﻿void SetStyle(IEnumerable<BaseShape> shapes, ShapeStyle style)
{
    if (shapes == null || style == null)
        return;
    foreach (var shape in shapes)
    {
        shape.Style = style;
        if (shape is XGroup)
            SetStyle((shape as XGroup).Shapes, style);
    }
}
var layers = Context?.Editor?.Container?.Layers;
var style = Context?.Editor?.Container?.CurrentStyle;
foreach (var layer in layers)
{
    SetStyle(layer?.Shapes, style);
    layer?.Invalidate();
}