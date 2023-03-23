using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JoggingTimesTrackerDAL.Services.IJwtAuthenticationService
{
    public interface IJwtAuthenticationService
    {
        Task<string> Authenticate(string email, string password);
    }
}
