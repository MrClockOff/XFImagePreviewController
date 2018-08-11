using System;
using CoreGraphics;
using ImagePreviewController.iOS.Renderers;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(ImagePreviewController.Controllers.ImagePreviewController), typeof(ImagePreviewControllerRenderer))]
namespace ImagePreviewController.iOS.Renderers
{
    internal class ImagePreviewControllerRenderer : ImageRenderer
    {
        private const double DefaultAnimationTimeSeconds = 0.5;
        private Image _element;

        private UIImageView _imageView;

        protected override void OnElementChanged(ElementChangedEventArgs<Image> e)
        {
            if (Equals(e.NewElement, e.OldElement))
            {
                return;
            }

            _element = e.NewElement;

            if (_element == null)
            {
                return;
            }

            var tapGestureRecogniser = new TapGestureRecognizer();
            tapGestureRecogniser.Tapped += OnTapped;
            _element.GestureRecognizers.Add(tapGestureRecogniser);

            _imageView = new UIImageView();

            SetNativeControl(_imageView);
            base.OnElementChanged(e);
        }

        private void OnTapped(object sender, EventArgs eventArgs)
        {
            var window = UIApplication.SharedApplication.KeyWindow;
            var windowFrame = window.Frame;
            var blackBackgroundView = new UIView
            {
                Frame = windowFrame,
                BackgroundColor = UIColor.Black,
                Alpha = 0
            };
            var initialImageViewFrame = _imageView.Superview.ConvertRectToView(_imageView.Frame, null);
            var zoomedImageView = new UIImageView
            {
                Frame = initialImageViewFrame,
                ContentMode = UIViewContentMode.ScaleAspectFit,
                Image = _imageView.Image,
                UserInteractionEnabled = true
            };
            var tapGestureRecognizer = new UITapGestureRecognizer(() =>
            {
                Animate(DefaultAnimationTimeSeconds, 0, UIViewAnimationOptions.CurveEaseOut,
                    () =>
                    {
                        blackBackgroundView.Alpha = 0;
                        zoomedImageView.Frame = initialImageViewFrame;
                    },
                    () =>
                    {
                        _imageView.Alpha = 1;
                        blackBackgroundView.RemoveFromSuperview();
                        zoomedImageView.RemoveFromSuperview();
                    });
            });

            _imageView.Alpha = 0;
            zoomedImageView.AddGestureRecognizer(tapGestureRecognizer);
            window.AddSubview(blackBackgroundView);
            window.AddSubview(zoomedImageView);

            Animate(DefaultAnimationTimeSeconds, 0, UIViewAnimationOptions.CurveEaseOut,
                () =>
                {
                    blackBackgroundView.Alpha = 1;
                    zoomedImageView.Frame = new CGRect(
                        0,
                        (windowFrame.Height - initialImageViewFrame.Height) / 2,
                        initialImageViewFrame.Width,
                        initialImageViewFrame.Height
                    );
                }, 
                null
            );
        }
    }
}