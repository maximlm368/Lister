﻿using Avalonia;
using ContentAssembler;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Avalonia.Controls;
using Avalonia.Media;
using System.Windows.Input;
using System.Text;
using System.Net.WebSockets;
using System.ComponentModel;
using Lister.Views;
using Lister.Extentions;
using System.Collections.ObjectModel;
using static QuestPDF.Helpers.Colors;
using Avalonia.Controls.Shapes;
using DynamicData;
using ReactiveUI;
using Avalonia.Input;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Buffers.Binary;
using System.Reflection;
using Microsoft.Win32;
using ExtentionsAndAuxiliary;
using QuestPDF.Helpers;
using System;
using Microsoft.Extensions.DependencyInjection;
using Avalonia.Platform.Storage;
using System.Reactive.Linq;
using Avalonia.Remote.Protocol.Viewport;
using MessageBox.Avalonia.Views;
using AvaloniaEdit;
using Avalonia.Threading;


namespace Lister.ViewModels;

public class TemplateChoosingViewModel : ViewModelBase
{
    private static readonly string _buildingLimitIsExhaustedMessage = "Исчерпан лимит построений.";
    private static readonly string _title = "Ошибка";
    private static readonly string _saveTitle = "Сохранение документа";
    private static readonly string _suggestedFileNames = "Badge";
    public static int TappedBadgesBuildingButton { get; private set; }
    public static bool BuildingOccured { get; private set; }

    private TemplateChoosingUserControl _view;
    private ConverterToPdf _converter;
    private SceneViewModel _sceneVM;
    private PersonChoosingViewModel _personChoosingVM;
    private PageNavigationZoomerViewModel _zoomNavigationVM;
    private WaitingViewModel _waitingVM;
    private List <TemplateName> _templateNames;
    private Stopwatch _stopWatch;
    private bool _buildingIsLocked;

    private ObservableCollection <TemplateViewModel> _templates;
    internal ObservableCollection <TemplateViewModel> Templates
    {
        get
        {
            return _templates;
        }
        set
        {
            this.RaiseAndSetIfChanged (ref _templates, value, nameof (Templates));
        }
    }

    private bool _isOpen;
    internal bool IsOpen
    {
        set
        {
            this.RaiseAndSetIfChanged (ref _isOpen, value, nameof (_isOpen));
        }
        get
        {
            return _isOpen;
        }
    }

    private bool _buildingIsPossible;
    internal bool BuildingIsPossible
    {
        set
        {
            if ( value )
            {
                if ( _personChoosingVM == null )
                {
                    _personChoosingVM = App.services.GetRequiredService<PersonChoosingViewModel> ();
                }

                if ( _personChoosingVM.ChosenTemplate == null )
                {
                    value = false;
                }
            }

            this.RaiseAndSetIfChanged (ref _buildingIsPossible, value, nameof (BuildingIsPossible));
        }
        get
        {
            return _buildingIsPossible;
        }
    }

    private bool _clearIsEnable;
    internal bool ClearIsEnable
    {
        set
        {
            this.RaiseAndSetIfChanged (ref _clearIsEnable, value, nameof (ClearIsEnable));
        }
        get
        {
            return _clearIsEnable;
        }
    }

    private bool _saveIsEnable;
    internal bool SaveIsEnable
    {
        set
        {
            this.RaiseAndSetIfChanged (ref _saveIsEnable, value, nameof (SaveIsEnable));
        }
        get
        {
            return _saveIsEnable;
        }
    }

    private bool _printIsEnable;
    internal bool PrintIsEnable
    {
        set
        {
            this.RaiseAndSetIfChanged (ref _printIsEnable, value, nameof (PrintIsEnable));
        }
        get
        {
            return _printIsEnable;
        }
    }


    public TemplateChoosingViewModel ( IUniformDocumentAssembler docAssembler, SceneViewModel sceneViewModel )
    {
        _templateNames = docAssembler.GetBadgeModels ();
        _sceneVM = sceneViewModel;
        _converter = new ConverterToPdf ();
    }


    internal void PassView ( TemplateChoosingUserControl view )
    {
        _view = view;
    }


