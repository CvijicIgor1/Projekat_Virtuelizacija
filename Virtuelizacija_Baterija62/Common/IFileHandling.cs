using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    [ServiceContract]
    internal interface IFileHandling
    {
        [OperationContract]
        FileManipulationResults SendFile(FileManipulationOptions options);

        [OperationContract]
        FileManipulationResults GetFiles(FileManipulationOptions options);
    }
}
