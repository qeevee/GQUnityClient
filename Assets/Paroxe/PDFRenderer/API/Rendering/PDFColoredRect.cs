﻿using UnityEngine;

namespace Paroxe.PdfRenderer
{
    /// <summary>
    /// Represent a colored rect within a page. This class is used mainly for search results highlighting.
    /// </summary>
    public struct PDFColoredRect
    {
        public Rect PageRect;
        public Color Color;

        public PDFColoredRect(Rect pageRect, Color color)
        {
            PageRect = pageRect;
            Color = color;
        }
    }
}