using Hydra.DTOs.ModelDTOs.SystemUserDTO;
using System.Threading.Tasks;

namespace Hydra.RazorClassLibrary.Services.Authentication
{
    public interface IAuthenticationService
    {
        Task<LoginViewDTO?> Login(LoginViewDTO loginModel);
        Task Logout();
    }
}
