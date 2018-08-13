using System;
using Android.Content;
using Com.Davemorrissey.Labs.Subscaleview;
using ImageSource_Subscaleview = Com.Davemorrissey.Labs.Subscaleview.ImageSource;
using ImagePreviewController.Droid.Renderers;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using Android.Graphics.Drawables;
using Android.App;

[assembly: ExportRenderer(typeof(ImagePreviewController.Controllers.ImagePreviewController), typeof(ImagePreviewControllerRenderer))]
namespace ImagePreviewController.Droid.Renderers
{
    internal class ImagePreviewControllerRenderer : ImageRenderer
    {
        public ImagePreviewControllerRenderer(Context context) : base(context) {}

        protected override void OnElementChanged(ElementChangedEventArgs<Image> e)
        {
            base.OnElementChanged(e);

            if (Element == null || Equals(Element, e.OldElement)) {
                return;
            }

            // Set Xamarin.Forms element tap gesture
            var tapGestureRecogniser = new TapGestureRecognizer();
            tapGestureRecogniser.Tapped += OnTapped;
            Element.GestureRecognizers.Add(tapGestureRecogniser);
        }

        private void OnTapped(object sender, EventArgs eventArgs)
        {
            var dialog = new Dialog(Context, Android.Resource.Style.ThemeBlackNoTitleBarFullScreen);
            var zoomImageView = Inflate(Context, Resource.Layout.ImagePreviewControllerLayout, null);
            var imageView = zoomImageView.FindViewById<SubsamplingScaleImageView>(Resource.Id.ImagePreviewController);
            var bitmap = (Control.Drawable as BitmapDrawable).Bitmap;

            imageView.SetImage(ImageSource_Subscaleview.InvokeBitmap(bitmap));
            dialog.SetContentView(zoomImageView);
            dialog.Show();
        }
    }
}
