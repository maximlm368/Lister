﻿using Avalonia;
using Avalonia.ReactiveUI;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using ContentAssembler;
using DataGateway;
using Microsoft.Extensions.DependencyInjection;
using Lister.ViewModels;
using Lister.Views;
using Splat;
using Avalonia.Styling;
using Avalonia.Controls;
//using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Runtime.InteropServices;
using Avalonia.Media;
using static System.Net.Mime.MediaTypeNames;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Media.Fonts;
using Avalonia.Platform;
using SkiaSharp;
using System.Globalization;
using System.Diagnostics;
using Microsoft.Win32;
using Microsoft.Extensions.Configuration;
using System.Runtime.CompilerServices;
using static SkiaSharp.HarfBuzz.SKShaper;
using ExtentionsAndAuxiliary;

namespace Lister;

public static class ServiceCollectionExtensions
{
    public static void AddNeededServices ( this IServiceCollection collection )
    {
        collection.AddSingleton <IServiceProvider, BadgeAppearenceServiceProvider> ();
        collection.AddSingleton <IPeopleSourceFactory, PeopleSourceFactory> ();
        collection.AddSingleton <IRowSource, PeopleXlsxSource> ();
        collection.AddSingleton <Lister.ViewModels.ConverterToPdf> ();
        collection.AddSingleton <Lister.ViewModels.PdfPrinter> ();
        collection.AddSingleton <IUniformDocumentAssembler, UniformDocAssembler> ();

        collection.AddSingleton (typeof (IBadgeAppearenceProvider), BadgeAppearenceFactory);
        collection.AddSingleton (typeof (IBadLineColorProvider), BadLineFactory);

        collection.AddSingleton (typeof (MainViewModel), MainViewModelFactory);
        collection.AddSingleton (typeof (PersonChoosingViewModel), PersonChoosingViewModelFactory);
        collection.AddSingleton (typeof (PersonSourceViewModel), PersonSourceViewModelFactory);
        collection.AddSingleton (typeof (SceneViewModel), SceneViewModelFactory);
        collection.AddSingleton (typeof (PageNavigationZoomerViewModel), NavigationZoomerViewModelFactory);
        collection.AddSingleton (typeof (PrintDialogViewModel), PrintDialogViewModelFactory);
        collection.AddSingleton (typeof (EditorViewModelArgs), EditorViewModelArgsFactory);
        collection.AddSingleton <BadgesBuildingViewModel> ();
        collection.AddSingleton (typeof (WaitingViewModel), WaitingViewModelFactory);
        collection.AddSingleton <LargeMessageViewModel> ();
    }


    private static IBadgeAppearenceProvider BadgeAppearenceFactory ( IServiceProvider serviceProvider )
    {
        object service = serviceProvider.GetService (typeof (BadgeAppearenceProvider));

        IBadgeAppearenceProvider result = new BadgeAppearenceProvider ();

        return result;
    }


    private static IBadLineColorProvider BadLineFactory ( IServiceProvider serviceProvider )
    {
        object service = serviceProvider.GetService (typeof (BadgeAppearenceProvider));

        IBadLineColorProvider result =
             new BadgeAppearenceProvider ();

        return result;
    }


    private static MainViewModel MainViewModelFactory ( IServiceProvider serviceProvider )
    {
        string suggestedName =
        GetterFromJson.GetSectionStrValue (new List<string> { "MainViewModel", "pdfFileName" }, App.ConfigPath);

        string saveTitle =
        GetterFromJson.GetSectionStrValue (new List<string> { "MainViewModel", "saveTitle" }, App.ConfigPath);

        string incorrectXSLX =
        GetterFromJson.GetSectionStrValue (new List<string> { "MainViewModel", "incorrectXSLX" }, App.ConfigPath);

        string limitIsExhaustedMessage =
        GetterFromJson.GetSectionStrValue (new List<string> { "MainViewModel", "limitIsExhaustedMessage" }, App.ConfigPath);

        string fileIsOpenMessage =
        GetterFromJson.GetSectionStrValue (new List<string> { "MainViewModel", "fileIsOpenMessage" }, App.ConfigPath);

        string fileIsTooBigMessage =
        GetterFromJson.GetSectionStrValue (new List<string> { "MainViewModel", "fileIsTooBig" }, App.ConfigPath);

        IEnumerable<IConfigurationSection> sections =
        GetterFromJson.GetChildren (new List<string> { "MainViewModel", "patterns" }, App.ConfigPath);

        List<string> patterns = new ();

        foreach ( IConfigurationSection section in sections )
        {
            patterns.Add (section.Value);
        }

        return new MainViewModel (App.OsName, suggestedName, saveTitle, incorrectXSLX, limitIsExhaustedMessage
                                                                      , fileIsOpenMessage, fileIsTooBigMessage);
    }


