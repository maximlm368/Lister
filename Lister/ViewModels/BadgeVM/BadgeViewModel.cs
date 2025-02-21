using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using ContentAssembler;
using ReactiveUI;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using static QuestPDF.Helpers.Colors;
using Lister.Extentions;
using System.Collections.ObjectModel;
using Avalonia.Media;
using Avalonia.Layout;
using System.Globalization;
using Avalonia.Controls;
using Avalonia.Interactivity;
using ExtentionsAndAuxiliary;
using Avalonia.Controls.Shapes;
using System;
using DataGateway;
using Microsoft.Extensions.DependencyInjection;
using QuestPDF.Fluent;
using System.Threading.Tasks;
using Avalonia.Threading;


namespace Lister.ViewModels;


public class BadgeViewModel : ViewModelBase
{
    private static Dictionary<string, Avalonia.Media.Imaging.Bitmap> _pathToImage;

    private readonly string _semiProtectedTypeName = "Lister.ViewModels.BadgeEditorViewModel";
    private readonly double _interLineAddition = 1;
    private SolidColorBrush _incorrectLineBackground;
    private SolidColorBrush _incorrectMemberBorderColor;
    private SolidColorBrush _correctMemberBorderColor;
    private Thickness _incorrectMemberBorderThickness;
    private Thickness _correctMemberBorderThickness;
    private SolidColorBrush _normMemberBorderColor;
    private IMemberColorProvider _badLineColorProvider;

    internal int Id { get; set; }
    internal double Scale { get; private set; }
    internal double LeftSpan { get; private set; }
    internal double TopSpan { get; private set; }
    internal double RightSpan { get; private set; }
    internal double BottomSpan { get; private set; }
    internal Badge BadgeModel { get; private set; }

    private Avalonia.Media.Imaging.Bitmap _imageBitmap;
    internal Avalonia.Media.Imaging.Bitmap ImageBitmap
    {
        get { return _imageBitmap; }
        private set
        {
            this.RaiseAndSetIfChanged (ref _imageBitmap, value, nameof (ImageBitmap));
        }

    }

    private double _widht;
    private double _height;

    private double _badgeWidth;
    internal double BadgeWidth
    {
        get { return _badgeWidth; }
        set
        {
            this.RaiseAndSetIfChanged (ref _badgeWidth, value, nameof (BadgeWidth));
        }
    }

    private double _badgeHeight;
    internal double BadgeHeight
    {
        get { return _badgeHeight; }
        set
        {
            this.RaiseAndSetIfChanged (ref _badgeHeight, value, nameof (BadgeHeight));
        }
    }

    private double _borderWidth;
    internal double BorderWidth
    {
        get { return _borderWidth; }
        set
        {
            this.RaiseAndSetIfChanged (ref _borderWidth, value, nameof (BorderWidth));
        }
    }

    private double _borderHeight;
    internal double BorderHeight
    {
        get { return _borderHeight; }
        set
        {
            this.RaiseAndSetIfChanged (ref _borderHeight, value, nameof (BorderHeight));
        }
    }

    private Avalonia.Thickness _margin;
    internal Avalonia.Thickness Margin
    {
        get { return _margin; }
        set
        {
            this.RaiseAndSetIfChanged (ref _margin, value, nameof (Margin));
        }
    }

    private TextLineViewModel _focusedLine;
    internal TextLineViewModel FocusedLine 
    {
        get { return _focusedLine; }
        private set 
        {
            if (( value == null ) && ( _focusedLine != null ))
            {
                _focusedLine.BecomeUnFocused ();
                _focusedLine = value;
            }
            else if ( value != null ) 
            {
                _focusedLine = value;

                if ( _focusedLine.isBorderViolent   ||   _focusedLine.isOverLayViolent )
                {
                    _focusedLine.BecomeFocused (_incorrectMemberBorderColor, _incorrectMemberBorderThickness);
                }
                else 
                {
                    _focusedLine.BecomeFocused (_correctMemberBorderColor, _correctMemberBorderThickness);
                }
            }

            if ((_focusedLine == null)   ||   string.IsNullOrWhiteSpace(_focusedLine.Content))
            {
                FocusedText = string.Empty;
            }
            else 
            {
                FocusedText = _focusedLine.Content;
            }
        } 
    }

    private string _focusedText;
    internal string FocusedText
    {
        get { return _focusedText; }
        set
        {
            this.RaiseAndSetIfChanged (ref _focusedText, value, nameof (FocusedText));
        }
    }

    private List <TextLineViewModel> BorderViolentLines { get; set; }
    private List <TextLineViewModel> OverlayViolentLines { get; set; }
    private ObservableCollection <TextLineViewModel> _textLines;
    internal ObservableCollection <TextLineViewModel> TextLines
    {
        get { return _textLines; }
        private set
        {
            this.RaiseAndSetIfChanged (ref _textLines, value, nameof (TextLines));
        }
    }

    private ImageViewModel _focusedImage;
    internal ImageViewModel FocusedImage 
    {
        get { return _focusedImage; }
        private set 
        {
            if ( ( _focusedImage != null ) && ( value == null ) )
            {
                _focusedImage.BecomeUnFocused ();
                _focusedImage = null;
            }
            else if ( value != null )
            {
                _focusedImage = value;
                _focusedImage.BecomeFocused (_correctMemberBorderColor, _correctMemberBorderThickness);
            }
        }
    }
    private ObservableCollection <ImageViewModel> _insideImages;
    internal ObservableCollection <ImageViewModel> InsideImages
    {
        get { return _insideImages; }
        private set
        {
            this.RaiseAndSetIfChanged (ref _insideImages, value, nameof (InsideImages));
        }
    }

