using System.Text;

namespace Exam.Options
{
    public class JwtOptions
    {
        public string Key { get; set; }
        public byte[] KeyInBytes => Encoding.ASCII.GetBytes(Key);
        public string Issuer { get; set; }
        public string Audience { get; set; }
    }
}