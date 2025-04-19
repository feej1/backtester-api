using System.Security.Policy;
using System.Text.Json;
using Backtesting.Models;
using Microsoft.VisualBasic;

namespace Backtesting.Clients
{


    public static class AlphaAdvantageRateLimitBypasser 
    {

        // random key so that if they get wise and see loads of requests bypassing a rate limit
        // they're not all from the same key
        // Keys are 16 digits of characters and numbers upper case
        public static string GetRandomApiKey()
        {
            var key = "";
            var random = new Random();
            for (var i = 0; i < 16; i++)
            {
                key += random.Next(0,2) == 1 ? GetRandomDigitCharacter() : GetRandomLetter();
            }
            return key;
        }

        // X-Forwarded-For : 8.8.8.8   will bypass any rate limit
        public static string GetRandomIpAddress()
        {
            // initailize to 200 because it was being denied ~230 for first number sometimes
            var ipString = GetRandomDigitCharacter(200) + ".";
            for (var i = 0; i < 2; i++)
            {
                ipString += GetRandomDigitCharacter(255) + ".";
            }
            return ipString += GetRandomDigitCharacter();
        }

        private static string GetRandomDigitCharacter(int max = 9)
        {
            var random = new Random();
            return random.Next(0, max + 1).ToString();
        }

        private static string GetRandomLetter()
        {
            int startingAscii = (int)'A';
            int endingAscii = startingAscii + 25;
            var random = new Random();
            return Convert.ToChar(random.Next(startingAscii, endingAscii + 1)).ToString();
        }

    }


}