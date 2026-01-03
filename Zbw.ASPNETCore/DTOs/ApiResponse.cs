namespace Zbw.ASPNETCore.DTOs
{
    /// <summary>
    /// 统一API响应格式
    /// ApiResponse:

    /// 这是一个泛型类，用于统一实现 API 的响应格式。ApiResponse 通常包含以下几个部分：
    /// 状态（如成功或失败）
    /// 消息（关于操作的描述）
    /// 数据（可选，存储实际返回的数据，如用户信息）
    /// </summary>
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public string? ErrorCode { get; set; }

        /*这里ApiResponse<T>的原因如下：
         * ApiResponse 是一个通用的响应封装类，UserResponse 是这个封装类的类型参数，表示具体的数据类型。
         * ApiResponse<UserResponse> 表示返回的结果包含是否成功、消息和数据（在成功情况下为 UserResponse 对象）。
         * 注册失败:
         * {
            "success": false,
            "message": "用户名已存在",
            "data": null
            }
            注册成功:
            {
            "success": true,
            "message": "注册成功",
            "data": {
                "userName": "exampleUser",
                "email": "example@example.com",
                "id": "12345678-1234-1234-1234-1234567890ab"
                    }
            }
         */
        public static ApiResponse<T> SuccessResult(T data, string message = "操作成功")
        {
            return new ApiResponse<T>
            {
                Success = true,
                Message = message,
                Data = data
            };
        }

        public static ApiResponse<T> ErrorResult(string message, string? errorCode = null)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message,
                ErrorCode = errorCode
            };
        }
    }

    /// <summary>
    /// 无数据的API响应
    /// </summary>
    public class ApiResponse : ApiResponse<object?>

    {
        public static ApiResponse SuccessResult(string message = "操作成功")  // ← 改名
        {
            return new ApiResponse
            {
                Success = true,
                Message = message
            };
        }

        /// <summary>
        /// 加new的原因是为了覆盖基类的ErrorResult方法，
        /// "咦，子类有个方法和基类签名一样，但返回类型不同...
        ///这是程序员的失误，还是有意为之？
        ///我要给个警告提醒一下！"
        /// </summary>
        public static new ApiResponse ErrorResult(string message, string? errorCode = null)  // ← 改名
        {
            return new ApiResponse
            {
                Success = false,
                Message = message,
                ErrorCode = errorCode
            };
        }
    }
}