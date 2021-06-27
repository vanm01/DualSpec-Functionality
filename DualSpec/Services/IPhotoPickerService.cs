using System.IO;
using System.Threading.Tasks;

namespace DualSpec.Services
{
    public interface IPhotoPickerService
    {
        Task<Stream> PickPhotoAsync();
        Task<bool> SavePhotoAsync(byte[] data, string folder, string filename);
    }
}
