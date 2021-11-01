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

namespace Notes_WPF
{
    
    public partial class MainWindow : Window
    {

        Dictionary<Button, Notefile> ButtFileDict = new Dictionary<Button, Notefile>();
        Button LastPressedButton = null;
        string NotesFilePath = @"..\..\..\Notes_text";
        string ArchiveFilePath = @"..\..\..\Archive_notes";
        bool NotesOpened = true;

        public MainWindow()
        {
            InitializeComponent();

            DirectoryInfo NotesDir = new DirectoryInfo(NotesFilePath); //choose directory

            FileInfo[] txtFiles = NotesDir.GetFiles("*.txt", SearchOption.AllDirectories); //Get all info about .txt files from chosen directory

            var bc = new BrushConverter();

            foreach (FileInfo file in txtFiles)
            {
                //add file data to list
                Notefile newNote = new Notefile();
                newNote.Path = file.FullName;
                newNote.Name = file.Name;
                newNote.CreatiionDate = file.CreationTime;
                newNote.isArchived = false;
                //NoteFilesList.Add(newNote);

                string nameWithoutExt = System.IO.Path.GetFileNameWithoutExtension(newNote.Name);    //get file name without any extentions (.txt)
                var newButton = new Button() { Content = nameWithoutExt, FontSize = 20, Height = 40, Foreground = (Brush)bc.ConvertFrom("#dcdde3"), Margin = new Thickness(0, 0, 0, 2), Background = (Brush)bc.ConvertFrom("#5b5b73"), BorderThickness = new Thickness(0, 0, 0, 0) };
                newButton.Click += SelectedNoteButton_Click;
                ButtFileDict.Add(newButton, newNote);   //add button and file to dict
                Buttons_StackPanel.Children.Add(newButton); //add new button to StackPanel
            }

            DirectoryInfo ArchivesDirectory = new DirectoryInfo(ArchiveFilePath);
            FileInfo[] ArchFiles = ArchivesDirectory.GetFiles("*.txt", SearchOption.AllDirectories);

            foreach(FileInfo file in ArchFiles)
            {
                Notefile newNote = new Notefile();
                newNote.Path = file.FullName;
                newNote.Name = file.Name;
                newNote.CreatiionDate = file.CreationTime;
                newNote.isArchived = true;

                string nameWithoutExt = System.IO.Path.GetFileNameWithoutExtension(newNote.Name);    //get file name without any extentions (.txt)
                var newButton = new Button() { Content = nameWithoutExt, FontSize = 20, Height = 40, Foreground = (Brush)bc.ConvertFrom("#dcdde3"), Margin = new Thickness(0, 0, 0, 2), Background = (Brush)bc.ConvertFrom("#5b5b73"), BorderThickness = new Thickness(0, 0, 0, 0) };
                newButton.Click += SelectedNoteButton_Click;
                ButtFileDict.Add(newButton, newNote);   //add button and file to dict
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
            var bc = new BrushConverter();
            var newButton = new Button() { Content = "", Height = 40, FontSize = 20, Foreground = (Brush)bc.ConvertFrom("#dcdde3") ,Margin = new Thickness(0, 0, 0, 2), Background = (Brush)bc.ConvertFrom("#5b5b73"), BorderThickness = new Thickness(0, 0, 0, 0) };
            newButton.Click += SelectedNoteButton_Click;
            Buttons_StackPanel.Children.Add(newButton); //add button to stackpanel
            Buttons_ScrollViewer.ScrollToBottom();

            //creating new file
            string NewFilePath = NotesFilePath + "\\" + " " + ".txt";
            var newFile = File.Create(NewFilePath);
            newFile.Close();
            Notefile nf = new Notefile();
            nf.Path = NewFilePath;
            FileInfo NewFileInfo = new FileInfo(NewFilePath);
            nf.Name = NewFileInfo.Name;
            nf.CreatiionDate = NewFileInfo.CreationTime;
            nf.isArchived = false;
            
            ButtFileDict.Add(newButton, nf);       //add new file and button to the dict
            SelectedNoteButton_Click(newButton, null);
            Filename_TextBox.Focus();
            Filename_TextBox.CaretIndex = Filename_TextBox.Text.Length;
        }

        private void SelectedNoteButton_Click(object sender, RoutedEventArgs e)
        {
            ButtFileDict.TryGetValue((Button)sender, out Notefile nf);                 //get file connected to this button

            Edit_TextBox.Text = nf.ReadFromFile();                                          //change textbox text equal to file text
            LastPressedButton = (Button)sender;
            Filename_TextBox.Text = System.IO.Path.GetFileNameWithoutExtension(nf.Name);    //edit textbox text equal to file name
            CreationData_label.Content = nf.CreatiionDate;                                  //changing date info label text equal to file creation data
            Filename_TextBox.IsEnabled = true;
            Filename_TextBox.CaretIndex = 0;
            Edit_TextBox.Focus();
            Edit_TextBox.CaretIndex = Edit_TextBox.Text.Length;
        }

        private void Edit_TextBox_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (LastPressedButton != null && ButtFileDict.TryGetValue(LastPressedButton, out Notefile nf))
            {                
                nf.WriteToFile(Edit_TextBox.Text);
            }
        }

