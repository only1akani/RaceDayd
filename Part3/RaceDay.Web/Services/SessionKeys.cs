namespace RaceDay.Web.Services
{//start of namespace

    //the names used to store things in the website's session
    public static class SessionKeys
    {//start of class

        //the session cookie that the API gave us, sent back with every API call
        public const string ApiCookie = "ApiCookie";
        public const string UserId = "UserId";
        public const string Role = "Role";
        public const string FullName = "FullName";

    }//end of class
}//end of namespace