using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace Diatom.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainView : Window
    {
        public MainView()
        {
            InitializeComponent();
        }
        //图片文件路径集合
        static IEnumerable<string> files = null;

        //图片类型
        static string[] arr = new string[] { "*.BMP", "*.TIF", "*.GIF", "*.JPEG", "*.JPG", "*.SVG", "*.PSD", "*.PNG", "*.ICO" };

        /// <summary>
        /// 获取指定目录下所有的指定类型文件
        /// </summary>
        /// <param name="path"></param>
        /// <param name="filters"></param>
        /// <returns></returns>
        private IEnumerable<string> GetImages(string path, string[] filters)
        {
            List<string> list = new List<string>();
            foreach (var item in filters)
            {
                try
                {
                    list.AddRange(Directory.EnumerateFiles(path, item, SearchOption.AllDirectories).ToList());
                }
                catch (Exception)
                {
                }
            }
            return list;
        }

        private void Select_Click(object sender, RoutedEventArgs e)   //双击button得到的点击效果，双击可以直接把button名给出来
        {
            FolderBrowserDialog ofd = new FolderBrowserDialog();
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                cur = 0;
                btnBegin.IsEnabled = btnCancel.IsEnabled = false;
                txt_Load.Text = ofd.SelectedPath;
                files = GetImages(txt_Load.Text, arr);
                progressBar1.Value = 0;
                progressBar1.Maximum = files.Count();
                label.Content = $"0/{progressBar1.Maximum}";
                btnBegin.IsEnabled = btnCancel.IsEnabled = true;
            }
        }
        int cur = 0;
        private void ShowImg()
        {
            btnBegin.IsEnabled = false;
            string toPath = txt_Load_Copy.Text;
            Task.Run(() =>
            {
                cur = cur > 0 ? cur : 0;
                int now = cur;
                try
                {
                    files = files.Skip(cur);
                    foreach (var file in files)
                    {
                        cur++;
                        if (isstop) break;
                        App.Current.Dispatcher.Invoke(() =>
                        {
                            progressBar1.Value = now++;
                            label.Content = $"{now}/{files.Count()}";
                            img_photo.Source = new BitmapImage(new Uri(file));
                        });
                        Task.Run(() => {
                            File.Copy(file, $@"{toPath}\{DateTime.Now.Ticks.ToString()}{file.Substring(file.LastIndexOf("."))}", true);
                            GC.Collect();
                            GC.WaitForPendingFinalizers();
                        });
                        Thread.Sleep(100);
                    }
                }
                catch (Exception ex)
                {
                    System.Windows.Forms.MessageBox.Show($"An exception occurred during the search and this search has been terminated \r\n{ex.Message}", "Tips", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                finally
                {
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        btnBegin.IsEnabled = true;
                    });
                    System.Windows.Forms.MessageBox.Show($"The search is complete and a total of {now} images were retrieved!", "Tips", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    Process.Start(toPath, "explorer.exe");
                }
            });
        }
        bool isstop = false;
        

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (!Directory.Exists(txt_Load.Text))
            {
                System.Windows.Forms.MessageBox.Show("The selected folder does not exist, please select the folder first and then start");
                return;
            }
            if (string.IsNullOrEmpty(txt_Load_Copy.Text))
            {
                System.Windows.Forms.MessageBox.Show("Please select the path to save the retrieved images");
                return;
            }
            if (!Directory.Exists(txt_Load_Copy.Text))
            {
                try
                {
                    Directory.CreateDirectory(txt_Load_Copy.Text);
                }
                catch (Exception)
                {
                    System.Windows.Forms.MessageBox.Show("The selected path is wrong, please choose again");
                    return;
                }
            }
            isstop = false;
            if (files == null || files.Count() <= 0) { System.Windows.Forms.MessageBox.Show("There are no image files in the currently selected folder"); return; }
            progressBar1.Value = 0;
            progressBar1.Maximum = files.Count();
            label.Content = $"0/{progressBar1.Maximum}";
            ShowImg();
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e) //关闭页面
        {
            var dep = sender as DependencyObject;
            if (null != dep)
            {
                var win = Window.GetWindow(dep);
                if (null != win)
                    win.Close();
            }

        }
        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) //点击蓝色部分拖动页面
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                this.DragMove();
        }

        private void btnMin_Click(object sender, RoutedEventArgs e) //隐藏页面
        {
            this.WindowState = WindowState.Minimized;
        }

        private void btnMax_Click(object sender, RoutedEventArgs e) //放大页面
        {
            this.WindowState = this.WindowState == WindowState.Maximized ?
                WindowState.Normal : WindowState.Maximized;
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog ofd = new FolderBrowserDialog();
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                txt_Load_Copy.Text = ofd.SelectedPath;
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (isstop) return;
            btnBegin.IsEnabled = true;
            isstop = true;
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            if (btnBegin.IsEnabled == false && System.Windows.Forms.MessageBox.Show("Mission in progress, confirmation of cancellation?", "Tips", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == System.Windows.Forms.DialogResult.No)
                return;
            cur = 0;
            isstop = true;
        }
    }
}
