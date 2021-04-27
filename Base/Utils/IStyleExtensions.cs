
using UnityEngine;
using UnityEngine.UIElements;

public static class IStyleExtensions
{
    public static void SetSlice(this IStyle style, int pixels)
    {
        SetSlice(style, pixels, pixels, pixels, pixels);
    }
    
    public static void SetSlice(this IStyle style, int top, int left, int right, int bottom)
    {
        style.unitySliceTop = top;
        style.unitySliceLeft = left;
        style.unitySliceRight = right;
        style.unitySliceBottom = bottom;
    }
    
    public static void SetBorderColor(this IStyle style, Color color)
    {
        SetBorderColor(style, color, color, color, color);
    }
    
    public static void SetBorderColor(this IStyle style, Color? top = default, Color? left = default, Color? right = default, Color? bottom = default)
    {
        style.borderTopColor = top ?? style.borderTopColor;
        style.borderLeftColor = left ?? style.borderTopColor;
        style.borderRightColor = right ?? style.borderTopColor;
        style.borderBottomColor = bottom ?? style.borderTopColor;
    }
    
    public static void SetBorderWidth(this IStyle style, float width)
    {
        SetBorderWidth(style, width, width, width, width);
    }

    public static void SetBorderWidth(this IStyle style, float top, float left, float right, float bottom)
    {
        style.borderTopWidth = top;
        style.borderLeftWidth = left;
        style.borderRightWidth = right;
        style.borderBottomWidth = bottom;
    }

    public static void SetBorderRadius(this IStyle style, float radius)
    {
        SetBorderRadius(style, radius, radius, radius, radius);
    }

    public static void SetBorderRadius(this IStyle style, float top, float left, float right, float bottom)
    {
        style.borderTopLeftRadius = top;
        style.borderTopRightRadius = left;
        style.borderBottomLeftRadius = right;
        style.borderBottomRightRadius = bottom;
    }
    
    public static void SetMargin(this IStyle style, float length)
    {
        SetMargin(style, length, length, length, length);
    }

    public static void SetMargin(this IStyle style, float top, float left, float right, float bottom)
    {
        style.marginTop = top;
        style.marginLeft = left;
        style.marginRight = right;
        style.marginBottom = bottom;
    }
    
    public static void SetPadding(this IStyle style, float length)
    {
        SetPadding(style, length, length, length, length);
    }
    
    public static void SetPadding(this IStyle style, float top, float left, float right, float bottom)
    {
        style.paddingTop = top;
        style.paddingLeft = left;
        style.paddingRight = right;
        style.paddingBottom = bottom;
    }
}