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
        SKPixmap savePixmap;

        public SavePhotoPage()
        {
            InitializeComponent();
        }

        public SavePhotoPage(SKImage image)
            : this()
        {
            savePixmap = image.SKImageToPixmap();
        }

        public async Task Dismiss()
        {
            await Navigation.PopModalAsync();
        }

        async void OnSaveButtonClicked(System.Object sender, System.EventArgs e)
        {
            SKEncodedImageFormat imageFormat = (SKEncodedImageFormat)formatPicker.SelectedItem;
            int quality = (int)qualitySlider.Value;

            if(savePixmap==null)
            {
                Console.WriteLine("********************************Pixmap is null********************************");
            }

            using (MemoryStream memStream = new MemoryStream())
            {
                using (SKManagedWStream wstream = new SKManagedWStream(memStream))
                {

                    if(savePixmap != null)
                    {
                        bool result = savePixmap.Encode(wstream, imageFormat, quality);
                        byte[] data = memStream.ToArray();

                        if (data == null || data.Length == 0)
                            statusLabel.Text = "Encode failed";
                        else
                        {
                            bool success = await DependencyService.Get<IPhotoPickerService>().SavePhotoAsync(data, folderNameEntry.Text, fileNameEntry.Text);

                            if (!success)
                                statusLabel.Text = "Save failed";
                            else
                                statusLabel.Text = "Save succeeded";
                        }
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
