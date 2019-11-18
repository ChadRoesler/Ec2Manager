using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using HybridScaffolding.Enums;
using Newtonsoft.Json;
using Ec2Manager.Constants;
using Ec2Manager.Interfaces;
using Ec2Manager.Models.ConfigManagement;

namespace Ec2Manager.Workers
{
    public static class KeyCryptography
    {
        internal static void EncryptKeys(IAwsAccountInfo AwsAccountInfo, string OutputDirectory, RunTypes RunType, bool AppSettingsOnly)
        {
            var keyByteString = string.Empty;
            var initializationVectorByteString = string.Empty;
            var encryptedAccessKey = string.Empty;
            var encryptedSecretKey = string.Empty;
            var currentDateTime = DateTime.Now;
            var createdDateTime = currentDateTime.Add(new TimeSpan(new Random().Next(365), 0, 0, new Random().Next(86400)));
            var modifiedDateTime = currentDateTime.Add(new TimeSpan(new Random().Next(365), 0, 0, new Random().Next(86400)));
            var stringDateTime = createdDateTime.ToString();
            using (var aesEncryption = new AesManaged())
            {
                aesEncryption.GenerateKey();
                aesEncryption.GenerateIV();
                keyByteString = Convert.ToBase64String(aesEncryption.Key);
                initializationVectorByteString = Convert.ToBase64String(aesEncryption.IV);

                var encryptor = aesEncryption.CreateEncryptor(aesEncryption.Key, aesEncryption.IV);
                using (var encryptorMemoryStream = new MemoryStream())
                {
                    using (var encryptorCryptoStream = new CryptoStream(encryptorMemoryStream, encryptor, CryptoStreamMode.Write))
                    {
                        using var encryptorStreamWriter = new StreamWriter(encryptorCryptoStream);
                        encryptorStreamWriter.Write(AwsAccountInfo.AccessKey + stringDateTime);
                    }
                    var encyrptedAccessKeyBytes = encryptorMemoryStream.ToArray();
                    encryptedAccessKey = Convert.ToBase64String(encyrptedAccessKeyBytes);
                    AwsAccountInfo.AccessKey = encryptedAccessKey;
                }
                using (var encryptorMemoryStream = new MemoryStream())
                {
                    using (var encryptorCryptoStream = new CryptoStream(encryptorMemoryStream, encryptor, CryptoStreamMode.Write))
                    {
                        using var encryptorStreamWriter = new StreamWriter(encryptorCryptoStream);
                        encryptorStreamWriter.Write(AwsAccountInfo.SecretKey + stringDateTime);
                    }
                    var encyrptedSecretKeyBytes = encryptorMemoryStream.ToArray();
                    encryptedSecretKey = Convert.ToBase64String(encyrptedSecretKeyBytes);
                    AwsAccountInfo.SecretKey = encryptedSecretKey;
                }
            }
            var codeToCompile = string.Format(ResourceStrings.KeyGenerationCode, keyByteString, initializationVectorByteString, AwsAccountInfo.AccountName);
            var syntaxTree = new[] { CSharpSyntaxTree.ParseText(codeToCompile) };
            var references = new MetadataReference[]
            {
                MetadataReference.CreateFromFile(typeof(object).GetTypeInfo().Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Console).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(AesManaged).GetTypeInfo().Assembly.Location),
                MetadataReference.CreateFromFile(typeof(ICryptoTransform).GetTypeInfo().Assembly.Location),
                MetadataReference.CreateFromFile(typeof(File).GetTypeInfo().Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Assembly).GetTypeInfo().Assembly.Location),
                MetadataReference.CreateFromFile(typeof(System.Runtime.AssemblyTargetedPatchBandAttribute).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Microsoft.CSharp.RuntimeBinder.CSharpArgumentInfo).Assembly.Location),
            };

            var assemblyName = string.Format(ResourceStrings.KeyFileName, AwsAccountInfo.AccountName);
            var compliationOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary);
            CSharpCompilation compilation = CSharpCompilation.Create(assemblyName, syntaxTree, references, compliationOptions);
            using var memoryStream = new MemoryStream();
            EmitResult result = compilation.Emit(memoryStream);

            if (!result.Success)
            {
                IEnumerable<Diagnostic> failures = result.Diagnostics.Where(diagnostic =>
                    diagnostic.Severity == DiagnosticSeverity.Error);
                foreach (Diagnostic diagnostic in failures)
                {
                    Console.Error.WriteLine("\t{0}: {1}", diagnostic.Id, diagnostic.GetMessage());
                }
            }
            else
            {
                var outputPath = Path.Combine(OutputDirectory, assemblyName).Replace("\\\\", "\\");
                compilation.Emit(outputPath);
                File.SetCreationTime(outputPath, createdDateTime);
                File.SetLastWriteTime(outputPath, modifiedDateTime);
                var keyInfo = string.Format(ResourceStrings.EncryptedKeys, AwsAccountInfo.AccessKey, AwsAccountInfo.SecretKey, outputPath);
                var awsAccountInfoJson = JsonConvert.SerializeObject(new AwsAccountInfo(AwsAccountInfo), Formatting.Indented);
                if (RunType == RunTypes.Powershell)
                {
                    Console.WriteLine(awsAccountInfoJson);
                }
                else
                {
                    if (!AppSettingsOnly)
                    {
                        Console.WriteLine(keyInfo);
                    }
                    var appSettingsInfo = string.Format(ResourceStrings.AppSettingsAddition, awsAccountInfoJson);
                    Console.WriteLine(appSettingsInfo);
                }
            }
        }
        internal static IAwsAccountInfo DecryptKeys(IAwsAccountInfo AwsAccountInfo)
        {
            var currentDir = AppDomain.CurrentDomain.BaseDirectory;
            var path = Path.Combine(currentDir, string.Format(ResourceStrings.KeyFileName, AwsAccountInfo.AccountName));
            var keyFile = Assembly.LoadFile(path);
            var cryptographyManagement = keyFile.GetType(ResourceStrings.KeyType);
            var cryptographyManagementObject = keyFile.CreateInstance(ResourceStrings.KeyType);
            var decryption = cryptographyManagement.GetMethod(string.Format(ResourceStrings.DecryptionMethodName, AwsAccountInfo.AccountName), new Type[] { typeof(string) });
            var encryptedAccessKeyAsObject = new object[] { AwsAccountInfo.AccessKey };
            var encryptedSecretKeyAsObject = new object[] { AwsAccountInfo.SecretKey };
            var decryptedAccessKey = (string)decryption.Invoke(cryptographyManagementObject, encryptedAccessKeyAsObject);
            var decryptedSecretKey = (string)decryption.Invoke(cryptographyManagementObject, encryptedSecretKeyAsObject);
            AwsAccountInfo.AccessKey = decryptedAccessKey;
            AwsAccountInfo.SecretKey = decryptedSecretKey;
            return AwsAccountInfo;
        }
    }
}
