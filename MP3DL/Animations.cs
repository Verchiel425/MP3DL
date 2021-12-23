using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace MP3DL
{
    public partial class MainWindow : Window
    {
        private void AnimateMenu()
        {
            double x = 50d;
            double y = 250d;
            Storyboard storyboard = new();
            var changeWidth = new DoubleAnimation
            {
                Duration = new Duration(TimeSpan.FromMilliseconds(150)),
                AccelerationRatio = 0.4,
                DecelerationRatio = 0.6
            };
            storyboard.Children.Add(changeWidth);
            Storyboard.SetTarget(storyboard, main_menu);
            Storyboard.SetTargetProperty(storyboard, new PropertyPath(Grid.WidthProperty));

            storyboard.Stop();
            if (main_menu.Width > x && main_menu.Tag.ToString() == "exp")
            {
                changeWidth.From = main_menu.Width;
                changeWidth.To = x;
                storyboard.Begin();
            }
            else if (main_menu.Width < y && main_menu.Tag.ToString() == "nexp")
            {
                changeWidth.From = main_menu.Width;
                changeWidth.To = y;
                storyboard.Begin();
            }

            if (main_menu.Tag.ToString() == "nexp")
            {
                main_menu.Tag = "exp";
            }
            else
            {
                main_menu.Tag = "nexp";
            }
        }
        private void AnimatePlayer()
        {
            double x = 60d;
            double y = 120d;
            Storyboard storyboard = new();
            Storyboard title = new();
            Storyboard artist = new();
            var changeHeight = new DoubleAnimation
            {
                Duration = new Duration(TimeSpan.FromMilliseconds(500)),
                AccelerationRatio = 0.4,
                DecelerationRatio = 0.6
            };
            var changeTitleMargin = new ThicknessAnimation
            {
                Duration = new Duration(TimeSpan.FromMilliseconds(500)),
                AccelerationRatio = 0.4,
                DecelerationRatio = 0.6
            };
            var changeArtistMargin = new ThicknessAnimation
            {
                Duration = new Duration(TimeSpan.FromMilliseconds(500)),
                AccelerationRatio = 0.4,
                DecelerationRatio = 0.6
            };

            storyboard.Children.Add(changeHeight);
            title.Children.Add(changeTitleMargin);
            artist.Children.Add(changeArtistMargin);
            Storyboard.SetTarget(storyboard, player);
            Storyboard.SetTarget(title, playerTitle);
            Storyboard.SetTarget(artist, playerArtist);
            Storyboard.SetTargetProperty(storyboard, new PropertyPath(Grid.HeightProperty));
            Storyboard.SetTargetProperty(title, new PropertyPath(Label.MarginProperty));
            Storyboard.SetTargetProperty(artist, new PropertyPath(Label.MarginProperty));

            var thick1 = new Thickness();
            var thick2 = new Thickness();
            var thick3 = new Thickness();
            var thick4 = new Thickness();
            thick1.Right = 0;
            thick1.Top = 0;
            thick1.Bottom = 0;
            thick1.Left = 67;
            thick2.Right = 0;
            thick2.Top = 0;
            thick2.Bottom = 0;
            thick2.Left = 127;
            thick3.Right = 0;
            thick3.Top = 28;
            thick3.Bottom = 0;
            thick3.Left = 67;
            thick4.Right = 0;
            thick4.Top = 28;
            thick4.Bottom = 0;
            thick4.Left = 127;

            storyboard.Stop();
            title.Stop();
            artist.Stop();

            if (player.Height > x && player.Tag.ToString() == "exp")
            {
                changeHeight.From = player.Height;
                changeHeight.To = x;

                changeTitleMargin.From = thick2;
                changeTitleMargin.To = thick1;
                changeArtistMargin.From = thick4;
                changeArtistMargin.To = thick3;
                artist.Begin();
                title.Begin();
                storyboard.Begin();
            }
            else if (player.Height < y && player.Tag.ToString() == "nexp")
            {
                changeHeight.From = player.Height;
                changeHeight.To = y;

                changeTitleMargin.From = thick1;
                changeTitleMargin.To = thick2;
                changeArtistMargin.From = thick3;
                changeArtistMargin.To = thick4;
                artist.Begin();
                title.Begin();
                storyboard.Begin();
            }

            if (player.Tag.ToString() == "nexp")
            {
                player.Tag = "exp";
            }
            else
            {
                player.Tag = "nexp";
            }
        }
        private void FadeTab()
        {
            Storyboard storyboard = new();
            var changeOpacity = new DoubleAnimation
            {
                Duration = new Duration(TimeSpan.FromMilliseconds(320)),
                AccelerationRatio = 0.4,
                DecelerationRatio = 0.6,
                From = 0,
                To = 1
            };
            storyboard.Children.Add(changeOpacity);
            Storyboard.SetTarget(storyboard, MainTabControl);
            Storyboard.SetTargetProperty(storyboard, new PropertyPath(TabControl.OpacityProperty));
            storyboard.Begin();
        }
        private void FadeOutElements(UIElement control,DependencyProperty property)
        {
            Storyboard storyboard = new();
            var changeOpacity = new DoubleAnimation
            {
                Duration = new Duration(TimeSpan.FromMilliseconds(250)),
                AccelerationRatio = 0.4,
                DecelerationRatio = 0.6,
                From = 1,
                To = 0
            };
            storyboard.Children.Add(changeOpacity);
            Storyboard.SetTarget(storyboard, control);
            Storyboard.SetTargetProperty(storyboard, new PropertyPath(property));
            storyboard.Begin();
            storyboard.Completed += (sender, eArgs) =>
            {
                control.Visibility = Visibility.Hidden;
            };
        }
        private void FadeInElements(UIElement control, DependencyProperty property)
        {
            Storyboard storyboard = new();
            var changeOpacity = new DoubleAnimation
            {
                Duration = new Duration(TimeSpan.FromMilliseconds(250)),
                AccelerationRatio = 0.4,
                DecelerationRatio = 0.6,
                From = 0,
                To = 1
            };
            storyboard.Children.Add(changeOpacity);
            Storyboard.SetTarget(storyboard, control);
            Storyboard.SetTargetProperty(storyboard, new PropertyPath(property));
            storyboard.Begin();
            storyboard.Completed += (sender, eArgs) =>
            {
                control.Visibility = Visibility.Visible;
            };
        }

        private void RotateButton()
        {

            var x = exprotate.Angle / 180;

            Storyboard storyboard = new();
            var rotate = new DoubleAnimation
            {
                Duration = new Duration(TimeSpan.FromMilliseconds(500)),
                AccelerationRatio = 0.4,
                DecelerationRatio = 0.6
            };
            rotate.From = exprotate.Angle;

            if (exprotate.Angle >= 360)
            {
                rotate.To = (Math.Round(x) * 180) - 180;
            }
            else
            {
                rotate.To = (Math.Round(x) * 180) + 180;
            }

            storyboard.Children.Add(rotate);
            Storyboard.SetTargetName(storyboard, "exprotate");
            Storyboard.SetTargetProperty(storyboard, new PropertyPath(RotateTransform.AngleProperty));
            storyboard.Completed += RotationCompleted;

            storyboard.Begin(this.expandicon);

        }
        private void RotationCompleted(object? sender, EventArgs e)
        {
            ExpandPlayerButton.IsEnabled = true;
        }
        private void IsPlaying(bool ISPLAYING)
        {
            if (ISPLAYING)
            {
                mplayer.Play();
            }
            else
            {
                
                mplayer.Pause();
            }
        }
    }
}
