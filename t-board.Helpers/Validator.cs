using System;

namespace t_board.Helpers
{
    public static class Validator
    {
        public static bool IsValidUri(string uri)
        {
            Uri validatedUri;
            return Uri.TryCreate(uri, UriKind.RelativeOrAbsolute, out validatedUri);
        }
    }

}