    private ShapeViewModel _focusedRect;
    internal ShapeViewModel FocusedRect 
    {
        get { return _focusedRect; }
        private set
        {
            if ( ( _focusedRect != null ) && ( value == null ) )
            {
                _focusedRect.BecomeUnFocused ();
                _focusedRect = null;
            }
            else if ( value != null )
            {
                _focusedRect = value;
                _focusedRect.BecomeFocused (_correctMemberBorderColor, _correctMemberBorderThickness);
            }
        }
    }
    private ObservableCollection <ShapeViewModel> _insideRectangles;
    internal ObservableCollection <ShapeViewModel> InsideRectangles
    {
        get { return _insideRectangles; }
        private set
        {
            this.RaiseAndSetIfChanged (ref _insideRectangles, value, nameof (InsideRectangles));
        }
    }

    private ShapeViewModel _focusedEllipse;
    internal ShapeViewModel FocusedEllipse 
    {
        get { return _focusedEllipse; }
        private set
        {
            if ( (_focusedEllipse != null)   &&   (value == null) ) 
            {
                _focusedEllipse.BecomeUnFocused ();
                _focusedEllipse = null;
            }
            else if ( value != null )
            {
                _focusedEllipse = value;
                _focusedEllipse.BecomeFocused (_correctMemberBorderColor, _correctMemberBorderThickness);
            }
        }
    }
    private ObservableCollection <ShapeViewModel> _insideEllipses;
    internal ObservableCollection <ShapeViewModel> InsideEllipses
    {
        get { return _insideEllipses; }
        private set
        {
            this.RaiseAndSetIfChanged (ref _insideEllipses, value, nameof (InsideEllipses));
        }
    }

    private double _borderThickness;
    private Avalonia.Thickness _thicknessOfBorder;
    internal Avalonia.Thickness BorderThickness
    {
        get { return _thicknessOfBorder; }
        private set
        {
            this.RaiseAndSetIfChanged (ref _thicknessOfBorder, value, nameof (BorderThickness));
        }
    }

    private string _focusedFontSize;
    internal string FocusedFontSize
    {
        get { return _focusedFontSize; }
        set
        {
            this.RaiseAndSetIfChanged (ref _focusedFontSize, value, nameof (FocusedFontSize));
        }
    }

    internal bool IsCorrect { get; private set; }

    private bool _isChanged;
    internal bool IsChanged
    {
        get { return _isChanged; }
        set
        {
            this.RaiseAndSetIfChanged (ref _isChanged, value, nameof (IsChanged));
        }
    }


    static BadgeViewModel ( )
    {
        _pathToImage = new Dictionary<string , Avalonia.Media.Imaging.Bitmap> ( );
    }


    public BadgeViewModel ( Badge badgeModel, int id )
    {
        Id = id;

        BadgeModel = badgeModel;
        BadgeLayout layout = badgeModel.BadgeLayout;

        LeftSpan = layout.LeftSpan;
        TopSpan = layout.TopSpan;
        RightSpan = layout.RightSpan;
        BottomSpan = layout.BottomSpan;

        BadgeWidth = layout.OutlineWidth;
        BorderWidth = BadgeWidth + 2;
        _widht = BadgeWidth;
        BadgeHeight = layout.OutlineHeight;
        BorderHeight = BadgeHeight + 2;
        _height = BadgeHeight;
        TextLines = new ObservableCollection<TextLineViewModel> ();
        BorderViolentLines = new List <TextLineViewModel> ( );
        OverlayViolentLines = new List <TextLineViewModel> ();
        InsideImages = new ObservableCollection <ImageViewModel> ();
        InsideRectangles = new ObservableCollection <ShapeViewModel> ();
        InsideEllipses = new ObservableCollection <ShapeViewModel> ();
        IsCorrect = true;
        _borderThickness = 1;
        Scale = 1;
        BorderThickness = new Avalonia.Thickness (_borderThickness);
        FocusedFontSize = string.Empty;

        if ( _badLineColorProvider == null ) 
        {
            _badLineColorProvider = App.services.GetRequiredService<IMemberColorProvider> ();
        }

        string incorrectLineBackgroundRGB = 
                                    _badLineColorProvider.GetIncorrectLineBackgroundStr (badgeModel.BadgeLayout.TemplateName);
        string incorrectMemberBorderRGB = 
                                     _badLineColorProvider.GetIncorrectMemberBorderStr (badgeModel.BadgeLayout.TemplateName);
        string correctMemberBorderRGB =
                                     _badLineColorProvider.GetCorrectMemberBorderStr (badgeModel.BadgeLayout.TemplateName);
        List<byte> incorrectMemberBorderThickness =
                                 _badLineColorProvider.GetIncorrectMemberBorderThickness (badgeModel.BadgeLayout.TemplateName);
        List<byte> correctMemberBorderThickness =
                                 _badLineColorProvider.GetCorrectMemberBorderThickness (badgeModel.BadgeLayout.TemplateName);


        _incorrectLineBackground = GetColor (incorrectLineBackgroundRGB);
        _incorrectMemberBorderColor = GetColor (incorrectMemberBorderRGB);
        _correctMemberBorderColor = GetColor (correctMemberBorderRGB);
        _incorrectMemberBorderThickness = GetThickness (incorrectMemberBorderThickness);
        _correctMemberBorderThickness = GetThickness (correctMemberBorderThickness);

        List <TextualAtom> atoms = layout.TextualFields;
        OrderTextlinesByVertical (atoms);
       
        SetUpTextLines (atoms);
        GatherIncorrectLines ();

        Dispatcher.UIThread.Invoke
        (() =>
        {
            List<InsideImage> images = layout.InsideImages;
            SetUpImages (images);

            List<InsideShape> shapes = layout.InsideShapes;
            SetUpShapes (shapes);
        });
    }


