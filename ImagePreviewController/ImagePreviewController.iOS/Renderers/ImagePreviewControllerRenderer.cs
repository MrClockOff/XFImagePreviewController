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
        private const float MinScale = 1.0f;
        private const float MaxScale = 5.0f;
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
            var backgroundView = new UIView
            {
                Frame = windowFrame,
                BackgroundColor = UIColor.Black,
                Alpha = 0,
            };
            var initialImageViewFrame = _imageView.Superview.ConvertRectToView(_imageView.Frame, null);
            var scrollView = new UIScrollView
            {
                Frame = windowFrame,
                MinimumZoomScale = MinScale,
                MaximumZoomScale = MaxScale,
                ContentSize = _imageView.Image.Size,
                ShowsVerticalScrollIndicator = false,
                ShowsHorizontalScrollIndicator = false
            };
            var zoomedImageView = new UIImageView
            {
                Frame = initialImageViewFrame,
                ContentMode = UIViewContentMode.ScaleAspectFit,
                Image = _imageView.Image,
                UserInteractionEnabled = true
            };
            var doubleTapGestureRecognizer = new UITapGestureRecognizer(() => 
            {
                if (Math.Abs(scrollView.ZoomScale - MaxScale) < 0.001) {
                    scrollView.SetZoomScale(MinScale, true);
                } else {
                    scrollView.SetZoomScale(MaxScale, true);
                }
            })
            {
                NumberOfTapsRequired = 2
            };
            var tapGestureRecognizer = new UITapGestureRecognizer(() =>
            {
                if (Math.Abs(scrollView.ZoomScale - MinScale) > 0.001)
                {
                    return;
                }

                window.AddSubview(zoomedImageView);

                Animate(DefaultAnimationTimeSeconds, 0, UIViewAnimationOptions.CurveEaseOut,
                    () =>
                    {
                        backgroundView.Alpha = 0;
                        zoomedImageView.Frame = initialImageViewFrame;
                    },
                    () =>
                    {
                        _imageView.Alpha = 1;
                        zoomedImageView.RemoveFromSuperview();
                        scrollView.RemoveFromSuperview();
                        backgroundView.RemoveFromSuperview();
                    });
            })
            {
                NumberOfTapsRequired = 1
            };

            _imageView.Alpha = 0;
            scrollView.ViewForZoomingInScrollView += (s) => zoomedImageView;
            zoomedImageView.AddGestureRecognizer(tapGestureRecognizer);
            zoomedImageView.AddGestureRecognizer(doubleTapGestureRecognizer);
            window.AddSubview(backgroundView);
            window.AddSubview(scrollView);
            window.AddSubview(zoomedImageView);

            Animate(DefaultAnimationTimeSeconds, 0, UIViewAnimationOptions.CurveEaseOut,
                () =>
                {
                    backgroundView.Alpha = 1;
                    zoomedImageView.Frame = windowFrame;
                }, 
                () => {
                    scrollView.AddSubview(zoomedImageView);
                    scrollView.SetZoomScale(MinScale, false);
                }
            );
        }
    }
}