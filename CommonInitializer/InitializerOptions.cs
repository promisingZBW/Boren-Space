namespace CommonInitializer
{
    /// <summary>
    /// 初始化选项
    /// </summary>
    public class InitializerOptions
    {
        public string LogFilePath { get; set; } = "logs/app.log";
        public string EventBusQueueName { get; set; } = "DefaultQueue";
    }
}