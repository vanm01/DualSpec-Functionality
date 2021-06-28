using System;
using System.Threading.Tasks;
using SkiaSharp;
using Xamarin.Forms;
using DualSpec.Services;
using System.IO;

namespace DualSpec.Views
{
    public partial class SavePhotoPage : ContentPage, IModelPage
    {
        SKImage saveImage;

        public SavePhotoPage()
        {
            InitializeComponent();
        }

        public SavePhotoPage(SKImage image)
        {
            InitializeComponent();

            saveImage = image;

        }

        public async Task Dismiss()
        {
            await Navigation.PopModalAsync();
        }

        async void OnSaveButtonClicked(System.Object sender, System.EventArgs e)
        {
            //SKEncodedImageFormat imageFormat = (SKEncodedImageFormat)formatPicker.SelectedItem;
            //int quality = (int)qualitySlider.Value;

            using (MemoryStream memStream = new MemoryStream())

            using (SKManagedWStream wstream = new SKManagedWStream(memStream))
            {

                byte[] saveData = saveImage.Encode(SKEncodedImageFormat.Jpeg, 100).ToArray();

                if (saveData == null)
                {
                    statusLabel.Text = "Encode returned null";
                }
                else if (saveData.Length == 0)
                {
                    statusLabel.Text = "Encode returned empty array";
                }
                else
                {
                    bool success = await DependencyService.Get<IPhotoPickerService>().
                        SavePhotoAsync(saveData, folderNameEntry.Text, fileNameEntry.Text);

                    if (!success)
                    {
                        statusLabel.Text = "SavePhotoAsync return false";
                    }
                    else
                    {
                        statusLabel.Text = "Success!";
                    }
                }
            }
        }
            
        void OnFormatPickerChanged(object sender, EventArgs e)
        {

            if (formatPicker.SelectedIndex != -1)
            {
                SKEncodedImageFormat imageFormat = (SKEncodedImageFormat)formatPicker.SelectedItem;
                fileNameEntry.Text = Path.ChangeExtension(fileNameEntry.Text, imageFormat.ToString().ToLower());
            }

        }

    }
}
