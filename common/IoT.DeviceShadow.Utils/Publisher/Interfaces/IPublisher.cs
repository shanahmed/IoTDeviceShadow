using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace IoT.DeviceShadow.Utils.Publisher.Interfaces
{
    public interface IPublisher<T>
    {
        Task Start();
        Task Stop();
        Task PublishSync(object message);
    }
}
