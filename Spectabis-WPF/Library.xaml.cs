﻿using SevenZipExtractor;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Spectabis_WPF
{
    public partial class Library : Page
    {

        //Spectabis Variables
        public string emuDir = Properties.Settings.Default.emuDir;
        public string GameConfigs;
        public string BaseDirectory = AppDomain.CurrentDomain.BaseDirectory;

        //Lists
        public List<string> regionList = new List<string>();
        public List<string> supportedScrappingFiles = new List<string>();

        public Library()
        {
            InitializeComponent();

            //Where game profile folders are saved
            GameConfigs = BaseDirectory + @"\resources\configs\";

            //Adds supported files for artScrapping to list
            supportedScrappingFiles.Add("iso");

            //Adds known items to region list
            regionList.Add("SLUS");
            regionList.Add("SCUS");
            regionList.Add("SCES");
            regionList.Add("SLES");
            regionList.Add("SCPS");
            regionList.Add("SLPS");
            regionList.Add("SLPM");
            regionList.Add("PSRM");
            regionList.Add("SCED");
            regionList.Add("SLPM");
            regionList.Add("SIPS");


            reloadGames();
        }


        //MouseDown event on boxArt image
        private void boxArt_Click(object sender, MouseButtonEventArgs e)
        {
            Image clickedBoxArt = (Image)sender;
            Debug.WriteLine(clickedBoxArt.Tag + " - clicked");

            //Get isoDir from Spectabis.ini
            //string _cfgDir = BaseDirectory + @"resources\configs\" + clickedBoxArt.Tag;
            string _cfgDir = GameConfigs + @"/" + clickedBoxArt.Tag;

            var _gameIni = new IniFile(_cfgDir + @"\spectabis.ini");
            var _isoDir = _gameIni.Read("isoDirectory", "Spectabis");

            //Save needed click count to variable
            int _ClickCount;
            if(Properties.Settings.Default.doubleClick == true)
            {
                _ClickCount = 2;
            }
            else
            {
                _ClickCount = 1;
            }

            //If right click
            if(e.XButton1 == e.RightButton)
            {
                //Checks for click count
                if(e.ClickCount == _ClickCount)
                {
                    if(File.Exists(_isoDir))
                    {
                        //If game file exists, launch

                        //Launch arguments
                        var _nogui = _gameIni.Read("nogui", "Spectabis");
                        var _fullscreen = _gameIni.Read("fullscreen", "Spectabis");
                        var _fullboot = _gameIni.Read("fullboot", "Spectabis");
                        var _nohacks = _gameIni.Read("nohacks", "Spectabis");

                        string _launchargs = "";

                        if (_nogui == "1") { _launchargs = "--nogui "; }
                        if (_fullscreen == "1") { _launchargs = _launchargs + "--fullscreen "; }
                        if (_fullboot == "1") { _launchargs = _launchargs + "--fullboot "; }
                        if (_nohacks == "1") { _launchargs = _launchargs + "--nohacks "; }

                        Debug.WriteLine(clickedBoxArt.Tag + " launched with commandlines:  " + _launchargs);
                        Debug.WriteLine(clickedBoxArt.Tag + " launched from: " + _isoDir);
                        Debug.WriteLine(emuDir + @"\pcsx2.exe", "" + _launchargs + "\"" + _isoDir + "\" --cfgpath \"" + _cfgDir + "\"");

                        Process.Start(emuDir + @"\pcsx2.exe", "" + _launchargs + "\"" + _isoDir + "\" --cfgpath \"" + _cfgDir + "\"");

                    }
                    else
                    {
                        Debug.WriteLine(_isoDir + " does not exist!");
                    }
                }
            }

            //If left click
            if(e.XButton1 == e.LeftButton)
            {
                //code
            }



        }

        //Rescans the game config directory and adds them to gamePanel
        private void reloadGames()
        {
            if (Directory.Exists(GameConfigs))
            {
                //Makes a collection of game folders from game config directory
                string[] _gamesdir = Directory.GetDirectories(GameConfigs);

                //Loops through each folder in game config directory
                foreach (string game in _gamesdir)
                {
                    if (File.Exists(game + @"\Spectabis.ini"))
                    {
                        //Sets _gameName to name of the folder
                        string _gameName = game.Remove(0, game.LastIndexOf(System.IO.Path.DirectorySeparatorChar) + 1);

                        Debug.WriteLine("adding to gamePanel - " + _gameName);

                        //Creates an image object
                        Image boxArt = new Image();
                        boxArt.Source = new ImageSourceConverter().ConvertFromString(game + @"\art.jpg") as ImageSource;
                        boxArt.Height = 200;
                        boxArt.Width = 150;
                        boxArt.MouseDown += boxArt_Click;
                        boxArt.Tag = _gameName;

                        //If showtitle is selected
                        if (Properties.Settings.Default.showTitle == true)
                        {
                            GroupBox gameTile = new GroupBox();
                            gameTile.Content = boxArt;
                            gameTile.Header = _gameName;
                            gamePanel.Children.Add(gameTile);
                        }
                        else
                        {
                            //Adds the object to gamePanel
                            gamePanel.Children.Add(boxArt);
                        }
                    }
                }
            }

            Directory.CreateDirectory(GameConfigs);

        }


        //Push snackbar function
        public void PushSnackbar(string message)
        {
            var messageQueue = Snackbar.MessageQueue;

            //the message queue can be called from any thread
            Task.Factory.StartNew(() => messageQueue.Enqueue(message));
        }

        //Add game method call wit
        public void addGame(string _img, string _isoDir, string _title)
        {
            //Creates a folder for game
            Directory.CreateDirectory(BaseDirectory + @"\resources\configs\" + _title);

            //copies existing pcsx2 inis to added game
            //looks for inis in pcsx2 directory
            if (Directory.Exists(emuDir + @"\inis\"))
            {
                string[] inisDir = Directory.GetFiles(emuDir + @"\inis\");
                foreach (string inifile in inisDir)
                {
                    Debug.WriteLine(inifile + " found!");
                    if (File.Exists(BaseDirectory + @"\resources\configs\" + _title + @"\" + Path.GetFileName(inifile)) == false)
                    {
                        string _destinationPath = Path.Combine(BaseDirectory + @"\resources\configs\" + _title + @"\" + Path.GetFileName(inifile));
                        File.Copy(inifile, _destinationPath);
                    }
                }
            }
            else
            {
                //looks for pcsx2 inis in documents folder
                if (Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\PCSX2\inis"))
                {
                    string[] inisDirDoc = Directory.GetFiles((Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\PCSX2\inis"));
                    foreach (string inifile in inisDirDoc)
                    {
                        if (File.Exists(BaseDirectory + @"\resources\configs\" + _title + @"\" + Path.GetFileName(inifile)) == false)
                        {
                            string _destinationPath = Path.Combine(BaseDirectory + @"\resources\configs\" + _title + @"\" + Path.GetFileName(inifile));
                            File.Copy(inifile, _destinationPath);
                        }
                    }
                }

                //if no inis are found, warning is shown
                else
                {
                    PushSnackbar("Cannot find default PCSX2 configuration");
                }
            }

            using (WebClient client = new WebClient())
            {
                try
                {
                    client.DownloadFile(_img, BaseDirectory + @"\resources\configs\" + _title + @"\art.jpg");
                }
                catch
                {
                    //throw;
                    PushSnackbar("Image for " + _title + " not set");
                }
            }

            var gameIni = new IniFile(BaseDirectory + @"\resources\configs\" + _title + @"\spectabis.ini");
            gameIni.Write("isoDirectory", _isoDir, "Spectabis");
            gameIni.Write("nogui", "0", "Spectabis");
            gameIni.Write("fullscreen", "0", "Spectabis");
            gameIni.Write("fullboot", "0", "Spectabis");

            PushSnackbar(_title + " added succesfully");
            reloadGames();
        }

        //Dragging file effect
        private void Grid_DragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.All;
        }

        //Drag and drop functionality
        private void Grid_Drop(object sender, System.Windows.DragEventArgs e)
        {

        }

        //Get serial number for then given file
        public string GetSerialNumber(string _isoDir)
        {
            string _filename;
            string gameserial = "NULL";

            //Open file as archive
            using (ArchiveFile archiveFile = new ArchiveFile(_isoDir))
            {
                //loop all files in archive
                foreach (Entry entry in archiveFile.Entries)
                {
                    _filename = new string(entry.FileName.Take(4).ToArray());

                    //if any files contains a region code, stucturize and return it
                    if (regionList.Contains(_filename))
                    {
                        gameserial = entry.FileName.Replace(".", String.Empty);
                        gameserial = gameserial.Replace("_", "-");

                        Console.WriteLine("Serial = " + gameserial);

                        return gameserial;
                    }
                    else
                    {
                        return gameserial;
                    }
                }
                return gameserial;
            }
        }

        //Returns a game name, using PCSX2 database file
        public string GetGameName(string _gameserial)
        {
            string GameIndex = emuDir + @"\GameIndex.dbf";
            string GameName = "UNKNOWN";

            //Reads the GameIndex file by line
            using (var reader = new StreamReader(GameIndex))
            {
                
                bool serialFound = false;
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();

                    //Forges a GameIndex.dbf entry
                    //If forged line appears in GameIndex.dbf stop and read the next line
                    if (line.Contains("Serial = " + _gameserial))
                    {
                        serialFound = true;
                    }
                    //The next line which contains name associated with gameserial
                    else if (serialFound == true)
                    {
                        //Cleans the data
                        GameName = line.Replace("Name   = ", String.Empty);
                        return GameName;
                    }
                }
                return GameName;
            }
        }

    }
}
