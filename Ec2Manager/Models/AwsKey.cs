using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Amazon;
using HybridScaffolding.Enums;
using Ec2Manager.Constants;

namespace Ec2Manager.Models
{
    internal class AwsKey
    {
        internal AwsKey(string AccessKey, string SecretKey, string AccountName)
        {
            this.AccessKey = AccessKey;
            this.SecretKey = SecretKey;
            this.AccountName = AccountName;
        }
        internal AwsKey(AwsAccount account)
        {
            AccessKey = account.AccessKeyHash;
            SecretKey = account.SecretKeyHash;
            AccountName = account.Name;
            NameTag = account.NameTag;
            Tag = account.TagToSearch;
            TagSearchString = account.TagSearchString;
            Region = RegionEndpoint.GetBySystemName(account.Region);
        }
        internal string AccessKey { get; set; }
        internal string SecretKey { get; set; }
        internal string AccountName { get; set; }
        internal string NameTag { get; set; }
        internal string Tag { get; set; }
        internal string TagSearchString { get; set; }
        internal RegionEndpoint Region { get; set; }

        internal void EncryptKeys(string OutputDirectory, RunTypes RunType)
        {

            var keyByteString = string.Empty;
            var initializationVectorByteString = string.Empty;
            var encryptedAccessKey = string.Empty;
            var encryptedSecretKey = string.Empty;
            var currentDateTime = DateTime.Now;
            var stringDateTime = currentDateTime.ToString();
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
                        using (var encryptorStreamWriter = new StreamWriter(encryptorCryptoStream))
                        {
                            encryptorStreamWriter.Write(AccessKey + stringDateTime);
                        }
                    }
                    var encyrptedAccessKeyBytes = encryptorMemoryStream.ToArray();
                    encryptedAccessKey = Convert.ToBase64String(encyrptedAccessKeyBytes);
                }
                using (var encryptorMemoryStream = new MemoryStream())
                {
                    using (var encryptorCryptoStream = new CryptoStream(encryptorMemoryStream, encryptor, CryptoStreamMode.Write))
                    {
                        using (var encryptorStreamWriter = new StreamWriter(encryptorCryptoStream))
                        {
                            encryptorStreamWriter.Write(SecretKey + stringDateTime);
                        }
                    }
                    var encyrptedSecretKeyBytes = encryptorMemoryStream.ToArray();
                    encryptedSecretKey = Convert.ToBase64String(encyrptedSecretKeyBytes);
                }
            }
            var codeToCompile = string.Format(ResourceStrings.KeyGenerationCode, keyByteString, initializationVectorByteString, AccountName);
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

            var assemblyName = string.Format(ResourceStrings.KeyFileName, AccountName);
            var compliationOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary);
            CSharpCompilation compilation = CSharpCompilation.Create(assemblyName, syntaxTree, references, compliationOptions);
            using (var ms = new MemoryStream())
            {
                EmitResult result = compilation.Emit(ms);

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
                    File.SetCreationTime(outputPath, currentDateTime);
                    File.SetLastWriteTime(outputPath, currentDateTime.Add(new TimeSpan(new Random().Next(365), 0, 0, new Random().Next(86400))));
                    var keyInfo = string.Format(ResourceStrings.EncryptedKeys, encryptedAccessKey, encryptedSecretKey, outputPath);
                    Console.WriteLine(keyInfo);
                    if (RunType == RunTypes.Powershell)
                    {
                        var appsettingsPowershellInfo = string.Format(ResourceStrings.AppSettingsPowershellAddition, encryptedAccessKey, encryptedSecretKey, AccountName);
                        Console.WriteLine(appsettingsPowershellInfo);
                    }
                    else
                    {
                        var appSettingsInfo = string.Format(ResourceStrings.AppSettingsAddition, encryptedAccessKey, encryptedSecretKey, AccountName);
                        Console.WriteLine(appSettingsInfo);
                    }
                }
            }
        }
    }
}