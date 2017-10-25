using System.Collections.Generic;
using System.Threading.Tasks;
using MyHerbalife3.Ordering.Widgets.Model;

namespace MyHerbalife3.Ordering.Widgets.Interfaces
{
    public interface IVolumeSource
    {
        Task<List<VolumeModel>> GetVolume(string id);
    }
}