    private BadgeViewModel ( BadgeViewModel badge )
    {
        Id = badge.Id;

        BadgeModel = badge.BadgeModel;
        BadgeLayout layout = BadgeModel.BadgeLayout;

        LeftSpan = badge.LeftSpan;
        TopSpan = badge.TopSpan;
        RightSpan = badge.RightSpan;
        BottomSpan = badge.BottomSpan;

        BadgeWidth = badge.BadgeWidth;
        BorderWidth = BadgeWidth + 2;
        BadgeHeight = badge.BadgeHeight;
        BorderHeight = BadgeHeight + 2;
        TextLines = new ObservableCollection <TextLineViewModel> ();
        BorderViolentLines = new List <TextLineViewModel> ();
        OverlayViolentLines = new List <TextLineViewModel> ();
        InsideImages = new ObservableCollection <ImageViewModel> ();
        InsideRectangles = new ObservableCollection <ShapeViewModel> ();
        InsideEllipses = new ObservableCollection <ShapeViewModel> ();
        IsCorrect = badge.IsCorrect;
        IsChanged = badge.IsChanged;
        Scale = badge.Scale;
        _borderThickness = 1;
        BorderThickness = new Avalonia.Thickness (_borderThickness);
        FocusedFontSize = string.Empty;
        _incorrectLineBackground = badge._incorrectLineBackground;
        _incorrectMemberBorderColor = badge._incorrectMemberBorderColor;
        _correctMemberBorderColor = badge._correctMemberBorderColor;
        _incorrectMemberBorderThickness = badge._incorrectMemberBorderThickness;
        _correctMemberBorderThickness = badge._correctMemberBorderThickness;

        foreach ( TextLineViewModel line   in   badge.TextLines )
        {
            TextLineViewModel clone = line.Clone ();
            TextLines.Add (clone);

            if ( line.isBorderViolent )
            {
                clone.isBorderViolent = true;
                BorderViolentLines.Add (clone);
            }

            if ( line.isOverLayViolent )
            {
                clone.isOverLayViolent = true;
                OverlayViolentLines.Add (clone);
            }
        }

        foreach ( ImageViewModel prototype   in   badge.InsideImages )
        {
            ImageViewModel image = prototype.Clone ();
            InsideImages.Add (image);
        }

        foreach ( ShapeViewModel prototype   in   badge.InsideRectangles )
        {
            ShapeViewModel rect = prototype.Clone ();
            InsideRectangles.Add (rect);
        }

        foreach ( ShapeViewModel prototype   in   badge.InsideEllipses )
        {
            ShapeViewModel ellipse = prototype.Clone ();
            InsideEllipses.Add (ellipse);
        }

        Scale = badge.Scale;
    }


    internal BadgeViewModel GetDimensionalOriginal () 
    {
        BadgeViewModel original = this.Clone();

        if ( original.Scale > 1 )
        {
            original.ZoomOut (Scale);
        }
        else if ( original.Scale < 1 )
        {
            original.ZoomOn (Scale);
        }

        return original;
    }


    internal BadgeViewModel Clone ()
    {
        BadgeViewModel clone = null;

        Dispatcher.UIThread.Invoke 
        (() => 
        {
            clone = new BadgeViewModel (this);
        });

        return clone;
    }


    internal void CopyFrom ( BadgeViewModel badge )
    {
        if ( FocusedLine != null ) 
        {
            FocusedLine = null;
        }

        InsideImages = new ();
        InsideRectangles = new ();
        InsideEllipses = new ();
        IsCorrect = badge.IsCorrect;
        IsChanged = badge.IsChanged;
        _borderThickness = 1;
        BorderThickness = new Avalonia.Thickness (_borderThickness);
        FocusedFontSize = string.Empty;
        _incorrectLineBackground = badge._incorrectLineBackground;
        _incorrectMemberBorderColor = badge._incorrectMemberBorderColor;
        _correctMemberBorderColor = badge._correctMemberBorderColor;
        _incorrectMemberBorderThickness = badge._incorrectMemberBorderThickness;
        _correctMemberBorderThickness = badge._correctMemberBorderThickness;

        TextLines = new ();
        BorderViolentLines = new List <TextLineViewModel> ();
        OverlayViolentLines = new List <TextLineViewModel> ();

        foreach ( TextLineViewModel line   in   badge.TextLines )
        {
            TextLineViewModel clone = line.Clone ();

            clone.ZoomOut (badge.Scale);
            clone.ZoomOn (Scale);

            TextLines.Add (clone);

            if ( line.isBorderViolent )
            {
                clone.isBorderViolent = true;
                BorderViolentLines.Add (clone);
            }

            if ( line.isOverLayViolent )
            {
                clone.isOverLayViolent = true;
                OverlayViolentLines.Add (clone);
            }
        }

        foreach ( ImageViewModel image   in   badge.InsideImages )
        {
            ImageViewModel clone = image.Clone ();
            clone.ZoomOut (badge.Scale);
            clone.ZoomOn (Scale);
            InsideImages.Add (clone);
        }

        foreach ( ShapeViewModel rect   in   badge.InsideRectangles )
        {
            ShapeViewModel clone = rect.Clone ();
            clone.ZoomOut (badge.Scale);
            clone.ZoomOn (Scale);
            InsideRectangles.Add (clone);
        }

        foreach ( ShapeViewModel ellipse   in   badge.InsideEllipses )
        {
            ShapeViewModel clone = ellipse.Clone ();
            clone.ZoomOut (badge.Scale);
            clone.ZoomOn (Scale);
            InsideEllipses.Add (clone);
        }
    }