    private static PersonSourceViewModel PersonSourceViewModelFactory ( IServiceProvider serviceProvider )
    {
        List<string> args = new ();

        string pickerTitle =
        GetterFromJson.GetSectionStrValue (new List<string> { "PersonSourceViewModel", "pickerTitle" }, App.ConfigPath);
        args.Add (pickerTitle);

        string sourcePathKeeper =
        GetterFromJson.GetSectionStrValue (new List<string> { "PersonSourceViewModel", "sourcePathKeeper" }, App.ConfigPath);
        args.Add (sourcePathKeeper);

        string fileIsOpenMessage =
        GetterFromJson.GetSectionStrValue (new List<string> { "PersonSourceViewModel", "filePickerTitle" }, App.ConfigPath);
        args.Add (fileIsOpenMessage);

        IEnumerable<IConfigurationSection> sections =
            GetterFromJson.GetChildren (new List<string> { "PersonSourceViewModel", "xlsxHeaderNames" }, App.ConfigPath);

        List<string> headers = new ();

        foreach ( IConfigurationSection section in sections )
        {
            headers.Add (section.Value);
        }

        sections =
            GetterFromJson.GetChildren (new List<string> { "PersonSourceViewModel", "sourceExtentions" }, App.ConfigPath);

        List<string> patterns = new ();

        foreach ( IConfigurationSection section in sections )
        {
            patterns.Add (section.Value);
        }

        int badgeLimit = 0;

        string badgeLimitStr =
        GetterFromJson.GetSectionStrValue (new List<string> { "SceneViewModel", "badgeCountLimit" }, App.ConfigPath);
        
        badgeLimit = DigitalStringParser.ParseToInt (badgeLimitStr);

        PersonSourceViewModel result = new PersonSourceViewModel (args, patterns, headers, badgeLimit);
        return result;
    }


    private static PersonChoosingViewModel PersonChoosingViewModelFactory ( IServiceProvider serviceProvider )
    {
        string placeHolder =
        GetterFromJson.GetSectionStrValue (new List<string> { "PersonChoosingViewModel", "placeHolder" }, App.ConfigPath);

        SolidColorBrush entireListColor = GetColor ("PersonChoosingViewModel", "entireListColor");

        int inputLimit =
            GetterFromJson.GetSectionIntValue (new List<string> { "PersonChoosingViewModel", "inputLimit" }, App.ConfigPath);

        SolidColorBrush incorrectTemplateColor = GetColor ("PersonChoosingViewModel", "incorrectTemplateColor");

        SolidColorBrush defaultBackgroundColor = GetColor ("PersonChoosingViewModel", "defaultBackgroundColor");
        SolidColorBrush defaultBorderColor = GetColor ("PersonChoosingViewModel", "defaultBorderColor");
        SolidColorBrush defaultForegroundColor = GetColor ("PersonChoosingViewModel", "defaultForegroundColor");

        List <SolidColorBrush> defaultColors = new List <SolidColorBrush> () { defaultBackgroundColor, defaultBorderColor 
                                                                              , defaultForegroundColor};

        SolidColorBrush focusedBackgroundColor = GetColor ("PersonChoosingViewModel", "focusedBackgroundColor");
        SolidColorBrush focusedBorderColor = GetColor ("PersonChoosingViewModel", "focusedBorderColor");

        List <SolidColorBrush> focusedColors = new List <SolidColorBrush> () { focusedBackgroundColor, focusedBorderColor };

        SolidColorBrush hoveredBackgroundColor = GetColor ("PersonChoosingViewModel", "hoveredBackgroundColor");
        SolidColorBrush hoveredBorderColor = GetColor ("PersonChoosingViewModel", "hoveredBorderColor");

        List <SolidColorBrush> hoveredColors = new List <SolidColorBrush> () { hoveredBackgroundColor, hoveredBorderColor };

        SolidColorBrush selectedBackgroundColor = GetColor ("PersonChoosingViewModel", "selectedBackgroundColor");
        SolidColorBrush selectedBorderColor = GetColor ("PersonChoosingViewModel", "selectedBorderColor");
        SolidColorBrush selectedForegroundColor = GetColor ("PersonChoosingViewModel", "selectedForegroundColor");

        List <SolidColorBrush> selectedColors = new List <SolidColorBrush> () { selectedBackgroundColor, selectedBorderColor
                                                                            , selectedForegroundColor };

        PersonChoosingUserControl.SetComboboxHoveredItemColors (hoveredBackgroundColor, hoveredBorderColor);
        VisiblePerson.SetColors (defaultColors, focusedColors, selectedColors);

        return new PersonChoosingViewModel (placeHolder, entireListColor, inputLimit, incorrectTemplateColor
                                                  , defaultColors, focusedColors, selectedColors);
    }


