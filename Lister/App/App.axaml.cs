using Avalonia;
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

namespace Lister;

public partial class App : Avalonia.Application
{
    private static bool _runningOnWindow = false;
    private static bool _runningOnLinux = false;
    private MainView _mainView;


    public static string JsonSchemeFolderName { get; private set; }
    public static string WorkDirectoryPath { get; private set; }
    public static string ImagesUri { get; private set; }
    public static string ConfigPath { get; private set; }



    public static MainWindow MainWindow { get; private set; }
    public static string ResourceFolderName { get; private set; }
    public static string ResourceUriType { get; private set; }
    public static string ResourceDirectoryUri { get; private set; }
    public static string OsName { get; private set; }
    public static ServiceProvider services;


    static App () 
    {
        ServiceCollection collection = new ServiceCollection ();
        collection.AddNeededServices ();
        services = collection.BuildServiceProvider ();

        string workDirectory = @"./";
        DirectoryInfo containingDirectory = new DirectoryInfo (workDirectory);
        WorkDirectoryPath = containingDirectory.FullName;

        bool isWindows = RuntimeInformation.IsOSPlatform (OSPlatform.Windows);
        bool isLinux = RuntimeInformation.IsOSPlatform (OSPlatform.Linux);
        string osDescription = RuntimeInformation.OSDescription;

        if ( isWindows )
        {
            JsonSchemeFolderName = @"Resources\JsonSchemes\";
            ResourceFolderName = @"Resources\";
            ResourceUriType = "file:///";
            OsName = "Windows";
        }
        else if ( isLinux ) 
        {
            JsonSchemeFolderName = "Resources/JsonSchemes/";
            ResourceFolderName = "Resources/";
            ResourceUriType = "file://";
            OsName = "Linux";
        }

        ResourceDirectoryUri = ResourceUriType + WorkDirectoryPath + ResourceFolderName;
        ImagesUri = "avares://Lister/Assets/";
        ConfigPath = WorkDirectoryPath + ResourceFolderName + "ListerConfig.json";
    }


    public App (){ }


    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }


    public override async void OnFrameworkInitializationCompleted ()
    {
        if ( ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop )
        {
            SplashWindow splashWindow = new SplashWindow ();
            desktop.MainWindow = splashWindow;
            splashWindow.Show ();

            await BadgeAppearenceProvider.SetUp (( App.WorkDirectoryPath + App.ResourceFolderName )
                                               , ( App.WorkDirectoryPath + App.JsonSchemeFolderName ), OsName);

            MainViewModel mainViewModel = services.GetRequiredService<MainViewModel> ();

            MainView mainView = new MainView ()
            {
                DataContext = mainViewModel
            };

            MainWindow mainWindow = new MainWindow ()
            {
                DataContext = new MainWindowViewModel (),
                Content = mainView
            };

            App.MainWindow = mainWindow;
            desktop.MainWindow = mainWindow;
            desktop.MainWindow.Show ();
            splashWindow.Close ();

            mainWindow.Closing += ( s, e ) =>
            {
                if ( MainViewModel.InWaitingState )
                {
                    e.Cancel = true;
                }
                else 
                {
                    e.Cancel = false;
                }
            };
        }
        else if ( ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform )
        {
            singleViewPlatform.MainView = _mainView;
        }

        base.OnFrameworkInitializationCompleted ();
    }


    [DllImport ("user32.dll")]
    static extern void keybd_event ( byte bvk, byte bScan, int flag, ulong extraInfo );

}


//Stopwatch sw = new Stopwatch ();
//sw.Start ();

//for ( int index = 0;   index < 100;   index++ )
//{
//    keybd_event (0x09, 0x45, 0x0001, 0);

//    int aaa = 0;

//    for ( int ind = 0;   ind < 10_000_000;   ind++ ) 
//    {
//        aaa += 1;
//    }
//}



//keybd_event (0x09, 0x45, 0x0001, 0);
//Thread.Sleep (1000);
//keybd_event (0x09, 0x45, 0x0001, 0);

//keybd_event (0x0d, 0x45, 0x0001, 0);