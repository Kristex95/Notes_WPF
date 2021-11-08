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
using System.Windows.Threading;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;
using System.Globalization;

namespace Notes_WPF
{
    public partial class MainWindow : Window
    {
        Dictionary<Button, Notefile> ButtonFileDict = new Dictionary<Button, Notefile>();
        Button LastPressedButton = null;
        bool NotesOpened = true;
        string NotesFileCSVPath = @"..\..\..\Notes.csv";

        List<Notefile> FileList = new List<Notefile>();

        public MainWindow()
        {
            InitializeComponent();

            var bc = new BrushConverter();


            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = false,
            };
            using (var streamReader = new StreamReader(@"..\..\..\Notes.csv"))
            using (var csvReader = new CsvReader(streamReader, config))
            {
                var records = csvReader.GetRecords<Notefile>();
                foreach (var item in records)
                {
                    var newButton = new Button() { Content = item.Name, /*Name =  ,*/ Height = 40, FontSize = 20, Foreground = (Brush)bc.ConvertFrom("#dcdde3"), Margin = new Thickness(0, 0, 0, 2), Background = (Brush)bc.ConvertFrom("#5b5b73"), BorderThickness = new Thickness(0, 0, 0, 0) };
                    newButton.Click += SelectedNoteButton_Click;

                    if (!item.IsArchived) { 
                        
                        Buttons_StackPanel.Children.Add(newButton);
                    }
                
                    ButtonFileDict.Add(newButton, item);
                }
            }

            DispatcherTimer LiveTime = new DispatcherTimer();
            LiveTime.Interval = TimeSpan.FromSeconds(1);
            LiveTime.Tick += timer_Tick;
            LiveTime.Start();
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
            //add new note
            var NewNote = new List<Notefile>
            {
                new Notefile { Name = "New Note", Text = "", CreationDate = DateTime.Now, IsArchived = false },
            };
            var Config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = false,
            };
            using (var Stream = File.Open(NotesFileCSVPath, FileMode.Append))
            using (var Writer = new StreamWriter(Stream))
            using (var CSV = new CsvWriter(Writer, Config))
            {
                CSV.WriteRecords(NewNote);
            }
            

            //add new button
            var bc = new BrushConverter();
            var newButton = new Button() { Content = NewNote[0].Name, /*Name =  ,*/ Height = 40, FontSize = 20, Foreground = (Brush)bc.ConvertFrom("#dcdde3") ,Margin = new Thickness(0, 0, 0, 2), Background = (Brush)bc.ConvertFrom("#5b5b73"), BorderThickness = new Thickness(0, 0, 0, 0) };
            newButton.Click += SelectedNoteButton_Click;
            ButtonFileDict.Add(newButton, NewNote[0]);
            Buttons_StackPanel.Children.Add(newButton); //add button to stackpanel
            Buttons_ScrollViewer.ScrollToBottom();
            LastPressedButton = newButton;

