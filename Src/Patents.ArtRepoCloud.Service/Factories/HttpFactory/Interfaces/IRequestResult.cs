using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Patents.ArtRepoCloud.Service.Factories.HttpFactory.Interfaces
{
    public interface IRequestResult<out T> : IRequestStatus
    {
        T? Result { get; }
    }
}