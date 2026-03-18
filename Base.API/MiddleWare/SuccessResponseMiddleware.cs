using System.Text.Json;

namespace Base.API.MiddleWare
{
    public class SuccessResponseMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<SuccessResponseMiddleware> _logger;

        public SuccessResponseMiddleware(RequestDelegate next, ILogger<SuccessResponseMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Path.StartsWithSegments("/hangfire"))
            {
                await _next(context);
                return;
            }

            var originalBody = context.Response.Body;
            await using var tempBody = new MemoryStream();
            context.Response.Body = tempBody;

            try
            {
                await _next(context);
            }
            catch
            {
                context.Response.Body = originalBody;
                throw;
            }

            tempBody.Seek(0, SeekOrigin.Begin);
            var bodyText = await new StreamReader(tempBody).ReadToEndAsync();

            if (context.Response.StatusCode >= 200 && context.Response.StatusCode < 300)
            {
                string traceId = context.TraceIdentifier ?? Guid.NewGuid().ToString();
                int statusCode = context.Response.StatusCode;
                string message = "Success";

                var finalDict = new Dictionary<string, object?>
                {
                    ["statusCode"] = statusCode,
                    ["message"] = message,
                    ["traceId"] = traceId
                };

                if (!string.IsNullOrWhiteSpace(bodyText))
                {
                    try
                    {
                        using var doc = JsonDocument.Parse(bodyText);
                        var root = doc.RootElement;

                        if (root.ValueKind == JsonValueKind.Object)
                        {
                            var dict = new Dictionary<string, object?>();

                            foreach (var prop in root.EnumerateObject())
                            {
                                var name = prop.Name.ToLower();

                                if (name == "statuscode")
                                {
                                    statusCode = prop.Value.GetInt32();
                                    finalDict["statusCode"] = statusCode;
                                }
                                else if (name == "message")
                                {
                                    message = prop.Value.GetString() ?? message;
                                    finalDict["message"] = message;
                                }
                                else
                                {
                                    dict[prop.Name] = JsonSerializer.Deserialize<object>(
                                        prop.Value.GetRawText());
                                }
                            }

                            bool handled = false;

                            // ── 1. Check for explicit "value" key first ───────────────
                            var valueMatch = dict.Keys.FirstOrDefault(
                                x => x.Equals("value", StringComparison.OrdinalIgnoreCase));

                            if (valueMatch != null)
                            {
                                finalDict["value"] = dict[valueMatch];
                                dict.Remove(valueMatch);

                                foreach (var kv in dict)
                                    finalDict[kv.Key] = kv.Value;

                                handled = true;
                            }

                            // ── 2. Check for "data" or "result" keys ─────────────────
                            if (!handled)
                            {
                                string[] flattenKeys = { "data", "result" };

                                foreach (var key in flattenKeys)
                                {
                                    var match = dict.Keys.FirstOrDefault(
                                        x => x.Equals(key, StringComparison.OrdinalIgnoreCase));

                                    if (match == null) continue;

                                    var raw = dict[match];

                                    if (raw is JsonElement je)
                                    {
                                        if (je.ValueKind == JsonValueKind.Object)
                                        {
                                            // Object under "data" → flatten to root
                                            var flat = JsonSerializer.Deserialize<
                                                Dictionary<string, object?>>(je.GetRawText());

                                            if (flat != null)
                                            {
                                                dict.Remove(match);
                                                foreach (var kv in dict)
                                                    flat[kv.Key] = kv.Value;
                                                foreach (var kv in flat)
                                                    finalDict[kv.Key] = kv.Value;

                                                handled = true;
                                                break;
                                            }
                                        }
                                        else if (je.ValueKind == JsonValueKind.Array)
                                        {
                                            // Array under "data" → keep under "data" key
                                            finalDict[match] = JsonSerializer.Deserialize<object>(
                                                je.GetRawText());
                                            dict.Remove(match);

                                            foreach (var kv in dict)
                                                finalDict[kv.Key] = kv.Value;

                                            handled = true;
                                            break;
                                        }
                                        else
                                        {
                                            // Primitive under "data" → keep as-is
                                            finalDict[match] = JsonSerializer.Deserialize<object>(
                                                je.GetRawText());
                                            dict.Remove(match);

                                            foreach (var kv in dict)
                                                finalDict[kv.Key] = kv.Value;

                                            handled = true;
                                            break;
                                        }
                                    }
                                    else if (raw is Dictionary<string, object?> d2)
                                    {
                                        dict.Remove(match);
                                        foreach (var kv in dict)
                                            d2[kv.Key] = kv.Value;
                                        foreach (var kv in d2)
                                            finalDict[kv.Key] = kv.Value;

                                        handled = true;
                                        break;
                                    }
                                }
                            }

                            // ── 3. No known wrapper key — wrap entire object in "value"
                            if (!handled)
                            {
                                finalDict["value"] = JsonSerializer.Deserialize<object>(bodyText);
                            }
                        }
                        else if (root.ValueKind == JsonValueKind.Array)
                        {
                            // Root array → wrap inside "value"
                            finalDict["value"] = JsonSerializer.Deserialize<object>(bodyText);
                        }
                        else
                        {
                            // Primitive → wrap inside "value"
                            finalDict["value"] = JsonSerializer.Deserialize<object>(bodyText);
                        }
                    }
                    catch
                    {
                        finalDict["value"] = bodyText.Trim('"');
                    }
                }

                var output = JsonSerializer.Serialize(finalDict, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = true
                });

