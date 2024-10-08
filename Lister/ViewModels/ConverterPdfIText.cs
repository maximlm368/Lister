﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lister.ViewModels
{
    //internal class ConverterPdfIText
    //{
    //    private string? currentImagePath = null;
    //    private Pdf.Image? image = null;
    //    private int step = 0;
    //    public IEnumerable<byte []> bytes = null;
    //    public List<string> intermidiateFiles = new ();


    //    internal bool ConvertToExtention ( List <PageViewModel> pages, string filePathToSave )
    //    {
    //        bool result = true;
    //        bool isNothingToDo = ( pages == null )   ||   ( pages.Count == 0 )   ||   ( filePathToSave == null );

    //        if ( isNothingToDo )
    //        {
    //            return false;
    //        }

    //        Settings.License = LicenseType.Community;

    //        var doc = QuestPDF.Fluent.Document.Create (container =>
    //        {
    //            for ( int pageNumber = 0;   pageNumber < pages.Count;   pageNumber++ )
    //            {
    //                container.Page (page =>
    //                {
    //                    PageViewModel currentPage = pages [pageNumber];
    //                    float width = ( float ) PageViewModel.PageSize.Width;
    //                    float height = ( float ) PageViewModel.PageSize.Height;

    //                    page.Size (width, height, Unit.Point);
    //                    page.MarginLeft (0, Unit.Point);
    //                    page.MarginTop (0, Unit.Point);
    //                    page.PageColor (Colors.White);
    //                    page.DefaultTextStyle (x => x.FontSize (10));

    //                    ObservableCollection<BadgeLine> lines = currentPage.Lines;

    //                    page.Content ().PaddingTop (20).PaddingLeft (( float ) currentPage.ContentLeftOffset).Column
    //                    (
    //                        column =>
    //                        {
    //                            foreach ( BadgeLine currentLine in lines )
    //                            {
    //                                RenderLine (column, currentLine);
    //                            }
    //                        }
    //                    );
    //                });

    //            }

    //        });

    //        try
    //        {
    //            doc.GeneratePdf (filePathToSave);
    //        }
    //        catch ( IOException ex )
    //        {
    //            result = false;
    //        }

    //        return result;

    //    }


    //    private void RenderLine ( ColumnDescriptor column, BadgeLine line )
    //    {
    //        column.Item ()
    //              .Table
    //              (
    //                  table =>
    //                  {
    //                      table.ColumnsDefinition (
    //                          columns =>
    //                          {
    //                              for ( int badgeNumber = 0; badgeNumber < line.Badges.Count; badgeNumber++ )
    //                              {
    //                                  BadgeViewModel beingRendered = line.Badges [badgeNumber];
    //                                  float badgeWidth = ( float ) beingRendered.BadgeWidth;
    //                                  columns.ConstantColumn (badgeWidth, Unit.Point);
    //                                  RenderBadge (table, beingRendered, badgeNumber);
    //                              }
    //                          });
    //                  }
    //              );
    //    }


    //    private void RenderBadge ( TableDescriptor tableForLine, BadgeViewModel beingRendered, int badgeIndex )
    //    {
    //        if ( beingRendered == null ) return;

    //        step++;
    //        float badgeWidth = 0;

    //        try
    //        {
    //            badgeWidth = ( float ) beingRendered.BadgeWidth;
    //        }
    //        catch ( Exception ex )
    //        {
    //            int stp = step;
    //        }

    //        float badgeHeight = ( float ) beingRendered.BadgeHeight;
    //        string imagePath = beingRendered.BadgeModel.BackgroundImagePath;
    //        bool firstTime = ( currentImagePath == null );
    //        bool itsTimeToSetNewImage = firstTime || ( currentImagePath != imagePath );

    //        if ( itsTimeToSetNewImage )
    //        {
    //            currentImagePath = imagePath;
    //            string complitedImagePath = GetImagePath (imagePath);
    //            image = Pdf.Image.FromFile (complitedImagePath);
    //            //image = Pdf.Image.FromFile (imagePath);
    //        }

    //        tableForLine.Cell ().Row (1).Column (( uint ) badgeIndex + 1)
    //            .Width (badgeWidth, Unit.Point).Height (badgeHeight, Unit.Point)
    //            .Layers
    //                        (
    //                           layers =>
    //                           {
    //                               layers.PrimaryLayer ()
    //                               .Border (0.5f, Unit.Point)
    //                               .BorderColor (Colors.Grey.Medium)
    //                               .Image (image)
    //                               .FitArea ();

    //                               RenderTextLines (layers, beingRendered.TextLines);
    //                               RenderInsideImages (layers, beingRendered.InsideImages);
    //                               RenderInsideShapes (layers, beingRendered.InsideShapes);
    //                           }
    //                       );
    //    }


    //    private void RenderTextLines ( LayersDescriptor layers, IEnumerable<TextLineViewModel> textLines )
    //    {
    //        foreach ( TextLineViewModel textLine in textLines )
    //        {
    //            string text = textLine.Content;
    //            float paddingLeft = ( float ) textLine.LeftOffset;
    //            float paddingTop = ( float ) textLine.TopOffset;
    //            string fontFamily = textLine.FontFamily.Name;
    //            float fontSize = ( float ) textLine.FontSize;
    //            float maxWidth = ( float ) textLine.Width;

    //            layers
    //               .Layer ()
    //               .PaddingLeft (paddingLeft, Unit.Point)
    //               .PaddingTop (paddingTop, Unit.Point)
    //               .Text (text).DirectionFromRightToLeft ()
    //               .FontFamily (fontFamily)
    //               .Bold ()
    //               .FontSize (fontSize);
    //        }
    //    }


    //    private void RenderInsideImages ( LayersDescriptor layers, IEnumerable<ImageViewModel> insideImages )
    //    {
    //        foreach ( ImageViewModel image in insideImages )
    //        {
    //            float paddingLeft = ( float ) image.LeftOffset;
    //            float paddingTop = ( float ) image.TopOffset;
    //            string imagePath = image.Path;
    //            Pdf.Image img = Pdf.Image.FromFile (imagePath);

    //            layers
    //                .Layer ()
    //                .PaddingLeft (paddingLeft)
    //                .PaddingTop (paddingTop)
    //                .Image (img);
    //        }
    //    }


    //    private void RenderInsideShapes ( LayersDescriptor layers, IEnumerable<ImageViewModel> insideImages )
    //    {
    //        foreach ( ImageViewModel image in insideImages )
    //        {
    //            float paddingLeft = ( float ) image.LeftOffset;
    //            float paddingTop = ( float ) image.TopOffset;

    //            layers
    //                .Layer ()
    //                .PaddingLeft (paddingLeft)
    //                .PaddingTop (paddingTop)
    //                .Canvas (DrawGeometryElement);

    //        }
    //    }


    //    private void DrawGeometryElement ( SKCanvas canvas, QuestPDF.Infrastructure.Size size )
    //    {
    //        SKPaint paint = new SKPaint ();
    //        paint.Color = new SKColor (877);
    //        SKRect rect = new SKRect ();
    //        rect.Size = new SKSize (size.Width, size.Height);
    //        canvas.DrawRect (rect, paint);
    //    }


    //    //private string GetImagePath ( string relativePath )
    //    //{
    //    //    var containingDirectory = AppDomain.CurrentDomain. BaseDirectory;

    //    //    for ( int ancestorDirectoryCounter = 0;   ancestorDirectoryCounter < 5;   ancestorDirectoryCounter++ )
    //    //    {
    //    //        containingDirectory = Directory.GetParent (containingDirectory).FullName;
    //    //    }

    //    //    string resultPath = containingDirectory + relativePath.Remove (0, 7);
    //    //    return resultPath;
    //    //}


    //    private string GetImagePath ( string relativePath )
    //    {
    //        string resultPath = relativePath.Remove (0, 8);
    //        return resultPath;
    //    }
    //}
}
