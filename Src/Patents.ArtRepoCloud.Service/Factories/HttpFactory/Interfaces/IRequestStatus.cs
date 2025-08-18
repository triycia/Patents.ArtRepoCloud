using Patents.ArtRepoCloud.Service.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Patents.ArtRepoCloud.Service.Factories.HttpFactory.Interfaces
{
    public interface IRequestStatus
    {
        HttpReasonCode ReasonCode { get; }
        bool Success { get; }
        string Message { get; }
    }
}