        private void Filename_TextBox_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            int SameNameCounter = 0;
            if (LastPressedButton != null || NotesOpened)
            {
                ButtFileDict.TryGetValue(LastPressedButton, out Notefile nf);//getting file that is connected to a button
                foreach (KeyValuePair<Button, Notefile> pair in ButtFileDict)
                {
                    if (Filename_TextBox.Text.Trim().ToLower() + ".txt" == pair.Value.Name.ToLower())
                    {
                        SameNameCounter++;
                    }
                }

                
                if (Filename_TextBox.Text.Trim() == "" && Edit_TextBox.Text.Trim() == "")
                {
                    DeleteLastNote();
                    Trace.WriteLine("123");
                }
                else if (Filename_TextBox.Text.Trim() == "" && Edit_TextBox.Text != null)
                {
                    MessageBox.Show("Enter name for your file");
                    Filename_TextBox.Focus();
                    Filename_TextBox.CaretIndex = Filename_TextBox.Text.Length;

                }
                else if (SameNameCounter > 1 || Filename_TextBox.Text == "")
                {
                    MessageBox.Show("File with this name already exists!");
                    Filename_TextBox.Text = System.IO.Path.GetFileNameWithoutExtension(nf.Name);
                    Filename_TextBox.Focus();
                    Filename_TextBox.CaretIndex = Filename_TextBox.Text.Length;
                    Trace.Write('2');
                }
                else {
                    LastPressedButton.Content = Filename_TextBox.Text.Trim();
                    string ParentPath = nf.Path.Remove(nf.Path.Length - nf.Name.Length - 1);    //getting file parent path

                    //changing file name
                    File.Move(nf.Path, ParentPath + "\\" + Filename_TextBox.Text.Trim() + ".txt");
                    if (nf.Name != Filename_TextBox.Text.Trim() + ".txt")
                    {
                        File.Delete(nf.Path);                                                       //deleting old file
                    }
                    nf.Name = Filename_TextBox.Text.Trim() + ".txt";
                    nf.Path = ParentPath + "\\" + nf.Name;
                    Trace.Write('3');
                }
            }
        }

