﻿using Ookii.Dialogs.Wpf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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
using System.Windows.Shapes;

namespace MP3DL
{
    /// <summary>
    /// Interaction logic for FolderDialog.xaml
    /// </summary>
    public partial class FolderDialog : Window
    {
        public FolderDialog()
        {
            InitializeComponent();

            ListControl.ItemsSource = list;
            StringList = new();

            foreach(var directory in ((MainWindow)Application.Current.MainWindow).directories)
            {
                list.Add(new CustomData(directory));
            }
        }
        BindingList<CustomData> list = new();
        public List<string> StringList { get; set; }
        private void ok(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            foreach(var item in list)
            {
                StringList.Add(item.path);
            }
            this.Close();
        }

        private void add(object sender, RoutedEventArgs e)
        {
            VistaFolderBrowserDialog browser = new VistaFolderBrowserDialog();
            if (browser.ShowDialog() == true)
            {
                if (!StringList.Contains(browser.SelectedPath))
                {
                    list.Add(new CustomData(browser.SelectedPath));
                }
            }
        }
        private void folder_clicked(object sender, RoutedEventArgs e)
        {
            Button temp = sender as Button;
            Process.Start("explorer.exe", @temp.Content.ToString());
        }
        private void remove_clicked(object sender, RoutedEventArgs e)
        {
            Button temp = sender as Button;
            string temppath = @temp.Tag.ToString();

            var tempdata = new CustomData(temppath);

            foreach(var item in list)
            {
                if (tempdata.Equals(item))
                {
                    tempdata = item;
                }
            }
            list.Remove(tempdata);
        }
        class CustomData : IEquatable<CustomData>
        {
            public string path { get; set; }
            public CustomData(string path)
            {
                this.path = path;
            }

            public bool Equals(CustomData? other)
            {
                if (other == null) return false;
                return (this.path.Equals(other.path));
            }
        }
    }
}
