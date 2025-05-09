﻿using LibraryAPI.Entities;

namespace LibraryAPI.Utils
{
    public static class NameFormatter
    {
        public static string GetAuthorFullName(Author author)
        {
            var fullName = $"{author.Name} {author.Surname1}";

            if (!string.IsNullOrEmpty(author.Surname2))
                fullName += $" {author.Surname2}";

            return fullName;
        }
    }
}