    internal TextLineViewModel ? GetCoincidence ( string focusedContent, int elementNumber )
    {
        string lineContent = string.Empty;
        TextLineViewModel goalLine = null;
        int counter = 0;

        foreach ( TextLineViewModel line   in   TextLines )
        {
            lineContent = line.Content;

            if ( (lineContent == focusedContent)   &&   (elementNumber == counter) )
            {
                goalLine = line;
                break;
            }

            counter++;
        }

        return goalLine;
    }


    internal void SetFocusedLine ( string focusedContent, int elementNumber )
    {
        FocusedLine = GetCoincidence (focusedContent, elementNumber);

        if ( FocusedLine != null )
        {
            int visibleFontSize = ( int ) Math.Round (FocusedLine. FontSize / Scale);
            FocusedFontSize = visibleFontSize.ToString ();
        }
    }


    internal void SetFocusedImage ( int id )
    {
        foreach ( ImageViewModel image   in   InsideImages )
        {
            if ( id == image.Id )
            {
                FocusedImage = image;
                break;
            }
        }
    }


    internal void SetFocusedRectangle ( int rectId )
    {
        foreach ( ShapeViewModel rect   in   InsideRectangles )
        {
            if ( rectId == rect.Id )
            {
                FocusedRect = rect;
                break;
            }
        }
    }


    internal void SetFocusedEllipse ( int ellipseId )
    {
        foreach ( ShapeViewModel ellipse   in   InsideEllipses )
        {
            if ( ellipseId == ellipse.Id )
            {
                FocusedEllipse = ellipse;
                break;
            }
        }
    }


    //internal void SetFocusedMember <T> ( int id, ICollection <T> members ) where T : BoundToTextLine
    //{
    //    foreach ( BoundToTextLine member   in   members )
    //    {
    //        if ( id == member.Id )
    //        {
    //            ShapeViewModel shape = member as ShapeViewModel;

    //            if ( shape != null )
    //            {
    //                if ( shape.Kind == ShapeKind.rectangle )
    //                {
    //                    FocusedEllipse = shape;
    //                    FocusedEllipse.BecomeFocused (_correctMemberBorderColor);
    //                    break;
    //                }
    //                else if ( shape.Kind == ShapeKind.ellipse )
    //                {
    //                    FocusedRect = shape;
    //                    FocusedRect.BecomeFocused (_correctMemberBorderColor);
    //                    break;
    //                }
    //            }

    //            ImageViewModel image = member as ImageViewModel;

    //            if ( image != null ) 
    //            {
                    
    //            }


    //        }
    //    }
    //}


    internal void Show ()
    {
        string path = BadgeModel.BackgroundImagePath;

        if ( !_pathToImage.ContainsKey (path) )
        {
            _pathToImage.Add (path, ImageHelper.LoadFromResource (path));
        }

        this.ImageBitmap = _pathToImage[path];
    }


    internal void Hide ()
    {
        this.ImageBitmap = null;
    }


    internal void ZoomOn ( double coefficient )
    {
        BadgeWidth *= coefficient;
        BadgeHeight *= coefficient;

        BorderWidth = BadgeWidth + 2;
        BorderHeight = BadgeHeight + 2;

        Scale *= coefficient;

        LeftSpan *= coefficient;
        TopSpan *= coefficient;
        RightSpan *= coefficient;
        BottomSpan *= coefficient;

        foreach ( TextLineViewModel line   in   TextLines ) 
        {
            line.ZoomOn (coefficient);
        }

        foreach ( ImageViewModel image   in   InsideImages )
        {
            image.ZoomOn (coefficient);
        }

        foreach ( ShapeViewModel rect   in   InsideRectangles )
        {
            rect.ZoomOn (coefficient);
        }

        foreach ( ShapeViewModel ellipse   in   InsideEllipses )
        {
            ellipse.ZoomOn (coefficient);
        }
    }


    internal void ZoomOut ( double coefficient )
    {
        BadgeWidth /= coefficient;
        BadgeHeight /= coefficient;

        BorderWidth = BadgeWidth + 2;
        BorderHeight = BadgeHeight + 2;

        Scale /= coefficient;

        LeftSpan /= coefficient;
        TopSpan /= coefficient;
        RightSpan /= coefficient;
        BottomSpan /= coefficient;

        foreach ( TextLineViewModel line   in   TextLines )
        {
            line.ZoomOut (coefficient);
        }

        foreach ( ImageViewModel image   in   InsideImages )
        {
            image.ZoomOut (coefficient);
        }

        foreach ( ShapeViewModel rect   in   InsideRectangles )
        {
            rect.ZoomOut (coefficient);
        }

        foreach ( ShapeViewModel ellipse   in   InsideEllipses )
        {
            ellipse.ZoomOut (coefficient);
        }
    }


    internal void SetCorrectScale ( double scale )
    {
        if ( scale != 1 )
        {
            ZoomOn (scale);
        }
    }


    internal void Split ( double scale )
    {
        if ( FocusedLine == null )
        {
            return;
        }

        string content = ( string ) FocusedLine. Content;

        List<string> pieces = content.SplitBySeparators ([' ', '-'], ['-']);

        double layoutWidth = BadgeWidth;
        List <TextLineViewModel> splitted = FocusedLine.SplitYourself (pieces, scale, layoutWidth);
        ReplaceTextLine (FocusedLine, splitted);
        FocusedLine = null;
        FocusedFontSize = null;
    }


    private void ReplaceTextLine ( TextLineViewModel replaceble, List <TextLineViewModel> replacings )
    {
        if ( replaceble.isBorderViolent )
        {
            BorderViolentLines.Remove (replaceble);
        }

        if ( replaceble.isOverLayViolent )
        {
            OverlayViolentLines.Remove (replaceble);
        }

        if ( ! replaceble.isBorderViolent   &&   ! replaceble.isOverLayViolent ) 
        {
            MarkAsCorrect (replaceble);
        }

        TextLines.Remove (replaceble);

        foreach ( TextLineViewModel line   in   replacings )
        {
            CheckLineCorrectness ( line );
            TextLines.Add (line);
            line.BecomeUnFocused ();
        }

        IsChanged = true;
    }


