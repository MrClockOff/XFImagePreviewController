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

        protected override void OnElementChanged(ElementChangedEventArgs<Image> e)
        {
            base.OnElementChanged(e);

            if (Element == null || Equals(Element, e.OldElement))
            {
                return;
            }

            // Set Xamarin.Forms element tap gesture
            var tapGestureRecogniser = new TapGestureRecognizer();
            tapGestureRecogniser.Tapped += OnTapped;
            Element.GestureRecognizers.Add(tapGestureRecogniser);
        }

        private void OnTapped(object sender, EventArgs eventArgs)
        {
            // Get screen frame (frame contains screen size)
            // and create background view for full screen preview
            var window = UIApplication.SharedApplication.KeyWindow;
            var windowFrame = window.Frame;
            var backgroundView = new UIView
            {
                Frame = windowFrame,
                BackgroundColor = UIColor.Black,
                Alpha = 0,
            };
            // Create scroll view to use for zooming in/out features
            var scrollView = new UIScrollView
            {
                Frame = windowFrame,
                MinimumZoomScale = MinScale,
                MaximumZoomScale = MaxScale,
                ContentSize = Control.Image.Size,
                ShowsVerticalScrollIndicator = false,
                ShowsHorizontalScrollIndicator = false
            };
            // Get initial image view relative coordinates which will be used in
            // exit full screen animation 
            // Create image view which will hold reference to original image
            var initialImageViewFrame = Control.Superview.ConvertRectToView(Control.Frame, null);
            var zoomedImageView = new UIImageView
            {
                Frame = initialImageViewFrame,
                ContentMode = UIViewContentMode.ScaleAspectFit,
                Image = Control.Image,
                UserInteractionEnabled = true
            };
            // Create tab gesture recognizer which will zoom in or out the image on double tap
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
            // Create tab gesture which will exit full screen on single tap
            // and animate image moving back to its initial place 
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
                        Control.Alpha = 1;
                        zoomedImageView.RemoveFromSuperview();
                        scrollView.RemoveFromSuperview();
                        backgroundView.RemoveFromSuperview();
                    });
            })
            {
                NumberOfTapsRequired = 1
            };

            // Once zooming image view created hide the original one
            // and animate transition into full screen
            Control.Alpha = 0;
            scrollView.ViewForZoomingInScrollView += (s) => zoomedImageView;
            scrollView.DidZoom += ScrollView_DidZoom;
            zoomedImageView.AddGestureRecognizer(tapGestureRecognizer);
            zoomedImageView.AddGestureRecognizer(doubleTapGestureRecognizer);
            window.AddSubview(backgroundView);
            window.AddSubview(scrollView);
            window.AddSubview(zoomedImageView);

            Animate(DefaultAnimationTimeSeconds, 0, UIViewAnimationOptions.CurveEaseOut,
                () =>
                {
                    backgroundView.Alpha = 1;
                    zoomedImageView.Frame = GetInitialZoomedImageViewFrame(window.Bounds.Size, Control.Image.Size);
                }, 
                () => {
                    scrollView.AddSubview(zoomedImageView);
                    scrollView.SetZoomScale(MinScale, false);
                }
            );
        }

        // Creates frame for zoomin image view with size of image to avoid blank space arround of image
        private CGRect GetInitialZoomedImageViewFrame(CGSize viewBoundsSize, CGSize imageSize)
        {
            var widthRatio = viewBoundsSize.Width / imageSize.Width;
            var heightRatio = viewBoundsSize.Height / imageSize.Height;
            var scale = Math.Min(widthRatio, heightRatio);
            var imageWidth = scale * imageSize.Width;
            var imageHeight = scale * imageSize.Height;
            var frame = new CGRect(0, (viewBoundsSize.Height - imageHeight) / 2, imageWidth, imageHeight);
            return frame;
        }

        // Centers scroll view content on zooming
        private void ScrollView_DidZoom(object sender, EventArgs e)
        {
            var scrollView = sender as UIScrollView;
            var subView = scrollView.Subviews[0];

            var offsetX = Math.Max((scrollView.Bounds.Size.Width - scrollView.ContentSize.Width) * 0.5, 0.0);
            var offsetY = Math.Max((scrollView.Bounds.Size.Height - scrollView.ContentSize.Height) * 0.5, 0.0);

            subView.Center = new CGPoint(scrollView.ContentSize.Width * 0.5 + offsetX, scrollView.ContentSize.Height * 0.5 + offsetY);
        }
    }
}