﻿using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Platform.Storage;
using ContentAssembler;
using Lister.Views;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static QuestPDF.Helpers.Colors;
using MessageBox.Avalonia.Views;
using Avalonia.Media;
using Microsoft.Extensions.DependencyInjection;
using DataGateway;
using static SkiaSharp.HarfBuzz.SKShaper;
using DynamicData;
using ExCSS;
using Avalonia;
using Avalonia.Interactivity;

namespace Lister.ViewModels
{
    public class PersonSourceViewModel : ReactiveObject
    {
        private IReadOnlyList<string> _patterns;
        private readonly string _pickerTitle;
        private readonly string _filePickerTitle;
        private IReadOnlyList<string> _xslxHeaders;

        private string _sourcePathKeeper;

        private bool _isFirstTimeLoading = true;
        private string _declinedFilePath;

        private FilePickerOpenOptions _filePickerOptions;
        private FilePickerOpenOptions FilePickerOptions => _filePickerOptions ??= new ()
        {
            Title = _filePickerTitle,
            AllowMultiple = false,
            FileTypeFilter =
            [
                new FilePickerFileType (_pickerTitle)
                {
                    Patterns = _patterns
                }
            ]
        };

        private string _sourceFilePath;
        internal string SourceFilePath
        {
            get { return _sourceFilePath; }
            private set
            {
                _sourcePathKeeper = value;
                this.RaiseAndSetIfChanged (ref _sourceFilePath, value, nameof (SourceFilePath));
            }
        }

        internal readonly int personsLimitForSource;

        private bool _personsFileIsOpen;
        public bool FileIsOpen
        {
            get { return _personsFileIsOpen; }
            private set
            {
                if ( _personsFileIsOpen == value )
                {
                    _personsFileIsOpen = !_personsFileIsOpen;
                }

                this.RaiseAndSetIfChanged (ref _personsFileIsOpen, value, nameof (FileIsOpen));
            }
        }

        private bool _fileIsDeclined;
        internal bool FileIsDeclined
        {
            get { return _fileIsDeclined; }
            private set
            {
                if ( _fileIsDeclined == value )
                {
                    _fileIsDeclined = !_fileIsDeclined;
                }

                this.RaiseAndSetIfChanged (ref _fileIsDeclined, value, nameof (FileIsDeclined));
            }
        }

        private bool _fileIsTooBig;
        internal bool FileIsTooBig
        {
            get { return _fileIsTooBig; }
            private set
            {
                if ( _fileIsTooBig == value )
                {
                    _fileIsTooBig = !_fileIsTooBig;
                }

                this.RaiseAndSetIfChanged (ref _fileIsTooBig, value, nameof (FileIsTooBig));
            }
        }

        internal string FilePath { get; private set; }


        public PersonSourceViewModel ( string pickerTitle, string filePickerTitle, List<string> patterns
                                                                    , List<string> xslxHeaders, int limit, string sourceKeeper )
        {
            _pickerTitle = pickerTitle;
            _filePickerTitle = filePickerTitle;

            _patterns = patterns;
            _xslxHeaders = xslxHeaders;
            personsLimitForSource = limit;

            _sourcePathKeeper = sourceKeeper;
        }


        internal void OnLoaded ()
        {
            if ( !_isFirstTimeLoading )
            {
                return;
            }

            try
            {
                SetPath (_sourcePathKeeper);
            }
            catch ( IndexOutOfRangeException ex )
            {
                SetPath (null);
            }

            _isFirstTimeLoading = false;
        }


        internal async void ChooseFile ()
        {
            IReadOnlyList<IStorageFile> files =
                await MainWindow.CommonStorageProvider.OpenFilePickerAsync (FilePickerOptions);

            if ( ( files.Count == 1 ) && ( files [0] is not null ) )
            {
                string path = files [0].Path.LocalPath;

                CheckIfOpenOrIncorrectAndUse (path, true);
            }
        }


        internal void DeclineKeepedFileIfIncorrect ()
        {
            if ( string.IsNullOrWhiteSpace (_declinedFilePath) )
            {
                return;
            }

            DeclineIncorrectFile (_declinedFilePath);
        }


        internal void EmptySourcePath ()
        {
            SourceFilePath = string.Empty;
        }


        private void SetPath ( string ? path )
        {
            bool pathExists = (( path != null )   &&   ( path != string.Empty ));

            if ( pathExists )
            {
                CheckIfOpenOrIncorrectAndUse (path, false);
            }
            else
            {
                SourceFilePath = null;
            }
        }


        private void CheckIfOpenOrIncorrectAndUse ( string path, bool shouldSave )
        {
            FileInfo fileInfo = new FileInfo (path);

            if ( fileInfo.Exists )
            {
                try
                {
                    FileStream stream = fileInfo.OpenWrite ();
                    stream.Close ();
                }
                catch ( IOException ex )
                {
                    FileIsOpen = true;
                    return;
                }
            }
            else 
            {
                SourceFilePath = null;
                return;
            }

            XlsxFileState fileState = CheckCorrectnessIfXSLX (path);

            if ( fileState == XlsxFileState.IsIncorrect )
            {
                DeclineIncorrectFile (path);
                return;
            }
            else if ( ! TryUse (path)) 
            {
                FilePath = path;
                FileIsTooBig = true;
                return;
            }
            else if ( fileState == XlsxFileState.IsOpen )
            {
                FileIsOpen = true;
                return;
            }
            else if ( fileState == XlsxFileState.NotXlsxFile )
            {
                SourceFilePath = path;

                if ( shouldSave )
                {
                    SavePath ();
                }
            }
        }


        private void SavePath ()
        {
            SetterInJson.WritePersonSource ( App.ConfigPath, SourceFilePath );
        }


        private XlsxFileState CheckCorrectnessIfXSLX ( string path )
        {
            XlsxFileState fileState = XlsxFileState.NotXlsxFile;

            if ( path.Last () == 'x' )
            {
                try 
                {
                    IRowSource headersSource = App.services.GetService<IRowSource> ();
                    List<string> headers = headersSource.GetRow (path, 0);

                    for ( int index = 0;   index < _xslxHeaders.Count;   index++ )
                    {
                        bool isNotCoincident = ( headers [index] != _xslxHeaders [index] );

                        if ( isNotCoincident )
                        {
                            fileState = XlsxFileState.IsIncorrect;

                            break;
                        }
                    }
                }
                catch ( IOException ex )
                {
                    fileState = XlsxFileState.IsOpen;
                }
            }

            return fileState;
        }


        private bool TryUse ( string path )
        {
            IUniformDocumentAssembler documentAssembler = App.services.GetRequiredService<IUniformDocumentAssembler>();
            bool fileIsAcceptable = documentAssembler.SetPersonsFrom (path, personsLimitForSource);

            return fileIsAcceptable;
        }


        private void DeclineIncorrectFile ( string filePath )
        {
            FilePath = filePath;
            FileIsDeclined = true;
        }


        private enum XlsxFileState 
        {
            NotXlsxFile = 0,
            IsOpen = 1,
            IsIncorrect = 2,
            IsTooBig = 3
        }
    }
}





