using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OrderManagement.Api.Serialization;

/// <summary>
/// تاریخ‌ها را در پاسخ‌های JSON فقط تا دقت دقیقه می‌نویسد (<c>yyyy-MM-ddTHH:mm</c>)؛
/// ثانیه و اجزای کوچک‌تر حذف می‌شوند. مقادیر داخلی (دیتابیس و منطق) دست‌نخورده می‌مانند
/// و فقط شکل نمایشی خروجی تمیز می‌شود. هنگام خواندن، هر قالب استاندارد پذیرفته می‌شود.
/// </summary>
public sealed class MinutePrecisionDateTimeConverter : JsonConverter<DateTime>
{
    /// <summary>
    /// قالب خروجی: تاریخ با ساعت و دقیقه، بدون ثانیه.
    /// </summary>
    private const string OutputFormat = "yyyy-MM-ddTHH:mm";

    /// <inheritdoc />
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        string? raw = reader.GetString();
        if (string.IsNullOrWhiteSpace(raw))
        {
            return default;
        }

        // هر قالب استاندارد (از جمله خروجی خودمان و ISO کامل با ثانیه) پذیرفته می‌شود.
        if (DateTime.TryParse(raw, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out DateTime parsed))
        {
            return parsed;
        }

        return DateTime.Parse(raw, CultureInfo.InvariantCulture);
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString(OutputFormat, CultureInfo.InvariantCulture));
    }
}
