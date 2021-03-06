using System;
using System.Threading.Tasks;
using TinyCqrs.Attributes;
using TinyCqrs.Classes;
using TinyCqrs.Interfaces;

// TODO: Code sharing.
namespace TinyCqrs.Abstract
{
    [CqrsIgnore]
    public abstract class TryCatchHandlerAsync<TCmd> : ICmdHandlerAsync<TCmd>
    {
        protected CmdResult CmdResult { get; set; }
        protected abstract Task ExecuteBody(TCmd cmd);

        private async Task<CmdResult> TryCatchNext(TCmd cmd)
        {
            var current = CmdResult ?? new CmdResult();
            
            try
            {
                await ExecuteBody(cmd);
            }
            catch (Exception ex)
            {
                current.AddIssue(ex.Message);
            }

            return current;
        }
        
        public async Task<CmdResult> Execute(TCmd cmd)
            => await TryCatchNext(cmd);
    }
    
    [CqrsIgnore]
    public abstract class TryCatchHandlerAsync<TCmd, TResult> : ICmdHandlerAsync<TCmd, TResult>
        where TResult : ICmdResult, new()
    {
        protected TResult CmdResult { get; set; }
        protected abstract Task ExecuteBody(TCmd cmd);

        private async Task<TResult> TryCatchNext(TCmd cmd)
        {
            var current = CmdResult;
            
            try
            {
                await ExecuteBody(cmd);
            }
            catch (Exception ex)
            {
                current.AddIssue(ex.Message);
            }

            return current;
        }
        
        public async Task<TResult> Execute(TCmd cmd)
            => await TryCatchNext(cmd);
    }
}