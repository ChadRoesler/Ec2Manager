namespace Ec2Manager.Constants
{
    public class ResourceStrings
    {
        internal const string LanguageType = "CSharp";
        internal const string KeyFileName = "{0}.dll";
        internal const string KeyType = "Ec2Manager.CryptographyManagement";
        internal const string DecryptionMethodName = "{0}Decryption";
        internal const string EncryptedKeys = @"===============[Encrypted Keys]===============
AccessKey: {0}
SecretKey: {1}
KeyFilePath: {2}
==============================================";
        internal const string AppSettingsAddition = @"==============[AppSetting Value]==============
      {{
        ""AccessKeyHash"": ""{0}"",
        ""NameTag"": ""NameOfTagForNameColumnHere"",
        ""Name"": ""{2}"",
        ""Region"": ""RegionHere"",
        ""SecretKeyHash"": ""{1}"",
        ""TagSearchString"": ""."",
        ""TagToSearch"": ""TagToSearchHere""
      }}
==============================================
";
        internal const string AppSettingsPowershellAddition = @"{{
    ""AccessKeyHash"": ""{0}"",
    ""NameTag"": ""NameOfTagForNameColumnHere"",
    ""Name"": ""{2}"",
    ""Region"": ""RegionHere"",
    ""SecretKeyHash"": ""{1}"",
    ""TagSearchString"": ""."",
    ""TagToSearch"": ""TagToSearchHere""
}}
";
        internal const string KeyGenerationCode = @"using System;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;

namespace Ec2Manager
{{
    public class CryptographyManagement
    {{
        public string {2}Decryption(string Value)
        {{
            var valueAsBytes = Convert.FromBase64String(Value);
            var decryptedString = string.Empty;
            var assemblyPath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location),""{2}.dll"");
            var fileCreateTime = File.GetCreationTime(assemblyPath);
            using (var aesEncryption = new AesManaged())
            {{
                aesEncryption.Key = Convert.FromBase64String(""{0}"");
                aesEncryption.IV = Convert.FromBase64String(""{1}"");
                var decryptor = aesEncryption.CreateDecryptor(aesEncryption.Key, aesEncryption.IV);
                using (var decryptionMemoryStream = new MemoryStream(valueAsBytes))
                {{
                    using (var decryptionCryptoStream = new CryptoStream(decryptionMemoryStream, decryptor, CryptoStreamMode.Read))
                    {{
                        using (var decryptionStreamReader = new StreamReader(decryptionCryptoStream))
                        {{
                            decryptedString = decryptionStreamReader.ReadToEnd();
                        }}
                    }}
                }}
            }}
            var trimmedDecryptedString = decryptedString.Replace(fileCreateTime.ToString(),"""");
            var trimmedFileCreateTime = decryptedString.Replace(trimmedDecryptedString,"""");
            if(trimmedFileCreateTime == fileCreateTime.ToString())
            {{
                return trimmedDecryptedString;
            }}
            else
            {{
                return null;
            }}
        }}
    }}
}}";
    }
}