                context.Response.Body = originalBody;
                context.Response.ContentType = "application/json; charset=utf-8";
                await context.Response.WriteAsync(output);
            }
            else
            {
                tempBody.Seek(0, SeekOrigin.Begin);
                await tempBody.CopyToAsync(originalBody);
            }
        }
    }
}





//using System.Text.Json;

//namespace Base.API.MiddleWare
//{
//    public class SuccessResponseMiddleware
//    {
//        private readonly RequestDelegate _next;
//        private readonly ILogger<SuccessResponseMiddleware> _logger;

//        public SuccessResponseMiddleware(RequestDelegate next, ILogger<SuccessResponseMiddleware> logger)
//        {
//            _next = next;
//            _logger = logger;
//        }

//        public async Task InvokeAsync(HttpContext context)
//        {
//            if (context.Request.Path.StartsWithSegments("/hangfire"))
//            {
//                await _next(context);
//                return;
//            }

//            var originalBody = context.Response.Body;
//            await using var tempBody = new MemoryStream();
//            context.Response.Body = tempBody;

//            try
//            {
//                await _next(context);
//            }
//            catch
//            {
//                context.Response.Body = originalBody;
//                throw;
//            }

//            tempBody.Seek(0, SeekOrigin.Begin);
//            var bodyText = await new StreamReader(tempBody).ReadToEndAsync();

//            if (context.Response.StatusCode >= 200 && context.Response.StatusCode < 300)
//            {
//                string traceId = context.TraceIdentifier ?? Guid.NewGuid().ToString();
//                int statusCode = context.Response.StatusCode;
//                string message = "Success";

//                var finalDict = new Dictionary<string, object?>
//                {
//                    ["statusCode"] = statusCode,
//                    ["message"] = message,
//                    ["traceId"] = traceId
//                };

//                if (!string.IsNullOrWhiteSpace(bodyText))
//                {
//                    try
//                    {
//                        using var doc = JsonDocument.Parse(bodyText);
//                        var root = doc.RootElement;

//                        if (root.ValueKind == JsonValueKind.Object)
//                        {
//                            var dict = new Dictionary<string, object?>();

//                            foreach (var prop in root.EnumerateObject())
//                            {
//                                var name = prop.Name.ToLower();

//                                if (name == "statuscode")
//                                {
//                                    statusCode = prop.Value.GetInt32();
//                                    finalDict["statusCode"] = statusCode;
//                                }
//                                else if (name == "message")
//                                {
//                                    message = prop.Value.GetString() ?? message;
//                                    finalDict["message"] = message;
//                                }
//                                else
//                                {
//                                    dict[prop.Name] = JsonSerializer.Deserialize<object>(prop.Value.GetRawText());
//                                }
//                            }

