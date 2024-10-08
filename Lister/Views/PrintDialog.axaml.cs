using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Input;
using Avalonia.ReactiveUI;
using Lister.ViewModels;
using MessageBox.Avalonia.Views;
using ReactiveUI;
using Avalonia.Media;
using ExtentionsAndAuxiliary;
using Microsoft.Extensions.DependencyInjection;
using Avalonia.Interactivity;
using static QuestPDF.Helpers.Colors;

namespace Lister.Views;

public partial class PrintDialog : BaseWindow
{
    private PrintDialogViewModel _vm;


    public PrintDialog ( int pageAmmount, PrintAdjustingData printAdjusting )
    {
        InitializeComponent ();

        PrintDialogViewModel printMV = new PrintDialogViewModel ();
        DataContext = printMV;
        _vm = printMV;

        _vm.Prepare ();
        _vm.TakeAmmountAndAdjusting (pageAmmount, printAdjusting);
        _vm.TakeView (this);

        printerChoosing.SelectedIndex = _vm.SelectedIndex;
        allPages.IsChecked = true;
        amountText.Text = "1";
    }


    public void TextChanged ( object sender, TextChangedEventArgs args ) 
    {
        TextBox textBox = (TextBox)sender;
        string text = textBox.Text;

        bool glyphIsIncorrect = true;

        for ( int index = 0;   index < text.Length;   index++ ) 
        {
            char ch = text [index];

            glyphIsIncorrect = ( ch == ' ' )   ||   ( ch == '-' )   ||   ( ch == ',' ) 
                                    ||   ( ch == '0' )   ||   ( ch == '1' )   ||   ( ch == '2' )
                                    ||   (ch == '3')   ||   ( ch == '4' )   ||   ( ch == '5' )
                                    ||   (ch == '6')   ||   ( ch == '7' )   ||   ( ch == '8' )   ||   ( ch == '9' );

            if ( ! glyphIsIncorrect )
            {
                textBox.Text = text.Remove ( index, 1 );
                break;
            }
        }
    }
}



public class PrintAdjustingData 
{
    public string PrinterName { get; set; }
    public List<int> PageNumbers { get; set; }
    public int CopiesAmount { get; set; }
    public bool Cancelled { get; set; }
}