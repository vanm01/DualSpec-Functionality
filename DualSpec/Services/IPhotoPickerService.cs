using System.IO;
using System.Threading.Tasks;

namespace DualSpec
{
    public interface IPhotoPickerService
    {
        Task<Stream> PickPhotoAsync();

        Task<bool> SavePhotoAsync(byte[] data, string folder, string filename);
    }
}
