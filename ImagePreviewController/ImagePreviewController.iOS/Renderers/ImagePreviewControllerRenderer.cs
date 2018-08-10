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
            var imageViewFrame = _imageView.Frame;
            var zoomedImageViewPosition = new CGPoint(0, (windowFrame.Height - imageViewFrame.Height) / 2);
            var zoomedImageViewFrame = new CGRect(zoomedImageViewPosition, imageViewFrame.Size);
            var zoomedImageView = new UIImageView
            {
                Frame = zoomedImageViewFrame,
                ContentMode = UIViewContentMode.ScaleAspectFit,
                Image = _imageView.Image,
                UserInteractionEnabled = true,
                Alpha = 0
            };
            var tapGestureRecognizer = new UITapGestureRecognizer(() =>
            {
                Animate(0.75,
                    () =>
                    {
                        blackBackgroundView.Alpha = 0;
                        zoomedImageView.Alpha = 0;
                    },
                    () =>
                    {
                        blackBackgroundView.RemoveFromSuperview();
                        zoomedImageView.RemoveFromSuperview();
                    }
                );
            });

            zoomedImageView.AddGestureRecognizer(tapGestureRecognizer);
            window.AddSubview(blackBackgroundView);
            window.AddSubview(zoomedImageView);

            AnimateAsync(0.75,
                () =>
                {
                    blackBackgroundView.Alpha = 1;
                    zoomedImageView.Alpha = 1;
                }
            );
        }
    }
}