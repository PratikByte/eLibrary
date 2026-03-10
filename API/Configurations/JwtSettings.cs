namespace eLibrary.API.Configurations;

public class JwtSettings
{

        public string Key { get; set; } = string.Empty;       // 🔑 Fury’s secret signing pen
        public string Issuer { get; set; } = string.Empty;    // 🏛 Who issues the token (S.H.I.E.L.D.)
        public string Audience { get; set; } = string.Empty;  // 👥 Who the token is for (Avengers client)
        public int DurationInMinutes { get; set; }            // ⏳ Pass validity duration

}