    private static SolidColorBrush GetColor ( string forWhat, string colorSectionName )
    {
        IEnumerable <IConfigurationSection> sections =
        GetterFromJson.GetChildren (new List<string> { forWhat, colorSectionName }, App.ConfigPath);

        List<int> rgb = new () { 100, 100, 100 };
        int counter = 0;

        foreach ( IConfigurationSection section   in   sections )
        {
            int rgbIndex = 0;

            rgbIndex = DigitalStringParser.ParseToInt (section.Value);

            rgb [counter] = rgbIndex;
            counter++;

            if ( counter >= 3 ) break;
        }

        return new SolidColorBrush (new Color (255, ( byte ) rgb [0], ( byte ) rgb [1], ( byte ) rgb [2]));
    }


    private static SceneViewModel SceneViewModelFactory ( IServiceProvider serviceProvider )
    {
        int badgeLimit = 0;

        string badgeLimitStr =
        GetterFromJson.GetSectionStrValue (new List<string> { "SceneViewModel", "badgeCountLimit" }, App.ConfigPath);

        badgeLimit = DigitalStringParser.ParseToInt (badgeLimitStr);

        string extentionToolTip =
        GetterFromJson.GetSectionStrValue (new List<string> { "SceneViewModel", "extentionToolTip" }, App.ConfigPath);

        string shrinkingToolTip =
        GetterFromJson.GetSectionStrValue (new List<string> { "SceneViewModel", "shrinkingToolTip" }, App.ConfigPath);

        string fileIsOpenMessage =
        GetterFromJson.GetSectionStrValue (new List<string> { "SceneViewModel", "fileIsOpenMessage" }, App.ConfigPath);

        return new SceneViewModel (badgeLimit, extentionToolTip, shrinkingToolTip, fileIsOpenMessage);
    }


    private static PageNavigationZoomerViewModel NavigationZoomerViewModelFactory ( IServiceProvider serviceProvider )
    {
        string procentSymbol =
        GetterFromJson.GetSectionStrValue (new List<string> { "NavigationZoomerViewModel", "procentSymbol" }, App.ConfigPath);

        string maxDepthStr =
        GetterFromJson.GetSectionStrValue (new List<string> { "NavigationZoomerViewModel", "maxDepth" }, App.ConfigPath);
        short maxDepth = 3;

        maxDepth = DigitalStringParser.ParseToShort (maxDepthStr);

        if ( maxDepth < 1 )
        {
            maxDepth = 3;
        }

        string minDepthStr =
        GetterFromJson.GetSectionStrValue (new List<string> { "NavigationZoomerViewModel", "minDepth" }, App.ConfigPath);
        short minDepth = -3;

        minDepth = DigitalStringParser.ParseToShort (minDepthStr);

        if ( minDepth > -1 )
        {
            minDepth = -3;
        }

        return new PageNavigationZoomerViewModel (procentSymbol, maxDepth, minDepth);
    }


    private static PrintDialogViewModel PrintDialogViewModelFactory ( IServiceProvider serviceProvider )
    {
        string warnImagePath =
        GetterFromJson.GetSectionStrValue (new List<string> { "PrintDialogViewModel", "warnImagePath" }, App.ConfigPath);

        string emptyCopies =
        GetterFromJson.GetSectionStrValue (new List<string> { "PrintDialogViewModel", "emptyCopies" }, App.ConfigPath);

        string emptyPages =
        GetterFromJson.GetSectionStrValue (new List<string> { "PrintDialogViewModel", "emptyPages" }, App.ConfigPath);

        string emptyPrinters =
        GetterFromJson.GetSectionStrValue (new List<string> { "PrintDialogViewModel", "emptyPrinters" }, App.ConfigPath);

        return new PrintDialogViewModel (warnImagePath, emptyCopies, emptyPages, emptyPrinters);
    }