    internal void ChangeAccordingTheme ( string theme )
    {
        SolidColorBrush foundColor = new SolidColorBrush (MainWindow.black);
        SolidColorBrush unfoundColor = new SolidColorBrush (new Color (100, 0, 0, 0));

        if ( theme == "Dark" ) 
        {
            foundColor  = new SolidColorBrush (MainWindow.white);
            unfoundColor = new SolidColorBrush (new Color (100, 255, 255, 255));
        }

        ObservableCollection <TemplateViewModel> templates = new ();

        foreach ( TemplateName name   in   _templateNames )
        {
            SolidColorBrush brush;

            if ( name.isFound )
            {
                brush = foundColor;
            }
            else
            {
                brush = unfoundColor;
            }

            templates.Add (new TemplateViewModel (name, brush));
        }

        Templates = templates;
    }


    internal void BuildBadges ()
    {
        if ( _zoomNavigationVM == null )
        {
            _zoomNavigationVM = App.services.GetRequiredService<PageNavigationZoomerViewModel> ();
        }

        BuildingOccured = true;

        BadgeEditorViewModel.BackingToMainViewEvent += ()=>
        { 
            BuildingOccured = false; 
        };
        
        Build ();
    }


    internal void Build ()
    {
        if ( _personChoosingVM == null )
        {
            _personChoosingVM = App.services.GetRequiredService<PersonChoosingViewModel> ();
        }

        if ( _personChoosingVM.ChosenTemplate == null )
        {
            return;
        }

        if ( _personChoosingVM.EntirePersonListIsSelected )
        {
            TappedBadgesBuildingButton = 1;
            ModernMainViewModel mainViewModel = App.services.GetRequiredService<ModernMainViewModel> ();
            mainViewModel.SetWaiting ();
        }
        else if ( _personChoosingVM.SinglePersonIsSelected )
        {
            BuildSingleBadge ();
            _sceneVM.EnableButtons ();
            _zoomNavigationVM.EnableZoom ();
            _zoomNavigationVM.SetEnablePageNavigation ();
        }
    }


    internal void BuildAllBadges ()
    {
        if ( _personChoosingVM.ChosenTemplate == null )
        {
            return;
        }

        _buildingIsLocked = true;
        BuildingIsPossible = false;

        Task task = new Task
        (
            () =>
            {
                bool buildingIsCompleted = _sceneVM.BuildBadges (_personChoosingVM.ChosenTemplate.Name);

                //Dispatcher.UIThread.Invoke 
                //(() => 
                //{
                //    ModernMainViewModel modernMV = App.services.GetRequiredService<ModernMainViewModel> ();
                //    modernMV.EndWaiting ();
                //});

                _sceneVM.EnableButtons ();
                _zoomNavigationVM.EnableZoom ();
                _zoomNavigationVM.SetEnablePageNavigation ();

                _buildingIsLocked = false;
                TappedBadgesBuildingButton = 0;

                if ( ! buildingIsCompleted )
                {
                    Dispatcher.UIThread.Invoke
                    (() =>
                    {
                        TappedBadgesBuildingButton = 0;
                        ModernMainViewModel mainViewModel = App.services.GetRequiredService<ModernMainViewModel> ();
                        mainViewModel.EndWaiting ();

                        var messegeDialog = new MessageDialog (ModernMainView.Instance);
                        messegeDialog.Message = _buildingLimitIsExhaustedMessage;
                        WaitingViewModel waitingVM = App.services.GetRequiredService<WaitingViewModel> ();
                        waitingVM.HandleDialogOpenig ();
                        messegeDialog.ShowDialog (MainWindow.Window);
                    });
                }
            }
        );

        task.Start ();
    }


    internal void BuildSingleBadge ()
    {
        if ( _personChoosingVM == null )
        {
            _personChoosingVM = App.services.GetRequiredService<PersonChoosingViewModel> ();
        }

        if ( _personChoosingVM.ChosenTemplate == null )
        {
            return;
        }

        _buildingIsLocked = true;
        bool buildingIsCompleted = _sceneVM.BuildSingleBadge (_personChoosingVM.ChosenTemplate. Name);
        _buildingIsLocked = false;

        if ( ! buildingIsCompleted )
        {
            var messegeDialog = new MessageDialog (ModernMainView.Instance);
            messegeDialog.Message = _buildingLimitIsExhaustedMessage;
            WaitingViewModel waitingVM = App.services.GetRequiredService<WaitingViewModel> ();
            waitingVM.HandleDialogOpenig ();
            messegeDialog.ShowDialog (MainWindow.Window);
        }
    }


