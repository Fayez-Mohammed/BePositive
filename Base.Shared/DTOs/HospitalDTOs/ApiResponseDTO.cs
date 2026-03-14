namespace Base.Shared.DTOs.HospitalDTOs
{
    public class ApiResponseDTO
    {
        public int StatusCode { get; set; }
        public string Message { get; set; } = string.Empty;
        public object Data { get; set; }
        public string TraceId { get; set; }

        public ApiResponseDTO() { }
        public ApiResponseDTO(int statusCode, string message, object data = null, string traceId = null)
        {
            StatusCode = statusCode;
            Message = message;
            Data = data;
            TraceId = traceId;
        }
    }
}
//namespace Base.API.DTOs
//{
//    public class ApiResponseDTO
//    {
//        public int StatusCode { get; set; }
//        public string Message { get; set; } = string.Empty;
//        public object? Data { get; set; } = null;
//        public string TraceId { get; set; }

//        public ApiResponseDTO()
//        {
//            TraceId = Guid.NewGuid().ToString();
//        }

//        public ApiResponseDTO(int statusCode, string message, object? data = null)
//        {
//            StatusCode = statusCode;
//            Message = message;
//            Data = data;
//            TraceId = Guid.NewGuid().ToString();
//        }
//    }
//}
