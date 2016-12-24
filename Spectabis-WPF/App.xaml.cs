﻿using System.Windows;

namespace Spectabis_WPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
	    public App(){
		    return;

			//Copy spinner.gif to temporary files
			Spectabis_WPF.Properties.Resources.spinner.Save(BaseDirectory + @"resources\_temp\spinner.gif");
	    }

		public string BaseDirectory = System.AppDomain.CurrentDomain.BaseDirectory;

    }
}