    private void SetUpTextLines ( List <TextualAtom> orderedTextualFields )
    {
        double summaryVerticalOffset = 0;

        for ( int index = 0;   index < orderedTextualFields.Count;   index++ ) 
        {
            bool isSplitingOccured = false;
            bool isTimeToShiftNextLine = false;
            TextualAtom textAtom = orderedTextualFields [index];
            double fontSize = textAtom.FontSize;
            double lineLength = textAtom.Width;
            textAtom.TopOffset += summaryVerticalOffset;
            double topOffset = textAtom.TopOffset;
            string beingProcessedLine = textAtom.Content.Trim();
            string additionalLine = string.Empty;

            while ( true )
            {
                double usefulTextBlockWidth;

                if ( MainViewModel.MainViewIsWaiting )
                {
                    var result1 = Dispatcher.UIThread.Invoke<double>
                    (() =>
                    {
                        return TextLineViewModel.CalculateWidth (beingProcessedLine, textAtom);
                    });

                    usefulTextBlockWidth = result1;
                }
                else
                {
                    usefulTextBlockWidth = TextLineViewModel.CalculateWidth (beingProcessedLine, textAtom);
                }

                bool lineIsOverflow = ( usefulTextBlockWidth >= lineLength );

                if ( ! lineIsOverflow ) 
                {
                    if ( isTimeToShiftNextLine )
                    {
                        summaryVerticalOffset += (textAtom.FontSize + _interLineAddition);
                        topOffset += ( textAtom.FontSize + _interLineAddition );
                    }

                    if ( isSplitingOccured )
                    {
                        isTimeToShiftNextLine = true;
                    }

                    TextualAtom atom = new TextualAtom (textAtom, topOffset, beingProcessedLine);
                    atom.TopOffset = topOffset;
                    atom.Content = beingProcessedLine;

                    TextLineViewModel textLine;

                    if ( MainViewModel.MainViewIsWaiting )
                    {
                        var result2 = Dispatcher.UIThread.Invoke<TextLineViewModel>
                        (() =>
                        {
                            return new TextLineViewModel (atom);
                        });

                        textLine = result2;
                    }
                    else
                    {
                        textLine = new TextLineViewModel (atom);
                    }

                    TextLines.Add (textLine);

                    if ( additionalLine != string.Empty ) 
                    {
                        beingProcessedLine = additionalLine.Trim();
                        additionalLine = string.Empty;
                        continue;
                    }
                    else 
                    {
                        break;
                    }
                }

                List<string> splited = beingProcessedLine.SeparateTail ();
                //List<string> splited = beingProcessedLine.Split ();


                string str = "fdfdf-dfdfd-fdfdf-dfdfd";





                if ( (splited.Count > 0)   &&   textAtom.IsSplitable )
                {
                    beingProcessedLine = splited [0].Trim();
                    additionalLine = splited [1] + " " + additionalLine;
                    isSplitingOccured = true;
                }
                else
                {
                    TextualAtom atom = new TextualAtom (textAtom, topOffset, beingProcessedLine);
                    atom.TopOffset = topOffset;
                    atom.Content = beingProcessedLine;

                    TextLineViewModel textLine;

                    if ( MainViewModel.MainViewIsWaiting )
                    {
                        var result3 = Dispatcher.UIThread.Invoke<TextLineViewModel>
                        (() =>
                        {
                            return new TextLineViewModel (atom);
                        });

                        textLine = result3;
                    }
                    else
                    {
                        textLine = new TextLineViewModel (atom);
                    }

                    TextLines.Add (textLine);
                    textLine.isBorderViolent = true;
                    break;
                }
            }
        }
    }


    private void OrderTextlinesByVertical ( List <TextualAtom> textualFields )
    {
        for ( int index = 0;   index < textualFields.Count;   index++ )
        {
            for ( int num = index;   num < textualFields.Count - 1;   num++ ) 
            {
                TextualAtom current = textualFields [index];
                TextualAtom next = textualFields [index + 1];

                if ( current.TopOffset > next.TopOffset )
                {
                    TextualAtom reserve = textualFields [index];
                    textualFields [index] = next;
                    textualFields [index + 1] = reserve;
                }
            }
        }
    }


    private void SetUpImages ( List <InsideImage> insideImages ) 
    {
        for ( int index = 0;   index < insideImages.Count;   index++ )
        {
            InsideImage image = insideImages [index];
            ImageViewModel imageViewModel = new ImageViewModel (index, image);

            InsideImages.Add (imageViewModel);

            ShiftDownBelowMembersIfShould ( imageViewModel );
        }
    }


    private void SetUpShapes ( List <InsideShape> insideShapes )
    {
        int rectId = 0;
        int ellipseId = 0;

        for ( int index = 0;   index < insideShapes.Count;   index++ )
        {
            InsideShape shape = insideShapes [index];

            if ( shape.Type == ShapeType.rectangle )
            {
                ShapeViewModel rectangle = new ShapeViewModel (rectId, shape);
                rectId++;
                InsideRectangles.Add (rectangle);
                ShiftDownBelowMembersIfShould( rectangle );
            }
            else if ( shape.Type == ShapeType.ellipse ) 
            {
                ShapeViewModel ellipse = new ShapeViewModel (ellipseId, shape);
                ellipseId++;
                InsideEllipses.Add (ellipse);
                ShiftDownBelowMembersIfShould( ellipse );
            }
        }
    }


