using IoT.DeviceShadow.Utils.Enum;
using IoT.DeviceShadow.Utils.Messages;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoT.DeviceShadow.Utils.Common
{
    public class Validation
    {
        public static ValidatedMessage ValidateProperties(string rawMessage)
        {
            var message = JsonConvert.DeserializeObject<Message>(rawMessage);

            if (message.IMEI == null) { return null; }

            var deviceMessage = new ValidatedMessage();

            if (message.IMEI == "")
            {
                return null;
            }
            else
            {
                deviceMessage.IMEI = message.IMEI;
            }

            try
            {
                deviceMessage.EventTime = (new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).AddSeconds(message.State.Time);
            }
            catch (Exception ex)
            {

                return null;
            }

            try
            {
                deviceMessage.ReceivedTime = (new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).AddSeconds(message.ReceievedTime);
            }
            catch (Exception ex)
            {
                return null;
            }

            if (string.IsNullOrEmpty(message.State.Latitude))
            {
                return null;
            }
            else
            {
                double lat;
                if (!double.TryParse(message.State.Latitude, System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture, out lat))
                {
                    return null;
                }
                else
                {
                    deviceMessage.Latitude = lat;
                }
            }

            if (string.IsNullOrEmpty(message.State.Longitude))
            {
                return null;
            }
            else
            {
                double lon;
                if (!double.TryParse(message.State.Longitude, System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture, out lon))
                {
                    return null;
                }
                else
                {
                    deviceMessage.Longitude = lon;
                }
            }

            if (!string.IsNullOrEmpty(message.State.PowerStatus))
            {
                if (message.State.PowerStatus == "1")
                {
                    deviceMessage.MainPowerStatus = MainPowerStatus.On;
                }
                else
                {
                    deviceMessage.MainPowerStatus = MainPowerStatus.Off;
                }
            }
            else
            {
                deviceMessage.MainPowerStatus = MainPowerStatus.Unknown;
            }

            return deviceMessage;
        }
    }
}