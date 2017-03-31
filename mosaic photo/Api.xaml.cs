using System;
using System.Windows;
using System.IO;

namespace mosaic_photo
{
    /// <summary>
    /// Api.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Api : Window
    {
        static string path = AppDomain.CurrentDomain.BaseDirectory;  //프로그램 실행되고 있는데 path 가져오기
        static string fileName = @"\config.ini";  //파일명
        static string filePath = path + fileName;   //ini 파일 경로

        public Api()
        {
            InitializeComponent();

            FileInfo fi = new FileInfo(filePath);
            if (fi.Exists) // config 가 존재한다면 해당 내용을 불러온다.
            {
                iniUtil ini = new iniUtil(filePath);
                string MS_API = ini.GetIniValue("Setting", "ms-api");
                string Flickr_API = ini.GetIniValue("Setting", "flickr-api");

                ms_api.Text = MS_API;
                flickr_api.Text = Flickr_API;
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            if (ms_api.Text.Length == 0 || flickr_api.Text.Length == 0)
            {
                MessageBox.Show("API키를 입력해주세요.");
                return;
            }

            iniUtil ini = new iniUtil(filePath);
            ini.SetIniValue("Setting", "ms-api", ms_api.Text);
            ini.SetIniValue("Setting", "flickr-api", flickr_api.Text);

            this.Close();
        }
    }
}
