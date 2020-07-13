using IoT.DeviceShadow.Utils;
using IoT.DeviceShadow.Utils.Messages;
using Orleans;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace IoT.DeviceShadow.Grain.Contract
{
    public interface ISensor : IGrainWithStringKey
    {
        Task ProcessMessage(ValidatedMessage message);

        Task SetGrainState(SensorState state);

        Task<SensorState> GetGrainState();
         
    }
}
