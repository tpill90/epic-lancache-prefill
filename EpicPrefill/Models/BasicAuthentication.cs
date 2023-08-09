namespace EpicPrefill.Models
{
    //TODO document
    public static class BasicAuthentication
    {
        public static AuthenticationHeaderValue ToAuthenticationHeader(string username, string password)
        {
            string authenticationString = $"{username}:{password}";
            byte[] inArray = Encoding.ASCII.GetBytes(authenticationString);
            var base64EncodedAuthenticationString = System.Convert.ToBase64String(inArray);

            return new AuthenticationHeaderValue("Basic", base64EncodedAuthenticationString);
        }
    }
}
