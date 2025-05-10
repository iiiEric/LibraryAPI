//using LibraryAPI.Services;
//using Microsoft.AspNetCore.DataProtection;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore.SqlServer.Query.Internal;
//using static System.Runtime.InteropServices.JavaScript.JSType;

//namespace LibraryAPI.Controllers.V1
//{
//    [ApiController]
//    [Route("api/v1/[controller]")]
//    public class SecurityController : ControllerBase
//    {
//        private readonly IDataProtector _dataProtector;
//        private readonly ITimeLimitedDataProtector _timeLimitedDataProtector;
//        private readonly IHashServicies _hashServicies;

//        public SecurityController(IDataProtectionProvider dataProtectionProvider, IHashServicies hashServicies)
//        {
//            _dataProtector = dataProtectionProvider.CreateProtector("SecurityController");
//            _timeLimitedDataProtector = _dataProtector.ToTimeLimitedDataProtector();
//            this._hashServicies = hashServicies;
//        }

//        [HttpGet("hashV1")]
//        public ActionResult Hash(string plainText)
//        {
//           var hash1 = _hashServicies.Hash(plainText);
//           var hash2 = _hashServicies.Hash(plainText);
//           var hash3 = _hashServicies.Hash(plainText, hash2.Salt);
//           return Ok( new { plainText, hash1, hash2, hash3 });
//        }

//        [HttpGet("encryptV1")]
//        public ActionResult Encrypt(string plainText)
//        {
//            if (string.IsNullOrEmpty(plainText))
//                return BadRequest("Plain text cannot be null or empty.");

//            var encryptedText = _dataProtector.Protect(plainText);
//            return Ok(new { encryptedText });
//        }

//        [HttpGet("decryptV1")]
//        public ActionResult Decrypt(string encryptedText)
//        {
//            if (string.IsNullOrEmpty(encryptedText))
//                return BadRequest("Encrypted text cannot be null or empty.");

//            var plainText = _dataProtector.Unprotect(encryptedText);
//            return Ok(new { plainText });
//        }

//        [HttpGet("time-limited-encryptV1")]
//        public ActionResult TimeLimitedEncrypt(string plainText)
//        {
//            if (string.IsNullOrEmpty(plainText))
//                return BadRequest("Plain text cannot be null or empty.");

//            var encryptedText = _timeLimitedDataProtector.Protect(plainText, lifetime: TimeSpan.FromSeconds(30));
//            return Ok(new { encryptedText });
//        }

//        [HttpGet("time-limited-decryptV1")]
//        public ActionResult TimeLimitedDecrypt(string encryptedText)
//        {
//            if (string.IsNullOrEmpty(encryptedText))
//                return BadRequest("Encrypted text cannot be null or empty.");

//            var plainText = _timeLimitedDataProtector.Unprotect(encryptedText);
//            return Ok(new { plainText });
//        }
//    }
//}