        private void NotesAndArchive_Button_Click(object sender, RoutedEventArgs e)
        {
            if (NotesOpened)
            {
                //archive
                NotesOpened = false;
                NotesAndArchive_Button.Content = "Archive";
                AddNote_Button.Visibility = Visibility.Hidden;
                Filename_TextBox.IsReadOnly = true;
                Edit_TextBox.IsReadOnly = true;
                Buttons_StackPanel.Children.Clear();

                foreach (KeyValuePair<Button, Notefile> pair in ButtFileDict)
                {
                    if (pair.Value.isArchived == true)
                    {
                        Buttons_StackPanel.Children.Add(pair.Key);
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
                Buttons_StackPanel.Children.Clear();

                foreach (KeyValuePair<Button, Notefile> pair in ButtFileDict)
                {
                    if(pair.Value.isArchived == false)
                    {
                        Buttons_StackPanel.Children.Add(pair.Key);
                    }
                }
            }

            Edit_TextBox.Text = "";
            Filename_TextBox.Text = "";
            CreationData_label.Content = "";
            LastPressedButton = null;
        }

        private void DeleteNote_Button_Click(object sender, RoutedEventArgs e)
        {
            DeleteLastNote();
        }

        private void SendToAcrhive_Button_Click(object sender, RoutedEventArgs e)
        {
            if(LastPressedButton != null)
            {
                ButtFileDict.TryGetValue(LastPressedButton, out Notefile nf);//getting file that is connected to a button
                if (nf.isArchived == false) {
                    ButtFileDict[LastPressedButton].isArchived = true;
                    File.Move(nf.Path, ArchiveFilePath + @"\" + nf.Name);
                    ButtFileDict[LastPressedButton].Path = ArchiveFilePath + @"\" + nf.Name;
                }
                else //if true
                {
                    ButtFileDict[LastPressedButton].isArchived = false;
                    File.Move(nf.Path, NotesFilePath + @"\" + nf.Name);
                    ButtFileDict[LastPressedButton].Path = NotesFilePath + @"\" + nf.Name;
                }

                Buttons_StackPanel.Children.Remove(LastPressedButton);
                Edit_TextBox.Text = "";
                Filename_TextBox.Text = "";
                CreationData_label.Content = "";
            }
        }

        private void AlphabetSort_Button_Click(object sender, RoutedEventArgs e)
        {
            Buttons_StackPanel.Children.Clear();

            List<Notefile> NotesList = new List<Notefile>();
            foreach(KeyValuePair<Button, Notefile> pair in ButtFileDict)
            {
                if (pair.Value.isArchived != NotesOpened)
                {
                    NotesList.Add(pair.Value);
                    ButtFileDict.Remove(pair.Key);
                }
            }

            for(int i = 0; i < NotesList.Count; i++)
            {
                for(int j = i + 1; j < NotesList.Count; j++)
                {
                    switch(string.Compare(NotesList[i].Name, NotesList[j].Name, StringComparison.CurrentCultureIgnoreCase))
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

                ButtFileDict.Add(newButton, nf);
            }
            foreach(KeyValuePair<Button, Notefile> pair in ButtFileDict)
            {
                if(pair.Value.isArchived != NotesOpened)
                {
                    Buttons_StackPanel.Children.Add(pair.Key);
                }
            }
            
            
            
        }

        private void CreationDateSort_Button_Click(object sender, RoutedEventArgs e)
        {
            Buttons_StackPanel.Children.Clear();

            List<Notefile> NotesList = new List<Notefile>();
            foreach (KeyValuePair<Button, Notefile> pair in ButtFileDict)
            {
                if (pair.Value.isArchived != NotesOpened)
                {
                    NotesList.Add(pair.Value);
                    ButtFileDict.Remove(pair.Key);
                }
            }

            for (int i = 0; i < NotesList.Count; i++)
            {
                for (int j = i + 1; j < NotesList.Count; j++)
                {
                    if(NotesList[i].CreatiionDate > NotesList[j].CreatiionDate) { 
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

                ButtFileDict.Add(newButton, nf);
            }
            foreach (KeyValuePair<Button, Notefile> pair in ButtFileDict)
            {
                if (pair.Value.isArchived != NotesOpened)
                {
                    Buttons_StackPanel.Children.Add(pair.Key);
                }
            }
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            LiveTimeLabel.Content = DateTime.Now.ToString("HH:mm:ss");
        }

        private void DeleteLastNote()
        {
            if (LastPressedButton != null)
            {
                ButtFileDict.TryGetValue(LastPressedButton, out Notefile nf);//getting file that is connected to a button
                File.Delete(nf.Path);
                Buttons_StackPanel.Children.Remove(LastPressedButton);
                ButtFileDict.Remove(LastPressedButton);
                Edit_TextBox.Text = "";
                Filename_TextBox.Text = "";
                CreationData_label.Content = "";
            }
        }
    }
}
