using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using System.IO;
using Microsoft.ProjectOxford.Vision;
using Microsoft.ProjectOxford.Vision.Contract;


namespace mosaic_photo
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        public static string MS_API { get; set; }
        public static string Flickr_API { get; set; }

        static string path = AppDomain.CurrentDomain.BaseDirectory;  //프로그램이 실행되고 있는 경로 가져오기
        static string fileName = @"\config.ini";  //파일명
        static string filePath = path + fileName;   //ini 파일 경로
        static string ImgPath { get; set; }
        static BitmapImage MyImage { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            if (chkConfig() == true)
            {
                loadSetting();
            }
            else
            {
                MS_API = "여기에 Microsoft Computer Vision API를 입력하세요.";
                Flickr_API = "여기에 Flickr API를 입력하세요.";
            }
        }

        #region 파일메뉴
        private void ImageOpen_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            openFileDialog.InitialDirectory = "c:\\"; // 기본경로
            openFileDialog.RestoreDirectory = true;
            openFileDialog.Filter = "Image files (*.jpg;*.jpeg;*.png;*.bmp;*.tif)|*.jpg;*.jpeg;*.png;*.bmp;*.tif";

            if (openFileDialog.ShowDialog() == true)
            {
                ImageSaveAs.IsEnabled = true;
                menu_look.IsEnabled = true;
                convertImg.IsEnabled = true;

                MyImage = new BitmapImage(new Uri(openFileDialog.FileName));
                ImgPath = openFileDialog.FileName;
                image.Source = MyImage;

                manage_image_view_mode(1);
            }
        }

        private void ProgramEnd_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var response = MessageBox.Show("정말로 종료하시겠습니까?", "종료 안내", MessageBoxButton.YesNo, MessageBoxImage.Exclamation);

            if (response == MessageBoxResult.No)
            {
                e.Cancel = true;
            }
            else
            {
                Application.Current.Shutdown();
            }
        }

        private void ImageSaveAs_Click(object sender, RoutedEventArgs e)
        {
            if (image.Source is DrawingImage == false)
                return;

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Image File(*.jpg)|*.jpg";

            if (saveFileDialog.ShowDialog() == true)
            {
                DrawingImage ResultImage = image.Source as DrawingImage;
                DrawingVisual Visual = new DrawingVisual();

                DrawingContext DC = Visual.RenderOpen();
                DC.DrawImage(ResultImage, new Rect(0, 0, ResultImage.Width, ResultImage.Height));
                DC.Close();

                RenderTargetBitmap Bitmap = new RenderTargetBitmap((int)ResultImage.Width, (int)ResultImage.Height, 96d, 96d, PixelFormats.Default); // 일반적인 데스크탑 환경의 DPI는 96
                Bitmap.Render(Visual);

                FileStream Stream = new FileStream(saveFileDialog.FileName, FileMode.Create);

                JpegBitmapEncoder Encoder = new JpegBitmapEncoder();
                Encoder.QualityLevel = 100;
                Encoder.Frames.Add(BitmapFrame.Create(Bitmap));
                Encoder.Save(Stream);

                Stream.Close();
            }
        }
        #endregion

        #region 이미지 보기 방법      
        private void viewImgScreenSize_Click(object sender, RoutedEventArgs e)
        {
            manage_image_view_mode(1);
        }

        private void viewOriginalSize_Click(object sender, RoutedEventArgs e)
        {
            manage_image_view_mode(2);
        }

        private void viewImgWidthSize_Click(object sender, RoutedEventArgs e)
        {
            manage_image_view_mode(3);
        }

        private void viewImgHeightSize_Click(object sender, RoutedEventArgs e)
        {
            manage_image_view_mode(4);
        }

        private void manage_image_view_mode(int order)
        {
            switch (order)
            {
                case 1: // 화면에 맞춰 보기
                    image.Stretch = Stretch.Uniform;

                    viewOriginalSize.IsChecked = false;
                    viewImgScreenSize.IsChecked = true;
                    viewImgHeightSize.IsChecked = false;
                    viewImgWidthSize.IsChecked = false;

                    ImageScroll.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled; // 가로 바
                    ImageScroll.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled; // 세로 바
                    break;
                case 2: // 100% 보기
                    image.Stretch = Stretch.None;

                    viewOriginalSize.IsChecked = true;
                    viewImgScreenSize.IsChecked = false;
                    viewImgHeightSize.IsChecked = false;
                    viewImgWidthSize.IsChecked = false;

                    ImageScroll.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto; // 가로 바
                    ImageScroll.VerticalScrollBarVisibility = ScrollBarVisibility.Auto; // 세로 바
                    break;
                case 3: // 가로폭에 맞춰 보기
                    image.Stretch = Stretch.Uniform;

                    viewOriginalSize.IsChecked = false;
                    viewImgScreenSize.IsChecked = false;
                    viewImgHeightSize.IsChecked = false;
                    viewImgWidthSize.IsChecked = true;

                    ImageScroll.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled; // 가로 바
                    ImageScroll.VerticalScrollBarVisibility = ScrollBarVisibility.Auto; // 세로 바
                    break;
                case 4: // 세로폭에 맞춰 보기
                    image.Stretch = Stretch.Uniform;

                    viewOriginalSize.IsChecked = false;
                    viewImgScreenSize.IsChecked = false;
                    viewImgHeightSize.IsChecked = true;
                    viewImgWidthSize.IsChecked = false;

                    ImageScroll.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto; // 가로 바
                    ImageScroll.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled; // 세로 바
                    break;
            }
        }
        #endregion

        private void setAPI_Click(object sender, RoutedEventArgs e)
        {
            Api api = new Api();
            api.Owner = this;
            api.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            api.ShowDialog();
        }

        public void updateLog(string txt)
        {
            // 해당 쓰레드가 UI쓰레드인가?
            if (logBox.Dispatcher.CheckAccess())
            {
                //UI 쓰레드인 경우
                logBox.AppendText(txt + Environment.NewLine);
                logBox.ScrollToLine(logBox.LineCount - 1); // 로그창 스크롤 아래로
            }
            else
            {
                // 작업쓰레드인 경우
                logBox.Dispatcher.BeginInvoke((Action)(() => { logBox.AppendText(txt + Environment.NewLine); logBox.ScrollToLine(logBox.LineCount - 1); }));
            }
        }

        #region API 세팅
        public bool chkConfig()
        {
            FileInfo fi = new FileInfo(filePath);
            if (fi.Exists)
            {
                return true;
            }

            return false;
        }

        private void loadSetting()
        {
            if (chkConfig() == true)
            {
                iniUtil ini = new iniUtil(filePath);
                MS_API = ini.GetIniValue("Setting", "ms-api");
                Flickr_API = ini.GetIniValue("Setting", "flickr-api");
            }
        }
        #endregion

        private async void convertImg_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MessageBox.Show("이 작업은 다소 시간이 걸릴 수 있습니다.");
                updateLog("MS서버로 이미지 전송이 시작되었습니다." + "\n");

                AnalysisResult analysisResult;
                analysisResult = await UploadAndAnalyzeImage(ImgPath);
                LogAnalysisResult(analysisResult);

                List<string> keywords = new List<string>(); // 키워드들을 담을 리스트
                foreach (var tag in analysisResult.Tags)
                {
                    if (tag.Confidence > 0.85) // 태그의 신뢰성 수치가 85%이상이라면
                    {
                        keywords.Add(tag.Name); // 리스트에 키워드 추가
                    }
                }

                updateLog("키워드는 이미지 분석 결과 태그 중 랜덤으로 선택됩니다.");

                Random r = new Random((int)DateTime.Now.TimeOfDay.TotalMilliseconds);
                int num = r.Next(0, keywords.Count - 1);

                TileHeightScale = 0.01; // 원본의 100분의 1 크기
                TileWidthScale = 0.01;

                updateLog("선택된 키워드 : " + keywords[num] + "\n");
                image.Source = getFlickrImage(keywords[num]);
            }
            catch (Exception ERR)
            {
                updateLog("\n\n=====ERROR =====\n" + ERR.ToString());
            }
        }

        private async Task<AnalysisResult> UploadAndAnalyzeImage(string imageFilePath)
        {
            VisionServiceClient VisionServiceClient = new VisionServiceClient(MainWindow.MS_API);

            using (Stream imageFileStream = File.OpenRead(imageFilePath))
            {
                VisualFeature[] visualFeatures = new VisualFeature[] { VisualFeature.Adult, VisualFeature.Categories, VisualFeature.Color, VisualFeature.Description, VisualFeature.Faces, VisualFeature.ImageType, VisualFeature.Tags };
                AnalysisResult analysisResult = await VisionServiceClient.AnalyzeImageAsync(imageFileStream, visualFeatures);
                return analysisResult;
            }
        }

        #region 이미지 분석 로그
        protected void LogAnalysisResult(AnalysisResult result)
        {
            updateLog("----- 이미지 분석 결과 -----");

            if (result == null)
            {
                updateLog("null");
                return;
            }

            if (result.Metadata != null)
            {
                updateLog("이미지 포맷 : " + result.Metadata.Format);
                updateLog("이미지 차원 : " + result.Metadata.Width + " x " + result.Metadata.Height);
            }

            if (result.Adult != null)
            {
                updateLog("성인물? : " + result.Adult.IsAdultContent);
                updateLog("성인물 판단 점수 : " + result.Adult.AdultScore);
                updateLog("꼴려? : " + result.Adult.IsRacyContent);
                updateLog("꼴림 판단 점수 : " + result.Adult.RacyScore);
            }

            if (result.Categories != null && result.Categories.Length > 0)
            {
                updateLog("카테고리 : ");
                foreach (var category in result.Categories)
                {
                    updateLog("   카테고리 이름 : " + category.Name + "; 점수 : " + category.Score);
                }
            }

            if (result.Faces != null && result.Faces.Length > 0)
            {
                updateLog("얼굴 : ");
                foreach (var face in result.Faces)
                {
                    updateLog("   추정 나이 : " + face.Age + "; 성별 : " + face.Gender);
                }
            }

            if (result.Color != null)
            {
                updateLog("강조되는 색상 : " + result.Color.AccentColor);
                updateLog("주요 배경색 : " + result.Color.DominantColorBackground);
                updateLog("주요 전경색 : " + result.Color.DominantColorForeground);

                if (result.Color.DominantColors != null && result.Color.DominantColors.Length > 0)
                {
                    string colors = "주요 색깔 : ";
                    foreach (var color in result.Color.DominantColors)
                    {
                        colors += color + " ";
                    }
                    updateLog(colors);
                }
            }

            if (result.Description != null)
            {
                updateLog("요약 : ");
                foreach (var caption in result.Description.Captions)
                {
                    updateLog("   설명 : " + caption.Text + "; 신뢰수치 : " + caption.Confidence);
                }
                string tags = "   태그 : ";
                foreach (var tag in result.Description.Tags)
                {
                    tags += tag + ", ";
                }
                updateLog(tags);
            }

            if (result.Tags != null)
            {
                updateLog("태그 : ");
                foreach (var tag in result.Tags)
                {
                    updateLog("   태그 : " + tag.Name + "; 신뢰수치 : " + tag.Confidence + "; 힌트 : " + tag.Hint);
                }
            }
            updateLog("----- 이미지 분석 완료 -----" + "\n");
        }
        #endregion

    }
}
