using FlickrNet;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

/* http://wpfkorea.tistory.com/158 참조 */

namespace mosaic_photo
{
    partial class MainWindow
    {
        public double TileWidthScale { get; set; }
        public double TileHeightScale { get; set; }

        public DrawingImage getFlickrImage(string Keyword)
        {
            updateLog("----- 키워드를 통해 플리커에서 이미지를 가져오는 중 -----");

            List<BitmapImage> FlickrImage = SearchImage(Keyword); // 키워드를 이용해 이미지를 검색한다.                       
            DrawingImage ResultImage = new DrawingImage(); // 결과가 저장될 객체
            DrawingGroup Group = new DrawingGroup();

            // BitmapImage를 Drawing하기 위한 DrawingContext
            DrawingContext DrawingContext = Group.Open();
            
            int MyImageWidth = MyImage.PixelWidth;
            int MyImageHeight = MyImage.PixelHeight;

            // Flickr Image의 크기
            double ImageWidth = MyImageWidth * TileWidthScale;
            double ImageHeight = MyImageHeight * TileHeightScale;

            // 실제 제작 부분. 바탕에 원본이미지를 그려준다.
            DrawingContext.DrawImage(MyImage, new Rect(0, 0, MyImageWidth, MyImageHeight));

            // DrawingContext에 Opacity를 Push해서 이후 그려질
            // 플리커 이미지 조각들에 대한 Opacity를 지정
            // Opacity가 낮을 수록 원본이미지가 선명하게 보임
            DrawingContext.PushOpacity(0.3);

            updateLog("\n----- 이미지를 제작하고 있습니다. ------\n");
            // FlickrImage에 저장된 이미지 중 랜덤으로 하나씩 선택하여 원래 이미지 위에 덮어쓰기를 수행
            Random Random = new Random((int)DateTime.Now.TimeOfDay.TotalMilliseconds);
            for (double X = 0; X < MyImageWidth - 1; X += ImageWidth)
            {
                for (double Y = 0; Y < MyImageHeight - 1; Y += ImageHeight)
                {
                    DrawingContext.DrawImage(FlickrImage[Random.Next(0, FlickrImage.Count)],
                        new Rect(X, Y, ImageWidth, ImageHeight));
                }
            }

            // DrawingContext의 Opacity를 Pop.
            DrawingContext.Pop();

            // DrawingContext 닫기
            DrawingContext.Close();

            // 데이터가 저장된 DrawingGroup을 ResultImage에 저장
            ResultImage.Drawing = Group;

            // 결과 return.

            return ResultImage;
        }

        private List<BitmapImage> SearchImage(string Keyword)
        {
            // Flickr에서 사진을 검색하기위해 Flickr API사용
            Flickr Flickr = new Flickr(Flickr_API);

            PhotoSearchOptions option = new PhotoSearchOptions();
            option.Extras = PhotoSearchExtras.AllUrls | PhotoSearchExtras.Description | PhotoSearchExtras.OwnerName;
            option.SortOrder = PhotoSearchSortOrder.Relevance; // 관련성 순서로 정렬
            option.ContentType = ContentTypeSearch.PhotosOnly; // 사진만 검색
            option.Tags = Keyword;

            List<BitmapImage> Images = new List<BitmapImage>();
            foreach (Photo PhotoItem in Flickr.PhotosSearch(option)) // Flickr에서 Keyword로 사진을 검색
            {
                BitmapImage ImageItem = new BitmapImage();
                ImageItem.BeginInit();

                // 사용 될 Image는 크기가 작아도 무방하므로 ThumbnailImage를 사용
                ImageItem.UriSource = new Uri(PhotoItem.ThumbnailUrl);
                // 빠른 처리를 위해 Cache사용
                ImageItem.CacheOption = BitmapCacheOption.OnLoad;
                ImageItem.CreateOptions = BitmapCreateOptions.PreservePixelFormat;

                ImageItem.EndInit();
                Images.Add(ImageItem);
            }

            return Images;
        }
    }
}
