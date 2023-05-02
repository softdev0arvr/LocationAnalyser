
using MIS.Domain.Enums;
using System.Net.NetworkInformation;

namespace Location.Models
{
    /// <summary>
    /// The ApiResponseModel class.
    /// </summary>
    public class ApiResponseModel
    {
        public ApiResponseModel(object result=null, string message=null)
        {
            Result = result;
            Message = message;
        }
        public ApiResponseModel(ApiStatus status, string message, Exception exceptionDetails=null)
        {
            Status = status.ToString();
            Message = message;
            ExceptionDetails = exceptionDetails;
        }

        public string Status { get; set; } = ApiStatus.Success.ToString();
        public Exception ExceptionDetails = null;
        public string Message { get; set; } = null;
        public object Result { get; set; } = null;
    }
}
