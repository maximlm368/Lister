﻿using System;
using System.IO;
using System.Drawing;
using QuestPDF;
using QuestPDF.Drawing;
using QuestPDF.Elements;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Pdf = QuestPDF.Infrastructure;
using QuestPDF.Previewer;
using SkiaSharp;
using static System.Net.Mime.MediaTypeNames;
using ContentAssembler;
using System.Reflection.Metadata;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using System.Data.Common;
using Lister.Extentions;
using Avalonia.Platform;

namespace Lister.ViewModels;

class ConverterToPdf
{
    private string ? currentImagePath = null;
    private Pdf.Image? image = null;
    private int step = 0;
    public IEnumerable<byte []> bytes;

    internal void ConvertToExtention(List<VMBadge> items, string filePathToSave)
    {
        if ( items.Count == 0 ) return;
        Settings.License = LicenseType.Community;
        List<VMBadge []> pairs = items.SeparateIntoPairs ();

        var doc = QuestPDF.Fluent.Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size (794, 1123, Unit.Point);
                //page.Size (PageSizes.A3);
                page.MarginLeft (46, Unit.Point);
                page.MarginTop (30, Unit.Point);
                page.PageColor(Colors.White);
                page.DefaultTextStyle (x => x.FontSize (10));

                page.Content ().Column
                (
                    column =>
                    {
                        for ( int rowNumber = 0;   rowNumber < pairs.Count;   rowNumber++ )
                        {
                            column.Item ()
                            .Table
                            (
                                table =>
                                {
                                    table.ColumnsDefinition (
                                    columns =>
                                    {
                                        columns.ConstantColumn (350, Unit.Point);
                                        columns.ConstantColumn (350, Unit.Point);
                                    });

                                    RenderPair (table, pairs [rowNumber]);
                                }
                            );
                        }

                    }
                );
            });
        });

        //doc.GeneratePdf (filePathToSave);

        ImageGenerationSettings imageGenerationSettings = new ImageGenerationSettings ();
        imageGenerationSettings.RasterDpi = DocumentSettings.DefaultRasterDpi;

        bytes = doc.GenerateImages (imageGenerationSettings);

        //doc.GenerateImages (GetFilePath, imageGenerationSettings);


        //int bytesLength = bytes.Count ();
        //var enumerator = bytes.GetEnumerator ();
        //enumerator.MoveNext ();
        ////enumerator.MoveNext ();
        //var data = enumerator.Current;

        //if ( data != null )
        //{
        //    int length = data.Length;
        //    SaveBitmap ("./FromBitmap.png", 794, 1123, data);
        //}
    }


    private string GetFilePath(int index ) 
    {
        string path = "./FromBitmap" + index.ToString() + ".bmp";
        return path;
    }



    private void RenderPair ( TableDescriptor tableForPair, VMBadge [] pair )
    {
        step++;
        float badgeWidth = 0;

        try 
        {
            badgeWidth = ( float ) pair [0].badgeModel. badgeDescription. badgeDimensions. outlineSize. width;
        }
        catch ( Exception ex ) 
        {
            int stp = step;
        }


       
        float badgeHeight = ( float ) pair [0].badgeModel. badgeDescription. badgeDimensions. outlineSize. height;
        float personDataBlockInBadgeWidth = 
                                  ( float ) pair [0].badgeModel. badgeDescription. badgeDimensions. personTextAreaSize. width;
        float personDataBlockInBadgeHeight = 
                                  ( float ) pair [0].badgeModel. badgeDescription. badgeDimensions. personTextAreaSize. height;
        float personDataBlockLeftPadding = badgeWidth - personDataBlockInBadgeWidth;
        float personDataBlockTopPadding = badgeHeight - personDataBlockInBadgeHeight;

        for ( uint inPairCounter = 0;   inPairCounter < pair.Length;   inPairCounter++ )
        {
            if ( pair [inPairCounter] == null ) 
            {
                break;
            }

            VMBadge beingRenderedBadge = pair [ inPairCounter ];
            string imagePath = beingRenderedBadge.badgeModel. backgroundImagePath;
            bool firstTime = (currentImagePath == null);
            bool itsTimeToSetNewImage = firstTime  ||  ( currentImagePath != imagePath );

            if ( itsTimeToSetNewImage )
            {
                currentImagePath = imagePath;
                string complitedImagePath = GetImagePath (imagePath);
                image = Pdf.Image.FromFile (complitedImagePath);
            }

            tableForPair.Cell ().Row (1).Column (inPairCounter + 1)
                .Width (badgeWidth, Unit.Point).Height (badgeHeight, Unit.Point)
                .Layers
                            (
                               layers =>
                               {
                                   layers.PrimaryLayer ().Border (0.5f, Unit.Point).BorderColor(Colors.Grey.Medium)
                                    .Image (image)
                                    .FitArea ();

                                   layers
                                    .Layer ()
                                    .PaddingLeft (130, Unit.Point)
                                    .PaddingTop (65, Unit.Point)
                                    .AlignCenter ()
                                    .Column (column =>
                                    {
                                        RenderLastNameLineOnBadge(column, beingRenderedBadge);
                                        RenderFirstAndMiddleNamesLineOnBadge(column, beingRenderedBadge);
                                        RenderDepartmentLineOnBadge(column, beingRenderedBadge);
                                        RenderPositionLineOnBadge(column, beingRenderedBadge);
                                    });  
                               }
                           );
        }
    }


    private string GetImagePath ( string relativePath ) 
    {
        var containingDirectory = AppDomain.CurrentDomain.BaseDirectory;

        for ( int ancestorDirectoryCounter = 0;   ancestorDirectoryCounter < 5;   ancestorDirectoryCounter++ )
        {
            containingDirectory = Directory.GetParent (containingDirectory).FullName;
        }

        string resultPath = containingDirectory + relativePath.Remove (0, 7);
        return resultPath;
    }


    private void RenderLastNameLineOnBadge ( ColumnDescriptor column, VMBadge beingRenderedItem )
    {
        string lastName = beingRenderedItem.lastName;
        float fontSize = ( float ) beingRenderedItem.firstLevelFontSize;

        column
        .Item ()
        .AlignCenter ()
        .Text (lastName)
        .FontFamily ("Arial")
        .FontSize (fontSize);

        int amountOfReserveBlocks = beingRenderedItem.reserveLastNameTextBlocks.Count;

        for ( int reserveBlocksCounter = 0;   reserveBlocksCounter < amountOfReserveBlocks;   reserveBlocksCounter++ )
        {
            lastName = beingRenderedItem.reserveLastNameTextBlocks [reserveBlocksCounter];

            column
            .Item ()
            .AlignCenter ()
            .Text (lastName)
            .FontFamily ("Arial")
            .FontSize (fontSize);
        }
    }


    private void RenderFirstAndMiddleNamesLineOnBadge ( ColumnDescriptor column, VMBadge beingRenderedItem )
    {
        string firstAndMiddleName = beingRenderedItem.firstAndSecondName;
        float fontSize = ( float ) beingRenderedItem.secondLevelFontSize;

        column
        .Item ()
        .AlignCenter ()
        .Text (firstAndMiddleName)
        .FontFamily ("Arial")
        .FontSize (fontSize);

        int amountOfReserveBlocks = beingRenderedItem.reserveFirstAndMiddleNamesTextBlocks.Count;

        for ( int reserveBlocksCounter = 0;   reserveBlocksCounter < amountOfReserveBlocks;   reserveBlocksCounter++ )
        {
            firstAndMiddleName = beingRenderedItem.reserveFirstAndMiddleNamesTextBlocks [reserveBlocksCounter];

            column
            .Item ()
            .AlignCenter ()
            .Text (firstAndMiddleName)
            .FontFamily ("Arial")
            .FontSize (fontSize);
        }
    }


    private void RenderDepartmentLineOnBadge ( ColumnDescriptor column, VMBadge beingRenderedItem )
    {
        string ? departmentName = beingRenderedItem.departmentName;
        float fontSize = ( float ) beingRenderedItem.thirdLevelFontSize;

        column
       .Item ()
       .AlignCenter ()
       .Text (departmentName)
       .FontFamily ("Arial")
       .FontSize (fontSize);

        int amountOfReserveBlocks = beingRenderedItem.reserveDepartmentTextBlocks.Count;

        for ( int reserveBlocksCounter = 0;   reserveBlocksCounter < amountOfReserveBlocks;   reserveBlocksCounter++ )
        {
            departmentName = beingRenderedItem.reserveDepartmentTextBlocks [reserveBlocksCounter];

            column
           .Item ()
           .AlignCenter ()
           .Text (departmentName)
           .FontFamily ("Arial")
           .FontSize (fontSize);
        }
    }


    private void RenderPositionLineOnBadge ( ColumnDescriptor column, VMBadge beingRenderedItem )
    {
        string positionName = beingRenderedItem.positionName;
        float fontSize = ( float ) beingRenderedItem.thirdLevelFontSize;

        column
       .Item ()
       .AlignCenter ()
       .Text (positionName)
       .FontFamily ("Arial")
       .FontSize (fontSize);

        int amountOfReserveBlocks = beingRenderedItem.reservePositionTextBlocks.Count;

        for ( int reserveBlocksCounter = 0;   reserveBlocksCounter < amountOfReserveBlocks;   reserveBlocksCounter++ )
        {
            positionName = beingRenderedItem.reservePositionTextBlocks [reserveBlocksCounter];

            column
           .Item ()
           .AlignCenter ()
           .Text (positionName)
           .FontFamily ("Arial")
           .FontSize (fontSize);
        }
    }



    public void SaveBitmap ( string fileName, int width, int height, byte [] imageData )
    {

        //byte [] data = new byte [width * height * 4];

        //int o = 0;

        //for ( int i = 0;   i < width * height;   i++ )
        //{
        //    byte value = imageData [i];


        //    data [o++] = value;
        //    data [o++] = value;
        //    data [o++] = value;
        //    data [o++] = 0;
        //}
        
        unsafe
        {
            fixed ( byte* ptr = imageData )
            {
                PixelSize bitmapSize = new PixelSize (width, height);
                Vector dpi = new Vector (72, 72);
                int stride = 974;

                using ( Bitmap image = new Bitmap (PixelFormat.Rgba8888, AlphaFormat.Unpremul, new IntPtr (ptr)
                      , bitmapSize, dpi, stride) )
                {
                    //image.Save (Path.ChangeExtension (fileName, ".png"));
                    image.Save (fileName);
                }
            }
        }
    }

}

 

