using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyHerbalife3.Ordering.Widgets.Interfaces;
using MyHerbalife3.Ordering.Widgets.Model;
using MyHerbalife3.Shared.Interfaces;

namespace MyHerbalife3.Ordering.Widgets
{
    public class VolumeSource : IVolumeSource
    {

        internal ILoader<List<VolumeModel>, GetVolumeById> _Loader;

        public VolumeSource()
            : this(new VolumeProvider())
        {
        }

        public VolumeSource(ILoader<List<VolumeModel>, GetVolumeById> loader)
        {
            _Loader = loader;
        }

        public Task<List<VolumeModel>> GetVolume(string id)
        {
            var query = new GetVolumeById { Id = id };

            var result = new Task<List<VolumeModel>>(() => _Loader.Load(query));
            result.Start();

            return result;
        }


    }
}