    private void ShiftDownBelowMembersIfShould ( BoundToTextLine member )
    {
        if ( ! string.IsNullOrWhiteSpace(member.Binding) ) 
        {
            double startMemberTopOffset = member.TopOffset;
            int scratch = 0;
            int boundPartsCount = 0;
            bool boundIsFound = false;
            double delta = 0;
            int index = 0;

            for ( ;   index < TextLines.Count;   index++ )
            {
                TextLineViewModel line = TextLines [index];

                if ( member.Binding == line.DataSource.Name )
                {
                    if ( ! boundIsFound ) 
                    {
                        delta += line.TopOffset;
                    }

                    boundIsFound = true;
                    boundPartsCount++;

                    if ( member.IsAboveOfBinding )
                    {
                        delta -= member.TopOffset;
                        delta -= member.HeightWithBorder;
                    }
                    else 
                    {
                        delta += line.FontSize;
                        delta += _interLineAddition;
                    }
                }
                else if ( boundIsFound ) 
                {
                    break;
                }
            }

            if ( boundIsFound ) 
            {
                scratch = index;
                delta -= _interLineAddition;
                member.TopOffset += delta;
                ShiftBelowMembers ( member, index, startMemberTopOffset );
            }
        }
    }


    private void ShiftBelowMembers ( BoundToTextLine member, int startIndex, double memberRelativeTopOffset ) 
    {
        for ( int index = startIndex;   index < TextLines.Count;   index++ )
        {
            TextLineViewModel line = TextLines [index];
            line.TopOffset += ( member.Height + memberRelativeTopOffset );
        }

        ShiftBelowBounds (InsideImages, member, startIndex, memberRelativeTopOffset);
        ShiftBelowBounds (InsideRectangles, member, startIndex, memberRelativeTopOffset);
        ShiftBelowBounds (InsideEllipses, member, startIndex, memberRelativeTopOffset);
    }


    private void ShiftBelowBounds <T> ( ObservableCollection <T> bounds, BoundToTextLine member, int startIndex
                                      , double memberRelativeTopOffset ) where T : BoundToTextLine
    {
        for ( int index = 0;   index < bounds.Count;   index++ )
        {
            BoundToTextLine bound = bounds [index];

            if (( ! string.IsNullOrWhiteSpace (bound.Binding))   &&   (bound.TopOffset > member.TopOffset))
            {
                bound.TopOffset += ( member.Height + memberRelativeTopOffset );
            }
        }
    }


    private void GatherIncorrectLines ( )
    {
        foreach ( TextLineViewModel line   in   TextLines )
        {
            bool isViolent = CheckInsideSpansViolation ( line );

            if ( isViolent )
            {
                BorderViolentLines.Add ( line );
                line.isBorderViolent = true;
                MarkAsIncorrect ( line );
                line.BecomeUnFocused ();
                IsCorrect = false;
            }
        }

        CheckOverlayViolation ();
    }


    #region CheckLineCorrectness

    internal void CheckFocusedLineCorrectness ()
    {
        if ( FocusedLine == null )
        {
            return;
        }

        CheckLineCorrectness (FocusedLine);

        bool borderViolentsExist = ( BorderViolentLines.Count > 0 );
        bool overlayingExist = ( OverlayViolentLines.Count > 0 );

        if ( OverlayViolentLines.Count > 0 )
        {
            TextLineViewModel line = OverlayViolentLines [0];
        }

        IsCorrect = ! ( borderViolentsExist   ||   overlayingExist );
    }


    private void CheckLineCorrectness ( TextLineViewModel checkable )
    {
        bool isBorderViolent = CheckInsideSpansViolation (checkable);

        if ( ! isBorderViolent )
        {
            if ( checkable.isBorderViolent )
            {
                BorderViolentLines.Remove (checkable);
                checkable.isBorderViolent = false;
            }
        }
        else
        {
            if ( ! checkable.isBorderViolent )
            {
                BorderViolentLines.Add (checkable);
                checkable.isBorderViolent = true;
                MarkAsIncorrect (checkable);
            }
        }

        bool isOverlayViolent = CheckSingleOverlayViolation (0, checkable);

        if ( ! isOverlayViolent )
        {
            if ( checkable.isOverLayViolent )
            {
                OverlayViolentLines.Remove (checkable);
                checkable.isOverLayViolent = false;
            }
        }
        else
        {
            if ( !checkable.isOverLayViolent )
            {
                OverlayViolentLines.Add (checkable);
                checkable.isOverLayViolent = true;
                MarkAsIncorrect (checkable);
            }
        }

        if ( !checkable.isBorderViolent   &&   ! checkable.isOverLayViolent )
        {
            MarkAsCorrect (checkable);
        }
    }


    private void MarkAsIncorrect ( TextLineViewModel setable )
    {
        setable.Background = _incorrectLineBackground;
        setable.Mark (_incorrectMemberBorderColor, _incorrectMemberBorderThickness);
    }


    private void MarkAsCorrect ( TextLineViewModel setable )
    {
        setable.Background = null;
        setable.Mark (_correctMemberBorderColor, _correctMemberBorderThickness);
    }


    private SolidColorBrush ? GetColor ( List<byte> rgb )
    {
        if ( rgb == null )
        {
            return null;
        }
        else
        {
            if ( rgb.Count == 3 )
            {
                byte r = rgb [0];
                byte g = rgb [1];
                byte b = rgb [2];

                SolidColorBrush brush;
                Color color = new Color (255, r, g, b);

                if ( MainViewModel.MainViewIsWaiting )
                {
                    var result = Dispatcher.UIThread.Invoke
                    (() =>
                    {
                        return new SolidColorBrush (color);
                    });

                    brush = result;
                }
                else
                {
                    brush = new SolidColorBrush (color);
                }

                return brush;
            }
            else
            {
                return null;
            }
        }
    }


