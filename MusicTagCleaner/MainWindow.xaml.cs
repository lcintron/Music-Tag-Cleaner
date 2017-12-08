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
using System.Windows.Forms;
using System.IO;
using System.Threading;

namespace MusicTagCleaner
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string musicDirectory = "";
        private List<string> musicFiles = new List<string>();
        private List<string> phrasesToRemove = new List<string>();

        public bool RecursiveDirectories { get; set; }
        

        public MainWindow()
        {
            InitializeComponent();
        }

        private void OnBtnGetDirPathClick(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.ShowNewFolderButton = false;
            DialogResult result =  dialog.ShowDialog();
            if(result == System.Windows.Forms.DialogResult.OK)
            {
                musicDirectory = dialog.SelectedPath;
                txtMusicDirectory.Text = musicDirectory;
                RecursiveDirectories = cbRecursiveDirs.IsChecked ?? cbRecursiveDirs.IsChecked.Value;
                int filesFound  = PreProcessSelectedDirectory(dialog.SelectedPath);
                txtFilesFound.Text = String.Format("{0} Music Files Found!", filesFound);
                txtFilesFound.Visibility = filesFound>0? Visibility.Visible:Visibility.Hidden;
            }
        }

        private void OnBtnProcessFilesClick(object sender, RoutedEventArgs e)
        {
            string[] phrasesToRemoveArray = txtTextToRemove.Text.Split('\n');
            phrasesToRemove.AddRange(phrasesToRemoveArray);
            RecursiveDirectories = cbRecursiveDirs.IsChecked??cbRecursiveDirs.IsChecked.Value;
            Thread t = new Thread(new ThreadStart(ProcessFiles));
            t.IsBackground = true;
            t.Start();
            //throw new NotImplementedException();
        }

        private void ProcessFiles()
        {
            PreProcessSelectedDirectory(musicDirectory);
            foreach (string file in musicFiles)
            {
                FileInfo fi = new FileInfo(file);
                TagLib.File f = TagLib.File.Create(file);
                string origFileName = fi.Name;
                string cleanFileName = fi.Name;
                foreach (string phrase in phrasesToRemove)
                {
                    char[] charsToTrim = { ' ', '\t', '\r' };

                    string cleanphrase = phrase.Trim(charsToTrim);

                    cleanFileName = cleanFileName.Replace(cleanphrase, String.Empty);

                    if (f.Tag.Title != null && f.Tag.Title.Contains(cleanphrase))
                    {
                        f.Tag.Title = f.Tag.Title.Replace(cleanphrase, String.Empty);
                    }
                    if (f.Tag.Comment != null && f.Tag.Comment.Contains(cleanphrase))
                    {
                        f.Tag.Comment = f.Tag.Comment.Replace(cleanphrase, String.Empty);
                    }

                    for (int i = 0; i < f.Tag.AlbumArtists.Length; i++)
                    {
                        if (f.Tag.AlbumArtists[i].Contains(cleanphrase))
                        {
                            f.Tag.AlbumArtists[i] = f.Tag.AlbumArtists[i].Replace(cleanphrase, String.Empty);
                        }
                    }

                    if (f.Tag.Album != null && f.Tag.Album.Contains(cleanphrase))
                    {
                        f.Tag.Album = f.Tag.Album.Replace(cleanphrase, String.Empty);
                    }

                    for (int i = 0; i < f.Tag.Composers.Length; i++)
                    {
                        if (f.Tag.Composers[i].Contains(cleanphrase))
                        {
                            f.Tag.Composers[i] = f.Tag.Composers[i].Replace(cleanphrase, String.Empty);
                        }
                    }
                }

                f.Save();

                if (!origFileName.Equals(cleanFileName))
                {
                    File.Move(file, file.Replace(origFileName, cleanFileName));
                }
            }
        }

        private void CleanElement(ref string element, string cleanphrase)
        {
            if(element!=null && element.ToLower().Contains(cleanphrase.ToLower()))
            {
                int phraseIndex = element.ToLower().IndexOf(cleanphrase, 0, StringComparison.CurrentCultureIgnoreCase);
                string phraseToReplace = element.Substring(phraseIndex, cleanphrase.Length);
                element.Replace(phraseToReplace, String.Empty);
                CleanElement(ref element, cleanphrase);
            }
        }

        //private static String CleanElement(this String element,  string cleanphrase)
        //{
        //    if (element != null && element.ToLower().Contains(cleanphrase.ToLower()))
        //    {
        //        int phraseIndex = element.ToLower().IndexOf(cleanphrase, 0, StringComparison.CurrentCultureIgnoreCase);
        //        string phraseToReplace = element.Substring(phraseIndex, cleanphrase.Length);
        //        element.Replace(phraseToReplace, String.Empty);
        //    }
        //    return element;
        //}

        private int PreProcessSelectedDirectory(string selectedPath)
        {
            musicFiles.Clear();
            musicDirectory = selectedPath;
            string[] files = Directory.GetFiles(musicDirectory, "*", RecursiveDirectories?SearchOption.AllDirectories:SearchOption.TopDirectoryOnly);
            foreach(string file in files)
            {
                FileInfo fileInfo = new FileInfo(file);
                if(fileInfo.Extension.ToLower() == ".mp3" || fileInfo.Extension.ToLower() == ".wav" || fileInfo.Extension.ToLower() == ".mp4" || fileInfo.Extension.ToLower().Equals(".m4a"))
                {
                    musicFiles.Add(file);
                }
            }
            return musicFiles.Count ;
        }
    }
}