//                            // ── Flatten "data" or "result" keys ──────────────────────
//                            // Handles three cases:
//                            //   1. data is an OBJECT  → flatten its properties to root
//                            //   2. data is an ARRAY   → keep it as-is under its original key
//                            //   3. no data key        → add all remaining dict keys to root
//                            string[] flattenKeys = { "data", "result" };
//                            bool flattened = false;

//                            foreach (var key in flattenKeys)
//                            {
//                                var match = dict.Keys.FirstOrDefault(
//                                    x => x.Equals(key, StringComparison.OrdinalIgnoreCase));

//                                if (match == null) continue;

//                                var raw = dict[match];

//                                if (raw is JsonElement je)
//                                {
//                                    if (je.ValueKind == JsonValueKind.Object)
//                                    {
//                                        // ✅ Case 1: array is an object — flatten it
//                                        var flat = JsonSerializer.Deserialize<Dictionary<string, object?>>(
//                                            je.GetRawText());

//                                        if (flat != null)
//                                        {
//                                            dict.Remove(match);

//                                            // Add remaining dict keys into flat
//                                            foreach (var kv in dict)
//                                                flat[kv.Key] = kv.Value;

//                                            // Add flat into finalDict
//                                            foreach (var kv in flat)
//                                                finalDict[kv.Key] = kv.Value;

//                                            flattened = true;
//                                            break;
//                                        }
//                                    }
//                                    else if (je.ValueKind == JsonValueKind.Array)
//                                    {
//                                        // ✅ Case 2: data is an ARRAY — keep it under its key
//                                        // Do NOT flatten — just add it as-is
//                                        finalDict[match] = JsonSerializer.Deserialize<object>(je.GetRawText());
//                                        dict.Remove(match);

//                                        // Add remaining dict keys to finalDict
//                                        foreach (var kv in dict)
//                                            finalDict[kv.Key] = kv.Value;

//                                        flattened = true;
//                                        break;
//                                    }
//                                    else
//                                    {
//                                        // ✅ Null, bool, number, string — keep as-is
//                                        finalDict[match] = JsonSerializer.Deserialize<object>(je.GetRawText());
//                                        dict.Remove(match);

//                                        foreach (var kv in dict)
//                                            finalDict[kv.Key] = kv.Value;

//                                        flattened = true;
//                                        break;
//                                    }
//                                }
//                                else if (raw is Dictionary<string, object?> d2)
//                                {
//                                    dict.Remove(match);
//                                    foreach (var kv in dict)
//                                        d2[kv.Key] = kv.Value;
//                                    foreach (var kv in d2)
//                                        finalDict[kv.Key] = kv.Value;

//                                    flattened = true;
//                                    break;
//                                }
//                            }

//                            if (!flattened)
//                            {
//                                // No data/result key — add everything to finalDict directly
//                                foreach (var kv in dict)
//                                    finalDict[kv.Key] = kv.Value;
//                            }
//                        }
//                        else if (root.ValueKind == JsonValueKind.Array)
//                        {
//                            // Root is an array — wrap it as "value"
//                            finalDict["value"] = JsonSerializer.Deserialize<object>(bodyText);
//                        }
//                        else
//                        {
//                            // Primitive
//                            finalDict["value"] = JsonSerializer.Deserialize<object>(bodyText);
//                        }
//                    }
//                    catch
//                    {
//                        finalDict["value"] = bodyText.Trim('"');
//                    }
//                }

//                var output = JsonSerializer.Serialize(finalDict, new JsonSerializerOptions
//                {
//                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
//                    WriteIndented = true
//                });

//                context.Response.Body = originalBody;
//                context.Response.ContentType = "application/json; charset=utf-8";
//                await context.Response.WriteAsync(output);
//            }
//            else
//            {
//                tempBody.Seek(0, SeekOrigin.Begin);
//                await tempBody.CopyToAsync(originalBody);
//            }
//        }
//    }
//}










//using System.Text.Json;

//namespace Base.API.MiddleWare
//{
//    public class SuccessResponseMiddleware
//    {
//        private readonly RequestDelegate _next;
//        private readonly ILogger<SuccessResponseMiddleware> _logger;

//        public SuccessResponseMiddleware(RequestDelegate next, ILogger<SuccessResponseMiddleware> logger)
//        {
//            _next = next;
//            _logger = logger;
//        }