    private SolidColorBrush ? GetColor ( string hexColor )
    {
        if ( hexColor == null )
        {
            return null;
        }
        else
        {
            if ( hexColor.Length == 7 )
            {
                SolidColorBrush brush;
                Color color;

                if ( ! Color.TryParse (hexColor, out color) )
                {
                    return null;
                }

                if ( MainViewModel.MainViewIsWaiting )
                {
                    var result = Dispatcher.UIThread.Invoke
                    (() =>
                    {
                        return new SolidColorBrush (color);
                    });

                    brush = result;
                }
                else
                {
                    brush = new SolidColorBrush (color);
                }

                return brush;
            }
            else
            {
                return null;
            }
        }
    }


    private Thickness GetThickness ( List<byte> ltrb )
    {
        if ( ltrb == null )
        {
            return new Thickness (0, 0, 0, 0);
        }
        else
        {
            if ( ltrb.Count == 4 )
            {
                double left = ltrb [0];
                double top = ltrb [1];
                double right = ltrb [2];
                double bottom = ltrb [3];

                Thickness thickness;

                if ( MainViewModel.MainViewIsWaiting )
                {
                    var result = Dispatcher.UIThread.Invoke
                    (() =>
                    {
                        return new Thickness (left, top, right, bottom);
                    });

                    thickness = result;
                }
                else
                {
                    thickness = new Thickness (left, top, right, bottom);
                }

                return thickness;
            }
            else
            {
                return new Thickness (0, 0, 0, 0);
            }
        }
    }


    private bool CheckInsideSpansViolation ( TextLineViewModel line )
    {
        double rest = BadgeWidth - ( line.LeftOffset + line.UsefullWidth );
        bool notExceedToRight = ( rest >= RightSpan );
        bool notExceedToLeft = ( line.LeftOffset >= LeftSpan );
        bool notExceedToTop = ( line.TopOffset >= TopSpan );
        rest = BadgeHeight - ( line.TopOffset + line.FontSize );
        bool notExceedToBottom = ( rest >= BottomSpan );

        bool isViolent = ! ( notExceedToRight   &&   notExceedToLeft   &&   notExceedToTop   &&   notExceedToBottom );

        return isViolent;
    }


    private void CheckOverlayViolation ( )
    {
        for ( int index = 0;   index < TextLines. Count;   index++ )
        {
            TextLineViewModel overlaying = TextLines [ index ];
            CheckSingleOverlayViolation (index, overlaying );
        }
    }


    private bool CheckSingleOverlayViolation ( int scratchInLines, TextLineViewModel overlaying )
    {
        bool isViolent = false;

        for ( int ind = scratchInLines;   ind < TextLines. Count;   ind++ )
        {
            TextLineViewModel underlaying = TextLines [ind];

            if ( underlaying.Equals (overlaying) ) continue; 
            
            double verticalDifference = Math.Abs (overlaying.TopOffset - underlaying.TopOffset);
            double topOffsetOfAbove = Math.Min (overlaying.TopOffset, underlaying.TopOffset);
            TextLineViewModel above = overlaying;

            if ( topOffsetOfAbove == underlaying.TopOffset )
            {
                above = underlaying;
            }

            isViolent = ( verticalDifference < above.FontSize );

            double horizontalDifference = Math.Abs (overlaying.LeftOffset - underlaying.LeftOffset);
            double leftOffsetOfLeftist = Math.Min (overlaying.LeftOffset, underlaying.LeftOffset);
            TextLineViewModel leftist = overlaying;

            if ( leftOffsetOfLeftist == underlaying.LeftOffset )
            {
                leftist = underlaying;
            }

            isViolent = isViolent   &&   ( horizontalDifference < leftist.UsefullWidth );

            if ( isViolent )
            {
                if ( ! overlaying.isOverLayViolent )
                {
                    OverlayViolentLines.Add (overlaying);
                    overlaying.isOverLayViolent = true;
                    MarkAsIncorrect ( overlaying );
                }

                break;
            }
        }

        return isViolent;
    }

    #endregion

    #region FontSizeChange

    internal void IncreaseFontSize ( double changeSize )
    {
        ChangeFontSize (changeSize, false);
    }


    internal void ReduceFontSize ( double changeSize )
    {
        ChangeFontSize (changeSize, true);
    }


    private void ChangeFontSize ( double changeSize, bool toReduce )
    {
        if ( FocusedLine == null )
        {
            return;
        }

        if ( toReduce )
        {
            FocusedLine.Reduce (changeSize);
        }
        else
        {
            FocusedLine.Increase (changeSize);
        }

        IsChanged = true;
        int visibleFontSize = (int) Math.Round (FocusedLine. FontSize / Scale);
        FocusedFontSize = visibleFontSize.ToString ();
        CheckFocusedLineCorrectness ();
    }
    #endregion

    #region Moving

    internal void MoveCaptured ( Avalonia.Point delta )
    {
        if ( FocusedLine != null )
        {
            double currentLeftOffset = FocusedLine.LeftOffset;
            double currentTopOffset = FocusedLine.TopOffset;

            FocusedLine.Shift (delta);
            PreventMemberHiding (FocusedLine, currentLeftOffset, currentTopOffset, GetDirection(delta));
            IsChanged = true;
        }
        else if ( FocusedImage != null )
        {
            double currentLeftOffset = FocusedImage.LeftOffset;
            double currentTopOffset = FocusedImage.TopOffset;

            FocusedImage.Shift (delta);
            PreventMemberHiding (FocusedImage, currentLeftOffset, currentTopOffset, GetDirection (delta));
            IsChanged = true;
        }
        else if ( FocusedRect != null )
        {
            double currentLeftOffset = FocusedRect.LeftOffset;
            double currentTopOffset = FocusedRect.TopOffset;

            FocusedRect.Shift (delta);
            PreventMemberHiding (FocusedRect, currentLeftOffset, currentTopOffset, GetDirection (delta));
            IsChanged = true;
        }
        else if ( FocusedEllipse != null ) 
        {
            double currentLeftOffset = FocusedEllipse.LeftOffset;
            double currentTopOffset = FocusedEllipse.TopOffset;

            FocusedEllipse.Shift (delta);
            PreventMemberHiding (FocusedEllipse, currentLeftOffset, currentTopOffset, GetDirection (delta));
            IsChanged = true;
        }
    }