    //internal void ClearBadges ()
    //{
    //    if ( _sceneVM == null )
    //    {
    //        _sceneVM = App.services.GetRequiredService<SceneViewModel> ();
    //    }

    //    _sceneVM.ClearBadges ();

    //    if ( _zoomNavigationVM == null )
    //    {
    //        _zoomNavigationVM = App.services.GetRequiredService<PageNavigationZoomerViewModel> ();
    //    }

    //    _zoomNavigationVM.DisableButtons ();
    //}


    //internal void DisableButtons ()
    //{
    //    ClearIsEnable = false;
    //    SaveIsEnable = false;
    //    PrintIsEnable = false;
    //}


    //internal void EnableButtons ()
    //{
    //    ClearIsEnable = true;
    //    SaveIsEnable = true;
    //    PrintIsEnable = true;
    //}


    //internal void GeneratePdff ()
    //{
    //    List<FilePickerFileType> fileExtentions = [];
    //    fileExtentions.Add (FilePickerFileTypes.Pdf);
    //    FilePickerSaveOptions options = new ();
    //    options.Title = _saveTitle;
    //    options.FileTypeChoices = new ReadOnlyCollection<FilePickerFileType> (fileExtentions);
    //    options.SuggestedFileName = _suggestedFileNames;
    //    Task<IStorageFile> chosenFile = MainWindow.CommonStorageProvider.SaveFilePickerAsync (options);

    //    TaskScheduler uiScheduler = TaskScheduler.FromCurrentSynchronizationContext ();

    //    chosenFile.ContinueWith
    //        (
    //           task =>
    //           {
    //               if ( task.Result != null )
    //               {
    //                   string result = task.Result.Path.ToString ();
    //                   int uriTypeLength = App.ResourceUriType. Length;
    //                   result = result.Substring (uriTypeLength, result.Length - uriTypeLength);

    //                   Task<bool> pdf = GeneratePdff (result);

    //                   pdf.ContinueWith
    //                       (
    //                       task =>
    //                       {
    //                           if ( pdf.Result == false )
    //                           {
    //                               var messegeDialog = new MessageDialog ();
    //                               messegeDialog.Message = _fileIsOpenMessage;
    //                               messegeDialog.ShowDialog (MainWindow._mainWindow);
    //                           }
    //                           else
    //                           {
    //                               if ( App.OsName == "Windows" )
    //                               {
    //                                   Process fileExplorer = new Process ();
    //                                   fileExplorer.StartInfo.FileName = "explorer.exe";
    //                                   result = ExtractPathWithoutFileName ( result );
    //                                   result = result.Replace ('/', '\\');
    //                                   fileExplorer.StartInfo.Arguments = result;
    //                                   fileExplorer.StartInfo.WindowStyle = ProcessWindowStyle.Maximized;
    //                                   fileExplorer.Start ();
    //                               }
    //                               else if ( App.OsName == "Linux" ) 
    //                               {
    //                                   Process fileExplorer = new Process ();
    //                                   fileExplorer.StartInfo.FileName = "Nautilus.bin";
    //                                   result = ExtractPathWithoutFileName ( result );
    //                                   fileExplorer.StartInfo.Arguments = result;
    //                                   fileExplorer.StartInfo.WindowStyle = ProcessWindowStyle.Maximized;
    //                                   fileExplorer.Start ();
    //                               }
    //                           }
    //                       }, uiScheduler);
    //               }
    //           }
    //        );
    //}


    //internal void GeneratePdf ()
    //{
    //    List<FilePickerFileType> fileExtentions = [];
    //    fileExtentions.Add (FilePickerFileTypes.Pdf);
    //    FilePickerSaveOptions options = new ();
    //    options.Title = _saveTitle;
    //    options.FileTypeChoices = new ReadOnlyCollection<FilePickerFileType> (fileExtentions);
    //    options.SuggestedFileName = _suggestedFileNames + GenerateNowDateString ();
    //    Task <IStorageFile> chosenFile = MainWindow.CommonStorageProvider.SaveFilePickerAsync (options);

