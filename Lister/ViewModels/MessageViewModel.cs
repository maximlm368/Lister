﻿using Avalonia.Controls;
using Avalonia.Input;
using Avalonia;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ContentAssembler;
using static System.Net.Mime.MediaTypeNames;
using System.Collections.ObjectModel;
using Avalonia.Media;
using System.Globalization;
using System.Reflection.Metadata;
using ExtentionsAndAuxiliary;
using Microsoft.VisualBasic;
using Avalonia.Media.Imaging;
using Lister.Extentions;
using System.Linq.Expressions;
using Lister.Views;
using System.Windows.Input;
using Avalonia.Platform.Storage;
using System.Reactive.Linq;

namespace Lister.ViewModels
{
    public class MessageViewModel : ViewModelBase
    {
        private static string _warnImageName = "Icons/warning-alert.ico";

        private readonly int _lineHeight = 16;
        private int _topMargin = 54;
        private MessageDialog _view;

        private Bitmap wI;
        internal Bitmap WarnImage
        {
            get { return wI; }
            private set
            {
                this.RaiseAndSetIfChanged (ref wI, value, nameof (WarnImage));
            }
        }

        private string _message;
        internal string Message
        {
            get { return _message; }
            set
            {
                this.RaiseAndSetIfChanged (ref _message, value, nameof (Message));
            }
        }


        public MessageViewModel ()
        {
            string warningIconPath = App.ResourceFolderName + _warnImageName;
            WarnImage = ImageHelper.LoadFromResource (warningIconPath);
        }


        internal void PassView ( MessageDialog view )
        {
            _view = view;
        }


        internal void Close ()
        {
            _view.Shut ();
        }
    }
}