using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using CommandLine;
using HybridScaffolding;
using HybridScaffolding.Enums;
using Ec2Manager.Models.CommandLine;

namespace Ec2Manager.Models
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
                    var keyToMake = new AwsKey(encrypt.AccessKey, encrypt.SecretKey, encrypt.AccountName);
                    keyToMake.EncryptKeys(encrypt.OutputDirectory);
                });
                //.WithNotParsed((errs) => { errs.Dump(); });


            base.ConsoleMain(arguments, runType);
        }

        public override object PreGuiExec(string[] arguments, object passable)
        {
            var builder = WebHost.CreateDefaultBuilder(arguments).UseStartup<Startup>();
            passable = builder.Build();
            return base.PreGuiExec(arguments, passable);
        }

        public override void GuiMain(string[] arguments, object passableObkect)
        {
            ((IWebHost)passableObkect).Run();
            base.GuiMain(arguments, passableObkect);
        }
    }
}