            Filename_TextBox.CaretIndex = 0;
            Filename_TextBox.Text = newButton.Content.ToString();
            Filename_TextBox.IsEnabled = true;
            CreationData_label.Content = NewNote[0].CreationDate.ToString("dddd, dd MMMM yyyy HH:mm:ss");
            Edit_TextBox.Focus();
            Edit_TextBox.Text = "";
        }

        private void SelectedNoteButton_Click(object sender, RoutedEventArgs e)
        {
            LastPressedButton = (Button)sender;

            //get file connected to this button
            if (ButtonFileDict.TryGetValue((Button)sender, out Notefile Note)) {

                Trace.WriteLine('1');

                //UI changes
                Filename_TextBox.Text = Note.Name;
                CreationData_label.Content = Note.CreationDate.ToString("dddd, dd MMMM yyyy HH:mm:ss");

                //Textbox
                Edit_TextBox.Text = Note.Text;
                Filename_TextBox.IsEnabled = true;
                Edit_TextBox.Focus();
                Edit_TextBox.CaretIndex = Edit_TextBox.Text.Length;
            }                    
        }

        private void Edit_TextBox_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (LastPressedButton != null && ButtonFileDict.TryGetValue(LastPressedButton, out Notefile Note))
            {
                Note.Text = Edit_TextBox.Text;

                var Config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HasHeaderRecord = false,
                };
                using (var Stream = File.Open(NotesFileCSVPath, FileMode.Create))
                using (var Writer = new StreamWriter(Stream))
                using (var CSV = new CsvWriter(Writer, Config))
                {
                    CSV.WriteRecords(ButtonFileDict.Values);
                }
            }
        }

        private void Filename_TextBox_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (LastPressedButton != null && ButtonFileDict.TryGetValue(LastPressedButton, out Notefile Note))
            {
                Note.Name = Filename_TextBox.Text;
                LastPressedButton.Content = Note.Name;

                var Config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HasHeaderRecord = false,
                };
                using (var Stream = File.Open(NotesFileCSVPath, FileMode.Create))
                using (var Writer = new StreamWriter(Stream))
                using (var CSV = new CsvWriter(Writer, Config))
                {
                    CSV.WriteRecords(ButtonFileDict.Values);
                }
            }
        }

        private void Filename_TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (LastPressedButton != null)
                LastPressedButton.Content = Filename_TextBox.Text;
        }

        private void NotesAndArchive_Button_Click(object sender, RoutedEventArgs e)
        {
            Buttons_StackPanel.Children.Clear();
            LastPressedButton = null;
            if (NotesOpened)
            {
                //archive
                NotesOpened = false;
                NotesAndArchive_Button.Content = "Archive";
                AddNote_Button.Visibility = Visibility.Hidden;
                Filename_TextBox.IsReadOnly = true;
                Edit_TextBox.IsReadOnly = true;
                
                foreach (var elem in ButtonFileDict)
                {
                    if (elem.Value.IsArchived == true)
                    {
                        elem.Key.Content = elem.Value.Name;
                        Buttons_StackPanel.Children.Add(elem.Key);
                    }
                }
            }
            else
            {
                //notes
                NotesOpened = true;
                NotesAndArchive_Button.Content = "Notes";
                AddNote_Button.Visibility = Visibility.Visible;
                Filename_TextBox.IsReadOnly = false;
                Edit_TextBox.IsReadOnly = false;
                
                foreach (var elem in ButtonFileDict)
                {
                    if (elem.Value.IsArchived == false)
                    {
                        elem.Key.Content = elem.Value.Name;
                        Buttons_StackPanel.Children.Add(elem.Key);
                    }
                }
                
            }

            Edit_TextBox.Text = "";
            Filename_TextBox.Text = "";
            CreationData_label.Content = "";
        }

        private void DeleteNote_Button_Click(object sender, RoutedEventArgs e)
        {
            if (LastPressedButton != null)
            {
                ButtonFileDict.Remove(LastPressedButton);

                var Config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HasHeaderRecord = false,
                };
                using (var Stream = File.Open(NotesFileCSVPath, FileMode.Create))
                using (var Writer = new StreamWriter(Stream))
                using (var CSV = new CsvWriter(Writer, Config))
                {
                    CSV.WriteRecords(ButtonFileDict.Values);
                }

                Buttons_StackPanel.Children.Remove(LastPressedButton);
                ButtonFileDict.Remove(LastPressedButton);

                //select another file from the list
                if(ButtonFileDict.Count() != 0)
                {
                    foreach (var elem in ButtonFileDict)
                    {
                        if(elem.Value.IsArchived != NotesOpened)
                        {
                            LastPressedButton = elem.Key;
                            Edit_TextBox.Text = elem.Value.Text;
                            Filename_TextBox.Text = elem.Value.Name;
                            CreationData_label.Content = elem.Value.CreationDate.ToString("dddd, dd MMMM yyyy HH:mm:ss");
                            break;
                        }
                    }
                }
                else
                {
                    LastPressedButton = null;
                    Edit_TextBox.Text = "";
                    Filename_TextBox.Text = "";
                    CreationData_label.Content = DateTime.MinValue;
                }
            }
        }

        private void SendToAcrhive_Button_Click(object sender, RoutedEventArgs e)
        {
            if (LastPressedButton != null && ButtonFileDict.TryGetValue(LastPressedButton, out Notefile Note))
            {
                Note.IsArchived = !Note.IsArchived;

                Buttons_StackPanel.Children.Clear();

                if (NotesOpened)
                {
                    foreach (var elem in ButtonFileDict)
                    {
                        if (!elem.Value.IsArchived)
                        {
                            Buttons_StackPanel.Children.Add(elem.Key);
                        }
                    }
                }
                else {
                    foreach (var elem in ButtonFileDict)
                    {
                        if (elem.Value.IsArchived)
                        {
                            Buttons_StackPanel.Children.Add(elem.Key);
                        }
                    }
                }

                //select another file from the list
                if (ButtonFileDict.Count() != 0)
                {
                    foreach (var elem in ButtonFileDict)
                    {
                        if (elem.Value.IsArchived != NotesOpened)
                        {
                            LastPressedButton = elem.Key;
                            Edit_TextBox.Text = elem.Value.Text;
                            Filename_TextBox.Text = elem.Value.Name;
                            CreationData_label.Content = elem.Value.CreationDate.ToString("dddd, dd MMMM yyyy HH:mm:ss");
                            break;
                        }
                    }
                }
                else
                {
                    LastPressedButton = null;
                    Edit_TextBox.Text = "";
                    Filename_TextBox.Text = "";
                    CreationData_label.Content = DateTime.MinValue;
                }
            }
        }

        private void AlphabetSort_Button_Click(object sender, RoutedEventArgs e)
        {
            Buttons_StackPanel.Children.Clear();

            List<Notefile> NotesList = new List<Notefile>();
            foreach (var elem in ButtonFileDict)
            {
                if (elem.Value.IsArchived != NotesOpened)
                {
                    NotesList.Add(elem.Value);
                    ButtonFileDict.Remove(elem.Key);
                }
            }

            for (int i = 0; i < NotesList.Count; i++)
            {
                for (int j = i + 1; j < NotesList.Count; j++)
                {
                    switch (string.Compare(NotesList[i].Name, NotesList[j].Name, StringComparison.CurrentCultureIgnoreCase))
                    {
                        case -1:
                            break;
                        case 1:
                            Notefile buff = NotesList[i];
                            NotesList[i] = NotesList[j];
                            NotesList[j] = buff;
                            break;
                        default:
                            break;
                    }
                }
            }

            NotesList.Reverse();
            foreach (Notefile nf in NotesList)
            {
                var bc = new BrushConverter();
                var newButton = new Button() { Content = System.IO.Path.GetFileNameWithoutExtension(nf.Name), Height = 40, FontSize = 20, Foreground = (Brush)bc.ConvertFrom("#dcdde3"), Margin = new Thickness(0, 0, 0, 2), Background = (Brush)bc.ConvertFrom("#5b5b73"), BorderThickness = new Thickness(0, 0, 0, 0) };
                newButton.Click += SelectedNoteButton_Click;

                ButtonFileDict.Add(newButton, nf);
            }
            foreach (var elem in ButtonFileDict)
            {
                if (elem.Value.IsArchived != NotesOpened)
                {
                    Buttons_StackPanel.Children.Add(elem.Key);
                }
            }
        }

        private void CreationDateSort_Button_Click(object sender, RoutedEventArgs e)
        {
            Buttons_StackPanel.Children.Clear();

            List<Notefile> NotesList = new List<Notefile>();
            foreach (var elem in ButtonFileDict)
            {
                if (elem.Value.IsArchived != NotesOpened)
                {
                    NotesList.Add(elem.Value);
                    ButtonFileDict.Remove(elem.Key);
                }
            }

            for (int i = 0; i < NotesList.Count; i++)
            {
                for (int j = i + 1; j < NotesList.Count; j++)
                {
                    if(NotesList[i].CreationDate > NotesList[j].CreationDate) { 
                            Notefile buff = NotesList[i];
                            NotesList[i] = NotesList[j];
                            NotesList[j] = buff;
                    }
                }
            }

            NotesList.Reverse();
            foreach (Notefile nf in NotesList)
            {
                var bc = new BrushConverter();
                var newButton = new Button() { Content = System.IO.Path.GetFileNameWithoutExtension(nf.Name), Height = 40, FontSize = 20, Foreground = (Brush)bc.ConvertFrom("#dcdde3"), Margin = new Thickness(0, 0, 0, 2), Background = (Brush)bc.ConvertFrom("#5b5b73"), BorderThickness = new Thickness(0, 0, 0, 0) };
                newButton.Click += SelectedNoteButton_Click;

                ButtonFileDict.Add(newButton, nf);
            }
            foreach (var elem in ButtonFileDict)
            {
                if (elem.Value.IsArchived != NotesOpened)
                {
                    Buttons_StackPanel.Children.Add(elem.Key);
                }
            }
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            LiveTimeLabel.Content = DateTime.Now.ToString("HH:mm:ss");
        }
    }
}
