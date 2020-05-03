public class ErrorHandler
    {
        // Critical addresses: You can use this variable to specified locations that is critical for any type of errors like authentication or authorization locations
        public static readonly List<string> CriticalNames = new List<string>() { "Default/Index" };

        // Fill Error Data table properties from exception details
        private static TblErrorLogs fillError(HttpContext context,Exception ex)
        {
            TblErrorLogs error = new TblErrorLogs();
            
            error.ErrorCode = (HttpStatusCode)Enum.Parse(typeof(HttpStatusCode), context.Response.StatusCode.ToString());
            error.Message = ex.Message;
            error.StackTrace = ex.StackTrace;
            error.OccurDate = DateTime.Now;
            error.ErrorTitle = ex.Source;
            
            // When execption parameter is null it may 404 or some http error occurred (i have set error type to 404 by default)
            if (ex == null)
            {
                error.ErrorType = ErrorTypes.NotFound;
            }
            else if (Utility.CriticalNames.Where(prop => context.Request.Path.ToString().Contains(prop)).Any())
            {
                error.ErrorType = ErrorTypes.Critical;
            }
            else
            {
                error.ErrorType = ErrorTypes.RunTime;
            }
            error.IpAddress = context.Connection.RemoteIpAddress.ToString();
            error.OccurAddress = context.Request.Path;
            if (context.User.Identity.IsAuthenticated)
            {
                error.UserId = long.Parse(context.User.Claims.First(prop => prop.Type == ClaimTypes.NameIdentifier).Value);
            }
            return error;
        }
        private static string LogFileFormat(TblErrorLogs errorLog)
        {
            string output = $"----------------- {errorLog.ErrorCode.ToString()} - {errorLog.ErrorType} -----------------\n" +
                $"Error Id:         {errorLog.Id}\n" +
                $"Error Title:      {errorLog.ErrorTitle}\n" +
                $"Occur Date:       {errorLog.OccurDate.ToString("yyyy/MM/dd H:m:s")}\n" +
                $"Occur Address:    {errorLog.OccurAddress}\n" +
                $"Ip Address:       {errorLog.IpAddress}\n" +
                $"User Id:          {(errorLog.UserId != null ? errorLog.UserId.ToString() : "Not logined")}\n" +
                $"Error Message:    {errorLog.Message}\n" +
                $"Stack trace:      {errorLog.StackTrace}";
            return output;
        }
        private static string LogXmlFormat(TblErrorLogs errorLog)
        {
            XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();
            xmlWriterSettings.Indent = true;
            xmlWriterSettings.IndentChars = ("    ");
            xmlWriterSettings.CloseOutput = true;
            xmlWriterSettings.OmitXmlDeclaration = true;
            StringBuilder outputBuilder = new StringBuilder();
            using (XmlWriter writer = XmlWriter.Create(outputBuilder,xmlWriterSettings))
            {
                writer.WriteStartElement("Error");
                writer.WriteAttributeString("OccurLocation", errorLog.OccurAddress);
                writer.WriteAttributeString("OccurDate", errorLog.OccurDate.ToString("yyyy/MM/dd H:m:s"));
                writer.WriteAttributeString("ErrorType", errorLog.ErrorType.ToString());
                writer.WriteElementString("ErrorCode", errorLog.ErrorCode.ToString());
                writer.WriteElementString("UserId", (errorLog.UserId!=null?errorLog.UserId.ToString():"Not logined"));
                writer.WriteElementString("IpAddress",errorLog.IpAddress );
                writer.WriteElementString("ErrorTitle", errorLog.ErrorTitle);
                writer.WriteElementString("ErrorMessage", errorLog.Message);
                writer.WriteElementString("StackTrace", errorLog.StackTrace);
                writer.WriteEndElement();
                writer.Flush();
            }
            return outputBuilder.ToString();
        }
        public static void LogExceptionFile(HttpContext context, Exception ex, bool inXmlFormat = false,string filePath="errorLog/error_{0}.log")
        {
            //Don't forget to set restricted permission or restricted location for log file to protect from direct users access.
            string fileName = string.Format(filePath, DateTime.Now.ToString("yyyyMMddHHmmss") + "_" + new Random().Next(100, 999));
            TblErrorLogs error = fillError(context, ex);
            if (inXmlFormat)
            {
                File.WriteAllText(filePath, LogXmlFormat(error));
            }
            else
            {
                File.WriteAllText(filePath, LogFileFormat(error));
            }
            context.Response.Redirect("/Error/Index");
        }
        public static void LogExceptionDb(HttpContext context, Exception ex)
        {
            // I have repository design pattern and i has registered my UnitOfWork services in startup.cs
            // Also you can use your DbContext directly or any strategy you have for store data in database 
            var unitOfWork = (IUnitOfWork)context.RequestServices.GetService(typeof(IUnitOfWork));

            TblErrorLogs error = fillError(context, ex);

            unitOfWord.tblErrorLog.Insert(error);
            unitOfWord.tblErrorLog.Save();
            context.Response.Redirect("/Error/Index");
        }
    }