    private List<string> GetDirection ( Avalonia.Point delta ) 
    {
        List<string> directions = [];

        if (delta.X > 0) 
        {
            directions.Add ("Left");
        }

        if ( delta.X < 0 )
        {
            directions.Add ("Right");
        }

        if ( delta.Y > 0 )
        {
            directions.Add ("Up");
        }

        if ( delta.Y < 0 )
        {
            directions.Add ("Down");
        }

        return directions;
    }


    internal void FocusedToSide ( string direction )
    {
        if ( FocusedLine != null )
        {
            IsChanged = ShiftFocusedIfShould (direction, FocusedLine);
            CheckFocusedLineCorrectness ();
        }
        else if ( FocusedImage != null )
        {
            IsChanged = ShiftFocusedIfShould (direction, FocusedImage);
        }
        else if ( FocusedEllipse != null )
        {
            IsChanged = ShiftFocusedIfShould (direction, FocusedEllipse);
        }
        else if ( FocusedRect != null )
        {
            IsChanged = ShiftFocusedIfShould (direction, FocusedRect);
        }
    }


    private bool ShiftFocusedIfShould ( string direction, BadgeMember shiftable )
    {
        List<string> directions = [];
        directions.Add (direction);

        double currentLeftOffset = shiftable.LeftOffset;
        double currentTopOffset = shiftable.TopOffset;

        if ( direction == "Left" )
        {
            shiftable.LeftOffset -= Scale;
            IsChanged = true;
        }

        if ( direction == "Right" )
        {
            shiftable.LeftOffset += Scale;
            IsChanged = true;
        }

        if ( direction == "Up" )
        {
            shiftable.TopOffset -= Scale;
            IsChanged = true;
        }

        if ( direction == "Down" )
        {
            shiftable.TopOffset += Scale;
            IsChanged = true;
        }

        PreventMemberHiding (shiftable, currentLeftOffset, currentTopOffset, directions);

        return IsChanged;
    }


    private void PreventMemberHiding ( BadgeMember preventable, double oldLeftOffset, double oldTopOffset
                                                                                      , List<string> directions )
    {
        bool isHiddenBeyondRight = (preventable.LeftOffset > (BadgeWidth - RightSpan))  &&  (directions.Contains ("Right"));

        if ( isHiddenBeyondRight ) 
        {
            preventable.LeftOffset = oldLeftOffset;
        }

        bool isHiddenBeyondLeft = (preventable.LeftOffset < ( preventable.Width - LeftSpan ) * ( -1 ))  
                                 &&  (directions.Contains ("Left"));

        if ( isHiddenBeyondLeft )
        {
            preventable.LeftOffset = oldLeftOffset;
        }

        bool isHiddenBeyondBottom = (preventable.TopOffset > ( BadgeHeight - BottomSpan ))  &&  (directions.Contains ("Down"));

        if ( isHiddenBeyondBottom )
        {
            preventable.TopOffset = oldTopOffset;
        }

        bool isHiddenBeyondTop = (preventable.TopOffset < ( preventable.Height / 4 ) * ( -1 ))  
                                 &&  (directions.Contains ("Up"));

        if ( isHiddenBeyondTop )
        {
            preventable.TopOffset = oldTopOffset;
        }
    }


    //private void PreventImageOrShapeHiding ( BoundToTextLine preventable, double oldLeftOffset, double oldTopOffset ) 
    //{
    //    bool isPreventableBeyondRight = ( preventable.LeftOffset > (BadgeWidth - preventable.WidthWithBorder/2) );

    //    if ( isPreventableBeyondRight )
    //    {
    //        preventable.LeftOffset = oldLeftOffset;
    //    }

    //    bool isPreventableBeyondLeft = (preventable.LeftOffset < ((preventable.Width/2) * ( -1 )));

    //    if ( isPreventableBeyondLeft )
    //    {
    //        preventable.LeftOffset = oldLeftOffset;
    //    }

    //    bool isPreventableBeyondBottom = ( preventable.TopOffset > (BadgeHeight - preventable.HeightWithBorder) );

    //    if ( isPreventableBeyondBottom )
    //    {
    //        preventable.TopOffset = oldTopOffset;
    //    }

    //    bool isPreventableBeyondTop = ( preventable.TopOffset < 0 );

    //    if ( isPreventableBeyondTop )
    //    {
    //        preventable.TopOffset = oldTopOffset;
    //    }
    //}

    #endregion


    internal void ResetFocusedText ( string newText )
    {
        if ( FocusedLine == null ) 
        {
            return;
        }

        if ( FocusedLine. Content != newText ) 
        {
            IsChanged = true;
        }

        FocusedLine. Content = newText;
        CheckFocusedLineCorrectness ();
    }


    internal void ReleaseFocused ( )
    {
        if ( FocusedLine != null )
        {
            CheckFocusedLineCorrectness ();
            FocusedFontSize = string.Empty;
            FocusedLine = null;
        }
        else if ( FocusedEllipse != null ) 
        {
            FocusedEllipse = null;
        }
        else if ( FocusedRect != null )
        {   
            FocusedRect = null;
        }
        else if ( FocusedImage != null )
        {
            FocusedImage = null;
        }
    }
}