//        public async Task InvokeAsync(HttpContext context)
//        {
//            if (context.Request.Path.StartsWithSegments("/hangfire"))
//            {
//                await _next(context);
//                return;
//            }

//            var originalBody = context.Response.Body;
//            await using var tempBody = new MemoryStream();
//            context.Response.Body = tempBody;

//            try
//            {
//                await _next(context);
//            }
//            catch
//            {
//                context.Response.Body = originalBody;
//                throw;
//            }

//            tempBody.Seek(0, SeekOrigin.Begin);
//            var bodyText = await new StreamReader(tempBody).ReadToEndAsync();

//            if (context.Response.StatusCode >= 200 && context.Response.StatusCode < 300)
//            {
//                string traceId = context.TraceIdentifier ?? Guid.NewGuid().ToString();
//                int statusCode = context.Response.StatusCode;
//                string message = "Success";

//                var finalDict = new Dictionary<string, object?>
//                {
//                    ["statusCode"] = statusCode,
//                    ["message"] = message,
//                    ["traceId"] = traceId
//                };

//                if (!string.IsNullOrWhiteSpace(bodyText))
//                {
//                    try
//                    {
//                        using var doc = JsonDocument.Parse(bodyText);
//                        var root = doc.RootElement;

//                        if (root.ValueKind == JsonValueKind.Object)
//                        {
//                            var dict = new Dictionary<string, object?>();

//                            foreach (var prop in root.EnumerateObject())
//                            {
//                                var name = prop.Name.ToLower();

//                                if (name == "statuscode")
//                                {
//                                    statusCode = prop.Value.GetInt32();
//                                    finalDict["statusCode"] = statusCode;
//                                }
//                                else if (name == "message")
//                                {
//                                    message = prop.Value.GetString() ?? message;
//                                    finalDict["message"] = message;
//                                }
//                                else
//                                {
//                                    dict[prop.Name] = JsonSerializer.Deserialize<object>(prop.Value.GetRawText());
//                                }
//                            }

//                            // flatten data/result if exists
//                            string[] flattenKeys = { "data", "result" };
//                            Dictionary<string, object?>? flat = null;

//                            foreach (var key in flattenKeys)
//                            {
//                                var match = dict.Keys.FirstOrDefault(x => x.Equals(key, StringComparison.OrdinalIgnoreCase));
//                                if (match != null)
//                                {
//                                    var raw = dict[match];

//                                    if (raw is JsonElement je && je.ValueKind == JsonValueKind.Object)
//                                    {
//                                        flat = JsonSerializer.Deserialize<Dictionary<string, object?>>(je.GetRawText());
//                                    }
//                                    else if (raw is Dictionary<string, object?> d2)
//                                    {
//                                        flat = d2;
//                                    }

//                                    dict.Remove(match);
//                                    break;
//                                }
//                            }

//                            if (flat != null)
//                            {
//                                foreach (var kv in dict)
//                                    flat[kv.Key] = kv.Value;

//                                foreach (var kv in flat)
//                                    finalDict[kv.Key] = kv.Value;
//                            }
//                            else
//                            {
//                                foreach (var kv in dict)
//                                    finalDict[kv.Key] = kv.Value;
//                            }
//                        }
//                        else
//                        {
//                            // primitive/array → return as value
//                            finalDict["value"] = JsonSerializer.Deserialize<object>(bodyText);
//                        }
//                    }
//                    catch
//                    {
//                        finalDict["value"] = bodyText.Trim('"');
//                    }
//                }

//                var output = JsonSerializer.Serialize(finalDict, new JsonSerializerOptions
//                {
//                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
//                    WriteIndented = true
//                });

//                context.Response.Body = originalBody;
//                context.Response.ContentType = "application/json; charset=utf-8";
//                await context.Response.WriteAsync(output);
//            }
//            else
//            {
//                tempBody.Seek(0, SeekOrigin.Begin);
//                await tempBody.CopyToAsync(originalBody);
//            }
//        }
//    }
//}



///*using Base.API.DTOs;
//using System.Text.Json;

