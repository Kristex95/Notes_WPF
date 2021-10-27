using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Diagnostics;

namespace Notes_WPF
{
    
    public partial class MainWindow : Window
    {
        List<Notefile> NoteFilesList = new List<Notefile>();
        Dictionary<Button, Notefile> ButtonAndFileDict = new Dictionary<Button, Notefile>();
        Button LastPressedButton = null;
        string NotesFilePath = @"..\..\..\Notes_text";
        string ArchiveFilePath = @"..\..\..\Archive_notes";

        public MainWindow()
        {
            InitializeComponent();

            DirectoryInfo NotesDir = new DirectoryInfo(NotesFilePath); //choose directory

            FileInfo[] txtFiles = NotesDir.GetFiles("*.txt", SearchOption.AllDirectories); //Get all info about .txt files from chosen directory

            foreach (FileInfo file in txtFiles)
            {
                //add file data to list
                Notefile newNote = new Notefile();
                newNote.Path = file.FullName;
                newNote.Name = file.Name;
                newNote.CreatiionDate = file.CreationTime;
                NoteFilesList.Add(newNote);
            }

            foreach (Notefile nf in NoteFilesList)
            {
                //adding buttons to the Stack Panel
                string nameWithoutExt = System.IO.Path.GetFileNameWithoutExtension(nf.Name);
                var bc = new BrushConverter();
                var newButton = new Button() { Content = nameWithoutExt, FontSize = 20, Height = 40, Margin = new Thickness(0, 0, 0, 2), Background = (Brush)bc.ConvertFrom("#5b5b73"), BorderThickness = new Thickness(0, 0, 0, 0) };
                newButton.Click += SelectedNoteButton_Click;
                ButtonAndFileDict.Add(newButton, nf);
                Buttons_StackPanel.Children.Add(newButton);
            }
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void ButtonMinimize_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.MainWindow.WindowState = WindowState.Minimized;
        }

        private void ButtonMaximize_Click(object sender, RoutedEventArgs e)
        {
            if(Application.Current.MainWindow.WindowState != WindowState.Maximized)
            {
                Application.Current.MainWindow.WindowState = WindowState.Maximized;
            }
            else
            {
                Application.Current.MainWindow.WindowState = WindowState.Normal;
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void AddNote_Button_Click(object sender, RoutedEventArgs e)
        {
            var bc = new BrushConverter();
            var newButton = new Button() { Content = "New Note", Height = 40, FontSize = 20, Margin = new Thickness(0, 0, 0, 2), Background = (Brush)bc.ConvertFrom("#5b5b73"), BorderThickness = new Thickness(0, 0, 0, 0) };
            Buttons_StackPanel.Children.Add(newButton);
        }

        private void SelectedNoteButton_Click(object sender, RoutedEventArgs e)
        {
            ButtonAndFileDict.TryGetValue((Button)sender, out Notefile nf);
            Edit_TextBox.Text = nf.ReadFromFile();
            LastPressedButton = (Button)sender;
            Filename_TextBox.Text = System.IO.Path.GetFileNameWithoutExtension(nf.Name);
            CreationData_label.Content = nf.CreatiionDate;
            Filename_TextBox.IsEnabled = true;
        }

        private void Edit_TextBox_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (LastPressedButton != null)
            {
                ButtonAndFileDict.TryGetValue(LastPressedButton, out Notefile nf);
                nf.WriteToFile(Edit_TextBox.Text);
            }
        }

        private void Filename_TextBox_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (LastPressedButton != null)
            {
                ButtonAndFileDict.TryGetValue(LastPressedButton, out Notefile nf);
                
                LastPressedButton.Content = Filename_TextBox.Text;
                string ParentPath = nf.Path.Remove(nf.Path.Length - nf.Name.Length - 1);

                File.Move(nf.Path, ParentPath + "\\" + Filename_TextBox.Text + ".txt");

                File.Delete(nf.Path);
                
                nf.Name = Filename_TextBox.Text+ ".txt";
                nf.Path = ParentPath + "\\" + nf.Name;
                ButtonAndFileDict[LastPressedButton] = nf;
            }
        }
    }
}