    private static EditorViewModelArgs EditorViewModelArgsFactory ( IServiceProvider serviceProvider )
    {
        string extentionToolTip =
        GetterFromJson.GetSectionStrValue (new List<string> { "EditorViewModel", "extentionToolTip" }, App.ConfigPath);

        string shrinkingToolTip =
        GetterFromJson.GetSectionStrValue (new List<string> { "EditorViewModel", "shrinkingToolTip" }, App.ConfigPath);

        string correctnessIcon =
        GetterFromJson.GetSectionStrValue (new List<string> { "EditorViewModel", "correctnessIcon" }, App.ConfigPath);

        string incorrectnessIcon =
        GetterFromJson.GetSectionStrValue (new List<string> { "EditorViewModel", "incorrectnessIcon" }, App.ConfigPath);

        string allFilter =
        GetterFromJson.GetSectionStrValue (new List<string> { "EditorViewModel", "allFilter" }, App.ConfigPath);

        string incorrectFilter =
        GetterFromJson.GetSectionStrValue (new List<string> { "EditorViewModel", "incorrectFilter" }, App.ConfigPath);

        string correctFilter =
        GetterFromJson.GetSectionStrValue (new List<string> { "EditorViewModel", "correctFilter" }, App.ConfigPath);

        string allTip =
        GetterFromJson.GetSectionStrValue (new List<string> { "EditorViewModel", "allTip" }, App.ConfigPath);

        string correctTip =
        GetterFromJson.GetSectionStrValue (new List<string> { "EditorViewModel", "correctTip" }, App.ConfigPath);

        string incorrectTip =
        GetterFromJson.GetSectionStrValue (new List<string> { "EditorViewModel", "incorrectTip" }, App.ConfigPath);

        SolidColorBrush focusedFontColor = GetColor ("EditorViewModel", "focusedFontColor");
        SolidColorBrush releasedFontColor = GetColor ("EditorViewModel", "releasedFontColor");
        SolidColorBrush focusedFontBorderColor = GetColor ("EditorViewModel", "focusedFontBorderColor");
        SolidColorBrush releasedFontBorderColor = GetColor ("EditorViewModel", "releasedFontBorderColor");

        EditorViewModelArgs result = new ();

        result.extentionToolTip = extentionToolTip;
        result.shrinkingToolTip = shrinkingToolTip;
        result.correctnessIcon = correctnessIcon;
        result.incorrectnessIcon = incorrectnessIcon;
        result.allFilter = allFilter;
        result.correctFilter = correctFilter;
        result.incorrectFilter = incorrectFilter;
        result.allTip = allTip;
        result.correctTip = correctTip;
        result.incorrectTip = incorrectTip;

        result.focusedFontSizeColor = focusedFontColor;
        result.releasedFontSizeColor = releasedFontColor;
        result.focusedFontSizeBorderColor = focusedFontBorderColor;
        result.releasedFontSizeBorderColor = releasedFontBorderColor;

        return result;
    }


    private static WaitingViewModel WaitingViewModelFactory ( IServiceProvider serviceProvider )
    {
        string gifName =
        GetterFromJson.GetSectionStrValue (new List<string> { "WaitingViewModel", "gifName" }, App.ConfigPath);

        return new WaitingViewModel(gifName);
    }
}



public class BadgeAppearenceServiceProvider : IServiceProvider
{
    public object? GetService ( Type serviceType )
    {
        if ( serviceType == null )
        {
            return null;
        }

        object result = null;

        bool isAimService = ( serviceType.FullName == "ContentAssembler .IBadgeAppearenceProvider" )
                            ||
                            ( serviceType.FullName == "ContentAssembler.IBadLineColorProvider" )
                            ||
                            ( serviceType.FullName == "ContentAssembler.IFontFileProvider" );

        if ( isAimService )
        {
            result = new BadgeAppearenceProvider ();
        }

        return result;
    }
}