//namespace Base.API.MiddleWare
//{
//    public class SuccessResponseMiddleware
//    {
//        private readonly RequestDelegate _next;
//        private readonly ILogger<SuccessResponseMiddleware> _logger;

//        public SuccessResponseMiddleware(RequestDelegate next, ILogger<SuccessResponseMiddleware> logger)
//        {
//            _next = next;
//            _logger = logger;
//        }

//        public async Task InvokeAsync(HttpContext context)
//        {
//            if (context.Request.Path.StartsWithSegments("/hangfire"))
//            {
//                await _next(context);
//                return;
//            }

//            var originalBodyStream = context.Response.Body;
//            await using var responseBody = new MemoryStream();
//            context.Response.Body = responseBody;

//            try
//            {
//                await _next(context);
//            }
//            catch
//            {
//                context.Response.Body = originalBodyStream;
//                throw;
//            }

//            responseBody.Seek(0, SeekOrigin.Begin);
//            var bodyText = await new StreamReader(responseBody).ReadToEndAsync();

//            if (context.Response.StatusCode >= 200 && context.Response.StatusCode < 300)
//            {
//                string traceId = context.TraceIdentifier ?? Guid.NewGuid().ToString();
//                int statusCode = context.Response.StatusCode;
//                string message = "Success";
//                object? data = null;

//                if (!string.IsNullOrWhiteSpace(bodyText))
//                {
//                    try
//                    {
//                        using var doc = JsonDocument.Parse(bodyText);
//                        var root = doc.RootElement;
//                        if (root.ValueKind == JsonValueKind.Object)
//                        {
//                            var dict = new Dictionary<string, object?>();

//                            foreach (var prop in root.EnumerateObject())
//                            {
//                                var propName = prop.Name.ToLower();

//                                if (propName == "statuscode")
//                                    statusCode = prop.Value.GetInt32();

//                                else if (propName == "message")
//                                    message = prop.Value.GetString() ?? message;

//                                else
//                                    dict[prop.Name] = JsonSerializer.Deserialize<object>(prop.Value.GetRawText());
//                            }

//                            // keys to flatten
//                            var flattenKeys = new[] { "data", "result" };

//                            object? flatData = null;

//                            foreach (var key in flattenKeys)
//                            {
//                                var match = dict.Keys.FirstOrDefault(k =>
//                                    k.Equals(key, StringComparison.OrdinalIgnoreCase));

//                                if (match != null)
//                                {
//                                    var elem = dict[match];

//                                    // 1) لو elem JsonElement → نفكه
//                                    if (elem is JsonElement je)
//                                    {
//                                        if (je.ValueKind == JsonValueKind.Object)
//                                        {
//                                            flatData = JsonSerializer.Deserialize<Dictionary<string, object?>>(
//                                                je.GetRawText()
//                                            );
//                                        }
//                                    }
//                                    // 2) لو elem Dictionary بالفعل
//                                    else if (elem is Dictionary<string, object?> innerDict)
//                                    {
//                                        flatData = innerDict;
//                                    }

//                                    if (flatData != null)
//                                    {
//                                        break;
//                                    }
//                                }
//                            }

//                            data = flatData ?? dict;
//                        }

//                        // لو response هو JSON object
//                        //if (root.ValueKind == JsonValueKind.Object)
//                        //{
//                        //    var dict = new Dictionary<string, object?>();

//                        //    foreach (var prop in root.EnumerateObject())
//                        //    {
//                        //        // شيل statusCode و message من البيانات
//                        //        if (prop.NameEquals("statusCode"))
//                        //            statusCode = prop.Value.GetInt32();
//                        //        else if (prop.NameEquals("message"))
//                        //            message = prop.Value.GetString() ?? message;
//                        //        else
//                        //            dict[prop.Name] = JsonSerializer.Deserialize<object>(prop.Value.GetRawText());
//                        //    }

//                        //    data = dict;
//                        //}
//                        else
//                        {
//                            // لو مش object (مثلا array) نحطه كله في data
//                            data = JsonSerializer.Deserialize<object>(bodyText);
//                        }
//                    }
//                    catch
//                    {
//                        message = bodyText.Trim('"', '\'');
//                    }
//                }

