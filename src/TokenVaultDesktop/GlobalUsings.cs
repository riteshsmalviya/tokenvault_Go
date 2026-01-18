// Resolve namespace conflicts between WPF and Windows Forms
global using Application = System.Windows.Application;
global using UserControl = System.Windows.Controls.UserControl;
global using MessageBox = System.Windows.MessageBox;
global using SaveFileDialog = Microsoft.Win32.SaveFileDialog;
global using OpenFileDialog = Microsoft.Win32.OpenFileDialog;

// Add System.IO for Path, Directory, File
global using Path = System.IO.Path;
global using Directory = System.IO.Directory;
global using File = System.IO.File;
