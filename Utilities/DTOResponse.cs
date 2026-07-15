namespace MPCRS.Utilities
{
    public class DTOResponse
    {
        public int SavedDBKey { get; set; }
        public string ResponseMessage { get; set; }
        public string ErrorMessage { get; set; }
        public bool Result { get; set; }

        public DTOResponse()
        {
            SavedDBKey = 0;
            ResponseMessage = "Success";
            Result = true;
            ErrorMessage = "";
        }
    }
}