//                var apiResponse = new
//                {
//                    statusCode,
//                    message,
//                    traceId,
//                    data
//                };

//                var jsonResponse = JsonSerializer.Serialize(apiResponse, new JsonSerializerOptions
//                {
//                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
//                    WriteIndented = true
//                });

//                await WriteResponseAsync(context, originalBodyStream, jsonResponse);
//            }
//            else
//            {
//                responseBody.Seek(0, SeekOrigin.Begin);
//                await responseBody.CopyToAsync(originalBodyStream);
//            }
//        }

//        private static async Task WriteResponseAsync(HttpContext context, Stream originalBodyStream, string jsonResponse)
//        {
//            context.Response.Body = originalBodyStream;
//            context.Response.ContentType = "application/json; charset=utf-8";
//            context.Response.ContentLength = System.Text.Encoding.UTF8.GetByteCount(jsonResponse);
//            await context.Response.WriteAsync(jsonResponse);
//        }
//    }
//}

//*/

///*using Base.API.DTOs;
//using System.Text.Json;

//namespace Base.API.MiddleWare
//{
//    public class SuccessResponseMiddleware
//    {
//        private readonly RequestDelegate _next;
//        private readonly ILogger<SuccessResponseMiddleware> _logger;

//        public SuccessResponseMiddleware(RequestDelegate next, ILogger<SuccessResponseMiddleware> logger)
//        {
//            _next = next;
//            _logger = logger;
//        }

//        public async Task InvokeAsync(HttpContext context)
//        {
//            // تخطي أي شيء يبدأ بـ /hangfire
//            if (context.Request.Path.StartsWithSegments("/hangfire"))
//            {
//                await _next(context);
//                return;
//            }
//            var originalBodyStream = context.Response.Body;
//            using var responseBody = new MemoryStream();
//            context.Response.Body = responseBody;

//            try
//            {
//                await _next(context);
//            }
//            catch
//            {
//                context.Response.Body = originalBodyStream;
//                throw;
//            }

//            responseBody.Seek(0, SeekOrigin.Begin);
//            var bodyText = await new StreamReader(responseBody).ReadToEndAsync();

//            // ✅ فقط لو 2xx
//            if (context.Response.StatusCode >= 200 && context.Response.StatusCode < 300)
//            {
//                string traceId = context.TraceIdentifier ?? Guid.NewGuid().ToString();
//                object? data = null;
//                string? message = null;

//                if (!string.IsNullOrWhiteSpace(bodyText))
//                {
//                    try
//                    {
//                        using var doc = JsonDocument.Parse(bodyText);
//                        if (doc.RootElement.TryGetProperty("statusCode", out _))
//                        {
//                            var json = JsonSerializer.Deserialize<Dictionary<string, object>>(bodyText);
//                            if (json != null)
//                            {
//                                if (!json.ContainsKey("traceId"))
//                                    json["traceId"] = traceId;

//                                var modifiedJson = JsonSerializer.Serialize(json,
//                                    new JsonSerializerOptions
//                                    {
//                                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
//                                        WriteIndented = true
//                                    });

//                                context.Response.Body = originalBodyStream;
//                                context.Response.ContentType = "application/json";
//                                await context.Response.WriteAsync(modifiedJson);
//                                return;
//                            }
//                        }
//                        data = JsonSerializer.Deserialize<object>(bodyText);
//                    }
//                    catch
//                    {
//                        message = bodyText.Trim('"', '\'');
//                    }
//                }

//                message ??= "Success";

//                var apiResponse = new ApiResponseDTO(
//                    statusCode: context.Response.StatusCode,
//                    message: message,
//                    data: data
//                )
//                {
//                    TraceId = traceId
//                };

//                var jsonResponse = JsonSerializer.Serialize(apiResponse, new JsonSerializerOptions
//                {
//                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
//                    WriteIndented = true
//                });

//                context.Response.Body = originalBodyStream;
//                context.Response.ContentType = "application/json";
//                await context.Response.WriteAsync(jsonResponse);
//            }
//            else
//            {
//                responseBody.Seek(0, SeekOrigin.Begin);
//                await responseBody.CopyToAsync(originalBodyStream);
//            }
//        }
//    }

//}*/
