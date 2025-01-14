﻿using System;
using System.Collections.Generic;
using Paroxe.PdfRenderer.Internal;
using UnityEngine;

namespace Paroxe.PdfRenderer
{
#if !UNITY_WEBGL
    /// <summary>
    /// This class allow to access the text of a page.
    /// </summary>
    public sealed class PDFTextPage : IDisposable, IEquatable<PDFTextPage>, ICoordinatedNativeDisposable
    {
	    private IntPtr m_NativePointer;
        private PDFPage m_Page;

        public PDFTextPage(PDFPage page)
        {
            if (page == null)
                throw new ArgumentNullException("page");

            m_Page = page;

            PDFLibrary.Instance.DisposeCoordinator.EnsureNativeLibraryIsInitialized();

            lock (PDFLibrary.nativeLock)
            {
	            m_NativePointer = NativeMethods.FPDFText_LoadPage(((ICoordinatedNativeDisposable)m_Page).NativePointer);

                if (m_NativePointer != IntPtr.Zero)
					PDFLibrary.Instance.DisposeCoordinator.AddReference(this);
            }
        }

        ~PDFTextPage()
        {
	        Close();
        }

        public void Dispose()
        {
	        Close();

            GC.SuppressFinalize(this);
        }

        private void Close()
        {
	        if (m_NativePointer == IntPtr.Zero)
		        return;

	        PDFLibrary.Instance.DisposeCoordinator.RemoveReference(this);

	        m_NativePointer = IntPtr.Zero;
        }

        public IntPtr NativePointer
        {
            get { return m_NativePointer; }
        }

        /// <summary>
        /// Return a refence to the page.
        /// </summary>
        public PDFPage Page
        {
            get { return m_Page; }
        }

        /// <summary>
        /// Return the page index of the text.
        /// </summary>
        public int PageIndex
        {
            get { return m_Page.PageIndex; }
        }

        public PDFPageWebLinks GetPageWebLinks()
        {
            return new PDFPageWebLinks(this);
        }

        /// <summary>
        /// Return the number of character in the page.
        /// </summary>
        /// <returns></returns>
        public int CountChars()
        {
            return NativeMethods.FPDFText_CountChars(m_NativePointer);
        }

        /// <summary>
        /// Count number of rectangular areas occupied by a segment of texts.
        /// </summary>
        /// <param name="startIndex"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public int CountRects(int startIndex, int count)
        {
            return NativeMethods.FPDFText_CountRects(m_NativePointer, startIndex, count);
        }

        /// <summary>
        /// Extract text within a rectangular boundary on the page.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="top"></param>
        /// <param name="right"></param>
        /// <param name="bottom"></param>
        /// <param name="charCount"></param>
        /// <returns></returns>
        public string GetBoundedText(float left, float top, float right, float bottom, int charCount)
        {
            byte[] textBuffer = new byte[(charCount + 1) * 2];
            NativeMethods.FPDFText_GetBoundedText(m_NativePointer, left, top, right, bottom, textBuffer, textBuffer.Length);

            return PDFLibrary.Encoding.GetString(textBuffer);
        }

        /// <summary>
        /// Get bounding box of a particular character.
        /// </summary>
        /// <param name="charIndex"></param>
        /// <returns></returns>
        public Rect GetCharBox(int charIndex)
        {
            double left;
            double right;
            double bottom;
            double top;

            NativeMethods.FPDFText_GetCharBox(m_NativePointer, charIndex, out left, out right, out bottom, out top);

            return new Rect((float)left, (float)top, Mathf.Abs((float)right - (float)left),
                Mathf.Abs((float)bottom - (float)top));
        }

        /// <summary>
        /// Get the index of a character at or nearby a certain position on the page.
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="tolerance"></param>
        /// <returns></returns>
        public int GetCharIndexAtPos(Vector2 pos, Vector2 tolerance)
        {
	        return NativeMethods.FPDFText_GetCharIndexAtPos(m_NativePointer, pos.x, pos.y, tolerance.x, tolerance.y);
        }

        /// <summary>
        /// Get the font size of a particular character.
        /// </summary>
        /// <param name="charIndex"></param>
        /// <returns></returns>
        public double GetFontSize(int charIndex)
        {
            return NativeMethods.FPDFText_GetFontSize(m_NativePointer, charIndex);
        }

        /// <summary>
        /// Get a rectangular area from the result generated by CountRects
        /// </summary>
        /// <param name="rectIndex"></param>
        /// <returns></returns>
        public Rect GetRect(int rectIndex)
        {
            double left;
            double right;
            double bottom;
            double top;

            NativeMethods.FPDFText_GetRect(m_NativePointer, rectIndex, out left, out top, out right, out bottom);

            return new Rect((float)left, (float)top, Mathf.Abs((float)right - (float)left), Mathf.Abs((float)bottom - (float)top));
        }

        /// <summary>
        /// Extract text string from the page.
        /// </summary>
        /// <param name="startIndex"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public string GetText(int startIndex, int count)
        {
            byte[] textBuffer = new byte[(count + 1) * 2];
            NativeMethods.FPDFText_GetText(m_NativePointer, startIndex, count, textBuffer);

            return PDFLibrary.Encoding.GetString(textBuffer);
        }

        /// <summary>
        /// Get a character in the page.
        /// </summary>
        /// <param name="charIndex"></param>
        /// <returns></returns>
        public string GetChar(int charIndex)
        {
            return char.ConvertFromUtf32((int) NativeMethods.FPDFText_GetUnicode(m_NativePointer, charIndex));
        }

        public IList<PDFSearchResult> Search(string findWhat,
            PDFSearchHandle.MatchOption flags = PDFSearchHandle.MatchOption.NONE, int startIndex = 0)
        {
            if (string.IsNullOrEmpty(findWhat.Trim()))
                return new List<PDFSearchResult>();

            return Search(PDFLibrary.Encoding.GetBytes(findWhat.Trim() + "\0"), flags, startIndex);
        }

        public IList<PDFSearchResult> Search(byte[] findWhatUnicode,
            PDFSearchHandle.MatchOption flags = PDFSearchHandle.MatchOption.NONE, int startIndex = 0)
        {
            List<PDFSearchResult> searchResults = new List<PDFSearchResult>();

            if (findWhatUnicode == null)
                return searchResults;

            using (PDFSearchHandle searchHandle = new PDFSearchHandle(this, findWhatUnicode, startIndex, flags))
            {
                foreach (PDFSearchResult result in searchHandle.EnumerateSearchResults())
                {
                    searchResults.Add(result);
                }
            }

            return searchResults;
        }

        public bool Equals(PDFTextPage other)
        {
	        if (other == null)
		        return false;

            return m_NativePointer != IntPtr.Zero && m_NativePointer == other.m_NativePointer;
        }

        IntPtr ICoordinatedNativeDisposable.NativePointer
        {
	        get { return NativePointer; }
        }

        ICoordinatedNativeDisposable ICoordinatedNativeDisposable.NativeParent
        {
	        get { return m_Page; }
        }

        Action<IntPtr> ICoordinatedNativeDisposable.GetDisposeMethod()
        {
	        return NativeMethods.FPDFText_ClosePage;
        }
    }
#endif
}