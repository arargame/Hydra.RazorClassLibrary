using System;
using System.Globalization;
using Hydra.DataModels;
using Hydra.DTOs;

namespace Hydra.RazorClassLibrary.Utils
{
    /// <summary>
    /// UI genelinde tarih/saat alanlarını kullanıcı dostu, kompakt ve zarif formatta
    /// sunan yardımcı sınıf. Uzun ISO/milisaniye dökümlerini okunabilir hale getirir.
    /// </summary>
    public static class DateTimeHelper
    {
        private static readonly string[] IsoFormats =
        {
            "yyyy-MM-ddTHH:mm:ss.fffffff",
            "yyyy-MM-ddTHH:mm:ss.ffffff",
            "yyyy-MM-ddTHH:mm:ss.fff",
            "yyyy-MM-ddTHH:mm:ss",
            "yyyy-MM-dd HH:mm:ss.fffffff",
            "yyyy-MM-dd HH:mm:ss.fff",
            "yyyy-MM-dd HH:mm:ss",
            "O", "s", "u"
        };

        /// <summary>
        /// DateTime veya ham string değeri '07 Nov 2026, 03:51' benzeri okunabilir formata dönüştürür.
        /// </summary>
        public static string FormatFriendly(object? value, string format = "dd MMM yyyy, HH:mm")
        {
            if (value == null)
                return "—";

            if (value is DateTime dt)
                return dt.ToString(format, CultureInfo.InvariantCulture);

            if (value is DateTimeOffset dto)
                return dto.LocalDateTime.ToString(format, CultureInfo.InvariantCulture);

            var raw = value.ToString()?.Trim();
            if (string.IsNullOrWhiteSpace(raw))
                return "—";

            if (DateTime.TryParseExact(raw, IsoFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
                return parsed.ToString(format, CultureInfo.InvariantCulture);

            if (DateTime.TryParse(raw, CultureInfo.InvariantCulture, DateTimeStyles.None, out parsed) ||
                DateTime.TryParse(raw, out parsed))
            {
                return parsed.ToString(format, CultureInfo.InvariantCulture);
            }

            return raw;
        }

        /// <summary>
        /// Tooltip veya pano kopyalama için mikro/milisaniyeyi de içeren tam ISO dizesi üretir.
        /// </summary>
        public static string FormatIsoPrecise(object? value)
        {
            if (value == null)
                return string.Empty;

            if (value is DateTime dt)
                return dt.ToString("yyyy-MM-ddTHH:mm:ss.fffffff", CultureInfo.InvariantCulture);

            if (value is DateTimeOffset dto)
                return dto.ToString("yyyy-MM-ddTHH:mm:ss.fffffffzzz", CultureInfo.InvariantCulture);

            return value.ToString() ?? string.Empty;
        }

        /// <summary>
        /// Kolonun tarih/saat alanı olup olmadığını saptar.
        /// </summary>
        public static bool IsDateTimeColumn(MetaColumnDTO column)
        {
            if (column.ValueType == ColumnValueType.DateTime)
                return true;

            var name = column.Name ?? string.Empty;
            return name.EndsWith("Date", StringComparison.OrdinalIgnoreCase) ||
                   name.EndsWith("Time", StringComparison.OrdinalIgnoreCase) ||
                   name.Equals("CreatedOn", StringComparison.OrdinalIgnoreCase) ||
                   name.Equals("ModifiedOn", StringComparison.OrdinalIgnoreCase);
        }
    }
}
