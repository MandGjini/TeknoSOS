using System.Collections.Generic;

namespace TeknoSOS.WebApp.Data.Seed
{
    public static class BlogSeedDataEn
    {
        public static List<(string Title, string Content, string CallToAction, string ImageUrl, string Category, string Tags)> GetPosts()
        {
            var posts = new List<(string, string, string, string, string, string)>();

            posts.Add((
                "10 Smart Tricks to Lower Your Energy Bill",
                "Discover 10 proven tricks to reduce your bills: from managing heating to simple home energy-saving routines.",
                "Try these tips and check your savings next month.",
                "https://picsum.photos/seed/energy-tricks-en/600/400",
                "Electricity",
                "tips,energy,saving"
            ));

            posts.Add((
                "3 Tricks to Install an EV Charger in 30 Minutes",
                "Learn how to prepare your electrical setup, pick the right outlet, and ensure fast, safe charging.",
                "Read these tips and make the install flawless.",
                "https://picsum.photos/seed/ev-tricks-en/600/400",
                "EV Mobility",
                "ev,charger,tips"
            ));

            posts.Add((
                "7 Quick Tricks to Spot Water Leaks",
                "Uncover hidden signs of leaks and save money by acting on the first signs of moisture.",
                "Get TeknoSOS guidance to stop the leak today.",
                "https://picsum.photos/seed/plumbing-tricks-en/600/400",
                "Plumbing",
                "water,leak,tips"
            ));

            // Create posts 4..16 mirroring the local-language style but in English
            for (int i = 4; i <= 16; i++)
            {
                var title = $"Essential Trick #{i}: How to Boost Safety and Save Costs";
                var content = "Discover a practical trick that improves performance, cuts costs, and fixes issues without calling a pro right away.";
                var cta = "Open the article and learn pro-level maintenance secrets.";
                var imageUrl = $"https://picsum.photos/seed/home-trick-en-{i}/600/400";
                var category = "Home & Tech";
                var tags = "tips,home,safety,saving";

                posts.Add((title, content, cta, imageUrl, category, tags));
            }

            return posts;
        }
    }
}
