using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    [ServiceContract]
    public interface IBatteryService
    {
        [OperationContract]
        void StartSession(EisMeta meta);

        [OperationContract]
        void PushSample(EisSample sample);

        [OperationContract]
        void EndSession();

        [OperationContract]
        string Ping(); // za proveru app.config-a

    }
}
