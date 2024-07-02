﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Globalization;
using Avalonia.Media;
using ReactiveUI;
using ContentAssembler;
using QuestPDF.Infrastructure;
using ExtentionsAndAuxiliary;
using System.Reflection;
using Avalonia.Controls.Shapes;
using static System.Net.Mime.MediaTypeNames;

namespace Lister.ViewModels
{
    public class TextLineViewModel : BadgeMember
    {
        internal static readonly double _additionOnEnd = 4;
        private TextualAtom _dataSource;

        private HorizontalAlignment al;
        internal HorizontalAlignment Alignment
        {
            get { return al; }
            private set
            {
                this.RaiseAndSetIfChanged (ref al, value, nameof (Alignment));
            }
        }

        private double fs;
        internal double FontSize
        {
            get { return fs; }
            private set
            {
                this.RaiseAndSetIfChanged (ref fs, value, nameof (FontSize));
            }
        }

        private FontFamily ff;
        internal FontFamily FontFamily
        {
            get { return ff; }
            private set
            {
                this.RaiseAndSetIfChanged (ref ff, value, nameof (FontFamily));
            }
        }

        private Avalonia.Media.FontWeight fW;
        internal Avalonia.Media.FontWeight FontWeight
        {
            get { return fW; }
            private set
            {
                this.RaiseAndSetIfChanged (ref fW, value, nameof (FontWeight));
            }
        }

        private string cn;
        internal string Content
        {
            get { return cn; }
            private set
            {
                this.RaiseAndSetIfChanged (ref cn, value, nameof (Content));
            }
        }

        private bool ish;
        internal bool IsSplitable
        {
            get { return ish; }
            private set
            {
                this.RaiseAndSetIfChanged (ref ish, value, nameof (IsSplitable));
            }
        }

        private double uW;
        internal double UsefullWidth
        {
            get { return uW; }
            private set
            {
                this.RaiseAndSetIfChanged (ref uW, value, nameof (UsefullWidth));
            }
        }

        internal bool isBorderViolent = false;
        internal bool isOverLayViolent = false;


        public TextLineViewModel ( TextualAtom text )
        {
            _dataSource = text;
            FontSize = text.FontSize;
            FontFamily = new FontFamily (text.FontFamily);
            FontWeight = Avalonia.Media.FontWeight.Normal;
            Content = text.Content;
            IsSplitable = text.IsSplitable;

            Typeface face = new Typeface (FontFamily, FontStyle.Normal, FontWeight);
            FormattedText formatted = new FormattedText (Content, CultureInfo.CurrentCulture
                                                                , FlowDirection.LeftToRight, face, FontSize, null);
            UsefullWidth = formatted.Width + 4;

            SetYourself (text.Width, text.Height, text.TopOffset, text.LeftOffset);
            SetAlignment (text.Alignment);
        }


        private TextLineViewModel ( TextLineViewModel source )
        {
            _dataSource = source._dataSource;
            FontSize = source.FontSize;
            FontFamily = new FontFamily (source._dataSource.FontFamily);
            FontWeight = source.FontWeight;
            Content = source.Content;
            IsSplitable = source.IsSplitable;
            UsefullWidth = source.UsefullWidth;

            SetYourself (source.Width, source.Height, source.TopOffset, source.LeftOffset);
            //SetAlignment (source._dataSource.Alignment);
        }


        internal TextLineViewModel Clone ()
        {
            TextLineViewModel clone = new TextLineViewModel (this);
            return clone;
        }


        internal void ZoomOn ( double coefficient )
        {
            FontSize *= coefficient;
            UsefullWidth *= coefficient;
            base.ZoomOn (coefficient );
        }


        internal void ZoomOut ( double coefficient )
        {
            FontSize /= coefficient;
            UsefullWidth /= coefficient;
            base.ZoomOut (coefficient);
        }


        internal void Increase ( double additable )
        {
            double oldFontSize = FontSize;
            FontSize += additable;
            Typeface face = new Typeface (FontFamily, FontStyle.Normal, Avalonia.Media.FontWeight.Normal);
            FormattedText formatted = new FormattedText (Content, CultureInfo.CurrentCulture
                                                                , FlowDirection.LeftToRight, face, FontSize, null);
            UsefullWidth = formatted.WidthIncludingTrailingWhitespace + (_additionOnEnd * BadgeEditorViewModel._scale);
            double proportion = FontSize / oldFontSize;
            Height *= proportion;
        }


        internal void Reduce ( double subtractable )
        {
            double oldFontSize = FontSize;
            FontSize -= subtractable;
            Typeface face = new Typeface (FontFamily, FontStyle.Normal, Avalonia.Media.FontWeight.Normal);
            FormattedText formatted = new FormattedText (Content, CultureInfo.CurrentCulture
                                                                , FlowDirection.LeftToRight, face, FontSize, null);
            UsefullWidth = formatted.WidthIncludingTrailingWhitespace + (_additionOnEnd * BadgeEditorViewModel._scale);
            double proportion = FontSize / oldFontSize;
            Height *= proportion;
        }


        private void SetAlignment ( string alignmentName ) 
        {
            if ( Width <= UsefullWidth )
            {
                return;
            }

            if ( alignmentName == "Right" )
            {
                LeftOffset += ( Width - UsefullWidth );
            }
            if ( alignmentName == "Center" )
            {
                LeftOffset += ( Width - UsefullWidth ) / 2;
            }
        }


        internal List <TextLineViewModel> SplitYourself ( List<string> splittedContents, double scale, double layoutWidth )
        {
            List <TextLineViewModel> result = new List <TextLineViewModel>();
            double previousLeftOffset = LeftOffset;

            foreach ( string content   in   splittedContents )
            {
                TextLineViewModel newLine = new TextLineViewModel (this._dataSource);
                newLine.ZoomOn (scale);
                newLine.ReplaceContent (content);
                newLine.LeftOffset = previousLeftOffset;

                if ( newLine.LeftOffset >= layoutWidth - 2 ) 
                {
                    newLine.LeftOffset = LeftOffset;
                }
                
                newLine.TopOffset = TopOffset;
                previousLeftOffset += newLine.Width;
                result.Add (newLine);
            }

            return result;
        }


        private void ReplaceContent ( string content )
        {
            if ( content == null ) 
            {
                content = string.Empty;
            }

            Typeface face = new Typeface (new FontFamily ("arial"), FontStyle.Normal, Avalonia.Media.FontWeight.Normal);
            FormattedText formatted = new FormattedText (content, CultureInfo.CurrentCulture
                                                                , FlowDirection.LeftToRight, face, FontSize, null);
            Content = content;
            Width = formatted.WidthIncludingTrailingWhitespace;
            UsefullWidth = formatted.Width + 4;
        }
    }




}
