namespace PetCareConnect.Helpers
{
    public static class PetHelper
    {
        public static string GetAvatarEmoji(string breed)
        {
            var b = breed.ToLower();
            if (b.Contains("cat") || b.Contains("kitten") || b.Contains("tabby") || b.Contains("persian") || b.Contains("siamese"))
                return "🐱";
            if (b.Contains("rabbit") || b.Contains("bunny"))
                return "🐰";
            if (b.Contains("bird") || b.Contains("parrot") || b.Contains("canary"))
                return "🐦";
            if (b.Contains("fish") || b.Contains("goldfish") || b.Contains("betta"))
                return "🐟";
            if (b.Contains("hamster") || b.Contains("gerbil") || b.Contains("guinea"))
                return "🐹";
            if (b.Contains("turtle") || b.Contains("tortoise"))
                return "🐢";
            if (b.Contains("snake") || b.Contains("lizard") || b.Contains("gecko"))
                return "🦎";
            // Default to dog
            return "🐶";
        }

        public static string GetAvatarColor(string name)
        {
            // Deterministic pastel based on first char
            var colors = new[]
            {
                "linear-gradient(135deg, #4A7C59, #6BA07A)",
                "linear-gradient(135deg, #E8A87C, #d4845a)",
                "linear-gradient(135deg, #6B7ED4, #4A5BBF)",
                "linear-gradient(135deg, #C084FC, #9333EA)",
                "linear-gradient(135deg, #FB923C, #EA580C)",
                "linear-gradient(135deg, #34D399, #059669)",
                "linear-gradient(135deg, #F472B6, #DB2777)",
                "linear-gradient(135deg, #60A5FA, #2563EB)",
            };
            var index = string.IsNullOrEmpty(name) ? 0 : name[0] % colors.Length;
            return colors[index];
        }
    }
}