    //    TaskScheduler uiScheduler = TaskScheduler.FromCurrentSynchronizationContext ();

    //    chosenFile.ContinueWith
    //        (
    //           task =>
    //           {
    //               if ( task.Result != null )
    //               {
    //                   string result = task.Result.Path.ToString ();
    //                   int uriTypeLength = App.ResourceUriType.Length;
    //                   result = result.Substring (uriTypeLength, result.Length - uriTypeLength);
                      
    //                   bool pdf = GeneratePdf (result);

    //                   if ( pdf == false )
    //                   {
    //                       var messegeDialog = new MessageDialog ();
    //                       messegeDialog.Message = _fileIsOpenMessage;
    //                       messegeDialog.ShowDialog (MainWindow._mainWindow);
    //                   }
    //                   else
    //                   {
    //                       if ( App.OsName == "Windows" )
    //                       {
    //                           Process fileExplorer = new Process ();
    //                           fileExplorer.StartInfo.FileName = "explorer.exe";
    //                           result = ExtractPathWithoutFileName (result);
    //                           result = result.Replace ('/', '\\');
    //                           fileExplorer.StartInfo.Arguments = result;
    //                           fileExplorer.StartInfo.WindowStyle = ProcessWindowStyle.Maximized;
    //                           fileExplorer.Start ();
    //                       }
    //                       else if ( App.OsName == "Linux" )
    //                       {
    //                           Process fileExplorer = new Process ();
    //                           fileExplorer.StartInfo.FileName = "nautilus";
    //                           result = ExtractPathWithoutFileName (result);
    //                           fileExplorer.StartInfo.Arguments = result;
    //                           fileExplorer.StartInfo.WindowStyle = ProcessWindowStyle.Maximized;
    //                           fileExplorer.Start ();
    //                       }
    //                   }
    //               }
    //           }, uiScheduler);
    //}


    //private string GenerateNowDateString ( )
    //{
    //    DateTime now = DateTime.Now;

    //    string day = now.Day.ToString ();

    //    if ( Int32.Parse(day) < 10 ) 
    //    {
    //        day = "0" + day;
    //    }

    //    string month = now.Month.ToString ();

    //    if ( Int32.Parse (month) < 10 )
    //    {
    //        month = "0" + month;
    //    }

    //    string hour = now.Hour.ToString ();

    //    if ( Int32.Parse (hour) < 10 )
    //    {
    //        hour = "0" + hour;
    //    }

    //    string minute = now.Minute.ToString ();

    //    if ( Int32.Parse (minute) < 10 )
    //    {
    //        minute = "0" + minute;
    //    }

    //    string result = "_" + day + month + now.Year.ToString () + "_" + hour + minute;
    //    return result;
    //}


    //internal Task<bool> GeneratePdff ( string fileToSave )
    //{
    //    List <PageViewModel> pages = GetAllPages ();
    //    Task<bool> task = new Task<bool> (() => { return _converter.ConvertToExtention (pages, fileToSave); });
    //    task.Start ();
    //    return task;
    //}


    //internal bool GeneratePdf ( string fileToSave )
    //{
    //    List <PageViewModel> pages = GetAllPages ();
    //    bool result = _converter.ConvertToExtention (pages, fileToSave);
    //    return result;
    //}


    //public void Print ()
    //{
    //    List <PageViewModel> pages = GetAllPages ();
    //    string fileToSave = @"intermidiate.pdf";
    //    Task pdf = new Task (() => { _converter.ConvertToExtention (pages, fileToSave); });
    //    pdf.Start ();
    //    Task printTask = pdf.ContinueWith
    //           (
    //              savingTask =>
    //              {
    //                  if ( App.OsName == "Windows" )
    //                  {
    //                      int length = _converter.intermidiateFiles.Count;
    //                      IStorageItem sItem = null;

