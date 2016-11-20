﻿using MahApps.Metro.Controls;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net.Cache;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace Spectabis_WPF
{
    public partial class MainWindow : MetroWindow
    {

        public string BaseDirectory = AppDomain.CurrentDomain.BaseDirectory;

        public MainWindow()
        {
            InitializeComponent();

            //Catch commandline arguments
            CatchCommandLineArguments();

            //Saves settings between versions
            Properties.Settings.Default.Upgrade();

            copyDLL();


            //Version
            Debug.WriteLine(Assembly.GetExecutingAssembly().GetName().Version);
            Title = "Spectabis " + Assembly.GetExecutingAssembly().GetName().Version;

            //Advanced options ini
            if (File.Exists(BaseDirectory + @"\advanced.ini"))
            {

                //Read values
                var advancedIni = new IniFile(BaseDirectory + @"\advanced.ini");
                int _framerate = Convert.ToInt16(advancedIni.Read("timelineFramerate", "Renderer"));

                //Timeline Framerate
                Timeline.DesiredFrameRateProperty.OverrideMetadata(typeof(Timeline), new FrameworkPropertyMetadata { DefaultValue = _framerate });
            }

            //Hide game settings panel, which is visible in designer
            Open_Settings(false);

            //Sets nightmode from variable
            new PaletteHelper().SetLightDark(Properties.Settings.Default.nightMode);

            //If emuDir is not set, launch first time setup
            if (Properties.Settings.Default.emuDir == "null")
            {
                FirstSetupFrame.Visibility = Visibility.Visible;
            }

            //Open game library page
            mainFrame.Source = new Uri("Library.xaml", UriKind.Relative);


        }

        private void CatchCommandLineArguments()
        {
            //Make alist of all arguments
            List<string> arguments = new List<string>(Environment.GetCommandLineArgs());

            //Force first time setup
            if (arguments.Contains("-firsttime"))
            {
                FirstSetupFrame.Visibility = Visibility.Visible;
            }
        }

        //Hides first time setup frame
        public void HideFirsttimeSetup()
        {
            FirstSetupFrame.Visibility = Visibility.Collapsed;
        }

        //Shows & hides overlay, when appropriate
        private void MenuToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            if(GameSettings.Visibility == Visibility.Visible)
            {
                MenuToggleButton.IsChecked = false;
                return;
            }
            sideMenu.Visibility = Visibility.Visible;
            Overlay(true);
        }

        private void MenuToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            if (GameSettings.Visibility == Visibility.Visible)
            {
                MenuToggleButton.IsChecked = false;
                return;
            }
            sideMenu.Visibility = Visibility.Collapsed;
            Overlay(false);
        }

        //Show or hide black overlay
        public void Overlay(bool _show)
        {
            if (_show == true)
            {
                overlay.Opacity = .5;
                overlay.IsEnabled = true;
                overlay.IsHitTestVisible = true;
            }
            else
            {
                overlay.Opacity = 0;
                overlay.IsEnabled = false;
                overlay.IsHitTestVisible = false;
                MenuToggleButton.IsChecked = false;
            }
        }

        //Menu - Library Button
        private void Menu_Library_Click(object sender, RoutedEventArgs e)
        {
            mainFrame.Source = new Uri("Library.xaml", UriKind.Relative);
            MainWindow_Header.Text = "Library";
            Overlay(false);

        }

        //Menu - Settings Button
        private void Menu_Settings_Click(object sender, RoutedEventArgs e)
        {
            mainFrame.Source = new Uri("Settings.xaml", UriKind.Relative);
            MainWindow_Header.Text = "Settings";
            Overlay(false);
        }

        private void Menu_Credits_Click(object sender, RoutedEventArgs e)
        {

        }

        public void Open_AddGame()
        {
            mainFrame.Source = new Uri("AddGame.xaml", UriKind.Relative);
            MainWindow_Header.Text = "Add Game";
        }

        public void Open_Library()
        {
            mainFrame.Source = new Uri("Library.xaml", UriKind.Relative);
            MainWindow_Header.Text = "Library";
        }

        //Open settings sidewindow
        //Bool true, to show - false to hide
        public void Open_Settings(bool e, [Optional] string _name)
        {
            if(e == true)
            {
                string _cfgDir = BaseDirectory + @"\resources\configs\" + _name;

                //Reads the values from Spectabis ini
                var gameIni = new IniFile(_cfgDir + @"\spectabis.ini");
                var _nogui = gameIni.Read("nogui", "Spectabis");
                var _fullscreen = gameIni.Read("fullscreen", "Spectabis");
                var _fullboot = gameIni.Read("fullboot", "Spectabis");
                var _nohacks = gameIni.Read("nohacks", "Spectabis");
                var _isodir = gameIni.Read("isoDirectory", "Spectabis");

                //Sets the values from spectabis ini
                if (_nogui == "1") { nogui.IsChecked = true; } else { nogui.IsChecked = false; }
                if (_fullscreen == "1") { fullscreen.IsChecked = true; } else {fullscreen.IsChecked = false; }
                if (_fullboot == "1") { fullboot.IsChecked = true; } else { fullboot.IsChecked = false; }
                if (_nohacks == "1") { nohacks.IsChecked = true; } else { nohacks.IsChecked = false; }

                //Reads PCSX2_vm file
                var vmIni = new IniFile(_cfgDir + @"\PCSX2_vm.ini");
                var _widescreen = vmIni.Read("EnableWideScreenPatches", "EmuCore");

                //Sets the values from PCSX2_vm ini
                if (_widescreen == "enabled") { widescreen.IsChecked = true; } else { widescreen.IsChecked = false; }

                //GDSX file mipmap hack
                var gsdxIni = new IniFile(_cfgDir + @"\GSdx.ini");
                var _mipmaphack = gsdxIni.Read("UserHacks_mipmap", "Settings");
                if (_mipmaphack == "1") { hwmipmap.IsChecked = true; } else { hwmipmap.IsChecked = false; }


                //Reads the PCSX2_ui ini file
                var uiIni = new IniFile(_cfgDir + @"\PCSX2_ui.ini");
                var _zoom = uiIni.Read("Zoom", "GSWindow");
                var _aspectratio = uiIni.Read("AspectRatio", "GSWindow");

                //Read aspect ratio
                //Create a list of all the aspect ratios and add them to aspectratio combobox
                List<string> aspectRatios = new List<string>();
                aspectRatios.Add("Letterbox");
                aspectRatios.Add("Widescreen");
                aspectRatios.Add("Stretched");
                aspectratio.ItemsSource = aspectRatios;

                if (_aspectratio == "4:3")
                {
                    aspectratio.SelectedIndex = 0;
                }
                else if (_aspectratio == "16:9")
                {
                    aspectratio.SelectedIndex = 1;
                }
                else
                {
                    aspectratio.SelectedIndex = 2;
                }

                //Set zoom level to textbox
                zoom.Text = _zoom;


                //Show the panel and overlay
                Overlay(true);
                GameSettings.Visibility = Visibility.Visible;

                //Set image and header text for the game
                Header_title.Text = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(_name);
                GameSettings_Header.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(BaseDirectory + @"\resources\configs\" + _name + @"\art.jpg"));
            }
            else
            {
                //Hide panel
                Overlay(false);
                GameSettings.Visibility = Visibility.Collapsed;
            }
        }

        //Close Game Settings button click
        private void CloseSettings_Button(object sender, RoutedEventArgs e)
        {
            //Save settings, take name from header text
            SaveGameSettings(Header_title.Text);

            //Hide panel
            Overlay(false);
            GameSettings.Visibility = Visibility.Collapsed;
        }

        //Change boxart
        private void ChangeArt_Button(object sender, RoutedEventArgs e)
        {
            Ookii.Dialogs.Wpf.VistaOpenFileDialog artBrowser = new Ookii.Dialogs.Wpf.VistaOpenFileDialog();
            artBrowser.Filter = "JPEG Files (*.jpg)|*.jpg|PNG Files (*.png)|*.png";
            artBrowser.Title = "Browse to a new boxart image";

            var browserResult = artBrowser.ShowDialog();
            if(browserResult == true)
            {
                string _file = artBrowser.FileName;
                string _game = Header_title.Text;

                //Url files don't get filtered, so let's just not break the game profile and stop, if 
                //selected file is indeed a url file
                if(_file.Contains(".url"))
                {
                    Debug.WriteLine("File was URL, returning.");
                    return;
                }

                //Replace the boxart image
                File.Copy(_file, BaseDirectory + @"\resources\configs\" + _game + @"\art.jpg", true);

                //Reload the image in header
                System.Windows.Media.Imaging.BitmapImage artSource = new System.Windows.Media.Imaging.BitmapImage();
                artSource.BeginInit();
                artSource.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.None;
                artSource.UriCachePolicy = new RequestCachePolicy(RequestCacheLevel.BypassCache);
                artSource.CreateOptions = System.Windows.Media.Imaging.BitmapCreateOptions.IgnoreImageCache;
                artSource.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;
                artSource.UriSource = new Uri(BaseDirectory + @"\resources\configs\" + _game + @"\art.jpg");
                artSource.EndInit();
                GameSettings_Header.Source = artSource;

                //Reload game library
                mainFrame.NavigationService.Refresh();

            }


        }

        private void SaveGameSettings(string _name)
        {
            //Create instances for every ini file to save
            var gameIni = new IniFile(AppDomain.CurrentDomain.BaseDirectory + @"\resources\configs\" + _name + @"\spectabis.ini");
            var uiIni = new IniFile(AppDomain.CurrentDomain.BaseDirectory + @"\resources\configs\" + _name + @"\PCSX2_ui.ini");
            var vmIni = new IniFile(AppDomain.CurrentDomain.BaseDirectory + @"\resources\configs\" + _name + @"\PCSX2_vm.ini");
            var gsdxIni = new IniFile(AppDomain.CurrentDomain.BaseDirectory + @"\resources\configs\" + _name + @"\GSdx.ini");

            //Emulation Settings - written to spectabis ini
            if (nogui.IsChecked == true)
            {
                gameIni.Write("nogui", "1", "Spectabis");
            }
            else
            {
                gameIni.Write("nogui", "0", "Spectabis");
            }

            if (fullscreen.IsChecked == true)
            {
                gameIni.Write("fullscreen", "1", "Spectabis");
            }
            else
            {
                gameIni.Write("fullscreen", "0", "Spectabis");
            }

            if (fullboot.IsChecked == true)
            {
                gameIni.Write("fullboot", "1", "Spectabis");
            }
            else
            {
                gameIni.Write("fullboot", "0", "Spectabis");
            }

            if (nohacks.IsChecked == true)
            {
                gameIni.Write("nohacks", "1", "Spectabis");
            }
            else
            {
                gameIni.Write("nohacks", "0", "Spectabis");
            }

            //Widescreen patch - written to pcsx2_vm
            if (widescreen.IsChecked == true)
            {
                vmIni.Write("EnableWideScreenPatches", "enabled", "EmuCore");
            }
            else
            {
                vmIni.Write("EnableWideScreenPatches", "disabled", "EmuCore");
            }

            //Mipmap hack - written to gsdx.ini
            if (hwmipmap.IsChecked == true)
            {
                gsdxIni.Write("UserHacks_mipmap", "1", "Settings");
                gsdxIni.Write("UserHacks", "1", "Settings");
            }
            else
            {
                gsdxIni.Write("UserHacks_mipmap", "0", "Settings");
            }

            //Aspect ratio - written to PCSX2_ui ini
            if(aspectratio.SelectedIndex == 0)
            {
                uiIni.Write("AspectRatio", "4:3", "GSWindow");
            }
            else if(aspectratio.SelectedIndex == 1)
            {
                uiIni.Write("AspectRatio", "16:9", "GSWindow");
            }
            else
            {
                uiIni.Write("AspectRatio", "Stretch", "GSWindow");
            }

            //Zoom level - writeen to PCSX2-ui ini
            uiIni.Write("Zoom", zoom.Text, "GSWindow");
        }

        //Search PCSX2 wiki button
        private void SearchWiki_Button(object sender, RoutedEventArgs e)
        {
            //Take the header title and replace spaces with + sign
            string _query = Header_title.Text;
            _query = _query.Replace(" ", "+");

            //Open up PCSX2 wiki
            Process.Start(@"http://wiki.pcsx2.net/index.php?search=" + _query);
        }

        //Copy dll from emulator plugins
        private static void copyDLL()
        {
            string emuDir = Properties.Settings.Default.emuDir;
            Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + @"\plugins\");
            //Checks and copies needed plugin files from emulator directory, if they exist

            if (File.Exists(emuDir + @"\plugins\LilyPad.dll"))
            {
                File.Copy(emuDir + @"\plugins\LilyPad.dll", AppDomain.CurrentDomain.BaseDirectory + @"\plugins\LilyPad.dll", true);
            }

            if (File.Exists(emuDir + @"\plugins\GSdx32-SSE2.dll"))
            {
                File.Copy(emuDir + @"\plugins\GSdx32-SSE2.dll", AppDomain.CurrentDomain.BaseDirectory + @"\plugins\GSdx32-SSE2.dll", true);
            }

            if (File.Exists(emuDir + @"\plugins\Spu2-X.dll"))
            {
                File.Copy(emuDir + @"\plugins\Spu2-X.dll", AppDomain.CurrentDomain.BaseDirectory + @"\plugins\Spu2-X.dll", true);
            }
        }

        //Imports GPUconfigure from GSdx plugin
        //All GSdx plugins have same settings, by the looks of it
        //It has no inputs, but writes/reads the ini files where .exe is located at folder /inis/
        [DllImport(@"\plugins\GSdx32-SSE2.dll")]
        static public extern void GSconfigure();

        //Configuration must be closed so .dll is not in use
        [DllImport(@"\plugins\GSdx32-SSE2.dll")]
        static public extern void GSclose();

        //Video Settings button
        private void VideoSettings_click(object sender, RoutedEventArgs e)
        {
            //Set currentgame from header title text
            string currentGame = Header_title.Text;

            if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + @"\resources\configs\" + currentGame + @"\GSdx.ini"))
            {
                //Creates inis folder and copies it from game profile folder
                Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + @"inis");
                File.Copy(AppDomain.CurrentDomain.BaseDirectory + @"\resources\configs\" + currentGame + @"\GSdx.ini", AppDomain.CurrentDomain.BaseDirectory + @"inis\GSdx.ini", true);
            }

            //GPUConfigure(); - Only software mode was available
            GSconfigure();
            GSclose();

            File.Copy(AppDomain.CurrentDomain.BaseDirectory + @"inis\GSdx.ini", AppDomain.CurrentDomain.BaseDirectory + @"\resources\configs\" + currentGame + @"\GSdx.ini", true);
            Directory.Delete(AppDomain.CurrentDomain.BaseDirectory + @"inis", true);
        }


        [DllImport(@"\plugins\Spu2-X.dll")]
        static public extern void SPU2configure();

        [DllImport(@"\plugins\Spu2-X.dll")]
        static public extern void SPU2close();

        //Audio Settings button
        private void AudioSettings_click(object sender, RoutedEventArgs e)
        {
            string currentGame = Header_title.Text;

            if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + @"\resources\configs\" + currentGame + @"\SPU2-X.ini"))
            {
                //Creates inis folder and copies it from game profile folder
                Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + @"inis");
                File.Copy(AppDomain.CurrentDomain.BaseDirectory + @"\resources\configs\" + currentGame + @"\SPU2-X.ini", AppDomain.CurrentDomain.BaseDirectory + @"inis\SPU2-X.ini", true);
            }

            SPU2configure();
            SPU2close();

            File.Copy(AppDomain.CurrentDomain.BaseDirectory + @"inis\SPU2-X.ini", AppDomain.CurrentDomain.BaseDirectory + @"\resources\configs\" + currentGame + @"\SPU2-X.ini", true);
            Directory.Delete(AppDomain.CurrentDomain.BaseDirectory + @"inis", true);
        }


        [DllImport(@"\plugins\LilyPad.dll")]
        static public extern void PADconfigure();

        //Configuration must be closed so .dll is not in use
        [DllImport(@"\plugins\LilyPad.dll")]
        static public extern void PADclose();

        private void InputSettings_click(object sender, RoutedEventArgs e)
        {
            string currentGame = Header_title.Text;

            //Copy the existing .ini file for editing if it exists
            if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + @"\resources\configs\" + currentGame + @"\LilyPad.ini"))
            {
                //Creates inis folder and copies it from game profile folder
                Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + @"inis");
                File.Copy(AppDomain.CurrentDomain.BaseDirectory + @"\resources\configs\" + currentGame + @"\LilyPad.ini", AppDomain.CurrentDomain.BaseDirectory + @"inis\LilyPad.ini", true);
            }

            //Calls the DLL configuration function
            PADconfigure();

            //Calls the configration close function
            PADclose();

            //Copies the modified file into the game profile & deletes the created folder
            File.Copy(AppDomain.CurrentDomain.BaseDirectory + @"inis\LilyPad.ini", AppDomain.CurrentDomain.BaseDirectory + @"\resources\configs\" + currentGame + @"\LilyPad.ini", true);
            Directory.Delete(AppDomain.CurrentDomain.BaseDirectory + @"inis", true);
        }

        //Public timer, because it needs to stop itself
        public System.Windows.Threading.DispatcherTimer timeTimer = new System.Windows.Threading.DispatcherTimer();

        //Because labels don't support Click, i'll have to do it on my own
        private void Header_title_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Debug.WriteLine("Clicked on game header!");

            int TimeBetweenClicks = System.Windows.Forms.SystemInformation.DoubleClickTime;

            //If the timer is not yet stopped, that means it's a double click
            //If the timer is not running, it's not a double click
            if (timeTimer.IsEnabled)
            {
                //Hide the real title label
                Header_title.Visibility = Visibility.Collapsed;

                //Set text from label and make the textbox visible
                TitleEditBox.Text = Header_title.Text;
                TitleEditBox.Visibility = Visibility.Visible;

                //Set focus to this textbox
                TitleEditBox.Focus();
                TitleEditBox.CaretIndex = TitleEditBox.Text.Length;
            }
            else
            {
                //Starts the timer, with system double click time as interval (500ms for me)
                timeTimer.Interval = new TimeSpan(0, 0, 0, 0, TimeBetweenClicks);
                timeTimer.Tick += timeTimer_Tick;
                timeTimer.Start();

                Debug.WriteLine("Started timer - after ms" + TimeBetweenClicks);
            }
        }

        //Stop double-click timer on first tick
        private void timeTimer_Tick(object sender, EventArgs e)
        {
            Debug.WriteLine("Tick!");
            timeTimer.Stop();
        }

        //When created textbox loeses focus, show the real label and don't save changes
        private void TitleEditBox_LostFocus(object sender, RoutedEventArgs e)
        {
            //Hide the textbox
            TitleEditBox.Visibility = Visibility.Collapsed;

            //Show real label
            Header_title.Visibility = Visibility.Visible;
        }

        //Catch created textbox key presses
        private void TitleEditBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            //Remove unsupported characters
            TitleEditBox.Text = TitleEditBox.Text.Replace(@"/", string.Empty);
            TitleEditBox.Text = TitleEditBox.Text.Replace(@"\", string.Empty);
            TitleEditBox.Text = TitleEditBox.Text.Replace(@":", string.Empty);
            TitleEditBox.Text = TitleEditBox.Text.Replace(@"|", string.Empty);
            TitleEditBox.Text = TitleEditBox.Text.Replace(@"*", string.Empty);
            TitleEditBox.Text = TitleEditBox.Text.Replace(@"<", string.Empty);
            TitleEditBox.Text = TitleEditBox.Text.Replace(@">", string.Empty);

            //This is the only way to save the name
            if (e.Key == Key.Enter)
            {

                //Save old name to a variable
                string _oldName = Header_title.Text;
                string _newName;

                //Hide the textbox
                TitleEditBox.Visibility = Visibility.Collapsed;

                //Pass text to label
                Header_title.Text = TitleEditBox.Text;

                //Show real label
                Header_title.Visibility = Visibility.Visible;

                //Save new name to the variable
                _newName = Header_title.Text;

                //Check, if old directory exists
                if(Directory.Exists(BaseDirectory + @"\resources\configs\" + _oldName))
                {
                    try
                    {
                        //Move old folder to new folder
                        Directory.Move(BaseDirectory + @"\resources\configs\" + _oldName, BaseDirectory + @"\resources\configs\" + _newName);

                        //Reload game library
                        mainFrame.NavigationService.Refresh();
                    }
                    catch
                    {
                        Debug.WriteLine("Couldn't rename the folder");
                    }
                }
            }

            if (e.Key == Key.Escape)
            {
                //Hide the textbox
                TitleEditBox.Visibility = Visibility.Collapsed;

                //Show real label
                Header_title.Visibility = Visibility.Visible;
            }
        }
    }
}