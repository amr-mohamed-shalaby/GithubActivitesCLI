using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GithubActivity.Models;

namespace GithubActivity.Commands
{
    public interface ICommand
    {
        string Command { get; }
        string Description { get; }
        List<CommandParam> Params { get; }
        Task<string> Execute(string[] args);
    }
}