    //                      ProcessStartInfo info = new ()
    //                      {
    //                          FileName = fileToSave,
    //                          Verb = "Print",
    //                          UseShellExecute = true,
    //                          ErrorDialog = false,
    //                          CreateNoWindow = true,
    //                          WindowStyle = ProcessWindowStyle.Minimized
    //                      };

    //                      bool ? procIsExited = Process.Start (info)?.WaitForExit (20_000);
    //                  }
    //                  else if ( App.OsName == "Linux" ) 
    //                  {
    //                      string printCommand = "lp " + fileToSave;
    //                      ExecuteBashCommand (printCommand);
    //                  }
    //              }
    //           );
    //}


    ////private bool PreventCommandExecution ()
    ////{
    ////    bool result = true;

    ////    if ( MainWindow.EventTimer == null )
    ////    {
    ////        return false;
    ////    }

    ////    MainWindow.EventTimer.Stop ();
    ////    TimeSpan expandTime = MainWindow.EventTimer.Elapsed;

    ////    if ( expandTime.Milliseconds < 50 )
    ////    {
    ////        return false;
    ////    }

    ////    return result;
    ////}


    //private static void ExecuteBashCommand ( string command )
    //{
    //    using ( Process process = new Process () )
    //    {
    //        process.StartInfo = new ProcessStartInfo
    //        {
    //            FileName = "/bin/bash",
    //            Arguments = $"-c \"{command}\"",
    //            RedirectStandardOutput = true,
    //            UseShellExecute = false,
    //            CreateNoWindow = true
    //        };

    //        process.Start ();

    //        //string result = process.StandardOutput.ReadToEnd ();

    //        process.WaitForExit ();
    //    }
    //}


    //private string ExtractPathWithoutFileName ( string wholePath )
    //{
    //    var builder = new StringBuilder ();
    //    string goalPath = string.Empty;

    //    for ( var index = wholePath.Length - 1; index >= 0; index-- )
    //    {
    //        bool fileNameIsAchieved = ( wholePath [index] == '/' ) || ( wholePath [index] == '\\' );

    //        if ( fileNameIsAchieved )
    //        {
    //            goalPath = wholePath.Substring (0, index);
    //            break;
    //        }
    //    }

    //    return goalPath;
    //}


    //private List <PageViewModel> GetAllPages ()
    //{
    //    List<PageViewModel> pages = _sceneVM.GetAllPages ();
    //    List<PageViewModel> dimensionallyOriginals = new List<PageViewModel> ();

    //    foreach ( PageViewModel page in pages )
    //    {
    //        dimensionallyOriginals.Add (page.GetDimendionalOriginal ());
    //    }

    //    return dimensionallyOriginals;
    //}


    private void TryToEnableBadgeCreationButton ()
    {
        if ( _personChoosingVM == null )
        {
            _personChoosingVM = App.services.GetRequiredService <PersonChoosingViewModel> ();
        }

        bool buildingIsPossible = ( _personChoosingVM.ChosenTemplate != null )   &&   _personChoosingVM.BuildingIsPossible;

        if ( buildingIsPossible )
        {
            BuildingIsPossible = true;
        }
        else 
        {
            BuildingIsPossible = false;
        }
    }


    internal void BuildDuringWaiting ( )
    {
        if ( ModernMainViewModel.MainViewIsWaiting    &&   ! _buildingIsLocked )
        {
            BuildAllBadges ();
        }
    }
}



//public enum TappedButton
//{
//    Build = 1,
//    Save = 2,
//    Print = 3
//}



public class TemplateViewModel : ViewModelBase
{
    private TemplateName TemplateName { get; set; }

    private string name;
    public string Name
    {
        get
        {
            return name;
        }
        set
        {
            this.RaiseAndSetIfChanged (ref name, value, nameof (Name));
        }
    }

    private SolidColorBrush cL;
    public SolidColorBrush Color
    {
        get
        {
            return cL;
        }
        set
        {
            this.RaiseAndSetIfChanged (ref cL, value, nameof (Color));
        }
    }


    public TemplateViewModel ( TemplateName templateName, SolidColorBrush color )
    {
        TemplateName = templateName;
        Name = templateName.name;
        Color = color;
    }
}


