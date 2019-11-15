using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using CommandLine;
using HybridScaffolding;
using HybridScaffolding.Enums;
using Ec2Manager.Models.CommandLine;
using Ec2Manager.Workers;

namespace Ec2Manager.Models.DataManagement
{
    public class Ec2ManagerScaffold : HybridScaffold
    {
        public override string[] PreConsoleExec(string[] arguments, RunTypes runType)
        {
            return base.PreConsoleExec(arguments, runType);
        }

        public override void ConsoleMain(string[] arguments, RunTypes runType)
        {
            var result = Parser.Default.ParseArguments<EncryptCommand>(arguments)
                .WithParsed((EncryptCommand encrypt) => {
                    KeyCryptography.EncryptKeys(encrypt, encrypt.OutputDirectory, runType, encrypt.AppSettingOnly);
                });
            base.ConsoleMain(arguments, runType);
        }

        public override object PreGuiExec(string[] arguments, object passable)
        {
            var hostBuilder = Host.CreateDefaultBuilder(arguments)
               .ConfigureWebHostDefaults(webBuilder =>
               {
                   webBuilder.UseStartup<Startup>();
               });
            passable = hostBuilder.Build();
            return base.PreGuiExec(arguments, passable);
        }

        public override void GuiMain(string[] arguments, object passableObject)
        {
            ((IHost)passableObject).Run();
            base.GuiMain(arguments, passableObject);
        }
    }
}
